using System;
using System . Collections . ObjectModel;
using System . Data;
using System . Data . SqlClient;
using System . Diagnostics;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Threading;

namespace WPFPages . Views
{
	/// <summary>
	/// Class to hold the Customers data for the system as an Observabke collection
	/// </summary>
	public class CustCollection : ObservableCollection<CustomerViewModel>
	{
		//Declare a global pointer to Observable Customers Collection
		public static CustCollection Custcollection = new CustCollection();
		public static DataTable dtCust = new DataTable();
		public  static CustCollection CustViewerDbcollection=new CustCollection ();
		public  static CustCollection SqlViewerCustcollection=new CustCollection ();
		public  static CustCollection  EditDbCustcollection=new CustCollection ();
		public  static CustCollection  MultiCustcollection=new CustCollection  ();
		public  static CustCollection  Custinternalcollection=new CustCollection  ();
		public  static BankCollection temp =new BankCollection();

		#region CONSTRUCTOR

		public CustCollection ( ) : base ( )
		{
		}

		#endregion CONSTRUCTOR

		#region startup/load data / load collection (CustCollection)
		public static CustCollection LoadCust ( CustCollection cc , int ViewerType = 1)
		{
			// Called to Load/reload the One & Only Bankcollection data source
			if ( dtCust . Rows . Count > 0 )
				dtCust . Clear ( );
			if ( cc != null )
				Custinternalcollection = cc;
			else
				Custinternalcollection = new CustCollection ( );

			if ( Custinternalcollection . Count > 0 )
				Custinternalcollection . ClearItems ( );

			CustCollection c = new CustCollection();
			c . LoadCustDataSql ( );

			if ( dtCust . Rows . Count > 0 )
				Custinternalcollection = LoadCustomerTest ( );
			// We now have the ONE AND ONLY pointer the the Bank data in variable Bankcollection
			Flags . CustCollection = Custinternalcollection;
			SqlViewerCustcollection = Custinternalcollection;
			if ( Flags . IsMultiMode == false )
			{
				// Finally fill and return The global Dataset
				SelectViewer ( ViewerType , Custinternalcollection );
				return Custinternalcollection;
			}
			else
			{
				// return the "working  copy" pointer, it has  filled the relevant collection to match the viewer
				return Custinternalcollection;
			}
		}

