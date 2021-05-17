using System;
using System . Data;
using System . Data . SqlClient;
using WPFPages . Views;


namespace WPFPages
{
	public class SQLDbSupport
	{
		public static bool LoadMultiSqlData ( string CmdString, DataTable dt, string CallerType, EditDb edb )
		{
			//THIS IS THE SQL COMMAND TO GET FULL LINES OF DUPLICATED CUSTOMER ACCOUNT #'S DATA
			CmdString = $"SELECT * FROM {CallerType}  WHERE CUSTNO IN "
				+ $"(SELECT CUSTNO FROM {CallerType} "
				+ $" GROUP BY CUSTNO"
				+ $" HAVING COUNT(*) > 1) ORDER BY ";

			CmdString  = Utils . GetDataSortOrder ( CmdString );
			try
			{
				SqlConnection con;
				string ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
				using ( con = new SqlConnection ( ConString ) )
				{
					SqlCommand cmd = new SqlCommand ( CmdString, con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dt );
				}
				{
					//					//load the SQL data into our OC named bankaccounts
					//					if (CallerType == "BANKACCOUNT")
					//					{
					////						SqlDbViewer sqldbv = new SqlDbViewer (0);
					//						if (LoadBankCollection(dt))
					//							MessageBox.Show($"Bank Account loaded successfully ({dt.Rows.Count}) records");
					//						else
					//						{
					//							return false;
					//						}
					//					}
					//					else if (CallerType == "CUSTOMER")
					//					{
					//						SqlDbViewer sqldbv = new SqlDbViewer (1);
					//						if (sqldbv.LoadCustomerCollection (dt))
					//							MessageBox.Show ($"Customer Account loaded successfully ({dt.Rows.Count}) records");
					//						else
					//						{							
					//							return false;
					//						}
					//					}
					//					else if (CallerType == "DETAILS")
					//					{
					//						SqlDbViewer sqldbv = new SqlDbViewer (2);
					//						if (sqldbv.LoadDetailsCollection (dt))
					//							MessageBox.Show ($"Details Account loaded successfully ({dt.Rows.Count}) records");
					//						else
					//						{
					//							return false;
					//						}
					//					}

					//					//reset filter flag
					//					return true;
					//				
				}
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"Failed to load Bank Details - {ex . Message}" );
				return false;
			}
			return true;
		}

		//		public bool LoadBankCollection (DataTable dtBank)
		//		{
		//			//Load the data into our ObservableCollection BankAccounts
		//			//			try
		//			//			{
		//			if (BankAccountCollection.Count > 0)
		//			{
		//				//				SelectedBankAccount = null;
		//				try
		//				{
		//					BankAccountCollection.Clear ();
		//				}
		//				catch (Exception ex) { Console.WriteLine ($"Failed to load clear Bank Details Obslist - {ex.Message}"); }
		//			}
		//			for (int i = 0; i < dtBank.Rows.Count; ++i)
		//			{
		//				BankAccountCollection.Add (new BankAccount
		//				{
		//					Id = Convert.ToInt32 (dtBank.Rows[i][0]),
		//					BankNo = dtBank.Rows[i][1].ToString (),
		//					CustNo = dtBank.Rows[i][2].ToString (),
		//					AcType = Convert.ToInt32 (dtBank.Rows[i][3]),
		//					Balance = Convert.ToDecimal (dtBank.Rows[i][4]),
		//					IntRate = Convert.ToDecimal (dtBank.Rows[i][5]),
		//					ODate = Convert.ToDateTime (dtBank.Rows[i][6]),
		//					CDate = Convert.ToDateTime (dtBank.Rows[i][7]),
		//				});
		//			}
		//			return true;
		//		}
	}
}
