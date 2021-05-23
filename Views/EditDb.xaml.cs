﻿using System;
using System . ComponentModel;
using System . Data;
using System . Diagnostics;
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

	public delegate void SelectedRowChanged ( int row, string CurentDb );

	public partial class EditDb : INotifyPropertyChanged
	{
		#region CLASS DECLARATIONS

		public static event DbUpdated NotifyOfDataChange;

		public  BankCollection EditDbBankcollection = BankCollection . EditDbBankcollection;
		public  CustCollection EditDbCustcollection = CustCollection . EditDbCustcollection;
		public  DetCollection EditDbDetcollection = DetCollection . EditDbDetcollection;

		public BankAccountViewModel bvm = MainWindow . bvm;
		public CustomerViewModel cvm = MainWindow . cvm;
		public DetailsViewModel dvm = MainWindow . dvm;
		//		public BankCollection Bankcollection  = new BankCollection();

		//		public BankCollection Bankcollection = bcn.Bankcollection;
		//public CustCollection Custcollection = CustCollection . Custcollection;
		//public DetCollection Detcollection = DetCollection.Detcollection;

		public DataChangeArgs dca = new DataChangeArgs ( );
		internal  SqlDbViewer ThisParent = null;

		//flag to let us know we sent the notification
		//		private bool EditHasNotifiedOfChange = false;

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
		public  bool SqlUpdating = false;
		public  bool EditStart = false;
		public  bool Startup = true;
		public  bool IsDirty = false;

		// Flags to let me handle jupdates to/From SqlViewer
		private int ViewerChangeType = 0;
		private  int EditdbchangeInProgress = -1;

		private int EditChangeType = 0;
		private bool key1 = false;
		public  static EditDb ThisWindow;
		private DataGrid dGrid = null;
		#endregion CLASS DECLARATIONS

		#region (TRUE) EVENT CALLBACK Declarations

		// We HAVE to duplicate this from SQLHandlers otherwise it cannot be found despite  being flagged as PPUBLIC
		//		public static event EventHandler<DataUpdatedEventArgs> DataUpdated;

		// We HAVE to duplicate this from SQLHandlers otherwise it cannot be found despite  being flagged as PPUBLIC
		//		public static event EventHandler<NotifyAllViewersOfUpdateEventArgs> AllViewersUpdate;

		#endregion (TRUE) EVENT CALLBACK Declarations


		// Trigger Method to be sent when data is updated (in a DbEdit Window)
		//public static void OnAllViewersUpdate ( object sender, string CurrentDb )
		//{
		//	if ( AllViewersUpdate != null )
		//	{
		//		Debug . WriteLine ( $"Broadcasting from OnDataLoaded in SQLHandlers()" );
		//		AllViewersUpdate?.Invoke ( sender, new NotifyAllViewersOfUpdateEventArgs
		//		{ CurrentDb = CurrentDb } );
		//	}
		//}

		#region DELEGATE Handlers

		/// <summary>
		/// EVENT HANDLER for  DataUpdated event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void EventControl_ViewerDataUpdated ( object sender, LoadedEventArgs e )
		{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				//				if ( e . DataSource == EditDbBankcollection ) return;
				if ( sender == EditDbBankcollection ) return;

				int currsel = e . RowCount;
				if ( currsel == -1 ) currsel = 0;
				this . DataGrid1 . ItemsSource = null;
				this . DataGrid1 . Items . Clear ( );
				Mouse . OverrideCursor = Cursors . Wait;
				EditDbBankcollection = await BankCollection . LoadBank ( EditDbBankcollection, 2 );
				this . DataGrid1 . ItemsSource = EditDbBankcollection;
				this . DataGrid1 . SelectedIndex = e . RowCount;
				this . DataGrid1 . Refresh ( );
				Utils . ScrollRecordInGrid ( DataGrid1, e . RowCount );
				Debug . WriteLine ( $"EventControl_BankDataLoaded has Updated the Bank Account Grid  content to the latest Db collection...." );
				Mouse . OverrideCursor = Cursors . Arrow;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				if ( sender == EditDbCustcollection ) return;
				//				if ( e . DataSource == EditDbCustcollection ) return;
				int currsel = e . RowCount;
				if ( currsel == -1 ) currsel = 0;
				this . DataGrid2 . ItemsSource = null;
				this . DataGrid2 . Items . Clear ( );
				Mouse . OverrideCursor = Cursors . Wait;
				EditDbCustcollection = await CustCollection . LoadCust ( EditDbCustcollection );
				this . DataGrid2 . ItemsSource = EditDbCustcollection;
				this . DataGrid2 . SelectedIndex = e . RowCount;
				this . DataGrid2 . Refresh ( );
				Utils . ScrollRecordInGrid ( DataGrid2, e . RowCount );
				Debug . WriteLine ( $"EventControl_BankDataLoaded has Updated the Customer Grid content to the latest Db collection...." );
				Mouse . OverrideCursor = Cursors . Arrow;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				if ( sender == EditDbDetcollection ) return;
				// get current row from Event Args
				int currsel = e . RowCount;
				if ( currsel == -1 ) currsel = 0;
				this . DetailsGrid . ItemsSource = null;
				this . DetailsGrid . Items . Clear ( );
				Mouse . OverrideCursor = Cursors . Wait;
				EditDbDetcollection = await DetCollection . LoadDet ( EditDbDetcollection );
				this . DetailsGrid . ItemsSource = EditDbDetcollection;
				this . DetailsGrid . SelectedIndex = e . RowCount;
				this . DetailsGrid . Refresh ( );
				Utils . ScrollRecordInGrid ( DetailsGrid, e . RowCount );
				Debug . WriteLine ( $"EventControl_BankDataLoaded has Updated the Details Grid content to the latest Db collection...." );
				Mouse . OverrideCursor = Cursors . Arrow;
			}
		}

		public static void HandleSQLEdit ( object sender, EditEventArgs e )
		{
			//Handler for Datagrid Edit occurred delegate
			Debug . WriteLine ( $"\r\nDelegate Recieved in EDITDB (83) Caller={e . Caller}, Index = {e . CurrentIndex},   {e . DataType . ToString ( )} \r\n" );
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

		#endregion DELEGATE Handlers

		#region CONSTRUCTORS
		public EditDb ( )
		{
			// Dumy for pre-loading
		}
		public EditDb ( string Caller, int index, object Item, SqlDbViewer sqldb )
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
			// data load code Now moved to WindowLoaded() method  16/5/2021

			{
				//if ( CurrentDb == "BANKACCOUNT" || CurrentDb == "DETAILS" )
				//{
				//	this . Height = 400;
				//	this . MinHeight = 400;
				//	if ( CurrentDb == "BANKACCOUNT" )
				//	{
				//		//					dGrid = this . DataGrid1;
				//		if ( EditDbBankcollection == null || EditDbBankcollection . Count == 0 )
				//			LoadBankData ( EditDbBankcollection );
				//		//EditDbBankcollection = BankCollection . LoadBank ( 2, false );
				//		this  . DataGrid1 . ItemsSource = EditDbDetcollection;
				//	}
				//	else
				//	{
				//		//					dGrid = this . DataGrid2;
				//		LoadDetailsAsync ( );
				//		this  . DetailsGrid . ItemsSource = EditDbDetcollection;
				//		//if ( EditDbDetcollection == null || EditDbDetcollection . Count == 0 )
				//		//	DetCollection . LoadDet ( EditDbDetcollection, 2 );
				//		//this  . DetailsGrid . ItemsSource = EditDbDetcollection;
				//	}
				//}
				//else
				//{
				//	this . Height = 640;
				//	this . MinHeight = 640;
				//	//				dGrid = this . DataGrid2;
				//	if ( EditDbCustcollection == null || EditDbCustcollection . Count == 0 )
				//		CustCollection . LoadCust ( EditDbCustcollection );
				//	this  . DataGrid2 . ItemsSource = EditDbDetcollection;
				//}
				//ViewerButton . IsEnabled = false;
			}
		}
		private async Task<DetCollection> LoadDetailsAsync ( )
		{
			if ( EditDbDetcollection == null || EditDbDetcollection . Count == 0 )
				Mouse . OverrideCursor = Cursors . Wait;
			EditDbDetcollection = await DetCollection . LoadDet ( EditDbDetcollection, 2 );
			Mouse . OverrideCursor = Cursors . Arrow;
			//this  . DetailsGrid . ItemsSource = EditDbDetcollection;
			return EditDbDetcollection;
		}
		private async static Task<BankCollection> LoadBankData ( BankCollection EditDbBankcollection )
		{
			EditDbBankcollection = await BankCollection . LoadBank ( EditDbBankcollection, 2, false );
			return EditDbBankcollection;
		}

		#endregion CONSTRUCTORS

		#region General EventHandlers

		/// <summary>
		/// Callback handler we receive for a db change notification sent by an SqlDbViewer
		/// We have to update our datagrid as relevant
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="Grid"></param>Detcollection
		/// <param name="args"></param>
		public void DbChangedHandler ( SqlDbViewer sender, DataGrid Grid, DataChangeArgs args )
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
				{ Debug . WriteLine ( $"Error encountered performing :  . GetDefaultView ( Bankcollection " ); }
				this . DataGrid1 . SelectedIndex = currentrow;
			}
			if ( this . DataGrid2 . Items . Count > 0 )
			{
				// refresh our grid
				this . DataGrid2 . ItemsSource = null;
				try
				{ this . DataGrid2 . ItemsSource = CollectionViewSource . GetDefaultView ( EditDbCustcollection ); }
				catch
				{ Debug . WriteLine ( $"Error encountered performing :  . GetDefaultView ( Custcollection " ); }
				this . DataGrid2 . SelectedIndex = currentrow;
			}
			if ( this . DetailsGrid . Items . Count > 0 )
			{
				// refresh our grid
				//?					ViewerChangeType = 2;
				this . DetailsGrid . ItemsSource = null;
				try
				{ this . DetailsGrid . ItemsSource = CollectionViewSource . GetDefaultView ( EditDbDetcollection ); }
				catch { Debug . WriteLine ( $"Error encountered performing :  . GetDefaultView ( Detcollection " ); }
				this . DetailsGrid . SelectedIndex = currentrow;
			}
			return;
		}


		#endregion General EventHandlers


		#region Display utilities

		private void SetupBackgroundGradient ( )
		{
			//Get a new LinearGradientBrush
			LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush ( );
			//Set the start and end points of the drawing
			myLinearGradientBrush . StartPoint = new Point ( 1.3, 0 );
			myLinearGradientBrush . EndPoint = new Point ( 0.0, 1 );
			if ( CurrentDb == "BANKACCOUNT" )
			// Gradient Stops below are light to dark
			{
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . PowderBlue, 1.0 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . LightSteelBlue, 0.5 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . DodgerBlue, 0 ) );
				//Now paint the buttons to match
				LinearGradientBrush myLinearGradientBrush2 = new LinearGradientBrush ( );
				myLinearGradientBrush2 . StartPoint = new Point ( 0.5, 0 );
				myLinearGradientBrush2 . EndPoint = new Point ( 0, 1 );

				GradientStop gs1 = new GradientStop ( );
				GradientStop gs2 = new GradientStop ( );
				GradientStop gs3 = new GradientStop ( );
				GradientStop gs4 = new GradientStop ( );
				GradientStop gs5 = new GradientStop ( );
				GradientStop gs6 = new GradientStop ( );
				gs1 . Color = Color . FromArgb ( 0xFF, 0x6B, 0x8E, 0x95 );
				gs2 . Color = Color . FromArgb ( 0xFF, 0x14, 0xA7, 0xC1 );
				gs3 . Color = Color . FromArgb ( 0xFF, 0x1E, 0x42, 0x4E );
				gs4 . Color = Color . FromArgb ( 0xFF, 0x1D, 0x48, 0x55 );
				gs5 . Color = Color . FromArgb ( 0xFF, 0x1D, 0x48, 0x55 );
				gs6 . Color = Color . FromArgb ( 0xFF, 0x19, 0x3A, 0x44 );
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
					new GradientStop ( Colors . White, 1.0 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . Gold, 0.3 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . DarkKhaki, 0.0 ) );
				//Now paint the buttons to match
				LinearGradientBrush myLinearGradientBrush2 = new LinearGradientBrush ( );
				myLinearGradientBrush2 . StartPoint = new Point ( 0.5, 0 );
				myLinearGradientBrush2 . EndPoint = new Point ( 0.5, 1 );

				GradientStop gs1 = new GradientStop ( );
				GradientStop gs2 = new GradientStop ( );
				GradientStop gs3 = new GradientStop ( );
				GradientStop gs4 = new GradientStop ( );
				GradientStop gs5 = new GradientStop ( );
				GradientStop gs6 = new GradientStop ( );
				//Yellow buttons
				gs1 . Color = Color . FromArgb ( 0xFF, 0x7a, 0x6f, 0x2d );
				gs2 . Color = Color . FromArgb ( 0xFF, 0xf5, 0xd8, 0x16 );
				gs3 . Color = Color . FromArgb ( 0xFF, 0x7d, 0x70, 0x15 );
				gs4 . Color = Color . FromArgb ( 0xFF, 0x5e, 0x56, 0x2a );
				gs5 . Color = Color . FromArgb ( 0xFF, 0x59, 0x50, 0x13 );
				gs6 . Color = Color . FromArgb ( 0xFF, 0x38, 0x32, 0x0c );
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
					new GradientStop ( Colors . White, 1.0 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . Green, 0.5 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . DarkGreen, 0.25 ) );
				//Now paint the buttons to match
				LinearGradientBrush myLinearGradientBrush2 = new LinearGradientBrush ( );
				myLinearGradientBrush2 . StartPoint = new Point ( 0.5, 0 );
				myLinearGradientBrush2 . EndPoint = new Point ( 0.5, 1 );

				GradientStop gs1 = new GradientStop ( );
				GradientStop gs2 = new GradientStop ( );
				GradientStop gs3 = new GradientStop ( );
				GradientStop gs4 = new GradientStop ( );
				GradientStop gs5 = new GradientStop ( );
				GradientStop gs6 = new GradientStop ( );
				gs1 . Color = Color . FromArgb ( 0xFF, 0x75, 0xDD, 0x75 );
				gs2 . Color = Color . FromArgb ( 0xFF, 0x00, 0xFF, 0x00 );
				gs3 . Color = Color . FromArgb ( 0xFF, 0x33, 0x66, 0x33 );
				gs4 . Color = Color . FromArgb ( 0xFF, 0x44, 0x55, 0x44 );
				gs5 . Color = Color . FromArgb ( 0xFF, 0x33, 0x55, 0x55 );
				gs6 . Color = Color . FromArgb ( 0xff, 0x22, 0x40, 0x22 );
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

		#region Window Open/Close code
		private async void WindowLoaded ( object sender, RoutedEventArgs e )
		{
			//===============================================
			// First we get the data loaded
			//===============================================
			if ( CurrentDb == "BANKACCOUNT" || CurrentDb == "DETAILS" )
			{
				this . Height = 400;
				this . MinHeight = 400;
				if ( CurrentDb == "BANKACCOUNT" )
				{
					//					dGrid = this . DataGrid1;
					if ( EditDbBankcollection == null || EditDbBankcollection . Count == 0 )
						EditDbBankcollection = await LoadBankData ( EditDbBankcollection );
					//EditDbBankcollection = BankCollection . LoadBank ( 2, false );
					this . DataGrid1 . ItemsSource = EditDbDetcollection;
				}
				else
				{
					//					dGrid = this . DataGrid2;
					EditDbDetcollection = await LoadDetailsAsync ( );
					this . DetailsGrid . ItemsSource = EditDbDetcollection;
					//if ( EditDbDetcollection == null || EditDbDetcollection . Count == 0 )
					//	DetCollection . LoadDet ( EditDbDetcollection, 2 );
					//this  . DetailsGrid . ItemsSource = EditDbDetcollection;
				}
			}
			else
			{
				this . Height = 640;
				this . MinHeight = 640;
				//				dGrid = this . DataGrid2;
				if ( EditDbCustcollection == null || EditDbCustcollection . Count == 0 )
					await CustCollection . LoadCust ( EditDbCustcollection );
				this . DataGrid2 . ItemsSource = EditDbDetcollection;
			}
			ViewerButton . IsEnabled = false;
			// No flag on window - Yet !!
			//			if ( Flags . LinkviewerRecords )
			//				LinkRecords . IsChecked = true;

			//===============================================
			// Now we can do the final setup processing
			//===============================================
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
					//				this  . DataGrid1 . ItemsSource = CollectionViewSource . GetDefaultView ( bvm . BankAccountObs );
				}
				catch
				{
					Debug . WriteLine ( $"Error encountered performing :  . GetDefaultView ( BankCollection . Bankcollection " );
				}
				Utils . SetUpGridSelection ( DataGrid1, 0 );

				//to get it to scroll the record into view we have to go thru this palaver....
				// But it does work, but only puts it on bottom row of viewer
				DataGridNavigation . SelectRowByIndex ( this . DataGrid1, CurrentIndex, -1 );

				CurrentGrid = this . DataGrid1;
				//Setup the Event handler to notify EditDb viewer of index changes
				Debug . WriteLine ( $"EditDb(242) Window just loaded : getting instance of EventHandlers class with this,DataGrid1,\"EDITDB\"" );
				new EventHandlers ( this . DataGrid1, "EDITDB", out EventHandler );

				//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
				Flags . CurrentEditDbViewerBankGrid = this . DataGrid1;
				BankAccountViewModel . ActiveEditDbViewer = this . DataGrid1;

				Flags . CurrentEditDbViewer = this;
				Flags . CurrentEditDbViewer . Name = "BankAccount";
				Flags . ActiveEditGrid = this;

				this . DataGrid1 . Focus ( );
				this . DataGrid1 . BringIntoView ( );
				Utils . SetUpGridSelection ( this . DataGrid1, 0 );
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
					Debug . WriteLine ( $"Error encountered performing :  . GetDefaultView ( custCollection . custcollection " );
				}
				IsDirty = false;
				Utils . SetUpGridSelection ( DataGrid2, 0 );

				//to get it to scroll the record into view we have to go thru this palaver....
				// But it does work, but only puts it on bottom row of viewer
				DataGridNavigation . SelectRowByIndex ( this . DataGrid2, CurrentIndex, -1 );
				CurrentGrid = this . DataGrid2;
				//Setup the Event handler to notify EditDb viewer of index changes
				Debug . WriteLine ( $"EditDb(287) Window just loaded :  getting instance of EventHandlers class with this,DataGrid2,\"EDITDB\"" );
				//				EventHandlers . SetWindowHandles ( this, null, null );
				new EventHandlers ( this . DataGrid2, "EDITDB", out EventHandler );
				//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
				Flags . CurrentEditDbViewerCustomerGrid = this . DataGrid2;
				BankAccountViewModel . ActiveEditDbViewer = this . DataGrid2;

				Flags . CurrentEditDbViewer = this;
				Flags . CurrentEditDbViewer . Name = "Customer";
				Flags . ActiveEditGrid = this;

				this . DataGrid2 . Focus ( );
				this . DataGrid2 . BringIntoView ( );
				Utils . SetUpGridSelection ( this . DataGrid2, 0 );
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
					Debug . WriteLine ( $"Error encountered performing :  . GetDefaultView ( SqlViewerDetcollection " );
				}
				// Set the 2 control flags so that we know we have changed data when we notify other windows
				ViewerChangeType = 2;
				Utils . SetUpGridSelection ( DetailsGrid, 0 );

				//to get it to scroll the record into view we have to go thru this palaver....
				// But it does work, but only puts it on bottom row of viewer
				DataGridNavigation . SelectRowByIndex ( this . DetailsGrid, CurrentIndex, -1 );
				CurrentGrid = this . DetailsGrid;
				//Setup the Event handler to notify EditDb viewer of index changes
				Debug . WriteLine ( $"EditDb(312) Window just loaded :  getting instance of EventHandlers class with this,DataGrid1,\"EDITDB\"" );
				//				EventHandlers . SetWindowHandles ( this, null, null );
				new EventHandlers ( this . DetailsGrid, "DETAILS", out EventHandler );
				//Store pointers to our DataGrid in BOTH ModelViews for access by Data row updating code
				if ( Flags . CurrentEditDbViewer == null )
					Flags . CurrentEditDbViewer = new EditDb ( );
				Flags . CurrentEditDbViewer = this;
				Flags . CurrentEditDbViewer . DetailsGrid = this . DetailsGrid;
				Flags . CurrentEditDbViewer . Name = "Details";
				if ( Flags . ActiveEditGrid == null )
					Flags . ActiveEditGrid = new EditDb ( );
				Flags . ActiveEditGrid = this;
				ViewerButton . IsEnabled = false;
				this . Height = 400;
				this . MinHeight = 400;
				this . Refresh ( );
				this . DetailsGrid . Focus ( );
				this . DetailsGrid . BringIntoView ( );
				Utils . SetUpGridSelection ( this . DetailsGrid, 0 );
			}

			MainWindow . gv . SqlCurrentEditViewer = this;

			// Set Form wide flags to see what  actions we need to take ?  (Edited value or not)
			ViewerChangeType = 0;        // Change made in Viewer

			if ( CurrentDb == "BANKACCOUNT" )
				EventControl . BankDataLoaded += EventControl_DataLoaded;
			else if ( CurrentDb == "CUSTOMER" )
				EventControl . CustDataLoaded += EventControl_DataLoaded;
			else if ( CurrentDb == "DETAILS" )
				EventControl . DetDataLoaded += EventControl_DataLoaded;


			NotifyOfDataChange += DbChangedHandler; // Callback in THIS FILE
								//			EventControl . ViewerDataHasBeenChanged += EditDbHasChangedIndex;      // Callback in THIS FILE
			EventControl . EditIndexChanged += EventControl_ViewerIndexChanged;

			// Main update notification handler
			EventControl . ViewerDataUpdated += EventControl_ViewerDataUpdated;
			// changes made inMultiviewer
			EventControl . MultiViewerDataUpdated += EventControl_ViewerDataUpdated;

			// SqlViewer has changed index
			EventControl . ViewerIndexChanged += EventControl_ViewerIndexChanged;
			EventControl . MultiViewerIndexChanged += EventControl_ViewerIndexChanged;

			EventControl . RecordDeleted += OnDeletion;

			// set up our windows dragging
			this . MouseDown += delegate { DoDragMove ( ); };
			IsDirty = false;
			Startup = false;
		}

		private void OnDeletion ( string Source, string bankno, string custno, int CurrrentRow )
		{
			//Handle record deletion notification

		}

		/// <summary>
		/// Handkler  for when data has changed ?
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EventControl_DataLoaded ( object sender, LoadedEventArgs e )
		{
			Debug . WriteLine ( "Data changed ?" );
		}

		private void Window_Closing ( object sender, CancelEventArgs e )
		{
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
		private void Window_Closed ( object sender, EventArgs e )
		{
			// Unsubscribe form Events as needed
			if ( NotifyOfDataChange != null )
				NotifyOfDataChange -= DbChangedHandler;

			if ( CurrentDb == "BANKACCOUNT" )
				EventControl . BankDataLoaded -= EventControl_DataLoaded;
			else if ( CurrentDb == "CUSTOMER" )
				EventControl . CustDataLoaded -= EventControl_DataLoaded;
			else if ( CurrentDb == "DETAILS" )
				EventControl . DetDataLoaded -= EventControl_DataLoaded;


			//			EventControl . ViewerDataHasBeenChanged -= EditDbHasChangedIndex;

			if ( NotifyOfDataChange != null )
				NotifyOfDataChange -= DbChangedHandler;

			//			EventControl . ViewerDataHasBeenChanged -= EditDbHasChangedIndex;
			EventControl . EditIndexChanged -= EventControl_ViewerIndexChanged;
			EventControl . ViewerIndexChanged -= EventControl_ViewerIndexChanged;
			EventControl . MultiViewerIndexChanged -= EventControl_ViewerIndexChanged;

			// Changes made by SqlDbviewer window
			EventControl . ViewerDataUpdated -= EventControl_ViewerDataUpdated;
			// Changes made by Multi viewer window
			EventControl . MultiViewerDataUpdated -= EventControl_ViewerDataUpdated;
			EventControl . RecordDeleted -= OnDeletion;

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

		private void Window_GotFocus ( object sender, RoutedEventArgs e )
		{
			Flags . CurrentEditDbViewer = this;
		}
		#endregion Winow Open/Close code

		#region Window Handling Methods
		private void EventControl_ViewerIndexChanged ( object sender, IndexChangedArgs e )
		{
			if ( e . Row == -1 )
			{
				return;
			}
			//			Debug . WriteLine ( $" 6-1 *** TRACE *** EDITDB : EventControl_ViewerIndexChanged Event Handler - Entering" );
			if ( Flags . EditDbIndexIsChanging )
				return;
			// Handle  index change in another window
			if ( e . Sender == "BANKACCOUNT" )
			{
				if ( DataGrid1 . Items . Count == 0 ) return;
				//				Debug . WriteLine ( $" 6-2-END *** TRACE *** EDITDB : EventControl_ViewerIndexChanged Event Handler - Scrolling Bankaccount" );
				if ( this . DataGrid1 == null ) return;
				if ( this . DataGrid1 . SelectedIndex != e . Row )
					this . DataGrid1 . SelectedIndex = e . Row;
				// force record to scroll into view correctly
				Utils . ScrollRecordInGrid ( DataGrid1, e . Row );
			}
			else if ( e . Sender == "CUSTOMER" )
			{
				if ( DataGrid2 . Items . Count == 0 ) return;
				//				Debug . WriteLine ( $" 6-2-END *** TRACE *** EDITDB : EventControl_ViewerIndexChanged Event Handler - Scrolling Customer" );
				if ( this . DataGrid2 == null ) return;
				if ( DataGrid2 . SelectedIndex != e . Row )
				{
					this . DataGrid2 . SelectedIndex = e . Row;
					this . DataGrid2 . SelectedItem = e . Row;
				}
				// force record to scroll into view correctly
				Utils . ScrollRecordInGrid ( DataGrid2, e . Row );
			}
			else if ( e . Sender == "DETAILS" )
			{
				if ( DetailsGrid . Items . Count == 0 ) return;
				//				Debug . WriteLine ( $" 6-2-END *** TRACE *** EDITDB : EventControl_ViewerIndexChanged Event Handler - Scrolling Details" );
				if ( this . DetailsGrid == null ) return;
				if ( this . DetailsGrid . SelectedIndex != e . Row )
					this . DetailsGrid . SelectedIndex = e . Row;
				// force record to scroll into view correctly
				Utils . ScrollRecordInGrid ( DetailsGrid, e . Row );
			}
			//			Debug . WriteLine ( $" 6-3-END *** TRACE *** EDITDB : EventControl_ViewerIndexChanged Event Handler - Exiting" );
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

		private void Window_PreviewKeyDown ( object sender, KeyEventArgs e )
		{
			//			Debug . WriteLine ( $"Key : {e . Key}" );
			DataGrid dg = null;
			if ( Utils . DataGridHasFocus ( this ) == false )
				return;
			//if ( e . Key == Key . Escape )
			//	Close ( );
			if ( e . Key == Key . LeftCtrl )
			{
				key1 = true;
				return;
			}
			else if ( key1 && e . Key == Key . F9 )    // CTRL + F9
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

			else if ( e . Key == Key . RightAlt )
			{
				Flags . ListGridviewControlFlags ( );
				key1 = false;
			}
			else if ( e . Key == Key . Up )
			{
				if ( CurrentDb == "BANKACCOUNT" )
					dg = this . DataGrid1;
				else if ( CurrentDb == "CUSTOMER" )
					dg = this . DataGrid2;
				else
					dg = this . DetailsGrid;
				if ( dg . SelectedIndex > 0 )
					dg . SelectedIndex--;

				if ( dg . SelectedItem != null )
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				key1 = false;
			}
			else if ( e . Key == Key . Down )
			{
				if ( CurrentDb == "BANKACCOUNT" )
					dg = this . DataGrid1;
				else if ( CurrentDb == "CUSTOMER" )
					dg = this . DataGrid2;
				else
					dg = this . DetailsGrid;
				if ( dg . SelectedIndex < dg . Items . Count - 1 )
					dg . SelectedIndex++;

				if ( dg . SelectedItem != null )
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
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
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
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
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
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
						Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				}
				else
				{
					dg . SelectedIndex = dg . Items . Count - 1;
					if ( dg . SelectedItem != null )
						Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
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
						Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				}
				else
				{
					dg . SelectedIndex = 0;
					if ( dg . SelectedItem != null )
						Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
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
				Flags . CurrentSqlViewer . Window_PreviewKeyDown ( sender, e );
			}
			if ( dg != null )
			{
				// Now process it
				if ( dg == this . DataGrid1 )
					DataGrid1_SelectionChanged ( dg, null );
				else if ( dg == this . DataGrid2 )
					DataGrid2_SelectionChanged ( dg, null );
				else if ( dg == this . DetailsGrid )
					DetailsGrid_SelectionChanged ( dg, null );
				if ( dg . SelectedItem != null )
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );

				e . Handled = true;
			}
			key1 = false;
		}

		// Window is closing via Close Button Click event
		private void Button_Click ( object sender, RoutedEventArgs e )
		{
			// Window is being closed
			//			BankAccountViewModel . EditdbWndBank = null;
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
				PropertyChanged ( this,
					new PropertyChangedEventArgs ( PropertyName ) );
			}
		}

		#endregion INotifyPropertyChanged Members	

		#region RowEdithandlers

		//Bank Grid
		private async void DataGrid1_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
		{
			//=====================================
			// ENTRY POINT when a data change has occurred
			//=====================================
			int currsel = 0;

			//// This ONLY called when a cell is edited
			var sqlh = new SQLHandlers ( );
			Flags . EditDbDataChanged = true;
			// Set Form wide flags to see what  actions we need to take ?  (Edited value or not)
			ViewerChangeType = 0;        // Change made in Viewer
						     //Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
			if ( e == null )
			{
				// Row Deleted ???
				BankAccountViewModel . SqlUpdating = true;
				Debug . WriteLine ( $"DataGrid1_RowEditEnding() Starting Db Update " );
				sqlh . UpdateDbRowAsync ( CurrentDb, this . DataGrid1 . SelectedItem, this . DataGrid1 . SelectedIndex );
				Debug . WriteLine ( $"DataGrid1_RowEditEnding() Db Update finished" );
				this . DataGrid1 . SelectedIndex = EditdbchangeInProgress;
				Debug . WriteLine ( $"DataGrid1_RowEditEnding() Db Update finished" );
				Flags . DataLoadIngInProgress = true;
				SendDataChanged ( CurrentDb );
			}
			else
			{
				//				Debug . WriteLine ( $" 2-1 *** TRACE *** EDITDB : DataGrid1_RowEditEnding - Entering to trigger SendDataChanged" );
				// Row has been changed
				BankAccountViewModel . SqlUpdating = true;
				//				Debug . WriteLine ( $" 2-2 *** TRACE *** EDITDB : DataGrid1_RowEditEnding() Updating Db Row ({this . DataGrid1 . SelectedIndex})" );
				sqlh . UpdateDbRow ( CurrentDb, this . DataGrid1 . SelectedItem );
				//				Debug . WriteLine ( $" 2-3 *** TRACE *** EDITDB : DataGrid1_RowEditEnding({this . DataGrid1 . SelectedIndex}) Sending \"SendDataChanged\" Event Trigger" );
				SendDataChanged ( CurrentDb );
				//				Debug . WriteLine ( $" 2-4 *** TRACE *** EDITDB : DataGrid1_RowEditEnding({this . DataGrid1 . SelectedIndex})  \"SendDataChanged\" Event Trigger sent" );
			}
			//			Debug . WriteLine ( $" 2-5-END *** TRACE *** EDITDB : DataGrid1_RowEditEnding - Exiting\n*** Should *** be Processing completed...\n" );
			Flags . EditDbDataChanged = false;
		}

		//Customer grid
		private async void DataGrid2_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
		{
			//// This ONLY called when a cell is edited
			var sqlh = new SQLHandlers ( );
			Flags . EditDbDataChanged = true;
			// Set Form wide flags to see what  actions we need to take ?  (Edited value or not)
			ViewerChangeType = 0;        // Change made in Viewer
						     //Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
			if ( e == null )
			{
				// Row Deleted ???
				CustomerViewModel . SqlUpdating = true;
				Debug . WriteLine ( $"DataGrid2_RowEditEnding({this . DataGrid2 . SelectedIndex}) Starting Db Update " );
				sqlh . UpdateDbRow ( CurrentDb, this . DataGrid2 . SelectedItem );
				Debug . WriteLine ( $"DataGrid2_RowEditEnding({this . DataGrid2 . SelectedIndex}) Db Update finished" );
				// Crucial flag for updating
				Flags . DataLoadIngInProgress = true;
				SendDataChanged ( CurrentDb );
			}
			else
			{
				// Row has been changed
				//				Debug . WriteLine ( $" 2-1 *** TRACE *** EDITDB : DataGrid2_RowEditEnding - Entering to trigger SendDataChanged" );
				CustomerViewModel . SqlUpdating = true;
				//				Debug . WriteLine ( $" 2-2 *** TRACE *** EDITDB : DataGrid2_RowEditEnding() Updating Db Row ({this . DataGrid1 . SelectedIndex})" );
				sqlh . UpdateDbRow ( CurrentDb, this . DataGrid2 . SelectedItem );
				//				Debug . WriteLine ( $" 2-3 *** TRACE *** EDITDB : DataGrid2_RowEditEnding({this . DataGrid1 . SelectedIndex}) Sending \"SendDataChanged\" Event Trigger" );
				SendDataChanged ( CurrentDb );
				//				Debug . WriteLine ( $" 2-4 *** TRACE *** EDITDB : DataGrid2_RowEditEnding({this . DataGrid1 . SelectedIndex})  \"SendDataChanged\" Event Trigger sent" );
			}
			//			Debug . WriteLine ( $" 2-5-END *** TRACE *** EDITDB : DataGrid2_RowEditEnding - Exiting\n*** Should *** be Processing completed...\n" );
			Flags . EditDbDataChanged = false;
		}

		//Details Grid
		private async void DetailsGrid_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
		{
			//// This ONLY called when a cell is edited
			var sqlh = new SQLHandlers ( );
			Flags . EditDbDataChanged = true;
			//Sort out the data as this Fn is called with null,null as arguments when a row is DELETED
			if ( e == null )
			{
				// Row Deleted ???
				DetailsViewModel . SqlUpdating = true;
				//				Debug . WriteLine ( $"DetailsGrid_RowEditEnding({this  . DetailsGrid . SelectedIndex}) Starting Db Update " );
				sqlh . UpdateDbRow ( CurrentDb, this . DataGrid1 . SelectedItem );
				//				Debug . WriteLine ( $"DetailsGrid_RowEditEnding({this  . DetailsGrid . SelectedIndex}) Db Update finished" );
				//Flags . EditDbDataChanged = true;
				Flags . DataLoadIngInProgress = true;
				SendDataChanged ( CurrentDb );
				//				Debug . WriteLine ( $"DetailsGrid_RowEditEnding() Broadcasting Changes" );
			}
			else
			{
				// Row data has been changed, update the Db's first, then notify other viewers
				//				Debug . WriteLine ( $" 2-1 *** TRACE *** EDITDB : DetailsGrid_RowEditEnding - Entering to trigger SendDataChanged" );
				DetailsViewModel . SqlUpdating = true;
				//				Debug . WriteLine ( $" 2-2 *** TRACE *** EDITDB : Details_RowEditEnding() Updating Db Row ({this . DataGrid1 . SelectedIndex})" );
				sqlh . UpdateDbRow ( CurrentDb, this . DetailsGrid . SelectedItem );
				//				Debug . WriteLine ( $" 2-3 *** TRACE *** EDITDB : Details RowEditEnding({this . DataGrid1 . SelectedIndex}) Sending \"SendDataChanged\" Event Trigger" );
				//				Debug . WriteLine ( $"DetailsGrid_RowEditEnding({this  . DetailsGrid . SelectedIndex}) Db Update finished" );
				SendDataChanged ( CurrentDb );
				//				Debug . WriteLine ( $" 2-4 *** TRACE *** EDITDB : Details_RowEditEnding({this . DataGrid1 . SelectedIndex})  \"SendDataChanged\" Event Trigger sent" );
			}
			//			Debug . WriteLine ( $" 2-5-END *** TRACE *** EDITDB : DetailsGrid_RowEditEnding - Exiting\n*** Should *** be Processing completed...\n" );
			Flags . EditDbDataChanged = false;
		}
		public void SendDataChanged ( string dbName )
		{
			// Databases have DEFINITELY been updated successfully after a change
			// We Now Broadcast this to ALL OTHER OPEN VIEWERS here and now

			if ( dbName == "BANKACCOUNT" )
			{
//				Debug . WriteLine ( $" 3-1 *** TRACE *** EDITDB : SENDDATACHANGED  Sending BANKACCOUNT TriggerEditDbDataUpdate Event trigger" );
				EventControl . TriggerEditDbDataUpdated ( EditDbBankcollection,
					new LoadedEventArgs
					{
						CallerDb = "BANKACCOUNT",
						DataSource = EditDbBankcollection,
						RowCount = this . DataGrid1 . SelectedIndex
					} );
			}
			else if ( dbName == "CUSTOMER" )
			{
//				Debug . WriteLine ( $" 3-2 *** TRACE *** EDITDB : SENDDATACHANGED  Sending CUSTOMER TriggerEditDbDataUpdate Event trigger" );
				EventControl . TriggerEditDbDataUpdated ( EditDbCustcollection,
					new LoadedEventArgs
					{
						CallerDb = "CUSTOMER",
						DataSource = EditDbCustcollection,
						RowCount = this . DataGrid2 . SelectedIndex
					} );
			}
			else if ( dbName == "DETAILS" )
			{
//				Debug . WriteLine ( $" 3-3 *** TRACE *** EDITDB : SENDDATACHANGED  Sending DETAILS TriggerEditDbDataUpdate Event trigger" );
				Flags . EditDbDataChange = true;
				EventControl . TriggerEditDbDataUpdated ( EditDbDetcollection,
					new LoadedEventArgs
					{
						CallerDb = "DETAILS",
						DataSource = EditDbDetcollection,
						RowCount = this . DetailsGrid . SelectedIndex
					} );
				Flags . EditDbDataChange = false;
			}
			Mouse . OverrideCursor = Cursors . Arrow;
		}

		#endregion RowEdithandlers

		#region RowSelection handlers

		private static bool SelectDone = false;
		// BankAccount
		/// <summary>
		/// Receives the notification from the main db viewer that a selection has been changed
		/// and sends it to that same viewer when changed in this window
		/// so both windows update the current row simaltaneously.
		/// </summary>
		public void DataGrid1_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			//			Debug . WriteLine ( $" 1-1 *** TRACE *** EDITDB : DataGrid1_SelectionChanged - Entering\n" );
			if ( SelectDone )
			{
				SelectDone = false;
				return;
			}
			SelectDone = true;
			if ( this . DataGrid1 . SelectedIndex == -1 ) return;
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

			IsDirty = false;
			BankEditFields . DataContext = this . DataGrid1 . SelectedItem;
			Utils . ScrollRecordIntoView ( this . DataGrid1, this . DataGrid1 . SelectedIndex );

			Flags . EditDbIndexIsChanging = true;
			// do NOT triger this if Viewer has triggered the index change
			if ( Flags . SqlViewerIndexIsChanging == false )
			{
//				Debug . WriteLine ( $" 4-2 *** TRACE *** EDITDB : DataGrid1_SelectionChanged BANKACCOUNT - Sending TriggerEditDbIndexChanged" );
				EventControl . TriggerEditDbIndexChanged ( this,
						new IndexChangedArgs
						{
							dGrid = this . DataGrid1,
							Sender = "BANKACCOUNT",
							Row = this . DataGrid1 . SelectedIndex
						} );
			}
			//			else
			//				Debug . WriteLine ( $" 1-2 *** TRACE *** EDITDB : DataGrid1_SelectionChanged - TriggerEditDbIndexChanged IGNORED" );


			ViewerButton . IsEnabled = false;
			IsDirty = false;
			Flags . EditDbIndexIsChanging = false;
			SelectDone = false;
			e . Handled = true;
			//			Debug . WriteLine ( $" 1-3-END *** TRACE *** EDITDB : DataGrid1_SelectionChanged - Sent TriggerEditDbIndexChanged: Handled = true, Exiting...\n" );
		}

		//Customer Db
		/// <summary>
		/// Receives the notification from the main db viewer that a selection has been changed
		/// and sends it to that same viewer when changed in this window
		/// so both windows update the current row simaltaneously.
		/// </summary>
		public void DataGrid2_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			//			Debug . WriteLine ( $" 1-1 *** TRACE *** EDITDB : DataGrid1_SelectionChanged - Entering\n" );
			if ( SelectDone )
			{
				SelectDone = false;
				return;
			}
			SelectDone = true;
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
			//			Utils . ScrollRecordIntoView ( this . DataGrid2 );
			IsDirty = false;
			CustomerEditFields . DataContext = this . DataGrid2 . SelectedItem;
			Utils . ScrollRecordIntoView ( this . DataGrid2, this . DataGrid2 . SelectedIndex );

			Flags . EditDbIndexIsChanging = true;
			// do NOT triger this if Viewer has triggered the index change
			if ( Flags . SqlViewerIndexIsChanging == false )
			{
//				Debug . WriteLine ( $" 4-2 *** TRACE *** EDITDB : DataGrid2_SelectionChanged CUSTOMER - Sending TriggerEditDbIndexChanged" );
				EventControl . TriggerEditDbIndexChanged ( this,
				new IndexChangedArgs
				{
					dGrid = this . DataGrid2,
					Sender = "CUSTOMER",
					Row = this . DataGrid2 . SelectedIndex
				} );
			}
			ViewerButton . IsEnabled = false;
			IsDirty = false;
			Flags . EditDbIndexIsChanging = false;
			SelectDone = false;
			//			Debug . WriteLine ( $" 1-3-END *** TRACE *** EDITDB : DataGrid1_SelectionChanged - Sent TriggerEditDbIndexChanged: Handled = true, Exiting...\n" );

		}

		// Details
		/// <summary>
		/// Receives the notification from the main db viewer that a selection has been changed
		/// and sends it to that same viewer when changed in this window
		/// so both windows update the current row simultaneously.
		/// </summary>
		public void DetailsGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			//			Debug . WriteLine ( $" 1-1 *** TRACE *** EDITDB : DataGrid1_SelectionChanged - Entering\n" );
			if ( SelectDone )
			{
				SelectDone = false;
				return;
			}
			SelectDone = true;
			if ( this . DetailsGrid . SelectedIndex == -1 ) return;
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
			Utils . ScrollRecordIntoView ( this . DetailsGrid, this . DetailsGrid . SelectedIndex );

			Flags . EditDbIndexIsChanging = true;
			// do NOT triger this if Viewer has triggered the index change
			if ( Flags . SqlViewerIndexIsChanging == false )
			{
//				Debug . WriteLine ( $" 4-2 *** TRACE *** EDITDB : DetailsGrid_SelectionChanged DETAILS - Sending TriggerEditDbIndexChanged" );
				EventControl . TriggerEditDbIndexChanged ( this,
				       new IndexChangedArgs
				       {
					       dGrid = this . DetailsGrid,
					       Sender = "DETAILS",
					       Row = this . DetailsGrid . SelectedIndex
				       } );
				Flags . SqlViewerIndexIsChanging = false;
			}

			ViewerButton . IsEnabled = false;
			IsDirty = false;
			Flags . EditDbIndexIsChanging = false;
			SelectDone = false;
			//			Debug . WriteLine ( $" 1-3-END *** TRACE *** EDITDB : DataGrid1_SelectionChanged - Sent TriggerEditDbIndexChanged: Handled = true, Exiting...\n" );
		}

		#endregion RowSelection handlers

		//Bank Edit fields

		#region Bank Editing fields

		private void ActypeEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }

		private void BanknoEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }

		private void CustNoEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }
		private void BalanceEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }

		private void IntRateEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }

		private void OpenDateEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }

		private void CloseDateEdit_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }

		#endregion Bank Editing fields

		//Customer edit fields

		#region Customer Editing fields

		private void BanknoEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }
		private void CustnoEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }
		private void FirstnameEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }
		private void LastnameEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }
		private void TownEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }
		private void AcTypeEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }

		private void Addr1Edit2_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }

		private void Addr2Edit2_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }

		private void MobileEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }

		private void PhoneEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }
		private void CountyEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }

		private void PcodeEdit2_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }

		private async void ODate2_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }
		private async void CDate2_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }
		private async void Dob2_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }
		#endregion Customer Editing fields

		// Details Edit fields

		#region Details Editing fields

		private void ActypeEdit3LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }

		private void CustnoEdit3_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }

		private void BanknoEdit3_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }

		private void BalanceEdit3_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }

		private void IntRateEdit3_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }
		private void OpenDateEdit3_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }
		private void CloseDateEdit3_LostFocus ( object sender, RoutedEventArgs e )
		{ EditStart = true; ViewerButton . IsEnabled = EditStart; }

		#endregion Details Editing fields

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

		private void DataGrid1_PreviewMouseDown ( object sender, MouseButtonEventArgs e )
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
				//				Flags . CurrentSqlViewer . BankGrid . ItemsSource = null;
				//				Flags . CurrentSqlViewer . BankGrid . ItemsSource = EditDbBankcollection;
				//				Flags . CurrentSqlViewer . BankGrid . Refresh ( );
			}
			else
				e . Handled = false;
		}

		private void DataGrid2_PreviewMouseDown ( object sender, MouseButtonEventArgs e )
		{
			// handle flags to let us know WE have triggered the selectedIndex change
			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			if ( e . ChangedButton == MouseButton . Right )
			{
				DataGridRow RowData;
				int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
				rip = new RowInfoPopup ( "CUSTOMER", DataGrid2, RowData );
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

		private void DetailsGrid_PreviewMouseDown ( object sender, MouseButtonEventArgs e )
		{
			// handle flags to let us know WE have triggered the selectedIndex change
			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			if ( e . ChangedButton == MouseButton . Right )
			{
				DataGridRow RowData;
				int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
				RowInfoPopup rip = new RowInfoPopup ( "DETAILS", this . DetailsGrid, RowData );
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


		private async void SaveChanges_Click ( object sender, RoutedEventArgs e )
		{
			DataGrid dGrid = null;
			// Save data
			//RoutedEventArgs ra =new RoutedEventArgs();
			//ra = e.OriginalSource  as RoutedEventArgs;
			//RoutedEvent  re = ra . RoutedEvent as RoutedEvent;
			//Type t = re.HandlerType;

			//			int currsel = dGrid . SelectedIndex;
			Flags . EditDbDataChanged = true;
			// NB the Grid on here now shows the New Data content, as does the grid's SelectedItem
			//So we ought to call a method to save the change made....
			//Now update   the Db via Sql - WORKS FINE 3/5/21
			//9/5/21 :  Now we gotta update other open Grid Viewers
			SQLHandlers sqlh = new SQLHandlers ( );

			if ( CurrentDb == "BANKACCOUNT" )
			{
				int currsel = this . DataGrid1 . SelectedIndex;
				dGrid = DataGrid1;
				Mouse . OverrideCursor = Cursors . Wait;
				sqlh . UpdateDbRow ( CurrentDb, this . DataGrid1 . SelectedItem );
				IsDirty = false;
				EditDbBankcollection = await BankCollection . LoadBank ( EditDbBankcollection, 2 );
				dGrid . ItemsSource = null;
				dGrid . ItemsSource = EditDbBankcollection;
				dGrid . SelectedIndex = currsel;
				// Crucial flag for updating
				Flags . DataLoadIngInProgress = true;
				SendDataChanged ( CurrentDb );
				dGrid . Refresh ( );
				Utils . ScrollRecordInGrid ( dGrid, currsel );
				dGrid . SelectedIndex = currsel;
				Mouse . OverrideCursor = Cursors . Arrow;

			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				int currsel = this . DataGrid2 . SelectedIndex;
				dGrid = DataGrid2;
				Mouse . OverrideCursor = Cursors . Wait;
				sqlh . UpdateDbRow ( CurrentDb, this . DataGrid2 . SelectedItem );
				IsDirty = false;
				EditDbCustcollection = await CustCollection . LoadCust ( EditDbCustcollection );
				dGrid . ItemsSource = null;
				dGrid . ItemsSource = EditDbCustcollection;
				dGrid . SelectedIndex = currsel;
				// Crucial flag for updating
				Flags . DataLoadIngInProgress = true;
				SendDataChanged ( CurrentDb );
				dGrid . Refresh ( );
				Utils . ScrollRecordInGrid ( dGrid, currsel );
				dGrid . SelectedIndex = currsel;
				Mouse . OverrideCursor = Cursors . Arrow;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				int currsel = this . DetailsGrid . SelectedIndex;
				dGrid = DetailsGrid;
				Mouse . OverrideCursor = Cursors . Wait;
				sqlh . UpdateDbRow ( CurrentDb, this . DetailsGrid . SelectedItem );
				IsDirty = false;
				EditDbDetcollection = await DetCollection . LoadDet ( EditDbDetcollection, 2 );
				dGrid . ItemsSource = null;
				dGrid . ItemsSource = EditDbDetcollection;
				dGrid . SelectedIndex = currsel;
				// Crucial flag for updating
				Flags . DataLoadIngInProgress = true;
				SendDataChanged ( CurrentDb );
				dGrid . Refresh ( );
				Utils . ScrollRecordInGrid ( dGrid, currsel );
				dGrid . SelectedIndex = currsel;
				Mouse . OverrideCursor = Cursors . Arrow;
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



		#endregion Debug support methods

		private void Data_TextChanged ( object sender, TextChangedEventArgs e )
		{
			EditStart = true;
			if ( Startup ) return;
			if ( Utils . DataGridHasFocus ( this ) == false )
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
		}

		#region UNUSED CODE
		//public static Delegate [ ] GetEventCount10 ( )
		//{
		//	Delegate [ ] dglist2 = null;
		//	if ( AllViewersUpdate != null )
		//		dglist2 = AllViewersUpdate?.GetInvocationList ( );
		//	return dglist2;
		//}

		private async Task RefreshItemsSource ( DataGrid Grid )
		{
			////// Set our pointer so Viewer Click can work
			//int currsel = Grid . SelectedIndex;
			//Flags . EditDbDataChanged = true;
			//// NB the Grid on here now shows the New Data content, as does the grid's SelectedItem
			////So we ought to call a method to save the change made....
			////Now update   the Db via Sql - WORKS FINE 3/5/21
			////9/5/21 :  Now we gotta update other open Grid Viewers
			//SQLHandlers sqlh = new SQLHandlers ( );

			//if ( CurrentDb == "BANKACCOUNT" )
			//{
			//	Mouse . OverrideCursor = Cursors . Wait;
			//	sqlh . UpdateDbRow ( CurrentDb, Grid . SelectedItem );
			//	BankCollection . LoadBank ( EditDbBankcollection , 2, false );
			//	Grid . ItemsSource = null;
			//	Grid . ItemsSource = EditDbBankcollection;
			//	Grid . SelectedIndex = currsel;
			//	Grid . Refresh ( );
			//	Utils . ScrollRecordInGrid ( Grid, currsel );
			//	Mouse . OverrideCursor = Cursors . Arrow;
			//}
			//else if ( CurrentDb == "CUSTOMER" )
			//{
			//	Mouse . OverrideCursor = Cursors . Wait;
			//	sqlh . UpdateDbRow ( CurrentDb, Grid . SelectedItem );
			//	CustCollection . LoadCust ( EditDbCustcollection );
			//	Grid . ItemsSource = null;
			//	Grid . ItemsSource = EditDbCustcollection;
			//	IsDirty = false;
			//	Grid . SelectedIndex = currsel;
			//	Grid . Refresh ( );
			//	Utils . ScrollRecordInGrid ( Grid, currsel );
			//	Mouse . OverrideCursor = Cursors . Arrow;
			//}
			//else if ( CurrentDb == "DETAILS" )
			//{
			//	Mouse . OverrideCursor = Cursors . Wait;
			//	sqlh . UpdateDbRow ( CurrentDb, Grid . SelectedItem );
			//	//				DetCollection dc = new DetCollection();
			//	//				Detcollection = await dc . LoadDetailsTaskInSortOrderAsync ( true , Grid . SelectedIndex );
			//	DetCollection . LoadDet ( EditDbDetcollection, 2 );
			//	Grid . ItemsSource = null;
			//	Grid . ItemsSource = EditDbDetcollection;
			//	Grid . SelectedIndex = currsel;
			//	Grid . Refresh ( );
			//	Utils . ScrollRecordInGrid ( Grid, currsel );
			//	// Crucial flag for updating
			//	Flags . DataLoadIngInProgress = true;
			//	Mouse . OverrideCursor = Cursors . Arrow;
			//}
			//// now we need to tell anny other viewers about the changes
			//Flags . EditDbDataChanged = false;
			//IsDirty = false;
			//EditStart = false;
		}

		//private async void DataGrid1_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		//{
		////	int currindx = this  . DataGrid1 . SelectedIndex;
		//	var curritem = this  . DataGrid1 . SelectedItem;

		//	//Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
		//	if ( e == null )
		//	{
		//		if ( CurrentDb == "BANKACCOUNT" )
		//		{
		//			SQLHandlers sqlh = new SQLHandlers ( );
		//			sqlh . UpdateDbRow ( "BANKACCOUNT", e . Row );
		//			SqlUpdating = true;
		//			//return;
		//		}
		//	}
		//	else
		//	{
		//		if ( CurrentDb == "BANKACCOUNT" )
		//		{
		//			BankAccountViewModel . SqlUpdating = true;
		//			SQLHandlers sqlh = new SQLHandlers ( );
		//			// This call updates the SQL Db and the main viewer is also updated correctly
		//			sqlh . UpdateDbRow ( "BANKACCOUNT", e . Row );
		//			//					if ( Flags . CurrentSqlViewer . BankGrid . SelectedItem != null )
		//			//						Flags . CurrentSqlViewer . BankGrid . ScrollIntoView ( curritem );
		//			//return;
		//		}
		//	}
		//	//Reset our selected row
		//	this  . DataGrid1 . SelectedIndex = currindx;
		//	this  . DataGrid1 . SelectedItem = currindx;
		//}

		//private async void DataGrid2_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		//{
		//	int currindx = this  . DataGrid2 . SelectedIndex;
		//	var curritem = this  . DataGrid2 . SelectedItem;

		//	//Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
		//	if ( e == null )
		//	{
		//		if ( CurrentDb == "CUSTOMER" )
		//		{
		//			SQLHandlers sqlh = new SQLHandlers ( );
		//			sqlh . UpdateDbRow ( "CUSTOMER", e . Row );
		//			SqlUpdating = true;
		//			return;
		//		}
		//	}
		//	else
		//	{
		//		if ( CurrentDb == "CUSTOMER" )
		//		{
		//			BankAccountViewModel . SqlUpdating = true;
		//			SQLHandlers sqlh = new SQLHandlers ( );
		//			sqlh . UpdateDbRow ( "CUSTOMER", e . Row );
		//			return;
		//		}
		//	}
		//	//Reset our selected row
		//	this  . DataGrid2 . SelectedIndex = currindx;
		//	this  . DataGrid2 . SelectedItem = curritem;
		//}

		//private async void DetailsGrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		//{
		//	int currindx = this  . DetailsGrid . SelectedIndex;
		//	var curritem = this  . DetailsGrid . SelectedItem;

		//	//Sort out the data as this Fn is called with null,null as arguments when a/c is "Closed"
		//	if ( e == null )
		//	{
		//		if ( CurrentDb == "DETAILS" )
		//		{
		//			SQLHandlers sqlh = new SQLHandlers ( );
		//			sqlh . UpdateDbRow ( "DETAILS", e . Row );
		//			SqlUpdating = true;
		//		}
		//	}
		//	else
		//	{
		//		if ( CurrentDb == "DETAILS" )
		//		{
		//			DetailsViewModel . SqlUpdating = true;
		//			SQLHandlers sqlh = new SQLHandlers ( );
		//			sqlh . UpdateDbRow ( "DETAILS", e . Row );
		//			//SqlUpdating = true;
		//			//if ( ViewerChangeType == 0 || EditChangeType == 1 )
		//			//{
		//			//	EditChangeType = 1;
		//			//	NotifyviewerOfDataChange (this  . DetailsGrid . SelectedIndex, "DETAILS" , DetailsGrid.SelectedItem);
		//			//}
		//		}
		//	}

		//	//Reset our selected row
		//	this  . DetailsGrid . SelectedIndex = currindx;
		//	this  . DetailsGrid . SelectedItem = curritem;
		//	//e . Cancel = true;
		//}

		/// <summary>
		/// We trigger this to tell Sql Viewer to update its grid data after we have changed it.
		/// Created 10 May 2021
		/// </summary>
		/// <param name="selectedindex"></param>
		/// <param name="currentDb"></param>
		/// <param name="selecteditem"></param>
		private void NotifyviewerOfDataChange ( int selectedindex, string currentDb, object selecteditem )
		{
			dca . DbName = CurrentDb;
			dca . SenderName = null;
			Flags . SqlDetViewer . UpdateDetailsOnEditDbChange ( currentDb, selectedindex, selecteditem );
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
		public void OnSqlViewerGridChanged ( int DbChangeType, int row, string CurrentDb )
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
					//					this  . DataGrid1 . ItemsSource = bvm . BankAccountObs;
				}
				this . DataGrid1 . SelectedItem = null;  //Clear current selection to avoid multiple selections
				this . DataGrid1 . SelectedIndex = row;
				this . DataGrid1 . SelectedItem = row;
				if ( this . DataGrid1 . SelectedItem != null )
					Utils . ScrollRecordInGrid ( this . DataGrid1, this . DataGrid1 . SelectedIndex );
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
					Utils . ScrollRecordInGrid ( this . DataGrid2, this . DataGrid2 . SelectedIndex );
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
				//				this  . DetailsGrid . SelectedItem = null;  //Clear current selection to avoid multiple selections

				// This triggers a call  to DetailsGrid_SelectionChanged in this file
				this . DetailsGrid . SelectedIndex = row;
				this . DetailsGrid . SelectedItem = row;
				if ( this . DetailsGrid . SelectedItem != null )
					Utils . ScrollRecordInGrid ( this . DetailsGrid, this . DetailsGrid . SelectedIndex );
				DetailsEditFields . DataContext = this . DetailsGrid . SelectedItem;
			}
			// Reset flags
			EditStart = false;
			ViewerButton . IsEnabled = EditStart;
			ViewerChangeType = 0;
		}


		#endregion UNUSED CODE

	}
}