		/// Handles the actual conneciton ot SQL to load the Details Db data required
		/// </summary>
		/// <returns></returns>
		public async Task<bool> LoadCustDataSql ( DataTable dt = null , int mode = -1 , bool isMultiMode = false )
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
					sda . Fill ( dtCust );
					Console . WriteLine ( $"CUSTOMERS : Sql data loaded into Customers DataTable [{dtCust . Rows . Count}] ...." );
				}
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"Failed to load Customer Details - {ex . Message}" );
				MessageBox . Show ( $"Failed to load Customer Details - {ex . Message}" );
				return false;
			}
			return true;
		}

		//**************************************************************************************************************************************************************//
		private static async Task<bool> LoadCustomerCollection ( bool Notify = true )

		{
			int count = 0;
			try
			{
				for ( int i = 0 ; i < dtCust . Rows . Count ; i++ )
				{
					Custinternalcollection . Add ( new CustomerViewModel
					{
						Id = Convert . ToInt32 ( dtCust . Rows [ i ] [ 0 ] ) ,
						CustNo = dtCust . Rows [ i ] [ 1 ] . ToString ( ) ,
						BankNo = dtCust . Rows [ i ] [ 2 ] . ToString ( ) ,
						AcType = Convert . ToInt32 ( dtCust . Rows [ i ] [ 3 ] ) ,
						FName = dtCust . Rows [ i ] [ 4 ] . ToString ( ) ,
						LName = dtCust . Rows [ i ] [ 5 ] . ToString ( ) ,
						Addr1 = dtCust . Rows [ i ] [ 6 ] . ToString ( ) ,
						Addr2 = dtCust . Rows [ i ] [ 7 ] . ToString ( ) ,
						Town = dtCust . Rows [ i ] [ 8 ] . ToString ( ) ,
						County = dtCust . Rows [ i ] [ 9 ] . ToString ( ) ,
						PCode = dtCust . Rows [ i ] [ 10 ] . ToString ( ) ,
						Phone = dtCust . Rows [ i ] [ 11 ] . ToString ( ) ,
						Mobile = dtCust . Rows [ i ] [ 12 ] . ToString ( ) ,
						Dob = Convert . ToDateTime ( dtCust . Rows [ i ] [ 13 ] ) ,
						ODate = Convert . ToDateTime ( dtCust . Rows [ i ] [ 14 ] ) ,
						CDate = Convert . ToDateTime ( dtCust . Rows [ i ] [ 15 ] )
					} );
					count = i;
				}
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"CUSTOMERS : ERROR {ex . Message} + {ex . Data} ...." );
				MessageBox . Show ( $"CUSTOMERS : ERROR :\n		Error was  : [{ex . Message}] ...." );
			}
			if ( Notify )
			{
				EventControl . TriggerCustDataLoaded ( null ,
					new LoadedEventArgs
					{
						CallerDb = "CUSTOMER" ,
						DataSource = Custinternalcollection ,
						RowCount = Custinternalcollection . Count
					} );
			}
			Flags . CustCollection = Custinternalcollection;
			return true;
		}
		private static CustCollection LoadCustomerTest ( bool Notify = true )

		{
			int count = 0;
			try
			{
				for ( int i = 0 ; i < dtCust . Rows . Count ; i++ )
				{
					Custinternalcollection . Add ( new CustomerViewModel
					{
						Id = Convert . ToInt32 ( dtCust . Rows [ i ] [ 0 ] ) ,
						CustNo = dtCust . Rows [ i ] [ 1 ] . ToString ( ) ,
						BankNo = dtCust . Rows [ i ] [ 2 ] . ToString ( ) ,
						AcType = Convert . ToInt32 ( dtCust . Rows [ i ] [ 3 ] ) ,
						FName = dtCust . Rows [ i ] [ 4 ] . ToString ( ) ,
						LName = dtCust . Rows [ i ] [ 5 ] . ToString ( ) ,
						Addr1 = dtCust . Rows [ i ] [ 6 ] . ToString ( ) ,
						Addr2 = dtCust . Rows [ i ] [ 7 ] . ToString ( ) ,
						Town = dtCust . Rows [ i ] [ 8 ] . ToString ( ) ,
						County = dtCust . Rows [ i ] [ 9 ] . ToString ( ) ,
						PCode = dtCust . Rows [ i ] [ 10 ] . ToString ( ) ,
						Phone = dtCust . Rows [ i ] [ 11 ] . ToString ( ) ,
						Mobile = dtCust . Rows [ i ] [ 12 ] . ToString ( ) ,
						Dob = Convert . ToDateTime ( dtCust . Rows [ i ] [ 13 ] ) ,
						ODate = Convert . ToDateTime ( dtCust . Rows [ i ] [ 14 ] ) ,
						CDate = Convert . ToDateTime ( dtCust . Rows [ i ] [ 15 ] )
					} );
					count = i;
				}
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"CUSTOMERS : ERROR {ex . Message} + {ex . Data} ...." );
				MessageBox . Show ( $"CUSTOMERS : ERROR :\n		Error was  : [{ex . Message}] ...." );
			}
			Flags . CustCollection = Custinternalcollection;
			return Custinternalcollection;
		}

		#endregion startup/load data / load collection (Custinternalcollection)
		public static bool SelectViewer ( int ViewerType , CustCollection tmp )
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

		private void notused ( )
		{
			//**************************************************************************************************************************************************************//
			// Entry point for all data load/Reload
			//**************************************************************************************************************************************************************//
			// NO LONGER USED
			//public async Task<CustCollection> LoadCustomerTaskInSortOrderAsync ( bool isOriginator = false , int mode = -1 )
			//{
			//	if ( dtCust . Rows . Count > 0 )
			//		dtCust . Clear ( );

			//	if ( Custinternalcollection . Items . Count > 0 )
			//		Custinternalcollection . ClearItems ( );

			//	// This all woks just fine, and DOES switch back to UI thread that is MANDATORY before doing the Collection load processing
			//	// thanks to the use of TaskScheduler.FromCurrentSynchronizationContext() that oerforms the magic switch back to the UI thread
			//	//			Console . WriteLine ( $"CUSTOMERS : Entering Method to call Task.Run in CustCollection  : Thread = { Thread . CurrentThread . ManagedThreadId}" );

			//	#region process code to load data

			//	Task t1 = Task . Run(
			//			async ( ) =>
			//			{
			//				await LoadCustDataSql ();
			//			}
			//		);
			//	t1 . ContinueWith
			//	(
			//		async ( Custinternalcollection ) =>
			//		{
			//			await LoadCustomerCollection ( );
			//		} , TaskScheduler . FromCurrentSynchronizationContext ( )
			//	 );

			//	#endregion process code to load data

			//	#region Success//Error reporting/handling

			//	// Now handle "post processing of errors etc"
			//	//This will ONLY run if there were No Exceptions  and it ALL ran successfully!!
			//	t1 . ContinueWith (
			//		( Custinternalcollection ) =>
			//		{
			//			Console . WriteLine ( $"CUSTOMERS : Task.Run() Completed : Status was [ {Custinternalcollection . Status} ]." );
			//		} , CancellationToken . None , TaskContinuationOptions . OnlyOnRanToCompletion , TaskScheduler . FromCurrentSynchronizationContext ( )
			//	);
			//	//This will iterate through ALL of the Exceptions that may have occured in the previous Tasks
			//	// but ONLY if there were any Exceptions !!
			//	t1 . ContinueWith (
			//		( Custinternalcollection ) =>
			//		{
			//			AggregateException ae =  t1 . Exception . Flatten ( );
			//			Console . WriteLine ( $"Exception in Custinternalcollection  data processing \n" );
			//			MessageBox . Show ( $"Exception in CustCollection  data processing \n" );
			//			foreach ( var item in ae . InnerExceptions )
			//			{
			//				Console . WriteLine ( $"CustCollection : Exception : {item . Message}, : {item . Data}" );
			//			}
			//		} , CancellationToken . None , TaskContinuationOptions . OnlyOnFaulted , TaskScheduler . FromCurrentSynchronizationContext ( )
			//	);
			//	Console . WriteLine ( $"CUSTOMER : END OF PROCESSING & Error checking functionality\nCUSTOMER : *** Detcollection total = {Custinternalcollection . Count} ***\n\n" );

			//	#endregion Success//Error reporting/handling
			//	Flags . CustCollection = Custinternalcollection;
			//	return Custinternalcollection;
		}
	}
}

