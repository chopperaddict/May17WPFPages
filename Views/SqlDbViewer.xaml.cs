#define SHOWSQLERRORMESSAGEBOX
#define SHOWWINDOWDATA
#define ALLOWREFRESH
#define LINKVIEWTOEDIT

using System;
using System . Collections . Generic;
using System . ComponentModel;
using System . Data;
using System . Data . SqlClient;
using System . Diagnostics;
using System . Linq;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Input;
using System . Windows . Media;
using WPFPages . Properties;
using WPFPages . ViewModels;
using WPFPages . Views;

namespace WPFPages
{
	public partial class SqlDbViewer : Window, INotifyPropertyChanged
	{
		private ClockTower _tower;
		public SqlDbViewer ThisViewer = null;
		//		public static Dispatcher UiThread = Dispatcher . CurrentDispatcher;

		// Declare all 3 of the local Db pointers
		public BankCollection SqlBankcollection = null;
		public CustCollection SqlCustcollection = null;
		public DetCollection SqlDetcollection = null;
		public DetCollection DetDbcollection = null;// = new DetCollection ( );//. DetViewerDbcollection;

		#region CollectionView stuff
		// Get our personal Collection views of the Db
		private ICollectionView _SQLBankviewerView;
		private ICollectionView SQLBankviewerView
		{
			get { return _SQLBankviewerView; }
			set { _SQLBankviewerView = value; }
		}
		private ICollectionView _SQLCustviewerView;
		private ICollectionView SQLCustviewerView
		{
			get { return _SQLCustviewerView; }
			set { _SQLCustviewerView = value; }
		}
		private ICollectionView _SQLDetviewerView;
		private ICollectionView SQLDetViewerView
		{
			get { return _SQLDetviewerView; }
			set { _SQLDetviewerView = value; }
		}
		#endregion CollectionView stuff

		#region Delegate/Event declarations

		// used  for delegate passing exoeriment - on keyboard hand,ker only right now CTRL + F12
		public int DelegateSelection = 0;

		#endregion Delegate/Event declarations

		#region Global ViewModel declarations

		// SQL Data Setup
		public BankAccountViewModel bvm = MainWindow . bvm;
		public CustomerViewModel cvm = MainWindow . cvm;
		public DetailsViewModel dvm = MainWindow . dvm;

		#endregion Global ViewModel declarations

		#region Private declarations
		private DataGrid CurrentDataGrid = null;
		private int EditChangeType = 0;
		private int ViewerChangeType = 0;
		private EditDb edb;
		private bool key1 = false;
		public static bool RefreshInProgress = false;
		#endregion Private declarationss

		#region Class setup - General Declarations

		public static string CurrentInstanceDb = "";

		private string columnToFilterOn = "";
		private string filtervalue1 = "";
		private string filtervalue2 = "";
		private string operand = "";

		private string IsFiltered = "";
		private string FilterCommand = "";
		private string PrettyDetails = "";

		private bool IsViewerLoaded = false;
		private int LoadIndex = -1;
		public bool SqlUpdating = false;
		public bool FilterResult = false;
		public bool Triggered = false;
		public bool IsDirty = false;
		public bool LoadingDbData = false;
		// Filtering flags
		public bool BankFiltered = false;
		public bool CustFiltered = false;
		public bool DetFiltered = false;

		//		public static SqlDbViewer sqldbForm = null;

		#endregion Class setup - General Declarations


		#region STD PROPERTIES

		//instance Window handle
		public Window ThisWindow { get; set; }          // Current selectedIndex for each type of viewer data
		private int SavedBankRow { get; set; }
		private int SavedCustRow { get; set; }
		private int SavedDetRow { get; set; }
		public DataGrid EditDataGrid { get; set; }
		public dynamic CurrentDb { get; set; }


		#endregion STD PROPERTIES

		#region FULL PROPERTIES
		// FULL PROPERTIES
		//***************** store the record data for whatever account type's record is the currently selected item
		//so DbSelector can bind to it as well
		private BankAccountViewModel currentBankSelectedRecord;

		public BankAccountViewModel CurrentBankSelectedRecord
		{ get { return currentBankSelectedRecord; } set { currentBankSelectedRecord = value; } }

		private CustomerViewModel currentCustomerSelectedRecord;

		public CustomerViewModel CurrentCustomerSelectedRecord
		{ get { return currentCustomerSelectedRecord; } set { currentCustomerSelectedRecord = value; } }

		private DetailsViewModel currentDetailsSelectedRecord;

		public DetailsViewModel CurrentDetailsSelectedRecord
		{ get { return currentDetailsSelectedRecord; } set { currentDetailsSelectedRecord = value; } }

		#endregion FULL PROPERTIES

		public EventHandlers EventHandler = null;
		private bool SelectionhasChanged = false;

		//Variables used when a cell is edited to se if we need to update via SQL
		private object OriginalCellData = null;

		private string OriginalDataType = "";
		private int OrignalCellRow = 0;
		private int OriginalCellColumn = 0;
		private bool OnSelectionChangedInProgress = false;
		public DataGridController dgControl;

		//---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
		/// <summary>
		/// Used to keep track of currently selected row in GridViwer
		/// </summary>
		private int _selectedRow;

		public int SelectedRow
		{
			get { return _selectedRow; }
			set { _selectedRow = value; OnPropertyChanged ( SelectedRow . ToString ( ) ); }
		}

		public struct scrollData
		{
			public double Banktop { get; set; }
			public double Bankbottom { get; set; }
			public double BankVisible { get; set; }
			public double Custtop { get; set; }
			public double Custbottom { get; set; }
			public double CustVisible { get; set; }
			public double Dettop { get; set; }
			public double Detbottom { get; set; }
			public double DetVisible { get; set; }
		}
		public static scrollData ScrollData = new scrollData ( );

		public DataGrid CurrentGrid;

		// Used by DbEdit Delegate notifier ONLY
		public static DataGrid CurrentActiveGrid;

		public static bool RemoteChangeActive = false;



		/// <summary>
		///  A Delegate method we send in a call to SqlDbViewer to have it reset its grid.selectedIndex to our index
		///  </summary>
		/// <param name="CurrentDb"></param>
		/// <param name="Bankno"></param>
		/// <param name="Custno"></param>
		public void resetViewerIndex ( int Bankno, int Custno )
		{
			int rec = 0;
			DataGrid Grid = null;
			if ( CurrentDb == "BANKACCOUNT" )
				Grid = this . BankGrid;
			else if ( CurrentDb == "CUSTOMER" )
				Grid = this . CustomerGrid;
			else if ( CurrentDb == "DETAILS" )
				Grid = this . DetailsGrid;
			//			Grid . SelectedIndex = -1;
			Grid . UnselectAll ( );
			rec = Utils . FindMatchingRecord ( Custno . ToString ( ), Bankno . ToString ( ), Grid, CurrentInstanceDb );
			Grid . SelectedIndex = rec != -1 ? rec : 0;
			Grid . SelectedItem = rec != -1 ? rec : 0;
			Utils . SetUpGridSelection ( Grid, rec != -1 ? rec : 0 );
			return;
		}

		#region SqlDbViewer Class Constructors
		//*************************************************************************************************************//
		public SqlDbViewer ( bool x )
		{
			// dummy constructor fpr pre=loading
		}

		//Dummy Constructor for Event handlers
		//*************************************************************************************************************//
		public SqlDbViewer ( char x )
		{
			// dummy constructor to let others get a pointer
			InitializeComponent ( );
			ThisViewer = this;
			Flags . CurrentSqlViewer = this;
			this . Show ( );
			WaitMessage . Visibility = Visibility . Visible;
			WaitMessage . Refresh ( );
		}

		//*************************************************************************************************************//
		private void _tower_Chime ( )
		{
			//			Debug . WriteLine ($"Chime event called in Method");
		}

		//*************************************************************************************************************//
		public SqlDbViewer ( )
		{
			InitializeComponent ( );

			//			sqldbForm = this;
			ThisViewer = this;
			SubscribeToEvents ( );
			WaitMessage . Visibility = Visibility . Visible;
			WaitMessage . Refresh ( );

			dgControl = new DataGridController ( );
			if ( Flags . CurrentSqlViewer != this )
				Flags . CurrentSqlViewer = this;

			// just used for Tower Test of events
			// assign handler to delegate
			//			NotifyViewer SendCommand = DbSelector . MyNotification;
			//			Utils . GetWindowHandles ( );
			// Handle window dragging
			this . MouseDown += delegate { DoDragMove ( ); };
			this . Show ( );
		}



		//*********************************************************************************************************//
		/// <summary>
		/// MAIN STARTUP CALL from DbSelector
		/// </summary>
		/// <param name="caller"></param>
		/// <returns></returns>
		//*************************************************************************************************************//
		public SqlDbViewer ( string caller, object Collection )
		{
			IsViewerLoaded = false;
			InitializeComponent ( );
			CurrentDb = caller;
			this . BankGrid . Visibility = Visibility . Collapsed;
			this . CustomerGrid . Visibility = Visibility . Collapsed;
			this . DetailsGrid . Visibility = Visibility . Collapsed;
			this . Show ( );
			WaitMessage . Visibility = Visibility . Visible;
			WaitMessage . Refresh ( );
			this . UpdateLayout ( );
			this . Refresh ( );

		}

		//*************************************************************************************************************//
		private async void OnWindowLoaded ( object sender, RoutedEventArgs e )
		{
			// THIS IS WHERE WE NEED TO SET THIS FLAG
			Flags . SqlViewerIsLoading = true;

			ThisViewer = this;
			this . Show ( );
			BringIntoView ( );
			this . Refresh ( );

			await LoadData ( CurrentDb );
			//DetCollection.go();
			// Handle window dragging
			this . MouseDown += delegate { DoDragMove ( ); };
			RefreshBtn . IsEnabled = true;
			//This DOES call handler in DbSelector !!
			//			sqldbForm = this;
			dgControl = new DataGridController ( );
			Flags . CurrentSqlViewer = this;

			// Test of Func IsRecordMatched<object, object, bool> declared in Utils
			// where the objects are Db types so we can compere Db column data
			//			Debug . WriteLine ( $"{Utils . IsRecordMatched ( this . BankGrid . SelectedItem, this . CustomerGrid . SelectedItem )} " );

			//This is the EventHandler declared  in THIS FILE


			LoadedEventArgs ex = new LoadedEventArgs ( );

			ex . CallerDb = CurrentDb;
			//this . BankGrid . Visibility = Visibility . Collapsed;
			//this . CustomerGrid . Visibility = Visibility . Collapsed;
			//this . DetailsGrid . Visibility = Visibility . Collapsed;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				CurrentInstanceDb = "BANKACCOUNT";
				//				WaitMessage . Visibility = Visibility . Collapsed;
				//				BankGrid . Visibility = Visibility . Visible;
				//				this . Refresh ( );
				//				this . UpdateLayout ( );
				// Set up the various globalflags we use to control activity
				Flags . SqlBankGrid = this . BankGrid;
				Flags . SqlBankViewer = this;
				Flags . ActiveSqlViewer = this;
				Flags . ActiveSqlGrid = this . BankGrid;
				MainWindow . gv . SqlBankViewer = ( SqlDbViewer ) this;
				Flags . SetGridviewControlFlags ( this, this . BankGrid );

				//if ( this . BankGrid . Items . Count > 0 )
				//{
				//	Utils . SetUpGridSelection ( this . BankGrid, this . BankGrid . SelectedIndex );
				//	//					Utils . ScrollRecordIntoView ( this . BankGrid, this . BankGrid . SelectedIndex );
				//	Utils . GridInitialSetup ( BankGrid, 0 );
				//	SetScrollVariables ( BankGrid );
				//	Count . Text = this . BankGrid . Items . Count . ToString ( );
				//}
				//SetButtonColor ( RefreshBtn, "BLUE" );
				//ParseButtonText ( true );
				CurrentActiveGrid = this . BankGrid;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				// Set up the various globalflags we use to control activity
				CurrentInstanceDb = "CUSTOMER";
				Flags . SqlCustGrid = this . CustomerGrid;
				Flags . SqlCustViewer = this;
				Flags . ActiveSqlViewer = this;
				Flags . ActiveSqlGrid = this . CustomerGrid;
				MainWindow . gv . SqlCustViewer = ( SqlDbViewer ) this;
				Flags . SetGridviewControlFlags ( this, this . CustomerGrid );

				//if ( this . CustomerGrid . Items . Count > 0 )
				//{
				//	Utils . SetUpGridSelection ( this . CustomerGrid, this . CustomerGrid . SelectedIndex );
				//	//					Utils . ScrollRecordIntoView ( this . CustomerGrid, this . CustomerGrid . SelectedIndex );
				//	Utils . GridInitialSetup ( CustomerGrid, 0 );
				//	SetScrollVariables ( CustomerGrid );
				//	Count . Text = this . CustomerGrid . Items . Count . ToString ( );
				//}
				//SetButtonColor ( RefreshBtn, "YELLOW" );
				//ParseButtonText ( true );
				CurrentActiveGrid = this . CustomerGrid;

			}
			else if ( CurrentDb == "DETAILS" )
			{
				// Set up the various globalflags we use to control activity
				CurrentInstanceDb = "DETAILS";
				Flags . SqlDetGrid = this . DetailsGrid;
				Flags . SqlDetViewer = this;
				Flags . ActiveSqlViewer = this;
				Flags . ActiveSqlGrid = this . DetailsGrid;
				MainWindow . gv . SqlDetViewer = ( SqlDbViewer ) this;
				Flags . SetGridviewControlFlags ( this, this . DetailsGrid );

				//if ( this . DetailsGrid . Items . Count > 0 )
				//{
				//	Utils . SetUpGridSelection ( this . DetailsGrid, this . DetailsGrid . SelectedIndex );
				//	//					Utils . ScrollRecordIntoView ( this . DetailsGrid, this . DetailsGrid . SelectedIndex );
				//	Utils . GridInitialSetup ( DetailsGrid, 0 );
				//	SetScrollVariables ( DetailsGrid );
				//	Count . Text = this . DetailsGrid . Items . Count . ToString ( );
				//}
				//SetButtonColor ( RefreshBtn, "GREEN" );
				//ParseButtonText ( true );
				CurrentActiveGrid = this . DetailsGrid;

			}

