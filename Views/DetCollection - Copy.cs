using System;
using System . Collections . ObjectModel;
using System . Data;
using System . Data . SqlClient;
using System . Diagnostics;
using System . Runtime . CompilerServices;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;

using WPFPages . ViewModels;

namespace WPFPages . Views
{
	public class DetCollection : ObservableCollection<DetailsViewModel>//, INotifyCompletion
	{
		//Declare a global pointer to Observable Details Collection
//		public  DetCollection DetcollectionStatic;
		public static DetCollection Detcollection;

		public  static DataTable dtDetails = new DataTable();
		public static Stopwatch  st;


		#region DATA LOADED EVENT trigger method

		// THIS IS  HOW  TO HANDLE EVENTS RIGHT NOW  - WORKING WELL 4/5/21//
		// We no longer need ot delcare a Delegate  to do this
		//Event CallBack for when Asynchronous data loading has been completed in the Various ViewModel classes
		public   static event EventHandler<LoadedEventArgs> DetDataLoaded;

		//-------------------------------------------------------------------------------------------------------------------------------------------------//
		protected virtual void OnDetDataLoaded ( )
		{
			if ( DetDataLoaded != null )
			{
				Console . WriteLine ( $"Broadcasting from OnDetDataLoaded in " );
				DetDataLoaded?.Invoke ( this , new LoadedEventArgs ( ) { DataSource = Detcollection , CallerDb = "DETAILS" } );
			}
		}

		#endregion DATA LOADED EVENT trigger method

		#region CONSTRUCTOR

		public DetCollection ( )
		{
			//set the static pointer to this class
			Detcollection = this;
			Flags . DetailsCollection = this;
		}

		#endregion CONSTRUCTOR


		#region startup/load data / load collection (DetCollection)

		// Entry point for all data load/Reload
		CancellationTokenSource  cts = new CancellationTokenSource();

		//**************************************************************************************************************************************************************//
		public async static Task<bool> LoadDetailsTaskInSortOrderAsync ( bool b = false )
		{
			try
			{
				if ( dtDetails . Rows . Count > 0 )
					dtDetails . Clear ( );

				if ( Detcollection . Items . Count > 0 )
					Detcollection . ClearItems ( );
				Detcollection = new DetCollection ( );

				st = Stopwatch . StartNew ( );
				Console . WriteLine ( $"Calling Task.Run in Detcollection ...." );


				await Task . Run ( async ( ) =>
				{
					Console . WriteLine ( $"Calling LoadtailsDataSql in Task.Run in Detcollection ...." );
					await LoadDetailsDataSql ( );
					Console . WriteLine ( $"Returned from  LoadDetailsDataSql in Task.Run in Detcollection ...." );
				} );

				Application . Current . Dispatcher . Invoke (
					 ( ) =>
					{
						LoadDetCollection ( );

					} );
				st . Stop ( );
				Console . WriteLine ( $"**** END **** OF ASYNC CALL METHOD {dtDetails . Rows . Count} records in DataTable, {Detcollection . Count} in Detcollection ...." );
				Console . WriteLine ( $"**** END **** SENDING CALLBACK MESSAGE TO SQLDBVIEWER WINDOW TO LOAD THEIR DATAGRID !!!" );

				if ( DetDataLoaded != null )
					DetDataLoaded . Invoke ( Detcollection , new LoadedEventArgs { CallerDb = "DETAILS" , DataSource = Detcollection } );
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"ERROR in LoadDetailsTaskInSortOrderAsync() : {ex . Message}, : {ex . Data}" );
				return false;
			}
			return true;
		}

