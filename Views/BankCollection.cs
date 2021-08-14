using System;
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
		public static BankCollection Bankinternalcollection = new BankCollection ( );
		public static BankCollection temp = new BankCollection ( );

		public static DataTable dtBank = new DataTable ( "BankDataTable" );
		public static bool USEFULLTASK = true;
		public static bool Notify = false;
		public static string Caller = "";

		// Object used to lock Data Load from Sql and load into collection
		private readonly object LockBankReadData = new object ( );
		private readonly object LockBankLoadData = new object ( );
		// Lock object


		#region CONSTRUCTOR

		public BankCollection ( ) : base ( )
		{
		}
		public static BankCollection LoadBank ( BankCollection cc, string caller, int ViewerType = 1, bool NotifyAll = false )
		{
			object lockobject = new object ( );
			Notify = NotifyAll;
			Caller = caller;

			try
			{
				// Called to Load/reload the One & Only Bankcollection data source
				//if ( dtBank . Rows . Count > 0 )
				//	dtBank . Clear ( );

				//				Debug . WriteLine ( $"\n ***** SQL WARNING Created a NEW MasterBankCollection ..................." );
				if ( USEFULLTASK )
				{
					Bankinternalcollection = null;
					Bankinternalcollection = new BankCollection ( );
					Bankinternalcollection . LoadBankTaskInSortOrderasync ( );
					cc = Bankinternalcollection;
					return Bankinternalcollection;
				}
				else
				{
					//					Debug . WriteLine ( $"\n ***** Loading BankAccount Data from disk (using Abbreviated Await Control system)*****\n" );
					Bankinternalcollection . ClearItems ( );
					// Abstract the main data load call to a method that uses AWAITABLE  calles
					ProcessRequest ( ) . ConfigureAwait ( false );

					// We now have the pointer to the Bank data in variable Bankinternalcollection
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
			//LoadBankData();
			//// this returns "Bankinternalcollection" as a pointer to the correct viewer
			//Bankinternalcollection = await LoadBankCollection().ConfigureAwait(false);
		}

		public bool LoadBankTaskInSortOrderasync ( bool Notify = false, int i = -1 )
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

			LoadBankData ( );
			LoadBankCollection ( );
			//Task t1 = Task.Run(
			//		async () =>
			//		{
			//			LoadBankData();
			//		}
			//	);
			//#region Continuations
			//t1.ContinueWith
			//(
			//	async (Bankinternalcollection) =>
			//	{
			//		LoadBankCollection();
			//		//					Debug . WriteLine ( $"Just Called LoadBankCollection () in second Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
			//	}, TaskScheduler.FromCurrentSynchronizationContext()
			// );
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
			return true;
		}

		public static bool SelectViewer ( int ViewerType, BankCollection tmp )
		{
			bool result = false;
			switch ( ViewerType )
			{
				//case 1:
				//	SqlViewerBankcollection = tmp;
				//	result = true;
				//	break;
				//case 2:
				//	EditDbBankcollection = tmp;
				//	result = true;
				//	break;
				//case 3:
				//	MultiBankcollection = tmp;
				//	result = true;
				//	break;
				//case 4:
				//	BankViewerDbcollection = tmp;
				//	result = true;
				//	break;


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

			return Bankinternalcollection;
		}

		/// Handles the actual conneciton ot SQL to load the Details Db data required
		/// </summary>
		/// <returns></returns>
		public static DataTable LoadBankData ( int max = 0, int mode = -1, bool isMultiMode = false )
		{
			object bptr = new object ( );
			try
			{
				SqlConnection con;
				string ConString = "";
				ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				Debug . WriteLine ( $"Making new SQL connection in BANKCOLLECTION" );
				con = new SqlConnection ( ConString );

				using ( con )
				{
					Debug . WriteLine ( $"Using new SQL connection in BANKCOLLECTION" );
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
					{
						commandline = Flags . FilterCommand;
					}
					else
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = "Select * from [BankAccount] order by ";
						commandline = Utils . GetDataSortOrder ( commandline );
					}
					//					lock (bptr)
					//					{
					Debug . WriteLine ( $"BANK : SQL Locking BankCollection Datatable Load (307) in BankCollection (331) load function " );
					SqlCommand cmd = new SqlCommand ( commandline, con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					if ( dtBank == null )
						dtBank = new DataTable ( );
					sda . Fill ( dtBank );
					Debug . WriteLine ( $"BANK : SQL UnLocking BankCollection Datatable Load (315) in BankCollection (331) load function " );
					//					}
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Failed to load Bank Details - {ex . Message}, {ex . Data}" );
				return null;
				//				MessageBox . Show ( $"Failed to load Bank Details - {ex . Message}, {ex . Data}" ); return false;
			}
			return dtBank;
		}

		public bool LoadBankCollection ( )
		{
			int count = 0;
			try
			{
				object bptr = new object ( );
				//				lock (bptr)
				//				{
				Debug . WriteLine ( $"BANK : SQL Locking BankCollection Load in BankCollection (331) load function " );
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
				Debug . WriteLine ( $"BANK : SQL Unlocking BankCollection Load in BankCollection (347) load function " );
				//				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"BANK : SQL Error in BankCollection(351) load function : {ex . Message}, {ex . Data}" );
				MessageBox . Show ( $"BANK : SQL Error in BankCollection (351) load function : {ex . Message}, {ex . Data}" );
			}
			finally
			{
				// This is ONLY called  if a requestor specifies the argument as TRUE
				if ( Notify )
				{
					EventControl . TriggerBankDataLoaded ( null,
						new LoadedEventArgs
						{
							CallerType = "SQLSERVER",
							CallerDb = Caller,
							DataSource = Bankinternalcollection,
							RowCount = Bankinternalcollection . Count
						} );
					//					Debug . WriteLine ( $"DEBUG : In BankCollection : Sending  BankDataLoaded EVENT trigger" );
				}
			}
			//			Flags . BankCollection = Bankcollection;
			//			Bankinternalcollection = null;
			return true;
		}



		public static DataTable LoadSelectedBankData ( int Min, int Max, int Tot )
		{
			DataTable dt = new DataTable ( );
			SqlConnection con;
			string ConString = "";
			string commandline = "";
			ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
			Debug . WriteLine ( $"Making new SQL connection in BANKCOLLECTION" );
			con = new SqlConnection ( ConString );
			try
			{
				Debug . WriteLine ( $"Using new SQL connection in BANKCOLLECTION" );
				using ( con )
				{
					// Create a valid Query Command string including any active sort ordering
					commandline = $"Select Top {Tot} Id, BankNo, CustNo, AcType, Balance, IntRate, ODate, CDate  from BankAccount  " +
						$" where CustNo > {Min} AND CustNo < {Max} order by CustNo + BankNo ";
					SqlCommand cmd = new SqlCommand ( commandline, con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dt );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"BANKACCOUNT : ERROR in LoadBankDataSql(): Failed to load Bank Details - {ex . Message}, {ex . Data}" );
				MessageBox . Show ( $"BANKACCOUNT: ERROR in LoadBankDataSql(): Failed to load Bank Details - {ex . Message}, {ex . Data}" );
			}
			finally
			{
				con . Close ( );
			}
			return dt;
		}

		/// <summary>
		/// A specialist version  to reload data from a provided DataTable
		///  July 2021
		/// </summary>
		/// <returns></returns>
		public async static Task<BankCollection >LoadSelectedCollection ( BankCollection temp, int max = -1, DataTable dtBank = null, bool Notify = false )
		{
			object bptr = new object ( );
			try
			{
				//if max is not Received, set to total records in DtBank
				if ( max == -1 )
					max = dtBank . Rows . Count;
				for ( int i = 0 ; i < max ; i++ )
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
				// This is ONLY called  if a requestor specifies the argument as TRUE
				if ( Notify )
				{
					EventControl . TriggerBankDataLoaded ( null,
						new LoadedEventArgs
						{
							CallerType = "SELECTEDDATA",
							CallerDb = Caller,
							DataSource = temp,
							RowCount = Bankinternalcollection . Count
						} );
					//					Debug . WriteLine ( $"DEBUG : In BankCollection : Sending  BankDataLoaded EVENT trigger" );
				}
			}
			if ( Notify == false )
				return temp;
			else
				return null;
		}
		#endregion LOAD THE DATA

		public void ListBankInfo ( KeyboardDelegate KeyBoardDelegate )
		{
			// Run a specified delegate sent by SqlDbViewer
			KeyBoardDelegate ( 1 );
		}
		public static bool UpdateBankDb ( BankAccountViewModel NewData, string CallerType )
		{
			SqlConnection con;
			SqlCommand cmd = null;
			string ConString = "";
			ConString = ( string ) Settings . Default [ "BankSysConnectionString" ];
			con = new SqlConnection ( ConString );
			try
			{
				using ( con )
				{
					con . Open ( );
					if ( CallerType == "CUSTOMER" )
					{
						cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, " +
							"ODATE=@odate, CDATE=@cdate where CUSTNO = @custno", con );
						cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( NewData . Id ) );
						cmd . Parameters . AddWithValue ( "@bankno", NewData . BankNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@custno", NewData . CustNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( NewData . AcType ) );
						cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( NewData . ODate ) );
						cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( NewData . CDate ) );
					}
					else
					{
						cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, " +
							"BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno", con );
						cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( NewData . Id ) );
						cmd . Parameters . AddWithValue ( "@bankno", NewData . BankNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@custno", NewData . CustNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( NewData . AcType ) );
						cmd . Parameters . AddWithValue ( "@balance", Convert . ToDecimal ( NewData . Balance ) );
						cmd . Parameters . AddWithValue ( "@intrate", Convert . ToDecimal ( NewData . IntRate ) );
						cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( NewData . ODate ) );
						cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( NewData . CDate ) );
					}
					cmd . ExecuteNonQuery ( );
					Debug . WriteLine ( "SQL Update of All Db's successful..." );
				}
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"BANKACCOUNT Update FAILED : {ex . Message}, {ex . Data}" );
			}
			finally
			{
				con . Close ( );
			}
			return true;
		}

		#region update functionality

		/// <summary>
		/// Called to allow any method to load FULL Bank data directly 
		/// it returns a populated DataTable
		/// </summary>
		/// <param name="dtBank"></param>
		/// <param name="Sqlcommand"></param>
		/// <returns></returns>
		public static DataTable LoadBankDirect ( DataTable dtBank, string Sqlcommand = "Select* from BankAccount order by CustNo, BankNo" )
		{
			SqlConnection con;
			string ConString = "";
			string commandline = "";
			dtBank . Clear ( );

			try
			{
				ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				Debug . WriteLine ( $"Making new SQL connection in BANKCOLLECTION" );
				con = new SqlConnection ( ConString );

				using ( con )
				{
					Debug . WriteLine ( $"Using new SQL connection in BANKCOLLECTION" );
					if ( Sqlcommand != "" )
						commandline = Sqlcommand;
					else
						commandline = "Select * from BankAccount order by CustNo, BankNo";

					SqlCommand cmd = new SqlCommand ( commandline, con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					if ( dtBank == null )
						dtBank = new DataTable ( );
					sda . Fill ( dtBank );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Failed to load Bank Details - {ex . Message}, {ex . Data}" );
				return null;
			}
			return dtBank;

		}
		/// <summary>
		/// A specialist version  to reload data WITHOUT changing global version
		/// </summary>
		/// <returns></returns>
		public static BankCollection LoadBankCollectionDirect ( BankCollection temp, DataTable dtBank )
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

		#endregion update functionality


		#region EXPORT FUNCTIONS TO READ/WRITE CSV files for BANKACCOUNT DB

		/// <summary>
		/// Load the data into a DataTable for our export functions below here
		/// </summary>
		/// <returns></returns>
		public static DataTable LoadBankExportData ( )
		{
			DataTable dt = new DataTable ( );
			SqlConnection con;
			string ConString = "";
			string commandline = "";
			ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
			Debug . WriteLine ( $"Making new SQL connection in BANKCOLLECTION" );
			con = new SqlConnection ( ConString );
			try
			{
				Debug . WriteLine ( $"Using new SQL connection in BANKCOLLECTION" );
				using ( con )
				{
					//if ( Flags . IsMultiMode )
					//{
					//	// Create a valid Query Command string including any active sort ordering
					//	commandline = $"SELECT * FROM SECACCOUNTS WHERE CUSTNO IN "
					//		+ $"(SELECT CUSTNO FROM SECACCOUNTS  "
					//		+ $" GROUP BY CUSTNO"
					//		+ $" HAVING COUNT(*) > 1) ORDER BY ";
					//	commandline = Utils . GetDataSortOrder ( commandline );
					//}
					//else if ( Flags . FilterCommand != "" )
					//{
					//	commandline = Flags . FilterCommand;
					//}
					//else
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = "Select * from BankAccount  order by ";
						commandline = Utils . GetDataSortOrder ( commandline );
					}
					SqlCommand cmd = new SqlCommand ( commandline, con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dt );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"BANKACCOUNT : ERROR in LoadBankDataSql(): Failed to load Bank Details - {ex . Message}, {ex . Data}" );
				MessageBox . Show ( $"BANKACCOUNT: ERROR in LoadBankDataSql(): Failed to load Bank Details - {ex . Message}, {ex . Data}" );
			}
			finally
			{
				con . Close ( );
			}
			return dt;
		}

		/// <summary>
		/// Writes the data directly from the Db vias Sqluery first, then output to disk file in CSV format
		/// Working well 7/6/21
		/// </summary>
		/// <param name="path"></param>
		/// <param name="dbType"></param>
		public static int ExportBankData ( string path, string dbType )
		{
			int count = 0;
			string output = "";

			// Read data in from disk first as a DataTable dt
			DataSet ds = new DataSet ( );

			DataTable dt = new DataTable ( );
			dt = LoadBankExportData ( );
			ds . Tables . Add ( dt );
			//£££££££££££££££££££££££££££££££££££££££££££
			// This works just fine with no external binding.
			// The data is  now accessible in ds.Tables[0].Rows
			// NB DATA ACCESS FORMAT IS [ $"{objRow["CustNo"]}"  ]
			//££££££££££££££££££££££££££££££££££££££££££££
			Console . WriteLine ( $"Writing results of SQL enquiry to {path} ..." );
			foreach ( DataRow objRow in ds . Tables [ 0 ] . Rows )
			{
				output += ParseDbRow ( "BANKACCOUNT", objRow );
				count++;
			}
			if ( path == "" )
				path = @"C:\Users\ianch\Documents\Bank";
			string savepath = Utils . GetExportFileName ( path );

			System . IO . File . WriteAllText ( savepath, output );
			Console . WriteLine ( $"Export of {count - 1} records from the [ {dbType} ] Db has been saved to {path} successfully." );
			return count;
		}


		//===============================================================================
		/// <summary>
		/// Special method to check the data format we are going to write to the CSV file 
		/// and creates the output line by line from a datarow of the DataTable we have just read in
		/// </summary>
		/// <param name="dbType"></param>
		/// <param name="objRow"></param>
		/// <returns></returns>
		public static string ParseDbRow ( string dbType, DataRow objRow )
		{
			string tmp = "", s = "";
			string [ ] odat, cdat, revstr;
			if ( dbType == "BANKACCOUNT" )
			{
				char [ ] ch = { ' ' };
				char [ ] ch2 = { '/' };
				s = $"{objRow [ "Odate" ] . ToString ( )}', '";
				odat = s . Split ( ch );
				string odate = odat [ 0 ];
				// now reverse it  to YYYY/MM/DD format as this is what SQL understands
				revstr = odate . Split ( ch2 );
				odate = revstr [ 2 ] + "/" + revstr [ 1 ] + "/" + revstr [ 0 ];
				// thats  the Open date handled - now do close data
				s = $"{objRow [ "cDate" ] . ToString ( )}', '";
				cdat = s . Split ( ch );   // split date on '/'
				string cdate = cdat [ 0 ];
				// now reverse it  to YYYY/MM/DD format as this is what SQL understands
				revstr = cdate . Split ( ch2 );
				cdate = revstr [ 2 ] + "/" + revstr [ 1 ] + "/" + revstr [ 0 ];
				string acTypestr = objRow [ "AcType" ] . ToString ( ) . Trim ( );

				//Creates the correct format for the CSV fle output, including adding single quotes to DATE fields
				// Tested and working 7/6/21
				tmp = $"{objRow [ "Id" ] . ToString ( )}, "
					+ $"{objRow [ "BankNo" ] . ToString ( )}, "
					+ $"{objRow [ "CustNo" ] . ToString ( )}, "
					+ $"{acTypestr}, "
					+ $"{objRow [ "Balance" ] . ToString ( )}, "
					+ $"{objRow [ "Intrate" ] . ToString ( )}, "
					+ $"'{odate}', '"
					+ $"{cdate}'\r\n";
			}
			return tmp;
		}
		#endregion EXPORT FUNCTIONS TO READ/WRITE CSV files


	}
}