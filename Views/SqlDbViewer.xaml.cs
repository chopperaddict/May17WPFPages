#define SHOWSQLERRORMESSAGEBOX
#define SHOWWINDOWDATA
#define ALLOWREFRESH
#define LINKVIEWTOEDIT

using System;
using System . Collections . Generic;
using System . ComponentModel;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Controls . Primitives;
using System . Data;
using System . Data . SqlClient;
using System . Diagnostics;
using System . Linq;
using System . Threading;
using System . Threading . Tasks;
using System . Windows . Data;
using WPFPages . Properties;
using WPFPages . ViewModels;
using WPFPages . Views;
using System . IO;
using System . Security . Permissions;
using WPFPages;
using Newtonsoft . Json;
using Newtonsoft . Json . Linq;
using System . Windows . Shapes;
using static System . Net . Mime . MediaTypeNames;
using System . Configuration;
using System . CodeDom;
using System . Runtime . CompilerServices;

//[assembly: SecurityPermissionAttribute ( SecurityAction . RequestMinimum, Flags = ( SecurityPermissionFlag ) UIPermissionClipboard . AllClipboard )]


//how to save settings strings			
//Utils . SaveProperty ( "DefaultTextviewer", Environment . GetFolderPath ( Environment . SpecialFolder . MyDocuments ) );
//ConfigurationManager . RefreshSection ( "DefaultTextviewer" );

// How to read strings from from App resources
//string program = ( string ) Properties . Settings . Default [ "DefaultTextviewer" ];


namespace WPFPages
{
	// Delegate for use with dragand drop operations
	public delegate Point GetDragDropPosition ( IInputElement theElement );

	public partial class SqlDbViewer : Window, System . ComponentModel.INotifyPropertyChanged
	{
		// Used by Drag&Drop code
		int prevRowIndex = -1;


		public Func<int, int, int, int> IntFuncsDelegate;
		public Func<int, int, int> MathDelegate;

		private ClockTower _tower;
		public SqlDbViewer ThisViewer = null;
		//		public static Dispatcher UiThread = Dispatcher . CurrentDispatcher;

		//Temporary collection to support linq querying
		private BankCollection BankReserved = new BankCollection ( );
		private CustCollection CustReserved = new CustCollection ( );
		private DetCollection DetReserved = new DetCollection ( );

		// Declare all 3 of the local Db pointers
		private BankCollection SqlBankcollection = null;
		private CustCollection SqlCustcollection = null;
		private DetCollection SqlDetcollection = null;

		// Crucial structure for use when a Grid row is being edited
		private static RowData bvmCurrent = null;
		private static CustRowData cvmCurrent = null;
		private static RowData dvmCurrent = null;
		private bool IsRightMouseDown = false;
		private Point currentpos
		{get; set;}
		private static Point _startPoint
		{
			get; set;
		}
		private static bool ScrollBarMouseMove
		{
			get; set;
		}

		public Stopwatch stopwatch = new Stopwatch ( );

		#region CollectionView stuff
		// Get our personal Collection views of the Db
		private ICollectionView _SQLBankviewerView;
		private ICollectionView _SQLCustviewerView;
		private ICollectionView _SQLDetviewerView;
		private ICollectionView SQLBankviewerView
		{
			get
			{
				return _SQLBankviewerView;
			}
			set
			{
				_SQLBankviewerView = value;
			}
		}
		private ICollectionView SQLCustviewerView
		{
			get
			{
				return _SQLCustviewerView;
			}
			set
			{
				_SQLCustviewerView = value;
			}
		}
		private ICollectionView SQLDetViewerView
		{
			get
			{
				return _SQLDetviewerView;
			}
			set
			{
				_SQLDetviewerView = value;
			}
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
		//private int EditChangeType = 0;
		//private int ViewerChangeType = 0;
		private EditDb edb;
		private bool key1 = false;
		private bool IsEditing
		{
			get; set;
		}
		private bool GridHasFocus
		{
			get; set;
		}
		public static bool RefreshInProgress = false;
		#endregion Private declarationss

		#region Class setup - General Declarations

		public string CurrentInstanceDb = "";

		private string columnToFilterOn = "";
		private string filtervalue1 = "";
		private string filtervalue2 = "";
		private string operand = "";

		private string IsFiltered = "";
		private string FilterCommand = "";
		private string PrettyDetails = "";
		private bool EscapePressed = false;
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
		private bool test
		{
			get; set;
		}
		//		public static SqlDbViewer sqldbForm = null;

		#endregion Class setup - General Declarations


		#region STD PROPERTIES

		//instance Window handle
		public Window ThisWindow
		{
			get; set;
		}          // Current selectedIndex for each type of viewer data
		private int SavedBankRow
		{
			get; set;
		}
		private int SavedCustRow
		{
			get; set;
		}
		private int SavedDetRow
		{
			get; set;
		}
		public dynamic CurrentDb
		{
			get; set;
		}

		// These MAINTAIN setting values across instances !!!
		public static int bindex
		{
			get; set;
		}
		public static int cindex
		{
			get; set;
		}
		public static int dindex
		{
			get; set;
		}


		#endregion STD PROPERTIES

		#region FULL PROPERTIES
		// FULL PROPERTIES
		//***************** store the record data for whatever account type's record is the currently selected item
		//so DbSelector can bind to it as well
		private BankAccountViewModel currentBankSelectedRecord;
		public BankAccountViewModel CurrentBankSelectedRecord
		{
			get
			{
				return currentBankSelectedRecord;
			}
			set
			{
				currentBankSelectedRecord = value;
			}
		}
		private CustomerViewModel currentCustomerSelectedRecord;
		public CustomerViewModel CurrentCustomerSelectedRecord
		{
			get
			{
				return currentCustomerSelectedRecord;
			}
			set
			{
				currentCustomerSelectedRecord = value;
			}
		}
		private DetailsViewModel currentDetailsSelectedRecord;
		public DetailsViewModel CurrentDetailsSelectedRecord
		{
			get
			{
				return currentDetailsSelectedRecord;
			}
			set
			{
				currentDetailsSelectedRecord = value;
			}
		}
		#endregion FULL PROPERTIES

		//		public EventHandlers EventHandler = null;
		//		private bool SelectionhasChanged = false;

		//Variables used when a cell is edited to se if we need to update via SQL
		//		private object OriginalCellData = null;

		//		private string OriginalDataType = "";
		//		private int  = 0;
		//		private int OriginalCellColumn = 0;
		//		private bool OnSelectionChangedInProgress = false;
		public DataGridController dgControl;

		//---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
		/// <summary>
		/// Used to keep track of currently selected row in GridViwer
		/// </summary>
		//private int _selectedRow;

		//public int SelectedRow
		//{
		//	get { return _selectedRow; }
		//	set { _selectedRow = value; OnPropertyChanged ( SelectedRow . ToString ( ) ); }
		//}

		public struct scrollData
		{
			public double Banktop
			{
				get; set;
			}
			public double Bankbottom
			{
				get; set;
			}
			public double BankVisible
			{
				get; set;
			}
			public double Custtop
			{
				get; set;
			}
			public double Custbottom
			{
				get; set;
			}
			public double CustVisible
			{
				get; set;
			}
			public double Dettop
			{
				get; set;
			}
			public double Detbottom
			{
				get; set;
			}
			public double DetVisible
			{
				get; set;
			}
		}
		public static scrollData ScrollData = new scrollData ( );

		public DataGrid CurrentGrid;

		// Used by DbEdit Delegate notifier ONLY
		public static DataGrid CurrentActiveGrid;

		public static bool RemoteChangeActive = false;

		private bool IsLeftButtonDown
		{
			get; set;
		}

		/// <summary>
		///  A Delegate method we send in a call to SqlDbViewer to have it reset its grid.selectedIndex to our index
		///  </summary>
		/// <param name="CurrentDb"></param>
		/// <param name="Bankno"></param>
		/// <param name="Custno"></param>
		public void resetViewerIndex ( int Bankno, int Custno )
		{
			//int rec = 0;
			//DataGrid Grid = null;
			//if ( CurrentDb == "BANKACCOUNT" )
			//{
			//	SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
			//	Grid = this . BankGrid;
			//}
			//else if ( CurrentDb == "CUSTOMER" )
			//{
			//	SaveCurrentIndex ( 2, this . CustomerGrid . SelectedIndex );
			//	Grid = this . CustomerGrid;
			//}
			//else if ( CurrentDb == "DETAILS" )
			//{
			//	SaveCurrentIndex ( 3, this . DetailsGrid . SelectedIndex );
			//	Grid = this . DetailsGrid;
			//}
			//Grid . UnselectAll ( );
			//rec = Utils . FindMatchingRecord ( Custno . ToString ( ), Bankno . ToString ( ), Grid, CurrentInstanceDb );
			//Grid . SelectedIndex = rec != -1 ? rec : 0;
			//Grid . SelectedItem = rec != -1 ? rec : 0;
			//Utils . SetUpGridSelection ( Grid, rec != -1 ? rec : 0 );
			//if ( CurrentDb == "BANKACCOUNT" )
			//	SaveCurrentIndex ( 1, Grid . SelectedIndex );
			//else if ( CurrentDb == "CUSTOMER" )
			//	SaveCurrentIndex ( 2, Grid . SelectedIndex );
			//else if ( CurrentDb == "DETAILS" )
			//	SaveCurrentIndex ( 3, Grid . SelectedIndex );
			//return;
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

			//Identify individual windows for update protection
			this . Tag = ( Guid ) Guid . NewGuid ( );
			ThisViewer = this;
			Flags . CurrentSqlViewer = this;
			this . Show ( );
			WaitMessage . Visibility = Visibility . Visible;
			WaitMessage . Refresh ( );
		}

		private void _tower_Chime ( )
		{
			//			Debug . WriteLine ($"Chime event called in Method");
		}

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
			Utils . SetupWindowDrag ( this );
			this . Show ( );
		}
		/// <summary>
		/// MAIN STARTUP CALL from DbSelector
		/// </summary>
		/// <param name="caller"></param>
		/// <returns></returns>
		public SqlDbViewer ( string caller, object Collection )
		{
			IsViewerLoaded = false;
			InitializeComponent ( );
			this . Tag = ( Guid ) Guid . NewGuid ( );
			CurrentDb = caller;
			this . Title = CurrentDb + $" - {Title}";
			this . BankGrid . Visibility = Visibility . Collapsed;
			this . CustomerGrid . Visibility = Visibility . Collapsed;
			this . DetailsGrid . Visibility = Visibility . Collapsed;
			this . Show ( );
			WaitMessage . Visibility = Visibility . Visible;
			WaitMessage . Refresh ( );
			this . UpdateLayout ( );
			this . Refresh ( );


			this . BankGrid . AllowDrop = true;

		}

		private void OnWindowLoaded ( object sender, RoutedEventArgs e )
		{
			// THIS IS WHERE WE NEED TO SET THIS FLAG
			Flags . SqlViewerIsLoading = true;

			ThisViewer = this;
			this . Show ( );
			BringIntoView ( );
			this . Refresh ( );

			LoadData ( CurrentDb );

			ResetMenuBarStatus ( );

			// Handle window dragging
			//			this . MouseDown += delegate { DoDragMove ( ); };
			Utils . SetupWindowDrag ( this );
			RefreshBtn . IsEnabled = true;
			//This DOES call handler in DbSelector !!
			dgControl = new DataGridController ( );
			Flags . CurrentSqlViewer = this;
			//This is the EventHandler declared  in THIS FILE
			LoadedEventArgs ex = new LoadedEventArgs ( );

			string ndx = ( string ) Properties . Settings . Default [ "SqlDbViewer_bindex" ];
			bindex = int . Parse ( ndx );
			ndx = ( string ) Properties . Settings . Default [ "SqlDbViewer_cindex" ];
			cindex = int . Parse ( ndx );
			ndx = ( string ) Properties . Settings . Default [ "SqlDbViewer_dindex" ];
			dindex = int . Parse ( ndx );

			ex . CallerDb = CurrentDb;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				CurrentInstanceDb = "BANKACCOUNT";
				// Set up the various globalflags we use to control activity
				Flags . SqlBankGrid = this . BankGrid;
				Flags . SqlBankViewer = this;
				Flags . ActiveSqlViewer = this;
				Flags . ActiveSqlGrid = this . BankGrid;
				MainWindow . gv . SqlBankViewer = ( SqlDbViewer ) this;
				Flags . SetGridviewControlFlags ( this, this . BankGrid );
				CurrentActiveGrid = this . BankGrid;
				CurrentGrid = this . BankGrid;
				ResetMenuBarStatus ( );

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
				CurrentActiveGrid = this . CustomerGrid;
				CurrentGrid = this . BankGrid;
				ResetMenuBarStatus ( );
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
				CurrentActiveGrid = this . DetailsGrid;
				CurrentGrid = this . BankGrid;
				ResetMenuBarStatus ( );
			}

			// Grab a Guid for this viewer window early on
			if ( Flags . CurrentSqlViewer == null )
			{
				Flags . CurrentSqlViewer = this;
			}
			//This is the ONE & ONLY place we should set the Guid
			//			Flags.CurrentSqlViewer.Tag = Guid.NewGuid();
			MainWindow . gv . SqlViewerGuid = ( Guid ) Flags . CurrentSqlViewer . Tag;
			IsViewerLoaded = true;
			// clear global "loading new window" flag
			Flags . SqlViewerIsLoading = false;
			if ( Flags . LinkviewerRecords )
				LinkRecords . IsChecked = true;
			Mouse . OverrideCursor = Cursors . Arrow;

			this . Topmost = false;
		}



		public DragDropEffects DragEffects = new DragDropEffects ( );

		//private void ellipse_MouseMove ( object sender, MouseEventArgs e )
		//{
		//	Ellipse ellipse = sender as Ellipse;
		//	if ( ellipse != null && e . LeftButton == MouseButtonState . Pressed )
		//	{
		//		DragDropEffects DoDragDrop ( BankGrid,
		//				     ellipse . Fill . ToString ( ),
		//				     DragDropEffects . Copy );
		//	}
		//}
		//static bool HitTestScrollBar ( object sender, MouseButtonEventArgs e )
		//{
		//	HitTestResult hit = VisualTreeHelper . HitTest ( ( Visual ) sender, e . GetPosition ( ( IInputElement ) sender ) );
		//	return hit . VisualHit . GetVisualAncestor<System . Windows . Controls . Primitives . ScrollBar> ( ) != null;
		//}

		/// <summary>
		/// Load the relevant data from SQl Db
		/// </summary>
		/// <param name="caller"></param>
		/// <returns></returns>
		private async void LoadData ( string caller )
		{


			switch ( caller )
			{
				case "BANKACCOUNT":
					CurrentDb = "BANKACCOUNT";
					//subscribing to data  loading and changed event !!!
					SubscribeToEvents ( );
					Flags . ActiveSqlViewer = this;
					Mouse . OverrideCursor = Cursors . Wait;
					Debug . WriteLine ( "\nSQLDBVIEWER : awaiting Load of Bank Data" );
					stopwatch . Start ( );
					Flags . SqlBankActive  = true;
					BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );
					break;

				case "CUSTOMER":
					CurrentDb = "CUSTOMER";
					//subscribing to data  loading and changed event !!!
					SubscribeToEvents ( );
					Flags . ActiveSqlViewer = this;
					Mouse . OverrideCursor = Cursors . Wait;
					stopwatch . Start ( );
					Debug . WriteLine ( "\nSQLDBVIEWER : awaiting Load of Customer Data" );
					Flags . SqlCustActive  = true;
					CustCollection . LoadCust ( SqlCustcollection, "SQLDBVIEWER", 1, true );
					break;

				case "DETAILS":
					CurrentDb = "DETAILS";
					//subscribing to data  loading and changed event !!!
					SubscribeToEvents ( );
					Flags . ActiveSqlViewer = this;
					Mouse . OverrideCursor = Cursors . Wait;
					stopwatch . Start ( );
					Debug . WriteLine ( "\nSQLDBVIEWER : awaiting Load of Details Data" );

					Flags . SqlDetActive  = true;
					DetailCollection . LoadDet ( "SQLDBVIEWER", 1, true );
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

			EventControl . GlobalDataChanged += EventControl_GlobalDataChanged;


			// Triggered when multi account data is beign transferred from one to the other Db via GetExportRecords Window
			EventControl . TransferDataUpdated += EventControl_TransferDataUpdated;
			//Subscribe to the notifier EVENT so we know when a record is deleted from one of the grids
			EventControl . RecordDeleted += OnDeletion;
		}
		private void EventControl_GlobalDataChanged ( object sender, GlobalEventArgs e )
		{
			int x = 0;
			if ( e . CallerType == "SQLDBVIEWER" && e . AccountType == CurrentDb )
				return;
			//Update our own data tyoe only
			if ( CurrentDb == "BANKACCOUNT" )
			{
				Flags . SqlBankActive = true;
				BankCollection . LoadBank ( null, "BANKACCOUNT", 1, true );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				Flags . SqlCustActive  = true;
				CustCollection . LoadCust ( null, "CUSTOMER", 2, true );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				Flags . SqlDetActive  = true;
				DetailCollection . LoadDet ( "DETAILS", 1, true );
			}
		}


