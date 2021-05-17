using System;
using System . Collections . ObjectModel;
using System . Data;
using System . Data . SqlClient;
using System . Diagnostics;
using System . Runtime . CompilerServices;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Threading;

using WPFPages . ViewModels;

namespace WPFPages . Views
{
	public class DetCollection : ObservableCollection<DetailsViewModel>
	{ 
		//Declare a global pointer to Observable Details Collection
		public static DetCollection Detcollection = new DetCollection();
		public  static DetCollection DetViewerDbcollection=new DetCollection();
		public  static DetCollection  EditDbDetcollection=new DetCollection();
		public  static DetCollection MultiDetcollection=new DetCollection();
		public  static DetCollection SqlViewerDetcollection=new DetCollection ();
		public  static DetCollection  Detinternalcollection=new DetCollection  ();

		public  static DataTable dtDetails = new DataTable();
		public static Stopwatch  st;

		#region CONSTRUCTOR

		public DetCollection ( )
		{
//			Flags . DetCollection = this;
		}

		#endregion CONSTRUCTOR

		public static DetCollection LoadDet ( DetCollection dc , int ViewerType = 1)
		{
			// Called to Load/reload the One & Only Bankcollection data source
			if ( dtDetails. Rows . Count > 0 )
				dtDetails . Clear ( );

			if ( dc != null )
				Detinternalcollection = dc;
			else
				Detinternalcollection = new DetCollection ( );

			if ( Detinternalcollection . Count > 0 )
				Detinternalcollection . ClearItems ( );

			DetCollection d = new DetCollection();
			d.LoadDetailsDataSql ( );
			if(dtDetails.Rows.Count > 0)
				LoadDetTest ( Detinternalcollection );
			// We now have the ONE AND ONLY pointer the the Bank data in variable Bankcollection
			Flags . DetCollection = Detinternalcollection;
			if ( Flags . IsMultiMode == false )
			{
				// Finally fill and return The global Dataset
				SelectViewer ( ViewerType , Detinternalcollection );
				return Detinternalcollection;
			}
			else
			{
				// return the "working  copy" pointer, it has  filled the relevant collection to match the viewer
				return Detinternalcollection;
			}
		}

		#region startup/load data / load collection (Detinternalcollection)

		// Entry point for all data load/Reload
		CancellationTokenSource  cts = new CancellationTokenSource();

		//**************************************************************************************************************************************************************//
		public async Task<DetCollection> LoadDetailsTaskInSortOrderAsync ( bool b = false , int row = 0 )
		{
//			if ( dtDetails . Rows . Count > 0 )
//				dtDetails . Clear ( );

//			if ( Detinternalcollection . Items . Count > 0 )
//				Detinternalcollection . ClearItems ( );

//			// This all woks just fine, and DOES switch back to UI thread that is MANDATORY before doing the Collection load processing
//			// thanks to the use of TaskScheduler.FromCurrentSynchronizationContext() that performs the magic switch back to the UI thread
//			//			Console . WriteLine ( $"DETAILS : Entering Method to call Task.Run in DetCollection  : Thread = { Thread . CurrentThread . ManagedThreadId}" );

//			#region process code to load data

//			Task t1 = Task . Run(
//					async ( ) =>
//						{
////							Console . WriteLine ( $"Before starting initial Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
//							await LoadDetailsDataSql(b);
////							Console . WriteLine ( $"After initial Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
//						}
//				);
//			t1 . ContinueWith
//			(
//				async ( Detinternalcollection ) =>
//				{
//					Console . WriteLine ( $"Before starting second Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
//					await LoadDetCollection ( row , b );
//				} , TaskScheduler . FromCurrentSynchronizationContext ( )
//			 );
//			//			Console . WriteLine ( $"After second Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );

//			#endregion process code to load data

//			#region Success//Error reporting/handling

//			// Now handle "post processing of errors etc"
//			//This will ONLY run if there were No Exceptions  and it ALL ran successfully!!
//			t1 . ContinueWith (
//				( Detinternalcollection ) =>
//				{
//					Console . WriteLine ( $"DETAILS : Task.Run() Completed : Status was [ {Detinternalcollection . Status} ]." );
//				} , CancellationToken . None , TaskContinuationOptions . OnlyOnRanToCompletion , TaskScheduler . FromCurrentSynchronizationContext ( )
//			);
//			//This will iterate through ALL of the Exceptions that may have occured in the previous Tasks
//			// but ONLY if there were any Exceptions !!
//			t1 . ContinueWith (
//				( Detinternalcollection ) =>
//				{
//					AggregateException ae =  t1 . Exception . Flatten ( );
//					Console . WriteLine ( $"Exception in DetCollection data processing \n" );
//					foreach ( var item in ae . InnerExceptions )
//					{
//						Console . WriteLine ( $"DetCollection : Exception : {item . Message}, : {item . Data}" );
//					}
//				} , CancellationToken . None , TaskContinuationOptions . NotOnRanToCompletion , TaskScheduler . FromCurrentSynchronizationContext ( )
//			);

//			//			Console . WriteLine ($"DETAILS : END OF PROCESSING & Error checking functionality\nDETAILS : *** Detcollection total = {Detcollection.Count} ***\n\n");
//			#endregion Success//Error reporting/handling

//			Flags . DetCollection = Detinternalcollection;
			return Detinternalcollection;
		}

