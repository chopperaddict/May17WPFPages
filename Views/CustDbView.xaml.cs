using System;
using System . Collections . Generic;
using System . ComponentModel;
using System . Diagnostics;
using System . Linq;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Input;
using System . Windows . Media;
using WPFPages . ViewModels;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for CustDbView.xaml
	/// </summary>
	public partial class CustDbView : Window
	{
		public static CustCollection CustDbViewcollection = null;// = new CustCollection ( );//. CustViewerDbcollection;
		// Get our personal Collection view of the Db
		public ICollectionView CustviewerView { get; set; }

		private bool IsDirty = false;
		private bool Startup = true;
		private bool Triggered = false;
		private bool LinktoParent = false;

		private string _bankno = "";
		private string _custno = "";
		private string _actype = "";
		private string _balance = "";
		private string _odate = "";
		private string _cdate = "";
		private SqlDbViewer SqlParentViewer;
		private MultiViewer MultiParentViewer;

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
			Mouse . OverrideCursor = Cursors . Wait;
			this . Show ( );
			this . Refresh ( );
			Startup = true;

			this . MouseDown += delegate { DoDragMove ( ); };
			// An EditDb has changed the current index
			EventControl . EditIndexChanged += EventControl_EditIndexChanged;
			// A Multiviewer has changed the current index
			EventControl . MultiViewerIndexChanged += EventControl_EditIndexChanged;
			// Another viewer has changed the current index
			EventControl . ViewerIndexChanged += EventControl_EditIndexChanged;      // Callback in THIS FILE
			EventControl . ViewerDataUpdated += EventControl_DataUpdated;
			EventControl . CustDataLoaded += EventControl_CustDataLoaded;

			await CustCollection . LoadCust ( CustDbViewcollection, "CUSTDBVIEW", 3, true );

			SaveBttn . IsEnabled = false;
			// Save linkage setting as we need to disable it while we are loading
			bool tmp = Flags . LinkviewerRecords;
			if ( Flags . LinkviewerRecords )
				LinkRecords . IsChecked = true;
			Flags . CustDbEditor = this;
			// Set window to TOPMOST
			OntopChkbox . IsChecked = true;
			this . Topmost = true;
			this . Focus ( );
			// Reset linkage setting
			Flags . LinkviewerRecords = tmp;
			LinktoParent = false;
			if ( sender . GetType ( ) == typeof ( SqlDbViewer ) )
			{
				MultiParentViewer = null;
				if ( sender . GetType ( ) == typeof ( SqlDbViewer ) )
				{
					SqlParentViewer = sender as SqlDbViewer;
				}
				else
				{
					if ( Flags . SqlCustViewer != null )
						SqlParentViewer = Flags . SqlCustViewer;
					else
					{
						LinktoParent = false;
						LinkToParent . IsEnabled = false;
					}
				}
			}
			else if ( sender . GetType ( ) == typeof ( MultiViewer ) )
			{
				SqlParentViewer = null;
				if ( sender . GetType ( ) == typeof ( MultiViewer ) )
				{

					MultiParentViewer = sender as MultiViewer;
					//					LinktoParent = true;
					LinkToParent . IsEnabled = true;
				}
				else
				{
					if ( Flags . MultiViewer != null )
					{
						MultiParentViewer = Flags . MultiViewer;
						//						LinktoParent = true;
						LinkToParent . IsEnabled = true;
					}
					else
					{
						//						LinktoParent = false;
						LinkToParent . IsEnabled = false;
					}
				}
			}
			else
			{
				MultiParentViewer = null;
				SqlParentViewer = null;
				LinktoParent = false;
				LinkToParent . IsEnabled = false;

				if ( Flags . SqlCustViewer != null )
				{
					SqlParentViewer = Flags . SqlCustViewer;
					//					LinktoParent = true;
					LinkToParent . IsEnabled = true;
					LinkToParent . Content = "Link to \nSqlViewer";
				}
				else if ( Flags . MultiViewer != null )
				{
					MultiParentViewer = Flags . MultiViewer;
					//					LinktoParent = true;
					LinkToParent . IsEnabled = true;
					LinkToParent . Content = "Link to \nMultiViewer";
				}
				else
				{
					//					LinktoParent = false;
					LinkToParent . IsEnabled = false;
				}
			}
			this . CustGrid . SelectedIndex = 0;
			Startup = false;
		}

		private void EventControl_EditIndexChanged ( object sender, IndexChangedArgs e )
		{
			Triggered = true;
			// Handle selecton change if linkage is ON
			this . CustGrid . SelectedIndex = e . Row;
			this . CustGrid . Refresh ( );
			Triggered = false;
		}

		private async void EventControl_DataUpdated ( object sender, LoadedEventArgs e )
		{
			Debug . WriteLine ( $"CustDbView : Data changed event notification received successfully." );
			if ( e . CallerType == "CUSTDBVIEW" ) return;
			int currsel = this . CustGrid . SelectedIndex;
			this . CustGrid . ItemsSource = null;
			this . CustGrid . Items . Clear ( );
			Mouse . OverrideCursor = Cursors . Wait;
			CustDbViewcollection = await CustCollection . LoadCust ( CustDbViewcollection, "CUSTDBVIEW", 3, true );
			this . CustGrid . ItemsSource = CustDbViewcollection;
			this . CustGrid . SelectedIndex = currsel;
			this . CustGrid . SelectedItem = currsel; this . CustGrid . Refresh ( );
		}

		public void ExternalDataUpdate ( int DbEditChangeType, int row, string currentDb )
		{
			// Reciiving Notifiaction from a remote viewer that data has been changed, so we MUST now update our DataGrid
			Debug . WriteLine ( $"CustDbView : Data changed event notification received successfully." );
			this . CustGrid . ItemsSource = null;
			this . CustGrid . Items . Clear ( );
			this . CustGrid . ItemsSource = CustDbViewcollection;
			this . CustGrid . Refresh ( );
		}
		#endregion Startup/ Closedown

		private async void ViewerGrid_RowEditEnding ( object sender, System . Windows . Controls . DataGridRowEditEndingEventArgs e )
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
			await sqlh . UpdateDbRowAsync ( "CUSTOMER", ss, this . CustGrid . SelectedIndex );

			this . CustGrid . SelectedIndex = currow;
			this . CustGrid . SelectedItem = currow;
			Utils . SetUpGridSelection ( this . CustGrid, currow );
			// Notify EditDb to upgrade its grid
			if ( Flags . CurrentEditDbViewer != null )
				Flags . CurrentEditDbViewer . UpdateGrid ( "CUSTOMER" );

			// ***********  DEFINITE WIN  **********
			// This DOES trigger a notidfication to SQLDBVIEWER for sure !!!   14/5/21
			EventControl . TriggerViewerDataUpdated ( CustDbViewcollection,
				new LoadedEventArgs
				{
					CallerType = "CUSTDBVIEW",
					CallerDb = "CUSTOMER",
					DataSource = CustDbViewcollection,
					RowCount = this . CustGrid . SelectedIndex
				} );
		}

		private async void EventControl_CustDataLoaded ( object sender, LoadedEventArgs e )
		{
			// Event handler for CustDataLoaded
			// ONLY proceeed if we triggered the new data request
			if ( e . CallerDb != "CUSTDBVIEW" ) return;
			this . CustGrid . ItemsSource = null;
			this . CustGrid . Items . Clear ( );
			if ( e . DataSource == null ) return;

			CustviewerView = CollectionViewSource . GetDefaultView ( e . DataSource as CustCollection );
			CustDbViewcollection = e . DataSource as CustCollection;
			CustviewerView . Refresh ( );
			this . CustGrid . ItemsSource = CustviewerView;

			this . CustGrid . SelectedIndex = 0;
			this . CustGrid . SelectedItem = 0;
			Mouse . OverrideCursor = Cursors . Arrow;
			this . CustGrid . Refresh ( );
			Debug . WriteLine ( "BANKDBVIEW : Customer Data fully loaded" );
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
			EventControl . EditIndexChanged -= EventControl_EditIndexChanged;
			// A Multiviewer has changed the current index
			EventControl . MultiViewerIndexChanged -= EventControl_EditIndexChanged;
			// Another SqlDbviewer has changed the current index
			EventControl . ViewerIndexChanged -= EventControl_EditIndexChanged;      // Callback in THIS FILE
			EventControl . ViewerDataUpdated -= EventControl_DataUpdated;
			EventControl . CustDataLoaded -= EventControl_CustDataLoaded;


		}

		private void CustGrid_SelectionChanged ( object sender, System . Windows . Controls . SelectionChangedEventArgs e )
		{
			//if ( Flags . LinkviewerRecords == false && IsDirty )
			//{
			//	MessageBoxResult result = System . Windows . MessageBox . Show
			//		( "You have unsaved changes.  Do you want them saved now ?", "Possible Data Loss", MessageBoxButton . YesNo, MessageBoxImage . Question, MessageBoxResult . Yes );
			//	if ( result == MessageBoxResult . Yes )
			//	{
			//		SaveButton ( );
			//	}
			//	// Do not want to save it, so disable  save button again
			//	SaveBttn . IsEnabled = false;
			//	IsDirty = false;
			//}
			IsDirty = false;
			if ( this . CustGrid . SelectedItem == null )
				return;
			Utils . SetUpGridSelection ( this . CustGrid, this . CustGrid . SelectedIndex );
			Startup = true;
			DataFields . DataContext = this . CustGrid . SelectedItem;
			if ( Flags . LinkviewerRecords && Triggered == false )
			{
				//				Debug . WriteLine ( $" 7-1 *** TRACE *** CUSTDBVIEWER : Itemsview_OnSelectionChanged  CUSTOMER - Sending TriggerEditDbIndexChanged Event trigger" );
				TriggerViewerIndexChanged ( this . CustGrid );
			}

			// check to see if an SqlDbViewer has been opened that we can link to
			if ( Flags . SqlCustViewer != null && LinkToParent . IsEnabled == false )
			{
				LinkToParent . IsEnabled = true;
				SqlParentViewer = Flags . SqlCustViewer;
			}
			else if ( Flags . SqlCustViewer == null )
			{
				if ( LinkToParent . IsEnabled )
				{
					LinkToParent . IsEnabled = false;
					LinkToParent . IsChecked = false;
					LinktoParent = false;
					SqlParentViewer = null;
				}
			}

			// Only  do this if global link is OFF
			if ( LinktoParent )
			{
				// update parents row selection
				string bankno = "";
				string custno = "";
				var dvm = this . CustGrid . SelectedItem as CustomerViewModel;
				if ( dvm == null ) return;

				if ( SqlParentViewer != null )
				{
					int rec = Utils . FindMatchingRecord ( dvm . CustNo, dvm . BankNo, SqlParentViewer . CustomerGrid, "CUSTOMER" );
					SqlParentViewer . CustomerGrid . SelectedIndex = rec;
					Utils . SetUpGridSelection ( SqlParentViewer . CustomerGrid, rec );
				}
				else if ( MultiParentViewer != null )
				{
					int rec = Utils . FindMatchingRecord ( dvm . CustNo, dvm . BankNo, MultiParentViewer . CustomerGrid, "CUSTOMER" );
					MultiParentViewer . CustomerGrid . SelectedIndex = rec;
					Utils . SetUpGridSelection ( MultiParentViewer . CustomerGrid, rec );
				}
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

			EventControl . TriggerViewerDataUpdated ( CustDbViewcollection,
				new LoadedEventArgs
				{
					CallerType = "CUSTDBVIEW",
					CallerDb = "CUSTOMER",
					DataSource = CustDbViewcollection,
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
			SaveBttn . IsEnabled = false; ;
			if ( _bankno != Bankno . Text )
				SaveBttn . IsEnabled = true;
			if ( _custno != Custno . Text )
				SaveBttn . IsEnabled = true;
			if ( _actype != acType . Text )
				SaveBttn . IsEnabled = true;
			//if ( _balance != balance . Text )
			//	SaveBttn . IsEnabled = true;
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
				if ( bgr == null ) return;

				Flags . IsMultiMode = true;

				CustDbViewcollection = await CustCollection . LoadCust ( CustDbViewcollection, "CUSTDBVIEW", 3, true );
				this . CustGrid . ItemsSource = null;
				this . CustGrid . ItemsSource = CustDbViewcollection;
				this . CustGrid . Refresh ( );

				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				MultiAccounts . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBorderBrushRed" );
				MultiAccounts . Background = br;
				MultiAccounts . Content = "Show All";
				Count . Text = this . CustGrid . Items . Count . ToString ( );

				// Get Custno from ACTIVE gridso we can find it in other grids
				MultiViewer mv = new MultiViewer ( );
				int rec = Utils . FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . CustGrid, "CUSTOMER" );
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
				CustDbViewcollection = await CustCollection . LoadCust ( CustDbViewcollection, "CUSTDBVIEW", 3, true );

				// Just reset our iremssource to man Db
				this . CustGrid . ItemsSource = null;
				this . CustGrid . ItemsSource = CustDbViewcollection;
				this . CustGrid . Refresh ( );

				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateYellow" );
				MultiAccounts . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushYellow" );
				MultiAccounts . Background = br;
				MultiAccounts . Content = "Multi Accounts";
				Count . Text = this . CustGrid . Items . Count . ToString ( );

				MultiViewer mv = new MultiViewer ( );
				int rec = Utils . FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . CustGrid, "CUSTOMER" );
				this . CustGrid . SelectedIndex = 0;
				if ( rec >= 0 )
					this . CustGrid . SelectedIndex = rec;
				else
					this . CustGrid . SelectedIndex = 0;
				Utils . ScrollRecordIntoView ( this . CustGrid, this . CustGrid . SelectedIndex );
			}
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
			if ( Flags . LinkviewerRecords == true )
			{
				LinktoParent = false;
				LinkToParent . IsEnabled = false;
				LinkToParent . IsChecked = false;
			}
			else
			{
				if ( SqlParentViewer != null )
					LinkToParent . IsEnabled = true;
				else
					LinkToParent . IsEnabled = false;
			}
		}

		/// <summary>
		/// Generic method to send Index changed Event trigger so that
		/// other viewers can update thier own grids as relevant
		/// </summary>
		/// <param name="grid"></param>
		//*************************************************************************************************************//
		public void TriggerViewerIndexChanged ( System . Windows . Controls . DataGrid grid )
		{
			string SearchCustNo = "";
			string SearchBankNo = "";
			CustomerViewModel CurrentCustSelectedRecord = this . CustGrid . SelectedItem as CustomerViewModel;
			if ( CurrentCustSelectedRecord == null ) return;
			SearchCustNo = CurrentCustSelectedRecord . CustNo;
			SearchBankNo = CurrentCustSelectedRecord . BankNo;
			EventControl . TriggerViewerIndexChanged ( this,
			new IndexChangedArgs
			{
				Senderviewer = this,
				Bankno = SearchBankNo,
				Custno = SearchCustNo,
				dGrid = this . CustGrid,
				Sender = "CUSTOMER",
				SenderId = "CUSTDBVIEW",
				Row = this . CustGrid . SelectedIndex
			} );
		}

		#region Menu items

		private void Linq1_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in CustDbViewcollection
					   where ( items . AcType == 1 )
					   orderby items . CustNo
					   select items;
			this . CustGrid . ItemsSource = bankaccounts;
		}
		private void Linq2_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in CustDbViewcollection
					   where ( items . AcType == 2 )
					   orderby items . CustNo
					   select items;
			this . CustGrid . ItemsSource = bankaccounts;
		}
		private void Linq3_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in CustDbViewcollection
					   where ( items . AcType == 3 )
					   orderby items . CustNo
					   select items;
			this . CustGrid . ItemsSource = bankaccounts;
		}
		private void Linq4_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in CustDbViewcollection
					   where ( items . AcType == 4 )
					   orderby items . CustNo
					   select items;
			this . CustGrid . ItemsSource = bankaccounts;
		}
		private void Linq5_Click ( object sender, RoutedEventArgs e )
		{
			//select All the items first;
			var bankaccounts = from items in CustDbViewcollection orderby items . CustNo, items . AcType select items;
			//Next Group BankAccountViewModel collection on Custno
			var grouped = bankaccounts . GroupBy (
				b => b . CustNo );

			//Now filter content down to only those a/c's with multiple Bank A/c's
			var sel = from g in grouped
				  where g . Count ( ) > 1
				  select g;

			// Finally, iterate thru the list of grouped CustNo's matching to CustNo in the full Bankaccounts data
			// giving us ONLY the full records for any recordss that have > 1 Bank accounts
			List<CustomerViewModel> output = new List<CustomerViewModel> ( );
			foreach ( var item1 in sel )
			{
				foreach ( var item2 in bankaccounts )
				{
					if ( item2 . CustNo . ToString ( ) == item1 . Key )
					{
						output . Add ( item2 );
					}
				}
			}
			this . CustGrid . ItemsSource = output;
		}
		private void Linq6_Click ( object sender, RoutedEventArgs e )
		{
			var accounts = from items in CustDbViewcollection orderby items . CustNo, items . AcType select items;
			this . CustGrid . ItemsSource = accounts;
		}

		private void Filter_Click ( object sender, RoutedEventArgs e )
		{
			// Show Filter system
			System . Windows . MessageBox . Show ( "Filter dialog will appear here !!" );
		}

		#endregion Menu items

		private void Exit_Click ( object sender, RoutedEventArgs e )
		{
			Close ( );
		}

		private void Options_Click ( object sender, RoutedEventArgs e )
		{

		}
		private void Minimize_click ( object sender, RoutedEventArgs e )
		{
			this . WindowState = WindowState . Normal;
		}

		private void Window_MouseDown ( object sender, MouseButtonEventArgs e )
		{

		}

		/// <summary>
		/// Link record selection to parent SQL viewer window only
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LinkToParent_Click ( object sender, RoutedEventArgs e )
		{
			LinktoParent = !LinktoParent;
		}

		private void CustGrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			//// Data has been changed in one of our rows.
			//CustomerViewModel cvm = sender as CustomerViewModel;
			//cvm = e . Row . Item as CustomerViewModel;
			//SQLHandlers sqlh = new SQLHandlers ( );
			//sqlh . UpdateDbRowAsync ( "CUSTOMER", cvm );
			//EventControl . TriggerViewerDataUpdated ( CustDbViewcollection,
			//	new LoadedEventArgs
			//	{
			//		CallerType = "CUSTDBVIEW",
			//		CallerDb = "CUSTOMER",
			//		DataSource = CustDbViewcollection,
			//		RowCount = this . CustGrid . SelectedIndex
			//	} );
			//Mouse . OverrideCursor = System . Windows . Input . Cursors . Arrow;
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


