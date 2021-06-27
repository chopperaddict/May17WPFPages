using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WPFPages.SqlDbViewer;
using WPFPages.Properties;
using WPFPages.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Threading;

namespace WPFPages.Views
{
	public class TestBankCollection : ObservableCollection<BankAccountViewModel>
	{
		//		//Declare a global pointer to Observable BankAccount Collection
		public static TestBankCollection TestBankinternalcollection = new TestBankCollection();
		public static TestBankCollection temp = new TestBankCollection();

		public static DataTable dtBank = new DataTable("BankDataTable");
		public static bool USEFULLTASK = true;
		public static bool Notify = false;
		public static string Caller = "";

		// Object used to lock Data Load from Sql and load into collection
		private readonly object LockBankReadData = new object();
		private readonly object LockBankLoadData = new object();
		// Lock object

		public ObservableCollection<BankAccountViewModel> TestBnkCollection = new ObservableCollection<BankAccountViewModel>();

		#region CONSTRUCTOR

		public TestBankCollection() : base()
		{
		}
		public async static Task<TestBankCollection> LoadBank(TestBankCollection cc, string caller, int ViewerType = 1, bool NotifyAll = false)
		{
			//			object lockobject = new object();
			Notify = NotifyAll;
			Caller = caller;
			try
			{
				// Called to Load/reload the One & Only TestBankCollection data source
				//if ( dtBank . Rows . Count > 0 )
				//	dtBank . Clear ( );

				//				Debug . WriteLine ( $"\n ***** SQL WARNING Created a NEW MasterBankCollection ..................." );
				if (USEFULLTASK)
				{
					// lock the process - JIC
					TestBankinternalcollection = null;
					TestBankinternalcollection = new TestBankCollection();
					TestBankinternalcollection.LoadBankTaskInSortOrderasync();
					return TestBankinternalcollection;
				}
				else
				{
					//					Debug . WriteLine ( $"\n ***** Loading BankAccount Data from disk (using Abbreviated Await Control system)*****\n" );
					TestBankinternalcollection.ClearItems();
					// Abstract the main data load call to a method that uses AWAITABLE  calles
					ProcessRequest().ConfigureAwait(false);

					// We now have the pointer to the Bank data in variable TestBankinternalcollection
					if (Flags.IsMultiMode == false)
					{
						TestBankCollection db = new TestBankCollection();
						SelectViewer(ViewerType, TestBankinternalcollection);
						return db;
					}
					else
					{
						// return the "working  copy" pointer, it has  filled the relevant collection to match the viewer
						return null;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Bank Load Exception : {ex.Message}, {ex.Data}");
				return null;
			}
		}

		private static async Task ProcessRequest()
		{
			// Load data fro SQL into dtBank Datatable
			LoadBankData();
			// this returns "TestBankinternalcollection" as a pointer to the correct viewer
			TestBankinternalcollection = await LoadBankCollection().ConfigureAwait(false);
		}

		public async Task<TestBankCollection> LoadBankTaskInSortOrderasync(bool Notify = false, int i = -1)
		// No longer used
		{
			if (dtBank.Rows.Count > 0)
				dtBank.Clear();

			if (TestBankinternalcollection.Items.Count > 0)
				TestBankinternalcollection.ClearItems();
			//}
			#region process code to load data

			Task t1 = Task.Run(
			async () =>
			{
				LoadBankData();
			}
			);
			#region Continuations
			t1.ContinueWith
			(
				async (TestBankinternalcollection) =>
				{
					LoadBankCollection();
					//					Debug . WriteLine ( $"Just Called LoadBankCollection () in second Task.Run() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
				}, TaskScheduler.FromCurrentSynchronizationContext()
			 );
			#endregion process code to load data

			#region Success//Error reporting/handling

			// Now handle "post processing of errors etc"
			//This will ONLY run if there were No Exceptions  and it ALL ran successfully!!
			t1.ContinueWith(
			(TestBankinternalcollection) =>
				{
								//					Debug . WriteLine ( $"BANKACCOUNT : Task.Run() Completed : Status was [ {TestBankinternalcollection . Status}" );
							}, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext()
			);
			//This will iterate through ALL of the Exceptions that may have occured in the previous Tasks
			// but ONLY if there were any Exceptions !!
			t1.ContinueWith(
				(TestBankinternalcollection) =>
				{
					AggregateException ae = t1.Exception.Flatten();
					Debug.WriteLine($"Exception in TestBankCollection data processing \n");
					MessageBox.Show($"Exception in TestBankCollection data processing \n");
					foreach (var item in ae.InnerExceptions)
					{
						Debug.WriteLine($"TestBankCollection : Exception : {item.Message}, : {item.Data}");
					}
				}, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.FromCurrentSynchronizationContext()
			);

			// Now handle "post processing of errors etc"
			//This will ONLY run if there were No Exceptions  and it ALL ran successfully!!
			t1.ContinueWith(
				(TestBankinternalcollection) =>
				{
								//					Debug . WriteLine ( $"TestBankCollection : Task.Run() processes all succeeded. \nBankcollection Status was [ {TestBankinternalcollection . Status} ]." );
								//					Console . WriteLine ( $"BANKACCOUNT : Task.Run() Completed : Status was [ {TestBankinternalcollection . Status} ]." );
							}, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext()
			);
			//This will iterate through ALL of the Exceptions that may have occured in the previous Tasks
			// but ONLY if there were any Exceptions !!
			t1.ContinueWith(
				(TestBankinternalcollection) =>
				{
					AggregateException ae = t1.Exception.Flatten();
					Debug.WriteLine($"Exception in TestBankCollection data processing \n");
					MessageBox.Show($"Exception in TestBankCollection data processing \n");
					foreach (var item in ae.InnerExceptions)
					{
						Debug.WriteLine($"TestBankCollection : Exception : {item.Message}, : {item.Data}");
					}
				}, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.FromCurrentSynchronizationContext()
			);

			#endregion Continuations

			Debug.WriteLine($"BANKACCOUNT : END OF PROCESSING & Error checking functionality\nBANKACCOUNT : *** TestBankCollection total = {TestBankinternalcollection.Count} ***\n\n");

			#endregion Success//Error reporting/handling
			
			//Finally fill and return The global Dataset
			//Flags.TestBankCollection = TestBankinternalcollection;
			//MasterBankcollection = TestBankinternalcollection;
			//TestBankCollection = TestBankinternalcollection;
			return null;
		}

		public static bool SelectViewer(int ViewerType, TestBankCollection tmp)
		{
			bool result = false;
			switch (ViewerType)
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
		#endregion CONSTRUCTOR

		#region LOAD THE DATA


		/// <summary>
		/// Method used ONLY when working with Multi accounts data
		/// </summary>
		/// <param name="b"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		public async Task<TestBankCollection> ReLoadBankData(bool b = false, int mode = -1)
		{
			if (dtBank.Rows.Count > 0)
				dtBank.Clear();

			//LoadBankTaskInSortOrderasync ( false );
			TestBankCollection temp = new TestBankCollection();
			if (temp.Count > 0)
			{
				temp.ClearItems();
			}

			await ProcessRequest();

			return TestBankinternalcollection;
		}

		/// Handles the actual conneciton ot SQL to load the Details Db data required
		/// </summary>
		/// <returns></returns>
		public static void LoadBankData(int mode = -1, bool isMultiMode = false)
		{
			object bptr = new object();
			try
			{
				SqlConnection con;
				string ConString = "";
				ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
				Debug.WriteLine($"Making new SQL connection in TestBankCollection");
				con = new SqlConnection(ConString);

				using (con)
				{
					Debug.WriteLine($"Using new SQL connection in TestBankCollection");
					string commandline = "";

					if (Flags.IsMultiMode)
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = $"SELECT * FROM BANKACCOUNT WHERE CUSTNO IN "
							+ $"(SELECT CUSTNO FROM BANKACCOUNT "
							+ $" GROUP BY CUSTNO"
							+ $" HAVING COUNT(*) > 1) ORDER BY ";

						commandline = Utils.GetDataSortOrder(commandline);
					}
					else if (Flags.FilterCommand != "")
					{ commandline = Flags.FilterCommand; }
					else
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = "Select * from BankAccount order by ";
						commandline = Utils.GetDataSortOrder(commandline);
					}
					lock (bptr)
					{
						Debug.WriteLine($"BANK : SQL Locking TestBankCollection Datatable Load (307) in TestBankCollection (331) load function ");
						SqlCommand cmd = new SqlCommand(commandline, con);
						SqlDataAdapter sda = new SqlDataAdapter(cmd);
						if (dtBank == null)
							dtBank = new DataTable();
						sda.Fill(dtBank);
						Debug.WriteLine($"BANK : SQL UnLocking TestBankCollection Datatable Load (315) in TestBankCollection (331) load function ");
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to load Bank Details - {ex.Message}, {ex.Data}"); return;
				//				MessageBox . Show ( $"Failed to load Bank Details - {ex . Message}, {ex . Data}" ); return false;
			}
			return;
		}

		public static async Task<TestBankCollection> LoadBankCollection()
		{
			int count = 0;
			try
			{
				object bptr = new object();
				lock (bptr)
				{
					Debug.WriteLine($"BANK : SQL Locking TestBankCollection Load in TestBankCollection (331) load function ");
					for (int i = 0; i < dtBank.Rows.Count; i++)
					{
						TestBankinternalcollection.Add(new BankAccountViewModel
						{
							Id = Convert.ToInt32(dtBank.Rows[i][0]),
							BankNo = dtBank.Rows[i][1].ToString(),
							CustNo = dtBank.Rows[i][2].ToString(),
							AcType = Convert.ToInt32(dtBank.Rows[i][3]),
							Balance = Convert.ToDecimal(dtBank.Rows[i][4]),
							IntRate = Convert.ToDecimal(dtBank.Rows[i][5]),
							ODate = Convert.ToDateTime(dtBank.Rows[i][6]),
							CDate = Convert.ToDateTime(dtBank.Rows[i][7]),
						});
						count = i;
					}
					Debug.WriteLine($"BANK : SQL Unlocking TestBankCollection Load in TestBankCollection (347) load function ");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"BANK : SQL Error in TestBankCollection(351) load function : {ex.Message}, {ex.Data}");
				MessageBox.Show($"BANK : SQL Error in TestBankCollection (351) load function : {ex.Message}, {ex.Data}");
			}
			finally
			{
				// This is ONLY called  if a requestor specifies the argument as TRUE
				if (Notify)
				{
					EventControl.TriggerBankDataLoaded(null,
						new LoadedEventArgs
						{
							CallerType = "SQLSERVER",
							CallerDb = Caller,
							DataSource = TestBankinternalcollection,
							RowCount = TestBankinternalcollection.Count
						});
					//					Debug . WriteLine ( $"DEBUG : In TestBankCollection : Sending  BankDataLoaded EVENT trigger" );
				}
			}
			return TestBankinternalcollection;
		}

		#endregion LOAD THE DATA


		#region UNUSED FUNCTIONS
		/// <summary>
		/// A specialist version  to reload data WITHOUT changing global version
		/// </summary>
		/// <returns></returns>
		public async static Task<TestBankCollection> LoadBankTest(TestBankCollection temp)
		{
			object bptr = new object();
			lock (bptr)
			{
				try
				{
					for (int i = 0; i < dtBank.Rows.Count; i++)
					{
						temp.Add(new BankAccountViewModel
						{
							Id = Convert.ToInt32(dtBank.Rows[i][0]),
							BankNo = dtBank.Rows[i][1].ToString(),
							CustNo = dtBank.Rows[i][2].ToString(),
							AcType = Convert.ToInt32(dtBank.Rows[i][3]),
							Balance = Convert.ToDecimal(dtBank.Rows[i][4]),
							IntRate = Convert.ToDecimal(dtBank.Rows[i][5]),
							ODate = Convert.ToDateTime(dtBank.Rows[i][6]),
							CDate = Convert.ToDateTime(dtBank.Rows[i][7]),
						});
					}

				}
				catch (Exception ex)
				{
					Debug.WriteLine($"BANK : SQL Error in TestBankCollection load function : {ex.Message}, {ex.Data}");
					MessageBox.Show($"BANK : SQL Error in TestBankCollection load function : {ex.Message}, {ex.Data}");
				}
				finally
				{
					Debug.WriteLine($"BANK : Completed load into TestBankCollection :  {temp.Count} records loaded successfully ....");
				}
			}
			return temp;
		}

		public void ListBankInfo(KeyboardDelegate KeyBoardDelegate)
		{
			// Run a specified delegate sent by SqlDbViewer
			KeyBoardDelegate(1);
		}
		public static bool UpdateBankDb(BankAccountViewModel NewData)
		{
			SqlConnection con;
			string ConString = "";
			ConString = (string)Settings.Default["BankSysConnectionString"];
			con = new SqlConnection(ConString);
			try
			{
				using (con)
				{
					con.Open();
					SqlCommand cmd = new SqlCommand("UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, " +
						"BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno", con);
					cmd.Parameters.AddWithValue("@id", Convert.ToInt32(NewData.Id));
					cmd.Parameters.AddWithValue("@bankno", NewData.BankNo.ToString());
					cmd.Parameters.AddWithValue("@custno", NewData.CustNo.ToString());
					cmd.Parameters.AddWithValue("@actype", Convert.ToInt32(NewData.AcType));
					cmd.Parameters.AddWithValue("@balance", Convert.ToDecimal(NewData.Balance));
					cmd.Parameters.AddWithValue("@intrate", Convert.ToDecimal(NewData.IntRate));
					cmd.Parameters.AddWithValue("@odate", Convert.ToDateTime(NewData.ODate));
					cmd.Parameters.AddWithValue("@cdate", Convert.ToDateTime(NewData.CDate));
					cmd.ExecuteNonQuery();
					Debug.WriteLine("SQL Update of BankAccounts successful...");
				}
			}
			catch (Exception ex)
			{ Console.WriteLine($"BANKACCOUNT Update FAILED : {ex.Message}, {ex.Data}"); }
			finally
			{ con.Close(); }
			return true;
		}


		/// <summary>
		/// Called to allow any method to load FULL Bank data directly 
		/// it returns a populated DataTable
		/// </summary>
		/// <param name="dtBank"></param>
		/// <param name="Sqlcommand"></param>
		/// <returns></returns>
		public static DataTable LoadBankDirect(DataTable dtBank, string Sqlcommand = "Select* from BankAccount order by CustNo, BankNo")
		{
			SqlConnection con;
			string ConString = "";
			string commandline = "";
			dtBank.Clear();

			try
			{
				ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
				Debug.WriteLine($"Making new SQL connection in TestBankCollection");
				con = new SqlConnection(ConString);

				using (con)
				{
					Debug.WriteLine($"Using new SQL connection in TestBankCollection");
					if (Sqlcommand != "")
						commandline = Sqlcommand;
					else
						commandline = "Select * from BankAccount order by CustNo, BankNo";

					SqlCommand cmd = new SqlCommand(commandline, con);
					SqlDataAdapter sda = new SqlDataAdapter(cmd);
					if (dtBank == null)
						dtBank = new DataTable();
					sda.Fill(dtBank);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to load Bank Details - {ex.Message}, {ex.Data}"); return null;
			}
			return dtBank;

		}
		/// <summary>
		/// A specialist version  to reload data WITHOUT changing global version
		/// </summary>
		/// <returns></returns>
		public static TestBankCollection LoadBankCollectionDirect(TestBankCollection temp, DataTable dtBank)
		{
			try
			{
				for (int i = 0; i < dtBank.Rows.Count; i++)
				{
					temp.Add(new BankAccountViewModel
					{
						Id = Convert.ToInt32(dtBank.Rows[i][0]),
						BankNo = dtBank.Rows[i][1].ToString(),
						CustNo = dtBank.Rows[i][2].ToString(),
						AcType = Convert.ToInt32(dtBank.Rows[i][3]),
						Balance = Convert.ToDecimal(dtBank.Rows[i][4]),
						IntRate = Convert.ToDecimal(dtBank.Rows[i][5]),
						ODate = Convert.ToDateTime(dtBank.Rows[i][6]),
						CDate = Convert.ToDateTime(dtBank.Rows[i][7]),
					});
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"BANK : SQL Error in TestBankCollection load function : {ex.Message}, {ex.Data}");
				MessageBox.Show($"BANK : SQL Error in TestBankCollection load function : {ex.Message}, {ex.Data}");
			}
			finally
			{
				Debug.WriteLine($"BANK : Completed load into TestBankCollection :  {temp.Count} records loaded successfully ....");
			}
			return temp;
		}

		#endregion UNUSED FUNCTIONS



		#region EXPORT FUNCTIONS TO READ/WRITE CSV files for BANKACCOUNT DB

		/// <summary>
		/// Load the data into a DataTable for our export functions below here
		/// </summary>
		/// <returns></returns>
		public static DataTable LoadBankExportData()
		{
			DataTable dt = new DataTable();
			SqlConnection con;
			string ConString = "";
			string commandline = "";
			ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
			Debug.WriteLine($"Making new SQL connection in TestBankCollection");
			con = new SqlConnection(ConString);
			try
			{
				Debug.WriteLine($"Using new SQL connection in TestBankCollection");
				using (con)
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
						commandline = Utils.GetDataSortOrder(commandline);
					}
					SqlCommand cmd = new SqlCommand(commandline, con);
					SqlDataAdapter sda = new SqlDataAdapter(cmd);
					sda.Fill(dt);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"BANKACCOUNT : ERROR in LoadBankDataSql(): Failed to load Bank Details - {ex.Message}, {ex.Data}");
				MessageBox.Show($"BANKACCOUNT: ERROR in LoadBankDataSql(): Failed to load Bank Details - {ex.Message}, {ex.Data}");
			}
			finally
			{
				con.Close();
			}
			return dt;
		}

		/// <summary>
		/// Writes the data directly from the Db vias Sqluery first, then output to disk file in CSV format
		/// Working well 7/6/21
		/// </summary>
		/// <param name="path"></param>
		/// <param name="dbType"></param>
		public static int ExportBankData(string path, string dbType)
		{
			int count = 0;
			string output = "";

			// Read data in from disk first as a DataTable dt
			DataSet ds = new DataSet();

			DataTable dt = new DataTable();
			dt = LoadBankExportData();
			ds.Tables.Add(dt);
			//£££££££££££££££££££££££££££££££££££££££££££
			// This works just fine with no external binding.
			// The data is  now accessible in ds.Tables[0].Rows
			// NB DATA ACCESS FORMAT IS [ $"{objRow["CustNo"]}"  ]
			//££££££££££££££££££££££££££££££££££££££££££££
			Console.WriteLine($"Writing results of SQL enquiry to {path} ...");
			foreach (DataRow objRow in ds.Tables[0].Rows)
			{
				output += ParseDbRow("BANKACCOUNT", objRow);
				count++;
			}
			if (path == "")
				path = @"C:\Users\ianch\Documents\Bank";
			string savepath = Utils.GetExportFileName(path);

			System.IO.File.WriteAllText(savepath, output);
			Console.WriteLine($"Export of {count - 1} records from the [ {dbType} ] Db has been saved to {path} successfully.");
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
		public static string ParseDbRow(string dbType, DataRow objRow)
		{
			string tmp = "", s = "";
			string[] odat, cdat, revstr;
			if (dbType == "BANKACCOUNT")
			{
				char[] ch = { ' ' };
				char[] ch2 = { '/' };
				s = $"{objRow["Odate"].ToString()}', '";
				odat = s.Split(ch);
				string odate = odat[0];
				// now reverse it  to YYYY/MM/DD format as this is what SQL understands
				revstr = odate.Split(ch2);
				odate = revstr[2] + "/" + revstr[1] + "/" + revstr[0];
				// thats  the Open date handled - now do close data
				s = $"{objRow["cDate"].ToString()}', '";
				cdat = s.Split(ch);   // split date on '/'
				string cdate = cdat[0];
				// now reverse it  to YYYY/MM/DD format as this is what SQL understands
				revstr = cdate.Split(ch2);
				cdate = revstr[2] + "/" + revstr[1] + "/" + revstr[0];
				string acTypestr = objRow["AcType"].ToString().Trim();

				//Creates the correct format for the CSV fle output, including adding single quotes to DATE fields
				// Tested and working 7/6/21
				tmp = $"{objRow["Id"].ToString()}, "
					+ $"{objRow["BankNo"].ToString()}, "
					+ $"{objRow["CustNo"].ToString()}, "
					+ $"{acTypestr}, "
					+ $"{objRow["Balance"].ToString()}, "
					+ $"{objRow["Intrate"].ToString()}, "
					+ $"'{odate}', '"
					+ $"{cdate}'\r\n";
			}
			return tmp;
		}
		#endregion EXPORT FUNCTIONS TO READ/WRITE CSV files
	}
}
