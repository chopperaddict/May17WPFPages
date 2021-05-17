using System;
using System . ComponentModel;
using System . Data;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Input;
using System . Windows . Media;

using WPFPages . ViewModels;

namespace WPFPages . Views
{
	//	public delegate void SqlSelChange ( int RowIndex, object selRowItem );

	public delegate void SelectedRowChanged ( int row , string CurentDb );

	public partial class EditDb : INotifyPropertyChanged
	{
		#region CLASS DECLARATIONS

		public static event DbUpdated NotifyOfDataChange;

		public  static BankCollection EditDbBankcollection=new BankCollection();
		public  static CustCollection EditDbCustcollection=new CustCollection();
		public  static DetCollection EditDbDetcollection=new DetCollection();

		public BankAccountViewModel bvm = MainWindow . bvm;
		public CustomerViewModel cvm = MainWindow . cvm;
		public DetailsViewModel dvm = MainWindow . dvm;
		//		public BankCollection Bankcollection  = new BankCollection();

		//		public BankCollection Bankcollection = bcn.Bankcollection;
		//public CustCollection Custcollection = CustCollection . Custcollection;
		//public DetCollection Detcollection = DetCollection.Detcollection;

		public DataChangeArgs dca = new DataChangeArgs ( );
		internal static SqlDbViewer ThisParent = null;

		//flag to let us know we sent the notification
		private bool  EditHasNotifiedOfChange = false;

		private DataTable dt = new DataTable ( );
		private SQLDbSupport sqlsupport = new SQLDbSupport ( );
		private int CurrentIndex = -1;
		private object CurrentItem = null;
		private SqlDbViewer sqldbv = null;
		public DataGrid CurrentGrid = null;
		public string CurrentDb = "";
		public EventHandlers EventHandler = null;
		public BankAccountViewModel Bank;
		private RowInfoPopup rip = null;
		//private bool PopupActive = false;

		//		private static bool EditHasChangedData = true;
		private SQLEditOcurred SqlEditOccurred = HandleSQLEdit;

		//		private EditEventArgs EditArgs = null;
		public Task mainTask = null;
		public static bool SqlUpdating = false;
		public static bool EditStart = false;
		public static bool  Startup = true;
		public static bool  IsDirty = false;

		// Flags to let me handle jupdates to/From SqlViewer
		private int ViewerChangeType = 0;

		//		private int EditChangeType = 0;
		private bool key1 = false;
		public static EditDb ThisWindow;
		private DataGrid dGrid = null;
		#endregion CLASS DECLARATIONS

		#region (TRUE) EVENT CALLBACK Declarations

		// We HAVE to duplicate this from SQLHandlers otherwise it cannot be found despite  being flagged as PPUBLIC
		public static  event EventHandler<DataUpdatedEventArgs> DataUpdated;

		// We HAVE to duplicate this from SQLHandlers otherwise it cannot be found despite  being flagged as PPUBLIC
		public static  event EventHandler<NotifyAllViewersOfUpdateEventArgs >  AllViewersUpdate;

		#endregion (TRUE) EVENT CALLBACK Declarations

		// Trigger Method to be sent when data is updated (in a DbEdit Window)
		public static void OnAllViewersUpdate ( object sender , string CurrentDb )
		{
			if ( AllViewersUpdate != null )
			{
				Console . WriteLine ( $"Broadcasting from OnDataLoaded in SQLHandlers()" );
				AllViewersUpdate?.Invoke ( sender , new NotifyAllViewersOfUpdateEventArgs
				{ CurrentDb = CurrentDb } );
			}
		}

		#region DELEGATE Handlers

		public void EditDbHasChangedIndex ( int a , int b , string s )
		{
			//This gets called whenever we are notified of a change to data in our grid
			int x = 0;
			x++;
			Console . WriteLine ( $"EditDb : Data changed event notification received successfully." );
			this . DataGrid1 . ItemsSource = null;
			this . DataGrid1 . Items . Clear ( );
			this . DataGrid1 . ItemsSource = EditDbBankcollection;
			this . DataGrid1 . Refresh ( );
			Console . WriteLine ( $"EditDbHasChangedIndex has Updated the Grid content to the latest Db collection...." );
		}


		public static void HandleSQLEdit ( object sender , EditEventArgs e )
		{
			//Handler for Datagrid Edit occurred delegate
			Console . WriteLine ( $"\r\nDelegate Recieved in EDITDB (83) Caller={e . Caller}, Index = {e . CurrentIndex},   {e . DataType . ToString ( )} \r\n" );
			//We no have at our disposal (in e) the entire Updated record, including its Index in the sender grid
			// plus we know its Model type from e.Caller (eg"BANKACCOUNT")
			//and we even have a pointer to the datagrid in the sender parameter
			//			int RowToFind = -1;
			if ( ThisWindow != null )
			{
				//only try this if we actually have an EditDb window open
				if ( e . Caller == "BANKACCOUNT" )
				{
				}
			}
		}

		private void EditDb_DataUpdated ( object sender , DataUpdatedEventArgs e )
		{
			// Broadcast the change to a Db to all viewers etc
			OnAllViewersUpdate ( this , CurrentDb );
		}

		#endregion DELEGATE Handlers

		#region CONSTRUCTOR

		public EditDb ( string Caller , int index , object Item , SqlDbViewer sqldb )
		{
			//Get handle to SQLDbViewer Window

			sqldbv = sqldb;
			CurrentIndex = index;
			CurrentItem = Item;
			CurrentDb = Caller;
			InitializeComponent ( );
			// Shows how to setup a gradient background for any control
			SetupBackgroundGradient ( );
			ThisWindow = this;
			ThisParent = sqldb;
			if ( CurrentDb == "BANKACCOUNT" || CurrentDb == "DETAILS" )
			{
				this . Height = 400;
				this . MinHeight = 400;
				if ( CurrentDb == "BANKACCOUNT" )
				{
					dGrid = this . DataGrid1;
					if ( EditDbBankcollection == null || EditDbBankcollection . Count == 0 )
						EditDbBankcollection = BankCollection . LoadBank ( 2 , false );
					this . DataGrid1 . ItemsSource = EditDbDetcollection;
				}
				else
				{
					dGrid = this . DataGrid2;
					if ( EditDbDetcollection == null || EditDbDetcollection . Count == 0 )
						EditDbDetcollection = DetCollection . LoadDet ( EditDbDetcollection , 2 );
					this . DetailsGrid . ItemsSource = EditDbDetcollection;
				}
			}
			else
			{
				this . Height = 640;
				this . MinHeight = 640;
				dGrid = this . DataGrid2;
				if ( EditDbCustcollection == null || EditDbCustcollection . Count == 0 )
					CustCollection . LoadCust ( EditDbCustcollection );
				this . DataGrid2 . ItemsSource = EditDbDetcollection;
			}
			ViewerButton . IsEnabled = false;

		}

		#endregion CONSTRUCTOR

		#region General EventHandlers

		/// <summary>
		/// Callback handler we receive for a db change notification sent by an SqlDbViewer
		/// We have to update our datagrid as relevant
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="Grid"></param>Detcollection
		/// <param name="args"></param>
		public void DbChangedHandler ( SqlDbViewer sender , DataGrid Grid , DataChangeArgs args )
		{
			int currentrow = Grid . SelectedIndex;
			//			if ( sender == ThisParent )
			//		{
			if ( this . DataGrid1 . Items . Count > 0 )
			{
				// refresh our grid
				this . DataGrid1 . ItemsSource = null;
				try
				{ this . DataGrid1 . ItemsSource = CollectionViewSource . GetDefaultView ( EditDbBankcollection ); }
				catch
				{ Console . WriteLine ( $"Error encountered performing :  . GetDefaultView ( Bankcollection " ); }
				this . DataGrid1 . SelectedIndex = currentrow;
			}
			if ( this . DataGrid2 . Items . Count > 0 )
			{
				// refresh our grid
				this . DataGrid2 . ItemsSource = null;
				try
				{ this . DataGrid2 . ItemsSource = CollectionViewSource . GetDefaultView ( EditDbCustcollection ); }
				catch
				{ Console . WriteLine ( $"Error encountered performing :  . GetDefaultView ( Custcollection " ); }
				this . DataGrid2 . SelectedIndex = currentrow;
			}
			if ( this . DetailsGrid . Items . Count > 0 )
			{
				// refresh our grid
				//?					ViewerChangeType = 2;
				this . DetailsGrid . ItemsSource = null;
				try
				{ this . DetailsGrid . ItemsSource = CollectionViewSource . GetDefaultView ( EditDbDetcollection ); }
				catch { Console . WriteLine ( $"Error encountered performing :  . GetDefaultView ( Detcollection " ); }
				this . DetailsGrid . SelectedIndex = currentrow;
			}
			return;
		}

		/// <summary>
		/// We trigger this to tell Sql Viewer to update its grid data after we have changed it.
		/// Created 10 May 2021
		/// </summary>
		/// <param name="selectedindex"></param>
		/// <param name="currentDb"></param>
		/// <param name="selecteditem"></param>
		private void NotifyviewerOfDataChange ( int selectedindex , string currentDb , object selecteditem )
		{
			dca . DbName = CurrentDb;
			dca . SenderName = null;
			Flags . SqlDetViewer . UpdateDetailsOnEditDbChange ( currentDb , selectedindex , selecteditem );
		}

