using System;
using System . Collections . ObjectModel;
using System . Data;
using System . Data . SqlClient;
using System . Diagnostics;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using WPFPages . Properties;

namespace WPFPages . Views
{
	/// <summary>
	/// Class to hold the Customers data for the system as an Observabke collection
	/// </summary>
	public class CustCollection : ObservableCollection<CustomerViewModel>
	{
		//Declare a global pointer to Observable Customers Collection
		public static CustCollection Custcollection = new CustCollection ( );
		public static DataTable dtCust = new DataTable ( );
		public static CustCollection CustViewerDbcollection = new CustCollection ( );
		public static CustCollection SqlViewerCustcollection = new CustCollection ( );
		public static CustCollection EditDbCustcollection = new CustCollection ( );
		public static CustCollection MultiCustcollection = new CustCollection ( );
		public static CustCollection Custinternalcollection = new CustCollection ( );
		public static BankCollection temp = new BankCollection ( );
		public static bool USEFULLTASK = true;
		public static bool Notify = false;

		private readonly object LockCustReadData = new object ( );
		private readonly object LockCustLoadData = new object ( );


		#region CONSTRUCTOR

		public CustCollection ( ) : base ( )
		{
		}

		#endregion CONSTRUCTOR

		#region startup/load data / load collection (CustCollection)
		public async static Task<CustCollection> LoadCust ( CustCollection cc, int ViewerType = 1, bool NotifyAll = false )
		{
			Notify = NotifyAll;
			try
			{
				// Called to Load/reload the One & Only Bankcollection data source
				if ( dtCust . Rows . Count > 0 )
					dtCust . Clear ( );
				Debug . WriteLine ( $"\n ***** SQL WARNING Created a NEW MasterBankCollection ..................." );
				await ProcessRequest ( ViewerType );

				if ( Flags . IsMultiMode == false )
				{
					CustCollection db = new CustCollection ( );
					SelectViewer ( ViewerType, Custinternalcollection );
					return db;
				}
				else
				{
					// return the "working  copy" pointer, it has  filled the relevant collection to match the viewer
					return null;
				}

			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"Customer Load Exception : {ex . Message}, {ex . Data}" );
				return null;
			}
		}

		private static async Task <CustCollection> ProcessRequest ( int ViewerType )
		{
			if ( USEFULLTASK )
			{
				Custinternalcollection = new CustCollection ( );
				await Custinternalcollection . LoadCustomerTaskInSortOrderAsync ( );
				return ( CustCollection ) null;
			}
			else
			{
				//					Debug . WriteLine ( $"\n ***** Loading BankAccount Data from disk (using Abbreviated Await Control system)*****\n" );
				//CustCollection c = new CustCollection();
				//c . LoadCustDataSql ( );

				//if ( dtCust . Rows . Count > 0 )
				//	Custinternalcollection = LoadCustomerTest ( );
				// We now have the ONE AND ONLY pointer the the Bank data in variable Bankcollection
				Flags . CustCollection = Custinternalcollection;
				if ( Flags . IsMultiMode == false )
				{
					// Finally fill and return The global Dataset
					SelectViewer ( ViewerType, Custinternalcollection );
					return null;// Custinternalcollection;
				}
				else
				{
					// return the "working  copy" pointer, it has  filled the relevant collection to match the viewer
					return null;// Custinternalcollection;
				}
			}
		}

