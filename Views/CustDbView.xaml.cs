using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Forms;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Imaging;
using System . Windows . Shapes;
using WPFPages . ViewModels;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for CustDbView.xaml
	/// </summary>
	public partial class CustDbView : Window
	{
		public static CustCollection CustViewerDbcollection = CustCollection . CustViewerDbcollection;
		private bool IsDirty = false;
		static bool Startup = true;
		static bool Triggered = false;
		private string _bankno = "";
		private string _custno = "";
		private string _actype = "";
		private string _balance = "";
		private string _odate = "";
		private string _cdate = "";
		public CustDbView ( )
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
		private async void Window_Loaded ( object sender, RoutedEventArgs e )
		{
			Startup = true;
			// Data source is handled in XAML !!!!
			if ( this . CustGrid . Items . Count > 0 )
				this . CustGrid . Items . Clear ( );
			this . CustGrid . ItemsSource = CustViewerDbcollection;

			if ( CustViewerDbcollection . Count == 0 )
				CustViewerDbcollection = await CustCollection . LoadCust ( CustViewerDbcollection );
			this . CustGrid . ItemsSource = CustViewerDbcollection;
			this . MouseDown += delegate { DoDragMove ( ); };
			Utils . SetUpGridSelection ( this.CustGrid, 0 );
			DataFields . DataContext = this . CustGrid . SelectedItem;

			// Main update notification handler
			EventControl . DataUpdated += EventControl_DataUpdated;

			// An EditDb has changed the current index 
			EventControl . EditIndexChanged += EventControl_EditIndexChanged;
			// A Multiviewer has changed the current index 
			EventControl . MultiViewerIndexChanged += EventControl_EditIndexChanged;
			// Another SqlDbviewer has changed the current index 
			EventControl . ViewerIndexChanged += EventControl_EditIndexChanged;      // Callback in THIS FILE

			SaveBttn . IsEnabled = false;
			Startup = false;
			Count . Text = this . CustGrid . Items . Count . ToString ( );
			//this . CustGrid . SelectedIndex = 0;
			Flags . CustDbEditor = this;
			if ( Flags . LinkviewerRecords )
				LinkRecords . IsChecked = true;


		}

		private void EventControl_EditIndexChanged ( object sender, IndexChangedArgs e )
		{
			Triggered = true;
			// Handle selecton change if linkage is ON
			this . CustGrid . SelectedIndex = e . Row;
			this . CustGrid . Refresh (  );
			Triggered = false;
		}

		private async void EventControl_DataUpdated ( object sender, LoadedEventArgs e )
		{
			// Reciiving Notifiaction from a remote viewer that data has been changed, so we MUST now update our DataGrid
			Debug . WriteLine ( $"CustDbView : Data changed event notification received successfully." );
			int currsel = this . CustGrid . SelectedIndex;
			this . CustGrid . ItemsSource = null;
			this . CustGrid . Items . Clear ( );
			CustViewerDbcollection = await CustCollection . LoadCust( CustViewerDbcollection );
			this . CustGrid . SelectedIndex = currsel;
			this . CustGrid . ItemsSource = CustViewerDbcollection;
			this . CustGrid . Refresh ( );
		}

		public void ExternalDataUpdate ( int DbEditChangeType, int row, string currentDb )
		{
			// Reciiving Notifiaction from a remote viewer that data has been changed, so we MUST now update our DataGrid
			Debug . WriteLine ( $"CustDbView : Data changed event notification received successfully." );
			this . CustGrid . ItemsSource = null;
			this . CustGrid . Items . Clear ( );
			this . CustGrid . ItemsSource = CustViewerDbcollection;
			this . CustGrid . Refresh ( );
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
			currow = this . CustGrid . SelectedIndex;
			// Save current row so we can reposition correctly at end of the entire refresh process					
//			Flags . SqlCustCurrentIndex = currow;
			CustomerViewModel ss = new CustomerViewModel ( );
			ss = this . CustGrid . SelectedItem as CustomerViewModel;
			// This is the NEW DATA from the current row
			SQLHandlers sqlh = new SQLHandlers ( );
			sqlh . UpdateDbRowAsync ( "CUSTOMER", ss, this . CustGrid . SelectedIndex );

			this . CustGrid . SelectedIndex = currow;
			this . CustGrid . ScrollIntoView ( currow );
			// Notify EditDb to upgrade its grid
			if ( Flags . CurrentEditDbViewer != null )
				Flags . CurrentEditDbViewer . UpdateGrid ( "CUSTOMER" );

			// ***********  DEFINITE WIN  **********
			// This DOES trigger a notidfication to SQLDBVIEWER for sure !!!   14/5/21
			EventControl . TriggerBankDataLoaded ( CustViewerDbcollection,
				new LoadedEventArgs
				{
					CallerDb = "CUSTOMER",
					DataSource = CustViewerDbcollection,
					RowCount = this . CustGrid . SelectedIndex
				} );
			EventControl . TriggerDetDataLoaded ( CustViewerDbcollection,
				new LoadedEventArgs
				{
					CallerDb = "CUSTOMER",
					DataSource = CustViewerDbcollection,
					RowCount = this . CustGrid . SelectedIndex
				} );


		}

		private async void EventControl_CustDataLoaded ( object sender, LoadedEventArgs e )
		{
			// Event handler for CustDataLoaded
			this . CustGrid . ItemsSource = null;
			CustViewerDbcollection = await CustCollection . LoadCust ( CustViewerDbcollection );
			this . CustGrid . ItemsSource = CustViewerDbcollection;
			this . CustGrid . Refresh ( );
		}

		private void ShowCust_KeyDown ( object sender, System . Windows . Input . KeyEventArgs e )
		{

		}

		private void Close_Click ( object sender, RoutedEventArgs e )
		{
			Close ( );
		}

		private void Window_Closing ( object sender, System . ComponentModel . CancelEventArgs e )
		{
			Flags . CustDbEditor = null;
			EventControl . DataUpdated -= EventControl_DataUpdated;
		
		}

		private void CustGrid_SelectionChanged ( object sender, System . Windows . Controls . SelectionChangedEventArgs e )
		{
			if (Flags.LinkviewerRecords  == false && IsDirty )
			{
				MessageBoxResult result = System . Windows . MessageBox . Show
					( "You have unsaved changes.  Do you want them saved now ?", "Possible Data Loss", MessageBoxButton . YesNo, MessageBoxImage . Question, MessageBoxResult . Yes );
				if ( result == MessageBoxResult . Yes )
				{
					SaveButton ( );
				}
				// Do not want ot save it, so disable  save button again
				SaveBttn . IsEnabled = false;
				IsDirty = false;
			}
			IsDirty = false;
			if ( this . CustGrid . SelectedItem == null )
				return;
			this . CustGrid . ScrollIntoView ( this . CustGrid . SelectedItem );
			//Startup = true;
			DataFields . DataContext = this . CustGrid . SelectedItem;
			Startup = false;
			if ( Flags . LinkviewerRecords && Triggered == false )
			{
				EventControl . TriggerEditDbIndexChanged ( this,
				new IndexChangedArgs
				{
					dGrid = this . CustGrid,
					Row = this . CustGrid . SelectedIndex,
					SenderId = "CUSTOMER",
					Sender = "CUSTDBVIEW"
				} );
			}
			IsDirty = false;
			Triggered = false;
		}

		private async Task<bool> SaveButton ( object sender = null, RoutedEventArgs e = null )
		{
			//inprogress = true;
			//bindex = this . CustGrid . SelectedIndex;
			//cindex = this . CustomerGrid . SelectedIndex;
			//dindex = this . DetailsGrid . SelectedIndex;

			// Get the current rows data
			IsDirty = false;
			int CurrentSelection = this . CustGrid . SelectedIndex;
			this . CustGrid . SelectedItem = this . CustGrid . SelectedIndex;
			CustomerViewModel cvm = new CustomerViewModel ( );
			cvm = this . CustGrid . SelectedItem as CustomerViewModel;

			SaveFieldData ( );

			// update the current rows data content to send  to Update process
			cvm . BankNo = Bankno . Text;
			cvm . CustNo = Custno . Text;
			cvm . AcType = Convert . ToInt32 ( acType . Text );
//	cvm . Balance = Convert . ToDecimal ( balance . Text );
			cvm . ODate = Convert . ToDateTime ( odate . Text );
			cvm . CDate = Convert . ToDateTime ( cdate . Text );
			// Call Handler to update ALL Db's via SQL
			SQLHandlers sqlh = new SQLHandlers ( );
			await sqlh . UpdateDbRow ( "CUSTOMER", cvm );
			
			EventControl . TriggerDataUpdated ( CustViewerDbcollection,
				new LoadedEventArgs
				{
					CallerDb = "CUSTOMER",
					DataSource = CustViewerDbcollection,
					RowCount = this . CustGrid . SelectedIndex
				} );

			//Gotta reload our data because the update clears it down totally to null
			this . CustGrid . SelectedIndex = CurrentSelection;
			this . CustGrid . SelectedItem = CurrentSelection;
			this . CustGrid . Refresh ( );

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
//			_balance = balance . Text;
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
			// Filter data to show ONLY Custoimers with multiple Cust accounts

			if ( MultiAccounts . Content != "Show All" )
			{
				int currsel = this . CustGrid . SelectedIndex;
				CustomerViewModel bgr = this . CustGrid . SelectedItem as CustomerViewModel;
				Flags . IsMultiMode = true;

				CustViewerDbcollection = await CustCollection . LoadCust ( CustViewerDbcollection, 3);
				this . CustGrid . ItemsSource = null;
				this . CustGrid . ItemsSource = CustViewerDbcollection;
				this . CustGrid . Refresh ( );

				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				MultiAccounts . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBorderBrushRed" );
				MultiAccounts . Background = br;
				MultiAccounts . Content = "Show All";
				Count . Text = this . CustGrid . Items . Count . ToString ( );

				// Get Custno from ACTIVE gridso we can find it in other grids
				MultiViewer mv = new MultiViewer ( );
				int rec = mv.FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . CustGrid, "CUSTOMER" );
				this . CustGrid . SelectedIndex = currsel;
				if ( rec >= 0 )
					this . CustGrid . SelectedIndex = rec;
				else
					this . CustGrid . SelectedIndex = 0;
				Utils . ScrollRecordIntoView ( this . CustGrid, this . CustGrid . SelectedIndex );
			}
			else
			{
				Flags . IsMultiMode = false;
				CustomerViewModel bgr = this . CustGrid . SelectedItem as CustomerViewModel;
				CustViewerDbcollection = await CustCollection . LoadCust ( CustViewerDbcollection );

				// Just reset our iremssource to man Db
				this . CustGrid . ItemsSource = null;
				this . CustGrid . ItemsSource = CustViewerDbcollection;
				this . CustGrid . Refresh ( );

				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateYellow" );
				MultiAccounts . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushYellow" );
				MultiAccounts . Background = br;
				MultiAccounts . Content = "Multi Accounts";
				Count . Text = this . CustGrid . Items . Count . ToString ( );

				MultiViewer mv = new MultiViewer ( );
				int rec = mv . FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . CustGrid, "CUSTOMER" );
				this . CustGrid . SelectedIndex = 0;
				if ( rec >= 0 )
					this . CustGrid . SelectedIndex = rec;
				else
					this . CustGrid . SelectedIndex = 0;
				Utils . ScrollRecordIntoView ( this . CustGrid, this . CustGrid . SelectedIndex );
			}
		}
		public void SendDataChanged ( SqlDbViewer o, System . Windows . Controls . DataGrid Grid, string dbName )
		{
			// Databases have DEFINITELY been updated successfully after a change
			// We Now Broadcast this to ALL OTHER OPEN VIEWERS here and now

			//dca . SenderName = o . ToString ( );
			//dca . DbName = dbName;

			EventControl . TriggerDataUpdated ( CustViewerDbcollection,
			new LoadedEventArgs
			{
				CallerDb = "DETAILS",
				DataSource = CustViewerDbcollection,
				RowCount = this . CustGrid . SelectedIndex
			} );
			Mouse . OverrideCursor = System . Windows . Input . Cursors . Arrow;
		}
		private void LinkRecords_Click ( object sender, RoutedEventArgs e )
		{
			// force viewers to change records in line with each other
			if ( LinkRecords . IsChecked == true )
				Flags . LinkviewerRecords = true;
			else
				Flags . LinkviewerRecords = false;
			if ( Flags . SqlBankViewer != null )
				Flags . SqlBankViewer . LinkRecords . IsChecked = Flags . LinkviewerRecords;
			if ( Flags . SqlCustViewer != null )
				Flags . SqlCustViewer . LinkRecords . IsChecked = Flags . LinkviewerRecords;
			if ( Flags . SqlDetViewer != null )
				Flags . SqlDetViewer . LinkRecords . IsChecked = Flags . LinkviewerRecords;
			if ( Flags . SqlMultiViewer != null )
				Flags . SqlMultiViewer . LinkRecords . IsChecked = Flags . LinkviewerRecords;
			if ( Flags . BankDbEditor != null )
				Flags . BankDbEditor . LinkRecords . IsChecked = Flags . LinkviewerRecords;
			if ( Flags . DetDbEditor != null )
				Flags . DetDbEditor . LinkRecords . IsChecked = Flags . LinkviewerRecords;
			LinkRecords . Refresh ( );
		}

	}
}

/*			BankAccountViewModel bank = new BankAccountViewModel();
//			var filtered = from bank inCustViewerDbcollection . Where ( x => bank . CustNo = "1055033" ) select x;
//		   GroupBy bank.CustNo having count(*) > 1
//where  
//having COUNT (*) > 1
//	select bank;
//	Where ( b.CustNo = "1055033") ;


			commandline = $"SELECT * FROM BANKACCOUNT WHERE CUSTNO IN "
	+ $"(SELECT CUSTNO FROM BANKACCOUNT "
	+ $" GROUP BY CUSTNO"
	+ $" HAVING COUNT(*) > 1) ORDER BY ";

    */


