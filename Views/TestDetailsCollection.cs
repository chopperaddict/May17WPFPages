using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WPFPages.ViewModels;
using System.Windows;

namespace WPFPages.Views
{
	public class TestDetailsCollection : ObservableCollection<DetailsViewModel>
	{
		public TestDetailsCollection()
		{
		}

		public ObservableCollection<DetailsViewModel> TestDetCollection = new ObservableCollection<DetailsViewModel>();

		// working entity to load data into
		public static TestDetailsCollection internalcollection = null;
		public static DataTable dtDetails = new DataTable();

		public static bool Notify = false;
		public static string Caller = "";

		public static async Task<TestDetailsCollection> LoadDet(TestDetailsCollection dc, string caller, int ViewerType = 1, bool NotifyAll = false)
		{
			bool result = false;
			object lockobject = new object();
			Notify = NotifyAll;
			Caller = caller;
			Debug.WriteLine($"About to lock Details Load system");
			try
			{
				lock (lockobject)
				{
					internalcollection = null;
					internalcollection = new TestDetailsCollection();

					LoadDetailsTaskInSortOrderAsync(internalcollection);
					Debug.WriteLine($"Exiting lock of Details Load system");
				}
				return internalcollection;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Erro in lock Details Load system : {ex.Message} + {ex.Data}");
			}
			return null;
		}

		public static async Task<bool> LoadDetailsTaskInSortOrderAsync(TestDetailsCollection internalcollection)
		{
			//			NotifyAll = NotifyAll;
			bool result = false;

			if (dtDetails.Rows.Count > 0)
				dtDetails.Clear();

			#region process code to load data

			Task t1 = Task.Run(
				async () =>
				{
					Debug.WriteLine($"TestDetailsCollection : Calling LOADDETAILSDATASQL()");
					await LoadDetailsDataSql();
					Debug.WriteLine($"TestDetailsCollection : Completed LOADDETAILSDATASQL()");
				}
				);
			t1.ContinueWith
			(
				async (result) =>
				{
					Debug.WriteLine($"TestDetailsCollection : Calling LOADDETCOLLECTION()");
					await LoadDetCollection(internalcollection);
					Debug.WriteLine($"TestDetailsCollection : Completed LOADDETCOLLECTION()");
				}, TaskScheduler.FromCurrentSynchronizationContext()
			 ); ;

			#endregion process code to load data

			#region Success//Error reporting/handling

			// Now handle "post processing of errors etc"
			//This will ONLY run if there were No Exceptions  and it ALL ran successfully!!
			t1.ContinueWith(
				(Detinternalcollection) =>
				{
					//				Debug . WriteLine ( $"DETAILS : Task.Run() Completed : Status was [ {Detinternalcollection . Status} ]." );
				}, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext()
			);
			//This will iterate through ALL of the Exceptions that may have occured in the previous Tasks
			// but ONLY if there were any Exceptions !!
			t1.ContinueWith(
				(Detinternalcollection) =>
				{
					AggregateException ae = t1.Exception.Flatten();
					Debug.WriteLine($"Exception in TestDetailsCollection data processing \n");
					foreach (var item in ae.InnerExceptions)
					{
						Debug.WriteLine($"TestDetailsCollection : Exception : {item.Message}, : {item.Data}");
					}
				}, CancellationToken.None, TaskContinuationOptions.NotOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext()
			);

			//			Debug . WriteLine ($"DETAILS : END OF PROCESSING & Error checking functionality\nDETAILS : *** TestDetailsCollection total = {TestDetailsCollection.Count} ***\n\n");
			#endregion Success//Error reporting/handling

			return true;
		}

