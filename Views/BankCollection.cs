using System;
using System . Collections . ObjectModel;
using System . Data;
using System . Data . SqlClient;
using System . Diagnostics;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Threading;

using WPFPages . ViewModels;

using static WPFPages . SqlDbViewer;

namespace WPFPages . Views
{

	public class BankCollection : ObservableCollection<BankAccountViewModel>
	{

		//		//Declare a global pointer to Observable BankAccount Collection
		public static BankCollection BankViewerDbcollection = new BankCollection ( );
		public static BankCollection SqlViewerBankcollection = new BankCollection ( );
		public static BankCollection EditDbBankcollection = new BankCollection ( );
		public static BankCollection MultiBankcollection = new BankCollection ( );
		public static BankCollection Bankinternalcollection = new BankCollection ( );
		public static BankCollection temp = new BankCollection ( );

		public static DataTable dtBank = new DataTable ( "BankDataTable" );
		public static bool USEFULLTASK = true;

		#region CONSTRUCTOR

		public BankCollection ( )
		{
		}
		public async static Task<BankCollection> LoadBank ( BankCollection cc, int ViewerType = 1, bool NotifyAll = false )
		//public async static Task<BankCollection> LoadBank ( int ViewerType, bool NotifyAll = false , BankCollection bankdata = null)
		{
			// Called to Load/reload the One & Only Bankcollection data source
			if ( dtBank . Rows . Count > 0 )
				dtBank . Clear ( );

			if ( cc != null )
				Bankinternalcollection = cc;
			else
			{
				Bankinternalcollection = new BankCollection ( );
				Debug . WriteLine ( $"\n ***** SQL WARNING Created a NEW MasterBankCollection ..................." );
			}
			Debug . WriteLine ( $"\n ***** Loading Bank Data from disk *****\n" );

			if ( USEFULLTASK )
			{
				Debug . WriteLine ( $"\n ***** Loading BankAccount Data from disk (using FULL Task Control system)*****\n" );
				BankCollection db = new BankCollection ( );
				await db.LoadBankTaskInSortOrderasync ( );
				return Bankinternalcollection;
			}
			else
			{
				Debug . WriteLine ( $"\n ***** Loading BankAccount Data from disk (using Abbreviated Await Control system)*****\n" );
				Bankinternalcollection . ClearItems ( );
				// Abstract the mail data load call to a method that uses AWAITABLE  calles
				ProcessRequest ( ) . ConfigureAwait ( false );

				// We now have the pointer to the the Bank data in variable Bankinternalcollection
				if ( Flags . IsMultiMode == false )
				{
					Debug . WriteLine ( "Returning Bank Data via Debug output...." );
					// Finally fill and return The global Dataset
					BankCollection db = new BankCollection ( );
					SelectViewer ( ViewerType, Bankinternalcollection, out db );
					if ( ViewerType == 1 )
						return SqlViewerBankcollection;
					if ( ViewerType == 2 )
						return EditDbBankcollection;
					if ( ViewerType == 3 )
					{
						cc = MultiBankcollection;
						return MultiBankcollection;
					}
					if ( ViewerType == 4 )
						return BankViewerDbcollection;
					else
						return ( BankCollection ) db;
				}
				else
				{
					// return the "working  copy" pointer, it has  filled the relevant collection to match the viewer
					return Bankinternalcollection;
				}
			}
		}
		private static async Task ProcessRequest ( )
		{
			// Load data fro SQL into dtBank Datatable
			 LoadBankData ( );
			// this returns "Bankinternalcollection" as a pointer to the correct viewer
			Bankinternalcollection = await LoadBankCollection ( ) . ConfigureAwait ( false );
		}
		public static BankCollection  SelectViewer ( int ViewerType, BankCollection tmp , out BankCollection db)
		{
			db = null;
			switch ( ViewerType )
			{
				case 1:
					SqlViewerBankcollection = tmp;
					db = SqlViewerBankcollection;
					break;
				case 2:
					EditDbBankcollection = tmp;
					db = 
					db = EditDbBankcollection;
					break;
				case 3:
					MultiBankcollection = tmp;
					db = MultiBankcollection;
					break;
				case 4:
					BankViewerDbcollection = tmp;
					db =  BankViewerDbcollection;
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
					break;
			}
			return db;
		}
		#endregion CONSTRUCTOR

		#region LOAD THE DATA


