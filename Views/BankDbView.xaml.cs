using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . ComponentModel;
using System . Diagnostics;
using System . Linq;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Input;
using System . Windows . Media;
using WPFPages . ViewModels;

namespace WPFPages . Views
{

	/// <summary>
	/// Interaction logic for BankDbView.xaml
	/// </summary>
	public partial class BankDbView : Window
	{
		public BankCollection BankViewcollection = null;

		// Get our personal Collection view of the Db
		public ICollectionView BankviewerView { get; set; }

		private bool IsDirty = false;
		static bool Startup = true;
		private static bool LinktoParent = false;
		private static bool LinktoMultiParent = false;
		private bool Triggered = false;
		private bool LoadingDbData = false;
		private bool RowHasBeenEdited { get; set; }
		private bool keyshifted { get; set; }
		private bool IsEditing { get; set; }
		public static int bindex { get; set; }
		public bool IsLeftButtonDown { get; set; }

		private string _bankno = "";
		private string _custno = "";
		private string _actype = "";
		private string _balance = "";
		private string _odate = "";
		private string _cdate = "";
		private SqlDbViewer SqlParentViewer;
		private MultiViewer MultiParentViewer;
		private Thread t1;

		// Crucial structure for use when a Grid row is being edited
		private static RowData bvmCurrent = null;
		public BankDbView ( )
		{
			Startup = true;
			InitializeComponent ( );
			this . Show ( );
			this . Refresh ( );
		}
		#region Mouse support
		private void DoDragMove ( )
		{//Handle the button NOT being the left mouse button
		 // which will crash the DragMove Fn.....
			try
			{ this . DragMove ( ); }
			catch { return; }
		}
		#endregion Mouse support

		#region Startup/ Closedown
		private async void Window_Loaded ( object sender, RoutedEventArgs e )
		{
			Mouse . OverrideCursor = System . Windows . Input . Cursors . Wait;
			this . Show ( );
			this . Refresh ( );
			Startup = true;

			string ndx = ( string ) Properties . Settings . Default [ "BankDbView_bindex" ];
			bindex = int . Parse ( ndx );
			this . BankGrid . SelectedIndex = bindex < 0 ? 0 : bindex;

			this . MouseDown += delegate { DoDragMove ( ); };
			// An EditDb has changed the current index 
			EventControl . EditIndexChanged += EventControl_EditIndexChanged;
			// A Multiviewer has changed the current index 
			EventControl . MultiViewerIndexChanged += EventControl_EditIndexChanged;
			// Another viewer has changed the current index 
			EventControl . ViewerIndexChanged += EventControl_EditIndexChanged;      // Callback in THIS FILE
			EventControl . ViewerDataUpdated += EventControl_DataUpdated;
			EventControl . BankDataLoaded += EventControl_BankDataLoaded;

			await BankCollection . LoadBank ( BankViewcollection, "BANKDBVIEW", 3, true );

			SaveBttn . IsEnabled = false;
			// Save linkage setting as we need to disable it while we are loading
			bool tmp = Flags . LinkviewerRecords;

			Flags . BankDbEditor = this;
			// Set window to TOPMOST
			OntopChkbox . IsChecked = true;
			this . Topmost = true;
			this . Focus ( );
			// Reset linkage setting
			Flags . LinkviewerRecords = tmp;
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
			// start our linkage monitor
			t1 = new Thread ( checkLinkages );
			t1 . IsBackground = true;
			t1 . Priority = ThreadPriority . Lowest;
			t1 . Start ( );
			Startup = false;
		}

		private void EventControl_EditIndexChanged ( object sender, IndexChangedArgs e )
		{
			Triggered = true;
			// Handle Selection change in another windowif linkage is ON
			if ( IsEditing )
			{
				//IsEditing = false;
				return;
			}
			this . BankGrid . SelectedIndex = e . Row;
			bindex = e . Row;
			this . BankGrid . Refresh ( );
			Triggered = false;
		}

