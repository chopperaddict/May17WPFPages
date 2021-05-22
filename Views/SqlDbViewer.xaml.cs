#define SHOWSQLERRORMESSAGEBOX
#define SHOWWINDOWDATA
#define ALLOWREFRESH
#define LINKVIEWTOEDIT

using System;
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
using System . Windows . Threading;

using WPFPages . Properties;
using WPFPages . ViewModels;
using WPFPages . Views;

namespace WPFPages
{
	public partial class SqlDbViewer : Window, INotifyPropertyChanged
	{
		private ClockTower _tower;
		public SqlDbViewer ThisViewer = null;
		public static Dispatcher UiThread = Dispatcher . CurrentDispatcher;

		// Declare all 3 of the Db pointers
		public BankCollection SqlViewerBankcollection = BankCollection . SqlViewerBankcollection;
		public CustCollection SqlViewerCustcollection = CustCollection . SqlViewerCustcollection;
		public DetCollection SqlViewerDetcollection = DetCollection . SqlViewerDetcollection;

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

		#region Class setup - General Declarations

		//		private int CurrentGridViewerIndex = -1;
		public dynamic CurrentDb = "";

		public SqlDataAdapter sda;
		private string columnToFilterOn = "";
		private string filtervalue1 = "";
		private string filtervalue2 = "";
		private string operand = "";
		public bool FilterResult = false;
		public bool Triggered = false;

		private string IsFiltered = "";
		private string FilterCommand = "";
		private string PrettyDetails = "";

		//		public static bool Flags.IsMultiMode = false;
		//private BankAccountViewModel BankCurrentRowAccount;
		//		private CustomerViewModel CustomerCurrentRowAccount;

		//		private DetailsViewModel DetailsCurrentRowAccount;
		//		private DataGridRow BankCurrentRow;
		//		private DataGridRow CustomerCurrentRow;
		//		private DataGridRow DetailsCurrentRow;
		public Window ThisWindow = new Window ( );

		private bool IsViewerLoaded = false;
		private int LoadIndex = -1;
		public bool SqlUpdating = false;

		//		private bool IsCellChanged = false;
		public DataGrid EditDataGrid = null;

		//Get "Local" copies of our global DataTables

		public DataTable dtBank = BankCollection . dtBank;
		public DataTable dtCust = CustCollection . dtCust;
		public DataTable dtDetails = DetCollection . dtDetails;

		//Variables for Edithasoccurred delegate
		//		private SQLEditOcurred SqlEdit = HandleEdit;
		private EditEventArgs EditArgs = null;

		public static SqlDbViewer sqldbForm = null;

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

		public EventHandlers EventHandler = null;
		private bool SelectionhasChanged = false;

		//Variables used when a cell is edited to se if we need to update via SQL
		private object OriginalCellData = null;

		private string OriginalDataType = "";
		private int OrignalCellRow = 0;
		private int OriginalCellColumn = 0;

		public DataGridController dgControl;

		//---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
		//Delegate & Event handler for Db Updates
		// I am declaring  the Event in THIS FILE
		// the ViewModels will be the subscribers
		//---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
		/// <summary>
		///  A(Globally visible) Delegate to hold all the global flags and other stuff that is needed to handle
		///  Static -> non static  movements with EditDb &b SqlDbViewer in particular
		/// </summary>

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

		#endregion Class setup - General Declarations

		#region Private declarations

		private DataGrid CurrentDataGrid = null;
		private int EditChangeType = 0;
		private int ViewerChangeType = 0;
		private EditDb edb;
		private bool key1 = false;
		public bool RefreshInProgress = false;

		#endregion Private declarations


		#region SqlDbViewer Class Constructors

		public SqlDbViewer ( bool x )
		{
			// dummy constructor fpr pre=loading
		}

		//Dummy Constructor for Event handlers
		//*********************************************************************************************************//
		public SqlDbViewer ( char x )
		{
			// dummy constructor to let others get a pointer
			InitializeComponent ( );
			ThisViewer = this;
			Flags . CurrentSqlViewer = this;
		}

		//*********************************************************************************************************//
		private void _tower_Chime ( )
		{
			//			Debug . WriteLine ($"Chime event called in Method");
		}

		public SqlDbViewer ( )
		{
			InitializeComponent ( );

			sqldbForm = this;
			ThisViewer = this;

			dgControl = new DataGridController ( );
			if ( Flags . CurrentSqlViewer != this )
				Flags . CurrentSqlViewer = this;

			// just used for Tower Test of events
			// assign handler to delegate
			NotifyViewer SendCommand = DbSelector . MyNotification;
			//SendDbSelectorCommand = DbSelector . MyNotification;
			//			SqlViewerNotify notifier = DbSelectorMessage;

			Utils . GetWindowHandles ( );
			// Handle window dragging
			this . MouseDown += delegate { DoDragMove ( ); };

			SubscribeToEvents ( );
		}

		//*********************************************************************************************************//
		/// <summary>
		/// MAIN STARTUP CALL from DbSelector
		/// </summary>
		/// <param name="caller"></param>
		/// <returns></returns>
		public SqlDbViewer ( string caller, object Collection )
		{
			//			int selectedDb = -1;
			IsViewerLoaded = false;
			InitializeComponent ( );
			CurrentDb = caller;
			this . Show ( );
			LoadData ( caller );
			//DetCollection.go();
			UiThread = Dispatcher . CurrentDispatcher;
			if ( 1 == 2 )
			{
				////Testing Event
				//// Static method code
				////Subscribe to Event here
				//// Handler is  directly  above here
				//// Chime is  the ** Static ** EVENT NAME declared in  Clocktower file
				//ClockTower . Chime += _tower_Chime;
				//// Or a  Lambda version with no seperate method
				//ClockTower . Chime += ( ) =>
				//{
				//	//				Debug . WriteLine ( $"Clock has chimed in Lambda" );
				//};
				//// Trigger it to work  by calling STATIC methods in clocktower
				////ClockTower . sChimeFivePm ( );
				////ClockTower . sChimeSixAm ( );
				//// End of Static method code

				////-------------------------------------------------------------------------------------------------------------------------------------------------//
				//// Instance method code
				//ClockTower ct = new ClockTower ( );
				//_tower = ct;

				//// Trigger it to work  by calling STATIC methods in clocktower
				////			ct. ChimeFivePm ( );
				////		ct. ChimeSixAm ( );
				//// End of Instance method code
				////-------------------------------------------------------------------------------------------------------------------------------------------------//
			}
			// just used for Tower Test of events
			//Setup our delegate receive function to get messages from DbSelector
			NotifyViewer SendCommand = DbSelector . MyNotification;
			//SendDbSelectorCommand = DbSelector . MyNotification;
			// Handle window dragging
			this . MouseDown += delegate { DoDragMove ( ); };
			RefreshBtn . IsEnabled = true;
			//This DOES call handler in DbSelector !!
			sqldbForm = this;
			dgControl = new DataGridController ( );
			Flags . CurrentSqlViewer = this;
			//			LoadData ( caller );

			// Test of Func IsRecordMatched<object, object, bool> declared in Utils
			// where the objects are Db types so we can compere Db column data
			Debug . WriteLine ( $"{Utils . IsRecordMatched ( this . BankGrid . SelectedItem, this . CustomerGrid . SelectedItem )} " );
		}

		private async Task LoadData ( string caller )
		{
			switch ( caller )
			{
				case "BANKACCOUNT":
					CurrentDb = "BANKACCOUNT";
					new EventHandlers ( BankGrid, "BANKACOUNT", out EventHandler );
					Flags . ActiveSqlViewer = this;
					Mouse . OverrideCursor = Cursors . Wait;
					SqlViewerBankcollection = await BankCollection . LoadBank ( SqlViewerBankcollection, 1 );//.ConfigureAwait(false);
					this . BankGrid . ItemsSource = SqlViewerBankcollection;
					Count . Text = BankCollection . SqlViewerBankcollection . Count . ToString ( );
					Mouse . OverrideCursor = Cursors . Arrow;
					BankGrid . Visibility = Visibility . Visible;
					this . BankGrid . Refresh ( );
					ParseButtonText ( false );
					break;

				case "CUSTOMER":
					CurrentDb = "CUSTOMER";
					new EventHandlers ( CustomerGrid, "CUSTOMER", out EventHandler );
					Flags . ActiveSqlViewer = this;
					Mouse . OverrideCursor = Cursors . Wait;
					SqlViewerCustcollection = await CustCollection . LoadCust ( SqlViewerCustcollection );
					this . CustomerGrid . ItemsSource = SqlViewerCustcollection;
					Count . Text = SqlViewerCustcollection . Count . ToString ( );
					Mouse . OverrideCursor = Cursors . Arrow;
					CustomerGrid . Visibility = Visibility . Visible;
					this . CustomerGrid . Refresh ( );
					ParseButtonText ( false );
					break;

				case "DETAILS":
					CurrentDb = "DETAILS";
					new EventHandlers ( DetailsGrid, "DETAILS", out EventHandler );
					Flags . ActiveSqlViewer = this;
					Mouse . OverrideCursor = Cursors . Wait;
					SqlViewerDetcollection = await DetCollection . LoadDet ( SqlViewerDetcollection );
					this . DetailsGrid . ItemsSource = SqlViewerDetcollection;
					Count . Text = SqlViewerDetcollection . Count . ToString ( );
					Mouse . OverrideCursor = Cursors . Arrow;
					DetailsGrid . Visibility = Visibility . Visible;
					this . DetailsGrid . Refresh ( );
					ParseButtonText ( false );
					break;

				default:
					break;
			}
			//subscribing viewmodels to data changed event !!!
			SubscribeToEvents ( );
			ThisWindow = this;
			EditArgs = new EditEventArgs ( );
			this . UpdateLayout ( );
		}

		#endregion SqlDbViewer Class Constructors

		#region *** CRUCIAL Methods - Event CALLBACK for data loading

