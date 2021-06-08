using System;
using System . Collections . Generic;
using System . Data . Linq . SqlClient;
using System . Data . SqlClient;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Diagnostics;
using System . Data;
using WPFPages . Properties;
using System . Windows;

namespace WPFPages . Views
{
	// Import CSV data into our Db system
	public class ImportDbData
	{

	#region WORKING 7/6/21
		public static void UpdateBankDbFromTextFile ( )
		{
			// THIS WORKS VERY WELL, AND IT IS PRETTY FAST		 - wel it did  on 8/10/2020
			// Still does now - after a few minor changes  7/6/21
			// write CSV comma delimited data to BankAccount Db from text file using SQL INSERT Clause
			//Assumes COMMA delimited - NOT TAB, nad dates MUST be in YYYY/MM/DD format
			string path = @"C:\Users\ianch\Documents\BankDb.csv";
			path = Utils . GetImportFileName ( path );
//			path = Utils . GetFileName ( path );
			if ( System . IO . File . Exists ( path ) )
			{
				int counter = 0;
				string output = "";
				string rawdata = System . IO . File . ReadAllText ( path );
				string [ ] lines;
				char [ ] ch = { '\r' };
				// split input data into lines of data (records)
				lines = rawdata . Split ( ch );
				char [ ] ch2 = { ',' };
				string [ ] fields;//, date1, date2;
				string SQLcommand = "";

				//if ( !DropBankAccountTable ( ) )
				//{
				//	MessageBox . Show ( $"Unable to Drop/Create BankAccount table, Import process cancelled." , "Db Error Information" ,
				//		MessageBoxButtons . OK , MessageBoxIcon . Information , MessageBoxDefaultButton . Button1 );
				//	Console . WriteLine ( $"SQL ERROR - Unable to DROP/CREATE BankAccount Table" );
				//}
				foreach ( var item in lines )
				{
					fields = item . Split ( ch2 );
					if ( item . Length == 0 )
						break;
					if ( fields . Length < 5 )
						break;
					// BEWARE = This assumes the dates are in YYYY/MM/DD format !!!!!, so if they are not, it WILL FAIL
					string odate = Utils . ConvertInputDate ( fields [ 6 ] . ToString ( ) . Trim ( ) );
					string cdate = Utils . ConvertInputDate ( fields [ 7 ] . ToString ( ) . Trim ( ) );

					output = fields [ 1 ] + "," + fields [ 2 ] + "," + fields [ 3 ] + "," +
						 fields [ 4 ] + "," + fields [ 5 ] + ",'" + odate + "','" + cdate + "'";

					// ID data does need to be in "ticks"
					SQLcommand = "Insert into BankAccount (BANKNO, CUSTNO, ACTYPE, BALANCE,  INTRATE, ODATE, CDATE) values ( " + output + ")";

					string ConString = ( string ) Settings . Default [ "BankSysConnectionString" ];

					SqlDataAdapter adapter = new SqlDataAdapter ( );
					SqlConnection cnn = new SqlConnection(ConString);
					adapter . InsertCommand = new SqlCommand ( SQLcommand, cnn );
					try
					{
						cnn . Open ( );
						adapter . InsertCommand . ExecuteNonQuery ( );
						counter++;
						Debug . WriteLine ($"Record {counter} from bulk data added to  BankAccount Db via SQL Insert command...");
						cnn . Close ( );
						adapter . Dispose ( );
					}
					catch ( Exception ex )
					{
						if ( ex . Message == "Incorrect syntax near '00'." )
							Console . WriteLine ( $"SQL ERROR - Exception message indicates the Date Values are in UK format, Year LAST. The CSV data MUST BE IN YYYY/MM/DD format" );
						else
							Console . WriteLine ( $"SQL ERROR - {ex . Message} + [{ex . Data}].." );
						MessageBox . Show ( $"SQL the Insert Command [{SQLcommand}]\r\nhas failed for some reason !.\r\n" +
											$"\r\nThe process has terminated, Check Output window & your data and Db for safety\r\nMsg= {ex . Message}, {ex . Data}", "Db Error Information" );//,
						//MessageBoxButtons . OK, MessageBoxIcon . Information, MessageBoxDefaultButton . Button1 );
						Console . WriteLine ( $"SQL ERROR - Failed to Insert a data row into Db\r\nCommand was [{SQLcommand}]..." );
						cnn . Close ( );
						adapter . Dispose ( );
						break;
					}
				}
				Console . WriteLine ( $"Added {counter} record from bulk data file to BANKACCOUNT Db..." );
			}
			else
			{
				MessageBox . Show ( $"Missing Data file {path}.!.", "Db Update Information" );//, MessageBoxButtons . OK, MessageBoxIcon . Information, MessageBoxDefaultButton . Button1 );
				return;
			}
		}
		#endregion WORKING 7/6/21