		public async Task<CustCollection> LoadCustomerTaskInSortOrderAsync ( bool b = false, int row = 0, bool NotifyAll = false )
		{

			if ( dtCust. Rows . Count > 0 )
				dtCust . Clear ( );

			if ( Custinternalcollection . Items . Count > 0 )
				Custinternalcollection . ClearItems ( );

				#region process code to load data

			Task t1 = Task . Run (
					async ( ) =>
					{
						await LoadCustDataSql ( );
					}
				);
			t1 . ContinueWith
			(
				async ( Custinternalcollection ) =>
				{
					await LoadCustomerCollection ( );
				}, TaskScheduler . FromCurrentSynchronizationContext ( )
			 );

			#endregion process code to load data

			#region Success//Error reporting/handling

			// Now handle "post processing of errors etc"
			//This will ONLY run if there were No Exceptions  and it ALL ran successfully!!
			t1 . ContinueWith (
				( Custinternalcollection ) =>
				{
					Debug . WriteLine ( $"CUSTOMERS : Task.Run() Completed : Status was [ {Custinternalcollection . Status} ]." );
				}, CancellationToken . None, TaskContinuationOptions . OnlyOnRanToCompletion, TaskScheduler . FromCurrentSynchronizationContext ( )
			);
			//This will iterate through ALL of the Exceptions that may have occured in the previous Tasks
			// but ONLY if there were any Exceptions !!
			t1 . ContinueWith (
				( Custinternalcollection ) =>
				{
					AggregateException ae = t1 . Exception . Flatten ( );
					Debug . WriteLine ( $"Exception in Custinternalcollection  data processing \n" );
					MessageBox . Show ( $"Exception in CustCollection  data processing \n" );
					foreach ( var item in ae . InnerExceptions )
					{
						Debug . WriteLine ( $"CustCollection : Exception : {item . Message}, : {item . Data}" );
					}
				}, CancellationToken . None, TaskContinuationOptions . OnlyOnFaulted, TaskScheduler . FromCurrentSynchronizationContext ( )
			);
			//			Debug . WriteLine ( $"CUSTOMER : END OF PROCESSING & Error checking functionality\nCUSTOMER : *** Detcollection total = {Custinternalcollection . Count} ***\n\n" );

			#endregion Success//Error reporting/handling
			Flags . CustCollection = Custinternalcollection;
			return Custinternalcollection;

			//// Load data fro SQL into dtBank Datatable
			//CustCollection c = new CustCollection ( );
			//await c . LoadCustDataSql ( ) . ConfigureAwait ( false );
			//// this returns "Bankinternalcollection" as a pointer to the correct viewer
			//await LoadCustomerTest ( ) . ConfigureAwait ( false );

		}




		/// Handles the actual conneciton ot SQL to load the Details Db data required
		/// </summary>
		/// <returns></returns>
		public async Task<bool> LoadCustDataSql ( DataTable dt = null, int mode = -1, bool isMultiMode = false )
		//Load data from Sql Server
		{

			try
			{
				SqlConnection con;
				string ConString = "";
				ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				con = new SqlConnection ( ConString );

				using ( con )
				{
					string commandline = "";

					if ( Flags . IsMultiMode )
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = $"SELECT * FROM CUSTOMER WHERE CUSTNO IN "
							+ $"(SELECT CUSTNO FROM CUSTOMER  "
							+ $" GROUP BY CUSTNO"
							+ $" HAVING COUNT(*) > 1) ORDER BY ";
						commandline = Utils . GetDataSortOrder ( commandline );
					}
					else if ( Flags . FilterCommand != "" )
					{ commandline = Flags . FilterCommand; }
					else
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = "Select * from Customer  order by ";
						commandline = Utils . GetDataSortOrder ( commandline );
					}
					SqlCommand cmd = new SqlCommand ( commandline, con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					CustCollection bptr = new CustCollection ( );
					//					lock ( bptr . LockCustReadData )
					//					{
					sda . Fill ( dtCust );
					//					Debug . WriteLine ( $"CUSTOMERS : Sql data loaded into Customers DataTable [{dtCust . Rows . Count}] ...." );
					//					}
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Failed to load Customer Details - {ex . Message}" );
				MessageBox . Show ( $"Failed to load Customer Details - {ex . Message}" );
				return false;
			}

			return true;
		}

		//**************************************************************************************************************************************************************//
		private static async Task<CustCollection> LoadCustomerCollection ( bool DoNotify = false )

