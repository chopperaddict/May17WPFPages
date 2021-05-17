//#define SHOWFLAGS
#define SHOWALLFLAGS
#undef SHOWALLFLAGS
#define USEDETAILEDEXCEPTIONHANDLER
#undef USEDETAILEDEXCEPTIONHANDLER

//using System;
using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Threading;
using System . Windows . Controls;

using WPFPages . Views;

namespace WPFPages
{

	public static class Flags
	{
	
		public static DataGrid SqlBankGrid = null;
		public static SqlDbViewer SqlBankViewer = null;
		public static  DataGrid CurrentEditDbViewerBankGrid = null;

		// Pointers to our data colections
		public static DetCollection DetCollection = null;
		public static CustCollection CustCollection = null;
		public static BankCollection BankCollection = null;

		private struct AllFlags
		{

			DataGrid  SqlBankGrid;
			//			SqlDbViewer SqlBankViewer;
			DataGrid CurrentEditDbViewerBankGrid;
			DataGrid SqlCustGrid;
			//			SqlDbViewer SqlCustViewer;
			DataGrid CurrentEditDbViewerCustomerGrid;
			DataGrid SqlDetGrid;
			//			SqlDbViewer SqlDetViewer;
			//			DataGrid CurrentEditDbViewerDetailsGrid;
			//			List< DataGrid > CurrentEditDbViewerBankGridLis2;
			//			List< DataGrid > CurrentEditDbViewerCustomerGridList;
			//			List< DataGrid > CurrentEditDbViewerDetailsGridList;
			//			EditDb ActiveEditGrid;
			bool isMultiMode;
			string MultiAccountCommandString;
			//			bool isEditDbCaller;
			//			bool SqlDataChanged;
			//			bool EditDbDataChanged;
			EditDb BankEditDb;
			EditDb CustEditDb;
			EditDb DetEditDb;
			//			DbSelector DbSelectorOpen;
			EditDb CurrentEditDbViewer;
			//			SqlDbViewer CurrentSqlViewer;
			//			SqlDbViewer SqlUpdateOriginatorViewer;
			//			bool EditDbChangeHandled;
			//			bool IsFiltered;
			//			string FilterCommand;
			//			bool EventHandlerDebug;
			bool IsMultiMode;
			//			SqlDbViewer ActiveSqlViewer;
			bool SqlViewerIsLoading;
			bool  SqlViewerIndexIsChanging;
			//			DataGrid ActiveSqlGrid;
			int SqlBankCurrentIndex;
			int SqlCustCurrentIndex;
			int SqlDetCurrentIndex;
		}

		public static DataGrid SqlCustGrid = null;
		public static SqlDbViewer SqlCustViewer = null;
		public static  DataGrid CurrentEditDbViewerCustomerGrid = null;

		public static DataGrid SqlDetGrid = null;
		public static SqlDbViewer SqlDetViewer = null;
		public static  DataGrid CurrentEditDbViewerDetailsGrid = null;

		public static List< DataGrid > CurrentEditDbViewerBankGridList;
		public static List< DataGrid > CurrentEditDbViewerCustomerGridList;
		public static List< DataGrid > CurrentEditDbViewerDetailsGridList;

		// Current active Grid pointer and Name - Used as a pointer to the currently active DataGrid
		public static DataGrid ActiveSqlGrid = null;

		// current SelectedIndex for each grid type in SqlDbViewers
		//Updated whenever the selection changes in any of the grids
		public static int SqlBankCurrentIndex = 0;
		public static int SqlCustCurrentIndex = 0;
		public static int SqlDetCurrentIndex = 0;

		//EditDb Grid info
		public static EditDb ActiveEditGrid = null;

		// Flag ot control Multi account data loading
		public static bool isMultiMode = false;
		public static string MultiAccountCommandString = "";

#pragma TODO add other Edit Db windows 2/4/21

		public static bool isEditDbCaller = false;
		public static bool SqlDataChanged = false;
		public static bool EditDbDataChanged = false;
		// system wide flags to avoid selection change processing when we are loading/Reloading FULL DATA in SqlDbViewer
		public static bool  DataLoadIngInProgress = false;
		public static bool UpdateInProgress = false;