		/// <summary>
		/// Method used ONLY when working with Multi accounts data
		/// </summary>
		/// <param name="b"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		public async Task<BankCollection> ReLoadBankData ( bool b = false, int mode = -1 )
		{
			if ( dtBank . Rows . Count > 0 )
				dtBank . Clear ( );

			//LoadBankTaskInSortOrderasync ( false );
			BankCollection temp = new BankCollection ( );
			if ( temp . Count > 0 )
			{
				temp . ClearItems ( );
			}

			await ProcessRequest ( );

			//await LoadBankData ( );
			//if ( Flags . IsMultiMode )
			//{
			//	// Loading  subset of multi accounts only
			//	//				BankCollection bank = new BankCollection();
			//	temp = await LoadBankTest (temp );
			//	// Just return  the subset of data without updating our
			//	// //Flags pointer or class Bankcollection pointer
			//	return temp;
			//}
			//else
			//{
			//	// :Loading full total or data
			//	Bankinternalcollection = LoadBank( mode);
			//	SelectViewer ( mode , Bankinternalcollection );

			//	// Set our globals etc
			//	//				Bankcollection = Bankinternalcollection;
			//	//				Flags . BankCollection = Bankcollection = Bankinternalcollection;
			return Bankinternalcollection;
			//			}
		}

		/// Handles the actual conneciton ot SQL to load the Details Db data required
		/// </summary>
		/// <returns></returns>
		public static  void LoadBankData ( int mode = -1, bool isMultiMode = false )
		{
			try
			{
				SqlConnection con;
				string ConString = "";

				//				ConString = ( string ) Properties . Settings . Default [ "ConnectionString" ];
				ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
				con = new SqlConnection ( ConString );
				using ( con )
				{
					string commandline = "";

					if ( Flags . IsMultiMode )
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = $"SELECT * FROM BANKACCOUNT WHERE CUSTNO IN "
							+ $"(SELECT CUSTNO FROM BANKACCOUNT "
							+ $" GROUP BY CUSTNO"
							+ $" HAVING COUNT(*) > 1) ORDER BY ";

						commandline = Utils . GetDataSortOrder ( commandline );
					}
					else if ( Flags . FilterCommand != "" )
					{ commandline = Flags . FilterCommand; }
					else
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = "Select * from BankAccount order by ";
						commandline = Utils . GetDataSortOrder ( commandline );
					}

					SqlCommand cmd = new SqlCommand ( commandline, con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					if ( dtBank == null )
						dtBank = new DataTable ( );
					sda . Fill ( dtBank );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Failed to load Bank Details - {ex . Message}, {ex . Data}" ); return ;
				//				MessageBox . Show ( $"Failed to load Bank Details - {ex . Message}, {ex . Data}" ); return false;
				return ;
			}
			return ;
		}