		/// <summary>
		///
		/// EVENT HANDLER for  BANKDATALOADED event.
		///
		/// Tiggered when the BankCollection Sql Data has reloaded data and it is is ready for use
		/// </summary>
		/// <param name="sender">
		/// <param name="e">DataSource = Pointer to the new Bankcollection Observable collection</param>
		/// </param>
		/// <summary>
		/// Generic method that handles the subscribing of this window to all the relevant EVENTS
		/// required for control as we work with in the Window.
		/// </summary>
		private void SubscribeToEvents ( )
		{
			//Subscribe to our clever Collections data system
			if ( CurrentDb == "BANKACCOUNT" )
				EventControl . BankDataLoaded += SqlDbViewer_BankDataLoaded;
			else if ( CurrentDb == "CUSTOMER" )
				EventControl . CustDataLoaded += SqlDbViewer_BankDataLoaded;
			else if ( CurrentDb == "DETAILS" )
				EventControl . DetDataLoaded += SqlDbViewer_BankDataLoaded;

			// An EditDb Viewer has updated a record  notification handler
			EventControl . EditDbDataUpdated += EventControl_EditDbDataUpdated;
			// Another SQL viewer has updated a record  notification handler
			EventControl . ViewerDataUpdated += EventControl_EditDbDataUpdated;
			// A Multi Viewer has updated a record  notification handler
			EventControl . MultiViewerDataUpdated += EventControl_EditDbDataUpdated;

			// Event triggers when a Specific Db viewer (BankDbViewer etc) updates the data
			EventControl . BankDataLoaded += EventControl_EditDbDataUpdated;
			EventControl . CustDataLoaded += EventControl_EditDbDataUpdated;
			EventControl . DetDataLoaded += EventControl_EditDbDataUpdated;

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
		/// Handler for data changes performed by ANY external viewer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public async void SqlDbViewer_BankDataLoaded ( object sender, LoadedEventArgs e )
		{
			bool result = false;
			result = await UpdateSelectedDb ( sender, e );
			if ( result == false )
			{
				if ( e . CallerDb == "BANKACCOUNT" )
				{
					this . BankGrid . ItemsSource = null;
					this . BankGrid . ItemsSource = SqlViewerBankcollection;
				}
				else if ( e . CallerDb == "CUSTOMER" )
				{
					this . CustomerGrid . ItemsSource = null;
					this . CustomerGrid . ItemsSource = SqlViewerCustcollection;
				}
				else if ( e . CallerDb == "DETAILS" )
				{
					this . DetailsGrid . ItemsSource = null;
					this . DetailsGrid . ItemsSource = SqlViewerDetcollection;
				}
			}
			return;
		}

		private async Task<bool> UpdateSelectedDb ( object sender, LoadedEventArgs e )
		{
			//*****************************************************************//
			// This now receives the DATA in SENDER, so we can load it directly in here (ONLY)
			//*****************************************************************//
			BankCollection bc = null;
			CustCollection cc = null;
			DetCollection dc = null;
			int selectedIndex = e . CurrSelection;

			if ( Flags . CurrentSqlViewer == null ) return false;
			if ( e . DataSource == null )
			{
				Debug . WriteLine ( $"\n*** NULL DATA returned by SQL Data  load call - Check Exceptions ***\n" );
				return false;
			}

			if ( e . CallerDb == "BANKACCOUNT" )
			{
				bc = sender as BankCollection;
				if ( bc . Count == 0 ) return false;

				if ( SqlViewerBankcollection . Count != bc . Count )
					Debug . WriteLine ( $"BANKACCOUNT : in SqlDbViewer.DataLoaded() method - \nBankcollection has {SqlViewerBankcollection . Count} records.\nsender has {bc . Count} records  returned by Sql Query" );
				if ( SqlViewerBankcollection . Count == 0 )
					return false;
			}
			else if ( e . CallerDb == "CUSTOMER" )
			{
				cc = sender as CustCollection;
				if ( cc . Count == 0 ) return false;
				if ( SqlViewerCustcollection . Count != cc . Count )
					Debug . WriteLine ( $"CUSTOMER: in SqlDbViewer.DataLoaded() method - \nCustcollection has {SqlViewerCustcollection . Count} records.\nsender has {cc . Count} records  returned by Sql Query" );
				if ( SqlViewerCustcollection . Count == 0 )
					return false;
			}
			else if ( e . CallerDb == "DETAILS" )
			{
				dc = sender as DetCollection;
				if ( dc . Count == 0 ) return false;
				if ( SqlViewerDetcollection . Count != dc . Count )
					Debug . WriteLine ( $"DETAILS : in SqlDbViewer.DataLoaded() method - \nDetcollection has {SqlViewerDetcollection . Count} records.\nsender has {dc . Count} records  returned by Sql Query" );
				if ( SqlViewerDetcollection . Count == 0 )
					return false;
			}

			string s = e . DataSource . ToString ( );
			int count = 0;
			int length = s . Length;
			int CurrentIndex = 0;
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
					CurrentDataGrid . ItemsSource = SqlViewerBankcollection;

					//					this . BankGrid . SelectedIndex = Flags . SqlBankCurrentIndex;
					//					this . BankGrid . SelectedItem = Flags . SqlBankCurrentIndex;

					var data = BankGrid . SelectedItem as BankAccountViewModel;
					StatusBar . Text = $"Data reloaded due to external changes in {CurrentDb} Viewer for Bank # {data?.CustNo}, Bank A/C # {data?.BankNo}";
					//					Debug . WriteLine ( $"\nDETAILS : SqlDbViewer : [{shortstring}] has provided {this . BankGrid . Items . Count} {e . CallerDb} records" );

					// this is the critical code to make it work
					ScrollViewer scroll = GetScrollViewer ( BankGrid ) as ScrollViewer;
					if ( scroll == null ) return false;
					//					Debug . WriteLine ( $"Calling ScrollToVerticalOffset () with		 {Flags . TopVisibleBankGridRow} or {topsaved}" );
					scroll . ScrollToVerticalOffset ( Flags . TopVisibleBankGridRow );
					Application . Current . Dispatcher . Invoke ( ( ) =>
					{
						scroll . ScrollToVerticalOffset ( Flags . TopVisibleBankGridRow );
					} );
					//					scroll . UpdateLayout ( );

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
					CurrentDataGrid . ItemsSource = SqlViewerCustcollection;

					//					this . CustomerGrid . SelectedIndex = Flags . SqlCustCurrentIndex;
					//					this . CustomerGrid . SelectedItem = Flags . SqlCustCurrentIndex;

					var data = CustomerGrid . SelectedItem as CustomerViewModel;
					StatusBar . Text = $"Data reloaded due to external changes in {CurrentDb} Viewer for Customer # {data?.CustNo}, Bank A/C # {data?.BankNo}";
					//					Debug . WriteLine ( $"\nDETAILS : SqlDbViewer : [{shortstring}] has provided {this . CustomerGrid . Items . Count} {e . CallerDb} records" );

					// this is the critical code to make it work
					ScrollViewer scroll = GetScrollViewer ( CustomerGrid ) as ScrollViewer;
					//					Debug . WriteLine ( $"Calling ScrollToVerticalOffset () with		 {Flags . TopVisibleCustGridRow} or {topsaved}" );
					if ( scroll == null ) return false;
					scroll . ScrollToVerticalOffset ( Flags . TopVisibleCustGridRow );
					Application . Current . Dispatcher . Invoke ( ( ) =>
					{
						scroll . ScrollToVerticalOffset ( Flags . TopVisibleCustGridRow );
					} );
					//					scroll . UpdateLayout ( );

					RefreshInProgress = false;
					if ( sender == null )
						Debug . WriteLine ( $"*** ERROR *** At End of data load, sender is NULL ????" );
					//					PrintCurrentviewportdata ( CustomerGrid );
					Flags . ActiveSqlViewer = this;

					if ( Flags . CurrentEditDbViewer != null )
					{
						// Notify currently open DbEbit as
						Thread . Sleep ( 500 );
						Flags . CurrentEditDbViewer . UpdateOnExternalChange ( );
					}
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
					CurrentDataGrid . ItemsSource = SqlViewerDetcollection;
					//					this . DetailsGrid . SelectedIndex = Flags . SqlDetCurrentIndex;
					//					this . DetailsGrid . SelectedItem = Flags . SqlDetCurrentIndex;

					var data = DetailsGrid . SelectedItem as DetailsViewModel;
					StatusBar . Text = $"Data reloaded due to external changes in {CurrentDb} Viewer for Bank # {data?.CustNo}, Bank A/C # {data?.BankNo}";
					//					Debug . WriteLine ( $"\nDETAILS : SqlDbViewer : [{shortstring}] has provided {DetailsGrid . Items . Count} {e . CallerDb} records" );

					// this is the critical code to make it work
					ScrollViewer scroll = new ScrollViewer ( );
					scroll = GetScrollViewer ( DetailsGrid ) as ScrollViewer;
					//					Debug . WriteLine ( $"Calling ScrollToVerticalOffset () with		 {Flags . TopVisibleDetGridRow} or {topsaved}" );
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
				Filters . Content = "Reset";

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


		private void EventControl_EditIndexChanged ( object sender, IndexChangedArgs e )
		{
			bool Proceed = false;
			// EditDb (oranother viewer if Flags.LinkviewerRecords is set has triggered the record index change, so we need ot update our grid index

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
				Triggered = true;
				//Handle index change made in another window
				if ( ( CurrentDb == "BANKACCOUNT" )//&& e . Sender != "BANKACCOUNT" )
					|| ( e . SenderId == "MultiBank" && BankGrid . Items . Count > 0 ) )
				{
					this . BankGrid . SelectedIndex = e . Row;
					this . BankGrid . SelectedItem = e . Row;
					Utils . SetUpGridSelection ( this . BankGrid, e . Row );
					BankGrid . Focus ( );
//					Utils . ScrollRecordInGrid ( BankGrid, e . Row );
//				BankGrid . Refresh ( );
				}
				else if ( ( CurrentDb == "CUSTOMER" )// && e . Sender != "CUSTOMER" )
					|| ( e . SenderId == "MultiCust" && CustomerGrid . Items . Count > 0 ) )
				{
					this.CustomerGrid . SelectedIndex = e . Row;
					this . CustomerGrid . SelectedItem = e . Row;
					Utils . SetUpGridSelection ( this . CustomerGrid, e . Row );
//					Utils . ScrollRecordInGrid ( CustomerGrid, e . Row );
				}
				else if ( ( CurrentDb == "DETAILS" )// && e.Sender != "DETAILS")
					|| ( e . SenderId == "MultiDet" && DetailsGrid . Items . Count > 0 ) )
				{
					Triggered = true;
					this . DetailsGrid . SelectedIndex = e . Row;
					this . DetailsGrid . SelectedItem = e . Row;
					Utils . SetUpGridSelection ( this . DetailsGrid, e . Row );
//					Utils . ScrollRecordInGrid ( DetailsGrid, e . Row );
					//Triggered = false;
				}
			}
		}

		/// <summary>
		/// Method to handle callback of DATAUPDATED when another viewer Updates a row of data
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void EventControl_EditDbDataUpdated ( object sender, LoadedEventArgs e )
		{
			// All working well when update is in another SqlDbViewer
			// Monday, 17 May 2021
			int currsel = 0;
			//			Debug . WriteLine ( $" 7-1 *** TRACE *** SQLDBVIEWER: EventControl_EditDbDataUpdated- Entering to handle trigger SendDataChanged from EditDb" );

			if ( CurrentDb == "BANKACCOUNT" )
			{
				//				Debug . WriteLine ( $" 7-2 *** TRACE *** SQLDBVIEWER: EventControl_EditDbDataUpdated- reloading Bank Data" );
				// We sent the message !!
				if ( e . DataSource == SqlViewerBankcollection ) return;
				RefreshInProgress = true;
				currsel = e . RowCount;
				RefreshInProgress = true;
				// This trigger  the IndexChanged Method in here
				//Data has been changed in EditDb, so reload it
				this . BankGrid . ItemsSource = null;
				this . BankGrid . Items . Clear ( );
				//				Debug . WriteLine ( $" 7-3 *** TRACE *** SQLDBVIEWER: EventControl_EditDbDataUpdated- reloading Bank Data" );
				Mouse . OverrideCursor = Cursors . Wait;
				SqlViewerBankcollection = await BankCollection . LoadBank ( SqlViewerBankcollection, 1 );
				//We cant use Collection syntax - it crashes it every time
				this . BankGrid . ItemsSource = SqlViewerBankcollection;
				this . BankGrid . SelectedIndex = currsel;
				try
				{
					this . BankGrid . SelectedItem = SqlViewerBankcollection . ElementAt ( currsel );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"{ex . Message}, @{ex . Data}" );
				}
				this . BankGrid . SelectedIndex = currsel;
				Utils . ScrollRecordInGrid ( this . BankGrid, currsel );
				RefreshInProgress = false;
				Mouse . OverrideCursor = Cursors . Arrow;
				Debug . WriteLine ( $"The record for Bank A/c {GetPrettyRowDetails ( this . BankGrid )} has been updated externally !" );
				StatusBar . Text = $"The record for Bank A/c {GetPrettyRowDetails ( this . BankGrid )} has been updated externally !";
				//				Debug . WriteLine ( $" 7-4-END *** TRACE *** SQLDBVIEWER: EventControl_EditDbDataUpdated- Bank Data Reload completed" );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				//				Debug . WriteLine ( $" 7-2 *** TRACE *** SQLDBVIEWER: EventControl_EditDbDataUpdated- reloading Customer Data" );
				// We sent the message !!
				if ( e . DataSource == SqlViewerCustcollection ) return;
				RefreshInProgress = true;
				currsel = e . RowCount;
				RefreshInProgress = true;
				// This trigger  the IndexChanged Method in here
				//Data has been changed in EditDb, so reload it
				this . CustomerGrid . ItemsSource = null;
				this . CustomerGrid . Items . Clear ( );
				//				Debug . WriteLine ( $" 7-3 *** TRACE *** SQLDBVIEWER: EventControl_EditDbDataUpdated- reloading Customer Data" );
				Mouse . OverrideCursor = Cursors . Wait;
				SqlViewerCustcollection = await CustCollection . LoadCust ( SqlViewerCustcollection );
				//We cant use Collection syntax - it crashes it every time
				this . CustomerGrid . ItemsSource = SqlViewerCustcollection;
				this . CustomerGrid . SelectedIndex = currsel;
				try
				{
					this . CustomerGrid . SelectedItem = SqlViewerCustcollection . ElementAt ( currsel );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"{ex . Message}, @{ex . Data}" );
				}
				this . CustomerGrid . SelectedIndex = currsel;
				this . CustomerGrid . Refresh ( );
				Utils . ScrollRecordInGrid ( this . CustomerGrid, currsel );
				RefreshInProgress = false;
				Mouse . OverrideCursor = Cursors . Arrow;
				Debug . WriteLine ( $"The record for Bank A/c {GetPrettyRowDetails ( this . CustomerGrid )} has been updated externally !" );
				StatusBar . Text = $"The record for Bank A/c {GetPrettyRowDetails ( this . CustomerGrid )} has been updated externally !";
				//				Debug . WriteLine ( $" 7-4-END *** TRACE *** SQLDBVIEWER: EventControl_EditDbDataUpdated- Customer Data Reload completed" );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				//				Debug . WriteLine ( $" 7-2 *** TRACE *** SQLDBVIEWER: EventControl_EditDbDataUpdated- reloading Details Data" );
				// We sent the message !!
				if ( e . DataSource == SqlViewerDetcollection ) return;
				RefreshInProgress = true;
				currsel = e . RowCount;
				RefreshInProgress = true;
				// This trigger  the IndexChanged Method in here
				//Data has been changed in EditDb, so reload it
				this . DetailsGrid . ItemsSource = null;
				this . DetailsGrid . Items . Clear ( );
				//				Debug . WriteLine ( $" 7-3 *** TRACE *** SQLDBVIEWER: EventControl_EditDbDataUpdated- reloading Details Data" );
				Mouse . OverrideCursor = Cursors . Wait;
				SqlViewerDetcollection = await DetCollection . LoadDet ( SqlViewerDetcollection );
				//We cant use Collection syntax - it crashes it every time
				this . DetailsGrid . ItemsSource = SqlViewerDetcollection;
				this . DetailsGrid . SelectedIndex = currsel;
				try
				{
					this . DetailsGrid . SelectedItem = SqlViewerDetcollection . ElementAt ( currsel );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"{ex . Message}, @{ex . Data}" );
				}
				this . DetailsGrid . SelectedIndex = currsel;
				this . DetailsGrid . Refresh ( );
				Utils . ScrollRecordInGrid ( this . DetailsGrid, currsel );
				RefreshInProgress = false;
				Mouse . OverrideCursor = Cursors . Arrow;
				Debug . WriteLine ( $"The record for Bank A/c {GetPrettyRowDetails ( this . DetailsGrid )} has been updated externally !" );
				StatusBar . Text = $"The record for Customer # {GetPrettyRowDetails ( this . DetailsGrid )} has been updated externally !";
				StatusBar . Refresh ( );
				//				Debug . WriteLine ( $" 7-4-END *** TRACE *** SQLDBVIEWER: EventControl_EditDbDataUpdated- Details Data Reload completed" );
			}
		}
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
		private async void OnDeletion ( string sender, string bank, string cust, int CurrentRow )
		{
			//Process the deletion for this grid here....
			if ( CurrentDb == "BANKACCOUNT" )
			{
				int currsel = 0;
				if ( CurrentDb != sender )
					currsel = this . BankGrid . SelectedIndex;
				else
					currsel = CurrentRow;
				currsel = CurrentRow;
				this . BankGrid . ItemsSource = null;
				this . BankGrid . Items . Clear ( );
				Mouse . OverrideCursor = Cursors . Wait;
				SqlViewerBankcollection = await BankCollection . LoadBank ( SqlViewerBankcollection, 1, false );
				this . BankGrid . ItemsSource = SqlViewerBankcollection;
				this . BankGrid . SelectedIndex = currsel;
				Utils . ScrollRecordInGrid ( this . BankGrid, currsel );
				this . BankGrid . Refresh ( );
				Mouse . OverrideCursor = Cursors . Arrow;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				int currsel = 0;
				if ( CurrentDb != sender )
					currsel = this . CustomerGrid . SelectedIndex;
				else
					currsel = CurrentRow;
				this . CustomerGrid . ItemsSource = null;
				this . CustomerGrid . Items . Clear ( );
				Mouse . OverrideCursor = Cursors . Wait;
				SqlViewerCustcollection = await CustCollection . LoadCust ( SqlViewerCustcollection );
				//				CustCollection b  =new CustCollection();
				//				Task . Run ( async ( ) => b . LoadCustomerTaskInSortOrderAsync ( false , 0 ) );
				this . CustomerGrid . ItemsSource = SqlViewerCustcollection;
				this . CustomerGrid . SelectedIndex = currsel;
				Utils . ScrollRecordInGrid ( this . CustomerGrid, currsel );
				this . CustomerGrid . Refresh ( );
				Mouse . OverrideCursor = Cursors . Arrow;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				int currsel = 0;
				if ( CurrentDb != sender )
					currsel = this . DetailsGrid . SelectedIndex;
				else
					currsel = CurrentRow;
				currsel = CurrentRow;
				this . DetailsGrid . ItemsSource = null;
				this . DetailsGrid . Items . Clear ( );
				//				DetCollection b  =new DetCollection();
				//				Task . Run ( async ( ) => b . LoadDetailsTaskInSortOrderAsync ( false , currsel ) );
				Mouse . OverrideCursor = Cursors . Wait;
				SqlViewerDetcollection = await DetCollection . LoadDet ( SqlViewerDetcollection );
				this . DetailsGrid . ItemsSource = SqlViewerDetcollection;
				this . DetailsGrid . SelectedIndex = currsel;
				Utils . ScrollRecordInGrid ( this . DetailsGrid, currsel );
				this . DetailsGrid . Refresh ( );
				Mouse . OverrideCursor = Cursors . Arrow;
			}
		}

		public void SqlDbViewer_AllViewersUpdate ( object sender, NotifyAllViewersOfUpdateEventArgs e )
		{
			int currow = 0;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				Debug . WriteLine ( $"Updating DataGrid (BankGrid) after change made elsewhere..." );
				this . BankGrid . ItemsSource = null;
				this . BankGrid . Items . Clear ( );
				this . BankGrid . ItemsSource = SqlViewerBankcollection;
				this . BankGrid . Refresh ( );
				this . BankGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerBankcollection );
				this . BankGrid . SelectedIndex = currow;
				Utils . ScrollRecordInGrid ( this . BankGrid, currow );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				Debug . WriteLine ( $"Updating DataGrid (customerGrid) after change made elsewhere..." );
				this . CustomerGrid . ItemsSource = null;
				this . CustomerGrid . Items . Clear ( );
				this . CustomerGrid . ItemsSource = SqlViewerCustcollection;
				this . CustomerGrid . Refresh ( );
				this . CustomerGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerCustcollection );
				this . CustomerGrid . SelectedIndex = currow;
				Utils . ScrollRecordInGrid ( this . CustomerGrid, currow );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				currow = this . DetailsGrid . SelectedIndex;
				Debug . WriteLine ( $"Updating DataGrid (DetailsGrid) after change made elsewhere..." );
				this . DetailsGrid . ItemsSource = null;
				this . DetailsGrid . ItemsSource = SqlViewerDetcollection;
				this . DetailsGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerDetcollection );
				this . DetailsGrid . SelectedIndex = currow;
				Utils . ScrollRecordInGrid ( this . DetailsGrid, currow );
			}
		}

		#endregion *** CRUCIAL Methods - Event CALLBACK for data loading

		#region General CallBack/Delegate stuff  - mosrtly NOT events as such

		// Use with EVENT NotifyOfDataChange (TRIGGER EVENT)
		//Declare our Event
		//declared in NameSpace
		public DataChangeArgs dca = new DataChangeArgs ( );

		//		private bool DoNotRespondToDbEdit = false;
		/// <summary>
		/// *** WORKING 27/4/21 ***
		///  A CallBack that recieves notifications from EditDb viewer that a selection has changed
		/// OR data has been changed by EditDb
		///  so that we can update our row position to match
		/// </summary>
		/// <param name="row"></param>
		/// <param name="CurrentDb"></param>
		public async void EditDbHasChangedIndex ( int DbEditChangeType, int row, string currentDb )
		{
			Debug . WriteLine ( $"SqlDbViewer : Data changed event notification received successfully." );

			int currsel = 0;
			// currentDb in this context is the SENDER TYPE
			if ( row == -1 )
				return;
			if ( DbEditChangeType == 2 )
				Debug . WriteLine (
					$"SqlDbViewer has received notification of DATA CHANGE from EditDb.\nUpdating SelectedIndex to {row} AND REFRESHING my DataGrid" );
			else if ( DbEditChangeType == 1 )
				Debug . WriteLine (
					$"SqlDbViewer has received notification of SIMPLE Index change from EditDb.\nUpdating SelectedIndex to {row}" );
			else
				Debug . WriteLine (
					$"SqlDbViewer has received UNFLAGGED notification of some type of change from EditDb.\nJust Updating SelectedIndex to {row}" );

			if ( CurrentDb == "BANKACCOUNT" )
			{
				currsel = this . BankGrid . SelectedIndex;
				RefreshInProgress = true;
				if ( DbEditChangeType == 2 )
				{
					//Data has been changed in EditDb, so reload it
					this . BankGrid . ItemsSource = null;
					this . BankGrid . Items . Clear ( );
					Mouse . OverrideCursor = Cursors . Wait;
					SqlViewerBankcollection = await BankCollection . LoadBank ( SqlViewerBankcollection, 1 );
					//We cant use Collection syntax - it crashes it every time
					this . BankGrid . ItemsSource = SqlViewerBankcollection;
					Mouse . OverrideCursor = Cursors . Arrow;
				}
				this . BankGrid . SelectedIndex = currsel;
				try
				{ this . BankGrid . SelectedItem = SqlViewerBankcollection . ElementAt ( currsel ); }
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"{ex . Message}, @{ex . Data}" );
				}
				// Ensure we are on  our current record
				this . BankGrid . SelectedIndex = currsel;
				Utils . ScrollRecordInGrid ( this . BankGrid, currsel );
				Refresh_Click ( null, null);
				RefreshInProgress = false;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				RefreshInProgress = true;
				currsel = this . CustomerGrid . SelectedIndex;
				RefreshInProgress = true;
				if ( DbEditChangeType == 2 )
				{
					//Data has been changed in EditDb, so reload it
					this . CustomerGrid . ItemsSource = null;
					this . CustomerGrid . Items . Clear ( );
					Mouse . OverrideCursor = Cursors . Wait;
					SqlViewerCustcollection = await CustCollection . LoadCust ( SqlViewerCustcollection );
					//We cant use Collection syntax - it crashes it every time
					this . CustomerGrid . ItemsSource = SqlViewerCustcollection;
					Mouse . OverrideCursor = Cursors . Arrow;
				}
				this . CustomerGrid . SelectedIndex = currsel;
				try
				{
					this . CustomerGrid . SelectedItem = SqlViewerCustcollection . ElementAt ( currsel );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"{ex . Message}, @{ex . Data}" );
				}
				this . CustomerGrid . SelectedIndex = currsel;
				Utils . ScrollRecordInGrid ( this . CustomerGrid, currsel );
				//				this . CustomerGrid . Refresh ( );
				Refresh_Click ( null, null );
				RefreshInProgress = false;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				RefreshInProgress = true;
				currsel = this . DetailsGrid . SelectedIndex;
				RefreshInProgress = true;
				// This trigger  the IndexChanged Method in here
				if ( DbEditChangeType == 2 )
				{
					//Data has been changed in EditDb, so reload it
					this . DetailsGrid . ItemsSource = null;
					this . DetailsGrid . Items . Clear ( );
					Mouse . OverrideCursor = Cursors . Wait;
					SqlViewerDetcollection = await DetCollection . LoadDet ( SqlViewerDetcollection );
					//We cant use Collection syntax - it crashes it every time
					this . DetailsGrid . ItemsSource = SqlViewerDetcollection;
					Mouse . OverrideCursor = Cursors . Arrow;
				}
				this . DetailsGrid . SelectedIndex = currsel;
				try
				{
					this . DetailsGrid . SelectedItem = SqlViewerDetcollection . ElementAt ( currsel );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"{ex . Message}, @{ex . Data}" );
				}
				this . DetailsGrid . SelectedIndex = currsel;
				Utils . ScrollRecordInGrid ( this . DetailsGrid, currsel );
				Refresh_Click ( null, null );
				RefreshInProgress = false;
			}
			// Reset our control flags
			ViewerChangeType = 0;
			EditChangeType = 0;
			// DEFINITELY LAST POINT AFTER INDEX CHANGE IN EDITDB
		}

		// No longer used
		public bool DbDataLoadedHandler ( object sender, DataLoadedArgs args )
		{
			//DataGrid Grid;
			//bool result = false;

			//Debug . WriteLine ( $"DbDataLoadedHandler Callback has been activated for {args . DbName} Db" );
			////Wrap all the data updating functionality into this function -
			////Set up the relevant DataGrid from assigning the ItemsSource to the current index and Refresh
			//// so it does not need to be handled elsewheree
			//if ( args . DbName == "BANKACCOUNT" )
			//{
			//	// Handle assigning  the Collections to the grids etc
			//	if ( SqlViewerBankcollection!= null )
			//	{
			//		this . BankGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerBankcollection);
			//		this . BankGrid . SelectedIndex = args . CurrentIndex;
			//		this . BankGrid . SelectedItem = args . CurrentIndex;
			//		this . BankGrid . Visibility = Visibility . Visible;
			//		this . DetailsGrid . Visibility = Visibility . Hidden;
			//		this . CustomerGrid . Visibility = Visibility . Hidden;
			//		//					ExtensionMethods . Refresh ( BankGrid );
			//		ParseButtonText ( true );
			//		UpdateAuxilliaries ( "BankAccount Data Loaded..." );
			//		Debug . WriteLine ( $"{this . BankGrid . Items . Count} Records reloaded for the {args . DbName} Db, CurrentIndex = {args . CurrentIndex}" );
			//		result = true;
			//	}
			//}
			//else if ( args . DbName == "CUSTOMER" )
			//{
			//	if ( CustCollection . Custcollection != null )
			//	{
			//		this . CustomerGrid . ItemsSource = CollectionViewSource . GetDefaultView ( CustCollection . Custcollection );
			//		this . CustomerGrid . SelectedIndex = args . CurrentIndex;
			//		this . CustomerGrid . SelectedItem = args . CurrentIndex;
			//		this . BankGrid . Visibility = Visibility . Hidden;
			//		this . DetailsGrid . Visibility = Visibility . Hidden;
			//		this . CustomerGrid . Visibility = Visibility . Visible;
			//		//					ExtensionMethods . Refresh ( CustomerGrid );
			//		ParseButtonText ( true );
			//		UpdateAuxilliaries ( "Customer Data Loaded..." );
			//		Debug . WriteLine ( $"{this . CustomerGrid . Items . Count} Records reloaded for the {args . DbName} Db, CurrentIndex = {args . CurrentIndex}" );
			//		result = true;
			//	}
			//}
			//else if ( args . DbName == "DETAILS" )
			//{
			//	if ( SqlViewerDetcollection != null )
			//	{
			//		this . DetailsGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerDetcollection );
			//		this . DetailsGrid . SelectedIndex = args . CurrentIndex;
			//		this . DetailsGrid . SelectedItem = args . CurrentIndex;
			//		this . BankGrid . Visibility = Visibility . Hidden;
			//		this . DetailsGrid . Visibility = Visibility . Visible;
			//		this . CustomerGrid . Visibility = Visibility . Hidden;
			//		//					ExtensionMethods . Refresh ( DetailsGrid );
			//		ParseButtonText ( true );
			//		UpdateAuxilliaries ( "Details Data Loaded..." );
			//		Debug . WriteLine ( $"{this . DetailsGrid . Items . Count} Records reloaded for the {args . DbName} Db, CurrentIndex = {args . CurrentIndex}" );

			//		result = true;
			//	}
			//}
			return true;
		}

		//*************************************************************************//
		// delegate object for others to access (listen for notifications sent by THIS CLASS)
		//		public delegate void SqlViewerNotify ( int status, string info, SqlDbViewer NewSqlViewer );
		//public static event  SqlViewerNotify Notifier;

		//		public event NotifyViewer //SendDbSelectorCommand;

		#endregion General CallBack/Delegate stuff  - mosrtly NOT events as such

		#region Callback response functions

		/// <summary>
		/// Calls  the relevant SQL data load calls to load data, fill Lists and populate Obs collections
		/// </summary>
		/// <param name="CurrentDb"></param>
		//*********************************************************************************************************//
		public async void GetData ( string CurrentDb )
		{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				if ( Flags . SqlBankGrid != null )
					Debug . WriteLine ( "\nA viewer showing BankAccount data is already open" );
				//				ExtensionMethods . Refresh ( this );
				//			ExtensionMethods . Refresh ( BankGrid );
				Debug . WriteLine ( $"Starting AWAITED task to load Bank Data via Sql" );
				Stopwatch sw = new Stopwatch ( );
				sw . Start ( );
				//This calls  LoadBankTask for us after sorting out the command line sort order requested
				// It also broadcasts to any other viewer to update  if needed
				// The Fn handles the Task.Run() to LOAD DATA from Db
				//				BankCollection bc = new BankCollection();
				//				Task< BankCollection> banktask =   bc. LoadBankTaskInSortOrderasync ( true , 0 );
				Mouse . OverrideCursor = Cursors . Wait;
				SqlViewerBankcollection = await BankCollection . LoadBank ( SqlViewerBankcollection, 1, false );
				sw . Stop ( );
				Debug . WriteLine ( $"BankAccount loaded {this . BankGrid . Items?.Count} records in {( double ) sw . ElapsedMilliseconds / ( double ) 1000} seconds " );
				Mouse . OverrideCursor = Cursors . Arrow;
				//ExtensionMethods . Refresh ( BankGrid );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				if ( Flags . SqlCustGrid != null )
					Debug . WriteLine ( "\nA viewer showing Customer data is already open\n" );
				//				ExtensionMethods . Refresh ( this );
				//			ExtensionMethods . Refresh ( CustomerGrid );
				Debug . WriteLine ( $"CUSTOMER: {this . CustomerGrid . Items?.Count}" );

				Stopwatch sw = new Stopwatch ( );
				sw . Start ( );
				//This calls  LoadcustomerTask for us after sorting out the command line sort order requested
				// It also broadcasts to any other viewer to update  if needed
				// The Fn handles the Task.Run() to LOAD DATA from Db
				//CustCollection cc = new CustCollection ();
				//Task< CustCollection> custtask  = cc. LoadCustomerTaskInSortOrderAsync ( true , 0 );
				//Custcollection = custtask . Result;
				Mouse . OverrideCursor = Cursors . Wait;
				SqlViewerCustcollection = await CustCollection . LoadCust ( SqlViewerCustcollection );
				sw . Stop ( );
				Debug . WriteLine ( $"Customer loaded {this . CustomerGrid . Items?.Count} records in {( double ) sw . ElapsedMilliseconds / ( double ) 1000} seconds " );
				Mouse . OverrideCursor = Cursors . Arrow;
				//ExtensionMethods . Refresh ( CustomerGrid );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				//Loads the data Asynchronously
				//				ExtensionMethods . Refresh ( DetailsGrid );
				Stopwatch sw1 = new Stopwatch ( );
				sw1 . Start ( );
				Debug . WriteLine ( $"Calling AWAITED task to load Details Data into DataGrid via Sql" );
				// The Fn handles the Task.Run() to LOAD DATA from Db
				//This calls  LoadDetailsTask for us after sorting out the command line sort order requested
				// It also broadcasts to any other viewer to update  if needed
				//DetCollection dc = new DetCollection();
				//Task< DetCollection> dettask  =  dc. LoadDetailsTaskInSortOrderAsync (true , 0);
				//Detcollection = dettask . Result;
				Mouse . OverrideCursor = Cursors . Wait;
				SqlViewerDetcollection = await DetCollection . LoadDet ( SqlViewerDetcollection );
				this . DetailsGrid . ItemsSource = SqlViewerDetcollection;
				sw1 . Stop ( );
				Debug . WriteLine ( $"{( double ) sw1 . ElapsedMilliseconds / ( double ) 1000} seconds - Details loading Task completed\n{this . DetailsGrid . Items?.Count} records loaded" );
				Mouse . OverrideCursor = Cursors . Arrow;
			}
		}

		//*********************************************************************************************************//
		public void SetGridVisibility ( string CurrentDb )
		{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				Utils . GetWindowHandles ( );
				this . BankGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerBankcollection );
				this . BankGrid . SelectedIndex = 0;
				ExtensionMethods . Refresh ( BankGrid );
			}
			if ( CurrentDb == "CUSTOMER" )
			{
				Utils . GetWindowHandles ( );

				this . CustomerGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerCustcollection );
				this . CustomerGrid . SelectedIndex = 0;
				ExtensionMethods . Refresh ( CustomerGrid );
			}
			if ( CurrentDb == "DETAILS" )
			{
				Utils . GetWindowHandles ( );
				this . DetailsGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerDetcollection );
				this . DetailsGrid . SelectedIndex = 0;
				ExtensionMethods . Refresh ( DetailsGrid );
			}
		}

		#endregion Callback response functions

		#region load/startup / Close down

		//*********************************************************************************************************//
		private void OnWindowLoaded ( object sender, RoutedEventArgs e )
		{
			// THIS IS WHERE WE NEED TO SET THIS FLAG
			Flags . SqlViewerIsLoading = true;

			this . Show ( );
			int currentindex = -1;
			ThisViewer = this;

			this . Show ( );
			//			this. Refresh ( );
			BringIntoView ( );
			//This is the EventHandler declared  in THIS FILE
			LoadedEventArgs ex = new LoadedEventArgs ( );

			ex . CallerDb = CurrentDb;

			//Broadcast that we have Arrived !!
			//			OnDataLoaded ( CurrentDb );

			//Use Delegate to notify DbSelector
			//			this . WaitMessage . Visibility = Visibility . Visible;
			this . BankGrid . Visibility = Visibility . Collapsed;
			this . CustomerGrid . Visibility = Visibility . Collapsed;
			this . DetailsGrid . Visibility = Visibility . Collapsed;
			//			ExtensionMethods . Refresh ( this );
			//			//SendDbSelectorCommand ( 103, $"<<< SqlDbViewer has Finished OnWindowLoading", Flags . CurrentSqlViewer );
			if ( CurrentDb == "BANKACCOUNT" )
			{
				// Set up the various globalflags we use to control activity
				//				Flags . ActiveDbGrid = this . BankGrid;
				Flags . SqlBankGrid = this . BankGrid;
				Flags . SqlBankViewer = this;

				Flags . ActiveSqlViewer = this;
				Flags . ActiveSqlGrid = this . BankGrid;
				MainWindow . gv . SqlBankViewer = ( SqlDbViewer ) this;
				Flags . SetGridviewControlFlags ( this, this . BankGrid );

				Utils . SetUpGridSelection ( this . BankGrid, 0 );
				Utils . ScrollRecordIntoView ( this . BankGrid, this . BankGrid . SelectedIndex );
				//				Flags . SqlBankCurrentIndex = 0;
				this . BankGrid . Visibility = Visibility . Visible;
				SetScrollVariables ( BankGrid );
				SetButtonColor ( RefreshBtn, "BLUE" );
				Utils . GridInitialSetup ( BankGrid, 0 );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				// Set up the various globalflags we use to control activity
				//				Flags . ActiveDbGrid = this . CustomerGrid;
				Flags . SqlCustGrid = this . CustomerGrid;
				Flags . SqlCustViewer = this;
				Flags . ActiveSqlViewer = this;
				MainWindow . gv . SqlCustViewer = ( SqlDbViewer ) this;
				Flags . SetGridviewControlFlags ( this, this . CustomerGrid );

				Utils . SetUpGridSelection ( this . CustomerGrid, 0 );
				Utils . ScrollRecordIntoView ( this . CustomerGrid, this . CustomerGrid . SelectedIndex );
				//				Flags . SqlCustCurrentIndex = 0;
				this . CustomerGrid . Visibility = Visibility . Visible;
				SetScrollVariables ( CustomerGrid );
				SetButtonColor ( RefreshBtn, "YELLOW" );
				Utils . GridInitialSetup ( CustomerGrid, 0 );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				// Set up the various globalflags we use to control activity
				//				Flags . ActiveDbGrid = this . DetailsGrid;
				Flags . SqlDetGrid = this . DetailsGrid;
				Flags . SqlDetViewer = this;
				MainWindow . gv . SqlDetViewer = ( SqlDbViewer ) this;
				Flags . SetGridviewControlFlags ( this, this . DetailsGrid );

				Utils . SetUpGridSelection ( this . DetailsGrid, 0 );
				Utils . ScrollRecordIntoView ( this . DetailsGrid, this . DetailsGrid . SelectedIndex );
				//				Flags . SqlDetCurrentIndex = this . DetailsGrid . SelectedIndex;
				this . DetailsGrid . Visibility = Visibility . Visible;
				SetScrollVariables ( DetailsGrid );
				SetButtonColor ( RefreshBtn, "GREEN" );
				Utils . SetUpGridSelection ( DetailsGrid, 0 );
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
			ParseButtonText ( true );
			// clear global "loading new window" flag
			Flags . SqlViewerIsLoading = false;
			if ( Flags . LinkviewerRecords )
				LinkRecords . IsChecked = true;
			Mouse . OverrideCursor = Cursors . Arrow;
		}
		private void Window_Closed ( object sender, EventArgs e )
		{
			// clear final Viewer pointer when ALL viewers are closed down

			//			if ( this . BankGrid != null )
			//			{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				this . BankGrid . ItemsSource = null;
				BankGrid?.Items . Clear ( );
				dtBank?.Rows . Clear ( );
				Flags . BankEditDb = null;
				Flags . CurrentEditDbViewerBankGrid = null;
				Flags . CurrentBankViewer = null;
				// Tidy up our Db pointers
				SqlViewerBankcollection?.Clear ( );
				SqlViewerBankcollection = null;
				//Flags . CurrentBankViewer = null;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				this . CustomerGrid . ItemsSource = null;
				CustomerGrid?.Items . Clear ( );
				dtCust?.Rows . Clear ( );
				Flags . CustEditDb = null;
				Flags . CurrentEditDbViewerCustomerGrid = null;
				Flags . CurrentCustomerViewer = null;
				// Tidy up our Db pointers
				SqlViewerCustcollection . Clear ( );
				SqlViewerCustcollection = null;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				this . DetailsGrid . ItemsSource = null;
				DetailsGrid?.Items . Clear ( );
				dtDetails?.Rows . Clear ( );
				Flags . DetEditDb = null;
				Flags . CurrentEditDbViewerDetailsGrid = null;
				Flags . CurrentDetailsViewer = null;
				// Tidy up our Db pointers
				SqlViewerDetcollection . Clear ( );
				SqlViewerDetcollection = null;
			}
			// clear EditDb flags in case one is open as closing this viewer does NOT  call WindowClosed !!!
			Flags . ActiveEditGrid = null;
			Flags . CurrentEditDbViewer = null;

			EventControl . ViewerIndexChanged -= EventControl_EditIndexChanged;
			EventControl . EditIndexChanged -= EventControl_EditIndexChanged;
			EventControl . MultiViewerIndexChanged -= EventControl_EditIndexChanged;
			//			EventControl . ViewerDataHasBeenChanged -= EditDbHasChangedIndex;
			// change made in Multi Viewer
			EventControl . MultiViewerDataUpdated -= EventControl_EditDbDataUpdated;
			// Another SQL viewer has updated a record  notification handler
			EventControl . ViewerDataUpdated -= EventControl_EditDbDataUpdated;

			// clear our callback function subscription - DbDataLoadedHandler(object, DataLoadedArgs)
			EventControl . BankDataLoaded -= SqlDbViewer_BankDataLoaded;
			EventControl . CustDataLoaded -= SqlDbViewer_BankDataLoaded;
			EventControl . DetDataLoaded -= SqlDbViewer_BankDataLoaded;

			// Event triggers when a Specific Db viewer (BankDbViewer etc) updates the data
			EventControl . BankDataLoaded -= EventControl_EditDbDataUpdated;
			EventControl . CustDataLoaded -= EventControl_EditDbDataUpdated;
			EventControl . DetDataLoaded -= EventControl_EditDbDataUpdated;

			// Main update notification handler
			EventControl . EditDbDataUpdated -= EventControl_EditDbDataUpdated;

			//			NotifyOfDataLoaded -= DbDataLoadedHandler;


			EventControl . RecordDeleted -= OnDeletion;

			if ( MainWindow . gv . ViewerCount == 0 )
			{
				// No more Viewers open, so clear Viewers list in DbSelector
				MainWindow . gv . SqlViewerWindow = null;

#pragma  TODO Clear Viewerlist in DbSelector
				//call a method that handles this
				//				DbSelector . CloseDeleteAllViewers ( );
			}
			UpdateDbSelectorBtns ( Flags . CurrentSqlViewer );

			// make sure the global pointer to any EditDb we may have opened is cleared
			//			BankAccountViewModel . EditdbWndBank = null;

			Debug . WriteLine ( $"{CurrentDb} has Unsubscribed from All events successfully" );
			Debug . WriteLine ( $"***Window has just closed***" );
		}

		#endregion load/startup / Close down

		#region EVENTHANDLERS

		//*******************************************************************************************************//
		// EVENT HANDLER
		//*******************************************************************************************************//

		//*********************************************************************************************************//

		//-------------------------------------------------------------------------------------------------------------------------------------------------//

		//-------------------------------------------------------------------------------------------------------------------------------------------------//

		/// <summary>
		///  Function that is broadcasts a notification to whoever to
		///  notify that one of the Obs collections has been changed by something
		/// </summary>
		/// <param name="o"> The sending object</param>
		/// <param name="args"> Sender name and Db Type</param>
		//		private static bool hasupdated = false;

		public void SendDataChanged ( SqlDbViewer o, DataGrid Grid, string dbName )
		{
			// Databases have DEFINITELY been updated successfully after a change
			// We Now Broadcast this to ALL OTHER OPEN VIEWERS here and now

			//dca . SenderName = o . ToString ( );
			//dca . DbName = dbName;

			if ( dbName == "BANKACCOUNT" )
			{
				EventControl . TriggerViewerDataUpdated ( SqlViewerBankcollection,
					new LoadedEventArgs
					{
						CallerDb = "BANKACCOUNT",
						DataSource = SqlViewerBankcollection,
						RowCount = this . BankGrid . SelectedIndex
					} );
			}
			else if ( dbName == "CUSTOMER" )
			{
				EventControl . TriggerViewerDataUpdated ( SqlViewerCustcollection,
					new LoadedEventArgs
					{
						CallerDb = "CUSTOMER",
						DataSource = SqlViewerCustcollection,
						RowCount = this . CustomerGrid . SelectedIndex
					} );
			}
			else if ( dbName == "DETAILS" )
			{
				EventControl . TriggerViewerDataUpdated ( SqlViewerDetcollection,
					new LoadedEventArgs
					{
						CallerDb = "DETAILS",
						DataSource = SqlViewerDetcollection,
						RowCount = this . DetailsGrid . SelectedIndex
					} );
			}
			Mouse . OverrideCursor = Cursors . Arrow;
		}

		#endregion EVENTHANDLERS

		#region Utility functions for Grid loading/Switching

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
				dtBank?.Clear ( );
				SqlViewerBankcollection = null;
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
				dtCust?.Clear ( );
				SqlViewerCustcollection = null;
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
				dtDetails?.Clear ( );
				SqlViewerDetcollection = null;
				this . DetailsGrid . Visibility = Visibility . Hidden;
			}
		}

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

		#region Show_Bank/Cust/Details cleanup functions for switching Db views

		/// <summary>
		/// Changing Db in viewer, so tidy up current pointers and also unsubscribe from EVENT Notifications
		/// </summary>
		private void ClearCurrentFlags ( )
		{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				Flags . SqlBankViewer = null;
				Flags . SqlBankGrid = null;
				//				Flags . SqlBankCurrentIndex = 0;
			}
			if ( CurrentDb == "CUSTOMER" )
			{
				Flags . SqlCustViewer = null;
				Flags . SqlCustGrid = null;
				//				Flags . SqlCustCurrentIndex = 0;
			}
			if ( CurrentDb == "DETAILS" )
			{
				Flags . SqlDetViewer = null;
				Flags . SqlDetGrid = null;
				//				Flags . SqlDetCurrentIndex = 0;
			}
			// Generic variables we need to clear
			Flags . ActiveSqlViewer = this;
			Flags . ActiveSqlGrid = null;
		}

		public async void ShowBank_Click ( object sender, RoutedEventArgs e )
		{
			//Close any EditDb window that may be open
			if ( MainWindow . gv . SqlCurrentEditViewer != null )
				MainWindow . gv . SqlCurrentEditViewer . Close ( );

			if ( Flags . SqlBankViewer != null )
			{
				// viewer already open with Bankgrid visible, so switch to it
				Flags . SqlBankViewer . Focus ( );
				return;
			}
			//We  re switching from another Db !
			if ( Flags . CurrentSqlViewer != null )
				//SendDbSelectorCommand ( 102, $">>> Entering ShowBank_Click()", Flags . CurrentSqlViewer );
				// Make sure this window has it's pointer "Registered" cos we can
				// Click the button before the window has had focus set
				Mouse . OverrideCursor = Cursors . Wait;

			// We have to sort out the control structures BEFORE loading Customer Db Data and showing it
			CleanViewerGridData ( );

			Flags . CurrentSqlViewer = this;

			if ( Flags . SqlBankGrid != null && this . BankGrid . Items . Count > 0 && !Flags . IsMultiMode )
			{
				// Already got a Bank Grid open !
				if ( MainWindow . gv . SqlBankViewer == null ) return;

				MainWindow . gv . SqlBankViewer?.Focus ( );
				MainWindow . gv . SqlBankViewer?.BringIntoView ( );
				ExtensionMethods . Refresh ( MainWindow . gv . SqlBankViewer );
				Mouse . OverrideCursor = Cursors . Arrow;
				return;
			}

			//if ( CurrentDb == "CUSTOMER" )
			//	CustCollection . UnSubscribeToLoadedEvent ( Custcollection );
			//if ( CurrentDb == "DETAILS" )
			//	DetCollection . UnSubscribeToLoadedEvent ( Detcollection );

			//// subscribe to data loaded event (for the new Bank window)
			//DetCollection . SubscribeToLoadedEvent ( Detcollection );

			// We have to Unsubscribe from previous Db Types Evet handlers BEFORE loading Bank Db Data and showing it
			ClearCurrentFlags ( );
			// Now we can subscribe to data loaded event (for the new Bank window)
			CurrentDb = "BANKACCOUNT";

			//Reset our Datgrid pointer
			CurrentDataGrid = BankGrid;
			Flags . SqlBankGrid = BankGrid;
			//Reset the MultiMode flag as user has requested a FULL reload
			Flags . IsMultiMode = false;
			this . BankGrid . Visibility = Visibility . Visible;
			this . CustomerGrid . Visibility = Visibility . Hidden;
			this . DetailsGrid . Visibility = Visibility . Hidden;

			// We MUST subscribe to EVENTS before calling the load  data functiuonality
			SubscribeToEvents ( );

			// Important call - it sets up global flags for all/any of the allowed viiewer windows
			if ( !SetFlagsForViewerGridChange ( this, BankGrid ) ) return;

			SqlViewerBankcollection = await BankCollection . LoadBank ( SqlViewerBankcollection, 1 );
			this . BankGrid . ItemsSource = SqlViewerBankcollection;
			//// create GV[] variables for this new viewer grid
			DbSelector . UpdateControlFlags ( Flags . CurrentSqlViewer, CurrentDb, "" );

			UpdateAuxilliaries ( "Bank Data Loaded..." );
			// UPDATE DbSelector ViewersList entry
			this . BankGrid . SelectedIndex = 0;
			if ( this . BankGrid . SelectedItem != null )
			{
				BankAccountViewModel c = this . BankGrid . SelectedItem as BankAccountViewModel;
				string str = $"Bank - # {c . CustNo}, Bank #{c . BankNo}, Customer # {c . CustNo}, £{c . Balance}, {c . IntRate}%,  {c . ODate}";
				UpdateDbSelectorItem ( str );
			}
			SetScrollVariables ( BankGrid );
			ParseButtonText ( true );
			this . Focus ( );
			IsFiltered = "";
			SetButtonColor ( RefreshBtn, "BLUE" );
			Mouse . OverrideCursor = Cursors . Arrow;
		}

		//*********************************************************************************************************//
		/// <summary>
		/// Fetches SQL data for Customer Db and fills relevant DataGrid
		/// </summary>
		//*********************************************************************************************************//
		public async void ShowCust_Click ( object sender, RoutedEventArgs e )
		{
			int CurrentSelection = 0;

			//Close any EditDb window that may be open
			if ( MainWindow . gv . SqlCurrentEditViewer != null )
				MainWindow . gv . SqlCurrentEditViewer . Close ( );

			if ( Flags . SqlCustViewer != null )
			{
				// Customer Grid NOT open in this viewer, check which grid IS OPEN here ?

				// viewer already open with Bankgrid visible, so switch to it
				Flags . SqlCustViewer . Focus ( );
				return;

			}
			if ( Flags . CurrentSqlViewer != null )
				//SendDbSelectorCommand ( 102, $">>> Entering ShowCust_Click()", Flags . CurrentSqlViewer );

				Mouse . OverrideCursor = Cursors . Wait;

			// We have to sort out the control structures BEFORE loading Customer Db Data and showing it
			CleanViewerGridData ( );
			// Make sure this window has it's pointer "Registreded" cos we can
			// Click the button before the window has had focus set
			Flags . CurrentSqlViewer = this;

			if ( Flags . SqlCustGrid != null && this . CustomerGrid . Items . Count > 0 && !Flags . IsMultiMode )
			{
				MainWindow . gv . SqlCustViewer . Focus ( );
				MainWindow . gv . SqlCustViewer . BringIntoView ( );
				ExtensionMethods . Refresh ( MainWindow . gv . SqlCustViewer );
				Mouse . OverrideCursor = Cursors . Arrow;
				return;
			}

			// We have to Unsubscribe from previous Db Types Evet handlers BEFORE loading Customer Db Data and showing it
			ClearCurrentFlags ( );

			CurrentDb = "CUSTOMER";
			//Reset our Datgrid pointer
			CurrentDataGrid = this . CustomerGrid;
			Flags . SqlCustGrid = this . CustomerGrid;
			//Reset the MultiMode flag as user has requested a FULL reload
			Flags . IsMultiMode = false;
			this . BankGrid . Visibility = Visibility . Hidden;
			this . CustomerGrid . Visibility = Visibility . Visible;
			this . DetailsGrid . Visibility = Visibility . Hidden;
			this . CustomerGrid . ItemsSource = null;
			// We MUST subscribe to EVENTS before calling the load  data functiuonality
			SubscribeToEvents ( );
			// LOAD THE NEW DATA
			// Important call - it sets up global flags for all/any of the allowed viiewer windows
			if ( !SetFlagsForViewerGridChange ( this, CustomerGrid ) )
				return;
			//This calls  LoadCustomerTask for us after sorting out the command line sort order requested
			///and it clears down any  existing data in DataTable or Collection
			SqlViewerCustcollection = await CustCollection . LoadCust ( SqlViewerCustcollection );
			Debug . WriteLine ( $"Db count = {SqlViewerCustcollection . Count}" );
			Thread . Sleep ( 250 );
			this . CustomerGrid . ItemsSource = SqlViewerCustcollection;
			this . CustomerGrid . SelectedIndex = 0;
			this . CustomerGrid . SelectedItem = 0;
			Debug . WriteLine ( $"DataGrid count = {this . CustomerGrid . Items . Count}, selected : {this . CustomerGrid . SelectedIndex}" );
			// create GV[] variables for this new viewer grid
			DbSelector . UpdateControlFlags ( Flags . CurrentSqlViewer, CurrentDb, "" );

			//// UPDATE DbSelector ViewersList entry
			if ( this . CustomerGrid . SelectedItem != null )
			{
				CustomerViewModel c = this . CustomerGrid . SelectedItem as CustomerViewModel;
				string str = $"Customer - # {c . CustNo}, Bank #@{c . BankNo}, {c . LName}, {c . Town}, {c . County} {c . PCode}";
				UpdateDbSelectorItem ( str );
			}
			SetScrollVariables ( CustomerGrid );
			ParseButtonText ( true );
			this . Focus ( );
			this . Refresh ( );
			Utils . ScrollRecordIntoView ( this . CustomerGrid, this . CustomerGrid . SelectedIndex );
			//			this .CustomerGrid . ScrollIntoView ( this . CustomerGrid );
			IsFiltered = "";
			SetButtonColor ( RefreshBtn, "YELLOW" );
			Mouse . OverrideCursor = Cursors . Arrow;
		}

		//*********************************************************************************************************//
		/// <summary>
		/// Fetches SQL data for DetailsViewModel Db and fills relevant DataGrid
		/// <param name="sender"></param>
		/// <param name="e"></param></summary>
		//*****************************************************************************************//
		public async void ShowDetails_Click ( object sender, RoutedEventArgs e )
		{
			int CurrentSelection = 0;

			//Close any EditDb window that may be open
			if ( MainWindow . gv . SqlCurrentEditViewer != null )
				MainWindow . gv . SqlCurrentEditViewer . Close ( );

			if ( Flags . SqlDetViewer != null )
			{
				// viewer already open with Bankgrid visible, so switch to it
				Flags . SqlDetViewer . Focus ( );
				return;
			}
			if ( Flags . CurrentSqlViewer != null )
				//SendDbSelectorCommand ( 102, $">>> Entering ShowDetails_Click()", Flags . CurrentSqlViewer );

				if ( Flags . FilterCommand == "" )
				{
					// We have to sort out the control structures BEFORE loading Details Db Data and showing it
					CleanViewerGridData ( );
					ClearCurrentFlags ( );
				}

			//if ( CurrentDb == "CUSTOMER" )
			//	CustCollection . UnSubscribeToLoadedEvent ( Custcollection );
			//if ( CurrentDb == "DETAILS" )
			//	DetCollection . UnSubscribeToLoadedEvent ( Detcollection );

			//// subscribe to data loaded event (for the new Bank window)
			//DetCollection . SubscribeToLoadedEvent ( Detcollection );

			CurrentDb = "DETAILS";
			//Reset our Datgrid pointer
			CurrentDataGrid = DetailsGrid;
			Flags . SqlDetGrid = DetailsGrid;

			Mouse . OverrideCursor = Cursors . Wait;
			// Make sure this window has it's pointer "Registered" cos we can
			// Click the button before the window has had focus set
			Flags . CurrentSqlViewer = this;
			if ( Flags . SqlDetGrid != DetailsGrid && this . DetailsGrid . Items . Count > 0 && !Flags . IsMultiMode )
			{
				MainWindow . gv . SqlDetViewer . Focus ( );
				MainWindow . gv . SqlDetViewer . BringIntoView ( );
				ExtensionMethods . Refresh ( MainWindow . gv . SqlDetViewer );
				Mouse . OverrideCursor = Cursors . Arrow;
				return;
			}

			this . BankGrid . Visibility = Visibility . Hidden;
			this . CustomerGrid . Visibility = Visibility . Hidden;
			this . DetailsGrid . Visibility = Visibility . Visible;
			//Reset the MultiMode flag as user has requested a FULL reload
			Flags . IsMultiMode = false;

			// Important call - it sets up global flags for all/any of the allowed viiewer windows
			if ( !SetFlagsForViewerGridChange ( this, DetailsGrid ) )
				return;

			Mouse . OverrideCursor = Cursors . Wait;
			SqlViewerDetcollection = await DetCollection . LoadDet ( SqlViewerDetcollection );
			this . DetailsGrid . ItemsSource = SqlViewerDetcollection;

			// create GV[] variables for this new viewer grid
			DbSelector . UpdateControlFlags ( Flags . CurrentSqlViewer, CurrentDb, "" );
			Mouse . OverrideCursor = Cursors . Arrow;
			//			ParseButtonText ( true );
			//			UpdateAuxilliaries ( "Details Data Loaded..." );
			//SendDbSelectorCommand ( 111, $"SqlDbViewer  - calling AWAIT Task LoadDetailsTask", Flags . CurrentSqlViewer );
			//			ParseButtonText ( true );
			// UPDATE DbSelector ViewersList entry
			this . DetailsGrid . SelectedIndex = 0;
			if ( this . DetailsGrid . SelectedItem != null )
			{
				DetailsViewModel c = this . DetailsGrid . SelectedItem as DetailsViewModel;
				string str = $"Bank - # {c . CustNo}, Bank #{c . BankNo}, Customer # {c . CustNo}, £{c . Balance}, {c . IntRate}%,  {c . ODate}";
				UpdateDbSelectorItem ( str );
			}
			Flags . CurrentDetailsViewer = this;
			SetScrollVariables ( DetailsGrid );
			ParseButtonText ( true );
			this . Focus ( );
			IsFiltered = "";
			//			}
			Mouse . OverrideCursor = Cursors . Arrow;
			SetButtonColor ( RefreshBtn, "GREEN" );
			return;
		}

		#endregion Show_Bank/Cust/Details cleanup functions for switching Db views

		#region Standard Click Events

		private void ExitFilter_Click ( object sender, RoutedEventArgs e )
		{
			//Just "Close" the Filter panel
			//			FilterFrame.Visibility = Visibility.Hidden;
		}

		//*********************************************************************************************************//
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

		//*********************************************************************************************************//
		private void ContextMenu2_Click ( object sender, RoutedEventArgs e )
		{
			//Delete current Row
			BankAccountViewModel dg = sender as BankAccountViewModel;
			DataRowView row = ( DataRowView ) this . BankGrid . SelectedItem;
		}

		//*********************************************************************************************************//
		private void ContextMenu3_Click ( object sender, RoutedEventArgs e )
		{
			//Close Window
		}

		//*********************************************************************************************************//
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
			}
			else
			{
				Flags . IsMultiMode = true;
				ResetOptionButtons ( 1 );
				SetButtonGradientBackground ( Multiaccounts );
				StatusBar . Text = "Database is filtered to ONLY show Customers with Multiple A/c's";
			}
			if ( CurrentDb == "BANKACCOUNT" )
			{
				this . BankGrid . ItemsSource = null;
				this . BankGrid . Items . Clear ( );
				dtBank . Clear ( );
				SqlViewerBankcollection = await BankCollection . LoadBank ( SqlViewerBankcollection, 1, false );
				this . BankGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerBankcollection );
				ExtensionMethods . Refresh ( this . BankGrid );
				Count . Text = this . BankGrid . Items . Count . ToString ( );
				ParseButtonText ( true );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				this . CustomerGrid . ItemsSource = null;
				this . CustomerGrid . Items . Clear ( );
				dtCust . Clear ( );
				SqlViewerCustcollection = await CustCollection . LoadCust ( SqlViewerCustcollection );
				this . CustomerGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerCustcollection );
				ExtensionMethods . Refresh ( this . CustomerGrid );
				Count . Text = this . CustomerGrid . Items . Count . ToString ( );
				ParseButtonText ( true );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				this . DetailsGrid . ItemsSource = null;
				this . DetailsGrid . Items . Clear ( );
				dtDetails . Clear ( );
				Mouse . OverrideCursor = Cursors . Wait;
				SqlViewerDetcollection = await DetCollection . LoadDet ( SqlViewerDetcollection );
				this . DetailsGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerDetcollection );
				ExtensionMethods . Refresh ( this . DetailsGrid );
				Count . Text = this . DetailsGrid . Items . Count . ToString ( );
				ParseButtonText ( true );
				Mouse . OverrideCursor = Cursors . Arrow;
			}
			// Set the gradient background
			SetButtonGradientBackground ( Filters );
		}

		//*********************************************************************************************************//
		private void ContextMenuFind_Click ( object sender, RoutedEventArgs e )
		{
			// find something - this returns  the top rows data in full
			BankAccountViewModel b = this . BankGrid . Items . CurrentItem as BankAccountViewModel;
		}

		//*********************************************************************************************************//
		private void CloseAccount_Click ( object sender, RoutedEventArgs e )
		{
			//			//Now get the actual data item behind the selected row
			//			if ( CurrentDb == "BANKACCOUNT" )
			//			{
			//				int id = BankCurrentRowAccount . Id;
			//				BankCurrentRowAccount . CDate = DateTime . Now;
			//#pragma checksum   why is thi smissing ?????
			//				//ViewerGrid_RowEditEnded (null, null);
			//				this . BankGrid . Items . Refresh ( );
			//			}
			//			else if ( CurrentDb == "CUSTOMER" )
			//			{
			//				int id = CustomerCurrentRowAccount . Id;
			//				CustomerCurrentRowAccount . CDate = DateTime . Now;
			//			}
			//			else if ( CurrentDb == "DETAILS" )
			//			{
			//				int id = DetailsCurrentRowAccount . Id;
			//				DetailsCurrentRowAccount . CDate = DateTime . Now;
			//			}
		}

		#endregion Standard Click Events

		#region grid row selection code

		private bool CheckForDataChange ( BankAccountViewModel bvm )
		{
			//object newvalue = null;
			//if (OriginalCellData == null)
			//	return false;
			//switch (OriginalDataType.ToUpper ())
			//{
			//	case "BANKNO":
			//		newvalue = bvm.BankNo;
			//		break;
			//	case "CUSTO":
			//		newvalue = bvm.CustNo;
			//		break;
			//	case "ACTYPE":
			//		newvalue = bvm.AcType;
			//		break;
			//	case "BALANCE":
			//		newvalue = bvm.Balance;
			//		break;
			//	case "INTRATE":
			//		newvalue = bvm.IntRate;
			//		break;
			//	case "ODATE":
			//		newvalue = bvm.ODate;
			//		break;
			//	case "CDATE":
			//		newvalue = bvm.CDate;
			//		break;
			//}

			//if (OriginalCellData == newvalue)
			return true;
			//else
			//	return false;
			////			if(c.   OriginalCellData
			////		       OriginalDataType
		}

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
				//Debugger . Break ( );
				if ( c == null ) return;
				string date = Convert . ToDateTime ( c . ODate ) . ToShortDateString ( );
				string s = $"Bank - # {c . CustNo}, Bank #{c . BankNo}, Customer # {c . CustNo}, £{c . Balance}, {c . IntRate}%,  {date}";
				// ensure global flag is cleared after loading a viewer
				Flags . SqlViewerIsLoading = false;
				UpdateDbSelectorItem ( s );
				//				Debug . WriteLine ( $" 3-2-END *** TRACE *** SQLDBVIEWER : BankGrid_SelectedCellsChanged BANKACCOUNT - Exiting\n" );

				//if ( this.BankGrid. Items . Count > 0 )
				//{
				//	if ( Flags . SqlViewerIndexIsChanging == false
				//		&& Flags . EditDbIndexIsChanging == false
				//		&& Flags.EditDbDataChanged == false)
				//	{
				//		Flags . EditDbIndexIsChanging = true;
				//		// WE are triggering index change
				//		EventControl . TriggerViewerIndexChanged ( this,
				//			new IndexChangedArgs
				//			{
				//				dGrid = this . BankGrid,
				//				Sender = "BANKACCOUNT",
				//				Row = this . BankGrid . SelectedIndex
				//			} );
				//	}
				//}
			}
		}

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
				//				ExtensionMethods . Refresh ( this );
			}
		}

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
				//				ExtensionMethods . Refresh ( this );
			}
		}

		#endregion grid row selection code

		#region CellEdit Checker functions

		private void BankGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
		//Get the BankAccount cell data and its Db Field name BEFORE
		// it has been changed and store in global variables
		{
			OrignalCellRow = e . Row . GetIndex ( );
			OriginalCellColumn = e . Column . DisplayIndex;
			DataGridColumn dgc = e . Column as DataGridColumn;
			string name = dgc . SortMemberPath;
			DataGridRow dgr = e . Row;
			//			BankAccountViewModel bvm = dgr.Item as BankAccountViewModel;
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
				//				DataGridRow dgr = DetailsGrid.SelectedIndex as DataGridRow;
				//				DataGridRowEditEndingEventArgs dga = new DataGridRowEditEndingEventArgs(, null);
				//				ViewerGrid_RowEditEnding ( null, dga);
			}
		}

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

		private void CustomerGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
		//Get the BankAccount cell data and its Db Field name BEFORE
		// it has been changed and store in global variables
		{
			OrignalCellRow = e . Row . GetIndex ( );
			OriginalCellColumn = e . Column . DisplayIndex;
			DataGridColumn dgc = e . Column as DataGridColumn;
			string name = dgc . SortMemberPath;
			DataGridRow dgr = e . Row;
			//			CustomerViewModel bvm = dgr.Item as CustomerViewModel;
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

		private void Window_MouseDown ( object sender, MouseButtonEventArgs e )
		{
			Window_GotFocus ( sender, e );
		}

		//*********************************************************************************************************//
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
		//*********************************************************************************************************//
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

		//*********************************************************************************************************//
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
				//				BankAccountViewModel bvm = bvm ();
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

		//*********************************************************************************************************//
		private void BankGrid_MouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{
			return;
		}

		private void ShowBank_KeyDown ( object sender, KeyEventArgs e )
		{
			if ( e . Key == Key . RightAlt )
			{
				Flags . ListGridviewControlFlags ( );
			}
		}

		#endregion Keyboard /Mousebutton handlers

		//*********************************************************************************************************//
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
				con . Close ( );
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

		//public void KeyboardFocusChangedEventHandler ( object sender, KeyboardFocusChangedEventArgs e )
		//{
		//	object o = sender;
		//	SqlDbViewer sql = o as SqlDbViewer;

		//}

		#region Tuple Handlers

		/// <returns>A fully populated Tuple</returns>
		//*********************************************************************************************************//
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
		//*********************************************************************************************************//
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

		private void BankGrid_GotFocus ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registreded" cos we can
			// Click the button before the window has had focus set
			Flags . CurrentSqlViewer = this;
			Flags . SqlBankGrid = sender as DataGrid;
			Flags . SetGridviewControlFlags ( this, this . BankGrid );
			DbSelector . SelectActiveViewer ( this );
		}

		private void CustomerGrid_GotFocus ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registreded" cos we can
			// Click the button before the window has had focus set
			Flags . CurrentSqlViewer = this;
			Flags . SqlCustGrid = sender as DataGrid;
			Flags . SetGridviewControlFlags ( this, this . CustomerGrid );
			DbSelector . SelectActiveViewer ( this );
		}

		private void DetailsGrid_GotFocus ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registreded" cos we can
			// Click the button before the window has had focus set
			Flags . CurrentSqlViewer = this;
			Flags . SqlDetGrid = sender as DataGrid;
			Flags . SetGridviewControlFlags ( this, this . DetailsGrid );
			DbSelector . SelectActiveViewer ( this );
		}

		#endregion Focus handling

		private static void HandleEdit ( object sender, EditEventArgs e )
		{
			//Handler for Datagrid Edit occurred delegate
			if ( Flags . EventHandlerDebug )
				Debug . WriteLine ( $"\r\nRecieved by SQLDBVIEWER (150) Caller={e . Caller}, Index = {e . CurrentIndex},  Grid = {e . ToString ( )}\r\n " );
		}

		#region Datagrid ROW UPDATING functionality

		public async Task UpdateDatabases ( string currentDb, DataGridRowEditEndingEventArgs e, DataGrid CallingGrid = null )
		{
			int currentrow = 0;
			BankAccountViewModel ss = bvm;
			CustomerViewModel cs = cvm;
			DetailsViewModel sa = dvm;

			SQLHandlers sqlh = new SQLHandlers ( );
			if ( e == null && CallingGrid == null )
				return;

			if ( !Flags . UpdateInProgress )
				return;

			if ( CallingGrid != null )
			{ currentrow = CallingGrid . SelectedIndex; }

			if ( currentDb == "BANKACCOUNT" )
			{
				if ( currentrow == 0 )
					currentrow = this . BankGrid . SelectedIndex;
				ss = new BankAccountViewModel ( );

				// This gets us the UPDATED data from the "Donor" data changed row
				ss = e . Row . Item as BankAccountViewModel;
				// set global flag to show we are in edit/Save mode
				BankAccountViewModel . SqlUpdating = true;
				//This updates  the Sql Db
				await sqlh . UpdateDbRow ( "BANKACCOUNT", ( object ) ss );

				// Notify Other viewers of the data change
				SendDataChanged ( this, BankGrid, CurrentDb );

				this . BankGrid . SelectedIndex = currentrow;
				Mouse . OverrideCursor = Cursors . Arrow;
				//e . Cancel = true;
				//this . Focus ( );
				//ViewerChangeType = 2;
				return;
			}
			else if ( currentDb == "CUSTOMER" )
			{
				if ( currentrow == 0 )
					currentrow = this . CustomerGrid . SelectedIndex;
				cs = new CustomerViewModel ( );
				cs = e . Row . Item as CustomerViewModel;
				// set global flag to show we are in edit/Save mode
				BankAccountViewModel . SqlUpdating = true;
				//This updates  the Sql Db
				await sqlh . UpdateDbRow ( "CUSTOMER", ( object ) cs );

				// Notify Other viewers of the data change
				SendDataChanged ( this, CustomerGrid, CurrentDb );

				this . CustomerGrid . SelectedIndex = currentrow;
				Mouse . OverrideCursor = Cursors . Arrow;
				//e . Cancel = true;
				//this . Focus ( );
				//ViewerChangeType = 2;
				return;
			}
			else if ( currentDb == "DETAILS" )
			{
				if ( currentrow == 0 )
					currentrow = this . DetailsGrid . SelectedIndex;
				sa = new DetailsViewModel ( );
				sa = e . Row . Item as DetailsViewModel;
				// set global flag to show we are in edit/Save mode
				BankAccountViewModel . SqlUpdating = true;
				//This updates  the Sql Db
				await sqlh . UpdateDbRow ( "DETAILS", ( object ) sa );
				Flags . SqlDataChanged = true;

				// Notify Other viewers of the data change
				SendDataChanged ( this, DetailsGrid, CurrentDb );

				Flags . SqlDataChanged = true;
				this . DetailsGrid . SelectedIndex = currentrow;
				Mouse . OverrideCursor = Cursors . Arrow;
				//e . Cancel = true;
				//this . Focus ( );
				//ViewerChangeType = 2;
				return;
			}
		}

		private void RefreshOtherviewersAfterUpdate ( DataGrid Grid )
		{
			//	Data has ben updated, so force other open viewers to refresh thier own girids
			SqlDbViewer thisViewer = null;
			if ( Flags . SqlBankGrid == Grid )
			{
				// This is the changes' ORIGINATING viewer
				thisViewer = this;
				if ( Flags . SqlCustGrid != null )
				{
					Flags . SqlCustViewer . Activate ( );
					this . CustomerGrid . ItemsSource = null;
					this . CustomerGrid . ItemsSource = SqlViewerCustcollection;
					ExtensionMethods . Refresh ( this . CustomerGrid );
					ExtensionMethods . Refresh ( Flags . SqlCustViewer );
				}
				if ( Flags . SqlDetGrid != null )
				{
					Flags . SqlDetViewer . Activate ( );
					this . DetailsGrid . ItemsSource = null;
					this . DetailsGrid . ItemsSource = SqlViewerDetcollection;
					ExtensionMethods . Refresh ( this . DetailsGrid );
					ExtensionMethods . Refresh ( Flags . SqlDetViewer );
				}
				//Activate our originating window again
				thisViewer . Activate ( );
			}
			if ( Flags . SqlCustGrid == Grid )
			{
				// This is the changes' ORIGINATING viewer
				thisViewer = this;
				if ( Flags . SqlBankGrid != null )
				{
					Flags . SqlBankViewer . Activate ( );
					this . BankGrid . ItemsSource = null; ;
					this . BankGrid . ItemsSource = SqlViewerBankcollection;
					ExtensionMethods . Refresh ( this . BankGrid );
					ExtensionMethods . Refresh ( Flags . SqlBankViewer );
				}
				if ( Flags . SqlDetGrid != null )
				{
					Flags . SqlDetViewer . Activate ( );
					this . DetailsGrid . ItemsSource = null;
					this . DetailsGrid . ItemsSource = SqlViewerDetcollection;
					ExtensionMethods . Refresh ( this . DetailsGrid );
					ExtensionMethods . Refresh ( Flags . SqlDetViewer );
				}
				//Activate our originating window again
				thisViewer . Activate ( );
			}
			if ( Flags . SqlDetGrid == Grid )
			{
				// This is the changes' ORIGINATING viewer
				thisViewer = this;
				if ( Flags . SqlBankGrid != null )
				{
					Flags . SqlBankViewer . Activate ( );
					this . BankGrid . ItemsSource = null;
					this . BankGrid . ItemsSource = SqlViewerBankcollection;
					ExtensionMethods . Refresh ( this . BankGrid );
					ExtensionMethods . Refresh ( Flags . SqlBankViewer );
				}
				if ( Flags . SqlCustGrid != null )
				{
					Flags . SqlCustViewer . Activate ( );
					this . CustomerGrid . ItemsSource = null;
					this . CustomerGrid . ItemsSource = SqlViewerCustcollection;
					ExtensionMethods . Refresh ( this . CustomerGrid );
					ExtensionMethods . Refresh ( Flags . SqlCustViewer );
				}
				//Activate our originating window again
				thisViewer . Activate ( );
			}
		}

		/// <summary>
		///  This is aMASSIVE Function that handles updating the Dbs via SQL plus sorting the current grid
		///  out & notifying all other viewers that a change has occurred so they can (& in fact do) update
		///  their own data grids rather nicely right now - 22/4/21
		/// </summary>
		/// <param name="sender">Unused</param>
		/// <param name="e">Unused</param>
		public void ViewerGrid_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
		{
			/// This ONLY gets called when a cell is edited in THIS viewer

			ViewerChangeType = 2;
			EditChangeType = 0;
			int currentrow = 0;
			BankAccountViewModel ss = bvm;
			CustomerViewModel cs = cvm;
			DetailsViewModel sa = dvm;
			//			DataGrid dg = null;
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
				// Save pointer to the viewer that is triggering the change event so we can get back to it
				//				Flags . SqlUpdateOriginatorViewer = this;
				//SendDbSelectorCommand ( 103, $"CHANGES made to Row data in {CurrentDb} Db) - Calling UpdateDatabases() for All Databases...", Flags . CurrentSqlViewer );
				SQLHandlers sqlh = new SQLHandlers ( );
				// These get the row with all the NEW data
				if ( CurrentDb == "BANKACCOUNT" )
				{
					int currow = 0;
					currow = this . BankGrid . SelectedIndex;
					// Save current row so we can reposition correctly at end of the entire refresh process
					//					Flags . SqlBankCurrentIndex = currow;
					ss = this . BankGrid . SelectedItem as BankAccountViewModel;
					// This is the NEW DATA from the current row
					sqlh . UpdateDbRowAsync ( CurrentDb, ss, this . BankGrid . SelectedIndex );

					Debug . WriteLine ( $" 7-1 *** TRACE *** SQLDBVIEWER : ViewerGrid_rowEditEnding - Sending BANKACCOUNT TriggerViewerDataUpdated" );

					SendDataChanged ( this, this . BankGrid, "BANKACCOUNT" );

					//EventControl . TriggerViewerDataUpdated ( SqlViewerBankcollection,
					//new LoadedEventArgs
					//{
					//	CallerDb = "BANKACCOUNT",
					//	DataSource = SqlViewerBankcollection,
					//	RowCount = this . BankGrid . SelectedIndex
					//} );

					if ( Flags . CurrentEditDbViewer != null )
						Flags . CurrentEditDbViewer . UpdateGrid ( "BANKACCOUNT" );
					this . BankGrid . SelectedIndex = currow;
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					int currow = 0;
					currow = this . CustomerGrid . SelectedIndex;
					// Save current row so we can reposition correctly at end of the entire refresh process
					//					Flags . SqlCustCurrentIndex = currow;
					cs = this . CustomerGrid . SelectedItem as CustomerViewModel;
					// This is the NEW DATA from the current row
					sqlh . UpdateDbRowAsync ( CurrentDb, cs, this . CustomerGrid . SelectedIndex );

					Debug . WriteLine ( $" 7-2 *** TRACE *** SQLDBVIEWER : ViewerGrid_rowEditEnding - Sending CUSTOMER TriggerViewerDataUpdated" );

					SendDataChanged ( this, this . CustomerGrid, "CUSTOMER" );
					//EventControl . TriggerViewerDataUpdated ( SqlViewerCustcollection,
					//new LoadedEventArgs
					//{
					//	CallerDb = "CUSTOMER",
					//	DataSource = SqlViewerCustcollection,
					//	RowCount = this . CustomerGrid . SelectedIndex
					//} );

					this . CustomerGrid . SelectedIndex = currow;
					Utils . ScrollRecordInGrid ( this . CustomerGrid, currow );
					// Notify EditDb to upgrade its grid
					if ( Flags . CurrentEditDbViewer != null )
						Flags . CurrentEditDbViewer . UpdateGrid ( "CUSTOMER" );
				}
				else if ( CurrentDb == "DETAILS" )
				{
					int currow = 0;
					currow = this . DetailsGrid . SelectedIndex;
					// Save current row so we can reposition correctly at end of the entire refresh process
					//					Flags . SqlDetCurrentIndex = currow;
					sa = this . DetailsGrid . SelectedItem as DetailsViewModel;
					// sa contains the NEW DATA from the current row
					// Update Db itself via SQL
					sqlh . UpdateDbRowAsync ( CurrentDb, sa, currow );

					Debug . WriteLine ( $" 7-3 *** TRACE *** SQLDBVIEWER : ViewerGrid_rowEditEnding - Sending DETAILS TriggerViewerDataUpdated" );

					SendDataChanged ( this, this . DetailsGrid, "DETAILS" );
					//EventControl . TriggerViewerDataUpdated ( SqlViewerDetcollection,
					//new LoadedEventArgs
					//{
					//	CallerDb = "DETAILS",
					//	DataSource = SqlViewerDetcollection,
					//	RowCount = this . DetailsGrid . SelectedIndex
					//} );

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
							//						sa = sacc;
							//						sa = e.Row.Item as DetailsViewModel;
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
						//	cs = this . CustomerGrid . SelectedItem as CustomerViewModel;
						//else if ( e == null && CurrentDb == "CUSTOMER" )
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

		public void ParseButtonText ( bool obj )
		{
			ShowBank . Content = $" Bank A/c's  (0)";
			ShowCust . Content = $"Customer A/c's  (0)";
			ShowDetails . Content = $"Details A/c's  (0)";
			//			Flags.Flags.IsMultiMode
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
						//	BankAccountViewModel record = (BankAccountViewModel)datarecord;
						var record = datarecord as BankAccountViewModel;//CurrentBankSelectedRecord;
												//						MainWindow . gv . CurrentDb [ x ] = $"Bank - A/c # {record?.BankNo}, Cust # {record?.CustNo}, Balance £ {record?.Balance}, Interest {record?.IntRate}%";
						PrettyDetails = $"Bank - A/c # {record?.BankNo}, Cust # {record?.CustNo}, Balance £ {record?.Balance}, Interest {record?.IntRate}%";
						//PrettyDetails = MainWindow . gv . CurrentDb [ x ];
						MainWindow . gv . PrettyDetails = PrettyDetails;
						Updated = true;
						//Update list in DbSelector
						UpdateDbSelectorItem ( PrettyDetails );
					}
					else if ( caller == "CustomerGrid" )

					{
						var record = datarecord as CustomerViewModel;
						//						MainWindow . gv . CurrentDb [ x ] = $"Customer - Customer # {record?.CustNo}, Bank # {record?.BankNo}, {record?.LName} {record?.Town}, {record?.County}";
						PrettyDetails = $"Customer - Customer # {record?.CustNo}, Bank # {record?.BankNo}, {record?.LName} {record?.Town}, {record?.County}";
						//PrettyDetails = MainWindow . gv . CurrentDb [ x ];
						MainWindow . gv . PrettyDetails = PrettyDetails;
						Updated = true;
						//Update list in DbSelector
#pragma NOT NEEDED????
						UpdateDbSelectorItem ( PrettyDetails );
					}
					else if ( caller == "DetailsGrid" )
					{
						var record = datarecord as DetailsViewModel;
						//						MainWindow . gv . CurrentDb [ x ] = $"Details - Bank A/C # {record?.BankNo}, Cust # {record?.CustNo}, Balance {record?.Balance}, Interest % {record?.IntRate}";
						PrettyDetails = $"Details - Bank A/C # {record?.BankNo}, Cust # {record?.CustNo}, Balance {record?.Balance}, Interest % {record?.IntRate}";
						//PrettyDetails = MainWindow . gv . CurrentDb [ x ];
						MainWindow . gv . PrettyDetails = PrettyDetails;
						Updated = true;
						//Update list in DbSelector
						UpdateDbSelectorItem ( PrettyDetails );
					}
					//					ExtensionMethods . Refresh ( Flags . DbSelectorOpen . ViewersList );
					break;
				}
			}
			if ( !Updated )
			{
				// UNUSED
				{
					//				Console.WriteLine ("\nUpdateRowDetails has NOT up[dated anything ???\n");
					//				if (Flags.SqlViewerIsLoading)
					//					return;
					//				// Seems there are no entries right now, so we are starting up
					//				int x = MainWindow.gv.ViewerCount;
					//				{
					//					MainWindow.gv.CurrentDb[x] = caller;
					//					MainWindow.gv.SqlViewerWindow = this;
					//					if (caller == "BankGrid")
					//					{
					//						MainWindow.gv.window[x] = this;
					//						MainWindow.gv.Bankviewer = MainWindow.gv.ListBoxId[x] = (Guid)this.Tag;
					//						MainWindow.gv.ViewerCount++;
					////						MainWindow.gv.Bankviewer = (Guid)this.Tag;
					//						MainWindow.gv.CurrentDb[x] = MainWindow.gv.PrettyDetails;
					//						MainWindow.gv.Datagrid[x] = BankGrid;

					//					}

					//					else if (caller == "CustomerGrid")
					//					{
					//						MainWindow.gv.window[x] = this;
					//						MainWindow.gv.ListBoxId[x] = (Guid)this.Tag;
					//						MainWindow.gv.ViewerCount++;
					//						MainWindow.gv.Custviewer = (Guid)this.Tag;
					//						MainWindow.gv.CurrentDb[x] = MainWindow.gv.PrettyDetails;
					//						MainWindow.gv.Datagrid[x] = CustomerGrid;
					//					}
					//					else if (caller == "DetailsGrid")
					//					{
					//						MainWindow.gv.window[x] = this;
					//						MainWindow.gv.ListBoxId[x] = (Guid)this.Tag;
					//						MainWindow.gv.ViewerCount++;
					//						MainWindow.gv.Detviewer = (Guid)this.Tag;
					//						MainWindow.gv.CurrentDb[x] = MainWindow.gv.PrettyDetails;
					//						MainWindow.gv.Datagrid[x] = DetailsGrid;
					//					}
					//				}
					//				Guid g = (Guid)Flags.CurrentSqlViewer.Tag;
				}
			}
		}

		public void CloseViewer_Click ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registered" cos we can
			// Click the button before the window has been fully closed
			Flags . CurrentSqlViewer = this;

			//Close current (THIS) Viewer Window
			//clear flags in GV[] & Flags Structures
			if ( CurrentDb == "BANKACCOUNT" )
			{
				// Clears Flags and the relevant Gv[] entry

#pragma TODO  Method needs work to handle 99 = Closing down actions needed
				RemoveFromViewerList ( 99 );
				Flags . SqlBankGrid = null;
				Flags . SqlBankViewer = null;
				//				Flags . SqlBankCurrentIndex = 0;
				MainWindow . gv . Bankviewer = Guid . Empty;
				Flags . ActiveSqlViewer = null;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				// Clears Flags and the relevant Gv[] entry
				RemoveFromViewerList ( 99 );
				Flags . SqlCustGrid = null;
				Flags . SqlCustViewer = null;
				//				Flags . SqlCustCurrentIndex = 0;
				MainWindow . gv . Custviewer = Guid . Empty;
				Flags . ActiveSqlViewer = null;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				// Clears Flags and the relevant Gv[] entry
				RemoveFromViewerList ( 99 );
				Flags . SqlDetGrid = null;
				Flags . SqlDetViewer = null;
				//				Flags . SqlDetCurrentIndex = 0;
				MainWindow . gv . Detviewer = Guid . Empty;
				Flags . ActiveSqlViewer = null;
			}
			//SendDbSelectorCommand ( 99, "Window is closing", Flags . CurrentSqlViewer );
			UpdateDbSelectorBtns ( Flags . CurrentSqlViewer );
			//			BankAccountViewModel . EditdbWndBank = null;
			Mouse . OverrideCursor = Cursors . Arrow;
			// Must null out the global pointers to any Sql Viewer window
			//Each other still open viewer window will reset it on Focus()
			Flags . ActiveSqlGrid = null;
			Flags . CurrentSqlViewer = null;
			Close ( );
		}

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
				Flags . SqlBankCurrentIndex = 0;
				MainWindow . gv . Bankviewer = Guid . Empty;
				Flags . ActiveSqlViewer = null;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				Flags . SqlCustGrid = null;
				Flags . SqlCustViewer = null;
				//				Flags . SqlCustCurrentIndex = 0;
				MainWindow . gv . Custviewer = Guid . Empty;
				Flags . ActiveSqlViewer = null;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				Flags . SqlDetGrid = null;
				Flags . SqlDetViewer = null;
				//				Flags . SqlDetCurrentIndex = 0;
				MainWindow . gv . Detviewer = Guid . Empty;
				Flags . ActiveSqlViewer = null;
			}
			//SendDbSelectorCommand ( 99, "Window is closing", Flags . CurrentSqlViewer );
			// THIS WORKED   19/4/21
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
						//					int currentindex = MainWindow.gv.DbSelectorWindow.ViewersList.SelectedIndex;
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

		private void CustomerGrid_TargetUpdated ( object sender, DataTransferEventArgs e )
		{
			// row data Loading ???
			MainWindow . gv . Datagrid [ LoadIndex ] = this . CustomerGrid;
			//			this . CustomerGrid . SelectedIndex = 0;
			//			SelectedRow = 0;
		}

		private void Window_GotFocus ( object sender, RoutedEventArgs e )
		{
			// Actually, this is Called mostly by MouseDown Handler
			//when Focus has been set to this window
			//Set global flags
			//			if ( Flags . CurrentSqlViewer == this ) return;

			Flags . CurrentSqlViewer = this;

			//Gotta osrt out the current Db as it has now chnaged
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
		private void Minimize_click ( object sender, RoutedEventArgs e )
		{
			Window_GotFocus ( sender, null );
			//Window_MouseDown ( sender, null );
			this . WindowState = WindowState . Normal;
		}

		public void CreateListboxItemBinding ( )
		{
			Binding binding = new Binding ( "ListBoxItemText" );
			binding . Source = PrettyDetails;
		}

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
		public static void UpdateDbSelectorItem ( string data )
		{
			if ( Flags . CurrentSqlViewer == null ) return;
			//			bool IsAdded = false;
			if ( Flags . CurrentSqlViewer . Tag is null ) return;
			// handle global flag to control viewer addition/updating
			if ( Flags . SqlViewerIsLoading ) return;
			//			else
			//				Flags.SqlViewerIsLoading = true;

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
				//if (!IsAdded)
				//{
				//	Flags.SqlViewerIsLoading = false;
				//	// Send Notification to DbSelector to Add/Update the ViewersList
				//	// clear flag cos we are calling iteratively via DbSelector 101 notification call below;
				//	//SendDbSelectorCommand (101, data, Flags.CurrentSqlViewer);
				//}
			}
			else
			{
#pragma NOT NEEDED ???
				// Send Notification to DbSelector to Add/Update the ViewersList
				// clear flag cos we are calling iteratively via DbSelector 101 notification call below;
				// add a new entry here !!!  101 does nothing but report it is received
				//				//SendDbSelectorCommand (101, data, this);
			}
		}

		//*********************************************************************************************************//
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
					Flags . BankEditDb . Focus ( ); Flags . BankEditDb . BringIntoView ( );
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
				//				ViewEdit = new ViewEditdb ( "BANKACCOUNT", this . BankGrid . SelectedIndex, this . BankGrid . SelectedItem, this );
				edb = new EditDb ( "BANKACCOUNT", this . BankGrid . SelectedIndex, this . BankGrid . CurrentItem, this );
				//				BankAccountViewModel . EditdbWndBank = edb;
				edb . Owner = this;
				edb . Show ( );
				//				ExtensionMethods . Refresh ( edb );
				//				EditDataGrid = edb . DataGrid1;
				Flags . BankEditDb = edb;
				RefreshBtn . IsEnabled = false;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				if ( Flags . CustEditDb != null ) { Flags . CustEditDb . Focus ( ); Flags . CustEditDb . BringIntoView ( ); return; }
				//					ViewEdit = new ViewEditdb ( "BANKACCOUNT", this . BankGrid . SelectedIndex, this . BankGrid . SelectedItem, this );
				edb = new EditDb ( "CUSTOMER", this . CustomerGrid . SelectedIndex, this . CustomerGrid . SelectedItem, this );
				//				BankAccountViewModel . EditdbWndBank = edb;
				edb . Owner = this;
				edb . Show ( );
				ExtensionMethods . Refresh ( edb );
				//				EditDataGrid = edb . DataGrid2;
				Flags . CustEditDb = edb;
				RefreshBtn . IsEnabled = false;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				if ( Flags . DetEditDb != null ) { Flags . DetEditDb . Focus ( ); Flags . DetEditDb . BringIntoView ( ); return; }
				//					ViewEdit = new ViewEditdb ( "BANKACCOUNT", this . BankGrid . SelectedIndex, this . BankGrid . SelectedItem, this );
				edb = new EditDb ( "DETAILS", this . DetailsGrid . SelectedIndex, this . DetailsGrid . SelectedItem, this );
				//				BankAccountViewModel . EditdbWndBank = edb;
				edb . Owner = this;
				edb . Show ( );
				ExtensionMethods . Refresh ( edb );
				//				EditDataGrid = edb . DataGrid1;
				Flags . DetEditDb = edb;
				RefreshBtn . IsEnabled = false;
			}
			//EditViewerOpen = true;
		}

		//*********************************************************************************************************//
		private void DoDragMove ( )
		{
			//Handle the button NOT being the left mouse button
			// which will crash the DragMove Fn.....     cos it has to be the primary button !!!
			try
			{ DragMove ( ); }
			catch ( Exception ex )
			{ Debug . WriteLine ( $"General Exception : {ex . Message}, {ex . Data}" ); return; }
		}

		private void TextBlock_RequestBringIntoView ( object sender, RequestBringIntoViewEventArgs e )
		{
			//			this.Show ();
			//			this.BringIntoView ();
		}

		//*******************************************************************************************************//
		private void ItemsView_OnSelectionChanged ( object sender, SelectionChangedEventArgs e )
		//User has clicked a row in our DataGrid// OR in EditDb grid
		{
			e . Handled = true;

			/* SelectedIndex = -1 on entry here ???????????????? */
			//if ( sender == this ) return;

			// Dont do this if we are updating due to changed in EditDb
			//if ( Flags . EditDbDataChanged )
			//	return;

			// dont do this if we are loading / reloading database
			//			if ( Flags . DataLoadIngInProgress ) return;
			//declare vars to hold item data from relevant Classes
			var datagrid = sender as DataGrid;
			if ( datagrid == null ) return;

			// do NOT go thru here if we are loading initial data
			//if ( Flags . DataLoadIngInProgress
			//	|| Flags . UpdateInProgress || RefreshInProgress )
			//	return;

			//Get the NEW selected index
			int index = 0;

			// ?? sender.Selected~Index has CORRECT VALUE 22/5/21

			// Crucial flag for updating - Set in EditDb, so here we do nothing
			//		if ( Flags . DataLoadIngInProgress ) return;

			//ENTRY POINT WHEN WE CHANGE THE INDEX	 Or change data, or when ItemsSource is set as well  it seems
			// It is different processing if an EditDb window is open !!
			if ( Flags . CurrentEditDbViewer != null )
			{
				//				if ( Flags . EditDbIndexIsChanging == false )
				//					return;
				//There is an EditDb window open, so this will trigger
				//an event that lets the DataGrid in the EditDb class
				// change it's own index internally
				if ( CurrentDb == "BANKACCOUNT" )
				{
					//					Debug . WriteLine ( $" 4-1 *** TRACE *** SQLDBVIEWER : Itemsview_OnSelectionChanged  BANKACCOUNT - Entering" );
					e . Handled = true;
					//					Flags . SqlBankCurrentIndex = this . BankGrid . SelectedIndex;
					index = this . BankGrid . SelectedIndex;
					if ( index == -1 ) return;
					this . BankGrid . CurrentItem = index;
					// Updates  the MainWindow.gv[] structure
					UpdateRowDetails ( this . BankGrid . CurrentItem, "BankGrid" );
					Utils . ScrollRecordInGrid ( this . BankGrid, index );

					Flags . SqlViewerIndexIsChanging = true;
					if ( sender != this . BankGrid && Flags . EditDbIndexIsChanging == false && Flags . EditDbDataChange == false )
					{
						Debug . WriteLine ( $" 1-1 *** TRACE *** SQLDBVIEWER : Itemsview_OnSelectionChanged  BANKACCOUNT - Sending TriggerViewerIndexChanged Event trigger" );
						EventControl . TriggerViewerIndexChanged ( this,
							new IndexChangedArgs
							{
								dGrid = this . BankGrid,
								Sender = "BANKACCOUNT",
								Row = this . BankGrid . SelectedIndex
							} );
					}
					Flags . SqlViewerIndexIsChanging = false;
					//					Debug . WriteLine ( $" 4-3-END *** TRACE *** SQLDBVIEWER : Itemsview_OnSelectionChanged - Exiting BANKACCOUNT : Handled = true\n" );
					e . Handled = true;
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					//					Flags . SqlCustCurrentIndex = this . CustomerGrid . SelectedIndex;
					index = this . CustomerGrid . SelectedIndex;
					// Updates  the MainWindow.gv[] structure
					UpdateRowDetails ( this . CustomerGrid . CurrentItem, "CustomerGrid" );
					Utils . ScrollRecordIntoView ( CustomerGrid, index );
					Flags . SqlViewerIndexIsChanging = true;
					if ( Flags . EditDbIndexIsChanging == false && Flags . EditDbDataChange == false )
					{
						Debug . WriteLine ( $" 1-2 *** TRACE *** SQLDBVIEWER : Itemsview_OnSelectionChanged  CUSTOMER- Sending TriggerViewerIndexChanged Event trigger" );
						EventControl . TriggerViewerIndexChanged ( this,
						new IndexChangedArgs
						{
							dGrid = this . CustomerGrid,
							Sender = "CUSTOMER",
							Row = this . CustomerGrid . SelectedIndex
						} );
					}
					Flags . SqlViewerIndexIsChanging = false;
				}
				else if ( CurrentDb == "DETAILS" )
				{
					if ( RefreshInProgress ) return;
					//					Flags . SqlDetCurrentIndex = this . DetailsGrid . SelectedIndex;
					index = this . DetailsGrid . SelectedIndex;
					//// Updates  the MainWindow.gv[] structure
					UpdateRowDetails ( this . DetailsGrid . CurrentItem, "DetailsGrid" );
					Utils . ScrollRecordIntoView ( DetailsGrid, index );
					Flags . SqlViewerIndexIsChanging = true;
					// dont trigger this if WE are updating due to EditDb index change or EditDb data change
					if ( Flags . EditDbIndexIsChanging == false && Flags . EditDbDataChange == false )
					{
						Debug . WriteLine ( $" 1-3 *** TRACE *** SQLDBVIEWER : Itemsview_OnSelectionChanged  DETAILS- Sending TriggerViewerIndexChanged Event trigger" );
						EventControl . TriggerViewerIndexChanged ( this,
							new IndexChangedArgs
							{
								dGrid = this . DetailsGrid,
								Sender = "DETAILS",
								Row = this . DetailsGrid . SelectedIndex
							} );
					}

					Flags . SqlViewerIndexIsChanging = false;
				}
			}
			else
			{
				//called when EditDb is NOT OPEN
				CustomerViewModel custacct = null;
				BankAccountViewModel bankacct = null;
				DetailsViewModel detsacct = null;
				int CurrentId = 0;

				if ( datagrid . Name == "CustomerGrid" )
				{
					if ( this . CustomerGrid . SelectedIndex == -1 ) return;
					//					Flags . SqlCustCurrentIndex = this . CustomerGrid . SelectedIndex;
					if ( this . CustomerGrid . CurrentItem != null )
					{
						custacct = CustomerGrid?.CurrentItem as CustomerViewModel;
						if ( custacct != null )
							CurrentId = custacct . Id;
					}
				}
				else if ( datagrid . Name == "BankAccountGrid" || datagrid . Name == "BankGrid" )
				{
					//					if ( this . BankGrid . SelectedIndex != -1 )
					//						Flags . SqlBankCurrentIndex = this . BankGrid . SelectedIndex;
					if ( this . BankGrid . CurrentItem != null )
					{
						//Get copy of entire BankAccvount record
						bankacct = this . BankGrid?.SelectedItem as BankAccountViewModel;
						if ( bankacct != null )
							CurrentId = bankacct . Id;
					}
				}
				else if ( datagrid . Name == "DetailsGrid" )
				{
					//					if ( this . DetailsGrid . SelectedIndex != -1 )
					//						Flags . SqlDetCurrentIndex = this . DetailsGrid . SelectedIndex;
					//					{
					if ( DetailsGrid?.SelectedItem != null )
					{
						detsacct = DetailsGrid?.SelectedItem as DetailsViewModel;
						if ( detsacct != null )
							CurrentId = detsacct . Id;
					}
					//					}
				}

				if ( CurrentDb == "BANKACCOUNT" )
				{
					if ( this . BankGrid . CurrentItem != null )
					{
						//SetTopViewRow ( BankGrid );
						//SetBottomViewRow ( BankGrid );
						//SetViewPort ( BankGrid );
						//Flags . PrintSundryVariables ( "BANKACCOUNT : ItemsView_OnSelectionChanged()" );

						//Get the NEW selected index
						index = ( int ) this . BankGrid . SelectedIndex;
						if ( index == -1 ) return;
						CurrentId = index;
						CurrentBankSelectedRecord = this . BankGrid . CurrentItem as BankAccountViewModel;
						// Fills/Updates the MainWindow.gv[] array
						Flags . SqlViewerIndexIsChanging = true;
						// Updates  the MainWindow.gv[] structure		&& ViewersList entry
						if ( Flags . UpdateInProgress == false )
							UpdateRowDetails ( this . BankGrid . CurrentItem, "BankGrid" );
						//BankGrid . SelectedIndex = index;
						//Utils . ScrollRecordIntoView ( BankGrid );
						//this . Activate ( );
						Flags . SqlViewerIndexIsChanging = false;
						if ( Flags . LinkviewerRecords && Triggered == false )
						{
							Debug . WriteLine ( $" 2-1 *** TRACE *** SQLDBVIEWER : Itemsview_OnSelectionChanged  BANKACCOUNT - Sending TriggerViewerIndexChanged Event trigger" );
							EventControl . TriggerViewerIndexChanged ( this,
								new IndexChangedArgs
								{
									dGrid = this . BankGrid,
									Sender = "BANKACCOUNT",
									Row = this . BankGrid . SelectedIndex
								} );
						}
					}
					Triggered = false;
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					if ( this . CustomerGrid . CurrentItem != null )
					{
						//SetTopViewRow ( CustomerGrid );
						//SetBottomViewRow ( CustomerGrid );
						//SetViewPort ( CustomerGrid );
						//Flags . PrintSundryVariables ( "CUSTOMER : ItemsView_OnSelectionChanged()" );

						index = ( int ) this . CustomerGrid . SelectedIndex;
						if ( index == -1 ) return;
						CurrentId = index;
						CurrentCustomerSelectedRecord = this . CustomerGrid . CurrentItem as CustomerViewModel;
						Flags . SqlViewerIndexIsChanging = true;
						// Updates  the MainWindow.gv[] structure
						if ( Flags . UpdateInProgress == false )
							UpdateRowDetails ( this . CustomerGrid . CurrentItem, "CustomerGrid" );
						//this . CustomerGrid . SelectedIndex = index;
						//this . CustomerGrid . SelectedItem = index;
						//Utils . ScrollRecordIntoView ( CustomerGrid );
						//this . Activate ( );
						Flags . SqlViewerIndexIsChanging = false;
//						if ( Flags . LinkviewerRecords )
						if ( Flags . LinkviewerRecords && Triggered == false )
							{
								Debug . WriteLine ( $" 2-2 *** TRACE *** SQLDBVIEWER : Itemsview_OnSelectionChanged  CUSTOMER- Sending TriggerViewerIndexChanged Event trigger" );
							EventControl . TriggerViewerIndexChanged ( this,
								new IndexChangedArgs
								{
									dGrid = this . CustomerGrid,
									Sender = "CUSTOMER",
									Row = this . CustomerGrid . SelectedIndex
								} );
						}
					}
					Triggered = false;
				}
				else if ( CurrentDb == "DETAILS" )
				{
					if ( this . DetailsGrid . CurrentItem != null )
					{
						//SetTopViewRow ( DetailsGrid );
						//SetBottomViewRow ( DetailsGrid );
						//SetViewPort ( DetailsGrid );
						//						Flags . PrintSundryVariables ( "DETAILS : ItemsView_OnSelectionChanged()" );

						index = ( int ) this . DetailsGrid . SelectedIndex;
						if ( index == -1 ) return;
						CurrentId = index;
						CurrentDetailsSelectedRecord = this . DetailsGrid . CurrentItem as DetailsViewModel;
						// This creates a new entry in gv[] if this is a new window being loaded
						Flags . SqlViewerIndexIsChanging = true;
						// Updates  the MainWindow.gv[] structure
						if ( Flags . UpdateInProgress == false )
							UpdateRowDetails ( this . DetailsGrid . CurrentItem, "DetailsGrid" );
						//						SelectCurrentRowByIndex ( DetailsGrid, DetailsGrid . SelectedIndex );
						//DetailsGrid . SelectedIndex = index;
						//DetailsGrid . SelectedItem = index;
						//Utils . ScrollRecordIntoView ( DetailsGrid );
						//this . Activate ( );
						Flags . SqlViewerIndexIsChanging = false;
						if ( Flags . LinkviewerRecords && Triggered == false )
						{
							Debug . WriteLine ( $" 2-3 *** TRACE *** SQLDBVIEWER : Itemsview_OnSelectionChanged  DETAILS - Sending TriggerViewerIndexChanged Event trigger" );
							EventControl . TriggerViewerIndexChanged ( this,
								new IndexChangedArgs
								{
									dGrid = this . DetailsGrid,
									Sender = "DETAILS",
									Row = this . DetailsGrid . SelectedIndex
								} );
						}
					}
					Triggered = false;
				}
			}
			UpdateAuxilliaries ( "" );
			//			DbEditHasChangedData = true;
			Mouse . OverrideCursor = Cursors . Arrow;
			EditChangeType = 0;
			ViewerChangeType = 0;
			// EXIT POINT WHEN VIEWER HAS CHANGED INDEX SELECTION
			//this . Activate ( );
		}

		public static void SelectCurrentRowByIndex ( DataGrid dataGrid, int rowIndex )
		{
			DataGridRow row = dataGrid . ItemContainerGenerator . ContainerFromIndex ( rowIndex ) as DataGridRow;
			if ( row != null )
			{
				Debug . WriteLine ( $"row.Focus failed" );
				int y = 0;
				//DataGridCell cell = GetCell ( dataGrid, row, 0 );
				//if ( cell != null )
				//	cell . Focus ( );
			}
		}

		//*******************************************************************************************************//
		public void UpdateAuxilliaries ( string comment )
		{
			//Application.Current.Dispatcher.Invoke (() =>
			ParseButtonText ( true );
			//			IsFiltered = "";
			ResetOptionButtons ( 1 );
			UpdateDbSelectorBtns ( Flags . CurrentSqlViewer );
			// Update DbSelector ListBoxItems structure and our GridViewer control Structure
			if ( IsViewerLoaded == false )
				UpdateGridviewController ( CurrentDb );
			// Reset ucilliary Buttons
			ResetauxilliaryButtons ( );
			if ( true == false )
			{
				//if ( CurrentDb == "BANKACCOUNT" )
				//{
				//	if ( Bankcollection == null ) return;
				//	if ( this . BankGrid . ItemsSource == null )
				//		this . BankGrid . ItemsSource = CollectionViewSource . GetDefaultView ( Bankcollection );
				//	//  paint the grid onscreen
				//	this . BankGrid . SelectionUnit = DataGridSelectionUnit . FullRow;
				//}
				//if ( CurrentDb == "CUSTOMER" )
				//{
				//	if ( Custcollection == null ) return;
				//	this . CustomerGrid . ItemsSource = CollectionViewSource . GetDefaultView ( Custcollection );
				//	//  paint the grid onscreen
				//	this . CustomerGrid . SelectionUnit = DataGridSelectionUnit . FullRow;
				//}
				//if ( CurrentDb == "DETAILS" )
				//{
				//	if ( SqlViewerDetcollection == null ) return;
				//	this . DetailsGrid . ItemsSource = CollectionViewSource . GetDefaultView ( Detcollection );
				//	//  paint the grid onscreen
				//	this . DetailsGrid . SelectionUnit = DataGridSelectionUnit . FullRow;
				//}
			}
#pragma NOT NEEDED ????
			//			UpdateViewersList ();
			//			StatusBar . Text = comment;
			//WaitMessage . Visibility = Visibility . Collapsed;
		}

		//*******************************************************************************************************//

		#region CheckBoxhandlers

		private void Id_option_Click ( object sender, RoutedEventArgs e )
		{
			Custno_option . IsChecked = false;
			actype_option . IsChecked = false;
			Dob_option . IsChecked = false;
			Bankno_option . IsChecked = false;
			Odate_option . IsChecked = false;
			Cdate_option . IsChecked = false;
			Default_option . IsChecked = false;
			Flags . SortOrderRequested = ( int ) Flags . SortOrderEnum . ID;
			GetData ( CurrentDb );
			StatusBar . Text = "Data reloaded in Id sort order....";
		}

		private void Bankno_option_Click ( object sender, RoutedEventArgs e )
		{
			Id_option . IsChecked = false;
			Custno_option . IsChecked = false;
			actype_option . IsChecked = false;
			Dob_option . IsChecked = false;
			Odate_option . IsChecked = false;
			Cdate_option . IsChecked = false;
			Default_option . IsChecked = false;
			Flags . SortOrderRequested = ( int ) Flags . SortOrderEnum . BANKNO;
			GetData ( CurrentDb );
			StatusBar . Text = "Data reloaded in Bank A/c # sort order....";
		}

		private void Custno_option_Click ( object sender, RoutedEventArgs e )
		{
			Id_option . IsChecked = false;
			actype_option . IsChecked = false;
			Dob_option . IsChecked = false;
			Bankno_option . IsChecked = false;
			Odate_option . IsChecked = false;
			Cdate_option . IsChecked = false;
			Default_option . IsChecked = false;
			Flags . SortOrderRequested = ( int ) Flags . SortOrderEnum . CUSTNO;
			GetData ( CurrentDb );
			StatusBar . Text = "Data reloaded in Customer # sort order....";
		}

		private void actype_option_Click ( object sender, RoutedEventArgs e )
		{
			Id_option . IsChecked = false;
			Custno_option . IsChecked = false;
			Dob_option . IsChecked = false;
			Bankno_option . IsChecked = false;
			Odate_option . IsChecked = false;
			Cdate_option . IsChecked = false;
			Default_option . IsChecked = false;
			Flags . SortOrderRequested = ( int ) Flags . SortOrderEnum . ACTYPE;
			GetData ( CurrentDb );
			StatusBar . Text = "Data reloaded in A/c Type sort order....";
		}

		private void Dob_option_Click ( object sender, RoutedEventArgs e )
		{
			Id_option . IsChecked = false;
			Custno_option . IsChecked = false;
			actype_option . IsChecked = false;
			Bankno_option . IsChecked = false;
			Odate_option . IsChecked = false;
			Cdate_option . IsChecked = false;
			Default_option . IsChecked = false;
			Flags . SortOrderRequested = ( int ) Flags . SortOrderEnum . DOB;
			GetData ( CurrentDb );
			StatusBar . Text = "Data reloaded in Birthday sort order....";
		}

		private void Odate_option_Click ( object sender, RoutedEventArgs e )
		{
			Id_option . IsChecked = false;
			Custno_option . IsChecked = false;
			actype_option . IsChecked = false;
			Dob_option . IsChecked = false;
			Bankno_option . IsChecked = false;
			Cdate_option . IsChecked = false;
			Default_option . IsChecked = false;
			Flags . SortOrderRequested = ( int ) Flags . SortOrderEnum . ODATE;
			GetData ( CurrentDb );
			StatusBar . Text = "Data reloaded in Date Opened sort order....";
		}

		private void Cdate_option_Click ( object sender, RoutedEventArgs e )
		{
			Id_option . IsChecked = false;
			Custno_option . IsChecked = false;
			actype_option . IsChecked = false;
			Dob_option . IsChecked = false;
			Bankno_option . IsChecked = false;
			Odate_option . IsChecked = false;
			Default_option . IsChecked = false;
			Flags . SortOrderRequested = ( int ) Flags . SortOrderEnum . CDATE;
			GetData ( CurrentDb );
			StatusBar . Text = "Data reloaded in Date Closed sort order....";
		}

		private void Default_option_Click ( object sender, RoutedEventArgs e )
		{
			Id_option . IsChecked = false;
			Custno_option . IsChecked = false;
			actype_option . IsChecked = false;
			Dob_option . IsChecked = false;
			Bankno_option . IsChecked = false;
			Odate_option . IsChecked = false;
			Cdate_option . IsChecked = false;
			Flags . SortOrderRequested = ( int ) Flags . SortOrderEnum . DEFAULT;
			GetData ( CurrentDb );
			StatusBar . Text = "Data reloaded in Bank A/c #  within Customer # sort order....";
		}

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

		//public static SqlDbViewer GetSqlInstance ( )
		//{
		//	if ( _Instance == null )
		//		_Instance = new SqlDbViewer ( );
		//	return _Instance;
		//}

		//*****************************************************//

		#endregion GetSqlInstance - Fn to allow me to call standard merthods from inside a Static method

		//*********************************************************************************************************//

		#region Filtering code

		//*****************************************************************************************//
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
				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				Filters . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
				Filters . Background = br;
				Filters . Content = "Reset";
				//				Tidy up general flags
				//				Flags . IsFiltered = false;
				//				Flags . FilterCommand = "";
				//ParseButtonText ( true );
			}
			else
			{
				Flags . IsFiltered = true;
				if ( CurrentDb == "BANKACCOUNT" )
					ShowBank_Click ( null, null );
				else if ( CurrentDb == "CUSTOMER" )
					ShowCust_Click ( null, null );
				else if ( CurrentDb == "DETAILS" )
					ShowDetails_Click ( null, null );
				Filters . IsEnabled = false;
				Filters . IsEnabled = true;
				ControlTemplate ctmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGreen" );
				Filters . Template = ctmp;
				Brush brs = Utils . GetDictionaryBrush ( "HeaderBrushGreen" );
				Filters . Background = brs;
				Filters . Content = "Filtering";
				Flags . FilterCommand = "";
				Flags . IsFiltered = false;
			}
			Mouse . OverrideCursor = Cursors . Arrow;
			return;
			//}
			//SQLFilter sf = new SQLFilter ( this );
			////// filter any table
			//if ( CurrentDb == "BANKACCOUNT" )
			//{
			//	sf . FilterList . Items . Clear ( );
			//	sf . FilterList . Items . Add ( "ID" );
			//	sf . FilterList . Items . Add ( "BANKNO" );
			//	sf . FilterList . Items . Add ( "CUSTNO" );
			//	sf . FilterList . Items . Add ( "ACTYPE" );
			//	sf . FilterList . Items . Add ( "BALANCE" );
			//	sf . FilterList . Items . Add ( "INTRATE" );
			//	sf . FilterList . Items . Add ( "ODATE" );
			//	sf . FilterList . Items . Add ( "CDATE" );
			//}
			//else if ( CurrentDb == "CUSTOMER" )
			//{
			//	sf . FilterList . Items . Clear ( );
			//	sf . FilterList . Items . Add ( "Id" );
			//	sf . FilterList . Items . Add ( "ID" );
			//	sf . FilterList . Items . Add ( "BANKNO" );
			//	sf . FilterList . Items . Add ( "CUSTNO" );
			//	sf . FilterList . Items . Add ( "ACTYPE" );
			//	sf . FilterList . Items . Add ( "FNAME" );
			//	sf . FilterList . Items . Add ( "LNAME" );
			//	sf . FilterList . Items . Add ( "ADDR1" );
			//	sf . FilterList . Items . Add ( "ADDR2" );
			//	sf . FilterList . Items . Add ( "TOWN" );
			//	sf . FilterList . Items . Add ( "COUNTY" );
			//	sf . FilterList . Items . Add ( "PCODE" );
			//	sf . FilterList . Items . Add ( "PHONE" );
			//	sf . FilterList . Items . Add ( "MOBILE" );
			//	sf . FilterList . Items . Add ( "DOB" );
			//	sf . FilterList . Items . Add ( "ODATE" );
			//	sf . FilterList . Items . Add ( "CDATE" );
			//}
			//else if ( CurrentDb == "DETAILS" )
			//{
			//	sf . FilterList . Items . Clear ( );
			//	sf . FilterList . Items . Add ( "ID" );
			//	sf . FilterList . Items . Add ( "BANKNO" );
			//	sf . FilterList . Items . Add ( "CUSTNO" );
			//	sf . FilterList . Items . Add ( "ACTYPE" );
			//	sf . FilterList . Items . Add ( "BALANCE" );
			//	sf . FilterList . Items . Add ( "INTRATE" );
			//	sf . FilterList . Items . Add ( "ODATE" );
			//	sf . FilterList . Items . Add ( "CDATE" );
			//}
			//sf . Operand . Items . Add ( "Equal to" );
			//sf . Operand . Items . Add ( "Not Equal to" );
			//sf . Operand . Items . Add ( "Greater than or Equal to" );
			//sf . Operand . Items . Add ( "Less than or Equal to" );
			//sf . Operand . Items . Add ( ">= value1 AND <= value2" );
			//sf . Operand . Items . Add ( "> value1 AND < value2" );
			//sf . Operand . Items . Add ( "< value1 OR > value2" );
			//sf . Operand . SelectedIndex = 0;
			////			}
			//sf . currentDb = CurrentDb;
			//sf . FilterResult = false;
			//sf . ShowDialog ( );
			//if ( sf . FilterResult )
			//{
			//	columnToFilterOn = sf . ColumnToFilterOn;
			//	filtervalue1 = sf . FilterValue . Text;
			//	filtervalue2 = sf . FilterValue2 . Text;
			//	operand = sf . operand;
			//	DoFilter ( sender , null );
			//	StatusBar . Text = $"Filtered Results are shown above. Column = {columnToFilterOn}, Condition = {operand}, Value(s) = {filtervalue1}, {filtervalue2} ";
			//Filters . IsEnabled = false;
			//Filters . Content = "Reset";
			//Filters . IsEnabled = true;
			//ControlTemplate ctmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGreen" );
			//Filters . Template = ctmp;
			//Brush brs = Utils . GetDictionaryBrush ( "HeaderBrushGreen" );
			//Filters . Background = brs;

			//}
			//else
			//{
			//	ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
			//	Filters . Template = tmp;
			//	Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
			//	Filters . Background = br;
			//	Filters . Content = "Filtering";
		}

		//*****************************************************************************************//
		//*****************************************************************************************//
		/// <summary>
		/// Just sets up the Filter/Duplicate buttons status
		/// </summary>
		private void ResetOptionButtons ( int mode )
		{
			if ( mode == 0 )
			{
				if ( !Filters . IsEnabled )
				{
					Filters . Content = "Reset";
					//					Filters . IsEnabled = true;
				}
				else
				{
					//					Filters . IsEnabled = false;
					Filters . Content = "Filtering";
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

		private void ResetauxilliaryButtons ( )
		{
			//ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
			//Filters . Template = tmp;
			//Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
			//Filters . Background = br;
			//Filters . Content = "Filtering";
		}

		public void SetButtonGradientBackground ( Button btn )
		{
			// how to change button background to a style in Code
			if ( btn == Filters )
			{
				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				btn . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
				btn . Background = br;
				btn . Content = "Filtering";
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

		#endregion Filtering code

		#region UPDATE NOTIFICATION HANDLERS

		/// <summary>
		/// Called by each ViewModel to handle loading new data into the relevant datagrid
		/// </summary>
		/// <param name="Viewer"></param>
		/// <param name="Grid"></param>
		/// <param name="args"></param>
		public async Task ReloadBankOnUpdateNotification ( SqlDbViewer Viewer, DataGrid Grid, DataChangeArgs args )
		{
			DataGrid CurrentGrid = null;
			// This gets called irrespective of who it triggered  it, so do not obther if the call was by a Bank Grid
			if ( Viewer . CurrentDb == "BANKACCOUNT" )
				return;

			if ( Flags . SqlBankViewer == null ) return;
			Flags . SqlBankViewer . Focus ( );
			CurrentGrid = Flags . SqlBankViewer . BankGrid;
			int currindx = CurrentGrid . SelectedIndex;
			Debug . WriteLine ( $"\nnREFRESHING { args . DbName}. SelectedIndex = {currindx}\n" );//\nIn Details update:\nCurrentViewer Tag = {this . Tag}\nFlags . CurrentSqlViewer?.Tag = {Flags . CurrentSqlViewer?.Tag}" );
			if ( currindx == -1 )
				currindx = 0;

			// Toggle ItemsSource to refresh datagrid
			CurrentGrid . ItemsSource = null;
			SqlViewerBankcollection = await BankCollection . LoadBank ( SqlViewerBankcollection, 1, false );

			Debug . WriteLine ( $"returned from Task loading Bank Details data ...." );
			CurrentGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerBankcollection );
			CurrentGrid . SelectedIndex = currindx;
			CurrentGrid . SelectedItem = currindx;
			ExtensionMethods . Refresh ( CurrentGrid );
			BankAccountViewModel data = CurrentGrid . SelectedItem as BankAccountViewModel;
			StatusBar . Text = $"Data reloaded due to external changes for Customer # {data?.CustNo}, Bank A/C # {data?.BankNo}";
			Debug . WriteLine ( $"\nCompleted REFRESHING { args . DbName}. SelectedIndex set to {currindx}\nCustNo = {data . CustNo}, BankNo = { data . BankNo}, AcType = {data . AcType}" );//\nIn Details update:\nCurrentViewer Tag = {this . Tag}\nFlags . CurrentSqlViewer?.Tag = {Flags . CurrentSqlViewer?.Tag}" );

			if ( args . DbName == "BANKACCOUNT" )
			{
				Flags . ActiveSqlViewer = Flags . SqlBankViewer;
				Flags . CurrentSqlViewer = Flags . SqlBankViewer;
			}
		}

		/// <summary>
		/// Called by each ViewModel to handle loading new data into the relevant datagrid
		/// </summary>
		/// <param name="Viewer"></param>
		/// <param name="Grid"></param>
		/// <param name="args"></param>
		public async Task ReloadCustomerOnUpdateNotification ( SqlDbViewer Viewer, DataGrid Grid, DataChangeArgs args )
		{
			DataGrid CurrentGrid = null;

			if ( Flags . SqlCustViewer == null ) return;
			Flags . SqlCustViewer . Focus ( );
			CurrentGrid = Flags . SqlCustViewer . CustomerGrid;
			int currindx = CurrentGrid . SelectedIndex;
			Debug . WriteLine ( $"\nREFRESHING { args . DbName}. SelectedIndex = {currindx}\n" );//\nIn Details update:\nCurrentViewer Tag = {this . Tag}\nFlags . CurrentSqlViewer?.Tag = {Flags . CurrentSqlViewer?.Tag}" );
			if ( currindx == -1 ) currindx = 0;

			// Toggle ItemsSource to refresh datagrid
			CurrentGrid . ItemsSource = null;

			Debug . WriteLine ( $"Calling Task to reload Customer data...." );
			SqlViewerCustcollection = await CustCollection . LoadCust ( SqlViewerCustcollection );
			Debug . WriteLine ( $"returned from Task loading Customer data ...." );
			CurrentGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerCustcollection );
			CurrentGrid . SelectedIndex = currindx;
			CurrentGrid . SelectedItem = currindx;
			ExtensionMethods . Refresh ( CurrentGrid );
			CustomerViewModel data = CurrentGrid . SelectedItem as CustomerViewModel;
			StatusBar . Text = $"Data reloaded due to external changes for Customer # {data?.CustNo}, Bank A/C # {data?.BankNo}";
			Debug . WriteLine ( $"\nCompleted REFRESHING { args . DbName}. SelectedIndex set to {currindx}\n" );//\nIn Details update:\nCurrentViewer Tag = {this . Tag}\nFlags . CurrentSqlViewer?.Tag = {Flags . CurrentSqlViewer?.Tag}" );

			if ( args . DbName == "CUSTOMER" )
			{
				Flags . ActiveSqlViewer = Flags . SqlCustViewer;
				Flags . CurrentSqlViewer = Flags . SqlCustViewer;
			}
		}

		/// <summary>
		/// Called by each ViewModel to handle loading new data into the relevant datagrid
		/// </summary>
		/// <param name="Viewer"></param>
		/// <param name="Grid"></param>
		/// <param name="args"></param>
		public async Task ReloadDetailsOnUpdateNotification ( SqlDbViewer Viewer, DataGrid Grid, DataChangeArgs args )
		{
			DataGrid CurrentGrid = null;

			if ( Flags . SqlDetViewer == null ) return;
			Flags . SqlDetViewer . Focus ( );
			CurrentGrid = Flags . SqlDetViewer . DetailsGrid;
			int currindx = CurrentGrid . SelectedIndex;
			Debug . WriteLine ( $"\nnREFRESHING { args . DbName}. SelectedIndex = {currindx}\n" );//\nIn Details update:\nCurrentViewer Tag = {this . Tag}\nFlags . CurrentSqlViewer?.Tag = {Flags . CurrentSqlViewer?.Tag}" );
			if ( currindx == -1 )
				currindx = 0;
			CurrentGrid . ItemsSource = null;

			Debug . WriteLine ( $"Calling Task to reload Details data...." );
			Mouse . OverrideCursor = Cursors . Wait;
			SqlViewerDetcollection = await DetCollection . LoadDet ( SqlViewerDetcollection );
			Debug . WriteLine ( $"returned from Task loading Details data ...." );
			CurrentGrid . ItemsSource = CollectionViewSource . GetDefaultView ( SqlViewerDetcollection );
			CurrentGrid . SelectedIndex = currindx;
			CurrentGrid . SelectedItem = currindx;
			ExtensionMethods . Refresh ( CurrentGrid );
			DetailsViewModel data = CurrentGrid . SelectedItem as DetailsViewModel;
			StatusBar . Text = $"Data reloaded due to external changes for Customer # {data?.CustNo}, Bank A/C # {data?.BankNo}";
			Debug . WriteLine ( $"\nCompleted REFRESHING { args . DbName}. SelectedIndex set to {currindx}\n" );//\nIn Details update:\nCurrentViewer Tag = {this . Tag}\nFlags . CurrentSqlViewer?.Tag = {Flags . CurrentSqlViewer?.Tag}" );
			Mouse . OverrideCursor = Cursors . Arrow;

			if ( args . DbName == "DETAILS" )
			{
				Flags . ActiveSqlViewer = Flags . SqlDetViewer;
				Flags . CurrentSqlViewer = Flags . SqlDetViewer;
			}
		}

		public async void UpdateDetailsOnEditDbChange ( string currentDb, int rowindex, object rowitem )
		{
			DataGrid dataGrid = null;
			Debug . WriteLine ( $"\nREMOTE DATA CHANGE HAS OCCURRED in {currentDb}\n" );//\nIn Details update:\nCurrentViewer Tag = {this . Tag}\nFlags . CurrentSqlViewer?.Tag = {Flags . CurrentSqlViewer?.Tag}" );
			dataGrid = DetailsGrid;
			SqlDbViewer ViewerPtr = this;
			int currindx = rowindex;

			if ( dataGrid . Items . NeedsRefresh )
			{
				dataGrid . Refresh ( );
				dataGrid . ItemsSource = SqlViewerDetcollection;
				dataGrid . SelectedIndex = currindx;
			}
			else
			{
				dataGrid . ItemsSource = null;
				Mouse . OverrideCursor = Cursors . Wait;
				SqlViewerDetcollection = await DetCollection . LoadDet ( SqlViewerDetcollection );
				dataGrid . ItemsSource = SqlViewerDetcollection;
				dataGrid . SelectedIndex = currindx;
				Mouse . OverrideCursor = Cursors . Arrow;
			}

			DetailsViewModel data = this . DetailsGrid . SelectedItem as DetailsViewModel;
			StatusBar . Text = $"Data reloaded due to external changes in { CurrentDb} Viewer for Customer # {data?.CustNo}, Bank A/C # {data?.BankNo}";
			dataGrid . Refresh ( );
		}

		#endregion UPDATE NOTIFICATION HANDLERS

		//*********************************************************************************************************//

		#region NotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		//*****************************************************************************************//
		private void OnPropertyChanged ( string PropertyName = null )
		{
			PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( PropertyName ) );
		}

		#endregion NotifyPropertyChanged

		//*********************************************************************************************************//

		#region PREVIEW Mouse METHODS

		private async void BankGrid_PreviewMouseDown ( object sender, MouseButtonEventArgs e )
		{
			int currsel = 0;
			// handle flags to let us know WE have triggered the selectedIndex change
			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			if ( e . ChangedButton == MouseButton . Right )
			{
				DataGridRow RowData = new DataGridRow ( );
				int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
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
					this . BankGrid . ItemsSource = null;
					this . BankGrid . Items . Clear ( );
					SqlViewerBankcollection = await BankCollection . LoadBank ( SqlViewerBankcollection, 1, false );
					this . BankGrid . ItemsSource = SqlViewerBankcollection;
					StatusBar . Text = "Current Record Updated Successfully...";
					// Notify everyone else of the data change
					EventControl . TriggerDataUpdated ( SqlViewerBankcollection,
						new LoadedEventArgs
						{
							CallerDb = "BANKACCOUNT",
							DataSource = SqlViewerBankcollection,
							RowCount = this . BankGrid . SelectedIndex
						} );
				}
				else
					this . BankGrid . SelectedItem = RowData . Item;

				// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
				Utils . SetUpGridSelection ( this . BankGrid, row );
				// This is essential to get selection activated again
				this . BankGrid . Focus ( );
			}
		}

		private async void CustomerGrid_PreviewMouseDown ( object sender, MouseButtonEventArgs e )
		{
			// handle flags to let us know WE have triggered the selectedIndex change
			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			if ( e . ChangedButton == MouseButton . Right )
			{
				DataGridRow RowData = new DataGridRow ( );
				int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
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
					this . CustomerGrid . ItemsSource = null;
					this . CustomerGrid . Items . Clear ( );
					SqlViewerCustcollection = await CustCollection . LoadCust ( SqlViewerCustcollection );
					this . CustomerGrid . ItemsSource = SqlViewerCustcollection;
					StatusBar . Text = "Current Record Updated Successfully...";
					// Notify everyone else of the data change
					EventControl . TriggerDataUpdated ( SqlViewerCustcollection,
						new LoadedEventArgs
						{
							CallerDb = "CUSTOMER",
							DataSource = SqlViewerCustcollection,
							RowCount = this . CustomerGrid . SelectedIndex
						} );
				}
				else
					this . CustomerGrid . SelectedItem = RowData . Item;

				// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
				Utils . SetUpGridSelection ( this . BankGrid, row );
				// This is essential to get selection activated again
				this . CustomerGrid . Focus ( );
			}
		}

		private async void DetailsGrid_PreviewMouseDown_1 ( object sender, MouseButtonEventArgs e )
		{
			//			int currsel = DetailsGrid.SelectedIndex;
			// handle flags to let us know WE have triggered the selectedIndex change
			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			try
			{
				if ( e . ChangedButton == MouseButton . Right )
				{
					DataGridRow RowData = new DataGridRow ( );
					int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
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
						this . DetailsGrid . ItemsSource = null;
						this . DetailsGrid . Items . Clear ( );
						SqlViewerDetcollection = await DetCollection . LoadDet ( SqlViewerDetcollection );
						this . DetailsGrid . ItemsSource = SqlViewerDetcollection;
						StatusBar . Text = "Current Record Updated Successfully...";
						// Notify everyone else of the data change
						EventControl . TriggerDataUpdated ( SqlViewerDetcollection,
							new LoadedEventArgs
							{
								CallerDb = "DETAILS",
								DataSource = SqlViewerDetcollection,
								RowCount = this . DetailsGrid . SelectedIndex
							} );
					}
					else
						this . DetailsGrid . SelectedIndex = row;

					// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
					Utils . GridInitialSetup ( DetailsGrid, row );
					// This is essential to get selection activated again
					this . DetailsGrid . Focus ( );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"General Exception : {ex . Message}, {ex . Data}" );
			}
		}

		#endregion PREVIEW Mouse METHODS

		#region REFRESH FUNCTIONALITY
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
		public async void ReloadGrid ( SqlDbViewer viewer, DataGrid DGrid )
		{
//			int topvisible = 0;
//			int bottonvisible = 0;
			try
			{
				Mouse . OverrideCursor = Cursors . Wait;
				// Make sure we are back on UI thread
				Debug . WriteLine ( $"Before thread switchback call	: Thread = { Thread . CurrentThread . ManagedThreadId}" );
				Debug . WriteLine ($"Reloading All Data...\n");
				int current = 0;
				current = DGrid . SelectedIndex == -1 ? 0 : DGrid . SelectedIndex;
				if ( CurrentDb == "BANKACCOUNT" )
				{
					BankAccountViewModel bvm = new BankAccountViewModel ( );
					int bbindex = this . BankGrid . SelectedIndex;
					this . BankGrid . SelectedItem = bbindex;
					bvm = this . BankGrid . SelectedItem as BankAccountViewModel;
					this . BankGrid . ItemsSource = null;
					this . BankGrid . Items . Clear ( );
					SqlViewerBankcollection = null;

					SqlViewerBankcollection = await BankCollection . LoadBank ( SqlViewerBankcollection, 1, false );
					this . BankGrid . ItemsSource = SqlViewerBankcollection;
					bbindex = Utils . FindMatchingRecord ( bvm . CustNo, bvm . BankNo, this . BankGrid, "BANKACCOUNT" );
					Utils . SetUpGridSelection ( this . BankGrid, bbindex );
					this . BankGrid . Focus ( );
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					CustomerViewModel bvm = new CustomerViewModel ( );
					int bbindex = this . CustomerGrid . SelectedIndex;
					this . CustomerGrid . SelectedItem = bbindex;
					bvm = this . CustomerGrid . SelectedItem as CustomerViewModel;
					this . CustomerGrid . ItemsSource = null;
					this . CustomerGrid . Items . Clear ( );
					SqlViewerCustcollection = null;

					SqlViewerCustcollection = await CustCollection . LoadCust( SqlViewerCustcollection);
					this . CustomerGrid . ItemsSource = SqlViewerCustcollection;
					bbindex = Utils . FindMatchingRecord ( bvm . CustNo, bvm . BankNo, this . CustomerGrid, "CUSTOMER" );
					Utils . SetUpGridSelection ( this . CustomerGrid, bbindex );
					this . CustomerGrid . Focus ( );
				}
				else if ( CurrentDb == "DETAILS" )
				{
					DetailsViewModel bvm = new DetailsViewModel ( );
					int bbindex = this . DetailsGrid . SelectedIndex;
					this . DetailsGrid . SelectedItem = bbindex;
					bvm = this . DetailsGrid . SelectedItem as DetailsViewModel;
					this . DetailsGrid . ItemsSource = null;
					this . DetailsGrid . Items . Clear ( );
					SqlViewerDetcollection = null;

					SqlViewerDetcollection = await DetCollection . LoadDet( SqlViewerDetcollection);
					this . DetailsGrid . ItemsSource = SqlViewerDetcollection;
					bbindex = Utils . FindMatchingRecord ( bvm . CustNo, bvm . BankNo, this . BankGrid, "BANKACCOUNT" );
					Utils . SetUpGridSelection ( this . BankGrid, bbindex );
					this . BankGrid . Focus ( );
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
		private void BankGrid_ScrollChanged ( object sender, ScrollChangedEventArgs e )
		{
			if ( RefreshInProgress ) return;
			DataGrid dg = null;
			dg = sender as DataGrid;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			scroll . CanContentScroll = true;
			SetScrollVariables ( sender );
			//Flags . TopVisibleBankGridRow = scroll . VerticalOffset;
			//Flags . BottomVisibleBankGridRow = scroll . VerticalOffset + scroll . ViewportHeight + 1;
			//Flags . ViewPortHeight = scroll . ViewportHeight;
		}

		private void CustomerGrid_ScrollChanged ( object sender, ScrollChangedEventArgs e )
		{
			if ( RefreshInProgress ) return;
			DataGrid dg = null;
			dg = sender as DataGrid;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			scroll . CanContentScroll = true;
			SetScrollVariables ( sender );
			//Flags . TopVisibleCustGridRow = scroll . VerticalOffset;
			//Flags . BottomVisibleCustGridRow = scroll . VerticalOffset + scroll . ViewportHeight + 1;
			//Flags . ViewPortHeight = scroll . ViewportHeight;
		}

		private void DetailsGrid_ScrollChanged ( object sender, ScrollChangedEventArgs e )
		{
			//if ( RefreshInProgress ) return;
			//DataGrid dg = null;
			//dg = sender as DataGrid;
			//var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			//scroll . CanContentScroll = true;
			//SetScrollVariables ( sender );
			////Flags . TopVisibleDetGridRow = scroll . VerticalOffset;
			//Flags . BottomVisibleDetGridRow = scroll . VerticalOffset + scroll . ViewportHeight + 1;
			//Flags . ViewPortHeight = scroll . ViewportHeight;
		}

		public void SetTopViewRow ( object sender )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			if ( dg . SelectedItem == null ) return;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			if ( scroll == null ) return;
			//			scroll . CanContentScroll = true;
			double d = scroll . VerticalOffset;
			int rounded = Convert . ToInt32 ( d );
			if ( dg == BankGrid )
			{
				//				Debug . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . TopVisibleBankGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . TopVisibleBankGridRow = ( double ) rounded;
			}
			else if ( dg == CustomerGrid )
			{
				//				Debug . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . TopVisibleCustGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . TopVisibleCustGridRow = ( double ) rounded;
			}
			else if ( dg == DetailsGrid )
			{
				//				Debug . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . TopVisibleDetGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . TopVisibleDetGridRow = ( double ) rounded;
			}
			//			Flags . ViewPortHeight = scroll . ViewportHeight;
		}

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
				//				Debug . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . BottomVisibleBankGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . BottomVisibleBankGridRow = ( double ) rounded;
			}
			else if ( dg . Name == "CustomerGrid" )
			{
				//				Debug . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . BottomVisibleCustGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . BottomVisibleCustGridRow = ( double ) rounded;
			}
			else if ( dg . Name == "DetailsGrid" )
			{
				//				Debug . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . BottomVisibleDetGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . BottomVisibleDetGridRow = ( double ) rounded;
			}
		}

		public void SetViewPort ( object sender )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			if ( dg . SelectedItem == null ) return;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			if ( scroll == null ) return;
			scroll . CanContentScroll = true;
			Flags . ViewPortHeight = scroll . ViewportHeight;
		}

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

		/// <summary>
		/// Calls three methods that store the scrollbar positions for a supplied datagrid
		/// </summary>
		/// <param name="sender"></param>
		public void SetScrollVariables ( object sender )
		{
			SetTopViewRow ( sender );
			SetBottomViewRow ( sender );
			SetViewPort ( sender );
		}

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

		private void DetailsGrid_Scroll ( object sender, System . Windows . Controls . Primitives . ScrollEventArgs e )
		{
			// This is called when scrolbar moves
			double x = e . NewValue;
		}

		#endregion Scroll bar utilities

		#region KEYBOARD DELEGATES & Window keyboard handler for SqlDbViewer class

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

		//*********************************************************************************************************//
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

			// apply a delegate
			if ( key1 && e . Key == Key . F12 )    // CTRL + F12
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

			if ( key1 && e . Key == Key . F9 )    // CTRL + F9
			{
				// lists all delegates & Events
				Debug . WriteLine ( "\nEvent subscriptions " );
				EventHandlers . ShowSubscribersCount ( );
				e . Handled = true;
				return;
			}
			else if ( key1 && e . Key == Key . System )     // CTRL + F10
			{
				// Major  listof GV[] variables (Guids etc]
				Debug . WriteLine ( "\nGridview GV[] Variables" );
				Flags . ListGridviewControlFlags ( 1 );
				key1 = false;
				e . Handled = true;
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
			else if ( key1 && e . Key == Key . F11 )
			{
				Debug . WriteLine ( "\nAll Flag. variables" );
				Flags . ShowAllFlags ( );
				key1 = false;
				return;
			}
			else if ( key1 && e . Key == Key . F7 )
			{
				Debug . WriteLine ( "\nAll Window handles" );
				Utils . GetWindowHandles ( );
				key1 = false;
				return;
			}
			//else if ( e . Key == Key . Escape )
			//{
			//	int currow = 0;
			//	//clear flags in ViewModel
			//	if ( CurrentDb == "BANKACCOUNT" )
			//	{
			//		Flags . ActiveSqlViewer = null;
			//		currow = this . BankGrid . SelectedIndex;
			//	}
			//	else if ( CurrentDb == "CUSTOMER" )
			//	{
			//		Flags . ActiveSqlViewer = null;
			//		currow = this . CustomerGrid . SelectedIndex;
			//	}
			//	else if ( CurrentDb == "DETAILS" )
			//	{
			//		Flags . ActiveSqlViewer = null;
			//		currow = this . DetailsGrid . SelectedIndex;
			//	}
			//	//SendDbSelectorCommand ( 99, "Window is closing", Flags . CurrentSqlViewer );
			//	// Clears Flags and the relevant Gv[] entry
			//	RemoveFromViewerList ( 99 );

			//	//				BankAccountViewModel . EditdbWndBank = null;
			//	UpdateDbSelectorBtns ( Flags . CurrentSqlViewer );
			//	Flags . CurrentSqlViewer = null;
			//	Close ( );
			//	e . Handled = true;
			//	key1 = false;
			//	return;
			//}
			else if ( e . Key == Key . OemQuestion )
			{
				// list Flags in Console
				Flags . PrintSundryVariables ( "Window_PreviewKeyDown()" );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( e . Key == Key . RightAlt ) //|| e . Key == Key . LeftCtrl )
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
					SqlViewerBankcollection . Clear ( );
					dtBank?.Clear ( );

					//Remove it from SQL Db as well
					DeleteRecord ( "BANKACCOUNT", BankRecord . BankNo, BankRecord . CustNo, CurrentRow );

					e . Handled = true;
					key1 = false;

					// Call the method to update any other Viewers that may be open
					EventControl . TriggerRecordDeleted ( CurrentDb, bank, cust, CurrentRow );
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
					SqlViewerCustcollection . Clear ( );
					CustCollection . dtCust?.Clear ( );

					//Remove it from SQL Db as well
					DeleteRecord ( "CUSTOMER", CustRecord . BankNo, CustRecord . CustNo, CurrentRow );

					e . Handled = true;
					key1 = false;

					// Call the method to update any other Viewers that may be open
					EventControl . TriggerRecordDeleted ( CurrentDb, bank, cust, CurrentRow );
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
					SqlViewerDetcollection . Clear ( );
					dtDetails?.Clear ( );

					//Remove it from SQL Db as well
					DeleteRecord ( "DETAILS", DetailsRecord . BankNo, DetailsRecord . CustNo, CurrentRow );
					e . Handled = true;
					key1 = false;

					// Call the method to update any other Viewers that may be open
					EventControl . TriggerRecordDeleted ( CurrentDb, bank, cust, CurrentRow );
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
			//else
			//{
			//	key1 = false;
			//}
			e . Handled = false;
		}

		#endregion KEYBOARD DELEGATES & Window keyboard handler for SqlDbViewer class

		#region color control support

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
				//				control . HorizontalContentAlignment
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
		/// Triigered by the three ViewModels
		/// so WE broadcast a notification to whoever to
		///  notify that the reloading of data via Asnyc SQL has been fuly completed
		/// </summary>
		/// <param name="o"> The sending object</param>
		/// <param name="args"> Sender name and Db Type</param>
		//public static void SendDBLoadedMsg ( object o, DataLoadedArgs args )
		//{
		//	if ( NotifyOfDataLoaded != null )
		//	{
		//		NotifyOfDataLoaded ( o, args );
		//	}
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
			if ( Flags . BankDbEditor != null )
				Flags . BankDbEditor . LinkRecords . IsChecked = Flags . LinkviewerRecords;
			if ( Flags . CustDbEditor != null )
				Flags . CustDbEditor . LinkRecords . IsChecked = Flags . LinkviewerRecords;
			if ( Flags . DetDbEditor != null )
				Flags . DetDbEditor . LinkRecords . IsChecked = Flags . LinkviewerRecords;
			LinkRecords . Refresh ( );
		}
	}
}