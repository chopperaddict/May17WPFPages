using System;
using System . Collections . Generic;
using System . ComponentModel;
using System . Configuration;
using System . Data;
using System . Diagnostics;
using System . IO;
using System . Linq;
using System . Reflection;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Imaging;

using WPFPages . Properties;
using WPFPages . ViewModels;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for DetailsDbView.xaml
	/// </summary>
	public partial class DetailsDbView : Window
	{
		public DetCollection DetViewerDbcollection = null;// = new DetCollection ( );//. DetViewerDbcollection;

		// Crucial structure for use when a Grid row is being edited
		private static RowData bvmCurrent = null;

		#region CollectionView stuff
		public CollectionViewSource DetViewSource
		{
			get; set;
		}

		// Get our personal Collection view of the Db
		public ICollectionView DetviewerView
		{
			get; set;
		}
		private bool IsLeftButtonDown
		{
			get; set;
		}
		// items for CollectionView
		public int CurrentItem
		{
			get; internal set;
		}
		public Action CurrentChanged
		{
			get; internal set;
		}

		#endregion CollectionView stuff
		//		DetViewer.GetEnumerator();

		private static Point _startPoint
		{
			get; set;
		}
		private bool IsDirty = false;
		private bool Startup = true;
		private static bool LinktoParent = false;
		private static bool LinktoMultiParent = false;
		private bool IsFiltered = false;
		public static bool Triggered = false;
		private bool TriggeredDataUpdate = false;
		private bool LoadingDbData = false;
		private bool IsEditing
		{
			get; set;
		}
		private bool keyshifted
		{
			get; set;
		}
		// This MAINTAINS setting values across instances !!!
		public static int dindex
		{
			get; set;
		}


		private string _bankno = "";
		private string _custno = "";
		private string _actype = "";
		private string _balance = "";
		private string _odate = "";
		private string _cdate = "";

		private DetailsViewModel _itemdata = new DetailsViewModel ( );
		private bool LeftClickinprogress = false;
		private SqlDbViewer SqlParentViewer = null;
		private MultiViewer MultiParentViewer = null;
		private DbSelector DbsParentViewer = null;
		public DataChangeArgs dca = new DataChangeArgs ( );

		private Thread t1;
		public DetailsDbView ( SqlDbViewer sqldbv = null, MultiViewer mv = null, DbSelector dbs = null )
		{
			Startup = true;
			InitializeComponent ( );
			//Type calltype = typeof ( callerWin );
			//Identify individual windows for update protection
			this . Tag = ( Guid ) Guid . NewGuid ( );

			MultiParentViewer = mv;
			SqlParentViewer = sqldbv;
			DbsParentViewer = dbs;
		}
		#region Mouse support
		private void DoDragMove ( )
		{//Handle the button NOT being the left mouse button
		 // which will crash the DragMove Fn.....
			try
			{
				this . DragMove ( );
			}
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

			string ndx = ( string ) Properties . Settings . Default [ "DetailsDbView_dindex" ];
			dindex = int . Parse ( ndx );
			this . DetGrid . SelectedIndex = dindex < 0 ? 0 : dindex;

			//			this . MouseDown += delegate { DoDragMove ( ); };
			Utils . SetupWindowDrag ( this );
			// An EditDb has changed the current index 
			EventControl . EditIndexChanged += EventControl_EditIndexChanged;
			// A Multiviewer has changed the current index 
			EventControl . MultiViewerIndexChanged += EventControl_EditIndexChanged;
			// Another viewer has changed the current index 
			EventControl . ViewerIndexChanged += EventControl_EditIndexChanged;      // Callback in THIS FILE
			EventControl . ViewerDataUpdated += EventControl_DataUpdated;
			EventControl . DetDataLoaded += EventControl_DetDataLoaded;

			EventControl . GlobalDataChanged += EventControl_GlobalDataChanged;


			await DetailCollection . LoadDet ( DetViewerDbcollection, "DETAILSDBVIEW", 2, true );

			SaveBttn . IsEnabled = false;
			// Save linkage setting as we need to disable it while we are loading
			bool tmp = Flags . LinkviewerRecords;

			Flags . DetDbEditor = this;
			// Set window to TOPMOST
			OntopChkbox . IsChecked = true;
			this . Topmost = true;
			this . Focus ( );
			// start our linkage monitor
			t1 = new Thread ( checkLinkages );
			t1 . IsBackground = true;
			t1 . Priority = ThreadPriority . Lowest;
			t1 . Start ( );
			// Reset linkage setting
			Flags . LinkviewerRecords = tmp;
			if ( Flags . LinkviewerRecords )
			{
				LinkRecords . IsChecked = true;
				LinktoParent = false;
			}
			else
			{
				LinkRecords . IsChecked = false;
				LinktoParent = false;
			}
			LinktoMultiParent = false;
			Startup = false;
		}

		private void EventControl_GlobalDataChanged ( object sender, GlobalEventArgs e )
		{
			if ( e . CallerType == "DETAILSDBVIEWER" )
				return;
			//Update our own data tyoe only
			DetailCollection . LoadDet ( null, "DETAILS", 1, true );

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
			dindex = e . Row;
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
			if ( e . DataSource == null )
				return;
			//ONLY do this if WE triggered the event
			if ( e . CallerDb != "DETAILSDBVIEW" )
			{
				Mouse . OverrideCursor = Cursors . Arrow;
				return;
			}

			if ( TriggeredDataUpdate )
			{
				TriggeredDataUpdate = false;
				return;
			}
			this . DetGrid . ItemsSource = null;
			this . DetGrid . Items . Clear ( );

			LoadingDbData = true;

			// This (DetviewerView) is simply an ICollectionView of the data received in e.DataSource from SQL load methods
			// So now we have an independent reference to the "Base" data that can be manipulated in any way we wish
			// without effecting any other Viewer's window content
			DetviewerView = CollectionViewSource . GetDefaultView ( e . DataSource as DetCollection );
			DetViewerDbcollection = e . DataSource as DetCollection;
			DetviewerView . Refresh ( );
			this . DetGrid . Focus ( );
			this . DetGrid . ItemsSource = DetviewerView;
			this . DetGrid . SelectedIndex = dindex;
			this . DetGrid . SelectedItem = dindex;
			Utils . SetUpGridSelection ( DetGrid, dindex );
			Thread . Sleep ( 250 );
			Count . Text = $"{this . DetGrid . SelectedIndex} / { this . DetGrid . Items . Count . ToString ( )}";
			Mouse . OverrideCursor = Cursors . Arrow;
			this . DetGrid . Refresh ( );
			Debug . WriteLine ( "BANKDBVIEW : Details Data fully loaded" );
			bool reslt = false;
		}
		private async void EventControl_DataUpdated ( object sender, LoadedEventArgs e )
		{
			if ( e . CallerDb == "DETAILSDBVIEW" || e . CallerDb == "DETAILS" )
				return;
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
			// Close our monitor thread

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

			EventControl . GlobalDataChanged -= EventControl_GlobalDataChanged;


			Utils . SaveProperty ( "DetailsDbView_dindex", dindex . ToString ( ) );
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

			// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
			Utils . SetUpGridSelection ( this . DetGrid, this . DetGrid . SelectedIndex );
			Startup = true;
			DataFields . DataContext = this . DetGrid . SelectedItem;

			if ( Flags . LinkviewerRecords && Triggered == false )
				TriggerViewerIndexChanged ( DetGrid );

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
				if ( IsMultiLinkActive ( LinktoMultiParent ) )
				{
					Flags . SqlMultiViewer . DetailsGrid . SelectedIndex = this . DetGrid . SelectedIndex;
					Flags . SqlMultiViewer . DetailsGrid . ScrollIntoView ( this . DetGrid . SelectedIndex );
					Utils . SetUpGridSelection ( Flags . SqlMultiViewer . DetailsGrid, this . DetGrid . SelectedIndex );
				}
			}
			else if ( LinktoMultiParent )
			{
				Flags . SqlMultiViewer . DetailsGrid . SelectedIndex = this . DetGrid . SelectedIndex;
				Flags . SqlMultiViewer . DetailsGrid . ScrollIntoView ( this . DetGrid . SelectedIndex );
				Utils . SetUpGridSelection ( Flags . SqlMultiViewer . DetailsGrid, this . DetGrid . SelectedIndex );
			}
			Count . Text = $"{this . DetGrid . SelectedIndex} / { this . DetGrid . Items . Count . ToString ( )}";
			dindex = this . DetGrid . SelectedIndex;
			Triggered = false;
		}
		public void TriggerViewerIndexChanged ( System . Windows . Controls . DataGrid grid )
		{
			string SearchCustNo = "";
			string SearchBankNo = "";
			DetailsViewModel CurrentDettSelectedRecord = this . DetGrid . SelectedItem as DetailsViewModel;
			if ( CurrentDettSelectedRecord == null )
				return;
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
			if ( bvm == null )
				return false;

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
					SenderGuid = this . Tag . ToString ( ),
					RowCount = this . DetGrid . SelectedIndex
				} );
			EventControl . TriggerGlobalDataChanged ( this, new GlobalEventArgs
			{
				CallerType = "DETAILSDBVIEW",
				AccountType = "DETAILS",
				SenderGuid = this . Tag?.ToString ( )
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
		{
			if ( !Startup )
				CompareFieldData ( );
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

		//private async void MultiAccts_Click ( object sender, RoutedEventArgs e )
		//{
		//	// Filter data to show ONLY Custoimers with multiple bank accounts

		//	if ( MultiAccounts . Content != "Show All" )
		//	{
		//		int currsel = this . DetGrid . SelectedIndex;
		//		DetailsViewModel bgr = this . DetGrid . SelectedItem as DetailsViewModel;
		//		Flags . IsMultiMode = true;
		//		DetailCollection det = new DetailCollection ( );
		//		await DetailCollection . LoadDet ( DetViewerDbcollection, "DETAILSDBVIEW", 2, true );

		//		ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
		//		MultiAccounts . Template = tmp;
		//		Brush br = Utils . GetDictionaryBrush ( "HeaderBorderBrushRed" );
		//		MultiAccounts . Background = br;
		//		MultiAccounts . Content = "Show All";
		//		Count . Text = $"{this . DetGrid . SelectedIndex} / { this . DetGrid . Items . Count . ToString ( )}";

		//		// Get Custno from ACTIVE gridso we can find it in other grids
		//		MultiViewer mv = new MultiViewer ( );
		//		int rec = Utils . FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . DetGrid, "DETAILS" );
		//		this . DetGrid . SelectedIndex = currsel;
		//		if ( rec >= 0 )
		//			this . DetGrid . SelectedIndex = rec;
		//		else
		//			this . DetGrid . SelectedIndex = 0;
		//		Utils . SetUpGridSelection ( this . DetGrid, this . DetGrid . SelectedIndex );
		//	}
		//	else
		//	{
		//		Flags . IsMultiMode = false;
		//		int currsel = this . DetGrid . SelectedIndex;
		//		DetailsViewModel bgr = this . DetGrid . SelectedItem as DetailsViewModel;

		//		DetailCollection det = new DetailCollection ( );
		//		await DetailCollection . LoadDet ( DetViewerDbcollection, "DETAILSDBVIEW", 2, true );
		//		// Just reset our current itemssource to man Db
		//		this . DetGrid . ItemsSource = null;
		//		this . DetGrid . ItemsSource = DetviewerView;
		//		this . DetGrid . Refresh ( );

		//		ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGreen" );
		//		MultiAccounts . Template = tmp;
		//		Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGreen" );
		//		MultiAccounts . Background = br;
		//		MultiAccounts . Content = "Multi Accounts";
		//		Count . Text = $"{this . DetGrid . SelectedIndex} / { this . DetGrid . Items . Count . ToString ( )}";

		//		MultiViewer mv = new MultiViewer ( );
		//		int rec = Utils . FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . DetGrid, "DETAILS" );
		//		this . DetGrid . SelectedIndex = 0;

		//		if ( rec >= 0 )
		//			this . DetGrid . SelectedIndex = rec;
		//		else
		//			this . DetGrid . SelectedIndex = 0;
		//		Utils . SetUpGridSelection ( this . DetGrid, this . DetGrid . SelectedIndex );
		//	}
		//}
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
				SenderGuid = this . Tag . ToString ( ),
				RowCount = this . DetGrid . SelectedIndex
			} );
			EventControl . TriggerGlobalDataChanged ( this, new GlobalEventArgs
			{
				CallerType = "DETAILSDBVIEW",
				AccountType = "DETAILS",
				SenderGuid = this . Tag?.ToString ( )
			} );
			Mouse . OverrideCursor = Cursors . Arrow;
		}

		public bool CheckLinkRecordsStatus ( bool status )
		{
			bool reslt = false;
			if ( IsLinkActive ( reslt ) == true )
			{
				// We do have an active SqlDbviewer
				if ( status == true )
				{
					// Active viewer and is currently checked
					LinkRecords . IsEnabled = true;
					SqlParentViewer = Flags . SqlDetViewer;
				}
				else
				{
					// Active viewer but Not currently checked
					LinkRecords . IsEnabled = true;
				}
			}
			else
			{
				// We do NOT have an active SqlDbviewer
				LinkRecords . IsEnabled = false;
				LinkRecords . IsChecked = false;
				LinktoParent = false;
				SqlParentViewer = null;
			}
			return false;
		}
		private void LinkRecords_Click ( object sender, RoutedEventArgs e )
		{
			bool reslt = false;
			if ( LinkRecords . IsChecked == true )
			{
				//				CheckLinkRecordsStatus ( true);
				// Checkbox is being CHECKED
				if ( IsLinkActive ( reslt ) == true )
				{
					// We have an open Db SqlDbViewer open
					LinkRecords . IsEnabled = true;
					LinktoParent = false;
					SqlParentViewer = Flags . SqlDetViewer;
				}
				else
				{
					// No Details Db SqlDbViewer open
					LinkRecords . IsEnabled = false;
					LinkRecords . IsChecked = false;
					LinktoParent = false;
					SqlParentViewer = null;
				}
			}
			else
			{
				// Checkbox is being UNCHECKED
				if ( IsLinkActive ( reslt ) == true )
				{
					// we do have an open SqlDbViewer 
					LinkRecords . IsEnabled = true;
					SqlParentViewer = Flags . SqlDetViewer;
					LinktoParent = false;
				}
				else
				{
					// No Details Db SqlDbViewer open
					LinkRecords . IsEnabled = false;
					SqlParentViewer = null;
					LinktoParent = false;

				}
			}
			if ( IsMultiLinkActive ( reslt ) )
			{
				LinkToMulti . IsEnabled = true;
				MultiParentViewer = Flags . SqlMultiViewer;
			}
			else
			{
				LinkToMulti . IsEnabled = false;
				MultiParentViewer = null;
				LinkToMulti . IsChecked = false;
				LinktoMultiParent = false;
			}
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
				{
					LinkToParent . IsEnabled = true;
					LinkToParent . Refresh ( );
				}
				else
				{
					LinkToParent . IsEnabled = false;
					LinktoParent = false;
				}

			}
		}

		/// <summary>
		/// Link record selection to parent SQL viewer window only
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LinkToParent_Click ( object sender, RoutedEventArgs e )
		{
			bool reslt = false;
			if ( IsLinkActive ( reslt ) == false )
			{
				LinkToParent . IsEnabled = false;
				LinkToParent . IsChecked = false;
				SqlParentViewer = null;
				LinkRecords . IsChecked = false;
			}
			else
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
			DetailsViewModel dvm = this . DetGrid . SelectedItem as DetailsViewModel;
			if ( dvm != null )
			{
				Bankno = dvm?.BankNo;
				Custno = dvm?.CustNo;
			}
			MenuItem mi = new MenuItem ( );
			mi = sender as MenuItem;
			if ( mi . Name == "Filter1" )
				DoFilter ( 1 );
			else if ( mi . Name == "Filter2" )
				DoFilter ( 2 );
			else if ( mi . Name == "Filter3" )
				DoFilter ( 3 );
			else if ( mi . Name == "Filter4" )
				DoFilter ( 4 );
			else if ( mi . Name == "FilterReset" )
				DoFilter ( 0 );
			// Try to reset selection 
			int rec = Utils . FindMatchingRecord ( Custno, Bankno, this . DetGrid, "DETAILS" );
			this . DetGrid . SelectedIndex = rec;
			Utils . SetUpGridSelection ( this . DetGrid, rec != -1 ? rec : 0 );
			// force it to top of data grid
			Utils . SetSelectedItemFirstRow ( this . DetGrid, this . DetGrid . SelectedItem );

			this . DetGrid . UpdateLayout ( );
			this . DetGrid . Focus ( );
		}
		private void DoFilter ( int filterValue )
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

		#region DATA EDIT CONTROL METHODS
		/// <summary>
		///  DATA EDIT CONTROL METHODS
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DetGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
		{
			IsEditing = true;
			// Save  the current data for checking later on when we exit editing
			// but first, check to see if we already have one being saved !
			if ( bvmCurrent == null )
			{
				// Nope, so create a new one and get on with the edit process
				DetailsViewModel tmp = new DetailsViewModel ( );
				tmp = e . Row . Item as DetailsViewModel;
				// This sets up a new bvmControl object if needed, else we  get a null back
				bvmCurrent = CellEditControl . DetGrid_EditStart ( bvmCurrent, e );
			}
		}

		/// <summary>
		/// does nothing at all because it is called whenver any single cell is exited
		///     and not just when ENTER is hit to save any changes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void DetGrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			if ( bvmCurrent == null )
				return;

			// Has Data been changed in one of our rows. ?
			DetailsViewModel dvm = this . DetGrid . SelectedItem as DetailsViewModel;
			dvm = e . Row . Item as DetailsViewModel;

			// The sequence of these next 2 blocks is critical !!!
			//if we get here, make sure we have been NOT been told to EsCAPE out
			//	this is a DataGridEditAction dgea
			if ( e . EditAction == DataGridEditAction . Cancel )
			{
				// Either ENTER or Escape was hit, so data has been saved, or we need ot refresh the row if cancelled
				// So we go ahead and reload our grid with new data
				// and this will notify any other open viewers as well
				bvmCurrent = null;
				await DetailCollection . LoadDet ( DetViewerDbcollection, "DETAILSDBVIEW", 2, true );
				return;
			}

			if ( CellEditControl . DetGrid_EditEnding ( bvmCurrent, DetGrid, e ) == false )
			{       // No change made
				return;
			}

		}

		/// <summary>
		/// Compares 2 rows of BANKACCOUNT or DETAILS data to see if there are any changes
		/// </summary>
		/// <param name="ss"></param>
		/// <returns></returns>
		private bool CompareDataContent ( DetailsViewModel ss )
		{
			if ( ss . CustNo != bvmCurrent . _CustNo . ToString ( ) )
				return false;
			if ( ss . BankNo != bvmCurrent . _BankNo . ToString ( ) )
				return false;
			if ( ss . AcType != bvmCurrent . _AcType )
				return false;
			if ( ss . IntRate != bvmCurrent . _IntRate )
				return false;
			if ( ss . Balance != bvmCurrent . _Balance )
				return false;
			if ( ss . ODate != bvmCurrent . _ODate )
				return false;
			if ( ss . CDate != bvmCurrent . _CDate )
				return false;
			return true;
		}

		/// <summary>
		/// Called when an EDIT ends. This occurs whenever a field is exited, even if ENTER has NOT been pressed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ViewerGrid_RowEditEnding ( object sender, System . Windows . Controls . DataGridRowEditEndingEventArgs e )
		{
			// Save changes and tell other viewers about the change
			// if our saved row is null, it has already been checked in Cell_EndDedit processing
			// and found no changes have been made, so we can abort this update
			if ( bvmCurrent == null )
			{
				this . DetGrid . Refresh ( );
				return;
			}
			// This is now confirmed as being CHANGED DATA in the current row
			// So we proceed and update SQL Db's' and notify all open viewers as well
			int currow = this . DetGrid . SelectedIndex;
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
					SenderGuid = this . Tag . ToString ( ),
					RowCount = this . DetGrid . SelectedIndex
				} );
			EventControl . TriggerGlobalDataChanged ( this, new GlobalEventArgs
			{
				CallerType = "DETAILSDBVIEW",
				AccountType = "DETAILS",
				SenderGuid = this . Tag?.ToString ( )
			} );
		}

		#endregion DATA EDIT CONTROL METHODS

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

		#region HANDLERS for linkage checkboxes, inluding Thread montior
		static bool IsLinkActive ( bool ParentLinkTo )
		{
			return Flags . SqlDetViewer != null && ParentLinkTo == false;
		}

		static bool IsMultiLinkActive ( bool MultiParentLinkTo )
		{
			if ( Flags . SqlMultiViewer == null )
				return false;
			else
				return true;
		}

		private void LinkToMulti_Click ( object sender, RoutedEventArgs e )
		{
			bool reslt = false;

			if ( IsMultiLinkActive ( reslt ) == false )
			{
				LinkToMulti . IsEnabled = false;
				LinkToMulti . IsChecked = false;
				MultiParentViewer = null;
				LinktoMultiParent = false;
			}
			else
			{
				LinktoMultiParent = !LinktoMultiParent;
				if ( LinktoMultiParent )
				{
					LinkToMulti . IsChecked = true;
					LinktoMultiParent = true;
				}
				else
				{
					LinkToMulti . IsChecked = false;
					LinktoMultiParent = false;
				}
			}
		}
		/// <summary>
		/// Runs as a thread to monitor SqlDbviewer & Multiviewer availabilty
		/// and resets checkboxes as necessary  - thread delay is TWO seconds
		/// </summary>
		private void checkLinkages ( )
		{
			while ( true )
			{
				int AllLinks = 0;
				Thread . Sleep ( 2000 );

				bool reslt = false;
				if ( IsLinkActive ( reslt ) )
				{
					AllLinks++;
					Application . Current . Dispatcher . Invoke ( ( ) =>
					{
						ResetLinkages ( "LINKTOPARENT", true );
					} );
				}
				else
				{
					Application . Current . Dispatcher . Invoke ( ( ) =>
					{
						ResetLinkages ( "LINKTOPARENT", false );
					} );
				}

				if ( IsMultiLinkActive ( reslt ) == false )
				{
					Application . Current . Dispatcher . Invoke ( ( ) =>
					{
						ResetLinkages ( "MULTILINKTOPARENT", false );
					} );
				}
				else
				{
					AllLinks++;
					Application . Current . Dispatcher . Invoke ( ( ) =>
					{
						ResetLinkages ( "MULTILINKTOPARENT", true );
					} );
				}
				if ( AllLinks >= 1 )
					Application . Current . Dispatcher . Invoke ( ( ) =>
					{
						ResetLinkages ( "ALLLINKS", true );
					} );
				else
					Application . Current . Dispatcher . Invoke ( ( ) =>
					{
						ResetLinkages ( "ALLLINKS", false );
					} );

			}
		}
		private void ResetLinkages ( string linktype, bool value )
		{
			if ( linktype == "LINKTOPARENT" )
			{
				LinkToParent . IsEnabled = value;
				if ( value )
					SqlParentViewer = Flags . SqlDetViewer;
				else
				{
					LinktoParent = false;
					SqlParentViewer = null;
				}
			}
			if ( linktype == "MULTILINKTOPARENT" )
			{
				if ( value )
				{
					LinkToMulti . IsEnabled = value;
					MultiParentViewer = Flags . SqlMultiViewer;
				}
				else
				{
					LinkToMulti . IsEnabled = false;
					LinkToMulti . IsChecked = false;
					MultiParentViewer = null;
					LinktoMultiParent = false;
				}
			}
			if ( linktype == "ALLLINKS" && value )
				LinkRecords . IsEnabled = true;
			else
				LinkRecords . IsEnabled = false;
			#endregion HANDLERS for linkage checkboxes, inluding Thread montior

		}
		private void Window_MouseDown ( object sender, MouseButtonEventArgs e )
		{
			int x = 0;
		}

		private void DetGrid_MouseDoubleClick ( object sender, MouseButtonEventArgs e )
		{
			DetailsViewModel dvm = new DetailsViewModel ( );

			DataGrid datagrid = sender as DataGrid;
			Point aP = e . GetPosition ( datagrid );
			IInputElement obj = datagrid . InputHitTest ( aP );
			DependencyObject target = obj as DependencyObject;

			while ( target != null )
			{
				if ( target is DataGridRow )
				{
					var s = datagrid . SelectedItem;
					dvm = s as DetailsViewModel;
					_itemdata = dvm;
				}
				target = VisualTreeHelper . GetParent ( target );
			}
			ShowItemData ( sender, e );
		}

		private void ShowItemData ( object sender, MouseButtonEventArgs e )
		{
			DetailsViewModel dvm = new DetailsViewModel ( );

			DataGrid datagrid = sender as DataGrid;
			Point aP = e . GetPosition ( datagrid );
			IInputElement obj = datagrid . InputHitTest ( aP );
			DependencyObject target = obj as DependencyObject;

			while ( target != null )
			{
				if ( target is DataGridRow )
				{
					var s = datagrid . SelectedItem;
					dvm = s as DetailsViewModel;
					_itemdata = dvm;
				}
				target = VisualTreeHelper . GetParent ( target );
			}
			CreateVisualDetailsRecord ( 1 );
		}
		private string CreateVisualDetailsRecord ( int mode = 1 )
		{
			if ( IsLeftButtonDown && LeftClickinprogress == false)
			{
				LeftClickinprogress = true;
				string Output = "";
				if ( mode == 0 )
				{
					Output = "Details Db Record Data\n";
					Output += $"Customer #	: " + _itemdata . CustNo . ToString ( ) + "\n";
					Output += $"Account #	: " + _itemdata . BankNo . ToString ( ) + "\n";
					Output += $"A/c Type#	: " + _itemdata . AcType . ToString ( ) + "\n";
					Output += $"Balance		: " + _itemdata . Balance . ToString ( ) + "\n";
					Output += $"Interest %	: " + _itemdata . IntRate . ToString ( ) + "\n";
					Output += $"Opened		: " + _itemdata . ODate . ToString ( ) . Substring ( 0, 10 ) + "\n";
					Output += $"Closed		: " + _itemdata . CDate . ToString ( ) . Substring ( 0, 10 ) + "\n";
					MessageBox . Show ( $"{Output}", "Details Db Data View" );
				}
				if ( mode == 1 )
				{
					Output += _itemdata . CustNo . ToString ( ) + "\n";
					Output += _itemdata . BankNo . ToString ( ) + "\n";
					Output += _itemdata . AcType . ToString ( ) + "\n";
					Output += _itemdata . Balance . ToString ( ) + "\n";
					Output += _itemdata . IntRate . ToString ( ) + "\n";
					Output += _itemdata . ODate . ToString ( ) . Substring ( 0, 10 ) + "\n";
					Output += _itemdata . CDate . ToString ( ) . Substring ( 0, 10 );
					return Output;
				}
			}
			return "";
		}
		private void DetGrid_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			// Gotta make sure it is not anywhere in the Scrollbar we clicked on 
			if ( Utils . HitTestScrollBar ( sender, e ) )
				return;
			if ( Utils . HitTestHeaderBar ( sender, e ) )
				return;
			_startPoint = e . GetPosition ( null );
			// Make sure the left mouse button is pressed down so we are really moving a record
			if ( e . LeftButton == MouseButtonState . Pressed )
			{
				IsLeftButtonDown = true;
//				ShowItemData ( sender, e );
			}
		}
		private void DetGrid_PreviewMouseMove ( object sender, MouseEventArgs e )
		{
			Point mousePos = e . GetPosition ( null );
			Vector diff = _startPoint - mousePos;

			if ( e . LeftButton == MouseButtonState . Pressed &&
			    Math . Abs ( diff . X ) > SystemParameters . MinimumHorizontalDragDistance ||
			    Math . Abs ( diff . Y ) > SystemParameters . MinimumVerticalDragDistance )
			{
				if ( IsLeftButtonDown && e . LeftButton == MouseButtonState . Pressed )
				{
					if ( DetGrid . SelectedItem != null )
					{
						// We are dragging from the DETAILS grid
						//Working string version
						DetailsViewModel dvm = new DetailsViewModel ( );
						dvm = DetGrid . SelectedItem as DetailsViewModel;
						string str = GetExportRecords . CreateTextFromRecord ( null, dvm, null, true, false );
						string dataFormat = DataFormats . Text;
						DataObject dataObject = new DataObject ( dataFormat, str );
						System . Windows . DragDrop . DoDragDrop (
						DetGrid,
						dataObject,
						DragDropEffects . Move );
						IsLeftButtonDown = false;
						e . Handled = true;
					}
				}
			}
		}

		private void Minimize_click ( object sender, RoutedEventArgs e )
		{
			this . WindowState = WindowState . Normal;
		}

		private void DetGrid_DragEnter ( object sender, DragEventArgs e )
		{
			e . Effects = ( DragDropEffects ) DragDropEffects . Move;
		}

		private async void DettGrid_PreviewMouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{
			ContextMenu cm = this . FindResource ( "ContextMenu1" ) as ContextMenu;
			cm . PlacementTarget = this . DetGrid as DataGrid;
			cm . IsOpen = true;
		}

		private void ContextClose_Click ( object sender, RoutedEventArgs e )
		{
			Close ( );
		}

		private void ContextSettings_Click ( object sender, RoutedEventArgs e )
		{
			Setup setup = new Setup ( );
			setup . Show ( );
			setup . BringIntoView ( );
			setup . Topmost = true;
			this . Focus ( );
		}

		private void ContextDisplayJsonData_Click ( object sender, RoutedEventArgs e )
		{
			JsonSupport . CreateShowJsonText ( false, "DETAILS", DetViewerDbcollection );
		}

		private void ContextSave_Click ( object sender, RoutedEventArgs e )
		{

		}

		private async void ContextEdit_Click ( object sender, RoutedEventArgs e )
		{
			DetailsViewModel dvm = new DetailsViewModel ( );
			int currsel = 0;
			DataGridRow RowData = new DataGridRow ( );
			dvm = this . DetGrid . SelectedItem as DetailsViewModel;
			currsel = this . DetGrid . SelectedIndex;
			RowInfoPopup rip = new RowInfoPopup ( "DETAILS", DetGrid );
			rip . Topmost = true;
			rip . DataContext = RowData;
			rip . BringIntoView ( );
			rip . Focus ( );
			rip . ShowDialog ( );

			//If data has been changed, update everywhere
			// Update the row on return in case it has been changed
			if ( rip . IsDirty )
			{
				this . DetGrid . ItemsSource = null;
				this . DetGrid . Items . Clear ( );
				await DetCollection . LoadDet ( DetViewerDbcollection, 1, true );
				this . DetGrid . ItemsSource = DetviewerView;
				// Notify everyone else of the data change
				EventControl . TriggerViewerDataUpdated ( DetviewerView,
					new LoadedEventArgs
					{
						CallerType = "DETDBVIEW",
						CallerDb = "DETAILS",
						DataSource = DetviewerView,
						SenderGuid = this . Tag . ToString ( ),
						RowCount = this . DetGrid . SelectedIndex
					} );
			}
			else
				this . DetGrid . SelectedItem = RowData . Item;

			// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
			this . DetGrid . SelectedIndex = currsel;
			Count . Text = $"{this . DetGrid . SelectedIndex} / { this . DetGrid . Items . Count . ToString ( )}";
			// This is essential to get selection activated again
			this . DetGrid . Focus ( );
		}

		private void ContextShowJson_Click ( object sender, RoutedEventArgs e )
		{
			//============================================//
			//MENU ITEM 'Read and display JSON File'
			//============================================//
			string Output = "";
			this . Refresh ( );
			////We need to save current Collectionview as a Json (binary) data to disk
			//// this is the best way to save persistent data in Json format
			////using tmp folder for interim file that we will then display
			DetailsViewModel bvm = this . DetGrid . SelectedItem as DetailsViewModel;
			Output = JsonSupport . CreateShowJsonText ( true, "DETAILS", bvm, "DetailsViewModel" );
			MessageBox . Show ( Output, "Currently selected record in JSON format", MessageBoxButton . OK, MessageBoxImage . Information, MessageBoxResult . OK );
		}

		private void DetGrid_PreviewMouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{
			ContextMenu cm = this . FindResource ( "ContextMenu1" ) as ContextMenu;
			cm . PlacementTarget = this .
				DetGrid as DataGrid;
			cm . IsOpen = true;


		}

		private void DetGrid_PreviewMouseLeftButtonUp ( object sender, MouseButtonEventArgs e )
		{
			// Clear flag for auto data collection process via (CreateVisualDetailsRecord() call )
			LeftClickinprogress = false;
		}

		public  void changesize_Click ( object sender, RoutedEventArgs e )
		{
			Thickness t = new Thickness ( );
			// All get the RUNNING directory "debug or release"
			//string syspath = System . AppDomain . CurrentDomain . BaseDirectory;
			string syspath = Directory . GetCurrentDirectory ( );
			//syspath = Environment . CurrentDirectory;
			//syspath = Path . GetDirectoryName ( Assembly . GetExecutingAssembly ( ) . Location );
			//syspath = Application .
			//string applicationDirectory = (
			//    from assembly in AppDomain . CurrentDomain . GetAssemblies ( )
			//	where assembly . CodeBase . EndsWith ( ".exe" )
			//	select System . IO . Path . GetDirectoryName ( assembly . CodeBase . Replace ( "file:///", "" ) )
			//) . FirstOrDefault ( );

			//Read entry from App Configuration system
			syspath =Utils . ReadConfigSetting ( "AppRoot");
			if ( DetGrid . RowHeight == 32 )
			{
				DetGrid . RowHeight = 25;
				SizeChangeMenuItem . Header = "Larger Font";
				SizeChangeMenuItem . FontSize = 16;
				t . Top = 0;
				t . Bottom = 0;
				SizeChangeMenuItem . Margin = t;
				Brush br = Utils . GetDictionaryBrush ( "Black0" );
				SizeChangeMenuItem . Foreground = br;
				
				string path = @"/Views/magnify plus red.png";
				FontsizeIcon . Source = new BitmapImage ( new Uri ( path, UriKind . RelativeOrAbsolute ) );
				t . Top = 0;
				t . Bottom = 0;
				FontsizeIcon . Margin = t;
			}
			else
			{
				DetGrid . RowHeight = 32;
				SizeChangeMenuItem . Header = "Smaller Font";
				SizeChangeMenuItem . FontSize = 10;
				t . Top = (double)8;
				SizeChangeMenuItem . Margin = t;
				Brush br = Utils . GetDictionaryBrush ( "Red0" );
				SizeChangeMenuItem . Foreground = br;

				string path = @"/Views/magnify minus red.png";
				FontsizeIcon . Source = new BitmapImage ( new Uri ( path, UriKind.RelativeOrAbsolute));
				t . Top = -5;
//				t . Bottom = 5;
//				t . Right = 5;
				FontsizeIcon . Margin = t;
//				FontsizeIcon . Width = 30;


			}
		}
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