		{
			int count = 0;
//			CustCollection bptr = new CustCollection ( );
			//			lock ( bptr . LockCustLoadData )
			//			{
			try
			{
				for ( int i = 0 ; i < dtCust . Rows . Count ; i++ )
				{
					Custinternalcollection . Add ( new CustomerViewModel
					{
						Id = Convert . ToInt32 ( dtCust . Rows [ i ] [ 0 ] ),
						CustNo = dtCust . Rows [ i ] [ 1 ] . ToString ( ),
						BankNo = dtCust . Rows [ i ] [ 2 ] . ToString ( ),
						AcType = Convert . ToInt32 ( dtCust . Rows [ i ] [ 3 ] ),
						FName = dtCust . Rows [ i ] [ 4 ] . ToString ( ),
						LName = dtCust . Rows [ i ] [ 5 ] . ToString ( ),
						Addr1 = dtCust . Rows [ i ] [ 6 ] . ToString ( ),
						Addr2 = dtCust . Rows [ i ] [ 7 ] . ToString ( ),
						Town = dtCust . Rows [ i ] [ 8 ] . ToString ( ),
						County = dtCust . Rows [ i ] [ 9 ] . ToString ( ),
						PCode = dtCust . Rows [ i ] [ 10 ] . ToString ( ),
						Phone = dtCust . Rows [ i ] [ 11 ] . ToString ( ),
						Mobile = dtCust . Rows [ i ] [ 12 ] . ToString ( ),
						Dob = Convert . ToDateTime ( dtCust . Rows [ i ] [ 13 ] ),
						ODate = Convert . ToDateTime ( dtCust . Rows [ i ] [ 14 ] ),
						CDate = Convert . ToDateTime ( dtCust . Rows [ i ] [ 15 ] )
					} );
					count = i;
				}
				//Debug . WriteLine ( $"CUSTOMER : Sql data loaded into Customer ObservableCollection \"Custinternalcollection\" [{count}] ...." );
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"CUSTOMERS : ERROR {ex . Message} + {ex . Data} ...." );
				Custinternalcollection = null;
			}
			finally
			{
				if ( Notify && count > 0 )
				{
					EventControl . TriggerCustDataLoaded ( null,
						new LoadedEventArgs
						{
							CallerDb = "CUSTOMER",
							DataSource = Custinternalcollection,
							RowCount = Custinternalcollection . Count
						} );
				}
			}
			//			} // End Lock
			return Custinternalcollection;
		}
		public async static Task<CustCollection> LoadCustomerTest ( bool Notify = true )

		{
			int count = 0;
			try
			{
				for ( int i = 0 ; i < dtCust . Rows . Count ; i++ )
				{
					Custinternalcollection . Add ( new CustomerViewModel
					{
						Id = Convert . ToInt32 ( dtCust . Rows [ i ] [ 0 ] ),
						CustNo = dtCust . Rows [ i ] [ 1 ] . ToString ( ),
						BankNo = dtCust . Rows [ i ] [ 2 ] . ToString ( ),
						AcType = Convert . ToInt32 ( dtCust . Rows [ i ] [ 3 ] ),
						FName = dtCust . Rows [ i ] [ 4 ] . ToString ( ),
						LName = dtCust . Rows [ i ] [ 5 ] . ToString ( ),
						Addr1 = dtCust . Rows [ i ] [ 6 ] . ToString ( ),
						Addr2 = dtCust . Rows [ i ] [ 7 ] . ToString ( ),
						Town = dtCust . Rows [ i ] [ 8 ] . ToString ( ),
						County = dtCust . Rows [ i ] [ 9 ] . ToString ( ),
						PCode = dtCust . Rows [ i ] [ 10 ] . ToString ( ),
						Phone = dtCust . Rows [ i ] [ 11 ] . ToString ( ),
						Mobile = dtCust . Rows [ i ] [ 12 ] . ToString ( ),
						Dob = Convert . ToDateTime ( dtCust . Rows [ i ] [ 13 ] ),
						ODate = Convert . ToDateTime ( dtCust . Rows [ i ] [ 14 ] ),
						CDate = Convert . ToDateTime ( dtCust . Rows [ i ] [ 15 ] )
					} );
					count = i;
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"CUSTOMERS : ERROR {ex . Message} + {ex . Data} ...." );
				MessageBox . Show ( $"CUSTOMERS : ERROR :\n		Error was  : [{ex . Message}] ...." );
			}
			Flags . CustCollection = Custinternalcollection;
			return Custinternalcollection;
		}

		#endregion startup/load data / load collection (Custinternalcollection)
		public static bool SelectViewer ( int ViewerType, CustCollection tmp )
		{
			bool result = false;
			switch ( ViewerType )
			{
				case 1:
					SqlViewerCustcollection = tmp;
					result = true;
					break;
				case 2:
					EditDbCustcollection = tmp;
					result = true;
					break;
				case 3:
					MultiCustcollection = tmp;
					result = true;
					break;
				case 4:
					CustViewerDbcollection = tmp;
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

		//**************************************************************************************************************************************************************//
		// Entry point for all data load/Reload
		//**************************************************************************************************************************************************************//
		// NO LONGER USED
		public static bool UpdateCustomerDb ( CustomerViewModel NewData )
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
					SqlCommand cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
					cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( NewData . Id ) );
					cmd . Parameters . AddWithValue ( "@bankno", NewData . BankNo . ToString ( ) );
					cmd . Parameters . AddWithValue ( "@custno", NewData . CustNo . ToString ( ) );
					cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( NewData . AcType ) );
					cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( NewData . ODate ) );
					cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( NewData . CDate ) );
					cmd . ExecuteNonQuery ( );
					Debug . WriteLine ( "SQL Update of Customers successful..." );
				}
			}
			catch ( Exception ex )
			{ Console . WriteLine ( $"CUSTOMER Update FAILED : {ex . Message}, {ex . Data}" ); }
			finally
			{ con . Close ( ); }
			return true;
		}
	}
}