		public static EditDb BankEditDb = null;
		public static EditDb CustEditDb = null;
		public static EditDb DetEditDb = null;

		//Flags to hold pointers to current DbSelector & SqlViewer Windows
		// Needed to avoi dInstance issues when calling methods from inside Static methods
		// that are needed to handle the interprocess messaging system I have designed for this thing
		public static DbSelector DbSelectorOpen = null;
		public static EditDb CurrentEditDbViewer = null;
		public static SqlDbViewer CurrentSqlViewer = null;
		//		public static SqlDbViewer SqlUpdateOriginatorViewer = null;

		// pointers  to any open Viewers
		public static SqlDbViewer CurrentBankViewer;
		public static SqlDbViewer CurrentCustomerViewer;
		public static SqlDbViewer  CurrentDetailsViewer;

		public static  MultiViewer MultiViewer;

		public static bool EditDbChangeHandled = false;

		public static bool IsFiltered = false;
		public static string FilterCommand = "";

		//Control CW output of event handlers
		public static bool EventHandlerDebug = false;
		public static bool IsMultiMode = false;

		/// <summary>
		///  Holds the DataGrid pointer fort each open SqlDbViewer Window as they
		///  can each have diffrent datasets in use at any one time
		/// </summary>
		public static SqlDbViewer ActiveSqlViewer= null;

		/// <summary>
		///  Used to  control the initial load of Viewer windows to avoid 
		///  mutliple additions to DbSelector's viewer  listbox
		/// </summary>
		public static bool SqlViewerIsLoading = false;
		public static bool SqlViewerIndexIsChanging= false;

		/*
		 *	Sorting Checkbox enumeration
		 *	Default_option . IsChecked = true;		// 0
			Id_option . IsChecked = false;			// 1
			Bankno_option . IsChecked = false;		// 2
			Custno_option . IsChecked = false;		// 3
			actype_option . IsChecked   = false;		// 4
			Dob_option . IsChecked = false;		// 5
			Odate_option . IsChecked = false;		// 6
			Flags . SortOrderRequested = 6;

		*/

		public static double TopVisibleBankGridRow = 0;
		public static double BottomVisibleBankGridRow = 0;
		public static double TopVisibleCustGridRow = 0;
		public static double BottomVisibleCustGridRow = 0;
		public static double TopVisibleDetGridRow = 0;
		public static double BottomVisibleDetGridRow = 0;
		public static double ViewPortHeight = 0;

		// Set default sort to Custno + Bankno
		public static int SortOrderRequested = 0;
		public enum SortOrderEnum
		{
			DEFAULT = 0,
			ID,
			BANKNO,
			CUSTNO,
			ACTYPE,
			DOB,
			ODATE,
			CDATE
		}

