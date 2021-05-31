using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Imaging;
using System . Windows . Shapes;
using WPFPages . ViewModels;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for DetailsDbView.xaml
	/// </summary>
	public partial class DetailsDbView : Window
	{
		public DetCollection DetViewerDbcollection = null;// = new DetCollection ( );//. DetViewerDbcollection;
		private bool IsDirty = false;
		private bool Startup = true;
		private bool LinktoParent = false;
		public static bool Triggered = false;
		private string _bankno = "";
		private string _custno = "";
		private string _actype = "";
		private string _balance = "";
		private string _odate = "";
		private string _cdate = "";
		private SqlDbViewer SqlParentViewer;
		private MultiViewer MultiParentViewer;
		public DataChangeArgs dca = new DataChangeArgs ( );

		public DetailsDbView ( )
		{
			Startup = true;
			InitializeComponent ( );
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

			dca . SenderName = sender . ToString ( );
			this . MouseDown += delegate { DoDragMove ( ); };
			// An EditDb has changed the current index 
			EventControl . EditIndexChanged += EventControl_EditIndexChanged;
			// A Multiviewer has changed the current index 
			EventControl . MultiViewerIndexChanged += EventControl_EditIndexChanged;
			// Another viewer has changed the current index 
			EventControl . ViewerIndexChanged += EventControl_EditIndexChanged;      // Callback in THIS FILE
			EventControl . ViewerDataUpdated += EventControl_DataUpdated;
			EventControl . DetDataLoaded += EventControl_DetDataLoaded;

			await DetCollection . LoadDet ( DetViewerDbcollection ,2, true);

			SaveBttn . IsEnabled = false;
			// Save linkage setting as we need to disable it while we are loading
			bool tmp = Flags . LinkviewerRecords;
			if ( Flags . LinkviewerRecords )
				LinkRecords . IsChecked = true;
			Flags . DetDbEditor = this;
			// Set window to TOPMOST
			OntopChkbox . IsChecked = true;
			this . Topmost = true;
			this . Focus ( );
			// Reset linkage setting
			Flags . LinkviewerRecords = tmp;
			LinktoParent = false;
			if ( sender . GetType ( ) == typeof ( SqlDbViewer ) )
			{
				MultiParentViewer = null;
				if ( sender . GetType ( ) == typeof ( SqlDbViewer ) )
				{
					SqlParentViewer = sender as SqlDbViewer;
				}
				else
				{
					if ( Flags . SqlCustViewer != null )
						SqlParentViewer = Flags . SqlCustViewer;
					else
					{
						LinktoParent = false;
						LinkToParent . IsEnabled = false;
					}
				}
			}
			else if ( sender . GetType ( ) == typeof ( MultiViewer ) )
			{
				SqlParentViewer = null;
				if ( sender . GetType ( ) == typeof ( MultiViewer ) )
				{

					MultiParentViewer = sender as MultiViewer;
					//LinktoParent = true;
					LinkToParent . IsEnabled = true;
				}
				else
				{
					if ( Flags . MultiViewer != null )
					{
						MultiParentViewer = Flags . MultiViewer;
						//LinktoParent = true;
						LinkToParent . IsEnabled = true;
					}
					else
					{
						//LinktoParent = false;
						LinkToParent . IsEnabled = false;
					}
				}
			}
			else
			{
				MultiParentViewer = null;
				SqlParentViewer = null;
				LinktoParent = false;
				LinkToParent . IsEnabled = false;

				if ( Flags . SqlDetViewer != null )
				{
					SqlParentViewer = Flags . SqlDetViewer;
//					LinktoParent = true;
					LinkToParent . IsEnabled = true;
					LinkToParent . Content = "Link to \nSqlViewer";
				}
				else if ( Flags . MultiViewer != null )
				{
					MultiParentViewer = Flags . MultiViewer;
//					LinktoParent = true;
					LinkToParent . IsEnabled = true;
					LinkToParent . Content = "Link to \nMultiViewer";
				}
				else
				{
//					LinktoParent = false;
					LinkToParent . IsEnabled = false;
				}
			}

			this . DetGrid . SelectedIndex = 0;
			Startup = false;
		}

		private void EventControl_EditIndexChanged ( object sender, IndexChangedArgs e )
		{
			// Handle selection change by other windows if linkage is ON
			Triggered = true;
			this . DetGrid . SelectedIndex = e . Row;
			this . DetGrid . Refresh ( );
			Triggered = false;
		}

		private async void EventControl_DataUpdated ( object sender, LoadedEventArgs e )
		{
			// Receiving Notification from a remote viewer that data has been changed, so we MUST now update our DataGrid
			Debug . WriteLine ( $"BankDbView : Data changed event notification received successfully." );
			int currsel = this . DetGrid . SelectedIndex;

			this . DetGrid . ItemsSource = null;
			this . DetGrid . Items . Clear ( );
			Mouse . OverrideCursor = Cursors . Wait;
			DetViewerDbcollection = await DetCollection . LoadDet ( DetViewerDbcollection, 2, true );
			this . DetGrid . ItemsSource = DetViewerDbcollection;
			this . DetGrid . Refresh ( );
			this . DetGrid . SelectedIndex = currsel;
		}

		public void ExternalDataUpdate ( int DbEditChangeType, int row, string currentDb )
		{
			// Receiving Notification from a remote viewer that data has been changed, so we MUST now update our DataGrid
			Debug . WriteLine ( $"BankDbView : Data changed event notification received successfully." );
			DetGrid . ItemsSource = null;
			this . DetGrid . Items . Clear ( );
			this . DetGrid . ItemsSource = DetViewerDbcollection;
			this . DetGrid . Refresh ( );
		}
		#endregion Startup/ Closedown



		private void Where ( bool v )
		{
			throw new NotImplementedException ( );
		}

		private void ViewerGrid_RowEditEnding ( object sender, System . Windows . Controls . DataGridRowEditEndingEventArgs e )
		{
			// Save changes and tell other viewers about the change
			int currow = 0;
			currow = this . DetGrid . SelectedIndex;
			// Save current row so we can reposition correctly at end of the entire refresh process					
			DetailsViewModel ss = new DetailsViewModel ( );
			ss = this . DetGrid . SelectedItem as DetailsViewModel;
			// This is the NEW DATA from the current row
			SQLHandlers sqlh = new SQLHandlers ( );
			sqlh . UpdateDbRowAsync ( "DETAILS", ss, this . DetGrid . SelectedIndex );

			this . DetGrid . SelectedIndex = currow;
			this . DetGrid . SelectedItem = currow;
			Utils . SetUpGridSelection ( this . DetGrid, this . DetGrid . SelectedIndex );
			// Notify EditDb to upgrade its grid
			if ( Flags . CurrentEditDbViewer != null )
				Flags . CurrentEditDbViewer . UpdateGrid ( "DETAILS" );

			// ***********  DEFINITE WIN  **********
			// This DOES trigger a notification to SQLDBVIEWER for sure !!!   14/5/21
			EventControl . TriggerDetDataLoaded ( DetViewerDbcollection,
				new LoadedEventArgs
				{
					CallerDb = "DETAILS",
					DataSource = DetViewerDbcollection,
					RowCount = this . DetGrid . SelectedIndex
				} );
		}

		private async void EventControl_DetDataLoaded ( object sender, LoadedEventArgs e )
		{
			// Event handler for BankDataLoaded
			if ( e . DataSource == null ) return;
			this . DetGrid . ItemsSource = null;
			this . DetGrid . Items . Clear ( );
			DetViewerDbcollection = e . DataSource as DetCollection;
			this . DetGrid . ItemsSource = DetViewerDbcollection;
			this . DetGrid . SelectedIndex = 0;
			this . DetGrid . SelectedItem = 0;
			Mouse . OverrideCursor = Cursors . Arrow;
			this . DetGrid . Refresh ( );
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
			//			EventControl . ViewerDataHasBeenChanged -= ExternalDataUpdate;      // Callback in THIS FILE
			//UnSubscribe from Bank Data Changed event declared in EventControl
			Flags . DetDbEditor = null;
			//Another viewer has changed selection
			EventControl . EditIndexChanged -= EventControl_EditIndexChanged;
			// A Multiviewer has changed the current index 
			EventControl . MultiViewerIndexChanged -= EventControl_EditIndexChanged;
			// Another SqlDbviewer has changed the current index 
			EventControl . ViewerIndexChanged -= EventControl_EditIndexChanged;      // Callback in THIS FILE
			 // Main update notification handler
//			EventControl . DataUpdated -= EventControl_DataUpdated;
			EventControl . ViewerDataUpdated -= EventControl_DataUpdated;
			EventControl . DetDataLoaded -= EventControl_DetDataLoaded;


		}

		private void DetGrid_SelectionChanged ( object sender, System . Windows . Controls . SelectionChangedEventArgs e )
		{
			if ( IsDirty )
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
			if ( this . DetGrid . SelectedItem == null )
				return;
			// This sets up the selected Index/Item and scrollintoview in one easy FUNC function call (GridInitialSetup is  the FUNC name)
			Utils . SetUpGridSelection ( this . DetGrid, this . DetGrid . SelectedIndex );
			Startup = true;
			DataFields . DataContext = this . DetGrid . SelectedItem;

			if ( Flags . LinkviewerRecords && Triggered == false )
			{
				//				Debug . WriteLine ( $" 6-1 *** TRACE *** DETAILSDBVIEWER : Itemsview_OnSelectionChanged  DETAILS - Sending TriggerEditDbIndexChanged Event trigger" );
				TriggerViewerIndexChanged ( this . DetGrid );
			}
			// Only  do this if global link is OFF
			if ( LinktoParent  )
			{
				// update parents row selection
				string bankno = "";
				string custno = "";
				var dvm = this . DetGrid . SelectedItem as DetailsViewModel;

				if ( SqlParentViewer != null )
				{
					int rec = Utils . FindMatchingRecord ( dvm . CustNo, dvm . BankNo, SqlParentViewer . DetailsGrid, "DETAILS" );
					SqlParentViewer . DetailsGrid . SelectedIndex = rec;
					Utils . SetUpGridSelection ( SqlParentViewer . DetailsGrid, rec );
				}
				else if ( MultiParentViewer != null )
				{
					int rec = Utils . FindMatchingRecord ( dvm . CustNo, dvm . BankNo, MultiParentViewer . DetailsGrid, "DETAILS" );
					MultiParentViewer . DetailsGrid . SelectedIndex = rec;
					Utils . SetUpGridSelection ( MultiParentViewer . DetailsGrid, rec );
				}
			}
			Triggered = false;
		}

		private async Task<bool> SaveButton ( object sender = null, RoutedEventArgs e = null )
		{
			// Get the current rows data
			IsDirty = false;
			int CurrentSelection = this . DetGrid . SelectedIndex;
			this . DetGrid . SelectedItem = this . DetGrid . SelectedIndex;
			DetailsViewModel bvm = new DetailsViewModel ( );
			bvm = this . DetGrid . SelectedItem as DetailsViewModel;
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
			await sqlh . UpdateDbRow ( "DETAILS", bvm );

			EventControl . TriggerDetDataLoaded ( DetViewerDbcollection,
				new LoadedEventArgs
				{
					CallerDb = "DETAILS",
					DataSource = DetViewerDbcollection,
					RowCount = this . DetGrid . SelectedIndex
				} );

			//Gotta reload our data because the update clears it down totally to null
			this . DetGrid . SelectedIndex = CurrentSelection;
			this . DetGrid . SelectedItem = CurrentSelection;
			this . DetGrid . Refresh ( );

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

		private async void MultiAccts_Click ( object sender, RoutedEventArgs e )
		{
			// Filter data to show ONLY Custoimers with multiple bank accounts

			if ( MultiAccounts . Content != "Show All" )
			{
				int currsel = this . DetGrid . SelectedIndex;
				DetailsViewModel bgr = this . DetGrid . SelectedItem as DetailsViewModel;
				Flags . IsMultiMode = true;
				DetCollection det = new DetCollection ( );
				det = await DetCollection . LoadDet ( DetViewerDbcollection, 2, true );
				this . DetGrid . ItemsSource = null;
				this . DetGrid . ItemsSource = det;
				this . DetGrid . Refresh ( );

				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				MultiAccounts . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBorderBrushRed" );
				MultiAccounts . Background = br;
				MultiAccounts . Content = "Show All";
				Count . Text = this . DetGrid . Items . Count . ToString ( );

				// Get Custno from ACTIVE gridso we can find it in other grids
				MultiViewer mv = new MultiViewer ( );
				int rec = Utils . FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . DetGrid, "DETAILS" );
				this . DetGrid . SelectedIndex = currsel;
				if ( rec >= 0 )
					this . DetGrid . SelectedIndex = rec;
				else
					this . DetGrid . SelectedIndex = 0;
				Utils . SetUpGridSelection ( this . DetGrid, this . DetGrid . SelectedIndex );
			}
			else
			{
				Flags . IsMultiMode = false;
				int currsel = this . DetGrid . SelectedIndex;
				DetailsViewModel bgr = this . DetGrid . SelectedItem as DetailsViewModel;

				DetCollection det = new DetCollection ( );
				det = await DetCollection . LoadDet ( DetViewerDbcollection, 2, true );
				// Just reset our iremssource to man Db
				this . DetGrid . ItemsSource = null;
				this . DetGrid . ItemsSource = DetViewerDbcollection;
				this . DetGrid . Refresh ( );

				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGreen" );
				MultiAccounts . Template = tmp;
				Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGreen" );
				MultiAccounts . Background = br;
				MultiAccounts . Content = "Multi Accounts";
				Count . Text = this . DetGrid . Items . Count . ToString ( );

				MultiViewer mv = new MultiViewer ( );
				int rec = Utils . FindMatchingRecord ( bgr . CustNo, bgr . BankNo, this . DetGrid, "DETAILS" );
				this . DetGrid . SelectedIndex = 0;

				if ( rec >= 0 )
					this . DetGrid . SelectedIndex = rec;
				else
					this . DetGrid . SelectedIndex = 0;
				Utils . SetUpGridSelection ( this . DetGrid, this . DetGrid . SelectedIndex );
			}
		}
		public void SendDataChanged ( SqlDbViewer o, DataGrid Grid, string dbName )
		{
			// Databases have DEFINITELY been updated successfully after a change
			// We Now Broadcast this to ALL OTHER OPEN VIEWERS here and now

			EventControl . TriggerDetDataLoaded ( DetViewerDbcollection,
			new LoadedEventArgs
			{
				CallerDb = "DETAILS",
				DataSource = DetViewerDbcollection,
				RowCount = this . DetGrid . SelectedIndex
			} );
			Mouse . OverrideCursor = Cursors . Arrow;
		}
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
			if ( Flags . CustDbEditor != null )
				Flags . CustDbEditor . LinkRecords . IsChecked = Flags . LinkviewerRecords;
			if ( Flags . BankDbEditor != null )
				Flags . BankDbEditor . LinkRecords . IsChecked = Flags . LinkviewerRecords;
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
				LinktoParent = false;

			}
		}

		/// <summary>
		/// Link record selection to parent SQL viewer window only
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LinkToParent_Click ( object sender, RoutedEventArgs e )
		{
			LinktoParent = ! LinktoParent;
		}

		#region Menu items

		private void Linq1_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var accounts = from items in DetViewerDbcollection
				       where ( items . AcType == 1 )
				       orderby items . CustNo
				       select items;
			this . DetGrid . ItemsSource = accounts;
		}
		private void Linq2_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var accounts = from items in DetViewerDbcollection
				       where ( items . AcType == 2 )
				       orderby items . CustNo
				       select items;
			this . DetGrid . ItemsSource = accounts;
		}
		private void Linq3_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var accounts = from items in DetViewerDbcollection
				       where ( items . AcType == 3 )
				       orderby items . CustNo
				       select items;
			this . DetGrid . ItemsSource = accounts;
		}
		private void Linq4_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			var accounts = from items in DetViewerDbcollection
				       where ( items . AcType == 4 )
				       orderby items . CustNo
				       select items;
			this . DetGrid . ItemsSource = accounts;
		}
		private void Linq5_Click ( object sender, RoutedEventArgs e )
		{
			//select All the items first;			
			var accounts = from items in DetViewerDbcollection orderby items . CustNo, items . AcType select items;
			//Next Group BankAccountViewModel collection on Custno
			var grouped = accounts . GroupBy (
				b => b . CustNo );

			//Now filter content down to only those a/c's with multiple Bank A/c's
			var sel = from g in grouped
				  where g . Count ( ) > 1
				  select g;

			// Finally, iterate thru the list of grouped CustNo's matching to CustNo in the full accountsdata
			// giving us ONLY the full records for any recordss that have > 1 Bank accounts
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
			this . DetGrid . ItemsSource = output;
		}
		private void Linq6_Click ( object sender, RoutedEventArgs e )
		{
			var accounts = from items in DetViewerDbcollection orderby items . CustNo, items . AcType select items;
			this . DetGrid . ItemsSource = accounts;
		}

		private void Filter_Click ( object sender, RoutedEventArgs e )
		{
			// Show Filter system
			MessageBox . Show ( "Filter dialog will appear here !!" );
		}
		#endregion Menu items
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
			DetailsViewModel CurrentDetSelectedRecord = grid . SelectedItem as DetailsViewModel;
			SearchCustNo = CurrentDetSelectedRecord . CustNo;
			SearchBankNo = CurrentDetSelectedRecord . BankNo;
			EventControl . TriggerViewerIndexChanged ( this,
				new IndexChangedArgs
				{
					Senderviewer = null,
					Bankno = SearchBankNo,
					Custno = SearchCustNo,
					dGrid = grid,
					Sender = "DETAILS",
					Row = grid . SelectedIndex
				} );
		}

		private void Exit_Click ( object sender, RoutedEventArgs e )
		{
			Close ( );
		}

		private void Options_Click ( object sender, RoutedEventArgs e )
		{

		}

	}
}

/*
//			BankAccountViewModel bank = new BankAccountViewModel();
//			var filtered = from bank inDetViewercollection . Where ( x => bank . CustNo = "1055033" ) select x;
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


