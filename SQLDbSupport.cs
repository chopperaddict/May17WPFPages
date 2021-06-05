using System;
using System . Data;
using System . Data . SqlClient;
using System . Diagnostics;
using System . Reflection;
using System . Windows . Forms;
using WPFPages . Properties;
using WPFPages . ViewModels;
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

			CmdString = Utils . GetDataSortOrder ( CmdString );
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
				Debug . WriteLine ( $"Failed to load Bank Details - {ex . Message}" );
				return false;
			}
			return true;
		}


		public static void AddNewRecord ( string CurrentDb, object bvm )
		{
			string SQLcommand = "";
			DateTime dt1;
			DateTime dt2;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				BankAccountViewModel bv = bvm as BankAccountViewModel;
				string dtstr1 = bv . ODate . ToShortDateString ( );
				string dtstr2 = bv . CDate . ToShortDateString ( );
				dtstr1 = Utils . ConvertInputDate ( dtstr1 );
				dtstr2 = Utils . ConvertInputDate ( dtstr2 );
				SQLcommand = "Insert into BankAccount (BANKNO, CUSTNO, ACTYPE, BALANCE,  INTRATE, ODATE, CDATE) values ( " +
					$"{bv . BankNo}, {bv . CustNo}, {bv . AcType}, {bv . Balance}, {bv . IntRate}, '{dtstr1}', '{dtstr2}') ";
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				CustomerViewModel cv = bvm as CustomerViewModel;
				string dtstr1 = cv . Dob. ToShortDateString ( );
				string dtstr2 = cv . ODate . ToShortDateString ( );
				string dtstr3 = cv . CDate . ToShortDateString ( );
				dtstr1 = Utils . ConvertInputDate ( dtstr1 );
				dtstr2 = Utils . ConvertInputDate ( dtstr2 );
				SQLcommand = "Insert into Customer (BANKNO, CUSTNO, ACTYPE, FNAME, LNAME, ADDR1, ADDR2, TOWN, COUNTY, PCODE, PHONE, MOBILE, DOB, ODATE, CDATE) values ( " +
					$"{cv . BankNo}, {cv . CustNo}, {cv . AcType}, {cv . AcType}, {cv . FName}, {cv . LName}, {cv . Addr1}, {cv . Addr2}, {cv . Town}, {cv . County}, " + 
					$"{cv . PCode}, {cv . Phone}, {cv . Mobile}, , '{dtstr1}',  '{dtstr2}', '{dtstr3}') ";
			}
			else if ( CurrentDb == "DETAILS" )
			{
				DetailsViewModel dv = bvm as DetailsViewModel;
				string dtstr1 = dv . ODate . ToShortDateString ( );
				string dtstr2 = dv . CDate . ToShortDateString ( );
				dtstr1 = Utils . ConvertInputDate ( dtstr1 );
				dtstr2 = Utils . ConvertInputDate ( dtstr2 );
				SQLcommand = "Insert into SecAccounts (BANKNO, CUSTNO, ACTYPE, BALANCE,  INTRATE, ODATE, CDATE) values ( " +
					$"{dv . BankNo}, {dv . CustNo}, {dv . AcType}, {dv . Balance}, {dv . IntRate},  '{dtstr1}', '{dtstr2}') ";
			}

			// GO AHEAD and Save it to disk
			// This command does  the code commented out below
			//	@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30;			Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
			string ConString = ( string ) Settings . Default [ "BankSysConnectionString" ];
			SqlConnection con = new SqlConnection ( ConString );
			SqlDataAdapter adapter = new SqlDataAdapter ( );
			adapter . InsertCommand = new SqlCommand ( SQLcommand, con );
			try
			{
				con . Open ( );
				adapter . InsertCommand . ExecuteNonQuery ( );

				if ( CurrentDb == "BANKACCOUNT" )
					Console . WriteLine ( "New record added to BANKACCOUNT Db successfully..." );
				else if ( CurrentDb == "CUSTOMER" )
					Console . WriteLine ( "New record added to CUSTOMER Db successfully..." );
				else if ( CurrentDb == "DETAILS" )
					Console . WriteLine ( "New record added to DETAILS Db successfully..." );
			}
			catch ( Exception ex )
			{
				if ( ex . Message == "Incorrect syntax near '00'." )
					Console . WriteLine ( $"SQL ERROR - Exception message indicates the Date Values are in UK format, Year LAST. The CSV data MUST BE IN YYYY/MM/DD format" );
				else
					Console . WriteLine ( $"SQL ERROR - {ex . Message} + [{ex . Data}].." );
				MessageBox . Show ( $"SQL the Insert Command [{SQLcommand}]\r\nhas failed for some reason !.\r\n" +
									$"\r\nThe process has terminated, Check Output window & your data and Db for safety\r\nMsg= {ex . Message}, {ex . Data}", "Db Error Information",
							MessageBoxButtons . OK, MessageBoxIcon . Information, MessageBoxDefaultButton . Button1 );
				Console . WriteLine ( $"SQL ERROR - Failed to Insert a data row into Db\r\nCommand was [{SQLcommand}]..." );
			}
			finally
			{
				con . Close ( );
				adapter . Dispose ( );

			}
		}
	}
}