		/// <summary>
		/// We are being notified of a data change, so we can update our own grid.
		/// Created 10 May 2021
		/// </summary>
		/// <param name="currentDb"></param>
		public void UpdateGrid ( string currentDb )
		{
			int currsel = 0;
			if ( currentDb == "BANKACCOUNT" )
			{
				currsel = this . DataGrid1 . SelectedIndex;
				this . DataGrid1 . ItemsSource = null;
				this . DataGrid1 . ItemsSource = EditDbBankcollection;
				this . DataGrid1 . SelectedIndex = Flags . SqlBankCurrentIndex;
			}
			if ( currentDb == "CUSTOMER" )
			{
				currsel = this . DataGrid2 . SelectedIndex;
				this . DataGrid2 . ItemsSource = null;
				this . DataGrid2 . ItemsSource = EditDbCustcollection;
				this . DataGrid2 . SelectedIndex = currsel;
			}
			if ( currentDb == "DETAILS" )
			{
				currsel = this . DetailsGrid . SelectedIndex;
				this . DetailsGrid . ItemsSource = null;
				this . DetailsGrid . ItemsSource = EditDbDetcollection;
				this . DetailsGrid . SelectedIndex = currsel;
			}
		}

		/// <summary>
		/// SqlViewerGridChanged
		///  A CallBack that RECEIVES notifications from SqlDbViewer on SelectIndex changes or data updates
		///  so that we can update our row position to match. It sends 1  for index or 2  for data change in (int DbChangeTpe)
		/// DELEGATE USED is : SQLViewerSelectionChanged(bool DbEditIndexChangeOnly, int row, string CurrentDb);
		///  EVENT = public static event SqlViewerGridChanged
		/// SendViewerIndexChange ( index, CurrentDb );
		/// </summary>
		/// <param name="row"></param>
		/// <param name="CurrentDb"></param>
		public void OnSqlViewerGridChanged ( int DbChangeType , int row , string CurrentDb )
		{
			// Received a notification from Viewer of a change
			// Set Form wide flags to see what  actions we need to take ?  (Edited value or just index change)
			ViewerChangeType = DbChangeType;        // Change of some type  made in Viewer
									    //EditChangeType = 0;     // We are being notified by viewer, so clear ourOWN  control flag but set the flags for ViewerChangeType
			if ( CurrentDb == "BANKACCOUNT" )
			{
				//				if ( DbChangeType == 2 )
				if ( ViewerChangeType == 2 )
				{
					this . DataGrid1 . ItemsSource = null;
					this . DataGrid1 . ItemsSource = EditDbBankcollection;
					//					this . DataGrid1 . ItemsSource = bvm . BankAccountObs;
				}
				this . DataGrid1 . SelectedItem = null;  //Clear current selection to avoid multiple selections
				this . DataGrid1 . SelectedIndex = row;
				this . DataGrid1 . SelectedItem = row;
				if ( this . DataGrid1 . SelectedItem != null )
					this . DataGrid1 . ScrollIntoView ( this . DataGrid1 . SelectedItem );
				this . DataGrid1 . Refresh ( );
				BankEditFields . DataContext = this . DataGrid1 . SelectedItem;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				//				if ( DbChangeType == 2 )
				if ( ViewerChangeType == 2 )
				{
					this . DataGrid2 . ItemsSource = null;
					this . DataGrid2 . ItemsSource = EditDbCustcollection;
				}
				this . DataGrid2 . SelectedItem = null;    //Clear current selection to avoid multiple selections
				this . DataGrid2 . SelectedIndex = row;
				this . DataGrid2 . SelectedItem = row;
				if ( this . DataGrid2 . SelectedItem != null )
					this . DataGrid2 . ScrollIntoView ( this . DataGrid2 . SelectedItem );
				this . DataGrid2 . Refresh ( );
				CustomerEditFields . DataContext = this . DataGrid2 . SelectedItem;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				if ( ViewerChangeType == 2 )
				{
					// Need to update our data cos SqlDbViewer has changed it
					this . DetailsGrid . ItemsSource = null;
					this . DetailsGrid . ItemsSource = EditDbDetcollection;
				}
				//				this . DetailsGrid . SelectedItem = null;  //Clear current selection to avoid multiple selections

				// This triggers a call  to DetailsGrid_SelectionChanged in this file
				this . DetailsGrid . SelectedIndex = row;
				this . DetailsGrid . SelectedItem = row;
				if ( this . DetailsGrid . SelectedItem != null )
					this . DetailsGrid . ScrollIntoView ( this . DetailsGrid . SelectedItem );
				this . DetailsGrid . Refresh ( );
				DetailsEditFields . DataContext = this . DetailsGrid . SelectedItem;
			}
			// Reset flags
			EditStart = false;
			ViewerButton . IsEnabled = EditStart;
			ViewerChangeType = 0;
		}


		//	We may have changed the selected item, so Trigger event to notify SqlDbViewer (and any other viewers ) of index changes
		private void NotifyViewerofEditIndexChange ( int EditHasChangedData , int row , string CurentDb )
		//	//NOT IN USE
		{
		}
		#endregion General EventHandlers



		#region Display utilities

		private void SetupBackgroundGradient ( )
		{
			//Get a new LinearGradientBrush
			LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush ( );
			//Set the start and end points of the drawing
			myLinearGradientBrush . StartPoint = new Point ( 1.3 , 0 );
			myLinearGradientBrush . EndPoint = new Point ( 0.0 , 1 );
			if ( CurrentDb == "BANKACCOUNT" )
			// Gradient Stops below are light to dark
			{
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . PowderBlue , 1.0 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . LightSteelBlue , 0.5 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . DodgerBlue , 0 ) );
				//Now paint the buttons to match
				LinearGradientBrush myLinearGradientBrush2 = new LinearGradientBrush ( );
				myLinearGradientBrush2 . StartPoint = new Point ( 0.5 , 0 );
				myLinearGradientBrush2 . EndPoint = new Point ( 0 , 1 );