		/// Handles the actual conneciton ot SQL to load the Details Db data required
		/// </summary>
		/// <returns></returns>
		//**************************************************************************************************************************************************************//
		public static async Task<DataTable> LoadDetailsDataSql(bool isMultiMode = false)
		{
			object bptr = new object();
			Stopwatch st = new Stopwatch();

			st.Start();
			SqlConnection con;
			string ConString = "";
			string commandline = "";
			ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
			Debug.WriteLine($"Making new SQL connection in DETAILSCOLLECTION");
			con = new SqlConnection(ConString);
			//if ( con== null )
			//{
			//	Debug . WriteLine ( $"DETAILCOLLECTION : No SQL Connection, reconnecting ..." );
			//	con= new SqlConnection ( ConString );
			//}
			try
			{
				Debug.WriteLine($"Using new SQL connection in DETAILSCOLLECTION");
				using (con)
				{
					if (Flags.IsMultiMode)
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = $"SELECT * FROM SECACCOUNTS WHERE CUSTNO IN "
							+ $"(SELECT CUSTNO FROM SECACCOUNTS  "
							+ $" GROUP BY CUSTNO"
							+ $" HAVING COUNT(*) > 1) ORDER BY ";
						commandline = Utils.GetDataSortOrder(commandline);
					}
					else if (Flags.FilterCommand != "")
					{
						commandline = Flags.FilterCommand;
					}
					else
					{
						// Create a valid Query Command string including any active sort ordering
						commandline = "Select * from SecAccounts  order by ";
						commandline = Utils.GetDataSortOrder(commandline);
					}
					SqlCommand cmd = new SqlCommand(commandline, con);
					SqlDataAdapter sda = new SqlDataAdapter(cmd);

					lock (bptr)
					{
						sda.Fill(dtDetails);
					}
					st.Stop();
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"DETAILS : ERROR in LoadDetailsDataSql(): Failed to load Details Details - {ex.Message}, {ex.Data}");
				MessageBox.Show($"DETAILS : ERROR in LoadDetailsDataSql(): Failed to load Details Details - {ex.Message}, {ex.Data}");
			}
			finally
			{
				con.Close();
			}
			return dtDetails;
		}

		//**************************************************************************************************************************************************************//
		public static async Task<bool> LoadDetCollection(TestDetailsCollection internalcollection)
		{
			object bptr = new object();
			int count = 0;
			lock (bptr)
			{
				try
				{
					for (int i = 0; i < dtDetails.Rows.Count; i++)
					{
						internalcollection.Add(new DetailsViewModel
						{
							Id = Convert.ToInt32(dtDetails.Rows[i][0]),
							BankNo = dtDetails.Rows[i][1].ToString(),
							CustNo = dtDetails.Rows[i][2].ToString(),
							AcType = Convert.ToInt32(dtDetails.Rows[i][3]),
							Balance = Convert.ToDecimal(dtDetails.Rows[i][4]),
							IntRate = Convert.ToDecimal(dtDetails.Rows[i][5]),
							ODate = Convert.ToDateTime(dtDetails.Rows[i][6]),
							CDate = Convert.ToDateTime(dtDetails.Rows[i][7]),
							//							Integervalues = dtDetails.Rows[i][8] as DetailsViewModel.Internalclass
						});
						count = i;
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"DETAILS : ERROR in  LoadDetCollection() : loading Details into ObservableCollection \"TestDetailsCollection\" : [{ex.Message}] : {ex.Data} ....");
					MessageBox.Show($"DETAILS : ERROR in  LoadDetCollection() : loading Details into ObservableCollection \"TestDetailsCollection\" : [{ex.Message}] : {ex.Data} ....");
					return false;
				}
				finally
				{
					if (Notify)
					{
						EventControl.TriggerDetDataLoaded(null,
							new LoadedEventArgs
							{
								CallerType = "SQLSERVER",
								CallerDb = Caller,
								DataSource = internalcollection,
//								SenderGuid = Guid.NewGuid().ToString(),
								RowCount = internalcollection.Count
							});
					}
				}
			}
			return true;
		}
		public static DataTable LoadDetailsDirect(DataTable dtDetails, string Sqlcommand = "Select* from SecAccounts order by CustNo, BankNo")
		{
			SqlConnection con;
			string ConString = "";
			string commandline = "";
			dtDetails.Clear();
			ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
			Debug.WriteLine($"Making new SQL connection in DETAILSCOLLECTION");
			con = new SqlConnection(ConString);
			try
			{
				Debug.WriteLine($"Using new SQL connection in DETAILSCOLLECTION");
				using (con)
				{
					// Create a valid Query Command string including any active sort ordering
					commandline = "Select * from SecAccounts  order by ";
					commandline = Utils.GetDataSortOrder(commandline);
					SqlCommand cmd = new SqlCommand(commandline, con);
					SqlDataAdapter sda = new SqlDataAdapter(cmd);

					TestDetailsCollection bptr = new TestDetailsCollection();
					sda.Fill(dtDetails);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"DETAILS : ERROR in LoadDetailsDataSql(): Failed to load Details Details - {ex.Message}, {ex.Data}");
				MessageBox.Show($"DETAILS : ERROR in LoadDetailsDataSql(): Failed to load Details Details - {ex.Message}, {ex.Data}");
			}
			finally
			{
				con.Close();
			}
			return dtDetails;
		}

