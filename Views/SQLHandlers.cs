using System;
using System . Data . SqlClient;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Input;

using WPFPages . Properties;
using WPFPages . ViewModels;

namespace WPFPages . Views
{
	public class SQLHandlers
	{
		public static BankAccountViewModel bvm = MainWindow.bvm;
		public static CustomerViewModel cvm = MainWindow.cvm;
		public static DetailsViewModel dvm = MainWindow.dvm;

		// New Observable collections
		//BankCollection bc = new BankCollection ( );
		//public BankCollection Bankcollection;// = bc.Bankcollection;

		//		public BankCollection Bankcollection = BankCollection .Bankcollection;
		//		public CustCollection Custcollection = CustCollection . Custcollection;
		//		public DetCollection Detcollection = DetCollection.Detcollection;
		public  static BankCollection BankViewercollection=new BankCollection();
		public  static BankCollection CustViewercollection=new BankCollection();
		public  static BankCollection DetViewercollection=new BankCollection();
		public  static BankCollection SqlViewerBankcollection=new BankCollection();
		public  static BankCollection EditDbBankcollection=new BankCollection();
		public  static BankCollection MultiBankcollection=new BankCollection();

		// THIS IS  HOW  TO HANDLE EVENTS RIGHT NOW //
		//Event CallBack for when Asynchronous data loading has been completed in the Various ViewModel classes
		//		public static  event EventHandler<DataUpdatedEventArgs> DataUpdated;
		//		public static  event  DetaiIsUpdated          DataUpdated;
		//-------------------------------------------------------------------------------------------------------------------------------------------------//
		//protected virtual void OnDataUpdated ( object o , DataUpdatedEventArgs e )
		//{
		//	object dummy = null;
		//}
		public async Task<bool> UpdateDbRowAsync ( string CurrentDb , object RowData , int Row )
		{
			///TRIGGERED when a Cell is EDITED
			/// After a fight, this is now working and updates the relevant RECORD correctly
			///
			BankAccountViewModel ss = new BankAccountViewModel ( );
			CustomerViewModel cs = new CustomerViewModel();
			DetailsViewModel sa = new DetailsViewModel();
			if ( CurrentDb == "BANKACCOUNT" )
			{
				ss = RowData as BankAccountViewModel;
				if ( ss == null ) return false;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				cs = RowData as CustomerViewModel;
				if ( cs == null ) return false;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				sa = RowData as DetailsViewModel;
				if ( sa == null ) return false;
			}

			//Sanity checks - are values actualy valid ???
			//They should be as Grid vlaidate entries itself !!
			if ( CurrentDb == "BANKACCOUNT" || CurrentDb == "DETAILS" )
			{
				#region BANK/DETAILS UPDATE PROCESSING

				try
				{
					int x;
					decimal Y;
					if ( CurrentDb == "BANKACCOUNT" )
					{
						x = Convert . ToInt32 ( ss . Id );
						x = Convert . ToInt32 ( ss . AcType );
						//Check for invalid A/C Type
						if ( x < 1 || x > 4 )
						{
							Console . WriteLine ( $"SQL UpdateDbRow(92) Invalid A/c type of {ss . AcType} in grid Data" );
							MessageBox . Show ( $"Invalid A/C Type ({ss . AcType}) in the Grid !!!!\r\nPlease correct this entry!" );
							return false;
						}
						Y = Convert . ToDecimal ( ss . Balance );
						Y = Convert . ToDecimal ( ss . IntRate );
						//Check for invalid Interest rate
						if ( Y > 100 )
						{
							Console . WriteLine ( $"SQL UpdateDbRow(101) Invalid Interest Rate of {ss . IntRate} > 100% in grid Data" );
							MessageBox . Show ( $"Invalid Interest rate ({ss . IntRate}) > 100 entered in the Grid !!!!\r\nPlease correct this entry!" );
							return false;
						}
						DateTime dtm = Convert.ToDateTime (ss.ODate);
						dtm = Convert . ToDateTime ( ss . CDate );
					}
					else if ( CurrentDb == "DETAILS" )
					{
						x = Convert . ToInt32 ( sa . Id );
						x = Convert . ToInt32 ( sa . AcType );
						//Check for invalid A/C Type
						if ( x < 1 || x > 4 )
						{
							Console . WriteLine ( $"SQL UpdateDbRow(117) Invalid A/c type of {sa . AcType} in grid Data" );
							MessageBox . Show ( $"Invalid A/C Type ({sa . AcType}) in the Grid !!!!\r\nPlease correct this entry!" );
							return false;
						}
						Y = Convert . ToDecimal ( sa . Balance );
						Y = Convert . ToDecimal ( sa . IntRate );
						//Check for invalid Interest rate
						if ( Y > 100 )
						{
							Console . WriteLine ( $"SQL UpdateDbRow(126) Invalid Interest Rate of {sa . IntRate} > 100% in grid Data" );
							MessageBox . Show ( $"Invalid Interest rate ({sa . IntRate}) > 100 entered in the Grid !!!!\r\nPlease correct this entry!" );
							return false;
						}
						DateTime dtm = Convert.ToDateTime (sa.ODate);
						dtm = Convert . ToDateTime ( sa . CDate );
					}
					//					string sndr = sender.ToString();
				}
				catch ( Exception ex )
				{
					Console . WriteLine ( $"SQL UpdateDbRow(137) Invalid grid Data - {ex . Message} Data = {ex . Data}" );
					MessageBox . Show ( "Invalid data entered in the Grid !!!! - See Output for details.\r\nNEITHER Db has been updated !!" );
					return false;
				}
				SqlConnection con =  null;
				string ConString = "";
				ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				try
				{
					//We need to update BOTH BankAccount AND DetailsViewModel to keep them in parallel
					using ( con = new SqlConnection ( ConString ) )
					{
						con . Open ( );
						if ( CurrentDb == "BANKACCOUNT" )//|| CurrentDb == "DETAILS" )
						{
							SqlCommand cmd = new SqlCommand ("UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( ss . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , ss . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , ss . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( ss . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( ss . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( ss . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( ss . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( ss . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update successful for Bank Account Data..." );

							cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( ss . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , ss . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , ss . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( ss . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( ss . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( ss . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( ss . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( ss . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update successful for Secondary Accounts Data..." );

							cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( ss . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , ss . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , ss . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( ss . AcType ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( ss . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( ss . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update successful for Customers Data..." );
						}
						else if ( CurrentDb == "DETAILS" )
						{
							SqlCommand cmd = new SqlCommand ("UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( sa . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( sa . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update successful for Bank Account Data..." );

							cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( sa . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( sa . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update successful for Secondary Accounts Data..." );

							cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update successful for Customers Data..." );
						}
					}
				}
				catch ( Exception ex )
				{
					Console . WriteLine ( $"SQL Error UpdateDbRow(180) - BankAccount/Sec" +
						$"accounts not updated {ex . Message} Data = {ex . Data}" );
				}
				finally
				{
					con . Close ( );
					Console . WriteLine ( $"SQL - Updated Row in ALL Db's after change(s) made in  {CurrentDb}" );
					// We have now updated the Db's via SQL !!!
					// This call should tell any other open viewers to refresh their views
					//					BankCollection.Bankcollection = null;
					//					BankCollection bc = new BankCollection();
					//					await bc.ReLoadBankData( );

#pragma TODO  notify other viewers of data being reloaded  13/5/21
				}

				//if ( CurrentDb == "BANKACCOUNT" )
				//{
				//	SqlDbViewer sqlv = new SqlDbViewer();
				//	sqlv . RefreshCustomerOnUpdateNotification ( null , null , new DataChangeArgs { SenderName = CurrentDb , DbName = CurrentDb } );
				//	sqlv . RefreshDetailsOnUpdateNotification ( null , null , new DataChangeArgs { SenderName = CurrentDb , DbName = CurrentDb } );
				//}
				//else if ( CurrentDb == "CUSTOMER" )
				//{
				//	SqlDbViewer sqlv = new SqlDbViewer();
				//	sqlv . RefreshBankOnUpdateNotification ( null , null , new DataChangeArgs { SenderName = CurrentDb , DbName = CurrentDb } );
				//	sqlv . RefreshDetailsOnUpdateNotification ( null , null , new DataChangeArgs { SenderName = CurrentDb , DbName = CurrentDb } );
				//}
				//else if ( CurrentDb == "DETAILS" )
				//{
				//	SqlDbViewer sqlv = new SqlDbViewer();
				//	sqlv . RefreshBankOnUpdateNotification ( null , null , new DataChangeArgs { SenderName = CurrentDb , DbName = CurrentDb } );
				//	sqlv . RefreshCustomerOnUpdateNotification ( null , null , new DataChangeArgs { SenderName = CurrentDb , DbName = CurrentDb } );
				//}
				#endregion BANK/DETAILS UPDATE PROCESSING
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				#region CUSTOMER UPDATE PROCESSING

				//				if ( Row == null && CurrentDb == "CUSTOMER")
				//					cs = Row.Item as CustomerViewModel;
				try
				{
					//Sanity check - are values actualy valid ???
					//They should be as Grid vlaidate entries itself !!
					int x;
					x = Convert . ToInt32 ( cs . Id );
					//					string sndr = sender.ToString();
					x = Convert . ToInt32 ( cs . AcType );
					//Check for invalid A/C Type
					if ( x < 1 || x > 4 )
					{
						Console . WriteLine ( $"SQL UpdateDbRow(204) Invalid A/c type of {cs . AcType} in grid Data" );
						MessageBox . Show ( $"Invalid A/C Type ({cs . AcType}) in the Grid !!!!\r\nPlease correct this entry!" );
						return false;
					}
					DateTime dtm = Convert.ToDateTime (cs.ODate);
					dtm = Convert . ToDateTime ( cs . CDate );
					dtm = Convert . ToDateTime ( cs . Dob );
				}
				catch ( Exception ex )
				{
					Console . WriteLine ( $"SQL Invalid grid Data UpdateDbRow(214)- {ex . Message} Data = {ex . Data}" );
					MessageBox . Show ( "Invalid data entered in the Grid !!!! - See Output for details" );
					return false;
				}
				SqlConnection con;
				string ConString = "";
				ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				con = new SqlConnection ( ConString );
				try
				{
					//We need to update BOTH BankAccount AND DetailsViewModel to keep them in parallel
					using ( con )
					{
						con . Open ( );

						SqlCommand cmd = new SqlCommand ("UPDATE Customer SET CUSTNO=@custno, BANKNO=@bankno, ACTYPE=@actype, " +
							"FNAME=@fname, LNAME=@lname, ADDR1=@addr1, ADDR2=@addr2, TOWN=@town, COUNTY=@county, PCODE=@pcode," +
							"PHONE=@phone, MOBILE=@mobile, DOB=@dob,ODATE=@odate, CDATE=@cdate WHERE Id=@id", con);

						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( cs . Id ) );
						cmd . Parameters . AddWithValue ( "@custno" , cs . CustNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@bankno" , cs . BankNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( cs . AcType ) );
						cmd . Parameters . AddWithValue ( "@fname" , cs . FName . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@lname" , cs . LName . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@addr1" , cs . Addr1 . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@addr2" , cs . Addr2 . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@town" , cs . Town . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@county" , cs . County . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@pcode" , cs . PCode . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@phone" , cs . Phone . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@mobile" , cs . Mobile . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@dob" , Convert . ToDateTime ( cs . Dob ) );
						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( cs . ODate ) );
						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( cs . CDate ) );
						cmd . ExecuteNonQuery ( );
						Console . WriteLine ( "SQL Update successful for Customers Data..." );

						cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( cs . Id ) );
						cmd . Parameters . AddWithValue ( "@bankno" , cs . BankNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@custno" , cs . CustNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( cs . AcType ) );
						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( cs . ODate ) );
						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( cs . CDate ) );
						cmd . ExecuteNonQuery ( );
						Console . WriteLine ( "SQL Update successful for Bank Account Data..." );

						cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( cs . Id ) );
						cmd . Parameters . AddWithValue ( "@bankno" , cs . BankNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@custno" , cs . CustNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( cs . AcType ) );
						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( cs . ODate ) );
						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( cs . CDate ) );
						cmd . ExecuteNonQuery ( );
						Console . WriteLine ( "SQL Update successful for Secondary Accounts Data..." );
					}
				}
				catch ( Exception ex )
				{
					con . Close ( );
					Console . WriteLine ( $"SQL Error UpdateDbRow(255)- {ex . Message} Data = {ex . Data}" );
				}
				finally
				{
					//Lets force the grids to update when we return from here ??
					con . Close ( );
					Console . WriteLine ( $"SQL - Updated Row in ALL Db's after change(s) made in  {CurrentDb}" );
				}
				return true;

				#endregion CUSTOMER UPDATE PROCESSING
			}
			return true;
		}
		//*****************************************************************************************//
		public async Task<bool> UpdateDbRow ( string CurrentDb , object RowData )
		{
			///TRIGGERED when a Cell is EDITED
			/// After a fight, this is now working and updates the relevant RECORD correctly
			///
			BankAccountViewModel ss = new BankAccountViewModel ( );
			CustomerViewModel cs = new CustomerViewModel();
			DetailsViewModel sa = new DetailsViewModel();
			if ( CurrentDb == "BANKACCOUNT" )
			{
				ss = RowData as BankAccountViewModel;
				if ( ss == null ) return false;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				cs = RowData as CustomerViewModel;
				if ( cs == null ) return false;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				sa = RowData as DetailsViewModel;
				if ( sa == null ) return false;
			}

			//Sanity checks - are values actualy valid ???
			//They should be as Grid vlaidate entries itself !!
			if ( CurrentDb == "BANKACCOUNT" || CurrentDb == "DETAILS" )
			{
				#region BANK/DETAILS UPDATE PROCESSING

				try
				{
					int x;
					decimal Y;
					if ( CurrentDb == "BANKACCOUNT" )
					{
						x = Convert . ToInt32 ( ss . Id );
						x = Convert . ToInt32 ( ss . AcType );
						//Check for invalid A/C Type
						if ( x < 1 || x > 4 )
						{
							Console . WriteLine ( $"SQL UpdateDbRow(92) Invalid A/c type of {ss . AcType} in grid Data" );
							MessageBox . Show ( $"Invalid A/C Type ({ss . AcType}) in the Grid !!!!\r\nPlease correct this entry!" );
							return false;
						}
						Y = Convert . ToDecimal ( ss . Balance );
						Y = Convert . ToDecimal ( ss . IntRate );
						//Check for invalid Interest rate
						if ( Y > 100 )
						{
							Console . WriteLine ( $"SQL UpdateDbRow(101) Invalid Interest Rate of {ss . IntRate} > 100% in grid Data" );
							MessageBox . Show ( $"Invalid Interest rate ({ss . IntRate}) > 100 entered in the Grid !!!!\r\nPlease correct this entry!" );
							return false;
						}
						DateTime dtm = Convert.ToDateTime (ss.ODate);
						dtm = Convert . ToDateTime ( ss . CDate );
					}
					else if ( CurrentDb == "DETAILS" )
					{
						x = Convert . ToInt32 ( sa . Id );
						x = Convert . ToInt32 ( sa . AcType );
						//Check for invalid A/C Type
						if ( x < 1 || x > 4 )
						{
							Console . WriteLine ( $"SQL UpdateDbRow(117) Invalid A/c type of {sa . AcType} in grid Data" );
							MessageBox . Show ( $"Invalid A/C Type ({sa . AcType}) in the Grid !!!!\r\nPlease correct this entry!" );
							return false;
						}
						Y = Convert . ToDecimal ( sa . Balance );
						Y = Convert . ToDecimal ( sa . IntRate );
						//Check for invalid Interest rate
						if ( Y > 100 )
						{
							Console . WriteLine ( $"SQL UpdateDbRow(126) Invalid Interest Rate of {sa . IntRate} > 100% in grid Data" );
							MessageBox . Show ( $"Invalid Interest rate ({sa . IntRate}) > 100 entered in the Grid !!!!\r\nPlease correct this entry!" );
							return false;
						}													

						DateTime dtm = Convert.ToDateTime (sa.ODate);
						dtm = Convert . ToDateTime ( sa . CDate );
					}
					//					string sndr = sender.ToString();
				}
				catch ( Exception ex )
				{
					Console . WriteLine ( $"SQL UpdateDbRow(137) Invalid grid Data - {ex . Message} Data = {ex . Data}" );
					MessageBox . Show ( "Invalid data entered in the Grid !!!! - See Output for details.\r\nNEITHER Db has been updated !!" );
					return false;
				}
				SqlConnection con =  null;
				string ConString = "";
				ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				try
				{
					//We need to update BOTH BankAccount AND DetailsViewModel to keep them in parallel
					using ( con = new SqlConnection ( ConString ) )
					{
						con . Open ( );
						if ( CurrentDb == "BANKACCOUNT" )//|| CurrentDb == "DETAILS" )
						{
							SqlCommand cmd = new SqlCommand ("UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( ss . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , ss . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , ss . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( ss . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( ss . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( ss . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( ss . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( ss . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update successful for Bank Account Data..." );

							cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( ss . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , ss . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , ss . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( ss . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( ss . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( ss . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( ss . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( ss . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update successful for Secondary Accounts Data..." );

							cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( ss . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , ss . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , ss . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( ss . AcType ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( ss . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( ss . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update successful for Customers Data..." );
						}
						else if ( CurrentDb == "DETAILS" )
						{
							SqlCommand cmd = new SqlCommand ("UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con);
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( sa . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( sa . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update successful for Bank Account Data..." );

							cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( sa . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( sa . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update successful for Secondary Accounts Data..." );

							cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update successful for Customer Accounts Data..." );
						}
					}
				}
				catch ( Exception ex )
				{
					Console . WriteLine ( $"SQL Error UpdateDbRow(180) - BankAccount/Sec" +
						$"accounts not updated {ex . Message} Data = {ex . Data}" );
				}
				finally
				{
					con . Close ( );
					Console . WriteLine ( $"SQL - Updated Row in ALL Db's after change(s) made in  {CurrentDb}" );
				}

				#endregion BANK/DETAILS UPDATE PROCESSING
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				#region CUSTOMER UPDATE PROCESSING

				//				if ( Row == null && CurrentDb == "CUSTOMER")
				//					cs = Row.Item as CustomerViewModel;
				try
				{
					//Sanity check - are values actualy valid ???
					//They should be as Grid vlaidate entries itself !!
					int x;
					x = Convert . ToInt32 ( cs . Id );
					//					string sndr = sender.ToString();
					x = Convert . ToInt32 ( cs . AcType );
					//Check for invalid A/C Type
					if ( x < 1 || x > 4 )
					{
						Console . WriteLine ( $"SQL UpdateDbRow(204) Invalid A/c type of {cs . AcType} in grid Data" );
						MessageBox . Show ( $"Invalid A/C Type ({cs . AcType}) in the Grid !!!!\r\nPlease correct this entry!" );
						return false;
					}
					DateTime dtm = Convert.ToDateTime (cs.ODate);
					dtm = Convert . ToDateTime ( cs . CDate );
					dtm = Convert . ToDateTime ( cs . Dob );
				}
				catch ( Exception ex )
				{
					Console . WriteLine ( $"SQL Invalid grid Data UpdateDbRow(214)- {ex . Message} Data = {ex . Data}" );
					MessageBox . Show ( "Invalid data entered in the Grid !!!! - See Output for details" );
					return false;
				}
				SqlConnection con;
				string ConString = "";
				ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
				con = new SqlConnection ( ConString );
				try
				{
					//We need to update BOTH BankAccount AND DetailsViewModel to keep them in parallel
					using ( con )
					{
						con . Open ( );

						SqlCommand cmd = new SqlCommand ("UPDATE Customer SET CUSTNO=@custno, BANKNO=@bankno, ACTYPE=@actype, " +
							"FNAME=@fname, LNAME=@lname, ADDR1=@addr1, ADDR2=@addr2, TOWN=@town, COUNTY=@county, PCODE=@pcode," +
							"PHONE=@phone, MOBILE=@mobile, DOB=@dob,ODATE=@odate, CDATE=@cdate WHERE Id=@id", con);

						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( cs . Id ) );
						cmd . Parameters . AddWithValue ( "@custno" , cs . CustNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@bankno" , cs . BankNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( cs . AcType ) );
						cmd . Parameters . AddWithValue ( "@fname" , cs . FName . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@lname" , cs . LName . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@addr1" , cs . Addr1 . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@addr2" , cs . Addr2 . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@town" , cs . Town . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@county" , cs . County . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@pcode" , cs . PCode . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@phone" , cs . Phone . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@mobile" , cs . Mobile . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@dob" , Convert . ToDateTime ( cs . Dob ) );
						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( cs . ODate ) );
						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( cs . CDate ) );
						cmd . ExecuteNonQuery ( );
						Console . WriteLine ( "SQL Update successful for Customers Data..." );

						cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( cs . Id ) );
						cmd . Parameters . AddWithValue ( "@bankno" , cs . BankNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@custno" , cs . CustNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( cs . AcType ) );
						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( cs . ODate ) );
						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( cs . CDate ) );
						cmd . ExecuteNonQuery ( );
						Console . WriteLine ( "SQL Update successful for Bank Account Data..." );

						cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( cs . Id ) );
						cmd . Parameters . AddWithValue ( "@bankno" , cs . BankNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@custno" , cs . CustNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( cs . AcType ) );
						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( cs . ODate ) );
						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( cs . CDate ) );
						cmd . ExecuteNonQuery ( );
						Console . WriteLine ( "SQL Update successful for Secondary Accounts Data..." );
					}
				}
				catch ( Exception ex )
				{
					con . Close ( );
					Console . WriteLine ( $"SQL Error UpdateDbRow(255)- {ex . Message} Data = {ex . Data}" );
				}
				finally
				{
					//Lets force the grids to update when we return from here ??
					con . Close ( );
					Console . WriteLine ( $"SQL - Updated Row in ALL Db's after change(s) made in  {CurrentDb}" );
				}
				return true;

				#endregion CUSTOMER UPDATE PROCESSING
			}
			return true;
		}
		public void UpdateAllDb ( string CurrentDb , DataGridRowEditEndingEventArgs e , int caller = -1)
		{
			BankCollection bss = BankViewercollection;
			CustCollection ccs = CustCollection .Custcollection;
			DetCollection dsa = DetCollection .Detcollection ;
			BankAccountViewModel ss= new BankAccountViewModel();
			CustomerViewModel cs = new CustomerViewModel ();
			DetailsViewModel sa = new  DetailsViewModel();

			if ( CurrentDb == "BANKACCOUNT" || CurrentDb == "DETAILS" )
			{
				// Editdb is NOT OPEN
				SqlCommand cmd = null;
				try
				{
					//Sanity check - are values actualy valid ???
					//They should be as Grid vlaidate entries itself !!
					int x;
					decimal Y;
					if ( CurrentDb == "BANKACCOUNT" )
					{
					
						ss = e . Row . Item as BankAccountViewModel;
						x = Convert . ToInt32 ( ss . Id );
						x = Convert . ToInt32 ( ss . AcType );
						//Check for invalid A/C Type
						if ( x < 1 || x > 4 )
						{
							Console . WriteLine ( $"SQL Invalid A/c type of {ss . AcType} in grid Data" );
							Mouse . OverrideCursor = Cursors . Arrow;
							MessageBox . Show ( $"Invalid A/C Type ({ss . AcType}) in the Grid !!!!\r\nPlease correct this entry!" );
							return;
						}
						Y = Convert . ToDecimal ( ss . Balance );
						Y = Convert . ToDecimal ( ss . IntRate );
						//Check for invalid Interest rate
						if ( Y > 100 )
						{
							Console . WriteLine ( $"SQL Invalid Interest Rate of {ss . IntRate} > 100% in grid Data" );
							Mouse . OverrideCursor = Cursors . Arrow;
							MessageBox . Show ( $"Invalid Interest rate ({ss . IntRate}) > 100 entered in the Grid !!!!\r\nPlease correct this entry!" );
							return;
						}
						DateTime dtm = Convert . ToDateTime ( ss . ODate );
						dtm = Convert . ToDateTime ( ss . CDate );
						sa . Id = ss . Id;
						sa . BankNo = ss . BankNo;
						sa . CustNo = ss . CustNo;
						sa . AcType = ss . AcType;
						sa . Balance = ss . Balance;
						sa . IntRate = ss . IntRate;
						sa . ODate = ss . ODate;
						sa .CDate = ss . CDate;
					}
					else if ( CurrentDb == "DETAILS" )
					{
						//						sa = sacc;
						sa = e . Row . Item as DetailsViewModel;
						x = Convert . ToInt32 ( sa . Id );
						x = Convert . ToInt32 ( sa . AcType );
						//Check for invalid A/C Type
						if ( x < 1 || x > 4 )
						{
							Console . WriteLine ( $"SQL Invalid A/c type of {sa . AcType} in grid Data" );
							Mouse . OverrideCursor = Cursors . Arrow;
							MessageBox . Show ( $"Invalid A/C Type ({sa . AcType}) in the Grid !!!!\r\nPlease correct this entry!" );
							return;
						}
						Y = Convert . ToDecimal ( sa . Balance );
						Y = Convert . ToDecimal ( sa . IntRate );
						//Check for invalid Interest rate
						if ( Y > 100 )
						{
							Console . WriteLine ( $"SQL Invalid Interest Rate of {sa . IntRate} > 100% in grid Data" );
							Mouse . OverrideCursor = Cursors . Arrow;
							MessageBox . Show ( $"Invalid Interest rate ({sa . IntRate}) > 100 entered in the Grid !!!!\r\nPlease correct this entry!" );
							return;
						}
						DateTime dtm = Convert . ToDateTime ( sa . ODate );
						dtm = Convert . ToDateTime ( sa . CDate );

						ss . Id = sa . Id;
						ss . BankNo = sa . BankNo;
						ss . CustNo = sa . CustNo;
						ss . AcType = sa . AcType;
						ss . Balance = sa . Balance;
						ss . IntRate = sa . IntRate;
						ss . ODate = sa . ODate;
						ss . CDate = sa . CDate;
					}
					//					string sndr = sender.ToString();
				}
				catch ( Exception ex )
				{
					Console . WriteLine ( $"SQL Invalid grid Data - {ex . Message} Data = {ex . Data}" );
					Mouse . OverrideCursor = Cursors . Arrow;
					MessageBox . Show ( "Invalid data entered in the Grid !!!! - See Output for details" );
					return;
				}
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

						if ( CurrentDb == "BANKACCOUNT" )
						{
							cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( ss . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , ss . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , ss . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( ss . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( ss . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( ss . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( ss . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( ss . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update of BankAccounts successful..." );

							cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( sa . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( sa . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update of SecAccounts successful..." );

							cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update of Customers successful..." );
						}
						else if ( CurrentDb == "DETAILS" )
						{
							cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( sa . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( sa . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update of BankAccounts successful..." );

							cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( sa . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( sa . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update of SecAccounts successful..." );

							cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update of customers successful..." );
						}
						if ( CurrentDb == "SECACCOUNTS" )
						{
							cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( ss . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , ss . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , ss . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( ss . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( ss . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( ss . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( ss . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( ss . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update of BankAccounts successful..." );

							cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@balance" , Convert . ToDecimal ( sa . Balance ) );
							cmd . Parameters . AddWithValue ( "@intrate" , Convert . ToDecimal ( sa . IntRate ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update of SecAccounts successful..." );

							cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
							cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( sa . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno" , sa . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno" , sa . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( sa . AcType ) );
							cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( sa . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( sa . CDate ) );
							cmd . ExecuteNonQuery ( );
							Console . WriteLine ( "SQL Update of Customers successful..." );
						}
						//						StatusBar . Text = "ALL THREE Databases updated successfully....";
						Console . WriteLine ( "ALL THREE Databases updated successfully...." );
					}
				}
				catch ( Exception ex )
				{
					Console . WriteLine ( $"SQL Error - {ex . Message} Data = {ex . Data}" );

#if SHOWSQLERRORMESSAGEBOX
									Mouse . OverrideCursor = Cursors . Arrow;
									MessageBox . Show ( "SQL error occurred - See Output for details" );
#endif
				}
				finally
				{
					Mouse . OverrideCursor = Cursors . Arrow;
					con . Close ( );
				}
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				if ( e == null && CurrentDb == "CUSTOMER" )
					//	cs = this . CustomerGrid . SelectedItem as CustomerViewModel;
					//else if ( e == null && CurrentDb == "CUSTOMER" )
					cs = e . Row . Item as CustomerViewModel;

				try
				{
					//Sanity check - are values actualy valid ???
					//They should be as Grid vlaidate entries itself !!
					int x;
					x = Convert . ToInt32 ( cs . Id );
					//					string sndr = sender.ToString();
					x = Convert . ToInt32 ( cs . AcType );
					//Check for invalid A/C Type
					if ( x < 1 || x > 4 )
					{
						Console . WriteLine ( $"SQL Invalid A/c type of {cs . AcType} in grid Data" );
						Mouse . OverrideCursor = Cursors . Arrow;
						MessageBox . Show ( $"Invalid A/C Type ({cs . AcType}) in the Grid !!!!\r\nPlease correct this entry!" );
						return;
					}
					DateTime dtm = Convert . ToDateTime ( cs . ODate );
					dtm = Convert . ToDateTime ( cs . CDate );
					dtm = Convert . ToDateTime ( cs . Dob );
				}
				catch ( Exception ex )
				{
					Console . WriteLine ( $"SQL Invalid grid Data - {ex . Message} Data = {ex . Data}" );
					MessageBox . Show ( "Invalid data entered in the Grid !!!! - See Output for details" );
					Mouse . OverrideCursor = Cursors . Arrow;
					return;
				}
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
						SqlCommand cmd = new SqlCommand ( "UPDATE Customer SET CUSTNO=@custno, BANKNO=@bankno, ACTYPE=@actype, " +
										"FNAME=@fname, LNAME=@lname, ADDR1=@addr1, ADDR2=@addr2, TOWN=@town, COUNTY=@county, PCODE=@pcode," +
										"PHONE=@phone, MOBILE=@mobile, DOB=@dob,ODATE=@odate, CDATE=@cdate WHERE Id=@id", con );

						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( cs . Id ) );
						cmd . Parameters . AddWithValue ( "@custno" , cs . CustNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@bankno" , cs . BankNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( cs . AcType ) );
						cmd . Parameters . AddWithValue ( "@fname" , cs . FName . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@lname" , cs . LName . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@addr1" , cs . Addr1 . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@addr2" , cs . Addr2 . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@town" , cs . Town . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@county" , cs . County . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@pcode" , cs . PCode . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@phone" , cs . Phone . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@mobile" , cs . Mobile . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@dob" , Convert . ToDateTime ( cs . Dob ) );
						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( cs . ODate ) );
						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( cs . CDate ) );
						cmd . ExecuteNonQuery ( );
						Console . WriteLine ( "SQL Update of Customers successful..." );

						cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype,  ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( cs . Id ) );
						cmd . Parameters . AddWithValue ( "@bankno" , cs . BankNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@custno" , cs . CustNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( cs . AcType ) );
						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( cs . ODate ) );
						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( cs . CDate ) );
						cmd . ExecuteNonQuery ( );
						Console . WriteLine ( "SQL Update of BankAccounts successful..." );

						cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo" , con );
						cmd . Parameters . AddWithValue ( "@id" , Convert . ToInt32 ( cs . Id ) );
						cmd . Parameters . AddWithValue ( "@bankno" , cs . BankNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@custno" , cs . CustNo . ToString ( ) );
						cmd . Parameters . AddWithValue ( "@actype" , Convert . ToInt32 ( cs . AcType ) );
						cmd . Parameters . AddWithValue ( "@odate" , Convert . ToDateTime ( cs . ODate ) );
						cmd . Parameters . AddWithValue ( "@cdate" , Convert . ToDateTime ( cs . CDate ) );
						cmd . ExecuteNonQuery ( );
						Console . WriteLine ( "SQL Update of SecAccounts successful..." );
					}
					//				StatusBar . Text = "ALL THREE Databases updated successfully....";
					Console . WriteLine ( "ALL THREE Databases updated successfully...." );
				}
				catch ( Exception ex )
				{
					Console . WriteLine ( $"SQL Error - {ex . Message} Data = {ex . Data}" );
#if SHOWSQLERRORMESSAGEBOX
									Mouse . OverrideCursor = Cursors . Arrow;
									MessageBox . Show ( "SQL error occurred - See Output for details" );
#endif
				}
				finally
				{
					Mouse . OverrideCursor = Cursors . Arrow;
					con . Close ( );
				}
				Mouse . OverrideCursor = Cursors . Arrow;
				return;
			}

		}

	}
}
