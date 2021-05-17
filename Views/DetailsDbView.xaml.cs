using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
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
	/// Interaction logic for DetailsDbView.xaml
	/// </summary>
	public partial class DetailsDbView : Window
	{
		public static DetCollection DetViewercollection = DetCollection.Detcollection;
		private bool IsDirty = false;
		static bool Startup = true;

		private string _bankno = "";
		private string _custno = "";
		private string _actype = "";
		private string _balance = "";
		private string _odate = "";
		private string _cdate = "";
		public DetailsDbView ( )
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
		private void Window_Loaded ( object sender, RoutedEventArgs e )
		{
			Startup = true;
			// Data source is handled in XAML !!!!
			if ( this . DetGrid . Items . Count > 0 )
				this . DetGrid . Items . Clear ( );
			this . DetGrid . ItemsSource = DetViewercollection;

			if ( DetViewercollection . Count == 0 )
				DetViewercollection = DetCollection . LoadDet( DetViewercollection );
			this . DetGrid . ItemsSource = DetViewercollection;
			this . MouseDown += delegate { DoDragMove ( ); };
			DataFields . DataContext = this . DetGrid . SelectedItem;

			EventControl . ViewerDataHasBeenChanged += ExternalDataUpdate;      // Callback in THIS FILE
											    //Subscribe to Bank Data Changed event declared in EventControl
			EventControl . DetDataLoaded += EventControl_DetDataLoaded;
			SaveBttn . IsEnabled = false;
			Startup = false;
			Count . Text = this . DetGrid . Items . Count . ToString ( );

		}

		public void ExternalDataUpdate ( int DbEditChangeType, int row, string currentDb )
		{
			// Reciiving Notifiaction from a remote viewer that data has been changed, so we MUST now update our DataGrid
			Console . WriteLine ( $"BankDbView : Data changed event notification received successfully." );
			this . DetGrid . ItemsSource = null;
			this . DetGrid . Items . Clear ( );
			this . DetGrid . ItemsSource = DetViewercollection;
			this . DetGrid . Refresh ( );
		}
		#endregion Startup/ Closedown



		private void Where ( bool v )
		{
			throw new NotImplementedException ( );
		}

		private void ViewerGrid_RowEditEnding ( object sender, System . Windows . Controls . DataGridRowEditEndingEventArgs e )
		{
			// Save changes and tell other viewers about the change
			int currow = 0;
			currow = this . DetGrid . SelectedIndex;
			// Save current row so we can reposition correctly at end of the entire refresh process					
			Flags . SqlBankCurrentIndex = currow;
			DetailsViewModel ss = new DetailsViewModel ( );
			ss = this . DetGrid . SelectedItem as DetailsViewModel;
			// This is the NEW DATA from the current row
			SQLHandlers sqlh = new SQLHandlers ( );
			sqlh . UpdateDbRowAsync ( "DETAILS", ss, this . DetGrid . SelectedIndex );

			this . DetGrid . SelectedIndex = Flags . SqlBankCurrentIndex;
			this . DetGrid . ScrollIntoView ( Flags . SqlBankCurrentIndex );
			// Notify EditDb to upgrade its grid
			if ( Flags . CurrentEditDbViewer != null )
				Flags . CurrentEditDbViewer . UpdateGrid ( "DETAILS" );

			// ***********  DEFINITE WIN  **********
			// This DOES trigger a notidfication to SQLDBVIEWER for sure !!!   14/5/21
			EventControl . TriggerViewerDataChanged ( 2, this . DetGrid . SelectedIndex, "DETAILS" );

		}

		private void EventControl_DetDataLoaded ( object sender, LoadedEventArgs e )
		{
			// Event handler for BankDataLoaded
			this . DetGrid . ItemsSource = null;
			this . DetGrid . ItemsSource = DetViewercollection;
			this . DetGrid . Refresh ( );
		}

		private void ShowBank_KeyDown ( object sender, System . Windows . Input . KeyEventArgs e )
		{

		}

		private void Close_Click ( object sender, RoutedEventArgs e )
		{
			Close ( );
		}

		private void Window_Closing ( object sender, System . ComponentModel . CancelEventArgs e )
		{
			EventControl . ViewerDataHasBeenChanged -= ExternalDataUpdate;      // Callback in THIS FILE
											    //UnSubscribe from Bank Data Changed event declared in EventControl
			EventControl . BankDataLoaded -= EventControl_DetDataLoaded;
			DetViewercollection = null;

		}

		private void DetGrid_SelectionChanged ( object sender, System . Windows . Controls . SelectionChangedEventArgs e )
		{
			if ( IsDirty )
			{
				MessageBoxResult result = MessageBox . Show
					( "You have unsaved changes.  Do you want them saved now ?", "Possible Data Loss", MessageBoxButton . YesNo, MessageBoxImage . Question, MessageBoxResult . Yes );
				if ( result == MessageBoxResult . Yes )
				{
					SaveButton ( );
				}
				// Do not want ot save it, so disable  save button again
				SaveBttn . IsEnabled = false;
				IsDirty = false;
			}
			if ( this . DetGrid . SelectedItem == null )
				return;
			this . DetGrid . ScrollIntoView ( this . DetGrid . SelectedItem );
			Startup = true;
			DataFields . DataContext = this . DetGrid . SelectedItem;
			Startup = false;
		}

		private async Task<bool> SaveButton ( object sender = null, RoutedEventArgs e = null )
		{
			//inprogress = true;
			//bindex = this . BankGrid . SelectedIndex;
			//cindex = this . CustomerGrid . SelectedIndex;
			//dindex = this . DetailsGrid . SelectedIndex;

			// Get the current rows data
			IsDirty = false;
			int CurrentSelection = this . DetGrid . SelectedIndex;
			this . DetGrid . SelectedItem = this . DetGrid . SelectedIndex;
			DetailsViewModel bvm = new DetailsViewModel ( );
			bvm = this . DetGrid . SelectedItem as DetailsViewModel;

			SaveFieldData ( );

			// update the current rows data content to send  to Update process
			bvm . BankNo = Bankno . Text;
			bvm . CustNo = Custno . Text;
			bvm . AcType = Convert . ToInt32 ( acType . Text );
			bvm . Balance = Convert . ToDecimal ( balance . Text );
			bvm . ODate = Convert . ToDateTime ( odate . Text );
			bvm . CDate = Convert . ToDateTime ( cdate . Text );
			// Call Handler to update ALL Db's via SQL
			SQLHandlers sqlh = new SQLHandlers ( );
			await sqlh . UpdateDbRow ( "DETAILS", bvm );
			EventControl . TriggerBankDataLoaded ( DetViewercollection,
				new LoadedEventArgs
				{
					CallerDb = "DETAILS",
					DataSource = DetViewercollection,
					RowCount = this . DetGrid . SelectedIndex
				} );

			//Gotta reload our data because the update clears it down totally to null
			this . DetGrid . SelectedIndex = CurrentSelection;
			this . DetGrid . SelectedItem = CurrentSelection;
			this . DetGrid . Refresh ( );

			SaveBttn . IsEnabled = false;
			return true;
		}


		/// <summary>
		/// Called by ALL edit fields
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SelectionChanged ( object sender, RoutedEventArgs e )
		{
			if ( !Startup )
				SaveFieldData ( );
		}

		/// <summary>
		/// Called by ALL edit fields when text is changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TextChanged ( object sender, TextChangedEventArgs e )
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

		private void OntopChkbox_Click ( object sender, RoutedEventArgs e )
		{
			if ( OntopChkbox . IsChecked == ( bool? ) true )
				this . Topmost = true;
			else
				this . Topmost = false;
		}

		private void SaveBtn ( object sender, RoutedEventArgs e )
		{
			SaveButton ( sender, e );
		}

		private async void MultiAccts_Click ( object sender, RoutedEventArgs e )
		{
			// Filter data to show ONLY Custoimers with multiple bank accounts

			if ( MultiAccounts . Content != "Show All" )
			{
				int currsel = this . DetGrid . SelectedIndex;
				DetailsViewModel bgr = this . DetGrid . SelectedItem as DetailsViewModel;
				Flags . IsMultiMode = true;
				DetCollection det = new DetCollection ( );
				det=  DetCollection .LoadDet(det );
				this . DetGrid . ItemsSource = null;
				this . DetGrid . ItemsSource = det;
				this . DetGrid . Refresh ( );
				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				MultiAccounts . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBorderBrushRed" );
				MultiAccounts . Background = br;
				MultiAccounts . Content = "Show All";
				Count . Text = this . DetGrid . Items . Count . ToString ( );

				// Get Custno from ACTIVE gridso we can find it in other grids
				MultiViewer mv = new MultiViewer ( );
				int rec = mv . FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . DetGrid, "DETAILS" );
				this . DetGrid . SelectedIndex = currsel;
				if ( rec >= 0 )
					this . DetGrid . SelectedIndex = rec;
				else
					this . DetGrid . SelectedIndex = 0;
				Utils . ScrollRecordIntoView ( this . DetGrid, 1 );
			}
			else
			{
				Flags . IsMultiMode = false;
				DetailsViewModel bgr = this . DetGrid . SelectedItem as DetailsViewModel;
				DetCollection det = new DetCollection ( );
				det = DetCollection . LoadDet ( det );
				// Just reset our iremssource to man Db
				this . DetGrid . ItemsSource = null;
				this . DetGrid . ItemsSource = DetViewercollection;
				this . DetGrid . Refresh ( );
				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGreen" );
				MultiAccounts . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGreen" );
				MultiAccounts . Background = br;
				MultiAccounts . Content = "Multi Accounts";
				Count . Text = this . DetGrid . Items . Count . ToString ( );

				MultiViewer mv = new MultiViewer ( );
				int rec = mv . FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . DetGrid, "DETAILS" );
				this . DetGrid . SelectedIndex = 0;

				if ( rec >= 0 )
					this . DetGrid . SelectedIndex = rec;
				else
					this . DetGrid . SelectedIndex = 0;
				Utils . ScrollRecordIntoView ( this . DetGrid, 1 );
			}


			//			BankAccountViewModel bank = new BankAccountViewModel();
			//			var filtered = from bank inDetViewercollection . Where ( x => bank . CustNo = "1055033" ) select x;
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
