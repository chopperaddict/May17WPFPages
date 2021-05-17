using System;
using System . Collections . Generic;
using System . ComponentModel;
using System . Diagnostics;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Input;
using System . Windows . Media;

using WPFPages . ViewModels;

using static WPFPages . SqlDbViewer;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for DbSelector.xaml
	/// </summary>
	///

	public static class ApplicationState
	{
		private static SqlDbViewer viewer;

		public static SqlDbViewer Viewer
		{
			get { return viewer; }
			set { viewer = value; }
		}
	}

	public partial class DbSelector : Window, INotifyPropertyChanged
	{
		private static BankAccountViewModel bvm = MainWindow . bvm;
		private static CustomerViewModel cvm = MainWindow . cvm;
		private static DetailsViewModel dvm = MainWindow . dvm;
		public BankCollection Bankcollection;

		public int selection = 0;
		private int CurrentList = -1;
		private static bool key1 = false;


		//*********************DELEGATE STUFF **************************************************//
		// I can now just use this : SendViewerCommand(x, "") to send messages to SqlDbViewer window
		//		EVent trigger for public delegate void NotifyViewer ( int status, string info, SqlDbViewer NewSqlViewer );
		//		public NotifyViewer SendViewerCommand = null;
		//*********************DELEGATE STUFF **************************************************//

		#region Receive  notifications into MyNotification() from SqlViewer - WORKS JUST FINE

		//*****************************************************//
		//DELEGATE in use is : Notifyviewer

		/// <summary>
		///  Handler  for delegate notification FROM SqlDbViewer
		///  Message SENT
		///  100 - Tell Dbs to command this  to start SQL Data loading
		///  Messgaes Received
		///  25 - I can command data loading
		///  102 - Mthod starting
		///  103 - Method ended
		///  111 - General report received
		/// </summary>
		public static void MyNotification ( int status , string info , SqlDbViewer NewSqlViewer )
		{
			switch ( status )
			{
				case 102:       // Starting a method
					Console . WriteLine ( $"DBSELECTOR NOTIFICATION : {status}  [{info}]" );
					break;

				case 103:       // Ending a process
					Console . WriteLine ( $"DBSELECTOR NOTIFICATION: {status}  [{info}]" );
					break;

				case 111:       // Info reports
					Console . WriteLine ( $"DBSELECTOR NOTIFICATION : [{status}] - [{info}]" );
					break;

				default:
//					Console . WriteLine ( $"DBSELECTOR NOTIFICATION : [{status}], [{info}]" );
					break;
			}

			//if ( status == 25 )
			//{
			//	// VERY IMPORTANT MSG - Send 100 Command to Tell Viewer to load data
			//	Console . WriteLine ( $"\r\nDBSELECTOR COMMAND : [{status}] -  Calling InitialLoad() to load data in SqlDbv\r\n" );
			//	EventHandlers . SendViewerCommand ( 100 , $"{info}" , null );
			//}
			if ( status == 99 )
			{
				Console . WriteLine ( $"\r\nDBSELECTOR NOTIFICATION : Received [{status}]  - Window is closing down\r\n" );
			}
			else if ( status == 100 )
			{
				Console . WriteLine ( $"\r\nDBSELECTOR NOTIFICATION : Received TEST SIGNAL {status} from SqlDbViewer\r\n" );
			}
			else if ( status == 101 )
			{
#pragma This is the one that works well
				// info contains the text to be added to the Viewers ListBox
				Console . WriteLine ( $"\r\nDBSELECTOR - Received request [{status}] to Add Viewer to Current Viewers List.\r\n" );
				Flags . SqlViewerIsLoading = true;
				DbSelector . AddViewerToList ( info , NewSqlViewer , -1 );
				Flags . SqlViewerIsLoading = false;
				Console . WriteLine ( $"\r\nDBSELECTOR - Viewer ADDED to List of Current Viewers \r\n" );
			}
		}

		//Constructor
		public DbSelector ( )
		{
			InitializeComponent ( );
			//			MainWindow.dbs = this;
			if ( ViewersList . Items . Count > 2 )
			{// ignore the dummy blank entry line
				ViewersList . SelectedIndex = 2;
				ViewersList . SelectedItem = 2;
			}
			sqlSelector . SelectedIndex = 2;
			sqlSelector . Focus ( );
			// Assign Handler to delegate SqlViewerNotify
//			SqlViewerNotify notifier = DbSelectorMessage;

//			EventHandlers . SendViewerCommand = SqlDbViewer . DbSelectorMessage;
			this . MouseDown += delegate { DoDragMove ( ); };
			//This DOES send a message to SqlDbViewer !!
//			EventHandlers . SendViewerCommand ( 103 , "<<< Completed DbSelector basic Constructor" , null );
			Utils . GetWindowHandles ( );
			OntopChkbox . IsChecked = false;
			this . Topmost = false;
		}

		public static ListBox listbox;
		public static int selected;
		public static string Command;
		//		public static  object  MyDispatcher;

		public void SetFocusToExistingViewer ( Guid guid )
		{
			for ( int x = 0 ; x < MainWindow . gv . MaxViewers ; x++ )
			{
				if ( MainWindow . gv . ListBoxId [ x ] == guid )
				{
					MainWindow . gv . window [ x ] . Focus ( );
					MainWindow . gv . window [ x ] . BringIntoView ( );
					break;
				}
			}
		}

		public void ClearClosingViewer ( string CallerDb , Guid guid )
		{
			for ( int x = 0 ; x < MainWindow . gv . MaxViewers ; x++ )
			{
				if ( MainWindow . gv . ListBoxId [ x ] == guid )
				{
					if ( CallerDb == "BANKACCOUNT" )
						MainWindow . gv . Bankviewer = Guid . Empty;
					else if ( CallerDb == "CUSTOMER" )
						MainWindow . gv . Custviewer = Guid . Empty;
					else if ( CallerDb == "DETAILS" )
						MainWindow . gv . Detviewer = Guid . Empty;
					break;
				}
			}
		}

		//********************************************************************************************//
		public static void UpdateControlFlags ( SqlDbViewer caller , string callertype , string PrettyString )
		{
			int x = 0;
			// We are starting up a new viewer, so need to create the flags structure
			// Get the first empty set of structures  and fill them out ofr this NEW Viewer Window
			for ( x = 0 ; x < 3 ; x++ )
			{
				// inserted here 29/4/21 to clear Viewerslist when a viewer window closes
				if ( caller == null && callertype == null )
				{
					ListBoxItem lbi = new ListBoxItem ( );

					for ( int i = 1 ; i < MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count ; i++ )
					{
						if ( i >= MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count )
							return;
						if ( MainWindow . gv . PrettyDetails == PrettyString )
						{
							lbi = Flags . DbSelectorOpen . ViewersList . Items [ i ] as ListBoxItem;
							if ( lbi != null )
								lbi . Content = "";
							Flags . DbSelectorOpen . ViewersList . Items . RemoveAt ( i );
							Flags . DbSelectorOpen . ViewersList . Refresh ( );
							break;
						}
					}
					return;
				}
				if ( MainWindow . gv . ListBoxId [ x ] == Guid . Empty )
				{
					MainWindow . gv . CurrentDb [ x ] = callertype;
					MainWindow . gv . window [ x ] = Flags . CurrentSqlViewer;
					MainWindow . gv . PrettyDetails = PrettyString;

					if ( callertype == "BANKACCOUNT" )
					{
						MainWindow . gv . Datagrid [ x ] = Flags . SqlBankGrid;
						MainWindow . gv . Bankviewer = MainWindow . gv . ListBoxId [ x ] = ( Guid ) caller . Tag;
						MainWindow . gv . SqlBankViewer = caller;
					}
					else if ( callertype == "CUSTOMER" )
					{
						MainWindow . gv . Datagrid [ x ] = Flags . SqlCustGrid;
						MainWindow . gv . Custviewer = MainWindow . gv . ListBoxId [ x ] = ( Guid ) caller . Tag;
						MainWindow . gv . SqlCustViewer = caller;
					}
					else if ( callertype == "DETAILS" )
					{
						MainWindow . gv . Datagrid [ x ] = Flags . SqlDetGrid;
						MainWindow . gv . Detviewer = MainWindow . gv . ListBoxId [ x ] = ( Guid ) caller . Tag;
						MainWindow . gv . SqlDetViewer = caller;
					}

					MainWindow . gv . ViewerCount++;
						MainWindow . gv . SqlViewerWindow = caller;
						break;
				}
			}
		}

		private async void HandleSelection ( ListBox listbox , string Command )
		{
			// Called when Opening/ Closing/deleting a Db Viewer window
			//and most other functionality in this window (All buttons and double clicks)
			int selected = -1;
			int callertype = -1;
			string selectedItem = "";
			string CallingType = "";

			//if ( listbox . Items . Count == 4 && Command != "DELETE" )
			//{
			//	MessageBox . Show ( "there are already 3 viewers open.  Duplicates are not allowed" );
			//	return;
			//}
			selected = listbox . SelectedIndex;
			if ( Command == "NEW" )
			{
				selectedItem = listbox . SelectedItem . ToString ( );
//				BankCollection Bankcollection = new BankCollection    ();
				CustCollection Custcollection = new CustCollection    ();
				DetCollection Detcollection = new DetCollection    ();
				if ( selectedItem . ToUpper ( ) . Contains ( "MULTI BANK ACCOUNTS" ) )
				{
					// DETAILS DATABASE
					if ( MainWindow . gv . Detviewer != Guid . Empty )
					{
						SetFocusToExistingViewer ( MainWindow . gv . Detviewer );
						return;
					}
					callertype = 2;
					CallingType = "DETAILS";
					// LOADS THE WINDOW HERE - it RETURNS IMMEDIATELY even though the data is not yet fully loaded
					Flags . CurrentSqlViewer = new SqlDbViewer ( "DETAILS" , Detcollection );
					Flags . CurrentSqlViewer . BringIntoView ( );
					Flags . CurrentSqlViewer . Show ( );
//					ExtensionMethods . Refresh ( Flags . CurrentSqlViewer );
					//Window is visible & Data is loaded by here .....
					if ( ( Guid ) Flags . CurrentSqlViewer . Tag == null || ( Guid ) Flags . CurrentSqlViewer . Tag == Guid . Empty )
					{
						Flags . CurrentSqlViewer . Tag = Guid . NewGuid ( );
						MainWindow . gv . SqlViewerGuid = ( Guid ) Flags . CurrentSqlViewer . Tag;
						// This is fine, new windows do NOT have their Guid when they arrive here
						this . Tag = Flags . CurrentSqlViewer . Tag;
					}
					callertype = 2;
					CallingType = "DETAILS";
				}
				else if ( selectedItem . ToUpper ( ) . Contains ( "BANK ACCOUNTS" ) )
				{
					// BANK DATABASE
					if ( MainWindow . gv . Bankviewer != Guid . Empty )
					{
						SetFocusToExistingViewer ( MainWindow . gv . Bankviewer );
						return;
					}
					Flags . CurrentSqlViewer = new SqlDbViewer ( "BANKACCOUNT" , Bankcollection );
					Flags . CurrentSqlViewer . BringIntoView ( );
					Flags . CurrentSqlViewer . Show ( );
//					ExtensionMethods . Refresh ( Flags . CurrentSqlViewer );

					//Data is loaded by here .....
					if ( ( Guid ) Flags . CurrentSqlViewer . Tag == Guid . Empty || ( Guid ) Flags . CurrentSqlViewer . Tag == Guid . Empty )
					{
						Flags . CurrentSqlViewer . Tag = Guid . NewGuid ( );
						MainWindow . gv . SqlViewerGuid = ( Guid ) Flags . CurrentSqlViewer . Tag;

						// This is fine, new windows do NOT have their Guid when they arrive here
						this . Tag = Flags . CurrentSqlViewer . Tag;
					}
					callertype = 0;
					CallingType = "BANKACCOUNT";
				}
				else if ( selectedItem . ToUpper ( ) . Contains ( "CUSTOMER ACCOUNTS" ) )
				{
					// CUSTOMER DATABASE
					if ( MainWindow . gv . Custviewer != Guid . Empty )
					{
						SetFocusToExistingViewer ( MainWindow . gv . Custviewer );
						return;
					}
					
					Flags . CurrentSqlViewer = new SqlDbViewer ( "CUSTOMER" , Custcollection );
					Flags . CurrentSqlViewer . BringIntoView ( );
					Flags . CurrentSqlViewer . Show ( );
//					ExtensionMethods . Refresh ( Flags . CurrentSqlViewer );

					//Data is loaded by here .....
					if ( ( Guid ) Flags . CurrentSqlViewer . Tag == Guid . Empty || ( Guid ) Flags . CurrentSqlViewer . Tag == Guid . Empty )
					{
						Flags . CurrentSqlViewer . Tag = Guid . NewGuid ( );
						MainWindow . gv . SqlViewerGuid = ( Guid ) Flags . CurrentSqlViewer . Tag;

						// This is fine, new windows do NOT have their Guid when they arrive here
						this . Tag = Flags . CurrentSqlViewer . Tag;
					}
					callertype = 1;
					CallingType = "CUSTOMER";
				}
				//When loading a new viewer, the  MainWindow.gv structure is completed correctly !!!!
				//				MessageBox.Show ("DbSelector has completed the load");

				// LOAD THE VIEWERS LIST HERE TO AVOID ISSUES
				Flags . SqlViewerIsLoading = true;
				string s = DbSelector . AddViewerToList ( "", Flags . CurrentSqlViewer, callertype );
				//This call sets up all the Gridview gv[] variables and the related singleton pointers in the gv[] structure
				UpdateControlFlags ( Flags . CurrentSqlViewer , CallingType , s );
				Flags . SqlViewerIsLoading = false;
				//Set the viewer Delete one/All/Select buttons up correctly
				UpdateSelectorButtons ( );
			}
			else if ( Command == "DELETEALL" )
			{
				CloseDeleteAllViewers ( );
				//Set the viewer Delete one/All/Select buttons up correctly
				UpdateSelectorButtons ( );
			}
			else if ( Command == "DELETE" )
			{
//				Window win = null;
				//Close selected viewer window
#pragma TODO  - DOES NOT WORK
				DeleteCurrentViewer ( );
				//Handle Flags entries here  ?
				//				if ( Flags . CurrentSqlViewer == null )
				//				{
				//					win = SelectAnyOpenViewer ( );
				//				}
				Flags . CurrentSqlViewer . Close ( );
				//				Flags . CurrentSqlViewer . CloseViewer_Click ( null, null );
				//Set the viewer Delete one/All/Select buttons up correctly
				//				if ( win != null )
				//					win . Close ( );
				//				else
				UpdateSelectorButtons ( );
			}
			else if ( Command == "SELECT" )
			{
				ListBoxItem lbi = new ListBoxItem ( );
				Guid tag = Guid . Empty;
				lbi = listbox . SelectedItem as ListBoxItem;
				if ( lbi == null ) return;
				tag = ( Guid ) lbi . Tag;
				for ( int x = 0 ; x < ViewersList . Items . Count ; x++ )
				{
					if ( MainWindow . gv . ListBoxId [ x ] == tag )
					{
						MainWindow . gv . window [ x ] . Focus ( );
						MainWindow . gv . SqlViewerWindow = MainWindow . gv . window [ x ] as SqlDbViewer;
						//Ensure our global viewer pointer is set to last viewer selected
						Flags . CurrentSqlViewer = MainWindow . gv . window [ x ] as SqlDbViewer;
						break;
					}
				}
			}
		}

		private Window SelectAnyOpenViewer ( )
		{
			Window WinHandle = null;
			Guid tag = ( Guid . Empty );
			ListBoxItem lbi = new ListBoxItem ( );
			tag = ( Guid ) lbi . Tag;
			if ( tag == null )
				return null;
			for ( int x = 0 ; x < ViewersList . Items . Count ; x++ )
			{
				lbi = ViewersList . Items [ x ] as ListBoxItem;
				if ( ( Guid ) lbi . Tag != Guid . Empty )
				{
					WinHandle = GetWindowFromTag ( ( Guid ) lbi . Tag );
					break;
				}
			}
			return WinHandle;
		}

		private Window GetWindowFromTag ( Guid guid )
		{
			Window WinHandle = null;
			for ( int x = 0 ; x < MainWindow . gv . MaxViewers ; x++ )
			{
				if ( MainWindow . gv . ListBoxId [ x ] == guid )
				{
					WinHandle = MainWindow . gv . window [ x ] as Window;
					break;
				}
			}
			return WinHandle;
		}

		private void UpdateSelectorButtons ( )
		{
			int counter = 0;
			//Set default active item to 1st valid entry
			counter = ViewersList . Items . Count;
			if ( counter <= 1 )
			{
				ViewerDeleteAll . IsEnabled = false;
				ViewerDelete . IsEnabled = false;
				SelectViewerBtn . IsEnabled = false;
			}
			else
			{
				ViewersList . SelectedIndex = 1;
				if ( counter > 1 )
					ViewerDelete . IsEnabled = true;
				else
					ViewerDelete . IsEnabled = false;
				if ( counter > 2 )
					ViewerDeleteAll . IsEnabled = true;
				else
					ViewerDeleteAll . IsEnabled = false;
				SelectViewerBtn . IsEnabled = true;
			}
		}

		public void DeleteCurrentViewer ( )
		{
			//Remove a SINGLE Viewer Windows data from Flags & gv[]
			Flags . DeleteViewerAndFlags ( ViewersList . SelectedIndex );
			return;

			int nextWin = -1;
			ListBoxItem lbi = new ListBoxItem ( );
			Guid tag = Guid . Empty;
			lbi = ViewersList . SelectedItem as ListBoxItem;
			tag = ( Guid ) lbi . Tag;
			//First Close the Viewer window itself & update the control structure
			for ( int x = 0 ; x < MainWindow . gv . MaxViewers ; x++ )
			{
				if ( MainWindow . gv . ListBoxId [ x ] == tag )
				{
					//Clear relevant viewer type flag
					if ( MainWindow . gv . Bankviewer == MainWindow . gv . ListBoxId [ x ] )
						ClearClosingViewer ( "BANKACCOUNT" , MainWindow . gv . ListBoxId [ x ] );
					else if ( MainWindow . gv . Custviewer == MainWindow . gv . ListBoxId [ x ] )
						ClearClosingViewer ( "BANKACCOUNT" , MainWindow . gv . ListBoxId [ x ] );
					else if ( MainWindow . gv . Detviewer == MainWindow . gv . ListBoxId [ x ] )
						ClearClosingViewer ( "BANKACCOUNT" , MainWindow . gv . ListBoxId [ x ] );

					SqlDbViewer sqlv = MainWindow . gv . window [ x ] as SqlDbViewer;
					UpdateDataGridController ( tag );
					sqlv . Close ( );
					MainWindow . gv . SqlViewerWindow = null;
					break;
				}
			}
			Debug . WriteLine ( $"listbox count = {listbox . Items . Count} before Removeat() " );
			//Remove the listbox entry
			listbox . Items . RemoveAt ( selected );
			Debug . WriteLine ( $"listbox count = {listbox . Items . Count} after Removeat() " );
			Debug . WriteLine ( $"ViewersList count = {Flags . DbSelectorOpen . ViewersList . Items . Count} after Removeat() " );
			if ( Flags . DbSelectorOpen . ViewersList . Items . Count == 1 )
				Debug . WriteLine ( $"All Viewers have been closed and GridView structure is cleared..." );

			Flags . DbSelectorOpen . ViewersList . Refresh ( );
			//Now highlight first one in list if we have one
			if ( Flags . DbSelectorOpen . ViewersList . Items . Count > 1 )
			{
				bool success = false;
				if ( Flags . DbSelectorOpen . ViewersList . Items . Count == 1 )
				{
					Flags . CurrentSqlViewer . UpdateDbSelectorBtns ( Flags . CurrentSqlViewer );
					// Clear global Flags structure
					Flags . ClearGridviewControlStructure ( null , null );
					return;
				}
				else
				{
					for ( int x = 0 ; x < MainWindow . gv . ViewerCount ; x++ )
					{
						if ( MainWindow . gv . window [ x ] != null )
						{
							success = true;
							nextWin = x;
							break;
						}
					}
				}
				if ( success )
				{
					MainWindow . gv . window [ nextWin ] . Focus ( );
					MainWindow . gv . window [ nextWin ] . BringIntoView ( );
					MainWindow . gv . window [ nextWin ] . Refresh ( );
					MainWindow . gv . SqlViewerWindow = MainWindow . gv . window [ nextWin ] as SqlDbViewer;
					Flags . DbSelectorOpen . ViewersList . SelectedIndex = nextWin;
					Flags . CurrentSqlViewer . UpdateDbSelectorBtns ( Flags . CurrentSqlViewer );
					Flags . CurrentSqlViewer . Focus ( );
					Flags . CurrentSqlViewer . BringIntoView ( );
					ExtensionMethods . Refresh ( Flags . CurrentSqlViewer );
				}
				else if ( MainWindow . gv . ViewerCount == 1 )
				{
					//no more opeen viewers, so clear control ctructure entirely
					Flags . SetGridviewControlFlags ( Flags . CurrentSqlViewer , null );
				}
			}
			//Reset global flags
			//			EventHandlers . ClearWindowHandles ( null, Flags . CurrentSqlViewer );
		}

		public void CloseDeleteAllViewers ( )
		{
			//Close selected viewer window
			// iterate the list form bottom up closing windows
			// and clearing all array entries for this entry
			for ( int x = ViewersList . Items . Count - 1 ; x >= 1 ; x-- )
			{
				if ( MainWindow . gv . window [ x - 1 ] != null )
				{
					//Physically close the window itself
					MainWindow . gv . window [ x - 1 ] . Close ( );
				}
			}

			//Remove all Viewer Windows data from Flags & gv[]
			Flags . DeleteViewerAndFlags ( -1 );
			//UpdateDataGridController ( null );

			// This is done in the call above
			ViewersList . Refresh ( );
			//			MainWindow . gv . SqlViewerWindow = null;
			return;
		}

		/// <summary>
		/// updates the Grid Controller structure when a viewer closes
		/// </summary>
		/// <param name="tag"></param>
		public void UpdateDataGridController ( object Tag )
		{
			if ( Tag == null )
			{
				//Remove a SINGLE Viewer Windows data from Flags & gv[]
				Flags . DeleteViewerAndFlags ( );
				return;
			}
			Guid tag = ( Guid ) Tag;
			// Removes  the Viewer entry identified by it's Tag
			//from the Control structure list & updates it as required
			ListBoxItem lbi = new ListBoxItem ( );
			for ( int y = 0 ; y < Flags . DbSelectorOpen . ViewersList . Items . Count - 1 ; y++ )
			{
				lbi = Flags . DbSelectorOpen . ViewersList . Items [ y + 1 ] as ListBoxItem;
				Guid lbtag = ( Guid ) lbi . Tag;
				if ( tag == Guid . Empty )
				{
					// Command to clear ALL entries
					if ( lbtag == tag )
					{
						//window has been closed - Remove data from Control Structure
						MainWindow . gv . ViewerCount--;
						MainWindow . gv . CurrentDb [ y ] = "";
						MainWindow . gv . ListBoxId [ y ] = Guid . Empty;
						MainWindow . gv . Datagrid [ y ] = null;
						MainWindow . gv . window [ y ] = null;
						MainWindow . gv . SqlViewerWindow = null;
						break;
					}
				}
				else
				{
					//Remove a SINGLE Viewer Windows data from Flags & gv[]
					Flags . DeleteViewerAndFlags ( y );
					break;
					//if (lbtag == (Guid)Tag)
					//{
					//	MainWindow.gv.CurrentDb[y] = "";
					//	MainWindow.gv.ListBoxId[y] = Guid.Empty;
					//	MainWindow.gv.Datagrid[y] = null;
					//	MainWindow.gv.window[y] = null;
					//	MainWindow.gv.SqlViewerWindow = null;

					//	//This needs to stay at ZERO, no tdecrmeented to -1 like the rest
					//	MainWindow.gv.ViewerCount--;
					//	break;
					//}
				}
			}
			//			MainWindow.gv.ViewerCount = 0;
		}

#pragma main viewerslist content updater NEEDED ???

		/// <summary>
		/// PRIMARY Fn TO ADD A VIEWER
		/// Adds the details of the newly loaded viewer window to the DbSelectors ViewersList window
		/// </summary>
		/// <param name="data"></param>
		public static string AddViewerToList ( string data , SqlDbViewer viewer , int DbType )
		{
			if ( viewer == null ) return "";
			if ( viewer . Tag == null )
				return "";
			//Binding binding = new Binding ("Content");
			//binding.Source = Flags.DbSelectorOpen.ListBoxItemText;
			ListBoxItem lbi = new ListBoxItem ( );
			// Set Tag of this LB Item to the DbViewer Windo.w
			//			lbi.Tag = SqlDbViewer.SequentialId;
			lbi . Tag = viewer . Tag;
			Guid guid = ( Guid ) lbi . Tag;
			if ( Utils . CheckForExistingGuid ( guid ) )
			{
				Flags . DbSelectorOpen . UpdateViewersList ( );
			}
			else
			{
				if ( Flags . CurrentSqlViewer . Tag == null ) return "";
				//Set tag in the Windows Memory space
				if ( ( Guid ) Flags . CurrentSqlViewer . Tag == Guid . Empty )
					Flags . CurrentSqlViewer . Tag = lbi . Tag;
				//update our DependencyProperty ListBoxItemText - in DbSelector.cs
				if ( DbType == 0 )
				{
					BankAccountViewModel rec = new BankAccountViewModel ( );
					rec = Flags . CurrentSqlViewer . BankGrid . SelectedItem as BankAccountViewModel;
					MainWindow . gv . PrettyDetails = $"Bank - A/c # {rec?.BankNo}, Cust # {rec?.CustNo}, Balance £ {rec?.Balance}, Interest {rec?.IntRate}%";
					lbi . Content = MainWindow . gv . PrettyDetails;
				}
				else if ( DbType == 1 )
				{
					CustomerViewModel rec = new CustomerViewModel ( );
					rec = Flags . CurrentSqlViewer .  CustomerGrid . SelectedItem as CustomerViewModel;
					MainWindow . gv . PrettyDetails = $"Customer - A/c # {rec?.BankNo}, Cust # {rec?.CustNo}, Forename: {rec?.FName}, Surname : {rec?.LName}, Town : {rec?.Town}";
					lbi . Content = MainWindow . gv . PrettyDetails;
				}
				else if ( DbType == 2 )
				{
					DetailsViewModel rec = new DetailsViewModel ( );
					rec = Flags . CurrentSqlViewer . DetailsGrid . SelectedItem as DetailsViewModel;
					MainWindow . gv . PrettyDetails = $"Details - A/c # {rec?.BankNo}, Cust # {rec?.CustNo}, Balance £ {rec?.Balance}, Interest {rec?.IntRate}%";
					lbi . Content = MainWindow . gv . PrettyDetails;
				}
				//				data = MainWindow.gv.PrettyDetails;
			}
			// This adds the entry on creation of a new viewer.  MainWindow.gv[] is also filled when we get here on new window loading
			//if (Flags.SqlViewerIsLoading || !CheckForExistingEntry (lbi.Content as string))
			//{
			int indx = Flags . DbSelectorOpen . ViewersList . Items . Add ( lbi );
			//			MainWindow.gv.PrettyDetails = lbi.Content as string;
			Flags . DbSelectorOpen . ViewersList . Items . Refresh ( );
			ExtensionMethods . Refresh ( Flags . DbSelectorOpen . ViewersList );
			Flags . DbSelectorOpen . ViewersList . SelectedIndex = indx;
			return MainWindow . gv . PrettyDetails;
		}

		private static bool CheckForExistingEntry ( string entry )
		{
			bool result = false;
			ListBoxItem lbi = new ListBoxItem ( );

			foreach ( var item in Flags . DbSelectorOpen . ViewersList . Items )
			{
				lbi = item as ListBoxItem;
				string lbentry = lbi . Content as string;
				// Strings are formatted differently, but this check should identify a match
				if ( lbentry . Contains ( entry ) || entry . Contains ( lbentry ) )
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public string GetCurrentViewerListEntry ( SqlDbViewer sender )
		{
			string retstring = "";
			for ( int x = 0 ; x < Flags . DbSelectorOpen . ViewersList . Items . Count ; x++ )
			{
				ListBoxItem lbi = new ListBoxItem ( );
				//lbi.Tag = viewer.Tag;
				lbi = Flags . DbSelectorOpen . ViewersList . Items [ x ] as ListBoxItem;
				if ( lbi . Tag == null ) return "";
				Guid g = ( Guid ) lbi . Tag;
				if ( g == ( Guid ) Flags . CurrentSqlViewer . Tag )
				{
					string lbistring = lbi . Content as string;
					retstring = lbistring;
					break;
				}
			}
			return retstring;
		}

		public static bool ChangeViewerListEntry ( string currentListentry , string newListEntry , SqlDbViewer viewer )
		{
			bool retval = false;
			for ( int x = 0 ; x < Flags . DbSelectorOpen . ViewersList . Items . Count ; x++ )
			{
				ListBoxItem lbi = new ListBoxItem ( );
				//lbi.Tag = viewer.Tag;
				lbi = Flags . DbSelectorOpen . ViewersList . Items [ x ] as ListBoxItem;
				//				if (lbi.Tag == null) return retval;
				//				Guid g = (Guid)lbi.Tag;
				if ( currentListentry == ( string ) lbi . Content )
				{
					lbi . Content = newListEntry;
					Flags . DbSelectorOpen . ViewersList . Refresh ( );
					retval = true;
					break;
				}
			}
			return retval;
		}

		//*****************************************************//

		#endregion Receive  notifications into MyNotification() from SqlViewer - WORKS JUST FINE

		// Variable to hold string content for ListBox items in ViewerList of DbSelector.
		private string _listBoxItemText;

		public string ListBoxItemText
		{
			get { return _listBoxItemText; }

			set
			{
				_listBoxItemText = value;
				OnPropertyChanged ( ListBoxItemText . ToString ( ) );
			}
		}

		private void OnWindowLoaded ( object sender , RoutedEventArgs e )
		{
#pragma LOADING  now in HandleSelection()

//			EventHandlers . SendViewerCommand ( 102 , ">>> Starting OnWindowLoaded()" , Flags . CurrentSqlViewer );
			int counter = 0;

			{
				//			AddViewerToList (MainWindow.gv.PrettyDetails, Flags.CurrentSqlViewer);

				//Try to populate our list of existing Viewers
				//for (int x = 0; x < MainWindow.gv.MaxViewers; x++)
				//{
				//	if (MainWindow.gv.window[x] != null)
				//	{
				//		ListBoxItem lbi = new ListBoxItem ();

				//		Binding binding = new Binding ("ListBoxItemText");
				//		binding.Source = ListBoxItemText;
				//		//lbi.SetBinding (ContentProperty, binding);
				//		lbi.Content = MainWindow.gv.PrettyDetails;

				//		ViewersList.SelectedIndex = ViewersList.Items.Add (lbi);

				//		//Inital values going into our listbox item (entry)!!
				//		lbi.Tag = MainWindow.gv.ListBoxId[x];
				//		counter++;
				//		//					ViewerDelete.IsEnabled = true;
				//		//					SelectViewerBtn.IsEnabled = true;
				//	}
				//}
			}
			//Set default active item to 1st valid entry
			counter = ViewersList . Items . Count;

			if ( counter <= 1 )
			{
				ViewerDeleteAll . IsEnabled = false;
				ViewerDelete . IsEnabled = false;
				SelectViewerBtn . IsEnabled = false;
			}
			else
			{
				ViewersList . SelectedIndex = 1;
				if ( counter > 1 )
					ViewerDelete . IsEnabled = true;
				else
					ViewerDelete . IsEnabled = false;
				if ( counter > 2 )
					ViewerDeleteAll . IsEnabled = true;
				else
					ViewerDeleteAll . IsEnabled = false;
				SelectViewerBtn . IsEnabled = true;
			}

			// select the 1st entry in the lower (New Viewer) list
			sqlSelector . SelectedIndex = 2;
			this . BringIntoView ( );
			OntopChkbox . IsChecked = false;
			this . Topmost = false;
			//Send commands to SqlDbViewer !!!!!
			//			EventHandlers . SendViewerCommand ( 103 , ">>> Ended OnWindowLoaded()" , Flags . CurrentSqlViewer );
		}

		//*****************************************************************************************//
		private void DoDragMove ( )
		{//Handle the button NOT being the left mouse button
		 // which will crash the DragMove Fn.....
			try
			{ this . DragMove ( ); }
			catch { return; }
		}

		//*****************************************************************************************//
		private void Cancel_Click ( object sender , RoutedEventArgs e )
		{
			// close this Db Selector window
			this . Visibility = Visibility . Collapsed;
		}

		private void sqlselector_Select ( object sender , MouseButtonEventArgs e )
		{//Select Btn or dbl click on top list, so get a new window of selected type
			if ( sqlSelector . SelectedIndex == -1 )
				return;
			HandleSelection ( sqlSelector , "NEW" );
		}

		//**************************** LOWER LIST - EXISTING VIEWER *************************************//
		private void SelectViewer_Click ( object sender , RoutedEventArgs e )
		{//Select Btn button for lower viewers list
		 //open / bring the window to the front
			HandleSelection ( ViewersList , "SELECT" );
			//ViewersList_Select (sender, null);
		}

		//********************************************************************************************//
		private void ViewersList_Select ( object sender , MouseButtonEventArgs e )
		{// double click on list2 - existing viewer list - pass the selected item data back
		 // and open/bring the window to the front
			if ( ViewersList . SelectedIndex == -1 )
				return;
			HandleSelection ( ViewersList , "SELECT" );
		}

		//********************************************************************************************//
		private void DeleteViewer_Click ( object sender , RoutedEventArgs e )
		{
			// delete just the selected viewer
			if ( ViewersList . SelectedIndex < 1 )
				return;

			HandleSelection ( ViewersList , "DELETE" );
		}

		//********************************************************************************************//
		private void SQLlist_Focused ( object sender , RoutedEventArgs e )
		{
			//Set the flag so we know which list is active for key press checking
			CurrentList = 1;
		}

		//********************************************************************************************//
		private void Viewerslist_Focused ( object sender , RoutedEventArgs e )
		{
			//Set the flag so we know which list is active for key press checking
			CurrentList = 2;
		}

		//********************************************************************************************//
		private void sqlselectorbtn_Select ( object sender , RoutedEventArgs e )
		{   // top list Select button pressed - open a new viewer of selected type
			if ( sqlSelector . SelectedIndex == -1 )
				return;
			if ( MainWindow . gv . ViewerCount == MainWindow . gv . MaxViewers )
			{
				MessageBox . Show ( $"Sorry, but the maximum of {MainWindow . gv . MaxViewers} Viewer Windows are already open.\r\nPlease close one or more, or select an existing Viewer..." , "Maximum viewer count reached" );
				return;
			}

			HandleSelection ( sqlSelector , "NEW" );
			Utils . GetWindowHandles ( );
		}

		//********************************************************************************************//
		private void DeleteAllViewers_Click ( object sender , RoutedEventArgs e )
		{
			if ( ViewersList . Items . Count == 1 )
				return;
			HandleSelection ( ViewersList , "DELETEALL" );
		}

		//*******************************MAIN KEY HANDLER FOR LIST BOXES*************************************//
		private void IsEnterKey ( object sender , KeyEventArgs e )
		{
//			Console . WriteLine ( $"Key1 = {key1}, Key : {e . Key . ToString ( )}" );
			//PreviewKeyDown - in either list
			if ( e . Key == Key . LeftCtrl )
			{
				key1 = true;
			}

			if ( key1 && e . Key == Key . F9 )     // CTRL + F9
			{
				// lists all delegates & Events
				EventHandlers . ShowSubscribersCount ( );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( key1 && e . Key == Key . System )     // CTRL + F10
			{
				// Major  listof GV[] variables (Guids etc]
				Flags . ListGridviewControlFlags ( 1 );
				key1 = false;
				e . Handled = true;
				return;
			}
			else if ( key1 && e . Key == Key . F11 )  // CTRL + F11
			{
				// list various Flags in Console
				Flags . PrintSundryVariables ( );
				e . Handled = true;
				key1 = false;
				return;
				//				Console . WriteLine ("Left Ctrl hit");
			}
			else if ( e . Key == Key . Enter )
			{
				if ( CurrentList == 1 )
				{ // Top list - new Viewer type
				  //					sqlselectorbtn_Select (sender, null);
					HandleSelection ( sqlSelector , "NEW" );
				}
				else if ( CurrentList == 2 )
				{ // Lower list (open Viewers)
					if ( ViewersList . SelectedIndex == -1 )
						return;

					HandleSelection ( ViewersList , "SELECT" );
				}
				key1 = false;
				return;
			}
			else if ( e . Key == Key . NumPad2 || e . Key == Key . Down )
			{
				ListBox lb = sender as ListBox;
				if ( lb . SelectedIndex < lb . Items . Count - 1 )
					lb . SelectedIndex++;
				key1 = false;
				return;
			}
			else if (key1 &&  e . Key == Key . F12)
			{
				if ( key1 )
				{
					Flags . ShowAllFlags ( );
					key1 = false;
				}
				return;
			}
			else if ( e . Key == Key . OemQuotes )
			{
				EventHandlers . ShowSubscribersCount ( );
				key1 = false;
			}
			else if ( e . Key == Key . NumPad8 || e . Key == Key . Up )
			{
				ListBox lb = sender as ListBox;
				if ( lb . SelectedIndex > 0 )
					lb . SelectedIndex--;
				key1 = false;
				return;
			}
			else if ( e . Key == Key . Home )
			{
				Flags . ListGridviewControlFlags ( );
				key1 = false;
				return;
			}
			else if ( key1 && e . Key == Key . End )
			{
				Flags . ListGridviewControlFlags ( 1 );
				key1 = false;
				return;
			}
			else if ( e . Key == Key . Escape )
			{
				Flags . DbSelectorOpen = null;
				Close ( );
				return;
			}
		}

		private void CloseviewerWindow ( int index )
		{
			//Close the specified viewer
			if ( MainWindow . gv . window != null )
			{
				//Fn removes all record of it's very existence
				MainWindow . gv . window [ index ] . Close ( );
				Flags . CurrentSqlViewer = null;
				MainWindow . gv . SqlViewerWindow = null;
			}
		}

		//********************************************************************************************//
		private void Window_Closing ( object sender , System . ComponentModel . CancelEventArgs e )
		{
			Flags . DbSelectorOpen = null;
		}

		private void MultiViewer_Click ( object sender , RoutedEventArgs e )
		{
			if ( Flags . CurrentSqlViewer == null )
			{
				MultiViewer mv = new MultiViewer ( );
				mv . Show ( );
			}
			else
			{
				MessageBox.Show($"Please close ALL open Db Viewers before openking the MultiViewer", "Data Conflict Warning");
			}
		}

		private void Window_KeyDown ( object sender , KeyEventArgs e )
		{
			if ( e . Key == Key . LeftCtrl )
			{
				key1 = true;
			}
//			Console . WriteLine ( $"Key1 = {key1}, Key = {e . Key}" );

			if ( key1 && e . Key == Key . F9 )     // CTRL + F9
			{
				// lists all delegates & Events
				EventHandlers . ShowSubscribersCount ( );
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
			else if ( key1 && e . Key == Key . System )     // CTRL + F10
			{
				// Major  listof GV[] variables (Guids etc]
				Flags . ListGridviewControlFlags ( 1 );
				key1 = false;
				e . Handled = true;
				return;
			}
			else if ( key1 && e . Key == Key . F11 )  // CTRL + F11
			{
				// list various Flags in Console
				Flags . PrintSundryVariables ( );
				e . Handled = true;
				key1 = false;
				return;
				//				Console . WriteLine ("Left Ctrl hit");
			}
			else if ( key1 && e . Key == Key . F12 )
			{
				if ( key1 )
				{
					Flags . ShowAllFlags ( );
					key1 = false;
				}
				return;
			}
			else if ( e . Key == Key . RightAlt )
			{
				Flags . ListGridviewControlFlags ( );
				key1 = false;
				return;
			}
			//else
			//{
			//	key1 = false;
			//}
		}

		/// <summary>
		/// Called byViewers when  focus  changes between them
		/// </summary>
		/// <param name="sqlv"></param>
		public static void SelectActiveViewer ( SqlDbViewer sqlv )
		{
			Guid tag = Guid . Empty;
			tag = ( Guid ) sqlv . Tag;
			for ( int x = 0 ; x < MainWindow . gv . MaxViewers ; x++ )
			{
				// find Tag that matches our Tag in ViewersList
				if ( MainWindow . gv . ListBoxId [ x ] == tag )
				{
					for ( int i = 1 ; i < Flags . DbSelectorOpen . ViewersList . Items . Count ; i++ )
					{
						ListBoxItem lbi = Flags . DbSelectorOpen . ViewersList . Items [ i ] as ListBoxItem;
						if ( ( Guid ) lbi . Tag == tag )
						{
							Flags . DbSelectorOpen . ViewersList . SelectedIndex = i;
							Flags . DbSelectorOpen . ViewersList . SelectedItem = i;
							Flags . DbSelectorOpen . ViewersList . Refresh ( );
							break;
						}
					}
					//					Flags . DbSelectorOpen . ViewersList . SelectedIndex = x ;
					//					Flags . DbSelectorOpen . ViewersList . SelectedItem = x ;
					//					Flags . DbSelectorOpen . ViewersList . Refresh ( );
					break;
				}
			}
		}

		/// <summary>
		///  Refresh the text Content of the ViewersList entires ot match currently selected  Viewer
		/// </summary>
		public void UpdateViewersList ( )
		{
			if ( this . Tag == null ) return;
			if ( MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count == 1 )
				return;
			for ( int i = 0 ; i < MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count ; i++ )
			{
				if ( i + 1 == MainWindow . gv . DbSelectorWindow . ViewersList . Items . Count )
					return;
				if ( MainWindow . gv . ListBoxId [ i ] == ( Guid ) Flags . CurrentSqlViewer . Tag )
				{
					ListBoxItem lbi = new ListBoxItem ( );
					lbi = Flags . DbSelectorOpen . ViewersList . Items [ i + 1 ] as ListBoxItem;
					lbi . Content = MainWindow . gv . CurrentDb [ i ];
					break;
				}
			}
		}

		#region GetInstance

		//*****************************************************************************************//
		//this is really clever stuff
		// It lets me call standard methods (private, public, protected etc)
		//from INSIDE a Static method
		// using syntax : GetInstance().MethodToCall();
		//and it works really great
		private static DbSelector _DbsInstance;

		public static DbSelector GetDbsInstance ( )
		{
			if ( _DbsInstance == null )
				_DbsInstance = new DbSelector ( );
			return _DbsInstance;
		}

		private static SqlDbViewer _SqlInstance;

		public static SqlDbViewer GetSqlInstance ( )
		{
			if ( _SqlInstance == null )
				_SqlInstance = new SqlDbViewer ( );
			return _SqlInstance;
		}

		private static CustomerViewModel _CvInstance;

		public static CustomerViewModel GetCvInstance ( )
		{
			if ( _CvInstance == null )
				_CvInstance = new CustomerViewModel ( );
			return _CvInstance;
		}

		private static BankAccountViewModel _BkInstance;

		public static BankAccountViewModel GetBkInstance ( )
		{
			if ( _BkInstance == null )
				_BkInstance = new BankAccountViewModel ( );
			return _BkInstance;
		}

		private static DetailsViewModel _DetInstance;

		public static DetailsViewModel GetDetInstance ( )
		{
			if ( _DetInstance == null )
				_DetInstance = new DetailsViewModel ( );
			return _DetInstance;
		}

		#endregion GetInstance

		private void ViewersList_PreviewMouseDown ( object sender , MouseButtonEventArgs e )
		{
			HandleSelection ( ViewersList , "SELECT" );
		}

		#region PropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged ( string PropertyName )
		{
			if ( null != PropertyChanged )
			{
				PropertyChanged ( this ,
					new PropertyChangedEventArgs ( PropertyName ) );
			}
		}

		#endregion PropertyChanged

		//		private void AddViewerToListFromTuple (Tuple<SqlDbViewer, string, int> tuple)
		//		//NOT USED
		//		{
		//			//Create/Add new viewer entry (ListBoxItem) to Selection viewer Listbox
		//			/*
		//				 Item1 = current SqlDbViewer
		//				Item2 = CurrentDb string`
		//				Item3 = Grid.SelectedIndex
		//			 */
		//			ListBoxItem lbi = new ListBoxItem ();
		//			{
		//				// Set Tag of this LB Item to the DbViewer Window
		//				SendViewerCommand (102, ">>> Starting AddViewerToListFromTuple()", Flags.CurrentSqlViewer);
		//				SqlDbViewer sqlv = tuple.Item1 as SqlDbViewer;
		//				lbi.Tag = sqlv.Tag;
		//				Flags.DbSelectorOpen.ListBoxItemText = ">>>>>>>>>>>";
		//				//This is the normal way to update the lists data
		//				if (tuple.Item2 == "BANKACCOUNT")
		//				{
		//					var v = sqlv.BankGrid.SelectedItem as BankAccountViewModel;

		//				}
		//				else if (tuple.Item2 == "CUSTOMER")
		//				{
		//					var v = sqlv.BankGrid.SelectedItem as CustomerViewModel;

		//				}
		//				else if (tuple.Item2 == "DETAILS")
		//				{
		//					var v = sqlv.BankGrid.SelectedItem as DetailsViewModel;

		//				}

		////				AddViewerToList (MainWindow.gv.PrettyDetails, Flags.CurrentSqlViewer);

		//				//lbi.Content = MainWindow.gv.PrettyDetails;

		//				//int indx = this.ViewersList.Items.Add (lbi);
		//				//this.ViewersList.SelectedIndex = indx;
		//				this.ViewersList.Items.Refresh ();
		//				ExtensionMethods.Refresh (this.ViewersList);

		//				if (this.ViewersList.Items.Count > 1)
		//				{
		//					if (this.ViewersList.Items.Count > 1)
		//						this.ViewerDeleteAll.IsEnabled = true;
		//					else
		//						this.ViewerDeleteAll.IsEnabled = false;
		//					this.ViewerDelete.IsEnabled = true;
		//					this.SelectViewerBtn.IsEnabled = true;
		//				}
		//				this.ViewersList.BringIntoView ();
		//				ExtensionMethods.Refresh (this.ViewersList);
		//				// This WORKS for details 2/4/21
		//				Debug.WriteLine ($" *** Current Active...3 =  {Flags.ActiveSqlGridStr}\r\n");
		//				if (Flags.ActiveSqlGrid?.ItemsSource != null)
		//					CollectionViewSource.GetDefaultView (Flags.ActiveSqlGrid.ItemsSource).Refresh ();
		//				SendViewerCommand (103, "<<< Ended AddViewerToList()", Flags.CurrentSqlViewer);

		//				Mouse.OverrideCursor = Cursors.Arrow;

		//				//				dg.CurrentSqlViewer.Focus ();
		//				//				break;
		//			}
		//		}

	
		private void OntopChkbox_Click ( object sender , RoutedEventArgs e )
		{
			if ( OntopChkbox . IsChecked == true )
				this . Topmost = true;
			else
				this . Topmost = false;
		}
		private void Bankedit_Click ( object sender , RoutedEventArgs e )
		{
			BankDbView cdbv = new BankDbView();
			cdbv . Show ( );
		}

		private void Closeapp_Click ( object sender , RoutedEventArgs e )
		{
			Application . Current . Shutdown ( );
		}
	}
}
