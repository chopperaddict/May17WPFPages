using System;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Imaging;
using System . Windows . Shapes;
using WPFPages . ViewModels;
namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for RowInfoPopup.xaml
	/// </summary>
	public partial class RowInfoPopup : Window
	{
		private string CurrentDb = "";
		private bool IsDirty = false;
		private DataGrid ParentGrid = null;

		//BankCollection bc = new BankCollection ( );
		//public BankCollection Bankcollection;// = bc.Bankcollection;
		//public CustCollection Custcollection = CustCollection . Custcollection;
		//public DetCollection Detcollection = DetCollection.Detcollection;

		public RowInfoPopup ( string callerType , DataGrid parentGrid,  DataGridRow RowData)
		{
			ParentGrid = parentGrid;
			DataGridRow dgr = RowData;
			try
			{
				//store the tyoe of Db we are working with
				CurrentDb = callerType;
				if ( callerType == "" )
					return;
				InitializeComponent ( );
				if ( callerType == "BANKACCOUNT" )
				{
					BankLabels . Visibility = Visibility . Visible;
					BankData . Visibility = Visibility . Visible;
					CustLabels . Visibility = Visibility . Hidden;
					CustData . Visibility = Visibility . Hidden;
					LeftCustBorder . Visibility = Visibility . Hidden;
					LeftBankBorder . Visibility = Visibility . Visible;
					//					BankAccountViewModel bvm = new BankAccountViewModel();
					//					DataContext = bvm.BankAccountObs;
					this . Height = 350;
				}

				if ( callerType == "CUSTOMER" )
				{
					CustData . Visibility = Visibility . Visible;
					CustLabels . Visibility = Visibility . Visible;
					BankLabels . Visibility = Visibility . Hidden;
					BankData . Visibility = Visibility . Hidden;
					LeftCustBorder . Visibility = Visibility . Visible;
					LeftBankBorder . Visibility = Visibility . Hidden;
					//					CustomerViewModel cvm = new CustomerViewModel ( );
					//					DataContext = cvm . CustomersObs;
					this . Height = 597;
				}

				if ( callerType == "DETAILS" )
				{
					BankLabels . Visibility = Visibility . Visible;
					BankData . Visibility = Visibility . Visible;
					CustData . Visibility = Visibility . Hidden;
					CustLabels . Visibility = Visibility . Hidden;
					LeftCustBorder . Visibility = Visibility . Hidden;
					LeftBankBorder . Visibility = Visibility . Visible;
					//					DetailsViewModel dvm = new DetailsViewModel ( );
					//					DataContext = Detcollection;
					this . Height = 350;
				}
				this . MouseDown += delegate { DoDragMove ( ); };
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"General Exception : {ex . Message}, {ex . Data}" );
			}

			if ( IsDirty )
				SaveBtn . Visibility = Visibility . Visible;
		}
		private void DoDragMove ( )
		{
			//Handle the button NOT being the left mouse button
			// which will crash the DragMove Fn.....
			try
			{
				this . DragMove ( );
			}
			catch
			{
				return;
			}
		}

		private void ButtonBase_OnClick ( object sender, RoutedEventArgs e )
		{
			this . Close ( );
		}

		private void Save_Click ( object sender, RoutedEventArgs e )
		{

		}

		private void Window_KeyDown ( object sender, KeyEventArgs e )
		{
			if ( e . Key == Key . Escape )
			{
				Close ( );
			}
		}

		private void ODateLostFocus ( object sender, RoutedEventArgs e )
		{
			UpdateCollection();
		}


		private void AcTypeLostFocus ( object sender, RoutedEventArgs e )
		{UpdateCollection();}

		private void CDateLostFocus ( object sender, RoutedEventArgs e )
		{ UpdateCollection ( ); }

		private void MobileLostFocus ( object sender, RoutedEventArgs e )
		{ UpdateCollection ( ); }

		private void PhoneLostFocus ( object sender, RoutedEventArgs e )
		{ UpdateCollection ( ); }

		private void PCodeLostFocus ( object sender, RoutedEventArgs e )
		{ UpdateCollection ( ); }

		private void CountyLostFocus ( object sender, RoutedEventArgs e )
		{ UpdateCollection ( ); }

		private void TownLostFocus ( object sender, RoutedEventArgs e )
		{ UpdateCollection ( ); }

		private void Addr2LostFocus ( object sender, RoutedEventArgs e )
		{ UpdateCollection ( ); }

		private void Addr1LostFocus ( object sender, RoutedEventArgs e )
		{ UpdateCollection ( ); }

		private void LNameLostFocus ( object sender, RoutedEventArgs e )
		{ UpdateCollection ( ); }

		private void FNameLostFocus ( object sender, RoutedEventArgs e )
		{ UpdateCollection ( ); }
		private void UpdateCollection ( )
		{
			return;
			//if ( CurrentDb == "BANKACCOUNT" )
			//{

			//}
			//if ( CurrentDb == "CUSTOMER" )
			//{
			//	int selected;
			//	selected = ParentGrid.SelectedIndex;
			//	ParentGrid.ItemsSource = null;
			//	ParentGrid.ItemsSource = CustCollection.Custcollection;
			//	ParentGrid.SelectedIndex = selected;
			//	ParentGrid . Refresh();
			//}
			//if ( CurrentDb == "DETAILS" )
			//{
			//}
		}
	}
}