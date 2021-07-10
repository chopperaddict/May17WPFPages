using System;
using System . Collections . Generic;
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
	/// Interaction logic for CustDbView.xaml
	/// </summary>
	public partial class CustDbView : Window
	{
		public static CustCollection CustDbViewcollection = null;// = new CustCollection ( );//. CustViewerDbcollection;
									 // Get our personal Collection view of the Db
		public ICollectionView CustviewerView { get; set; }

		// Crucial structure for use when a Grid row is being edited
		private static CustRowData cvmCurrent = null;

		private bool IsDirty = false;
		private bool Startup = true;
		private bool Triggered = false;
		private static bool LinktoParent = false;
		private static bool LinktoMultiParent = false;
		private bool LoadingDbData = false;
		private bool IsEditing { get; set; }
		private bool keyshifted { get; set; }
		private static Point _startPoint { get; set; }
		public static int cindex { get; set; }
		private bool  IsLeftButtonDown { get; set; }
		private string _bankno = "";
		private string _custno = "";
		private string _actype = "";
		private string _balance = "";
		private string _odate = "";
		private string _cdate = "";
		private SqlDbViewer SqlParentViewer;
		private MultiViewer MultiParentViewer;
		private Thread t1;

		public CustDbView ( )
		{
			Startup = true;
			InitializeComponent ( );
			//Identify individual windows for update protection
			this.Tag = (Guid)Guid.NewGuid();
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
			Mouse . OverrideCursor = Cursors . Wait;
			this . Show ( );
			this . Refresh ( );
			Startup = true;

			string ndx = ( string ) Properties . Settings . Default [ "CustDbView_cindex" ];
			cindex = int . Parse ( ndx );
			this . CustGrid . SelectedIndex = cindex < 0 ? 0 : cindex;

			//			this . MouseDown += delegate { DoDragMove ( ); };
			Utils.SetupWindowDrag(this);
			// An EditDb has changed the current index
			EventControl. EditIndexChanged += EventControl_EditIndexChanged;
			// A Multiviewer has changed the current index
			EventControl . MultiViewerIndexChanged += EventControl_EditIndexChanged;
			// Another viewer has changed the current index
			EventControl . ViewerIndexChanged += EventControl_EditIndexChanged;      // Callback in THIS FILE
			EventControl . ViewerDataUpdated += EventControl_DataUpdated;
			EventControl . CustDataLoaded += EventControl_CustDataLoaded;


			EventControl.GlobalDataChanged += EventControl_GlobalDataChanged;


			await CustCollection . LoadCust ( CustDbViewcollection, "CUSTDBVIEW", 3, true );

			SaveBttn . IsEnabled = false;
			// Save linkage setting as we need to disable it while we are loading
			bool tmp = Flags . LinkviewerRecords;

			Flags . CustDbEditor = this;
			// Set window to TOPMOST
			OntopChkbox . IsChecked = true;
			this . Topmost = true;
			this . Focus ( );
			// start our linkage monitor
			t1 = new Thread ( checkLinkages );
			t1 . IsBackground = true;
			t1 . Priority = ThreadPriority . Lowest;
			t1 . Start ( );
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

			Mouse . OverrideCursor = Cursors . Arrow;
			Startup = false;
		}
		private void EventControl_GlobalDataChanged(object sender, GlobalEventArgs e)
		{
			if (e.CallerType == "CUSTDBVIEWER" )
				return;
			//Update our own data tyoe only
			CustCollection.LoadCust(null, "CUSTOMER", 2, true);

		}

		private void EventControl_EditIndexChanged ( object sender, IndexChangedArgs e )
		{
			Triggered = true;
			// Handle selecton change if linkage is ON

			if ( IsEditing )
			{
				//IsEditing = false;
				return;
			}
			this . CustGrid . SelectedIndex = e . Row;
			cindex = e . Row;
			this . CustGrid . Refresh ( );
			Triggered = false;
		}

		private async void EventControl_DataUpdated ( object sender, LoadedEventArgs e )
		{
			Debug . WriteLine ( $"CustDbView : Data changed event notification received successfully." );
			if ( e . CallerType == "CUSTDBVIEW" || e . CallerDb == "CUSTOMER" ) return;
			int currsel = this . CustGrid . SelectedIndex;
			this . CustGrid . ItemsSource = null;
			this . CustGrid . Items . Clear ( );
			Mouse . OverrideCursor = Cursors . Wait;
			CustDbViewcollection = await CustCollection . LoadCust ( CustDbViewcollection, "CUSTDBVIEW", 3, true );
			IsDirty = false;
		}

		public void ExternalDataUpdate ( int DbEditChangeType, int row, string currentDb )
		{
			// Reciiving Notifiaction from a remote viewer that data has been changed, so we MUST now update our DataGrid
			Debug . WriteLine ( $"CUSTDBVIEW : Data changed event notification received successfully. in ExternalDataUpdate(187)" );
			this . CustGrid . ItemsSource = null;
			this . CustGrid . Items . Clear ( );
			this . CustGrid . ItemsSource = CustDbViewcollection;
			this . CustGrid . Refresh ( );
		}
		#endregion Startup/ Closedown


		private void CustGrid_BeginningEdit ( object sender, DataGridBeginningEditEventArgs e )
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
		}
		private async void CustGrid_CellEditEnding ( object sender, DataGridCellEditEndingEventArgs e )
		{
			if ( cvmCurrent == null ) return;

			// Has Data been changed in one of our rows. ?
			CustomerViewModel cvm = this . CustGrid . SelectedItem as CustomerViewModel;
			cvm = e . Row . Item as CustomerViewModel;

			// The sequence of these next 2 blocks is critical !!!
			//if we get here, make sure we have been NOT been told to EsCAPE out
			//	this is a DataGridEditAction dgea
			if ( e . EditAction == DataGridEditAction . Cancel )
			{
				// ENTER was hit, so data has been saved - go ahead and reload our grid with new data
				// and this will notify any other open viewers as well
				cvmCurrent = null;
				await CustCollection . LoadCust ( CustDbViewcollection, "CUSTDBVIEW", 2, true );
				return;
			}

			if ( CellEditControl . CustGrid_EditEnding ( cvmCurrent, CustGrid, e ) == false )
			{       // No change made
				//	cvmCurrent = null;
				return;
			}
			IsEditing = false;

		}
		private async void ViewerGrid_RowEditEnding ( object sender, System . Windows . Controls . DataGridRowEditEndingEventArgs e )
		{
			// Save changes and tell other viewers about the change
			int currow = 0;
			currow = this . CustGrid . SelectedIndex;
			cindex = currow;
			// Save changes and tell other viewers about the change
			// if our saved row is null, it has already been checked in Cell_EndDedit processing
			// and found no changes have been made, so we can abort this update
			if ( cvmCurrent == null )
			{
				this . CustGrid . Refresh ( );
				return;
			}
			// Save current row so we can reposition correctly at end of the entire refresh process
			//			Flags . SqlCustCurrentIndex = currow;
			CustomerViewModel ss = new CustomerViewModel ( );
			ss = this . CustGrid . SelectedItem as CustomerViewModel;
			// This is the NEW DATA from the current row
			SQLHandlers sqlh = new SQLHandlers ( );
			await sqlh . UpdateDbRowAsync ( "CUSTOMER", ss, this . CustGrid . SelectedIndex );

			this . CustGrid . SelectedIndex = currow;
			this . CustGrid . SelectedItem = currow;
			Utils . SetUpGridSelection ( this . CustGrid, currow );
			// Notify EditDb to upgrade its grid
			if ( Flags . CurrentEditDbViewer != null )
				Flags . CurrentEditDbViewer . UpdateGrid ( "CUSTOMER" );

			// ***********  DEFINITE WIN  **********
			// This DOES trigger a notidfication to SQLDBVIEWER for sure !!!   14/5/21
			EventControl . TriggerViewerDataUpdated ( CustDbViewcollection,
				new LoadedEventArgs
				{
					CallerType = "CUSTDBVIEW",
					CallerDb = "CUSTOMER",
					DataSource = CustDbViewcollection,
					SenderGuid = this.Tag.ToString(),
					RowCount = this . CustGrid . SelectedIndex
				} );
			EventControl.TriggerGlobalDataChanged(this, new GlobalEventArgs
			{
				CallerType = "CUSTDBVIEW",
				AccountType = "CUSTOMER",
				SenderGuid = this.Tag?.ToString()
			});
		}

		private async void EventControl_CustDataLoaded ( object sender, LoadedEventArgs e )
		{
			// Event handler for CustDataLoaded
			// ONLY proceeed if we triggered the new data request
			if ( e . CallerDb != "CUSTDBVIEW" ) return;
			this . CustGrid . ItemsSource = null;
			this . CustGrid . Items . Clear ( );
			if ( e . DataSource == null ) return;

			LoadingDbData = true;

			CustviewerView = CollectionViewSource . GetDefaultView ( e . DataSource as CustCollection );
			CustDbViewcollection = e . DataSource as CustCollection;
			CustviewerView . Refresh ( );
			this . CustGrid . ItemsSource = CustviewerView;
			this . CustGrid . SelectedIndex = cindex;
			this . CustGrid . SelectedItem = cindex;
			this . CustGrid . CurrentItem = cindex;
			this . CustGrid . UpdateLayout ( );
			Utils . SetUpGridSelection ( CustGrid, cindex );
			bool reslt = false;

			Thread . Sleep ( 250 );
			DataFields . Refresh ( );
			Count . Text = $"{this . CustGrid . SelectedIndex} / { this . CustGrid . Items . Count . ToString ( )}";
			this . CustGrid . Refresh ( );
			Debug . WriteLine ( "BANKDBVIEW : Customer Data fully loaded" );
		}

		private void ShowCust_KeyDown ( object sender, System . Windows . Input . KeyEventArgs e )
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
			t1 . Abort ( null );
			Flags . CustDbEditor = null;
			EventControl . EditIndexChanged -= EventControl_EditIndexChanged;
			// A Multiviewer has changed the current index
			EventControl . MultiViewerIndexChanged -= EventControl_EditIndexChanged;
			// Another SqlDbviewer has changed the current index
			EventControl . ViewerIndexChanged -= EventControl_EditIndexChanged;      // Callback in THIS FILE
			EventControl . ViewerDataUpdated -= EventControl_DataUpdated;
			EventControl . CustDataLoaded -= EventControl_CustDataLoaded;

			EventControl.GlobalDataChanged -= EventControl_GlobalDataChanged;

			Utils. SaveProperty ( "CustDbView_cindex", cindex . ToString ( ) );
		}

		private void CustGrid_SelectionChanged ( object sender, System . Windows . Controls . SelectionChangedEventArgs e )
		{
			if ( LoadingDbData )
			{
				LoadingDbData = false;
				return;
			}
			IsDirty = false;
			if ( this . CustGrid . SelectedItem == null )
				return;
			Utils . SetUpGridSelection ( this . CustGrid, this . CustGrid . SelectedIndex );
			cindex = this . CustGrid . SelectedIndex;
			Startup = true;
			DataFields . DataContext = this . CustGrid . SelectedItem;
			if ( Flags . LinkviewerRecords && Triggered == false )
			{
				//				Debug . WriteLine ( $" 7-1 *** TRACE *** CUSTDBVIEWER : Itemsview_OnSelectionChanged  CUSTOMER - Sending TriggerEditDbIndexChanged Event trigger" );
				TriggerViewerIndexChanged ( this . CustGrid );
			}

			// check to see if an SqlDbViewer has been opened that we can link to
			if ( Flags . SqlCustViewer != null && LinkToParent . IsEnabled == false )
			{
				LinkToParent . IsEnabled = true;
				SqlParentViewer = Flags . SqlCustViewer;
			}
			//else if ( Flags . SqlCustViewer == null )
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
			if ( LinktoParent )
			{
				// update parents row selection
				string bankno = "";
				string custno = "";
				var dvm = this . CustGrid . SelectedItem as CustomerViewModel;
				if ( dvm == null ) return;

				if ( SqlParentViewer != null )
				{
					int rec = Utils . FindMatchingRecord ( dvm . CustNo, dvm . BankNo, SqlParentViewer . CustomerGrid, "CUSTOMER" );
					SqlParentViewer . CustomerGrid . SelectedIndex = rec;
					Utils . SetUpGridSelection ( SqlParentViewer . CustomerGrid, rec );
				}
				else if ( MultiParentViewer != null )
				{
					int rec = Utils . FindMatchingRecord ( dvm . CustNo, dvm . BankNo, MultiParentViewer . CustomerGrid, "CUSTOMER" );
					MultiParentViewer . CustomerGrid . SelectedIndex = rec;
					Utils . SetUpGridSelection ( MultiParentViewer . CustomerGrid, rec );
				}
			}
			else if ( LinktoMultiParent )
			{
				Flags . SqlMultiViewer . CustomerGrid . SelectedIndex = this . CustGrid . SelectedIndex;
				Flags . SqlMultiViewer . CustomerGrid . ScrollIntoView ( this . CustGrid . SelectedIndex );
				Utils . SetUpGridSelection ( Flags . SqlMultiViewer . CustomerGrid, this . CustGrid . SelectedIndex );
			}

			IsDirty = false;
			Count . Text = $"{this . CustGrid . SelectedIndex} / { this . CustGrid . Items . Count . ToString ( )}";
			Triggered = false;
		}

		private async Task<bool> SaveButton ( object sender = null, RoutedEventArgs e = null )
		{
			//inprogress = true;
			//bindex = this . CustGrid . SelectedIndex;
			//cindex = this . CustomerGrid . SelectedIndex;
			//dindex = this . DetailsGrid . SelectedIndex;

			// Get the current rows data
			IsDirty = false;
			int CurrentSelection = this . CustGrid . SelectedIndex;
			this . CustGrid . SelectedItem = this . CustGrid . SelectedIndex;
			CustomerViewModel cvm = new CustomerViewModel ( );
			cvm = this . CustGrid . SelectedItem as CustomerViewModel;

			SaveFieldData ( );

			// update the current rows data content to send  to Update process
			cvm . BankNo = Bankno . Text;
			cvm . CustNo = Custno . Text;
			cvm . AcType = Convert . ToInt32 ( acType . Text );
			//	cvm . Balance = Convert . ToDecimal ( balance . Text );
			cvm . ODate = Convert . ToDateTime ( odate . Text );
			cvm . CDate = Convert . ToDateTime ( cdate . Text );
			// Call Handler to update ALL Db's via SQL
			SQLHandlers sqlh = new SQLHandlers ( );
			await sqlh . UpdateDbRow ( "CUSTOMER", cvm );

			EventControl . TriggerViewerDataUpdated ( CustDbViewcollection,
				new LoadedEventArgs
				{
					CallerType = "CUSTDBVIEW",
					CallerDb = "CUSTOMER",
					DataSource = CustDbViewcollection,
					SenderGuid = this.Tag.ToString(),
					RowCount = this . CustGrid . SelectedIndex
				} );
			EventControl.TriggerGlobalDataChanged(this, new GlobalEventArgs
			{
				CallerType = "CUSTDBVIEW",
				AccountType = "CUSTOMER",
				SenderGuid = this.Tag?.ToString()
			});

			//Gotta reload our data because the update clears it down totally to null
			this. CustGrid . SelectedIndex = CurrentSelection;
			this . CustGrid . SelectedItem = CurrentSelection;
			this . CustGrid . Refresh ( );

			SaveBttn . IsEnabled = false;
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
		{ if ( !Startup ) CompareFieldData ( ); }

		private void SaveFieldData ( )
		{
			_bankno = Bankno . Text;
			_custno = Custno . Text;
			_actype = acType . Text;
			//			_balance = balance . Text;
			_odate = odate . Text;
			_cdate = cdate . Text;
		}
		private void CompareFieldData ( )
		{
			if ( SaveBttn == null )
				return;
			SaveBttn . IsEnabled = false; ;
			if ( _bankno != Bankno . Text )
				SaveBttn . IsEnabled = true;
			if ( _custno != Custno . Text )
				SaveBttn . IsEnabled = true;
			if ( _actype != acType . Text )
				SaveBttn . IsEnabled = true;
			//if ( _balance != balance . Text )
			//	SaveBttn . IsEnabled = true;
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
		public void TriggerViewerIndexChanged ( System . Windows . Controls . DataGrid grid )
		{
			string SearchCustNo = "";
			string SearchBankNo = "";
			CustomerViewModel CurrentCustSelectedRecord = this . CustGrid . SelectedItem as CustomerViewModel;
			if ( CurrentCustSelectedRecord == null ) return;
			SearchCustNo = CurrentCustSelectedRecord . CustNo;
			SearchBankNo = CurrentCustSelectedRecord . BankNo;
			EventControl . TriggerViewerIndexChanged ( this,
			new IndexChangedArgs
			{
				Senderviewer = this,
				Bankno = SearchBankNo,
				Custno = SearchCustNo,
				dGrid = this . CustGrid,
				Sender = "CUSTOMER",
				SenderId = "CUSTDBVIEW",
				Row = this . CustGrid . SelectedIndex
			} );
		}

		#region Menu items

		private void Linq1_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in CustDbViewcollection
					   where ( items . AcType == 1 )
					   orderby items . CustNo
					   select items;
			this . CustGrid . ItemsSource = bankaccounts;
		}
		private void Linq2_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in CustDbViewcollection
					   where ( items . AcType == 2 )
					   orderby items . CustNo
					   select items;
			this . CustGrid . ItemsSource = bankaccounts;
		}
		private void Linq3_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in CustDbViewcollection
					   where ( items . AcType == 3 )
					   orderby items . CustNo
					   select items;
			this . CustGrid . ItemsSource = bankaccounts;
		}
		private void Linq4_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var bankaccounts = from items in CustDbViewcollection
					   where ( items . AcType == 4 )
					   orderby items . CustNo
					   select items;
			this . CustGrid . ItemsSource = bankaccounts;
		}
		private void Linq5_Click ( object sender, RoutedEventArgs e )
		{
			//select All the items first;
			var bankaccounts = from items in CustDbViewcollection orderby items . CustNo, items . AcType select items;
			//Next Group BankAccountViewModel collection on Custno
			var grouped = bankaccounts . GroupBy (
				b => b . CustNo );

			//Now filter content down to only those a/c's with multiple Bank A/c's
			var sel = from g in grouped
				  where g . Count ( ) > 1
				  select g;

			// Finally, iterate thru the list of grouped CustNo's matching to CustNo in the full Bankaccounts data
			// giving us ONLY the full records for any recordss that have > 1 Bank accounts
			List<CustomerViewModel> output = new List<CustomerViewModel> ( );
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
			this . CustGrid . ItemsSource = output;
		}
		private void Linq6_Click ( object sender, RoutedEventArgs e )
		{
			var accounts = from items in CustDbViewcollection orderby items . CustNo, items . AcType select items;
			this . CustGrid . ItemsSource = accounts;
		}

		private void Filter_Click ( object sender, RoutedEventArgs e )
		{
			// Show Filter system
			System . Windows . MessageBox . Show ( "Filter dialog will appear here !!" );
		}

		#endregion Menu items

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
		private void Edit_LostFocus ( object sender, RoutedEventArgs e )
		{
			IsDirty = true;
			SaveBttn . IsEnabled = true;
		}


		#region KEYHANDLER for EDIT fields
		// These let us tab thtorugh the editfields back and forward correctly
		private void Window_PreviewKeyUp ( object sender, KeyEventArgs e )
		{

			if ( e . Key == Key . RightShift || e . Key == Key . LeftShift )
			{
				keyshifted = false;
				return;
			}

			if ( keyshifted && ( e . Key == Key . RightShift || e . Key == Key . LeftShift ) )
			{
				//keyshifted = false;
				e . Handled = true;
				return;
			}

		}

		private void Window_PreviewKeyDown ( object sender, KeyEventArgs e )
		{
			if ( e . Key == Key . RightShift || e . Key == Key . LeftShift )
			{
				keyshifted = true;
				e . Handled = true;
				return;
			}
			if ( keyshifted && ( e . Key == Key . RightShift || e . Key == Key . LeftShift ) )
			{
				keyshifted = false;
				e . Handled = true;
				return;
			}

			if ( keyshifted == false )
			{
				// NO SHIFT KEY - KEY DOWN
				if ( e . Key == Key . Tab && e . Source == odate )
				{
					e . Handled = true;
					cdate . Focus ( );
					return;
				}
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
					//keyshifted = false;
					return;
				}
				if ( e . Key == Key . Tab && e . Source == Custno )
				{
					e . Handled = true;
					cdate . Focus ( );
					return;
				}
			}
		}

		#endregion KEYHANDLER for EDIT fields

		#region HANDLERS for linkage checkboxes, inluding Thread montior
		static bool IsLinkActive ( bool ParentLinkTo )
		{
			return Flags . SqlCustViewer != null && ParentLinkTo == false;
		}

		static bool IsMultiLinkActive ( bool MultiParentLinkTo )
		{
			if ( Flags . SqlMultiViewer == null )
				return false;
			else
				return true;
		}
		private void CustGrid_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			// Gotta make sure it is not anywhere in the Scrollbar we clicked on 
			if ( Utils . HitTestScrollBar ( sender, e ) ) return;
			if ( Utils . HitTestHeaderBar ( sender, e ) ) return;
			_startPoint = e . GetPosition ( null );
			// Make sure the left mouse button is pressed down so we are really moving a record
			if ( e . LeftButton == MouseButtonState . Pressed )
			{
				IsLeftButtonDown = true;
			}
		}
		private void CustGrid_PreviewMouseMove ( object sender, MouseEventArgs e )
		{
			Point mousePos = e.GetPosition(null);
			Vector diff = _startPoint - mousePos;

			if (e.LeftButton == MouseButtonState.Pressed &&
			    Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
			    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
			{
				if (IsLeftButtonDown && e.LeftButton == MouseButtonState.Pressed)
				{
					if (CustGrid.SelectedItem != null)
					{
						// We are dragging from the DETAILS grid
						//Working string version
						CustomerViewModel cvm = new CustomerViewModel();
						cvm = CustGrid.SelectedItem as CustomerViewModel;
						string str = GetExportRecords.CreateTextFromRecord(null, null, cvm, true, false);
						string dataFormat = DataFormats.Text;
						DataObject dataObject = new DataObject(dataFormat, str);
						System.Windows.DragDrop.DoDragDrop(
						CustGrid,
						dataObject,
						DragDropEffects.Move);
						IsLeftButtonDown = false;
					}
				}
			}
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
				try
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
				catch (Exception ex)
				{
					Debug . WriteLine ("Auto link sensor crashed....");
				}
			}
		}
		private void ResetLinkages ( string linktype, bool value )
		{
			if ( linktype == "LINKTOPARENT" )
			{
				LinkToParent . IsEnabled = value;
				if ( value )
					SqlParentViewer = Flags . SqlCustViewer;
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
		}


		#endregion HANDLERS for linkage checkboxes, inluding Thread montior
		private async void CustGrid_PreviewMouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{
			ContextMenu cm = this . FindResource ( "ContextMenu1" ) as ContextMenu;
			cm . PlacementTarget = this . CustGrid as DataGrid;
			cm . IsOpen = true;
		}

		private void CustGrid_DragEnter ( object sender, DragEventArgs e )
		{
			e . Effects = ( DragDropEffects ) DragDropEffects . Move;
		}

		private void ContextClose_Click ( object sender, RoutedEventArgs e )
		{
			Close ( );
		}

		private void ContextSave_Click ( object sender, RoutedEventArgs e )
		{

		}

		private async void ContextEdit_Click ( object sender, RoutedEventArgs e )
		{
			CustomerViewModel cvm = new CustomerViewModel ( );
			int currsel = 0;
			DataGridRow RowData = new DataGridRow ( );
			cvm = this . CustGrid . SelectedItem as CustomerViewModel;
			currsel = this . CustGrid . SelectedIndex;
			RowInfoPopup rip = new RowInfoPopup ( "CUSTOMER", CustGrid );
			rip . Topmost = true;
			rip . DataContext = RowData;
			rip . BringIntoView ( );
			rip . Focus ( );
			rip . ShowDialog ( );

			//If data has been changed, update everywhere
			// Update the row on return in case it has been changed
			if ( rip . IsDirty )
			{
				this . CustGrid . ItemsSource = null;
				this . CustGrid . Items . Clear ( );
				await CustCollection . LoadCust( CustDbViewcollection, "CUSTDBVIEW", 1, true );
				this . CustGrid . ItemsSource = CustviewerView;
				// Notify everyone else of the data change
				EventControl . TriggerViewerDataUpdated ( CustviewerView,
					new LoadedEventArgs
					{
						CallerType = "CUSTBVIEW",
						CallerDb = "CUSTOMER",
						DataSource = CustviewerView,
						SenderGuid = this.Tag.ToString(),
						RowCount = this . CustGrid . SelectedIndex
					} );
				EventControl.TriggerGlobalDataChanged(this, new GlobalEventArgs
				{
					CallerType = "CUSTDBVIEW",
					AccountType = "CUSTOMER",
					SenderGuid = this.Tag?.ToString()
				});
			}
			else
				this . CustGrid . SelectedItem = RowData . Item;

			// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
			this . CustGrid . SelectedIndex = currsel;
			Count . Text = $"{this . CustGrid . SelectedIndex} / { this . CustGrid . Items . Count . ToString ( )}";
			// This is essential to get selection activated again
			this . CustGrid . Focus ( );

		}

		private void ContextSettings_Click ( object sender, RoutedEventArgs e )
		{
			Setup setup = new Setup ( );
			setup . Show ( );
			setup . BringIntoView ( );
			setup . Topmost = true;
			this . Focus ( );
		}

		private void ContextDisplayJsonData_Click ( object sender, RoutedEventArgs e )
		{
			JsonSupport . CreateShowJsonText (false, "CUSTOMER", CustDbViewcollection);
		}

		private void ContextShowJson_Click ( object sender, RoutedEventArgs e )
		{
			//============================================//
			//MENU ITEM 'Read and display JSON File'
			//============================================//
			string Output = "";
			this.Refresh();
			////We need to save current Collectionview as a Json (binary) data to disk
			//// this is the best way to save persistent data in Json format
			////using tmp folder for interim file that we will then display
			CustomerViewModel bvm = this.CustGrid.SelectedItem as CustomerViewModel;
			Output = JsonSupport.CreateShowJsonText(true, "CUSTOMER", bvm, "CustomerViewModel");
			MessageBox.Show(Output, "Currently selected record in JSON format", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
		}

		private void ViewJsonRecord_Click(object sender, RoutedEventArgs e)
		{
			//============================================//
			//MENU ITEM 'Read and display JSON File'
			//============================================//
			string Output = "";
			this.Refresh();
			////We need to save current Collectionview as a Json (binary) data to disk
			//// this is the best way to save persistent data in Json format
			////using tmp folder for interim file that we will then display
			CustomerViewModel bvm = this.CustGrid.SelectedItem as CustomerViewModel;
			Output = JsonSupport.CreateShowJsonText(true, "CUSTOMER", bvm, "CustomerViewModel");
			MessageBox.Show(Output, "Currently selected record in JSON format", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
		}
	}
}

/*			BankAccountViewModel bank = new BankAccountViewModel();
//			var filtered = from bank inCustViewerDbcollection . Where ( x => bank . CustNo = "1055033" ) select x;
//		   GroupBy bank.CustNo having count(*) > 1
//where
//having COUNT (*) > 1
//	select bank;
//	Where ( b.CustNo = "1055033") ;


			commandline = $"SELECT * FROM BANKACCOUNT WHERE CUSTNO IN "
	+ $"(SELECT CUSTNO FROM BANKACCOUNT "
	+ $" GROUP BY CUSTNO"
	+ $" HAVING COUNT(*) > 1) ORDER BY ";

    */