		public static async Task<BankCollection> LoadBankCollection ( bool Notify = false )
		{
			int count = 0;
			try
			{
				BankCollection bc = new BankCollection ( );
				for ( int i = 0 ; i < dtBank . Rows . Count ; i++ )
				{
					Bankinternalcollection . Add ( new BankAccountViewModel
					{
						Id = Convert . ToInt32 ( dtBank . Rows [ i ] [ 0 ] ),
						BankNo = dtBank . Rows [ i ] [ 1 ] . ToString ( ),
						CustNo = dtBank . Rows [ i ] [ 2 ] . ToString ( ),
						AcType = Convert . ToInt32 ( dtBank . Rows [ i ] [ 3 ] ),
						Balance = Convert . ToDecimal ( dtBank . Rows [ i ] [ 4 ] ),
						IntRate = Convert . ToDecimal ( dtBank . Rows [ i ] [ 5 ] ),
						ODate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 6 ] ),
						CDate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 7 ] ),
					} );
					count = i;
				}
				Debug . WriteLine ( $"BANKACCOUNT : Sql data loaded into Bank ObservableCollection \"Bankinternalcollection\" [{count}] ...." );
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"BANK : SQL Error in BankCollection load function : {ex . Message}, {ex . Data}" );
				MessageBox . Show ( $"BANK : SQL Error in BankCollection load function : {ex . Message}, {ex . Data}" );
			}
			finally
			{
				// This is ONLY called  if a requestor specifies the argument as TRUE
				if ( Notify )
				{
					EventControl . TriggerBankDataLoaded ( null,
						new LoadedEventArgs
						{
							CallerDb = "BankAccount",
							DataSource = Bankinternalcollection,
							RowCount = Bankinternalcollection . Count
						} );
				}
			}
			//			Flags . BankCollection = Bankcollection;
			return Bankinternalcollection;
		}


		/// <summary>
		/// A specialist version  to reload data WITHOUT changing global version
		/// </summary>
		/// <returns></returns>
		public async static Task<BankCollection> LoadBankTest ( BankCollection temp )
		{
			try
			{
				for ( int i = 0 ; i < dtBank . Rows . Count ; i++ )
				{
					temp . Add ( new BankAccountViewModel
					{
						Id = Convert . ToInt32 ( dtBank . Rows [ i ] [ 0 ] ),
						BankNo = dtBank . Rows [ i ] [ 1 ] . ToString ( ),
						CustNo = dtBank . Rows [ i ] [ 2 ] . ToString ( ),
						AcType = Convert . ToInt32 ( dtBank . Rows [ i ] [ 3 ] ),
						Balance = Convert . ToDecimal ( dtBank . Rows [ i ] [ 4 ] ),
						IntRate = Convert . ToDecimal ( dtBank . Rows [ i ] [ 5 ] ),
						ODate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 6 ] ),
						CDate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 7 ] ),
					} );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"BANK : SQL Error in BankCollection load function : {ex . Message}, {ex . Data}" );
				MessageBox . Show ( $"BANK : SQL Error in BankCollection load function : {ex . Message}, {ex . Data}" );
			}
			finally
			{
				Debug . WriteLine ( $"BANK : Completed load into Bankcollection :  {temp . Count} records loaded successfully ...." );
			}
			return temp;
		}
		#endregion LOAD THE DATA

		public void ListBankInfo ( KeyboardDelegate KeyBoardDelegate )
		{
			// Run a specified delegate sent by SqlDbViewer
			KeyBoardDelegate ( 1 );
		}
		public async Task<BankCollection> LoadBankTaskInSortOrderasync ( bool Notify = false, int i = -1 )
		// No longer used
		{
			if ( dtBank . Rows . Count > 0 )
				dtBank . Clear ( );

			if ( Bankinternalcollection . Items . Count > 0 )
				Bankinternalcollection . ClearItems ( );

			// This all woks just fine, and DOES switch back to UI thread that is MANDATORY before doing the Collection load processing
			// thanks to the use of TaskScheduler.FromCurrentSynchronizationContext() that oerforms the magic switch back to the UI thread
			//			Debug . WriteLine ( $"BANK : Entering Method to call Task.Run in BankCollection  : Thread = { Thread . CurrentThread . ManagedThreadId}" );

			#region process code to load data

			Task t1 = Task . Run (
					async ( ) =>
					{
						LoadBankData ( );
					}
				);
			#region Continuations
			t1 . ContinueWith
			(
				async ( Bankinternalcollection ) =>
				{
					//					Debug . WriteLine ( $"Before starting second Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
					LoadBankCollection ( Notify );
				}, TaskScheduler . FromCurrentSynchronizationContext ( )
			 );
			#endregion process code to load data

			#region Success//Error reporting/handling

			// Now handle "post processing of errors etc"
			//This will ONLY run if there were No Exceptions  and it ALL ran successfully!!
			t1 . ContinueWith (
			( Bankinternalcollection ) =>
				{
					Debug . WriteLine ( $"BANKACCOUNT : Task.Run() Completed : Status was [ {Bankinternalcollection . Status}" );
				}, CancellationToken . None, TaskContinuationOptions . OnlyOnRanToCompletion, TaskScheduler . FromCurrentSynchronizationContext ( )
			);
			//This will iterate through ALL of the Exceptions that may have occured in the previous Tasks
			// but ONLY if there were any Exceptions !!
			t1 . ContinueWith (
				( Bankinternalcollection ) =>
				{
					AggregateException ae = t1 . Exception . Flatten ( );
					Debug . WriteLine ( $"Exception in BankCollection data processing \n" );
					MessageBox . Show ( $"Exception in BankCollection data processing \n" );
					foreach ( var item in ae . InnerExceptions )
					{
						Debug . WriteLine ( $"BankCollection : Exception : {item . Message}, : {item . Data}" );
					}
				}, CancellationToken . None, TaskContinuationOptions . OnlyOnFaulted, TaskScheduler . FromCurrentSynchronizationContext ( )
			);

			// Now handle "post processing of errors etc"
			//This will ONLY run if there were No Exceptions  and it ALL ran successfully!!
			t1 . ContinueWith (
				( Bankinternalcollection ) =>
				{
					Debug . WriteLine ( $"BankCollection : Task.Run() processes all succeeded. \nBankcollection Status was [ {Bankinternalcollection . Status} ]." );
				}, CancellationToken . None, TaskContinuationOptions . OnlyOnRanToCompletion, TaskScheduler . FromCurrentSynchronizationContext ( )
			);
			//This will iterate through ALL of the Exceptions that may have occured in the previous Tasks
			// but ONLY if there were any Exceptions !!
			t1 . ContinueWith (
				( Bankinternalcollection ) =>
				{
					AggregateException ae = t1 . Exception . Flatten ( );
					Debug . WriteLine ( $"Exception in BankCollection data processing \n" );
					MessageBox . Show ( $"Exception in BankCollection data processing \n" );
					foreach ( var item in ae . InnerExceptions )
					{
						Debug . WriteLine ( $"BankCollection : Exception : {item . Message}, : {item . Data}" );
					}
				}, CancellationToken . None, TaskContinuationOptions . OnlyOnFaulted, TaskScheduler . FromCurrentSynchronizationContext ( )
			);

			#endregion Continuations

			Debug . WriteLine ( $"BANKACCOUNT : END OF PROCESSING & Error checking functionality\nBANKACCOUNT : *** Bankcollection total = {Bankinternalcollection . Count} ***\n\n" );

			#endregion Success//Error reporting/handling
			// Finally fill and return The global Dataset
			Flags . BankCollection = Bankinternalcollection;
//			MasterBankcollection = Bankinternalcollection;
//			Bankcollection = Bankinternalcollection;
			return null;
		}
	}
}