		/// <summary>
		///  handle maintenance of global flags used to control mutliple 
		///  viewers and EditDb windows, called from Focus()
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="CurrentDb"></param>
		public static void SetGridviewControlFlags ( SqlDbViewer instance , DataGrid Grid )
		{
			//Setup global flags -
			Flags . CurrentSqlViewer = instance;
			//only do this if we are not closing a windows (Sends Grid=null)
			if ( Grid != null )
			{
				if ( Grid == Flags . SqlBankGrid )
				{
					Flags . ActiveSqlGrid = Grid;
					Flags . SqlBankGrid = Grid;
					Flags . SqlBankViewer = Flags . CurrentSqlViewer;
					Flags . CurrentSqlViewer = instance;
				}
				else if ( Grid == Flags . SqlCustGrid )
				{
					Flags . ActiveSqlGrid = Grid;
					Flags . SqlCustGrid = Grid;
					Flags . SqlCustViewer = Flags . CurrentSqlViewer;
					Flags . CurrentSqlViewer = instance;
				}
				else if ( Grid == Flags . SqlDetGrid )
				{
					Flags . ActiveSqlGrid = Grid;
					Flags . SqlDetGrid = Grid;
					Flags . SqlDetViewer = Flags . CurrentSqlViewer;
					Flags . CurrentSqlViewer = instance;
				}
			}
			else
			{
				// we need to clear the  details in Gridviewer flag system
				ClearGridviewControlStructure ( instance , Grid );
			}
#if SHOWFLAGS
			ListGridviewControlFlags();
#endif
		}
		public static void ClearGridviewControlStructure ( SqlDbViewer instance , DataGrid Grid )
		{
			//No more viewers open, so clear entire gv[] control structure
			if ( instance == null || Flags . DbSelectorOpen . ViewersList . Items . Count == 1 )
			{
				// Remove ALL Viewers Data - There are no Viewers open apparently !
				for ( int x = 0 ; x < MainWindow . gv . MaxViewers ; x++ )
				{
					MainWindow . gv . window [ x ] = null;
					MainWindow . gv . CurrentDb [ x ] = "";
					MainWindow . gv . Datagrid [ x ] = null;
					MainWindow . gv . ListBoxId [ x ] = Guid . Empty;
					MainWindow . gv . SelectedViewerType = -1;
					MainWindow . gv . ViewerSelectiontype = -1;
				}
				MainWindow . gv . ViewerCount = 0;
				MainWindow . gv . PrettyDetails = "";
			}
			else
			{
				//Remove a SINGLE Viewer Windows data from Flags & gv[]
				DeleteViewerAndFlags ( );
				Flags . CurrentSqlViewer . UpdateDbSelectorBtns ( Flags . CurrentSqlViewer );
			}
		}
		//Remove a SINGLE Viewer Windows data from Flags & gv[]
		public static bool DeleteViewerAndFlags ( int index = -1 , string currentDb = "" )
		{
			int x = index;
			SqlDbViewer sqlv;                        // x = GridView[] index if received
			if ( Flags . CurrentSqlViewer == null )
				return false;
			Guid tag = ( Guid ) Flags . CurrentSqlViewer?.Tag;
			ListBoxItem lbi = new ListBoxItem ( );

			if ( x == -1 )
			{ // Delete all
				for ( int z = 0 ; z < MainWindow . gv . MaxViewers ; z++ )
				{
					DbSelector . UpdateControlFlags ( null , null , MainWindow . gv . PrettyDetails );
					MainWindow . gv . CurrentDb [ z ] = "";
					MainWindow . gv . ListBoxId [ z ] = Guid . Empty;
					MainWindow . gv . Datagrid [ z ] = null;
					MainWindow . gv . window [ z ] = null;
				}
				MainWindow . gv . ViewerCount = 0;
				MainWindow . gv . PrettyDetails = "";
				MainWindow . gv . SqlBankViewer = null;
				MainWindow . gv . SqlCustViewer = null;
				MainWindow . gv . SqlDetViewer = null;
				MainWindow . gv . SqlViewerGuid = Guid . Empty;
				MainWindow . gv . SqlViewerWindow = null;

				MainWindow . gv . Bankviewer = Guid . Empty;
				MainWindow . gv . Custviewer = Guid . Empty;
				MainWindow . gv . Detviewer = Guid . Empty;

				Flags . ActiveSqlGrid = null;
				Flags . SqlBankViewer = null;
				Flags . SqlCustViewer = null;
				Flags . SqlDetViewer = null;
//				Flags . CurrentSqlViewer = null;

				// ALL entries in our GridView structure are now cleared  ** totally **
				return true;
			}
			else
			{
				int GridViewerArrayIndex = 0;
				// we may have NOT received the index of the viewer in the list
				// so  get the index for the correct Entry
				if ( x == 99 )
				{
					// got to find it ourselves - iterate thruMainWindow.gv[] array  (Range  is 0 - 3)
					for ( int i = 0 ; i < 3 ; i++ )
					{
						if ( MainWindow . gv . CurrentDb [ i ] == currentDb )
						{ x = i; break; }
					}
					GridViewerArrayIndex = x;
					// we have got the index in "x"  of the viewer in the Mainindow.gv[] array
					// so  get the Tag of that selected Entry vin the ViewersList
					for ( int i = 1 ; i < 4 ; i++ )
					{
						lbi = Flags . DbSelectorOpen . ViewersList . Items [ i ] as ListBoxItem;
						if ( MainWindow . gv . ListBoxId [ GridViewerArrayIndex ] == ( Guid ) lbi . Tag )
						{
							//lbi = Flags . DbSelectorOpen . ViewersList . Items [ i ] as ListBoxItem;
							Flags . DbSelectorOpen . ViewersList . Items . RemoveAt ( i );
							Flags . DbSelectorOpen . ViewersList . Refresh ( );
							break;
						}
					}
				}
				else
				{
					// we have the ViewersList index given to us, so use it
					lbi = Flags . DbSelectorOpen . ViewersList . Items [ index ] as ListBoxItem;
					Flags . DbSelectorOpen . ViewersList . Items . RemoveAt ( index );
					Flags . DbSelectorOpen . ViewersList . Refresh ( );
					// got to findit in thruMainWindow.gv[] array  (Range  is 0 - 3)
					for ( int i = 0 ; i < 4 ; i++ )
					{
						if ( MainWindow . gv . ListBoxId [ i ] == ( Guid ) lbi . Tag )
						{
							GridViewerArrayIndex = i;
							break;
						}
					}
				}
				sqlv = Flags . CurrentSqlViewer as SqlDbViewer;
				sqlv . Close ( );

				// We know which gv[] entry  we need to clear, so do it and return
				MainWindow . gv . CurrentDb [ GridViewerArrayIndex ] = "";
				MainWindow . gv . ListBoxId [ GridViewerArrayIndex ] = Guid . Empty;
				MainWindow . gv . Datagrid [ GridViewerArrayIndex ] = null;
				MainWindow . gv . window [ GridViewerArrayIndex ] = null;
				MainWindow . gv . PrettyDetails = "";
				MainWindow . gv . SqlViewerGuid = Guid . Empty;
				MainWindow . gv . ViewerCount--;
				// Reposition selected viewer if we have one
				if ( Flags . DbSelectorOpen . ViewersList . Items . Count > GridViewerArrayIndex + 1 )
				{
					Flags . DbSelectorOpen . ViewersList . SelectedIndex = GridViewerArrayIndex + 1;
					Flags . DbSelectorOpen . ViewersList . SelectedItem = GridViewerArrayIndex + 1;
				}
				else if ( Flags . DbSelectorOpen . ViewersList . Items . Count == GridViewerArrayIndex + 1 )
				{
					Flags . DbSelectorOpen . ViewersList . SelectedIndex = GridViewerArrayIndex - 1;
					Flags . DbSelectorOpen . ViewersList . SelectedItem = GridViewerArrayIndex - 1;
				}
				return true;
			}

			// Unreachable code ...

			// Now sort out the  global gv[] flags
			for ( int y = 1 ; y < Flags . DbSelectorOpen . ViewersList . Items . Count ; y++ )
			{
				// Get the Tag of eaxch Viewer in the list
				lbi = Flags . DbSelectorOpen . ViewersList . Items [ y ] as ListBoxItem;
				Guid lbtag = ( Guid ) lbi . Tag;
				//See if it matches the one we are closing down
				if ( ( Guid ) lbtag == ( Guid ) tag )
				{
					//Yes, we have got a match, so go ahead and remove its gv[] entries first
					for ( int z = 0 ; z < MainWindow . gv . MaxViewers ; z++ )
					{
						if ( MainWindow . gv . ListBoxId [ z ] == lbtag )
						{
							MainWindow . gv . ViewerCount--;
							MainWindow . gv . CurrentDb [ z ] = "";
							MainWindow . gv . ListBoxId [ z ] = Guid . Empty;
							MainWindow . gv . Datagrid [ z ] = null;
							MainWindow . gv . window [ z ] = null;
							break;
						}

					}
					MainWindow . gv . PrettyDetails = "";
					//Finally we can remove this entry from ViewersList
					lbi = Flags . DbSelectorOpen . ViewersList . Items [ y ] as ListBoxItem;
					lbi . Content = "";
					Flags . DbSelectorOpen . ViewersList . Items . RemoveAt ( y );
					// Set selectedIndex pointer to current position in list
					int currentIndex = y - 1;
					if ( y <= 1 )             // List is basically empty (No viewers in  the list)
						return true;
					if ( Flags . DbSelectorOpen . ViewersList . Items . Count > currentIndex )
					{
						Flags . DbSelectorOpen . ViewersList . SelectedIndex = currentIndex;
						Flags . DbSelectorOpen . ViewersList . SelectedItem = currentIndex;
					}
					else if ( Flags . DbSelectorOpen . ViewersList . Items . Count == currentIndex )
					{
						Flags . DbSelectorOpen . ViewersList . SelectedIndex = currentIndex - 1;
						Flags . DbSelectorOpen . ViewersList . SelectedItem = currentIndex - 1;
					}
					return true;
				}
			}
			MainWindow . gv . SqlViewerGuid = Guid . Empty;

			return false;
		}

