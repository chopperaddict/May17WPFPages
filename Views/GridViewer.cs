using System;
using System . Windows;
using System . Windows . Controls;

using WPFPages . Views;

namespace WPFPages
{
	//this is a classic Singleton class - only one can ever exist
	//& it is used to store the details of ALL Db Gridviewer windows
	//currently open, so we can allow selection of any currently open
	//window even if they are hidden.
	public sealed class GridViewer
	{
		public static readonly GridViewer Viewer_instance = new GridViewer ( );

		public Window [ ] window = { null, null, null };
		public int MaxViewers = 3;
		//Save windows Handle
		//Save content of this list box as text string
		public string [ ] CurrentDb = { "", "", "" };
		public string PrettyDetails = "";
		//store this windows position in the Listbox
		public DataGrid [ ] Datagrid = { null, null, null };
		//The handle to the ONLY Selection Window currently open
		public DbSelector DbSelectorWindow = ( DbSelector ) null;

		//This matches  : SqlDbViewer  Flags.CurrentSqlViewer
		public SqlDbViewer SqlViewerWindow = ( SqlDbViewer ) null;

		// pointers to the host window for each type of Db
		public SqlDbViewer SqlBankViewer = ( SqlDbViewer ) null;
		public SqlDbViewer SqlCustViewer = ( SqlDbViewer ) null;
		public SqlDbViewer SqlDetViewer = ( SqlDbViewer ) null;

		public EditDb SqlCurrentEditViewer = ( EditDb) null;
		public DataGrid SqlCurrentEditGrid = null;

		public Guid [ ] ListBoxId = { Guid . Empty, Guid . Empty, Guid . Empty };
		public Guid Bankviewer = Guid . Empty;
		public Guid Custviewer = Guid . Empty;
		public Guid Detviewer = Guid . Empty;
		public Guid SqlViewerGuid = Guid . Empty;

		public int ViewerSelectiontype = -1;    // flag  for return value from selection window
							//total viewers open right now
		public int ViewerCount = 0;
		public int SelectedViewerType = 0;

		public  static void CheckResetAllGridViewData ( string KillType, SqlDbViewer caller , bool mode = true)
		{
			if ( !mode )
			{
				// CLEAR DOWN all data entries for the current Viewer
				// as it is probably chnaging to a different Db
				SqlDbViewer sqlv = caller;
				for ( int z = 0 ; z < 3 ; z++ )
				{
					if ( MainWindow . gv . window [ z ] == sqlv )
					{
						MainWindow . gv . ViewerCount--;
						MainWindow . gv . ListBoxId [ z ] = Guid . Empty;
						MainWindow . gv . Datagrid [ z ] = null;
						MainWindow . gv . window [ z ] = null;
						if ( KillType  == "BANKACCOUNT" )
						{
							MainWindow . gv . Bankviewer = Guid . Empty;
							MainWindow . gv . SqlBankViewer = null;
							MainWindow . gv . Bankviewer = Guid . Empty;
						}
						else if ( KillType == "CUSTOMER" )
						{
							MainWindow . gv . Custviewer = Guid . Empty;
							MainWindow . gv . SqlCustViewer = null;
							MainWindow . gv . Custviewer = Guid . Empty;
						}
						else if ( KillType == "DETAILS" )
						{
							MainWindow . gv . Detviewer = Guid . Empty;
							MainWindow . gv . SqlDetViewer = null;
							MainWindow . gv . Detviewer = Guid . Empty;
						}
						// general flags
						MainWindow . gv . CurrentDb [ z ] = "";
						MainWindow . gv . PrettyDetails = "";
						MainWindow . gv . SqlViewerWindow = null;
						MainWindow . gv . SqlViewerGuid = Guid . Empty;
						MainWindow . gv . SqlCurrentEditViewer = null;
						break;
					}
				}
			}
			else
			{
				// individual set only
				for ( int x = 0 ; x < 3 ; x++ )
				{
					if ( MainWindow . gv . CurrentDb [ x ] == KillType )
					{
						MainWindow . gv . ViewerCount--;
						MainWindow . gv . ListBoxId [ x ] = Guid . Empty;
						MainWindow . gv . Datagrid [ x ] = null;
						MainWindow . gv . window [ x ] = null;
						if ( KillType == "BANKACCOUNT" )
						{
							MainWindow . gv . Bankviewer = Guid . Empty;
							MainWindow . gv . SqlBankViewer = null;
							MainWindow . gv . Bankviewer = Guid . Empty;
						}
						else if ( KillType == "CUSTOMER" )
						{
							MainWindow . gv . Custviewer = Guid . Empty;
							MainWindow . gv . SqlCustViewer = null;
							MainWindow . gv . Custviewer = Guid . Empty;
						}
						else if ( KillType  == "DETAILS" )
						{
							MainWindow . gv . Detviewer = Guid . Empty;
							MainWindow . gv . SqlDetViewer = null;
							MainWindow . gv . Detviewer = Guid . Empty;
						}
						// general flags
						MainWindow . gv . CurrentDb [ x ] = "";
						MainWindow . gv . PrettyDetails = "";
						MainWindow . gv . SqlViewerWindow = null;
						MainWindow . gv . SqlViewerGuid = Guid . Empty;
						MainWindow . gv . SqlCurrentEditViewer = null;
						break;
					}
				}
			}
		}

	}
	/*
GridViewer Viewer_instance = new GridViewer ( );
int MaxViewers
Window window

string CurrentDb [ ]
string PrettyDetails
DataGrid Datagrid [ ]
DbSelector DbSelectorWindow 
SqlDbViewer  SqlViewerWindow
SqlDbViewer SqlBankViewer
SqlDbViewer SqlCustViewer
SqlDbViewer SqlDetViewer

Guid  ListBoxId [ ]
Guid Bankviewer
Guid Custviewer
Guid Detviewer 
Guid SqlViewerGuid

int ViewerSelectiontype
int ViewerCount 
int SelectedViewerType 
*/
}
/*
 MainWindow.gv structure
Maximum is 10

Window[ ] window
string[ ] CurrentDb 
string PrettyDetails
int[ ] ListBoxId 
DataGrid[ ] Datagrid
DbSelector DbSelectorWindow
int ViewerSelectiontype 
int ViewerCount 
int SelectedViewerType
*/
