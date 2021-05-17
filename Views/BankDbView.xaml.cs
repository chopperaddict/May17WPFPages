using System;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Linq;
using WPFPages . ViewModels;
using System . Windows . Media;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for BankDbView.xaml
	/// </summary>
	public partial class BankDbView : Window
	{
		public  static BankCollection BankViewercollection=new BankCollection();
		private bool IsDirty = false;
		static bool Startup = true;

		private string _bankno = "";
		private string _custno  = "";
		private string _actype = "";
		private string _balance = "";
		private string _odate  = "";
		private string _cdate  = "";
		public BankDbView ( )
		{
			Startup = true;
			InitializeComponent ( );
		}
		#region Mouse support
		private void DoDragMove ( )
		{//Handle the button NOT being the left mouse button
		 // which will crash the DragMove Fn.....
			try
			{ this . DragMove ( ); }
			catch { return; }
		}
		#endregion Mouse support

		#region Startup/ Closedown
		private void Window_Loaded ( object sender , RoutedEventArgs e )
		{
			Startup = true;
			// Data source is handled in XAML !!!!
			if ( this . BankGrid . Items . Count > 0 )
				this . BankGrid . Items . Clear ( );
			this . BankGrid . ItemsSource = BankViewercollection;

			if ( BankViewercollection . Count == 0 )
				BankViewercollection  = BankCollection . LoadBank ( 4 , false);
			this . BankGrid . ItemsSource = BankViewercollection;
			this . MouseDown += delegate { DoDragMove ( ); };
			DataFields . DataContext = this . BankGrid . SelectedItem;

			EventControl . ViewerDataHasBeenChanged += ExternalDataUpdate;      // Callback in THIS FILE
			  //Subscribe to Bank Data Changed event declared in EventControl
			EventControl . BankDataLoaded += EventControl_BankDataLoaded;
			SaveBttn . IsEnabled = false;
			Startup = false;
			Count . Text = this . BankGrid . Items . Count . ToString ( );

		}

		public void ExternalDataUpdate ( int DbEditChangeType , int row , string currentDb )
		{
			// Reciiving Notifiaction from a remote viewer that data has been changed, so we MUST now update our DataGrid
			Console . WriteLine ( $"BankDbView : Data changed event notification received successfully." );
			this . BankGrid . ItemsSource = null;
			this . BankGrid . Items . Clear ( );
			this . BankGrid . ItemsSource =BankViewercollection;
			this . BankGrid . Refresh ( );
		}
		#endregion Startup/ Closedown



		private void Where ( bool v )
		{
			throw new NotImplementedException ( );
		}

		private void ViewerGrid_RowEditEnding ( object sender , System . Windows . Controls . DataGridRowEditEndingEventArgs e )
		{
			// Save changes and tell other viewers about the change
			int currow = 0;
			currow = this . BankGrid . SelectedIndex;
			// Save current row so we can reposition correctly at end of the entire refresh process					
			Flags . SqlBankCurrentIndex = currow;
			BankAccountViewModel ss = new BankAccountViewModel();
			ss = this . BankGrid . SelectedItem as BankAccountViewModel;
			// This is the NEW DATA from the current row
			SQLHandlers sqlh = new SQLHandlers();
			sqlh . UpdateDbRowAsync ( "BANKACCOUNT" , ss , this . BankGrid . SelectedIndex );

			this . BankGrid . SelectedIndex = Flags . SqlBankCurrentIndex;
			this . BankGrid . ScrollIntoView ( Flags . SqlBankCurrentIndex );
			// Notify EditDb to upgrade its grid
			if ( Flags . CurrentEditDbViewer != null )
				Flags . CurrentEditDbViewer . UpdateGrid ( "BANKACCOUNT" );

			// ***********  DEFINITE WIN  **********
			// This DOES trigger a notidfication to SQLDBVIEWER for sure !!!   14/5/21
			EventControl . TriggerViewerDataChanged ( 2 , this . BankGrid . SelectedIndex , "BANKACCOUNT" );

		}

		private void EventControl_BankDataLoaded ( object sender , LoadedEventArgs e )
		{
			// Event handler for BankDataLoaded
			this . BankGrid . ItemsSource = null;
			this . BankGrid . ItemsSource =BankViewercollection;
			this . BankGrid . Refresh ( );
		}

		private void ShowBank_KeyDown ( object sender , System . Windows . Input . KeyEventArgs e )
		{

		}

		private void Close_Click ( object sender , RoutedEventArgs e )
		{
			Close ( );
		}

		private void Window_Closing ( object sender , System . ComponentModel . CancelEventArgs e )
		{
			EventControl . ViewerDataHasBeenChanged -= ExternalDataUpdate;      // Callback in THIS FILE
			//UnSubscribe from Bank Data Changed event declared in EventControl
			EventControl . BankDataLoaded -= EventControl_BankDataLoaded;

		}

		private void BankGrid_SelectionChanged ( object sender , System . Windows . Controls . SelectionChangedEventArgs e )
		{
			if ( IsDirty )
			{
				MessageBoxResult result = MessageBox . Show
					( "You have unsaved changes.  Do you want them saved now ?" , "P:ossible Data Loss" , MessageBoxButton . YesNo , MessageBoxImage . Question , MessageBoxResult . Yes );
				if ( result == MessageBoxResult . Yes )
				{
					SaveButton ( );
				}
				// Do not want ot save it, so disable  save button again
				SaveBttn . IsEnabled = false;
				IsDirty = false;
			}
			if ( this . BankGrid . SelectedItem == null )
				return;
			this . BankGrid . ScrollIntoView ( this . BankGrid . SelectedItem );
			Startup = true;
			DataFields . DataContext = this . BankGrid . SelectedItem;
			Startup = false;
		}

		private async Task<bool> SaveButton ( object sender = null , RoutedEventArgs e = null )
		{
			//inprogress = true;
			//bindex = this . BankGrid . SelectedIndex;
			//cindex = this . CustomerGrid . SelectedIndex;
			//dindex = this . DetailsGrid . SelectedIndex;

			// Get the current rows data
			IsDirty = false;
			int CurrentSelection = this . BankGrid . SelectedIndex;
			this . BankGrid . SelectedItem = this . BankGrid . SelectedIndex;
			BankAccountViewModel bvm = new BankAccountViewModel   ();
			bvm = this . BankGrid . SelectedItem as BankAccountViewModel;

			SaveFieldData ( );

			// update the current rows data content to send  to Update process
			bvm . BankNo = Bankno . Text;
			bvm . CustNo = Custno . Text;
			bvm . AcType = Convert . ToInt32 ( acType . Text );
			bvm . Balance = Convert . ToDecimal ( balance . Text );
			bvm . ODate = Convert . ToDateTime ( odate . Text );
			bvm . CDate = Convert . ToDateTime ( cdate . Text );
			// Call Handler to update ALL Db's via SQL
			SQLHandlers sqlh = new SQLHandlers  ();
			await sqlh . UpdateDbRow ( "BANKACCOUNT" , bvm );
			EventControl . TriggerBankDataLoaded (BankViewercollection ,
				new LoadedEventArgs
				{
					CallerDb = "BANKACCOUNT" ,
					DataSource =BankViewercollection ,
					RowCount = this . BankGrid . SelectedIndex
				} );

			//Gotta reload our data because the update clears it down totally to null
			this . BankGrid . SelectedIndex = CurrentSelection;
			this . BankGrid . SelectedItem = CurrentSelection;
			this . BankGrid . Refresh ( );

			SaveBttn . IsEnabled = false;
			return true;
		}


		/// <summary>
		/// Called by ALL edit fields
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SelectionChanged ( object sender , RoutedEventArgs e )
		{
			if ( !Startup )
				SaveFieldData ( );
		}

		/// <summary>
		/// Called by ALL edit fields when text is changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TextChanged ( object sender , TextChangedEventArgs e )
		{ if ( !Startup ) CompareFieldData ( ); }

		private void SaveFieldData ( )
		{
			_bankno = Bankno . Text;
			_custno = Custno . Text;
			_actype = acType . Text;
			_balance = balance . Text;
			_odate = odate . Text;
			_cdate = cdate . Text;
		}
		private void CompareFieldData ( )
		{
			if ( SaveBttn == null )
				return;
			if ( _bankno != Bankno . Text )
				SaveBttn . IsEnabled = true;
			if ( _custno != Custno . Text )
				SaveBttn . IsEnabled = true;
			if ( _actype != acType . Text )
				SaveBttn . IsEnabled = true;
			if ( _balance != balance . Text )
				SaveBttn . IsEnabled = true;
			if ( _odate != odate . Text )
				SaveBttn . IsEnabled = true;
			if ( _cdate != cdate . Text )
				SaveBttn . IsEnabled = true;

			if ( SaveBttn . IsEnabled )
				IsDirty = true;
		}

		private void OntopChkbox_Click ( object sender , RoutedEventArgs e )
		{
			if ( OntopChkbox . IsChecked == ( bool? ) true )
				this . Topmost = true;
			else
				this . Topmost = false;
		}

		private void SaveBtn ( object sender , RoutedEventArgs e )
		{
			SaveButton ( sender , e );
		}

		private async void MultiAccts_Click ( object sender , RoutedEventArgs e )
		{
			// Filter data to show ONLY Custoimers with multiple bank accounts

			if ( MultiAccounts . Content != "Show All" )
			{
				Flags . IsMultiMode = true;
				BankCollection bank = new BankCollection();
				bank = await bank . ReLoadBankData ( );
				this . BankGrid . ItemsSource = null;
				this . BankGrid . ItemsSource = bank;
				this . BankGrid . Refresh ( );
				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				MultiAccounts . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBorderBrushRed" );
				MultiAccounts . Background = br;
				MultiAccounts . Content = "Show All";
				Count . Text = this . BankGrid . Items . Count.ToString();
			}
			else
			{
				Flags . IsMultiMode = false;
				BankCollection bank = new BankCollection();
//				bank = await bank . ReLoadBankData ( );
				// Just reset our iremssource to man Db
				this . BankGrid . ItemsSource = null;
				this . BankGrid . ItemsSource =BankViewercollection;
				this . BankGrid . Refresh ( );
				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateBlue" );
				MultiAccounts . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushBlue" );
				MultiAccounts . Background = br;
				MultiAccounts . Content = "Multi Accounts";
				Count . Text = this . BankGrid . Items . Count . ToString ( );

			}


			//			BankAccountViewModel bank = new BankAccountViewModel();
			//			var filtered = from bank inBankViewercollection . Where ( x => bank . CustNo = "1055033" ) select x;
			//		   GroupBy bank.CustNo having count(*) > 1
			//where  
			//having COUNT (*) > 1
			//	select bank;
			//	Where ( b.CustNo = "1055033") ;
			/*
						commandline = $"SELECT * FROM BANKACCOUNT WHERE CUSTNO IN "
				+ $"(SELECT CUSTNO FROM BANKACCOUNT "
				+ $" GROUP BY CUSTNO"
				+ $" HAVING COUNT(*) > 1) ORDER BY ";

			    */


		}
	}
}