		public static void ListGridviewControlFlags ( int mode = 0 )
		{
			if ( mode == 1 )
			{
				Debug . WriteLine (
				"#################################################################\n" +
				$"FULL INFO\n" +
				"#################################################################\n" +
				$"\nMAJOR FLAGS :\n===========\n" +
				$"CurrentSqlViewer :-		 [{CurrentSqlViewer?.Tag}]\n" +
				$"\nBANKACCOUNT SqlBankCurrentIndex :- [{SqlBankCurrentIndex}]\n" +
				$"CUSTOMER    SqlCustCurrentIndex :- [{SqlCustCurrentIndex }]\n" +
				$"DETAILS     SqlDetCurrentIndex:-   [{SqlDetCurrentIndex }]" );
			}
			Debug . WriteLine ( $"\n" +
#if SHOWALLFLAGS
				$"ACTIVE DB                            \"{currentDb}\"\r\n" +
				$"EditDb Flags :" +
				$"ActiveDbGridStr =                                 {ActiveDbGridStr}\r\n" +
				$" *** Current ActiveDbGrid *** =       {ActiveDbGrid}\r\n" +
				$" *** Current ActiveDbGridStr *** =  {ActiveDbGridStr}\r\n" +
				"----\r\n" +
				$"Sql Db Flags :\r\n" +
				$"ActiveSqlViewer =                     {ActiveSqlViewer}\r\n" +
				$"ActiveSqlViewerStr  =               {ActiveSqlViewerStr}\r\n" +
				"----\r\n" +
				$"SqlBankGridStr =                    {SqlBankGridStr} \r\n" +
				$"SqlCustGridStr =                     {SqlCustGridStr}\r\n" +
				$"SqlDetGridStr =                      {SqlDetGridStr}\r\n" +
				"----\r\n" +
				$" *** Current ActiveSqlGrid *** =     {ActiveSqlGrid}\r\n" +
				$" *** Current ActiveSqlGridStr *** =  {ActiveSqlGridStr}\r\n" +
				$"Bank SelectedItem :-\r\n{bvmBankRecord}" +
				$"Cust SelectedItem :-\r\n{bvmCustRecord}" +
				$"Details  SelectedItem :-\r\n{bvmDetRecord}" +
				$"\nMAJOR FLAGS :\n===========\n" + 
				$"CurrentSqlViewer :- [{CurrentSqlViewer?.Tag}]\n" +
				$"\nBANKACCOUNT SqlBankGrid :- [{SqlBankGrid?.GetHashCode()}]" +
				$"\nCUSTOMER    SqlCustGrid :- [{SqlCustGrid? .GetHashCode ( )}]" +
				$"\nDETAILS     SqlDetGrid  :- [{SqlDetGrid? .GetHashCode ( )}]\n\n" + 
				$"\nBANKACCOUNT SqlBankCurrentIndex :- [{SqlBankCurrentIndex}]" +
				$"\nCUSTOMER    SqlCustCurrentIndex :- [{SqlCustCurrentIndex }]" +
				$"\nDETAILS     SqlDetCurrentIndex:-   [{SqlDetCurrentIndex }]\n" +
#endif
			"-----------------------------------------------------------------\n" +
			$"ActiveSqlGrid  =		[{ActiveSqlGrid?.Name}]\n" +
			$"CurrentSqlViewer :-		[{CurrentSqlViewer?.Tag}]\n" +
			$"CurrentEditDbViewer		[{CurrentEditDbViewer?.Name}]\n" +
			$"\nACTIVE VIEWERS:\n===============\n" +
			$"CurrentSqlViewer :-		 [ {CurrentSqlViewer?.Tag} ]\n" +
			$"ALL VIEWERS:\n===========\n" +
			$"BANK     SqlBankViewer:- [ {SqlBankViewer?.Tag} ]\n" +
			$"CUST     SqlCustViewer:- [ {SqlCustViewer?.Tag} ]\n" +
			$"DETS     SqlDetViewer:-  [ {SqlDetViewer?.Tag} ]\n" +
			$"\nFLAGS : CurrentSqlViewer :-  [ {Flags.CurrentSqlViewer?.Tag} ]\n" +
			"=================================================================\n"
			);
		}