		/// Handles the actual conneciton ot SQL to load the Details Db data required
		/// </summary>
		/// <returns></returns>
		//**************************************************************************************************************************************************************//
		public async Task<bool> LoadDetailsDataSql ( bool isMultiMode = false )
		{
			Stopwatch st = new Stopwatch();
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
					Console . WriteLine ( $"DETAILS : Sql data loaded  [{dtDetails . Rows . Count}] row(s) into Details DataTable in {( double ) st . ElapsedMilliseconds / ( double ) 1000}...." );
				}
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"DETAILS : ERROR in LoadDetailsDataSql(): Failed to load Details Details - {ex . Message}, {ex . Data}" );
				MessageBox . Show ( $"DETAILS : ERROR in LoadDetailsDataSql(): Failed to load Details Details - {ex . Message}, {ex . Data}" );
				return false;
			}
			return true;
		}

		//**************************************************************************************************************************************************************//
		public static async Task<DetCollection> LoadDetCollection ( int row , bool Notify = true )
		{
			int count = 0;
			try
			{
				for ( int i = 0 ; i < dtDetails . Rows . Count ; i++ )
				{
					Detinternalcollection . Add ( new DetailsViewModel
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
				Console . WriteLine ( $"DETAILS : Sql data loaded into Details ObservableCollection \"DetCollection\" [{count}] ...." );
				if ( Notify )
				{
//					OnDetDataLoaded ( Detcollection , row );
					EventControl . TriggerDetDataLoaded ( null ,
						new LoadedEventArgs
						{
							CallerDb = "DETAILS" ,
							DataSource = Detinternalcollection ,
							RowCount = Detinternalcollection . Count
						} );
                       }
				Flags . DetCollection = Detinternalcollection;
				return Detinternalcollection;
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"DETAILS : ERROR in  LoadDetCollection() : loading Details into ObservableCollection \"DetCollection\" : [{ex . Message}] : {ex . Data} ...." );
				MessageBox . Show ( $"DETAILS : ERROR in  LoadDetCollection() : loading Details into ObservableCollection \"DetCollection\" : [{ex . Message}] : {ex . Data} ...." );
				return null;
			}
		}

		public static async Task<DetCollection> LoadDetTest ( DetCollection Detinternalcollection)
		{
			int count = 0;
			try
			{
				for ( int i = 0 ; i < dtDetails . Rows . Count ; i++ )
				{
					Detinternalcollection . Add ( new DetailsViewModel
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
				Console . WriteLine ( $"DETAILS : Sql data loaded into Details ObservableCollection \"DetCollection\" [{count}] ...." );
				Flags . DetCollection = Detinternalcollection;
				return Detinternalcollection;
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"DETAILS : ERROR in  LoadDetCollection() : loading Details into ObservableCollection \"DetCollection\" : [{ex . Message}] : {ex . Data} ...." );
				MessageBox . Show ( $"DETAILS : ERROR in  LoadDetCollection() : loading Details into ObservableCollection \"DetCollection\" : [{ex . Message}] : {ex . Data} ...." );
				return null;
			}
		}
		//**************************************************************************************************************************************************************//

		#endregion startup/load data / load collection (Detinternalcollection)
		public static bool SelectViewer ( int ViewerType , DetCollection tmp )
		{
			bool result = false;
			switch ( ViewerType )
			{
				case 1:
					SqlViewerDetcollection = tmp;
					result = true;
					break;
				case 2:
					EditDbDetcollection = tmp;
					result = true;
					break;
				case 3:
					MultiDetcollection = tmp;
					result = true;
					break;
				case 4:
					DetViewerDbcollection = tmp;
					result = true;
					break;
				//case 5:
				//	CustViewerDbcollection = tmp;
				//	result = true;
				//	break;
				//case 6:
				//	DetViewerDbcollection = tmp;
				//	result = true;
				//	break;
				//case 7:
				//	SqlViewerCustcollection = tmp;
				//	result = true;
				//	break;
				//case 8:
				//	SqlViewerDetcollection = tmp;
				//	result = true;
				//	break;
				case 9:
					//					= tmp;
					result = true;
					break;
			}
			return result;
		}

		public static bool IsCompleted ( )
		{
			return false;
		}

		public static bool GetResult ( )
		{
			return true;
		}
		#region TEST CODE
		//**************************************************************************************************************************************************************//
		//Test codeonly
		//**************************************************************************************************************************************************************//
		#endregion TEST CODE
	}
}
/*
 * 
 				Console . WriteLine ( $"\nCalling dat aload system in DetCollection  : Thread = { Thread . CurrentThread . ManagedThreadId}" );
				Task t1 = Task . Run( 
					async ( ) =>
						{
//							Console . WriteLine ( $"Before starting initial Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
							await LoadDetailsDataSql();
//							Console . WriteLine ( $"After initial Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
						}
				) . ContinueWith (
					{
//					Console . WriteLine ( $"Before starting second Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
					async ( Detcollection ) => await LoadDetCollection ( ),
						TaskScheduler . FromCurrentSynchronizationContext ( )
//					Console . WriteLine ( $"After second Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
				}
				);
* */