		private async void EventControl_DataUpdated ( object sender, LoadedEventArgs e )
		{
			if ( e . CallerDb == "BANKDBVIEW" || e . CallerDb == "BANKACCOUNT" ) return;
			int currsel = this . BankGrid . SelectedIndex;
			Debug . WriteLine ( $"BankDbView : Data changed event notification received successfully." );
			this . BankGrid . ItemsSource = null;
			this . BankGrid . Items . Clear ( );
			Mouse . OverrideCursor = Cursors . Wait;
			BankViewcollection = await BankCollection . LoadBank ( BankViewcollection, "BANKDBVIEW", 3, true );
			this . BankGrid . ItemsSource = BankViewcollection;
			this . BankGrid . Refresh ( );
			IsDirty = false;
		}

		public void ExternalDataUpdate ( int DbEditChangeType, int row, string currentDb )
		{
			// Reciiving Notifiaction from a remote viewer that data has been changed, so we MUST now update our DataGrid
			Debug . WriteLine ( $"BankDbView : Data changed event notification received successfully." );
			this . BankGrid . ItemsSource = null;
			this . BankGrid . Items . Clear ( );
			this . BankGrid . ItemsSource = BankViewcollection;
			this . BankGrid . Refresh ( );
		}
		#endregion Startup/ Closedown


