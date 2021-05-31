using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . Data;
using System . Data . SqlClient;
using System . Diagnostics;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using WPFPages . Properties;
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
		public static bool Notify = false;

		// Object used to lock Data Load from Sql and load into collection
		private readonly object LockBankReadData = new object ( );
		private readonly object LockBankLoadData = new object ( );
		// Lock object

		/// <summary>
		/// Allows us to get a listb of all Db collections ?
		/// </summary>
		/// <returns></returns>
		public static List<BankCollection> GetDbInfo ( )
		{
			List<BankCollection> Banks = new List<BankCollection> ( );
			Banks . Add ( BankViewerDbcollection );
			Banks . Add ( SqlViewerBankcollection );
			Banks . Add ( EditDbBankcollection );
			Banks . Add ( MultiBankcollection );
			Banks . Add ( Bankinternalcollection );
			return Banks;
		}

		#region CONSTRUCTOR

		public BankCollection ( )
		{
		}
		public async static Task<BankCollection> LoadBank ( BankCollection cc, int ViewerType = 1, bool NotifyAll = false )
		{
			Notify = NotifyAll;
			try
			{
				// Called to Load/reload the One & Only Bankcollection data source
				//if ( dtBank . Rows . Count > 0 )
				//	dtBank . Clear ( );

				Bankinternalcollection = new BankCollection ( );
				Debug . WriteLine ( $"\n ***** SQL WARNING Created a NEW MasterBankCollection ..................." );
				if ( USEFULLTASK )
				{
					//					BankCollection db = new BankCollection ( );
					await Bankinternalcollection . LoadBankTaskInSortOrderasync ( );
					return ( BankCollection ) null;

				}
				else
				{
					//					Debug . WriteLine ( $"\n ***** Loading BankAccount Data from disk (using Abbreviated Await Control system)*****\n" );
					Bankinternalcollection . ClearItems ( );
					// Abstract the mail data load call to a method that uses AWAITABLE  calles
					ProcessRequest ( ) . ConfigureAwait ( false );

					// We now have the pointer to the the Bank data in variable Bankinternalcollection
					if ( Flags . IsMultiMode == false )
					{
						BankCollection db = new BankCollection ( );
						SelectViewer ( ViewerType, Bankinternalcollection );
						return db;
					}
					else
					{
						// return the "working  copy" pointer, it has  filled the relevant collection to match the viewer
						return null;
					}
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Bank Load Exception : {ex . Message}, {ex . Data}" );
				return null;
			}
		}

		private static async Task ProcessRequest ( )
		{
			// Load data fro SQL into dtBank Datatable
			LoadBankData ( );
			// this returns "Bankinternalcollection" as a pointer to the correct viewer
			Bankinternalcollection = await LoadBankCollection ( ) . ConfigureAwait ( false );
		}

		public async Task<BankCollection> LoadBankTaskInSortOrderasync ( bool Notify = false, int i = -1 )
		// No longer used
		{
			//object LockBankReadData = null;
			//lock ( LockBankReadData )
			//{

				if ( dtBank . Rows . Count > 0 )
					dtBank . Clear ( );

				if ( Bankinternalcollection . Items . Count > 0 )
					Bankinternalcollection . ClearItems ( );
			//}
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
					LoadBankCollection ( Notify );
					Debug . WriteLine ( $"Just Called LoadBankCollection () in second Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
				}, TaskScheduler . FromCurrentSynchronizationContext ( )
			 );
			#endregion process code to load data

			#region Success//Error reporting/handling

			//			// Now handle "post processing of errors etc"
			//			//This will ONLY run if there were No Exceptions  and it ALL ran successfully!!
			//			t1 . ContinueWith (
			//			( Bankinternalcollection ) =>
			//				{
			//					//					Debug . WriteLine ( $"BANKACCOUNT : Task.Run() Completed : Status was [ {Bankinternalcollection . Status}" );
			//				}, CancellationToken . None, TaskContinuationOptions . OnlyOnRanToCompletion, TaskScheduler . FromCurrentSynchronizationContext ( )
			//			);
			//			//This will iterate through ALL of the Exceptions that may have occured in the previous Tasks
			//			// but ONLY if there were any Exceptions !!
			//			t1 . ContinueWith (
			//				( Bankinternalcollection ) =>
			//				{
			//					AggregateException ae = t1 . Exception . Flatten ( );
			//					Debug . WriteLine ( $"Exception in BankCollection data processing \n" );
			//					MessageBox . Show ( $"Exception in BankCollection data processing \n" );
			//					foreach ( var item in ae . InnerExceptions )
			//					{
			//						Debug . WriteLine ( $"BankCollection : Exception : {item . Message}, : {item . Data}" );
			//					}
			//				}, CancellationToken . None, TaskContinuationOptions . OnlyOnFaulted, TaskScheduler . FromCurrentSynchronizationContext ( )
			//			);

			//			// Now handle "post processing of errors etc"
			//			//This will ONLY run if there were No Exceptions  and it ALL ran successfully!!
			//			t1 . ContinueWith (
			//				( Bankinternalcollection ) =>
			//				{
			//					//					Debug . WriteLine ( $"BankCollection : Task.Run() processes all succeeded. \nBankcollection Status was [ {Bankinternalcollection . Status} ]." );
			////					Console . WriteLine ( $"BANKACCOUNT : Task.Run() Completed : Status was [ {Bankinternalcollection . Status} ]." );
			//				}, CancellationToken . None, TaskContinuationOptions . OnlyOnRanToCompletion, TaskScheduler . FromCurrentSynchronizationContext ( )
			//			);
			//			//This will iterate through ALL of the Exceptions that may have occured in the previous Tasks
			//			// but ONLY if there were any Exceptions !!
			//			t1 . ContinueWith (
			//				( Bankinternalcollection ) =>
			//				{
			//					AggregateException ae = t1 . Exception . Flatten ( );
			//					Debug . WriteLine ( $"Exception in BankCollection data processing \n" );
			//					MessageBox . Show ( $"Exception in BankCollection data processing \n" );
			//					foreach ( var item in ae . InnerExceptions )
			//					{
			//						Debug . WriteLine ( $"BankCollection : Exception : {item . Message}, : {item . Data}" );
			//					}
			//				}, CancellationToken . None, TaskContinuationOptions . OnlyOnFaulted, TaskScheduler . FromCurrentSynchronizationContext ( )
			//			);

			#endregion Continuations

			//			Debug . WriteLine ( $"BANKACCOUNT : END OF PROCESSING & Error checking functionality\nBANKACCOUNT : *** Bankcollection total = {Bankinternalcollection . Count} ***\n\n" );

			#endregion Success//Error reporting/handling
			// Finally fill and return The global Dataset
			Flags . BankCollection = Bankinternalcollection;
			//			MasterBankcollection = Bankinternalcollection;
			//			Bankcollection = Bankinternalcollection;
			return null;
		}

		public static bool SelectViewer ( int ViewerType, BankCollection tmp )
		{
			bool result = false;
			switch ( ViewerType )
			{
				case 1:
					SqlViewerBankcollection = tmp;
					result = true;
					break;
				case 2:
					EditDbBankcollection = tmp;
					result = true;
					break;
				case 3:
					MultiBankcollection = tmp;
					result = true;
					break;
				case 4:
					BankViewerDbcollection = tmp;
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
		public static void LoadBankData ( int mode = -1, bool isMultiMode = false )
		{
			BankCollection bptr = new BankCollection ( );
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
					//lock ( bptr . LockBankReadData )
					//{
					sda . Fill ( dtBank );
					//}
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Failed to load Bank Details - {ex . Message}, {ex . Data}" ); return;
				//				MessageBox . Show ( $"Failed to load Bank Details - {ex . Message}, {ex . Data}" ); return false;
			}
			return;
		}

		public static async Task<BankCollection> LoadBankCollection ( bool DoNotify = false )
		{
			int count = 0;
			try
			{
				//				BankCollection bptr = new BankCollection ( );
				//lock ( bptr . LockBankLoadData )
				//{
				//					BankCollection bc = new BankCollection ( );
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
				//				Debug . WriteLine ( $"BANKACCOUNT : Sql data loaded into Bank ObservableCollection \"Bankinternalcollection\" [{count}] ...." );
				//				}
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
							CallerDb = "BANKACCOUNT",
							DataSource = Bankinternalcollection,
							RowCount = Bankinternalcollection . Count
						} );
					Debug . WriteLine ( $"DEBUG : In BankCollection : Sending  BankDataLoaded EVENT trigger" );
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
		public static bool UpdateBankDb ( BankAccountViewModel NewData )
		{

			SqlConnection con;
			string ConString = "";
			ConString = ( string ) Settings . Default [ "BankSysConnectionString" ];
			//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
			con = new SqlConnection ( ConString );
			try
			{
				//We need to update BOTH BankAccount AND DetailsViewModel to keep them in parallel
				using ( con )
				{
					con . Open ( );
					SqlCommand cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, " +
						"BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
					cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( NewData . Id ) );
					cmd . Parameters . AddWithValue ( "@bankno", NewData . BankNo . ToString ( ) );
					cmd . Parameters . AddWithValue ( "@custno", NewData . CustNo . ToString ( ) );
					cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( NewData . AcType ) );
					cmd . Parameters . AddWithValue ( "@balance", Convert . ToDecimal ( NewData . Balance ) );
					cmd . Parameters . AddWithValue ( "@intrate", Convert . ToDecimal ( NewData . IntRate ) );
					cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( NewData . ODate ) );
					cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( NewData . CDate ) );
					cmd . ExecuteNonQuery ( );
					Debug . WriteLine ( "SQL Update of BankAccounts successful..." );
				}
			}
			catch ( Exception ex )
			{ Console . WriteLine ( $"BANKACCOUNT Update FAILED : {ex . Message}, {ex . Data}" ); }
			finally
			{ con . Close ( ); }
			return true;
		}
	}
}