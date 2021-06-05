using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . ComponentModel;
using System . Diagnostics;
using System . Linq;
using System . Threading;
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
	/// Interaction logic for BankDbView.xaml
	/// </summary>
	public partial class BankDbView : Window
	{
		public BankCollection BankViewcollection = null;// = new BankCollection ( );//. EditDbBankcollection;
								// Get our personal Collection view of the Db
		public ICollectionView BankviewerView { get; set; }

		private bool IsDirty = false;
		static bool Startup = true;
		private bool LinktoParent = false;
		private bool Triggered = false;
		private bool LoadingDbData = false;
		private bool RowHasBeenEdited { get; set; }
		private bool keyshifted { get; set; }
		private bool IsEditing { get; set; }
		private string _bankno = "";
		private string _custno = "";
		private string _actype = "";
		private string _balance = "";
		private string _odate = "";
		private string _cdate = "";
		private SqlDbViewer SqlParentViewer;
		private MultiViewer MultiParentViewer;


		BankAccountViewModel bvmCurrent { get; set; }
		CustomerViewModel cvmCurrent { get; set; }
		DetailsViewModel dvmCurrent { get; set; }

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
			EventControl . BankDataLoaded += EventControl_BankDataLoaded;

			await BankCollection . LoadBank ( BankViewcollection, "BANKDBVIEW", 3, true );

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
				//				LinktoParent = false;
				LinkToParent . IsEnabled = false;

				if ( Flags . SqlBankViewer != null )
				{
					SqlParentViewer = Flags . SqlBankViewer;
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
			this . BankGrid . SelectedIndex = 0;
			Startup = false;
		}

		private void EventControl_EditIndexChanged ( object sender, IndexChangedArgs e )
		{
			Triggered = true;
			// Handle Selection change in another windowif linkage is ON
			if ( IsEditing )
			{
				//IsEditing = false;
				return;
			}
			this . BankGrid . SelectedIndex = e . Row;
			this . BankGrid . Refresh ( );
			Triggered = false;
		}

		private async void EventControl_DataUpdated ( object sender, LoadedEventArgs e )
		{
			if ( e . CallerDb == "BANKDBVIEW" || e . CallerDb == "BANKACCOUNT" ) return;
			int currsel = this . BankGrid . SelectedIndex;
			Debug . WriteLine ( $"BankDbView : Data changed event notification received successfully." );
			this . BankGrid . ItemsSource = null;
			this . BankGrid . Items . Clear ( );
			Mouse . OverrideCursor = Cursors . Wait;
			BankViewcollection = await BankCollection . LoadBank ( BankViewcollection, "BANKDBVIEW", 3, true );
			this . BankGrid . ItemsSource = BankViewcollection;
			this . BankGrid . Refresh ( );
			IsDirty = false;
		}

		public void ExternalDataUpdate ( int DbEditChangeType, int row, string currentDb )
		{
			// Reciiving Notifiaction from a remote viewer that data has been changed, so we MUST now update our DataGrid
			Debug . WriteLine ( $"BankDbView : Data changed event notification received successfully." );
			this . BankGrid . ItemsSource = null;
			this . BankGrid . Items . Clear ( );
			this . BankGrid . ItemsSource = BankViewcollection;
			this . BankGrid . Refresh ( );
		}
		#endregion Startup/ Closedown

		private async void ViewerGrid_RowEditEnding ( object sender, System . Windows . Controls . DataGridRowEditEndingEventArgs e )
		{
			DataGridEditAction dgea;
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
			IsDirty = false;
			// Notify EditDb to upgrade its grid
			if ( Flags . CurrentEditDbViewer != null )
				Flags . CurrentEditDbViewer . UpdateGrid ( "BANKACCOUNT" );

			// ***********  DEFINITE WIN  **********
			// This DOES trigger a notification to SQLDBVIEWER AND OTHERS for sure !!!   14/5/21
			EventControl . TriggerViewerDataUpdated ( BankViewcollection,
				new LoadedEventArgs
				{
					CallerType = "BANKDBVIEW",
					CallerDb = "BANKACCOUNT",
					DataSource = BankViewcollection,
					RowCount = this . BankGrid . SelectedIndex
				} );

		}

		private async void EventControl_BankDataLoaded ( object sender, LoadedEventArgs e )
		{
			// Event handler for BankDataLoaded
			if ( e . DataSource == null ) return;
			// ONLY proceeed if we triggered the new data request
			if ( e . CallerDb != "BANKDBVIEW" ) return;
			this . BankGrid . ItemsSource = null;
			this . BankGrid . Items . Clear ( );
			LoadingDbData = true;
			BankviewerView = CollectionViewSource . GetDefaultView ( e . DataSource as BankCollection );
			BankViewcollection = e . DataSource as BankCollection;
			BankviewerView . Refresh ( );
			this . BankGrid . ItemsSource = BankviewerView;
			this . BankGrid . SelectedIndex = 0;
			this . BankGrid . SelectedItem = 0;
			this . BankGrid . CurrentItem = 0;
			this . BankGrid . UpdateLayout ( );
			this . BankGrid . Refresh ( );
			Mouse . OverrideCursor = Cursors . Arrow;
			Thread . Sleep ( 250 );
			DataFields . Refresh ( );
			Count . Text = $"{this . BankGrid . SelectedIndex} / { this . BankGrid . Items . Count . ToString ( )}";
			IsDirty = false;
			Debug . WriteLine ( "BANKDBVIEW : Bank Data fully loaded" );
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
					( "You have unsaved changes.  Do you want them saved now ?", "Possible Data Loss", MessageBoxButton . YesNo, MessageBoxImage . Question, MessageBoxResult . Yes );
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
			EventControl . BankDataLoaded -= EventControl_BankDataLoaded;
			DataFields . DataContext = this . BankGrid . SelectedItem;
			BankViewcollection = null;

		}

		private void BankGrid_SelectionChanged ( object sender, System . Windows . Controls . SelectionChangedEventArgs e )
		{
			if ( LoadingDbData )
			{
				LoadingDbData = false;
				return;
			}
			Utils . SetUpGridSelection ( this . BankGrid, this . BankGrid . SelectedIndex );
			Startup = true;
			DataFields . DataContext = this . BankGrid . SelectedItem;
			if ( Flags . LinkviewerRecords && Triggered == false )
			{
				//				Debug . WriteLine ( $" 4-1 *** TRACE *** BANKDBVIEW : BankGrid_SelectionChanged  BANKACCOUNT - Sending TriggerEditDbIndexChanged Event trigger" );
				TriggerViewerIndexChanged ( this . BankGrid );
			}

			// check to see if an SqlDbViewer has been opened that we can link to
			if ( Flags . SqlBankViewer != null && LinkToParent . IsEnabled == false )
			{
				LinkToParent . IsEnabled = true;
				SqlParentViewer = Flags . SqlBankViewer;
			}
			//else if ( Flags . SqlBankViewer == null )
			//{
			//	if ( LinkToParent . IsEnabled )
			//	{
			//		LinkToParent . IsEnabled = false;
			//		LinkToParent . IsChecked = false;
			//		LinktoParent = false;
			//		SqlParentViewer = null;
			//	}
			//}

			// Only  do this if global link is OFF
			if ( LinktoParent )// && LinkRecords . IsChecked == false )
			{
				// update parents row selection
				string bankno = "";
				string custno = "";
				var dvm = this . BankGrid . SelectedItem as BankAccountViewModel;
				if ( dvm == null ) return;

				if ( SqlParentViewer != null )
				{
					int rec = Utils . FindMatchingRecord ( dvm . CustNo, dvm . BankNo, SqlParentViewer . BankGrid, "BANKACCOUNT" );
					SqlParentViewer . BankGrid . SelectedIndex = rec;
					Utils . SetUpGridSelection ( SqlParentViewer . BankGrid, rec );
				}
				else if ( MultiParentViewer != null )
				{
					int rec = Utils . FindMatchingRecord ( dvm . CustNo, dvm . BankNo, MultiParentViewer . BankGrid, "BANKACCOUNT" );
					MultiParentViewer . BankGrid . SelectedIndex = rec;
					Utils . SetUpGridSelection ( MultiParentViewer . BankGrid, rec );
				}
			}

			Count . Text = $"{this . BankGrid . SelectedIndex} / { this . BankGrid . Items . Count . ToString ( )}";

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
			BankViewcollection = await bank . ReLoadBankData ( );
			this . BankGrid . ItemsSource = null;
			this . BankGrid . ItemsSource = BankViewcollection;
			this . BankGrid . Refresh ( );

			//			Debug . WriteLine ( $" 4-3 *** TRACE *** BANKDBVIEW : SaveButton BANKACCOUNT - Sending TriggerBankDataLoaded Event trigger" );
			//			SendDataChanged ( null, this . BankGrid, "BANKACCOUNT" );

			EventControl . TriggerViewerDataUpdated ( BankViewcollection,
				new LoadedEventArgs
				{
					CallerType = "BANKDBVIEW",
					CallerDb = "BANKACCOUNT",
					DataSource = BankViewcollection,
					RowCount = this . BankGrid . SelectedIndex
				} );

			//Gotta reload our data because the update clears it down totally to null
			this . BankGrid . SelectedIndex = CurrentSelection;
			this . BankGrid . SelectedItem = CurrentSelection;
			this . BankGrid . Refresh ( );

			SaveBttn . IsEnabled = false;
			IsDirty = false;
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
		{
			return;
			if ( !Startup ) CompareFieldData ( );
		}

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
				if ( bgr == null ) return;

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
				Count . Text = $"{this . BankGrid . SelectedIndex} / { this . BankGrid . Items . Count . ToString ( )}";

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
				if ( bgr == null ) return;

				BankCollection bank = new BankCollection ( );
				bank = await bank . ReLoadBankData ( );
				this . BankGrid . ItemsSource = null;
				this . BankGrid . ItemsSource = BankViewcollection;
				this . BankGrid . Refresh ( );

				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateBlue" );
				MultiAccounts . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushBlue" );
				MultiAccounts . Background = br;
				MultiAccounts . Content = "Multi Accounts";
				Count . Text = $"{this . BankGrid . SelectedIndex} / { this . BankGrid . Items . Count . ToString ( )}";

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
		//public void SendDataChanged ( SqlDbViewer o, DataGrid Grid, string dbName )
		//{
		//	// Called internally to broadcast data change event notification
		//	// Databases have DEFINITELY been updated successfully after a change
		//	// We Now Broadcast this to ALL OTHER OPEN VIEWERS here and now

		//	EventControl . TriggerBankDataLoaded ( BankViewcollection,
		//	new LoadedEventArgs
		//	{
		//		CallerType = "BANKDBVIEW",
		//		CallerDb = "BANKACCOUNT",
		//		DataSource = BankViewcollection,
		//		RowCount = this . BankGrid . SelectedIndex
		//	} );
		//	Mouse . OverrideCursor = Cursors . Arrow;
		//}
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
		public void TriggerViewerIndexChanged ( DataGrid grid )
		{
			string SearchCustNo = "";
			string SearchBankNo = "";
			if ( grid . ItemsSource == null ) return;
			BankAccountViewModel CurrentBankSelectedRecord = grid . SelectedItem as BankAccountViewModel;
			SearchCustNo = CurrentBankSelectedRecord . CustNo;
			SearchBankNo = CurrentBankSelectedRecord . BankNo;
			EventControl . TriggerViewerIndexChanged ( this,
				new IndexChangedArgs
				{
					Senderviewer = this,
					Bankno = SearchBankNo,
					Custno = SearchCustNo,
					dGrid = grid,
					Sender = "BANKACCOUNT",
					SenderId = "BANKDBVIEW",
					Row = grid . SelectedIndex
				} );
		}

		#region Menu items

		private void Linq1_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in BankViewcollection
					   where ( items . AcType == 1 )
					   orderby items . CustNo
					   select items;
			this . BankGrid . ItemsSource = bankaccounts;
		}
		private void Linq2_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in BankViewcollection
					   where ( items . AcType == 2 )
					   orderby items . CustNo
					   select items;
			this . BankGrid . ItemsSource = bankaccounts;
		}
		private void Linq3_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in BankViewcollection
					   where ( items . AcType == 3 )
					   orderby items . CustNo
					   select items;
			this . BankGrid . ItemsSource = bankaccounts;
		}
		private void Linq4_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in BankViewcollection
					   where ( items . AcType == 4 )
					   orderby items . CustNo
					   select items;
			this . BankGrid . ItemsSource = bankaccounts;
		}
		private void Linq5_Click ( object sender, RoutedEventArgs e )
		{
			//select All the items first;			
			var bankaccounts = from items in BankViewcollection orderby items . CustNo, items . AcType select items;
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
			var accounts = from items in BankViewcollection orderby items . CustNo, items . AcType select items;
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

		private async void BankGrid_PreviewMouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{
			// handle flags to let us know WE have triggered the selectedIndex change
			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			if ( e . ChangedButton == MouseButton . Right )
			{
				DataGridRow RowData = new DataGridRow ( );
				int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
				if ( row == -1 ) row = 0;
				RowInfoPopup rip = new RowInfoPopup ( "BANKACCOUNT", BankGrid, RowData );
				rip . Topmost = true;
				rip . DataContext = RowData;
				rip . BringIntoView ( );
				rip . Focus ( );
				rip . ShowDialog ( );

				//If data has been changed, update everywhere
				// Update the row on return in case it has been changed
				if ( rip . IsDirty )
				{
					this . BankGrid . ItemsSource = null;
					this . BankGrid . Items . Clear ( );
					await BankCollection . LoadBank ( BankViewcollection, "BANKDBVIEW", 1, true );
					this . BankGrid . ItemsSource = BankviewerView;
					// Notify everyone else of the data change
					EventControl . TriggerViewerDataUpdated ( BankviewerView,
						new LoadedEventArgs
						{
							CallerType = "BANKBVIEW",
							CallerDb = "BANKACCOUNT",
							DataSource = BankviewerView,
							RowCount = this . BankGrid . SelectedIndex
						} );
				}
				else
					this . BankGrid . SelectedItem = RowData . Item;

				// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
				Utils . SetUpGridSelection ( this . BankGrid, row );
				Count . Text = $"{this . BankGrid . SelectedIndex} / { this . BankGrid . Items . Count . ToString ( )}";
				// This is essential to get selection activated again
				this . BankGrid . Focus ( );
			}

		}

		private void Edit_LostFocus ( object sender, RoutedEventArgs e )
		{
			IsDirty = true;
			SaveBttn . IsEnabled = true;
		}

		private void BankGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
		{
			IsEditing = true;
			// Save  the curret data for checking later on when we exit editing
			//			RowHasBeenEdited = true;
			bvmCurrent = e . Row . Item as BankAccountViewModel;

		}
		private async void BankGrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			//bool updated = false;
			//DataGridEditAction dgea;
			//dgea = e . EditAction;
			//// if NOT Commit (1) the exit here
			//if ( dgea == 0 ) return;
			//	// Data has been changed in one of our rows.
			//BankAccountViewModel bvm = this.BankGrid.SelectedItem as BankAccountViewModel;
			//bvm = e . Row . Item as BankAccountViewModel;

			//if ( bvm . BankNo != bvmCurrent . BankNo )
			//	updated = true;
			//else if ( bvm . CustNo != bvmCurrent . CustNo )
			//	updated = true;
			//else if ( bvm . IntRate != bvmCurrent . IntRate )
			//	updated = true;
			//else if ( bvm . Balance!= bvmCurrent . Balance )
			//	updated = true;
			//else if ( bvm . ODate!= bvmCurrent . ODate)
			//	updated = true;
			//else if ( bvm . CDate!= bvmCurrent . CDate)
			//	updated = true;

			//// If no change,don't bother
			//if ( updated  == false) return;
			////RowHasBeenEdited = true;
			////SQLHandlers sqlh = new SQLHandlers ( );
			////await sqlh . UpdateDbRowAsync ( "BANKACCOUNT", bvm );

			////// Notify other viewers
			////EventControl . TriggerViewerDataUpdated ( BankViewcollection,
			////	new LoadedEventArgs
			////	{
			////		CallerType = "BANKDBVIEW",
			////		CallerDb = "BANKACCOUNT",
			////		DataSource = BankViewcollection,
			////		RowCount = this . BankGrid . SelectedIndex
			////	} );
			//IsEditing = false;

		}

		#region KEYHANDLER for EDIT fields
		// These let us tab thtorugh the editfields back and forward correctly
		private void Window_PreviewKeyUp ( object sender, KeyEventArgs e )
		{
			Debug . WriteLine ( $"  KEYUP key = {e . Key}, Shift = {keyshifted}" );

			if ( e . Key == Key . RightShift || e . Key == Key . LeftShift )
			{
				keyshifted = false;
				return;
			}

			if ( keyshifted && ( e . Key == Key . RightShift || e . Key == Key . LeftShift ) )
			{
				keyshifted = false;
				e . Handled = true;
				return;
			}

		}


		private void Window_PreviewKeyDown ( object sender, KeyEventArgs e )
		{
			if ( e . Key == Key . RightShift || e . Key == Key . LeftShift )
			{
				keyshifted = true;
				e . Handled = true;
				return;
			}

			if ( keyshifted == false )
			{
				if ( e . Key == Key . Tab && e . Source == cdate )
				{
					e . Handled = true;
					Custno . Focus ( );
					return;
				}
				return;
			}
			else
			{
				// SHIFT KEY DOWN - KEY DOWN
				// Handle  the tabs to make them cycle around the data entry fields
				if ( e . Key == Key . Tab && e . Source == cdate )
				{
					e . Handled = true;
					odate . Focus ( );
					return;
				}
				if ( e . Key == Key . Tab && e . Source == Custno )
				{
					e . Handled = true;
					cdate . Focus ( );
					//					Debug . WriteLine ( $"KEYDOWN Shift turned OFF" );
					return;
				}
			}
		}

		#endregion KEYHANDLER for EDIT fields

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