		#region DATA EDIT CONTROL METHODS
		/// <summary>
		///  DATA EDIT CONTROL METHODS
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BankGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
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
				bvmCurrent = CellEditControl . BankGrid_EditStart ( bvmCurrent, e );
			}
			// doesn't work right now - returns NULL
			//string str = CellEditControl.GetSelectedCellValue ( this . BankGrid );
		}

		/// <summary>
		/// does nothing at all because it is called whenver any single cell is exited
		///     and not just when ENTER is hit to save any changes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void BankGrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			if ( bvmCurrent == null ) return;

			// Has Data been changed in one of our rows. ?
			BankAccountViewModel dvm = this . BankGrid . SelectedItem as BankAccountViewModel;
			dvm = e . Row . Item as BankAccountViewModel;

			// The sequence of these next 2 blocks is critical !!!
			//if we get here, make sure we have been NOT been told to EsCAPE out
			//	this is a DataGridEditAction dgea
			if ( e . EditAction == 0 )
			{
				// ENTER was hit, so data has been saved - go ahead and reload our grid with new data
				// and this will notify any other open viewers as well
				bvmCurrent = null;
				await BankCollection . LoadBank ( BankViewcollection, "BANKDBVIEW", 1, true );
				return;
			}

			if ( CellEditControl . BankGrid_EditEnding ( bvmCurrent, BankGrid, e ) == false )
			{       // No change made
				return;
			}
		}

		/// <summary>
		/// Compares 2 rows of BANKACCOUNT or DETAILS data to see if there are any changes
		/// </summary>
		/// <param name="ss"></param>
		/// <returns></returns>
		private bool CompareDataContent ( BankAccountViewModel ss )
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
		private async void ViewerGrid_RowEditEnding ( object sender, System . Windows . Controls . DataGridRowEditEndingEventArgs e )
		{
			int currow = 0;
			// if our saved row is null, it has already been checked in Cell_EndDedit processing
			// and found no changes have been made, so we can abort this update
			if ( bvmCurrent == null ) return;

			// This is now confirmed as being CHANGED DATA in the current row
			// So we proceed and update SQL Db's' and notify all open viewers as well
			BankAccountViewModel ss = new BankAccountViewModel ( );
			ss = this . BankGrid . SelectedItem as BankAccountViewModel;
			SQLHandlers sqlh = new SQLHandlers ( );
			await sqlh . UpdateDbRowAsync ( "BANKACCOUNT", ss, this . BankGrid . SelectedIndex );

			this . BankGrid . SelectedIndex = currow;
			this . BankGrid . SelectedItem = currow;
			Utils . SetUpGridSelection ( this . BankGrid, currow );
			IsDirty = false;
			// Notify EditDb to upgrade its grid
			if ( Flags . CurrentEditDbViewer != null )
				Flags . CurrentEditDbViewer . UpdateGrid ( "BANKACCOUNT" );

			// ***********  DEFINITE WIN  **********
			// This DOES trigger a notification to SQLDBVIEWER AND OTHERS for sure !!!   14/5/21
			EventControl . TriggerViewerDataUpdated ( BankViewcollection,
				new LoadedEventArgs
				{
					CallerType = "BANKDBVIEW",
					CallerDb = "BANKACCOUNT",
					DataSource = BankViewcollection,
					RowCount = this . BankGrid . SelectedIndex
				} );

		}

		#endregion DATA EDIT CONTROL METHODS

		private async void EventControl_BankDataLoaded ( object sender, LoadedEventArgs e )
		{
			// Event handler for BankDataLoaded
			if ( e . DataSource == null ) return;
			// ONLY proceeed if we triggered the new data request
			if ( e . CallerDb != "BANKDBVIEW" ) return;
			this . BankGrid . ItemsSource = null;
			this . BankGrid . Items . Clear ( );
			LoadingDbData = true;
			BankviewerView = CollectionViewSource . GetDefaultView ( e . DataSource as BankCollection );
			BankViewcollection = e . DataSource as BankCollection;
			BankviewerView . Refresh ( );
			this . BankGrid . ItemsSource = BankviewerView;
			this . BankGrid . SelectedIndex = bindex;
			this . BankGrid . SelectedItem = bindex;
			this . BankGrid . CurrentItem = bindex;
			this . BankGrid . UpdateLayout ( );
			this . BankGrid . Refresh ( );
			Utils . SetUpGridSelection ( BankGrid, bindex );
			Mouse . OverrideCursor = Cursors . Arrow;
			Thread . Sleep ( 250 );
			DataFields . Refresh ( );
			Count . Text = $"{this . BankGrid . SelectedIndex} / { this . BankGrid . Items . Count . ToString ( )}";
			IsDirty = false;
			this . BankGrid . SelectedItem = bindex;
			this . BankGrid . CurrentItem = bindex;
			this . BankGrid . Focus ( );
			DataFields . Refresh ( );
			Debug . WriteLine ( "BANKDBVIEW : Bank Data fully loaded" );
		}


		private void ShowBank_KeyDown ( object sender, System . Windows . Input . KeyEventArgs e )
		{

		}

		private void Close_Click ( object sender, RoutedEventArgs e )
		{
			Close ( );
		}

		private void Window_Closing ( object sender, System . ComponentModel . CancelEventArgs e )
		{
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
			Flags . BankDbEditor = null;
			EventControl . EditIndexChanged -= EventControl_EditIndexChanged;
			// A Multiviewer has changed the current index 
			EventControl . MultiViewerIndexChanged -= EventControl_EditIndexChanged;
			// Another SqlDbviewer has changed the current index 
			EventControl . ViewerIndexChanged -= EventControl_EditIndexChanged;      // Callback in THIS FILE
			EventControl . ViewerDataUpdated -= EventControl_DataUpdated;
			EventControl . BankDataLoaded -= EventControl_BankDataLoaded;
			DataFields . DataContext = this . BankGrid . SelectedItem;
			BankViewcollection = null;
			Utils . SaveProperty ( "BankDbView_bindex", bindex . ToString ( ) );
		}

		private void BankGrid_SelectionChanged ( object sender, System . Windows . Controls . SelectionChangedEventArgs e )
		{
			if ( LoadingDbData )
			{
				LoadingDbData = false;
				return;
			}
			bindex = this . BankGrid . SelectedIndex;
			Utils . SetUpGridSelection ( this . BankGrid, this . BankGrid . SelectedIndex );
			Startup = true;
			DataFields . DataContext = this . BankGrid . SelectedItem;
			if ( Flags . LinkviewerRecords && Triggered == false )
			{
				//				Debug . WriteLine ( $" 4-1 *** TRACE *** BANKDBVIEW : BankGrid_SelectionChanged  BANKACCOUNT - Sending TriggerEditDbIndexChanged Event trigger" );
				TriggerViewerIndexChanged ( this . BankGrid );
			}

			// check to see if an SqlDbViewer has been opened that we can link to
			if ( Flags . SqlBankViewer != null && LinkToParent . IsEnabled == false )
			{
				LinkToParent . IsEnabled = true;
				SqlParentViewer = Flags . SqlBankViewer;
			}
			//else if ( Flags . SqlBankViewer == null )
			//{
			//	if ( LinkToParent . IsEnabled )
			//	{
			//		LinkToParent . IsEnabled = false;
			//		LinkToParent . IsChecked = false;
			//		LinktoParent = false;
			//		SqlParentViewer = null;
			//	}
			//}

			// Only  do this if global link is OFF
			if ( LinktoParent )// && LinkRecords . IsChecked == false )
			{
				// update parents row selection
				string bankno = "";
				string custno = "";
				var dvm = this . BankGrid . SelectedItem as BankAccountViewModel;
				if ( dvm == null ) return;

				if ( SqlParentViewer != null )
				{
					int rec = Utils . FindMatchingRecord ( dvm . CustNo, dvm . BankNo, SqlParentViewer . BankGrid, "BANKACCOUNT" );
					SqlParentViewer . BankGrid . SelectedIndex = rec;
					Utils . SetUpGridSelection ( SqlParentViewer . BankGrid, rec );
				}
				else if ( MultiParentViewer != null )
				{
					int rec = Utils . FindMatchingRecord ( dvm . CustNo, dvm . BankNo, MultiParentViewer . BankGrid, "BANKACCOUNT" );
					MultiParentViewer . BankGrid . SelectedIndex = rec;
					Utils . SetUpGridSelection ( MultiParentViewer . BankGrid, rec );
				}
			}
			if ( LinktoMultiParent )
			{
				Flags . SqlMultiViewer . BankGrid . SelectedIndex = this . BankGrid . SelectedIndex;
				Flags . SqlMultiViewer . BankGrid . ScrollIntoView ( this . BankGrid . SelectedIndex );
				Utils . SetUpGridSelection ( Flags . SqlMultiViewer . BankGrid, this . BankGrid . SelectedIndex );
			}

			Count . Text = $"{this . BankGrid . SelectedIndex} / { this . BankGrid . Items . Count . ToString ( )}";

			IsDirty = false;
			Startup = false;
		}

		private async Task<bool> SaveButton ( object sender = null, RoutedEventArgs e = null )
		{
			//inprogress = true;
			//bindex = this . BankGrid . SelectedIndex;
			//cindex = this . CustomerGrid . SelectedIndex;
			//dindex = this . DetailsGrid . SelectedIndex;

			// Get the current rows data
			IsDirty = false;
			int CurrentSelection = this . BankGrid . SelectedIndex;
			this . BankGrid . SelectedItem = this . BankGrid . SelectedIndex;
			BankAccountViewModel bvm = new BankAccountViewModel ( );
			bvm = this . BankGrid . SelectedItem as BankAccountViewModel;
			if ( bvm == null ) return false;

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
			await sqlh . UpdateDbRow ( "BANKACCOUNT", bvm );

			BankCollection bank = new BankCollection ( );
			BankViewcollection = await bank . ReLoadBankData ( );
			this . BankGrid . ItemsSource = null;
			this . BankGrid . ItemsSource = BankViewcollection;
			this . BankGrid . Refresh ( );

			//			Debug . WriteLine ( $" 4-3 *** TRACE *** BANKDBVIEW : SaveButton BANKACCOUNT - Sending TriggerBankDataLoaded Event trigger" );
			//			SendDataChanged ( null, this . BankGrid, "BANKACCOUNT" );

			EventControl . TriggerViewerDataUpdated ( BankViewcollection,
				new LoadedEventArgs
				{
					CallerType = "BANKDBVIEW",
					CallerDb = "BANKACCOUNT",
					DataSource = BankViewcollection,
					RowCount = this . BankGrid . SelectedIndex
				} );

			//Gotta reload our data because the update clears it down totally to null
			this . BankGrid . SelectedIndex = CurrentSelection;
			this . BankGrid . SelectedItem = CurrentSelection;
			this . BankGrid . Refresh ( );

			SaveBttn . IsEnabled = false;
			IsDirty = false;
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
			return;
			if ( !Startup ) CompareFieldData ( );
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

		private void LinkRecords_Click ( object sender, RoutedEventArgs e )
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
			if ( Flags . DetDbEditor != null )
				Flags . DetDbEditor . LinkRecords . IsChecked = Flags . LinkviewerRecords;
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
					LinkToParent . IsEnabled = true;
				else
					LinkToParent . IsEnabled = false;
			}
		}
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
			if ( grid . ItemsSource == null ) return;
			BankAccountViewModel CurrentBankSelectedRecord = grid . SelectedItem as BankAccountViewModel;
			SearchCustNo = CurrentBankSelectedRecord . CustNo;
			SearchBankNo = CurrentBankSelectedRecord . BankNo;
			EventControl . TriggerViewerIndexChanged ( this,
				new IndexChangedArgs
				{
					Senderviewer = this,
					Bankno = SearchBankNo,
					Custno = SearchCustNo,
					dGrid = grid,
					Sender = "BANKACCOUNT",
					SenderId = "BANKDBVIEW",
					Row = grid . SelectedIndex
				} );
		}

		private void BankGrid_PreviewMouseLeftButtondown ( object sender, MouseButtonEventArgs e )
		{
			// Make sure the left mouse button is pressed down so we are really moving a record
			if ( e . LeftButton == MouseButtonState . Pressed )
			{
				IsLeftButtonDown = true;
			}
		}

		private void BankGrid_PreviewMouseMove ( object sender, MouseEventArgs e )
		{
			if ( IsLeftButtonDown && e . LeftButton == MouseButtonState . Pressed )
			{
				if ( BankGrid . SelectedItem != null )
				{
					// We are dragging from the DETAILS grid
					//Working string version
					BankAccountViewModel bvm = new BankAccountViewModel ( );
					bvm = BankGrid . SelectedItem as BankAccountViewModel;
					string str = GetExportRecords . CreateTextFromRecord ( bvm, null, null, true, false );
					string dataFormat = DataFormats . Text;
					DataObject dataObject = new DataObject ( dataFormat, str );
					DragDrop . DoDragDrop (
					BankGrid,
					dataObject,
					DragDropEffects . Move );
					IsLeftButtonDown = false;
				}
			}
		}

		#region Menu items

		private void Linq1_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in BankViewcollection
					   where ( items . AcType == 1 )
					   orderby items . CustNo
					   select items;
			this . BankGrid . ItemsSource = bankaccounts;
		}
		private void Linq2_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in BankViewcollection
					   where ( items . AcType == 2 )
					   orderby items . CustNo
					   select items;
			this . BankGrid . ItemsSource = bankaccounts;
		}
		private void Linq3_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in BankViewcollection
					   where ( items . AcType == 3 )
					   orderby items . CustNo
					   select items;
			this . BankGrid . ItemsSource = bankaccounts;
		}
		private void Linq4_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in BankViewcollection
					   where ( items . AcType == 4 )
					   orderby items . CustNo
					   select items;
			this . BankGrid . ItemsSource = bankaccounts;
		}
		private void Linq5_Click ( object sender, RoutedEventArgs e )
		{
			//select All the items first;			
			var bankaccounts = from items in BankViewcollection orderby items . CustNo, items . AcType select items;
			//Next Group BankAccountViewModel collection on Custno
			var grouped = bankaccounts . GroupBy (
				b => b . CustNo );

			//Now filter content down to only those a/c's with multiple Bank A/c's
			var sel = from g in grouped
				  where g . Count ( ) > 1
				  select g;

			// Finally, iterate thru the list of grouped CustNo's matching to CustNo in the full Bankaccounts data
			// giving us ONLY the full records for any recordss that have > 1 Bank accounts
			List<BankAccountViewModel> output = new List<BankAccountViewModel> ( );
			foreach ( var item1 in sel )
			{
				foreach ( var item2 in bankaccounts )
				{
					if ( item2 . CustNo . ToString ( ) == item1 . Key )
					{
						output . Add ( item2 );
					}
				}
			}
			this . BankGrid . ItemsSource = output;
		}
		private void Linq6_Click ( object sender, RoutedEventArgs e )
		{
			var accounts = from items in BankViewcollection orderby items . CustNo, items . AcType select items;
			this . BankGrid . ItemsSource = accounts;
		}

		private void Filter_Click ( object sender, RoutedEventArgs e )
		{
			// Show Filter system
			MessageBox . Show ( "Filter dialog will appear here !!" );
		}

		private void Exit_Click ( object sender, RoutedEventArgs e )
		{
			Close ( );
		}

		private void Options_Click ( object sender, RoutedEventArgs e )
		{

		}
		#endregion Menu items



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

		private async void BankGrid_PreviewMouseRightButtonDown ( object sender, MouseButtonEventArgs e )
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

				//If data has been changed, update everywhere
				// Update the row on return in case it has been changed
				if ( rip . IsDirty )
				{
					this . BankGrid . ItemsSource = null;
					this . BankGrid . Items . Clear ( );
					await BankCollection . LoadBank ( BankViewcollection, "BANKDBVIEW", 1, true );
					this . BankGrid . ItemsSource = BankviewerView;
					// Notify everyone else of the data change
					EventControl . TriggerViewerDataUpdated ( BankviewerView,
						new LoadedEventArgs
						{
							CallerType = "BANKBVIEW",
							CallerDb = "BANKACCOUNT",
							DataSource = BankviewerView,
							RowCount = this . BankGrid . SelectedIndex
						} );
				}
				else
					this . BankGrid . SelectedItem = RowData . Item;

				// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
				Utils . SetUpGridSelection ( this . BankGrid, row );
				Count . Text = $"{this . BankGrid . SelectedIndex} / { this . BankGrid . Items . Count . ToString ( )}";
				// This is essential to get selection activated again
				this . BankGrid . Focus ( );
			}

		}

		private void Edit_LostFocus ( object sender, RoutedEventArgs e )
		{
			IsDirty = true;
			SaveBttn . IsEnabled = true;
		}
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

		/// <summary>
		/// Key handling to allow proper tabbing between data Editing fieds
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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
			return Flags . SqlBankViewer != null && ParentLinkTo == false;
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
					SqlParentViewer = Flags . SqlBankViewer;
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

		}

		private void Minimize_click ( object sender, RoutedEventArgs e )
		{
			this . WindowState = WindowState . Normal;
		}

		//			BankAccountViewModel bank = new BankAccountViewModel();
		//			var filtered = from bank inBankViewercollection . Where ( x => bank . CustNo = "1055033" ) select x;
		//		   GroupBy bank.CustNo having count(*) > 1
		//where  
		//having COUNT (*) > 1
		//	select bank;
		//	Where ( b.CustNo = "1055033") ;
		/*
					commandline = $"SELECT * FROM BANKACCOUNT WHERE CUSTNO IN "
			+ $"(SELECT CUSTNO FROM BANKACCOUNT "
			+ $" GROUP BY CUSTNO"
			+ $" HAVING COUNT(*) > 1) ORDER BY ";

		    */


	}

}