		public static DependencyObject GetScrollViewer ( DependencyObject o )
		{
			// Return the DependencyObject if it is a ScrollViewer
			if ( o is ScrollViewer )
			{
				return o;
			}

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
		private void EventControl_EditIndexChanged ( object sender, IndexChangedArgs e )
		{
			bool Proceed = false;
			string SearchBankno = e . Bankno;
			string SearchCustno = e . Custno;
			// EditDb (oranother viewer if Flags.LinkviewerRecords is set has triggered the record index change, so we need ot update our grid index
			//			Debug . WriteLine ( $"SQLDBVIEWER : Grid SelectedIndex changed event notification received from a [{e . Sender}] DataGrd. in EventControl_EditIndexChanged(824)" );

			if ( LoadingDbData )
			{
				LoadingDbData = false;
				return;
			}

			if ( IsEditing )
				return;

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
				//				if ( ( this . BankGrid . Items . Count > 0 )              // its us that triggered it
				//					|| ( e . SenderId == "SQLDBSERVER" && CurrentDb == "BANKACCOUNT" ) )            // Loading Db from Sql request
				if ( ( this . BankGrid . Items . Count > 0 && BankFiltered == false )              // its us that triggered it
					|| ( e . SenderId == "SQLDBSERVER" && CurrentDb == "BANKACCOUNT" ) )            // Loading Db from Sql request
				{
					if ( e . SenderId != "SQLDBSERVER" && CurrentDb == "BANKACCOUNT" && this . BankGrid != e . dGrid )      //Its a remote viewer that has made the index change
					{
						// All clear, so we can go ahead and update without effecting other viewers
						int rec = Utils . FindMatchingRecord ( SearchCustno, SearchBankno, this . BankGrid, CurrentDb );
						//						Debug . WriteLine ( $"SQLDBVIEWER : Resetting {CurrentDb} Grid SelectedIndex event to [{rec}] in EventControl_EditIndexChanged(859)" );
						if ( rec == -1 )
							return; //do nothing, no match found
						this . BankGrid . SelectedIndex = rec != -1 ? rec : 0;
						this . BankGrid . SelectedItem = rec != -1 ? rec : 0;
						Utils . SetUpGridSelection ( this . BankGrid, rec != -1 ? rec : 0 );
						SaveCurrentIndex ( 1, BankGrid . SelectedIndex );

					}
				}
				else if ( ( this . CustomerGrid . Items . Count > 0 )          // its us that triggered it
					|| ( e . SenderId == "SQLDBSERVER" && CurrentDb == "CUSTOMER" ) )               // Loading Db from Sql request
															//else if ( ( this . CustomerGrid . Items . Count > 0 && BankFiltered == false )          // its us that triggered it
															//	|| ( e . SenderId == "SQLDBSERVER" && CurrentDb == "CUSTOMER" ) )               // Loading Db from Sql request
				{
					if ( e . SenderId != "SQLDBSERVER" && CurrentDb == "CUSTOMER" && this . CustomerGrid != e . dGrid )     //Its a remote viewer that has made the index change
					{
						// All clear, so we can go ahead and update without effecting other viewers
						int rec = Utils . FindMatchingRecord ( SearchCustno, SearchBankno, this . CustomerGrid, CurrentDb );
						//						Debug . WriteLine ( $"SQLDBVIEWER : Resetting {CurrentDb} Grid SelectedIndex event to [{rec}] in EventControl_EditIndexChanged(869)" );
						if ( rec == -1 )
							return; //do nothing, no match found
						this . CustomerGrid . SelectedIndex = rec != -1 ? rec : 0;
						this . CustomerGrid . SelectedItem = rec != -1 ? rec : 0;
						Utils . SetUpGridSelection ( this . CustomerGrid, rec != -1 ? rec : 0 );
						SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
					}
				}
				else if ( ( this . DetailsGrid . Items . Count > 0 )                   // its us that triggered it
					|| ( e . SenderId == "SQLDBSERVER" && CurrentDb == "DETAILS" ) )                // Loading Db from Sql request
															//else if ( ( this . DetailsGrid . Items . Count > 0 && BankFiltered == false )                   // its us that triggered it
															//	|| ( e . SenderId == "SQLDBSERVER" && CurrentDb == "DETAILS" ) )                // Loading Db from Sql request
				{
					if ( e . SenderId != "SQLDBSERVER" && CurrentDb == "DETAILS" && this . DetailsGrid != e . dGrid )       //Its a remote viewer that has made the index change
					{
						// All clear, so we can go ahead and update without effecting other viewers
						int rec = Utils . FindMatchingRecord ( SearchCustno, SearchBankno, this . DetailsGrid, CurrentDb );
						//						Debug . WriteLine ( $"SQLDBVIEWER : Resetting {CurrentDb} Grid SelectedIndex event to [{rec}] in EventControl_EditIndexChanged(879)" );
						if ( rec == -1 )
							return; //do nothing, no match found
						DetailsGrid . SelectedIndex = rec >= 0 ? rec : 0;
						DetailsGrid . SelectedItem = rec != -1 ? rec : 0;
						Utils . SetUpGridSelection ( this . DetailsGrid, rec );
						SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
					}
				}
			}
		}

