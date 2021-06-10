using System;
using System . Collections . Generic;
using System . Data . SqlClient;
using System . Diagnostics;
using System . Linq;
using System . Linq . Expressions;
using System . Reflection;
using System . Runtime . CompilerServices;
using System . Security . AccessControl;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Controls . Primitives;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Forms;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Imaging;
using System . Windows . Shapes;
using WPFPages . Properties;
using WPFPages . ViewModels;

namespace WPFPages . Views
{

	/// <summary>
	/// Interaction logic for GetExportRecords.xaml
	/// </summary>
	public partial class GetExportRecords : Window
	{
		public List<BankAccountViewModel> Bankrecords = new List<BankAccountViewModel> ( );
		public List<BankAccountViewModel> selectedBankData = new List<BankAccountViewModel> ( );
		//public static List<CustomerViewModel> Custrecords = new List<CustomerViewModel> ( );
		public List<CustomerViewModel> selectedCustData = new List<CustomerViewModel> ( );
		public List<DetailsViewModel> Detrecords = new List<DetailsViewModel> ( );
		public List<DetailsViewModel> selectedDetData = new List<DetailsViewModel> ( );
		public string CurrentDb = "";

		List<BankAccountViewModel> BankMoved = new List<BankAccountViewModel> ( );
		List<DetailsViewModel> DetMoved = new List<DetailsViewModel> ( );

		public GetExportRecords ( string currentDb,
						ref List<BankAccountViewModel> Bankrecordreceived,
						ref List<DetailsViewModel> Detrecordreceived )
		{
			// We receive pointers to both a Bank and a Details collection
			InitializeComponent ( );
			CurrentDb = currentDb;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				// Setup internal pointers to our Collections
				Bankrecords = Bankrecordreceived;
				Detrecords = Detrecordreceived;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				// Setup internal pointers to our Collections
				Bankrecords = Bankrecordreceived;
				Detrecords = Detrecordreceived;
			}
		}
		private void Cancelbutton_Click ( object sender, RoutedEventArgs e )
		{
			// Clear databases so no update occurs on return To caller function
			Bankrecords = null;
			Detrecords = null;
			Close ( );
		}

		private void Gobutton_Click ( object sender, RoutedEventArgs e )
		{
			DbManipulation . OutputSelectedRecords ( CurrentDb, selectedBankData, selectedCustData, selectedDetData );
		}

