using System;
using System . Collections . Generic;
using System . ComponentModel;
using System . Data;
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
	/// Interaction logic for DetailsDbView.xaml
	/// </summary>
	public partial class DetailsDbView : Window
	{
		public DetCollection DetViewerDbcollection = null;// = new DetCollection ( );//. DetViewerDbcollection;

		#region CollectionView stuff
		public CollectionViewSource DetViewSource { get; set; }

		// Get our personal Collection view of the Db
		public ICollectionView DetviewerView { get; set; }

		// items for CollectionView
		public int CurrentItem { get; internal set; }
		public Action CurrentChanged { get; internal set; }

		#endregion CollectionView stuff
		//		DetViewer.GetEnumerator();

		private bool IsDirty = false;
		private bool Startup = true;
		private bool LinktoParent = false;
		private bool IsFiltered = false;
		public static bool Triggered = false;
		private bool TriggeredDataUpdate = false;
		private bool LoadingDbData = false;
		private bool IsEditing { get; set; }
		private bool keyshifted {get; set; }

		private string _bankno = "";
		private string _custno = "";
		private string _actype = "";
		private string _balance = "";
		private string _odate = "";
		private string _cdate = "";
		private SqlDbViewer SqlParentViewer = null;
		private MultiViewer MultiParentViewer = null;
		private DbSelector DbsParentViewer = null;
		public DataChangeArgs dca = new DataChangeArgs ( );

		public DetailsDbView (SqlDbViewer sqldbv = null, MultiViewer mv = null, DbSelector dbs=null)
		{
			Startup = true;
			InitializeComponent ( );
			//Type calltype = typeof ( callerWin );
			MultiParentViewer = mv ;
			SqlParentViewer = sqldbv;
			DbsParentViewer = dbs;			
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
			Type callerType = null;
			Mouse . OverrideCursor = Cursors . Wait;
			this . Show ( );
			this . Refresh ( );
			Startup = true;

			dca . SenderName = sender . ToString ( );
			this . MouseDown += delegate { DoDragMove ( ); };
			// An EditDb has changed the current index 
			EventControl . EditIndexChanged += EventControl_EditIndexChanged;
			// A Multiviewer has changed the current index 
			EventControl . MultiViewerIndexChanged += EventControl_EditIndexChanged;
			// Another viewer has changed the current index 
			EventControl . ViewerIndexChanged += EventControl_EditIndexChanged;      // Callback in THIS FILE
			EventControl . ViewerDataUpdated += EventControl_DataUpdated;
			EventControl . DetDataLoaded += EventControl_DetDataLoaded;

			await DetailCollection . LoadDet ( DetViewerDbcollection, "DETAILSDBVIEW", 2, true );

			SaveBttn . IsEnabled = false;
			// Save linkage setting as we need to disable it while we are loading
			bool tmp = Flags . LinkviewerRecords;
			if ( Flags . LinkviewerRecords )
				LinkRecords . IsChecked = true;
			Flags . DetDbEditor = this;
			// Set window to TOPMOST
			OntopChkbox . IsChecked = true;
			this . Topmost = true;
			this . Focus ( );
			// Reset linkage setting
			Flags . LinkviewerRecords = tmp;
			LinktoParent = false;
			this . DetGrid . SelectedIndex = 0;
			Startup = false;
		}

		private void ViewerGrid_RowEditEnding ( object sender, System . Windows . Controls . DataGridRowEditEndingEventArgs e )
		{
			// Save changes and tell other viewers about the change
			int currow = 0;
			currow = this . DetGrid . SelectedIndex;
			// Save current row so we can reposition correctly at end of the entire refresh process					
			DetailsViewModel ss = new DetailsViewModel ( );
			ss = this . DetGrid . SelectedItem as DetailsViewModel;
			// This is the NEW DATA from the current row
			SQLHandlers sqlh = new SQLHandlers ( );
			sqlh . UpdateDbRowAsync ( "DETAILS", ss, this . DetGrid . SelectedIndex );

			this . DetGrid . SelectedIndex = currow;
			this . DetGrid . SelectedItem = currow;
			Utils . SetUpGridSelection ( this . DetGrid, this . DetGrid . SelectedIndex );
			// Notify EditDb to upgrade its grid
			if ( Flags . CurrentEditDbViewer != null )
				Flags . CurrentEditDbViewer . UpdateGrid ( "DETAILS" );
			TriggeredDataUpdate = true;
			// ***********  DEFINITE WIN  **********
			// This DOES trigger a notification to SQLDBVIEWER for sure !!!   14/5/21
			EventControl . TriggerViewerDataUpdated ( DetViewerDbcollection,
				new LoadedEventArgs
				{
					CallerType = "DETAILSDBVIEW",
					CallerDb = "DETAILS",
					DataSource = DetViewerDbcollection,
					RowCount = this . DetGrid . SelectedIndex
				} );
		}

		#region DATA BASED EVENT HANDLERS
		private void EventControl_EditIndexChanged ( object sender, IndexChangedArgs e )
		{
			// Handle selection change by other windows if linkage is ON
			Triggered = true;
			if ( IsEditing )
			{
				//IsEditing = false;
				return;
			}
			this . DetGrid . SelectedIndex = e . Row;
			Utils . SetSelectedItemFirstRow ( this . DetGrid, this . DetGrid . SelectedItem );
			this . DetGrid . Refresh ( );
			Triggered = false;
		}

		/// <summary>
		/// Event handler that is triggered once Data is ready for us from the SQL loading functions.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void EventControl_DetDataLoaded ( object sender, LoadedEventArgs e )
		{
			// Event handler for BankDataLoaded
			if ( e . DataSource == null ) return;
			//ONLY do this if WE triggered the event
			if ( e . CallerDb != "DETAILSDBVIEW" )
			{Mouse . OverrideCursor = Cursors . Arrow;return;}

			if ( TriggeredDataUpdate )
			{
				TriggeredDataUpdate = false;
				return;
			}
			this . DetGrid . ItemsSource = null;
			this . DetGrid . Items . Clear ( );

			LoadingDbData = true;

			// Get our personal Collection view of the Db
			// We even can create a seperate data source as a List <DetailsViewModel> as shown below
			// but we do not need to do so
			//IList<DetailsViewModel> DetailsDataListView = e . DataSource as DetCollection;

			// This (DetviewerView) is simply an ICollectionView of the data received in e.DataSource from SQL load methods
			// So now we have an independent reference to the "Base" data that can be manipulated in any way we wish
			// without effecting any other Viewer's window content
			DetviewerView = CollectionViewSource . GetDefaultView ( e . DataSource as DetCollection );
			DetViewerDbcollection = e . DataSource as DetCollection;
			DetviewerView . Refresh ( );
			this . DetGrid . Focus ( );
			this . DetGrid . ItemsSource = DetviewerView;
			this . DetGrid . SelectedIndex = 0;
			this . DetGrid . SelectedItem = 0;
//			this . DetGrid . CurrentItem = 0;
//			this . DetGrid . UpdateLayout ( );
			Thread . Sleep ( 250 );
			//			DataFields . Refresh ( );
			Count . Text = $"{this . DetGrid . SelectedIndex} / { this . DetGrid . Items . Count . ToString ( )}";
			Mouse . OverrideCursor = Cursors . Arrow;
			this . DetGrid . Refresh ( );
			Debug . WriteLine ( "BANKDBVIEW : Details Data fully loaded" );
		}
		private async void EventControl_DataUpdated ( object sender, LoadedEventArgs e )
		{
			if ( e . CallerDb == "DETAILSDBVIEW" || e . CallerDb == "DETAILS" ) return;
			// Receiving Notification from a remote viewer that data has been changed, so we MUST now update our DataGrid
			if ( TriggeredDataUpdate )
			{
				TriggeredDataUpdate = false;
				return;
			}
			Debug . WriteLine ( $"BankDbView : Data changed event notification received successfully." );
			int currsel = this . DetGrid . SelectedIndex;

			this . DetGrid . ItemsSource = null;
			this . DetGrid . Items . Clear ( );
			Mouse . OverrideCursor = Cursors . Wait;
			await DetailCollection . LoadDet ( DetViewerDbcollection, "DETAILSDBVIEW", 2, true );
			IsDirty = false;
		}
		#endregion DATA BASED EVENT HANDLERS

		//public void ExternalDataUpdate ( int DbEditChangeType, int row, string currentDb )
		//{
		//	// Receiving Notification from a remote viewer that data has been changed, so we MUST now update our DataGrid
		//	Debug . WriteLine ( $"BankDbView : Data changed event notification received successfully." );
		//	DetGrid . ItemsSource = null;
		//	this . DetGrid . Items . Clear ( );
		//	// Get our personal Collection view of the Db
		//	IList<DetailsViewModel> SQLDetViewerView = e . DataSource as DetCollection;
		//	// This (DetviewerView) is an ICollectionView 
		//	DetviewerView = CollectionViewSource . GetDefaultView ( SQLDetViewerView );
		//	this . DetGrid . ItemsSource = DetviewerView;

		//	this . DetGrid . SelectedIndex = 0;
		//	this . DetGrid . SelectedItem = 0;
		//	Mouse . OverrideCursor = Cursors . Arrow;
		//	this . DetGrid . Refresh ( );
		//}
		#endregion Startup/ Closedown


		private void ShowBank_KeyDown ( object sender, System . Windows . Input . KeyEventArgs e )
		{

		}

		private void Close_Click ( object sender, RoutedEventArgs e )
		{
			Close ( );
		}

		private void Window_Closing ( object sender, System . ComponentModel . CancelEventArgs e )
		{
			//			EventControl . ViewerDataHasBeenChanged -= ExternalDataUpdate;      // Callback in THIS FILE
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
			//UnSubscribe from Bank Data Changed event declared in EventControl
			Flags . DetDbEditor = null;
			//Another viewer has changed selection
			EventControl . EditIndexChanged -= EventControl_EditIndexChanged;
			// A Multiviewer has changed the current index 
			EventControl . MultiViewerIndexChanged -= EventControl_EditIndexChanged;
			// Another SqlDbviewer has changed the current index 
			EventControl . ViewerIndexChanged -= EventControl_EditIndexChanged;      // Callback in THIS FILE
												 // Main update notification handler
												 //			EventControl . DataUpdated -= EventControl_DataUpdated;
			EventControl . ViewerDataUpdated -= EventControl_DataUpdated;
			EventControl . DetDataLoaded -= EventControl_DetDataLoaded;


		}

		static bool IsLinkActive ( bool ParentLinkTo )
		{
			return Flags . SqlDetViewer != null && ParentLinkTo == false;
		}
		private void DetGrid_SelectionChanged ( object sender, System . Windows . Controls . SelectionChangedEventArgs e )
		{
			bool reslt = false;
			// This uses the external Method above
			//			Predicate<bool> IsActive = delegate ( bool b ) { return IsLinkActive ( LinkToParent . IsEnabled ); };
			// direct action (Lambda) version 
			Predicate<bool> IsActive = delegate ( bool b )
			{
				return Flags . SqlDetViewer != null && LinktoParent == false;
			};

			if ( LoadingDbData )
			{
				LoadingDbData = false;
				return;
			}

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

			// check to see if an SqlDbViewer has been opened that we can link to
			//			if ( Flags . SqlDetViewer != null && LinkToParent . IsEnabled == false )
			if ( IsActive ( reslt ) )
			{
				LinkToParent . IsEnabled = true;
				SqlParentViewer = Flags . SqlDetViewer;
			}
			//else if ( Flags . SqlDetViewer == null )
			//{
			//	if ( LinkToParent . IsEnabled )
			//	{
			//		LinkToParent . IsEnabled = false;
			//		LinkToParent . IsChecked = false;
			//		LinktoParent = false;
			//		SqlParentViewer = null;
			//	}
			//}
			//			Debug . WriteLine ( $"DetviewerView CurrentItem has changed = {DetviewerView .}" );
			// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
			Utils . SetUpGridSelection ( this . DetGrid, this . DetGrid . SelectedIndex );
			Startup = true;
			DataFields . DataContext = this . DetGrid . SelectedItem;

			if ( Flags . LinkviewerRecords && Triggered == false )
			{
				//				Debug . WriteLine ( $" 6-1 *** TRACE *** DETAILSDBVIEWER : Itemsview_OnSelectionChanged  DETAILS - Sending TriggerEditDbIndexChanged Event trigger" );
				TriggerViewerIndexChanged ( DetGrid );    
			}

			// Only  do this if global link is OFF
			if ( LinktoParent )
			{
				// update parents row selection
				string bankno = "";
				string custno = "";
				var dvm = this . DetGrid . SelectedItem as DetailsViewModel;

				if ( SqlParentViewer != null )
				{
					int rec = Utils . FindMatchingRecord ( dvm . CustNo, dvm . BankNo, SqlParentViewer . DetailsGrid, "DETAILS" );
					SqlParentViewer . DetailsGrid . SelectedIndex = rec;
					Utils . SetUpGridSelection ( SqlParentViewer . DetailsGrid, rec );
				}
				else if ( MultiParentViewer != null )
				{
					int rec = Utils . FindMatchingRecord ( dvm . CustNo, dvm . BankNo, MultiParentViewer . DetailsGrid, "DETAILS" );
					MultiParentViewer . DetailsGrid . SelectedIndex = rec;
					Utils . SetUpGridSelection ( MultiParentViewer . DetailsGrid, rec );
				}
//				Utils . SetSelectedItemFirstRow ( this . DetGrid, this . DetGrid . SelectedItem );
			}
			Count . Text = $"{this . DetGrid . SelectedIndex} / { this . DetGrid . Items . Count . ToString ( )}";
			Triggered = false;
		}
		public void TriggerViewerIndexChanged ( System . Windows . Controls . DataGrid grid )
		{
			string SearchCustNo = "";
			string SearchBankNo = "";
			DetailsViewModel CurrentDettSelectedRecord = this . DetGrid . SelectedItem as DetailsViewModel;
			if ( CurrentDettSelectedRecord == null ) return;
			SearchCustNo = CurrentDettSelectedRecord . CustNo;
			SearchBankNo = CurrentDettSelectedRecord . BankNo;
			EventControl . TriggerViewerIndexChanged ( this,
			new IndexChangedArgs
			{
				Senderviewer = this,
				Bankno = SearchBankNo,
				Custno = SearchCustNo,
				dGrid = this . DetGrid,
				Sender = "DETAILS",
				SenderId = "DETAILSDBVIEW",
				Row = this . DetGrid . SelectedIndex
			} );
		}

		private async Task<bool> SaveButton ( object sender = null, RoutedEventArgs e = null )
		{
			// Get the current rows data
			IsDirty = false;
			int CurrentSelection = this . DetGrid . SelectedIndex;
			this . DetGrid . SelectedItem = this . DetGrid . SelectedIndex;
			DetailsViewModel bvm = new DetailsViewModel ( );
			bvm = this . DetGrid . SelectedItem as DetailsViewModel;
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
			await sqlh . UpdateDbRow ( "DETAILS", bvm );

			EventControl . TriggerViewerDataUpdated ( DetViewerDbcollection,
				new LoadedEventArgs
				{
					CallerType = "DETAILSDBVIEW",
					CallerDb = "DETAILS",
					DataSource = DetViewerDbcollection,
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
				DetailCollection det = new DetailCollection ( );
				await DetailCollection . LoadDet ( DetViewerDbcollection, "DETAILSDBVIEW", 2, true );

				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				MultiAccounts . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBorderBrushRed" );
				MultiAccounts . Background = br;
				MultiAccounts . Content = "Show All";
				Count . Text = $"{this . DetGrid . SelectedIndex} / { this . DetGrid . Items . Count . ToString ( )}";

				// Get Custno from ACTIVE gridso we can find it in other grids
				MultiViewer mv = new MultiViewer ( );
				int rec = Utils . FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . DetGrid, "DETAILS" );
				this . DetGrid . SelectedIndex = currsel;
				if ( rec >= 0 )
					this . DetGrid . SelectedIndex = rec;
				else
					this . DetGrid . SelectedIndex = 0;
				Utils . SetUpGridSelection ( this . DetGrid, this . DetGrid . SelectedIndex );
			}
			else
			{
				Flags . IsMultiMode = false;
				int currsel = this . DetGrid . SelectedIndex;
				DetailsViewModel bgr = this . DetGrid . SelectedItem as DetailsViewModel;

				DetailCollection det = new DetailCollection ( );
				await DetailCollection . LoadDet ( DetViewerDbcollection, "DETAILSDBVIEW", 2, true );
				// Just reset our current itemssource to man Db
				this . DetGrid . ItemsSource = null;
				this . DetGrid . ItemsSource = DetviewerView;
				this . DetGrid . Refresh ( );

				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGreen" );
				MultiAccounts . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGreen" );
				MultiAccounts . Background = br;
				MultiAccounts . Content = "Multi Accounts";
				Count . Text = $"{this . DetGrid . SelectedIndex} / { this . DetGrid . Items . Count . ToString ( )}";

				MultiViewer mv = new MultiViewer ( );
				int rec = Utils . FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . DetGrid, "DETAILS" );
				this . DetGrid . SelectedIndex = 0;

				if ( rec >= 0 )
					this . DetGrid . SelectedIndex = rec;
				else
					this . DetGrid . SelectedIndex = 0;
				Utils . SetUpGridSelection ( this . DetGrid, this . DetGrid . SelectedIndex );
			}
		}
		public void SendDataChanged ( SqlDbViewer o, DataGrid Grid, string dbName )
		{
			// Databases have DEFINITELY been updated successfully after a change
			// We Now Broadcast this to ALL OTHER OPEN VIEWERS here and now

			EventControl . TriggerViewerDataUpdated ( DetViewerDbcollection,
			new LoadedEventArgs
			{
				CallerType = "DETAILSDBVIEW",
				CallerDb = "DETAILS",
				DataSource = DetViewerDbcollection,
				RowCount = this . DetGrid . SelectedIndex
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
			if ( Flags . BankDbEditor != null )
				Flags . BankDbEditor . LinkRecords . IsChecked = Flags . LinkviewerRecords;
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
				LinktoParent = false;

			}
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

		#region Menu items

		private void Linq1_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var accounts = from items in DetViewerDbcollection
				       where ( items . AcType == 1 )
				       orderby items . CustNo
				       select items;
			this . DetGrid . ItemsSource = accounts;
		}
		private void Linq2_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var accounts = from items in DetViewerDbcollection
				       where ( items . AcType == 2 )
				       orderby items . CustNo
				       select items;
			this . DetGrid . ItemsSource = accounts;
		}
		private void Linq3_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var accounts = from items in DetViewerDbcollection
				       where ( items . AcType == 3 )
				       orderby items . CustNo
				       select items;
			this . DetGrid . ItemsSource = accounts;
		}
		private void Linq4_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var accounts = from items in DetViewerDbcollection
				       where ( items . AcType == 4 )
				       orderby items . CustNo
				       select items;
			this . DetGrid . ItemsSource = accounts;
		}
		private void Linq5_Click ( object sender, RoutedEventArgs e )
		{
			//select All the items first;			
			var accounts = from items in DetViewerDbcollection orderby items . CustNo, items . AcType select items;
			//Next Group BankAccountViewModel collection on Custno
			var grouped = accounts . GroupBy (
				b => b . CustNo );

			//Now filter content down to only those a/c's with multiple Bank A/c's
			var sel = from g in grouped
				  where g . Count ( ) > 1
				  select g;

			// Finally, iterate thru the list of grouped CustNo's matching to CustNo in the full accountsdata
			// giving us ONLY the full records for any recordss that have > 1 Bank accounts
			List<DetailsViewModel> output = new List<DetailsViewModel> ( );
			foreach ( var item1 in sel )
			{
				foreach ( var item2 in accounts )
				{
					if ( item2 . CustNo . ToString ( ) == item1 . Key )
					{
						output . Add ( item2 );
					}
				}
			}
			this . DetGrid . ItemsSource = output;
		}
		private void Linq6_Click ( object sender, RoutedEventArgs e )
		{
			var accounts = from items in DetViewerDbcollection orderby items . CustNo, items . AcType select items;
			this . DetGrid . ItemsSource = accounts;
		}

		private void Filter_Click ( object sender, RoutedEventArgs e )
		{
			// Show Filter system
			//	MessageBox . Show ( "Filter dialog will appear here !!" );
		}
		#endregion Menu items
		/// <summary>
		/// Generic method to send Index changed Event trigger so that 
		/// other viewers can update thier own grids as relevant
		/// </summary>
		/// <param name="grid"></param>
		//*************************************************************************************************************//
		//public void TriggerViewerIndexChanged ( DataGrid grid )
		//{
		//	string SearchCustNo = "";
		//	string SearchBankNo = "";
		//	DetailsViewModel CurrentDetSelectedRecord = grid . SelectedItem as DetailsViewModel;
		//	SearchCustNo = CurrentDetSelectedRecord . CustNo;
		//	SearchBankNo = CurrentDetSelectedRecord . BankNo;
		//	EventControl . TriggerViewerIndexChanged ( this,
		//		new IndexChangedArgs
		//		{
		//			Senderviewer = null,
		//			Bankno = SearchBankNo,
		//			Custno = SearchCustNo,
		//			dGrid = grid,
		//			Sender = "DETAILS",
		//			SenderId = "DETAILSDBVIEW",
		//			Row = grid . SelectedIndex
		//		} );
		//	this . Focus ( );
		//}

		private void Exit_Click ( object sender, RoutedEventArgs e )
		{
			Close ( );
		}

		private void Options_Click ( object sender, RoutedEventArgs e )
		{

		}
		private void Filterx_Click ( object sender, RoutedEventArgs e )
		{
			string Custno = "0";
			string Bankno = "0";
			DetailsViewModel dvm = this.DetGrid.SelectedItem as DetailsViewModel;
			if ( dvm != null )
			{
				Bankno = dvm?.BankNo;
				Custno = dvm?.CustNo;
			}
			MenuItem mi = new MenuItem ( );
			mi = sender as MenuItem;
			if ( mi . Name == "Filter1" )
				DoFilter( 1); 
			else if ( mi . Name == "Filter2" )
				DoFilter ( 2 );
			else if ( mi . Name == "Filter3" )
				DoFilter ( 3 );
			else if ( mi . Name == "Filter4" )
				DoFilter ( 4 );
			else if ( mi . Name == "FilterReset" )
				DoFilter ( 0 );
			// Try to reset selection 
			int rec =Utils . FindMatchingRecord (Custno, Bankno, this.DetGrid, "DETAILS" );
			this . DetGrid . SelectedIndex = rec;
			Utils . SetUpGridSelection ( this . DetGrid, rec != -1 ? rec : 0 );
			// force it to top of data grid
			Utils.SetSelectedItemFirstRow ( this . DetGrid, this . DetGrid . SelectedItem );

			this . DetGrid . UpdateLayout (  );
		}
		private void DoFilter( int filterValue)
		{
			// Test filter to try out my new Db ColloectionView stuff
			if ( filterValue == 0 && IsFiltered )
			{
				// Return to original collection
				this . DetGrid . ItemsSource = DetviewerView;
				this . DetGrid . Refresh ( );
				IsFiltered = false;
				return;
			}
			DetailsViewModel dvm = new DetailsViewModel ( );
			var temp = DetViewerDbcollection . Where ( dvm => dvm . AcType == filterValue );
			ICollection<DetailsViewModel> t = CollectionViewSource . GetDefaultView ( temp ) as ICollection<DetailsViewModel>;
			this . DetGrid . ItemsSource = temp;
			this . DetGrid . Refresh ( );
//			Filter1 . Header = "Reset to All records";
			IsFiltered = true;
		}
		//private void Filter2_Click ( object sender, RoutedEventArgs e )
		//{
		//	// Test filter to try out my new Db ColloectionView stuff
		//	if ( IsFiltered )
		//	{
		//		// Return to original collection
		//		this . DetGrid . ItemsSource = DetviewerView;
		//		this . DetGrid . Refresh ( );
		//		IsFiltered = false;
		//		Filter2 . Header = "A/c Type = 2";
		//		return;
		//	}
		//	DetailsViewModel dvm = new DetailsViewModel ( );
		//	var temp = DetViewerDbcollection . Where ( dvm => dvm . AcType == 2 );
		//	ICollection<DetailsViewModel> t = CollectionViewSource . GetDefaultView ( temp ) as ICollection<DetailsViewModel>;
		//	this . DetGrid . ItemsSource = temp;
		//	this . DetGrid . Refresh ( );
		//	Filter2 . Header = "Reset to All records";
		//	IsFiltered = true;
		//}
		//private void Filter3_Click ( object sender, RoutedEventArgs e )
		//{
		//	// Test filter to try out my new Db ColloectionView stuff
		//	if ( IsFiltered )
		//	{
		//		// Return to original collection
		//		this . DetGrid . ItemsSource = DetviewerView;
		//		this . DetGrid . Refresh ( );
		//		IsFiltered = false;
		//		Filter3 . Header = "A/c Type = 3";
		//		return;
		//	}
		//	DetailsViewModel dvm = new DetailsViewModel ( );
		//	var temp = DetViewerDbcollection . Where ( dvm => dvm . AcType == 3 );
		//	ICollection<DetailsViewModel> t = CollectionViewSource . GetDefaultView ( temp ) as ICollection<DetailsViewModel>;
		//	this . DetGrid . ItemsSource = temp;
		//	this . DetGrid . Refresh ( );
		//	Filter3 . Header = "Reset to All records";
		//	IsFiltered = true;
		//}
		//private void Filter4_Click ( object sender, RoutedEventArgs e )
		//{
		//	// Test filter to try out my new Db ColloectionView stuff
		//	if ( IsFiltered )
		//	{
		//		// Return to original collection
		//		this . DetGrid . ItemsSource = DetviewerView;
		//		this . DetGrid . Refresh ( );
		//		IsFiltered = false;
		//		Filter4 . Header = "A/c Type = 4";
		//		return;
		//	}
		//	DetailsViewModel dvm = new DetailsViewModel ( );
		//	var temp = DetViewerDbcollection . Where ( dvm => dvm . AcType == 4 );
		//	ICollection<DetailsViewModel> t = CollectionViewSource . GetDefaultView ( temp ) as ICollection<DetailsViewModel>;
		//	this . DetGrid . ItemsSource = temp;
		//	this . DetGrid . Refresh ( );
		//	Filter4 . Header = "Reset to All records";
		//	IsFiltered = true;
		//}
		public void FilterView ( object sender, FilterEventArgs e )
		{
			bool result = false;
			DetailsViewModel dvm = e . Item as DetailsViewModel;
			if ( Convert . ToInt32 ( dvm . CustNo ) > 105700 )
				e . Accepted = true;
			else
				e . Accepted = false;
		}
		private void Edit_LostFocus ( object sender, RoutedEventArgs e )
		{
			IsDirty = true;
			SaveBttn . IsEnabled = true;
		}

		private void DetGrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			//// Data has been changed in one of our rows.
			//DetailsViewModel dvm = sender as DetailsViewModel;
			//dvm = e . Row.Item as DetailsViewModel;
			//DataGridColumn dgc = e . Column;
			//SQLHandlers sqlh = new SQLHandlers ( );
			//sqlh . UpdateDbRowAsync ( "DETAILS", dvm );
			//SendDataChanged ( null, this . DetGrid, "DETAILS" );
			IsEditing = false;


		}

		private void DetGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
		{
			IsEditing = true;
		}

		private void cdate_PreviewKeyUp ( object sender, KeyEventArgs e )
		{

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
	}
}


/*
//			BankAccountViewModel bank = new BankAccountViewModel();
//			var filtered = from bank inDetViewercollection . Where ( x => bank . CustNo = "1055033" ) select x;
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


