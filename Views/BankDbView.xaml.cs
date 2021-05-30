using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Linq;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Input;
using System . Windows . Media;
using WPFPages . ViewModels;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for BankDbView.xaml
	/// </summary>
	public partial class BankDbView : Window
	{
		public BankCollection BankViewercollection = BankCollection . EditDbBankcollection;
		private bool IsDirty = false;
		static bool Startup = true;
		private bool LinktoParent = false;
		private bool Triggered = false;
		private string _bankno = "";
		private string _custno = "";
		private string _actype = "";
		private string _balance = "";
		private string _odate = "";
		private string _cdate = "";
		private SqlDbViewer ParentViewer;

		public BankDbView ( )
		{
			Startup = true;
			InitializeComponent ( );
			this . Show ( );
			this . Refresh ( );
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
			this . Show ( );
			this . Refresh ( );
			Startup = true;
			// Data source is handled in XAML !!!!
			if ( this . BankGrid . Items . Count > 0 )
				this . BankGrid . Items . Clear ( );
			this . BankGrid . ItemsSource = BankViewercollection;

			if ( BankViewercollection . Count == 0 )
				BankViewercollection = await BankCollection . LoadBank ( BankViewercollection, 4, true );
			else
			{
				this . BankGrid . ItemsSource = BankViewercollection;
				this . BankGrid . SelectedIndex = 0;
				this . BankGrid . SelectedItem = 0;
				DataFields . DataContext = this . BankGrid . SelectedItem;
				Utils . SetUpGridSelection ( this . BankGrid, 0 );
				Count . Text = this . BankGrid . Items . Count . ToString ( );
			}

			this . MouseDown += delegate { DoDragMove ( ); };
			// An EditDb has changed the current index 
			EventControl . EditIndexChanged += EventControl_EditIndexChanged;
			// A Multiviewer has changed the current index 
			EventControl . MultiViewerIndexChanged += EventControl_EditIndexChanged;
			// Another viewer has changed the current index 
			EventControl . ViewerIndexChanged += EventControl_EditIndexChanged;      // Callback in THIS FILE
												 // Main Database updated notification handler
												 //			EventControl . DataUpdated += EventControl_DataUpdated;
			EventControl . ViewerDataUpdated += EventControl_DataUpdated;

			SaveBttn . IsEnabled = false;
			// Save linkage setting as we need to disable it while we are loading
			bool tmp = Flags . LinkviewerRecords;
			if ( Flags . LinkviewerRecords )
				LinkRecords . IsChecked = true;
			Flags . BankDbEditor = this;
			// Set window to TOPMOST
			OntopChkbox . IsChecked = true;
			this . Topmost = true;
			this . Focus ( );
			this . BankGrid . Focus ( );
			// Reset linkage setting
			Flags . LinkviewerRecords = tmp;
			if ( sender . GetType ( ) == typeof ( SqlDbViewer ) )
			{
				ParentViewer = sender as SqlDbViewer;
			}
			else
			{
				if ( Flags . SqlBankViewer != null )
					ParentViewer = Flags . SqlBankViewer;
				else
				{
					LinktoParent = false;
					LinkToParent . IsEnabled = false;
				}
			}
			Startup = false;
		}

		private void EventControl_EditIndexChanged ( object sender, IndexChangedArgs e )
		{
			Triggered = true;
			// Handle Selection change in another windowif linkage is ON
			this . BankGrid . SelectedIndex = e . Row;
			this . BankGrid . Refresh ( );
			Triggered = false;
		}


		private async void EventControl_DataUpdated ( object sender, LoadedEventArgs e )
		{
			int currsel = this . BankGrid . SelectedIndex;
			Debug . WriteLine ( $"BankDbView : Data changed event notification received successfully." );
			this . BankGrid . ItemsSource = null;
			this . BankGrid . Items . Clear ( );
			BankViewercollection = await BankCollection . LoadBank ( BankViewercollection, 4, true );
			this . BankGrid . ItemsSource = BankViewercollection;
			this . BankGrid . Refresh ( );
			this . BankGrid . SelectedIndex = currsel;
		}

		public void ExternalDataUpdate ( int DbEditChangeType, int row, string currentDb )
		{
			// Reciiving Notifiaction from a remote viewer that data has been changed, so we MUST now update our DataGrid
			Debug . WriteLine ( $"BankDbView : Data changed event notification received successfully." );
			this . BankGrid . ItemsSource = null;
			this . BankGrid . Items . Clear ( );
			this . BankGrid . ItemsSource = BankViewercollection;
			this . BankGrid . Refresh ( );
		}
		#endregion Startup/ Closedown


		private async void ViewerGrid_RowEditEnding ( object sender, System . Windows . Controls . DataGridRowEditEndingEventArgs e )
		{
			// Save changes and tell other viewers about the change
			int currow = 0;
			currow = this . BankGrid . SelectedIndex;
			// Save current row so we can reposition correctly at end of the entire refresh process					
			//			Flags . SqlBankCurrentIndex = currow;
			BankAccountViewModel ss = new BankAccountViewModel ( );
			ss = this . BankGrid . SelectedItem as BankAccountViewModel;
			// This is the NEW DATA from the current row
			SQLHandlers sqlh = new SQLHandlers ( );
			await sqlh . UpdateDbRowAsync ( "BANKACCOUNT", ss, this . BankGrid . SelectedIndex );

			this . BankGrid . SelectedIndex = currow;
			this . BankGrid . SelectedItem = currow;
			Utils . SetUpGridSelection ( this . BankGrid, currow );
			//			this . BankGrid . ScrollIntoView ( currow );
			// Notify EditDb to upgrade its grid
			if ( Flags . CurrentEditDbViewer != null )
				Flags . CurrentEditDbViewer . UpdateGrid ( "BANKACCOUNT" );

			// ***********  DEFINITE WIN  **********
			// This DOES trigger a notification to SQLDBVIEWER AND OTHERS for sure !!!   14/5/21
			//			Debug . WriteLine ( $" 4-2 *** TRACE *** BANKDBVIEW : ViewerGrid_RowEndingEdit BANKACCOUNT - Sending TriggerBankDataLoaded Event trigger" );
			SendDataChanged ( null, this . BankGrid, "BANKACCOUNT" );

			//EventControl . TriggerBankDataLoaded ( BankViewercollection,
			//	new LoadedEventArgs
			//	{
			//		CallerDb = "BANKACCOUNT",
			//		DataSource = BankViewercollection,
			//		RowCount = this . BankGrid . SelectedIndex
			//	} );
		}

		private async void EventControl_BankDataLoaded ( object sender, LoadedEventArgs e )
		{
			// Event handler for BankDataLoaded
			this . BankGrid . ItemsSource = null;
			BankViewercollection = await BankCollection . LoadBank ( BankViewercollection, 2, false );
			this . BankGrid . ItemsSource = BankViewercollection;
			this . BankGrid . SelectedIndex = 0;
			this . BankGrid . SelectedItem = 0;
			this . BankGrid . Refresh ( );
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
			if ( ( Flags . LinkviewerRecords == false && IsDirty )
					|| SaveBttn . IsEnabled )
			{
				MessageBoxResult result = MessageBox . Show
					( "You have unsaved changes.  Do you want them saved now ?", "P:ossible Data Loss", MessageBoxButton . YesNo, MessageBoxImage . Question, MessageBoxResult . Yes );
				if ( result == MessageBoxResult . Yes )
				{
					SaveButton ( );
				}
				// Do not want ot save it, so disable  save button again
				SaveBttn . IsEnabled = false;
				IsDirty = false;
			}
			Flags . BankDbEditor = null;
			EventControl . EditIndexChanged -= EventControl_EditIndexChanged;
			// A Multiviewer has changed the current index 
			EventControl . MultiViewerIndexChanged -= EventControl_EditIndexChanged;
			// Another SqlDbviewer has changed the current index 
			EventControl . ViewerIndexChanged -= EventControl_EditIndexChanged;      // Callback in THIS FILE
												 // Main update notification handler
												 //			EventControl . DataUpdated -= EventControl_DataUpdated;
			EventControl . ViewerDataUpdated -= EventControl_DataUpdated;
			DataFields . DataContext = this . BankGrid . SelectedItem;

		}

		private void BankGrid_SelectionChanged ( object sender, System . Windows . Controls . SelectionChangedEventArgs e )
		{
			if ( ( Flags . LinkviewerRecords == false && IsDirty )
					|| SaveBttn . IsEnabled )
			{
				MessageBoxResult result = MessageBox . Show
					( "You have unsaved changes.  Do you want them saved now ?", "P:ossible Data Loss", MessageBoxButton . YesNo, MessageBoxImage . Question, MessageBoxResult . Yes );
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
			Utils . SetUpGridSelection ( this . BankGrid, this . BankGrid . SelectedIndex );
			//			this . BankGrid . ScrollIntoView ( this . BankGrid . SelectedItem );
			Startup = true;
			DataFields . DataContext = this . BankGrid . SelectedItem;
			if ( Flags . LinkviewerRecords && Triggered == false )
			{
				//				Debug . WriteLine ( $" 4-1 *** TRACE *** BANKDBVIEW : BankGrid_SelectionChanged  BANKACCOUNT - Sending TriggerEditDbIndexChanged Event trigger" );
				TriggerViewerIndexChanged ( this . BankGrid );
				//EventControl . TriggerEditDbIndexChanged ( this,
				//new IndexChangedArgs
				//{
				//	dGrid = this .
				//	BankGrid,
				//	Row = this .
				//	BankGrid . SelectedIndex,
				//	SenderId = "BANKACCOUNT",
				//	Sender = "BANKDBVIEW"
				//} );
			}
			// Only  do this if global link is OFF
			if ( LinktoParent && LinkRecords . IsChecked == false )
			{
				// update parents row selection
				string bankno = "";
				string custno = "";
				var dvm = this . BankGrid . SelectedItem as BankAccountViewModel;
				int rec = Utils . FindMatchingRecord ( dvm . CustNo, dvm . BankNo, ParentViewer . BankGrid, "BANKACCOUNT" );
				ParentViewer . BankGrid . SelectedIndex = rec;
				Utils . SetUpGridSelection ( ParentViewer . BankGrid, rec );
			}

			Triggered = false;
			IsDirty = false;
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
			int CurrentSelection = this . BankGrid . SelectedIndex;
			this . BankGrid . SelectedItem = this . BankGrid . SelectedIndex;
			BankAccountViewModel bvm = new BankAccountViewModel ( );
			bvm = this . BankGrid . SelectedItem as BankAccountViewModel;
			if ( bvm == null ) return false;

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
			await sqlh . UpdateDbRow ( "BANKACCOUNT", bvm );

			BankCollection bank = new BankCollection ( );
			BankViewercollection = await bank . ReLoadBankData ( );
			this . BankGrid . ItemsSource = null;
			this . BankGrid . ItemsSource = BankViewercollection;
			this . BankGrid . Refresh ( );

			//			Debug . WriteLine ( $" 4-3 *** TRACE *** BANKDBVIEW : SaveButton BANKACCOUNT - Sending TriggerBankDataLoaded Event trigger" );
			SendDataChanged ( null, this . BankGrid, "BANKACCOUNT" );

			//EventControl . TriggerBankDataLoaded ( BankViewercollection,
			//	new LoadedEventArgs
			//	{
			//		CallerDb = "BANKACCOUNT",
			//		DataSource = BankViewercollection,
			//		RowCount = this . BankGrid . SelectedIndex
			//	} );

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
				int currsel = this . BankGrid . SelectedIndex;
				BankAccountViewModel bgr = this . BankGrid . SelectedItem as BankAccountViewModel;
				Flags . IsMultiMode = true;

				BankCollection bank = new BankCollection ( );
				bank = await bank . ReLoadBankData ( );
				this . BankGrid . ItemsSource = null;
				this . BankGrid . ItemsSource = bank;
				this . BankGrid . Refresh ( );

				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				MultiAccounts . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBorderBrushRed" );
				MultiAccounts . Background = br;
				MultiAccounts . Content = "Show All";
				Count . Text = this . BankGrid . Items . Count . ToString ( );

				// Get Custno from ACTIVE gridso we can find it in other grids
				MultiViewer mv = new MultiViewer ( );
				int rec = Utils . FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . BankGrid, "BANKACCOUNT" );
				this . BankGrid . SelectedIndex = currsel;
				if ( rec >= 0 )
					this . BankGrid . SelectedIndex = rec;
				else
					this . BankGrid . SelectedIndex = 0;
				Utils . SetUpGridSelection ( this . BankGrid, this . BankGrid . SelectedIndex );
				//				Utils . ScrollRecordIntoView ( this . BankGrid, this . BankGrid . SelectedIndex );
			}
			else
			{
				Flags . IsMultiMode = false;
				int currsel = this . BankGrid . SelectedIndex;
				BankAccountViewModel bgr = this . BankGrid . SelectedItem as BankAccountViewModel;

				BankCollection bank = new BankCollection ( );
				bank = await bank . ReLoadBankData ( );
				this . BankGrid . ItemsSource = null;
				this . BankGrid . ItemsSource = BankViewercollection;
				this . BankGrid . Refresh ( );

				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateBlue" );
				MultiAccounts . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushBlue" );
				MultiAccounts . Background = br;
				MultiAccounts . Content = "Multi Accounts";
				Count . Text = this . BankGrid . Items . Count . ToString ( );

				// Get Custno from ACTIVE gridso we can find it in other grids
				MultiViewer mv = new MultiViewer ( );
				int rec = Utils . FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . BankGrid, "BANKACCOUNT" );
				this . BankGrid . SelectedIndex = 0;

				if ( rec >= 0 )
					this . BankGrid . SelectedIndex = rec;
				else
					this . BankGrid . SelectedIndex = 0;
				Utils . SetUpGridSelection ( this . BankGrid, this . BankGrid . SelectedIndex );
				//				Utils . ScrollRecordIntoView ( this . BankGrid, this . BankGrid . SelectedIndex );
			}
		}
		public void SendDataChanged ( SqlDbViewer o, DataGrid Grid, string dbName )
		{
			// Called internally to broadcast data change event notification
			// Databases have DEFINITELY been updated successfully after a change
			// We Now Broadcast this to ALL OTHER OPEN VIEWERS here and now

			//			Debug . WriteLine ( $" 4-4 *** TRACE *** BANKDBVIEW : SendDataChanged BANKDBVIEW - Sending TriggerBankDataLoaded Event trigger" );
			EventControl . TriggerBankDataLoaded ( BankViewercollection,
			new LoadedEventArgs
			{
				CallerDb = "BANKACCOUNT",
				DataSource = BankViewercollection,
				RowCount = this . BankGrid . SelectedIndex
			} );
			Mouse . OverrideCursor = Cursors . Arrow;
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
			if ( Flags . CustDbEditor != null )
				Flags . CustDbEditor . LinkRecords . IsChecked = Flags . LinkviewerRecords;
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
				if ( ParentViewer != null )
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
		public void TriggerViewerIndexChanged ( DataGrid grid )
		{
			string SearchCustNo = "";
			string SearchBankNo = "";
			BankAccountViewModel CurrentBankSelectedRecord = grid . SelectedItem as BankAccountViewModel;
			SearchCustNo = CurrentBankSelectedRecord . CustNo;
			SearchBankNo = CurrentBankSelectedRecord . BankNo;
			EventControl . TriggerViewerIndexChanged ( this,
				new IndexChangedArgs
				{
					Senderviewer = null,
					Bankno = SearchBankNo,
					Custno = SearchCustNo,
					dGrid = grid,
					Sender = "BANKACCOUNT",
					Row = grid . SelectedIndex
				} );
		}

		#region Menu items

		private void Linq1_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in BankViewercollection
					   where ( items . AcType == 1 )
					   orderby items . CustNo
					   select items;
			this . BankGrid . ItemsSource = bankaccounts;
		}
		private void Linq2_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in BankViewercollection
					   where ( items . AcType == 2 )
					   orderby items . CustNo
					   select items;
			this . BankGrid . ItemsSource = bankaccounts;
		}
		private void Linq3_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in BankViewercollection
					   where ( items . AcType == 3 )
					   orderby items . CustNo
					   select items;
			this . BankGrid . ItemsSource = bankaccounts;
		}
		private void Linq4_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in BankViewercollection
					   where ( items . AcType == 4 )
					   orderby items . CustNo
					   select items;
			this . BankGrid . ItemsSource = bankaccounts;
		}
		private void Linq5_Click ( object sender, RoutedEventArgs e )
		{
			//select All the items first;			
			var bankaccounts = from items in BankViewercollection orderby items . CustNo, items . AcType select items;
			//Next Group BankAccountViewModel collection on Custno
			var grouped = bankaccounts . GroupBy (
				b => b . CustNo );

			//Now filter content down to only those a/c's with multiple Bank A/c's
			var sel = from g in grouped
				  where g . Count ( ) > 1
				  select g;

			// Finally, iterate thru the list of grouped CustNo's matching to CustNo in the full Bankaccounts data
			// giving us ONLY the full records for any recordss that have > 1 Bank accounts
			List<BankAccountViewModel> output = new List<BankAccountViewModel> ( );
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
			this . BankGrid . ItemsSource = output;
		}
		private void Linq6_Click ( object sender, RoutedEventArgs e )
		{
			var accounts = from items in BankViewercollection orderby items . CustNo, items . AcType select items;
			this . BankGrid . ItemsSource = accounts;
		}

		private void Filter_Click ( object sender, RoutedEventArgs e )
		{
			// Show Filter system
			MessageBox . Show ( "Filter dialog will appear here !!" );
		}

		private void Exit_Click ( object sender, RoutedEventArgs e )
		{
			Close ( );
		}

		private void Options_Click ( object sender, RoutedEventArgs e )
		{

		}
		#endregion Menu items

		/// <summary>
		/// Link record selection to parent SQL viewer window only
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LinkToParent_Click ( object sender, RoutedEventArgs e )
		{
			LinktoParent = !LinktoParent;
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
