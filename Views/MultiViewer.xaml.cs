﻿using System;
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
using System . Diagnostics;
using static WPFPages . SqlDbViewer;
using DataGrid = System . Windows . Controls . DataGrid;
using System . Windows . Controls . Primitives;
using System . Collections . Generic;

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
		dynamic key1 = false;
		dynamic GridsLinked = true;
		#region DELEGATES / EVENTS Declarations

		//		public static event EventHandler<LoadedEventArgs> DetDataLoaded;

		#endregion DELEGATES / EVENTS Declarations

		#region DECLARATIONS

		public string CurrentDb = "";
		private bool inprogress = false;
		private bool Triggered = false;
		private bool ReloadingData = false;
		#endregion DECLARATIONS


		#region STARTUP/CLOSE

		public MultiViewer ( )
		{
			InitializeComponent ( );
		}
		private async void Window_Loaded ( object sender, RoutedEventArgs e )
		{
			Mouse . OverrideCursor = Cursors . Wait;
			inprogress = true;
			LoadAllData ( );
			this . MouseDown += delegate { DoDragMove ( ); };
			this . BankGrid . MouseDown += delegate { DoDragMove ( ); };
			this . CustomerGrid . MouseDown += delegate { DoDragMove ( ); };
			this . DetailsGrid . MouseDown += delegate { DoDragMove ( ); };

			// An EditDb has changed the current index 
			EventControl . EditIndexChanged += EventControl_ViewerIndexChanged;
			// A Multiviewer has changed the current index 
			EventControl . MultiViewerIndexChanged += EventControl_ViewerIndexChanged;
			// Another SqlDbviewer has changed the current index 
			EventControl . ViewerIndexChanged += EventControl_ViewerIndexChanged;      // Callback in THIS FILE

			// Event triggers when a Specific Db viewer (BankDbViewer etc) updates the data
			EventControl . ViewerDataUpdated += EventControl_DataUpdated;
			EventControl . EditDbDataUpdated += EventControl_DataUpdated;
			EventControl . BankDataLoaded += EventControl_BankDataLoaded;
			EventControl . CustDataLoaded += EventControl_BankDataLoaded;
			EventControl . DetDataLoaded += EventControl_BankDataLoaded;

			Flags . MultiViewer = this;
			Flags . SqlMultiViewer = this;

			this . BankGrid . ItemsSource = MultiBankcollection;
			this . CustomerGrid . ItemsSource = MultiCustcollection;
			this . DetailsGrid . ItemsSource = MultiDetcollection;

			//Select first record in all grids using an Action declared in SqlDbViewer
			Utils . GridInitialSetup ( BankGrid, 0 );
			Utils . GridInitialSetup ( CustomerGrid, 0 );
			Utils . GridInitialSetup ( DetailsGrid, 0 );

			Utils . SetUpGridSelection ( this . BankGrid, 0 );
			Utils . SetUpGridSelection ( this . CustomerGrid, 0 );
			Utils . SetUpGridSelection ( this . DetailsGrid, 0 );

			if ( Flags . LinkviewerRecords )
				LinkRecords . IsChecked = true;

			LinkGrids . IsChecked = true;
			GridsLinked = true;

			// Set window to TOPMOST
			OntopChkbox . IsChecked = true;
			this . Topmost = true;
			inprogress = false;
			Mouse . OverrideCursor = Cursors . Arrow;

		}

		private async void EventControl_BankDataLoaded ( object sender, LoadedEventArgs e )
		{
			// Data has been changed externally, so update all Data Grids
			ReLoadAllDataBases ( CurrentDb, this . BankGrid . SelectedIndex );
//			await Utils . DoBeep ( 700, 400, true );

			Utils . SetUpGridSelection ( this . BankGrid, this . BankGrid . SelectedIndex );
			Utils . SetUpGridSelection ( this . CustomerGrid, this . CustomerGrid . SelectedIndex );
			Utils . SetUpGridSelection ( this . DetailsGrid, this . DetailsGrid . SelectedIndex );
		}

		private void EventControl_EditIndexChanged ( object sender, IndexChangedArgs e )
		{
			EventControl_ViewerIndexChanged ( sender, e );
			//Triggered = true;
			//// update rows when another window changes it if linkage is ON
			//this . BankGrid . SelectedIndex = e . Row;
			//Triggered = false;
		}

		private async void EventControl_ViewerIndexChanged ( object sender, IndexChangedArgs e )
		{
			// Sanity check, if we MADE the index change, so don't bother
			if ( e . Senderviewer == null ) return;

//			await Utils . DoBeep ( 700, 200, true );
			if ( Flags . LinkviewerRecords && Triggered == false )
			{
				object RowTofind = null;
				object gr = null;
				int rec = 0;
				inprogress = true;
				if ( e . Sender == "BANKACCOUNT" && GridsLinked == false )
				{
					// Only Update the specific grid
					rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . BankGrid, "BANKACCOUNT" );
					this . BankGrid . SelectedIndex = rec;
					bindex = rec;
					Utils . ScrollRecordIntoView ( this . BankGrid, rec );
					return;
				}
				else if ( e . Sender == "CUSTOMER" && GridsLinked == false )
				{
					// Only Update the specific grid
					rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . CustomerGrid, "CUSTOMER" );
					this . CustomerGrid . SelectedIndex = rec;
					cindex = rec;
					BankData . DataContext = this . CustomerGrid . SelectedItem;
					Utils . ScrollRecordIntoView ( this . CustomerGrid, rec );
					return;
				}
				if ( e . Sender == "DETAILS" && GridsLinked == false )
				{
					// Only Update the specific grid
					rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . DetailsGrid, "DETAILS" );
					this . DetailsGrid . SelectedIndex = rec;
					dindex = rec;
					Utils . ScrollRecordIntoView ( this . DetailsGrid, rec );
					return;
				}
				else
				{
					// Update all three grids
					rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . BankGrid, "BANKACCOUNT" );
					this . BankGrid . SelectedIndex = rec;
					bindex = rec;
					Utils . ScrollRecordIntoView ( this . BankGrid, rec );
					rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . CustomerGrid, "CUSTOMER" );
					this . CustomerGrid . SelectedIndex = rec;
					cindex = rec;
					BankData . DataContext = this . CustomerGrid . SelectedItem;
					Utils . ScrollRecordIntoView ( this . CustomerGrid, rec );
					rec = Utils . FindMatchingRecord ( e . Custno, e . Bankno, this . DetailsGrid, "DETAILS" );
					this . DetailsGrid . SelectedIndex = rec;
					dindex = rec;
					Utils . ScrollRecordIntoView ( this . DetailsGrid, rec );
				}
				inprogress = false;
			}
		}

		private void Window_Closing ( object sender, System . ComponentModel . CancelEventArgs e )
		{
			// Unsubscribe from Bank data change event notificatoin
			// Main update notification handler
			//			EventControl . DataUpdated -= EventControl_DataUpdated;
			EventControl . ViewerDataUpdated -= EventControl_DataUpdated;
			EventControl . EditDbDataUpdated -= EventControl_DataUpdated;

			// Event triggers when a Specific Db viewer (BankDbViewer etc) updates the data
			EventControl . BankDataLoaded -= EventControl_DataUpdated;
			EventControl . CustDataLoaded -= EventControl_DataUpdated;
			EventControl . DetDataLoaded -= EventControl_DataUpdated;

			// Listen ofr index changes
			EventControl . ViewerIndexChanged -= EventControl_ViewerIndexChanged;

			// Clear databases
			MultiBankcollection . Clear ( );
			MultiCustcollection . Clear ( );
			MultiDetcollection . Clear ( );

			Flags . MultiViewer = null;
		}

		/// <summary>
		/// Main Event handkler for data changes made by other windows
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void EventControl_DataUpdated ( object sender, LoadedEventArgs e )
		{
			// Update ALL datagrids - IF we didnt   truiigger the change
			if ( sender == MultiBankcollection || sender == MultiCustcollection || sender == MultiDetcollection )
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

		private  async Task LoadAllData ( )
		{
			// load the data
			Mouse . OverrideCursor = Cursors . Wait;
			if ( MultiBankcollection == null || MultiBankcollection . Count == 0 )
				await BankCollection . LoadBank ( MultiBankcollection, 3, true );
			BankGrid . ItemsSource = MultiBankcollection;
			if ( MultiCustcollection == null || MultiCustcollection . Count == 0 )
				await CustCollection . LoadCust ( MultiCustcollection );
			if ( MultiDetcollection == null || MultiDetcollection . Count == 0 )
				await DetCollection . LoadDet ( MultiDetcollection);

			Flags . MultiViewer = this;
			this . BankGrid . ItemsSource = MultiBankcollection;
			this . CustomerGrid . ItemsSource = MultiCustcollection;
			this . DetailsGrid . ItemsSource = MultiDetcollection;
			//			Mouse . OverrideCursor = Cursors . Arrow;
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
			Mouse . OverrideCursor = Cursors . Wait;
			SQLHandlers sqlh = new SQLHandlers ( );
			sqlh . UpdateAllDb ( CurrentDb, e );
			bindex = this . BankGrid . SelectedIndex;
			cindex = this . CustomerGrid . SelectedIndex;
			dindex = this . DetailsGrid . SelectedIndex;
			//Gotta reload our data because the update clears it down totally to null
			// Refresh our grids
			RefreshAllGrids ( CurrentDb, e . Row . GetIndex ( ) );
			inprogress = false;
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
			Custno = bvm? . CustNo;
			Bankno = bvm? . BankNo;

			// Sanity check
			if ( bvm == null || cvm == null || dvm == null )
			{
//				await Utils . DoBeep ( 175, 300 ) . ConfigureAwait ( false );
				return;
			}

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
			MultiBankcollection = await BankCollection . LoadBank ( MultiBankcollection, 3 , true);
//			MultiBankcollection = BankCollection . MultiBankcollection;

			MultiCustcollection = await CustCollection . LoadCust ( MultiCustcollection );
//			MultiCustcollection = CustCollection . MultiCustcollection;

			MultiDetcollection = await DetCollection . LoadDet ( MultiDetcollection );
//			MultiDetcollection = DetCollection . MultiDetcollection;

			this . BankGrid . ItemsSource = MultiBankcollection;
			
			// This causes a data load of bank data !!!!
			this . BankGrid . Refresh ( );
			//bbindex = Utils . FindMatchingRecord ( Custno, Bankno, this . BankGrid, "BANKACCOUNT" );
			//inprogress = true;
                       if ( this . BankGrid . Items . Count > 0 )
			{
				this . BankGrid . SelectedIndex = bbindex;
				this . BankGrid . SelectedItem = bbindex;
				this . BankGrid . Refresh ( );
				this . BankGrid . ScrollIntoView ( bbindex );
			}


			this . CustomerGrid . ItemsSource = MultiCustcollection;
			//ccindex = Utils . FindMatchingRecord ( Custno, Bankno, this . CustomerGrid, "CUSTOMER" );
			//inprogress = true;
			if ( this . CustomerGrid . Items . Count > 0 )
			{
				this . CustomerGrid . SelectedIndex = ccindex;
				this . CustomerGrid . SelectedItem = ccindex;
				this . CustomerGrid . Refresh ( );
				this . CustomerGrid . ScrollIntoView ( ccindex );
			}

			this . DetailsGrid . ItemsSource = MultiDetcollection;
			//ddindex = Utils . FindMatchingRecord ( Custno, Bankno, this . DetailsGrid, "DETAILS" );
			//inprogress = true;
			if ( this . DetailsGrid . Items . Count > 0 )
			{
				this . DetailsGrid . SelectedIndex = ddindex;
				this . DetailsGrid . SelectedItem = ddindex;
				this . DetailsGrid . Refresh ( );
				this . DetailsGrid . ScrollIntoView ( ddindex );
			}
			
			//inprogress = false;;

			Console . WriteLine ( $"bbindex={bbindex}, ccindex={ccindex}, ddindex={ddindex}" );
			Console . WriteLine ( $"Bank={Bankno}, Cust={Custno}" );
			Console . WriteLine ( $"Bank={this . BankGrid . SelectedIndex}, Cust={this . CustomerGrid . SelectedIndex}, Det={this . DetailsGrid . SelectedIndex}" );
			if ( Flags . FilterCommand != "" )
			{
				string tmp = Flags . FilterCommand;
				string shortstring = tmp . Substring ( 25 );
				tmp = "Select * from Customer " + shortstring;
				Flags . FilterCommand = tmp;
			}
			//			Utils . SetUpGridSelection ( this . BankGrid, bbindex );
			//			Utils . SetUpGridSelection ( this . CustomerGrid, ccindex );
			//			Utils . SetUpGridSelection ( this . DetailsGrid, ddindex );

			if ( CurrentDb == "BANKACCOUNT" )
				this . BankGrid . Focus ( );
			else if ( CurrentDb == "CUSTOMER" )
				this . CustomerGrid . Focus ( );
			else if ( CurrentDb == "DETAILS" )
				this . DetailsGrid . Focus ( );
			Mouse . OverrideCursor = System . Windows . Input . Cursors . Arrow;

		}

		#endregion EVENT DATA UPDATING


		/// <summary>
		/// Just resets  the SelectedITEM and scroills it into view
		/// </summary>
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
				Debug . WriteLine ( $"End of ReloadGrid() : Thread = { Thread . CurrentThread . ManagedThreadId}" );
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"ERROR: ReloadGrid() {ex . Message}, : {ex . Data}" );
			}
		}

		public void ExternalDataUpdate ( int DbEditChangeType, int row, string currentDb )
		{
			Debug . WriteLine ( $"MultiViewer : Data changed event notification received successfully." );
			int x = 0;
		}

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


		private void Window_PreviewKeyDown ( object sender, System . Windows . Input . KeyEventArgs e )
		{
			DataGrid dg = null;
			int CurrentRow = 0;
			bool showdebug = false;

			if ( showdebug ) Debug . WriteLine ( $"key1 = {key1},  Key = : {e . Key}" );

			if ( e . Key == Key . LeftCtrl )
			{
				key1 = true;
				if ( showdebug ) Debug . WriteLine ( $"key1 = set to TRUE" );
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
			else if ( key1 && e . Key == Key . F7 )  // CTRL + F7
			{
				// list various Flags in Console
				Flags . PrintDbInfo ( );
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
				if ( dg . SelectedItem == null )
					Utils . ScrollRecordInGrid ( dg, 0);
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
					CustCollection . dtCust?.Clear ( );
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
					e . Handled = true;
					key1 = false;

					// Call the method to update any other Viewers that may be open
					EventControl . TriggerRecordDeleted ( CurrentDb, bank, cust, CurrentRow );
					// Keep our focus in originating window for now
					Thisviewer . Activate ( );
					Thisviewer . Focus ( );
					return;
				}
				e . Handled = false;
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

				}
			}
			e . Handled = false;
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

			if ( ReloadingData )
				return;

			BankAccountViewModel CurrentSelectedRecord =this.BankGrid.CurrentItem as BankAccountViewModel;
			if ( CurrentSelectedRecord == null )
			{
				Console . WriteLine ( $"Bank Grid ERROR - Currentitem is NULL on Entry to Selectionchanged !!" );
				//if(Flags.UseBeeps) 
				//	await Utils . DoErrorBeep ( 200, 400 , 2).ConfigureAwait(false);
				return;
			}

			ScrollViewer scroll;

			if ( inprogress )
				return;
			inprogress = true;

			DataGrid dg = sender as DataGrid;
			int currsel = dg . SelectedIndex;
			bindex = currsel;
			if ( currsel <= 0 ) currsel = bindex;
			this . BankGrid . SelectedIndex = currsel;
			this . BankGrid . SelectedItem = currsel;

			Type tthis = this . GetType ( );
			if ( tthis . FullName . Contains ( "MultiViewer" ) )
			{
				// We triggered this change
				CurrentSelectedRecord = this . BankGrid . CurrentItem as BankAccountViewModel;
				if ( CurrentSelectedRecord == null )
				{
					Console . WriteLine ( $"Bank Grid ERROR - Currentitem is NULL !!" );
//					await Utils . DoSingleBeep ( 200, 200, 4 ) . ConfigureAwait ( false );
					return;
				}
				SearchCustNo = CurrentSelectedRecord?.CustNo;
				SearchBankNo = CurrentSelectedRecord?.BankNo;
				if ( SearchCustNo == null && SearchBankNo == null )
				{
					inprogress = false;
//					await Utils . DoBeep ( 300, 300 ) . ConfigureAwait ( false );
//					await Utils . DoSingleBeep ( 300, 300, 1 ) . ConfigureAwait ( false );
					return;
				}
				//EventControl . TriggerMultiViewerIndexChanged ( MultiBankcollection,
				//// Send message to othrr viewers teling them of our index change
				////				Debug . WriteLine ( $" 7-1 *** TRACE *** MULTIVIEWER: Grid_SelectionChanged  BANKACCOUNT- Sending TriggerMultiViewerIndexChanged Event trigger" );
				//new IndexChangedArgs
				//{
				//	Senderviewer = null,            // This is ONLY for SqlDbViewers
				//	SenderId = "MultiBank",
				//	Bankno = SearchBankNo,
				//	Custno = SearchCustNo,
				//	dGrid = this . BankGrid,
				//	Sender = "BANKACCOUNT",
				//	Row = this . BankGrid . SelectedIndex
				//} );
//				await Utils . DoSingleBeep ( 200, 300,1 ) . ConfigureAwait ( false);
			}

			if ( GridsLinked )
			{
				// update all grids position
				Triggered = true;
				rec = Utils . FindMatchingRecord ( SearchCustNo, SearchBankNo, this . CustomerGrid, "CUSTOMER" );
				// Store current index to global
				cindex = rec;
				Utils . ScrollRecordIntoView ( this . CustomerGrid, rec );

				rec = Utils . FindMatchingRecord ( SearchCustNo, SearchBankNo, this . DetailsGrid, "DETAILS" );
				// Store current index to global
				dindex = rec;
				Utils . ScrollRecordIntoView ( this . DetailsGrid, rec );
				Triggered = false;
			}
			else
			{
				// just update our own position
				Triggered = true;
				rec = Utils . FindMatchingRecord ( SearchCustNo, SearchBankNo, this . BankGrid, "BANKACCOUNT" );
				// Store current index to global
				bindex = rec;
				Utils . ScrollRecordIntoView ( this . BankGrid, rec );
				Triggered = false;
			}
			if ( tthis . FullName . Contains ( "MultiViewer" ) )
			{
				EventControl . TriggerMultiViewerIndexChanged ( MultiBankcollection,
				// Send message to othrr viewers teling them of our index change
				//				Debug . WriteLine ( $" 7-1 *** TRACE *** MULTIVIEWER: Grid_SelectionChanged  BANKACCOUNT- Sending TriggerMultiViewerIndexChanged Event trigger" );
				new IndexChangedArgs
				{
					Senderviewer = null,            // This is ONLY for SqlDbViewers
					SenderId = "MultiBank",
					Bankno = SearchBankNo,
					Custno = SearchCustNo,
					dGrid = this . BankGrid,
					Sender = "BANKACCOUNT",
					Row = this . BankGrid . SelectedIndex
				} );
			}
			Triggered = false;
			inprogress = false;
			BankData . DataContext = this . BankGrid . SelectedItem;
			return;
		}
		//****************************************************************************************************//
		private async void CustGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			int rec = 0;
			string SearchCustNo = "";
			string SearchBankNo = "";

			if ( ReloadingData )
				return;

			CustomerViewModel CurrentSelectedRecord = this . CustomerGrid . CurrentItem as CustomerViewModel;
			if ( CurrentSelectedRecord == null )
			{
				Console . WriteLine ( $"Customer Grid ERROR - Currentitem is NULL on Entry to Selectionchanged !!" );
//				await Utils . DoErrorBeep ( 300, 400, 2 ) . ConfigureAwait ( false );
				return;
			}

			ScrollViewer scroll;

			if ( inprogress )
				return;
			inprogress = true;

			DataGrid dg = sender as DataGrid;
			int currsel = dg . SelectedIndex;
			cindex = currsel;
			if ( currsel <= 0 ) currsel = cindex;
			this . CustomerGrid . SelectedIndex = currsel;
			this . CustomerGrid . SelectedItem = currsel;

			Type tthis = this . GetType ( );
			if ( tthis . FullName . Contains ( "MultiViewer" ) )
			{
				// We triggered this change
				CurrentSelectedRecord = this . CustomerGrid . CurrentItem as CustomerViewModel;
				if ( CurrentSelectedRecord == null )
				{
					Console . WriteLine ($"Customer Grid ERROR - Currentitem is NULL !!");
//					await Utils . DoErrorBeep ( 300, 200 , 4) . ConfigureAwait ( false );
					return;
				}
				SearchCustNo = CurrentSelectedRecord?.CustNo;
				SearchBankNo = CurrentSelectedRecord?.BankNo;
				if ( SearchCustNo == null && SearchBankNo == null )
				{
					inprogress = false;
					return;
				}
				EventControl . TriggerMultiViewerIndexChanged ( MultiBankcollection,
				// Send message to othrr viewers teling them of our index change
				//				Debug . WriteLine ( $" 7-2 *** TRACE *** MULTIVIEWER: CustGrid_SelectionChanged  CUSTOMER - Sending TriggerMultiViewerIndexChanged Event trigger" );
				new IndexChangedArgs
				{
					Senderviewer = null,            // This is ONLY for SqlDbViewers
					Bankno = SearchBankNo,
					Custno = SearchCustNo,
					SenderId = "Multicust",
					dGrid = this . CustomerGrid,
					Sender = "CUSTOMER",
					Row = this . CustomerGrid . SelectedIndex
				} );
				//				Utils . DoBeep ( 450, 100, false);
//				await Utils . DoSingleBeep ( 300, 400, 1 ) . ConfigureAwait ( false);
				
			}

			if ( GridsLinked )
			{
				// update all grids position
				Triggered = true;
				rec = Utils . FindMatchingRecord ( SearchCustNo, SearchBankNo, this . BankGrid, "BANKACCOUNT" );
				// Store current index to global
				bindex = rec;
				Utils . ScrollRecordIntoView ( this . BankGrid, rec );

				rec = Utils . FindMatchingRecord ( SearchCustNo, SearchBankNo, this . DetailsGrid, "DETAILS" );
				// Store current index to global
				dindex = rec;
				Utils . ScrollRecordIntoView ( this . DetailsGrid, rec );
				Triggered = false;
			}
			else
			{
				// just update our own position
				Triggered = true;
				rec = Utils . FindMatchingRecord ( SearchCustNo, SearchBankNo, this . CustomerGrid, "CUSTOMER" );
				// Store current index to global
				cindex = rec;
				Utils . ScrollRecordIntoView ( this . CustomerGrid, rec );
				Triggered = false;
			}
			Triggered = false;
			inprogress = false;
			BankData . DataContext = this . CustomerGrid . SelectedItem;
			return;

		}
		private async void DetGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			int rec = 0;
			string SearchCustNo = "";
			string SearchBankNo = "";

			if ( ReloadingData )
				return;

			DetailsViewModel CurrentSelectedRecord = this . DetailsGrid . CurrentItem as DetailsViewModel;
			if ( CurrentSelectedRecord == null )
			{
				Console . WriteLine ( $"Details Grid ERROR - Currentitem is NULL on Entry to Selectionchanged !!" );
//				await Utils . DoErrorBeep ( 400, 400, 2 ) . ConfigureAwait ( false );
				return;
			}

			ScrollViewer scroll;

			if ( inprogress )
				return;
			inprogress = true;

			DataGrid dg = sender as DataGrid;
			int currsel = dg . SelectedIndex;
			dindex = currsel;
			if ( currsel <= 0 ) currsel = dindex;
			this . DetailsGrid . SelectedIndex = currsel;
			this . DetailsGrid . SelectedItem = currsel;

			Type tthis = this . GetType ( );
			if ( tthis . FullName . Contains ( "MultiViewer" ) )
			{
				// We triggered this change
				CurrentSelectedRecord = this . DetailsGrid . CurrentItem as DetailsViewModel;
				if ( CurrentSelectedRecord == null )
				{
					Console . WriteLine ( $"Details Grid ERROR - Currentitem is NULL !!" );
					return;
				}
				SearchCustNo = CurrentSelectedRecord?.CustNo;
				SearchBankNo = CurrentSelectedRecord?.BankNo;
				if ( SearchCustNo == null && SearchBankNo == null )
				{
					inprogress = false;
//					await Utils . DoErrorBeep ( 400, 200, 4 ) . ConfigureAwait ( false );
					return;
				}
				// Send message to othrr viewers teling them of our index change
				//				Debug . WriteLine ( $" 7-3 *** TRACE *** MULTIVIEWER: DetGrid_SelectionChanged  DETAILS - Sending TriggerMultiViewerIndexChanged Event trigger" );
				EventControl . TriggerMultiViewerIndexChanged ( MultiBankcollection,
					new IndexChangedArgs
					{
						Senderviewer = null,            // This is ONLY for SqlDbViewers
						Bankno = SearchBankNo,
						Custno = SearchCustNo,
						SenderId = "MultiDet",
						dGrid = this . DetailsGrid,
						Sender = "DETAILS",
						Row = this . DetailsGrid . SelectedIndex
					} );
//				await Utils . DoSingleBeep ( 400, 400, 1 ) . ConfigureAwait ( false);

			}

			if ( GridsLinked )
			{
				// update all grids position
				Triggered = true;
				rec = Utils . FindMatchingRecord ( SearchCustNo, SearchBankNo, this . BankGrid, "BANKACCOUNT" );
				// Store current index to global
				bindex = rec;
				Utils . ScrollRecordIntoView ( this . BankGrid, rec );

				rec = Utils . FindMatchingRecord ( SearchCustNo, SearchBankNo, this . CustomerGrid, "CUSTOMER" );
				// Store current index to global
				cindex = rec;
				Utils . ScrollRecordIntoView ( this . CustomerGrid, rec );
				Triggered = false;
			}
			else
			{
				// just update our own position
				Triggered = true;
				rec = Utils . FindMatchingRecord ( SearchCustNo, SearchBankNo, this . DetailsGrid, "DETAILS" );
				// Store current index to global
				dindex = rec;
				Utils . ScrollRecordIntoView ( this . DetailsGrid, rec );
				Triggered = false;
			}
			Triggered = false;
			inprogress = false;
			BankData . DataContext = this . DetailsGrid . SelectedItem;
			return;
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
				//				Debug . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . TopVisibleBankGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . TopVisibleBankGridRow = ( double ) rounded;
			}
			else if ( dg == this . CustomerGrid )
			{
				//				Debug . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . TopVisibleCustGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . TopVisibleCustGridRow = ( double ) rounded;
			}
			else if ( dg == this . DetailsGrid )
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
			if ( dg == this . BankGrid )
			{
				//				Debug . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . BottomVisibleBankGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . BottomVisibleBankGridRow = ( double ) rounded;
			}
			else if ( dg == this . CustomerGrid )
			{
				//				Debug . WriteLine ( $"\n######## Flags . TopVisibleDetGridRow == {scroll . VerticalOffset}\n######## TopVisible = { Flags . BottomVisibleCustGridRow}\n######## NEW Value = { scroll . VerticalOffset}" );
				Flags . BottomVisibleCustGridRow = ( double ) rounded;
			}
			else if ( dg == this . DetailsGrid )
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
				DetailsDbView cdbv = new DetailsDbView ( );
				cdbv . Show ( );
			}
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

			Mouse . OverrideCursor = Cursors . Arrow;
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
			var accounts = from items in MultiBankcollection
				       where ( items . AcType == 1 )
				       orderby items . CustNo
				       select items;
			this . BankGrid . ItemsSource = accounts;
			var accounts1 = from items in MultiCustcollection
					where ( items . AcType == 1 )
					orderby items . CustNo
					select items;
			this . CustomerGrid . ItemsSource = accounts1;
			var accounts2 = from items in MultiDetcollection
					where ( items . AcType == 1 )
					orderby items . CustNo
					select items;
			this . DetailsGrid . ItemsSource = accounts2;
			StatusBar . Text = "Only Records matching Account Type = 1 are shown above";
			Mouse . OverrideCursor = Cursors . Arrow;
		}
		private void Linq2_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			Mouse . OverrideCursor = Cursors . Wait;
			var accounts = from items in MultiBankcollection
				       where ( items . AcType == 2 )
				       orderby items . CustNo
				       select items;
			this . BankGrid . ItemsSource = accounts;
			var accounts1 = from items in MultiCustcollection
					where ( items . AcType == 2 )
					orderby items . CustNo
					select items;
			this . CustomerGrid . ItemsSource = accounts1;
			var accounts2 = from items in MultiDetcollection
					where ( items . AcType == 2 )
					orderby items . CustNo
					select items;
			this . DetailsGrid . ItemsSource = accounts2;
			StatusBar . Text = "Only Records matching Account Type = 2 are shown above";
			Mouse . OverrideCursor = Cursors . Arrow;
		}
		private void Linq3_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			Mouse . OverrideCursor = Cursors . Wait;
			var accounts = from items in MultiBankcollection
				       where ( items . AcType == 3 )
				       orderby items . CustNo
				       select items;
			this . BankGrid . ItemsSource = accounts;
			var accounts1 = from items in MultiCustcollection
					where ( items . AcType == 3 )
					orderby items . CustNo
					select items;
			this . CustomerGrid . ItemsSource = accounts1;
			var accounts2 = from items in MultiDetcollection
					where ( items . AcType == 3 )
					orderby items . CustNo
					select items;
			this . DetailsGrid . ItemsSource = accounts2;
			StatusBar . Text = "Only Records matching Account Type = 3 are shown above";
			Mouse . OverrideCursor = Cursors . Arrow;
		}
		private void Linq4_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			Mouse . OverrideCursor = Cursors . Wait;
			var accounts = from items in MultiBankcollection
				       where ( items . AcType == 4 )
				       orderby items . CustNo
				       select items;
			this . BankGrid . ItemsSource = accounts;
			var accounts1 = from items in MultiCustcollection
					where ( items . AcType == 4 )
					orderby items . CustNo
					select items;
			this . CustomerGrid . ItemsSource = accounts1;
			var accounts2 = from items in MultiDetcollection
					where ( items . AcType == 4 )
					orderby items . CustNo
					select items;
			this . DetailsGrid . ItemsSource = accounts2;
			StatusBar . Text = "Only Records matching Account Type = 4 are shown above";
			Mouse . OverrideCursor = Cursors . Arrow;
		}
		private void Linq5_Click ( object sender, RoutedEventArgs e )
		{
			int q = 1;
			//select All the items first;			
			Mouse . OverrideCursor = Cursors . Wait;
			if ( q == 1 )
			{
				var accounts = from items in MultiBankcollection orderby items . CustNo, items . AcType select items;
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
				var accounts = from items in MultiCustcollection orderby items . CustNo, items . AcType select items;
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
				var accounts = from items in MultiDetcollection orderby items . CustNo, items . AcType select items;
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
			Mouse . OverrideCursor = Cursors . Arrow;
		}
		private void Linq6_Click ( object sender, RoutedEventArgs e )
		{
			Mouse . OverrideCursor = Cursors . Wait;
			var accounts = from items in MultiBankcollection orderby items . CustNo, items . AcType select items;
			var accounts1 = from items in MultiCustcollection orderby items . CustNo, items . AcType select items;
			var accounts2 = from items in MultiDetcollection orderby items . CustNo, items . AcType select items;
			this . BankGrid . ItemsSource = accounts;
			this . CustomerGrid . ItemsSource = accounts1;
			this . DetailsGrid . ItemsSource = accounts2;
			StatusBar . Text = "All available Records are shown above in all three grids";
			Mouse . OverrideCursor = Cursors . Arrow;
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
		/// Method that is called when ANY grid has a data change made to it.
		/// It updates ALL the Db's first, then triggers a ViewerDataUpdated()  EVENT
		/// to notify any other open viewers
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void ViewerGrid_RowEditEnding ( object sender, DataGridRowEditEndingEventArgs e )
		{
			string SearchCustNo = "";
			string SearchBankNo = "";
			// Save current positions so we can reposition later
			inprogress = true;

			// Set globals
			bindex = this . BankGrid . SelectedIndex;
			cindex = this . CustomerGrid . SelectedIndex;
			dindex = this . DetailsGrid . SelectedIndex;


			if ( CurrentDb == "BANKACCOUNT" )
			{
				BankAccountViewModel CurrentBankSelectedRecord = this . BankGrid . CurrentItem as BankAccountViewModel;
				if ( CurrentBankSelectedRecord == null )
				{
					Console . WriteLine ( $"Bank Grid ERROR - Currentitem is NULL on Entry to Selectionchanged !!" );
//					await Utils . DoBeep ( 300, 300 ) . ConfigureAwait ( false );
					return;
				}
				SearchCustNo = CurrentBankSelectedRecord . CustNo;
				SearchBankNo = CurrentBankSelectedRecord . BankNo;
				bindex = this . BankGrid . SelectedIndex;
				// This does the SQL update of the record that has been changed
				UpdateOnDataChange ( CurrentDb, e );
				EventControl . TriggerMultiViewerDataUpdated ( MultiBankcollection,
					new LoadedEventArgs
					{
						Bankno = SearchBankNo,
						Custno = SearchCustNo,
						CallerDb = "BANKACCOUNT",
						DataSource = MultiBankcollection,
						RowCount = this . BankGrid . SelectedIndex
					} );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				CustomerViewModel CurrentBankSelectedRecord = this . CustomerGrid . CurrentItem as CustomerViewModel;
				if ( CurrentBankSelectedRecord == null )
				{
					Console . WriteLine ( $"Customer Grid ERROR - Currentitem is NULL on Entry to Selectionchanged !!" );
//					await Utils . DoBeep ( 300, 300 ) . ConfigureAwait ( false );
					return;
				}
				SearchCustNo = CurrentBankSelectedRecord . CustNo;
				SearchBankNo = CurrentBankSelectedRecord . BankNo;
				cindex = this . CustomerGrid . SelectedIndex;

				// This does the SQL update of the record that has been changed
				UpdateOnDataChange ( CurrentDb, e );
				EventControl . TriggerMultiViewerDataUpdated ( MultiCustcollection,
					new LoadedEventArgs
					{
						Bankno = SearchBankNo,
						Custno = SearchCustNo,
						CallerDb = "CUSTOMER",
						DataSource = MultiCustcollection,
						RowCount = this . CustomerGrid . SelectedIndex
					} );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				DetailsViewModel CurrentBankSelectedRecord = this . DetailsGrid . CurrentItem as DetailsViewModel;
				if ( CurrentBankSelectedRecord == null )
				{
					Console . WriteLine ( $"Details Grid ERROR - Currentitem is NULL on Entry to Selectionchanged !!" );
//					await Utils . DoBeep ( 300, 300 ) . ConfigureAwait ( false );
					return;
				}
				SearchCustNo = CurrentBankSelectedRecord . CustNo;
				SearchBankNo = CurrentBankSelectedRecord . BankNo;
				dindex = this . DetailsGrid . SelectedIndex;

				// This does the SQL update of the record that has been changed
				UpdateOnDataChange ( CurrentDb, e );
				EventControl . TriggerMultiViewerDataUpdated ( MultiDetcollection,
					new LoadedEventArgs
					{
						Bankno = SearchBankNo,
						Custno = SearchCustNo,
						CallerDb = "DETAILS",
						DataSource = MultiDetcollection,
						RowCount = this . DetailsGrid . SelectedIndex
					} );
			}
			ResetIndexes ( );
			inprogress = false;
		}
	}
}