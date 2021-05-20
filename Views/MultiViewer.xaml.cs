using System;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Input;
using System . Windows . Media;
using System . Linq;
using WPFPages . ViewModels;
using static System . Windows . Forms . VisualStyles . VisualStyleElement . ProgressBar;
using static System . Windows . Forms . VisualStyles . VisualStyleElement . Status;
using System . Runtime . Remoting . Channels;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for MultiViewer.xaml
	/// </summary>
	public partial class MultiViewer : Window
	{
		public static BankCollection MultiBankcollection = BankCollection . MultiBankcollection;
		public static CustCollection MultiCustcollection = CustCollection . MultiCustcollection;
		public static DetCollection MultiDetcollection = DetCollection . MultiDetcollection;

		dynamic bindex = 0;
		dynamic cindex = 0;
		dynamic dindex = 0;
		dynamic CurrentSelection = 0;

		#region DELEGATES / EVENTS Declarations

//		public static event EventHandler<LoadedEventArgs> DetDataLoaded;

		#endregion DELEGATES / EVENTS Declarations

		#region DECLARATIONS

		public string CurrentDb = "";
		static bool inprogress = false;

		#endregion DECLARATIONS


		#region STARTUP/CLOSE

		public MultiViewer ( )
		{
			InitializeComponent ( );
		}
		private async void Window_Loaded ( object sender, RoutedEventArgs e )
		{
			LoadAllData ( );
			this . MouseDown += delegate { DoDragMove ( ); };
			this . BankGrid . MouseDown += delegate { DoDragMove ( ); };
			this . CustomerGrid . MouseDown += delegate { DoDragMove ( ); };
			this . DetailsGrid . MouseDown += delegate { DoDragMove ( ); };

			Flags . SqlBankGrid = this . BankGrid;
			Flags . SqlCustGrid = this . CustomerGrid;
			Flags . SqlDetGrid = this . DetailsGrid;
			Flags . MultiViewer = this;

			// subscribe to data change events for all 3 types of data sets
			// Main update notification handler
			EventControl . ViewerDataUpdated += EventControl_DataUpdated;
			EventControl . EditDbDataUpdated += EventControl_DataUpdated;


			this . BankGrid . ItemsSource = MultiBankcollection;
			this . CustomerGrid . ItemsSource = MultiCustcollection;
			this . DetailsGrid . ItemsSource = MultiDetcollection;
			//Select first record in all grids
			this . BankGrid . SelectedIndex = 0;
			this . CustomerGrid . SelectedIndex = 0;
			this . DetailsGrid . SelectedIndex = 0;
		}
		private void Window_Closing ( object sender, System . ComponentModel . CancelEventArgs e )
		{
			// Unsubscribe from Bank data change event notificatoin
			// Main update notification handler
			//			EventControl . DataUpdated -= EventControl_DataUpdated;
			EventControl . ViewerDataUpdated -= EventControl_DataUpdated;
			EventControl . EditDbDataUpdated -= EventControl_DataUpdated;

			// Clear databases
			MultiBankcollection . Clear ( );
			MultiCustcollection . Clear ( );
			MultiDetcollection . Clear ( );

			Flags . SqlBankGrid = null;
			Flags . SqlCustGrid = null;
			Flags . SqlDetGrid = null;
			Flags . MultiViewer = null;
		}

		private void EventControl_DataUpdated ( object sender, LoadedEventArgs e )
		{
			// Update ALL datagrids - IF we didnt   truiigger the change
			if ( sender == MultiBankcollection || sender == MultiCustcollection || sender == MultiDetcollection )
			{
				Mouse . OverrideCursor = Cursors . Arrow;
				return;
			}
			RefreshAllGrids ( CurrentDb, e . RowCount );
			Mouse . OverrideCursor = Cursors . Arrow;
			inprogress = false;
		}

		private async Task LoadAllData ( )
		{
			// load the data
			if ( MultiBankcollection == null || MultiBankcollection . Count == 0 )
				BankCollection . LoadBank ( MultiBankcollection, 3, false);
			BankGrid . ItemsSource = MultiBankcollection;
			if ( MultiCustcollection == null || MultiCustcollection . Count == 0 )
				CustCollection . LoadCust ( MultiCustcollection );
			if ( MultiDetcollection == null || MultiDetcollection . Count == 0 )
				DetCollection . LoadDet ( MultiDetcollection );

			Flags . MultiViewer = this;
			this . BankGrid . ItemsSource = MultiBankcollection;
			this . CustomerGrid . ItemsSource = MultiCustcollection;
			this . DetailsGrid . ItemsSource = MultiDetcollection;
		}

	#endregion STARTUP/CLOSE

		#region EVENT HANDLERS

		/// <summary>
		/// EVENT HANDLER
		/// for data changes made to BankCollection
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UpdatedDataLoaded ( object sender, LoadedEventArgs e )
		{
			// Update ALL datagrids
			if ( inprogress ) return;

			Mouse . OverrideCursor = Cursors . Wait;
			RefreshAllGrids ( CurrentDb, e . RowCount );
			Mouse . OverrideCursor = Cursors . Arrow;
			inprogress = false;
		}
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
			SQLHandlers sqlh = new SQLHandlers ( );
			sqlh . UpdateAllDb ( CurrentDb, e );
			bindex = this . BankGrid . SelectedIndex;
			cindex = this . CustomerGrid . SelectedIndex;
			dindex = this . DetailsGrid . SelectedIndex;
			//Gotta reload our data because the update clears it down totally to null
			// Refresh our grids
			RefreshAllGrids ( CurrentDb, e . Row . GetIndex ( ) );
			inprogress = false;
		}
		public void RefreshAllGrids ( string CurrentDb, int row )
		{
			ReLoadAllDataBases ( CurrentDb, row );
		}

		private async void ReLoadAllDataBases ( string CurrentD, int row )
		{
			int bbindex = 0;
			int ccindex = 0;
			int ddindex = 0;
			int rec = 0;
			if ( row == -1 ) row = 0;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				return;
				bbindex = row;
				this . BankGrid . ItemsSource = null;
			}
			else if ( CurrentDb == "BANKACCOUNT" )
			{
				return;
				ccindex = row;
			}
			else if ( CurrentDb == "BANKACCOUNT" )
			{
				return;
				ccindex = row;
				ddindex = row;
			}
			this . BankGrid . ItemsSource = null;
			this . CustomerGrid . ItemsSource = null;
			this . DetailsGrid . ItemsSource = null;
			this . BankGrid . Items . Clear ( );
			this . CustomerGrid . Items . Clear ( );
			this . DetailsGrid . Items . Clear ( );

			BankCollection . LoadBank ( MultiBankcollection, 3 );
			CustCollection . LoadCust ( MultiCustcollection );
			DetCollection . LoadDet ( MultiDetcollection );
			//int b = bindex;
			//int c = cindex;
			//int d = dindex;
			this . BankGrid . ItemsSource = MultiBankcollection;
			this . CustomerGrid . ItemsSource = MultiCustcollection;
			this . DetailsGrid . ItemsSource = MultiDetcollection;
			//bindex = b;
			//cindex = c;
			//dindex = d;
			//this . BankGrid . Refresh ( );
			//this . CustomerGrid . Refresh ( );
			//this . DetailsGrid . Refresh ( );

			this . BankGrid . SelectedIndex = bbindex;
			this . CustomerGrid . SelectedIndex = ccindex;
			this . DetailsGrid . SelectedIndex = ddindex;
			this . BankGrid . SelectedItem = bbindex;
			this . CustomerGrid . SelectedItem = ccindex;
			this . DetailsGrid . SelectedItem = ddindex;
			//this . BankGrid . ScrollIntoView ( bbindex );
			//this . CustomerGrid . ScrollIntoView ( ccindex );
			//this . DetailsGrid . ScrollIntoView ( ddindex );

			if ( CurrentDb == "BANKACCOUNT" )
			{
				// Get Custno from ACTIVE gridso we can find it in other grids
				BankAccountViewModel bgr = new BankAccountViewModel ( );
				bgr = this . BankGrid . SelectedItem as BankAccountViewModel;
				if ( bgr == null ) return;
				rec = FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . BankGrid, "BANKACCOUNT" );
				this . BankGrid . SelectedIndex = rec;
				bindex = rec;
				Utils . ScrollRecordIntoView ( this . BankGrid, rec );
				rec = FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . CustomerGrid, "CUSTOMER" );
				this . CustomerGrid . SelectedIndex = rec;
				cindex = rec;
				BankData . DataContext = this . CustomerGrid . SelectedItem;
				Utils . ScrollRecordIntoView ( this . CustomerGrid, rec );
				rec = FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . DetailsGrid, "DETAILS" );
				this . DetailsGrid . SelectedIndex = rec;
				dindex = rec;
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				// Get Custno from ACTIVE gridso we can find it in other grids
				CustomerViewModel bgr = new CustomerViewModel ( );
				bgr = this . CustomerGrid. SelectedItem as CustomerViewModel;
				if ( bgr == null ) return;
				rec = FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . BankGrid, "BANKACCOUNT" );
				this . BankGrid . SelectedIndex = rec;
				bindex = rec;
				Utils . ScrollRecordIntoView ( this . BankGrid, rec );
				rec = FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . CustomerGrid, "CUSTOMER" );
				this . CustomerGrid . SelectedIndex = rec;
				cindex = rec;
				BankData . DataContext = this . CustomerGrid . SelectedItem;
				Utils . ScrollRecordIntoView ( this . CustomerGrid, rec );
				rec = FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . DetailsGrid, "DETAILS" );
				this . DetailsGrid . SelectedIndex = rec;
				dindex = rec;
			}
			else if ( CurrentDb == "DETAILS" )
			{
				// Get Custno from ACTIVE gridso we can find it in other grids
				DetailsViewModel bgr = this . DetailsGrid . SelectedItem as DetailsViewModel;
				if ( bgr == null ) return;
				rec = FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . BankGrid, "BANKACCOUNT" );
				this . BankGrid . SelectedIndex = rec;
				bindex = rec;
				Utils . ScrollRecordIntoView ( this . BankGrid, rec );
				rec = FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . CustomerGrid, "CUSTOMER" );
				this . CustomerGrid . SelectedIndex = rec;
				cindex = rec;
				BankData . DataContext = this . CustomerGrid . SelectedItem;
				Utils . ScrollRecordIntoView ( this . CustomerGrid, rec );
				rec = FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . DetailsGrid, "DETAILS" );
				this . DetailsGrid . SelectedIndex = rec;
				dindex = rec;
			}
			//this . BankGrid . SelectedIndex = bbindex;
			//this . CustomerGrid . SelectedIndex = ccindex;
			//this . DetailsGrid . SelectedIndex = ddindex;
			//this . BankGrid . SelectedItem = bbindex;
			//this . CustomerGrid . SelectedItem = ccindex;
			//this . DetailsGrid . SelectedItem = ddindex;
		}

		#endregion DATA UPDATING

		private void ViewerGrid_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
		{
			// Save current positions so we can reposition later
			inprogress = true;
			bindex = this . BankGrid . SelectedIndex;
			cindex = this . CustomerGrid . SelectedIndex;
			dindex = this . DetailsGrid . SelectedIndex;
			CurrentSelection = this . BankGrid . SelectedIndex;
			this . BankGrid . SelectedItem = this . BankGrid . SelectedIndex;
			UpdateOnDataChange ( CurrentDb, e );
			ResetIndexes ( );

			if ( CurrentDb == "BANKACCOUNT" )
			{
				EventControl . TriggerMultiViewerDataUpdated ( MultiBankcollection,
					new LoadedEventArgs
					{
						CallerDb = "BANKACCOUNT",
						DataSource = MultiBankcollection,
						RowCount = this . BankGrid . SelectedIndex
					} );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				EventControl . TriggerMultiViewerDataUpdated ( MultiCustcollection,
					new LoadedEventArgs
					{
						CallerDb = "CUSTOMER",
						DataSource = MultiCustcollection,
						RowCount = this . CustomerGrid . SelectedIndex
					} );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				EventControl . TriggerMultiViewerDataUpdated ( MultiDetcollection,
					new LoadedEventArgs
					{
						CallerDb = "DETAILS",
						DataSource = MultiDetcollection,
						RowCount = this . DetailsGrid . SelectedIndex
					} );
			}
			// Notify any other interested parties of data update
			//			SendDataChanged ( CurrentDb );
			inprogress = false;
		}

		public void ResetIndexes ( )
		{
			inprogress = true;
			BankGrid . SelectedIndex = bindex;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				this . BankGrid . SelectedItem = bindex;
				this . BankGrid . ScrollIntoView ( bindex );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				this . CustomerGrid . SelectedItem = cindex;
				this . CustomerGrid . ScrollIntoView ( cindex );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				this . DetailsGrid . SelectedItem = dindex;
				this . DetailsGrid . ScrollIntoView ( dindex );
			}
			inprogress = false;
		}
		private async void ReloadData ( DataGrid DGrid )
		{
			try
			{
				Mouse . OverrideCursor = Cursors . Wait;
				// Make sure we are back on UI thread

				int current = 0;
				current = DGrid . SelectedIndex == -1 ? 0 : DGrid . SelectedIndex;
				this . BankGrid . ItemsSource = null;
				this . CustomerGrid . ItemsSource = null;
				this . DetailsGrid . ItemsSource = null;

				BankCollection . LoadBank ( MultiBankcollection, 1, false );
				this . BankGrid . ItemsSource = MultiBankcollection;

				CustCollection . LoadCust ( MultiCustcollection );
				this . CustomerGrid . ItemsSource = MultiCustcollection;

				DetCollection . LoadDet ( MultiDetcollection );
				this . DetailsGrid . ItemsSource = MultiDetcollection;

				DGrid . SelectedIndex = current;
				Console . WriteLine ( $"End of ReloadGrid() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"ERROR: ReloadGrid() {ex . Message}, : {ex . Data}" );
			}
		}

		public void ExternalDataUpdate ( int DbEditChangeType, int row, string currentDb )
		{
			Console . WriteLine ( $"MultiViewer : Data changed event notification received successfully." );
			int x = 0;
		}

		public void RefreshData ( int row )
		{
			bindex = this . BankGrid . SelectedIndex;
			cindex = this . CustomerGrid . SelectedIndex;
			dindex = this . DetailsGrid . SelectedIndex;
			this . BankGrid . ItemsSource = null;
			this . BankGrid . ItemsSource = MultiBankcollection;
			this . CustomerGrid . ItemsSource = null;
			this . CustomerGrid . ItemsSource = MultiCustcollection;
			this . DetailsGrid . ItemsSource = null;
			this . DetailsGrid . ItemsSource = MultiDetcollection;

			RefreshAllGrids ( CurrentDb, row );
			// Refresh all grids
			Mouse . OverrideCursor = Cursors . Wait;
			this . BankGrid . SelectedIndex = bindex;
			this . CustomerGrid . SelectedIndex = cindex;
			this . DetailsGrid . SelectedIndex = dindex;

			this . BankGrid . ScrollIntoView ( bindex );
			this . CustomerGrid . ScrollIntoView ( cindex );
			this . DetailsGrid . ScrollIntoView ( dindex );
			inprogress = false;
			Mouse . OverrideCursor = Cursors . Arrow;
		}

		private void Close_Click ( object sender, RoutedEventArgs e )
		{
			Close ( );
		}


		private void Window_PreviewKeyDown ( object sender, System . Windows . Input . KeyEventArgs e )
		{
			if ( e . Key == Key . RightAlt )
			{
				Flags . ListGridviewControlFlags ( );
			}
			else if ( e . Key == Key . Escape )
			{
				Close ( );
			}
			else if ( e . Key == Key . Home )
			{
				this . BankGrid . SelectedIndex = 0;
				this . CustomerGrid . SelectedIndex = 0;
				this . DetailsGrid . SelectedIndex = 0;
				ExtensionMethods . Refresh ( this . BankGrid );
				ExtensionMethods . Refresh ( this . CustomerGrid );
				ExtensionMethods . Refresh ( this . DetailsGrid );
			}
			else if ( e . Key == Key . End )
			{
				this . BankGrid . SelectedIndex = this . BankGrid . Items . Count - 1;
				this . CustomerGrid . SelectedIndex = this . CustomerGrid . Items . Count - 1;
				this . DetailsGrid . SelectedIndex = this . DetailsGrid . Items . Count - 1;
				this . BankGrid . SelectedItem = this . BankGrid . Items . Count - 1;
				this . CustomerGrid . SelectedItem = this . CustomerGrid . Items . Count - 1;
				this . DetailsGrid . SelectedItem = this . DetailsGrid . Items . Count - 1;
				this . BankGrid . ScrollIntoView ( this . BankGrid . Items . Count - 1 );
				this . CustomerGrid . ScrollIntoView ( this . CustomerGrid . Items . Count - 1 );
				this . DetailsGrid . ScrollIntoView ( this . DetailsGrid . Items . Count - 1 );
				ExtensionMethods . Refresh ( this . BankGrid );
				ExtensionMethods . Refresh ( this . CustomerGrid );
				ExtensionMethods . Refresh ( this . DetailsGrid );
			}
		}

		#region DATAGRID  SELECTION CHANGE  HANDLING  (SelectedIndex matching across all 3 grids)
		/// <summary>
		/// /// *************************************************************************
		/// THESE ALL WORK CORRECTLY, AND THE SELECTED ROWS ALL MATCH PERFECTLY - 15/5/2021
		/// /// *************************************************************************
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BankGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			int rec = 0;
			ScrollViewer scroll;

			if ( inprogress )
				return;
			//if ( sender == this . BankGrid )
			//	return;
			inprogress = true;

			DataGrid dg = sender as DataGrid;
			int currsel = dg . SelectedIndex;
			bindex = currsel;
			if ( currsel <= 0 ) currsel = bindex;
			this . BankGrid . SelectedIndex = currsel;
			Utils . ScrollRecordIntoView ( this . BankGrid, currsel );

			// Get Custno from ACTIVE gridso we can find it in other grids
			BankAccountViewModel bgr = this . BankGrid . SelectedItem as BankAccountViewModel;
			if ( bgr == null ) return;
			rec = FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . BankGrid, "BANKACCOUNT" );
			this . BankGrid . SelectedIndex = rec;
			bindex = rec;
			Utils . ScrollRecordIntoView ( this . BankGrid, rec );

			rec = FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . CustomerGrid, "CUSTOMER" );
			this . CustomerGrid . SelectedIndex = rec;
			cindex = rec;
			BankData . DataContext = this . CustomerGrid . SelectedItem;
			Utils . ScrollRecordIntoView ( this . CustomerGrid, rec );

			rec = FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . DetailsGrid, "DETAILS" );
			this . DetailsGrid . SelectedIndex = rec;
			dindex = rec;
			Utils . ScrollRecordIntoView ( this . DetailsGrid, rec );
			inprogress = false;
			return;

		}
		private void CustGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			int rec = 0;
			ScrollViewer scroll;

			if ( inprogress )
				return;
			inprogress = true;

			DataGrid dg = sender as DataGrid;
			int currsel = dg . SelectedIndex;
			cindex = currsel;
			if ( currsel <= 0 ) currsel = cindex;
			this . CustomerGrid . SelectedIndex = currsel;
			Utils . ScrollRecordIntoView ( this . CustomerGrid, cindex );

			// Get Custno from ACTIVE grid so we can find it in other grids
			CustomerViewModel cgr = CustomerGrid . SelectedItem as CustomerViewModel;
			if ( cgr == null ) return;
			rec = FindMatchingRecord ( cgr . CustNo, cgr . BankNo, this . CustomerGrid, "CUSTOMER" );
			this . CustomerGrid . SelectedIndex = rec;
			cindex = rec;
			Utils . ScrollRecordIntoView ( this . CustomerGrid, cindex );

			rec = FindMatchingRecord ( cgr . CustNo, cgr . BankNo, this . BankGrid, "BANKACCOUNT" );
			this . BankGrid . SelectedIndex = rec;
			bindex = rec;
			Utils . ScrollRecordIntoView ( this . BankGrid, bindex );
			// Now use SAME CUSTNO to findmatch in Bank  DbGrid

			rec = FindMatchingRecord ( cgr . CustNo, cgr . BankNo, this . DetailsGrid, "DETAILS" );
			this . DetailsGrid . SelectedIndex = rec;
			dindex = rec;
			Utils . ScrollRecordIntoView ( this . DetailsGrid, dindex );
			inprogress = false;

			BankData . DataContext = this . CustomerGrid . SelectedItem;
		}
		private void DetGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			int rec = 0;
			ScrollViewer scroll;

			if ( inprogress )
				return;
			inprogress = true;

			DataGrid dg = sender as DataGrid;
			int currsel = dg . SelectedIndex;
			dindex = currsel;
			if ( currsel <= 0 ) currsel = dindex;
			this . DetailsGrid . SelectedIndex = currsel;
			Utils . ScrollRecordIntoView ( DetailsGrid, dindex );

			// Get Custno from ACTIVE gridso we can find it in other grids
			DetailsViewModel dgr = DetailsGrid . SelectedItem as DetailsViewModel;
			if ( dgr == null ) return;
			rec = FindMatchingRecord ( dgr . CustNo, dgr . BankNo, this . DetailsGrid, "DETAILS" );
			this . DetailsGrid . SelectedIndex = rec;
			dindex = rec;
			Utils . ScrollRecordIntoView ( DetailsGrid, dindex );

			rec = FindMatchingRecord ( dgr . CustNo, dgr . BankNo, this . BankGrid, "BANKACCOUNT" );
			this . BankGrid . SelectedIndex = rec;
			bindex = rec;
			Utils . ScrollRecordIntoView ( this . BankGrid, bindex );

			// Now use SAME CUSTNO to findmatch in Customer DbGrid
			rec = FindMatchingRecord ( dgr . CustNo, dgr . BankNo, this . CustomerGrid, "CUSTOMER" );
			this . CustomerGrid . SelectedIndex = rec;
			cindex = rec;
			Utils . ScrollRecordIntoView ( this . CustomerGrid, cindex );

			inprogress = false;
			BankData . DataContext = this . CustomerGrid . SelectedItem;
		}
		public int FindMatchingRecord ( string Custno, string Bankno, DataGrid Grid, string CurrentDb )
		{
			int index = 0;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				foreach ( var item in Grid . Items )
				{
					BankAccountViewModel cvm = item as BankAccountViewModel;
					if ( cvm == null ) break;
					if ( cvm . CustNo == Custno && cvm . BankNo == Bankno )
					{
						break;
					}
					index++;
				}
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				foreach ( var item in Grid . Items )
				{
					CustomerViewModel cvm = item as CustomerViewModel;
					if ( cvm == null ) break;
					if ( cvm . CustNo == Custno && cvm . BankNo == Bankno )
					{
						break;
					}
					index++;
				}
			}
			else if ( CurrentDb == "DETAILS" )
			{
				foreach ( var item in Grid . Items )
				{
					DetailsViewModel cvm = item as DetailsViewModel;
					if ( cvm == null ) break;
					if ( cvm . CustNo == Custno && cvm . BankNo == Bankno )
					{
						break;
					}
					index++;
				}
			}
			return index;
		}


		#endregion DATAGRID  SELECTION CHANGE  HANDLING

		#region focus events

		private void CustomerGrid_GotFocus ( object sender, RoutedEventArgs e )
		{ CurrentDb = "CUSTOMER"; }
		private void BankGrid_GotFocus ( object sender, RoutedEventArgs e )
		{ CurrentDb = "BANKACCOUNT"; }
		private void DetailsGrid_GotFocus ( object sender, RoutedEventArgs e )
		{ CurrentDb = "DETAILS"; }

		#endregion focus events

		#region SCROLLBARS

		// scroll bar movement is automatically   stored by these three methods
		// So we can use them to reset position CORRECTLY after refreshes
		private void BankGrid_ScrollChanged ( object sender, ScrollChangedEventArgs e )
		{
			int rec = 0;
			DataGrid dg = null;
			dg = sender as DataGrid;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			scroll . CanContentScroll = true;
			SetScrollVariables ( sender );
			this . BankGrid . ScrollIntoView ( this . BankGrid . SelectedIndex );
			this . CustomerGrid . ScrollIntoView ( this . BankGrid . SelectedIndex );
			this . DetailsGrid . ScrollIntoView ( this . BankGrid . SelectedIndex );

		}
		private void CustomerGrid_ScrollChanged ( object sender, ScrollChangedEventArgs e )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			scroll . CanContentScroll = true;
			SetScrollVariables ( sender );
			this . BankGrid . ScrollIntoView ( this . CustomerGrid . SelectedIndex );
			this . CustomerGrid . ScrollIntoView ( this . CustomerGrid . SelectedIndex );
			this . DetailsGrid . ScrollIntoView ( this . CustomerGrid . SelectedIndex );
		}

		private void DetailsGrid_ScrollChanged ( object sender, ScrollChangedEventArgs e )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( ( DependencyObject ) dg );
			scroll . CanContentScroll = true;
			SetScrollVariables ( sender );
			this . CustomerGrid . ScrollIntoView ( this . DetailsGrid . SelectedIndex );
			this . BankGrid . ScrollIntoView ( this . DetailsGrid . SelectedIndex );
			this . DetailsGrid . ScrollIntoView ( this . DetailsGrid . SelectedIndex );
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
				//				Console . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . TopVisibleBankGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . TopVisibleBankGridRow = ( double ) rounded;
			}
			else if ( dg == this . CustomerGrid )
			{
				//				Console . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . TopVisibleCustGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . TopVisibleCustGridRow = ( double ) rounded;
			}
			else if ( dg == this . DetailsGrid )
			{
				//				Console . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . TopVisibleDetGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
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
			if ( dg == this . BankGrid )
			{
				//				Console . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . BottomVisibleBankGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . BottomVisibleBankGridRow = ( double ) rounded;
			}
			else if ( dg == this . CustomerGrid )
			{
				//				Console . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . BottomVisibleCustGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . BottomVisibleCustGridRow = ( double ) rounded;
			}
			else if ( dg == this . DetailsGrid )
			{
				//				Console . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . BottomVisibleDetGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
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


		#endregion Scroll bar utilities

		private void DoDragMove ( )
		{
			//Handle the button NOT being the left mouse button
			// which will crash the DragMove Fn.....     cos it has to be the primary button !!!
			try
			{ DragMove ( ); }
			catch ( Exception ex )
			{ Console . WriteLine ( $"General Exception : {ex . Message}, {ex . Data}" ); return; }
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
			string s = MultiAccountText . Text as string;
			if ( s . Contains ( "<<-" ) || s . Contains ( "Show All" ) )
			{
				Flags . IsMultiMode = false;
				MultiAccountText . Text = "Multi Accounts";
				//// Set the gradient background
				//SetButtonGradientBackground ( Multiaccounts );
			}
			else
			{
				Flags . IsMultiMode = true;
				//SetButtonGradientBackground ( Multiaccounts );
				MultiAccountText . Text = "Show All A/C's";
			}
			this . BankGrid . ItemsSource = null;
			this . BankGrid . Items . Clear ( );
			BankCollection . LoadBank ( MultiBankcollection, 1, false );
			this . BankGrid . ItemsSource = CollectionViewSource . GetDefaultView ( MultiBankcollection );
			ExtensionMethods . Refresh ( this . BankGrid );
			this . CustomerGrid . ItemsSource = null;
			this . CustomerGrid . Items . Clear ( );
			CustCollection . LoadCust ( MultiCustcollection );
			this . CustomerGrid . ItemsSource = CollectionViewSource . GetDefaultView ( MultiCustcollection );
			ExtensionMethods . Refresh ( this . CustomerGrid );
			this . DetailsGrid . ItemsSource = null;
			this . DetailsGrid . Items . Clear ( );
			DetCollection . LoadDet ( MultiDetcollection );
			this . DetailsGrid . ItemsSource = CollectionViewSource . GetDefaultView ( MultiDetcollection );
			ExtensionMethods . Refresh ( this . DetailsGrid );
		}

		private void Filter_Click ( object sender, RoutedEventArgs e )
		{
			Filtering f = new Filtering ( );
			if ( CurrentDb == "" )
			{
				MessageBox . Show ( "Please select an entry in one of the data grids before trying to filter the data listed." );
				return;
			}
			Flags . FilterCommand = f . DoFilters ( sender, CurrentDb, 1 );
			ReLoadAllDataBases ( CurrentDb, -1 );
		}

		private void BankGrid_Selected ( object sender, RoutedEventArgs e )
		{
			// hit when grid selection is changed by anything
			int x = 0;
			//Console . WriteLine("...");
		}


		private void Refresh_Click ( object sender, RoutedEventArgs e )
		{
			RefreshData ( -1 );
		}

		private void BankDb_Click ( object sender, RoutedEventArgs e )
		{
			BankDbView cdbv = new BankDbView ( );
			cdbv . Show ( );
		}
		private void CustDb_Click ( object sender, RoutedEventArgs e )
		{
			CustDbView cdbv = new CustDbView ( );
			cdbv . Show ( );
		}
		private void DetDb_Click ( object sender, RoutedEventArgs e )
		{
			DetailsDbView cdbv = new DetailsDbView ( );
			cdbv . Show ( );
		}

		/// <summary>
		///  Function that broadcasts a notification to whoever to
		///  notify that one of the Obs collections has been changed by something
		/// </summary>
		/// <param name="o"> The sending object</param>
		/// <param name="args"> Sender name and Db Type</param>
		//		private static bool hasupdated = false;

		public void SendDataChanged ( string dbName )
		{
			// Databases have DEFINITELY been updated successfully after a change
			// We Now Broadcast this to ALL OTHER OPEN VIEWERS here and now

			//dca . SenderName = o . ToString ( );
			//dca . DbName = dbName;

			if ( dbName == "BANKACCOUNT" )
			{
				EventControl . TriggerMultiViewerDataUpdated ( MultiBankcollection,
					new LoadedEventArgs
					{
						CallerDb = "BANKACCOUNT",
						DataSource = MultiBankcollection,
						RowCount = this . BankGrid . SelectedIndex
					} );
			}
			else if ( dbName == "CUSTOMER" )
			{
				EventControl . TriggerMultiViewerDataUpdated ( MultiCustcollection,
					new LoadedEventArgs
					{
						CallerDb = "CUSTOMER",
						DataSource = MultiCustcollection,
						RowCount = this . CustomerGrid . SelectedIndex

					} );
			}
			else if ( dbName == "DETAILS" )
			{
				EventControl . TriggerMultiViewerDataUpdated ( MultiDetcollection,
					new LoadedEventArgs
					{
						CallerDb = "DETAILS",
						DataSource = MultiDetcollection,
						RowCount = this . DetailsGrid . SelectedIndex
					} );
			}

			//if ( dbName == "BANKACCOUNT" )
			//{
			//	ReloadCustomerOnUpdateNotification ( o , Grid , dca );
			//	ReloadDetailsOnUpdateNotification ( o , Grid , dca );
			//	//				hasupdated = false;
			//}
			//else if ( dbName == "CUSTOMER" )
			//{
			//	ReloadBankOnUpdateNotification ( o , Grid , dca );
			//	ReloadDetailsOnUpdateNotification ( o , Grid , dca );
			//}
			//else if ( dbName == "DETAILS" )
			//{
			//	ReloadCustomerOnUpdateNotification ( o , Grid , dca );
			//	ReloadBankOnUpdateNotification ( o , Grid , dca );
			//}
			Mouse . OverrideCursor = Cursors . Arrow;
		}
	}
}
