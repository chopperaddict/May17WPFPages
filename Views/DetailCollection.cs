using System;
using System . Collections . ObjectModel;
using System . Data;
using System . Data . SqlClient;
using System . Diagnostics;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using WPFPages . ViewModels;

// Used by DetailsDbViewer
namespace WPFPages . Views
{
	public class DetailCollection
	{
		public static bool Notify = false;
		public static string Caller = "";

		public DetailCollection ( )
		{
		}

		public ObservableCollection<DetailsViewModel> DetCollection = new ObservableCollection<DetailsViewModel> ( );

		// working entity to load data into
		public static DetCollection internalcollection = null;// new DetailCollection ( );

		public static DataTable dtDetails = new DataTable ( );

		public static async Task<bool> LoadDet ( DetCollection dc, string caller, int ViewerType = 1, bool NotifyAll = false )
		{
			bool result = false;
			object lockobject = new object ( );
			Notify = NotifyAll;
			Caller = caller;
			Debug . WriteLine ( $"About to lock Details Load system" );
			try
			{
				lock ( lockobject )
				{
					internalcollection = null;
					internalcollection = new DetCollection ( );
					LoadDetailsTaskInSortOrderAsync ( internalcollection );
					Debug . WriteLine ( $"Exiting lock of Details Load system" );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Erro in lock Details Load system : {ex . Message} + {ex . Data}" );
			}
			return false;
		}

	public static async Task<bool> LoadDetailsTaskInSortOrderAsync ( DetCollection internalcollection )
	{
		//			NotifyAll = NotifyAll;
		bool result = false;

		if ( dtDetails . Rows . Count > 0 )
			dtDetails . Clear ( );

		#region process code to load data

		Task t1 = Task . Run (
			async ( ) =>
			{
				Debug . WriteLine ( $"DETCOLLECTION : Calling LOADDETAILSDATASQL()" );
				await LoadDetailsDataSql ( );
				Debug . WriteLine ( $"DETCOLLECTION : Completed LOADDETAILSDATASQL()" );
			}
			);
		t1 . ContinueWith
		(
			async ( result ) =>
			{
				Debug . WriteLine ( $"DETCOLLECTION : Calling LOADDETCOLLECTION()" );
				await LoadDetCollection ( internalcollection );
				Debug . WriteLine ( $"DETCOLLECTION : Completed LOADDETCOLLECTION()" );
			}, TaskScheduler . FromCurrentSynchronizationContext ( )
		 ); ;

		#endregion process code to load data

		#region Success//Error reporting/handling

		// Now handle "post processing of errors etc"
		//This will ONLY run if there were No Exceptions  and it ALL ran successfully!!
		t1 . ContinueWith (
			( Detinternalcollection ) =>
			{
//				Debug . WriteLine ( $"DETAILS : Task.Run() Completed : Status was [ {Detinternalcollection . Status} ]." );
			}, CancellationToken . None, TaskContinuationOptions . OnlyOnRanToCompletion, TaskScheduler . FromCurrentSynchronizationContext ( )
		);
		//This will iterate through ALL of the Exceptions that may have occured in the previous Tasks
		// but ONLY if there were any Exceptions !!
		t1 . ContinueWith (
			( Detinternalcollection ) =>
			{
				AggregateException ae = t1 . Exception . Flatten ( );
				Debug . WriteLine ( $"Exception in DetCollection data processing \n" );
				foreach ( var item in ae . InnerExceptions )
				{
					Debug . WriteLine ( $"DetCollection : Exception : {item . Message}, : {item . Data}" );
				}
			}, CancellationToken . None, TaskContinuationOptions . NotOnRanToCompletion, TaskScheduler . FromCurrentSynchronizationContext ( )
		);

		//			Debug . WriteLine ($"DETAILS : END OF PROCESSING & Error checking functionality\nDETAILS : *** Detcollection total = {Detcollection.Count} ***\n\n");
		#endregion Success//Error reporting/handling

		return true;
	}

	/// Handles the actual conneciton ot SQL to load the Details Db data required
	/// </summary>
	/// <returns></returns>
	//**************************************************************************************************************************************************************//
	public static async Task<DataTable> LoadDetailsDataSql ( bool isMultiMode = false )
	{
		Stopwatch st = new Stopwatch ( );

		st . Start ( );
		SqlConnection con;
		string ConString = "";
		string commandline = "";
		ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
		Debug . WriteLine ($"Making new SQL connection in DETAILSCOLLECTION");
		con = new SqlConnection ( ConString );
		//if ( con== null )
		//{
		//	Debug . WriteLine ( $"DETAILCOLLECTION : No SQL Connection, reconnecting ..." );
		//	con= new SqlConnection ( ConString );
		//}
		try
		{
				Debug . WriteLine ( $"Using new SQL connection in DETAILSCOLLECTION" );
				using ( con )
			{
				if ( Flags . IsMultiMode )
				{
					// Create a valid Query Command string including any active sort ordering
					commandline = $"SELECT * FROM SECACCOUNTS WHERE CUSTNO IN "
						+ $"(SELECT CUSTNO FROM SECACCOUNTS  "
						+ $" GROUP BY CUSTNO"
						+ $" HAVING COUNT(*) > 1) ORDER BY ";
					commandline = Utils . GetDataSortOrder ( commandline );
				}
				else if ( Flags . FilterCommand != "" )
				{
					commandline = Flags . FilterCommand;
				}
				else
				{
					// Create a valid Query Command string including any active sort ordering
					commandline = "Select * from SecAccounts  order by ";
					commandline = Utils . GetDataSortOrder ( commandline );
				}
				SqlCommand cmd = new SqlCommand ( commandline, con );
				SqlDataAdapter sda = new SqlDataAdapter ( cmd );

				DetCollection bptr = new DetCollection ( );
				sda . Fill ( dtDetails );
				st . Stop ( );
			}
		}
		catch ( Exception ex )
		{
			Debug . WriteLine ( $"DETAILS : ERROR in LoadDetailsDataSql(): Failed to load Details Details - {ex . Message}, {ex . Data}" );
			MessageBox . Show ( $"DETAILS : ERROR in LoadDetailsDataSql(): Failed to load Details Details - {ex . Message}, {ex . Data}" );
		}
		finally
		{
			con . Close ( );
		}
		return dtDetails;
	}

	//**************************************************************************************************************************************************************//
	public static async Task<bool> LoadDetCollection ( DetCollection internalcollection )
	{
		int count = 0;
		try
		{
			for ( int i = 0 ; i < dtDetails . Rows . Count ; i++ )
			{
				internalcollection . Add ( new DetailsViewModel
				{
					Id = Convert . ToInt32 ( dtDetails . Rows [ i ] [ 0 ] ),
					BankNo = dtDetails . Rows [ i ] [ 1 ] . ToString ( ),
					CustNo = dtDetails . Rows [ i ] [ 2 ] . ToString ( ),
					AcType = Convert . ToInt32 ( dtDetails . Rows [ i ] [ 3 ] ),
					Balance = Convert . ToDecimal ( dtDetails . Rows [ i ] [ 4 ] ),
					IntRate = Convert . ToDecimal ( dtDetails . Rows [ i ] [ 5 ] ),
					ODate = Convert . ToDateTime ( dtDetails . Rows [ i ] [ 6 ] ),
					CDate = Convert . ToDateTime ( dtDetails . Rows [ i ] [ 7 ] )
				} );
				count = i;
			}
			//				Debug . WriteLine ( $"DETAILS : Sql data loaded into Details ObservableCollection \"Detinternalcollection\" [{count}] ...." );
		}
		catch ( Exception ex )
		{
			Debug . WriteLine ( $"DETAILS : ERROR in  LoadDetCollection() : loading Details into ObservableCollection \"DetCollection\" : [{ex . Message}] : {ex . Data} ...." );
			MessageBox . Show ( $"DETAILS : ERROR in  LoadDetCollection() : loading Details into ObservableCollection \"DetCollection\" : [{ex . Message}] : {ex . Data} ...." );
			return false;
		}
		finally
		{
			if ( Notify )
			{
				//					OnDetDataLoaded ( Detcollection , row );
				EventControl . TriggerDetDataLoaded ( null,
					new LoadedEventArgs
					{
						CallerType = "SQLSERVER",
						CallerDb = Caller,
						DataSource = internalcollection,
						RowCount = internalcollection . Count
					} );
			}
			///					Flags . DetCollection = Detinternalcollection;
		}
		return true;
		//			}
	}




}
}
