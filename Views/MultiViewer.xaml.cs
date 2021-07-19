using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Linq;
using System . Net . Mail;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Input;
using System . Windows . Media;
using Newtonsoft . Json . Linq;
using Newtonsoft . Json;
using WPFPages . Commands;
using WPFPages . ViewModels;
using DataGrid = System . Windows . Controls . DataGrid;
using System . IO;
using static System . Windows . Forms . VisualStyles . VisualStyleElement . ProgressBar;
using System . Xml . Linq;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for MultiViewer.xaml
	/// </summary>
	public partial class MultiViewer : Window
	{
		public DetCollection BankMultiViewerDbcollection = null;
		public DetCollection CustMultiViewerDbcollection = null;
		public DetCollection DetMultiViewerDbcollection = null;

		public BankCollection MBankcollection = null;
		public CustCollection MCustcollection = null;
		public DetCollection MDetcollection = null;
		public Stopwatch stopwatch1 = new Stopwatch ( );
		public Stopwatch stopwatch2 = new Stopwatch ( );
		public Stopwatch stopwatch3 = new Stopwatch ( );

		public Point _startPoint { get; set; }
		// These MAINTAIN setting values across instances !!!
		public static int bindex { get; set; }
		public static  int cindex { get; set; }
		public static int dindex { get; set; }
		public int CurrentSelection { get; set; }
		public bool key1 { get; set; }
		public bool GridsLinked { get; set; }
		public bool ScrollBarMouseMove { get; set; }
		public static RoutedCommand CloseApp = new RoutedCommand ( );

		#region DECLARATIONS

		public string CurrentDb { get; set; }
		private bool inprogress { get; set; }
		private bool Triggered { get; set; }
		private bool ReloadingData { get; set; }
		private bool IsEditing { get; set; }
		public bool isLoading { get; set; }
		public bool IsDirty { get; set; }
		public bool LoadingDbData { get; set; }

		List<string> tmp3 = new List<string> ( );

		public bool IsLeftButtonDown { get; set; }
		#endregion DECLARATIONS

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
		public scrollData ScrollData = new scrollData ( );
		#region STARTUP/CLOSE

		public MultiViewer ( )
		{
			InitializeComponent ( );
			//CommandBinding myCommandBinding = new CommandBinding ( CustomCommands.myCommand, CustomCommands . ExecutedmyCommand, CustomCommands . CanExecutemyCommand );
			// attach CommandBinding to root element
			//			this . CommandBindings . Add ( myCommandBinding );
			this . Show ( );

			//Identify individual windows for update protection
			this.Tag = (Guid)Guid.NewGuid();

			this. WaitMessage . Visibility = Visibility . Visible;
			this . Refresh ( );
			tmp3 . Add ( $"Please wait, The system is loading the data from 3 seperate SQL Databases..." );
			//			tmp3 . Add ( $"This process can take a few soconds or so." );
			// Show our wait message initially
			WaitMessage . ItemsSource = tmp3;
			WaitMessage . SelectedIndex = 0;
			WaitMessage . SelectedItem = 0;
			WaitMessage . CurrentItem = 1;
			WaitMessage . Refresh ( );
			this . Refresh ( );
		}
		private async void Window_Loaded ( object sender, RoutedEventArgs e )
		{
			Mouse . OverrideCursor = Cursors . Wait;
			inprogress = true;

			string ndx = ( string ) Properties . Settings . Default [ "Multi_bindex" ];
			bindex = int . Parse ( ndx );
			ndx = ( string ) Properties . Settings . Default [ "Multi_cindex" ];
			cindex = int . Parse ( ndx );
			ndx = ( string ) Properties . Settings . Default [ "Multi_dindex" ];
			dindex = int . Parse ( ndx );

			BankGrid . SelectedIndex = bindex < 0 ? 0 : bindex;
			CustomerGrid . SelectedIndex = cindex < 0 ? 0 : bindex;
			DetailsGrid . SelectedIndex = dindex < 0 ? 0 : bindex;
			SubscribeToEvents();
			this. Show ( );
			WaitMessage . UpdateLayout ( );
			await LoadAllData ( );
			//Thread . Sleep ( 500 );


			Flags . SqlMultiViewer = this;
			Flags . SqlMultiViewer = this;

			// Setup global pointers to our data grids
			Flags . SqlBankGrid = this . BankGrid;
			Flags . SqlCustGrid = this . CustomerGrid;
			Flags . SqlDetGrid = this . DetailsGrid;

			LinkGrids . IsChecked = false;
			GridsLinked = false;
			if ( Flags . LinkviewerRecords )
			{
				LinkRecords . IsChecked = true;
				GridsLinked = true;
			}

			// Set window to TOPMOST
			OntopChkbox . IsChecked = true;
			this . Topmost = true;
			inprogress = false;
			Mouse . OverrideCursor = Cursors . Arrow;
			//			StartDataWatcher ( );
			isLoading = false;
		}

		#region WATCH FOR Db CLOSURE - not used right now, but works well' ish
		public async void StartDataWatcher ( )
		{
			Task t1 = Task . Run ( WatchForDbLoss );
		}
		public async void WatchForDbLoss ( )
		{
			while ( true )
			{
				Thread . Sleep ( 1500 );

				if ( isLoading )
					continue;
				if ( this . BankGrid . Items . Count == 0 )
				{
					Application . Current . Dispatcher . Invoke ( ( ) =>
					{
						Mouse . OverrideCursor = Cursors . Wait;
						ReloadBankDb ( );
					} );
					Thread . Sleep ( 5000 );
				}
				else if ( this . CustomerGrid . Items . Count == 0 )
				{
					Application . Current . Dispatcher . Invoke ( ( ) =>
					{
						Mouse . OverrideCursor = Cursors . Wait;
						ReloadCustDb ( );
					} );
					Thread . Sleep ( 5000 );
				}
				else if ( this . DetailsGrid . Items . Count == 0 )
				{
					Application . Current . Dispatcher . Invoke ( ( ) =>
					{
						Mouse . OverrideCursor = Cursors . Wait;
						ReloadDetDb ( );
					} );
					Thread . Sleep ( 5000 );
				}
			}
			return;
		}
		private Task ReloadBankDb ( )
		{
			Task t1 = null;
			isLoading = true;
			Mouse . OverrideCursor = Cursors . Wait;
			Application . Current . Dispatcher . Invoke ( ( ) =>
			{
				BankCollection . LoadBank ( MBankcollection, "MULTIVIEWER", 3, true );
			} );

			Thread . Sleep ( 5000 );
			return t1;
		}
		private Task ReloadCustDb ( )
		{
			Task t1 = null;
			Application . Current . Dispatcher . Invoke ( ( ) =>
			{
				Mouse . OverrideCursor = Cursors . Wait;
				CustCollection . LoadCust ( MCustcollection, "MULTIVIEWER", 3, true );
			} );
			Thread . Sleep ( 5000 );
			return t1;
		}
		private Task ReloadDetDb ( )
		{
			Task t1 = null;
			Application . Current . Dispatcher . Invoke ( ( ) =>
			{
				Mouse . OverrideCursor = Cursors . Wait;
				DetailCollection . LoadDet ( MDetcollection, "MULTIVIEWER", 3, true );
			} );
			Thread . Sleep ( 5000 );
			return t1;
		}

		#endregion WATCH FOR Db CLOSURE
		#region Post Data Reloaded event handlers - ALL WORKING WELL 26/5/21

		/// <summary>
		/// Handles rsetting the index after Bank data has been reoloaded
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void EventControl_BankDataLoaded ( object sender, LoadedEventArgs e )
		{
			if ( e . DataSource == null ) return; //|| this . BankGrid . Items . Count > 0 ) return;
//			if ( e . CallerDb != "MULTIVIEWER" )
//				return;
			Debug . WriteLine ($"\n*** Loading Bank data in BankDbView after BankDataLoaded trigger\n" );
							      // ONLY proceed if we triggered the new data request
							      //			if ( e . CallerDb != "MULTIVIEWER" ) return;
			Debug . WriteLine ( $"\n*** Loading Bank data in MultiViewer after BankDataLoaded trigger\n" );

			this . BankGrid . ItemsSource = null;

			stopwatch1 . Stop ( );
			Debug . WriteLine ( $"MULTIVIEWER : Bank Data fully loaded : {stopwatch1 . ElapsedMilliseconds} ms" );
			LoadingDbData = true;
			MBankcollection?.Clear();
			// This is how to convert to  CollectionView
			MBankcollection = e . DataSource as BankCollection;
			this . BankGrid . ItemsSource = CollectionViewSource . GetDefaultView ( MBankcollection );

			this . BankGrid . Refresh ( );
			BankGrid . SelectedIndex = bindex;
			BankGrid . SelectedItem = bindex;
			BankGrid . Refresh ( );
			BankGrid . UpdateLayout ( );
			Utils . SetUpGridSelection ( this . BankGrid, bindex );
			BankCount . Text = $"{this . BankGrid . SelectedIndex} / {this . BankGrid . Items . Count}";
			Mouse . OverrideCursor = Cursors . Arrow;
			isLoading = false;
			IsDirty = false;
			// Let em see our message
			ClearWaitMessage ( );

		}

		private void ClearWaitMessage ( )
		{
			WaitMessage . Refresh ( );
			//Thread . Sleep ( 500 );
			this . BankGrid . Visibility = Visibility . Visible;
			this . CustomerGrid . Visibility = Visibility . Visible;
			this . DetailsGrid . Visibility = Visibility . Visible;
			WaitMessage . Visibility = Visibility . Collapsed;
		}

		private async void EventControl_CustDataLoaded ( object sender, LoadedEventArgs e )
		/// <summary>
		/// Handles rsetting the index after Customer data has been reoloaded
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		{
			if ( e . DataSource == null ) return; //|| this . CustomerGrid . Items . Count > 0 ) return;
							      // ONLY proceeed if we triggered the new data request
			if ( e . CallerDb != "MULTIVIEWER" ) return;
			this . CustomerGrid . ItemsSource = null;

			stopwatch2 . Stop ( );
			Debug . WriteLine ( $"MULTIVIEWER : Customer Data fully loaded : {stopwatch2 . ElapsedMilliseconds} ms" );
			LoadingDbData = true;
			MCustcollection?.Clear();
			// This is how to convert to  CollectionView
			MCustcollection = e . DataSource as CustCollection;
			this . CustomerGrid . ItemsSource = CollectionViewSource . GetDefaultView ( MCustcollection );

			this . CustomerGrid . Refresh ( );
			CustomerGrid . SelectedIndex = cindex;
			CustomerGrid . SelectedItem = cindex;
			CustomerGrid . Refresh ( );
			CustomerGrid . UpdateLayout ( );
			Utils . SetUpGridSelection ( this . CustomerGrid, cindex );
			CustCount . Text = $"{this . CustomerGrid . SelectedIndex} / {this . CustomerGrid . Items . Count}";
			Mouse . OverrideCursor = Cursors . Arrow;
			LoadingDbData = false;
			IsDirty = false;
			// Let em see our message
			ClearWaitMessage ( );
		}
		/// <summary>
		/// Handles resetting the index after Details data has been reloaded
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void EventControl_DetDataLoaded ( object sender, LoadedEventArgs e )
		{
			if ( e . DataSource == null ) return;//|| this.DetailsGrid.Items.Count > 0) return;
			if ( e . CallerDb != "MULTIVIEWER" && e . CallerType != "SQLSERVER" ) return;
			this . DetailsGrid . ItemsSource = null;

			stopwatch3 . Stop ( );
			Debug . WriteLine ( $"MULTIVIEWER : Details Data fully loaded : {stopwatch3 . ElapsedMilliseconds} ms" );
			LoadingDbData = true;
			MDetcollection?.Clear();
			MDetcollection = e . DataSource as DetCollection;
			this . DetailsGrid . ItemsSource = CollectionViewSource . GetDefaultView ( MDetcollection );

			this . DetailsGrid . Refresh ( );
			DetailsGrid . SelectedIndex = dindex;
			DetailsGrid . SelectedItem = dindex;
			DetailsGrid . Refresh ( );
			DetailsGrid . UpdateLayout ( );
			Utils . SetUpGridSelection ( this . DetailsGrid, dindex );
			DetCount . Text = $"{this . DetailsGrid . SelectedIndex} / {this . DetailsGrid . Items . Count}";
			Mouse . OverrideCursor = Cursors . Arrow;
			LoadingDbData = false;
			// Let em see our message
			ClearWaitMessage ( );
		}
		#endregion Post Data Reloaded event handlers

		private void SubscribeToEvents ( )
		{
			Utils.SetupWindowDrag(this);

			// An EditDb has changed the current index
			EventControl. EditIndexChanged += EventControl_ViewerIndexChanged;
			// Another SqlDbviewer has changed the current index
			EventControl . ViewerIndexChanged += EventControl_ViewerIndexChanged;      // Callback in THIS FILE

			// data updated by another grid 
			EventControl . ViewerDataUpdated += EventControl_SqlViewerDataUpdated;
			EventControl . MultiViewerDataUpdated += EventControl_ViewerDataUpdated;
			EventControl.GlobalDataChanged += EventControl_GlobalDataChanged;
			EventControl. EditDbDataUpdated += EventControl_DataUpdated;
			// Data loaded event handlers
			EventControl . BankDataLoaded += EventControl_BankDataLoaded;
			EventControl . CustDataLoaded += EventControl_CustDataLoaded;
			EventControl . DetDataLoaded += EventControl_DetDataLoaded;
		}
		private async void EventControl_GlobalDataChanged(object sender, GlobalEventArgs e)
		{
			if (e.CallerType == "MULTIVIEWER" && e.AccountType == CurrentDb)
				return;
			// update all grids EXCEPT the default in AccountType
			//Update our own data tyoe only
			if (CurrentDb == "BANKACCOUNT")
				await BankCollection.LoadBank(null, "BANKACCOUNT", 1, true);
			else if (CurrentDb == "CUSTOMER")
				await CustCollection . LoadCust(null, "CUSTOMER", 2, true);
			else if (CurrentDb == "DETAILS")
				await DetailCollection . LoadDet(null, "DETAILS", 1, true);

		}

		private async void EventControl_ViewerIndexChanged ( object sender, IndexChangedArgs e )
		{
			if ( IsEditing )
			{
				return;
			}

			if ( Flags . LinkviewerRecords && Triggered == false ) //|| Flags.IsFiltered|| Flags.IsMultiMode	)
			{
				object RowTofind = null;
				object gr = null;
				int rec = 0;
				//				if ( inprogress )
				//					return;
				inprogress = true;
				if ( GridsLinked == false )
				{
					if ( e . Sender == "BANKACCOUNT" )
					{
						// Only Update the specific grid
						rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . BankGrid, "BANKACCOUNT" );
						this . BankGrid . SelectedIndex = rec != -1 ? rec : 0;
						bindex = rec;
						Utils . ScrollRecordIntoView ( this . BankGrid, rec );
						inprogress = false;
						SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
						BankCount . Text = $"{this . BankGrid . SelectedIndex} / {this . BankGrid . Items . Count}";
						return;
					}
					else if ( e . Sender == "CUSTOMER" )
					{
						// Only Update the specific grid
						rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . CustomerGrid, "CUSTOMER" );
						this . CustomerGrid . SelectedIndex = rec != -1 ? rec : 0;
						cindex = rec;
						BankData . DataContext = this . CustomerGrid . SelectedItem;
						Utils . ScrollRecordIntoView ( this . CustomerGrid, rec );
						inprogress = false;
						SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
						CustCount . Text = $"{this . CustomerGrid . SelectedIndex} / {this . CustomerGrid . Items . Count}";
						return;
					}
					if ( e . Sender == "DETAILS" )
					{
						// Only Update the specific grid
						rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . DetailsGrid, "DETAILS" );
						this . DetailsGrid . SelectedIndex = rec != -1 ? rec : 0;
						dindex = rec;
						Utils . ScrollRecordIntoView ( this . DetailsGrid, rec );
						inprogress = false;
						SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
						DetCount . Text = $"{this . DetailsGrid . SelectedIndex} / {this . DetailsGrid . Items . Count}";
						return;
					}
				}
				else
				{
					// Update all three grids
					rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . BankGrid, "BANKACCOUNT" );
					this . BankGrid . SelectedIndex = rec != -1 ? rec : 0;
					bindex = rec;
					Utils . ScrollRecordIntoView ( this . BankGrid, rec );
					rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . CustomerGrid, "CUSTOMER" );
					this . CustomerGrid . SelectedIndex = rec != -1 ? rec : 0;
					cindex = rec;
					BankData . DataContext = this . CustomerGrid . SelectedItem;
					Utils . ScrollRecordIntoView ( this . CustomerGrid, rec );
					rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . DetailsGrid, "DETAILS" );
					this . DetailsGrid . SelectedIndex = rec != -1 ? rec : 0;
					dindex = rec;
					Utils . ScrollRecordIntoView ( this . DetailsGrid, rec );

					BankCount . Text = $"{this . BankGrid . SelectedIndex} / {this . BankGrid . Items . Count}";
					CustCount . Text = $"{this . CustomerGrid . SelectedIndex} / {this . CustomerGrid . Items . Count}";
					DetCount . Text = $"{this . DetailsGrid . SelectedIndex} / {this . DetailsGrid . Items . Count}";

					// Finally, tell other viewers about the index change
					if ( e . Sender == "BANKACCOUNT" )
					{
						SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
						BankAccountViewModel bvm = new BankAccountViewModel ( );
						bvm = this . BankGrid . CurrentItem as BankAccountViewModel;
						if ( bvm == null )
						{
							inprogress = false;
							return;
						}
						if ( e . Sender == "MULTIVIEWER" )
							TriggerMultiViewerIndexChanged ( this . BankGrid );
					}
					else if ( e . Sender == "CUSTOMER" )
					{
						SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
						CustomerViewModel bvm = new CustomerViewModel ( );
						bvm = this . CustomerGrid . CurrentItem as CustomerViewModel;
						if ( bvm == null )
						{
							inprogress = false;
							return;
						}
						if ( e . Sender == "MULTIVIEWER" )
							TriggerMultiViewerIndexChanged ( this . CustomerGrid );
					}
					else if ( e . Sender == "DETAILS" )
					{
						SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
						DetailsViewModel bvm = new DetailsViewModel ( );
						bvm = this . DetailsGrid . CurrentItem as DetailsViewModel;
						if ( bvm == null )
						{
							inprogress = false;
							return;
						}
						if ( e . Sender == "MULTIVIEWER" )
							TriggerMultiViewerIndexChanged ( this . DetailsGrid );
					}
				}
				BankCount . Text = $"{this . BankGrid . SelectedIndex} / {this . BankGrid . Items . Count}";
				CustCount . Text = $"{this . CustomerGrid . SelectedIndex} / {this . CustomerGrid . Items . Count}";
				DetCount . Text = $"{this . DetailsGrid . SelectedIndex} / {this . DetailsGrid . Items . Count}"; inprogress = false;
			}
		}

		private void Window_Closing ( object sender, System . ComponentModel . CancelEventArgs e )
		{
			// Unsubscribe from Bank data change event notificatoin
			// Main update notification handler
			//			EventControl . DataUpdated -= EventControl_DataUpdated;
			EventControl . ViewerDataUpdated -= EventControl_SqlViewerDataUpdated;
			EventControl . EditDbDataUpdated -= EventControl_DataUpdated;
			EventControl . MultiViewerDataUpdated -= EventControl_ViewerDataUpdated;

			// Event triggers when a Specific Db viewer (BankDbViewer etc) updates the data
			EventControl . BankDataLoaded -= EventControl_BankDataLoaded;
			EventControl . CustDataLoaded -= EventControl_CustDataLoaded;
			EventControl . DetDataLoaded -= EventControl_DetDataLoaded;
			EventControl.GlobalDataChanged -= EventControl_GlobalDataChanged;

			// Listen ofr index changes
			EventControl. ViewerIndexChanged -= EventControl_ViewerIndexChanged;
			EventControl . EditIndexChanged -= EventControl_ViewerIndexChanged;

			Utils . SaveProperty ( "Multi_bindex", bindex . ToString ( ) );
			Utils . SaveProperty ( "Multi_cindex", cindex . ToString ( ) );
			Utils . SaveProperty ( "Multi_dindex", dindex . ToString ( ) );
			// Clear databases
			MBankcollection?.Clear ( );
			MCustcollection?.Clear ( );
			MDetcollection?.Clear ( );
			Flags . SqlMultiViewer = null;
		}

		private async Task LoadAllData ( )
		{
			// load the data
			Mouse . OverrideCursor = Cursors . Wait;
			//			if ( MultiBankcollection == null || MultiBankcollection . Count == 0 )
			MBankcollection = null;
			stopwatch1 . Start ( );
			await BankCollection . LoadBank ( MBankcollection, "MULTIVIEWER", 3, true );
			//BankGrid . ItemsSource = MultiBankcollection;
			//			if ( MultiCustcollection == null || MultiCustcollection . Count == 0 )
			MCustcollection = null;
			stopwatch2 . Start ( );
			await CustCollection . LoadCust ( MCustcollection, "MULTIVIEWER", 3, true );
			//			if ( MultiDetcollection == null || MultiDetcollection . Count == 0 )
			MDetcollection = null;

			DetCollection det = new DetCollection ( );
			stopwatch3 . Start ( );
			await DetailCollection . LoadDet ( MDetcollection, "MULTIVIEWER", 2, true );

			Flags . SqlMultiViewer = this;
		}

		#endregion STARTUP/CLOSE

		#region EVENT HANDLERS

		#endregion EVENT HANDLERS

		#region DATA UPDATING

		/// <summary>
		/// Handles the SQL updateof any changes made and updates all grids
		/// </summary>
		/// <param name="CurrentDb"></param>
		/// <param name="e"></param>
		public void UpdateOnDataChange ( string CurrentDb, DataGridRowEditEndingEventArgs e )
		{
			// Call Handler to update ALL Db's via SQL
			Mouse . OverrideCursor = Cursors . Wait;
			SQLHandlers sqlh = new SQLHandlers ( );
			sqlh . UpdateAllDb ( CurrentDb, e );
			//bindex = this . BankGrid . SelectedIndex;
			//cindex = this . CustomerGrid . SelectedIndex;
			//dindex = this . DetailsGrid . SelectedIndex;
			////Gotta reload our data because the update clears it down totally to null
			//// Refresh our grids
			//RefreshAllGrids ( CurrentDb, e . Row . GetIndex ( ) );
			//inprogress = false;
			Mouse . OverrideCursor = Cursors . Arrow;
		}
		public async void RefreshAllGrids ( string CurrentDb, int row, string Custno = "", string Bankno = "" )
		{
			Mouse . OverrideCursor = Cursors . Wait;
			ReloadingData = true;
			await ReLoadAllDataBases ( CurrentDb, row, Custno, Bankno );
			ReloadingData = false; ;
			StatusBar . Text = "All available Records are shown above in all three grids";
			Mouse . OverrideCursor = Cursors . Arrow;
			ReloadingData = false;
		}

		private async Task ReLoadAllDataBases ( string CurrentD, int row, string Custno = "", string Bankno = "" )
		{
			int bbindex = 0;
			int ccindex = 0;
			int ddindex = 0;
			bool DataAvailable = false;
			BankAccountViewModel bvm = new BankAccountViewModel ( );
			CustomerViewModel cvm = new CustomerViewModel ( );
			DetailsViewModel dvm = new DetailsViewModel ( );
			int rec = 0;
			if ( row == -1 ) row = 0;

			// If we have received updated data, reset global indexes
			if ( Custno != "" && Bankno != "" )
			{
				bbindex = Utils . FindMatchingRecord ( Custno, Bankno, this . BankGrid, "BANKACCOUNT" );
				ccindex = Utils . FindMatchingRecord ( Custno, Bankno, this . CustomerGrid, "CUSTOMER" );
				ddindex = Utils . FindMatchingRecord ( Custno, Bankno, this . DetailsGrid, "DETAILS" );
				DataAvailable = true;
			}
			else
			{
				// Get the current records data from each datagrid
				bbindex = this . BankGrid . SelectedIndex < 0 ? 0 : this . BankGrid . SelectedIndex;
				ccindex = this . CustomerGrid . SelectedIndex < 0 ? 0 : this . CustomerGrid . SelectedIndex;
				ddindex = this . DetailsGrid . SelectedIndex < 0 ? 0 : this . DetailsGrid . SelectedIndex;
			}
			//Utils . PlayMary ( );

			// Assign correct index to item source
			// These SelectedIndex changes ALL trigger the SelectionChanged Method() !!!!!
			this . BankGrid . SelectedIndex = bbindex;
			this . CustomerGrid . SelectedIndex = ccindex;
			this . DetailsGrid . SelectedIndex = ddindex;


			this . BankGrid . SelectedItem = bbindex;
			this . CustomerGrid . SelectedItem = ccindex;
			this . DetailsGrid . SelectedItem = ddindex;

			bvm = this . BankGrid . SelectedItem as BankAccountViewModel;
			cvm = this . CustomerGrid . SelectedItem as CustomerViewModel;
			dvm = this . DetailsGrid . SelectedItem as DetailsViewModel;
			Custno = bvm?.CustNo;
			Bankno = bvm?.BankNo;

			// Sanity check
			//if ( bvm == null || cvm == null || dvm == null )
			//{
			//	//				await Utils . DoBeep ( 175, 300 ) . ConfigureAwait ( false );
			//	return;
			//}

			// Now go ahead and clear the data and then reload it
			// These SelectedIndex changes ALL trigger the SelectionChanged Method() !!!!!
			this . BankGrid . ItemsSource = null;
			this . CustomerGrid . ItemsSource = null;
			this . DetailsGrid . ItemsSource = null;
			this . BankGrid . Items . Clear ( );
			this . CustomerGrid . Items . Clear ( );
			this . DetailsGrid . Items . Clear ( );

			//MultiBankcollection = null;
			//MultiCustcollection = null;
			//MultiDetcollection = null;

			/// Reoad the data into our Items Source collections
			await BankCollection . LoadBank ( MBankcollection, "MULTIVIEWER", 3, true );
			//			MultiBankcollection = BankCollection . MultiBankcollection;

			await CustCollection . LoadCust ( MCustcollection, "MULTIVIEWER", 3, true );
			//			MultiCustcollection = CustCollection . MultiCustcollection;

			await DetailCollection . LoadDet ( MDetcollection, "MULTIVIEWER", 3, true );
			//			MultiDetcollection = DetCollection . MultiDetcollection;

			//this . BankGrid . ItemsSource = MBankcollection;

			//// This causes a data load of bank data !!!!
			//this . BankGrid . Refresh ( );
			////bbindex = Utils . FindMatchingRecord ( Custno, Bankno, this . BankGrid, "BANKACCOUNT" );
			////inprogress = true;
			//if ( this . BankGrid . Items . Count > 0 )
			//{
			//	this . BankGrid . SelectedIndex = bbindex;
			//	this . BankGrid . SelectedItem = bbindex;
			//	this . BankGrid . Refresh ( );
			//	Utils . SetUpGridSelection ( this . BankGrid, bbindex );
			//	//				this . BankGrid . ScrollIntoView ( bbindex );
			//}


			//this . CustomerGrid . ItemsSource = MCustcollection;
			////ccindex = Utils . FindMatchingRecord ( Custno, Bankno, this . CustomerGrid, "CUSTOMER" );
			////inprogress = true;
			//if ( this . CustomerGrid . Items . Count > 0 )
			//{
			//	this . CustomerGrid . SelectedIndex = ccindex;
			//	this . CustomerGrid . SelectedItem = ccindex;
			//	this . CustomerGrid . Refresh ( );
			//	Utils . SetUpGridSelection ( this . CustomerGrid, ccindex );
			//	//				this . CustomerGrid . ScrollIntoView ( ccindex );
			//}

			//this . DetailsGrid . ItemsSource = MDetcollection;
			////ddindex = Utils . FindMatchingRecord ( Custno, Bankno, this . DetailsGrid, "DETAILS" );
			////inprogress = true;
			//if ( this . DetailsGrid . Items . Count > 0 )
			//{
			//	this . DetailsGrid . SelectedIndex = ddindex;
			//	this . DetailsGrid . SelectedItem = ddindex;
			//	this . DetailsGrid . Refresh ( );
			//	Utils . SetUpGridSelection ( this . DetailsGrid, ddindex );
			//	//				this . DetailsGrid . ScrollIntoView ( ddindex );
			//}

			////inprogress = false;;

			//Console . WriteLine ( $"bbindex={bbindex}, ccindex={ccindex}, ddindex={ddindex}" );
			//Console . WriteLine ( $"Bank={Bankno}, Cust={Custno}" );
			//Console . WriteLine ( $"Bank={this . BankGrid . SelectedIndex}, Cust={this . CustomerGrid . SelectedIndex}, Det={this . DetailsGrid . SelectedIndex}" );
			//if ( Flags . FilterCommand != "" )
			//{
			//	string tmp = Flags . FilterCommand;
			//	string shortstring = tmp . Substring ( 25 );
			//	tmp = "Select * from Customer " + shortstring;
			//	Flags . FilterCommand = tmp;
			//}
			////			Utils . SetUpGridSelection ( this . BankGrid, bbindex );
			////			Utils . SetUpGridSelection ( this . CustomerGrid, ccindex );
			////			Utils . SetUpGridSelection ( this . DetailsGrid, ddindex );

			//if ( CurrentDb == "BANKACCOUNT" )
			//	this . BankGrid . Focus ( );
			//else if ( CurrentDb == "CUSTOMER" )
			//	this . CustomerGrid . Focus ( );
			//else if ( CurrentDb == "DETAILS" )
			//	this . DetailsGrid . Focus ( );
			Mouse . OverrideCursor = System . Windows . Input . Cursors . Arrow;

		}
		#endregion EVENT DATA UPDATING

		#region User Defined Commands
		//
		CustomCommands _InfoCommand = new CustomCommands ( );
		public CustomCommands InformationCommand
		{
			get { return _InfoCommand; }
		}

		private void ExecutedCloseApp ( object sender, ExecutedRoutedEventArgs e )
		{
			App . Current . Shutdown ( );
		}
		#endregion User Defined Commands


		public void RefreshData ( int row, string Custno = "", string Bankno = "" )
		{
			//bindex = this . BankGrid . SelectedIndex;
			//cindex = this . CustomerGrid . SelectedIndex;
			//dindex = this . DetailsGrid . SelectedIndex;
			//this . BankGrid . ItemsSource = null;
			//this . BankGrid . ItemsSource = MultiBankcollection;
			//this . CustomerGrid . ItemsSource = null;
			//this . CustomerGrid . ItemsSource = MultiCustcollection;
			//this . DetailsGrid . ItemsSource = null;
			//this . DetailsGrid . ItemsSource = MultiDetcollection;

			// This handles row selection AND refocus
			RefreshAllGrids ( CurrentDb, row );
			Mouse . OverrideCursor = Cursors . Wait;
			inprogress = false;
			Mouse . OverrideCursor = System . Windows . Input . Cursors . Arrow;
		}

		private void Close_Click ( object sender, RoutedEventArgs e )
		{
			Close ( );
		}
		#region DATAGRID  SELECTION CHANGE  HANDLING  (SelectedIndex matching across all 3 grids)
		/// <summary>
		/// /// *************************************************************************
		/// THESE ALL WORK CORRECTLY, AND THE SELECTED ROWS ALL MATCH PERFECTLY - 15/5/2021
		/// /// *************************************************************************
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//****************************************************************************************************//
		private async void BankGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			int rec = 0;
			string SearchCustNo = "";
			string SearchBankNo = "";

			if ( LoadingDbData || ReloadingData )
			{
				LoadingDbData = false;
				return;
			}
			if ( inprogress )
				return;

			if ( IsEditing )
			{
				e . Handled = true;
				return;
			}

			BankAccountViewModel CurrentSelectedRecord = this . BankGrid . SelectedItem as BankAccountViewModel;
			if ( CurrentSelectedRecord == null ) return;
			bindex = this . BankGrid . SelectedIndex;

			inprogress = true;

			// See if we need to update th eother grids on this multi viewer
			if ( LinkGrids . IsChecked == true )
			{
				int currsel = this . BankGrid . SelectedIndex;
				// We have the link our own grids option checked
				// so update our other 2 grids positions
				SearchCustNo = CurrentSelectedRecord?.CustNo;
				SearchBankNo = CurrentSelectedRecord?.BankNo;
				if ( SearchCustNo == null && SearchBankNo == null )
				{ inprogress = false; return; }

				Triggered = true;

				rec = Utils . FindMatchingRecord ( SearchCustNo, SearchBankNo, this . CustomerGrid, "CUSTOMER" );
				// Store current index to global
				cindex = rec;
				this . CustomerGrid . UnselectAll ( );
				this . CustomerGrid . SelectedIndex = rec;
				this . CustomerGrid . SelectedItem = rec;
				Utils . SetUpGridSelection ( this . CustomerGrid, rec );
				rec = Utils . FindMatchingRecord ( SearchCustNo, SearchBankNo, this . DetailsGrid, "DETAILS" );
				// Store current index to global
				dindex = rec;
				this . DetailsGrid . UnselectAll ( );
				this . DetailsGrid . SelectedIndex = rec;
				this . DetailsGrid . SelectedItem = rec;
				Utils . SetUpGridSelection ( this . DetailsGrid, rec );
				// The global linkage is ALSO set, so
				// we must notify any other windows that may need to update themselves
				if ( Flags . LinkviewerRecords )
					TriggerMultiViewerIndexChanged ( this . BankGrid );
				BankData . DataContext = this . BankGrid . SelectedItem;
				Triggered = false;
			}
			else if ( Flags . LinkviewerRecords )
				TriggerMultiViewerIndexChanged ( this . BankGrid );

			BankCount . Text = $"{this . BankGrid . SelectedIndex} / {this . BankGrid . Items . Count}";
			CustCount . Text = $"{this . CustomerGrid . SelectedIndex} / {this . CustomerGrid . Items . Count}";
			DetCount . Text = $"{this . DetailsGrid . SelectedIndex} / {this . DetailsGrid . Items . Count}";
			bindex = this . BankGrid . SelectedIndex;
			BankCount . Text = $"{bindex} / {this . BankGrid . Items . Count}";
			//			Debug . WriteLine ( $"BankGrid Index = {bindex}" );
			SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
			inprogress = false;
			try
			{ e . Handled = true; }
			catch ( Exception ex ) { }
			return;
		}
		//****************************************************************************************************//
		private async void CustGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			int rec = 0;
			string SearchCustNo = "";
			string SearchBankNo = "";

			if ( LoadingDbData || ReloadingData )
			{
				LoadingDbData = false;
				return;
			}
			if ( inprogress )
				return;

			if ( IsEditing )
			{
				e . Handled = true;
				return;
			}

			CustomerViewModel CurrentSelectedRecord = this . CustomerGrid . SelectedItem as CustomerViewModel;
			if ( CurrentSelectedRecord == null ) return;
			cindex = this . CustomerGrid . SelectedIndex;

			inprogress = true;

			if ( LinkGrids . IsChecked == true )
			{
				int currsel = this . CustomerGrid . SelectedIndex;
				// We triggered this change
				SearchCustNo = CurrentSelectedRecord?.CustNo;
				SearchBankNo = CurrentSelectedRecord?.BankNo;
				if ( SearchCustNo == null && SearchBankNo == null )
				{ inprogress = false; return; }

				// We have the link our own grids option checked
				// so update all our grids position
				Triggered = true;

				rec = Utils . FindMatchingRecord ( SearchCustNo, SearchBankNo, this . BankGrid, "BANKACCOUNT" );
				// Store current index to global
				bindex = rec;
				this . BankGrid . UnselectAll ( );
				this . BankGrid . SelectedIndex = rec;
				this . BankGrid . SelectedItem = rec;
				Utils . SetUpGridSelection ( this . BankGrid, rec );
				//Utils . ScrollRecordIntoView ( this . BankGrid, rec );

				rec = Utils . FindMatchingRecord ( SearchCustNo, SearchBankNo, this . DetailsGrid, "DETAILS" );
				// Store current index to global
				dindex = rec;
				this . DetailsGrid . UnselectAll ( );
				this . DetailsGrid . SelectedIndex = rec;
				this . DetailsGrid . SelectedItem = rec;
				Utils . SetUpGridSelection ( this . DetailsGrid, rec );

				BankData . DataContext = this . DetailsGrid . SelectedItem;
				// The global linkage is set, so
				// we must notify any other windows that may need to update themselves
				if ( Flags . LinkviewerRecords )
					TriggerMultiViewerIndexChanged ( this . CustomerGrid );

				Triggered = false;
			}
			else if ( Flags . LinkviewerRecords )
				TriggerMultiViewerIndexChanged ( this . CustomerGrid );

			BankCount . Text = $"{this . BankGrid . SelectedIndex} / {this . BankGrid . Items . Count}";
			CustCount . Text = $"{this . CustomerGrid . SelectedIndex} / {this . CustomerGrid . Items . Count}";
			DetCount . Text = $"{this . DetailsGrid . SelectedIndex} / {this . DetailsGrid . Items . Count}";
			inprogress = false;
			cindex = this . CustomerGrid . SelectedIndex;
			CustCount . Text = $"{cindex} / {this . CustomerGrid . Items . Count}";
			SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );

			try
			{ e . Handled = true; }
			catch ( Exception ex ) { }
			return;

		}

		private async void DetGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			int rec = 0;
			string SearchCustNo = "";
			string SearchBankNo = "";

			if ( LoadingDbData || ReloadingData )
			{
				LoadingDbData = false;
				return;
			}
			if ( inprogress )
				return;

			if ( IsEditing )
			{
				e . Handled = true;
				return;
			}

			dindex = this . DetailsGrid . SelectedIndex;
			DetailsViewModel CurrentSelectedRecord = this . DetailsGrid . SelectedItem as DetailsViewModel;
			if ( CurrentSelectedRecord == null ) return;

			inprogress = true;

			if ( LinkGrids . IsChecked == true )
			{
				int currsel = this . DetailsGrid . SelectedIndex;
				SearchCustNo = CurrentSelectedRecord?.CustNo;
				SearchBankNo = CurrentSelectedRecord?.BankNo;
				if ( SearchCustNo == null && SearchBankNo == null )
				{ inprogress = false; return; }

				// update all grids position
				Triggered = true;

				rec = Utils . FindMatchingRecord ( SearchCustNo, SearchBankNo, this . BankGrid, "BANKACCOUNT" );
				// Store current index to global
				bindex = rec;
				//				this . BankGrid . UnselectAll ( );
				this . BankGrid . SelectedIndex = rec;
				this . BankGrid . SelectedItem = rec;
				Utils . SetUpGridSelection ( this . BankGrid, rec );

				rec = Utils . FindMatchingRecord ( SearchCustNo, SearchBankNo, this . CustomerGrid, "CUSTOMER" );
				// Store current index to global
				cindex = rec;
				this . CustomerGrid . UnselectAll ( );
				this . CustomerGrid . SelectedIndex = rec;
				this . CustomerGrid . SelectedItem = rec;
				Utils . SetUpGridSelection ( this . CustomerGrid, rec );
				// The global linkage is set, so
				BankData . DataContext = this . DetailsGrid . SelectedItem;
				// we must notify any other windows that may need to update themselves
				if ( Flags . LinkviewerRecords )
					TriggerMultiViewerIndexChanged ( this . DetailsGrid );
				Triggered = false;
			}
			else if ( Flags . LinkviewerRecords )
				TriggerMultiViewerIndexChanged ( this . DetailsGrid );

			BankCount . Text = $"{this . BankGrid . SelectedIndex} / {this . BankGrid . Items . Count}";
			CustCount . Text = $"{this . CustomerGrid . SelectedIndex} / {this . CustomerGrid . Items . Count}";
			DetCount . Text = $"{this . DetailsGrid . SelectedIndex} / {this . DetailsGrid . Items . Count}";

			inprogress = false;
			dindex = this . DetailsGrid . SelectedIndex;
			DetCount . Text = $"{dindex} / {this . DetailsGrid . Items . Count}";
			SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
			try
			{ e . Handled = true; }
			catch ( Exception ex ) { }
			return;
		}

		#endregion DATAGRID  SELECTION CHANGE  HANDLING

		#region focus events

		private void CustGrid_GotFocus ( object sender, RoutedEventArgs e )
		{ CurrentDb = "CUSTOMER"; }
		private void BankGrid_GotFocus ( object sender, RoutedEventArgs e )
		{ CurrentDb = "BANKACCOUNT"; }
		private void DetGrid_GotFocus ( object sender, RoutedEventArgs e )
		{ CurrentDb = "DETAILS"; }

		#endregion focus events

		#region SCROLLBARS

		// scroll bar movement is automatically   stored by these three methods
		// So we can use them to reset position CORRECTLY after refreshes
		private void BankGrid_ScrollChanged ( object sender, ScrollChangedEventArgs e )
		{
			//int rec = 0;
			//DataGrid dg = null;
			//dg = sender as DataGrid;
			//var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			//scroll . CanContentScroll = true;
			//SetScrollVariables ( sender );
			//Utils . SetUpGridSelection ( this . BankGrid, this . BankGrid . SelectedIndex );
			//Utils . SetUpGridSelection ( this . CustomerGrid, this . BankGrid . SelectedIndex );
			//Utils . SetUpGridSelection ( this . DetailsGrid, this . BankGrid . SelectedIndex );
			////			this . BankGrid . ScrollIntoView ( this . BankGrid . SelectedIndex );
			////			this . CustomerGrid . ScrollIntoView ( this . BankGrid . SelectedIndex );
			////			this . DetailsGrid . ScrollIntoView ( this . BankGrid . SelectedIndex );

		}
		private void CustGrid_ScrollChanged ( object sender, ScrollChangedEventArgs e )
		{
			//DataGrid dg = null;
			//dg = sender as DataGrid;
			//var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			//scroll . CanContentScroll = true;
			//SetScrollVariables ( sender );
			//Utils . SetUpGridSelection ( this . BankGrid, this . BankGrid . SelectedIndex );
			//Utils . SetUpGridSelection ( this . CustomerGrid, this . BankGrid . SelectedIndex );
			//Utils . SetUpGridSelection ( this . DetailsGrid, this . BankGrid . SelectedIndex );
			////			this . BankGrid . ScrollIntoView ( this . CustomerGrid . SelectedIndex );
			////			this . CustomerGrid . ScrollIntoView ( this . CustomerGrid . SelectedIndex );
			////			this . DetailsGrid . ScrollIntoView ( this . CustomerGrid . SelectedIndex );
		}

		private void DetGrid_ScrollChanged ( object sender, ScrollChangedEventArgs e )
		{
			//DataGrid dg = null;
			//dg = sender as DataGrid;
			//var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			//scroll . CanContentScroll = true;
			//SetScrollVariables ( sender );
			//Utils . SetUpGridSelection ( this . BankGrid, this . DetailsGrid . SelectedIndex );
			//Utils . SetUpGridSelection ( this . CustomerGrid, this . DetailsGrid . SelectedIndex );
			//Utils . SetUpGridSelection ( this . DetailsGrid, this . DetailsGrid . SelectedIndex );
			////			this . CustomerGrid . ScrollIntoView ( this . DetailsGrid . SelectedIndex );
			////			this . BankGrid . ScrollIntoView ( this . DetailsGrid . SelectedIndex );
			////			this . DetailsGrid . ScrollIntoView ( this . DetailsGrid . SelectedIndex );
		}
		#endregion SCROLLBARS

		#region Scroll bar utilities
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
			if ( dg . SelectedItem == null ) return;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) sender );
			if ( scroll == null ) return;
			scroll . CanContentScroll = true;
			double d = scroll . VerticalOffset;
			int rounded = Convert . ToInt32 ( d );
			if ( dg == this . BankGrid )
			{
				//				Debug . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . TopVisibleBankGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . TopVisibleBankGridRow = ( double ) rounded;
				ScrollData . Banktop = ( double ) rounded;
			}
			else if ( dg == this . CustomerGrid )
			{
				//				Debug . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . TopVisibleCustGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . TopVisibleCustGridRow = ( double ) rounded;
				ScrollData . Custtop = ( double ) rounded;
			}
			else if ( dg == this . DetailsGrid )
			{
				//				Debug . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . TopVisibleDetGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . TopVisibleDetGridRow = ( double ) rounded;
				ScrollData . Dettop = ( double ) rounded;
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
			if ( dg == this . BankGrid )
			{
				//				Debug . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . BottomVisibleBankGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . BottomVisibleBankGridRow = ( double ) rounded;
				ScrollData . Bankbottom = ( double ) rounded;
			}
			else if ( dg == this . CustomerGrid )
			{
				//				Debug . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . BottomVisibleCustGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . BottomVisibleCustGridRow = ( double ) rounded;
				ScrollData . Custbottom = ( double ) rounded;
			}
			else if ( dg == this . DetailsGrid )
			{
				//				Debug . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . BottomVisibleDetGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . BottomVisibleDetGridRow = ( double ) rounded;
				ScrollData . Detbottom = ( double ) rounded;
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
			if ( dg == this . BankGrid )
				ScrollData . BankVisible = ( double ) scroll . ViewportHeight;
			else if ( dg == this . CustomerGrid )
				ScrollData . CustVisible = ( double ) scroll . ViewportHeight;
			else if ( dg == this . DetailsGrid )
				ScrollData . DetVisible = ( double ) scroll . ViewportHeight;
		}


		#endregion Scroll bar utilities

		private void DoDragMove ( )
		{
			//Handle the button NOT being the left mouse button
			// which will crash the DragMove Fn.....     cos it has to be the primary button !!!
			try
			{ DragMove ( ); }
			catch ( Exception ex )
			{ Debug . WriteLine ( $"General Exception : {ex . Message}, {ex . Data}" ); return; }
		}



		/// <summary>
		/// Limit datagrid content to multiple accounts data only
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Details_Click ( object sender, RoutedEventArgs e )
		{
			// display multi data only
			ReloadDataAllDataAsMulti ( );
		}

		private async Task ReloadDataAllDataAsMulti ( )
		{
			// Make sure this window has it's pointer "Registered" cos we can
			//Show only Customers with multiple Bank Accounts
			string s = MultiAccountText . Text;
			if ( s . Contains ( "<<-" ) || s . Contains ( "Show All" ) )
			{
				Flags . IsMultiMode = false;
				MultiAccountText . Text = "Multi Accounts";
				//// Set the gradient background
				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				Multiaccounts . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
				Multiaccounts . Background = br;
				Multiaccounts . Content = "Multi A/c Only";
				MultiAccountText . Text = "Multi A/c Only";
			}
			else
			{
				Flags . IsMultiMode = true;
				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGreen" );
				Multiaccounts . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGreen" );
				Multiaccounts . Background = br;
				Multiaccounts . Content = "Show All A/c's";
				MultiAccountText . Text = "Show All A/c's";
			}
			this . BankGrid . ItemsSource = null;
			this . BankGrid . Items . Clear ( );
			await BankCollection . LoadBank ( MBankcollection, "MULTIVIEWER", 1, true );
			this . BankGrid . ItemsSource = CollectionViewSource . GetDefaultView ( MBankcollection );
			this . BankGrid . Refresh ( );
			this . CustomerGrid . ItemsSource = null;
			this . CustomerGrid . Items . Clear ( );
			await CustCollection . LoadCust ( MCustcollection, "MULTIVIEWER", 3, true );
			this . CustomerGrid . ItemsSource = CollectionViewSource . GetDefaultView ( MCustcollection );
			this . CustomerGrid . Refresh ( );
			//			ExtensionMethods . Refresh ( this . CustomerGrid );
			this . DetailsGrid . ItemsSource = null;
			this . DetailsGrid . Items . Clear ( );
			await DetailCollection . LoadDet ( MDetcollection, "MULTIVIEWER", 3, true );
			this . DetailsGrid . ItemsSource = CollectionViewSource . GetDefaultView ( MDetcollection );
			this . DetailsGrid . Refresh ( );
			//			ExtensionMethods . Refresh ( this . DetailsGrid );
		}

		private void Filter_Click ( object sender, RoutedEventArgs e )
		{
			if ( CurrentDb == "" )
			{
				MessageBox . Show ( "Please select an entry in one of the data grids before trying to filter the data listed." );
				return;
			}
			if ( FilterBtn . Content == "Clear Filter" )
			{
				Flags . FilterCommand = "";
				ReLoadAllDataBases ( CurrentDb, -1 );
				FilterBtn . Content = "Filter";
				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				FilterBtn . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
				FilterBtn . Background = br;
			}
			else
			{
				Filtering f = new Filtering ( );
				Flags . FilterCommand = f . DoFilters ( this, "BANKACCOUNT", 1 );
				if ( Flags . FilterCommand == "" ) return;
				ReLoadAllDataBases ( CurrentDb, -1 );
				// Clear our filter string
				Flags . FilterCommand = "";
				FilterBtn . Content = "Clear Filter";
				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGreen" );
				FilterBtn . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGreen" );
				FilterBtn . Background = br;
			}
		}

		private void BankGrid_Selected ( object sender, RoutedEventArgs e )
		{
			// hit when grid selection is changed by anything
			//int x = 0;
			//Console . WriteLine("...");
		}


		private void Refresh_Click ( object sender, RoutedEventArgs e )
		{
			RefreshData ( -1 );
		}

		private void BankDb_Click ( object sender, RoutedEventArgs e )
		{
			Window handle = null;
			if ( Utils . FindWindowFromTitle ( "Bank a/c editor", ref handle ) )
			{
				handle . Focus ( );
				handle . BringIntoView ( );
				return;
			}
			else
			{
				BankDbView cdbv = new BankDbView ( );
				cdbv . Show ( );
			}
		}
		private void CustDb_Click ( object sender, RoutedEventArgs e )
		{
			Window handle = null;
			if ( Utils . FindWindowFromTitle ( "customer account editor", ref handle ) )
			{
				handle . Focus ( );
				handle . BringIntoView ( );
				return;
			}
			else
			{
				CustDbView cdbv = new CustDbView ( );
				cdbv . Show ( );
			}
		}
		private void DetDb_Click ( object sender, RoutedEventArgs e )
		{
			Window handle = null;
			if ( Utils . FindWindowFromTitle ( "details a/c editor", ref handle ) )
			{
				handle . Focus ( );
				handle . BringIntoView ( );
				return;
			}
			else
			{
				DetailsDbView cdbv = new DetailsDbView ( null, this, null );
				cdbv . Show ( );
			}
		}

		private void LinkRecords_Click ( object sender, RoutedEventArgs e )
		{
			Flags . LinkviewerRecords = !Flags . LinkviewerRecords;
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

		private void OntopChkbox_Click ( object sender, RoutedEventArgs e )
		{
			if ( this . Topmost )
			{
				OntopChkbox . IsChecked = false;
				this . Topmost = false;
			}
			else
			{
				OntopChkbox . IsChecked = true;
				this . Topmost = true;
			}
		}

		#region LINQ queries
		private void Linq1_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			Mouse . OverrideCursor = Cursors . Wait;
			var accounts = from items in MBankcollection
				       where ( items . AcType == 1 )
				       orderby items . CustNo
				       select items;
			this . BankGrid . ItemsSource = accounts;
			var accounts1 = from items in MCustcollection
					where ( items . AcType == 1 )
					orderby items . CustNo
					select items;
			this . CustomerGrid . ItemsSource = accounts1;
			var accounts2 = from items in MDetcollection
					where ( items . AcType == 1 )
					orderby items . CustNo
					select items;
			this . DetailsGrid . ItemsSource = accounts2;
			StatusBar . Text = "Only Records matching Account Type = 1 are shown above";
			BankCount . Text = $"{Utils . GetPrettyGridStatistics ( this . BankGrid, this . BankGrid . SelectedIndex )}";
			CustCount . Text = $"{Utils . GetPrettyGridStatistics ( this . CustomerGrid, this . CustomerGrid . SelectedIndex )}";
			DetCount . Text = $"{Utils . GetPrettyGridStatistics ( this . DetailsGrid, this . DetailsGrid . SelectedIndex )}";

			Mouse . OverrideCursor = Cursors . Arrow;
		}
		private void Linq2_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			Mouse . OverrideCursor = Cursors . Wait;
			var accounts = from items in MBankcollection
				       where ( items . AcType == 2 )
				       orderby items . CustNo
				       select items;
			this . BankGrid . ItemsSource = accounts;
			var accounts1 = from items in MCustcollection
					where ( items . AcType == 2 )
					orderby items . CustNo
					select items;
			this . CustomerGrid . ItemsSource = accounts1;
			var accounts2 = from items in MDetcollection
					where ( items . AcType == 2 )
					orderby items . CustNo
					select items;
			this . DetailsGrid . ItemsSource = accounts2;
			StatusBar . Text = "Only Records matching Account Type = 2 are shown above";
			BankCount . Text = $"{Utils . GetPrettyGridStatistics ( this . BankGrid, this . BankGrid . SelectedIndex )}";
			CustCount . Text = $"{Utils . GetPrettyGridStatistics ( this . CustomerGrid, this . CustomerGrid . SelectedIndex )}";
			DetCount . Text = $"{Utils . GetPrettyGridStatistics ( this . DetailsGrid, this . DetailsGrid . SelectedIndex )}";
			Mouse . OverrideCursor = Cursors . Arrow;
		}
		private void Linq3_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			Mouse . OverrideCursor = Cursors . Wait;
			var accounts = from items in MBankcollection
				       where ( items . AcType == 3 )
				       orderby items . CustNo
				       select items;
			this . BankGrid . ItemsSource = accounts;
			var accounts1 = from items in MCustcollection
					where ( items . AcType == 3 )
					orderby items . CustNo
					select items;
			this . CustomerGrid . ItemsSource = accounts1;
			var accounts2 = from items in MDetcollection
					where ( items . AcType == 3 )
					orderby items . CustNo
					select items;
			this . DetailsGrid . ItemsSource = accounts2;
			StatusBar . Text = "Only Records matching Account Type = 3 are shown above";
			BankCount . Text = $"{Utils . GetPrettyGridStatistics ( this . BankGrid, this . BankGrid . SelectedIndex )}";
			CustCount . Text = $"{Utils . GetPrettyGridStatistics ( this . CustomerGrid, this . CustomerGrid . SelectedIndex )}";
			DetCount . Text = $"{Utils . GetPrettyGridStatistics ( this . DetailsGrid, this . DetailsGrid . SelectedIndex )}";
			Mouse . OverrideCursor = Cursors . Arrow;
		}
		private void Linq4_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			Mouse . OverrideCursor = Cursors . Wait;
			var accounts = from items in MBankcollection
				       where ( items . AcType == 4 )
				       orderby items . CustNo
				       select items;
			this . BankGrid . ItemsSource = accounts;
			var accounts1 = from items in MCustcollection
					where ( items . AcType == 4 )
					orderby items . CustNo
					select items;
			this . CustomerGrid . ItemsSource = accounts1;
			var accounts2 = from items in MDetcollection
					where ( items . AcType == 4 )
					orderby items . CustNo
					select items;
			this . DetailsGrid . ItemsSource = accounts2;
			StatusBar . Text = "Only Records matching Account Type = 4 are shown above";
			BankCount . Text = $"{Utils . GetPrettyGridStatistics ( this . BankGrid, this . BankGrid . SelectedIndex )}";
			CustCount . Text = $"{Utils . GetPrettyGridStatistics ( this . CustomerGrid, this . CustomerGrid . SelectedIndex )}";
			DetCount . Text = $"{Utils . GetPrettyGridStatistics ( this . DetailsGrid, this . DetailsGrid . SelectedIndex )}";
			Mouse . OverrideCursor = Cursors . Arrow;
		}
		private void Linq5_Click ( object sender, RoutedEventArgs e )
		{
			int q = 1;
			//select All the items first;
			Mouse . OverrideCursor = Cursors . Wait;
			if ( q == 1 )
			{
				var accounts = from items in MBankcollection orderby items . CustNo, items . BankNo select items;
				//Next Group BankAccountViewModel collection on Custno
				var grouped = accounts . GroupBy ( b => b . CustNo );
				//Now filter content down to only those a/c's with multiple Bank A/c's
				var sel = from g in grouped where g . Count ( ) > 1 select g;
				// Finally, iterate thru the list of grouped CustNo's matching to CustNo in the full accounts data
				// giving us ONLY the full records for any recordss that have > 1 Bank accounts
				List<BankAccountViewModel> output = new List<BankAccountViewModel> ( );
				foreach ( var item1 in sel )
				{
					foreach ( var item2 in accounts )
					{
						if ( item2 . CustNo . ToString ( ) == item1 . Key )
						{ output . Add ( item2 ); }
					}
				}
				this . BankGrid . ItemsSource = output;
			}
			if ( q == 1 )
			{
				var accounts = from items in MCustcollection orderby items . CustNo, items . BankNo select items;
				//Next Group  collection on Custno
				var grouped = accounts . GroupBy ( b => b . CustNo );
				//Now filter content down to only those a/c's with multiple Bank A/c's
				var sel = from g in grouped where g . Count ( ) > 1 select g;
				// Finally, iterate thru the list of grouped CustNo's matching to CustNo in the full accounts data
				// giving us ONLY the full records for any recordss that have > 1 Bank accounts
				List<CustomerViewModel> output = new List<CustomerViewModel> ( );
				foreach ( var item1 in sel )
				{
					foreach ( var item2 in accounts )
					{
						if ( item2 . CustNo . ToString ( ) == item1 . Key )
						{ output . Add ( item2 ); }
					}
				}
				this . CustomerGrid . ItemsSource = output;
			}
			if ( q == 1 )
			{
				var accounts = from items in MDetcollection orderby items . CustNo, items . BankNo select items;
				//Next Group  collection on Custno
				var grouped = accounts . GroupBy ( b => b . CustNo );
				//Now filter content down to only those a/c's with multiple Bank A/c's
				var sel = from g in grouped where g . Count ( ) > 1 select g;
				// Finally, iterate thru the list of grouped CustNo's matching to CustNo in the full accounts data
				// giving us ONLY the full records for any recordss that have > 1 Bank accounts
				List<DetailsViewModel> output = new List<DetailsViewModel> ( );

				//				System . Diagnostics . PresentationTraceSources . SetTraceLevel ( DetailsGrid . ItemContainerGenerator, System . Diagnostics . PresentationTraceLevel . High );

				foreach ( var item1 in sel )
				{
					foreach ( var item2 in accounts )
					{
						if ( item2 . CustNo . ToString ( ) == item1 . Key )
						{ output . Add ( item2 ); }
					}
				}
				this . DetailsGrid . ItemsSource = output;
			}
			StatusBar . Text = "Only Records of Customers with multiple Bank Accounts are shown above";
			BankCount . Text = $"{Utils . GetPrettyGridStatistics ( this . BankGrid, this . BankGrid . SelectedIndex )}";
			CustCount . Text = $"{Utils . GetPrettyGridStatistics ( this . CustomerGrid, this . CustomerGrid . SelectedIndex )}";
			DetCount . Text = $"{Utils . GetPrettyGridStatistics ( this . DetailsGrid, this . DetailsGrid . SelectedIndex )}";
			Mouse . OverrideCursor = Cursors . Arrow;
		}
		private void Linq6_Click ( object sender, RoutedEventArgs e )
		{
			Mouse . OverrideCursor = Cursors . Wait;
			var accounts = from items in MBankcollection orderby items . CustNo, items . AcType select items;
			var accounts1 = from items in MCustcollection orderby items . CustNo, items . AcType select items;
			var accounts2 = from items in MDetcollection orderby items . CustNo, items . AcType select items;
			this . BankGrid . ItemsSource = accounts;
			this . CustomerGrid . ItemsSource = accounts1;
			this . DetailsGrid . ItemsSource = accounts2;
			StatusBar . Text = "All available Records are shown above in all three grids";
			BankCount . Text = $"{Utils . GetPrettyGridStatistics ( this . BankGrid, this . BankGrid . SelectedIndex )}";
			CustCount . Text = $"{Utils . GetPrettyGridStatistics ( this . CustomerGrid, this . CustomerGrid . SelectedIndex )}";
			DetCount . Text = $"{Utils . GetPrettyGridStatistics ( this . DetailsGrid, this . DetailsGrid . SelectedIndex )}";
			Mouse . OverrideCursor = Cursors . Arrow;
		}

		private void bankjoin_Click ( object sender, RoutedEventArgs e )
		{
			List<DetailsViewModel> output = new List<DetailsViewModel> ( );
			List<int> joinData = new List<int> ( );

			// create 2 lists first
			var bank = from item1 in MBankcollection select item1;
			var detail = from item2 in MDetcollection select item2;

			//select All the items first;				
			var accounts = from alldata in bank . Join (
				detail,
				bank => bank . CustNo,
				detail => detail . CustNo,
				( bank, detail ) => new
				{
					bank1 = bank . BankNo,
					bank2 = detail . BankNo,
					custno1 = bank . CustNo,
					custno2 = detail . CustNo,
					actype1 = detail . AcType,
					actype2 = bank . AcType,
					Balance1 = detail . Balance,
					Balance2 = detail . Balance,
				} )
				       select alldata;
			//accounts.So
			// Finally, iterate though the list of grouped CustNo's matching to CustNo in the full BankAccount data
			// giving us ONLY the full records for any records that have > 1 Bank accounts
			//foreach ( var item1 in sel )
			//{
			//	foreach ( var item2 in accounts )
			//	{
			//		if ( item2 . CustNo . ToString ( ) == item1 . Key )
			//		{ output . Add ( item2 ); }
			//	}
			//}
			DetailsGrid . ItemsSource = accounts;
			StatusBar . Text = $"Filtering completed, {output . Count} Multi Account records match";
			BankCount . Text = $"{Utils . GetPrettyGridStatistics ( this . BankGrid, this . BankGrid . SelectedIndex )}";
			CustCount . Text = $"{Utils . GetPrettyGridStatistics ( this . CustomerGrid, this . CustomerGrid . SelectedIndex )}";
			DetCount . Text = $"{Utils . GetPrettyGridStatistics ( this . DetailsGrid, this . DetailsGrid . SelectedIndex )}";
		}

		#endregion LINQ queries

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

		private void LinkGrid_Click ( object sender, RoutedEventArgs e )
		{
			if ( LinkGrids . IsChecked == true )
				GridsLinked = true;
			else
				GridsLinked = false;

		}

		/// <summary>
		///  Event handkler for data changes made by EditDb viewers only
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void EventControl_DataUpdated ( object sender, LoadedEventArgs e )
		{
			// Update ALL datagrids - IF we didnt   truiigger the change
			if ( sender == MBankcollection || sender == MCustcollection || sender == MDetcollection )
			{
				Mouse . OverrideCursor = Cursors . Arrow;
				return;
			}
			//			await Utils . DoBeep ( 600, 300, true );
			//			await Utils . DoSingleBeep ( 500, 100, 5 ) . ConfigureAwait ( false );

			RefreshAllGrids ( CurrentDb, e . RowCount, e . Custno, e . Bankno );
			Mouse . OverrideCursor = Cursors . Arrow;
			inprogress = false;
		}

		#region Data Edited event creators

		/// <summary>
		/// Method that is called when Bank grid has a data change made to it.
		/// It updates ALL the Db's first, then triggers a ViewerDataUpdated()  EVENT
		/// to notify any other open viewers
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BankGrid_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
		{
			string SearchCustNo = "";
			string SearchBankNo = "";
			// Save current positions so we can reposition later
			inprogress = true;

			// Set globals
			bindex = this . BankGrid . SelectedIndex;

			BankAccountViewModel CurrentBankSelectedRecord = this . BankGrid . CurrentItem as BankAccountViewModel;
			if ( CurrentBankSelectedRecord == null )
			{
				//				Console . WriteLine ( $"\nBank Grid ERROR - Currentitem is NULL on Entry to Selectionchanged !!\n" );
				//				Utils . DoErrorBeep ( 200, 100, 2 );
				//					await Utils . DoBeep ( 300, 300 ) . ConfigureAwait ( false );
				inprogress = false;
				return;
			}
			SearchCustNo = CurrentBankSelectedRecord . CustNo;
			SearchBankNo = CurrentBankSelectedRecord . BankNo;
			bindex = this . BankGrid . SelectedIndex;
			// This does the SQL update of the record that has been changed
			UpdateOnDataChange ( CurrentDb, e );
			EventControl .TriggerMultiViewerDataUpdated( MBankcollection,
				new LoadedEventArgs
				{
					CallerType = "MULTIVIEWER",
					Bankno = SearchBankNo,
					Custno = SearchCustNo,
					CallerDb = "BANKACCOUNT",
					SenderGuid = this.Tag.ToString(),
					DataSource = null,
					RowCount = this . BankGrid . SelectedIndex
				} );
			IsEditing = false;
			SaveCurrentIndex ( 1, BankGrid . SelectedIndex );

			Utils . DoSingleBeep ( 200, 300, 1 );
		}


		/// <summary>
		/// Method that is called when Customer grid has a data change made to it.
		/// It updates ALL the Db's first, then triggers a ViewerDataUpdated()  EVENT
		/// to notify any other open viewers
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CustGrid_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
		{
			string SearchCustNo = "";
			string SearchBankNo = "";
			// Save current positions so we can reposition later
			inprogress = true;

			// Set globals
			cindex = this . CustomerGrid . SelectedIndex;
			CustomerViewModel CurrentBankSelectedRecord = this . CustomerGrid . CurrentItem as CustomerViewModel;
			if ( CurrentBankSelectedRecord == null )
			{
				//				Console . WriteLine ( $"\nCustomer Grid ERROR - Currentitem is NULL on Entry to Selectionchanged !!\n" );
				//				Utils . DoErrorBeep ( 250, 100, 3 );
				//					await Utils . DoBeep ( 300, 300 ) . ConfigureAwait ( false );
				inprogress = false;
				return;
			}
			SearchCustNo = CurrentBankSelectedRecord . CustNo;
			SearchBankNo = CurrentBankSelectedRecord . BankNo;
			cindex = this . CustomerGrid . SelectedIndex;

			// This does the SQL update of the record that has been changed
			UpdateOnDataChange ( CurrentDb, e );
//			EventControl.TriggerMultiViewerDataUpdated
			EventControl.TriggerMultiViewerDataUpdated(
				MCustcollection,
				new LoadedEventArgs
				{
					CallerType = "MULTIVIEWER",
					Bankno = SearchBankNo,
					Custno = SearchCustNo,
					CallerDb = "CUSTOMER",
					SenderGuid = this.Tag.ToString(),
					DataSource = MCustcollection,
					RowCount = this . CustomerGrid . SelectedIndex
				} );
			IsEditing = false;
			Utils . DoSingleBeep ( 300, 300, 2 );
			SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
			inprogress = false;
		}
		/// <summary>
		/// Method that is called when Details grid has a data change made to it.
		/// It updates ALL the Db's first, then triggers a ViewerDataUpdated()  EVENT
		/// to notify any other open viewers
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DetailsGrid_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
		{
			string SearchCustNo = "";
			string SearchBankNo = "";
			// Save current positions so we can reposition later
			inprogress = true;

			// Set globals
			//dindex = this . DetailsGrid . SelectedIndex;
			DetailsViewModel CurrentBankSelectedRecord = this . DetailsGrid . CurrentItem as DetailsViewModel;
			if ( CurrentBankSelectedRecord == null )
			{
				//				Console . WriteLine ( $"\nDetails Grid ERROR - Currentitem is NULL on Entry to Selectionchanged !!\n" );
				//				Utils . DoErrorBeep ( 300, 100 , 4);
				inprogress = false;
				return;
			}
			SearchCustNo = CurrentBankSelectedRecord . CustNo;
			SearchBankNo = CurrentBankSelectedRecord . BankNo;
			dindex = this . DetailsGrid . SelectedIndex;

			// This does the SQL update of the record that has been changed
			UpdateOnDataChange ( CurrentDb, e );
			EventControl .TriggerMultiViewerDataUpdated( MDetcollection,
				new LoadedEventArgs
				{
					CallerType = "MULTIVIEWER",
					Bankno = SearchBankNo,
					Custno = SearchCustNo,
					CallerDb = "DETAILS",
					SenderGuid = this.Tag.ToString(),
					DataSource = MDetcollection,
					RowCount = this . DetailsGrid . SelectedIndex
				} );
			IsEditing = false;
			Utils . DoSingleBeep ( 400, 300, 3 );
			SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
			inprogress = false;
		}
		#endregion Data Edited event creators



		/// <summary>
		/// Main Event handler for data changes made in this Multi viewer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void EventControl_ViewerDataUpdated ( object sender, LoadedEventArgs e )
		{
			// Update ALL datagrids - IF we didnt   truiigger the change
			if ( sender == MBankcollection )// || sender == MultiCustcollection || sender == MultiDetcollection )
			{
				// Bank updated a row, so just update Customer and Details
				await CustCollection . LoadCust ( MCustcollection, "MULTIVIEWER", 3, true );
				await DetailCollection . LoadDet ( MDetcollection, "MULTIVIEWER", 3, true );
				Mouse . OverrideCursor = Cursors . Arrow;
				inprogress = false;
				return;
			}
			else if ( sender == MCustcollection )// || sender == MultiCustcollection || sender == MultiDetcollection )
			{
				// Customer updated a row, so just update Bank and Details
				await BankCollection . LoadBank ( MBankcollection, "MULTIVIEWER", 3, true );
				await DetailCollection . LoadDet ( MDetcollection, "MULTIVIEWER", 3, true );
				Mouse . OverrideCursor = Cursors . Arrow;
				inprogress = false;
				return;
			}
			else if ( sender == MDetcollection )// || sender == MultiCustcollection || sender == MultiDetcollection )
			{
				// Details updated a row, so just update Customer and Bank
				await BankCollection . LoadBank ( MBankcollection, "MULTIVIEWER", 3, true );
				await CustCollection . LoadCust ( MCustcollection, "MULTIVIEWER", 3, true );
				Mouse . OverrideCursor = Cursors . Arrow;
				inprogress = false;
				return;
			}
			//RefreshAllGrids ( CurrentDb, e . RowCount, e . Custno, e . Bankno );
		}
		/// <summary>
		/// Main Event handler for data changes made in EXTERNAL multiviewers
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void EventControl_SqlViewerDataUpdated ( object sender, LoadedEventArgs e )
		{
			// Update ALL datagrids - IF we didnt   triigger the change

			if ( e . CallerDb == "MULTIVIEWER" ) return;

			await BankCollection . LoadBank ( MBankcollection, "MULTIVIEWER", 3, true );
			await CustCollection . LoadCust ( MCustcollection, "MULTIVIEWER", 3, true );
			await DetailCollection . LoadDet ( MDetcollection, "MULTIVIEWER", 3, true );
			Mouse . OverrideCursor = Cursors . Arrow;
			inprogress = false;
			return;
		}

		/// <summary>
		/// Generic method to send Index changed Event trigger so that
		/// other viewers can update thier own grids as relevant
		/// </summary>
		/// <param name="grid"></param>
		//*************************************************************************************************************//
		public void TriggerMultiViewerIndexChanged ( DataGrid grid )
		{
			string SearchCustNo = "";
			string SearchBankNo = "";

			if ( LoadingDbData ) return;
			if ( grid == this . BankGrid )
			{
				BankAccountViewModel CurrentBankSelectedRecord = this . BankGrid . SelectedItem as BankAccountViewModel;
				if ( CurrentBankSelectedRecord == null ) return;
				SearchCustNo = CurrentBankSelectedRecord . CustNo;
				SearchBankNo = CurrentBankSelectedRecord . BankNo;
				EventControl . TriggerMultiViewerIndexChanged ( this,
					new IndexChangedArgs
					{
						Senderviewer = this,
						Bankno = SearchBankNo,
						Custno = SearchCustNo,
						dGrid = this . BankGrid,
						Sender = "BANKACCOUNT",
						SenderId = "MULTIVIEWER",
						Row = this . BankGrid . SelectedIndex
					} );
			}
			else if ( grid == this . CustomerGrid )
			{
				CustomerViewModel CurrentCustSelectedRecord = this . CustomerGrid . CurrentItem as CustomerViewModel;
				if ( CurrentCustSelectedRecord == null ) return;
				SearchCustNo = CurrentCustSelectedRecord . CustNo;
				SearchBankNo = CurrentCustSelectedRecord . BankNo;
				EventControl . TriggerMultiViewerIndexChanged ( this,
				new IndexChangedArgs
				{
					Senderviewer = this,
					Bankno = SearchBankNo,
					Custno = SearchCustNo,
					dGrid = this . CustomerGrid,
					Sender = "CUSTOMER",
					SenderId = "MULTIVIEWER",
					Row = this . CustomerGrid . SelectedIndex
				} );
			}
			else if ( grid == this . DetailsGrid )
			{
				DetailsViewModel CurrentDetSelectedRecord = this . DetailsGrid . CurrentItem as DetailsViewModel;
				if ( CurrentDetSelectedRecord == null ) return;
				SearchCustNo = CurrentDetSelectedRecord . CustNo;
				SearchBankNo = CurrentDetSelectedRecord . BankNo;
				EventControl . TriggerMultiViewerIndexChanged ( this,
					new IndexChangedArgs
					{
						Senderviewer = this,
						Bankno = SearchBankNo,
						Custno = SearchCustNo,
						dGrid = this . DetailsGrid,
						Sender = "DETAILS",
						SenderId = "MULTIVIEWER",
						Row = this . DetailsGrid . SelectedIndex
					} );
			}
		}
		private void Window_PreviewKeyDown ( object sender, System . Windows . Input . KeyEventArgs e )
		{
			return;

			DataGrid dg = null;
			int CurrentRow = 0;
			bool showdebug = false;

			if ( showdebug ) Debug . WriteLine ( $"key1 = {key1},  Key = : {e . Key}" );

			if ( IsEditing ) return;

			if ( e . Key == Key . LeftCtrl )
			{
				key1 = true;
				if ( showdebug ) Debug . WriteLine ( $"key1 = set to TRUE" );
				return;
			}
			//if ( key1 )
			//{
			//	Utils . HandleCtrlFnKeys ( key1, e );
			//	key1 = false;
			//}
			if ( key1 && e . Key == Key . F3 )  // CTRL + F3
			{
				// list MultiViewer static indexes
				Debug . WriteLine ( $"\nMultiViewer static indexes" );
				Debug . WriteLine ( $"bindex = {bindex}" );
				Debug . WriteLine ( $"cindex = {cindex}" );
				Debug . WriteLine ( $"dindex = {dindex}" );
				e . Handled = true;
				key1 = false;
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
			else if ( key1 && e . Key == Key . F8 )  // CTRL + F8
			{
				// list various Flags in Console
				Flags . PrintSundryVariables ( "Window_PreviewKeyDown()" );
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
				if ( showdebug ) Debug . WriteLine ( "\nGridview GV[] Variables" );
				Flags . ListGridviewControlFlags ( 1 );
				key1 = false;
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
					dg = this . BankGrid;
				else if ( CurrentDb == "CUSTOMER" )
					dg = this . CustomerGrid;
				else
					dg = this . DetailsGrid;
				if ( dg . SelectedIndex > 0 )
				{
					dg . SelectedIndex--;
					dg . SelectedItem = dg . SelectedIndex;
					if ( dg . SelectedItem != null )
						Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				}
				if ( dg == BankGrid )
					SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
				else if ( dg == CustomerGrid )
					SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
				else if ( dg == DetailsGrid )
					SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
				if ( dg . SelectedItem == null )
					Utils . ScrollRecordInGrid ( dg, 0 );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( e . Key == Key . Down )
			{       // DataGrid keyboard navigation = DOWN
				if ( CurrentDb == "BANKACCOUNT" )
					dg = this . BankGrid;
				else if ( CurrentDb == "CUSTOMER" )
					dg = this . CustomerGrid;
				else if ( CurrentDb == "DETAILS" )
					dg = this . DetailsGrid;
				if ( dg . SelectedIndex < dg . Items . Count - 1 )
				{
					dg . SelectedIndex++;
					dg . SelectedItem = dg . SelectedIndex;
					if ( dg . SelectedItem != null )
						Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				}
				if ( dg == BankGrid )
					SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
				else if ( dg == CustomerGrid )
					SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
				else if ( dg == DetailsGrid )
					SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
				if ( dg . SelectedItem == null )
					Utils . ScrollRecordInGrid ( dg, 0 );

				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( e . Key == Key . PageUp )
			{       // DataGrid keyboard navigation = PAGE UP
				if ( CurrentDb == "BANKACCOUNT" )
					dg = this . BankGrid;
				else if ( CurrentDb == "CUSTOMER" )
					dg = this . CustomerGrid;
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
				if ( dg == BankGrid )
					SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
				else if ( dg == CustomerGrid )
					SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
				else if ( dg == DetailsGrid )
					SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
				if ( dg . SelectedItem != null )
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( e . Key == Key . PageDown )
			{       // DataGrid keyboard navigation = PAGE DOWN
				if ( CurrentDb == "BANKACCOUNT" )
					dg = this . BankGrid;
				else if ( CurrentDb == "CUSTOMER" )
					dg = this . CustomerGrid;
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
				if ( dg == BankGrid )
					SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
				else if ( dg == CustomerGrid )
					SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
				else if ( dg == DetailsGrid )
					SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
				if ( dg . SelectedItem != null )
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( e . Key == Key . Home )
			{       // DataGrid keyboard navigation = HOME
				if ( CurrentDb == "BANKACCOUNT" )
					dg = this . BankGrid;
				else if ( CurrentDb == "CUSTOMER" )
					dg = this . CustomerGrid;
				else
					dg = this . DetailsGrid;
				dg . SelectedIndex = 0;
				if ( dg . SelectedItem != null )
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				if ( dg . SelectedItem != null )
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				if ( dg == BankGrid )
					SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
				else if ( dg == CustomerGrid )
					SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
				else if ( dg == DetailsGrid )
					SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( e . Key == Key . End )
			{       // DataGrid keyboard navigation = END
				if ( CurrentDb == "BANKACCOUNT" )
					dg = this . BankGrid;
				else if ( CurrentDb == "CUSTOMER" )
					dg = this . CustomerGrid;
				else
					dg = this . DetailsGrid;
				dg . SelectedIndex = dg . Items . Count - 1;
				if ( dg . SelectedItem != null )
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				if ( dg . SelectedItem != null )
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );
				if ( dg == BankGrid )
					SaveCurrentIndex ( 1, BankGrid . SelectedIndex );
				else if ( dg == CustomerGrid )
					SaveCurrentIndex ( 2, CustomerGrid . SelectedIndex );
				else if ( dg == DetailsGrid )
					SaveCurrentIndex ( 3, DetailsGrid . SelectedIndex );
				e . Handled = true;
				key1 = false;
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
				MultiViewer Thisviewer = this;

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
						SenderGuid = this.Tag.ToString(),
						DataSource = MBankcollection,
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
					CustCollection . dtCust?.Clear ( );
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
						SenderGuid = this.Tag.ToString(),
						DataSource = MCustcollection,
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
						SenderGuid = this.Tag.ToString(),
						DataSource = MDetcollection,
						RowCount = CurrentRow
					} );
					// Keep our focus in originating window for now
					Thisviewer . Activate ( );
					Thisviewer . Focus ( );
					return;
				}
				e . Handled = false;
				// Tidy up our own grid after ourselves
				if ( dg . Items . Count > 0 && CurrentRow >= 0 )
					dg . SelectedIndex = CurrentRow;
				else if ( dg . Items . Count == 1 )
					dg . SelectedIndex = 0;

				//dg.SelectedIndex = Flags.
				dg . SelectedItem = dg . SelectedIndex;
				if ( dg . SelectedItem != null )
					Utils . ScrollRecordInGrid ( dg, dg . SelectedIndex );

			}
			e . Handled = false;
		}

		private async void BankGrid_PreviewMouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{
			ContextMenu cm = this . FindResource ( "ContextMenu1" ) as ContextMenu;
			cm . PlacementTarget = this . BankGrid as DataGrid;
			cm . IsOpen = true;
			CurrentDb = "BANKACCOUNT";


			//			if ( e . ChangedButton == MouseButton . Right )
			//			{
			//				DataGridRow RowData = new DataGridRow ( );
			//				int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
			//				if ( row == -1 ) row = 0;
			//				RowInfoPopup rip = new RowInfoPopup ( "BANKACCOUNT", BankGrid);
			//				rip . Topmost = true;
			//				rip . DataContext = RowData;
			//				rip . BringIntoView ( );
			//				rip . Focus ( );
			//				rip . ShowDialog ( );

			//				//If data has been changed, update everywhere
			//				// Update the row on return in case it has been changed
			//				if ( rip . IsDirty )
			//				{
			//					this . BankGrid . ItemsSource = null;
			//					this . BankGrid . Items . Clear ( );
			//					MBankcollection = await BankCollection . LoadBank ( MBankcollection, "MULTIVIEWER", 1, true );
			////					this . BankGrid . ItemsSource = MultiBankcollection;
			//					//					StatusBar . Text = "Current Record Updated Successfully...";
			//					// Notify everyone else of the data change
			//					EventControl . TriggerViewerDataUpdated ( MBankcollection,
			//						new LoadedEventArgs
			//						{
			//							CallerType = "MULTIVIEWER",
			//							CallerDb = "BANKACCOUNT",
			//							DataSource = MBankcollection,
			//							RowCount = this . BankGrid . SelectedIndex
			//						} );
			//				}
			//				else
			//					this . BankGrid . SelectedItem = RowData . Item;

			//				// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
			//				//Utils . SetUpGridSelection ( this . BankGrid, row );
			//				//// This is essential to get selection activated again
			//				this . BankGrid . Focus ( );
			//			}
		}

		private async void CustGrid_PreviewMouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{
			ContextMenu cm = this . FindResource ( "ContextMenu1" ) as ContextMenu;
			cm . PlacementTarget = this . CustomerGrid as DataGrid;
			cm . IsOpen = true;
			CurrentDb = "CUSTOMER";
			//			if ( e . ChangedButton == MouseButton . Right )
			//			{
			//				DataGridRow RowData = new DataGridRow ( );
			//				int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
			//				if ( row == -1 ) row = 0;
			//				RowInfoPopup rip = new RowInfoPopup ( "CUSTOMER", CustomerGrid);
			//				rip . Topmost = true;
			//				rip . DataContext = RowData;
			//				rip . BringIntoView ( );
			//				rip . Focus ( );
			//				rip . ShowDialog ( );

			//				//If data has been changed, update everywhere
			//				// Update the row on return in case it has been changed
			//				if ( rip . IsDirty )
			//				{
			//					this . CustomerGrid . ItemsSource = null;
			//					this . CustomerGrid . Items . Clear ( );
			//					MCustcollection = await CustCollection . LoadCust ( MCustcollection, "MULTIVIEWER", 1, true );
			////					this . CustomerGrid . ItemsSource = MultiCustcollection;
			//					//					StatusBar . Text = "Current Record Updated Successfully...";
			//					// Notify everyone else of the data change
			//					EventControl . TriggerViewerDataUpdated ( MCustcollection,
			//						new LoadedEventArgs
			//						{
			//							CallerType = "MULTIVIEWER",
			//							CallerDb = "CUSTOMER",
			//							DataSource = MCustcollection,
			//							RowCount = this . CustomerGrid . SelectedIndex
			//						} );
			//				}
			//				else
			//					this . CustomerGrid . SelectedItem = RowData . Item;

			//				// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
			////				Utils . SetUpGridSelection ( this . CustomerGrid, row );
			//				// This is essential to get selection activated again
			//				this . CustomerGrid . Focus ( );
			//			}
		}

		private async void DetGrid_PreviewMouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{
			ContextMenu cm = this . FindResource ( "ContextMenu1" ) as ContextMenu;
			cm . PlacementTarget = this . DetailsGrid as DataGrid;
			cm . IsOpen = true;
			CurrentDb = "DETAILS";
			//			if ( e . ChangedButton == MouseButton . Right )
			//			{
			//				DataGridRow RowData = new DataGridRow ( );
			//				int row = DataGridSupport . GetDataGridRowFromTree ( e, out RowData );
			//				if ( row == -1 ) row = 0;
			//				RowInfoPopup rip = new RowInfoPopup ( "DETAILS", DetailsGrid);
			//				rip . Topmost = true;
			//				rip . DataContext = RowData;
			//				rip . BringIntoView ( );
			//				rip . Focus ( );
			//				rip . ShowDialog ( );

			//				//If data has been changed, update everywhere
			//				// Update the row on return in case it has been changed
			//				if ( rip . IsDirty )
			//				{
			//					this . DetailsGrid . ItemsSource = null;
			//					this . DetailsGrid . Items . Clear ( );
			//					MDetcollection = await DetCollection . LoadDet ( MDetcollection, 1, true );
			////					this . DetailsGrid . ItemsSource = MultiDetcollection;
			//					//					StatusBar . Text = "Current Record Updated Successfully...";
			//					// Notify everyone else of the data change
			//					EventControl . TriggerViewerDataUpdated ( MDetcollection,
			//						new LoadedEventArgs
			//						{
			//							CallerType = "MULTIVIEWER",
			//							CallerDb = "DETAILS",
			//							DataSource = MDetcollection,
			//							RowCount = this . DetailsGrid . SelectedIndex
			//						} );
			//				}
			//				else
			//					this . DetailsGrid . SelectedItem = RowData . Item;

			//				// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
			////				Utils . SetUpGridSelection ( this . DetailsGrid, row );
			//				// This is essential to get selection activated again
			//				this . DetailsGrid . Focus ( );
			//			}
		}

		private void BankGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
		{
			IsEditing = true;
		}

		private void CustomerGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
		{
			IsEditing = true;

		}

		private void DetailsGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
		{
			IsEditing = true;
		}
		/// <summary>
		/// Savres the current selectedIndex for each grid
		/// </summary>
		/// <param name="type"></param>
		/// <param name="index"></param>
		private void SaveCurrentIndex ( int type, int index )
		{
			if ( type == 1 ) bindex = index;
			if ( type == 2 ) cindex = index;
			if ( type == 3 ) dindex = index;
		}

		private void ExportBankCSV_Click ( object sender, RoutedEventArgs e )
		{

		}

		private void ExportCustCSV_Click ( object sender, RoutedEventArgs e )
		{

		}

		private void ExportDetCSV_Click ( object sender, RoutedEventArgs e )
		{

		}

		private void ImportBankCSV_Click ( object sender, RoutedEventArgs e )
		{

		}

		private void ImportCustCSV_Click ( object sender, RoutedEventArgs e )
		{

		}

		private void ImportDetCSV_Click ( object sender, RoutedEventArgs e )
		{

		}
		private void BankGrid_PreviewDragEnter ( object sender, DragEventArgs e )
		{
			e . Effects = ( DragDropEffects ) DragDropEffects . Move;
			Debug . WriteLine ( $"Setting drag cursor...." );

		}
		private void Grids_PreviewMouseLeftButtondown ( object sender, MouseButtonEventArgs e )
		{
			// Gotta make sure it is not anywhere in the Scrollbar we clicked on 
			if ( Utils . HitTestScrollBar ( sender, e ) ) { ScrollBarMouseMove = true; return; }
			if ( Utils . HitTestHeaderBar ( sender, e ) ) return;
			_startPoint = e . GetPosition ( null );
			// Make sure the left mouse button is pressed down so we are really moving a record
			if ( e . LeftButton == MouseButtonState . Pressed )
			{
				IsLeftButtonDown = true;
				this . Focus ( );
			}
		}
		private void Grids_PreviewMouseLeftButtonup ( object sender, MouseButtonEventArgs e )
		{
			ScrollBarMouseMove = false;
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
				// Make sure the left mouse button is pressed down so we are really moving a record
				if ( e . LeftButton == MouseButtonState . Pressed )
				{
					if ( ScrollBarMouseMove )
					{
						return;
					}

					if ( BankGrid . SelectedItem != null )
					{
						// We are dragging from the BANK grid
						//Working string version
						BankAccountViewModel bvm = new BankAccountViewModel ( );
						bvm = BankGrid . SelectedItem as BankAccountViewModel;
						string str = GetExportRecords . CreateTextFromRecord ( bvm, null, null, true, false );
						string dataFormat = DataFormats . Text;
						DataObject dataObject = new DataObject ( dataFormat, str );

						System . Windows . DragDrop . DoDragDrop (
						BankGrid,
						dataObject,
						DragDropEffects . Move );
					}
				}
			}
		}

		private void CustomerGrid_PreviewMouseMove ( object sender, MouseEventArgs e )
		{
			Point mousePos = e.GetPosition(null);
			Vector diff = _startPoint - mousePos;

			if (e.LeftButton == MouseButtonState.Pressed &&
			    Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
			    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
			{
				// Make sure the left mouse button is pressed down so we are really moving a record
				if (e.LeftButton == MouseButtonState.Pressed)
				{
					if (CustomerGrid.SelectedItem != null)
					{
						if (ScrollBarMouseMove)
						{
							return;
						}
						// We are dragging from the Customer grid
						//Working string version
						CustomerViewModel cvm = new CustomerViewModel();
						cvm = CustomerGrid.SelectedItem as CustomerViewModel;
						string str = GetExportRecords.CreateTextFromRecord(null, null, cvm, true, false);
						string dataFormat = DataFormats.Text;
						DataObject dataObject = new DataObject(dataFormat, str);
						System.Windows.DragDrop.DoDragDrop(
						CustomerGrid,
						dataObject,
						DragDropEffects.Move);
					}
				}
			}
		}

		private void DetailsGrid_PreviewMouseMove ( object sender, MouseEventArgs e )
		{
			Point mousePos = e.GetPosition(null);
			Vector diff = _startPoint - mousePos;

			if (e.LeftButton == MouseButtonState.Pressed &&
			    Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
			    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
			{
				// Make sure the left mouse button is pressed down so we are really moving a record
				if (e.LeftButton == MouseButtonState.Pressed)
				{
					if (DetailsGrid.SelectedItem != null)
					{
						if (ScrollBarMouseMove)
						{
							return;
						}
						// We are dragging from the DETAILS grid
						//Working string version
						DetailsViewModel dvm = new DetailsViewModel();
						dvm = DetailsGrid.SelectedItem as DetailsViewModel;
						string str = GetExportRecords.CreateTextFromRecord(null, dvm, null, true, false);
						string dataFormat = DataFormats.Text;
						DataObject dataObject = new DataObject(dataFormat, str);
						System.Windows.DragDrop.DoDragDrop(
						BankGrid,
						dataObject,
						DragDropEffects.Move);
					}
				}
			}
		}

		private void ContextEdit_Click ( object sender, RoutedEventArgs e )
		{
			RowInfoPopup rip = ( RowInfoPopup ) null;
			// handle flags to let us know WE have triggered the selectedIndex change
			//			MainWindow . DgControl . SelectionChangeInitiator = 2; // tells us it is a EditDb initiated the record change
			DataGridRow RowData = new DataGridRow ( );
			if ( CurrentDb == "BANKACCOUNT" )
			{
				BankAccountViewModel bvm = new BankAccountViewModel ( );
				bvm = this . BankGrid . SelectedItem as BankAccountViewModel;
				rip = new RowInfoPopup ( "BANKACCOUNT", BankGrid );
				rip . DataContext = bvm;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				CustomerViewModel cvm = new CustomerViewModel ( );
				cvm = this . CustomerGrid . SelectedItem as CustomerViewModel;
				rip = new RowInfoPopup ( "CUSTOMER", CustomerGrid );
				rip . DataContext = cvm;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				DetailsViewModel dvm = new DetailsViewModel ( );
				dvm = this . DetailsGrid . SelectedItem as DetailsViewModel;
				rip = new RowInfoPopup ( "DETAILS", DetailsGrid );
				rip . DataContext = dvm;
			}
			rip . Topmost = true;
			rip . DataContext = RowData;
			rip . BringIntoView ( );
			rip . Focus ( );
			rip . ShowDialog ( );

			//If data has been changed, update everywhere
			// Update the row on return in case it has been changed
			//if ( rip . IsDirty )
			//{
			//	this . BankGrid . ItemsSource = null;
			//	this . BankGrid . Items . Clear ( );
			//	// Save our reserve collection
			//	BankReserved = null;

			//	BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );
			//	this . BankGrid . ItemsSource = SqlBankcollection;
			//	StatusBar . Text = "Current Record Updated Successfully...";
			//	// Notify everyone else of the data change
			//	EventControl . TriggerViewerDataUpdated ( SqlBankcollection,
			//		new LoadedEventArgs
			//		{
			//			CallerType = "SQLDBVIEWER",
			//			CallerDb = "BANKACCOUNT",
			//			DataSource = SqlBankcollection,
			//			RowCount = this . BankGrid . SelectedIndex
			//		} );
			//}
			//else
			//	this . BankGrid . SelectedItem = RowData . Item;

			// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
			Utils . SetUpGridSelection ( this . BankGrid, this . BankGrid . SelectedIndex );
			//ParseButtonText ( true );
			//Count . Text = $"{Utils . GetPrettyGridStatistics ( this . BankGrid, this . BankGrid . SelectedIndex )}";
			//				Count . Text = $"{this . BankGrid . SelectedIndex} / { this . BankGrid . Items . Count . ToString ( )}";
			//				Count . Text = this . BankGrid . Items . Count . ToString ( );
			// This is essential to get selection activated again
			this . BankGrid . Focus ( );
		}

		private void ContextSave_Click ( object sender, RoutedEventArgs e )
		{
			string path = "";
			if ( CurrentDb == "BANKACCOUNT" )
			{
				path = @"C:\\Users\\Ianch\\Documents\\BankCollectiondata.json";
				JsonSupport . JsonSerialize ( MBankcollection, path );
				MessageBox . Show ( $"The Db data has been saved successfully in 'Json' format ...\n\nFile is : [{path}]", "Data Persistence System" );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				path = @"C:\\Users\\Ianch\\Documents\\CustCollectiondata.json";
				JsonSupport . JsonSerialize ( MCustcollection, path );
				MessageBox . Show ( $"The Db data has been saved successfully in 'Json' format ...\n\nFile is : [{path}]", "Data Persistence System" );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				path = @"C:\\Users\\Ianch\\Documents\\DetCollectiondata.json";
				JsonSupport . JsonSerialize ( MDetcollection, path );
				MessageBox . Show ( $"The Db data  has been saved successfully in 'Json' format ...\n\nFile is : [{path}]", "Data Persistence System" );
			}
		}

		private void ContextShowJson_Click ( object sender, RoutedEventArgs e )
		{
			if ( CurrentDb == "BANKACCOUNT" )
			{
				// Works fine !!! 22/6/21
				// grab current record and  convert it to a Json record
				BankAccountViewModel bvm = new BankAccountViewModel ( );
				bvm = this . BankGrid . SelectedItem as BankAccountViewModel;
				JObject obj = JObject . FromObject ( bvm );
				string s = JsonConvert . SerializeObject ( new { obj } );
				// we have our string in 's'
				// // show it in a messagebox fully formatted				
				int rows = 0;
				MessageBox . Show ( JsonSupport . CreateFormattedJsonOutput ( s, "BankAccount" ) . ToString ( ), "Json formatted record data" );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				CustomerViewModel cvm = new CustomerViewModel ( );
				cvm = this . CustomerGrid . SelectedItem as CustomerViewModel;
				JObject obj = JObject . FromObject ( cvm );
				string s = JsonConvert . SerializeObject ( new { obj } );
				// we have our string in 's'
				// // show it in a messagebox fully formatted				
				int rows = 0;
				MessageBox . Show ( JsonSupport . CreateFormattedJsonOutput ( s, "Customer" ) . ToString ( ), "Json formatted record data" );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				DetailsViewModel dvm = new DetailsViewModel ( );
				dvm = this . DetailsGrid . SelectedItem as DetailsViewModel;
				JObject obj = JObject . FromObject ( dvm );
				string s = JsonConvert . SerializeObject ( new { obj } );
				// we have our string in 's'
				// // show it in a messagebox fully formatted				
				int rows = 0;
				MessageBox . Show ( JsonSupport . CreateFormattedJsonOutput ( s, "Details" ) . ToString ( ), "Json formatted record data" );
			}
		}
		private void ContextDisplayJsonData_Click ( object sender, RoutedEventArgs e )
		{
			//============================================//
			//MENU ITEM 'Read and display JSON File'
			//============================================//
			object DbData = new object ( );
			string resultString = "", path = "";
			string jsonresult = "";

			Progressbar pbar = new Progressbar ( );
			pbar . Show ( );
			Mouse . OverrideCursor = Cursors . Wait;
			StatusBar . Text = "Please wait, This process can take a little while !!";
			this . Refresh ( );
			//We need to save current Collectionview as a Json (binary) data to disk
			// this is the best way to save persistent data in Json format
			//using tmp folder for interim file that we will then display

			if ( CurrentDb == "BANKACCOUNT" )
				JsonSupport . CreateShowJsonText (false, CurrentDb, MBankcollection );
			else if ( CurrentDb == "CUSTOMER" )
				JsonSupport . CreateShowJsonText (false, CurrentDb, MCustcollection );
			else if ( CurrentDb == "DETAILS" )
				JsonSupport . CreateShowJsonText (false, CurrentDb, MDetcollection );
		}

		private async void ContextCreateJsonOutput_Click ( object sender, RoutedEventArgs e )
		{
			int rows = 0;
			Stopwatch sw = new Stopwatch ( );
			//Read the data from disk file BankCollectiondata.json & then
			// parse  it out into traditional JSON format for viewing
			// Takes a while though !!! about 20 seconds for 5000 records
			JsonReader reader;
			BankAccountViewModel bvm = new BankAccountViewModel ( );
			//Read Json (binary) data from disk
			object obj = JsonSupport . JsonDeserialize ( @"C:\\users\ianch\documents\BankCollectiondata.json " );
			string Output = "", jsonstring = "";
			//			StringBuilder sb = Utils . CreateJsonFileFromJsonObject ( obj, out Output );
			Debug . WriteLine ( $"\nStarting creation of JSON format file of database ..." );
			sw . Start ( );
			jsonstring = JsonSupport . CreateFormattedJsonOutput ( Output, "Bank" );
			//jsonstring = tmp . ToString ( );
			sw . Stop ( );
			Debug . WriteLine ( $"Completed creation of JSON format file of database - {rows} were createdin {( double ) sw . ElapsedMilliseconds / ( double ) 1000} seconds...\n" );
			// Read string data back
			// We now have a formatted JSON style output buffer :- jsonstring
			// Save it to disk so wecan display it in Wordpad or whatever is chosen ?
			string path = @"C:\users\ianch\Documents\Formatteddata.json";
			File . WriteAllText ( path, jsonstring );
			try
			{
				//Setup our delegate
				QualifyingFileLocations FindPathHandler = SupportMethods . qualifiers;
				// pass the delegate method thru to our search for executable path method
				// It contains all the specialist paths we want to have searched
				// WORKS VERY WELL 15/6/21
				string test = SupportMethods . FindExecutePath ( $"Notepad.exe jsonstring", SupportMethods . qualifiers );
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Failure in Utils.FindExecutePath()\n{ex . Message}, {ex . Data}" );
				//return false;
			}
			Process p = null;
			// Show the traditional JSON output we have just created
			p = Process . Start ( "Wordpad.exe", path );
		}

		private void ContextClose_Click ( object sender, RoutedEventArgs e )
		{
			this . Close ( );
		}

		private void ContextSettings_Click ( object sender, RoutedEventArgs e )
		{
			Setup setup = new Setup ( );
			setup . Show ( );
			setup . BringIntoView ( );
			setup . Topmost = true;
			this . Focus ( );
		}
		/* UNUSED METHODS

	#region UNUSED CODE

	/// <summary>
	/// Method that is called when ANY grid has a data change made to it.
	/// It updates ALL the Db's first, then triggers a ViewerDataUpdated()  EVENT
	/// to notify any other open viewers
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private async void ViewerGrid_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
	{
		//	string SearchCustNo = "";
		//	string SearchBankNo = "";
		//	// Save current positions so we can reposition later
		//	inprogress = true;

		//	// Set globals
		//	bindex = this . BankGrid . SelectedIndex;
		//	cindex = this . CustomerGrid . SelectedIndex;
		//	dindex = this . DetailsGrid . SelectedIndex;


		//	if ( CurrentDb == "BANKACCOUNT" )
		//	{
		//		BankAccountViewModel CurrentBankSelectedRecord = this . BankGrid . CurrentItem as BankAccountViewModel;
		//		if ( CurrentBankSelectedRecord == null )
		//		{
		//			Console . WriteLine ( $"Bank Grid ERROR - Currentitem is NULL on Entry to Selectionchanged !!" );
		//			//					await Utils . DoBeep ( 300, 300 ) . ConfigureAwait ( false );
		//			return;
		//		}
		//		SearchCustNo = CurrentBankSelectedRecord . CustNo;
		//		SearchBankNo = CurrentBankSelectedRecord . BankNo;
		//		bindex = this . BankGrid . SelectedIndex;
		//		// This does the SQL update of the record that has been changed
		//		UpdateOnDataChange ( CurrentDb, e );
		//		EventControl . TriggerMultiViewerDataUpdated ( MultiBankcollection,
		//			new LoadedEventArgs
		//			{
		//				Bankno = SearchBankNo,
		//				Custno = SearchCustNo,
		//				CallerDb = "BANKACCOUNT",
		//				DataSource = MultiBankcollection,
		//				RowCount = this . BankGrid . SelectedIndex
		//			} );
		//	}
		//	else if ( CurrentDb == "CUSTOMER" )
		//	{
		//		CustomerViewModel CurrentBankSelectedRecord = this . CustomerGrid . CurrentItem as CustomerViewModel;
		//		if ( CurrentBankSelectedRecord == null )
		//		{
		//			Console . WriteLine ( $"Customer Grid ERROR - Currentitem is NULL on Entry to Selectionchanged !!" );
		//			//					await Utils . DoBeep ( 300, 300 ) . ConfigureAwait ( false );
		//			return;
		//		}
		//		SearchCustNo = CurrentBankSelectedRecord . CustNo;
		//		SearchBankNo = CurrentBankSelectedRecord . BankNo;
		//		cindex = this . CustomerGrid . SelectedIndex;

		//		// This does the SQL update of the record that has been changed
		//		UpdateOnDataChange ( CurrentDb, e );
		//		EventControl . TriggerMultiViewerDataUpdated ( MultiCustcollection,
		//			new LoadedEventArgs
		//			{
		//				Bankno = SearchBankNo,
		//				Custno = SearchCustNo,
		//				CallerDb = "CUSTOMER",
		//				DataSource = MultiCustcollection,
		//				RowCount = this . CustomerGrid . SelectedIndex
		//			} );
		//	}
		//	else if ( CurrentDb == "DETAILS" )
		//	{
		//		DetailsViewModel CurrentBankSelectedRecord = this . DetailsGrid . CurrentItem as DetailsViewModel;
		//		if ( CurrentBankSelectedRecord == null )
		//		{
		//			Console . WriteLine ( $"Details Grid ERROR - Currentitem is NULL on Entry to Selectionchanged !!" );
		//			//					await Utils . DoBeep ( 300, 300 ) . ConfigureAwait ( false );
		//			return;
		//		}
		//		SearchCustNo = CurrentBankSelectedRecord . CustNo;
		//		SearchBankNo = CurrentBankSelectedRecord . BankNo;
		//		dindex = this . DetailsGrid . SelectedIndex;

		//		// This does the SQL update of the record that has been changed
		//		UpdateOnDataChange ( CurrentDb, e );
		//		EventControl . TriggerMultiViewerDataUpdated ( MultiDetcollection,
		//			new LoadedEventArgs
		//			{
		//				Bankno = SearchBankNo,
		//				Custno = SearchCustNo,
		//				CallerDb = "DETAILS",
		//				DataSource = MultiDetcollection,
		//				RowCount = this . DetailsGrid . SelectedIndex
		//			} );
		//	}
		//	ResetIndexes ( );
		//	inprogress = false;
	}

	/// <summary>
	/// NOT IN USE currently !!!
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void EventControl_EditIndexChanged ( object sender, IndexChangedArgs e )
	{
		EventControl_ViewerIndexChanged ( sender, e );
		//Triggered = true;
		//// update rows when another window changes it if linkage is ON
		//this . BankGrid . SelectedIndex = e . Row;
		//Triggered = false;
	}

	/// <summary>
	/// NOT IN USE currently !!!
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public void SendDataChanged ( string dbName )
	{
		//// Databases have DEFINITELY been updated successfully after a change
		//// We Now Broadcast this to ALL OTHER OPEN VIEWERS here and now

		////dca . SenderName = o . ToString ( );
		////dca . DbName = dbName;

		//if ( dbName == "BANKACCOUNT" )
		//{
		//	EventControl . TriggerMultiViewerDataUpdated ( MultiBankcollection,
		//		new LoadedEventArgs
		//		{
		//			CallerDb = "BANKACCOUNT",
		//			DataSource = MultiBankcollection,
		//			RowCount = this . BankGrid . SelectedIndex
		//		} );
		//}
		//else if ( dbName == "CUSTOMER" )
		//{
		//	EventControl . TriggerMultiViewerDataUpdated ( MultiCustcollection,
		//		new LoadedEventArgs
		//		{
		//			CallerDb = "CUSTOMER",
		//			DataSource = MultiCustcollection,
		//			RowCount = this . CustomerGrid . SelectedIndex

		//		} );
		//}
		//else if ( dbName == "DETAILS" )
		//{
		//	EventControl . TriggerMultiViewerDataUpdated ( MultiDetcollection,
		//		new LoadedEventArgs
		//		{
		//			CallerDb = "DETAILS",
		//			DataSource = MultiDetcollection,
		//			RowCount = this . DetailsGrid . SelectedIndex
		//		} );
		//}

		//Mouse . OverrideCursor = Cursors . Arrow;
	}

	/// <summary>
	/// NOT IN USE currently !!!
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public void ExternalDataUpdate ( int DbEditChangeType, int row, string currentDb )
	{
		//Debug . WriteLine ( $"MultiViewer : Data changed event notification received successfully." );
		//int x = 0;
	}
	private async void ReloadData ( DataGrid DGrid )
	{
		//try
		//{
		//	Mouse . OverrideCursor = Cursors . Wait;
		//	// Make sure we are back on UI thread

		//	int current = 0;
		//	current = DGrid . SelectedIndex == -1 ? 0 : DGrid . SelectedIndex;
		//	this . BankGrid . ItemsSource = null;
		//	this . CustomerGrid . ItemsSource = null;
		//	this . DetailsGrid . ItemsSource = null;

		//	await BankCollection . LoadBank ( MultiBankcollection, 1, true );
		//	this . BankGrid . ItemsSource = MultiBankcollection;

		//	await CustCollection . LoadCust ( MultiCustcollection, 3, true );
		//	this . CustomerGrid . ItemsSource = MultiCustcollection;

		//	await DetCollection . LoadDet ( MultiDetcollection, 3, true );
		//	this . DetailsGrid . ItemsSource = MultiDetcollection;

		//	DGrid . SelectedIndex = current;
		//	Debug . WriteLine ( $"End of ReloadGrid() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
		//}
		//catch ( Exception ex )
		//{
		//	Debug . WriteLine ( $"ERROR: ReloadGrid() {ex . Message}, : {ex . Data}" );
		//}
	}

	/// <summary>
	/// Just resets  the SelectedITEM and scroills it into view
	/// </summary>
	public void ResetIndexes ( )
	{
		//inprogress = true;
		//BankGrid . SelectedIndex = bindex;
		//if ( CurrentDb == "BANKACCOUNT" )
		//{
		//	this . BankGrid . SelectedItem = bindex;
		//	this . BankGrid . ScrollIntoView ( bindex );
		//}
		//else if ( CurrentDb == "CUSTOMER" )
		//{
		//	this . CustomerGrid . SelectedItem = cindex;
		//	this . CustomerGrid . ScrollIntoView ( cindex );
		//}
		//else if ( CurrentDb == "DETAILS" )
		//{
		//	this . DetailsGrid . SelectedItem = dindex;
		//	this . DetailsGrid . ScrollIntoView ( dindex );
		//}
		//inprogress = false;
	}

	/// <summary>
	/// EVENT HANDLER
	/// for data changes made to BankCollection
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void UpdatedDataLoaded ( object sender, LoadedEventArgs e )
	{
		//// Update ALL datagrids
		//if ( inprogress ) return;

		//Mouse . OverrideCursor = Cursors . Wait;
		//RefreshAllGrids ( CurrentDb, e . RowCount );
		//Mouse . OverrideCursor = Cursors . Arrow;
		//inprogress = false;
	}


//		private async void EventControl_MultiViewerIndexChanged ( object sender, IndexChangedArgs e )
//		{
	//object RowTofind = null;
	//object gr = null;
	//int rec = 0;
	//inprogress = true;
	//if ( ( Flags . IsFiltered || Flags . IsMultiMode ) && GridsLinked )
	//{
	//	rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . BankGrid, "BANKACCOUNT" );
	//	this . BankGrid . SelectedIndex = rec;// != -1 ? rec : 0;
	//	bindex = rec;
	//	Utils . ScrollRecordIntoView ( this . BankGrid, rec );
	//	rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . CustomerGrid, "CUSTOMER" );
	//	if ( rec == -1 )
	//	{
	//		this . CustomerGrid . SelectedIndex = -1;
	//		this . CustomerGrid . SelectedItem = -1;
	//	}
	//	else
	//	{
	//		this . CustomerGrid . SelectedIndex = rec;// != -1 ? rec : 0;
	//		cindex = rec;
	//		BankData . DataContext = this . CustomerGrid . SelectedItem;
	//		Utils . ScrollRecordIntoView ( this . CustomerGrid, rec );
	//		rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . DetailsGrid, "DETAILS" );
	//		this . DetailsGrid . SelectedIndex = rec;// != -1 ? rec : 0;
	//		dindex = rec;
	//		Utils . ScrollRecordIntoView ( this . DetailsGrid, rec );
	//	}
	//}

	//// Now check to see if we need  to update the other grids in our own MultiViewer window here
	//if ( GridsLinked )
	//{                                       // Update all three grids
	//	if ( e . Sender == "BANKACCOUNT" )
	//	{
	//		rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . CustomerGrid, "CUSTOMER" );
	//		this . CustomerGrid . SelectedIndex = rec != -1 ? rec : 0;
	//		cindex = rec;
	//		BankData . DataContext = this . CustomerGrid . SelectedItem;
	//		Utils . ScrollRecordIntoView ( this . CustomerGrid, rec );
	//		rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . DetailsGrid, "DETAILS" );
	//		this . DetailsGrid . SelectedIndex = rec != -1 ? rec : 0;
	//		dindex = rec;
	//		Utils . ScrollRecordIntoView ( this . DetailsGrid, rec );
	//	}
	//	else if ( e . Sender == "CUSTOMER" )
	//	{
	//		rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . BankGrid, "BANKACCOUNT" );
	//		this . BankGrid . SelectedIndex = rec != -1 ? rec : 0;
	//		bindex = rec;
	//		Utils . ScrollRecordIntoView ( this . BankGrid, rec );
	//		rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . DetailsGrid, "DETAILS" );
	//		this . DetailsGrid . SelectedIndex = rec != -1 ? rec : 0;
	//		dindex = rec;
	//		Utils . ScrollRecordIntoView ( this . DetailsGrid, rec );
	//	}
	//	else if ( e . Sender == "DETAILS" )
	//	{
	//		rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . BankGrid, "BANKACCOUNT" );
	//		this . BankGrid . SelectedIndex = rec != -1 ? rec : 0;
	//		bindex = rec;
	//		Utils . ScrollRecordIntoView ( this . BankGrid, rec );
	//		rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . CustomerGrid, "CUSTOMER" );
	//		this . CustomerGrid . SelectedIndex = rec != -1 ? rec : 0;
	//		cindex = rec;
	//		BankData . DataContext = this . CustomerGrid . SelectedItem;
	//		Utils . ScrollRecordIntoView ( this . CustomerGrid, rec );
	//	}
	//}

	//return;
//		}
	#endregion UNUSED CODE


*
* */
	}
}