		/// <summary>
		/// Method to handle callback of DATAUPDATED when any of the 3  Sql viewer types Updates a row of data, (including ourselves)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EventControl_SqlDataUpdated ( object sender, LoadedEventArgs e )
		{
			Debug . WriteLine ( $"SQLDBVIEWER : Data updated event notification received successfully. in EventControl_SqlDataUpdated(919)" );

			// Handles ViewerDataChanged Event notification
			//			if ( e . CallerType == "SQLDBVIEWER" ) return;

			if ( CurrentDb == "BANKACCOUNT" && e . SenderGuid == this . Tag . ToString ( ) )
			{
				// its not for us, so dont bother
				return;
			}
			if ( CurrentDb == "CUSTOMER" && e . SenderGuid == this . Tag . ToString ( ) )
			{
				// its not for us, so dont bother
				return;
			}
			if ( CurrentDb == "DETAILS" && e . SenderGuid == this . Tag . ToString ( ) )
			{
				// we triggered it, so dont bother
				return;
			}

			// if we reach here, we need to update our grid, so reload the data
			// And then wait for the callback
			if ( CurrentDb == "BANKACCOUNT" )
			{
				// Save our current position
				SavedBankRow = this . BankGrid . SelectedIndex;
				Flags . SqlBankActive = true;
				BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );
				Console . WriteLine ( $"\nDEBUG RELOADING : Entered SQLDBVIEWER EventControl_SqlDataUpdated : Bank data triggered by BankCollection\nData is being loaded here ?\n " );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				// Save our current position
				SavedCustRow = this . CustomerGrid . SelectedIndex;
				Flags . SqlCustActive  = true;
				CustCollection . LoadCust ( SqlCustcollection, "SQLDBVIEWER", 1, true );
				Console . WriteLine ( $"\nDEBUG RELOADING : Entered SQLDBVIEWER EventControl_SqlDataUpdated : Customer data triggered by BankCollection\nData is being loaded here ?\n " );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				// Save our current position
				SavedDetRow = this . DetailsGrid . SelectedIndex;
				Flags . SqlDetActive  = true;
				DetailCollection . LoadDet ( "SQLDBVIEWER", 1, true );
				Console . WriteLine ( $"\nDEBUG RELOADING : Entered SQLDBVIEWER EventControl_SqlDataUpdated : Details data triggered by BankCollection\nData is being loaded here ?\n " );
			}
		}
		private void EventControl_BankDataLoaded ( object sender, LoadedEventArgs e )
		{
			if ( Flags . SqlBankActive == false )
				return;

			// Event handler for BankDataLoaded
			// it was us, ignore it
			if ( sender == null && e . DataSource == null )
				return;
			if ( e . CallerDb != "SQLDBVIEWER" )
				return;
			Debug . WriteLine ( $"\n * **Loading Bank data in SqlDbViewer after BankDataLoaded trigger\n" );
			stopwatch . Stop ( );
			if ( e . SenderGuid == this . Tag?.ToString ( ) )
				return;
			if ( e . CallerType != "SQLSERVER" )
				return;
			if ( e . DataSource == null || e . RowCount == 0 )
				return;

			SqlBankcollection = e . DataSource as BankCollection;
			if ( SqlBankcollection ?. Count == 0 )
				return;

			Flags . SqlBankActive = false;
			Debug . WriteLine ( $"SQLDBVIEWER : BankAccount  BankDataLoaded = Data fully loaded: {stopwatch . ElapsedMilliseconds} ms" );
			LoadingDbData = true;
			WaitMessage . Visibility = Visibility . Collapsed;
			BankGrid . Visibility = Visibility . Visible;

			if ( CollectionViewSource . GetDefaultView ( SqlBankcollection ) . IsEmpty )
				return;
			// Get our personal Collection view of the Db
			_SQLBankviewerView = CollectionViewSource . GetDefaultView ( SqlBankcollection );
			_SQLBankviewerView . Refresh ( );
			if ( _SQLBankviewerView . IsEmpty )
				return;
			this . BankGrid . ItemsSource = _SQLBankviewerView;
			this . BankGrid . SelectedIndex = bindex < 0 ? 0 : bindex;
			this . BankGrid . SelectedItem = bindex < 0 ? 0 : bindex;
			Utils . SetUpGridSelection ( this . BankGrid, this . BankGrid . SelectedIndex );
			//this . BankGrid . Refresh ( );
			ParseButtonText ( false );
			Count . Text = $"{Utils . GetPrettyGridStatistics ( this . BankGrid, this . BankGrid . SelectedIndex )}";
			Debug . WriteLine ( $"SQLDBVIEWER : BankAccount DataGrid fully populated" );
			if ( GridHasFocus == true )
			{
				this . BankGrid . Focus ( );
				GridHasFocus = false;
			}
			SaveCurrentIndex ( 1, this . BankGrid . SelectedIndex );
			ResetMenuBarStatus ( );

			// Reset main Db collection to FULL set of data
			BankReserved? . Clear ( );
			// Save our reserve collection
			BankReserved = SqlBankcollection;
			this . BankGrid . Refresh ( );
			StatusBar . Text = "All records for Bank Db are displayed...";
			//Force the selected row to be FULLY selected
			Utils . SetUpGridSelection ( this . BankGrid, bindex );
			this . BankGrid . ScrollIntoView ( this . BankGrid . SelectedItem );

		}
		private void EventControl_CustDataLoaded ( object sender, LoadedEventArgs e )
		{
			if ( Flags . SqlCustActive == false )
				return;
			// it was us, ignore it
			if ( e . SenderGuid == this . Tag . ToString ( ) )
				return;
			if ( e . CallerType != "SQLSERVER" )
				return;

			stopwatch . Stop ( );
			Debug . WriteLine ( $"SQLDBVIEWER : Customer Data fully loaded: {stopwatch . ElapsedMilliseconds} ms" );
			LoadingDbData = true;

			// Event handler for CustDataLoaded
			if ( e . DataSource == null || e . RowCount == 0 )
				return;

			SqlCustcollection = e . DataSource as CustCollection;
			if ( SqlCustcollection . Count == 0 )
				return;

			if ( CollectionViewSource . GetDefaultView ( SqlCustcollection ) . IsEmpty )
				return;

			Flags . SqlCustActive  = false;
			// Get our personal Collection view of the Db
			_SQLCustviewerView = CollectionViewSource . GetDefaultView ( SqlCustcollection );
			_SQLCustviewerView . Refresh ( );
			if ( _SQLCustviewerView . IsEmpty )
				return;
			this . CustomerGrid . ItemsSource = _SQLCustviewerView;
			this . CustomerGrid . SelectedIndex = cindex < 0 ? 0 : cindex;
			this . CustomerGrid . SelectedItem = cindex < 0 ? 0 : cindex;
			Utils . SetUpGridSelection ( this . CustomerGrid, this . CustomerGrid . SelectedIndex );
			this . CustomerGrid . UpdateLayout ( );
			this . CustomerGrid . Refresh ( );
			WaitMessage . Visibility = Visibility . Collapsed;
			this . CustomerGrid . Visibility = Visibility . Visible;
			this . CustomerGrid . Refresh ( );
			ParseButtonText ( true );
			Count . Text = $"{Utils . GetPrettyGridStatistics ( this . CustomerGrid, this . CustomerGrid . SelectedIndex )}";
			stopwatch . Stop ( );
			Debug . WriteLine ( $"SQLDBVIEWER : Customer DataGrid fully populated" );
			if ( GridHasFocus == true )
			{
				this . CustomerGrid . Focus ( );
				GridHasFocus = false;
			}
			// Save our reserve collection
			if ( CustReserved?.Count > 0 )
				// Reset main Db collection to FULL set of data
				CustReserved . Clear ( );
			CustReserved = SqlCustcollection;

			SaveCurrentIndex ( 2, this . CustomerGrid . SelectedIndex );
			StatusBar . Text = "All records for Customer Db are displayed...";
			//Force the selected row to be FULLY selected
			Utils . SetUpGridSelection ( this . CustomerGrid, cindex );
			this . CustomerGrid . ScrollIntoView ( this . CustomerGrid . SelectedItem );

		}
		private void EventControl_DetDataLoaded ( object sender, LoadedEventArgs e )
		{
			if ( Flags . SqlDetActive == false )
				return;
			//Triggered by any viewer updating a rows data
			// it was us, ignore it
			if ( e . SenderGuid == this . Tag . ToString ( ) )
				return;
			if ( e . CallerType != "SQLSERVER" )
				return;

			stopwatch . Stop ( );
			if ( e . DataSource == null )
				return;

			if ( e . RowCount == 0 )
				return;

			Flags . SqlDetActive  = false;
			WaitMessage . Visibility = Visibility . Collapsed;
			this . DetailsGrid . Visibility = Visibility . Visible;
			Debug . WriteLine ( $"SQLDBVIEWER : Details Data fully loaded: {stopwatch . ElapsedMilliseconds} ms" );
			LoadingDbData = true;

			if ( DetReserved?.Count > 0 )
				// Reset main Db collection to FULL set of data
				DetReserved . Clear ( );

			SqlDetcollection = e . DataSource as DetCollection;
			DetReserved = SqlDetcollection;
			// Get our personal Collection view of the Db
			if ( CollectionViewSource . GetDefaultView ( SqlDetcollection ) . IsEmpty )
				return;
			SQLDetViewerView = CollectionViewSource . GetDefaultView ( SqlDetcollection );
			SQLDetViewerView . Refresh ( );
			if ( SQLDetViewerView . IsEmpty )
				return;

			this . DetailsGrid . ItemsSource = SQLDetViewerView;
			this . DetailsGrid . SelectedIndex = dindex < 0 ? 0 : dindex;
			this . DetailsGrid . SelectedItem = dindex < 0 ? 0 : dindex;
			Utils . SetUpGridSelection ( this . DetailsGrid, this . DetailsGrid . SelectedIndex );
			this . DetailsGrid . UpdateLayout ( );
			this . DetailsGrid . Refresh ( );
			ParseButtonText ( true );
			Count . Text = $"{Utils . GetPrettyGridStatistics ( this . DetailsGrid, this . DetailsGrid . SelectedIndex )}";
			Debug . WriteLine ( $"SQLDBVIEWER : Details DataGrid fully populated" );
			if ( GridHasFocus == true )
			{
				this . DetailsGrid . Focus ( );
				GridHasFocus = false;
			}
			SaveCurrentIndex ( 3, this . DetailsGrid . SelectedIndex );
			ResetMenuBarStatus ( );
			StatusBar . Text = "All records for Details Db are displayed...";
			//Force the selected row to be FULLY selected
			Utils . SetUpGridSelection ( this . DetailsGrid, dindex );
			this . DetailsGrid . ScrollIntoView ( this . DetailsGrid . SelectedItem );
		}

		/// <summary>
		/// Method to handle callback of DATAUPDATED when another viewer Updates a row of data
		/// NB = The data is already reloaded when we get here
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EventControl_EditDbDataUpdated ( object sender, LoadedEventArgs e )
		{
			// All working well when update is in another SqlDbViewer or this viewers EditDb sends a  trigger
			// Monday, 28 May 2021
			int currsel = 0;
			// This STOPS the Multivewer loading from wiping out our selections and scroll position
			if ( sender == null && IsViewerLoaded == false )
				return;

			if ( e . CallerDb == "BANKACCOUNT" )
			{
				if ( SqlBankcollection == null )
					return;
				if ( this . BankGrid . Items . Count == 0 )
					return;
				RefreshInProgress = true;
				if ( e . Custno != null && e . Bankno != null )
				{
					this . BankGrid . UnselectAll ( );
					currsel = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . BankGrid, "BANKACCOUNT" );
					if ( currsel == -1 )
						currsel = SavedBankRow;
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
				this . BankGrid . SelectedItem = currsel;
				try
				{
					this . BankGrid . SelectedItem = SqlBankcollection . ElementAt ( currsel );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"{ex . Message}, @{ex . Data}" );
				}
				Utils . SetUpGridSelection ( this . BankGrid, currsel );
				RefreshInProgress = false;
				ParseButtonText ( true );
				Count . Text = $"{this . BankGrid . SelectedIndex} / { this . BankGrid . Items . Count . ToString ( )}";
				Mouse . OverrideCursor = Cursors . Arrow;
				SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
			}
			else if ( e . CallerDb == "CUSTOMER" )
			{
				if ( SqlCustcollection == null )
					return;
				if ( this . CustomerGrid . Items . Count == 0 )
					return;
				RefreshInProgress = true;
				if ( e . Custno != null && e . Bankno != null )
				{
					this . CustomerGrid . UnselectAll ( );
					currsel = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . CustomerGrid, "CUSTOMER" );
					if ( currsel == -1 )
						currsel = SavedCustRow != -1 ? SavedCustRow : 0;
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
				this . CustomerGrid . SelectedItem = currsel;
				try
				{
					this . CustomerGrid . SelectedItem = SqlCustcollection . ElementAt ( currsel );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"{ex . Message}, @{ex . Data}" );
				}
				Utils . SetUpGridSelection ( this . CustomerGrid, currsel );
				RefreshInProgress = false;
				ParseButtonText ( true );
				Count . Text = $"{this . CustomerGrid . SelectedIndex} / { this . CustomerGrid . Items . Count . ToString ( )}";
				Mouse . OverrideCursor = Cursors . Arrow;
				SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
			}
			else if ( e . CallerDb == "DETAILS" )
			{
				if ( SqlDetcollection == null )
					return;
				if ( this . DetailsGrid . Items . Count == 0 )
					return;
				RefreshInProgress = true;
				if ( e . Custno != null && e . Bankno != null )
				{
					this . DetailsGrid . UnselectAll ( );
					currsel = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . CustomerGrid, "DETAILS" );
					if ( currsel == -1 )
						currsel = SavedDetRow != -1 ? SavedDetRow : 0;
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
				this . DetailsGrid . SelectedItem = currsel;
				try
				{
					this . DetailsGrid . SelectedItem = SqlDetcollection . ElementAt ( currsel );
					Console . WriteLine ( $"Selecting Details row {currsel}" );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"{ex . Message}, @{ex . Data}" );
				}
				Utils . SetUpGridSelection ( this . DetailsGrid, currsel );
				RefreshInProgress = false;
				ParseButtonText ( true );
				Count . Text = $"{Utils . GetPrettyGridStatistics ( this . DetailsGrid, this . DetailsGrid . SelectedIndex )}";
				//				Count . Text = $"{this . DetailsGrid . SelectedIndex} / { this . DetailsGrid . Items . Count . ToString ( )}";
				//				Count . Text = this . DetailsGrid . Items . Count . ToString ( );
				Mouse . OverrideCursor = Cursors . Arrow;
				SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
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
				SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
				return output;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				var tmp = dg . SelectedItem as CustomerViewModel;
				output += tmp?.CustNo . ToString ( );
				output += ", Bank A/c : " + tmp?.BankNo . ToString ( );
				SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				var tmp = dg . SelectedItem as DetailsViewModel;
				output += tmp?.CustNo . ToString ( );
				output += ", Bank A/c : " + tmp?.BankNo . ToString ( );
				SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
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
		//		private void OnDeletion ( string sender, string bank, string cust, int CurrentRow )
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
				Flags . SqlBankActive = true;
				BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );
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
				Flags . SqlCustActive  = true;
				await CustCollection . LoadCust ( SqlCustcollection, "SQLDBVIEWER", 1, true );
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
				Mouse . OverrideCursor = Cursors . Wait;
				Flags . SqlDetActive  = true;
				DetailCollection . LoadDet("SQLDBVIEWER", 1, true );
				Mouse . OverrideCursor = Cursors . Arrow;
			}
		}


		#endregion *** CRUCIAL Methods - Event CALLBACK for data loading


		#region load/startup / Close down
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
			EventControl . DetDataLoaded -= EventControl_DetDataLoaded;
			;
			EventControl . GlobalDataChanged -= EventControl_GlobalDataChanged;

			// Triggered when multi account data is beign transferred from one to the other Db via GetExportRecords Window
			EventControl . TransferDataUpdated -= EventControl_TransferDataUpdated;
			EventControl . RecordDeleted -= OnDeletion;

			if ( MainWindow . gv . ViewerCount == 0 )
			{
				// No more Viewers open, so clear Viewers list in DbSelector
				MainWindow . gv . SqlViewerWindow = null;
			}
			UpdateDbSelectorBtns ( Flags . CurrentSqlViewer );

			Utils . SaveProperty ( "SqlDbViewer_bindex", bindex . ToString ( ) );
			Utils . SaveProperty ( "SqlDbViewer_cindex", cindex . ToString ( ) );
			Utils . SaveProperty ( "SqlDbViewer_dindex", dindex . ToString ( ) );


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
				if ( bvm == null )
					return;
				EventControl . TriggerViewerDataUpdated ( SqlBankcollection,
					new LoadedEventArgs
					{
						CallerType = "SQLDBVIEWER",
						Custno = bvm . CustNo,
						Bankno = bvm . BankNo,
						CallerDb = "BANKACCOUNT",
						DataSource = SqlBankcollection,
						SenderGuid = this . Tag . ToString ( ),
						RowCount = this . BankGrid . SelectedIndex
					} );
				EventControl . TriggerGlobalDataChanged ( this, new GlobalEventArgs
				{
					CallerType = "SQLDBVIEWER",
					AccountType = "BANKACCOUNT",
					SenderGuid = this . Tag?.ToString ( )
				} );
				Debug . WriteLine ( $"EditDb(1485) SQLDBVIEWER : in SendDataChanged : Sending ViewerDataUpdated EVENT for Bank" );
			}
			else if ( dbName == "CUSTOMER" )
			{
				CustomerViewModel bvm = this . CustomerGrid . SelectedItem as CustomerViewModel;
				if ( bvm == null )
					return;
				EventControl . TriggerViewerDataUpdated ( SqlCustcollection,
					new LoadedEventArgs
					{
						CallerType = "SQLDBVIEWER",
						Custno = bvm . CustNo,
						Bankno = bvm . BankNo,
						CallerDb = "CUSTOMER",
						SenderGuid = this . Tag . ToString ( ),
						DataSource = SqlCustcollection,
						RowCount = this . CustomerGrid . SelectedIndex
					} );
				EventControl . TriggerGlobalDataChanged ( this, new GlobalEventArgs
				{
					CallerType = "SQLDBVIEWER",
					AccountType = "CUSTOMER",
					SenderGuid = this . Tag?.ToString ( )
				} );
				Debug . WriteLine ( $"EditDb(1499) SQLDBVIEWER : in SendDataChanged : Sending ViewerDataUpdated EVENT for customer" );
			}
			else if ( dbName == "DETAILS" )
			{
				DetailsViewModel bvm = this . DetailsGrid . SelectedItem as DetailsViewModel;
				if ( bvm == null )
					return;
				EventControl . TriggerViewerDataUpdated ( SqlDetcollection,
					new LoadedEventArgs
					{
						CallerType = "SQLDBVIEWER",
						Custno = bvm . CustNo,
						Bankno = bvm . BankNo,
						CallerDb = "DETAILS",
						SenderGuid = this . Tag . ToString ( ),
						DataSource = SqlDetcollection,
						RowCount = this . DetailsGrid . SelectedIndex
					} );
				EventControl . TriggerGlobalDataChanged ( this, new GlobalEventArgs
				{
					CallerType = "SQLDBVIEWER",
					AccountType = "DETAILS",

					SenderGuid = this . Tag?.ToString ( )
				} );
				Debug . WriteLine ( $"EditDb(1514) SQLDBVIEWER : in SendDataChanged : Sending ViewerDataUpdated EVENT for Details" );
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
				SqlBankcollection = null;
				// Save our reserve collection
				BankReserved = null;
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
				SqlDetcollection = null;
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
				// Setup our pointer to this Viewer and Grid
				ClearPreviousDb ( this );
			}
			else if ( Flags . SqlCustViewer == this )
			{
				Flags . SqlCustGrid = null;
				this . CustomerGrid . ItemsSource = null;
				// Setup our pointer to this Viewer and Grid
				ClearPreviousDb ( this );
			}
			else if ( Flags . SqlDetViewer == this )
			{
				Flags . SqlDetGrid = null;
				this . DetailsGrid . ItemsSource = null;
				// Setup our pointer to this Viewer and Grid
				ClearPreviousDb ( this );
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
		private void ClearPreviousDb ( object obj )
		{
			if ( obj == Flags . SqlBankViewer )
			{
				Flags . SqlBankViewer = null;
				Flags . SqlBankGrid = null;
				SqlBankcollection?.Clear ( );
				this . BankGrid . ItemsSource = null;
			}
			else if ( obj == Flags . SqlCustViewer )
			{
				Flags . SqlCustViewer = null;
				Flags . SqlCustGrid = null;
				SqlCustcollection?.Clear ( );
				this . CustomerGrid . ItemsSource = null;
			}
			else if ( obj == Flags . SqlDetViewer )
			{
				Flags . SqlDetViewer = null;
				Flags . SqlDetGrid = null;
				SqlDetcollection?.Clear ( );
				this . DetailsGrid . ItemsSource = null;
			}
		}
		public void ShowBank_Click ( object sender, RoutedEventArgs e )
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
			ClearPreviousDb ( this );

			CurrentDb = "BANKACCOUNT";
			this . Title = CurrentDb + $" - Database Edit/View Utility";
			CurrentGrid . Visibility = Visibility . Hidden;
			this . BankGrid . Visibility = Visibility . Visible;
			CurrentGrid = this . BankGrid;
			Flags . SqlBankViewer = this;
			// We have to sort out the control structures BEFORE loading Customer Db Data and showing it
			CleanViewerGridData ( );
			// We have to Unsubscribe from previous Db Types Evet handlers BEFORE loading Customer Db Data and showing it
			ClearCurrentFlags ( );
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
			//This calls  LoadBankAsyncTask for us after sorting out the command line sort order requested
			///and it clears down any  existing data in DataTable or Collection
			Flags . SqlBankActive  = true;
			BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );

			CurrentActiveGrid = this . BankGrid;
			//// create GV[] variables for this new viewer grid
			DbSelector . UpdateControlFlags ( Flags . CurrentSqlViewer, CurrentDb, "" );

			SetScrollVariables ( this . BankGrid );
			Flags . SqlBankGrid = this . BankGrid;

			IsFiltered = "";
			SetButtonColor ( RefreshBtn, "BLUE" );
			Mouse . OverrideCursor = Cursors . Arrow;
			return;
		}
		/// <summary>
		/// Fetches SQL data for Customer Db and fills relevant DataGrid
		/// </summary>
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
			ClearPreviousDb ( this );

			CurrentDb = "CUSTOMER";
			this . Title = CurrentDb + $" - Database Edit/View Utility";
			CurrentGrid . Visibility = Visibility . Hidden;
			this . CustomerGrid . Visibility = Visibility . Visible;
			CurrentGrid = this . CustomerGrid;

			Flags . SqlCustViewer = this;
			// We have to sort out the control structures BEFORE loading Customer Db Data and showing it
			CleanViewerGridData ( );
			// We have to Unsubscribe from previous Db Types Evet handlers BEFORE loading Customer Db Data and showing it
			ClearCurrentFlags ( );

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
			Flags . SqlCustActive  = true;
			CustCollection . LoadCust ( SqlCustcollection, "SQLDBVIEWER", 1, true );

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

		/// <summary>
		/// Fetches SQL data for DetailsViewModel Db and fills relevant DataGrid
		/// <param name="sender"></param>
		/// <param name="e"></param></summary>
		public void ShowDetails_Click ( object sender, RoutedEventArgs e )
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
			ClearPreviousDb ( this );

			CurrentDb = "DETAILS";
			this . Title = CurrentDb + $" - Database Edit/View Utility";
			this . CurrentGrid . Visibility = Visibility . Hidden;
			this . DetailsGrid . Visibility = Visibility . Visible;
			CurrentGrid = this . DetailsGrid;

			Flags . SqlDetViewer = this;
			// We have to sort out the control structures BEFORE loading Details Db Data and showing it
			CleanViewerGridData ( );
			// We have to Unsubscribe from previous Db Types Evet handlers BEFORE loading Customer Db Data and showing it
			ClearCurrentFlags ( );

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
			Flags . SqlDetActive  = true;
			DetailCollection . LoadDet ( "SQLDBVIEWER", 1, true );

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

		private void ExitFilter_Click ( object sender, RoutedEventArgs e )
		{
			//Just "Close" the Filter panel
		}

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

		private void ContextMenu2_Click ( object sender, RoutedEventArgs e )
		{
			//Delete current Row
			BankAccountViewModel dg = sender as BankAccountViewModel;
			DataRowView row = ( DataRowView ) this . BankGrid . SelectedItem;
		}
		private void ContextMenu3_Click ( object sender, RoutedEventArgs e )
		{
			//Close Window
		}
		private void Multiaccs_Click ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registered" cos we can
			// Click the button before the window has had focus set
			Flags . CurrentSqlViewer = this;
			//Show only Customers with multiple Bank Accounts
			//			Window_MouseDown ( sender, null );
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
				Flags . SqlBankActive  = true;
				BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );
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
				Flags . SqlCustActive  = true;
				CustCollection . LoadCust ( SqlCustcollection, "SQLDBVIEWER", 1, true );
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
				Flags . SqlDetActive  = true;
				DetailCollection . LoadDet ( "SQLDBVIEWER", 1, true );
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

		private void ContextMenuFind_Click ( object sender, RoutedEventArgs e )
		{
			// find something - this returns  the top rows data in full
			BankAccountViewModel b = this . BankGrid . SelectedItem as BankAccountViewModel;
		}
		#endregion Standard Click Events

		#region grid row selection code

		private void BankGrid_SelectedCellsChanged ( object sender, SelectedCellsChangedEventArgs e )
		{
			//This fires whenever we click inside the grid !!!
			// Even just selecting a different row
			//This is THE ONE to use to update our DbSleector ViewersList text
			// Crucial flag for updating
			//			if ( Flags . DataLoadIngInProgress ) return;
			if ( Flags . DataLoadIngInProgress || Flags . EditDbDataChange )
				return;

			if ( CurrentDb == "BANKACCOUNT" )
			{
				if ( this . BankGrid . Items . Count == 0 )
					return;
				//				Debug . WriteLine ( $" 3-1 *** TRACE *** SQLDBVIEWER : BankGrid_SelectedCellsChanged BANKACCOUNT - Entering" );
				// All We are doing here is just updating the text in the DbSelectorViewersList
				//This gives me the entire Db Record in "c"
				BankAccountViewModel c = this . BankGrid?.SelectedItem as BankAccountViewModel;
				if ( c == null )
					return;
				string date = Convert . ToDateTime ( c . ODate ) . ToShortDateString ( );
				string s = $"Bank - # {c . CustNo}, Bank #{c . BankNo}, Customer # {c . CustNo}, £{c . Balance}, {c . IntRate}%,  {date}";
				// ensure global flag is cleared after loading a viewer
				Flags . SqlViewerIsLoading = false;
				UpdateDbSelectorItem ( s );
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
				if ( c == null )
					return;
				string s = $"Customer - # {c . CustNo}, Bank #@{c . BankNo}, {c . LName}, {c . Town}, {c . County} {c . PCode}";
				// ensure global flag is cleared after loading a viewer
				Flags . SqlViewerIsLoading = false;
				UpdateDbSelectorItem ( s );
			}
		}

		private void DetailsGrid_SelectedCellsChanged ( object sender, SelectedCellsChangedEventArgs e )
		{
			if ( RefreshInProgress )
				return;
			//This fires when we click inside the grid !!!
			//This is THE ONE to use to update our DbSelector ViewersList text
			if ( CurrentDb == "DETAILS" )
			{
				//This gives me an entire Db Record in "c"
				DetailsViewModel c = this . DetailsGrid?.SelectedItem as DetailsViewModel;
				if ( c == null )
					return;
				string date = Convert . ToDateTime ( c . ODate ) . ToShortDateString ( );
				string s = $"Details - # {c . CustNo}, Bank #@{c . BankNo}, Customer # {c . CustNo}, £{c . Balance}, {c . IntRate}%,  {date}";
				// ensure global flag is cleared after loading a viewer
				Flags . SqlViewerIsLoading = false;
				UpdateDbSelectorItem ( s );
			}
		}

		#endregion grid row selection code


		#region Keyboard /Mousebutton handlers

		private void Window_MouseDown ( object sender, MouseButtonEventArgs e )
		{
			Window_GotFocus ( sender, e );
		}

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

		private void BankGrid_MouseRightButtonUp ( object sender, MouseButtonEventArgs e )
		{

			//return;
			//Type type;
			//string cellData;
			//int row = -1;
			//int col = -1;
			//string colName = "";
			//object rowdata = null;
			//object cellValue = null;

			////Displays a Dialog with relevant info on the data record Right clicked on (Now After RowPopup is spawned)
			//// So it is disabled right now
			//return;

			//if ( CurrentDb == "BANKACCOUNT" )
			//{
			//	cellValue = DataGridSupport . GetCellContent ( sender, e, CurrentDb, out row, out col, out colName, out rowdata );
			//	if ( row == -1 )
			//		row = this . BankGrid . SelectedIndex;
			//}
			//else if ( CurrentDb == "CUSTOMER" )
			//{
			//	CustomerViewModel bvm = cvm;
			//	cellValue = DataGridSupport . GetCellContent ( sender, e, CurrentDb, out row, out col, out colName, out rowdata );
			//	if ( row == -1 )
			//		row = this . BankGrid . SelectedIndex;
			//}
			//else if ( CurrentDb == "DETAILS" )
			//{
			//	CustomerViewModel bvm = cvm;
			//	cellValue = DataGridSupport . GetCellContent ( sender, e, CurrentDb, out row, out col, out colName, out rowdata );
			//	if ( row == -1 )
			//		row = this . BankGrid . SelectedIndex;
			//}
			//if ( cellValue == null )
			//{
			//	MessageBox . Show ( $"Cannot access Data in the current cell, Row returned = {row}, Column = {col}, Column Name = {colName}" );
			//	return;
			//}
			//else if ( row == -1 && col == -1 )
			//{
			//	//Header was clicked in
			//	type = cellValue . GetType ( );
			//	cellData = cellValue . ToString ( );
			//	if ( cellData != "" )
			//	{
			//		if ( cellData . Contains ( ":" ) )
			//		{
			//			int offset = cellData . IndexOf ( ':' );
			//			string result = cellData . Substring ( offset + 1 ) . Trim ( );
			//			MessageBox . Show ( $"Column clicked was a Header  =\"{result}\"" );
			//		}
			//	}
			//	return;
			//}
			//type = cellValue . GetType ( );
			//cellData = cellValue . ToString ( );
			//MessageBox . Show ( $"Data in the current cell \r\nColumn is \"{colName},\", Data Type=\"{type . Name}\"\r\nData = [{cellData}]\",\r\nRow={row}, Column={col}", "Requested Cell Contents" );
		}

		private void BankGrid_MouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{
			//int currsel = 0;
			//// handle flags to let us know WE have triggered the selectedIndex change
			//MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			//if ( e . ChangedButton == MouseButton . Right )
			//{
			//	DataGridRow RowData = new DataGridRow ( );
			//	int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
			//	if ( row == -1 ) row = 0;
			//	RowInfoPopup rip = new RowInfoPopup ( "BANKACCOUNT", BankGrid);
			//	rip . DataContext = RowData;
			//	rip . BringIntoView ( );
			//	rip . Focus ( );
			//	rip . Topmost = true;
			//	rip . ShowDialog ( );

			//	Flags . ActiveSqlViewer = this;
			//	Flags . CurrentSqlViewer = this;
			//	//If data has been changed, update everywhere
			//	if ( rip . IsDirty )
			//	{
			//		// This is done in RowPopup()
			//		//BankAccountViewModel bvm = new BankAccountViewModel ( );
			//		//bvm = this . BankGrid . SelectedItem as BankAccountViewModel;
			//		//SQLHandlers sqlh = new SQLHandlers ( );
			//		//sqlh . UpdateDbRowAsync ( "BANKACCOUNT",bvm );
			//		this . BankGrid . ItemsSource = null;
			//		this . BankGrid . Items . Clear ( );
			//		BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );
			//		//this . BankGrid . ItemsSource = SqlViewerBankcollection;
			//		//StatusBar . Text = "Current Record Updated Successfully...";
			//		// Notify everyone else of the data change
			//		EventControl . TriggerViewerDataUpdated ( SqlBankcollection,
			//			new LoadedEventArgs
			//			{
			//				CallerType = "SQLDBVIEWER",
			//				Custno = bvm . CustNo,
			//				Bankno = bvm . BankNo,
			//				CurrSelection = this . BankGrid . SelectedIndex,
			//				CallerDb = "BANKACCOUNT",
			//				DataSource = SqlBankcollection,
			//				RowCount = this . BankGrid . SelectedIndex
			//			} );
			//	}
			//	else
			//		this . BankGrid . SelectedItem = RowData . Item;

			//	// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
			//	Utils . SetUpGridSelection ( this . BankGrid, row );
			//	ParseButtonText ( true );
			//	Count . Text = $"{Utils . GetPrettyGridStatistics ( this . BankGrid, this . BankGrid . SelectedIndex )}";
			//	//				Count . Text = $"{this . BankGrid . SelectedIndex} / { this . BankGrid . Items . Count . ToString ( )}";
			//	//				Count . Text = this . BankGrid . Items . Count . ToString ( );

			//	// This is essential to get selection activated again
			//	this . BankGrid . Focus ( );
			//}
		}

		#endregion Keyboard /Mousebutton handlers


		#region SQL SUPPORT FUNCTIONS
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

		private void CustomerGrid_GotFocus ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registered" cos we can
			// Click the button before the window has had focus set
			bool inhere = false;
			if ( inhere )
				return;
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
		#region Datagrid ROW UPDATING functionality

		#region CellEdit Checker functions

		private void BankGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
		//Get the BankAccount cell data and its Db Field name BEFORE
		// it has been changed and store in global variables
		{
			IsEditing = true;
			// Save  the current data for checking later on when we exit editing
			// but first, check to see if we already have one being saved !
			if ( bvmCurrent == null )
			{
				// Nope, so create a new one and get on with the edit process
				BankAccountViewModel tmp = new BankAccountViewModel ( );
				tmp = e . Row . Item as BankAccountViewModel;
				// This sets up a new bvmControl object if needed, else we  get a null back

				// How to obtain a ROWDATA object
				bvmCurrent = CellEditControl . BankGrid_EditStart ( bvmCurrent, e );
			}
		}

		//These all set a global bool to flag whether a cell has actually been changed
		//so we do not call SQL Update uneccessarily
		//*************************************************************************************************************//
		private void BankGrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			IsEditing = false;
			if ( bvmCurrent == null )
				return;

			// Has Data been changed in one of our rows. ?
			BankAccountViewModel dvm = this . BankGrid . SelectedItem as BankAccountViewModel;
			// The sequence of these next 2 blocks is critical !!!
			//if we get here, make sure we have been NOT been told to EsCAPE out
			//	this is a DataGridEditAction in Args e
			if ( e . EditAction == DataGridEditAction . Cancel )
			{
				// ESCAPE was hit, so we need to reload our grid with new data JIC
				// and this will notify any other open viewers as well
				bvmCurrent = null;
				Flags . SqlBankActive  = true;
				BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );
				return;
			}

			if ( CellEditControl . BankGrid_EditEnding ( bvmCurrent, BankGrid, e ) == false )
			{       // No change made
				return;
			}
		}
		private void CustomerGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
		//Get the BankAccount cell data and its Db Field name BEFORE
		// it has been changed and store in global variables
		{
			IsEditing = true;
			// Save  the current data for checking later on when we exit editing
			// but first, check to see if we already have one being saved !
			if ( cvmCurrent == null )
			{
				// Nope, so create a new one and get on with the edit process
				CustomerViewModel tmp = new CustomerViewModel ( );
				tmp = e . Row . Item as CustomerViewModel;
				// This sets up a new bvmControl object if needed, else we  get a null back
				cvmCurrent = CellEditControl . CustGrid_EditStart ( cvmCurrent, e );
			}
			IsEditing = true;

			//OrignalCellRow = e . Row . GetIndex ( );
			//OriginalCellColumn = e . Column . DisplayIndex;
			//DataGridColumn dgc = e . Column as DataGridColumn;
			//string name = dgc . SortMemberPath;
			//DataGridRow dgr = e . Row;
			//OriginalDataType = name;
			//switch ( name . ToUpper ( ) )
			//{
			//	case "BANKNO":
			//		OriginalCellData = cvm . BankNo;
			//		break;

			//	case "CUSTO":
			//		OriginalCellData = cvm . CustNo;
			//		break;

			//	case "ACTYPE":
			//		OriginalCellData = cvm . AcType;
			//		break;

			//	case "FNAME":
			//		OriginalCellData = cvm . FName;
			//		break;

			//	case "LNAME":
			//		OriginalCellData = cvm . LName;
			//		break;

			//	case "ADDR1":
			//		OriginalCellData = cvm . Addr1;
			//		break;

			//	case "ADDR2":
			//		OriginalCellData = cvm . Addr2;
			//		break;

			//	case "TOWN":
			//		OriginalCellData = cvm . Town;
			//		break;

			//	case "COUNTY":
			//		OriginalCellData = cvm . County;
			//		break;

			//	case "PCODE":
			//		OriginalCellData = cvm . PCode;
			//		break;

			//	case "PHONE":
			//		OriginalCellData = cvm . Phone;
			//		break;

			//	case "MOBILE":
			//		OriginalCellData = cvm . Mobile;
			//		break;

			//	case "DOB":
			//		OriginalCellData = cvm . Dob;
			//		break;

			//	case "ODATE":
			//		OriginalCellData = cvm . ODate;
			//		break;

			//	case "CDATE":
			//		OriginalCellData = cvm . CDate;
			//		break;
			//}
			//IsEditing = true;

		}
		private void CustomerGrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			if ( cvmCurrent == null )
				return;

			// Has Data been changed in one of our rows. ?
			CustomerViewModel cvm = this . CustomerGrid . SelectedItem as CustomerViewModel;
			cvm = e . Row . Item as CustomerViewModel;

			// The sequence of these next 2 blocks is critical !!!
			//if we get here, make sure we have been NOT been told to EsCAPE out
			//	this is a DataGridEditAction dgea
			if ( e . EditAction == DataGridEditAction . Cancel )
			{
				// ENTER was hit, so data has been saved - go ahead and reload our grid with new data
				// and this will notify any other open viewers as well
				cvmCurrent = null;
				Flags . SqlCustActive  = true;
				CustCollection . LoadCust ( SqlCustcollection, "SQLDBVIEWER", 2, true );
				return;
			}

			if ( CellEditControl . CustGrid_EditEnding ( cvmCurrent, CustomerGrid, e ) == false )
			{       // No change made
				return;
			}
			IsEditing = false;


			//CustomerViewModel c = CustomerGrid?.SelectedItem as CustomerViewModel;
			//TextBox textBox = e . EditingElement as TextBox;
			//if ( textBox == null )
			//{
			//	//default to save data - probably a date field that has been changed
			//	SelectionhasChanged = true;
			//	return;
			//}
			//string str = textBox . Text;
			//SelectionhasChanged = ( OriginalCellData?.ToString ( ) != str );
			//IsEditing = false;
		}
		private void DetailsGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
		//Get the BankAccount cell data and its Db Field name BEFORE
		// it has been changed and store in global variables
		{

			IsEditing = true;
			// Save  the current data for checking later on when we exit editing
			// but first, check to see if we already have one being saved !
			if ( dvmCurrent == null )
			{
				// Nope, so create a new one and get on with the edit process
				DetailsViewModel tmp = new DetailsViewModel ( );
				tmp = e . Row . Item as DetailsViewModel;
				// This sets up a new bvmControl object if needed, else we  get a null back
				dvmCurrent = CellEditControl . DetGrid_EditStart ( dvmCurrent, e );
			}

			//OrignalCellRow = e . Row . GetIndex ( );
			//OriginalCellColumn = e . Column . DisplayIndex;
			//DataGridColumn dgc = e . Column as DataGridColumn;
			//string name = dgc . SortMemberPath;
			//DataGridRow dgr = e . Row;
			////			DetailsViewModel dvm = dgr.Item as DetailsViewModel;
			//OriginalDataType = name;
			//switch ( name . ToUpper ( ) )
			//{
			//	case "BANKNO":
			//		OriginalCellData = dvm . BankNo;
			//		break;

			//	case "CUSTO":
			//		OriginalCellData = dvm . CustNo;
			//		break;

			//	case "ACTYPE":
			//		OriginalCellData = dvm . AcType;
			//		break;

			//	case "BALANCE":
			//		OriginalCellData = dvm . Balance;
			//		break;

			//	case "INTRATE":
			//		OriginalCellData = dvm . IntRate;
			//		break;

			//	case "ODATE":
			//		OriginalCellData = dvm . ODate;
			//		break;

			//	case "CDATE":
			//		OriginalCellData = dvm . CDate;
			//		break;
			//}
			//IsEditing = true;
		}
		private void DetailsGrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			if ( dvmCurrent == null )
				return;

			DetailsViewModel c = this . DetailsGrid . SelectedItem as DetailsViewModel;

			dvm = e . Row . Item as DetailsViewModel;

			// The sequence of these next 2 blocks is critical !!!
			//if we get here, make sure we have been NOT been told to EsCAPE out
			//	this is a DataGridEditAction in Args e
			if ( e . EditAction == DataGridEditAction . Cancel )
			{
				// ESCAPE was hit, so we need to reload our grid with new data JIC
				// and this will notify any other open viewers as well
				dvmCurrent = null;
				DetailCollection . LoadDet ( "SQLDBVIEWER", 2, true );
				return;
			}

			if ( CellEditControl . DetGrid_EditEnding ( dvmCurrent, DetailsGrid, e ) == false )
			{       // No change made
				return;
			}

			//TextBox textBox = e . EditingElement as TextBox;
			//if ( textBox == null )
			//{
			//	//default to save data - probably a date field that has been changed
			//	SelectionhasChanged = true;
			//	return;
			//}
			//string str = textBox . Text;
			//SelectionhasChanged = ( OriginalCellData?.ToString ( ) != str );
			//IsEditing = false;
		}

		#endregion CellEdit Checker functions

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

			//ViewerChangeType = 2;
			//EditChangeType = 0;
			BankAccountViewModel ss = bvm;
			CustomerViewModel cs = cvm;
			DetailsViewModel sa = dvm;

			Mouse . OverrideCursor = Cursors . Wait;

			// Set the control flags so that we know we have changed data when we notify other windows
			Flags . UpdateInProgress = true;

			// Set a global flag so we know we are in editing mode in the grid
			GridHasFocus = true;

			if ( e != null )
			{
				//Only called whn an edit has been completed
				SQLHandlers sqlh = new SQLHandlers ( );
				// These get the row with all the NEW data
				if ( CurrentDb == "BANKACCOUNT" )
				{
					int currow = 0;

					// if our saved row is null, it has already been checked in Cell_EndDedit processing
					// and found no changes have been made, so we can abort this update
					if ( bvmCurrent == null )
						return;

					currow = this . BankGrid . SelectedIndex != -1 ? this . BankGrid . SelectedIndex : 0;
					ss = this . BankGrid . SelectedItem as BankAccountViewModel;
					// This is the NEW DATA from the current row that we are sendign to SQL handler to update the DB's
					sqlh . UpdateDbRowAsync ( CurrentDb, ss, this . BankGrid . SelectedIndex );
					// Notify other Viewers of the update
					SendDataChanged ( this, this . BankGrid, "BANKACCOUNT" );

					this . BankGrid . SelectedIndex = currow;
					Utils . ScrollRecordInGrid ( this . BankGrid, currow );
					// Notify EditDb to upgrade its grid
					if ( Flags . CurrentEditDbViewer != null )
						Flags . CurrentEditDbViewer . UpdateGrid ( "BANKACCOUNT" );
					SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					int currow = 0;

					// if our saved row is null, it has already been checked in Cell_EndDedit processing
					// and found no changes have been made, so we can abort this update
					if ( cvmCurrent == null )
						return;

					currow = this . CustomerGrid . SelectedIndex != -1 ? this . CustomerGrid . SelectedIndex : 0;
					cs = this . CustomerGrid . SelectedItem as CustomerViewModel;
					// This is the NEW DATA from the current row
					sqlh . UpdateDbRowAsync ( CurrentDb, cs, this . CustomerGrid . SelectedIndex );
					// Notify other Viewers of the update
					SendDataChanged ( this, this . CustomerGrid, "CUSTOMER" );

					this . CustomerGrid . SelectedIndex = currow;
					Utils . ScrollRecordInGrid ( this . CustomerGrid, currow );
					// Notify EditDb to upgrade its grid
					if ( Flags . CurrentEditDbViewer != null )
						Flags . CurrentEditDbViewer . UpdateGrid ( "CUSTOMER" );
					SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
				}
				else if ( CurrentDb == "DETAILS" )
				{
					int currow = 0;

					// if our saved row is null, it has already been checked in Cell_EndDedit processing
					// and found no changes have been made, so we can abort this update
					if ( dvmCurrent == null )
						return;

					currow = this . DetailsGrid . SelectedIndex != -1 ? this . DetailsGrid . SelectedIndex : 0;
					sa = this . DetailsGrid . SelectedItem as DetailsViewModel;
					// sa contains the NEW DATA from the current row
					// Update Db itself via SQL
					sqlh . UpdateDbRowAsync ( CurrentDb, sa, currow );

					SendDataChanged ( this, this . DetailsGrid, "DETAILS" );

					this . DetailsGrid . SelectedIndex = currow;
					Utils . ScrollRecordInGrid ( this . DetailsGrid, currow );
					// Notify EditDb to upgrade its grid
					if ( Flags . CurrentEditDbViewer != null )
						Flags . CurrentEditDbViewer . UpdateGrid ( "DETAILS" );
					SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
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
								cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno", con );
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

								cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno", con );
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

								cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno", con );
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
								cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno", con );
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

								cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno", con );
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

								cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno", con );
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
								cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno", con );
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

								cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, BALANCE=@balance, INTRATE=@intrate, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno", con );
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

								cmd = new SqlCommand ( "UPDATE Customer SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno", con );
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
											"PHONE=@phone, MOBILE=@mobile, DOB=@dob,ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno", con );

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

							cmd = new SqlCommand ( "UPDATE BankAccount SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, " +
								" ODATE=@odate, CDATE=@cdate where CUSTNO = @custno AND BANKNO = @bankno", con );
							cmd . Parameters . AddWithValue ( "@id", Convert . ToInt32 ( cs . Id ) );
							cmd . Parameters . AddWithValue ( "@bankno", cs . BankNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@custno", cs . CustNo . ToString ( ) );
							cmd . Parameters . AddWithValue ( "@actype", Convert . ToInt32 ( cs . AcType ) );
							cmd . Parameters . AddWithValue ( "@odate", Convert . ToDateTime ( cs . ODate ) );
							cmd . Parameters . AddWithValue ( "@cdate", Convert . ToDateTime ( cs . CDate ) );
							cmd . ExecuteNonQuery ( );
							Debug . WriteLine ( "SQL Update of BankAccounts successful..." );

							cmd = new SqlCommand ( "UPDATE SecAccounts SET BANKNO=@bankno, CUSTNO=@custno, ACTYPE=@actype, " +
								"ODATE=@odate, CDATE=@cdate where CUSTNO=@custno AND BANKNO = @bankno", con );
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
			if ( IsFiltered != "" )
			{
				// filtered or multi accounts only displayed
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


		/// <summary>
		///  Method to update the contents of the ViewersList in DbSelector Window
		/// </summary>
		/// <param name="datarecord"></param>
		/// <param name="caller"></param>
		private void UpdateRowDetails ( object datarecord, string caller )
		// This updates the data in the DbSelector window's Viewers listbox, or add a new entry ????
		{
			bool Updated = false;
			if ( this . Tag == null )
				return;
			if ( Flags . DbSelectorOpen == null )
				return;
			for ( int x = 0 ; x < MainWindow . gv . MaxViewers ; x++ )
			{
				if ( x >= Flags . DbSelectorOpen?.ViewersList?.Items . Count )
					break;
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

		public void CloseViewer_Click ( object sender, RoutedEventArgs e )
		{
			// Make sure this window has it's pointer "Registered" cos we can
			// Click the button before the window has been fully closed
			Flags . CurrentSqlViewer = this;

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
		public void RemoveFromViewerList ( int x = -1 )
		{
			//Close current (THIS) Viewer Window
			//AND
			//clear flags in GV[] & Flags Structures
			// Clears Flags and theDbSelector Listview  entry
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
		}


		private void CustomerGrid_TargetUpdated ( object sender, DataTransferEventArgs e )
		{
			// row data Loading ???
			MainWindow . gv . Datagrid [ LoadIndex ] = this . CustomerGrid;
		}

		private void Window_GotFocus ( object sender, RoutedEventArgs e )
		{
			this . Focus ( );
			// Actually, this is Called mostly by MouseDown Handler
			//when Focus has been set to this window
			if ( Flags . CurrentSqlViewer == this )
				return;
			else
				Flags . CurrentSqlViewer = this;

			//Gotta sort out the current Db as it has now changed
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
			this . WindowState = WindowState . Normal;
		}

		public void CreateListboxItemBinding ( )
		{
			Binding binding = new Binding ( "ListBoxItemText" );
			binding . Source = PrettyDetails;
		}

		/// <summary>
		/// Updates the text of the relevant ViewersList entry when selection is changed
		/// </summary>
		/// <param name="data"></param>
		public static void UpdateDbSelectorItem ( string data )
		{
			if ( Flags . CurrentSqlViewer == null )
				return;
			if ( Flags . CurrentSqlViewer . Tag is null )
				return;
			// handle global flag to control viewer addition/updating
			if ( Flags . SqlViewerIsLoading )
				return;
			if ( RefreshInProgress )
				return;
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
				if ( Flags . CustEditDb != null )
				{
					Flags . CustEditDb . Focus ( );
					Flags . CustEditDb . BringIntoView ( );
					return;
				}
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
				if ( Flags . DetEditDb != null )
				{
					Flags . DetEditDb . Focus ( );
					Flags . DetEditDb . BringIntoView ( );
					return;
				}
				edb = new EditDb ( "DETAILS", this . DetailsGrid . SelectedIndex, this . DetailsGrid . SelectedItem, this );
				edb . Owner = this;
				edb . Show ( );
				//				ExtensionMethods . Refresh ( edb );
				edb . Refresh ( );
				Flags . DetEditDb = edb;
				RefreshBtn . IsEnabled = false;
			}
		}

		private void ItemsView_OnSelectionChanged ( object sender, SelectionChangedEventArgs e )
		//User has clicked a row in our DataGrid// OR in EditDb grid
		{
			int index = 0;
			string SearchCustNo = "";
			string SearchBankNo = "";
			EditDb edb = new EditDb ( );
			bool UpdateInProgress = false;
			Mouse . OverrideCursor = Cursors . Arrow;

			if ( RefreshInProgress )
				return;
			if ( UpdateInProgress )
				return;
			if ( LoadingDbData )
			{
				LoadingDbData = false;
				return;
			}

			if ( IsEditing )
				return;

			e . Handled = true;
			//			OnSelectionChangedInProgress = true;

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
					//					if ( CurrentBankSelectedRecord == null ) { OnSelectionChangedInProgress = true; return; }
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
					if ( index == -1 )
						index = 0;
					this . BankGrid . SelectedItem = index;
					Utils . SetUpGridSelection ( this . BankGrid, index );
					//// Updates  the MainWindow.gv[] structure
					UpdateRowDetails ( this . BankGrid . SelectedItem, "BankGrid" );
					RefreshInProgress = false;
					IsDirty = false;
					Count . Text = $"{Utils . GetPrettyGridStatistics ( this . BankGrid, this . BankGrid . SelectedIndex )}";
					//					Count . Text = $"{this . BankGrid . SelectedIndex} / { this . BankGrid . Items . Count . ToString ( )}";
					SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
					//					Debug . WriteLine ( $" *** TRACE 1-0 *** SQLDBVIEWER : Itemsview_OnSelectionChanged  BANKACCOUNT - Index = {this . BankGrid . SelectedIndex}" );
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					index = this . CustomerGrid . SelectedIndex;
					//					this . CustomerGrid . UnselectAll ( );
					if ( index == -1 )
						index = 0;
					this . CustomerGrid . SelectedItem = index;

					//Get the CustNo data to pass ot other viewers for their search
					CurrentCustomerSelectedRecord = this . CustomerGrid . SelectedItem as CustomerViewModel;
					if ( CurrentCustomerSelectedRecord == null )
						return;
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
					Count . Text = $"{Utils . GetPrettyGridStatistics ( this . CustomerGrid, this . CustomerGrid . SelectedIndex )}";
					//					Count . Text = $"{this . CustomerGrid . SelectedIndex} / { this . CustomerGrid . Items . Count . ToString ( )}";
					SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
					//					Debug . WriteLine ( $" *** TRACE 1-1***  SQLDBVIEWER : Itemsview_OnSelectionChanged  CUSTOMER - Index = {this . CustomerGrid . SelectedIndex}" );
				}
				else if ( CurrentDb == "DETAILS" )
				{
					index = this . DetailsGrid . SelectedIndex;
					//					this . DetailsGrid . UnselectAll ( );
					if ( index == -1 )
						index = 0;
					this . DetailsGrid . SelectedItem = index;
					//					Debug . WriteLine ( $" *** TRACE 1-2 *** SQLDBVIEWER : Itemsview_OnSelectionChanged  DETAILS - ENTERING  {this . DetailsGrid . SelectedIndex}, {this . DetailsGrid . SelectedItem}" );
					//Get the CustNo data to pass ot other viewers for their search
					CurrentDetailsSelectedRecord = this . DetailsGrid . SelectedItem as DetailsViewModel;
					if ( CurrentDetailsSelectedRecord == null )
						return;
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
					Count . Text = $"{Utils . GetPrettyGridStatistics ( this . DetailsGrid, this . DetailsGrid . SelectedIndex )}";
					//					Count . Text = $"{this . DetailsGrid . SelectedIndex} / { this . DetailsGrid . Items . Count . ToString ( )}";
					SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
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
						if ( index == -1 )
							index = 0;
						bindex = index;
						//Get the CustNo data to pass ot other viewers for their search
						CurrentBankSelectedRecord = this . BankGrid . SelectedItem as BankAccountViewModel;
						SearchCustNo = CurrentBankSelectedRecord . CustNo;
						SearchBankNo = CurrentBankSelectedRecord . BankNo;
						// Updates  the MainWindow.gv[] structure		&& ViewersList entry
						if ( Flags . UpdateInProgress == false )
							UpdateRowDetails ( this . BankGrid . SelectedItem, "BankGrid" );
						Flags . SqlViewerIndexIsChanging = false;
						IsDirty = false;
						if ( Flags . LinkviewerRecords )// && Triggered == false )
						{
							TriggerViewerIndexChanged ( this . BankGrid );
						}
					}
					//					Triggered = false;
					Count . Text = $"{Utils . GetPrettyGridStatistics ( this . BankGrid, this . BankGrid . SelectedIndex )}";
					SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					if ( this . CustomerGrid . SelectedItem != null )
					{
						index = this . CustomerGrid . SelectedIndex;
						if ( index == -1 )
							index = 0;
						cindex = index;
						CurrentCustomerSelectedRecord = this . CustomerGrid . SelectedItem as CustomerViewModel;
						SearchCustNo = CurrentCustomerSelectedRecord . CustNo;
						SearchBankNo = CurrentCustomerSelectedRecord . BankNo;
						// Updates  the MainWindow.gv[] structure
						if ( Flags . UpdateInProgress == false )
							UpdateRowDetails ( this . CustomerGrid . SelectedItem, "CustomerGrid" );
						IsDirty = false;
						Flags . SqlViewerIndexIsChanging = false;
						if ( Flags . LinkviewerRecords )// && Triggered == false )
						{
							TriggerViewerIndexChanged ( this . CustomerGrid );
						}
					}
					//					Triggered = false;
					Count . Text = $"{Utils . GetPrettyGridStatistics ( this . CustomerGrid, this . CustomerGrid . SelectedIndex )}";
					SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
				}
				else if ( CurrentDb == "DETAILS" )
				{
					if ( this . DetailsGrid . SelectedItem != null )
					{
						index = this . DetailsGrid . SelectedIndex;
						if ( index == -1 )
							index = 0;
						dindex = index;
						CurrentDetailsSelectedRecord = this . DetailsGrid . SelectedItem as DetailsViewModel;
						// This creates a new entry in gv[] if this is a new window being loaded
						SearchCustNo = CurrentDetailsSelectedRecord . CustNo;
						SearchBankNo = CurrentDetailsSelectedRecord . BankNo;
						// Updates  the MainWindow.gv[] structure
						if ( Flags . UpdateInProgress == false )
							UpdateRowDetails ( this . DetailsGrid . SelectedItem, "DetailsGrid" );
						IsDirty = false;
						Flags . SqlViewerIndexIsChanging = false;
						if ( Flags . LinkviewerRecords )//&& Triggered == false )
						{
							TriggerViewerIndexChanged ( this . DetailsGrid );
						}
					}
					//					Triggered = false;
					SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
					Count . Text = $"{Utils . GetPrettyGridStatistics ( this . DetailsGrid, this . DetailsGrid . SelectedIndex )}";
				}
			}
			UpdateAuxilliaries ( "" );
			Mouse . OverrideCursor = Cursors . Arrow;
			//EditChangeType = 0;
			//ViewerChangeType = 0;
			RefreshInProgress = false;
			UpdateInProgress = false;
			try
			{
				//				e . Handled = true;
			}
			catch ( Exception ex ) { }
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
			}
		}

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
				if ( Flags . FilterCommand == "" )
					return;
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

		private void ResetauxilliaryButtons ( )
		{
			//ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
			//Filters . Template = tmp;
			//Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
			//Filters . Background = br;
			//Filters . Content = "Filter";
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

		#region NotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged ( string PropertyName = null )
		{
			PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( PropertyName ) );
		}

		#endregion NotifyPropertyChanged


		#region PREVIEW Mouse METHODS

		//*************************************************************************************************************//
		//*************************************************************************************************************//
		private void CustomerGrid_PreviewMouseDown ( object sender, MouseButtonEventArgs e )
		{
			// handle flags to let us know WE have triggered the selectedIndex change
			//MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			//if ( e . ChangedButton == MouseButton . Right )
			//{
			//	DataGridRow RowData = new DataGridRow ( );
			//	int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
			//	if ( row == -1 ) row = 0;
			//	RowInfoPopup rip = new RowInfoPopup ( "CUSTOMER", CustomerGrid);
			//	rip . Topmost = true;
			//	rip . DataContext = RowData;
			//	rip . BringIntoView ( );
			//	rip . Focus ( );
			//	rip . ShowDialog ( );

			//	Flags . ActiveSqlViewer = this;
			//	Flags . CurrentSqlViewer = this;
			//	//If data has been changed, update everywhere
			//	// Update the row on return in case it has been changed
			//	if ( rip . IsDirty )
			//	{
			//		// This is done in RowPopup()
			//		//CustomerViewModel bvm = new CustomerViewModel ( );
			//		//cvm = this . CustomerGrid . SelectedItem as CustomerViewModel;
			//		//SQLHandlers sqlh = new SQLHandlers ( );
			//		//sqlh . UpdateDbRowAsync ( "CUSTOMER", cvm );
			//		this . CustomerGrid . ItemsSource = null;
			//		this . CustomerGrid . Items . Clear ( );
			//		CustCollection . LoadCust ( SqlCustcollection, "SQLDBVIEWER", 1, true );
			//		//this . CustomerGrid . ItemsSource = SqlViewerCustcollection;
			//		//StatusBar . Text = "Current Record Updated Successfully...";
			//		//// Notify everyone else of the data change
			//		//EventControl . TriggerDataUpdated ( SqlViewerCustcollection,
			//		//	new LoadedEventArgs
			//		//	{
			//		//		CallerDb = "CUSTOMER",
			//		//		DataSource = SqlViewerCustcollection,
			//		//		RowCount = this . CustomerGrid . SelectedIndex
			//		//	} );
			//	}
			//	//else
			//	//	this . CustomerGrid . SelectedItem = RowData . Item;

			//	// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
			//	Utils . SetUpGridSelection ( this . BankGrid, row );
			//	ParseButtonText ( true );
			//	Count . Text = $"{Utils . GetPrettyGridStatistics ( this . CustomerGrid, this . CustomerGrid . SelectedIndex )}";
			//	//				Count . Text = $"{this . CustomerGrid . SelectedIndex} / { this . CustomerGrid . Items . Count . ToString ( )}";
			//	//				Count . Text = this . CustomerGrid . Items . Count . ToString ( );
			//	// This is essential to get selection activated again
			//	this . CustomerGrid . Focus ( );
			//}
		}

		private void DetailsGrid_PreviewMouseDown_1 ( object sender, MouseButtonEventArgs e )
		{
			//			int currsel = DetailsGrid.SelectedIndex;
			// handle flags to let us know WE have triggered the selectedIndex change
			//			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			//try
			//{
			//	if ( e . ChangedButton == MouseButton . Right )
			//	{
			//		DataGridRow RowData = new DataGridRow ( );
			//		int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
			//		if ( row == -1 ) row = 0;
			//		RowInfoPopup rip = new RowInfoPopup ( "DETAILS", DetailsGrid);
			//		rip . DataContext = RowData;
			//		rip . BringIntoView ( );
			//		rip . Focus ( );
			//		rip . Topmost = true;
			//		rip . ShowDialog ( );

			//		Flags . ActiveSqlViewer = this;
			//		Flags . CurrentSqlViewer = this;
			//		//If data has been changed, update everywhere
			//		this . DetailsGrid . SelectedItem = RowData . Item;
			//		if ( rip . IsDirty )
			//		{
			//			DetailsViewModel bvm = new DetailsViewModel ( );
			//			dvm = this . DetailsGrid . SelectedItem as DetailsViewModel;
			//			SQLHandlers sqlh = new SQLHandlers ( );
			//			sqlh . UpdateDbRowAsync ( "DETAILS", dvm );
			//			this . DetailsGrid . ItemsSource = null;
			//			this . DetailsGrid . Items . Clear ( );
			//			DetailCollection . LoadDet ( SqlDetcollection, "SQLDBVIEWER", 1, true );
			//			//this . DetailsGrid . ItemsSource = SqlViewerDetcollection;
			//			//StatusBar . Text = "Current Record Updated Successfully...";
			//			//// Notify everyone else of the data change
			//			//EventControl . TriggerDataUpdated ( SqlViewerDetcollection,
			//			//	new LoadedEventArgs
			//			//	{
			//			//		CallerDb = "DETAILS",
			//			//		DataSource = SqlViewerDetcollection,
			//			//		RowCount = this . DetailsGrid . SelectedIndex
			//			//	} );
			//		}
			//		//else
			//		//	this . DetailsGrid . SelectedIndex = row;

			//		// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
			//		//Utils . GridInitialSetup ( DetailsGrid, row );
			//		//ParseButtonText ( true );
			//		//Count . Text = this . DetailsGrid . Items . Count . ToString ( );
			//		//// This is essential to get selection activated again
			//		//this . DetailsGrid . Focus ( );
			//	}
			//	else
			//	{
			//		// Left button clicked
			//		e . Handled = false;
			//	}

			//}
			//catch ( Exception ex )
			//{
			//	Debug . WriteLine ( $"General Exception : {ex . Message}, {ex . Data}" );
			//}
		}

		#endregion PREVIEW Mouse METHODS

		#region REFRESH FUNCTIONALITY
		private void Refresh_Click ( object sender, RoutedEventArgs e )
		{
			int currsel = 0;
			Mouse . OverrideCursor = Cursors . Wait;
			RefreshInProgress = true;
			if ( CurrentDb == "BANKACCOUNT" )
				ReloadGrid ( this, this . BankGrid );
			if ( CurrentDb == "CUSTOMER" )
				ReloadGrid ( this, this . CustomerGrid );
			if ( CurrentDb == "DETAILS" )
				ReloadGrid ( this, this . DetailsGrid );
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
		public void ReloadGrid ( SqlDbViewer viewer, DataGrid DGrid )
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
				if ( DGrid . SelectedIndex == -1 )
				{
					MessageBox . Show ( "Please ensure you have one record selected by clicking in the Data Grid !!" );
					return;
				}
				current = DGrid . SelectedIndex == -1 ? 0 : DGrid . SelectedIndex;
				if ( CurrentDb == "BANKACCOUNT" )
				{
					BankAccountViewModel bvm = new BankAccountViewModel ( );
					if ( bvm == null )
						return;
					int bbindex = this . BankGrid . SelectedIndex;
					this . BankGrid . SelectedItem = bbindex;
					bvm = this . BankGrid . SelectedItem as BankAccountViewModel;
					if ( bvm == null )
						return;
					this . BankGrid . ItemsSource = null;
					this . BankGrid . Items . Clear ( );
					SqlBankcollection = null;
					// Save our reserve collection
					BankReserved = null;

					Flags . SqlBankActive  = true;
					BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					CustomerViewModel bvm = new CustomerViewModel ( );
					if ( bvm == null )
						return;
					int bbindex = this . CustomerGrid . SelectedIndex;
					this . CustomerGrid . SelectedItem = bbindex;
					bvm = this . CustomerGrid . SelectedItem as CustomerViewModel;
					if ( bvm == null )
						return;
					this . CustomerGrid . ItemsSource = null;
					this . CustomerGrid . Items . Clear ( );
					SqlCustcollection = null;

					Flags . SqlCustActive  = true;
					CustCollection . LoadCust ( SqlCustcollection, "SQLDBVIEWER", 1, true );
				}
				else if ( CurrentDb == "DETAILS" )
				{
					DetailsViewModel bvm = new DetailsViewModel ( );
					if ( bvm == null )
						return;
					int bbindex = this . DetailsGrid . SelectedIndex;
					this . DetailsGrid . SelectedItem = bbindex;
					bvm = this . DetailsGrid . SelectedItem as DetailsViewModel;
					if ( bvm == null )
						return;
					this . DetailsGrid . ItemsSource = null;
					this . DetailsGrid . Items . Clear ( );
					SqlDetcollection = null;

					Flags . SqlDetActive  = true;
					DetailCollection . LoadDet ( "SQLDBVIEWER", 1, true );
				}
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
			if ( RefreshInProgress )
				return;
			DataGrid dg = null;
			dg = sender as DataGrid;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			scroll . CanContentScroll = true;
			SetScrollVariables ( sender );
		}

		private void CustomerGrid_ScrollChanged ( object sender, ScrollChangedEventArgs e )
		{
			if ( RefreshInProgress )
				return;
			DataGrid dg = null;
			dg = sender as DataGrid;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			scroll . CanContentScroll = true;
			SetScrollVariables ( sender );
		}

		private void DetailsGrid_ScrollChanged ( object sender, ScrollChangedEventArgs e )
		{
			if ( RefreshInProgress )
				return;
			DataGrid dg = null;
			dg = sender as DataGrid;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			scroll . CanContentScroll = true;
			SetScrollVariables ( sender );
		}

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
					scroll . ScrollToVerticalOffset ( ScrollData . Custtop );
				}
				else if ( sender == this . DetailsGrid )
				{
					scroll . ScrollToVerticalOffset ( ScrollData . Dettop );
					Console . WriteLine ( $"Setting Dettop to {ScrollData . Dettop}" );
				}
			}
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

		public void SetTopViewRow ( object sender )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			if ( dg . SelectedItem == null )
				return;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			if ( scroll == null )
				return;
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

		public void SetBottomViewRow ( object sender )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			if ( dg . SelectedItem == null )
				return;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			if ( scroll == null )
				return;
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
		public void SetViewPort ( object sender )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			if ( dg . SelectedItem == null )
				return;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			if ( scroll == null )
				return;
			scroll . CanContentScroll = true;
			Flags . ViewPortHeight = scroll . ViewportHeight;

			if ( sender == this . BankGrid )
				ScrollData . BankVisible = ( double ) scroll . ViewportHeight;
			else if ( sender == this . CustomerGrid )
				ScrollData . CustVisible = ( double ) scroll . ViewportHeight;
			else if ( sender == this . DetailsGrid )
				ScrollData . DetVisible = ( double ) scroll . ViewportHeight;

		}

		public void PrintCurrentviewportdata ( object sender )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			if ( dg == null )
				return;
			if ( dg . SelectedItem == null )
				return;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) sender );
			scroll . CanContentScroll = true;
			Debug . WriteLine (
			$"Top Visible Row		: Flag : {Flags . TopVisibleBankGridRow }	: Actual : {scroll . VerticalOffset}\n" +
			$"Bottom Visible Row	: Flag : {Flags . BottomVisibleBankGridRow }	: Actual : {scroll . VerticalOffset + scroll . ViewportHeight + 1}\n" +
			$"ViewPort : {scroll . ViewportHeight}\n" );
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
		{
		}

		public void KeyboardDelegate2 ( int y )
		{
		}

		public void KeyboardDelegate3 ( int y )
		{
		}

		public void KeyboardDelegate4 ( int y )
		{
		}

		public void KeyboardDelegate5 ( int y )
		{
		}

		public void KeyboardDelegate6 ( int y )
		{
		}

		public void KeyboardDelegate7 ( int y )
		{
		}

		public void KeyboardDelegate8 ( int y )
		{
		}

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

		public void Window_PreviewKeyDown ( object sender, KeyEventArgs e )
		{
			DataGrid dg = null;
			int CurrentRow = 0;
			bool showdebug = false;

			if ( IsEditing )
				return;

			if ( e . Key == Key . LeftCtrl )
			{
				key1 = true;
				if ( showdebug )
					Debug . WriteLine ( $"key1 = set to TRUE" );
				return;
			}
			if ( showdebug )
				Debug . WriteLine ( $"key1 = {key1},  Key = : {e . Key}" );
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
			else if ( e . Key == Key . Escape )  // CTRL + F3
			{
				EscapePressed = true;
				key1 = false;
				return;
			}
			else if ( key1 && e . Key == Key . F3 )  // CTRL + F3
			{
				// list SqlDbViewer static indexes
				Debug . WriteLine ( $"\nSqlDbViewer static indexes" );
				Debug . WriteLine ( $"bindex = {bindex}" );
				Debug . WriteLine ( $"cindex = {cindex}" );
				Debug . WriteLine ( $"dindex = {dindex}" );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( key1 && e . Key == Key . F4 )
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
				if ( res2 == 57942021 )
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
				Debug . WriteLine ( $"Success.... Remainder result = {total}" );
				return;
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
				key1 = false;
				return;
			}
			else if ( key1 && e . Key == Key . F11 )
			{
				Debug . WriteLine ( "\nAll Flag. variables" );
				Flags . ShowAllFlags ( );
				e . Handled = true;
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
				if ( ds . DialogResult != true )
					return;
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
				e . Handled = true;
				key1 = false;
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
				{
					BankGrid_SelectedCellsChanged ( dg, null );
					SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
				}
				else if ( dg == CustomerGrid )
				{
					CustomerGrid_SelectedCellsChanged ( dg, null );
					SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
				}
				else if ( dg == DetailsGrid )
				{
					DetailsGrid_SelectedCellsChanged ( dg, null );
					SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
				}
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
				{
					BankGrid_SelectedCellsChanged ( dg, null );
					SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
				}
				else if ( dg == CustomerGrid )
				{
					CustomerGrid_SelectedCellsChanged ( dg, null );
					SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
				}
				else if ( dg == DetailsGrid )
				{
					DetailsGrid_SelectedCellsChanged ( dg, null );
					SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
				}
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
				{
					BankGrid_SelectedCellsChanged ( dg, null );
					SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
				}
				else if ( dg == CustomerGrid )
				{
					CustomerGrid_SelectedCellsChanged ( dg, null );
					SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
				}
				else if ( dg == DetailsGrid )
				{
					DetailsGrid_SelectedCellsChanged ( dg, null );
					SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
				}
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
				{
					BankGrid_SelectedCellsChanged ( dg, null );
					SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
				}
				else if ( dg == CustomerGrid )
				{
					CustomerGrid_SelectedCellsChanged ( dg, null );
					SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
				}
				else if ( dg == DetailsGrid )
				{
					DetailsGrid_SelectedCellsChanged ( dg, null );
					SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
				}
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
				{
					BankGrid_SelectedCellsChanged ( dg, null );
					SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
				}
				else if ( dg == CustomerGrid )
				{
					CustomerGrid_SelectedCellsChanged ( dg, null );
					SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
				}
				else if ( dg == DetailsGrid )
				{
					DetailsGrid_SelectedCellsChanged ( dg, null );
					SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
				}
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
				if ( dg == BankGrid )
				{
					BankGrid_SelectedCellsChanged ( dg, null );
					SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
				}
				else if ( dg == CustomerGrid )
				{
					CustomerGrid_SelectedCellsChanged ( dg, null );
					SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
				}
				else if ( dg == DetailsGrid )
				{
					DetailsGrid_SelectedCellsChanged ( dg, null );
					SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
				}
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
						SenderGuid = this . Tag . ToString ( ),
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
						SenderGuid = this . Tag . ToString ( ),
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
						SenderGuid = this . Tag . ToString ( ),
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
		private void Linq1_Click ( object sender, RoutedEventArgs e )
		{
			LinqResults lq = new LinqResults ( );
			//			if ( sender . GetType ( ) == typeof ( SqlDbViewer ) )
			//			{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				BankAccountViewModel bvm = new BankAccountViewModel ( );
				if ( BankReserved . Count > SqlBankcollection . Count )
				{
					// Reset main Db collection to FULL set of data
					SqlBankcollection = BankReserved;
				}
				BankCollection vm = new BankCollection ( );
				foreach ( var item in vm)
				{
					vm . Add ( item );
				}
				SqlBankcollection = vm;
				BankGrid . ItemsSource = SqlBankcollection;
				ParseButtonText ( true );
				Count . Text = $"{Utils . GetPrettyGridStatistics ( this . BankGrid, this . BankGrid . SelectedIndex )}";
				BankFiltered = true;
				StatusBar . Text = "All records for Bank Account type 1 alone are displayed...";
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				if ( CustReserved . Count > SqlCustcollection . Count )
				{
					// Reset main Db collection to FULL set of data
					SqlCustcollection = CustReserved;
				}
				var accounts = from items in SqlCustcollection
					       where ( items . AcType == 1 )
					       orderby items . CustNo
					       select items;
				CustCollection vm = new CustCollection ( );
				foreach ( var item in accounts )
				{
					vm . Add ( item );
				}
				SqlCustcollection = vm;
				CustomerGrid . ItemsSource = SqlCustcollection;
				ParseButtonText ( true );
				Count . Text = $"{Utils . GetPrettyGridStatistics ( this . CustomerGrid, this . CustomerGrid . SelectedIndex )}";
				//				Count . Text = $"{this . CustomerGrid . SelectedIndex} / { this . CustomerGrid . Items . Count . ToString ( )}";
				//				Count . Text = CustomerGrid . Items . Count . ToString ( );
				CustFiltered = true;
				StatusBar . Text = "All records for Customer Account type 1 alone are displayed...";
			}
			else if ( CurrentDb == "DETAILS" )
			{
				if ( DetReserved . Count > SqlDetcollection . Count )
				{
					// Reset main Db collection to FULL set of data
					SqlDetcollection = DetReserved;
				}
				var accounts = from items in SqlDetcollection
					       where ( items . AcType == 1 )
					       orderby items . CustNo
					       select items;
				DetCollection vm = new DetCollection ( );
				foreach ( var item in accounts )
				{
					vm . Add ( item );
				}
				SqlDetcollection = vm;
				DetailsGrid . ItemsSource = SqlDetcollection;
				ParseButtonText ( true );
				Count . Text = $"{Utils . GetPrettyGridStatistics ( this . DetailsGrid, this . DetailsGrid . SelectedIndex )}";
				//				Count . Text = $"{this . DetailsGrid . SelectedIndex} / { this . DetailsGrid . Items . Count . ToString ( )}";
				//Count . Text = DetailsGrid . Items . Count . ToString ( );
				DetFiltered = true;
				StatusBar . Text = "All records for Details Account type 1 alone are displayed...";
			}
		}

		private void Linq2_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				if ( BankReserved . Count > SqlBankcollection . Count )
				{
					// Reset main Db collection to FULL set of data
					SqlBankcollection = BankReserved;
				}
				var accounts = from items in SqlBankcollection
					       where ( items . AcType == 2 )
					       orderby items . CustNo
					       select items;
				BankCollection vm = new BankCollection ( );
				foreach ( var item in accounts )
				{
					vm . Add ( item );
				}
				SqlBankcollection = vm;
				BankGrid . ItemsSource = SqlBankcollection;
				ParseButtonText ( true );
				Count . Text = $"{Utils . GetPrettyGridStatistics ( this . BankGrid, this . BankGrid . SelectedIndex )}";
				//				Count . Text = $"{this . BankGrid . SelectedIndex} / { this . BankGrid . Items . Count . ToString ( )}";
				//				Count . Text = BankGrid . Items . Count . ToString ( );
				BankFiltered = true;
				StatusBar . Text = "All records for Bank Account type 2 alone are displayed...";
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				if ( CustReserved . Count > SqlCustcollection . Count )
				{
					// Reset main Db collection to FULL set of data
					SqlCustcollection = CustReserved;
				}
				var accounts = from items in SqlCustcollection
					       where ( items . AcType == 2 )
					       orderby items . CustNo
					       select items;
				CustCollection vm = new CustCollection ( );
				foreach ( var item in accounts )
				{
					vm . Add ( item );
				}
				SqlCustcollection = vm;
				CustomerGrid . ItemsSource = SqlCustcollection;
				ParseButtonText ( true );
				Count . Text = $"{Utils . GetPrettyGridStatistics ( this . CustomerGrid, this . CustomerGrid . SelectedIndex )}";
				//				Count . Text = $"{this . CustomerGrid . SelectedIndex} / { this . CustomerGrid . Items . Count . ToString ( )}";
				//				Count . Text = CustomerGrid . Items . Count . ToString ( );
				CustFiltered = true;
				StatusBar . Text = "All records for Customer Account type 2 alone are displayed...";
			}
			else if ( CurrentDb == "DETAILS" )
			{
				if ( DetReserved . Count > SqlDetcollection . Count )
				{
					// Reset main Db collection to FULL set of data
					SqlDetcollection = DetReserved;
				}
				var accounts = from items in SqlDetcollection
					       where ( items . AcType == 2 )
					       orderby items . CustNo
					       select items;
				DetCollection vm = new DetCollection ( );
				foreach ( var item in accounts )
				{
					vm . Add ( item );
				}
				SqlDetcollection = vm;
				DetailsGrid . ItemsSource = SqlDetcollection;
				DetailsGrid . UpdateLayout ( );
				ParseButtonText ( true );
				Count . Text = $"{Utils . GetPrettyGridStatistics ( this . DetailsGrid, this . DetailsGrid . SelectedIndex )}";
				//				Count . Text = $"{this . DetailsGrid . SelectedIndex} / { this . DetailsGrid . Items . Count . ToString ( )}";
				//				Count . Text = DetailsGrid . Items . Count . ToString ( );
				DetFiltered = true;
				StatusBar . Text = "All records for Details Account type 2 alone are displayed...";
			}
		}

		private void Linq3_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				if ( BankReserved . Count > SqlBankcollection . Count )
				{
					// Reset main Db collection to FULL set of data
					SqlBankcollection = BankReserved;
				}
				var accounts = from items in SqlBankcollection
					       where ( items . AcType == 3 )
					       orderby items . CustNo
					       select items;
				BankCollection vm = new BankCollection ( );
				foreach ( var item in accounts )
				{
					vm . Add ( item );
				}
				SqlBankcollection = vm;
				BankGrid . ItemsSource = SqlBankcollection;
				ParseButtonText ( true );
				Count . Text = $"{Utils . GetPrettyGridStatistics ( this . BankGrid, this . BankGrid . SelectedIndex )}";
				//				Count . Text = $"{this . BankGrid . SelectedIndex} / { this . BankGrid . Items . Count . ToString ( )}";
				//				Count . Text = BankGrid . Items . Count . ToString ( );
				BankFiltered = true;
				StatusBar . Text = "All records for Bank Account type 3 alone are displayed...";
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				if ( CustReserved . Count > SqlCustcollection . Count )
				{
					// Reset main Db collection to FULL set of data
					SqlCustcollection = CustReserved;
				}
				var accounts = from items in SqlCustcollection
					       where ( items . AcType == 3 )
					       orderby items . CustNo
					       select items;
				CustCollection vm = new CustCollection ( );
				foreach ( var item in accounts )
				{
					vm . Add ( item );
				}
				SqlCustcollection = vm;
				CustomerGrid . ItemsSource = SqlCustcollection;
				ParseButtonText ( true );
				Count . Text = $"{Utils . GetPrettyGridStatistics ( this . CustomerGrid, this . CustomerGrid . SelectedIndex )}";
				//				Count . Text = $"{this . CustomerGrid . SelectedIndex} / { this . CustomerGrid . Items . Count . ToString ( )}";
				//				Count . Text = CustomerGrid . Items . Count . ToString ( );
				CustFiltered = true;
				StatusBar . Text = "All records for Customer Account type 3 alone are displayed...";
			}
			else if ( CurrentDb == "DETAILS" )
			{
				if ( DetReserved . Count > SqlDetcollection . Count )
				{
					// Reset main Db collection to FULL set of data
					SqlDetcollection = DetReserved;
				}
				var accounts = from items in SqlDetcollection
					       where ( items . AcType == 3 )
					       orderby items . CustNo
					       select items;
				DetCollection vm = new DetCollection ( );
				foreach ( var item in accounts )
				{
					vm . Add ( item );
				}
				SqlDetcollection = vm;
				DetailsGrid . ItemsSource = SqlDetcollection;
				ParseButtonText ( true );
				Count . Text = $"{Utils . GetPrettyGridStatistics ( this . DetailsGrid, this . DetailsGrid . SelectedIndex )}";
				//				Count . Text = $"{this . DetailsGrid . SelectedIndex} / { this . DetailsGrid . Items . Count . ToString ( )}";
				//				Count . Text = DetailsGrid . Items . Count . ToString ( );
				DetFiltered = true;
				StatusBar . Text = "All records for Details Account type 3 alone are displayed...";
			}
		}
		private void Linq4_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				if ( BankReserved . Count > SqlBankcollection . Count )
				{
					// Reset main Db collection to FULL set of data
					SqlBankcollection = BankReserved;
				}
				var accounts = from items in SqlBankcollection
					       where ( items . AcType == 4 )
					       orderby items . CustNo
					       select items;
				BankCollection vm = new BankCollection ( );
				foreach ( var item in accounts )
				{
					vm . Add ( item );
				}
				SqlBankcollection = vm;
				BankGrid . ItemsSource = SqlBankcollection;
				ParseButtonText ( true );
				Count . Text = $"{this . BankGrid . SelectedIndex} / { this . BankGrid . Items . Count . ToString ( )}";
				//				Count . Text = BankGrid . Items . Count . ToString ( );
				BankFiltered = true;
				StatusBar . Text = "All records for Bank Account type 4 alone are displayed...";
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				if ( CustReserved . Count > SqlCustcollection . Count )
				{
					// Reset main Db collection to FULL set of data
					SqlCustcollection = CustReserved;
				}
				var accounts = from items in SqlCustcollection
					       where ( items . AcType == 4 )
					       orderby items . CustNo
					       select items;
				CustCollection vm = new CustCollection ( );
				foreach ( var item in accounts )
				{
					vm . Add ( item );
				}
				SqlCustcollection = vm;
				CustomerGrid . ItemsSource = SqlCustcollection;
				ParseButtonText ( true );
				Count . Text = $"{this . CustomerGrid . SelectedIndex} / { this . CustomerGrid . Items . Count . ToString ( )}";
				//				Count . Text = CustomerGrid . Items . Count . ToString ( );
				CustFiltered = true;
				StatusBar . Text = "All records for Customer Account type 4 alone are displayed...";
			}
			else if ( CurrentDb == "DETAILS" )
			{
				if ( DetReserved . Count > SqlDetcollection . Count )
				{
					// Reset main Db collection to FULL set of data
					SqlDetcollection = DetReserved;
				}
				var accounts = from items in SqlDetcollection
					       where ( items . AcType == 4 )
					       orderby items . CustNo
					       select items;
				DetCollection vm = new DetCollection ( );
				foreach ( var item in accounts )
				{
					vm . Add ( item );
				}
				SqlDetcollection = vm;
				DetailsGrid . ItemsSource = SqlDetcollection;
				ParseButtonText ( true );
				Count . Text = $"{this . DetailsGrid . SelectedIndex} / { this . DetailsGrid . Items . Count . ToString ( )}";
				//				Count . Text = DetailsGrid . Items . Count . ToString ( );
				DetFiltered = true;
				StatusBar . Text = "All records for Details Account type 4 alone are displayed...";
			}
		}

		/// <summary>
		/// Create a subset that only includes those cust acs with >1 bankaccounts
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Linq5_Click ( object sender, RoutedEventArgs e )
		{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				if ( BankReserved . Count > SqlBankcollection . Count )
				{
					// Reset main Db collection to FULL set of data
					SqlBankcollection = BankReserved;
				}
				//select All the items first;
				var accounts = from items in SqlBankcollection orderby items . CustNo, items . BankNo select items;
				//Next Group collection on CustNo
				var grouped = accounts . GroupBy ( b => b . CustNo );

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
						{
							output . Add ( item2 );
						}
					}
				}
				BankCollection vm = new BankCollection ( );
				foreach ( var item in output )
				{
					vm . Add ( item );
				}
				SqlBankcollection = vm;
				BankGrid . ItemsSource = SqlBankcollection;
				//BankGrid . ItemsSource = output;
				ParseButtonText ( true );
				Count . Text = $"{this . BankGrid . SelectedIndex} / { this . BankGrid . Items . Count . ToString ( )}";
				//				Count . Text = BankGrid . Items . Count . ToString ( );
				StatusBar . Text = $"Filtering completed, {output . Count} Multi Account records match";
				BankFiltered = true;
			}
			if ( CurrentDb == "CUSTOMER" )
			{
				if ( CustReserved . Count > SqlCustcollection . Count )
				{
					// Reset main Db collection to FULL set of data
					SqlCustcollection = CustReserved;
				}
				//select All the items first;
				var accounts = from items in SqlCustcollection orderby items . CustNo, items . BankNo select items;
				//Next Group collection on CustNo
				var grouped = accounts . GroupBy ( b => b . CustNo );

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
						{
							output . Add ( item2 );
						}
					}
				}
				CustCollection vm = new CustCollection ( );
				foreach ( var item in accounts )
				{
					vm . Add ( item );
				}
				SqlCustcollection = vm;
				CustomerGrid . ItemsSource = SqlCustcollection;
				StatusBar . Text = $"Filtering completed, {output . Count} Multi Account records match";
				ParseButtonText ( true );
				Count . Text = $"{this . CustomerGrid . SelectedIndex} / { this . CustomerGrid . Items . Count . ToString ( )}";
				//				Count . Text = CustomerGrid . Items . Count . ToString ( );
				CustFiltered = true;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				if ( DetReserved . Count > SqlDetcollection . Count )
				{
					// Reset main Db collection to FULL set of data
					SqlDetcollection = DetReserved;
				}
				//select All the items first;
				var accounts = from items in SqlDetcollection orderby items . CustNo, items . BankNo select items;
				//Next Group collection on CustNo
				var grouped = accounts . GroupBy ( b => b . CustNo );

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
						{
							output . Add ( item2 );
						}
					}
				}
				DetCollection vm = new DetCollection ( );
				foreach ( var item in output )
				{
					vm . Add ( item );
				}
				SqlDetcollection = vm;
				DetailsGrid . ItemsSource = SqlDetcollection;
				StatusBar . Text = $"Filtering completed, {output . Count} Multi Account records match";
				ParseButtonText ( true );
				Count . Text = $"{this . DetailsGrid . SelectedIndex} / { this . DetailsGrid . Items . Count . ToString ( )}";
				//				Count . Text = DetailsGrid . Items . Count . ToString ( );
				DetFiltered = true;
			}
		}
		//*************************************************************************************************************//
		// Turn filter OFF
		/// <summary>
		/// Reset our viewer to FULL record display by reloading  the Db from disk - JIC
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Linq6_Click ( object sender, RoutedEventArgs e )
		{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				BankReserved . Clear ( );
				BankGrid . ItemsSource = null;
				Flags . SqlBankActive  = true;
				BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );
				BankGrid . Refresh ( );
				BankFiltered = false;
				Count . Text = $"{this . BankGrid . SelectedIndex} / { this . BankGrid . Items . Count . ToString ( )}";
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				CustomerGrid . ItemsSource = null;
				Flags . SqlCustActive  = true;
				CustCollection . LoadCust ( SqlCustcollection, "SQLDBVIEWER", 1, true );
				CustomerGrid . Refresh ( );
				CustFiltered = false;
				Count . Text = $"{this . CustomerGrid . SelectedIndex} / { this . CustomerGrid . Items . Count . ToString ( )}";
			}
			else if ( CurrentDb == "DETAILS" )
			{
				DetailsGrid . ItemsSource = null;
				Flags . SqlDetActive  = true;
				DetailCollection . LoadDet ( "SQLDBVIEWER", 1, true );
				DetailsGrid . Refresh ( );
				DetFiltered = false;
				Count . Text = $"{this . DetailsGrid . SelectedIndex} / { this . DetailsGrid . Items . Count . ToString ( )}";
			}
			ParseButtonText ( true );
			StatusBar . Text = "All available records are displayed...";

		}



		#endregion LINQ methods

		private void Filter_Click ( object sender, RoutedEventArgs e )
		{
			// opens a sub menu for exporting multi records
			ResetMenuBarStatus ( );
		}

		private void Options_Click ( object sender, RoutedEventArgs e )
		{
		}

		private void Exit_Click ( object sender, RoutedEventArgs e )
		{
			CloseViewer_Click ( sender, e );
		}

		/// <summary>
		/// Generic method to send Index changed Event trigger so that
		/// other viewers can update thier own grids as relevant
		/// </summary>
		/// <param name="grid"></param>
		public void TriggerViewerIndexChanged ( DataGrid grid )
		{
			string SearchCustNo = "";
			string SearchBankNo = "";
			if ( grid == this . BankGrid )
			{
				BankAccountViewModel CurrentBankSelectedRecord = this . BankGrid . SelectedItem as BankAccountViewModel;
				if ( CurrentBankSelectedRecord == null )
					return;
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
				if ( CurrentCustSelectedRecord == null )
					return;
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
				if ( CurrentDetSelectedRecord == null )
					return;
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
		//
		private void BankGrid_Scroll ( object sender, System . Windows . Controls . Primitives . ScrollEventArgs e )
		{

		}


		private void CustomerGrid_Scroll ( object sender, System . Windows . Controls . Primitives . ScrollEventArgs e )
		{

		}

		private void BankGrid_TargetUpdated ( object sender, DataTransferEventArgs e )
		{
			//MainWindow . gv . Datagrid [ LoadIndex ] = this . BankGrid;

		}
		private void DetailsGrid_TargetUpdated ( object sender, DataTransferEventArgs e )
		{
			//			MainWindow . gv . Datagrid [ LoadIndex ] = this . DetailsGrid;
		}

		//private void DoDragMove ( )
		//{//Handle the button NOT being the left mouse button
		// // which will crash the DragMove Fn.....
		//	try
		//	{
		//		this . DragMove ( );
		//	}
		//	catch { return; }
		//}

		private void TopMost_Click ( object sender, RoutedEventArgs e )
		{
			if ( this . TopMostOption . IsChecked == true )
				Topmost = true;
			else
				Topmost = false;
		}
		/// <summary>
		/// Savres the current selectedIndex for each grid
		/// </summary>
		/// <param name="type"></param>
		/// <param name="index"></param>
		private void SaveCurrentIndex ( int type, int index )
		{
			if ( index == -1 )
				return;
			if ( type == 1 )
				bindex = index;
			if ( type == 2 )
				cindex = index;
			if ( type == 3 )
				dindex = index;
		}

		#endregion UNUSED CODE

		private void ExportBankCSV_Click ( object sender, RoutedEventArgs e )
		{
			string message = "";
			string part2 = "";
			string outstats = "Please check Output for failure details";
			// Export BANK DATA to CSV
			int count = BankCollection . ExportBankData ( @"C:\users\ianch\Documents\Bankb.csv", CurrentDb );
			if ( count > 0 )
			{
				part2 = $"\n{count} records have been saved successully.";
				message = $"Bank Data Exported successfully.{part2}";
			}
			else
			{
				part2 = $"The data was NOT saved correctly...\n{outstats}";
				message = part2;
			}
			MessageBox . Show ( message );
		}
		private void ExportCustCSV_Click ( object sender, RoutedEventArgs e )
		{
			string message = "";
			string part2 = "";
			string outstats = "Please check Output for failure details";
			// Export CUSTOMER DATA to CSV
			int count = CustCollection . ExportCustData ( @"C:\users\ianch\Documents\CustomerDbBankb.csv", CurrentDb );
			if ( count > 0 )
			{
				part2 = $"\n{count} records have been saved successully.";
				message = $"Bank Data Exported successfully.{part2}";
			}
			else
			{
				part2 = $"The data was NOT saved correctly...\n{outstats}";
				message = part2;
			}
			MessageBox . Show ( message );
		}
		private void ExportDetCSV_Click ( object sender, RoutedEventArgs e )
		{
			string message = "";
			string part2 = "";
			string outstats = "Please check Output for failure details";
			// Export DETAILS DATA to CSV
			int count = DetailCollection . ExportDetData ( @"C:\users\ianch\Documents\DetailsDb.csv", CurrentDb );
			if ( count > 0 )
			{
				part2 = $"\n{count} records have been saved successully.";
				message = $"Bank Data Exported successfully.{part2}";
			}
			else
			{
				part2 = $"The data was NOT saved correctly...\n{outstats}";
				message = part2;
			}
			MessageBox . Show ( message );
		}

		private void ImportDetCSV_Click ( object sender, RoutedEventArgs e )
		{

		}

		private void ImportCustCSV_Click ( object sender, RoutedEventArgs e )
		{

		}

		private void ImportBankCSV_Click ( object sender, RoutedEventArgs e )
		{
			ImportDbData . UpdateBankDbFromTextFile ( );
		}


		/// <summary>
		/// All working 10/6/21
		/// Allows selection of any records in BankAccount Db that are multi accounts and then add them to the Bank Db
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Exportselected_Click ( object sender, RoutedEventArgs e )
		{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				if ( SqlBankcollection == null )
					return;
				List<BankAccountViewModel> recs = new List<BankAccountViewModel> ( );
				//select All the items first;
				var accounts = from items in SqlBankcollection orderby items . CustNo, items . BankNo select items;
				//Next Group collection on CustNo
				var grouped = accounts . GroupBy ( b => b . CustNo );

				//Now filter content down to only those a/c's with multiple Bank A/c's
				var sel = from g in grouped
					  where g . Count ( ) > 1
					  select g;

				// Finally, iterate thru the list of grouped CustNo's matching to CustNo in the full BankAccounts data
				// giving us ONLY the full records for any records that have > 1 Bank accounts
				List<BankAccountViewModel> outputt = new List<BankAccountViewModel> ( );

				foreach ( var item1 in sel )
				{
					foreach ( var item2 in accounts )
					{
						if ( item2 . CustNo . ToString ( ) == item1 . Key )
						{
							recs . Add ( item2 );
						}
					}
				}
				List<BankAccountViewModel> Banknolist = new List<BankAccountViewModel> ( );
				List<DetailsViewModel> NewDetlist = new List<DetailsViewModel> ( );
				GetExportRecords getExportrec = new GetExportRecords ( CurrentDb, ref recs, ref NewDetlist );

				// load the selection window
				getExportrec . Topmost = true;
				getExportrec . Show ( );

				//int counter = 0;
				//if ( NewDetlist . Count > 0 )
				//{
				//	// NewDetlist contains the data for the selected records in DetailsViewModel format

				//	// Get new structure for our data comparison
				//	Utils . bankrec brec = new Utils . bankrec ( );

				//	// Load a fresh copy of the Details Data
				//	DataTable dtdetails = new DataTable ( );
				//	DetailsViewModel dvm = new DetailsViewModel ( );
				//	DetCollection OriginalDetcollection = new DetCollection ( );

				//	//Get full Detail data into a DataTable
				//	dtdetails = DetailCollection . LoadDetailsDirect ( dtdetails );
				//	// Get a Details Collection  from the DataTable above
				//	OriginalDetcollection = DetailCollection . LoadDetailsCollectionDirect ( OriginalDetcollection, dtdetails );

				//	int index = 0;
				//	DetCollection DetUpdatecollection = new DetCollection ( );

				//	// iterate thru our new, full copy of the Details Collection checking for any duplicates
				//	// by matching the records in our selected recorde table -( NewDetList)
				//	do
				//	{
				//		foreach ( var item in OriginalDetcollection )
				//		{
				//			Debug . WriteLine ( $"{item . CustNo},  NewDetList {NewDetlist [ counter ] . CustNo}  -  {NewDetlist [ counter ] . BankNo}" );
				//			//item is the ORIGINAL DATA Record;
				//			if ( item . CustNo == NewDetlist [ counter ] . CustNo . ToString ( ) && item . BankNo == NewDetlist [ counter ] . BankNo . ToString ( ) )
				//			{
				//				// already in the receiving Db. so ignore it by removing it from the int list<int>
				//				// AFTER we save its details to our update Details Collection
				//				//								Banknolist . RemoveAt ( counter );
				//				NewDetlist . RemoveAt ( counter );
				//				// We need to restart the iteration thru Original Bank records in case it cae up BEFORE this one !!
				//				break;
				//			}
				//			else if ( item . CustNo == NewDetlist [ counter ] . CustNo . ToString ( ) && item . BankNo != NewDetlist [ counter ] . BankNo . ToString ( ) )
				//			{
				//				DetUpdatecollection . Add ( NewDetlist [ counter ] );
				//				counter++;
				//				break;
				//			}
				//			index++;
				//		}
				//		if ( counter >= NewDetlist . Count )
				//			break;
				//	} while ( true );

				//	// See if we have any records left to add to Details Db
				//	if ( DetUpdatecollection . Count > 0 )
				//	{
				//		// Yes, so go ahead nd add the to the DetailsDb

				//		foreach ( var item in DetUpdatecollection )
				//		{
				//			ImportDbData . InsertSingleDetailsRecord ( item );
				//		}
				//	}
				//	else
				//	{
				//		Debug . WriteLine ( $"No Records added, they already exist in the destination Db....." );
				//	}
				//}
			}
			if ( CurrentDb == "DETAILS" )
			{
				if ( SqlDetcollection == null )
					return;
				List<DetailsViewModel> recs = new List<DetailsViewModel> ( );
				//select All the items first;
				var accounts = from items in SqlDetcollection orderby items . CustNo, items . BankNo select items;
				//Next Group collection on CustNo
				var grouped = accounts . GroupBy ( b => b . CustNo );

				//Now filter content down to only those a/c's with multiple Bank A/c's
				var sel = from g in grouped
					  where g . Count ( ) > 1
					  select g;

				// Finally, iterate thru the list of grouped CustNo's matching to CustNo in the full BankAccounts data
				// giving us ONLY the full records for any records that have > 1 Bank accounts
				List<DetailsViewModel> outputt = new List<DetailsViewModel> ( );

				foreach ( var item1 in sel )
				{
					foreach ( var item2 in accounts )
					{
						if ( item2 . CustNo . ToString ( ) == item1 . Key )
						{
							recs . Add ( item2 );
						}
					}
				}
				List<DetailsViewModel> Detnolist = new List<DetailsViewModel> ( );
				List<BankAccountViewModel> NewBanklist = new List<BankAccountViewModel> ( );
				GetExportRecords getExportrec = new GetExportRecords ( CurrentDb, ref NewBanklist, ref recs );

				// load the selection window
				getExportrec . Show ( );

				return;




				//if ( DialogResult == false )
				//	return;

				//int counter = 0;
				//if ( NewBanklist . Count > 0 )
				//{
				//	// NewBanklist contains the data for the selected records in BankAccountViewModel format

				//	// Get new structure for our data comparison
				//	Utils . bankrec brec = new Utils . bankrec ( );

				//	// Load a fresh copy of the Details Data
				//	DataTable dtbank = new DataTable ( );
				//	BankAccountViewModel bvm = new BankAccountViewModel ( );
				//	BankCollection OriginalBankcollection = new BankCollection ( );

				//	//Get full Bank data into a DataTable
				//	dtbank = BankCollection . LoadBankDirect ( dtbank );
				//	// Get a Details Collection  from the DataTable above
				//	OriginalBankcollection = BankCollection . LoadBankCollectionDirect ( OriginalBankcollection, dtbank );

				//	int index = 0;
				//	BankCollection BankUpdatecollection = new BankCollection ( );

				//	// iterate thru our new, full copy of the Details Collection checking for any duplicates
				//	// by matching the records in our selected recorde table -( NewDetList)
				//	do
				//	{
				//		foreach ( var item in OriginalBankcollection )
				//		{
				//			Debug . WriteLine ( $"{item . CustNo},  NewDetList {NewBanklist [ counter ] . CustNo}  -  {NewBanklist [ counter ] . BankNo}" );
				//			//item is the ORIGINAL DATA Record;
				//			if ( item . CustNo == NewBanklist [ counter ] . CustNo . ToString ( ) && item . BankNo == NewBanklist [ counter ] . BankNo . ToString ( ) )
				//			{
				//				// already in the receiving Db. so ignore it by removing it from the int list<int>
				//				// AFTER we save its details to our update Details Collection
				//				//								Detnolist. RemoveAt ( counter );
				//				NewBanklist . RemoveAt ( counter );
				//				// We need to restart the iteration thru Original Bank records in case it cae up BEFORE this one !!
				//				break;
				//			}
				//			else if ( item . CustNo == NewBanklist [ counter ] . CustNo . ToString ( ) && item . BankNo != NewBanklist [ counter ] . BankNo . ToString ( ) )
				//			{
				//				BankUpdatecollection . Add ( NewBanklist [ counter ] );
				//				counter++;
				//				break;
				//			}
				//			index++;
				//		}
				//		if ( counter >= NewBanklist . Count )
				//			break;
				//	} while ( true );

				//	// See if we have any records left to add to Details Db
				//	if ( BankUpdatecollection . Count > 0 )
				//	{
				//		// Yes, so go ahead nd add the to the DetailsDb

				//		foreach ( var item in BankUpdatecollection )
				//		{
				//			ImportDbData . InsertSingleBankRecord ( item );
				//		}
				//	}
				//	else
				//	{
				//		Debug . WriteLine ( $"No Records added, they already exist in the destination Db....." );
				//	}
				//}
			}
		}
		/// <summary>
		/// Triggered by an event to update aftermutli db transfer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EventControl_TransferDataUpdated ( object sender, LoadedEventArgs e )
		{
			if ( e . CallerDb == "BANKACCOUNT" && CurrentDb == "DETAILS" )
			{
				// We are the DETAILS Window, and are receiving DETAILS records for inclusion in our db
				int counter = 0;
				List<DetailsViewModel> NewDetlist = e . DataSource as List<DetailsViewModel>;
				if ( NewDetlist . Count > 0 )
				{
					// NewDetlist contains the data for the selected records in DetailsViewModel format

					// Get new structure for our data comparison
					Utils . bankrec brec = new Utils . bankrec ( );

					// Load a fresh copy of the Details Data
					DataTable dtdetails = new DataTable ( );
					DetailsViewModel dvm = new DetailsViewModel ( );
					DetCollection OriginalDetcollection = new DetCollection ( );

					//Get full Detail data into a DataTable
					dtdetails = DetailCollection . LoadDetailsDirect ( dtdetails );
					// Get a Details Collection  from the DataTable above
					OriginalDetcollection = DetailCollection . LoadDetailsCollectionDirect ( OriginalDetcollection, dtdetails );

					int index = 0;
					DetCollection DetUpdatecollection = new DetCollection ( );

					// iterate thru our new, full copy of the Details Collection checking for any duplicates
					// by matching the records in our selected recorde table -( NewDetList)
					do
					{
						foreach ( var item in OriginalDetcollection )
						{
							//							Debug . WriteLine ( $"{item . CustNo},  NewDetList {NewDetlist [ counter ] . CustNo}  -  {NewDetlist [ counter ] . BankNo}" );
							//item is the ORIGINAL DATA Record;
							if ( item . CustNo == NewDetlist [ counter ] . CustNo . ToString ( ) && item . BankNo == NewDetlist [ counter ] . BankNo . ToString ( ) )
							{
								// already in the receiving Db. so ignore it by removing it from the int list<int>
								// AFTER we save its details to our update Details Collection
								//								Banknolist . RemoveAt ( counter );
								NewDetlist . RemoveAt ( counter );
								// We need to restart the iteration thru Original Bank records in case it cae up BEFORE this one !!
								break;
							}
							else if ( item . CustNo == NewDetlist [ counter ] . CustNo . ToString ( ) && item . BankNo != NewDetlist [ counter ] . BankNo . ToString ( ) )
							{
								DetUpdatecollection . Add ( NewDetlist [ counter ] );
								counter++;
								break;
							}
							index++;
						}
						if ( counter >= NewDetlist . Count )
							break;
						if ( index > OriginalDetcollection . Count )
							break;
					} while ( true );

					if ( NewDetlist . Count > 0 && DetUpdatecollection . Count == 0 )
					{
						// found NO MATCHES AT ALL, so add them all to our db
						foreach ( var item in NewDetlist )
						{
							ImportDbData . InsertSingleDetailsRecord ( item );
						}
					}
					else
					{
						// See if we have any records left to add to Details Db
						if ( DetUpdatecollection . Count > 0 )
						{
							// Yes, so go ahead nd add the to the DetailsDb

							foreach ( var item in DetUpdatecollection )
							{
								ImportDbData . InsertSingleDetailsRecord ( item );
							}

						}
						else
						{
							Debug . WriteLine ( $"No Records added, they already exist in the destination Db....." );
							MessageBox . Show ( $"The Details Db already has the selected Account records\nso no additions have been made.", "Update information" );
						}
					}
				}
			}
			else if ( e . CallerDb == "DETAILS" && CurrentDb == "BANKACCOUNT" )
			{
				int counter = 0;
				List<BankAccountViewModel> NewBanklist = e . DataSource as List<BankAccountViewModel>;
				if ( NewBanklist . Count > 0 )
				{
					// NewBanklist contains the data for the selected records in BankAccountViewModel format

					// Get new structure for our data comparison
					Utils . bankrec brec = new Utils . bankrec ( );

					// Load a fresh copy of the Details Data
					DataTable dtbank = new DataTable ( );
					BankAccountViewModel bvm = new BankAccountViewModel ( );
					BankCollection OriginalBankcollection = new BankCollection ( );

					//Get full Bank data into a DataTable
					dtbank = BankCollection . LoadBankDirect ( dtbank );
					// Get a Details Collection  from the DataTable above
					Flags . SqlBankActive  = true;
					OriginalBankcollection = BankCollection . LoadBankCollectionDirect ( OriginalBankcollection, dtbank );

					int index = 0;
					BankCollection BankUpdatecollection = new BankCollection ( );

					// iterate thru our new, full copy of the Details Collection checking for any duplicates
					// by matching the records in our selected recorde table -( NewDetList)
					do
					{
						foreach ( var item in OriginalBankcollection )
						{
							Debug . WriteLine ( $"{item . CustNo},  NewDetList {NewBanklist [ counter ] . CustNo}  -  {NewBanklist [ counter ] . BankNo}" );
							//item is the ORIGINAL DATA Record;
							if ( item . CustNo == NewBanklist [ counter ] . CustNo . ToString ( ) && item . BankNo == NewBanklist [ counter ] . BankNo . ToString ( ) )
							{
								// already in the receiving Db. so ignore it by removing it from the int list<int>
								// AFTER we save its details to our update Details Collection
								//								Detnolist. RemoveAt ( counter );
								NewBanklist . RemoveAt ( counter );
								// We need to restart the iteration thru Original Bank records in case it cae up BEFORE this one !!
								break;
							}
							else if ( item . CustNo == NewBanklist [ counter ] . CustNo . ToString ( ) && item . BankNo != NewBanklist [ counter ] . BankNo . ToString ( ) )
							{
								BankUpdatecollection . Add ( NewBanklist [ counter ] );
								counter++;
								break;
							}
							index++;
						}
						if ( counter >= NewBanklist . Count )
							break;
						if ( index > BankUpdatecollection . Count )
							break;
					} while ( true );

					if ( NewBanklist . Count > 0 && BankUpdatecollection . Count == 0 )
					{
						// found NO MATCHES AT ALL, so add them all to our db
						foreach ( var item in NewBanklist )
						{
							ImportDbData . InsertSingleBankRecord ( item );
						}
					}
					else
					{
						// See if we have any records left to add to Details Db
						if ( BankUpdatecollection . Count > 0 )
						{
							// Yes, so go ahead nd add the to the DetailsDb

							foreach ( var item in BankUpdatecollection )
							{
								ImportDbData . InsertSingleBankRecord ( item );
							}
						}
						else
						{
							Debug . WriteLine ( $"No Records added, they already exist in the destination Db....." );
						}
					}
				}
			}
		}

		private void ResetMenuBarStatus ( )
		{
			return;
		}



		private void Exportany_Click ( object sender, RoutedEventArgs e )
		{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				if ( SqlBankcollection == null )
					return;
				List<BankAccountViewModel> recs = new List<BankAccountViewModel> ( );
				//select All the items first;
				recs = SqlBankcollection . ToList ( );
				//				OrderBy ( CustNo => CustNo ) .
				//recs . Sort ( );
				List<BankAccountViewModel> Banknolist = new List<BankAccountViewModel> ( );
				List<DetailsViewModel> NewDetlist = new List<DetailsViewModel> ( );
				GetExportRecords getExportrec = new GetExportRecords ( CurrentDb, ref recs, ref NewDetlist );

				// load the selection window
				getExportrec . Topmost = true;
				getExportrec . Show ( );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				if ( SqlDetcollection == null )
					return;
				// Get a new List sorted by BankNo within CustNo
				List<DetailsViewModel> recs = SqlDetcollection . OrderBy ( DetailsViewModel => DetailsViewModel . CustNo )
					. ThenBy ( DetailsViewModel => DetailsViewModel . BankNo )
					. ToList ( );
				List<BankAccountViewModel> NewBanklist = new List<BankAccountViewModel> ( );
				GetExportRecords getExportrec = new GetExportRecords ( CurrentDb, ref NewBanklist, ref recs );

				// load the selection window
				getExportrec . Topmost = true;
				getExportrec . Show ( );

			}
		}

		#region // DRAG AND DROP STUFF - working now


		/// <summary>
		/// down
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CustomerGrid_MouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{

		}
		private void BankGrid_PreviewDragEnter ( object sender, DragEventArgs e )
		{
			e . Effects = ( DragDropEffects ) DragDropEffects . Move;
			//Debug . WriteLine ( $"Setting drag cursor...." );
		}

		#region DRAG CODE
		private void BankGrid_PreviewMouseLeftButtondown ( object sender, MouseButtonEventArgs e )
		{
			// Gotta make sure it is not anywhere in the Scrollbar we clicked on 
			if ( Utils . HitTestScrollBar ( sender, e ) )
			{
				ScrollBarMouseMove = true;
				return;
			}
			if ( Utils . HitTestHeaderBar ( sender, e ) )
				return;

			_startPoint = e . GetPosition ( null );
			// Make sure the left mouse button is pressed down so we are really moving a record
			if ( e . LeftButton == MouseButtonState . Pressed )
			{
				IsLeftButtonDown = true;
			}

		}

		private void CustomerGrid_PreviewMouseLeftButtondown ( object sender, MouseButtonEventArgs e )
		{
			// Gotta make sure it is not anywhere in the Scrollbar we clicked on 
			if ( Utils . HitTestScrollBar ( sender, e ) )
			{
				ScrollBarMouseMove = true;
				return;
			}
			if ( Utils . HitTestHeaderBar ( sender, e ) )
				return;
			_startPoint = e . GetPosition ( null );
			// Make sure the left mouse button is pressed down so we are really moving a record
			if ( e . LeftButton == MouseButtonState . Pressed )
			{
				IsLeftButtonDown = true;
			}
		}
		private void DetailsGrid_PreviewMouseLeftButtondown ( object sender, MouseButtonEventArgs e )
		{
			// Gotta make sure it is not anywhere in the Scrollbar we clicked on 
			if ( Utils . HitTestScrollBar ( sender, e ) )
			{
				ScrollBarMouseMove = true;
				return;
			}
			if ( Utils . HitTestHeaderBar ( sender, e ) )
				return;
			_startPoint = e . GetPosition ( null );
			// Make sure the left mouse button is pressed down so we are really moving a record
			if ( e . LeftButton == MouseButtonState . Pressed )
			{
				IsLeftButtonDown = true;
			}
		}

		private void Drag_Click ( object sender, RoutedEventArgs e )
		{
			DragDropClient ddc = new DragDropClient ( );
			ddc . Show ( );
		}

		private void BankGrid_PreviewMouseMove ( object sender, MouseEventArgs e )
		{
			Point mousePos = e . GetPosition ( null );
			Vector diff = _startPoint - mousePos;

			if ( e . LeftButton == MouseButtonState . Pressed &&
			    Math . Abs ( diff . X ) > SystemParameters . MinimumHorizontalDragDistance ||
			    Math . Abs ( diff . Y ) > SystemParameters . MinimumVerticalDragDistance )
			{
				if ( IsLeftButtonDown && e . LeftButton == MouseButtonState . Pressed )
				{
					if ( BankGrid . SelectedItem != null )
					{
						if ( ScrollBarMouseMove )
							return;
						// We are dragging from the DETAILS grid
						//Working string version
						BankAccountViewModel bvm = new BankAccountViewModel ( );
						bvm = BankGrid . SelectedItem as BankAccountViewModel;
						string str = GetExportRecords . CreateTextFromRecord ( bvm, null, null, true, false );
						string dataFormat = DataFormats . Text;
						DataObject dataObject = new DataObject ( dataFormat, str );
						System . Windows . DragDrop . DoDragDrop (
						BankGrid,
						dataObject,
						DragDropEffects . Copy );
						IsLeftButtonDown = false;
					}
				}
			}
		}

		private void CustomerGrid_PreviewMouseMove ( object sender, MouseEventArgs e )
		{
			Point mousePos = e . GetPosition ( null );
			Vector diff = _startPoint - mousePos;

			if ( e . LeftButton == MouseButtonState . Pressed &&
			    Math . Abs ( diff . X ) > SystemParameters . MinimumHorizontalDragDistance ||
			    Math . Abs ( diff . Y ) > SystemParameters . MinimumVerticalDragDistance )
			{
				if ( IsLeftButtonDown && e . LeftButton == MouseButtonState . Pressed )
				{
					if ( CustomerGrid . SelectedItem != null )
					{
						// We are dragging from the CUSTOMER  grid
						//Working string version
						CustomerViewModel cvm = new CustomerViewModel ( );
						cvm = CustomerGrid . SelectedItem as CustomerViewModel;
						string str = GetExportRecords . CreateTextFromRecord ( null, null, cvm, true, false );
						string dataFormat = DataFormats . Text;
						DataObject dataObject = new DataObject ( dataFormat, str );
						System . Windows . DragDrop . DoDragDrop (
						CustomerGrid,
						dataObject,
						DragDropEffects . Copy );
						IsLeftButtonDown = false;
					}
				}
			}
		}

		private void DetailsGrid_PreviewMouseMove ( object sender, MouseEventArgs e )
		{
			Point mousePos = e . GetPosition ( null );
			Vector diff = _startPoint - mousePos;

			if ( e . LeftButton == MouseButtonState . Pressed &&
			    Math . Abs ( diff . X ) > SystemParameters . MinimumHorizontalDragDistance ||
			    Math . Abs ( diff . Y ) > SystemParameters . MinimumVerticalDragDistance )
			{
				if ( IsLeftButtonDown && e . LeftButton == MouseButtonState . Pressed )
				{
					DetailsViewModel dvm = new DetailsViewModel ( );
					if ( DetailsGrid . SelectedItem != null )
					{
						// We are dragging from the DETAILS grid
						//Working Record  version

						//string str = GetExportRecords . CreateTextFromRecord ( null, dvm,null, true, false );
						//string dataFormat = DataFormats . Text;
						//DataObject dataObject = new DataObject ( dataFormat, str );
						//System . Windows . DragDrop . DoDragDrop (
						//CustomerGrid,
						//dataObject,
						//DragDropEffects . Copy );

						//return;
						try
						{
							dvm = DetailsGrid . SelectedItem as DetailsViewModel;
							var dataObject = new DataObject ( "DETAILS", dvm );
							System . Windows . DragDrop . DoDragDrop (
							DetailsGrid,
							dataObject,
							DragDropEffects . Copy );
						}
						catch ( Exception ex )
						{
							Debug . WriteLine ( $"Drag DataObject Error :\n{ex . Message}, {ex . Data}" );
						}
						IsLeftButtonDown = false;
					}
				}
			}
		}
		public static string getValidFormat ( object bvm )
		{
			DataObject dataObj = new DataObject ( bvm );
			// Get an array of strings, each string denoting a data format
			// that is available in the data object.  This overload of GetDataFormats
			// accepts a Boolean parameter inidcating whether to include auto-convertible
			// data formats, or only return native data formats.
			string [ ] dataFormats = dataObj . GetFormats ( true /* Include auto-convertible? */);
			// Get the number of native data formats present in the data object.
			int numberOfDataFormats = dataFormats . Length;
			if ( numberOfDataFormats == 1 )
			{
				Console . WriteLine ( $"DataObject : {dataFormats [ 0 ]}" );
				return dataFormats [ 0 ];
			}
			// To enumerate the resulting array of data formats, and take some action when
			// a particular data format is found, use a code structure similar to the following.
			foreach ( string dataFormat in dataFormats )
			{
				Console . WriteLine ( $"DataObjects : {dataFormat}" );
				if ( numberOfDataFormats == 1 )
					return dataFormat;
				//if ( dataFormat == DataFormats . Text )
				//{
				//	// Take some action if/when data in the Text data format is found.
				//	break;
				//}
			}
			return "";
		}

		#endregion DRAG CODE

		private void CustomerGrid_PreviewMouseLeftButtonup ( object sender, MouseButtonEventArgs e )
		{
			ScrollBarMouseMove = false;
		}


		private void BankGrid_PreviewMouseRightButtondown ( object sender, MouseButtonEventArgs e )
		{
			ContextMenu cm = this . FindResource ( "ContextMenu1" ) as ContextMenu;
			cm . PlacementTarget = this . BankGrid as DataGrid;
			cm . IsOpen = true;
		}
		private void CustomerGrid_PreviewMouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{
			ContextMenu cm = this . FindResource ( "ContextMenu1" ) as ContextMenu;
			cm . PlacementTarget = this . CustomerGrid as DataGrid;
			cm . IsOpen = true;
			Brush b = ( Brush ) FindResource ( "Cyan4" );
			cm . Background = b;
			b = ( Brush ) FindResource ( "Black0" );
			cm . Foreground = b;
		}

		private void DetailsGrid_PreviewMouseRightButtondown ( object sender, MouseButtonEventArgs e )
		{
			ContextMenu cm = this . FindResource ( "ContextMenu1" ) as ContextMenu;
			cm . PlacementTarget = this . DetailsGrid as DataGrid;
			cm . IsOpen = true;
		}
		#endregion DRAG

		#region CONTEXT MENU METHODS
		private void ContextSave_Click ( object sender, RoutedEventArgs e )
		{
			//============================================//
			//MENU ITEM 'Save current Grid Db data as JSON File'
			//============================================//
			object DbData = new object ( );
			string resultString = "", path = "";
			string jsonresult = "";
			// Get default text files viewer application from App resources
			string program = ( string ) Properties . Settings . Default [ "DefaultTextviewer" ];

			//HOW to save current Collectionview as a Json (binary) data from disk
			// this is the best way to save persistent data in Json format
			//Save data (XXXXViewModel[]) as binary to disk file
			if ( CurrentDb == "BANKACCOUNT" )
			{
				path = @"C:\\Users\\Ianch\\Documents\\BankCollectiondata.json";
				jsonresult = JsonConvert . SerializeObject ( SqlBankcollection );
				JsonSupport . JsonSerialize ( jsonresult, path );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				path = @"C:\\Users\\Ianch\\Documents\\CustomerCollectiondata.json";
				jsonresult = JsonConvert . SerializeObject ( SqlCustcollection );
				JsonSupport . JsonSerialize ( jsonresult, path );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				path = @"C:\\Users\\Ianch\\Documents\\detailsCollectiondata.json";
				jsonresult = JsonConvert . SerializeObject ( SqlDetcollection );
				JsonSupport . JsonSerialize ( jsonresult, path );
			}
			MessageBox . Show ( $"The data from this Database has been saved\nfor you in 'Json' format successfully ...\n\nFile is : {path}", "Data Persistence System" );
		}

		private void ContextEdit_Click ( object sender, RoutedEventArgs e )
		{
			//============================================//
			//MENU ITEM 'Edit currently Selected Account'
			//============================================//
			int currsel = 0;
			RowInfoPopup rip = ( RowInfoPopup ) null;
			// handle flags to let us know WE have triggered the selectedIndex change
			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			DataGridRow RowData = new DataGridRow ( );
			if ( CurrentDb == "BANKACCOUNT" )
			{
				bvm = this . BankGrid . SelectedItem as BankAccountViewModel;
				currsel = this . BankGrid . SelectedIndex;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				cvm = this . CustomerGrid . SelectedItem as CustomerViewModel;
				currsel = this . CustomerGrid . SelectedIndex;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				dvm = this . DetailsGrid . SelectedItem as DetailsViewModel;
				currsel = this . DetailsGrid . SelectedIndex;
			}
			//int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
			//if ( row == -1 ) row = 0;
			if ( CurrentDb == "BANKACCOUNT" )
				rip = new RowInfoPopup ( "BANKACCOUNT", BankGrid );
			else if ( CurrentDb == "CUSTOMER" )
				rip = new RowInfoPopup ( "CUSTOMER", CustomerGrid );
			else if ( CurrentDb == "DETAILS" )
				rip = new RowInfoPopup ( "DETAILS", DetailsGrid );
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
				if ( CurrentDb == "BANKACCOUNT" )
				{
					this . BankGrid . Items . Clear ( );
					// Save our reserve collection
					BankReserved = null;
					Flags . SqlBankActive  = true;
					BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );
					this . BankGrid . ItemsSource = SqlBankcollection;
				}
				else if ( CurrentDb == "CUSTOMER" )
				{
					this . CustomerGrid . Items . Clear ( );
					// Save our reserve collection
					CustReserved = null;
					Flags . SqlCustActive  = true;
					CustCollection . LoadCust ( SqlCustcollection, "SQLDBVIEWER", 1, true );
					this . CustomerGrid . ItemsSource = SqlCustcollection;
				}
				else if ( CurrentDb == "DETAILS" )
				{
					this . DetailsGrid . Items . Clear ( );
					// Save our reserve collection
					BankReserved = null;
					Flags . SqlDetActive  = true;
					DetailCollection . LoadDet ("DETAILS",1, true );
					//this . DetailsGrid . ItemsSource = SqlDetcollection;
				}
				StatusBar . Text = "Current Record Updated Successfully...";
				// Notify everyone else of the data change
				EventControl . TriggerViewerDataUpdated ( SqlBankcollection,
					new LoadedEventArgs
					{
						CallerType = "SQLDBVIEWER",
						CallerDb = "BANKACCOUNT",
						DataSource = SqlBankcollection,
						SenderGuid = this . Tag . ToString ( ),
						RowCount = this . BankGrid . SelectedIndex
					} );
				EventControl . TriggerGlobalDataChanged ( this, new GlobalEventArgs
				{
					CallerType = "SQLDBVIEWER",
					AccountType = "DETAILS",
					SenderGuid = this . Tag?.ToString ( )
				} );
			}
			else
				this . BankGrid . SelectedItem = RowData . Item;

			// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
			if ( CurrentDb == "BANKACCOUNT" )
				this . BankGrid . SelectedIndex = currsel;
			else if ( CurrentDb == "CUSTOMER" )
				this . CustomerGrid . SelectedIndex = currsel;
			else if ( CurrentDb == "DETAILS" )
				this . DetailsGrid . SelectedIndex = currsel;

			Utils . SetUpGridSelection ( this . BankGrid, this . BankGrid . SelectedIndex );
			ParseButtonText ( true );
			Count . Text = $"{Utils . GetPrettyGridStatistics ( this . BankGrid, this . BankGrid . SelectedIndex )}";
			//				Count . Text = $"{this . BankGrid . SelectedIndex} / { this . BankGrid . Items . Count . ToString ( )}";
			//				Count . Text = this . BankGrid . Items . Count . ToString ( );
			// This is essential to get selection activated again
			this . BankGrid . Focus ( );
		}

		private void ContextClose_Click ( object sender, RoutedEventArgs e )
		{
			//============================================//
			//MENU ITEM 'Exit this Viewer'
			//============================================//
			CloseViewer_Click ( sender, e );
		}
		/// <summary>
		/// Show the content of the currently  selected record in Standard Json format
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ContextShowJson_Click ( object sender, RoutedEventArgs e )
		{
			//============================================//
			//	'View currently selected Record in JSON Format'
			//============================================//
			if ( CurrentDb == "BANKACCOUNT" )
			{
				// Works fine !!! 22/6/21
				// grab current record and  convert it to a Json record
				bvm = this . BankGrid . SelectedItem as BankAccountViewModel;
				JObject obj = JObject . FromObject ( bvm );
				string s = JsonConvert . SerializeObject ( new
				{
					obj
				} );
				// we have our string in 's'
				// // show it in a messagebox fully formatted				
				int rows = 0;
				string jsonstring = JsonSupport . CreateFormattedJsonOutput ( s, "BankAccount" );
				//				string jsonstring = tmp . Result;
				MessageBox . Show ( jsonstring, "Json formatted record data" );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				cvm = this . CustomerGrid . SelectedItem as CustomerViewModel;
				JObject obj = JObject . FromObject ( cvm );
				string s = JsonConvert . SerializeObject ( new
				{
					obj
				} );
				// we have our string in 's'
				// // show it in a messagebox fully formatted				
				int rows = 0;
				string jsonstring = JsonSupport . CreateFormattedJsonOutput ( s, "Customer" );
				MessageBox . Show ( jsonstring, "Json formatted record data" );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				dvm = this . DetailsGrid . SelectedItem as DetailsViewModel;
				JObject obj = JObject . FromObject ( dvm );
				string s = JsonConvert . SerializeObject ( new
				{
					obj
				} );
				// we have our string in 's'
				// // show it in a messagebox fully formatted				
				int rows = 0;
				string jsonstring = JsonSupport . CreateFormattedJsonOutput ( s, "Details" );
				MessageBox . Show ( jsonstring, "Json formatted record data" );
			}
		}

		private void ContextDisplayJsonData_Click ( object sender, RoutedEventArgs e )
		{
			//============================================//
			//MENU ITEM 'Read and display JSON File'
			//============================================//
			Mouse . OverrideCursor = Cursors . Wait;
			StatusBar . Text = "Please wait, This process can take a little while !!";
			this . Refresh ( );
			////We need to save current Collectionview as a Json (binary) data to disk
			//// this is the best way to save persistent data in Json format
			////using tmp folder for interim file that we will then display
			if ( CurrentDb == "BANKACCOUNT" )
				JsonSupport . CreateShowJsonText ( false, CurrentDb, SqlBankcollection, "BankAccountViewModel" );
			else if ( CurrentDb == "CUSTOMER" )
				JsonSupport . CreateShowJsonText ( false, CurrentDb, SqlCustcollection, "CustomerViewModel" );
			else if ( CurrentDb == "DETAILS" )
				JsonSupport . CreateShowJsonText ( false, CurrentDb, SqlDetcollection, "DetailsViewModel" );

		}

		private void Settings_Click ( object sender, RoutedEventArgs e )
		{
			ContextSettings_Click ( sender, e );
		}

		private void ContextSettings_Click ( object sender, RoutedEventArgs e )
		{
			Setup setup = new Setup ( );
			setup . Show ( );
			setup . BringIntoView ( );
			setup . Topmost = true;
			this . Focus ( );
		}
		#endregion CONTEXT MENU METHODS

		#region JSON support function
		#endregion JSON support function

		private void xxxxx ( object sender, RoutedEventArgs e )
		{

		}

		private void ViewJsonRecord_Click ( object sender, RoutedEventArgs e )
		{
			//============================================//
			//MENU ITEM 'Read and display JSON File'
			//============================================//
			string Output = "";
			Mouse . OverrideCursor = Cursors . Wait;
			StatusBar . Text = "Please wait, This process can take a little while !!";
			this . Refresh ( );
			////We need to save current Collectionview as a Json (binary) data to disk
			//// this is the best way to save persistent data in Json format
			////using tmp folder for interim file that we will then display
			if ( CurrentDb == "BANKACCOUNT" )
			{
				BankAccountViewModel bvm = this . BankGrid . SelectedItem as BankAccountViewModel;
				Output = JsonSupport . CreateShowJsonText ( true, CurrentDb, bvm, "BankAccountViewModel" );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				CustomerViewModel bvm = this . CustomerGrid . SelectedItem as CustomerViewModel;
				Output = JsonSupport . CreateShowJsonText ( true, CurrentDb, bvm, "CustomerViewModel" );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				DetailsViewModel bvm = this . DetailsGrid . SelectedItem as DetailsViewModel;
				Output = JsonSupport . CreateShowJsonText ( true, CurrentDb, bvm, "DetailsViewModel" );
			}
			MessageBox . Show ( Output, "Currently selected record in JSON format", MessageBoxButton . OK, MessageBoxImage . Information, MessageBoxResult . OK );
		}

		private void Window_PreviewMouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{
			IsRightMouseDown = true;
		}

		/// <summary>
		/// Drag left/right with Right mouse down changes opacity of the windows grid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_PreviewMouseMove ( object sender, MouseEventArgs e )
		{
			LinearGradientBrush newbrush = new LinearGradientBrush ( );

			SqlDbViewer sql = sender as SqlDbViewer;
			if ( IsRightMouseDown )
			{
				Point newpos = new Point ( );
				newpos = e . GetPosition ( null );
				if ( newpos . X > currentpos . X )
				{
					newbrush = sqlgrid. Background as LinearGradientBrush;
					if ( newbrush . Opacity > 0 )
						newbrush . Opacity -= 0.03;
					if ( BankGrid . Opacity > 0 )
					{
						WaitMessage . Opacity -= 0.5;
						BankGrid . Opacity -= 0.01;
					}
					this . Refresh ( );
				}
				else
				{
					newbrush = sqlgrid . Background as LinearGradientBrush;
					if ( newbrush . Opacity < 1 )
						newbrush . Opacity += 0.03;
					if ( BankGrid . Opacity < 1 )
					{
						WaitMessage . Opacity += 0.5;
						BankGrid . Opacity += 0.01;
					}
					this . Refresh ( );
				}
				currentpos = newpos;
			}
		}

		private void Window_PreviewMouseRightButtonUp ( object sender, MouseButtonEventArgs e )
		{
			IsRightMouseDown = false;
		}

	}
}