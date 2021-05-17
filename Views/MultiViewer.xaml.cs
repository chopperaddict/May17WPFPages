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

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for MultiViewer.xaml
	/// </summary>
	public partial class MultiViewer : Window
	{
		public BankAccountViewModel bvm = MainWindow . bvm;
		public CustomerViewModel cvm = MainWindow . cvm;
		public DetailsViewModel dvm = MainWindow . dvm;

		public static BankCollection Bankcollection = Flags.BankCollection;
		public static CustCollection Custcollection = Custcollection;
		public static DetCollection Detcollection = Detcollection;


		public  static BankCollection MultiBankcollection=new BankCollection();
		public  static CustCollection MultiCustcollection=new CustCollection();
		public  static DetCollection MultiDetcollection=new DetCollection();

		dynamic  bindex = 0;
		dynamic  cindex = 0;
		dynamic  dindex = 0;
		dynamic  CurrentSelection = 0;

		#region DELEGATES / EVENTS Declarations

		public static  event EventHandler<LoadedEventArgs> DetDataLoaded;

		#endregion DELEGATES / EVENTS Declarations

		#region DECLARATIONS

		public string CurrentDb = "";
		static bool inprogress = false;

		#endregion DECLARATIONS


		#region STARTUP/CLOSE

		public MultiViewer ( )
		{
			InitializeComponent ( );
			//			BankCollection bc = new BankCollection ( );
			this . BankGrid . ItemsSource = MultiBankcollection;
			this . CustomerGrid . ItemsSource = MultiCustcollection;
			this. DetailsGrid . ItemsSource = MultiDetcollection;
			this . MouseDown += delegate { DoDragMove ( ); };
			this . BankGrid . MouseDown += delegate { DoDragMove ( ); };
			this . CustomerGrid . MouseDown += delegate { DoDragMove ( ); };
			this. DetailsGrid . MouseDown += delegate { DoDragMove ( ); };

			Flags . SqlBankGrid = this . BankGrid;
			Flags . SqlCustGrid = this . CustomerGrid;
			Flags . SqlDetGrid = this . DetailsGrid;
			Flags . MultiViewer = this;
		}
		private async void Window_Loaded ( object sender , RoutedEventArgs e )
		{
			Flags . MultiViewer = this;

			EventControl . BankDataLoaded += UpdatedDataLoaded;

			EventControl . CustDataLoaded += UpdatedDataLoaded;

			EventControl . DetDataLoaded += UpdatedDataLoaded;

			EventControl . ViewerDataHasBeenChanged += EventControl_ViewerDataHasBeenChanged;

			if ( MultiBankcollection == null || MultiBankcollection . Count == 0 )
				MultiBankcollection  = BankCollection . LoadBank ( 3 );
			BankGrid . ItemsSource = MultiBankcollection;
			if ( MultiCustcollection == null || MultiCustcollection . Count == 0 )
				MultiCustcollection = CustCollection . LoadCust ( MultiCustcollection );
			if ( MultiDetcollection == null || MultiDetcollection . Count == 0 )
				MultiDetcollection = DetCollection . LoadDet( MultiDetcollection );
			this . BankGrid . ItemsSource = MultiBankcollection;
			this . CustomerGrid . ItemsSource = MultiCustcollection;
			this . DetailsGrid . ItemsSource = MultiDetcollection;
		}

		private void EventControl_ViewerDataHasBeenChanged ( int EditDbChangeType , int row , string CurentDb )
		{
			// This works 14/5/21 - Yeahhhhhhhh  Events rule !!
			Console . WriteLine ( $"MultiViewer : Data changed event notification received successfully." );
//			Console . WriteLine ( $"MultiViewer : No code implemented yet to handle the update !!!!." );
			RefreshAllGrids ( );
			int x= 0;
		}

		private void Window_Closing ( object sender , System . ComponentModel . CancelEventArgs e )
		{
			// Unsubscribe from Bank data change event notificatoin
			EventControl . BankDataLoaded -= UpdatedDataLoaded;

			EventControl.ViewerDataHasBeenChanged -= ExternalDataUpdate;      // Callback in THIS FILE
			// Clear databases
			MultiBankcollection . Clear ( );
			MultiCustcollection . Clear ( );
			MultiDetcollection . Clear ( );

			Flags . SqlBankGrid = null;
			Flags . SqlCustGrid = null;
			Flags . SqlDetGrid = null;
			Flags . MultiViewer = null;
		}

		#endregion STARTUP/CLOSE

		#region EVENT HANDLERS

				/// <summary>
		/// EVENT HANDLER
		/// for data changes made to BankCollection
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UpdatedDataLoaded ( object sender , LoadedEventArgs e )
		{
			// Update ALL datagrids
			Mouse . OverrideCursor = Cursors . Wait;
			RefreshAllGrids ( );
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
		public void UpdateOnDataChange ( string CurrentDb , DataGridRowEditEndingEventArgs e )
		{
			// Call Handler to update ALL Db's via SQL
			SQLHandlers sqlh = new SQLHandlers  ();
			sqlh . UpdateAllDb ( CurrentDb , e , 2 );
			//Gotta reload our data because the update clears it down totally to null
			// Refresh our grids
			RefreshAllGrids ( );
			inprogress = false;

		}
		public async void RefreshAllGrids ( )
		{
			ReLoadAllDataBases ( );
		}

		private async void ReLoadAllDataBases ( )
		{
			this . BankGrid . ItemsSource = null;
			this . CustomerGrid . ItemsSource = null;
			this. DetailsGrid . ItemsSource = null;

			this . BankGrid . Items . Clear ( );
			this . CustomerGrid . Items . Clear ( );
			this. DetailsGrid . Items . Clear ( );

			BankCollection . LoadBank ( 3);
			MultiBankcollection = BankCollection . LoadBank( 3 );
			MultiCustcollection = CustCollection . LoadCust ( MultiCustcollection );
			MultiDetcollection = DetCollection . LoadDet ( MultiDetcollection );
			int b =bindex;
			int c = cindex;
			int d = dindex;
			this . BankGrid . ItemsSource = MultiBankcollection;
			this . CustomerGrid . ItemsSource = MultiCustcollection;
			this . DetailsGrid . ItemsSource = MultiDetcollection;
			bindex = b;
			cindex = c;
			dindex = d;
			this . BankGrid . Refresh ( );
			this . CustomerGrid . Refresh ( );
			this. DetailsGrid . Refresh ( );

		}

		#endregion DATA UPDATING

		private void ViewerGrid_RowEditEnding ( object sender , DataGridRowEditEndingEventArgs e )
		{
			// Save current positions so we can reposition later
			inprogress = true;
			bindex = this . BankGrid . SelectedIndex;
			cindex = this . CustomerGrid . SelectedIndex;
			dindex = this. DetailsGrid . SelectedIndex;

			CurrentSelection = this . BankGrid . SelectedIndex;
			this . BankGrid  . SelectedItem = this . BankGrid  . SelectedIndex;
			//			if ( CurrentSelection == -1 )
			//				CurrentSelection = 0;
			//var item = BankGrid.SelectedItem as BankAccountViewModel;
			UpdateOnDataChange ( CurrentDb , e );
			ResetIndexes ( );
			inprogress = false;
			return;
		}

		public void ResetIndexes ( )
		{
			inprogress = true;
			BankGrid . SelectedIndex = bindex;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				this . BankGrid  . SelectedItem= bindex;
				this . BankGrid  . ScrollIntoView(bindex );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				this . CustomerGrid . SelectedItem = cindex;
				this . CustomerGrid . ScrollIntoView ( cindex );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				this. DetailsGrid . SelectedItem = dindex;
				this. DetailsGrid . ScrollIntoView ( dindex );
			}
			inprogress = false;
		}
		void ReloadData ( DataGrid DGrid )
		{
			try
			{
				Mouse . OverrideCursor = Cursors . Wait;
				// Make sure we are back on UI thread

				int current = 0;
				current = DGrid . SelectedIndex == -1 ? 0 : DGrid . SelectedIndex;
				this . BankGrid  . ItemsSource = null;
				this . CustomerGrid . ItemsSource = null;
				this. DetailsGrid . ItemsSource = null;

				BankCollection . LoadBank (3 );
				this . BankGrid  . ItemsSource = MultiBankcollection;

				MultiCustcollection = CustCollection . LoadCust ( MultiCustcollection );
				this . CustomerGrid . ItemsSource = MultiCustcollection;

				MultiDetcollection = DetCollection . LoadDet ( MultiDetcollection );
				this. DetailsGrid . ItemsSource = MultiDetcollection;

				DGrid . SelectedIndex = current;
				Console . WriteLine ( $"End of ReloadGrid() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
			}
			catch ( Exception ex )
			{
				Console . WriteLine ( $"ERROR: ReloadGrid() {ex . Message}, : {ex . Data}" );
			}
		}

		public void ExternalDataUpdate ( int DbEditChangeType , int row , string currentDb )
		{
			Console . WriteLine ( $"MultiViewer : Data changed event notification received successfully." );
			int x = 0;
		}

		public void RefrehData ( )
		{
			bindex = this . BankGrid  . SelectedIndex;
			cindex = this . CustomerGrid . SelectedIndex;
			dindex = this. DetailsGrid . SelectedIndex;

			this . BankGrid  . ItemsSource = null;
			this . BankGrid  . ItemsSource = MultiBankcollection;
			this . CustomerGrid . ItemsSource = null;
			this . CustomerGrid . ItemsSource = MultiCustcollection;
			this. DetailsGrid . ItemsSource = null;
			this. DetailsGrid . ItemsSource = MultiDetcollection;

			RefreshAllGrids ( );
			// Refresh all grids
			Mouse . OverrideCursor = Cursors . Wait;
			this . BankGrid  . SelectedIndex = bindex;
			this . CustomerGrid . SelectedIndex = cindex;
			this. DetailsGrid . SelectedIndex = dindex;

			this . BankGrid  . ScrollIntoView ( bindex );
			this . CustomerGrid . ScrollIntoView ( cindex );
			this. DetailsGrid . ScrollIntoView ( dindex );
			inprogress = false;
			Mouse . OverrideCursor = Cursors . Arrow;
		}

		private void Close_Click ( object sender , RoutedEventArgs e )
		{
			Close ( );
		}


		private void Window_PreviewKeyDown ( object sender , System . Windows . Input . KeyEventArgs e )
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
				this . BankGrid  . SelectedIndex = 0;
				this . CustomerGrid . SelectedIndex = 0;
				this. DetailsGrid . SelectedIndex = 0;
				ExtensionMethods . Refresh ( this . BankGrid  );
				ExtensionMethods . Refresh ( this . CustomerGrid );
				ExtensionMethods . Refresh ( this . DetailsGrid );
			}
			else if ( e . Key == Key . End )
			{
				this . BankGrid  . SelectedIndex = this . BankGrid  . Items . Count - 1;
				this . CustomerGrid . SelectedIndex = this . CustomerGrid . Items . Count - 1;
				this. DetailsGrid . SelectedIndex = this. DetailsGrid . Items . Count - 1;
				this . BankGrid  . SelectedItem = this . BankGrid  . Items . Count - 1;
				this . CustomerGrid . SelectedItem = this . CustomerGrid . Items . Count - 1;
				this. DetailsGrid . SelectedItem = this. DetailsGrid . Items . Count - 1;
				this . BankGrid  . ScrollIntoView ( this . BankGrid  . Items . Count - 1 );
				this . CustomerGrid . ScrollIntoView ( this . CustomerGrid . Items . Count - 1 );
				this. DetailsGrid . ScrollIntoView ( this. DetailsGrid . Items . Count - 1 );
				ExtensionMethods . Refresh ( this . BankGrid  );
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
		private void BankGrid_SelectionChanged ( object sender , SelectionChangedEventArgs e )
		{
			int rec = 0;
			ScrollViewer scroll;

			if ( inprogress )
				return;
			inprogress = true;

			DataGrid dg = sender as   DataGrid;
			int  currsel = dg.SelectedIndex;
			bindex = currsel;
			if ( currsel <= 0 ) currsel = bindex;
			this . BankGrid  . SelectedIndex = currsel;
			Utils . ScrollRecordIntoView ( this . BankGrid  , 1 );

			// Get Custno from ACTIVE gridso we can find it in other grids
			BankAccountViewModel bgr = this . BankGrid .SelectedItem as BankAccountViewModel ;
			if ( bgr == null ) return;
			rec = FindMatchingRecord ( bgr . CustNo , bgr . BankNo , this.BankGrid , "BANKACCOUNT" );
			this . BankGrid . SelectedIndex = rec;
			bindex = rec;
			Utils . ScrollRecordIntoView ( this . BankGrid  , 1 );

			rec = FindMatchingRecord ( bgr . CustNo , bgr . BankNo , this . CustomerGrid , "CUSTOMER" );
			this . CustomerGrid . SelectedIndex = rec;
			cindex = rec;
			BankData . DataContext = this . CustomerGrid . SelectedItem;
			Utils . ScrollRecordIntoView ( this . CustomerGrid , 1 );

			rec = FindMatchingRecord ( bgr . CustNo , bgr . BankNo , this . DetailsGrid , "DETAILS" );
			this. DetailsGrid . SelectedIndex = rec;
			dindex = rec;
			Utils . ScrollRecordIntoView ( this . DetailsGrid , 1 );
			inprogress = false;
			return;

		}
		private void CustGrid_SelectionChanged ( object sender , SelectionChangedEventArgs e )
		{
			int rec = 0;
			ScrollViewer scroll;

			if ( inprogress )
				return;
			inprogress = true;

			DataGrid dg = sender as   DataGrid;
			int  currsel = dg .SelectedIndex;
			cindex = currsel;
			if ( currsel <= 0 ) currsel = cindex;
			this . CustomerGrid . SelectedIndex = currsel;
			Utils . ScrollRecordIntoView ( this . CustomerGrid , 1 );

			// Get Custno from ACTIVE grid so we can find it in other grids
			CustomerViewModel cgr = CustomerGrid.SelectedItem as CustomerViewModel ;
			if ( cgr == null ) return;
			rec = FindMatchingRecord ( cgr . CustNo , cgr . BankNo , this . CustomerGrid , "CUSTOMER" );
			this . CustomerGrid . SelectedIndex = rec;
			cindex = rec;
			Utils . ScrollRecordIntoView ( this . CustomerGrid , 1 );

			rec = FindMatchingRecord ( cgr . CustNo , cgr . BankNo , this . BankGrid  , "BANKACCOUNT" );
			this . BankGrid  . SelectedIndex = rec;
			bindex = rec;
			Utils . ScrollRecordIntoView ( this . BankGrid  , 1 );
			// Now use SAME CUSTNO to findmatch in Bank  DbGrid

			rec = FindMatchingRecord ( cgr . CustNo , cgr . BankNo , this . DetailsGrid , "DETAILS" );
			this. DetailsGrid . SelectedIndex = rec;
			dindex = rec;
			Utils . ScrollRecordIntoView ( this . DetailsGrid , 1 );
			inprogress = false;

			BankData . DataContext = this . CustomerGrid . SelectedItem;
		}
		private void DetGrid_SelectionChanged ( object sender , SelectionChangedEventArgs e )
		{
			int rec = 0;
			ScrollViewer scroll;

			if ( inprogress )
				return;

			inprogress = true;
			DataGrid dg = sender as   DataGrid;
			int  currsel = dg .SelectedIndex;
			dindex = currsel;
			if ( currsel <= 0 ) currsel = dindex;
			this. DetailsGrid . SelectedIndex = currsel;
			dindex = rec;
			Utils . ScrollRecordIntoView ( DetailsGrid , 1 );

			// Get Custno from ACTIVE gridso we can find it in other grids
			DetailsViewModel dgr = DetailsGrid.SelectedItem as DetailsViewModel ;
			if ( dgr == null ) return;
			rec = FindMatchingRecord ( dgr . CustNo , dgr . BankNo , this.DetailsGrid , "DETAILS" );
			this. DetailsGrid . SelectedIndex = rec;
			dindex = rec;
			Utils . ScrollRecordIntoView ( DetailsGrid , 1 );

			rec = FindMatchingRecord ( dgr . CustNo , dgr . BankNo , this . BankGrid  , "BANKACCOUNT" );
			this . BankGrid  . SelectedIndex = rec;
			bindex = rec;
			Utils . ScrollRecordIntoView ( this . BankGrid  , 1 );

			// Now use SAME CUSTNO to findmatch in Customer DbGrid
			rec = FindMatchingRecord ( dgr . CustNo , dgr . BankNo , this . CustomerGrid , "CUSTOMER" );
			this . CustomerGrid . SelectedIndex = rec;
			cindex = rec;
			Utils . ScrollRecordIntoView ( this . CustomerGrid , 1 );

			inprogress = false;
			BankData . DataContext = this . CustomerGrid . SelectedItem;
		}
		private int FindMatchingRecord ( string Custno , string Bankno , DataGrid Grid , string CurrentDb )
		{
			int index = 0;
			if ( CurrentDb == "BANKACCOUNT" )
			{
				foreach ( var item in this . BankGrid . Items )
				{
					BankAccountViewModel cvm = item as  BankAccountViewModel ;
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
				foreach ( var item in this . CustomerGrid . Items )
				{
					CustomerViewModel cvm = item as     CustomerViewModel;
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
				foreach ( var item in this . DetailsGrid . Items )
				{
					DetailsViewModel cvm = item as     DetailsViewModel ;
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

		private void CustomerGrid_GotFocus ( object sender , RoutedEventArgs e )
		{ CurrentDb = "CUSTOMER"; }
		private void BankGrid_GotFocus ( object sender , RoutedEventArgs e )
		{CurrentDb = "BANKACCOUNT"; }
		private void DetailsGrid_GotFocus ( object sender , RoutedEventArgs e )
		{ CurrentDb = "DETAILS"; }

		#endregion focus events

		#region SCROLLBARS

		// scroll bar movement is automatically   stored by these three methods
		// So we can use them to reset position CORRECTLY after refreshes
		private void BankGrid_ScrollChanged ( object sender , ScrollChangedEventArgs e )
		{
			int rec = 0;
			DataGrid dg = null;
			dg = sender as DataGrid;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( (DependencyObject)dg);
			scroll . CanContentScroll = true;
			SetScrollVariables ( sender );
			this . BankGrid  . ScrollIntoView ( this . BankGrid  . SelectedIndex );
			this . CustomerGrid . ScrollIntoView ( this. BankGrid . SelectedIndex );
			this. DetailsGrid . ScrollIntoView ( this . BankGrid . SelectedIndex );

		}
		private void CustomerGrid_ScrollChanged ( object sender , ScrollChangedEventArgs e )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( (DependencyObject)dg );
			scroll . CanContentScroll = true;
			SetScrollVariables ( sender );
			this . BankGrid  . ScrollIntoView ( this . CustomerGrid . SelectedIndex );
			this . CustomerGrid . ScrollIntoView ( this. CustomerGrid . SelectedIndex );
			this. DetailsGrid . ScrollIntoView ( this . CustomerGrid . SelectedIndex );
		}

		private void DetailsGrid_ScrollChanged ( object sender , ScrollChangedEventArgs e )
		{
			DataGrid dg = null;
			dg = sender as DataGrid;
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( (DependencyObject)dg);
			scroll . CanContentScroll = true;
			SetScrollVariables ( sender );
			this . CustomerGrid . ScrollIntoView ( this. DetailsGrid . SelectedIndex );
			this . BankGrid  . ScrollIntoView ( this. DetailsGrid . SelectedIndex );
			this. DetailsGrid . ScrollIntoView ( this . DetailsGrid . SelectedIndex );
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
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( (DependencyObject)sender );
			if ( scroll == null ) return;
			scroll . CanContentScroll = true;
			double d = scroll . VerticalOffset;
			int rounded = Convert.ToInt32(d);
			if ( dg == this . BankGrid  )
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
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( (DependencyObject)dg );
			if ( scroll == null ) return;
			scroll . CanContentScroll = true;
			double d = scroll . VerticalOffset;
			int rounded = Convert.ToInt32(d);
			if ( dg == this . BankGrid  )
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
			var scroll = DataGridNavigation . FindVisualChild<ScrollViewer> ( (DependencyObject)dg);
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
		private void Details_Click ( object sender , RoutedEventArgs e )
		{
		}

		private void Filter_Click ( object sender , RoutedEventArgs e )
		{
			Filtering f = new Filtering();
			if ( CurrentDb == "" )
			{
				MessageBox . Show ( "Please select an entry in one of the data grids before trying to filter the data listed." );
				return;
			}
			Flags . FilterCommand = f . DoFilters ( sender , CurrentDb , 1 );
			ReLoadAllDataBases ( );
		}

		private void BankGrid_Selected ( object sender , RoutedEventArgs e )
		{
			// hit when grid selection is changed by anything
			int x = 0;
			//Console . WriteLine("...");
		}


		private async void Refresh_Click ( object sender , RoutedEventArgs e )
		{
			RefrehData ( );
		}

		private void Db_Click ( object sender , RoutedEventArgs e )
		{
			BankDbView cdbv = new BankDbView();
			cdbv . Show ( );
		}

	}
}