			// Grab a Guid for this viewer window early on
			if ( Flags . CurrentSqlViewer == null )
			{
				Flags . CurrentSqlViewer = this;
			}
			//This is the ONE & ONLY place we should set the Guid
			Flags . CurrentSqlViewer . Tag = Guid . NewGuid ( );
			MainWindow . gv . SqlViewerGuid = ( Guid ) Flags . CurrentSqlViewer . Tag;
			IsViewerLoaded = true;
			//			ParseButtonText ( true );
			// clear global "loading new window" flag
			Flags . SqlViewerIsLoading = false;
			if ( Flags . LinkviewerRecords )
				LinkRecords . IsChecked = true;
			Mouse . OverrideCursor = Cursors . Arrow;
		}
		/// <summary>
		/// Load the relevant data from SQl Db
		/// </summary>
		/// <param name="caller"></param>
		/// <returns></returns>
		//*************************************************************************************************************//
		private async Task LoadData ( string caller )
		{


			switch ( caller )
			{
				case "BANKACCOUNT":
					CurrentDb = "BANKACCOUNT";
					//subscribing to data  loading and changed event !!!
					SubscribeToEvents ( );
					Flags . ActiveSqlViewer = this;
					Mouse . OverrideCursor = Cursors . Wait;
					Debug . WriteLine ( "SQLDBVIEWER : Loading Bank Data" );
					await BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );//.ConfigureAwait(false);
					break;

				case "CUSTOMER":
					CurrentDb = "CUSTOMER";
					//subscribing to data  loading and changed event !!!
					SubscribeToEvents ( ); Flags . ActiveSqlViewer = this;
					Mouse . OverrideCursor = Cursors . Wait;
					Debug . WriteLine ( "SQLDBVIEWER : Loading Customer Data" );
					await CustCollection . LoadCust ( SqlCustcollection, "SQLDBVIEWER", 1, true );
					break;

				case "DETAILS":
					CurrentDb = "DETAILS";
					//subscribing to data  loading and changed event !!!
					SubscribeToEvents ( ); Flags . ActiveSqlViewer = this;
					Mouse . OverrideCursor = Cursors . Wait;
					Debug . WriteLine ( "SQLDBVIEWER : Loading Details Data" );
					await DetailCollection . LoadDet ( SqlDetcollection, "SQLDBVIEWER", 1, true );
					break;

				default:
					break;
			}
			ThisWindow = this;
			Mouse . OverrideCursor = Cursors . Arrow;
		}

		#endregion SqlDbViewer Class Constructors

		#region *** CRUCIAL Methods - Event CALLBACK for data loading
		/// <summary>
		/// Generic method that handles the subscribing of this window to all the relevant EVENTS
		/// required for control as we work within the Window.
		/// </summary>
		//*************************************************************************************************************//
		private void SubscribeToEvents ( )
		{
			//Subscribe to our clever Collections data system
			if ( CurrentDb == "BANKACCOUNT" )
				EventControl . BankDataLoaded += EventControl_BankDataLoaded;
			else if ( CurrentDb == "CUSTOMER" )
				EventControl . CustDataLoaded += EventControl_CustDataLoaded;
			else if ( CurrentDb == "DETAILS" )
				EventControl . DetDataLoaded += EventControl_DetDataLoaded;


			// An EditDb Viewer has updated a record  notification handler
			EventControl . EditDbDataUpdated += EventControl_EditDbDataUpdated;
			// Another SQL viewer has updated a record  notification handler
			EventControl . ViewerDataUpdated += EventControl_SqlDataUpdated;
			EventControl . EditDbDataUpdated += EventControl_SqlDataUpdated;
			EventControl . MultiViewerDataUpdated += EventControl_SqlDataUpdated;


			// An EditDb has changed the current index
			EventControl . EditIndexChanged += EventControl_EditIndexChanged;
			// A Multiviewer has changed the current index
			EventControl . MultiViewerIndexChanged += EventControl_EditIndexChanged;
			// Another SqlDbviewer has changed the current index
			EventControl . ViewerIndexChanged += EventControl_EditIndexChanged;      // Callback in THIS FILE

			//Subscribe to the notifier EVENT so we know when a record is deleted from one of the grids
			EventControl . RecordDeleted += OnDeletion;

		}

		/// <summary>
		/// Handler for All types of Sql data having just been Loaded/Reload
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//*************************************************************************************************************//
		public async void SqlDbViewer_BankDataLoaded ( object sender, LoadedEventArgs e )
		{
			bool result = false;

			if ( e . CallerDb != "SQLDBVIEWER" ) return;
			Console . WriteLine ( $"\nDEBUG : SQL DATA LOADED : Entered SqlDbviewer_BankDataLoaded... " );

			result = await UpdateSelectedDb ( sender, e );
			if ( result == false )
			{
				if ( e . CallerDb == "BANKACCOUNT" )
				{
					this . BankGrid . ItemsSource = null;
					this . BankGrid . ItemsSource = e . DataSource as BankCollection;
					Console . WriteLine ( $"\nDEBUG RELOADING : Entered SqlDbviewer_BankDataLoaded : Bank data triggered by BankCollection\nData should be loaded by here ? {BankGrid . Items . Count}" );
					ParseButtonText ( true );
					Count . Text = this . BankGrid . Items . Count . ToString ( );
				}
				else if ( e . CallerDb == "CUSTOMER" )
				{
					this . CustomerGrid . ItemsSource = null;
					this . CustomerGrid . ItemsSource = SqlCustcollection;
					Console . WriteLine ( $"\nDEBUG RELOADING : Entered SqlDbviewer_BankDataLoaded : Customer data triggered by BankCollection\nData should be loaded by here ? {CustomerGrid . Items . Count}" );
					ParseButtonText ( true );
					Count . Text = this . CustomerGrid . Items . Count . ToString ( );
				}
				else if ( e . CallerDb == "DETAILS" )
				{
					this . DetailsGrid . ItemsSource = null;
					this . DetailsGrid . ItemsSource = SqlDetcollection;
					Console . WriteLine ( $"\nDEBUG RELOADING : Entered SqlDbviewer_BankDataLoaded : Details data triggered by BankCollection\nData should be loaded by here ? {DetailsGrid . Items . Count}" );
					ParseButtonText ( true );
					Count . Text = this . DetailsGrid . Items . Count . ToString ( );
				}
			}
			else
			{
				Console . WriteLine ( $"\nDEBUG LOADING : Exiting SqlDbviewer_BankDataLoaded... RESULT == TRUE = FAILURE !! " );
			}
			return;
		}

		//*************************************************************************************************************//
		private async Task<bool> UpdateSelectedDb ( object sender, LoadedEventArgs e )
		{
			//*****************************************************************//
			// This now receives the DATA in SENDER, so we can load it directly in here (ONLY)
			//*****************************************************************//
			BankCollection bc = null;
			CustCollection cc = null;
			DetCollection dc = null;
			int selectedIndex = e . CurrSelection != -1 ? e . CurrSelection : 0;

			if ( Flags . CurrentSqlViewer == null ) return false;
			if ( e . DataSource == null )
			{
				Debug . WriteLine ( $"\n*** NULL DATA returned by SQL Data  load call - Check Exceptions ***\n" );
				return false;
			}

			if ( e . CallerDb == "BANKACCOUNT" )
			{
				bc = sender as BankCollection;
				if ( bc == null ) return false;

				if ( bc . Count == 0 ) return false;

				if ( SqlBankcollection . Count != bc . Count )
					Debug . WriteLine ( $"BANKACCOUNT : in SqlDbViewer.DataLoaded() method - \nBankcollection has {SqlBankcollection . Count} records.\nsender has {bc . Count} records  returned by Sql Query" );
				if ( SqlBankcollection . Count == 0 )
					return false;
			}
			else if ( e . CallerDb == "CUSTOMER" )
			{
				cc = sender as CustCollection;
				if ( cc == null ) return false;

				if ( cc . Count == 0 ) return false;
				if ( SqlCustcollection . Count != cc . Count )
					Debug . WriteLine ( $"CUSTOMER: in SqlDbViewer.DataLoaded() method - \nCustcollection has {SqlCustcollection . Count} records.\nsender has {cc . Count} records  returned by Sql Query" );
				if ( SqlCustcollection . Count == 0 )
					return false;
			}
			else if ( e . CallerDb == "DETAILS" )
			{
				dc = sender as DetCollection;
				if ( dc == null ) return false;

				if ( dc . Count == 0 ) return false;
				if ( SqlDetcollection . Count != dc . Count )
					Debug . WriteLine ( $"DETAILS : in SqlDbViewer.DataLoaded() method - \nDetcollection has {SqlDetcollection . Count} records.\nsender has {dc . Count} records  returned by Sql Query" );
				if ( SqlDetcollection . Count == 0 )
					return false;
			}

			string s = e . DataSource . ToString ( );
			int count = 0;
			int length = s . Length;
			//			int CurrentIndex = 0;
			string shortstring = s . Substring ( 15, length - 15 );
			if ( e . CallerDb == "BANKACCOUNT" )
			{
				try
				{
					double topsaved = Flags . TopVisibleBankGridRow;
					double bottomsaved = Flags . BottomVisibleBankGridRow;
					double viewport = Flags . ViewPortHeight;
					Debug . WriteLine ( $"Variables in use : \n" +
					$"topsaved		: {topsaved}\n" +
					$"bottomsaved	: {bottomsaved}\n" +
					$"viewport		: {viewport}\n" );

					CurrentDataGrid = BankGrid;
					//Reset our grids source data
					CurrentDataGrid . ItemsSource = null;
					CurrentDataGrid . ItemsSource = SqlBankcollection;

					var data = BankGrid . SelectedItem as BankAccountViewModel;
					StatusBar . Text = $"Data reloaded due to external changes in {CurrentDb} Viewer for Bank # {data?.CustNo}, Bank A/C # {data?.BankNo}";

					// this is the critical code to make it work
					ScrollViewer scroll = GetScrollViewer ( BankGrid ) as ScrollViewer;
					if ( scroll == null ) return false;
					scroll . ScrollToVerticalOffset ( Flags . TopVisibleBankGridRow );
					Application . Current . Dispatcher . Invoke ( ( ) =>
					{
						scroll . ScrollToVerticalOffset ( Flags . TopVisibleBankGridRow );
					} );

					RefreshInProgress = false;
					if ( sender == null )
						Debug . WriteLine ( $"*** ERROR *** At End of data load, sender is NULL ????" );
					//					PrintCurrentviewportdata ( BankGrid );
					Flags . ActiveSqlViewer = this;
					if ( Flags . CurrentEditDbViewer != null )
					{
						// Notify currently open DbEbit as
						Thread . Sleep ( 500 );
						Flags . CurrentEditDbViewer . UpdateOnExternalChange ( );
					}
					ParseButtonText ( true );
					Count . Text = this . BankGrid . Items . Count . ToString ( );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"\nDETAILS : SqlDbViewer ERROR in processing DataLoaded EVENT" );
					Debug . WriteLine ( $"DETAILS : {ex . Message} - {ex . Data}" );
					return false;
				}
			}
			else if ( e . CallerDb == "CUSTOMER" )
			{
				try
				{
					double topsaved = Flags . TopVisibleCustGridRow;
					double bottomsaved = Flags . BottomVisibleCustGridRow;
					double viewport = Flags . ViewPortHeight;
					Debug . WriteLine ( $"Variables in use : \n" +
					$"topsaved		: {topsaved}\n" +
					$"bottomsaved	: {bottomsaved}\n" +
					$"viewport		: {viewport}\n" );

					CurrentDataGrid = CustomerGrid;
					//Reset our grids source data
					CurrentDataGrid . ItemsSource = null;
					CurrentDataGrid . ItemsSource = SqlCustcollection;

					var data = CustomerGrid . SelectedItem as CustomerViewModel;
					StatusBar . Text = $"Data reloaded due to external changes in {CurrentDb} Viewer for Customer # {data?.CustNo}, Bank A/C # {data?.BankNo}";

					// this is the critical code to make it work
					ScrollViewer scroll = GetScrollViewer ( CustomerGrid ) as ScrollViewer;
					if ( scroll == null ) return false;
					scroll . ScrollToVerticalOffset ( Flags . TopVisibleCustGridRow );
					Application . Current . Dispatcher . Invoke ( ( ) =>
					{
						scroll . ScrollToVerticalOffset ( Flags . TopVisibleCustGridRow );
					} );

					RefreshInProgress = false;
					if ( sender == null )
						Debug . WriteLine ( $"*** ERROR *** At End of data load, sender is NULL ????" );
					Flags . ActiveSqlViewer = this;

					if ( Flags . CurrentEditDbViewer != null )
					{
						// Notify currently open DbEbit as
						Thread . Sleep ( 500 );
						Flags . CurrentEditDbViewer . UpdateOnExternalChange ( );
					}
					ParseButtonText ( true );
					Count . Text = this . CustomerGrid . Items . Count . ToString ( );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"\nDETAILS : SqlDbViewer ERROR in processing DataLoaded EVENT" );
					Debug . WriteLine ( $"DETAILS : {ex . Message} - {ex . Data}" );
					return false;
				}
			}
			else if ( e . CallerDb == "DETAILS" )
			{
				try
				{
					double topsaved = Flags . TopVisibleDetGridRow;
					double bottomsaved = Flags . BottomVisibleDetGridRow;
					double viewport = Flags . ViewPortHeight;

					CurrentDataGrid = DetailsGrid;
					//Reset our grids source data
					CurrentDataGrid . ItemsSource = null;
					CurrentDataGrid . ItemsSource = SqlDetcollection;

					var data = DetailsGrid . SelectedItem as DetailsViewModel;
					StatusBar . Text = $"Data reloaded due to external changes in {CurrentDb} Viewer for Bank # {data?.CustNo}, Bank A/C # {data?.BankNo}";

					// this is the critical code to make it work
					ScrollViewer scroll = new ScrollViewer ( );
					scroll = GetScrollViewer ( DetailsGrid ) as ScrollViewer;
					if ( scroll == null ) return false;
					scroll . ScrollToVerticalOffset ( topsaved );
					Application . Current . Dispatcher . Invoke ( ( ) =>
					{
						scroll . ScrollToVerticalOffset ( topsaved );
					} );
					scroll . UpdateLayout ( );

					RefreshInProgress = false;
					if ( sender == null )
						Debug . WriteLine ( $"*** ERROR *** At End of data load, sender is NULL ????" );
					//					PrintCurrentviewportdata ( DetailsGrid );
					if ( Flags . IsMultiMode )
					{
						StatusBar . Text = $"Data is filtered on only Customers with multiple Bank A/c's : Total Accounts = {this . DetailsGrid . Items . Count}";
					}
					Flags . ActiveSqlViewer = this;

					if ( Flags . CurrentEditDbViewer != null )
					{
						// Notify currently open DbEbit as
						Thread . Sleep ( 500 );
						Flags . CurrentEditDbViewer . UpdateOnExternalChange ( );
					}
					ParseButtonText ( true );
					Count . Text = this . DetailsGrid . Items . Count . ToString ( );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"\nDETAILS : SqlDbViewer ERROR in processing DataLoaded EVENT" );
					Debug . WriteLine ( $"DETAILS : {ex . Message} - {ex . Data}" );
					return false;
				}
				ParseButtonText ( true );
			}
			if ( Flags . IsFiltered )
			{
				Filters . Content = "Show All";

				// how te reset a control's color from code
				ControlTemplate ctmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGreen" );
				Filters . Template = ctmp;
				Brush brs = Utils . GetDictionaryBrush ( "HeaderBrushGreen" );
				Filters . Background = brs;
				StatusBar . Text = $"The Data displayed is Filtered on {Flags . FilterCommand}";
				Flags . FilterCommand = "";
			}
			else
			{
				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				Filters . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
				Filters . Background = br;
			}
			//Ensure our current row is selected based on the row passed to us fro Db Load system
			CurrentDataGrid . SelectedIndex = selectedIndex;
			Mouse . OverrideCursor = Cursors . Arrow;
			return true;
		}
		//*************************************************************************************************************//

		public static DependencyObject GetScrollViewer ( DependencyObject o )
		{
			// Return the DependencyObject if it is a ScrollViewer
			if ( o is ScrollViewer )
			{ return o; }

			for ( int i = 0 ; i < VisualTreeHelper . GetChildrenCount ( o ) ; i++ )
			{
				var child = VisualTreeHelper . GetChild ( o, i );

				var result = GetScrollViewer ( child );
				if ( result == null )
				{
					continue;
				}
				else
				{
					return result;
				}
			}
			return null;
		}

		/// <summary>
		/// Event handler Called whenever the index changes in a viewr, inlcuding this one
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//*************************************************************************************************************//
		private void EventControl_EditIndexChanged ( object sender, IndexChangedArgs e )
		{
			bool Proceed = false;
			string SearchBankno = e . Bankno;
			string SearchCustno = e . Custno;
			// EditDb (oranother viewer if Flags.LinkviewerRecords is set has triggered the record index change, so we need ot update our grid index
			Debug . WriteLine ( $"SQLDBVIEWER : Grid SelectedIndex changed event notification received from a [{e . Sender}] DataGrd. in EventControl_EditIndexChanged(824)" );

			if ( LoadingDbData ) {
				LoadingDbData = false;
				return;
			}

			// If we triggered it, dont bother
			if ( e . SenderId != "SQLDBVIEWER" && e . Senderviewer == this )
				return;

			// dont bother if we are just loading data
//			if ( LoadingDbData ) return;

			if (
				( sender == Flags . BankEditDb && CurrentDb == "BANKACCOUNT" )
				|| ( sender == Flags . CustEditDb && CurrentDb == "CUSTOMER" )
				|| ( sender == Flags . DetEditDb && CurrentDb == "DETAILS" ) )
			{
				// It is an EditDb   changing the Index, so let it update it's own parent Viewer
				Proceed = true;
			}
			else
			{
				if ( Flags . LinkviewerRecords == false )
				{
					if ( Flags . SqlViewerIndexIsChanging == false )
						return;
				}
				else
					Proceed = true;
			}
			if ( Proceed )
			{
				//Handle index change made in another window
				//We can now actually FIND the correct record to be highlighted
				// Rather than just using selectedIndex
				Triggered = true;

				// These tests make sure we do not update ourselves (if we instigated the index changed)
				// They look complicated, but all work well (so far = 2/6/21) 
				// and NO LONGER iterates in endless times
				if ( ( this . BankGrid . Items . Count > 0 && BankFiltered == false )              // its us that triggered it
					|| ( e . SenderId == "SQLDBSERVER" && CurrentDb == "BANKACCOUNT" ) )		// Loading Db from Sql request
				{
					if ( e . SenderId != "SQLDBSERVER" && CurrentDb == "BANKACCOUNT" && this.BankGrid != e.dGrid)	//Its a remote viewer that has made the index change
					{
						// All clear, so we can go ahead and update without effecting other viewers
						int rec = Utils . FindMatchingRecord ( SearchCustno, SearchBankno, this . BankGrid, CurrentDb );
						Debug . WriteLine ( $"SQLDBVIEWER : Resetting {CurrentDb} Grid SelectedIndex event to [{rec}] in EventControl_EditIndexChanged(859)" );
						this . BankGrid . SelectedIndex = rec != -1 ? rec : 0;
						this . BankGrid . SelectedItem = rec != -1 ? rec : 0;
						Utils . SetUpGridSelection ( this . BankGrid, rec != -1 ? rec : 0 );
					}
				}
				else if ( ( this . CustomerGrid . Items . Count > 0 && BankFiltered == false )          // its us that triggered it
					|| ( e . SenderId == "SQLDBSERVER" && CurrentDb == "CUSTOMER" ) )               // Loading Db from Sql request
				{
					if ( e . SenderId != "SQLDBSERVER" && CurrentDb == "CUSTOMER" && this . CustomerGrid != e . dGrid )     //Its a remote viewer that has made the index change
					{
						// All clear, so we can go ahead and update without effecting other viewers
						int rec = Utils . FindMatchingRecord ( SearchCustno, SearchBankno, this . CustomerGrid, CurrentDb );
						Debug . WriteLine ( $"SQLDBVIEWER : Resetting {CurrentDb} Grid SelectedIndex event to [{rec}] in EventControl_EditIndexChanged(869)" );
						this . CustomerGrid . SelectedIndex = rec != -1 ? rec : 0;
						this . CustomerGrid . SelectedItem = rec != -1 ? rec : 0;
						Utils . SetUpGridSelection ( this . CustomerGrid, rec != -1 ? rec : 0 );
					}
				}
				else if ( ( this . DetailsGrid . Items . Count > 0 && BankFiltered == false )                   // its us that triggered it
					|| ( e . SenderId == "SQLDBSERVER" && CurrentDb == "DETAILS" ) )                // Loading Db from Sql request
				{
					if ( e . SenderId != "SQLDBSERVER" && CurrentDb == "DETAILS" && this . DetailsGrid != e . dGrid )       //Its a remote viewer that has made the index change
					{
						// All clear, so we can go ahead and update without effecting other viewers
						int rec = Utils . FindMatchingRecord ( SearchCustno, SearchBankno, this . DetailsGrid, CurrentDb );
						Debug . WriteLine ( $"SQLDBVIEWER : Resetting {CurrentDb} Grid SelectedIndex event to [{rec}] in EventControl_EditIndexChanged(879)" );
						DetailsGrid . SelectedIndex = rec >= 0 ? rec : 0;
						DetailsGrid . SelectedItem = rec != -1 ? rec : 0;
						Utils . SetUpGridSelection ( this . DetailsGrid, rec );
					}
				}
			}
		}

		/// <summary>
		/// Method to handle callback of DATAUPDATED when any of the 3  Sql viewer types Updates a row of data, (including ourselves)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//*************************************************************************************************************//
		private async void EventControl_SqlDataUpdated ( object sender, LoadedEventArgs e )
		{
			Debug . WriteLine ( $"SQLDBVIEWER : Data updated event notification received successfully. in EventControl_SqlDataUpdated(919)" );

			// Handles ViewerDataChanged Event notification
			if ( e . CallerType == "SQLDBVIEWER" ) return;

			if ( CurrentDb == "BANKACCOUNT" && e . CallerDb != "BANKACCOUNT" )
			{
				// its not for us, so dont bother
				return;
			}
			if ( CurrentDb == "CUSTOMER" && e . CallerDb != "CUSTOMER" )
			{
				// its not for us, so dont bother
				return;
			}
			if ( CurrentDb == "DETAILS" && e . CallerDb != "DETAILS" )
			{
				// its not for us, so dont bother
				return;
			}

			// if we reach here, we need to update our grid, so reload the data
			// And then wait for the callback
			if ( CurrentDb == "BANKACCOUNT" )
			{
				// Save our current position
				SavedBankRow = this . BankGrid . SelectedIndex;
				await BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );
				Console . WriteLine ( $"\nDEBUG RELOADING : Entered SQLDBVIEWER EventControl_SqlDataUpdated : Bank data triggered by BankCollection\nData is being loaded here ?\n " );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				// Save our current position
				SavedCustRow = this . CustomerGrid . SelectedIndex;
				await CustCollection . LoadCust ( SqlCustcollection, "SQLDBVIEWER", 1, true );
				Console . WriteLine ( $"\nDEBUG RELOADING : Entered SQLDBVIEWER EventControl_SqlDataUpdated : Customer data triggered by BankCollection\nData is being loaded here ?\n " );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				// Save our current position
				SavedDetRow = this . DetailsGrid . SelectedIndex;
				await DetailCollection . LoadDet ( SqlDetcollection, "SQLDBVIEWER", 1, true );
				Console . WriteLine ( $"\nDEBUG RELOADING : Entered SQLDBVIEWER EventControl_SqlDataUpdated : Details data triggered by BankCollection\nData is being loaded here ?\n " );
			}
		}
		private async void EventControl_BankDataLoaded ( object sender, LoadedEventArgs e )
		{
			// Event handler for BankDataLoaded
			if ( e . CallerDb != "SQLDBVIEWER" ) return;
			if ( e . CallerType != "SQLSERVER" ) return;
			if ( e . DataSource == null ) return;

			LoadingDbData = true;

			SqlBankcollection = e . DataSource as BankCollection;
			// Get our personal Collection view of the Db
			_SQLBankviewerView = CollectionViewSource . GetDefaultView ( SqlBankcollection );
			_SQLBankviewerView . Refresh ( );
			this . BankGrid . ItemsSource = _SQLBankviewerView;
			this . BankGrid . SelectedIndex = 0;
			this . BankGrid . SelectedItem = 0;
			BankGrid . UpdateLayout ( );
			this . BankGrid . Refresh ( );
			WaitMessage . Visibility = Visibility . Collapsed;
			BankGrid . Visibility = Visibility . Visible;
			BankGrid . Refresh ( );
			ParseButtonText ( false );
			Count . Text = this . BankGrid . Items . Count . ToString ( );
			Debug . WriteLine ( "SQLDBVIEWER : Bank Data fully loaded" );
		}
		private async void EventControl_CustDataLoaded ( object sender, LoadedEventArgs e )
		{
			if ( e . CallerDb != "SQLDBVIEWER" ) return;
			if ( e . CallerType != "SQLSERVER" ) return;

			LoadingDbData = true;

			// Event handler for CustDataLoaded
			if ( e . DataSource == null ) return;//|| this . CustomerGrid . Items . Count > 0 ) return;
			SqlCustcollection = e . DataSource as CustCollection;
			// Get our personal Collection view of the Db
			// Get our personal Collection view of the Db
			_SQLCustviewerView = CollectionViewSource . GetDefaultView ( SqlCustcollection );
			_SQLCustviewerView . Refresh ( );
			this . CustomerGrid . ItemsSource = _SQLCustviewerView;
			this . CustomerGrid . SelectedIndex = 0;
			this . CustomerGrid . SelectedItem = 0;
			this . CustomerGrid . UpdateLayout ( );
			this . CustomerGrid . Refresh ( );
			WaitMessage . Visibility = Visibility . Collapsed;
			this . CustomerGrid . Visibility = Visibility . Visible;
			this . CustomerGrid . Refresh ( );
			ParseButtonText ( true );
			Count . Text = this . CustomerGrid . Items . Count . ToString ( );
			Debug . WriteLine ( "SQLDBVIEWER : Customer Data fully loaded" );
		}
		private async void EventControl_DetDataLoaded ( object sender, LoadedEventArgs e )
		{
			if ( e . CallerDb != "SQLDBVIEWER" ) return;
			if ( e . CallerType != "SQLSERVER" ) return;

			LoadingDbData = true;

			// Event handler for DetDataLoaded
			if ( e . DataSource == null ) return;// || this . DetailsGrid . Items . Count > 0 ) return;
			SqlDetcollection = e . DataSource as DetCollection;
			// Get our personal Collection view of the Db
			SQLDetViewerView = CollectionViewSource . GetDefaultView ( SqlDetcollection );
			SQLDetViewerView . Refresh ( );
			this . DetailsGrid . ItemsSource = SQLDetViewerView;
			this . DetailsGrid . SelectedIndex = 0;
			this . DetailsGrid . SelectedItem = 0;
			this . DetailsGrid . UpdateLayout ( );
			this . DetailsGrid . Refresh ( );
			WaitMessage . Visibility = Visibility . Collapsed;
			this . DetailsGrid . Visibility = Visibility . Visible;
			this . DetailsGrid . Refresh ( );
			ParseButtonText ( true );
			Count . Text = this . DetailsGrid . Items . Count . ToString ( );
			Debug . WriteLine ( "SQLDBVIEWER : Details Data fully loaded" );
		}

		/// <summary>
		/// Method to handle callback of DATAUPDATED when another viewer Updates a row of data
		/// NB = The data is already reloaded when we get here
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//*************************************************************************************************************//
		private async void EventControl_EditDbDataUpdated ( object sender, LoadedEventArgs e )
		{
			// All working well when update is in another SqlDbViewer or this viewers EditDb sends a  trigger
			// Monday, 28 May 2021
			int currsel = 0;
			// This STOPS the Multivewer loading from wiping out our selections and scroll position
			if ( sender == null && IsViewerLoaded == false ) return;

			if ( e . CallerDb == "BANKACCOUNT" )
			{
				if ( SqlBankcollection == null ) return;
				if ( this . BankGrid . Items . Count == 0 ) return;
				RefreshInProgress = true;
				if ( e . Custno != null && e . Bankno != null )
				{
					this . BankGrid . UnselectAll ( );
					currsel = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . BankGrid, "BANKACCOUNT" );
					if ( currsel == -1 ) currsel = SavedBankRow;
				}
				else
					currsel = SavedBankRow != -1 ? SavedBankRow : 0;
				RefreshInProgress = true;
				// This trigger  the IndexChanged Method in here
				//Data has been changed in EditDb, so reload it
				this . BankGrid . ItemsSource = null;
				this . BankGrid . Items . Clear ( );
				//We cant use Collection syntax - it crashes it every time
				this . BankGrid . ItemsSource = SqlBankcollection;
				this . BankGrid . SelectedIndex = currsel;
				try
				{
					this . BankGrid . SelectedItem = SqlBankcollection . ElementAt ( currsel );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"{ex . Message}, @{ex . Data}" );
				}
				this . BankGrid . SelectedIndex = currsel;
				this . BankGrid . SelectedItem = currsel;
				Utils . SetUpGridSelection ( this . BankGrid, currsel );
				//				this . BankGrid . ScrollIntoView ( this . BankGrid . SelectedItem );
				// This scroll current active record to top/bottom of window - not nice !!
				//				Utils . ScrollRecordInGrid ( this . BankGrid, currsel );
				RefreshInProgress = false;
				ParseButtonText ( true );
				Count . Text = this . BankGrid . Items . Count . ToString ( );
				Mouse . OverrideCursor = Cursors . Arrow;
			}
			else if ( e . CallerDb == "CUSTOMER" )
			{
				if ( SqlCustcollection == null ) return;
				if ( this . CustomerGrid . Items . Count == 0 ) return;
				RefreshInProgress = true;
				if ( e . Custno != null && e . Bankno != null )
				{
					this . CustomerGrid . UnselectAll ( );
					currsel = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . CustomerGrid, "CUSTOMER" );
					if ( currsel == -1 ) currsel = SavedCustRow != -1 ? SavedCustRow : 0;
				}
				else
					currsel = SavedCustRow != -1 ? SavedCustRow : 0;
				RefreshInProgress = true;
				// This trigger  the IndexChanged Method in here
				this . CustomerGrid . ItemsSource = null;
				this . CustomerGrid . Items . Clear ( );
				//We cant use Collection syntax - it crashes it every time
				this . CustomerGrid . ItemsSource = SqlCustcollection;
				this . CustomerGrid . SelectedIndex = currsel;
				try
				{
					this . CustomerGrid . SelectedItem = SqlCustcollection . ElementAt ( currsel );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"{ex . Message}, @{ex . Data}" );
				}
				this . CustomerGrid . SelectedIndex = currsel;
				this . CustomerGrid . SelectedItem = currsel;
				Utils . SetUpGridSelection ( this . CustomerGrid, currsel );
				//				this . CustomerGrid . ScrollIntoView ( this . CustomerGrid . SelectedItem );
				//				this . CustomerGrid . Refresh ( );
				// This scroll current active record to top/bottom of window - not nice !!
				//				Utils . ScrollRecordInGrid ( this . CustomerGrid, currsel );
				RefreshInProgress = false;
				ParseButtonText ( true );
				Count . Text = this . CustomerGrid . Items . Count . ToString ( );
				Mouse . OverrideCursor = Cursors . Arrow;
			}
			else if ( e . CallerDb == "DETAILS" )
			{
				if ( SqlDetcollection == null ) return;
				if ( this . DetailsGrid . Items . Count == 0 ) return;
				RefreshInProgress = true;
				if ( e . Custno != null && e . Bankno != null )
				{
					this . DetailsGrid . UnselectAll ( );
					currsel = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . CustomerGrid, "DETAILS" );
					if ( currsel == -1 ) currsel = SavedDetRow != -1 ? SavedDetRow : 0;
				}
				else
					currsel = SavedDetRow != -1 ? SavedDetRow : 0;

				RefreshInProgress = true;
				// This trigger  the IndexChanged Method in here
				//Data has been changed in EditDb, so reload it
				this . DetailsGrid . ItemsSource = null;
				this . DetailsGrid . Items . Clear ( );
				Mouse . OverrideCursor = Cursors . Wait;
				//We cant use Collection syntax - it crashes it every time
				this . DetailsGrid . ItemsSource = SqlDetcollection;
				this . DetailsGrid . SelectedIndex = currsel;
				try
				{
					this . DetailsGrid . SelectedItem = SqlDetcollection . ElementAt ( currsel );
					Console . WriteLine ( $"Selecting Details row {currsel}" );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"{ex . Message}, @{ex . Data}" );
				}
				this . DetailsGrid . SelectedIndex = currsel;
				this . DetailsGrid . SelectedItem = currsel;
				Utils . SetUpGridSelection ( this . DetailsGrid, currsel );
				//				this . DetailsGrid . ScrollIntoView ( this . DetailsGrid . SelectedItem );
				// This scroll current active record to top/bottom of window - not nice !!
				//				Utils . ScrollRecordInGrid ( this . DetailsGrid, currsel);
				//SetScrollToTop ( this .DetailsGrid );
				//Details
				RefreshInProgress = false;
				ParseButtonText ( true );
				Count . Text = this . DetailsGrid . Items . Count . ToString ( );
				Mouse . OverrideCursor = Cursors . Arrow;
			}
		}
		//*************************************************************************************************************//
		private string GetPrettyRowDetails ( DataGrid dg )
		{
			string output = "";
			if ( dg . SelectedIndex == -1 )
				dg . SelectedIndex = 0;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				var tmp = dg . SelectedItem as BankAccountViewModel;
				output += tmp . CustNo . ToString ( );
				output += ", Bank A/c : " + tmp . BankNo . ToString ( );
				return output;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				var tmp = dg . SelectedItem as CustomerViewModel;
				output += tmp?.CustNo . ToString ( );
				output += ", Bank A/c : " + tmp?.BankNo . ToString ( );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				var tmp = dg . SelectedItem as DetailsViewModel;
				output += tmp?.CustNo . ToString ( );
				output += ", Bank A/c : " + tmp?.BankNo . ToString ( );
			}
			return output;
		}

		/// <summary>
		/// Event that is called whenever a record is deleted so that
		/// ALL open viewers can refresh themselves
		/// </summary>
		/// <param name="o"></param>
		/// <param name="b"></param>
		/// <param name="c"></param>
		//*************************************************************************************************************//
		//		private async void OnDeletion ( string sender, string bank, string cust, int CurrentRow )
		private async void OnDeletion ( object sender, LoadedEventArgs e )
		{
			//Process the deletion for this grid here....
			if ( CurrentDb == "BANKACCOUNT" )
			{
				int currsel = 0;
				if ( CurrentDb != sender )
					currsel = this . BankGrid . SelectedIndex;
				else
					currsel = e . CurrSelection;// CurrentRow;

				// not sure why I do this twice ?????
				currsel = e . CurrSelection != -1 ? e . CurrSelection : 0;

				this . BankGrid . ItemsSource = null;
				this . BankGrid . Items . Clear ( );
				Mouse . OverrideCursor = Cursors . Wait;
				await BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );
				//this . BankGrid . ItemsSource = SqlViewerBankcollection;
				//this . BankGrid . SelectedIndex = currsel;
				//Utils . ScrollRecordInGrid ( this . BankGrid, currsel );
				//this . BankGrid . Refresh ( );
				//ParseButtonText ( true );
				//Count . Text = this . BankGrid . Items . Count . ToString ( );
				Mouse . OverrideCursor = Cursors . Arrow;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				int currsel = 0;
				if ( CurrentDb != sender )
					currsel = this . CustomerGrid . SelectedIndex;
				else
					currsel = e . CurrSelection != -1 ? e . CurrSelection : 0;
				this . CustomerGrid . ItemsSource = null;
				this . CustomerGrid . Items . Clear ( );
				Mouse . OverrideCursor = Cursors . Wait;
				await CustCollection . LoadCust ( SqlCustcollection, "SQLDBVIEWER", 1, true );
				//CustCollection b  =new CustCollection();
				//Task . Run ( async ( ) => b . LoadCustomerTaskInSortOrderAsync ( false , 0 ) );
				//this . CustomerGrid . ItemsSource = SqlViewerCustcollection;
				//this . CustomerGrid . SelectedIndex = currsel;
				//Utils . ScrollRecordInGrid ( this . CustomerGrid, currsel );
				//this . CustomerGrid . Refresh ( );
				//ParseButtonText ( true );
				//Count . Text = this . CustomerGrid . Items . Count . ToString ( );
				Mouse . OverrideCursor = Cursors . Arrow;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				int currsel = 0;
				if ( CurrentDb != sender )
					currsel = this . DetailsGrid . SelectedIndex;
				else
					currsel = e . CurrSelection != -1 ? e . CurrSelection : 0;
				currsel = e . CurrSelection;
				this . DetailsGrid . ItemsSource = null;
				this . DetailsGrid . Items . Clear ( );
				//DetCollection b  =new DetCollection();
				//Task . Run ( async ( ) => b . LoadDetailsTaskInSortOrderAsync ( false , currsel ) );
				Mouse . OverrideCursor = Cursors . Wait;
				await DetailCollection . LoadDet ( SqlDetcollection, "SQLDBVIEWER", 1, true );
				//this . DetailsGrid . ItemsSource = SqlViewerDetcollection;
				//this . DetailsGrid . SelectedIndex = currsel;
				//Utils . ScrollRecordInGrid ( this . DetailsGrid, currsel );
				//this . DetailsGrid . Refresh ( );
				//ParseButtonText ( true );
				//Count . Text = this . DetailsGrid . Items . Count . ToString ( );
				Mouse . OverrideCursor = Cursors . Arrow;
			}
		}

		//*************************************************************************************************************//
		public void SqlDbViewer_AllViewersUpdate ( object sender, NotifyAllViewersOfUpdateEventArgs e )
		{
			//int currow = 0;
			//if ( CurrentDb == "BANKACCOUNT" )
			//{
			//	Debug . WriteLine ( $"Updating DataGrid (BankGrid) after change made elsewhere..." );
			//	this . BankGrid . ItemsSource = null;
			//	this . BankGrid . Items . Clear ( );
			//	this . BankGrid . ItemsSource = SqlBankcollection;
			//	this . BankGrid . Refresh ( );
			//	this . BankGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlBankcollection );
			//	this . BankGrid . SelectedIndex = currow != -1 ? currow : 0;
			//	Utils . ScrollRecordInGrid ( this . BankGrid, currow );
			//	ParseButtonText ( true );
			//	Count . Text = this . BankGrid . Items . Count . ToString ( );
			//}
			//else if ( CurrentDb == "CUSTOMER" )
			//{
			//	Debug . WriteLine ( $"Updating DataGrid (customerGrid) after change made elsewhere..." );
			//	this . CustomerGrid . ItemsSource = null;
			//	this . CustomerGrid . Items . Clear ( );
			//	this . CustomerGrid . ItemsSource = SqlCustcollection;
			//	this . CustomerGrid . Refresh ( );
			//	this . CustomerGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlCustcollection );
			//	this . CustomerGrid . SelectedIndex = currow != -1 ? currow : 0;
			//	Utils . ScrollRecordInGrid ( this . CustomerGrid, currow );
			//	ParseButtonText ( true );
			//	Count . Text = this . CustomerGrid . Items . Count . ToString ( );
			//}
			//else if ( CurrentDb == "DETAILS" )
			//{
			//	currow = this . DetailsGrid . SelectedIndex;
			//	Debug . WriteLine ( $"Updating DataGrid (DetailsGrid) after change made elsewhere..." );
			//	this . DetailsGrid . ItemsSource = null;
			//	this . DetailsGrid . ItemsSource = SqlDetcollection;
			//	this . DetailsGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlDetcollection );
			//	this . DetailsGrid . SelectedIndex = currow != -1 ? currow : 0;
			//	Utils . ScrollRecordInGrid ( this . DetailsGrid, currow );
			//	ParseButtonText ( true );
			//	Count . Text = this . DetailsGrid . Items . Count . ToString ( );
			//}
		}

		#endregion *** CRUCIAL Methods - Event CALLBACK for data loading

		#region General CallBack/Delegate stuff  - mostly NOT events as such


		#endregion General CallBack/Delegate stuff  - mostly NOT events as such

		#region Callback response functions


		#endregion Callback response functions

		#region load/startup / Close down

		//*************************************************************************************************************//
		private void Window_Closed ( object sender, EventArgs e )
		{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				this . BankGrid . ItemsSource = null;
				BankGrid?.Items . Clear ( );
				Flags . BankEditDb = null;
				Flags . CurrentEditDbViewerBankGrid = null;
				Flags . CurrentBankViewer = null;
				Flags . SqlBankViewer = null;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				this . CustomerGrid . ItemsSource = null;
				CustomerGrid?.Items . Clear ( );
				Flags . CustEditDb = null;
				Flags . CurrentEditDbViewerCustomerGrid = null;
				Flags . CurrentCustomerViewer = null;
				Flags . SqlCustViewer = null;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				this . DetailsGrid . ItemsSource = null;
				DetailsGrid?.Items . Clear ( );
				Flags . DetEditDb = null;
				Flags . CurrentEditDbViewerDetailsGrid = null;
				Flags . CurrentDetailsViewer = null;
				Flags . SqlDetViewer = null;
			}

			// make sure we clear this as it is a global static, so new oiwndows will load thinking they are in multi mode !
			Flags . IsMultiMode = false;
			// clear EditDb flags in case one is open as closing this viewer does NOT  call WindowClosed !!!
			Flags . ActiveEditGrid = null;
			Flags . CurrentEditDbViewer = null;

			EventControl . ViewerIndexChanged -= EventControl_EditIndexChanged;
			EventControl . EditIndexChanged -= EventControl_EditIndexChanged;
			EventControl . MultiViewerIndexChanged -= EventControl_EditIndexChanged;
			// Main update notification handler
			EventControl . EditDbDataUpdated -= EventControl_EditDbDataUpdated;
			// change made in Multi Viewer
			EventControl . MultiViewerDataUpdated -= EventControl_EditDbDataUpdated;
			// Another SQL viewer has updated a record  notification handler
			EventControl . ViewerDataUpdated -= EventControl_SqlDataUpdated;

			// clear our callback function subscription - DbDataLoadedHandler(object, DataLoadedArgs)
			EventControl . BankDataLoaded -= EventControl_BankDataLoaded;
			EventControl . CustDataLoaded -= EventControl_CustDataLoaded;
			EventControl . DetDataLoaded -= EventControl_DetDataLoaded; ;

			EventControl . RecordDeleted -= OnDeletion;

			if ( MainWindow . gv . ViewerCount == 0 )
			{
				// No more Viewers open, so clear Viewers list in DbSelector
				MainWindow . gv . SqlViewerWindow = null;
			}
			UpdateDbSelectorBtns ( Flags . CurrentSqlViewer );
			Debug . WriteLine ( $"{CurrentDb} has Unsubscribed from All events successfully" );
			Debug . WriteLine ( $"***Window has just closed***" );
		}

		#endregion load/startup / Close down

		#region EVENTHANDLERS

		/// <summary>
		///  Function that is broadcasts a notification to whoever to
		///  notify that one of the Obs collections has been changed by something
		/// </summary>
		/// <param name="o"> The sending object</param>
		/// <param name="args"> Sender name and Db Type</param>
		//		private static bool hasupdated = false;

		//*************************************************************************************************************//
		public void SendDataChanged ( SqlDbViewer o, DataGrid Grid, string dbName )
		{
			// Databases have DEFINITELY been updated successfully after a change
			// We Now Broadcast this to ALL OTHER OPEN VIEWERS here and now

			if ( dbName == "BANKACCOUNT" )
			{
				BankAccountViewModel bvm = this . BankGrid . SelectedItem as BankAccountViewModel;
				if ( bvm == null ) return;
				EventControl . TriggerViewerDataUpdated ( SqlBankcollection,
					new LoadedEventArgs
					{
						CallerType = "SQLDBVIEWER",
						Custno = bvm . CustNo,
						Bankno = bvm . BankNo,
						CallerDb = "BANKACCOUNT",
						DataSource = SqlBankcollection,
						RowCount = this . BankGrid . SelectedIndex
					} );
				Debug . WriteLine ( $"EditDb(1485) SQLDBVIEWER : in SendDataChanged : Sending ViewerDataUpdated EVENT for Bank" );
			}
			else if ( dbName == "CUSTOMER" )
			{
				CustomerViewModel bvm = this . CustomerGrid . SelectedItem as CustomerViewModel;
				if ( bvm == null ) return;
				EventControl . TriggerViewerDataUpdated ( SqlCustcollection,
					new LoadedEventArgs
					{
						CallerType = "SQLDBVIEWER",
						Custno = bvm . CustNo,
						Bankno = bvm . BankNo,
						CallerDb = "CUSTOMER",
						DataSource = SqlCustcollection,
						RowCount = this . CustomerGrid . SelectedIndex
					} );
				Debug . WriteLine ( $"EditDb(1499) SQLDBVIEWER : in SendDataChanged : Sending ViewerDataUpdated EVENT for customer" );
			}
			else if ( dbName == "DETAILS" )
			{
				DetailsViewModel bvm = this . DetailsGrid . SelectedItem as DetailsViewModel;
				if ( bvm == null ) return;
				EventControl . TriggerViewerDataUpdated ( SqlDetcollection,
					new LoadedEventArgs
					{
						CallerType = "SQLDBVIEWER",
						Custno = bvm . CustNo,
						Bankno = bvm . BankNo,
						CallerDb = "DETAILS",
						DataSource = SqlDetcollection,
						RowCount = this . DetailsGrid . SelectedIndex
					} );
				Debug . WriteLine ( $"EditDb(1514) SQLDBVIEWER : in SendDataChanged : Sending ViewerDataUpdated EVENT for Details" );
			}
			Mouse . OverrideCursor = Cursors . Arrow;
		}

		#endregion EVENTHANDLERS

		#region Utility functions for Grid loading/Switching

		//*************************************************************************************************************//
		private void CleanViewerGridData ( )
		{
			if ( CurrentDataGrid == BankGrid )
			{
				//Clear the GridView data structure first, (false means kill ALL)
				GridViewer . CheckResetAllGridViewData ( "BANKACCOUNT", this, false );
				// now clear Flags structure
				Flags . SqlBankViewer = null;
				Flags . SqlBankGrid = null;
				this . BankGrid . ItemsSource = null;
				this . BankGrid . Items . Clear ( );
				//				dtBank?.Clear ( );
				SqlBankcollection = null;
				this . BankGrid . Visibility = Visibility . Hidden;
			}
			else if ( CurrentDataGrid == CustomerGrid )
			{
				//Clear the GridView data structure first, (false means kill ALL)
				GridViewer . CheckResetAllGridViewData ( "CUSTOMER", this, false );
				// now clear Flags structure
				Flags . SqlCustViewer = null;
				Flags . SqlCustGrid = null;
				this . CustomerGrid . ItemsSource = null;
				this . CustomerGrid . Items . Clear ( );
				//				dtCust?.Clear ( );
				SqlCustcollection = null;
				this . CustomerGrid . Visibility = Visibility . Hidden;
			}
			else if ( CurrentDataGrid == DetailsGrid )
			{
				//Clear the GridView data structure first, (false means kill ALL)
				GridViewer . CheckResetAllGridViewData ( "DETAILS", this, false );
				Flags . SqlDetViewer = null;
				Flags . SqlDetGrid = null;
				this . DetailsGrid . ItemsSource = null;
				this . DetailsGrid . Items . Clear ( );
				//				dtDetails?.Clear ( );
				SqlDetcollection = null;
				this . DetailsGrid . Visibility = Visibility . Hidden;
			}
		}

		//*************************************************************************************************************//
		private bool SetFlagsForViewerGridChange ( SqlDbViewer Viewer, DataGrid Grid )
		{
			// ONLY CALLED BY SHOWxxxxx_CLICK HANDLERS
			//First off - Clear other GRID flags so we dont have any confusion
			//but be aware they may be open in a different viewer so only null
			// them out if they do not have an owner Viewer Handle
			if ( Flags . SqlBankViewer == this )
			{
				Flags . SqlBankGrid = null;
				this . BankGrid . ItemsSource = null;
			}
			else if ( Flags . SqlCustViewer == this )
			{
				Flags . SqlCustGrid = null;
				this . CustomerGrid . ItemsSource = null;
			}
			else if ( Flags . SqlDetViewer == this )
			{
				Flags . SqlDetGrid = null;
				this . DetailsGrid . ItemsSource = null;
			}
			// Setup our pointer to this Viewer and Grid
			if ( Grid == BankGrid )
			{
				Flags . SqlBankGrid = this . BankGrid;
				Flags . SqlBankViewer = Viewer;
			}
			else if ( Grid == CustomerGrid )
			{
				Flags . SqlCustGrid = this . CustomerGrid;
				Flags . SqlCustViewer = Viewer;
			}
			else if ( Grid == DetailsGrid )
			{
				Flags . SqlDetGrid = this . DetailsGrid;
				Flags . SqlDetViewer = Viewer;
			}
			// Sort out ItemsSources for all possible open windows
			if ( Flags . SqlCustViewer == null )
				this . CustomerGrid . ItemsSource = null;
			if ( Flags . SqlBankViewer == null )
				this . BankGrid . ItemsSource = null;
			if ( Flags . SqlDetViewer == null )
				this . DetailsGrid . ItemsSource = null;

			return true;
		}

		#endregion Utility functions for Grid loading/Switching

		#region Show_Bank/Customer/Details cleanup functions for switching Db views

		/// <summary>
		/// Changing Db in viewer, so tidy up current pointers
		/// </summary>
		//*************************************************************************************************************//
		private void ClearCurrentFlags ( )
		{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				Flags . SqlBankViewer = null;
				Flags . SqlBankGrid = null;
			}
			if ( CurrentDb == "CUSTOMER" )
			{
				Flags . SqlCustViewer = null;
				Flags . SqlCustGrid = null;
			}
			if ( CurrentDb == "DETAILS" )
			{
				Flags . SqlDetViewer = null;
				Flags . SqlDetGrid = null;
			}
			// Generic variables we need to clear
			Flags . ActiveSqlViewer = this;
			Flags . ActiveSqlGrid = null;
		}

		//*************************************************************************************************************//
		public async void ShowBank_Click ( object sender, RoutedEventArgs e )
		{
			int CurrentSelection = 0;

			//Close any EditDb window that may be open
			if ( MainWindow . gv . SqlCurrentEditViewer != null )
				MainWindow . gv . SqlCurrentEditViewer . Close ( );

			if ( Flags . SqlBankViewer != null && Flags . IsFiltered == false )
			{
				// viewer already open with Bankgrid visible, so switch to it
				Flags . SqlBankViewer . Focus ( );
				return;
			}
			if ( Flags . CurrentSqlViewer != null )
				Mouse . OverrideCursor = Cursors . Wait;

			// Clear the previous global flag identifier
			if ( this == Flags . SqlCustViewer )
				Flags . SqlCustViewer = null;
			else if ( this == Flags . SqlDetViewer )
				Flags . SqlDetViewer = null;

			// We have to sort out the control structures BEFORE loading Customer Db Data and showing it
			CleanViewerGridData ( );
			// We have to Unsubscribe from previous Db Types Evet handlers BEFORE loading Customer Db Data and showing it
			ClearCurrentFlags ( );

			CurrentDb = "BANKACCOUNT";
			//Reset our Datgrid pointer
			CurrentDataGrid = BankGrid;
			Flags . CurrentSqlViewer = this;

			// We MUST subscribe to EVENTS before calling the load  data functiuonality
			SubscribeToEvents ( );
			Mouse . OverrideCursor = Cursors . Wait;
			Flags . CurrentSqlViewer = this;

			if ( Flags . SqlBankGrid != null && this . BankGrid . Items . Count > 0 && !Flags . IsMultiMode && Flags . IsFiltered == false )
			{
				MainWindow . gv . SqlBankViewer . Focus ( );
				MainWindow . gv . SqlBankViewer . BringIntoView ( );
				MainWindow . gv . SqlBankViewer . Refresh ( );
				Mouse . OverrideCursor = Cursors . Arrow;
				return;
			}

			//Reset the MultiMode flag as user has requested a FULL reload
			Flags . IsMultiMode = false;
			CurrentActiveGrid = this . BankGrid;
			this . BankGrid . Visibility = Visibility . Visible;
			this . CustomerGrid . Visibility = Visibility . Hidden;
			this . DetailsGrid . Visibility = Visibility . Hidden;

			// Important call - it sets up global flags for all/any of the allowed viewer windows
			if ( !SetFlagsForViewerGridChange ( this, BankGrid ) )
				return;

			// LOAD THE NEW DATA
			//This calls  LoadCustomerTask for us after sorting out the command line sort order requested
			///and it clears down any  existing data in DataTable or Collection
			await BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );

			CurrentActiveGrid = this . BankGrid;
			this . BankGrid . ItemsSource = SqlBankcollection;

			//// create GV[] variables for this new viewer grid
			DbSelector . UpdateControlFlags ( Flags . CurrentSqlViewer, CurrentDb, "" );

			SetScrollVariables ( this . BankGrid );
			Flags . SqlBankGrid = this . BankGrid;

			IsFiltered = "";
			SetButtonColor ( RefreshBtn, "BLUE" );
			Mouse . OverrideCursor = Cursors . Arrow;
			return;
		}

		//*************************************************************************************************************//
		/// <summary>
		/// Fetches SQL data for Customer Db and fills relevant DataGrid
		/// </summary>
		//*************************************************************************************************************//
		public async void ShowCust_Click ( object sender, RoutedEventArgs e )
		{
			int CurrentSelection = 0;

			//Close any EditDb window that may be open
			if ( MainWindow . gv . SqlCurrentEditViewer != null )
				MainWindow . gv . SqlCurrentEditViewer . Close ( );

			if ( Flags . SqlCustViewer != null && Flags . IsFiltered == false )
			{
				// viewer already open with Bankgrid visible, so switch to it
				Flags . SqlCustViewer . Focus ( );
				return;
			}
			if ( Flags . CurrentSqlViewer != null )
				Mouse . OverrideCursor = Cursors . Wait;

			// Clear the previous global flag identifier
			if ( this == Flags . SqlDetViewer )
				Flags . SqlDetViewer = null;
			else if ( this == Flags . SqlBankViewer )
				Flags . SqlBankViewer = null;

			// We have to sort out the control structures BEFORE loading Customer Db Data and showing it
			CleanViewerGridData ( );
			// We have to Unsubscribe from previous Db Types Evet handlers BEFORE loading Customer Db Data and showing it
			ClearCurrentFlags ( );

			CurrentDb = "CUSTOMER";
			//Reset our Datgrid pointer
			CurrentDataGrid = this . CustomerGrid;
			Flags . SqlCustGrid = this . CustomerGrid;

			// We MUST subscribe to EVENTS before calling the load  data functiuonality
			SubscribeToEvents ( );
			Mouse . OverrideCursor = Cursors . Wait;
			Flags . CurrentSqlViewer = this;

			if ( Flags . SqlCustGrid != null && this . CustomerGrid . Items . Count > 0 && !Flags . IsMultiMode && Flags . IsFiltered == false )
			{
				MainWindow . gv . SqlCustViewer . Focus ( );
				MainWindow . gv . SqlCustViewer . BringIntoView ( );
				MainWindow . gv . SqlCustViewer . Refresh ( );
				Mouse . OverrideCursor = Cursors . Arrow;
				return;
			}

			//Reset the MultiMode flag as user has requested a FULL reload
			Flags . IsMultiMode = false;
			CurrentActiveGrid = this . CustomerGrid;
			this . BankGrid . Visibility = Visibility . Hidden;
			this . CustomerGrid . Visibility = Visibility . Visible;
			this . DetailsGrid . Visibility = Visibility . Hidden;

			// Important call - it sets up global flags for all/any of the allowed viiewer windows
			if ( !SetFlagsForViewerGridChange ( this, CustomerGrid ) )
				return;

			// LOAD THE NEW DATA
			//This calls  LoadCustomerTask for us after sorting out the command line sort order requested
			///and it clears down any  existing data in DataTable or Collection
			await CustCollection . LoadCust ( SqlCustcollection, "SQLDBVIEWER", 1, true );

			CurrentActiveGrid = this . CustomerGrid;
			this . CustomerGrid . ItemsSource = SqlCustcollection;

			// create GV[] variables for this new viewer grid
			DbSelector . UpdateControlFlags ( Flags . CurrentSqlViewer, CurrentDb, "" );

			SetScrollVariables ( this . CustomerGrid );
			Flags . SqlCustGrid = this . CustomerGrid;

			IsFiltered = "";
			SetButtonColor ( RefreshBtn, "YELLOW" );
			Mouse . OverrideCursor = Cursors . Arrow;
			return;
		}

		//*************************************************************************************************************//
		/// <summary>
		/// Fetches SQL data for DetailsViewModel Db and fills relevant DataGrid
		/// <param name="sender"></param>
		/// <param name="e"></param></summary>
		//*************************************************************************************************************//
		public async void ShowDetails_Click ( object sender, RoutedEventArgs e )
		{
			int CurrentSelection = 0;

			//Close any EditDb window that may be open
			if ( MainWindow . gv . SqlCurrentEditViewer != null )
				MainWindow . gv . SqlCurrentEditViewer . Close ( );

			if ( Flags . SqlDetViewer != null && Flags . IsFiltered == false )
			{
				// viewer already open with Bankgrid visible, so switch to it
				Flags . SqlDetViewer . Focus ( );
				return;
			}
			if ( Flags . CurrentSqlViewer != null )
				Mouse . OverrideCursor = Cursors . Wait;

			// Clear the previous global flag identifier
			if ( this == Flags . SqlCustViewer )
				Flags . SqlCustViewer = null;
			else if ( this == Flags . SqlBankViewer )
				Flags . SqlBankViewer = null;

			// We have to sort out the control structures BEFORE loading Details Db Data and showing it
			CleanViewerGridData ( );
			// We have to Unsubscribe from previous Db Types Evet handlers BEFORE loading Customer Db Data and showing it
			ClearCurrentFlags ( );

			CurrentDb = "DETAILS";
			//Reset our Datgrid pointer
			CurrentDataGrid = DetailsGrid;
			Flags . SqlDetGrid = DetailsGrid;

			// We MUST subscribe to EVENTS before calling the load  data functionality
			SubscribeToEvents ( );
			Mouse . OverrideCursor = Cursors . Wait;
			Flags . CurrentSqlViewer = this;

			if ( Flags . SqlDetGrid != DetailsGrid && this . DetailsGrid . Items . Count > 0 && !Flags . IsMultiMode && Flags . IsFiltered == false )
			{
				MainWindow . gv . SqlDetViewer . Focus ( );
				MainWindow . gv . SqlDetViewer . BringIntoView ( );
				MainWindow . gv . SqlDetViewer . Refresh ( );
				Mouse . OverrideCursor = Cursors . Arrow;
				return;
			}

			//Reset the MultiMode flag as user has requested a FULL reload
			Flags . IsMultiMode = false;
			CurrentActiveGrid = this . BankGrid;
			this . BankGrid . Visibility = Visibility . Hidden;
			this . CustomerGrid . Visibility = Visibility . Hidden;
			this . DetailsGrid . Visibility = Visibility . Visible;

			// Important call - it sets up global flags for all/any of the allowed viewer windows
			if ( !SetFlagsForViewerGridChange ( this, DetailsGrid ) )
				return;

			// LOAD THE NEW DATA
			//This calls  LoadCustomerTask for us after sorting out the command line sort order requested
			///and it clears down any  existing data in DataTable or Collection
			await DetailCollection . LoadDet ( SqlDetcollection, "SQLDBVIEWER", 1, true );

			CurrentActiveGrid = this . DetailsGrid;
			this . DetailsGrid . ItemsSource = SqlDetcollection;

			// create GV[] variables for this new viewer grid
			DbSelector . UpdateControlFlags ( Flags . CurrentSqlViewer, CurrentDb, "" );

			SetScrollVariables ( this . DetailsGrid );
			Flags . SqlBankGrid = this . DetailsGrid;

			IsFiltered = "";
			SetButtonColor ( RefreshBtn, "GREEN" );
			Mouse . OverrideCursor = Cursors . Arrow;
			return;
		}

		#endregion Show_Bank/Cust/Details cleanup functions for switching Db views

		#region Standard Click Events

		//*************************************************************************************************************//
		private void ExitFilter_Click ( object sender, RoutedEventArgs e )
		{
			//Just "Close" the Filter panel
		}

		//*************************************************************************************************************//
		private void ContextMenu1_Click ( object sender, RoutedEventArgs e )
		{
			//Add a new row
			if ( CurrentDb == "BANKACCOUNT" )
			{
				//DataRow dr = BankAccountViewModel . dtBank . NewRow ( );
				//BankAccountViewModel . dtBank . Rows . Add ( dr );
				//				BankGrid.DataContext = dtBank;
			}
		}

		//*************************************************************************************************************//
		private void ContextMenu2_Click ( object sender, RoutedEventArgs e )
		{
			//Delete current Row
			BankAccountViewModel dg = sender as BankAccountViewModel;
			DataRowView row = ( DataRowView ) this . BankGrid . SelectedItem;
		}

		//*************************************************************************************************************//
		private void ContextMenu3_Click ( object sender, RoutedEventArgs e )
		{
			//Close Window
		}

		//*************************************************************************************************************//
		private async void Multiaccs_Click ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registered" cos we can
			// Click the button before the window has had focus set
			Flags . CurrentSqlViewer = this;
			//Show only Customers with multiple Bank Accounts
			Window_MouseDown ( sender, null );
			string s = Flags . MultiAccountCommandString = Multiaccounts . Content as string;
			if ( s . Contains ( "<<-" ) || s . Contains ( "Show All" ) )
			{
				Flags . IsMultiMode = false;
				ResetOptionButtons ( 1 );
				// Set the gradient background
				SetButtonGradientBackground ( Multiaccounts );
				SetButtonGradientBackground ( Filters );
				StatusBar . Text = "All Database records are shown...";
				ShowBank . IsEnabled = true;
				ShowDetails . IsEnabled = true;
				ShowCust . IsEnabled = true;
				Edit . IsEnabled = true;
				Filters . IsEnabled = true;
			}
			else
			{
				Flags . IsMultiMode = true;
				ResetOptionButtons ( 1 );
				SetButtonGradientBackground ( Multiaccounts );
				StatusBar . Text = "Database is filtered to ONLY show Customers with Multiple A/c's";
				ShowBank . IsEnabled = false;
				ShowDetails . IsEnabled = false;
				ShowCust . IsEnabled = false;
				Edit . IsEnabled = false;
				Filters . IsEnabled = false;
			}
			if ( CurrentDb == "BANKACCOUNT" )
			{
				this . BankGrid . ItemsSource = null;
				this . BankGrid . Items . Clear ( );
				//				dtBank . Clear ( );
				await BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );
				//this . BankGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerBankcollection );
				////				ExtensionMethods . Refresh ( this . BankGrid );
				//this . BankGrid . Refresh ( );
				//Count . Text = this . BankGrid . Items . Count . ToString ( );
				//ParseButtonText ( true );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				this . CustomerGrid . ItemsSource = null;
				this . CustomerGrid . Items . Clear ( );
				//				dtCust . Clear ( );
				await CustCollection . LoadCust ( SqlCustcollection, "SQLDBVIEWER", 1, true );
				//this . CustomerGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerCustcollection );
				//this . CustomerGrid . Refresh ( );
				////				ExtensionMethods . Refresh ( this . CustomerGrid );
				//Count . Text = this . CustomerGrid . Items . Count . ToString ( );
				//ParseButtonText ( true );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				this . DetailsGrid . ItemsSource = null;
				this . DetailsGrid . Items . Clear ( );
				//				dtDetails . Clear ( );
				Mouse . OverrideCursor = Cursors . Wait;
				await DetailCollection . LoadDet ( SqlDetcollection, "SQLDBVIEWER", 1, true );
				//this . DetailsGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerDetcollection );
				//this . DetailsGrid . Refresh ( );
				////				ExtensionMethods . Refresh ( this . DetailsGrid );
				//Count . Text = this . DetailsGrid . Items . Count . ToString ( );
				//ParseButtonText ( true );
				Mouse . OverrideCursor = Cursors . Arrow;
			}
			// Set the gradient background
			SetButtonGradientBackground ( Filters );
		}

		//*************************************************************************************************************//
		private void ContextMenuFind_Click ( object sender, RoutedEventArgs e )
		{
			// find something - this returns  the top rows data in full
			BankAccountViewModel b = this . BankGrid . SelectedItem as BankAccountViewModel;
		}
		#endregion Standard Click Events

		#region grid row selection code

		//*************************************************************************************************************//
		private void BankGrid_SelectedCellsChanged ( object sender, SelectedCellsChangedEventArgs e )
		{
			//This fires whenever we click inside the grid !!!
			// Even just selecting a different row
			//This is THE ONE to use to update our DbSleector ViewersList text
			// Crucial flag for updating
			//			if ( Flags . DataLoadIngInProgress ) return;
			if ( Flags . DataLoadIngInProgress || Flags . EditDbDataChange ) return;

			if ( CurrentDb == "BANKACCOUNT" )
			{
				if ( this . BankGrid . Items . Count == 0 ) return;
				//				Debug . WriteLine ( $" 3-1 *** TRACE *** SQLDBVIEWER : BankGrid_SelectedCellsChanged BANKACCOUNT - Entering" );
				// All We are doing here is just updating the text in the DbSelectorViewersList
				//This gives me the entire Db Record in "c"
				BankAccountViewModel c = this . BankGrid?.SelectedItem as BankAccountViewModel;
				if ( c == null ) return;
				string date = Convert . ToDateTime ( c . ODate ) . ToShortDateString ( );
				string s = $"Bank - # {c . CustNo}, Bank #{c . BankNo}, Customer # {c . CustNo}, £{c . Balance}, {c . IntRate}%,  {date}";
				// ensure global flag is cleared after loading a viewer
				Flags . SqlViewerIsLoading = false;
				UpdateDbSelectorItem ( s );
			}
		}

		//*************************************************************************************************************//
		private void CustomerGrid_SelectedCellsChanged ( object sender, SelectedCellsChangedEventArgs e )
		{
			//This fires when we click inside the grid !!!
			//This is THE ONE to use to update our DbSleector ViewersList text
			if ( CurrentDb == "CUSTOMER" )

			{
				//This gives me an entrie Db Record in "c"
				CustomerViewModel c = this . CustomerGrid?.SelectedItem as CustomerViewModel;
				if ( c == null ) return;
				string s = $"Customer - # {c . CustNo}, Bank #@{c . BankNo}, {c . LName}, {c . Town}, {c . County} {c . PCode}";
				// ensure global flag is cleared after loading a viewer
				Flags . SqlViewerIsLoading = false;
				UpdateDbSelectorItem ( s );
			}
		}

		//*************************************************************************************************************//
		private void DetailsGrid_SelectedCellsChanged ( object sender, SelectedCellsChangedEventArgs e )
		{
			if ( RefreshInProgress ) return;
			//This fires when we click inside the grid !!!
			//This is THE ONE to use to update our DbSelector ViewersList text
			if ( CurrentDb == "DETAILS" )
			{
				//This gives me an entire Db Record in "c"
				DetailsViewModel c = this . DetailsGrid?.SelectedItem as DetailsViewModel;
				if ( c == null ) return;
				string date = Convert . ToDateTime ( c . ODate ) . ToShortDateString ( );
				string s = $"Details - # {c . CustNo}, Bank #@{c . BankNo}, Customer # {c . CustNo}, £{c . Balance}, {c . IntRate}%,  {date}";
				// ensure global flag is cleared after loading a viewer
				Flags . SqlViewerIsLoading = false;
				UpdateDbSelectorItem ( s );
			}
		}

		#endregion grid row selection code

		#region CellEdit Checker functions

		//*************************************************************************************************************//
		private void BankGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
		//Get the BankAccount cell data and its Db Field name BEFORE
		// it has been changed and store in global variables
		{
			OrignalCellRow = e . Row . GetIndex ( );
			OriginalCellColumn = e . Column . DisplayIndex;
			DataGridColumn dgc = e . Column as DataGridColumn;
			string name = dgc . SortMemberPath;
			DataGridRow dgr = e . Row;
			OriginalDataType = name;
			switch ( name . ToUpper ( ) )
			{
				case "BANKNO":
					OriginalCellData = bvm . BankNo;
					break;

				case "CUSTO":
					OriginalCellData = bvm . CustNo;
					break;

				case "ACTYPE":
					OriginalCellData = bvm . AcType;
					break;

				case "BALANCE":
					OriginalCellData = bvm . Balance;
					break;

				case "INTRATE":
					OriginalCellData = bvm . IntRate;
					break;

				case "ODATE":
					OriginalCellData = bvm . ODate;
					break;

				case "CDATE":
					OriginalCellData = bvm . CDate;
					break;
			}
		}

		//These all set a global bool to flag whether a cell has actually been changed
		//so we do not call SQL Update uneccessarily
		//*************************************************************************************************************//
		private void BankGrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			BankAccountViewModel c = BankGrid?.SelectedItem as BankAccountViewModel;
			TextBox textBox = e . EditingElement as TextBox;
			if ( textBox == null )
			{
				//default to save data - probably a date field that has been changed
				SelectionhasChanged = true;
				return;
			}
			string str = textBox . Text;
			SelectionhasChanged = ( OriginalCellData?.ToString ( ) != str );
		}

		//*************************************************************************************************************//
		private void CustomerGrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			CustomerViewModel c = CustomerGrid?.SelectedItem as CustomerViewModel;
			TextBox textBox = e . EditingElement as TextBox;
			if ( textBox == null )
			{
				//default to save data - probably a date field that has been changed
				SelectionhasChanged = true;
				return;
			}
			string str = textBox . Text;
			SelectionhasChanged = ( OriginalCellData?.ToString ( ) != str );
		}

		//*************************************************************************************************************//
		private void DetailsGrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			DetailsViewModel c = this . DetailsGrid . SelectedItem as DetailsViewModel;
			TextBox textBox = e . EditingElement as TextBox;
			if ( textBox == null )
			{
				//default to save data - probably a date field that has been changed
				SelectionhasChanged = true;
				return;
			}
			string str = textBox . Text;
			SelectionhasChanged = ( OriginalCellData?.ToString ( ) != str );
			if ( SelectionhasChanged )
			{
				ViewerChangeType = 2;   // done in next call anyway
				EditChangeType = 0;
			}
		}

		//*************************************************************************************************************//
		private void DetailsGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
		//Get the BankAccount cell data and its Db Field name BEFORE
		// it has been changed and store in global variables
		{
			OrignalCellRow = e . Row . GetIndex ( );
			OriginalCellColumn = e . Column . DisplayIndex;
			DataGridColumn dgc = e . Column as DataGridColumn;
			string name = dgc . SortMemberPath;
			DataGridRow dgr = e . Row;
			//			DetailsViewModel dvm = dgr.Item as DetailsViewModel;
			OriginalDataType = name;
			switch ( name . ToUpper ( ) )
			{
				case "BANKNO":
					OriginalCellData = dvm . BankNo;
					break;

				case "CUSTO":
					OriginalCellData = dvm . CustNo;
					break;

				case "ACTYPE":
					OriginalCellData = dvm . AcType;
					break;

				case "BALANCE":
					OriginalCellData = dvm . Balance;
					break;

				case "INTRATE":
					OriginalCellData = dvm . IntRate;
					break;

				case "ODATE":
					OriginalCellData = dvm . ODate;
					break;

				case "CDATE":
					OriginalCellData = dvm . CDate;
					break;
			}
		}

		//*************************************************************************************************************//
		private void CustomerGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
		//Get the BankAccount cell data and its Db Field name BEFORE
		// it has been changed and store in global variables
		{
			OrignalCellRow = e . Row . GetIndex ( );
			OriginalCellColumn = e . Column . DisplayIndex;
			DataGridColumn dgc = e . Column as DataGridColumn;
			string name = dgc . SortMemberPath;
			DataGridRow dgr = e . Row;
			OriginalDataType = name;
			switch ( name . ToUpper ( ) )
			{
				case "BANKNO":
					OriginalCellData = cvm . BankNo;
					break;

				case "CUSTO":
					OriginalCellData = cvm . CustNo;
					break;

				case "ACTYPE":
					OriginalCellData = cvm . AcType;
					break;

				case "FNAME":
					OriginalCellData = cvm . FName;
					break;

				case "LNAME":
					OriginalCellData = cvm . LName;
					break;

				case "ADDR1":
					OriginalCellData = cvm . Addr1;
					break;

				case "ADDR2":
					OriginalCellData = cvm . Addr2;
					break;

				case "TOWN":
					OriginalCellData = cvm . Town;
					break;

				case "COUNTY":
					OriginalCellData = cvm . County;
					break;

				case "PCODE":
					OriginalCellData = cvm . PCode;
					break;

				case "PHONE":
					OriginalCellData = cvm . Phone;
					break;

				case "MOBILE":
					OriginalCellData = cvm . Mobile;
					break;

				case "DOB":
					OriginalCellData = cvm . Dob;
					break;

				case "ODATE":
					OriginalCellData = cvm . ODate;
					break;

				case "CDATE":
					OriginalCellData = cvm . CDate;
					break;
			}
		}

		#endregion CellEdit Checker functions

		#region Keyboard /Mousebutton handlers

		//*************************************************************************************************************//
		private void Window_MouseDown ( object sender, MouseButtonEventArgs e )
		{
			//			Window_GotFocus ( sender, e );
		}

		//*************************************************************************************************************//
		public void UpdateDbSelectorBtns ( SqlDbViewer viewer )
		{
			// works with multiple entries 22 March 2021
			if ( Flags . DbSelectorOpen == null )
				return;

			if ( Flags . DbSelectorOpen . ViewersList . Items . Count == 1 )
			{
				Flags . DbSelectorOpen . ViewerDeleteAll . IsEnabled = false;
				Flags . DbSelectorOpen . ViewerDelete . IsEnabled = false;
				Flags . DbSelectorOpen . SelectViewerBtn . IsEnabled = false;
				return;
			}
			else
			{
				if ( Flags . DbSelectorOpen . ViewersList . Items . Count > 2 )
					Flags . DbSelectorOpen . ViewerDeleteAll . IsEnabled = true;
				else
					Flags . DbSelectorOpen . ViewerDeleteAll . IsEnabled = false;
				Flags . DbSelectorOpen . ViewerDelete . IsEnabled = true;
				Flags . DbSelectorOpen . SelectViewerBtn . IsEnabled = true;
			}
		}

		/// <summary>
		/// No longer used - ignore....
		/// </summary>
		/// <param name="selection"></param>
		//*************************************************************************************************************//
		private void SetButtonStatus ( string selection )
		{
			//			return;
			//This sets the currently selected Db button to be defaulted
			// making it change background color
			if ( selection == "BANKACCOUNT" )
			{
				ShowDetails . Tag = false;
				ShowBank . Tag = true;    //Allows auto coloration
			}
			else if ( selection == "CUSTOMER" )
			{
				ShowDetails . Tag = false;
				ShowCust . Tag = true;
			}
			else if ( selection == "DETAILS" )
			{
				Tag = false;
				ShowDetails . Tag = true;
			}
		}

		//*************************************************************************************************************//
		private void BankGrid_MouseRightButtonUp ( object sender, MouseButtonEventArgs e )
		{
			Type type;
			string cellData;
			int row = -1;
			int col = -1;
			string colName = "";
			object rowdata = null;
			object cellValue = null;

			//Displays a Dialog with relevant info on the data record Right clicked on (Now After RowPopup is spawned)
			// So it is disabled right now
			return;

			if ( CurrentDb == "BANKACCOUNT" )
			{
				cellValue = DataGridSupport . GetCellContent ( sender, e, CurrentDb, out row, out col, out colName, out rowdata );
				if ( row == -1 )
					row = this . BankGrid . SelectedIndex;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				CustomerViewModel bvm = cvm;
				cellValue = DataGridSupport . GetCellContent ( sender, e, CurrentDb, out row, out col, out colName, out rowdata );
				if ( row == -1 )
					row = this . BankGrid . SelectedIndex;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				CustomerViewModel bvm = cvm;
				cellValue = DataGridSupport . GetCellContent ( sender, e, CurrentDb, out row, out col, out colName, out rowdata );
				if ( row == -1 )
					row = this . BankGrid . SelectedIndex;
			}
			if ( cellValue == null )
			{
				MessageBox . Show ( $"Cannot access Data in the current cell, Row returned = {row}, Column = {col}, Column Name = {colName}" );
				return;
			}
			else if ( row == -1 && col == -1 )
			{
				//Header was clicked in
				type = cellValue . GetType ( );
				cellData = cellValue . ToString ( );
				if ( cellData != "" )
				{
					if ( cellData . Contains ( ":" ) )
					{
						int offset = cellData . IndexOf ( ':' );
						string result = cellData . Substring ( offset + 1 ) . Trim ( );
						MessageBox . Show ( $"Column clicked was a Header  =\"{result}\"" );
					}
				}
				return;
			}
			type = cellValue . GetType ( );
			cellData = cellValue . ToString ( );
			MessageBox . Show ( $"Data in the current cell \r\nColumn is \"{colName},\", Data Type=\"{type . Name}\"\r\nData = [{cellData}]\",\r\nRow={row}, Column={col}", "Requested Cell Contents" );
		}

		//*************************************************************************************************************//
		private async void BankGrid_MouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{
			int currsel = 0;
			// handle flags to let us know WE have triggered the selectedIndex change
			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			if ( e . ChangedButton == MouseButton . Right )
			{
				DataGridRow RowData = new DataGridRow ( );
				int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
				if ( row == -1 ) row = 0;
				RowInfoPopup rip = new RowInfoPopup ( "BANKACCOUNT", BankGrid, RowData );
				rip . DataContext = RowData;
				rip . BringIntoView ( );
				rip . Focus ( );
				rip . Topmost = true;
				rip . ShowDialog ( );

				Flags . ActiveSqlViewer = this;
				Flags . CurrentSqlViewer = this;
				//If data has been changed, update everywhere
				if ( rip . IsDirty )
				{
					// This is done in RowPopup()
					//BankAccountViewModel bvm = new BankAccountViewModel ( );
					//bvm = this . BankGrid . SelectedItem as BankAccountViewModel;
					//SQLHandlers sqlh = new SQLHandlers ( );
					//sqlh . UpdateDbRowAsync ( "BANKACCOUNT",bvm );
					this . BankGrid . ItemsSource = null;
					this . BankGrid . Items . Clear ( );
					SqlBankcollection = await BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );
					//this . BankGrid . ItemsSource = SqlViewerBankcollection;
					//StatusBar . Text = "Current Record Updated Successfully...";
					// Notify everyone else of the data change
					EventControl . TriggerViewerDataUpdated ( SqlBankcollection,
						new LoadedEventArgs
						{
							CallerType = "SQLDBVIEWER",
							Custno = bvm . CustNo,
							Bankno = bvm . BankNo,
							CurrSelection = this . BankGrid . SelectedIndex,
							CallerDb = "BANKACCOUNT",
							DataSource = SqlBankcollection,
							RowCount = this . BankGrid . SelectedIndex
						} );
				}
				else
					this . BankGrid . SelectedItem = RowData . Item;

				// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
				Utils . SetUpGridSelection ( this . BankGrid, row );
				ParseButtonText ( true );
				Count . Text = this . BankGrid . Items . Count . ToString ( );

				// This is essential to get selection activated again
				this . BankGrid . Focus ( );
			}
		}

		#endregion Keyboard /Mousebutton handlers


		#region SQL SUPPORT FUNCTIONS
		//*************************************************************************************************************//
		/// <summary>
		/// CALLED BY ANY GRID TO PHYSICALLY DELETE A ROW of DATA FROM THE dB
		/// </summary>
		/// <param name="Caller"></param>
		/// <param name="Bankno"></param>
		/// <param name="Custno"></param>
		/// <returns></returns>
		public bool DeleteRecord ( string Caller, string Bankno, string Custno, int CurrentRow )
		{
			string Command = "";
			bool Result = false;

			SqlConnection con = null;
			string ConString = "";
			ConString = ( string ) Settings . Default [ "BankSysConnectionString" ];
			try
			{
				//We need to update BOTH BankAccount AND DetailsViewModel to keep them in parallel
				using ( con = new SqlConnection ( ConString ) )
				{
					con . Open ( );
					Debug . WriteLine ( "\n" );
					Command = $"Delete from BANKACCOUNT WHERE BANKNO= {Bankno} AND Custno= {Custno} ";
					SqlCommand cmd = new SqlCommand ( Command, con );
					cmd . ExecuteNonQuery ( );
					Debug . WriteLine ( $"SQL Deletion successful from BANKACCOUNT DataBase for CustNo= {Custno} & BankNo= {Bankno}..." );

					Command = $"Delete from CUSTOMER WHERE BANKNO={Bankno} AND Custno={Custno}";
					cmd = new SqlCommand ( Command, con );
					cmd . ExecuteNonQuery ( );
					Debug . WriteLine ( $"SQL Deletion successful from CUSTOMER DataBase for CustNo= {Custno} & BankNo= {Bankno}..." );

					Command = $"Delete from SECACCOUNTS WHERE BANKNO= {Bankno} AND Custno= {Custno}";
					cmd = new SqlCommand ( Command, con );
					cmd . ExecuteNonQuery ( );
					Debug . WriteLine ( $"SQL Deletion successful from DETAILS  DataBase for CustNo= {Custno} & BankNo= {Bankno}..." );
					Result = true;
					Debug . WriteLine ( "" );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"SQL Error DeleteRecord(2381)- {ex . Message} Data = {ex . Data}" );
#if SHOWSQLERRORMESSAGEBOX
				MessageBox . Show ( "SQL error occurred in DeleteRecord(2383) - See Output for details" );
#endif
			}
			finally
			{
				//Lets force the grids to update when we return from here ??
				Debug . WriteLine ( $"SQL - Row for Bank A/c {Bankno} & Customer # {Custno} deleted in ALL Db's successfully" );
				con . Close ( );
			}
			return Result;
		}

		#endregion SQL SUPPORT FUNCTIONS

		#region Tuple Handlers

		/// <returns>A fully populated Tuple</returns>
		//*************************************************************************************************************//
		public static object CreateTuple ( string currentDb )
		{
			object tpl = null;
			//content of Tuple is : (This, string "currentDb", int selectedIndex, , int Tag, object selectedItem)
			/*
			Item1 = current SqlDbViewer
			Item2 = CurrentDb string`
			Item3 = Grid.SelectedIndex
				  */
			if ( currentDb == "BANKACCOUNT" )
				tpl = Tuple . Create ( Flags . CurrentSqlViewer, currentDb, Flags . SqlBankGrid . SelectedIndex );
			else if ( currentDb == "CUSTOMER" )
				tpl = Tuple . Create ( Flags . CurrentSqlViewer, currentDb, Flags . SqlCustGrid . SelectedIndex );
			else if ( currentDb == "DETAILS" )

				tpl = Tuple . Create ( Flags . CurrentSqlViewer, currentDb, Flags . SqlDetGrid . SelectedIndex );
			return tpl;
		}

		/// <summary>
		/// Fjunction that create a fully populated Tuple from the current DataGrid and Viewer Window
		/// Content of Tuple is : ( this, string "currentDb", int selectedIndex, , int Tag, object (a datarecord basically) selectedItem)
		/// </summary>
		/// <param name="currentDb" is the current viewer type identifier  eg"CUSTOMER"></param>

		//*********************************************************************************************************//
		//Receiver for messages FROM DbSelector
		/// <summary>
		/// Good example of how to pass Tuples around
		/// </summary>
		/// <param name="tuple"></param>
		//*************************************************************************************************************//
		public void GetTupleData ( Tuple<SqlDbViewer, string, int> tuple )
		{
			//content of Tuple is : (This, string "currentDb", int selectedIndex, , int Tag, object selectedItem)
			if ( tuple . Item2 == "BANKACCOUNT" )
			{
			}
			else if ( tuple . Item2 == "CUSTOMER" )
			{
			}
			else if ( tuple . Item2 == "DETAILS" )
			{
			}
		}

		#endregion Tuple Handlers

		#region Focus handling

		//*************************************************************************************************************//
		private void BankGrid_GotFocus ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registreded" cos we can
			// Click the button before the window has had focus set
			DbSelector . SelectActiveViewer ( this );
			if ( this . BankGrid . IsFocused )
			{
				//this . Focus ( );
			}
			//			else
			//				this . BankGrid . Focus ( );
			Flags . CurrentSqlViewer = this;
			Flags . SqlBankGrid = sender as DataGrid;
			Flags . SetGridviewControlFlags ( this, this . BankGrid );
		}
		//*************************************************************************************************************//

		private void CustomerGrid_GotFocus ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registered" cos we can
			// Click the button before the window has had focus set
			bool inhere = false;
			if ( inhere ) return;
			inhere = true;
			DbSelector . SelectActiveViewer ( this );
			if ( this . CustomerGrid . IsFocused )
			{
				//				this . Focus ( );
				return;
			}
			else
			{
				//this . CustomerGrid . Focus ( );
				Flags . CurrentSqlViewer = this;
				Flags . SqlCustGrid = sender as DataGrid;
				Flags . SetGridviewControlFlags ( this, this . CustomerGrid );
			}
			inhere = false;
		}

		//*************************************************************************************************************//
		private void DetailsGrid_GotFocus ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registreded" cos we can
			// Click the button before the window has had focus set
			DbSelector . SelectActiveViewer ( this );
			if ( this . DetailsGrid . IsFocused )
			{
				//				this . Focus ( );
			}
			//			else
			//				this . DetailsGrid . Focus ( );
			Flags . CurrentSqlViewer = this;
			Flags . SqlDetGrid = sender as DataGrid;
			Flags . SetGridviewControlFlags ( this, this . DetailsGrid );
		}

		#endregion Focus handling

		//*************************************************************************************************************//
		private static void HandleEdit ( object sender, EditEventArgs e )
		{
			//Handler for Datagrid Edit occurred delegate
			if ( Flags . EventHandlerDebug )
				Debug . WriteLine ( $"\r\nRecieved by SQLDBVIEWER (150) Caller={e . Caller}, Index = {e . CurrentIndex},  Grid = {e . ToString ( )}\r\n " );
		}

		#region Datagrid ROW UPDATING functionality

		/// <summary>
		///  This is aMASSIVE Function that handles updating the Dbs via SQL plus sorting the current grid
		///  out & notifying all other viewers that a change has occurred so they can (& in fact do) update
		///  their own data grids rather nicely right now - 22/4/21
		/// </summary>
		/// <param name="sender">Unused</param>
		/// <param name="e">Unused</param>
		//*************************************************************************************************************//
		public async void ViewerGrid_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
		{
			/// This ONLY gets called when a cell is edited in THIS viewer

			ViewerChangeType = 2;
			EditChangeType = 0;
			BankAccountViewModel ss = bvm;
			CustomerViewModel cs = cvm;
			DetailsViewModel sa = dvm;
			//if data has NOT changed, do NOT bother updating the Db
			// Clever stuff Eh - saves lots of processing time?
			if ( !SelectionhasChanged )
				return;
			else
				SelectionhasChanged = false;    // clear the edit status flag again

			Mouse . OverrideCursor = Cursors . Wait;

			// Set the control flags so that we know we have changed data when we notify other windows
			Flags . UpdateInProgress = true;

			//Only called whn an edit has been completed
			if ( e != null )
			{
				SQLHandlers sqlh = new SQLHandlers ( );
				// These get the row with all the NEW data
				if ( CurrentDb == "BANKACCOUNT" )
				{
					int currow = 0;
					currow = this . BankGrid . SelectedIndex != -1 ? this . BankGrid . SelectedIndex : 0;
					ss = this . BankGrid . SelectedItem as BankAccountViewModel;
					// This is the NEW DATA from the current row that we are sendign to SQL handler to update the DB's
					sqlh . UpdateDbRowAsync ( CurrentDb, ss, this . BankGrid . SelectedIndex );
					// Notify other Viewers of the update
					SendDataChanged ( this, this . BankGrid, "BANKACCOUNT" );

					//if ( Flags . CurrentEditDbViewer != null )
					//	Flags . CurrentEditDbViewer . UpdateGrid ( "BANKACCOUNT" );
					//this . BankGrid . SelectedIndex = currow;
					//this . BankGrid . SelectedItem = currow;
					//this . BankGrid . Refresh ( );
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					int currow = 0;
					currow = this . CustomerGrid . SelectedIndex != -1 ? this . CustomerGrid . SelectedIndex : 0;
					cs = this . CustomerGrid . SelectedItem as CustomerViewModel;
					// This is the NEW DATA from the current row
					sqlh . UpdateDbRowAsync ( CurrentDb, cs, this . CustomerGrid . SelectedIndex );

					//					Debug . WriteLine ( $" 7-2 *** TRACE *** SQLDBVIEWER : ViewerGrid_rowEditEnding - Sending CUSTOMER TriggerViewerDataUpdated" );

					SendDataChanged ( this, this . CustomerGrid, "CUSTOMER" );

					this . CustomerGrid . SelectedIndex = currow;
					Utils . ScrollRecordInGrid ( this . CustomerGrid, currow );
					// Notify EditDb to upgrade its grid
					if ( Flags . CurrentEditDbViewer != null )
						Flags . CurrentEditDbViewer . UpdateGrid ( "CUSTOMER" );
				}
				else if ( CurrentDb == "DETAILS" )
				{
					int currow = 0;
					currow = this . DetailsGrid . SelectedIndex != -1 ? this . DetailsGrid . SelectedIndex : 0;
					sa = this . DetailsGrid . SelectedItem as DetailsViewModel;
					// sa contains the NEW DATA from the current row
					// Update Db itself via SQL
					sqlh . UpdateDbRowAsync ( CurrentDb, sa, currow );

					//					Debug . WriteLine ( $" 7-3 *** TRACE *** SQLDBVIEWER : ViewerGrid_rowEditEnding - Sending DETAILS TriggerViewerDataUpdated" );

					SendDataChanged ( this, this . DetailsGrid, "DETAILS" );

					this . DetailsGrid . SelectedIndex = currow;
					Utils . ScrollRecordInGrid ( this . DetailsGrid, currow );
					// Notify EditDb to upgrade its grid
					if ( Flags . CurrentEditDbViewer != null )
						Flags . CurrentEditDbViewer . UpdateGrid ( "DETAILS" );
				}
				Mouse . OverrideCursor = Cursors . Arrow;
				// Set the control flags so that we know we have changed data when we notify other windows
				Flags . UpdateInProgress = false;
				return;
			}
			else
			{
				SQLHandlers sqlh = new SQLHandlers ( );
				sqlh . UpdateDbRow ( CurrentDb, e );

				if ( CurrentDb == "BANKACCOUNT" || CurrentDb == "DETAILS" )
				{
					// Editdb is NOT OPEN
					SqlCommand cmd = null;
					try
					{
						//Sanity check - are values actualy valid ???
						//They should be as Grid vlaidate entries itself !!
						int x;
						decimal Y;
						if ( CurrentDb == "BANKACCOUNT" )
						{
							//						ss = e.Row.Item as BankAccount;
							x = Convert . ToInt32 ( ss . Id );
							x = Convert . ToInt32 ( ss . AcType );
							//Check for invalid A/C Type
							if ( x < 1 || x > 4 )
							{
								Debug . WriteLine ( $"SQL Invalid A/c type of {ss . AcType} in grid Data" );
								Mouse . OverrideCursor = Cursors . Arrow;
								MessageBox . Show ( $"Invalid A/C Type ({ss . AcType}) in the Grid !!!!\r\nPlease correct this entry!" );
								return;
							}
							Y = Convert . ToDecimal ( ss . Balance );
							Y = Convert . ToDecimal ( ss . IntRate );
							//Check for invalid Interest rate
							if ( Y > 100 )
							{
								Debug . WriteLine ( $"SQL Invalid Interest Rate of {ss . IntRate} > 100% in grid Data" );
								Mouse . OverrideCursor = Cursors . Arrow;
								MessageBox . Show ( $"Invalid Interest rate ({ss . IntRate}) > 100 entered in the Grid !!!!\r\nPlease correct this entry!" );
								return;
							}
							DateTime dtm = Convert . ToDateTime ( ss . ODate );
							dtm = Convert . ToDateTime ( ss . CDate );
						}
						else if ( CurrentDb == "DETAILS" )
						{
							x = Convert . ToInt32 ( sa . Id );
							x = Convert . ToInt32 ( sa . AcType );
							//Check for invalid A/C Type
							if ( x < 1 || x > 4 )
							{
								Debug . WriteLine ( $"SQL Invalid A/c type of {sa . AcType} in grid Data" );
								Mouse . OverrideCursor = Cursors . Arrow;
								MessageBox . Show ( $"Invalid A/C Type ({sa . AcType}) in the Grid !!!!\r\nPlease correct this entry!" );
								return;
							}
							Y = Convert . ToDecimal ( sa . Balance );
							Y = Convert . ToDecimal ( sa . IntRate );
							//Check for invalid Interest rate
							if ( Y > 100 )
							{
								Debug . WriteLine ( $"SQL Invalid Interest Rate of {sa . IntRate} > 100% in grid Data" );
								Mouse . OverrideCursor = Cursors . Arrow;
								MessageBox . Show ( $"Invalid Interest rate ({sa . IntRate}) > 100 entered in the Grid !!!!\r\nPlease correct this entry!" );
								return;
							}
							DateTime dtm = Convert . ToDateTime ( sa . ODate );
							dtm = Convert . ToDateTime ( sa . CDate );
						}
						//					string sndr = sender.ToString();
					}
					catch ( Exception ex )
					{
						Debug . WriteLine ( $"SQL Invalid grid Data - {ex . Message} Data = {ex . Data}" );
						Mouse . OverrideCursor = Cursors . Arrow;
						MessageBox . Show ( "Invalid data entered in the Grid !!!! - See Output for details" );
						return;
					}
					SqlConnection con;
					string ConString = "";
					ConString = ( string ) Settings . Default [ "BankSysConnectionString" ];
					//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
					con = new SqlConnection ( ConString );
					try
					{
						//We need to update BOTH BankAccount AND DetailsViewModel to keep them in parallel
						using ( con )
						{
							con . Open ( );

							if ( CurrentDb == "BANKACCOUNT" )
							{
								cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
								cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( ss . Id ) );
								cmd . Parameters . AddWithValue ( "@bankno", ss . BankNo . ToString ( ) );
								cmd . Parameters . AddWithValue ( "@custno", ss . CustNo . ToString ( ) );
								cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( ss . AcType ) );
								cmd . Parameters . AddWithValue ( "@balance", Convert . ToDecimal ( ss . Balance ) );
								cmd . Parameters . AddWithValue ( "@intrate", Convert . ToDecimal ( ss . IntRate ) );
								cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( ss . ODate ) );
								cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( ss . CDate ) );
								cmd . ExecuteNonQuery ( );
								Debug . WriteLine ( "SQL Update of BankAccounts successful..." );

								cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
								cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( sa . Id ) );
								cmd . Parameters . AddWithValue ( "@bankno", sa . BankNo . ToString ( ) );
								cmd . Parameters . AddWithValue ( "@custno", sa . CustNo . ToString ( ) );
								cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( sa . AcType ) );
								cmd . Parameters . AddWithValue ( "@balance", Convert . ToDecimal ( sa . Balance ) );
								cmd . Parameters . AddWithValue ( "@intrate", Convert . ToDecimal ( sa . IntRate ) );
								cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( sa . ODate ) );
								cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( sa . CDate ) );
								cmd . ExecuteNonQuery ( );
								Debug . WriteLine ( "SQL Update of SecAccounts successful..." );

								cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
								cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( sa . Id ) );
								cmd . Parameters . AddWithValue ( "@bankno", sa . BankNo . ToString ( ) );
								cmd . Parameters . AddWithValue ( "@custno", sa . CustNo . ToString ( ) );
								cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( sa . AcType ) );
								cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( sa . ODate ) );
								cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( sa . CDate ) );
								cmd . ExecuteNonQuery ( );
								Debug . WriteLine ( "SQL Update of Customers successful..." );
							}
							else if ( CurrentDb == "DETAILS" )
							{
								cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
								cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( sa . Id ) );
								cmd . Parameters . AddWithValue ( "@bankno", sa . BankNo . ToString ( ) );
								cmd . Parameters . AddWithValue ( "@custno", sa . CustNo . ToString ( ) );
								cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( sa . AcType ) );
								cmd . Parameters . AddWithValue ( "@balance", Convert . ToDecimal ( sa . Balance ) );
								cmd . Parameters . AddWithValue ( "@intrate", Convert . ToDecimal ( sa . IntRate ) );
								cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( sa . ODate ) );
								cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( sa . CDate ) );
								cmd . ExecuteNonQuery ( );
								Debug . WriteLine ( "SQL Update of BankAccounts successful..." );

								cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
								cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( sa . Id ) );
								cmd . Parameters . AddWithValue ( "@bankno", sa . BankNo . ToString ( ) );
								cmd . Parameters . AddWithValue ( "@custno", sa . CustNo . ToString ( ) );
								cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( sa . AcType ) );
								cmd . Parameters . AddWithValue ( "@balance", Convert . ToDecimal ( sa . Balance ) );
								cmd . Parameters . AddWithValue ( "@intrate", Convert . ToDecimal ( sa . IntRate ) );
								cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( sa . ODate ) );
								cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( sa . CDate ) );
								cmd . ExecuteNonQuery ( );
								Debug . WriteLine ( "SQL Update of SecAccounts successful..." );

								cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
								cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( sa . Id ) );
								cmd . Parameters . AddWithValue ( "@bankno", sa . BankNo . ToString ( ) );
								cmd . Parameters . AddWithValue ( "@custno", sa . CustNo . ToString ( ) );
								cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( sa . AcType ) );
								cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( sa . ODate ) );
								cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( sa . CDate ) );
								cmd . ExecuteNonQuery ( );
								Debug . WriteLine ( "SQL Update of customers successful..." );
							}
							if ( CurrentDb == "SECACCOUNTS" )
							{
								cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
								cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( ss . Id ) );
								cmd . Parameters . AddWithValue ( "@bankno", ss . BankNo . ToString ( ) );
								cmd . Parameters . AddWithValue ( "@custno", ss . CustNo . ToString ( ) );
								cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( ss . AcType ) );
								cmd . Parameters . AddWithValue ( "@balance", Convert . ToDecimal ( ss . Balance ) );
								cmd . Parameters . AddWithValue ( "@intrate", Convert . ToDecimal ( ss . IntRate ) );
								cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( ss . ODate ) );
								cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( ss . CDate ) );
								cmd . ExecuteNonQuery ( );
								Debug . WriteLine ( "SQL Update of BankAccounts successful..." );

								cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
								cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( sa . Id ) );
								cmd . Parameters . AddWithValue ( "@bankno", sa . BankNo . ToString ( ) );
								cmd . Parameters . AddWithValue ( "@custno", sa . CustNo . ToString ( ) );
								cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( sa . AcType ) );
								cmd . Parameters . AddWithValue ( "@balance", Convert . ToDecimal ( sa . Balance ) );
								cmd . Parameters . AddWithValue ( "@intrate", Convert . ToDecimal ( sa . IntRate ) );
								cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( sa . ODate ) );
								cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( sa . CDate ) );
								cmd . ExecuteNonQuery ( );
								Debug . WriteLine ( "SQL Update of SecAccounts successful..." );

								cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
								cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( sa . Id ) );
								cmd . Parameters . AddWithValue ( "@bankno", sa . BankNo . ToString ( ) );
								cmd . Parameters . AddWithValue ( "@custno", sa . CustNo . ToString ( ) );
								cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( sa . AcType ) );
								cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( sa . ODate ) );
								cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( sa . CDate ) );
								cmd . ExecuteNonQuery ( );
								Debug . WriteLine ( "SQL Update of Customers successful..." );
							}
							StatusBar . Text = "ALL THREE Databases updated successfully....";
							Debug . WriteLine ( "ALL THREE Databases updated successfully...." );
						}
					}
					catch ( Exception ex )
					{
						Debug . WriteLine ( $"SQL Error - {ex . Message} Data = {ex . Data}" );

#if SHOWSQLERRORMESSAGEBOX
						Mouse . OverrideCursor = Cursors . Arrow;
						MessageBox . Show ( "SQL error occurred - See Output for details" );
#endif
					}
					finally
					{
						Mouse . OverrideCursor = Cursors . Arrow;
						con . Close ( );
					}
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					if ( e == null && CurrentDb == "CUSTOMER" )
						cs = e . Row . Item as CustomerViewModel;

					try
					{
						//Sanity check - are values actualy valid ???
						//They should be as Grid vlaidate entries itself !!
						int x;
						x = Convert . ToInt32 ( cs . Id );
						//					string sndr = sender.ToString();
						x = Convert . ToInt32 ( cs . AcType );
						//Check for invalid A/C Type
						if ( x < 1 || x > 4 )
						{
							Debug . WriteLine ( $"SQL Invalid A/c type of {cs . AcType} in grid Data" );
							Mouse . OverrideCursor = Cursors . Arrow;
							MessageBox . Show ( $"Invalid A/C Type ({cs . AcType}) in the Grid !!!!\r\nPlease correct this entry!" );
							return;
						}
						DateTime dtm = Convert . ToDateTime ( cs . ODate );
						dtm = Convert . ToDateTime ( cs . CDate );
						dtm = Convert . ToDateTime ( cs . Dob );
					}
					catch ( Exception ex )
					{
						Debug . WriteLine ( $"SQL Invalid grid Data - {ex . Message} Data = {ex . Data}" );
						MessageBox . Show ( "Invalid data entered in the Grid !!!! - See Output for details" );
						Mouse . OverrideCursor = Cursors . Arrow;
						return;
					}
					SqlConnection con;
					string ConString = "";
					ConString = ( string ) Settings . Default [ "BankSysConnectionString" ];
					//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
					con = new SqlConnection ( ConString );
					try
					{
						//We need to update BOTH BankAccount AND DetailsViewModel to keep them in parallel
						using ( con )
						{
							con . Open ( );
							SqlCommand cmd = new SqlCommand ( "UPDATE Customer SET CUSTNO=@custno, BANKNO=@bankno, ACTYPE=@actype, " +
											"FNAME=@fname, LNAME=@lname, ADDR1=@addr1, ADDR2=@addr2, TOWN=@town, COUNTY=@county, PCODE=@pcode," +
											"PHONE=@phone, MOBILE=@mobile, DOB=@dob,ODATE=@odate, CDATE=@cdate WHERE Id=@id", con );

							cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( cs . Id ) );
							cmd . Parameters . AddWithValue ( "@custno", cs . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@bankno", cs . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( cs . AcType ) );
							cmd . Parameters . AddWithValue ( "@fname", cs . FName . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@lname", cs . LName . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@addr1", cs . Addr1 . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@addr2", cs . Addr2 . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@town", cs . Town . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@county", cs . County . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@pcode", cs . PCode . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@phone", cs . Phone . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@mobile", cs . Mobile . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@dob", Convert . ToDateTime ( cs . Dob ) );
							cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( cs . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( cs . CDate ) );
							cmd . ExecuteNonQuery ( );
							Debug . WriteLine ( "SQL Update of Customers successful..." );

							cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype,  ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
							cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( cs . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno", cs . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno", cs . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( cs . AcType ) );
							cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( cs . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( cs . CDate ) );
							cmd . ExecuteNonQuery ( );
							Debug . WriteLine ( "SQL Update of BankAccounts successful..." );

							cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate WHERE BankNo=@BankNo", con );
							cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( cs . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno", cs . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno", cs . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( cs . AcType ) );
							cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( cs . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( cs . CDate ) );
							cmd . ExecuteNonQuery ( );
							Debug . WriteLine ( "SQL Update of SecAccounts successful..." );
						}
						StatusBar . Text = "ALL THREE Databases updated successfully....";
						Debug . WriteLine ( "ALL THREE Databases updated successfully...." );
					}
					catch ( Exception ex )
					{
						Debug . WriteLine ( $"SQL Error - {ex . Message} Data = {ex . Data}" );
#if SHOWSQLERRORMESSAGEBOX
						Mouse . OverrideCursor = Cursors . Arrow;
						MessageBox . Show ( "SQL error occurred - See Output for details" );
#endif
					}
					finally
					{
						Mouse . OverrideCursor = Cursors . Arrow;
						con . Close ( );
					}
					Mouse . OverrideCursor = Cursors . Arrow;
					// Set the control flags so that we know we have changed data when we notify other windows
					Flags . UpdateInProgress = true;
					return;
				}
			}
			Mouse . OverrideCursor = Cursors . Arrow;
			SqlDbViewer sqlv = new SqlDbViewer ( );
			// Set the control flags so that we know we have changed data when we notify other windows
			Flags . UpdateInProgress = true;
			sqlv . SendDataChanged ( Flags . CurrentSqlViewer, Flags . ActiveSqlGrid, CurrentDb );
			return;
		}

		#endregion Datagrid ROW UPDATING functionality

		/// <summary>
		/// We are loading a Db into a Grid....
		/// Updates the MainWindow.GridViewer structure data, called
		/// by the 3  different "Show xxxxx" Funstion's
		/// </summary>
		/// <param name="type"></param>
		//*************************************************************************************************************//
		private void UpdateGridviewController ( string type )
		{
			//Retrieve Window handle of current Viewer window
			int newindex = -1;

			newindex = MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count - 1;
			if ( newindex < 0 )
				return;
			if ( PrettyDetails != "" )
				MainWindow . gv . CurrentDb [ newindex ] = PrettyDetails;
		}

		//*************************************************************************************************************//
		public void ParseButtonText ( bool obj )
		{
			ShowBank . Content = $" Bank A/c's  (0)";
			ShowCust . Content = $"Customer A/c's  (0)";
			ShowDetails . Content = $"Details A/c's  (0)";
			if ( IsFiltered != "" )
			{
				if ( IsFiltered == "BANKACCOUNT" )
				{
					ShowBank . Content = $"(F) Bank A/c's  ({this . BankGrid . Items . Count})";
					MainWindow . gv . CurrentDb [ MainWindow . gv . ViewerCount - 1 ] = ( string ) ShowBank . Content;
				}
				else if ( IsFiltered == "CUSTOMER" )
				{
					ShowCust . Content = $"(F) Customer A/c's  ({this . CustomerGrid . Items . Count})";
					MainWindow . gv . CurrentDb [ MainWindow . gv . ViewerCount - 1 ] = ( string ) ShowCust . Content;
				}
				else if ( IsFiltered == "DETAILS" )
				{
					ShowDetails . Content = $"(F) Details A/c's  ({this . DetailsGrid . Items . Count})";
					MainWindow . gv . CurrentDb [ MainWindow . gv . ViewerCount - 1 ] = ( string ) ShowDetails . Content;
				}
			}
			else
			{
				// filtered or multi accounts only displayed
				if ( CurrentDb == "BANKACCOUNT" )
				{
					if ( !obj )
					{
						if ( Flags . IsMultiMode )
							ShowBank . Content = $"<M> Bank A/c's  ({this . BankGrid . Items . Count})";
						else
							ShowBank . Content = $"Bank A/c's  ({this . BankGrid . Items . Count})";
					}
					else
						ShowBank . Content = $"Bank A/c's  ({this . BankGrid . Items . Count})";
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					if ( !obj )
					{
						if ( Flags . IsMultiMode )
							ShowCust . Content = $"<M> Customer A/c's  ({this . CustomerGrid . Items . Count})";
						else
							ShowCust . Content = $"Customer A/c's  ({this . CustomerGrid . Items . Count})";
					}
					else
						ShowCust . Content = $"Customer A/c's  ({this . CustomerGrid . Items . Count})";
				}
				else if ( CurrentDb == "DETAILS" )
				{
					if ( !obj )
					{
						if ( Flags . IsMultiMode )
							ShowDetails . Content = $"<M> Details A/c's  ({this . DetailsGrid . Items . Count})";
						else
							ShowDetails . Content = $"Details A/c's  ({this . DetailsGrid . Items . Count})";
					}
					else
						ShowDetails . Content = $"Details A/c's  ({this . DetailsGrid . Items . Count})";
				}
			}
		}

		#region ViewersList methods

		/// <summary>
		///  Method to update the contents of the ViewersList in DbSelector Window
		/// </summary>
		/// <param name="datarecord"></param>
		/// <param name="caller"></param>
		//*************************************************************************************************************//
		private void UpdateRowDetails ( object datarecord, string caller )
		// This updates the data in the DbSelector window's Viewers listbox, or add a new entry ????
		{
			bool Updated = false;
			if ( this . Tag == null ) return;
			if ( Flags . DbSelectorOpen == null ) return;
			for ( int x = 0 ; x < MainWindow . gv . MaxViewers ; x++ )
			{
				if ( x >= Flags . DbSelectorOpen?.ViewersList?.Items . Count ) break;
				if ( MainWindow . gv . ListBoxId [ x ] == ( Guid ) this . Tag )
				{
					if ( caller == "BankGrid" )
					{
						var record = datarecord as BankAccountViewModel;//CurrentBankSelectedRecord;
						PrettyDetails = $"Bank - A/c # {record?.BankNo}, Cust # {record?.CustNo}, Balance £ {record?.Balance}, Interest {record?.IntRate}%";
						MainWindow . gv . PrettyDetails = PrettyDetails;
						Updated = true;
						//Update list in DbSelector
						UpdateDbSelectorItem ( PrettyDetails );
					}
					else if ( caller == "CustomerGrid" )

					{
						var record = datarecord as CustomerViewModel;
						PrettyDetails = $"Customer - Customer # {record?.CustNo}, Bank # {record?.BankNo}, {record?.LName} {record?.Town}, {record?.County}";
						MainWindow . gv . PrettyDetails = PrettyDetails;
						Updated = true;
						//Update list in DbSelector
#pragma NOT NEEDED????
						UpdateDbSelectorItem ( PrettyDetails );
					}
					else if ( caller == "DetailsGrid" )
					{
						var record = datarecord as DetailsViewModel;
						PrettyDetails = $"Details - Bank A/C # {record?.BankNo}, Cust # {record?.CustNo}, Balance {record?.Balance}, Interest % {record?.IntRate}";
						MainWindow . gv . PrettyDetails = PrettyDetails;
						Updated = true;
						//Update list in DbSelector
						UpdateDbSelectorItem ( PrettyDetails );
					}
					break;
				}
			}
		}

		//*************************************************************************************************************//
		public void CloseViewer_Click ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registered" cos we can
			// Click the button before the window has been fully closed
			Flags . CurrentSqlViewer = this;

			//if ( CurrentDb == "BANKACCOUNT" )
			//{
			//	if ( Flags . BankDbEditor != null )
			//	{
			//		MessageBoxResult result = MessageBox . Show ( $"There is a Bank Account Edit Window open that will also\nbe closed if you continue to close this Window ?\n\nAre you sure you want to continue and close this window ?", "Possible Data Loss", MessageBoxButton . YesNo, MessageBoxImage . Warning, MessageBoxResult . No );
			//		if ( result == MessageBoxResult . Yes )
			//		{
			//			// close the editor window as well
			//			Flags . BankDbEditor . Close ( );
			//		}
			//		else
			//			return;
			//	}
			//}
			//if ( CurrentDb == "CUSTOMER" )
			//{
			//	if ( Flags . CustDbEditor != null )
			//	{
			//		MessageBoxResult result = MessageBox . Show ( $"There is a Customer Account Edit Window open that will also \nbe closed if you continue to close this Window ?\n\nAre you sure you want to continue and close this window ?", "Possible Data Loss", MessageBoxButton . YesNo, MessageBoxImage . Warning, MessageBoxResult . No );
			//		if ( result == MessageBoxResult . Yes )
			//		{
			//			// close the editor window as well
			//			Flags . CustDbEditor . Close ( );
			//		}
			//		else
			//			return;
			//	}
			//}
			//if ( CurrentDb == "DETAILS" )
			//{
			//	if ( Flags . DetDbEditor != null )
			//	{
			//		MessageBoxResult result = MessageBox . Show ( $"There is a Bank Details A/c  Edit Window open that will also \nbe closed if you continue to close this Window ?\n\nAre you sure you want to continue and close this window ?", "Possible Data Loss", MessageBoxButton . YesNo, MessageBoxImage . Warning, MessageBoxResult . No );
			//		if ( result == MessageBoxResult . Yes )
			//		{
			//			// close the editor window as well
			//			Flags . DetDbEditor . Close ( );
			//		}
			//		else
			//			return;
			//	}
			//}
			//Close current (THIS) Viewer Window
			//clear flags in GV[] & Flags Structures
			RemoveFromViewerList ( 99 );
			//			if ( CurrentDb == "BANKACCOUNT" )
			//			{
			//				// Clears Flags and the relevant Gv[] entry

			//#pragma TODO  Method needs work to handle 99 = Closing down actions needed
			//				RemoveFromViewerList ( 99 );
			//				Flags . SqlBankGrid = null;
			//				Flags . SqlBankViewer = null;
			//				MainWindow . gv . Bankviewer = Guid . Empty;
			//			}
			//			else if ( CurrentDb == "CUSTOMER" )
			//			{
			//				// Clears Flags and the relevant Gv[] entry
			//				RemoveFromViewerList ( 99 );
			//				Flags . SqlCustGrid = null;
			//				Flags . SqlCustViewer = null;
			//				MainWindow . gv . Custviewer = Guid . Empty;
			//			}
			//			else if ( CurrentDb == "DETAILS" )
			//			{
			//				// Clears Flags and the relevant Gv[] entry
			//				RemoveFromViewerList ( 99 );
			//				Flags . SqlDetGrid = null;
			//				Flags . SqlDetViewer = null;
			//				MainWindow . gv . Detviewer = Guid . Empty;
			//			}
			UpdateDbSelectorBtns ( Flags . CurrentSqlViewer );
			Mouse . OverrideCursor = Cursors . Arrow;
			// Must null out the global pointers to any Sql Viewer window
			//Each other still open viewer window will reset it on Focus()
			Flags . ActiveSqlGrid = null;
			Flags . CurrentSqlViewer = null;
			this . Close ( );
		}

		//*************************************************************************************************************//
		public void RemoveFromViewerList ( int x = -1 )
		{
			//Close current (THIS) Viewer Window
			//AND
			//clear flags in GV[] & Flags Structures
			// Clears Flags and the relevant Gv[] entry
			Flags . DeleteViewerAndFlags ( x, CurrentDb );

			if ( CurrentDb == "BANKACCOUNT" )
			{
				Flags . SqlBankGrid = null;
				Flags . SqlBankViewer = null;
				MainWindow . gv . Bankviewer = Guid . Empty;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				Flags . SqlCustGrid = null;
				Flags . SqlCustViewer = null;
				MainWindow . gv . Custviewer = Guid . Empty;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				Flags . SqlDetGrid = null;
				Flags . SqlDetViewer = null;
				MainWindow . gv . Detviewer = Guid . Empty;
			}
			return;

			{
				int viewerEntryCount = 0;
				if ( this . Tag == null ) return;
				if ( MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count == 1 )
					return;
				for ( int i = 0 ; i < MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count ; i++ )
				{
					if ( i >= MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count )
						return;
					if ( MainWindow . gv . ListBoxId [ i ] == ( Guid ) this . Tag )
					{
						Flags . DbSelectorOpen . ViewersList . Items . RemoveAt ( viewerEntryCount );
						break;
					}
					viewerEntryCount--;
				}

				Flags . DbSelectorOpen . ViewersList . Refresh ( );
				// If all viewers are closed, tidy up control structure dv[]
				if ( MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count == 1 )
				{
					MainWindow . gv . PrettyDetails = "";
					MainWindow . gv . SqlViewerWindow = null;
				}
			}
		}

		#endregion ViewersList methods

		//*************************************************************************************************************//
		private void CustomerGrid_TargetUpdated ( object sender, DataTransferEventArgs e )
		{
			// row data Loading ???
			MainWindow . gv . Datagrid [ LoadIndex ] = this . CustomerGrid;
		}

		//*************************************************************************************************************//
		private void Window_GotFocus ( object sender, RoutedEventArgs e )
		{
			// Actually, this is Called mostly by MouseDown Handler
			//when Focus has been set to this window
			if ( Flags . CurrentSqlViewer == this ) return;
			Flags . CurrentSqlViewer = this;
			//			this . Focus ( );

			//Gotta osrt out the current Db as it has now changed
			Guid guid = ( Guid ) Flags . CurrentSqlViewer . Tag;
			for ( int i = 0 ; i < 3 ; i++ )
			{
				if ( MainWindow . gv . ListBoxId [ i ] == guid )
				{
					CurrentDb = MainWindow . gv . CurrentDb [ i ];
					break;
				}
			}
			//Switch Flags details as required to match this window
			if ( CurrentDb == "BANKACCOUNT" )
				Flags . SetGridviewControlFlags ( this, this . BankGrid );
			else if ( CurrentDb == "CUSTOMER" )
				Flags . SetGridviewControlFlags ( this, this . CustomerGrid );
			else if ( CurrentDb == "DETAILS" )
				Flags . SetGridviewControlFlags ( this, this . DetailsGrid );

			//reposition selection in list of open viewers
			DbSelector . SelectActiveViewer ( this );
		}

		/// <summary>
		///  Function handles TWO seperate functions
		///  1 - Clear the entry MainWindow.gv[]
		///  2 - Remove correct line form DbSelector.ViewersList
		/// </summary>
		//*************************************************************************************************************//
		private void Minimize_click ( object sender, RoutedEventArgs e )
		{
			Window_GotFocus ( sender, null );
			this . WindowState = WindowState . Normal;
		}

		//*************************************************************************************************************//
		public void CreateListboxItemBinding ( )
		{
			Binding binding = new Binding ( "ListBoxItemText" );
			binding . Source = PrettyDetails;
		}

		//*************************************************************************************************************//
		private void CustomerGrid_CurrentCellChanged ( object sender, EventArgs e )
		{
			//We get this on entering any cell to edit
#pragma possible cure
			//if (CustomerGrid.SelectedItem != null)
			//{
			//	if (CustomerGrid.SelectedItem == null)
			//		return;
			//	TextBlock tb = new TextBlock ();

			//	//This gives me an entrie Db Record in "c"
			//	Customer c = CustomerGrid.SelectedItem as  Customer;
			//	Console.WriteLine ($"CustomerGrid_CurrentCellChanged - Identified row data of [{c.CustNo} - {c.FName} {c.LName}]");

			//}
		}

		/// <summary>
		/// Updates the text of the relevant ViewersList entry when selection is changed
		/// </summary>
		/// <param name="data"></param>
		//*************************************************************************************************************//
		public static void UpdateDbSelectorItem ( string data )
		{
			if ( Flags . CurrentSqlViewer == null ) return;
			if ( Flags . CurrentSqlViewer . Tag is null ) return;
			// handle global flag to control viewer addition/updating
			if ( Flags . SqlViewerIsLoading ) return;
			if ( RefreshInProgress ) return;
			Guid tag = ( Guid ) Flags . CurrentSqlViewer . Tag;

			ListBoxItem lbi;// = new ListBoxItem ();
			if ( MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count > 1 )
			{
				for ( int i = 1 ; i < MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count ; i++ )
				{
					lbi = Flags . DbSelectorOpen . ViewersList . Items [ i ] as ListBoxItem;
					//We start loop at ONE, so need ot use index MINUS 1 to access gv[] data correctly
					Guid lbtag = MainWindow . gv . ListBoxId [ i - 1 ];
					if ( lbtag == tag )
					{
						//got the matching entry, update its "Content" field
						Flags . DbSelectorOpen . ListBoxItemText = data;
						lbi . Content = data;
						Flags . DbSelectorOpen?.ViewersList?.Refresh ( );
						//						IsAdded = true;
						break;
					}
				}
			}
		}

		//*************************************************************************************************************//
		private void Edit_Click ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registered" cos we can
			// Click the button before the window has had focus set
			Flags . CurrentSqlViewer = this;
			ViewEditdb ViewEdit;

			// Open Edit Window for the current record in SqlDbViewer DataGrid
			if ( CurrentDb == "BANKACCOUNT" )
			{
				if ( Flags . BankEditDb != null )
				{
					Flags . BankEditDb . Focus ( );
					Flags . BankEditDb . BringIntoView ( );
					return;
				}
				if ( Flags . CurrentEditDbViewer != null )
				{
					MessageBox . Show ( "An Edit Window is already open.\nYou can only have one open at any one time !" );
					Flags . CurrentEditDbViewer . BringIntoView ( );
					Flags . CurrentEditDbViewer . Focus ( );
					return;
				}
				/// ViewEdit is just a wrapper for EditDb
				edb = new EditDb ( "BANKACCOUNT", this . BankGrid . SelectedIndex, this . BankGrid . SelectedItem, this );
				edb . Owner = this;

				edb . Show ( );
				Flags . BankEditDb = edb;
				RefreshBtn . IsEnabled = false;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				if ( Flags . CustEditDb != null ) { Flags . CustEditDb . Focus ( ); Flags . CustEditDb . BringIntoView ( ); return; }
				edb = new EditDb ( "CUSTOMER", this . CustomerGrid . SelectedIndex, this . CustomerGrid . SelectedItem, this );
				edb . Owner = this;
				edb . Show ( );
				//				ExtensionMethods . Refresh ( edb );
				edb . Refresh ( );
				Flags . CustEditDb = edb;
				RefreshBtn . IsEnabled = false;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				if ( Flags . DetEditDb != null ) { Flags . DetEditDb . Focus ( ); Flags . DetEditDb . BringIntoView ( ); return; }
				edb = new EditDb ( "DETAILS", this . DetailsGrid . SelectedIndex, this . DetailsGrid . SelectedItem, this );
				edb . Owner = this;
				edb . Show ( );
				//				ExtensionMethods . Refresh ( edb );
				edb . Refresh ( );
				Flags . DetEditDb = edb;
				RefreshBtn . IsEnabled = false;
			}
		}


		//*************************************************************************************************************//
		private void ItemsView_OnSelectionChanged ( object sender, SelectionChangedEventArgs e )
		//User has clicked a row in our DataGrid// OR in EditDb grid
		{
			int index = 0;
			string SearchCustNo = "";
			string SearchBankNo = "";
			EditDb edb = new EditDb ( );
			bool UpdateInProgress = false;

			if ( RefreshInProgress ) return;
			if ( UpdateInProgress ) return;
			if ( LoadingDbData )
			{
				LoadingDbData = false;
				return;
			}

				e . Handled = true;
			OnSelectionChangedInProgress = true;

			//ENTRY POINT WHEN WE CHANGE THE INDEX	 Or change data, or when ItemsSource is set as well  it seems
			// It is different processing if an EditDb window is open !!
			if ( Flags . CurrentEditDbViewer != null )
			{
				//There is an EditDb window open, so this will trigger
				//an event that lets the DataGrid in the EditDb class
				// change it's own index internally

				if ( CurrentDb == "BANKACCOUNT" )
				{
					index = this . BankGrid . SelectedIndex;
					//					this . BankGrid . UnselectAll ( );
					//					if ( index == -1 ) index = 0;
					//					this . BankGrid . SelectedItem = index;

					//Get the CustNo data to pass to other viewers for their search
					CurrentBankSelectedRecord = this . BankGrid . SelectedItem as BankAccountViewModel;
					if ( CurrentBankSelectedRecord == null ) { OnSelectionChangedInProgress = true; return; }
					SearchCustNo = CurrentBankSelectedRecord . CustNo;
					SearchBankNo = CurrentBankSelectedRecord . BankNo;

					//We  MUST  still send an event Trigger for any other open viewers
					if ( Flags . LinkviewerRecords )
					{
						// We trigger with full search data included so other viewers can
						TriggerViewerIndexChanged ( this . BankGrid );
					}

					// finally Update ONLY any EditDb windows that are open
					//					if (LinkRecords)
					EventControl . TriggerForceEditDbIndexChanged ( this, new IndexChangedArgs
					{
						Bankno = SearchBankNo,
						Custno = SearchCustNo,
						Row = index,
						Senderviewer = this,
						SenderId = "BANKACCOUNT",
						dGrid = this . BankGrid,
						Sender = "BANKACCOUNT"
					} );
					RefreshInProgress = true;
					this . BankGrid . UnselectAll ( );
					if ( index == -1 ) index = 0;
					this . BankGrid . SelectedItem = index;
					Utils . SetUpGridSelection ( this . BankGrid, index );
					//// Updates  the MainWindow.gv[] structure
					UpdateRowDetails ( this . BankGrid . SelectedItem, "BankGrid" );
					RefreshInProgress = false;
					OnSelectionChangedInProgress = false;
					IsDirty = false;

					//					Debug . WriteLine ( $" *** TRACE 1-0 *** SQLDBVIEWER : Itemsview_OnSelectionChanged  BANKACCOUNT - Index = {this . BankGrid . SelectedIndex}" );
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					index = this . CustomerGrid . SelectedIndex;
					//					this . CustomerGrid . UnselectAll ( );
					if ( index == -1 ) index = 0;
					this . CustomerGrid . SelectedItem = index;

					//Get the CustNo data to pass ot other viewers for their search
					CurrentCustomerSelectedRecord = this . CustomerGrid . SelectedItem as CustomerViewModel;
					if ( CurrentCustomerSelectedRecord == null ) return;
					SearchCustNo = CurrentCustomerSelectedRecord . CustNo;
					SearchBankNo = CurrentCustomerSelectedRecord . BankNo;

					// Now handle updating to other window Viewers
					if ( Flags . LinkviewerRecords )
					{
						// We still trigger with full search data included so other viewers can
						TriggerViewerIndexChanged ( this . CustomerGrid );
					}
					if ( UpdateInProgress )
						// finally Update ONLY any EditDb windows that are open
						EventControl . TriggerForceEditDbIndexChanged ( this, new IndexChangedArgs
						{
							Bankno = SearchBankNo,
							Custno = SearchCustNo,
							Row = index,
							Senderviewer = this,
							SenderId = "CUSTOMER",
							dGrid = this . CustomerGrid,
							Sender = "CUSTOMER"
						} );
					Utils . SetUpGridSelection ( this . CustomerGrid, index );
					//// Updates  the MainWindow.gv[] structure
					UpdateRowDetails ( this . CustomerGrid . SelectedItem, "CustomerGrid" );
					IsDirty = false;
					//					Debug . WriteLine ( $" *** TRACE 1-1***  SQLDBVIEWER : Itemsview_OnSelectionChanged  CUSTOMER - Index = {this . CustomerGrid . SelectedIndex}" );
				}
				else if ( CurrentDb == "DETAILS" )
				{
					index = this . DetailsGrid . SelectedIndex;
					//					this . DetailsGrid . UnselectAll ( );
					if ( index == -1 ) index = 0;
					this . DetailsGrid . SelectedItem = index;
					//					Debug . WriteLine ( $" *** TRACE 1-2 *** SQLDBVIEWER : Itemsview_OnSelectionChanged  DETAILS - ENTERING  {this . DetailsGrid . SelectedIndex}, {this . DetailsGrid . SelectedItem}" );
					//Get the CustNo data to pass ot other viewers for their search
					CurrentDetailsSelectedRecord = this . DetailsGrid . SelectedItem as DetailsViewModel;
					if ( CurrentDetailsSelectedRecord == null ) return;
					SearchCustNo = CurrentDetailsSelectedRecord . CustNo;
					SearchBankNo = CurrentDetailsSelectedRecord . BankNo;

					// Now handle updating to other window Viewers
					if ( Flags . LinkviewerRecords )
					{
						// We must trigger with full search data included so other viewers can
						TriggerViewerIndexChanged ( this . DetailsGrid );
					}
//					if ( UpdateInProgress )
						// finally Update ONLY any EditDb windows that are open
						// They can toggle the setting on/off
						EventControl . TriggerForceEditDbIndexChanged ( this, new IndexChangedArgs
						{
							Bankno = SearchBankNo,
							Custno = SearchCustNo,
							Row = index,
							Senderviewer = this,
							SenderId = "DETAILS",
							dGrid = this . DetailsGrid,
							Sender = "DETAILS"
						} );

					Utils . SetUpGridSelection ( this . DetailsGrid, index );
					//// Updates  the MainWindow.gv[] structure
					UpdateRowDetails ( this . DetailsGrid . SelectedItem, "DetailsGrid" );
					//					Debug . WriteLine ( $" *** TRACE 1-2 *** SQLDBVIEWER : Itemsview_OnSelectionChanged  DETAILS - Index = {this . DetailsGrid . SelectedIndex}, {this . DetailsGrid . SelectedItem}" );
					IsDirty = false;
				}

			}
			else
			{
				if ( CurrentDb == "BANKACCOUNT" )
				{
					if ( this . BankGrid . SelectedItem != null )
					{
						//Get the NEW selected index
						index = this . BankGrid . SelectedIndex;
						if ( index == -1 ) index = 0;
						//Get the CustNo data to pass ot other viewers for their search
						CurrentBankSelectedRecord = this . BankGrid . SelectedItem as BankAccountViewModel;
						SearchCustNo = CurrentBankSelectedRecord . CustNo;
						SearchBankNo = CurrentBankSelectedRecord . BankNo;
						// Updates  the MainWindow.gv[] structure		&& ViewersList entry
						if ( Flags . UpdateInProgress == false )
							UpdateRowDetails ( this . BankGrid . SelectedItem, "BankGrid" );
						Flags . SqlViewerIndexIsChanging = false;
						IsDirty = false;
						if ( Flags . LinkviewerRecords && Triggered == false )
						{
							TriggerViewerIndexChanged ( this . BankGrid );
						}
					}
					Triggered = false;
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					if ( this . CustomerGrid . SelectedItem != null )
					{
						index = this . CustomerGrid . SelectedIndex;
						if ( index == -1 ) index = 0;
						CurrentCustomerSelectedRecord = this . CustomerGrid . SelectedItem as CustomerViewModel;
						SearchCustNo = CurrentCustomerSelectedRecord . CustNo;
						SearchBankNo = CurrentCustomerSelectedRecord . BankNo;
						// Updates  the MainWindow.gv[] structure
						if ( Flags . UpdateInProgress == false )
							UpdateRowDetails ( this . CustomerGrid . SelectedItem, "CustomerGrid" );
						IsDirty = false;
						Flags . SqlViewerIndexIsChanging = false;
						//						if ( Flags . LinkviewerRecords )
						if ( Flags . LinkviewerRecords && Triggered == false )
						{
							TriggerViewerIndexChanged ( this . CustomerGrid );
						}
					}
					Triggered = false;
				}
				else if ( CurrentDb == "DETAILS" )
				{
					if ( this . DetailsGrid . SelectedItem != null )
					{
						index = this . DetailsGrid . SelectedIndex;
						if ( index == -1 ) index = 0;
						CurrentDetailsSelectedRecord = this . DetailsGrid . SelectedItem as DetailsViewModel;
						// This creates a new entry in gv[] if this is a new window being loaded
						SearchCustNo = CurrentDetailsSelectedRecord . CustNo;
						SearchBankNo = CurrentDetailsSelectedRecord . BankNo;
						// Updates  the MainWindow.gv[] structure
						if ( Flags . UpdateInProgress == false )
							UpdateRowDetails ( this . DetailsGrid . SelectedItem, "DetailsGrid" );
						IsDirty = false;
						Flags . SqlViewerIndexIsChanging = false;
						if ( Flags . LinkviewerRecords && Triggered == false )
						{
							TriggerViewerIndexChanged ( this . DetailsGrid );
						}
					}
					Triggered = false;
				}
			}
			UpdateAuxilliaries ( "" );
			Mouse . OverrideCursor = Cursors . Arrow;
			EditChangeType = 0;
			ViewerChangeType = 0;
			UpdateInProgress = false;
			try
			{
				//				e . Handled = true;
			}
			catch ( Exception ex ) { }
			// EXIT POINT WHEN VIEWER HAS CHANGED INDEX SELECTION
			//this . Activate ( );
		}

		//*************************************************************************************************************//
		public static void SelectCurrentRowByIndex ( DataGrid dataGrid, int rowIndex )
		{
			DataGridRow row = dataGrid . ItemContainerGenerator . ContainerFromIndex ( rowIndex ) as DataGridRow;
			if ( row != null )
			{
				Debug . WriteLine ( $"row.Focus failed" );
				int y = 0;
			}
		}

		//*******************************************************************************************************//
		//*************************************************************************************************************//
		public void UpdateAuxilliaries ( string comment )
		{
			//Application.Current.Dispatcher.Invoke (() =>
			ParseButtonText ( true );
			ResetOptionButtons ( 1 );
			UpdateDbSelectorBtns ( Flags . CurrentSqlViewer );
			// Update DbSelector ListBoxItems structure and our GridViewer control Structure
			if ( IsViewerLoaded == false )
				UpdateGridviewController ( CurrentDb );
			// Reset ucilliary Buttons
			ResetauxilliaryButtons ( );
#pragma NOT NEEDED ????
			//			WaitMessage . Visibility = Visibility . Collapsed;
		}

		//*******************************************************************************************************//

		#region CheckBoxhandlers


		#endregion CheckBoxhandlers

		//*********************************************************************************************************//

		#region GetSqlInstance - Fn to allow me to call standard merthods from inside a Static method

		//*****************************************************//
		//this is really clever stuff
		// It lets me call standard methods (private, public, protected etc)
		//from INSIDE a Static method
		// using syntax : GetSqlInstance().MethodToCall();
		//and it works really great
		private static SqlDbViewer _Instance;


		#endregion GetSqlInstance - Fn to allow me to call standard merthods from inside a Static method


		#region Filter code

		//*************************************************************************************************************//
		private void SetFilter_Click ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registered" cos we can
			// Click the button before the window has had focus set
			Flags . CurrentSqlViewer = this;
			// Call up the Filtering Window to select
			// the filtering conditions required
			Window_GotFocus ( sender, null );

			if ( !Flags . IsFiltered )
			{
				Filtering f = new Filtering ( );
				Flags . FilterCommand = f . DoFilters ( this, CurrentDb, 0 );
				// clear any previous filter command line data
				if ( Flags . FilterCommand == "" ) return;
				if ( CurrentDb == "BANKACCOUNT" )
					ShowBank_Click ( null, null );
				else if ( CurrentDb == "CUSTOMER" )
					ShowCust_Click ( null, null );
				else if ( CurrentDb == "DETAILS" )
					ShowDetails_Click ( null, null );

				ControlTemplate ctmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGreen" );
				Filters . Template = ctmp;
				Brush brs = Utils . GetDictionaryBrush ( "HeaderBrushGreen" );
				Filters . Background = brs;

				Filters . Content = "Show All";
				ParseButtonText ( true );
				StatusBar . Text = $"Data Filtered on : [{Flags . FilterCommand}]";
			}
			else
			{
				//Need this flag set to allow the Show() method to complete anew load of the data
				Flags . IsFiltered = true;
				Flags . FilterCommand = "";
				if ( CurrentDb == "BANKACCOUNT" )
					ShowBank_Click ( null, null );
				else if ( CurrentDb == "CUSTOMER" )
					ShowCust_Click ( null, null );
				else if ( CurrentDb == "DETAILS" )
					ShowDetails_Click ( null, null );

				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				Filters . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
				Filters . Background = br;
				Filters . IsEnabled = false;
				Filters . IsEnabled = true;
				Filters . Content = "Filter";
				Flags . FilterCommand = "";
				Flags . IsFiltered = false;
				ParseButtonText ( true );
				StatusBar . Text = $"All Data Records are shown... ";
			}
			Mouse . OverrideCursor = Cursors . Arrow;
			return;
		}

		//*****************************************************************************************//
		//*****************************************************************************************//
		/// <summary>
		/// Just sets up the Filter/Duplicate buttons status
		/// </summary>
		//*************************************************************************************************************//
		private void ResetOptionButtons ( int mode )
		{
			if ( mode == 0 )
			{
				if ( !Filters . IsEnabled )
				{
					Filters . Content = "Show All";
					//					Filters . IsEnabled = true;
				}
				else
				{
					//					Filters . IsEnabled = false;
					Filters . Content = "Filter";
				}
			}
			else
			{
				if ( Flags . IsMultiMode )
				{
					Multiaccounts . Content = "Show All";
				}
				else
				{
					Multiaccounts . Content = " Multiple A'C's Only";
				}
			}
		}

		//*************************************************************************************************************//
		private void ResetauxilliaryButtons ( )
		{
			//ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
			//Filters . Template = tmp;
			//Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
			//Filters . Background = br;
			//Filters . Content = "Filter";
		}

		//*************************************************************************************************************//
		public void SetButtonGradientBackground ( Button btn )
		{
			// how to change button background to a style in Code
			if ( btn == Filters )
			{
				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				btn . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
				btn . Background = br;
				btn . Content = "Filter";
			}
			if ( btn == Multiaccounts )
			{
				if ( Flags . IsMultiMode )
				{
					ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateRed" );
					btn . Template = tmp;
					Brush br = Utils . GetDictionaryBrush ( "HeaderBrushRed" );
					btn . Background = br;
					btn . Content = "Show All";
					StatusBar . Text = "All Database records are shown...";
				}
				else
				{
					ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
					btn . Template = tmp;
					Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
					btn . Background = br;
					btn . Content = "Multiple A/C's Only";
					StatusBar . Text = "Database is filtered to show Customers with Multiple aA/C's only...";
				}
			}
		}

		#endregion Filter code

		#region UPDATE NOTIFICATION HANDLERS

		/// <summary>
		/// Called by each ViewModel to handle loading new data into the relevant datagrid
		/// </summary>
		/// <param name="Viewer"></param>
		/// <param name="Grid"></param>
		/// <param name="args"></param>
		///
		///	UNUSED RIGHT NOW
		///
		//*************************************************************************************************************//
		public async Task ReloadBankOnUpdateNotification ( SqlDbViewer Viewer, DataGrid Grid, DataChangeArgs args )
		{
			//DataGrid CurrentGrid = null;
			//// This gets called irrespective of who it triggered  it, so do not obther if the call was by a Bank Grid
			//if ( Viewer . CurrentDb == "BANKACCOUNT" )
			//	return;

			//if ( Flags . SqlBankViewer == null ) return;
			//Flags . SqlBankViewer . Focus ( );
			//CurrentGrid = Flags . SqlBankViewer . BankGrid;
			//int currindx = CurrentGrid . SelectedIndex != -1 ? CurrentGrid . SelectedIndex : 0;
			//Debug . WriteLine ( $"\nnREFRESHING { args . DbName}. SelectedIndex = {currindx}\n" );//\nIn Details update:\nCurrentViewer Tag = {this . Tag}\nFlags . CurrentSqlViewer?.Tag = {Flags . CurrentSqlViewer?.Tag}" );
			//if ( currindx == -1 )
			//	currindx = 0;

			//// Toggle ItemsSource to refresh datagrid
			//CurrentGrid . ItemsSource = null;
			//SqlViewerBankcollection = await BankCollection . LoadBank ( SqlViewerBankcollection, 1, true );

			//Debug . WriteLine ( $"returned from Task loading Bank Details data ...." );
			//CurrentGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerBankcollection );
			//CurrentGrid . SelectedIndex = currindx;
			//CurrentGrid . SelectedItem = currindx;
			////			ExtensionMethods . Refresh ( CurrentGrid );
			//CurrentGrid . Refresh ( );
			//BankAccountViewModel data = CurrentGrid . SelectedItem as BankAccountViewModel;
			//StatusBar . Text = $"Data reloaded due to external changes for Customer # {data?.CustNo}, Bank A/C # {data?.BankNo}";
			//Debug . WriteLine ( $"\nCompleted REFRESHING { args . DbName}. SelectedIndex set to {currindx}\nCustNo = {data . CustNo}, BankNo = { data . BankNo}, AcType = {data . AcType}" );
			//if ( args . DbName == "BANKACCOUNT" )
			//{
			//	Flags . ActiveSqlViewer = Flags . SqlBankViewer;
			//	Flags . CurrentSqlViewer = Flags . SqlBankViewer;
			//}
			//ParseButtonText ( true );
			//Count . Text = this . BankGrid . Items . Count . ToString ( );
		}



		//*************************************************************************************************************//
		public async void UpdateDetailsOnEditDbChange ( string currentDb, int rowindex, object rowitem )
		{
			DataGrid dataGrid = null;
			Debug . WriteLine ( $"\nREMOTE DATA CHANGE HAS OCCURRED in {currentDb}\n" );
			dataGrid = DetailsGrid;
			SqlDbViewer ViewerPtr = this;
			int currindx = rowindex != -1 ? rowindex : 0;

			if ( dataGrid . Items . NeedsRefresh )
			{
				dataGrid . Refresh ( );
				dataGrid . ItemsSource = SqlDetcollection;
				dataGrid . SelectedIndex = currindx;
			}
			else
			{
				dataGrid . ItemsSource = null;
				Mouse . OverrideCursor = Cursors . Wait;
				await DetailCollection . LoadDet ( SqlDetcollection, "SQLDBVIEWER", 1, true );
				dataGrid . ItemsSource = SqlDetcollection;
				dataGrid . SelectedIndex = currindx;
				Mouse . OverrideCursor = Cursors . Arrow;
			}

			DetailsViewModel data = this . DetailsGrid . SelectedItem as DetailsViewModel;
			StatusBar . Text = $"Data reloaded due to external changes in { CurrentDb} Viewer for Customer # {data?.CustNo}, Bank A/C # {data?.BankNo}";
			dataGrid . Refresh ( );
		}

		#endregion UPDATE NOTIFICATION HANDLERS


		#region NotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		//*************************************************************************************************************//
		private void OnPropertyChanged ( string PropertyName = null )
		{
			PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( PropertyName ) );
		}

		#endregion NotifyPropertyChanged


		#region PREVIEW Mouse METHODS

		//*************************************************************************************************************//
		//*************************************************************************************************************//
		private async void CustomerGrid_PreviewMouseDown ( object sender, MouseButtonEventArgs e )
		{
			// handle flags to let us know WE have triggered the selectedIndex change
			//MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			if ( e . ChangedButton == MouseButton . Right )
			{
				DataGridRow RowData = new DataGridRow ( );
				int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
				if ( row == -1 ) row = 0;
				RowInfoPopup rip = new RowInfoPopup ( "CUSTOMER", CustomerGrid, RowData );
				rip . Topmost = true;
				rip . DataContext = RowData;
				rip . BringIntoView ( );
				rip . Focus ( );
				rip . ShowDialog ( );

				Flags . ActiveSqlViewer = this;
				Flags . CurrentSqlViewer = this;
				//If data has been changed, update everywhere
				// Update the row on return in case it has been changed
				if ( rip . IsDirty )
				{
					// This is done in RowPopup()
					//CustomerViewModel bvm = new CustomerViewModel ( );
					//cvm = this . CustomerGrid . SelectedItem as CustomerViewModel;
					//SQLHandlers sqlh = new SQLHandlers ( );
					//sqlh . UpdateDbRowAsync ( "CUSTOMER", cvm );
					this . CustomerGrid . ItemsSource = null;
					this . CustomerGrid . Items . Clear ( );
					await CustCollection . LoadCust ( SqlCustcollection, "SQLDBVIEWER", 1, true );
					//this . CustomerGrid . ItemsSource = SqlViewerCustcollection;
					//StatusBar . Text = "Current Record Updated Successfully...";
					//// Notify everyone else of the data change
					//EventControl . TriggerDataUpdated ( SqlViewerCustcollection,
					//	new LoadedEventArgs
					//	{
					//		CallerDb = "CUSTOMER",
					//		DataSource = SqlViewerCustcollection,
					//		RowCount = this . CustomerGrid . SelectedIndex
					//	} );
				}
				//else
				//	this . CustomerGrid . SelectedItem = RowData . Item;

				// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
				Utils . SetUpGridSelection ( this . BankGrid, row );
				ParseButtonText ( true );
				Count . Text = this . CustomerGrid . Items . Count . ToString ( );
				// This is essential to get selection activated again
				this . CustomerGrid . Focus ( );
			}
		}

		//*************************************************************************************************************//
		private async void DetailsGrid_PreviewMouseDown_1 ( object sender, MouseButtonEventArgs e )
		{
			//			int currsel = DetailsGrid.SelectedIndex;
			// handle flags to let us know WE have triggered the selectedIndex change
			//			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			try
			{
				if ( e . ChangedButton == MouseButton . Right )
				{
					DataGridRow RowData = new DataGridRow ( );
					int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
					if ( row == -1 ) row = 0;
					RowInfoPopup rip = new RowInfoPopup ( "DETAILS", DetailsGrid, RowData );
					rip . DataContext = RowData;
					rip . BringIntoView ( );
					rip . Focus ( );
					rip . Topmost = true;
					rip . ShowDialog ( );

					Flags . ActiveSqlViewer = this;
					Flags . CurrentSqlViewer = this;
					//If data has been changed, update everywhere
					this . DetailsGrid . SelectedItem = RowData . Item;
					if ( rip . IsDirty )
					{
						DetailsViewModel bvm = new DetailsViewModel ( );
						dvm = this . DetailsGrid . SelectedItem as DetailsViewModel;
						SQLHandlers sqlh = new SQLHandlers ( );
						sqlh . UpdateDbRowAsync ( "DETAILS", dvm );
						this . DetailsGrid . ItemsSource = null;
						this . DetailsGrid . Items . Clear ( );
						await DetailCollection . LoadDet ( SqlDetcollection, "SQLDBVIEWER", 1, true );
						//this . DetailsGrid . ItemsSource = SqlViewerDetcollection;
						//StatusBar . Text = "Current Record Updated Successfully...";
						//// Notify everyone else of the data change
						//EventControl . TriggerDataUpdated ( SqlViewerDetcollection,
						//	new LoadedEventArgs
						//	{
						//		CallerDb = "DETAILS",
						//		DataSource = SqlViewerDetcollection,
						//		RowCount = this . DetailsGrid . SelectedIndex
						//	} );
					}
					//else
					//	this . DetailsGrid . SelectedIndex = row;

					// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
					//Utils . GridInitialSetup ( DetailsGrid, row );
					//ParseButtonText ( true );
					//Count . Text = this . DetailsGrid . Items . Count . ToString ( );
					//// This is essential to get selection activated again
					//this . DetailsGrid . Focus ( );
				}
				else
				{
					// Left button clicked
					e . Handled = false;
				}

			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"General Exception : {ex . Message}, {ex . Data}" );
			}
		}

		#endregion PREVIEW Mouse METHODS

		#region REFRESH FUNCTIONALITY
		//*************************************************************************************************************//
		private void Refresh_Click ( object sender, RoutedEventArgs e )
		{
			int currsel = 0;
			Mouse . OverrideCursor = Cursors . Wait;
			RefreshInProgress = true;
			ReloadGrid ( this, this . BankGrid );
			Mouse . OverrideCursor = Cursors . Arrow;
			return;
		}

		/// <summary>
		/// Method called by the Refresh button on a viewer to hande the repositioning
		/// of the selection and top visible row in the
		/// </summary>
		/// <param name="viewer"></param>
		/// <param name="DGrid"></param>
		/// <returns></returns>
		//*************************************************************************************************************//
		public async void ReloadGrid ( SqlDbViewer viewer, DataGrid DGrid )
		{
			//			int topvisible = 0;
			//			int bottonvisible = 0;
			try
			{
				Mouse . OverrideCursor = Cursors . Wait;
				// Make sure we are back on UI thread
				Debug . WriteLine ( $"Before thread switchback call	: Thread = { Thread . CurrentThread . ManagedThreadId}" );
				Debug . WriteLine ( $"Reloading All Data...\n" );
				int current = 0;
				current = DGrid . SelectedIndex == -1 ? 0 : DGrid . SelectedIndex;
				if ( CurrentDb == "BANKACCOUNT" )
				{
					BankAccountViewModel bvm = new BankAccountViewModel ( );
					if ( bvm == null ) return;
					int bbindex = this . BankGrid . SelectedIndex;
					this . BankGrid . SelectedItem = bbindex;
					bvm = this . BankGrid . SelectedItem as BankAccountViewModel;
					if ( bvm == null ) return;
					this . BankGrid . ItemsSource = null;
					this . BankGrid . Items . Clear ( );
					SqlBankcollection = null;

					await BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );
					//this . BankGrid . ItemsSource = SqlViewerBankcollection;
					//bbindex = Utils . FindMatchingRecord ( bvm . CustNo, bvm . BankNo, this . BankGrid, "BANKACCOUNT" );
					//Utils . SetUpGridSelection ( this . BankGrid, bbindex );
					//ParseButtonText ( true );
					//Count . Text = this . BankGrid . Items . Count . ToString ( );
					//this . BankGrid . Focus ( );
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					CustomerViewModel bvm = new CustomerViewModel ( );
					if ( bvm == null ) return;
					int bbindex = this . CustomerGrid . SelectedIndex;
					this . CustomerGrid . SelectedItem = bbindex;
					bvm = this . CustomerGrid . SelectedItem as CustomerViewModel;
					if ( bvm == null ) return;
					this . CustomerGrid . ItemsSource = null;
					this . CustomerGrid . Items . Clear ( );
					SqlCustcollection = null;

					await CustCollection . LoadCust ( SqlCustcollection, "SQLDBVIEWER", 1, true );
					//this . CustomerGrid . ItemsSource = SqlViewerCustcollection;
					//bbindex = Utils . FindMatchingRecord ( bvm . CustNo, bvm . BankNo, this . CustomerGrid, "CUSTOMER" );
					//Utils . SetUpGridSelection ( this . CustomerGrid, bbindex );
					//ParseButtonText ( true );
					//Count . Text = this . CustomerGrid . Items . Count . ToString ( );
					//this . CustomerGrid . Focus ( );
				}
				else if ( CurrentDb == "DETAILS" )
				{
					DetailsViewModel bvm = new DetailsViewModel ( );
					if ( bvm == null ) return;
					int bbindex = this . DetailsGrid . SelectedIndex;
					this . DetailsGrid . SelectedItem = bbindex;
					bvm = this . DetailsGrid . SelectedItem as DetailsViewModel;
					if ( bvm == null ) return;
					this . DetailsGrid . ItemsSource = null;
					this . DetailsGrid . Items . Clear ( );
					SqlDetcollection = null;

					await DetailCollection . LoadDet ( SqlDetcollection, "SQLDBVIEWER", 1, true );
					//this . DetailsGrid . ItemsSource = SqlViewerDetcollection;
					//bbindex = Utils . FindMatchingRecord ( bvm . CustNo, bvm . BankNo, this . BankGrid, "BANKACCOUNT" );
					//Utils . SetUpGridSelection ( this . BankGrid, bbindex );
					//ParseButtonText ( true );
					//Count . Text = this . DetailsGrid . Items . Count . ToString ( );
					//this . DetailsGrid . Focus ( );
				}
				DGrid . SelectedIndex = current;
				Debug . WriteLine ( $"End of ReloadGrid() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
				Mouse . OverrideCursor = Cursors . Arrow;
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"ERROR: ReloadGrid() {ex . Message}, : {ex . Data}" );
			}
			//return true;
		}

		#endregion REFRESH FUNCTIONALITY

		#region Scroll bar utilities

		// scroll bar movement is automatically   stored by these three methods
		// So we can use them to reset position CORRECTLY after refreshes
		//*************************************************************************************************************//
		private void BankGrid_ScrollChanged ( object sender, ScrollChangedEventArgs e )
		{
			if ( RefreshInProgress ) return;
			DataGrid dg = null;
			dg = sender as DataGrid;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			scroll . CanContentScroll = true;
			SetScrollVariables ( sender );
		}

		//*************************************************************************************************************//
		private void CustomerGrid_ScrollChanged ( object sender, ScrollChangedEventArgs e )
		{
			if ( RefreshInProgress ) return;
			DataGrid dg = null;
			dg = sender as DataGrid;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			scroll . CanContentScroll = true;
			SetScrollVariables ( sender );
		}

		//*************************************************************************************************************//
		private void DetailsGrid_ScrollChanged ( object sender, ScrollChangedEventArgs e )
		{
			if ( RefreshInProgress ) return;
			DataGrid dg = null;
			dg = sender as DataGrid;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			scroll . CanContentScroll = true;
			SetScrollVariables ( sender );
		}

		//*************************************************************************************************************//
		public void SetScrollToTop ( DataGrid sender )
		{
			SqlDbViewer sqlv = new SqlDbViewer ( );
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) sender );
			if ( scroll != null )
			{
				if ( sender == this . BankGrid )
				{
					scroll . ScrollToVerticalOffset ( ScrollData . Banktop );
				}
				else if ( sender == this . CustomerGrid )
				{
					//if ( ScrollData . Custtop > 5 )
					//	scroll . ScrollToVerticalOffset ( ScrollData . Custtop - ( ScrollData.CustVisible/2) );
					//else
					scroll . ScrollToVerticalOffset ( ScrollData . Custtop );
				}
				else if ( sender == this . DetailsGrid )
				{
					//if ( ScrollData . Dettop > 5 )
					//	scroll . ScrollToVerticalOffset ( ScrollData . Dettop - ( ScrollData . DetVisible / 2 ) );
					//else
					scroll . ScrollToVerticalOffset ( ScrollData . Dettop );
					Console . WriteLine ( $"Setting Dettop to {ScrollData . Dettop}" );

				}
			}
		}
		/// <summary>
		/// Calls three methods that store the scrollbar positions for a supplied datagrid
		/// </summary>
		/// <param name="sender"></param>
		//*************************************************************************************************************//
		public void SetScrollVariables ( object sender )
		{
			SetTopViewRow ( sender );
			SetBottomViewRow ( sender );
			SetViewPort ( sender );
		}
		//*************************************************************************************************************//
		public void SetTopViewRow ( object sender )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			if ( dg . SelectedItem == null ) return;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			if ( scroll == null ) return;
			double d = scroll . VerticalOffset;
			int rounded = Convert . ToInt32 ( d );
			if ( dg == BankGrid )
			{
				Flags . TopVisibleBankGridRow = ( double ) rounded;
				ScrollData . Banktop = ( double ) rounded;
			}
			else if ( dg == CustomerGrid )
			{
				Flags . TopVisibleCustGridRow = ( double ) rounded;
				ScrollData . Custtop = ( double ) rounded;
			}
			else if ( dg == DetailsGrid )
			{
				Flags . TopVisibleDetGridRow = ( double ) rounded;
				ScrollData . Dettop = ( double ) rounded;
			}
		}

		//*************************************************************************************************************//
		public void SetBottomViewRow ( object sender )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			if ( dg . SelectedItem == null ) return;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			if ( scroll == null ) return;
			scroll . CanContentScroll = true;
			double d = scroll . VerticalOffset;
			int rounded = Convert . ToInt32 ( d );
			if ( dg . Name == "BankGrid" )
			{
				Flags . BottomVisibleBankGridRow = ( double ) rounded;
				ScrollData . Bankbottom = ( double ) rounded;
			}
			else if ( dg . Name == "CustomerGrid" )
			{
				Flags . BottomVisibleCustGridRow = ( double ) rounded;
				ScrollData . Custbottom = ( double ) rounded;
			}
			else if ( dg . Name == "DetailsGrid" )
			{
				Flags . BottomVisibleDetGridRow = ( double ) rounded;
				ScrollData . Detbottom = ( double ) rounded;
			}
		}

		//*************************************************************************************************************//
		public void SetViewPort ( object sender )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			if ( dg . SelectedItem == null ) return;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			if ( scroll == null ) return;
			scroll . CanContentScroll = true;
			Flags . ViewPortHeight = scroll . ViewportHeight;

			if ( sender == this . BankGrid )
				ScrollData . BankVisible = ( double ) scroll . ViewportHeight;
			else if ( sender == this . CustomerGrid )
				ScrollData . CustVisible = ( double ) scroll . ViewportHeight;
			else if ( sender == this . DetailsGrid )
				ScrollData . DetVisible = ( double ) scroll . ViewportHeight;

		}

		//*************************************************************************************************************//
		public void PrintCurrentviewportdata ( object sender )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			if ( dg == null ) return;
			if ( dg . SelectedItem == null ) return;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) sender );
			scroll . CanContentScroll = true;
			Debug . WriteLine (
			$"Top Visible Row		: Flag : {Flags . TopVisibleBankGridRow }	: Actual : {scroll . VerticalOffset}\n" +
			$"Bottom Visible Row	: Flag : {Flags . BottomVisibleBankGridRow }	: Actual : {scroll . VerticalOffset + scroll . ViewportHeight + 1}\n" +
			$"ViewPort : {scroll . ViewportHeight}\n" );
		}


		//*************************************************************************************************************//
		public void Scroll_ScrollChanged ( object sender, ScrollChangedEventArgs e )
		{
			ScrollViewer scroll = sender as ScrollViewer;
			double diff = e . ExtentHeight;
			double diff2 = e . VerticalOffset;
			double Diff = diff - diff2;
			double vp1 = e . ViewportHeight;
			double vp2 = e . ViewportHeightChange;
			double Vp = vp1 - vp2;
			double newoffset = diff - Vp;

			if ( Diff < Vp )
				scroll . ScrollToVerticalOffset ( newoffset );
		}

		//*************************************************************************************************************//
		private void DetailsGrid_Scroll ( object sender, System . Windows . Controls . Primitives . ScrollEventArgs e )
		{
			// This is called when scrolbar moves
			double x = e . NewValue;
		}

		#endregion Scroll bar utilities

		#region KEYBOARD DELEGATES & Window keyboard handler for SqlDbViewer class
		//*************************************************************************************************************//

		public delegate void KeyboardDelegate ( int x );

		#region delegate methods to be passed

		public void KeyboardDelegate1 ( int y )
		{ }

		public void KeyboardDelegate2 ( int y )
		{ }

		public void KeyboardDelegate3 ( int y )
		{ }

		public void KeyboardDelegate4 ( int y )
		{ }

		public void KeyboardDelegate5 ( int y )
		{ }

		public void KeyboardDelegate6 ( int y )
		{ }

		public void KeyboardDelegate7 ( int y )
		{ }

		public void KeyboardDelegate8 ( int y )
		{ }

		/// <summary>
		///  These are DELEGATE METHODS that are here to be called from Keyboard shortcuts
		///  to extend the functionlaity of the shortcuts
		/// </summary>
		/// <param name="x"></param>
		//*************************************************************************************************************//
		public void KeyboardDelegate9 ( int y )
		{
			Debug . WriteLine ( $"\n\nThis is the end Method called via a delegate via by a keyboard shortcut., cute eh ??" );
			string output = "";
			for ( int x = 0 ; x < 100 ; x++ )
			{
				output = ".";
				Console . Write ( output );
			}
			Debug . WriteLine ( "\nFlags Variables" );
			Flags . ShowAllFlags ( );
		}

		#endregion delegate methods to be passed

		/// <summary>
		///  Delegate that is called from keyboard shortcuts and allows us to choose the relevant functionality
		///  via the parameter vlaue (1-9)we received from the keyboard
		/// </summary>
		//*************************************************************************************************************//
		public void DelegateMaster ( int x )
		{
			Debug . WriteLine ( $"This is a delegated Method called  by a keyboard shortcut....." );
			switch ( x )
			{
				case 1:
					PostDelegateMethod ( 1, KeyboardDelegate1 );
					break;

				case 2:
					PostDelegateMethod ( 2, KeyboardDelegate2 );
					break;

				case 3:
					PostDelegateMethod ( 3, KeyboardDelegate3 );
					break;

				case 4:
					PostDelegateMethod ( 4, KeyboardDelegate4 );
					break;

				case 5:
					PostDelegateMethod ( 5, KeyboardDelegate5 );
					break;

				case 6:
					PostDelegateMethod ( 6, KeyboardDelegate6 );
					break;

				case 7:
					PostDelegateMethod ( 7, KeyboardDelegate7 );
					break;

				case 8:
					PostDelegateMethod ( 8, KeyboardDelegate8 );
					break;

				case 9:
					PostDelegateMethod ( 9, KeyboardDelegate9 );
					break;
			}
		}

		/// <summary>
		/// Dummy method  used topost 1-9 different (delegate) methods to another function
		/// </summary>
		/// <param name="KeyboardDelegate9"></param>
		//*************************************************************************************************************//
		public void PostDelegateMethod ( int id, KeyboardDelegate KeyBoardDelegate )
		{
			switch ( id )
			{
				case 1:
					break;

				case 2:
					break;

				case 3:
					break;

				case 4:
					break;

				case 5:
					break;

				case 6:
					break;

				case 7:
					break;

				case 8:
					break;

				case 9:
					BankCollection bc = new BankCollection ( );
					bc . ListBankInfo ( KeyBoardDelegate );
					break;
			}
		}
		public Func<int, int, int, int> IntFuncsDelegate;
		public Func<int, int, int> MathDelegate;

		//*************************************************************************************************************//
		public void Window_PreviewKeyDown ( object sender, KeyEventArgs e )
		{
			DataGrid dg = null;
			int CurrentRow = 0;
			bool showdebug = false;

			if ( e . Key == Key . LeftCtrl )
			{
				key1 = true;
				if ( showdebug ) Debug . WriteLine ( $"key1 = set to TRUE" );
				return;
			}
			if ( showdebug ) Debug . WriteLine ( $"key1 = {key1},  Key = : {e . Key}" );
			//if ( key1 )
			//{
			//	Utils . HandleCtrlFnKeys ( key1, e );
			//	key1 = false;
			//	return;
			//}
			if ( key1 && e . Key == Key . System )     // CTRL + F10
			{
				// Major  listof GV[] variables (Guids etc]
				Debug . WriteLine ( "\nGridview GV[] Variables" );
				Flags . ListGridviewControlFlags ( 1 );
				key1 = false;
				e . Handled = true;
				return;
			}
			else if ( key1 && e . Key == Key . F3 )
			{
				// Demonstrates how to use a Func delegate declared above to select a match function and then perform it
				// totall independently from the Function that actually handles all the different maths functions via the delegate
				int total = 0;
				int res1 = 0;
				int res2 = 0;
				int rem = 0;
				//two diiferent way to use similar Func delegates
				// Method 1
				IntFuncsDelegate = EventControl . CalcInts;
				//Get dividend
				 res1 = IntFuncsDelegate ( 4, 57942021, 8392 );
				//Get remainder 
				rem = IntFuncsDelegate ( 6, 57942021, 8392 );
				// Recalc to confirm
				res2 = IntFuncsDelegate ( 3, res1, 8392 ) + rem;
				if(res2 == 57942021 )
					Debug . WriteLine ( $"Success.... remainder of 57942021/ 8392 ) = {rem}" );
				else
					Debug . WriteLine ( "Failed....!" );

				// Method 2
				MathDelegate = EventControl . CalcAdd;
				total = MathDelegate ( 45932, 87 );
				Debug . WriteLine ( $"Success.... Add Total = {total}" );
				
				MathDelegate = EventControl . CalcSub;
				total = MathDelegate ( 45932, 87 );
				Debug . WriteLine ( $"Success.... Suntract Total = {total}" );
				
				MathDelegate = EventControl . CalcMult;
				total = MathDelegate ( 45932, 87 );
				Debug . WriteLine ( $"Success.... Multiply Total = {total}" );
				
				MathDelegate = EventControl . CalcDiv;
				total = MathDelegate ( 45932, 87 );
				Debug . WriteLine ( $"Success.... Divide Total = {total}" );

				MathDelegate = EventControl . CalcMod;
				total = MathDelegate ( 45932, 87 );
				Debug . WriteLine ( $"Success.... Mod result = {total}" );

				MathDelegate = EventControl . CalcRem;
				total = MathDelegate ( 45932, 87 );
				Debug . WriteLine ( $"Success.... Remainder result = {total}" ); return;
			}
			else if ( key1 && e . Key == Key . F5 )
			{
				// list Flags in Console
				Utils . GetWindowHandles ( );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( key1 && e . Key == Key . F6 )  // CTRL + F6
			{
				// list various Flags in Console
				Flags . UseBeeps = !Flags . UseBeeps;
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( key1 && e . Key == Key . F7 )  // CTRL + F7
			{
				// list various Flags in Console
				Flags . PrintDbInfo ( );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( key1 && e . Key == Key . F8 )  // CTRL + F8
			{
				// list various Flags in Console
				Flags . PrintSundryVariables ( "Window_PreviewKeyDown()" );
				e . Handled = true;
				key1 = false;
				return;
			}
			if ( key1 && e . Key == Key . F9 )    // CTRL + F9
			{
				// lists all delegates & Events
				Debug . WriteLine ( "\nEvent subscriptions " );
				EventHandlers . ShowSubscribersCount ( );
				e . Handled = true;
				return;
			}
			else if ( key1 && e . Key == Key . F11 )
			{
				Debug . WriteLine ( "\nAll Flag. variables" );
				Flags . ShowAllFlags ( );
				key1 = false;
				return;
			}
			else if ( key1 && e . Key == Key . F12 )    // CTRL + F12
			{
				int result1 = -1;
				KeyboardDelegate RunDelegate = new KeyboardDelegate ( DelegateMaster );
				// This allows an external  function to be called via delegates
				// To  clal them, pass the number of the delegate you want to have invoked (1- 9)
				DelegateSelection ds = new DelegateSelection ( );
				ds . ShowDialog ( );
				if ( ds . DialogResult != true ) return;
				switch ( DelegateSelection )
				{
					//case 1:
					//	RunDelegate = KeyboardDelegate1;
					//	break;
					//case 2:
					//	RunDelegate = KeyboardDelegate2;
					//	break;
					//case 3:
					//	RunDelegate = KeyboardDelegate3;
					//	break;
					//case 4:
					//	RunDelegate = KeyboardDelegate4;
					//	break;
					//case 5:
					//	RunDelegate = KeyboardDelegate5;
					//	break;
					//case 6:
					//	RunDelegate = KeyboardDelegate6;
					//	break;
					//case 7:
					//	RunDelegate = KeyboardDelegate7;
					//	break;
					//case 8:
					//	RunDelegate = KeyboardDelegate8;
					//	break;
					case 9:
						RunDelegate = KeyboardDelegate9;
						break;
				}
				RunDelegate ( 9 );
			}
			else if ( key1 && e . Key == Key . OemQuestion )
			{
				// list Flags in Console
				Flags . PrintSundryVariables ( "Window_PreviewKeyDown()" );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( key1 && e . Key == Key . RightAlt ) //|| e . Key == Key . LeftCtrl )
			{       // list Flags in Console
				Flags . ListGridviewControlFlags ( );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( e . Key == Key . Up )
			{       // DataGrid keyboard navigation = UP
				if ( CurrentDb == "BANKACCOUNT" )
					dg = BankGrid;
				else if ( CurrentDb == "CUSTOMER" )
					dg = CustomerGrid;
				else
					dg = DetailsGrid;
				if ( dg . SelectedIndex > 0 )
				{
					dg . SelectedIndex--;
					dg . SelectedItem = dg . SelectedIndex;
					if ( dg . SelectedItem != null )
						Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				}
				if ( dg == BankGrid )
					BankGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == CustomerGrid )
					CustomerGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == DetailsGrid )
					DetailsGrid_SelectedCellsChanged ( dg, null );
				if ( dg . SelectedItem != null )
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( e . Key == Key . Down )
			{       // DataGrid keyboard navigation = DOWN
				if ( CurrentDb == "BANKACCOUNT" )
					dg = BankGrid;
				else if ( CurrentDb == "CUSTOMER" )
					dg = CustomerGrid;
				else
					dg = DetailsGrid;
				if ( dg . SelectedIndex < dg . Items . Count - 1 )
				{
					dg . SelectedIndex++;
					dg . SelectedItem = dg . SelectedIndex;
					if ( dg . SelectedItem != null )
						Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				}
				if ( dg == BankGrid )
					BankGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == CustomerGrid )
					CustomerGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == DetailsGrid )
					DetailsGrid_SelectedCellsChanged ( dg, null );
				if ( dg . SelectedItem != null )
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( e . Key == Key . PageUp )
			{       // DataGrid keyboard navigation = PAGE UP
				if ( CurrentDb == "BANKACCOUNT" )
					dg = BankGrid;
				else if ( CurrentDb == "CUSTOMER" )
					dg = CustomerGrid;
				else
					dg = DetailsGrid;
				if ( dg . SelectedIndex >= 10 )
				{
					dg . SelectedIndex -= 10;
					if ( dg . SelectedItem != null )
						Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				}
				else
				{
					dg . SelectedIndex = 0;
					if ( dg . SelectedItem != null )
						Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				}
				if ( dg == BankGrid )
					BankGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == CustomerGrid )
					CustomerGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == DetailsGrid )
					DetailsGrid_SelectedCellsChanged ( dg, null );
				if ( dg . SelectedItem != null )
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( e . Key == Key . PageDown )
			{       // DataGrid keyboard navigation = PAGE DOWN
				if ( CurrentDb == "BANKACCOUNT" )
					dg = BankGrid;
				else if ( CurrentDb == "CUSTOMER" )
					dg = CustomerGrid;
				else
					dg = DetailsGrid;
				if ( dg . SelectedIndex < dg . Items . Count - 10 )
				{
					dg . SelectedIndex += 10;
					if ( dg . SelectedItem != null )
						Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				}
				else
				{
					dg . SelectedIndex = dg . Items . Count - 1;
					if ( dg . SelectedItem != null )
						Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				}
				if ( dg == BankGrid )
					BankGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == CustomerGrid )
					CustomerGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == DetailsGrid )
					DetailsGrid_SelectedCellsChanged ( dg, null );
				if ( dg . SelectedItem != null )
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				this . Refresh ( );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( e . Key == Key . Home )
			{       // DataGrid keyboard navigation = HOME
				if ( CurrentDb == "BANKACCOUNT" )
					dg = BankGrid;
				else if ( CurrentDb == "CUSTOMER" )
					dg = CustomerGrid;
				else
					dg = DetailsGrid;
				dg . SelectedIndex = 0;
				if ( dg . SelectedItem != null )
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				if ( dg == BankGrid )
					BankGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == CustomerGrid )
					CustomerGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == DetailsGrid )
					DetailsGrid_SelectedCellsChanged ( dg, null );
				if ( dg . SelectedItem != null )
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( e . Key == Key . End )
			{       // DataGrid keyboard navigation = END
				if ( CurrentDb == "BANKACCOUNT" )
					dg = BankGrid;
				else if ( CurrentDb == "CUSTOMER" )
					dg = CustomerGrid;
				else
					dg = DetailsGrid;
				dg . SelectedIndex = dg . Items . Count - 1;
				if ( dg . SelectedItem != null )
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				//				ItemsView_OnSelectionChanged ( dg, null );
				if ( dg == BankGrid )
					BankGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == CustomerGrid )
					CustomerGrid_SelectedCellsChanged ( dg, null );
				else if ( dg == DetailsGrid )
					DetailsGrid_SelectedCellsChanged ( dg, null );
				if ( dg . SelectedItem != null )
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( e . Key == Key . Delete )
			{       // DataGrid keyboard navigation = DELETE
				// This is a ONE SHOT PASS, In here The selected Record will be deleted from the Db's on disk
				// After this  the Event callback should handle the update of this viewer + all/any other open viewers
				int currentindex = 0;
				string bank = "";
				string cust = "";
				var v = e . OriginalSource . GetType ( );
				SqlDbViewer Thisviewer = this;

				// Check to see if Del was pressed while Editing a field (in ANY of our grids)
				// if we have pressed it with just a Row selected, it will return "DataGridCell"  in v.Name
				// else it will have cell info in it
				if ( v . Name != "DataGridCell" )
				{
					e . Handled = false;
					return;         //NOT a Row that is selected, so let OS handle it normally
				}

				// Just ONE of these will call the delete process
				if ( CurrentDb == "BANKACCOUNT" )
				{
					dg = Flags . SqlBankGrid;
					CurrentRow = dg . SelectedIndex;
					// Get and save the data in the row so we have access to it once it has gone from interface
					BankAccountViewModel BankRecord = this . BankGrid . SelectedItem as BankAccountViewModel;
					bank = BankRecord . BankNo;
					cust = BankRecord . CustNo;
					dg . ItemsSource = null;
					SqlBankcollection . Clear ( );
					//					dtBank?.Clear ( );

					//Remove it from SQL Db as well
					DeleteRecord ( "BANKACCOUNT", BankRecord . BankNo, BankRecord . CustNo, CurrentRow );

					e . Handled = true;
					key1 = false;

					// Call the method to update any other Viewers that may be open
					//					EventControl . TriggerRecordDeleted ( CurrentDb, bank, cust, CurrentRow );
					EventControl . TriggerRecordDeleted ( this, new LoadedEventArgs
					{
						Bankno = bank,
						Custno = cust,
						CallerDb = "BANKACCOUNT",
						CurrSelection = CurrentRow,
						DataSource = SqlBankcollection,
						RowCount = CurrentRow
					} );
					// Keep our focus in originating window for now
					Thisviewer . Activate ( );
					Thisviewer . Focus ( );
					return;
				}
				if ( CurrentDb == "CUSTOMER" )
				{
					dg = Flags . SqlCustGrid;
					CurrentRow = dg . SelectedIndex;
					// Get and save the data in the row so we have access to it once it has gone from interface
					CustomerViewModel CustRecord = this . CustomerGrid . SelectedItem as CustomerViewModel;
					bank = CustRecord . BankNo;
					cust = CustRecord . CustNo;
					dg . ItemsSource = null;
					SqlCustcollection . Clear ( );
					CustCollection . dtCust?.Clear ( );

					//Remove it from SQL Db as well
					DeleteRecord ( "CUSTOMER", CustRecord . BankNo, CustRecord . CustNo, CurrentRow );

					e . Handled = true;
					key1 = false;

					// Call the method to update any other Viewers that may be open
					//					EventControl . TriggerRecordDeleted ( CurrentDb, bank, cust, CurrentRow );
					EventControl . TriggerRecordDeleted ( this, new LoadedEventArgs
					{
						Bankno = bank,
						Custno = cust,
						CallerDb = "CUSTOMER",
						CurrSelection = CurrentRow,
						DataSource = SqlCustcollection,
						RowCount = CurrentRow
					} );
					// Keep our focus in originating window for now
					Thisviewer . Activate ( );
					Thisviewer . Focus ( );
					return;
				}
				else if ( CurrentDb == "DETAILS" )
				{
					dg = Flags . SqlDetGrid;
					// Get and save the data in the row so we have access to it once it has gone from interface
					DetailsViewModel DetailsRecord = this . DetailsGrid . SelectedItem as DetailsViewModel;
					bank = DetailsRecord . BankNo;
					cust = DetailsRecord . CustNo;
					CurrentRow = dg . SelectedIndex;
					// Remove it form THIS DataGrid here
					dg . ItemsSource = null;
					SqlDetcollection . Clear ( );
					//					dtDetails?.Clear ( );

					//Remove it from SQL Db as well
					DeleteRecord ( "DETAILS", DetailsRecord . BankNo, DetailsRecord . CustNo, CurrentRow );
					e . Handled = true;
					key1 = false;

					// Call the method to update any other Viewers that may be open
					//					EventControl . TriggerRecordDeleted ( CurrentDb, bank, cust, CurrentRow );
					EventControl . TriggerRecordDeleted ( this, new LoadedEventArgs
					{
						Bankno = bank,
						Custno = cust,
						CallerDb = "DETAILS",
						CurrSelection = CurrentRow,
						DataSource = SqlDetcollection,
						RowCount = CurrentRow
					} );
					// Keep our focus in originating window for now
					Thisviewer . Activate ( );
					Thisviewer . Focus ( );
					return;
				}
				//				else
				//				key1 = false;
				e . Handled = false;
				//				return;
#pragma TESTING DATA LOAD CALLBACK

				{
					// Tidy up our own grid after ourselves
					if ( dg . Items . Count > 0 && CurrentRow >= 0 )
						dg . SelectedIndex = CurrentRow;
					else if ( dg . Items . Count == 1 )
						dg . SelectedIndex = 0;

					//dg.SelectedIndex = Flags.
					dg . SelectedItem = dg . SelectedIndex;
					if ( dg . SelectedItem != null )
						Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );

					// now tell any other open viewers they need to update
					ItemsView_OnSelectionChanged ( dg, null );
					if ( CurrentDb == "BANKACCOUNT" )
					{
						SendDataChanged ( this, BankGrid, "BANKACCOUNT" );
						DetailsGrid_SelectedCellsChanged ( dg, null );
					}
					else if ( CurrentDb == "CUSTOMER" )

					{
						SendDataChanged ( this, CustomerGrid, "CUSTOMER" );
						BankGrid_SelectedCellsChanged ( dg, null );
					}
					else if ( CurrentDb == "DETAILS" )

					{
						SendDataChanged ( this, DetailsGrid, "DETAILS" );
						BankGrid_SelectedCellsChanged ( dg, null );
					}
				}
			}
			e . Handled = false;
		}

		#endregion KEYBOARD DELEGATES & Window keyboard handler for SqlDbViewer class

		#region color control support

		//*************************************************************************************************************//
		private void SetButtonColor ( Button control, string color )
		{
			Brush brs = null;
			if ( color == "GREEN" )
			{
				ControlTemplate ctmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGreen" );
				control . Template = ctmp;
				brs = Utils . GetDictionaryBrush ( "HeaderBrushGreen" );
			}
			if ( color == "YELLOW" )
			{
				ControlTemplate ctmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateYellow" );
				control . Template = ctmp;
				brs = Utils . GetDictionaryBrush ( "HeaderBrushYellow" );
			}
			if ( color == "RED" )
			{
				ControlTemplate ctmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateRed" );
				control . Template = ctmp;
				brs = Utils . GetDictionaryBrush ( "HeaderBrushRed" );
			}
			if ( color == "BLUE" )
			{
				ControlTemplate ctmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateBlue" );
				control . Template = ctmp;
				brs = Utils . GetDictionaryBrush ( "HeaderBrushBlue" );
			}
			if ( color == "GRAY" )
			{
				ControlTemplate ctmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				control . Template = ctmp;
				brs = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
			}
			control . Background = brs;
		}

		#endregion color control support

		/// <summary>
		/// This method Controls all  the windows linkage to allow
		/// for simultaneous scrolling of ALL OPEN DataGrids
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//*************************************************************************************************************//
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
			if ( Flags . CustDbEditor != null )
				Flags . CustDbEditor . LinkRecords . IsChecked = Flags . LinkviewerRecords;
			if ( Flags . DetDbEditor != null )
				Flags . DetDbEditor . LinkRecords . IsChecked = Flags . LinkviewerRecords;
			LinkRecords . Refresh ( );
		}
		#region LINQ methods
		//*************************************************************************************************************//
		private void Linq1_Click ( object sender, RoutedEventArgs e )
		{
			LinqResults lq = new LinqResults ( );
			//			if ( sender . GetType ( ) == typeof ( SqlDbViewer ) )
			//			{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				var accounts = from items in SqlBankcollection
					       where ( items . AcType == 1 )
					       orderby items . CustNo
					       select items;
				BankGrid . ItemsSource = accounts;
				ParseButtonText ( true );
				Count . Text = BankGrid . Items . Count . ToString ( );
				BankFiltered = true;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				var accounts = from items in SqlCustcollection
					       where ( items . AcType == 1 )
					       orderby items . CustNo
					       select items;
				CustomerGrid . ItemsSource = accounts;
				ParseButtonText ( true );
				Count . Text = CustomerGrid . Items . Count . ToString ( );
				CustFiltered = true;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				var accounts = from items in SqlDetcollection
					       where ( items . AcType == 1 )
					       orderby items . CustNo
					       select items;
				DetailsGrid . ItemsSource = accounts;
				ParseButtonText ( true );
				Count . Text = DetailsGrid . Items . Count . ToString ( );
				DetFiltered = true;
			}
		}

		//*************************************************************************************************************//
		private void Linq2_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				var accounts = from items in SqlBankcollection
					       where ( items . AcType == 2 )
					       orderby items . CustNo
					       select items;
				BankGrid . ItemsSource = accounts;
				ParseButtonText ( true );
				Count . Text = BankGrid . Items . Count . ToString ( );
				BankFiltered = true;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				var accounts = from items in SqlCustcollection
					       where ( items . AcType == 2 )
					       orderby items . CustNo
					       select items;
				CustomerGrid . ItemsSource = accounts;
				ParseButtonText ( true );
				Count . Text = CustomerGrid . Items . Count . ToString ( );
				CustFiltered = true;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				var accounts = from items in SqlDetcollection
					       where ( items . AcType == 2 )
					       orderby items . CustNo
					       select items;
				DetailsGrid . ItemsSource = accounts;
				ParseButtonText ( true );
				Count . Text = DetailsGrid . Items . Count . ToString ( );
				DetFiltered = true;
			}
		}

		//*************************************************************************************************************//
		private void Linq3_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				var accounts = from items in SqlBankcollection
					       where ( items . AcType == 3 )
					       orderby items . CustNo
					       select items;
				BankGrid . ItemsSource = accounts;
				ParseButtonText ( true );
				Count . Text = BankGrid . Items . Count . ToString ( );
				BankFiltered = true;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				var accounts = from items in SqlCustcollection
					       where ( items . AcType == 3 )
					       orderby items . CustNo
					       select items;
				CustomerGrid . ItemsSource = accounts;
				ParseButtonText ( true );
				Count . Text = CustomerGrid . Items . Count . ToString ( );
				CustFiltered = true;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				var accounts = from items in SqlDetcollection
					       where ( items . AcType == 3 )
					       orderby items . CustNo
					       select items;
				DetailsGrid . ItemsSource = accounts;
				ParseButtonText ( true );
				Count . Text = DetailsGrid . Items . Count . ToString ( );
				DetFiltered = true;
			}
		}

		//*************************************************************************************************************//
		private void Linq4_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				var accounts = from items in SqlBankcollection
					       where ( items . AcType == 4 )
					       orderby items . CustNo
					       select items;
				BankGrid . ItemsSource = accounts;
				ParseButtonText ( true );
				Count . Text = BankGrid . Items . Count . ToString ( );
				BankFiltered = true;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				var accounts = from items in SqlCustcollection
					       where ( items . AcType == 4 )
					       orderby items . CustNo
					       select items;
				CustomerGrid . ItemsSource = accounts;
				ParseButtonText ( true );
				Count . Text = CustomerGrid . Items . Count . ToString ( );
				CustFiltered = true;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				var accounts = from items in SqlDetcollection
					       where ( items . AcType == 4 )
					       orderby items . CustNo
					       select items;
				DetailsGrid . ItemsSource = accounts;
				ParseButtonText ( true );
				Count . Text = DetailsGrid . Items . Count . ToString ( );
				DetFiltered = true;
			}
		}

		//*************************************************************************************************************//
		private void Linq5_Click ( object sender, RoutedEventArgs e )
		{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				//select All the items first;
				var accounts = from items in SqlBankcollection orderby items . CustNo, items . AcType select items;
				//Next Group collection on CustNo
				var grouped = accounts . GroupBy (
					b => b . CustNo );

				//Now filter content down to only those a/c's with multiple Bank A/c's
				var sel = from g in grouped
					  where g . Count ( ) > 1
					  select g;

				// Finally, iterate thru the list of grouped CustNo's matching to CustNo in the full BankAccounts data
				// giving us ONLY the full records for any records that have > 1 Bank accounts
				List<BankAccountViewModel> output = new List<BankAccountViewModel> ( );
				foreach ( var item1 in sel )
				{
					foreach ( var item2 in accounts )
					{
						if ( item2 . CustNo . ToString ( ) == item1 . Key )
						{ output . Add ( item2 ); }
					}
				}
				BankGrid . ItemsSource = output;
				ParseButtonText ( true );
				Count . Text = BankGrid . Items . Count . ToString ( );
				StatusBar . Text = $"Filtering completed, {output . Count} Multi Account records match";
				BankFiltered = true;
			}
			if ( CurrentDb == "CUSTOMER" )
			{
				//select All the items first;
				var accounts = from items in SqlCustcollection orderby items . CustNo, items . AcType select items;
				//Next Group collection on CustNo
				var grouped = accounts . GroupBy (
					b => b . CustNo );

				//Now filter content down to only those a/c's with multiple Bank A/c's
				var sel = from g in grouped
					  where g . Count ( ) > 1
					  select g;

				// Finally, iterate thru the list of grouped CustNo's matching to CustNo in the full BankAccounts data
				// giving us ONLY the full records for any records that have > 1 Bank accounts
				List<CustomerViewModel> output = new List<CustomerViewModel> ( );
				foreach ( var item1 in sel )
				{
					foreach ( var item2 in accounts )
					{
						if ( item2 . CustNo . ToString ( ) == item1 . Key )
						{ output . Add ( item2 ); }
					}
				}
				CustomerGrid . ItemsSource = output;
				StatusBar . Text = $"Filtering completed, {output . Count} Multi Account records match";
				ParseButtonText ( true );
				Count . Text = CustomerGrid . Items . Count . ToString ( );
				CustFiltered = true;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				//select All the items first;
				var accounts = from items in SqlDetcollection orderby items . CustNo, items . AcType select items;
				//Next Group collection on CustNo
				var grouped = accounts . GroupBy (
					b => b . CustNo );

				//Now filter content down to only those a/c's with multiple Bank A/c's
				var sel = from g in grouped
					  where g . Count ( ) > 1
					  select g;

				// Finally, iterate though the list of grouped CustNo's matching to CustNo in the full BankAccount data
				// giving us ONLY the full records for any records that have > 1 Bank accounts
				List<DetailsViewModel> output = new List<DetailsViewModel> ( );
				foreach ( var item1 in sel )
				{
					foreach ( var item2 in accounts )
					{
						if ( item2 . CustNo . ToString ( ) == item1 . Key )
						{ output . Add ( item2 ); }
					}
				}
				DetailsGrid . ItemsSource = output;
				StatusBar . Text = $"Filtering completed, {output . Count} Multi Account records match";
				ParseButtonText ( true );
				Count . Text = DetailsGrid . Items . Count . ToString ( );
				DetFiltered = true;
			}
		}
		//*************************************************************************************************************//
		// Turn filter OFF
		private async void Linq6_Click ( object sender, RoutedEventArgs e )
		{
			//			var accounts = from items in SqlBankcollection orderby items . CustNo, items . AcType select items;
			//			var accounts1 = from items in SqlCustcollection orderby items . CustNo, items . AcType select items;
			//			var accounts2 = from items in SqlDetcollection orderby items . CustNo, items . AcType select items;
			//			SqlDbViewer sqldb = sender as SqlDbViewer;
			if ( BankGrid . Items . Count > 0 )
			{
				BankGrid . ItemsSource = null;
				await BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );
				BankGrid . Refresh ( );
				BankFiltered = false;
			}
			else if ( CustomerGrid . Items . Count > 0 )
			{
				CustomerGrid . ItemsSource = null;
				await CustCollection . LoadCust ( SqlCustcollection, "SQLDBVIEWER", 1, true );
				CustomerGrid . Refresh ( );
				CustFiltered = false;
			}
			else if ( DetailsGrid . Items . Count > 0 )
			{
				DetailsGrid . ItemsSource = null;
				await DetailCollection . LoadDet ( SqlDetcollection, "SQLDBVIEWER", 1, true );
				DetailsGrid . Refresh ( );
				DetFiltered = false;
			}
		}
		#endregion LINQ methods

		//*************************************************************************************************************//
		private void Filter_Click ( object sender, RoutedEventArgs e )
		{ }

		//*************************************************************************************************************//
		private void Options_Click ( object sender, RoutedEventArgs e )
		{ MessageBox . Show ( $"Sorry, but there are NO options right now !!", "Database Viewer Options" ); }

		//*************************************************************************************************************//
		private void Exit_Click ( object sender, RoutedEventArgs e )
		{ Close ( ); }

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
			if ( grid == this . BankGrid )
			{
				BankAccountViewModel CurrentBankSelectedRecord = this . BankGrid . SelectedItem as BankAccountViewModel;
				if ( CurrentBankSelectedRecord == null ) return;
				SearchCustNo = CurrentBankSelectedRecord . CustNo;
				SearchBankNo = CurrentBankSelectedRecord . BankNo;
				EventControl . TriggerViewerIndexChanged ( this,
					new IndexChangedArgs
					{
						Senderviewer = this,
						Bankno = SearchBankNo,
						Custno = SearchCustNo,
						dGrid = this . BankGrid,
						Sender = "BANKACCOUNT",
						SenderId = "SQLDBVIEWER",
						Row = this . BankGrid . SelectedIndex
					} );
			}
			else if ( grid == this . CustomerGrid )
			{
				CustomerViewModel CurrentCustSelectedRecord = this . CustomerGrid . SelectedItem as CustomerViewModel;
				if ( CurrentCustSelectedRecord == null ) return;
				SearchCustNo = CurrentCustSelectedRecord . CustNo;
				SearchBankNo = CurrentCustSelectedRecord . BankNo;
				EventControl . TriggerViewerIndexChanged ( this,
				new IndexChangedArgs
				{
					Senderviewer = this,
					Bankno = SearchBankNo,
					Custno = SearchCustNo,
					dGrid = this . CustomerGrid,
					Sender = "CUSTOMER",
					SenderId = "SQLDBVIEWER",
					Row = this . CustomerGrid . SelectedIndex
				} );
			}
			else if ( grid == this . DetailsGrid )
			{
				DetailsViewModel CurrentDetSelectedRecord = this . DetailsGrid . SelectedItem as DetailsViewModel;
				if ( CurrentDetSelectedRecord == null ) return;
				SearchCustNo = CurrentDetSelectedRecord . CustNo;
				SearchBankNo = CurrentDetSelectedRecord . BankNo;
				EventControl . TriggerViewerIndexChanged ( this,
					new IndexChangedArgs
					{
						Senderviewer = this,
						Bankno = SearchBankNo,
						Custno = SearchCustNo,
						dGrid = this . DetailsGrid,
						Sender = "DETAILS",
						SenderId = "SQLDBVIEWER",
						Row = this . DetailsGrid . SelectedIndex
					} );
			}
		}

		#region UNUSED CODE
		/// <summary>
		/// Calls  the relevant SQL data load calls to load data, fill Lists and populate Obs collections
		/// </summary>
		/// <param name="CurrentDb"></param>
		//*************************************************************************************************************//
		public async void GetData ( string CurrentDb )
		{
			//if ( CurrentDb == "BANKACCOUNT" )
			//{
			//	if ( Flags . SqlBankGrid != null )
			//		Debug . WriteLine ( "\nA viewer showing BankAccount data is already open" );
			//	Debug . WriteLine ( $"Starting AWAITED task to load Bank Data via Sql" );
			//	Stopwatch sw = new Stopwatch ( );
			//	sw . Start ( );
			//	Mouse . OverrideCursor = Cursors . Wait;
			//	SqlViewerBankcollection = await BankCollection . LoadBank ( SqlViewerBankcollection, 1, true );
			//	sw . Stop ( );
			//	Debug . WriteLine ( $"BankAccount loaded {this . BankGrid . Items?.Count} records in {( double ) sw . ElapsedMilliseconds / ( double ) 1000} seconds " );
			//	ParseButtonText ( true );
			//	Count . Text = this . BankGrid . Items . Count . ToString ( );
			//	Mouse . OverrideCursor = Cursors . Arrow;
			//}
			//else if ( CurrentDb == "CUSTOMER" )
			//{
			//	if ( Flags . SqlCustGrid != null )
			//		Debug . WriteLine ( "\nA viewer showing Customer data is already open\n" );
			//	Debug . WriteLine ( $"CUSTOMER: {this . CustomerGrid . Items?.Count}" );

			//	Stopwatch sw = new Stopwatch ( );
			//	sw . Start ( );
			//	Mouse . OverrideCursor = Cursors . Wait;
			//	SqlViewerCustcollection = await CustCollection . LoadCust ( SqlViewerCustcollection, 1, true );
			//	sw . Stop ( );
			//	Debug . WriteLine ( $"Customer loaded {this . CustomerGrid . Items?.Count} records in {( double ) sw . ElapsedMilliseconds / ( double ) 1000} seconds " );
			//	ParseButtonText ( true );
			//	Count . Text = this . CustomerGrid . Items . Count . ToString ( );
			//	Mouse . OverrideCursor = Cursors . Arrow;
			//}
			//else if ( CurrentDb == "DETAILS" )
			//{
			//	//Loads the data Asynchronously
			//	//				ExtensionMethods . Refresh ( DetailsGrid );
			//	Stopwatch sw1 = new Stopwatch ( );
			//	sw1 . Start ( );
			//	Debug . WriteLine ( $"Calling AWAITED task to load Details Data into DataGrid via Sql" );
			//	Mouse . OverrideCursor = Cursors . Wait;
			//	SqlViewerDetcollection = await DetCollection . LoadDet ( SqlViewerDetcollection, 1, true );
			//	this . DetailsGrid . ItemsSource = SqlViewerDetcollection;
			//	sw1 . Stop ( );
			//	Debug . WriteLine ( $"{( double ) sw1 . ElapsedMilliseconds / ( double ) 1000} seconds - Details loading Task completed\n{this . DetailsGrid . Items?.Count} records loaded" );
			//	ParseButtonText ( true );
			//	Count . Text = this . DetailsGrid . Items . Count . ToString ( );
			//	Mouse . OverrideCursor = Cursors . Arrow;
			//}
			#endregion UNUSED CODE
		}

		private async void BankGrid_PreviewMouseDown ( object sender, MouseButtonEventArgs e )
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

				Flags . ActiveSqlViewer = this;
				Flags . CurrentSqlViewer = this;
				//If data has been changed, update everywhere
				// Update the row on return in case it has been changed
				if ( rip . IsDirty )
				{
					// This is done in RowPopup()
					//BankAccountViewModel bvm = new BankAccountViewModel ( );
					//bvm = this . BankGrid . SelectedItem as BankAccountViewModel;
					//SQLHandlers sqlh = new SQLHandlers ( );
					//sqlh . UpdateDbRowAsync ( "BANKACCOUNT", bvm );
					this . BankGrid . ItemsSource = null;
					this . BankGrid . Items . Clear ( );
					SqlBankcollection = await BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );
					this . BankGrid . ItemsSource = SqlBankcollection;
					StatusBar . Text = "Current Record Updated Successfully...";
					// Notify everyone else of the data change
					EventControl . TriggerViewerDataUpdated ( SqlBankcollection,
						new LoadedEventArgs
						{
							CallerType = "SQLDBVIEWER",
							CallerDb = "BANKACCOUNT",
							DataSource = SqlBankcollection,
							RowCount = this . BankGrid . SelectedIndex
						} );
				}
				else
					this . BankGrid . SelectedItem = RowData . Item;

				// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
				Utils . SetUpGridSelection ( this . BankGrid, row );
				ParseButtonText ( true );
				Count . Text = this . BankGrid . Items . Count . ToString ( );
				// This is essential to get selection activated again
				this . BankGrid . Focus ( );
			}

		}

		private void BankGrid_Scroll ( object sender, System . Windows . Controls . Primitives . ScrollEventArgs e )
		{

		}

		private void BankGrid_TargetUpdated ( object sender, DataTransferEventArgs e )
		{
			//MainWindow . gv . Datagrid [ LoadIndex ] = this . BankGrid;

		}

		private void CustomerGrid_MouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{

		}

		private void CustomerGrid_Scroll ( object sender, System . Windows . Controls . Primitives . ScrollEventArgs e )
		{

		}

		private void DetailsGrid_TargetUpdated ( object sender, DataTransferEventArgs e )
		{
			//			MainWindow . gv . Datagrid [ LoadIndex ] = this . DetailsGrid;
		}

		private void DetailsGrid_MouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{

		}
		private void DoDragMove ( )
		{//Handle the button NOT being the left mouse button
		 // which will crash the DragMove Fn.....
			try
			{ this . DragMove ( ); }
			catch { return; }
		}

		#region UNUSED CODE

		/// <summary>
		/// Called by each ViewModel to handle loading new data into the relevant datagrid
		/// </summary>
		/// <param name="Viewer"></param>
		/// <param name="Grid"></param>
		/// <param name="args"></param>
		//*************************************************************************************************************//
		public async Task ReloadDetailsOnUpdateNotification ( SqlDbViewer Viewer, DataGrid Grid, DataChangeArgs args )
		{
			//DataGrid CurrentGrid = null;

			//if ( Flags . SqlDetViewer == null ) return;
			//Flags . SqlDetViewer . Focus ( );
			//CurrentGrid = Flags . SqlDetViewer . DetailsGrid;
			//int currindx = CurrentGrid . SelectedIndex != -1 ? CurrentGrid . SelectedIndex : 0;
			//Debug . WriteLine ( $"\nnREFRESHING { args . DbName}. SelectedIndex = {currindx}\n" );
			//if ( currindx == -1 )
			//	currindx = 0;
			//CurrentGrid . ItemsSource = null;

			//Debug . WriteLine ( $"Calling Task to reload Details data...." );
			//Mouse . OverrideCursor = Cursors . Wait;
			//SqlViewerDetcollection = await DetCollection . LoadDet ( SqlViewerDetcollection, 1, true );
			//Debug . WriteLine ( $"returned from Task loading Details data ...." );
			//CurrentGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerDetcollection );
			//CurrentGrid . SelectedIndex = currindx;
			//CurrentGrid . SelectedItem = currindx;
			////			ExtensionMethods . Refresh ( CurrentGrid );
			//CurrentGrid . Refresh ( );
			//DetailsViewModel data = CurrentGrid . SelectedItem as DetailsViewModel;
			//StatusBar . Text = $"Data reloaded due to external changes for Customer # {data?.CustNo}, Bank A/C # {data?.BankNo}";
			//Debug . WriteLine ( $"\nCompleted REFRESHING { args . DbName}. SelectedIndex set to {currindx}\n" );
			//Mouse . OverrideCursor = Cursors . Arrow;

			//if ( args . DbName == "DETAILS" )
			//{
			//	Flags . ActiveSqlViewer = Flags . SqlDetViewer;
			//	Flags . CurrentSqlViewer = Flags . SqlDetViewer;
			//}
			//ParseButtonText ( true );
			//Count . Text = this . DetailsGrid . Items . Count . ToString ( );
		}

		/// <summary>
		/// Called by each ViewModel to handle loading new data into the relevant datagrid
		/// </summary>
		/// <param name="Viewer"></param>
		/// <param name="Grid"></param>
		/// <param name="args"></param>
		//*************************************************************************************************************//
		public async Task ReloadCustomerOnUpdateNotification ( SqlDbViewer Viewer, DataGrid Grid, DataChangeArgs args )
		{
			//DataGrid CurrentGrid = null;

			//if ( Flags . SqlCustViewer == null ) return;
			//Flags . SqlCustViewer . Focus ( );
			//CurrentGrid = Flags . SqlCustViewer . CustomerGrid;
			//int currindx = CurrentGrid . SelectedIndex != -1 ? CurrentGrid . SelectedIndex : 0;
			//Debug . WriteLine ( $"\nREFRESHING { args . DbName}. SelectedIndex = {currindx}\n" );
			//if ( currindx == -1 ) currindx = 0;

			//// Toggle ItemsSource to refresh datagrid
			//CurrentGrid . ItemsSource = null;

			//Debug . WriteLine ( $"Calling Task to reload Customer data...." );
			//SqlViewerCustcollection = await CustCollection . LoadCust ( SqlViewerCustcollection, 1, true );
			//Debug . WriteLine ( $"returned from Task loading Customer data ...." );
			//CurrentGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerCustcollection );
			//CurrentGrid . SelectedIndex = currindx;
			//CurrentGrid . SelectedItem = currindx;
			////			ExtensionMethods . Refresh ( CurrentGrid );
			//CurrentGrid . Refresh ( );
			//CustomerViewModel data = CurrentGrid . SelectedItem as CustomerViewModel;
			//StatusBar . Text = $"Data reloaded due to external changes for Customer # {data?.CustNo}, Bank A/C # {data?.BankNo}";
			//Debug . WriteLine ( $"\nCompleted REFRESHING { args . DbName}. SelectedIndex set to {currindx}\n" );

			//if ( args . DbName == "CUSTOMER" )
			//{
			//	Flags . ActiveSqlViewer = Flags . SqlCustViewer;
			//	Flags . CurrentSqlViewer = Flags . SqlCustViewer;
			//}
			//ParseButtonText ( true );
			//Count . Text = this . CustomerGrid . Items . Count . ToString ( );
		}

		//*************************************************************************************************************//
		/// <summary>
		///  UNUSED COE
		/// </summary>
		/// <param name="Grid"></param>
		private void RefreshOtherviewersAfterUpdate ( DataGrid Grid )
		{
			////	Data has ben updated, so force other open viewers to refresh thier own girids
			//SqlDbViewer thisViewer = null;
			//if ( Flags . SqlBankGrid == Grid )
			//{
			//	// This is the changes' ORIGINATING viewer
			//	thisViewer = this;
			//	if ( Flags . SqlCustGrid != null )
			//	{
			//		Flags . SqlCustViewer . Activate ( );
			//		this . CustomerGrid . ItemsSource = null;
			//		this . CustomerGrid . ItemsSource = SqlViewerCustcollection;
			//		//					ExtensionMethods . Refresh ( this . CustomerGrid );
			//		this . CustomerGrid . Refresh ( );
			//		//					ExtensionMethods . Refresh ( Flags . SqlCustViewer );
			//		Flags . SqlCustViewer . Refresh ( );
			//	}
			//	if ( Flags . SqlDetGrid != null )
			//	{
			//		Flags . SqlDetViewer . Activate ( );
			//		this . DetailsGrid . ItemsSource = null;
			//		this . DetailsGrid . ItemsSource = SqlViewerDetcollection;
			//		this . DetailsGrid . Refresh ( );
			//		//					ExtensionMethods . Refresh ( this . DetailsGrid );
			//		//					ExtensionMethods . Refresh ( Flags . SqlDetViewer );
			//		Flags . SqlDetViewer . Refresh ( );
			//	}
			//	//Activate our originating window again
			//	thisViewer . Activate ( );
			//}
			//if ( Flags . SqlCustGrid == Grid )
			//{
			//	// This is the changes' ORIGINATING viewer
			//	thisViewer = this;
			//	if ( Flags . SqlBankGrid != null )
			//	{
			//		Flags . SqlBankViewer . Activate ( );
			//		this . BankGrid . ItemsSource = null; ;
			//		this . BankGrid . ItemsSource = SqlViewerBankcollection;
			//		//					ExtensionMethods . Refresh ( this . BankGrid );
			//		this . BankGrid . Refresh ( );
			//		//					ExtensionMethods . Refresh ( Flags . SqlBankViewer );
			//		Flags . SqlBankViewer . Refresh ( );
			//	}
			//	if ( Flags . SqlDetGrid != null )
			//	{
			//		Flags . SqlDetViewer . Activate ( );
			//		this . DetailsGrid . ItemsSource = null;
			//		this . DetailsGrid . ItemsSource = SqlViewerDetcollection;
			//		this . DetailsGrid . Refresh ( );
			//		//					ExtensionMethods . Refresh ( this . DetailsGrid );
			//		//ExtensionMethods . Refresh ( Flags . SqlDetViewer );
			//		Flags . SqlDetViewer . Refresh ( );
			//	}
			//	//Activate our originating window again
			//	thisViewer . Activate ( );
			//}
			//if ( Flags . SqlDetGrid == Grid )
			//{
			//	// This is the changes' ORIGINATING viewer
			//	thisViewer = this;
			//	if ( Flags . SqlBankGrid != null )
			//	{
			//		Flags . SqlBankViewer . Activate ( );
			//		this . BankGrid . ItemsSource = null;
			//		this . BankGrid . ItemsSource = SqlViewerBankcollection;
			//		//					ExtensionMethods . Refresh ( this . BankGrid );
			//		this . BankGrid . Refresh ( );
			//		//ExtensionMethods . Refresh ( Flags . SqlBankViewer );
			//		Flags . SqlBankViewer . Refresh ( );
			//	}
			//	if ( Flags . SqlCustGrid != null )
			//	{
			//		Flags . SqlCustViewer . Activate ( );
			//		this . CustomerGrid . ItemsSource = null;
			//		this . CustomerGrid . ItemsSource = SqlViewerCustcollection;
			//		this . CustomerGrid . Refresh ( );
			//		//					ExtensionMethods . Refresh ( this . CustomerGrid );
			//		//					ExtensionMethods . Refresh ( Flags . SqlCustViewer );
			//		Flags . SqlCustViewer . Refresh ( );
			//	}
			//	//Activate our originating window again
			//	thisViewer . Activate ( );
			//}
		}

		#endregion UNUSED CODE

	}
}