				GradientStop gs1 = new GradientStop ( );
				GradientStop gs2 = new GradientStop ( );
				GradientStop gs3 = new GradientStop ( );
				GradientStop gs4 = new GradientStop ( );
				GradientStop gs5 = new GradientStop ( );
				GradientStop gs6 = new GradientStop ( );
				gs1 . Color = Color . FromArgb ( 0xFF , 0x6B , 0x8E , 0x95 );
				gs2 . Color = Color . FromArgb ( 0xFF , 0x14 , 0xA7 , 0xC1 );
				gs3 . Color = Color . FromArgb ( 0xFF , 0x1E , 0x42 , 0x4E );
				gs4 . Color = Color . FromArgb ( 0xFF , 0x1D , 0x48 , 0x55 );
				gs5 . Color = Color . FromArgb ( 0xFF , 0x1D , 0x48 , 0x55 );
				gs6 . Color = Color . FromArgb ( 0xFF , 0x19 , 0x3A , 0x44 );
				gs1 . Offset = 1;
				gs2 . Offset = 0.509;
				gs3 . Offset = 0.542;
				gs4 . Offset = 0.542;
				gs5 . Offset = 0.526;
				gs6 . Offset = 0;
				myLinearGradientBrush2 . GradientStops . Add ( gs1 );
				myLinearGradientBrush2 . GradientStops . Add ( gs2 );
				myLinearGradientBrush2 . GradientStops . Add ( gs3 );
				myLinearGradientBrush2 . GradientStops . Add ( gs4 );
				myLinearGradientBrush2 . GradientStops . Add ( gs5 );
				myLinearGradientBrush2 . GradientStops . Add ( gs6 );
			}
			if ( CurrentDb == "CUSTOMER" )
			{
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . White , 1.0 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . Gold , 0.3 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . DarkKhaki , 0.0 ) );
				//Now paint the buttons to match
				LinearGradientBrush myLinearGradientBrush2 = new LinearGradientBrush ( );
				myLinearGradientBrush2 . StartPoint = new Point ( 0.5 , 0 );
				myLinearGradientBrush2 . EndPoint = new Point ( 0.5 , 1 );

				GradientStop gs1 = new GradientStop ( );
				GradientStop gs2 = new GradientStop ( );
				GradientStop gs3 = new GradientStop ( );
				GradientStop gs4 = new GradientStop ( );
				GradientStop gs5 = new GradientStop ( );
				GradientStop gs6 = new GradientStop ( );
				//Yellow buttons
				gs1 . Color = Color . FromArgb ( 0xFF , 0x7a , 0x6f , 0x2d );
				gs2 . Color = Color . FromArgb ( 0xFF , 0xf5 , 0xd8 , 0x16 );
				gs3 . Color = Color . FromArgb ( 0xFF , 0x7d , 0x70 , 0x15 );
				gs4 . Color = Color . FromArgb ( 0xFF , 0x5e , 0x56 , 0x2a );
				gs5 . Color = Color . FromArgb ( 0xFF , 0x59 , 0x50 , 0x13 );
				gs6 . Color = Color . FromArgb ( 0xFF , 0x38 , 0x32 , 0x0c );
				gs1 . Offset = 1;
				gs2 . Offset = 0.209;
				gs3 . Offset = 0.342;
				gs4 . Offset = 0.442;
				gs5 . Offset = 0.526;
				gs6 . Offset = 0;
				myLinearGradientBrush2 . GradientStops . Add ( gs1 );
				myLinearGradientBrush2 . GradientStops . Add ( gs2 );
				myLinearGradientBrush2 . GradientStops . Add ( gs3 );
				myLinearGradientBrush2 . GradientStops . Add ( gs4 );
				myLinearGradientBrush2 . GradientStops . Add ( gs5 );
				myLinearGradientBrush2 . GradientStops . Add ( gs6 );
			}
			if ( CurrentDb == "DETAILS" )
			{
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . White , 1.0 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . Green , 0.5 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . DarkGreen , 0.25 ) );
				//Now paint the buttons to match
				LinearGradientBrush myLinearGradientBrush2 = new LinearGradientBrush ( );
				myLinearGradientBrush2 . StartPoint = new Point ( 0.5 , 0 );
				myLinearGradientBrush2 . EndPoint = new Point ( 0.5 , 1 );

				GradientStop gs1 = new GradientStop ( );
				GradientStop gs2 = new GradientStop ( );
				GradientStop gs3 = new GradientStop ( );
				GradientStop gs4 = new GradientStop ( );
				GradientStop gs5 = new GradientStop ( );
				GradientStop gs6 = new GradientStop ( );
				gs1 . Color = Color . FromArgb ( 0xFF , 0x75 , 0xDD , 0x75 );
				gs2 . Color = Color . FromArgb ( 0xFF , 0x00 , 0xFF , 0x00 );
				gs3 . Color = Color . FromArgb ( 0xFF , 0x33 , 0x66 , 0x33 );
				gs4 . Color = Color . FromArgb ( 0xFF , 0x44 , 0x55 , 0x44 );
				gs5 . Color = Color . FromArgb ( 0xFF , 0x33 , 0x55 , 0x55 );
				gs6 . Color = Color . FromArgb ( 0xff , 0x22 , 0x40 , 0x22 );
				gs1 . Offset = 1;
				gs2 . Offset = 0.509;
				gs3 . Offset = 0.542;
				gs4 . Offset = 0.542;
				gs5 . Offset = 0.526;
				gs6 . Offset = 0;
				myLinearGradientBrush2 . GradientStops . Add ( gs1 );
				myLinearGradientBrush2 . GradientStops . Add ( gs2 );
				myLinearGradientBrush2 . GradientStops . Add ( gs3 );
				myLinearGradientBrush2 . GradientStops . Add ( gs4 );
				myLinearGradientBrush2 . GradientStops . Add ( gs5 );
				myLinearGradientBrush2 . GradientStops . Add ( gs6 );
			}
			// Use the brush to paint the rectangle.
			Background = myLinearGradientBrush;
		}

		#endregion Display utilities

		#region Window Handling Methods
		private void WindowLoaded ( object sender , RoutedEventArgs e )
		{
			// Subscribe to notifications of data changes to SQL data
			if ( CurrentDb == "BANKACCOUNT" )
			{
				this . Height = 400;
				this . MinHeight = 400;
				// Hide all relevant controls
				CustomerLabelsGrid . Visibility = Visibility . Collapsed;
				CustomerEditFields . Visibility = Visibility . Collapsed;
				DetailsEditFields . Visibility = Visibility . Collapsed;
				this . DataGrid2 . Visibility = Visibility . Collapsed;
				this . DetailsGrid . Visibility = Visibility . Collapsed;
				this . DataGrid2 . Visibility = Visibility . Collapsed;

				BankLabels . Visibility = Visibility . Visible;
				BankEditFields . Visibility = Visibility . Visible;
				this . DataGrid1 . Visibility = Visibility . Visible;

				this . Title += " Bank Accounts Db";
				try
				{
					// setup the Data Contexts for THIS type of grid
					this . DataGrid1 . ItemsSource = CollectionViewSource . GetDefaultView ( EditDbBankcollection );
					BankEditFields . DataContext = CollectionViewSource . GetDefaultView ( EditDbBankcollection );
					//				this . DataGrid1 . ItemsSource = CollectionViewSource . GetDefaultView ( bvm . BankAccountObs );
				}
				catch
				{
					Console . WriteLine ( $"Error encountered performing :  . GetDefaultView ( BankCollection . Bankcollection " );
				}
				//			BankEditFields . DataContext = CollectionViewSource . GetDefaultView ( bvm . BankAccountObs );

				this . DataGrid1 . SelectedIndex = 0;
				this . DataGrid1 . SelectedItem = 0;

				//to get it to scroll the record into view we have to go thru this palaver....
				// But it does work, but only puts it on bottom row of viewer
				DataGridNavigation . SelectRowByIndex ( this . DataGrid1 , CurrentIndex , -1 );

				CurrentGrid = this . DataGrid1;
				//Setup the Event handler to notify EditDb viewer of index changes
				Console . WriteLine ( $"EditDb(242) Window just loaded : getting instance of EventHandlers class with this,DataGrid1,\"EDITDB\"" );
				new EventHandlers ( this . DataGrid1 , "EDITDB" , out EventHandler );

				//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
				Flags . CurrentEditDbViewerBankGrid = this . DataGrid1;
				BankAccountViewModel . ActiveEditDbViewer = this . DataGrid1;

				Flags . CurrentEditDbViewer = this;
				Flags . CurrentEditDbViewer . Name = "BankAccount";
				Flags . ActiveEditGrid = this;

				this . DataGrid1 . Focus ( );
				this . DataGrid1 . BringIntoView ( );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				this . Height = 640;
				this . MinHeight = 640;
				// Hide all relevant controls
				BankLabels . Visibility = Visibility . Collapsed;
				BankEditFields . Visibility = Visibility . Collapsed;
				DetailsEditFields . Visibility = Visibility . Collapsed;
				this . DataGrid1 . Visibility = Visibility . Collapsed;
				this . DetailsGrid . Visibility = Visibility . Collapsed;

				CustomerLabelsGrid . Visibility = Visibility . Visible;
				CustomerEditFields . Visibility = Visibility . Visible;
				this . DataGrid2 . Visibility = Visibility . Visible;

				this . Title += " Customer Accounts Db";

				try
				{
					this . DataGrid2 . ItemsSource = CollectionViewSource . GetDefaultView ( EditDbCustcollection );
					this . DataGrid2 . DataContext = CollectionViewSource . GetDefaultView ( EditDbCustcollection );
					CustomerEditFields . DataContext = CollectionViewSource . GetDefaultView ( EditDbCustcollection );
				}
				catch
				{
					Console . WriteLine ( $"Error encountered performing :  . GetDefaultView ( custCollection . custcollection " );
				}
				IsDirty = false;
				this . DataGrid2 . SelectedIndex = CurrentIndex;
				this . DataGrid2 . SelectedItem = CurrentItem;

				//to get it to scroll the record into view we have to go thru this palaver....
				// But it does work, but only puts it on bottom row of viewer
				DataGridNavigation . SelectRowByIndex ( this . DataGrid2 , CurrentIndex , -1 );
				CurrentGrid = this . DataGrid2;
				//Setup the Event handler to notify EditDb viewer of index changes
				Console . WriteLine ( $"EditDb(287) Window just loaded :  getting instance of EventHandlers class with this,DataGrid2,\"EDITDB\"" );
				//				EventHandlers . SetWindowHandles ( this, null, null );
				new EventHandlers ( this . DataGrid2 , "EDITDB" , out EventHandler );
				//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
				Flags . CurrentEditDbViewerCustomerGrid = this . DataGrid2;
				BankAccountViewModel . ActiveEditDbViewer = this . DataGrid2;

				Flags . CurrentEditDbViewer = this;
				Flags . CurrentEditDbViewer . Name = "Customer";
				Flags . ActiveEditGrid = this;

				this . DataGrid2 . Focus ( );
				this . DataGrid2 . BringIntoView ( );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				// Hide all relevant controls
				this . Height = 400;
				this . MinHeight = 400;
				BankEditFields . Visibility = Visibility . Collapsed;
				this . DataGrid1 . Visibility = Visibility . Collapsed;
				CustomerLabelsGrid . Visibility = Visibility . Collapsed;
				CustomerEditFields . Visibility = Visibility . Collapsed;
				this . DataGrid2 . Visibility = Visibility . Collapsed;
				this . DataGrid1 . Visibility = Visibility . Collapsed;

				BankLabels . Visibility = Visibility . Visible;
				DetailsEditFields . Visibility = Visibility . Visible;
				this . DetailsGrid . Visibility = Visibility . Visible;

				this . Title += " Secondary Accounts Db";

				try
				{
					this . DetailsGrid . ItemsSource = CollectionViewSource . GetDefaultView ( EditDbDetcollection );
					DetailsEditFields . DataContext = CollectionViewSource . GetDefaultView ( EditDbDetcollection );
				}
				catch
				{
					Console . WriteLine ( $"Error encountered performing :  . GetDefaultView ( SqlViewerDetcollection " );
				}
				// Set the 2 control flags so that we know we have changed data when we notify other windows
				ViewerChangeType = 2;
				//EditChangeType = 0;
				//EditHasChangedData = false;
				this . DetailsGrid . SelectedIndex = 0;
				this . DetailsGrid . SelectedItem = 0;

				//to get it to scroll the record into view we have to go thru this palaver....
				// But it does work, but only puts it on bottom row of viewer
				DataGridNavigation . SelectRowByIndex ( this . DetailsGrid , CurrentIndex , -1 );
				CurrentGrid = this . DetailsGrid;
				//Setup the Event handler to notify EditDb viewer of index changes
				Console . WriteLine ( $"EditDb(312) Window just loaded :  getting instance of EventHandlers class with this,DataGrid1,\"EDITDB\"" );
				//				EventHandlers . SetWindowHandles ( this, null, null );
				new EventHandlers ( this . DetailsGrid , "DETAILS" , out EventHandler );
				//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
				Flags . CurrentEditDbViewerDetailsGrid = this . DetailsGrid;
				BankAccountViewModel . ActiveEditDbViewer = this . DetailsGrid;

				Flags . CurrentEditDbViewer = this;
				Flags . CurrentEditDbViewer . Name = "Details";
				Flags . ActiveEditGrid = this;
				ViewerButton . IsEnabled = false;
				this . DetailsGrid . Focus ( );
				this . DetailsGrid . BringIntoView ( );
			}

			MainWindow . gv . SqlCurrentEditViewer = this;

			// Set Form wide flags to see what  actions we need to take ?  (Edited value or not)
			ViewerChangeType = 0;        // Change made in Viewer
							     //EditChangeType = 0;     // We have not done anything

			NotifyOfDataChange += DbChangedHandler; // Callback in THIS FILE
			EventControl . ViewerDataHasBeenChanged += EditDbHasChangedIndex;      // Callback in THIS FILE

			// moved from 
			DataUpdated += EditDb_DataUpdated;

			// set up our windows dragging
			this . MouseDown += delegate { DoDragMove ( ); };
			IsDirty = false;
			Startup = false;
		}


		public void UpdateOnExternalChange ( )
		{
			// received notification of data change by a Viewer
			if ( CurrentDb == "BANKACCOUNT" )
			{
				this . DataGrid1 . ItemsSource = null;
				this . DataGrid1 . ItemsSource = EditDbBankcollection;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				this . DataGrid2 . ItemsSource = null;
				this . DataGrid2 . ItemsSource = EditDbCustcollection; ;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				this . DetailsGrid . ItemsSource = null;
				this . DetailsGrid . ItemsSource = EditDbDetcollection;
				this . DetailsGrid . Refresh ( );
			}

		}
		private void Window_Closing ( object sender , CancelEventArgs e )
		{
			if ( NotifyOfDataChange != null )
				NotifyOfDataChange -= DbChangedHandler;

			EventControl . ViewerDataHasBeenChanged -= EditDbHasChangedIndex;


			// Clear up pointers to this instance of an EditDb window
			DataUpdated -= EditDb_DataUpdated;

			MainWindow . gv . SqlCurrentEditViewer = null;

			//Clear flags
			if ( CurrentDb == "BANKACCOUNT" )
			{
				Flags . BankEditDb = null;
				Flags . CurrentEditDbViewerBankGrid = null;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				Flags . CustEditDb = null;
				Flags . CurrentEditDbViewerCustomerGrid = null;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				Flags . DetEditDb = null;
				Flags . CurrentEditDbViewerDetailsGrid = null;
			}
			Flags . ActiveEditGrid = null;
			Flags . CurrentEditDbViewer = null;
			Flags . CurrentSqlViewer . RefreshBtn . IsEnabled = true;
		}

		private void Window_GotFocus ( object sender , RoutedEventArgs e )
		{
			Flags . CurrentEditDbViewer = this;
		}

		private void Window_PreviewKeyDown ( object sender , KeyEventArgs e )
		{
			Console . WriteLine ( $"Key : {e . Key}" );
			DataGrid dg = null;
			if ( Utils . DataGridHasFocus ( this ) == false )
				return;
			if ( e . Key == Key . Escape )
			{
				//if ( CurrentDb == "BANKACCOUNT" )
				//	BankAccountViewModel . ClearFromEditDbList ( DataGrid1, CurrentDb );
				//else if ( CurrentDb == "CUSTOMER" )
				//	BankAccountViewModel . ClearFromEditDbList ( DataGrid2, CurrentDb );
				//else if ( CurrentDb == "DETAILS" )
				//	BankAccountViewModel . ClearFromEditDbList ( DetailsGrid, CurrentDb );
				BankAccountViewModel . EditdbWndBank = null;
				Close ( );
			}
			else if ( e . Key == Key . LeftCtrl )
				key1 = true;
			else if ( e . Key == Key . RightAlt )
			{
				Flags . ListGridviewControlFlags ( );
				key1 = false;
			}
			else if ( e . Key == Key . Up )
			{
				//				int CurrentRow = 0;

				//	Application . Current . Shutdown ( );
				if ( CurrentDb == "BANKACCOUNT" )
					dg = this . DataGrid1;
				else if ( CurrentDb == "CUSTOMER" )
					dg = this . DataGrid2;
				else
					dg = this . DetailsGrid;
				if ( dg . SelectedIndex > 0 )
					dg . SelectedIndex--;

				if ( dg . SelectedItem != null )
					dg . ScrollIntoView ( dg . SelectedItem );
				key1 = false;
			}
			else if ( e . Key == Key . Down )
			{
				//				int CurrentRow = 0;

				//	Application . Current . Shutdown ( );
				if ( CurrentDb == "BANKACCOUNT" )
					dg = this . DataGrid1;
				else if ( CurrentDb == "CUSTOMER" )
					dg = this . DataGrid2;
				else
					dg = this . DetailsGrid;
				if ( dg . SelectedIndex < dg . Items . Count - 1 )
					dg . SelectedIndex++;

				if ( dg . SelectedItem != null )
					dg . ScrollIntoView ( dg . SelectedItem );
				key1 = false;
			}
			else if ( e . Key == Key . Home )
			{
				//				int CurrentRow = 0;

				//	Application . Current . Shutdown ( );
				if ( CurrentDb == "BANKACCOUNT" )
					dg = this . DataGrid1;
				else if ( CurrentDb == "CUSTOMER" )
					dg = this . DataGrid2;
				else
					dg = this . DetailsGrid;
				dg . SelectedIndex = 0;
				if ( dg . SelectedItem != null )
					dg . ScrollIntoView ( dg . SelectedItem );
				key1 = false;
			}
			else if ( e . Key == Key . End )
			{
				// DataGrid keyboard navigation = END
				if ( CurrentDb == "BANKACCOUNT" )
					dg = this . DataGrid1;
				else if ( CurrentDb == "CUSTOMER" )
					dg = this . DataGrid2;
				else
					dg = this . DetailsGrid;
				dg . SelectedIndex = dg . Items . Count - 1;
				if ( dg . SelectedItem != null )
					dg . ScrollIntoView ( dg . SelectedItem );
				key1 = false;
			}
			else if ( e . Key == Key . PageDown )
			{
				// DataGrid keyboard navigation = PAGE DOWN
				if ( CurrentDb == "BANKACCOUNT" )
					dg = this . DataGrid1;
				else if ( CurrentDb == "CUSTOMER" )
					dg = this . DataGrid2;
				else
					dg = this . DetailsGrid;
				if ( dg . SelectedIndex < dg . Items . Count - 10 )
				{
					dg . SelectedIndex += 10;
					if ( dg . SelectedItem != null )
						dg . ScrollIntoView ( dg . SelectedItem );
				}
				else
				{
					dg . SelectedIndex = dg . Items . Count - 1;
					if ( dg . SelectedItem != null )
						dg . ScrollIntoView ( dg . SelectedItem );
				}
				key1 = false;
			}
			else if ( e . Key == Key . PageUp )
			{
				// DataGrid keyboard navigation = PAGE UP
				if ( CurrentDb == "BANKACCOUNT" )
					dg = this . DataGrid1;
				else if ( CurrentDb == "CUSTOMER" )
					dg = this . DataGrid2;
				else
					dg = this . DetailsGrid;
				if ( dg . SelectedIndex >= 10 )
				{
					dg . SelectedIndex -= 10;
					if ( dg . SelectedItem != null )
						dg . ScrollIntoView ( dg . SelectedItem );
				}
				else
				{
					dg . SelectedIndex = 0;
					if ( dg . SelectedItem != null )
						dg . ScrollIntoView ( dg . SelectedItem );
				}
				key1 = false;
			}
			else if ( e . Key == Key . OemQuotes )
			{
				EventHandlers . ShowSubscribersCount ( );
				key1 = false;
			}
			else if ( e . Key == Key . RWin )
			{
				if ( key1 )
				{
					Flags . ShowAllFlags ( );
					key1 = false;
				}
			}
			else if ( e . Key == Key . Delete )
			{
				Flags . CurrentSqlViewer . Window_PreviewKeyDown ( sender , e );
			}
			if ( dg != null )
			{
				// Now process it
				if ( dg == this . DataGrid1 )
					DataGrid1_SelectionChanged ( dg , null );
				else if ( dg == this . DataGrid2 )
					DataGrid2_SelectionChanged ( dg , null );
				else if ( dg == this . DetailsGrid )
					DetailsGrid_SelectionChanged ( dg , null );
				if ( dg . SelectedItem != null )
					dg . ScrollIntoView ( dg . SelectedItem );
				e . Handled = true;
			}
			key1 = false;
		}

		// Window is closing via Close Button Click event
		private void Button_Click ( object sender , RoutedEventArgs e )
		{
			// Window is being closed
			BankAccountViewModel . EditdbWndBank = null;
			Flags . CurrentEditDbViewer = null;
			Close ( );
		}

		#endregion Window Handling Methods

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged ( string PropertyName )
		{
			if ( null != PropertyChanged )
			{
				PropertyChanged ( this ,
					new PropertyChangedEventArgs ( PropertyName ) );
			}
		}

		#endregion INotifyPropertyChanged Members

		#region Cell Editing methods

		private async void DataGrid1_CellEditEnding ( object sender , DataGridCellEditEndingEventArgs e )
		{
			int currindx = this . DataGrid1 . SelectedIndex;
			var curritem = this . DataGrid1 . SelectedItem;

			//Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
			if ( e == null )
			{
				if ( CurrentDb == "BANKACCOUNT" )
				{
					SQLHandlers sqlh = new SQLHandlers ( );
					await sqlh . UpdateDbRow ( "BANKACCOUNT" , e . Row );
					SqlUpdating = true;
					return;
				}
			}
			else
			{
				if ( CurrentDb == "BANKACCOUNT" )
				{
					BankAccountViewModel . SqlUpdating = true;
					SQLHandlers sqlh = new SQLHandlers ( );
					// This call updates the SQL Db and the main viewer is also updated correctly
					await sqlh . UpdateDbRow ( "BANKACCOUNT" , e . Row );
					if ( Flags . CurrentSqlViewer . BankGrid . SelectedItem != null )
						Flags . CurrentSqlViewer . BankGrid . ScrollIntoView ( curritem );
					return;
				}
			}
			//Reset our selected row
			this . DataGrid1 . SelectedIndex = currindx;
			this . DataGrid1 . SelectedItem = curritem;
		}

		private async void DataGrid2_CellEditEnding ( object sender , DataGridCellEditEndingEventArgs e )
		{
			int currindx = this . DataGrid2 . SelectedIndex;
			var curritem = this . DataGrid2 . SelectedItem;

			//Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
			if ( e == null )
			{
				if ( CurrentDb == "CUSTOMER" )
				{
					SQLHandlers sqlh = new SQLHandlers ( );
					await sqlh . UpdateDbRow ( "CUSTOMER" , e . Row );
					SqlUpdating = true;
					return;
				}
			}
			else
			{
				if ( CurrentDb == "CUSTOMER" )
				{
					BankAccountViewModel . SqlUpdating = true;
					SQLHandlers sqlh = new SQLHandlers ( );
					await sqlh . UpdateDbRow ( "CUSTOMER" , e . Row );
					return;
				}
			}
			//Reset our selected row
			this . DataGrid2 . SelectedIndex = currindx;
			this . DataGrid2 . SelectedItem = curritem;
		}

		private async void DetailsGrid_CellEditEnding ( object sender , DataGridCellEditEndingEventArgs e )
		{
			int currindx =this . DetailsGrid . SelectedIndex;
			var curritem =this . DetailsGrid . SelectedItem;

			//Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
			if ( e == null )
			{
				if ( CurrentDb == "DETAILS" )
				{
					SQLHandlers sqlh = new SQLHandlers ( );
					await sqlh . UpdateDbRow ( "DETAILS" , e . Row );
					SqlUpdating = true;
				}
			}
			else
			{
				if ( CurrentDb == "DETAILS" )
				{
					//DetailsViewModel . SqlUpdating = true;
					//SQLHandlers sqlh = new SQLHandlers ( );
					//await sqlh . UpdateDbRow ( "DETAILS", e . Row );
					//SqlUpdating = true;
					//if ( ViewerChangeType == 0 || EditChangeType == 1 )
					//{
					//	EditChangeType = 1;
					//	NotifyviewerOfDataChange (this . DetailsGrid . SelectedIndex, "DETAILS" , DetailsGrid.SelectedItem);
					//}
				}
			}

			//Reset our selected row
			this . DataGrid1 . SelectedIndex = currindx;
			this . DataGrid1 . SelectedItem = curritem;
			//e . Cancel = true;
		}

		#endregion Cell Editing methods

		#region RowEdithandlers

		//Bank Grid
		private async void DataGrid1_RowEditEnding ( object sender , DataGridRowEditEndingEventArgs e )
		{
			//// This ONLY called when a cell is edited
			var sqlh = new SQLHandlers ( );
			Flags . EditDbDataChanged = true;

			// Set Form wide flags to see what  actions we need to take ?  (Edited value or not)
			ViewerChangeType = 0;        // Change made in Viewer
							     //EditChangeType = 2;     // We have changed data in grid

			//Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
			if ( e == null )
			{
				// Row Deleted ???
				BankAccountViewModel . SqlUpdating = true;
				await sqlh . UpdateDbRowAsync ( CurrentDb , this . DataGrid1 . SelectedItem , this . DataGrid1 . SelectedIndex );
				Flags . EditDbDataChanged = true;
				Flags . CurrentSqlViewer . UpdateBankOnEditDbChange ( CurrentDb , this . DataGrid1 . SelectedIndex , this . DataGrid1 . SelectedItem );
				dca . SenderName = CurrentDb;
				dca . DbName = CurrentDb;
			}
			else
			{
				// Row has been changed
				BankAccountViewModel . SqlUpdating = true;
				Flags . CurrentSqlViewer . BankGrid . ItemsSource = null;
				await sqlh . UpdateDbRowAsync ( CurrentDb , this . DataGrid1 . SelectedItem , this . DataGrid1 . SelectedIndex );
				Flags . EditDbDataChanged = true;
				// This call returns when callee resets ItemsSource ???? & never returns there
				//				Flags . CurrentSqlViewer . UpdateBankOnEditDbChange ( CurrentDb , this . DataGrid1 . SelectedIndex , this . DataGrid1 . SelectedItem );
				dca . SenderName = CurrentDb;
				dca . DbName = CurrentDb;
				Console . WriteLine ( $"DataGrid1_RowEditEnding() finished" );
				//				Flags . CurrentSqlViewer . BankGrid. Refresh ( );
				//SendDataChanged ( Flags . CurrentSqlViewer , DataGrid1 , CurrentDb );
				Flags . CurrentSqlViewer . UpdateBankOnEditDbChange ( CurrentDb , this . DataGrid1 . SelectedIndex , this . DataGrid1 . SelectedItem );
			}
			Flags . EditDbDataChanged = false;
		}

		//Customer grid
		private async void DataGrid2_RowEditEnding ( object sender , DataGridRowEditEndingEventArgs e )
		{
			//// This ONLY called when a cell is edited
			var sqlh = new SQLHandlers ( );
			Flags . EditDbDataChanged = true;

			// Set Form wide flags to see what  actions we need to take ?  (Edited value or not)
			ViewerChangeType = 0;        // Change made in Viewer
							     //EditChangeType = 2;     // We have changed data in grid

			//Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
			if ( e == null )
			{
				// Row Deleted ???
				CustomerViewModel . SqlUpdating = true;
				await sqlh . UpdateDbRow ( CurrentDb , this . DataGrid2 . SelectedItem );
				Flags . EditDbDataChanged = true;
				Flags . CurrentSqlViewer . UpdateCustOnEditDbChange ( CurrentDb , this . DataGrid2 . SelectedIndex , this . DataGrid2 . SelectedItem );
				dca . SenderName = CurrentDb;
				dca . DbName = CurrentDb;
				SendDataChanged ( Flags . CurrentSqlViewer , DataGrid2 , CurrentDb );
			}
			else
			{
				// Row has been changed
				CustomerViewModel . SqlUpdating = true;
				await sqlh . UpdateDbRow ( CurrentDb , this . DataGrid2 . SelectedItem );
				Flags . EditDbDataChanged = true;
				Flags . CurrentSqlViewer . UpdateCustOnEditDbChange ( CurrentDb , this . DataGrid2 . SelectedIndex , this . DataGrid2 . SelectedItem );
				dca . SenderName = CurrentDb;
				dca . DbName = CurrentDb;
				SendDataChanged ( Flags . CurrentSqlViewer , DataGrid2 , CurrentDb );
			}
			Flags . EditDbDataChanged = false;
		}

		//Details Grid
		private async void DetailsGrid_RowEditEnding ( object sender , DataGridRowEditEndingEventArgs e )
		{
			//// This ONLY called when a cell is edited
			var sqlh = new SQLHandlers ( );
			Flags . EditDbDataChanged = true;

			// Set Form wide flags to see what  actions we need to take ?  (Edited value or not)
			ViewerChangeType = 0;        // Change made in Viewer
							     //EditChangeType = 2;     // We have changed data in grid

			//Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
			if ( e == null )
			{
				// Row Deleted ???
				DetailsViewModel . SqlUpdating = true;
				await sqlh . UpdateDbRow ( CurrentDb , this . DataGrid1 . SelectedItem );
				Flags . EditDbDataChanged = true;
				Flags . CurrentSqlViewer . UpdateDetailsOnEditDbChange ( CurrentDb , this . DataGrid1 . SelectedIndex , this . DataGrid1 . SelectedItem );
				dca . SenderName = CurrentDb;
				dca . DbName = CurrentDb;
				SendDataChanged ( Flags . CurrentSqlViewer , this . DetailsGrid , CurrentDb );
			}
			else
			{
				// Row data has been changed, update the Db's first, then notify other viewers
				DetailsViewModel . SqlUpdating = true;
				int currsel = this. DetailsGrid.SelectedIndex;
				await sqlh . UpdateDbRow ( CurrentDb , this . DetailsGrid . SelectedItem );
				Flags . EditDbDataChanged = true;
				Flags . CurrentSqlViewer . UpdateDetailsOnEditDbChange ( CurrentDb , this . DetailsGrid . SelectedIndex , this . DetailsGrid . SelectedItem );
				dca . SenderName = CurrentDb;
				dca . DbName = CurrentDb;
				SendDataChanged ( Flags . CurrentSqlViewer , this . DetailsGrid , CurrentDb );
			}
			Flags . EditDbDataChanged = false;
		}

		public void SendDataChanged ( SqlDbViewer o , DataGrid Grid , string dbName )
		{
			// Databases have DEFINITELY been updated successfully after a change
			// We Now Broadcast this to ALL OTHER OPEN VIEWERS here and now

			dca . SenderName = o . ToString ( );
			dca . DbName = dbName;

			if ( dbName == "BANKACCOUNT" )
			{
				Flags . CurrentSqlViewer . ReloadCustomerOnUpdateNotification ( o , Grid , dca );
				Flags . CurrentSqlViewer . ReloadDetailsOnUpdateNotification ( o , Grid , dca );
				this . DataGrid1 . Refresh ( );
			}
			else if ( dbName == "CUSTOMER" )
			{
				Flags . CurrentSqlViewer . ReloadBankOnUpdateNotification ( o , Grid , dca );
				Flags . CurrentSqlViewer . ReloadDetailsOnUpdateNotification ( o , Grid , dca );
				this . DataGrid2 . Refresh ( );
			}
			else if ( dbName == "DETAILS" )
			{
				Flags . CurrentSqlViewer . ReloadCustomerOnUpdateNotification ( o , Grid , dca );
				Flags . CurrentSqlViewer . ReloadBankOnUpdateNotification ( o , Grid , dca );
				this . DetailsGrid . Refresh ( );
				DetailsEditFields . DataContext = this . DetailsGrid . SelectedItem; ;
			}
			Mouse . OverrideCursor = Cursors . Arrow;
		}

		#endregion RowEdithandlers

		#region RowSelection handlers

		//public void ViewerHasChangedIndex ( int newRow, string CurrentDb )
		//{
		//	if ( CurrentDb == "BANKACCOUNT" )
		//	{
		//		this . DataGrid1 . SelectedIndex = newRow;

		//	}
		//	else if ( CurrentDb == "CUSTOMER" )
		//	{
		//	}
		//	else if ( CurrentDb == "DETAILS" )
		//	{
		//	}
		//}

		// BankAccount
		/// <summary>
		/// Receives the notification from the main db viewer that a selection has been changed
		/// and sends it to that same viewer when changed in this window
		/// so both windows update the current row simaltaneously.
		/// </summary>
		public void DataGrid1_SelectionChanged ( object sender , SelectionChangedEventArgs e )
		{
			int selrow = 0;
			if ( sqldbv == null ) return;
			IsDirty = false;
			try
			{
				if ( Flags . isMultiMode )
					this . Status . Content = $"Total Records = {this . DataGrid1 . Items . Count}, Current Record = {this . DataGrid1 . SelectedIndex}, Duplicate A/C's only shown...";
				else if ( Flags . IsFiltered )
					this . Status . Content = $"Total Records = {this . DataGrid1 . Items . Count}, Current Record = {this . DataGrid1 . SelectedIndex}, Resullts ARE Filtered";
				else
					this . Status . Content = $"Total Records = {this . DataGrid1 . Items . Count}, Current Record = {this . DataGrid1 . SelectedIndex}";
			}
			catch { }
			selrow = this . DataGrid1 . SelectedIndex;

			//			Utils . ScrollRecordIntoView ( DataGrid1 );
			IsDirty = false;
			BankEditFields . DataContext = this . DataGrid1 . SelectedItem;
			if ( Flags . CurrentSqlViewer != null )
				Flags . CurrentSqlViewer . BankGrid . SelectedIndex = this . DataGrid1 . SelectedIndex;
			Utils . ScrollRecordIntoView ( DataGrid1 , 1 );
			ViewerButton . IsEnabled = false;
			IsDirty = false;
		}

		//Customer Db
		/// <summary>
		/// Receives the notification from the main db viewer that a selection has been changed
		/// and sends it to that same viewer when changed in this window
		/// so both windows update the current row simaltaneously.
		/// </summary>
		public void DataGrid2_SelectionChanged ( object sender , SelectionChangedEventArgs e )
		{
			if ( this . DataGrid2 . SelectedIndex == -1 ) return;
			if ( sqldbv == null ) return;
			IsDirty = false;

			try
			{
				if ( Flags . isMultiMode )
					this . Status . Content = $"Total Records = {this . DataGrid2 . Items . Count}, Current Record = {this . DataGrid2 . SelectedIndex}, Duplicate A/C's only shown...";
				else if ( Flags . IsFiltered )
					this . Status . Content = $"Total Records = {this . DataGrid2 . Items . Count}, Current Record = {this . DataGrid2 . SelectedIndex}, Resullts ARE Filtered";
				else
					this . Status . Content = $"Total Records = {this . DataGrid2 . Items . Count}, Current Record = {this . DataGrid2 . SelectedIndex}";
			}
			catch { }
			//			Utils . ScrollRecordIntoView ( DataGrid2 );
			IsDirty = false;
			CustomerEditFields . DataContext = this . DataGrid2 . SelectedItem;
			Utils . ScrollRecordIntoView ( DataGrid2 , 2 );
			if ( Flags . CurrentSqlViewer != null )
				Flags . CurrentSqlViewer . CustomerGrid . SelectedIndex = this . DataGrid2 . SelectedIndex;
			ViewerButton . IsEnabled = false;
	}

		// Details
		/// <summary>
		/// Receives the notification from the main db viewer that a selection has been changed
		/// and sends it to that same viewer when changed in this window
		/// so both windows update the current row simultaneously.
		/// </summary>
		public void DetailsGrid_SelectionChanged ( object sender , SelectionChangedEventArgs e )
		{
			if ( this . DetailsGrid . SelectedIndex == -1 ) return;
			if ( sqldbv == null ) return;
			if ( sqldbv == null ) return;
			IsDirty = false;
			try
			{
				if ( Flags . isMultiMode )
					this . Status . Content = $"Total Records = {this . DetailsGrid . Items . Count}, Current Record = {this . DetailsGrid . SelectedIndex}, Duplicate A/C's only shown...";
				else if ( Flags . IsFiltered )
					this . Status . Content = $"Total Records = {this . DetailsGrid . Items . Count}, Current Record = {this . DetailsGrid . SelectedIndex}, Resullts ARE Filtered";
				else
					this . Status . Content = $"Total Records = {this . DetailsGrid . Items . Count}, Current Record = {this . DetailsGrid . SelectedIndex}";
			}
			catch { }
			IsDirty = false;
			DetailsEditFields . DataContext = this . DetailsGrid . SelectedItem;
			Utils . ScrollRecordIntoView ( this . DetailsGrid , 0 );
			if ( Flags . CurrentSqlViewer != null )
				Flags . CurrentSqlViewer . DetailsGrid . SelectedIndex = this . DetailsGrid . SelectedIndex;
			ViewerButton . IsEnabled = false;
		}

		#endregion RowSelection handlers


		//Bank Edit fields

		#region Bank Editing fields

		private void ActypeEdit_LostFocus ( object sender , RoutedEventArgs e )
		{
			//if ( BalanceEdit . Text == "" ) return;
			//var row = DataGrid1.SelectedItem as BankAccountViewModel;
			//row . AcType = Convert . ToInt32 ( ActypeEdit . Text );
			//this . DetailsGrid . Refresh ( );
			//RefreshItemsSource ( this . DetailsGrid );
		}

		private void BanknoEdit_LostFocus ( object sender , RoutedEventArgs e )
		{
			//if ( BanknoEdit . Text == "" ) return;
			//var row = DataGrid1.SelectedItem as BankAccountViewModel;
			//row . BankNo = BanknoEdit . Text;
			//this . DataGrid1 . Refresh ( );
			//RefreshItemsSource ( this . DataGrid1 );
		}

		private void CustNoEdit_LostFocus ( object sender , RoutedEventArgs e )
		{
			//if ( CustnoEdit . Text == "" ) return;
			//var row = DataGrid1.SelectedItem as BankAccountViewModel;
			//row . CustNo = CustnoEdit . Text;
			//this . DataGrid1 . Refresh ( );
			//RefreshItemsSource ( this . DataGrid1 );
		}

		private void BalanceEdit_LostFocus ( object sender , RoutedEventArgs e )
		{
			//if ( BalanceEdit . Text == "" ) return;
			//var row = DataGrid1.SelectedItem as BankAccountViewModel;
			//row . Balance = Convert . ToDecimal ( BalanceEdit .Text);
			//this . DataGrid1 . Refresh ( );
			//RefreshItemsSource ( this . DataGrid1 );
		}

		private void IntRateEdit_LostFocus ( object sender , RoutedEventArgs e )
		{
			//if ( IntRateEdit . Text == "" ) return;
			//var row = DataGrid1.SelectedItem as BankAccountViewModel;
			//row . IntRate = Convert . ToDecimal ( IntRateEdit.Text );
			//this . DataGrid1 . Refresh ( );
			//RefreshItemsSource ( this . DataGrid1 );
		}

		private void OpenDateEdit_LostFocus ( object sender , RoutedEventArgs e )
		{
			//if ( OpenDateEdit . Text == "" ) return;
			//var row = DataGrid1.SelectedItem as BankAccountViewModel;
			//row . ODate = Convert . ToDateTime ( OpenDateEdit . Text );
			//this . DataGrid1 . Refresh ( );
			//RefreshItemsSource ( this . DataGrid1 );
		}

		private void CloseDateEdit_LostFocus ( object sender , RoutedEventArgs e )
		{
			//if ( CloseDateEdit . Text == "" ) return;
			//var row = DataGrid1.SelectedItem as BankAccountViewModel;
			//row . CDate = Convert . ToDateTime ( CloseDateEdit . Text );
			//this . DataGrid1 . Refresh ( );
			//RefreshItemsSource ( this . DataGrid1 );
		}

		#endregion Bank Editing fields

		//Customer edit fields

		#region Customer Editing fields

		private void BanknoEdit2_LostFocus ( object sender , RoutedEventArgs e )
		{
			var row = DataGrid2.SelectedItem as CustomerViewModel;
			row . BankNo = BanknoEdit2 . Text;
			this . DataGrid2 . Refresh ( );
			RefreshItemsSource ( this . DataGrid2 );
		}
		private void CustnoEdit2_LostFocus ( object sender , RoutedEventArgs e )
		{
			var row = DataGrid2.SelectedItem as CustomerViewModel;
			row . CustNo = CustnoEdit2 . Text;
			this . DataGrid2 . Refresh ( );
			RefreshItemsSource ( this . DataGrid2 );
		}
		private void FirstnameEdit2_LostFocus ( object sender , RoutedEventArgs e )
		{
			var row = DataGrid2.SelectedItem as CustomerViewModel;
			row . FName = Firstname2 . Text;
			this . DataGrid2 . Refresh ( );
			RefreshItemsSource ( this . DataGrid2 );
		}
		private void LastnameEdit2_LostFocus ( object sender , RoutedEventArgs e )
		{
			var row = DataGrid2.SelectedItem as CustomerViewModel;
			row . LName = Lastname2 . Text;
			this . DataGrid2 . Refresh ( );
			RefreshItemsSource ( this . DataGrid2 );
		}
		private void TownEdit2_LostFocus ( object sender , RoutedEventArgs e )
		{
			var row = DataGrid2.SelectedItem as CustomerViewModel;
			row . Town = town . Text;
			this . DataGrid2 . Refresh ( );
			RefreshItemsSource ( this . DataGrid2 );
		}
		private void AcTypeEdit2_LostFocus ( object sender , RoutedEventArgs e )
		{
			var row = DataGrid2.SelectedItem as CustomerViewModel;
			row . AcType = Convert . ToInt32 ( AcType2 . Text );
			this . DataGrid2 . Refresh ( );
			RefreshItemsSource ( this . DataGrid2 );
		}

		private void Addr1Edit2_LostFocus ( object sender , RoutedEventArgs e )
		{
			var row = DataGrid2.SelectedItem as CustomerViewModel;
			row . Addr1 = addr1 . Text;
			this . DataGrid2 . Refresh ( );
			RefreshItemsSource ( this . DataGrid2 );
		}

		private void Addr2Edit2_LostFocus ( object sender , RoutedEventArgs e )
		{
			var row = DataGrid2.SelectedItem as CustomerViewModel;
			row . Addr2 = addr2 . Text;
			this . DataGrid2 . Refresh ( );
			RefreshItemsSource ( this . DataGrid2 );
		}

		private void MobileEdit2_LostFocus ( object sender , RoutedEventArgs e )
		{
			var row = DataGrid2.SelectedItem as CustomerViewModel;
			row . Mobile = mobile2 . Text;
			this . DataGrid2 . Refresh ( );
			RefreshItemsSource ( this . DataGrid2 );
		}

		private void PhoneEdit2_LostFocus ( object sender , RoutedEventArgs e )
		{
			var row = DataGrid2.SelectedItem as CustomerViewModel;
			row . Phone = phone2 . Text;
			this . DataGrid2 . Refresh ( );
			RefreshItemsSource ( this . DataGrid2 );

		}
		private void CountyEdit2_LostFocus ( object sender , RoutedEventArgs e )
		{
			var row = DataGrid2.SelectedItem as CustomerViewModel;
			row . County = County2 . Text;
			this . DataGrid2 . Refresh ( );
			RefreshItemsSource ( this . DataGrid2 );
		}

		private void PcodeEdit2_LostFocus ( object sender , RoutedEventArgs e )
		{
			var row = DataGrid2.SelectedItem as CustomerViewModel;
			row . PCode = pcode2 . Text;
			this . DataGrid2 . Refresh ( );
			RefreshItemsSource ( this . DataGrid2 );
		}

		private void ODate2_LostFocus ( object sender , RoutedEventArgs e )
		{
			var row = DataGrid2.SelectedItem as CustomerViewModel;
			row . ODate = Convert . ToDateTime ( ODate2 . Text );
			this . DataGrid2 . Refresh ( );
			RefreshItemsSource ( this . DataGrid2 );
		}
		private void CDate2_LostFocus ( object sender , RoutedEventArgs e )
		{
			var row = DataGrid2.SelectedItem as CustomerViewModel;
			row . CDate = Convert . ToDateTime ( Cdate2 . Text );
			this . DataGrid2 . Refresh ( );
			RefreshItemsSource ( this . DataGrid2 );
		}
		private void Dob2_LostFocus ( object sender , RoutedEventArgs e )
		{
			var row = DataGrid2.SelectedItem as CustomerViewModel;
			row . Dob = Convert . ToDateTime ( Dob2 . Text );
			this . DataGrid2 . Refresh ( );
			RefreshItemsSource ( this . DataGrid2 );
		}
		#endregion Customer Editing fields

		// Details Edit fields

		#region Details Editing fields

		private void ActypeEdit3LostFocus ( object sender , RoutedEventArgs e )
		{
			EditStart = true;
			ViewerButton . IsEnabled = EditStart;
			//var row = DetailsGrid.SelectedItem as DetailsViewModel;
			//row . AcType = Convert . ToInt32 ( ActypeEdit3 . Text );
			//this . DetailsGrid . Refresh ( );
			//RefreshItemsSource ( this . DetailsGrid );
		}

		private void CustnoEdit3_LostFocus ( object sender , RoutedEventArgs e )
		{
			EditStart = true;
			ViewerButton . IsEnabled = EditStart;
			//var row = DetailsGrid.SelectedItem as DetailsViewModel;
			//row . CustNo = CustnoEdit3 . Text ;
			//this . DetailsGrid . Refresh ( );
			//RefreshItemsSource ( this . DetailsGrid );
		}

		private void BanknoEdit3_LostFocus ( object sender , RoutedEventArgs e )
		{
			EditStart = true;
			ViewerButton . IsEnabled = EditStart;
			//var row = DetailsGrid.SelectedItem as DetailsViewModel;
			//row . BankNo = BanknoEdit . Text ;
			//this . DetailsGrid . Refresh ( );
			//RefreshItemsSource ( this . DetailsGrid );
		}

		private void BalanceEdit3_LostFocus ( object sender , RoutedEventArgs e )
		{
			EditStart = true;
			ViewerButton . IsEnabled = EditStart;
			//var row = DetailsGrid.SelectedItem as DetailsViewModel;
			//row . Balance = Convert . ToDecimal ( BalanceEdit3 . Text );
			//this . DetailsGrid . Refresh ( );
			//RefreshItemsSource ( this . DetailsGrid );
		}

		private void IntRateEdit3_LostFocus ( object sender , RoutedEventArgs e )
		{
			EditStart = true;
			ViewerButton . IsEnabled = EditStart;
			//var row = DetailsGrid.SelectedItem as DetailsViewModel;
			//row . IntRate = Convert . ToDecimal ( IntRateEdit3 . Text );
			//this . DetailsGrid . Refresh ( );
			//RefreshItemsSource ( this . DetailsGrid );
		}

		private void OpenDateEdit3_LostFocus ( object sender , RoutedEventArgs e )
		{
			EditStart = true;
			ViewerButton . IsEnabled = EditStart;
			//var row = DetailsGrid.SelectedItem as DetailsViewModel;
			//row . ODate = Convert . ToDateTime( OpenDateEdit3 . Text );
			//this . DetailsGrid . Refresh ( );
			//RefreshItemsSource ( this . DetailsGrid );
		}

		private void CloseDateEdit3_LostFocus ( object sender , RoutedEventArgs e )
		{
			EditStart = true;
			ViewerButton . IsEnabled = EditStart;
			//var row = DetailsGrid.SelectedItem as DetailsViewModel;
			//row . CDate = Convert . ToDateTime ( CloseDateEdit . Text );
			//this . DetailsGrid . Refresh ( );
			//RefreshItemsSource ( this . DetailsGrid );
		}

		#endregion Details Editing fields

		#region Edit fields "LostFocus" handlers (Calls RefreshItems()

		private async Task RefreshItemsSource ( DataGrid Grid )
		{
			//// Set our pointer so Viewer Click can work
			int currsel = Grid.SelectedIndex;
			Flags . EditDbDataChanged = true;
			// NB the Grid on here now shows the New Data content, as does the grid's SelectedItem
			//So we ought to call a method to save the change made....
			//Now update   the Db via Sql - WORKS FINE 3/5/21
			//9/5/21 :  Now we gotta update other open Grid Viewers
			SQLHandlers sqlh = new SQLHandlers ();

			if ( CurrentDb == "BANKACCOUNT" )
			{
				sqlh . UpdateDbRow ( CurrentDb , Grid . SelectedItem );
				BankCollection . LoadBank ( 2 , false );
				Grid . ItemsSource = null;
				Grid . ItemsSource = EditDbBankcollection;
				Grid . SelectedIndex = currsel;
				Grid . Refresh ( );
				Grid . ScrollIntoView ( currsel );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				sqlh . UpdateDbRow ( CurrentDb , Grid . SelectedItem );
				CustCollection . LoadCust ( EditDbCustcollection );
				Grid . ItemsSource = null;
				Grid . ItemsSource = EditDbCustcollection;
				IsDirty = false;
				Grid . SelectedIndex = currsel;
				Grid . Refresh ( );
				Grid . ScrollIntoView ( currsel );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				sqlh . UpdateDbRow ( CurrentDb , Grid . SelectedItem );
				//				DetCollection dc = new DetCollection();
				//				Detcollection = await dc . LoadDetailsTaskInSortOrderAsync ( true , Grid . SelectedIndex );
				DetCollection . LoadDet ( EditDbDetcollection , 2 );
				Grid . ItemsSource = null;
				Grid . ItemsSource = EditDbDetcollection;
				Grid . SelectedIndex = currsel;
				Grid . Refresh ( );
				Grid . ScrollIntoView ( currsel );
				SendDataChanged ( Flags . CurrentSqlViewer , DetailsGrid , CurrentDb );
			}
			// now we need to tell anny other viewers about the changes
			Flags . EditDbDataChanged = false;
			IsDirty = false;
			EditStart = false;
		}


		#endregion Edit fields "LostFocus" handler

		#region Mouse Preview handlers

		private void DoDragMove ( )
		{
			//Handle the button NOT being the left mouse button
			// which will crash the DragMove Fn.....
			try
			{
				this . DragMove ( );
			}
			catch
			{
				return;
			}
		}

		private void DataGrid1_PreviewMouseDown ( object sender , MouseButtonEventArgs e )
		{
			// handle flags to let us know WE have triggered the selectedIndex change
			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			if ( e . ChangedButton == MouseButton . Right )
			{
				DataGridRow RowData;
				int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
				RowInfoPopup rip = new RowInfoPopup ( "BANKACCOUNT", DataGrid1, RowData );
				rip . DataContext = RowData;
				e . Handled = true;
				rip . ShowDialog ( );
				this . DataGrid1 . SelectedItem = RowData . Item;
				this . DataGrid1 . ItemsSource = null;
				this . DataGrid1 . ItemsSource = EditDbBankcollection;
				this . DataGrid1 . Refresh ( );
				Flags . CurrentSqlViewer . BankGrid . ItemsSource = null;
				Flags . CurrentSqlViewer . BankGrid . ItemsSource = EditDbBankcollection;
				Flags . CurrentSqlViewer . BankGrid . Refresh ( );
			}
			else
				e . Handled = false;
		}

		private void DataGrid2_PreviewMouseDown ( object sender , MouseButtonEventArgs e )
		{
			// handle flags to let us know WE have triggered the selectedIndex change
			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			if ( e . ChangedButton == MouseButton . Right )
			{
				DataGridRow RowData;
				int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
				rip = new RowInfoPopup ( "CUSTOMER" , DataGrid2 , RowData );
				//PopupActive = true;
				rip . DataContext = RowData;
				e . Handled = true;
				rip . ShowDialog ( );

				//If data has been changed, update everywhere
				this . DataGrid2 . SelectedItem = RowData . Item;
				this . DataGrid2 . ItemsSource = null;
				this . DataGrid2 . ItemsSource = EditDbCustcollection;
				this . DataGrid2 . Refresh ( );
				Flags . CurrentSqlViewer . CustomerGrid . ItemsSource = null;
				Flags . CurrentSqlViewer . CustomerGrid . ItemsSource = EditDbCustcollection;
				Flags . CurrentSqlViewer . CustomerGrid . Refresh ( );
			}
			else
				e . Handled = false;
		}

		private void DetailsGrid_PreviewMouseDown ( object sender , MouseButtonEventArgs e )
		{
			// handle flags to let us know WE have triggered the selectedIndex change
			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			if ( e . ChangedButton == MouseButton . Right )
			{
				DataGridRow RowData;
				int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
				RowInfoPopup rip = new RowInfoPopup ( "DETAILS", this. DetailsGrid, RowData );
				rip . DataContext = RowData;
				e . Handled = true;
				rip . ShowDialog ( );

				//If data has been changed, update everywhere
				this . DetailsGrid . SelectedItem = RowData . Item;
				this . DetailsGrid . ItemsSource = null;
				this . DetailsGrid . ItemsSource = EditDbDetcollection;
				this . DetailsGrid . Refresh ( );
				Flags . CurrentSqlViewer . DetailsGrid . ItemsSource = null;
				Flags . CurrentSqlViewer . DetailsGrid . ItemsSource = EditDbDetcollection;
				Flags . CurrentSqlViewer . DetailsGrid . Refresh ( );
			}
			//			else
			//				e . Handled = false;
		}

		#endregion Mouse Preview handlers

		#region Task experimentation - NO LONGER USED

		//***********************************************************************************//
		//************************Task to handle selection changes**********************************//
		//***********************************************************************************//

		//		public async void HandleSelChange ()
		//		{
		////			if(tokenSource  != null)
		//			try
		//			{
		//				mainTask = Task.Run (() => looper (token), token);
		//			}
		//                        catch (ObjectDisposedException ex)
		//			{
		//				Console.WriteLine ($"\r\nObject Disposed Task Exception has occurred.{ex.Message}\r\n\r\n");
		//			}
		//			catch (OperationCanceledException ex)
		//			{
		//				Console.WriteLine ($"\r\nTask was cancelled by me.{ex.Message}\r\n\r\n");
		//			}
		//			catch (Exception ex)
		//			{
		//				Console.WriteLine ($"\r\nUnknown Task Exception has occurred.{ex.Message}\r\n\r\n");
		//			}
		//			finally
		//			{
		//		//		tokenSource.Dispose ();
		//			}

		//		}
		//		private async void looper (CancellationToken ct)
		//		{
		//			while (true)
		//			{
		////				if (ct.IsCancellationRequested)
		////					ct.ThrowIfCancellationRequested ();
		//				Thread.Sleep (350);
		//				if (MainWindow.DgControl.SqlSelChange == true)
		//				//only  do this  if called by SqlDbViewer, else it hangs big time
		//				{
		//					MainWindow.DgControl.SqlSelChange = false;
		//					Dispatcher.Invoke (() =>
		//					{
		//						try
		//						{
		//							Task task = Task.Factory.StartNew (UpdateGrid);
		//							Console.WriteLine ($"\r\nTask thread cancelled intentionally.\r\n\r\n");
		//						}
		//						catch (ObjectDisposedException ex)
		//						{
		//							Console.WriteLine ($"\r\nObject Disposed Task Exception has occurred.{ex.Message}\r\n\r\n");
		//						}
		//						catch (OperationCanceledException ex)
		//						{
		//							Console.WriteLine ($"\r\nTask was cancelled by me.{ex.Message}\r\n\r\n");
		//						}
		//						catch (Exception ex )
		//						{
		//							Console.WriteLine ($"\r\nUnknown Task Exception has occurred.{ex.Message}\r\n\r\n");
		//						}
		//						finally
		//						{
		//							//tokenSource.Dispose ();
		//						}
		//					});
		//				}
		//			}
		//		}

		//		private async void UpdateGrid ()
		//		{
		//			// Handle the updating of the current selection
		//			Dispatcher.Invoke (() =>
		//			{
		//				DataGrid1.SelectedIndex = MainWindow.DgControl.SelectedIndex;
		//			});
		//		}

		#endregion Task experimentation - NO LONGER USED

		private async void SaveChanges_Click ( object sender , RoutedEventArgs e )
		{
			// Save data
			//RoutedEventArgs ra =new RoutedEventArgs();
			//ra = e.OriginalSource  as RoutedEventArgs;
			//RoutedEvent  re = ra . RoutedEvent as RoutedEvent;
			//Type t = re.HandlerType;

			int currsel = dGrid.SelectedIndex;
			Flags . EditDbDataChanged = true;
			// NB the Grid on here now shows the New Data content, as does the grid's SelectedItem
			//So we ought to call a method to save the change made....
			//Now update   the Db via Sql - WORKS FINE 3/5/21
			//9/5/21 :  Now we gotta update other open Grid Viewers
			SQLHandlers sqlh = new SQLHandlers ();

			if ( CurrentDb == "BANKACCOUNT" )
			{
				sqlh . UpdateDbRow ( CurrentDb , dGrid . SelectedItem );
				IsDirty = false;
				EditDbBankcollection = BankCollection . LoadBank ( 2 );
				dGrid . ItemsSource = null;
				dGrid . ItemsSource = EditDbBankcollection;
				dGrid . SelectedIndex = currsel;
				SendDataChanged ( Flags . CurrentSqlViewer , DataGrid1 , CurrentDb );
				dGrid . Refresh ( );
				dGrid . ScrollIntoView ( currsel );
				dGrid . SelectedIndex = currsel;

			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				sqlh . UpdateDbRow ( CurrentDb , dGrid . SelectedItem );
				IsDirty = false;
				EditDbCustcollection = CustCollection . LoadCust ( EditDbCustcollection );
				dGrid . ItemsSource = null;
				dGrid . ItemsSource = EditDbCustcollection;
				dGrid . SelectedIndex = currsel;
				SendDataChanged ( Flags . CurrentSqlViewer , DataGrid2 , CurrentDb );
				dGrid . Refresh ( );
				dGrid . ScrollIntoView ( currsel );
				dGrid . SelectedIndex = currsel;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				sqlh . UpdateDbRow ( CurrentDb , dGrid . SelectedItem );
				IsDirty = false;
				EditDbDetcollection = DetCollection . LoadDet ( EditDbDetcollection , 2 );
				dGrid . ItemsSource = null;
				dGrid . ItemsSource = EditDbDetcollection;
				dGrid . SelectedIndex = currsel;
				Flags . CurrentSqlViewer . UpdateDetailsOnEditDbChange ( CurrentDb , this . DetailsGrid . SelectedIndex , this . DetailsGrid . SelectedItem );
				dGrid . Refresh ( );
				dGrid . ScrollIntoView ( currsel );
				dGrid . SelectedIndex = currsel;
			}
			Flags . EditDbDataChanged = false;
			EditStart = false;
			ViewerButton . IsEnabled = false;
			CloseButton . Content = "Close Editor";
			CloseButton . FontSize = 11;
			CloseButton . Foreground = Brushes . White;
			dGrid . IsEnabled = true;

			return;
		}
		#region Debug support methods


		public static Delegate [ ] GetEventCount9 ( )
		{
			Delegate [ ] dglist2 = null;
			if ( DataUpdated != null )
				dglist2 = DataUpdated?.GetInvocationList ( );
			return dglist2;
		}

		public static Delegate [ ] GetEventCount10 ( )
		{
			Delegate [ ] dglist2 = null;
			if ( AllViewersUpdate != null )
				dglist2 = AllViewersUpdate?.GetInvocationList ( );
			return dglist2;
		}

		#endregion Debug support methods

		private void Data_TextChanged ( object sender , TextChangedEventArgs e )
		{
			EditStart = true;
			if ( Startup ) return;
			if(Utils.DataGridHasFocus ( this) == false)
			{
				this . ViewerButton . IsEnabled = true;
				if ( CurrentDb == "BANKACCOUNT" )
					this . DataGrid1 . IsEnabled = false;
				else if ( CurrentDb == "CUSTOMER" )
					this . DataGrid2 . IsEnabled = false;
				else if ( CurrentDb == "DETAILS" )
					this . DetailsGrid . IsEnabled = false;

				CloseButton . Content = "Ignore Changes";
				CloseButton . FontSize = 12;
				CloseButton . Foreground = Brushes . White;
				IsDirty = true;
			}
			//catch ( Exception ex )
			//{
			//	Console . WriteLine ( $"ViewerButton fasiled to enable !!  {ex . Message}, {ex . Data}" );
			//}
		}

		private void Window_Closed ( object sender , EventArgs e )
		{
			if ( NotifyOfDataChange != null )
				NotifyOfDataChange -= DbChangedHandler;

			EventControl . ViewerDataHasBeenChanged -= EditDbHasChangedIndex;


			// Clear up pointers to this instance of an EditDb window
			DataUpdated -= EditDb_DataUpdated;

			MainWindow . gv . SqlCurrentEditViewer = null;

			//Clear flags
			if ( CurrentDb == "BANKACCOUNT" )
			{
				Flags . BankEditDb = null;
				Flags . CurrentEditDbViewerBankGrid = null;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				Flags . CustEditDb = null;
				Flags . CurrentEditDbViewerCustomerGrid = null;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				Flags . DetEditDb = null;
				Flags . CurrentEditDbViewerDetailsGrid = null;
			}
			//			EditDbDetcollection . Clear ( );
			EditDbDetcollection = null;
			Flags . ActiveEditGrid = null;
			Flags . CurrentEditDbViewer = null;
			Flags . CurrentSqlViewer . RefreshBtn . IsEnabled = true;

		}
	}

}