	#region UNKNOWN WORKING 7/6/21

		public bool UpdateCustomerDbFromTextFile ( )
		{
			// unchecked 7/6/21

			// THIS WORKS VERY WELL, AND IT IS PRETTY FAST
			// write  data to Customer Db using SQL INSERT Clause
			string path = @"C:\Users\ianch\source\C#SavedData\BulkData\CustomerDb.csv";
			//			path = Utils . GetFileName ( path );
			path = Utils . GetImportFileName ( path );

			if ( !System . IO . File . Exists ( path ) )
			{
				MessageBox . Show (
								$"There does not appear to be a suitable Raw Data File in the BulkData Folder ?\r\nTry using the [Create New data] option to create some suitable data",
								"" );//, MessageBoxButtons . OK );
				return false;
			}
			int counter = 0;
			string output = "";
			string rawdata = System . IO . File . ReadAllText ( path );
			string [ ] lines;
			char [ ] ch = { '\r' };
			// split input data into lines of data (records)
			lines = rawdata . Split ( ch );
			char [ ] ch2 = { ',' };
			string [ ] fields;
			string SQLcommand = "";
			foreach ( var item in lines )
			{
				fields = item . Split ( ch2 );
				if ( fields . Length < 12 ) break;
				/*
				 *				CORRECT FORMAT that works inSQL Server
				 * 	 USE [ian1]
				INSERT INTO Customer (CustNo,BankNo,Actype,Fname,Lname,Addr1,Addr2,Town,County,Pcode,Phone,Mobile,Dob,Odate,Cdate)
				values ( '1055004', '1055005', '3', 'Andrew', 'Prosser', '33  Buckingham Place', 'Torrington Condominiums', 'Bicester', 'Cornwall', 'KF65 5YY',  
					'0530 535343', '0445 322573', '2007/03/21', '2017/03/21', '2009/03/21')*/
				//Convert date into US YYYY/mm/dd
				string dob = Utils . ConvertInputDate ( fields [ 12 ] . ToString ( ) . Trim ( ) );
				string odate = Utils . ConvertInputDate ( fields [ 13 ] . ToString ( ) . Trim ( ) );
				string cdate = Utils . ConvertInputDate ( fields [ 14 ] . ToString ( ) . Trim ( ) );
				output = fields [ 0 ] . Trim ( ) + "," + fields [ 1 ] . Trim ( ) + "," + fields [ 2 ] . Trim ( ) + ",'" + fields [ 3 ] . Trim ( ) + "','" +
					 fields [ 4 ] . Trim ( )
						 + "','" + fields [ 5 ] . Trim ( ) + "','" + fields [ 6 ] . Trim ( ) + "','" + fields [ 7 ] . Trim ( ) + "','" + fields [ 8 ] . Trim ( )
						 + "','" + fields [ 9 ] . Trim ( ) + "','" + fields [ 10 ] . Trim ( ) + "','" + fields [ 11 ] . Trim ( )
						 + "','" + dob + "','" + odate + "','" + cdate + "'";// + fields [ 15 ] + "'" ;

				SQLcommand = "USE Ian1 INSERT INTO Customer (CustNo, BankNo, actype, Fname, Lname, Addr1, Addr2, Town, County, Pcode, Phone, Mobile, Dob, Odate, Cdate)"
					  + $" values ( " + output + ")";

				// This command does  the code commented out below
				SqlDataAdapter adapter = new SqlDataAdapter ( );
				SqlConnection cnn = new SqlConnection ( );
				adapter . InsertCommand = new SqlCommand ( SQLcommand, cnn );
				try
				{
					adapter . InsertCommand . ExecuteNonQuery ( );
					counter++;
					Debug . WriteLine ( $"Added record {counter} via SQL Insert command to Customer Db");
					adapter . Dispose ( );
				}
				catch ( Exception ex )
				{
					//MessageBox . Show ( $"SQL the Insert Command [{SQLcommand}]\r\nhas failed for some reason !.\r\n" +
					//                  $"\r\nThe process has terminated, Check your data and Db for safety" , "Db Error Information" ,
					//  MessageBoxButtons . OK , MessageBoxIcon . Information , MessageBoxDefaultButton . Button1 );
					Console . WriteLine ( $"SQL ERROR - Failed to Insert a data row into Db\r\nCommand was [{SQLcommand}]..." );
					Console . WriteLine ( $"SQL ERROR - {ex . Message}, {ex . Data}" );
					adapter . Dispose ( );
					return false;
				}
				Console . WriteLine ( $"Added {counter} record from bulk data file to Customer Db..." );
			}
			return false;

		}
		private void UpdateSecAccountsDbFromTextFile ( )
		{
			// unchecked 7/6/21

			// THIS WORKS VERY WELL, AND IT IS PRETTY FAST
			// write data to SecAccounts Db from text file using SQL INSERT Clause
			string path = @"C:\Users\ianch\source\C#SavedData\BulkData\DetailsDb.csv";
			path = Utils . GetImportFileName ( path );
			if ( System . IO . File . Exists ( path ) )
			{
				int counter = 0;
				string output = "";
				string rawdata = System . IO . File . ReadAllText ( path );
				string [ ] lines;
				char [ ] ch = { '\n' };
				// split input data into lines of data (records)	  using '/n'
				lines = rawdata . Split ( ch );
				char [ ] ch2 = { ',' };
				string [ ] fields;
				string SQLcommand = "";
				//				string odate = "", cdate = ""; ;
				foreach ( var item in lines )
				{
					fields = item . Split ( ch2 );
					if ( fields . Length < 4 ) break;
					if ( item . Length <= 2 )
						break;
					string dummy = fields [ 6 ];
					fields [ 6 ] = dummy . Substring ( 0, dummy . Length - 1 );

					string odate = Utils . ConvertInputDate ( fields [ 5 ] . ToString ( ) . Trim ( ) );
					string cdate = Utils . ConvertInputDate ( fields [ 6 ] . ToString ( ) . Trim ( ) );

					output = fields [ 0 ] + "," + fields [ 1 ] + "," + fields [ 2 ] + "," + fields [ 3 ] + "," + fields [ 4 ] + ",'" +
							odate + "','" + cdate + "'";
					// This is the correct FIELDS sequnece for our CSV data
					SQLcommand = "use ian1 Insert into SecAccounts (BANKNO, CUSTNO, ACTYPE, Balance, Intrate, Odate, Cdate) values ( " + output + ")";
					// This command does  the code commented out below
					SqlDataAdapter adapter = new SqlDataAdapter ( );
					SqlConnection cnn = new SqlConnection ( );
					adapter . InsertCommand = new SqlCommand ( SQLcommand, cnn );
					try
					{
						adapter . InsertCommand . ExecuteNonQuery ( );
						counter++;
						Console . WriteLine ( $"Added record {counter} from bulk data..." );
						adapter . Dispose ( );
					}
					catch ( Exception ex )
					{
						Console . WriteLine ( $"SQL ERROR - {ex . Message} + [{ex . Data}].." );
						MessageBox . Show ( $"SQL the Insert Command [{SQLcommand}]\r\nhas failed for some reason !.\r\n" +
										  $"\r\nThe process has terminated, Check your data and Db for safety", "Db Error Information" );//,
									//MessageBoxButtons . OK, MessageBoxIcon . Information, MessageBoxDefaultButton . Button1 );
						Console . WriteLine ( $"SQL ERROR - Failed to Insert a data row into Db\r\nCommand was -  {SQLcommand}..." );
						adapter . Dispose ( );
						break;
					}
				}
				Console . WriteLine ( $"Added {counter} record from bulk data file to SECACCOUNTS Db..." );
			}
			else
			{
				MessageBox . Show ( $"Missing Data file {path}.!.", "Db Update Information" );//, MessageBoxButtons . OK, MessageBoxIcon . Information, MessageBoxDefaultButton . Button1 );
				return;
			}
		}

		#endregion UNKNOWN WORKING 7/6/21

	}
}