		/// Handles the actual conneciton ot SQL to load the Details Db data required
		/// </summary>
		/// <returns></returns>
		//**************************************************************************************************************************************************************//
		public static async Task<bool> LoadDetailsDataSql ( bool isMultiMode = false )
		{
			try
			{
				st . Start ( );
				SqlConnection con;
				string ConString = "";
				string commandline = "";
				ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				con = new SqlConnection ( ConString );
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
					sda . Fill ( dtDetails );
					st . Stop ( );
					Console . WriteLine ( $"Sql data loaded  [{dtDetails . Rows . Count}] row(s) into Details DataTable in {( double ) st . ElapsedMilliseconds / ( double ) 1000}...." );
					return true;
				}
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"ERROR in LoadDetailsDataSql(): Failed to load Details Details - {ex . Message}, {ex . Data}" );
				return false;
			}
			return true;
		}

		//**************************************************************************************************************************************************************//
		public static async Task<bool> LoadDetCollection ( )
		{
			int count = 0;
			try
			{
				for ( int i = 0 ; i < dtDetails . Rows . Count ; i++ )
				{
					Detcollection . Add ( new DetailsViewModel
					{
						Id = Convert . ToInt32 ( dtDetails . Rows [ i ] [ 0 ] ) ,
						BankNo = dtDetails . Rows [ i ] [ 1 ] . ToString ( ) ,
						CustNo = dtDetails . Rows [ i ] [ 2 ] . ToString ( ) ,
						AcType = Convert . ToInt32 ( dtDetails . Rows [ i ] [ 3 ] ) ,
						Balance = Convert . ToDecimal ( dtDetails . Rows [ i ] [ 4 ] ) ,
						IntRate = Convert . ToDecimal ( dtDetails . Rows [ i ] [ 5 ] ) ,
						ODate = Convert . ToDateTime ( dtDetails . Rows [ i ] [ 6 ] ) ,
						CDate = Convert . ToDateTime ( dtDetails . Rows [ i ] [ 7 ] )
					} );
					count = i;
				}
				Console . WriteLine ( $"Sql data loaded into Details ObservableCollection \"DetCollection\" [{count}] ...." );
				return true;
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"ERROR in  LoadDetCollection() : loading Details into ObservableCollection \"DetCollection\" : [{ex . Message}] : {ex . Data} ...." );
				return false;
			}
		}

		//**************************************************************************************************************************************************************//

		#endregion startup/load data / load collection (DetCollection)

		#region Event Subscribing Hsndlers

		public static void SubscribeToLoadedEvent ( object o )
		{
			if ( o == Detcollection && DetDataLoaded == null )
				DetDataLoaded += Flags . CurrentSqlViewer . SqlDbViewer_DataLoaded;
		}

		public static void UnSubscribeToLoadedEvent ( object o )
		{
			if ( DetDataLoaded != null )
				DetDataLoaded -= Flags . CurrentSqlViewer . SqlDbViewer_DataLoaded;
		}

		#endregion Event Subscribing Hsndlers

		public static Delegate [ ] GetEventCount8 ( )
		{
			Delegate [ ] dglist2 = null;
			if ( DetDataLoaded != null )
				dglist2 = DetDataLoaded?.GetInvocationList ( );
			return dglist2;
		}

		#region INotifyCompletion Interface code
		// INotifyCompletion Interface code

		//public static TaskAwaiter GetAwaiter ( object o)
		//{
		//	return GetResult ( );
		//}
		public static bool IsCompleted ( )
		{
			return false;
		}

		public static bool GetResult ( )
		{
			return true;
		}
		#endregion INotifyCompletion Interface code
		#region TEST CODE
		//**************************************************************************************************************************************************************//
		//Test codeonly
		//**************************************************************************************************************************************************************//
		public DataTable DoTask ( )
		{
			DataTable dt = new DataTable();
			return dt;
		}
		public static async Task go ( )
		{
			DataTable dt = null ;
			Task task  =  Task . Run ( async () =>
			{
				dt = Flags.DetailsCollection.DoTask();
			} );


			Console . WriteLine ( $"New dt = {dt . Rows}, {dt . Columns}" );

		}

		public void OnCompleted ( Action continuation )
		{
			throw new NotImplementedException ( );
		}
		#endregion TEST CODE
	}
}
