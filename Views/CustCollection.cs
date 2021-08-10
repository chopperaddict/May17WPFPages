﻿using System;
using WPFPages;

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
//		public static CustCollection Custcollection = new CustCollection ( );
		public static DataTable dtCust = new DataTable ( );
		//public static CustCollection CustViewDbcollection = new CustCollection ( );
		//public static CustCollection SqlViewerCustcollection = new CustCollection ( );
		//public static CustCollection EditDbCustcollection = new CustCollection ( );
		//public static CustCollection MultiCustcollection = new CustCollection ( );
		public static CustCollection Custinternalcollection = new CustCollection ( );
		//public static BankCollection temp = new BankCollection ( );
		public static bool USEFULLTASK = true;
		public static bool Notify = false;

		public static string Caller = "";
		#region CONSTRUCTOR

		public CustCollection ( ) 
		{
		}

		#endregion CONSTRUCTOR

		#region startup/load data / load collection (CustCollection)
		public async static Task<CustCollection> LoadCust ( CustCollection cc, string caller, int ViewerType = 1, bool NotifyAll = false )
		{
			Notify = NotifyAll;
			Caller = caller;
			try
			{
				object lockobj = new object ( );

				if ( USEFULLTASK )
				{
					{
						Custinternalcollection = new CustCollection ( );
						Custinternalcollection . LoadCustomerTaskInSortOrderAsync ( );
					}
					return Custinternalcollection;
				}
				else
				{
					//// We now have the ONE AND ONLY pointer the the Bank data in variable Bankcollection
					//Flags . CustCollection = Custinternalcollection;
					//if ( Flags . IsMultiMode == false )
					//{
					//	// Finally fill and return The global Dataset
					//	SelectViewer ( ViewerType, Custinternalcollection );
					//	return null;
					//}
					//else
					//{
					//	// return the "working  copy" pointer, it has  filled the relevant collection to match the viewer
					//	return null;
					//}
				}
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

		private static async Task<CustCollection> ProcessRequest ( int ViewerType )
		{
			object lockobject = new object ( );

			if ( USEFULLTASK )
			{
				{
					Custinternalcollection = null;
					Custinternalcollection = new CustCollection ( );
					CustCollection cc = new CustCollection ( );
					cc . LoadCustomerTaskInSortOrderAsync ( );
				}
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

			if ( dtCust . Rows . Count > 0 )
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
					//					Debug . WriteLine ( $"CUSTOMERS : Task.Run() Completed : Status was [ {Custinternalcollection . Status} ]." );
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
			object bptr = new object ( );

			SqlConnection con;
			string ConString = "";
			ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
			Debug . WriteLine ( $"Making new SQL connection in CUSTCOLLECTION" );
			con = new SqlConnection ( ConString );
			//if ( con== null )
			//{
			//	Debug . WriteLine ( $"CUSTCOLLECTION : No SQL Connection, reconnecting ..." );
			//	con= new SqlConnection ( ConString );
			//}
			try
			{
				using ( con )
				{
					Debug . WriteLine ( $"Loading dtCust in CUSTCOLLECTION" );
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
					//CustCollection bptr = new CustCollection ( );
					//					lock ( bptr . LockCustReadData )
					//					{
//					lock ( bptr )
//					{
						sda . Fill ( dtCust );
						Debug . WriteLine ( $"CUSTOMERS : dtCust loaded [{dtCust . Rows . Count}] ...." );
										//					}
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Failed to load Customer Details - {ex . Message}" );
				MessageBox . Show ( $"Failed to load Customer Details - {ex . Message}" );
				return false;
			}
			finally
			{
				con . Close ( );
			}

			return true;
		}

		//**************************************************************************************************************************************************************//
		private static async Task<CustCollection> LoadCustomerCollection ( )
		{
			object bptr = new object ( );
			int count = 0;
			//			CustCollection bptr = new CustCollection ( );
			//			lock ( bptr . LockCustLoadData )
			//			{
			//			lock ( bptr )
			Flags . SqlCustActive  = true;
			{
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
								CallerType = "SQLSERVER",
								CallerDb = Caller,
								DataSource = Custinternalcollection,
								RowCount = Custinternalcollection . Count
							} );
					}
				}
			} // End Lock
			  //			Custinternalcollection = null;
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
				//case 1:
				//	SqlViewerCustcollection = tmp;
				//	result = true;
				//	break;
				//case 2:
				//	EditDbCustcollection = tmp;
				//	result = true;
				//	break;
				//case 3:
				//	MultiCustcollection = tmp;
				//	result = true;
				//	break;
				//case 4:
				//	CustViewDbcollection = tmp;
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
				default:
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
		public static bool UpdateCustomerDb ( CustomerViewModel NewData , string CallerType)
		{

			SqlConnection con;
			SqlCommand cmd = null;
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
					if (CallerType == "CUSTOMER")
					{
						cmd = new SqlCommand("UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, " + 
							"FNAME=@FName, LNAME=@LName, ADDR1=Addr1, ADDR2=@Addr2, TOWN=@Town, COUNTY=@County," +
							" PCODE=@PCode, PHONE=@Phone, MOBILE=@Mobile, DOB=@dob, ODATE=@odate, CDATE=@cdate " + 
							" where CUSTNO = @custno AND BANKNO = @bankno", con);
						cmd.Parameters.AddWithValue("@id", Convert.ToInt32(NewData.Id));
						cmd.Parameters.AddWithValue("@bankno", NewData.BankNo.ToString());
						cmd.Parameters.AddWithValue("@custno", NewData.CustNo.ToString());
						cmd.Parameters.AddWithValue("@actype", Convert.ToInt32(NewData.AcType));
						cmd.Parameters.AddWithValue("@fname", NewData.FName.ToString());
						cmd.Parameters.AddWithValue("@lname", NewData.LName.ToString());
						cmd.Parameters.AddWithValue("@addr1", NewData.Addr1.ToString());
						cmd.Parameters.AddWithValue("@addr2", NewData.Addr2.ToString());
						cmd.Parameters.AddWithValue("@town", NewData.Town.ToString());
						cmd.Parameters.AddWithValue("@county", NewData.County.ToString());
						cmd.Parameters.AddWithValue("@pcode", NewData.PCode.ToString());
						cmd.Parameters.AddWithValue("@phone", NewData.Phone.ToString());
						cmd.Parameters.AddWithValue("@mobile", NewData.Mobile.ToString());
						cmd.Parameters.AddWithValue("@dob", Convert.ToDateTime(NewData.Dob));
						cmd.Parameters.AddWithValue("@odate", Convert.ToDateTime(NewData.ODate));
						cmd.Parameters.AddWithValue("@cdate", Convert.ToDateTime(NewData.CDate));
					}
					else
					{
						cmd = new SqlCommand("UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno", con);
						cmd.Parameters.AddWithValue("@id", Convert.ToInt32(NewData.Id));
						cmd.Parameters.AddWithValue("@bankno", NewData.BankNo.ToString());
						cmd.Parameters.AddWithValue("@custno", NewData.CustNo.ToString());
						cmd.Parameters.AddWithValue("@actype", Convert.ToInt32(NewData.AcType));
						cmd.Parameters.AddWithValue("@odate", Convert.ToDateTime(NewData.ODate));
						cmd.Parameters.AddWithValue("@cdate", Convert.ToDateTime(NewData.CDate));
					}
					cmd . ExecuteNonQuery ( );
					Debug . WriteLine ( "SQL Update of Customers successful..." );
				}
			}
			catch ( Exception ex )
			{ Console . WriteLine ( $"CUSTOMER Update FAILED : {ex . Message}, {ex . Data}" ); }
			return true;
		}

		#region EXPORT FUNCTIONS TO READ/WRITE CSV files
		public static DataTable LoadCustExportData ( )
		{
			DataTable dt = new DataTable ( );
			SqlConnection con;
			string ConString = "";
			string commandline = "";
			ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
			Debug . WriteLine ( $"Making new SQL connection in CUSTCOLLECTION" );
			con = new SqlConnection ( ConString );
			try
			{
				Debug . WriteLine ( $"Using new SQL connection in CUSTCOLLECTION" );
				using ( con )
				{
					if ( Flags . IsMultiMode )
					{
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
					}
					else
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = "Select * from Customer  order by ";
						commandline = Utils . GetDataSortOrder ( commandline );
					}
					SqlCommand cmd = new SqlCommand ( commandline, con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dt );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"DETAILS : ERROR in LoadCustDataSql(): Failed to load Customer Details - {ex . Message}, {ex . Data}" );
				MessageBox . Show ( $"DETAILS : ERROR in LoadCustDataSql(): Failed to load Customer Details - {ex . Message}, {ex . Data}" );
			}
			finally
			{ con . Close ( ); }
			return dt;
		}

		public static int ExportCustData ( string path, string dbType )
		{
			int count = 0;
			string output = "";

			// Read data in from disk first as a DataTable dt
			DataSet ds = new DataSet ( );

			DataTable dt = new DataTable ( );
			dt = LoadCustExportData ( );
			ds . Tables . Add ( dt );
			//£££££££££££££££££££££££££££££££££££££££££££
			// This works just fine with no external binding.
			// The data is  now accessible in ds.Tables[0].Rows
			// NB DATA ACCESS FORMAT IS [ $"{objRow["CustNo"]}"  ]
			//££££££££££££££££££££££££££££££££££££££££££££
			Console . WriteLine ( $"Writing results of SQL enquiry to {path} ..." );
			foreach ( DataRow objRow in ds . Tables [ 0 ] . Rows )
			{
				output += ParseDbRow ( "CUSTOMER", objRow );
				count++;
			}
			System . IO . File . WriteAllText ( path, output );
			Console . WriteLine ( $"Export of {count - 1} records from the [ {dbType} ] Db in CSV format has been completed successfully." );
			return count;
		}

		//===============================================================================
		public static string ParseDbRow ( string dbType, DataRow objRow )
		//===============================================================================

		{
			string tmp = "", s = "";
			string [ ] dob, odat, cdat, revstr;
			if ( dbType == "CUSTOMER" )
			{
				char [ ] ch = { ' ' };
				char [ ] ch2 = { '/' };

				// Do the Dob
				s = $"{objRow [ "Dob" ] . ToString ( )}', '";
				dob = s . Split ( ch );
				string doB = dob [ 0 ];
				// now reverse it  to YYYY/MM/DD format as this is what SQL understands
				revstr = doB . Split ( ch2 );
				doB = revstr [ 2 ] + "/" + revstr [ 1 ] + "/" + revstr [ 0 ];

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

				tmp = $"{objRow [ "Id" ] . ToString ( )}, "
					+ $"{objRow [ "BankNo" ] . ToString ( )}, "
					+ $"{objRow [ "CustNo" ] . ToString ( )}, "
					+ $"{acTypestr}, "
					+ $"'{objRow [ "Fname" ] . ToString ( )}', "
					+ $"'{objRow [ "Lname" ] . ToString ( )}', "

					+ $"'{objRow [ "Addr1" ] . ToString ( )}', "
					+ $"'{objRow [ "Addr2" ] . ToString ( )}', "
					+ $"'{objRow [ "Town" ] . ToString ( )}', "
					+ $"'{objRow [ "County" ] . ToString ( )}', "
					+ $"'{objRow [ "Pcode" ] . ToString ( )}', "
					+ $"'{objRow [ "Phone" ] . ToString ( )}', "
					+ $"'{objRow [ "Mobile" ] . ToString ( )}', "
					+ $"'{doB}', "
					+ $"'{odate}', "
					+ $"'{cdate}'\r\n";
			}
			return tmp;
		}
		#endregion EXPORT FUNCTIONS TO READ/WRITE CSV files

	}
}