		public static void ShowAllFlags ( )
		{
			Console . WriteLine (
			$"\nbool EditDbDataChanged						: { Flags . EditDbDataChanged}" +
			$"\nbool EditDbChangeHandled					: { Flags . EditDbChangeHandled}" +
			$"\nbool EventHandlerDebug						: { Flags . EventHandlerDebug}" +
			$"\nbool isEditDbCaller							: { Flags . isEditDbCaller}" +
			$"\nbool isMultiMode							: { Flags . isMultiMode}" +
			$"\nbool IsFiltered								: { Flags . IsFiltered}" +
			$"\nbool IsMultiMode							: { Flags . IsMultiMode}" +
			$"\nbool SqlViewerIsLoading						: { Flags . SqlViewerIsLoading}" +
			$"\nbool  SqlViewerIndexIsChanging				: { Flags . SqlViewerIndexIsChanging}" +
			"\n" +
			$"\nDataGrid ActiveSqlGrid						: { Flags . ActiveSqlGrid?.Name}" +
			$"\nDataGrid CurrentEditDbViewerBankGrid		: { Flags . CurrentEditDbViewerBankGrid?.Name}" +
			$"\nDataGrid CurrentEditDbViewerCustomerGrid: { Flags . CurrentEditDbViewerCustomerGrid?.Name}" +
			$"\nDataGrid CurrentEditDbViewerDetailsGrid		: { Flags . CurrentEditDbViewerDetailsGrid?.Name}" +
			$"\nDataGrid SqlBankGrid						: { Flags . SqlBankGrid?.Name}" +
			$"\nDataGrid SqlCustGrid						: { Flags . SqlCustGrid?.Name}" +
			$"\nDataGrid SqlDetGrid							: { Flags . SqlDetGrid?.Name}" +
			"\n" +
			$"\nDbSelector DbSelectorOpen					: { Flags . DbSelectorOpen}" +
			"\n" +
			$"\nEditDb ActiveEditGrid						: { Flags . ActiveEditGrid?.Name}" +
			$"\nEditDb BankEditDb							: { Flags . BankEditDb?.Name}" +
			$"\nEditDb CustEditDb							: { Flags . CustEditDb?.Name}" +
			$"\nEditDb DetEditDb							: { Flags . DetEditDb?.Name}" +
			$"\nEditDb CurrentEditDbViewer					: { Flags . CurrentEditDbViewer?.Name}" +
			"\n" +
			$"\nint SqlBankCurrentIndex						: { Flags . SqlBankCurrentIndex}" +
			$"\nint SqlCustCurrentIndex						: { Flags . SqlCustCurrentIndex}" +
			$"\nint SqlDetCurrentIndex						: { Flags . SqlDetCurrentIndex}" +
			"\n" +
			$"\nSqlDbViewer ActiveSqlViewer					: { Flags . ActiveSqlViewer?.CurrentDb}" +
			$"\nSqlDbViewer CurrentSqlViewer				: { Flags . CurrentSqlViewer?.CurrentDb}" +
			$"\nSqlDbViewer SqlBankViewer					: { Flags . SqlBankViewer} + {Flags . SqlBankViewer?.BankGrid?.Name}" +
			$"\nSqlDbViewer SqlCustViewer					: { Flags . SqlCustViewer} + {Flags . SqlCustViewer?.CustomerGrid?.Name}" +
			$"\nSqlDbViewer SqlDetViewer					: { Flags . SqlDetViewer} + {Flags . SqlDetViewer?.DetailsGrid?.Name}" +
			//			$"\nSqlDbViewer SqlUpdateOriginatorViewer					: { Flags .SqlUpdateOriginatorViewer?.Name}" +
			"\n" +
			$"\nstring FilterCommand						: { Flags . FilterCommand}" +
			$"\nstring MultiAccountCommandString			: { Flags . MultiAccountCommandString}" +
			$"\nCurrentThread								: {Thread . CurrentThread . ManagedThreadId}\n" +
			$"\nSQL Database pointers\n" +
			$"Bank : Bankinternalcollection				: { BankCollection . Bankinternalcollection . Count}\n" +
			$"Bank : SqlViewerBankcollection				: { BankCollection . SqlViewerBankcollection . Count}\n" +
			$"Bank : EditDbViewercollection 				: { BankCollection . EditDbBankcollection . Count}\n" +
			$"Bank : MultiBankcollection					: { BankCollection . MultiBankcollection . Count}\n" +
			$"Bank : BankViewerDbcollection				: { BankCollection . BankViewerDbcollection . Count}\n" +

			$"\nCustcollection								: { CustCollection . Custcollection . Count}\n" +
			$"CustViewerDbcollection						: { CustCollection.CustViewerDbcollection . Count}\n" +
			$"SqlViewerCustcollection	  					: { CustCollection . SqlViewerCustcollection . Count}\n" +
			$"EditDbCustcollection 						: { CustCollection . EditDbCustcollection . Count}\n" +
			$"MultiCustcollection							: { CustCollection . MultiCustcollection . Count}\n" +

			$"\nDetcollection								: { DetCollection . Detcollection . Count}\n" +
			$"DetViewerDbcollection						: { DetCollection . DetViewerDbcollection . Count}\n" +
			$"SqlViewerDetcollection	  					: { DetCollection . SqlViewerDetcollection . Count}\n" +
			$"EditDbDetcollection 						: { DetCollection . EditDbDetcollection . Count}\n" +
			$"MultiDetcollection							: { DetCollection . MultiDetcollection . Count}\n\n" 
			);
		}
		public static void PrintSundryVariables (string comment ="")
		{
			if(comment.Length > 0)
				Console . WriteLine ($"\n COMMENT : {comment}");
				else
				Console . WriteLine ( "" );
			if ( Flags.CurrentSqlViewer != null && Flags . SqlBankGrid != null )
				Console . WriteLine ( $" Current Viewer : ItemsSource :		{ Flags . SqlBankGrid . Name}" );
			if ( Flags . CurrentSqlViewer != null && Flags . SqlCustGrid != null )
				Console . WriteLine ( $" Current Viewer : ItemsSource :		{ Flags . SqlCustGrid . Name}" );
			if ( Flags . CurrentSqlViewer != null && Flags . SqlDetGrid != null )
				Console . WriteLine ( $" Current Viewer : ItemsSource :		{ Flags . SqlDetGrid . Name}" );
			Console . WriteLine ( $" Flags . TopVisibleBankGridRow		= { Flags . TopVisibleBankGridRow }" );
			Console . WriteLine ( $" Flags . BottomVisibleBankGridRow	= {Flags . BottomVisibleBankGridRow}" );
			Console . WriteLine ( $" Flags . TopVisibleCustGridRow		= { Flags . TopVisibleCustGridRow }" );
			Console . WriteLine ( $" Flags . BottomVisibleCustGridRow	= { Flags . BottomVisibleCustGridRow}" );
			Console . WriteLine ( $" Flags . TopVisibleDetGridRow		= { Flags . TopVisibleDetGridRow}" );
			Console . WriteLine ( $" Flags . BottomVisibleDetGridRow	= { Flags . BottomVisibleDetGridRow}" );
			Console . WriteLine ( $" Flags . ViewPortHeight				= { Flags . ViewPortHeight } rows visible" );
			if ( Flags . ActiveSqlViewer? . CurrentDb == "BANKACCOUNT" )
				Console . WriteLine ( $" BANK record's offset (from top)	= { ( Flags . SqlBankCurrentIndex - Flags . TopVisibleDetGridRow ) + 1}" );
			else if ( Flags . ActiveSqlViewer?. CurrentDb == "CUSTOMER" )
				Console . WriteLine ( $" CUST record's offset (from top)	= { ( Flags . SqlCustCurrentIndex - Flags . TopVisibleDetGridRow ) + 1}" );
			else if ( Flags . ActiveSqlViewer? . CurrentDb == "DETAILS" )
				Console . WriteLine ( $" DETAILS record offset (from top)	= { ( Flags . SqlDetCurrentIndex - Flags . TopVisibleDetGridRow ) + 1}" );
			Console . WriteLine ( $"\n Flags . SqlBankCurrentIndex		= { Flags . SqlBankCurrentIndex}" );
			Console . WriteLine ( $" Flags . SqlCustCurrentIndex		= { Flags . SqlCustCurrentIndex}" );
			Console . WriteLine ( $" Flags . SqlDetCurrentIndex			= { Flags . SqlDetCurrentIndex}" );

			string buffer = "\n Multi Grid Info :-";
			if ( Flags . SqlBankGrid != null )
				buffer += $"\n Flags.SqlBankGrid					= { Flags . SqlBankGrid?.Items . Count} / {Flags . SqlBankGrid?.SelectedIndex}";
			if ( Flags . SqlCustGrid != null )
				buffer += $"\n Flags.SqlCustGrid					= { Flags . SqlCustGrid?.Items . Count} / {Flags . SqlCustGrid?.SelectedIndex}";
			if ( Flags . SqlDetGrid != null )
				buffer  += $"\n Flags.SqlDetGrid					= { Flags . SqlDetGrid? . Items . Count} / {Flags . SqlDetGrid? . SelectedIndex}";			
			if ( buffer . Length > 18 )
			{
				Console . WriteLine ( buffer );
				Console . WriteLine ( "\n" );
			}

			buffer = "Sql Viewer Info :-";
			if ( Flags . CurrentBankViewer?.BankGrid != null )
				buffer += $" \n Flags.CurrentBankViewer.BankGrid							= { Flags . CurrentBankViewer . BankGrid?.Items . Count} / {Flags . CurrentBankViewer . BankGrid?.SelectedIndex}";
			//				Console . WriteLine ( $" Flags.CurrentBankViewer.BankGrid				= { Flags .CurrentBankViewer.BankGrid?.Items . Count} / {Flags . CurrentBankViewer . BankGrid? . SelectedIndex}" );
			if ( Flags . CurrentCustomerViewer?.BankGrid != null )
				buffer += $"\n Flags.CurrentCustomerViewer . CustomerGrid			= { Flags . CurrentCustomerViewer . CustomerGrid?.Items . Count} / {Flags . CurrentCustomerViewer . CustomerGrid?.SelectedIndex}";
			//					Console . WriteLine ( $" Flags.CurrentCustomerViewer . CustomerGrid	= { Flags . CurrentCustomerViewer . CustomerGrid? . Items . Count} / {Flags . CurrentCustomerViewer . CustomerGrid? . SelectedIndex}" );
			if ( Flags . CurrentDetailsViewer?.BankGrid != null )
				buffer += $"\n Flags.CurrentDetailsViewer . DetailsGrid					= { Flags . CurrentDetailsViewer . DetailsGrid?.Items . Count} / {Flags . CurrentDetailsViewer . DetailsGrid?.SelectedIndex}";
			//					Console . WriteLine ( $" Flags.CurrentDetailsViewer . DetailsGrid		= { Flags . CurrentDetailsViewer . DetailsGrid? . Items . Count} / {Flags . CurrentDetailsViewer . DetailsGrid? . SelectedIndex}" );
			if ( buffer . Length > 18 )
			{
				Console . WriteLine ( buffer );
				Console . WriteLine ( "\n" );
			}
//			Console . WriteLine ( $" Flags . BankCollection			= { Flags . BankCollection.Count}" );
//			Console . WriteLine ( $" Flags . CustCollection			= { Flags . CustCollection . Count}" );
//			Console . WriteLine ( $" Flags . DetCollection			= { Flags . DetCollection . Count}" );
		}
	}
}