		private void OnLoaded ( object sender, RoutedEventArgs e )
		{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				dataGrid . ItemsSource = Bankrecords;
				selectedGrid . ItemsSource = selectedBankData;
				UpperGridName . Text = "Bank Account Database";
				LowerGridName . Text = "Details Account Database";
			}
			else if ( CurrentDb == "DETAILS" )
			{
				dataGrid . ItemsSource = Detrecords;
				selectedGrid . ItemsSource = selectedDetData;
				LowerGridName . Text = "Bank Account Database";
				UpperGridName . Text = "Details Account Database";
			}
			dataGrid . SelectedIndex = 0;
			dataGrid . SelectedItem = 0;
			dataGrid . Refresh ( );
		}

		private async void Select_Click ( object sender, RoutedEventArgs e )
		{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				EventControl . TriggerTransferDataUpdated ( this, new LoadedEventArgs
				{
					DataSource = Detrecords,
					CallerType = "Updatemultiaccounts",
					CallerDb = CurrentDb
				} ); ;
			}
			else
			{
				EventControl . TriggerTransferDataUpdated ( this, new LoadedEventArgs
				{
					DataSource = Bankrecords,
					CallerType = "Updatemultiaccounts",
					CallerDb = CurrentDb
				} );
			}
			Close ( );
		}

		/// <summary>
		/// Handles doubleclick on a record by copying it AND any other records
		/// with the same CustNo and FIRSTLY copies them into the lower grid.
		/// 
		/// It then iterates through the top list and removes them from there before 
		/// redisplaying both grids
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void dataGrid_MouseDoubleClick ( object sender, MouseButtonEventArgs e )
		{
			int selindex = 0, delindex = 0;
			string custno = "", bankno = "";
			List<int> deleted = new List<int> ( );
			if ( CurrentDb == "BANKACCOUNT" )
			{
				// File Handling Bank moving records to Details
				BankAccountViewModel bvm = new BankAccountViewModel ( );
				BankAccountViewModel tmp = new BankAccountViewModel ( );

				selindex = dataGrid . SelectedIndex;
				bvm = dataGrid . SelectedItem as BankAccountViewModel;
				bankno = bvm . BankNo;
				custno = bvm . CustNo;
				//Get all records in our selected Bank records grid matching this customer #
				foreach ( var item in dataGrid . Items )
				{
					tmp = item as BankAccountViewModel;
					if ( tmp == null )
						break;
					if ( tmp . CustNo == custno )
					{
						//Add to lower list
						selectedBankData . Add ( tmp );

						// Bit convoluted here, but we massage a Bank record into a Details record witht he Create Fn call
						// and threnadd it to our list that is passedback to the caller 
						DetailsViewModel dvm = new DetailsViewModel ( );
						dvm = CreateDetailsRecord ( tmp );
						Detrecords . Add ( dvm );
					}
				}
				// iterate thru from end working backwards
				for ( int i = dataGrid . Items . Count - 1 ; i >= 0 ; i-- )
				{
					tmp = dataGrid . Items [ i ] as BankAccountViewModel;
					if ( tmp . CustNo == custno )
						Bankrecords . Remove ( tmp );
				}

				dataGrid . ItemsSource = null;
				dataGrid . ItemsSource = Bankrecords;
				dataGrid . Refresh ( );
				selectedGrid . ItemsSource = null;
				selectedGrid . ItemsSource = selectedBankData;
				selectedGrid . Refresh ( );
			}
			if ( CurrentDb == "DETAILS" )
			{
				// File Handling Details moving records to Bank
				DetailsViewModel dvm = new DetailsViewModel ( );
				DetailsViewModel tmp = new DetailsViewModel ( );

				selindex = dataGrid . SelectedIndex;
				dvm = dataGrid . SelectedItem as DetailsViewModel;
				custno = dvm . CustNo;
				//Get all records matching this customer No 
				foreach ( var item in dataGrid . Items )
				{
					tmp = item as DetailsViewModel;
					if ( tmp == null )
						break;
					if ( tmp . CustNo == custno )
					{
						//Add to lower list
						selectedDetData . Add ( tmp );
						// Bit convoluted here, but we massage a Bank record into a Details record witht he Create Fn call
						// and threnadd it to our list that is passedback to the caller 
						BankAccountViewModel bvm = new BankAccountViewModel ( );
						bvm = CreateBankRecord ( tmp );
						Bankrecords . Add ( bvm );
					}
				}
				// iterate thru from end working backwards as we are deleting records from the grid
				for ( int i = dataGrid . Items . Count - 1 ; i >= 0 ; i-- )
				{
					tmp = dataGrid . Items [ i ] as DetailsViewModel;
					if ( tmp . CustNo == custno )
						Detrecords . Remove ( tmp );
				}
				dataGrid . ItemsSource = null;
				dataGrid . ItemsSource = Detrecords;
				dataGrid . Refresh ( );
				selectedGrid . ItemsSource = null;
				selectedGrid . ItemsSource = selectedDetData;
				selectedGrid . Refresh ( );
			}
		}
		private BankAccountViewModel CreateBankRecord ( DetailsViewModel dvm )
		{
			BankAccountViewModel bvm = new BankAccountViewModel ( );
			bvm . CustNo = dvm . CustNo;
			bvm . BankNo = dvm . BankNo;
			bvm . AcType = dvm . AcType;
			bvm . IntRate = dvm . IntRate;
			bvm . Balance = dvm . Balance;
			bvm . ODate = dvm . ODate;
			bvm . CDate = dvm . CDate;
			return bvm;
		}
		private DetailsViewModel CreateDetailsRecord ( BankAccountViewModel bvm )
		{
			DetailsViewModel dvm = new DetailsViewModel ( );
			dvm . CustNo = bvm . CustNo;
			dvm . BankNo = bvm . BankNo;
			dvm . AcType = bvm . AcType;
			dvm . IntRate = bvm . IntRate;
			dvm . Balance = bvm . Balance;
			dvm . ODate = bvm . ODate;
			dvm . CDate = bvm . CDate;
			return dvm;
		}
	}
}