		public static TestDetailsCollection LoadDetailsCollectionDirect(TestDetailsCollection collection, DataTable dtDetails)
		{
			int count = 0;
			try
			{
				for (int i = 0; i < dtDetails.Rows.Count; i++)
				{
					collection.Add(new DetailsViewModel
					{
						Id = Convert.ToInt32(dtDetails.Rows[i][0]),
						BankNo = dtDetails.Rows[i][1].ToString(),
						CustNo = dtDetails.Rows[i][2].ToString(),
						AcType = Convert.ToInt32(dtDetails.Rows[i][3]),
						Balance = Convert.ToDecimal(dtDetails.Rows[i][4]),
						IntRate = Convert.ToDecimal(dtDetails.Rows[i][5]),
						ODate = Convert.ToDateTime(dtDetails.Rows[i][6]),
						CDate = Convert.ToDateTime(dtDetails.Rows[i][7])
					});
					count = i;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"DETAILS : ERROR in  LoadDetCollection() : loading Details into ObservableCollection \"TestDetailsCollection\" : [{ex.Message}] : {ex.Data} ....");
				MessageBox.Show($"DETAILS : ERROR in  LoadDetCollection() : loading Details into ObservableCollection \"TestDetailsCollection\" : [{ex.Message}] : {ex.Data} ....");
				return null;
			}
			finally
			{
				if (Notify)
				{
					EventControl.TriggerDetDataLoaded(null,
						new LoadedEventArgs
						{
							CallerType = "SQLSERVER",
							CallerDb = Caller,
							DataSource = internalcollection,
//							SenderGuid = Guid.NewGuid().ToString(),
							RowCount = internalcollection.Count
						});
				}
			}
			return collection;
		}

		#region EXPORT FUNCTIONS TO READ/WRITE CSV files
		public static DataTable LoadDetailsExportData()
		{
			DataTable dt = new DataTable();
			SqlConnection con;
			string ConString = "";
			string commandline = "";
			ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
			Debug.WriteLine($"Making new SQL connection in DETAILSCOLLECTION");
			con = new SqlConnection(ConString);
			try
			{
				Debug.WriteLine($"Using new SQL connection in DETAILSCOLLECTION");
				using (con)
				{
					if (Flags.IsMultiMode)
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
						commandline = "Select * from SecAccounts  order by ";
						commandline = Utils.GetDataSortOrder(commandline);
					}
					SqlCommand cmd = new SqlCommand(commandline, con);
					SqlDataAdapter sda = new SqlDataAdapter(cmd);
					sda.Fill(dt);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"DETAILS : ERROR in LoadDetailsDataSql(): Failed to load Details Details - {ex.Message}, {ex.Data}");
				MessageBox.Show($"DETAILS : ERROR in LoadDetailsDataSql(): Failed to load Details Details - {ex.Message}, {ex.Data}");
			}
			finally
			{
				con.Close();
			}
			return dt;
		}

		public static int ExportDetData(string path, string dbType)
		{
			int count = 0;
			string output = "";

			// Read data in from disk first as a DataTable dt
			DataSet ds = new DataSet();

			DataTable dt = new DataTable();
			dt = LoadDetailsExportData();
			ds.Tables.Add(dt);
			//£££££££££££££££££££££££££££££££££££££££££££
			// This works just fine with no external binding.
			// The data is  now accessible in ds.Tables[0].Rows
			// NB DATA ACCESS FORMAT IS [ $"{objRow["CustNo"]}"  ]
			//££££££££££££££££££££££££££££££££££££££££££££
			Console.WriteLine($"Writing results of SQL enquiry to {path} ...");
			foreach (DataRow objRow in ds.Tables[0].Rows)
			{
				output += ParseDbRow("DETAILS", objRow);
				count++;
			}
			System.IO.File.WriteAllText(path, output);
			Console.WriteLine($"Export of {count - 1} records from the [ {dbType} ] Db has been completed successfully.");
			return count;
		}

		//===============================================================================
		/// <summary>
		/// Method to output CSV data correctly formatted
		/// </summary>
		/// <param name="dbType"></param>
		/// <param name="objRow"></param>
		/// <returns></returns>
		public static string ParseDbRow(string dbType, DataRow objRow)
		{
			string tmp = "", s = "";
			string[] odat, cdat, revstr;
			if (dbType == "DETAILS")
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

				// Formats data correctly for CSV output, with single quotes around dates converted to YYYY/MM/DD format as required by SQL Server
				tmp = $"{objRow["Id"].ToString()}, "
					+ $"{objRow["BankNo"].ToString()}, "
					+ $"{objRow["CustNo"].ToString()}, "
					+ $"{acTypestr},"
					+ $"{objRow["Balance"].ToString()}, "
					+ $"{objRow["Intrate"].ToString()}, "
					+ $"'{odate}', "
					+ $"'{cdate}'\r\n";
			}
			return tmp;
		}
		#endregion EXPORT FUNCTIONS TO READ/WRITE CSV files

	}
}
