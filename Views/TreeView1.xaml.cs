using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . ComponentModel;
using System . Diagnostics;
using System . Globalization;
using System . IO;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Media;

using WPFPages . ViewModels;

namespace WPFPages . Views
{
	public class bank
	{
		public string  custno;
		public string bankno;
		public int actype;
		public decimal balance;
		public DateTime odate;
		public DateTime cdate;
		public bank ( )
		{
			bankscollection = new ObservableCollection<bank> ( );
		}
		public static ObservableCollection<bank> bankscollection {get; set;}

		public static string SecCustno
		{
			get; set;
		}
		public static string SecBankno
		{
			get; set;
		}
		public static string SecActype
		{
			get; set;
		}
		public static string SecBalance
		{
			get; set;
		}
		public static string SecOdate
		{
			get; set;
		}
		public static string SecCdate
		{
			get; set;
		}

	}


	public class bankdatacollection : ObservableCollection<bank>
	{
		public bankdatacollection ( ) : base ( )
		{
		}
	}
	public class DriveDirectory
	{
		public DriveDirectory ( )
		{
		}
		private DriveInfo drive;
		public DriveInfo Drive
		{
			get
			{
				return drive;
			}
			set
			{
				drive = value;
			}
		}

		private DirectoryInfo directory;
		public DirectoryInfo Directory
		{
			get
			{
				return directory;
			}
			set
			{
				directory = value;
			}
		}
	}

	public partial class TreeView1 : Window
	{

//		public bank Bankacctsprimary = new bank ( );
//		public bank Bankacctssecondary = new bank ( );

		public  bankdatacollection bprimarycollection = new bankdatacollection ( );
		public  bankdatacollection bsecondarycollection = new bankdatacollection ();

//		bankdatacollection bcollection = new bankdatacollection ( );

		// Data structure to hold names of subfolders to be
		// examined for files.
		public static DriveInfo [ ] AllDrives;
		public static DirectoryInfo [ ] AllDirs;
		public static FileInfo [ ] AllFiles;
		public static DriveDirectory dd = new DriveDirectory ( );
		private readonly object dummyNode = null;
		private int LeftButtoncount = 0;
		string data = "";
		private string DrvString = "";
		private string DirString = "";


		//		List<BankAccountViewModel> multiaccounts = new List<BankAccountViewModel> ( );

		public TreeView1 ( )
		{
			InitializeComponent ( );
			Utils . SetupWindowDrag ( this );

//			load primary & Secondary records (top level and 2nd level of multi accounts)
			GetBankPrimaryData ( );

			// Load primary data from our primary collection into treeview
			LoadTreeview ( bprimarycollection );
			foldersItem . DataContext = bprimarycollection;
			
			//foldersItem . ItemsSource = bprimarycollection;
		}
		public void GetBankPrimaryData ( )
		{
			string path = @"C:\users\ianch\documents\multiaccounts.dat";

			string data = System . IO . File . ReadAllText ( path );
			string [ ] datas = data . Split ( '\n' );
			int x = 0;
			string prevcustno = "";
			bool isprimary = false;
			foreach ( var item in datas )
			{
				BankAccountViewModel bvm = new BankAccountViewModel ( );
				bank b = new bank ( );
				string [ ] s = datas [ x++ ] . Split ( ',' );
				if ( s [ 0 ] . Length == 0 )
					break;
				bvm . CustNo = s [ 0 ];
				bvm . BankNo = s [ 1 ];
				bvm . AcType = Convert . ToInt32 ( s [ 2 ] );
				bvm . Balance = Convert . ToDecimal ( s [ 3 ] );
				bvm . ODate = Convert . ToDateTime ( s [ 4 ] );
				bvm . CDate = Convert . ToDateTime ( s [ 5 ] );
				b . custno = bvm . CustNo;
				b . bankno = bvm . BankNo;
				b . actype = bvm . AcType;
				b . balance = bvm . Balance;
				b . odate = bvm . ODate;
				b . cdate = bvm . CDate;
				if ( prevcustno == b . custno )
				{
					bsecondarycollection . Add ( b );
					isprimary = false;
				}
				else
				{
					bprimarycollection . Add ( b );
					//bank bk = new bank ( );
					bank.bankscollection . Add ( b );
					isprimary = true;
					prevcustno = bvm . CustNo;
				}
			}
		}
		public void LoadTreeview ( bankdatacollection bprimarycollection )
		{
			foreach ( var item in bprimarycollection )
			{
				TreeViewItem tv = new TreeViewItem ( );
				string header = item . custno;
				tv . Header = header;
				tv . Tag = item . custno;
				tv . FontWeight = FontWeights . Normal;
				//				item . Items . DetachFromSourceCollection ( );
				tv . Items . Add ( dummyNode );
				tv . Expanded += new RoutedEventHandler ( folder_Expanded );
				foldersItem . Items . Add ( tv );
			}
		}


		/// <summary>
		/// used for loading Dirs/Directories etc only
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Loaded ( object sender, RoutedEventArgs e )
		{
			// Load Drives/Directories/Files
//			foreach ( string s in Directory . GetLogicalDrives ( ) )
//			{
//				TreeViewItem item = new TreeViewItem ( );
//				item . Header = s;
//				item . Tag = s;
//				item . FontWeight = FontWeights . Normal;
////				item . Items . DetachFromSourceCollection ( );
//				item . Items . Add ( dummyNode );
//				item . Expanded += new RoutedEventHandler ( folder_Expanded );
//				foldersItem . Items . Add ( item );
//				Utils . SetupWindowDrag ( this );

//			}
			// load multi account data
			//string path = @"C:\users\ianch\documents\multiaccounts.dat";
			//data = System . IO . File . ReadAllText ( path );
			//string [ ] datas = data . Split ( '\n' );
			//int x = 0;
			//string prevcustno = "";
			//bool isprimary = false;
			//foreach ( var item in datas )
			//{
			//	BankAccountViewModel bvm = new BankAccountViewModel ( );
			//	string [ ] s = datas [ x++ ].Split ( ',' );
			//	if ( s[0]. Length == 0 )
			//		break;
			//	bvm . CustNo = s [ 0 ];
			//	bvm . BankNo = s [ 1 ];
			//	bvm . AcType = Convert.ToInt32(s [ 2 ]);
			//	bvm . Balance = Convert.ToDecimal(s [ 3 ]);
			//	bvm . ODate = Convert . ToDateTime ( s [ 4 ] );
			//	bvm . CDate = Convert . ToDateTime ( s [ 5 ] );
			//	bank pbank = new bank ( );
			//	pbank . custno = bvm . CustNo;
			//	pbank . bankno = bvm . BankNo;
			//	pbank . actype = bvm . AcType;
			//	pbank . balance = bvm . Balance;
			//	pbank . odate = bvm . ODate;
			//	pbank . cdate = bvm . CDate;
			//	//Split data into 2 collections
			//	if ( prevcustno == bvm . CustNo )
			//	{
			//		Bankacctssecondary . Add ( pbank );
			//		isprimary = false;
			//	}
			//	else
			//	{
			//		Bankacctsprimary . Add ( pbank );
			//		isprimary = true;
			//		prevcustno = bvm . CustNo;
			//	}
			//		multiaccounts . Add ( bvm );
			//	if ( isprimary )
			//	{
			//		TreeViewItem tv = new TreeViewItem ( );
			//		string header = bvm . CustNo;
			//		tv . Header = header;
			//		tv . Tag = bvm . CustNo;
			//		tv . FontWeight = FontWeights . Normal;
			//		//				item . Items . DetachFromSourceCollection ( );
			//		tv . Items . Add ( dummyNode );
			//		tv . Expanded += new RoutedEventHandler ( folder_Expanded );
			//		foldersItem . Items . Add ( tv );
			//	}
			//}
		}


		/// <summary>
		/// finds and laod the relevant sub accounts form SecondaryBankaccounts
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void folder_Expanded ( object sender, RoutedEventArgs e )
		{
			// load directory contents - unused
			{

				//TreeViewItem item = ( TreeViewItem ) sender;
				//if ( item . Items . Count == 1 && item . Items [ 0 ] == dummyNode )
				//{
				//	item . Items . Clear ( );
				//	try
				//	{
				//		foreach ( string s in Directory . GetDirectories ( item . Tag . ToString ( ) ) )
				//		{
				//			TreeViewItem subitem = new TreeViewItem ( );
				//			subitem . Header = s . Substring ( s . LastIndexOf ( "\\" ) + 1 );
				//			subitem . Tag = s;
				//			subitem . FontWeight = FontWeights . Normal;
				//			subitem . Items . Add ( dummyNode );
				//			subitem . Expanded += new RoutedEventHandler ( folder_Expanded );
				//			item . Items . Add ( subitem );
				//			DrvString = item . Tag . ToString ( );
				//		}
				//	}
				//	catch ( Exception ) { }
				//	//Drops thru to get any files that may also be there
				//	try
				//	{
				//		foreach ( string s in Directory . GetFiles ( item . Tag . ToString ( ) ) )
				//		{
				//			TreeViewItem subitem = new TreeViewItem ( );
				//			subitem . Header = s . Substring ( s . LastIndexOf ( "\\" ) + 1 );
				//			subitem . Tag = s;
				//			subitem . FontWeight = FontWeights . Normal;
				//			subitem . Expanded += new RoutedEventHandler ( folder_Expanded );
				//			item . Items . Add ( subitem );
				//		}
				//		DirString = item . Tag . ToString ( );
				//	}
				//	catch ( Exception ) { }
				//}
			}
			TreeViewItem item = ( TreeViewItem ) sender;

			bool isdone = false;
			item . Items . Clear ( );
			try
			{
				TreeViewItem subitem = new TreeViewItem ( );
				foreach ( var secacct in bsecondarycollection )
				{
					if ( secacct . custno == item . Header as string )
					{
						string entry = "";
						entry = secacct . custno + "  "
							+ secacct . bankno + "  "
							+ secacct . actype + "  "
							+ secacct . balance + "  "
							+ secacct . odate . ToShortDateString ( ) + "  "
							+ secacct . cdate . ToShortDateString ( );
						bank . SecCustno = secacct . custno;
						bank . SecBankno = secacct . bankno;
						bank . SecActype = secacct . actype.ToString();
						bank . SecBalance = secacct . balance.ToString();
						bank . SecOdate = Convert . ToDateTime ( secacct . odate) . ToShortDateString ( );						
						bank . SecCdate = Convert . ToDateTime(secacct . cdate).ToShortDateString();
						

						subitem . Header = bank . SecCustno;
						subitem . Header += "  ";
						subitem . Header += bank . SecBankno;
						subitem . Header += "  ";
						subitem . Header += bank . SecActype;
						subitem . Header += "  ";
						subitem . Header += bank . SecBalance;
						subitem . Header += "  ";
						subitem . Header += bank . SecOdate;
						subitem . Header += "  ";
						subitem . Header += bank . SecCdate;
						//subitem.
						subitem . Tag = bank . SecCustno;
						subitem . FontWeight = FontWeights . Bold;
						//only needed for entries with subitems - creates the correct icon
						//subitem . Items . Add ( dummyNode );
						subitem . Expanded += new RoutedEventHandler ( folder_Expanded );
						item . Items . Add ( subitem );
					}
				}
			}
			catch ( Exception ) { }
				{

					//Drops thru to get any files that may also be there
					//\				try
					//				{
					//					foreach ( string s in Directory . GetFiles ( item . Tag . ToString ( ) ) )
					//					{
					//						TreeViewItem subitem = new TreeViewItem ( );
					//						subitem . Header = s . Substring ( s . LastIndexOf ( "\\" ) + 1 );
					//						subitem . Tag = s;
					//						subitem . FontWeight = FontWeights . Normal;
					//						subitem . Expanded += new RoutedEventHandler ( folder_Expanded );
					//						item . Items . Add ( subitem );
					//					}
					//					DirString = item . Tag . ToString ( );
					//				}
				}
//			}
		}

		public void TreeViewItem_Expanded ( object sender, RoutedEventArgs e )
		{
		//	TreeViewItem item = ( TreeViewItem ) sender;
		//	if ( item . Items . Count == 1 && item . Items [ 0 ] == dummyNode )
		//	{
		//		item . Items . Clear ( );
		//		try
		//		{
		//			foreach ( string s in Directory . GetDirectories ( item . Tag . ToString ( ) ) )
		//			{
		//				TreeViewItem subitem = new TreeViewItem ( );
		//				subitem . Header = s . Substring ( s . LastIndexOf ( "\\" ) + 1 );
		//				subitem . Tag = s;
		//				subitem . FontWeight = FontWeights . Normal;
		//				subitem . Items . Add ( dummyNode );
		//				subitem . Expanded += new RoutedEventHandler ( folder_Expanded );
		//				item . Items . Add ( subitem );
		//			}
		//		}
		//		catch ( Exception ) { }
		//	}
		//	{

		//		//string path = "";
		//		//TreeViewItem item = e . Source as TreeViewItem;
		//		//string rootPath = "";
		//		//if ( ( item . Items . Count == 1 ) && ( item . Items [ 0 ] is string ) )
		//		//{
		//		//	// Only contiue in here if item is Empty (Placeholder only)
		//		//	item . Items . Clear ( );

		//		//	DirectoryInfo expandedDir = null;
		//		//	// Get contents of a Drive from top level
		//		//	if ( item . Tag is DriveInfo )
		//		//	{
		//		//		dd . Drive = item . Tag as DriveInfo;
		//		//		rootPath = item . Tag . ToString ( );
		//		//		expandedDir = ( item . Tag as DriveInfo ) . RootDirectory;
		//		//	}
		//		//	// Get contents of a Folder at any level
		//		//	if ( item . Tag is DirectoryInfo )
		//		//	{
		//		//		dd . Directory = item . Tag as DirectoryInfo;
		//		//		path = dd . Drive . ToString ( ) + dd.Directory.ToString();
		//		//		expandedDir = ( item . Tag as DirectoryInfo );
		//		//		//TraverseTree ( path);
		//		//	}
		//		//	else if ( item . Tag is FileInfo )
		//		//	{

		//		//		Directory . GetFiles ( item . Tag as string );
		//		//	}
		//		//	try
		//		//	{
		//		//		//Actually load the relevant folders/Files as decided above
		//		//		foreach ( DirectoryInfo subDir in expandedDir . GetDirectories ( ) )	
		//		//			item . Items . Add ( CreateTreeItem ( subDir ) );
		//		//		if ( item . Items . Count == 0 )
		//		//		{
		//		//			string [ ] files;
		//		//			path = dd . Drive . ToString ( ) + dd . Directory . ToString ( ) + "\\";
		//		//			Debug . WriteLine ($"{path}");
		//		//			files = System . IO . Directory . GetFiles ( path );
		//		//			foreach ( var file in files)
		//		//			{
		//		//				//item . Items . Add ( CreateTreeItem ( file) );
		//		//				TreeViewItem tv = new TreeViewItem ( );
		//		//				//The actual Drive/Folder/File Name as a string
		//		//				tv . Header = file;
		//		//				//Save the object to the Tag (Drive/Folder/File) so we can identify it later 
		//		//				string [ ] strings = file . Split ( '\\' );
		//		//				int selection = strings . Length - 1;
		//		//				tv . Tag = strings[selection];
		//		//				item . Items . Add ( tv.Tag );

		//		//			}
		//		//		}
		//		//	}
		//		//	catch(Exception ex) {
		//		//		Debug . WriteLine (	$"{ex.Message}, {ex.Data}");
		//		//	}
		//		//	int t = 0;
		//		//}
		//		//else
		//		//{
		//		//	dd . Drive = null;
		//		//	dd . Directory= null;
		//		//	if ( item . Tag as DriveInfo != null )
		//		//		dd . Drive = item . Tag as DriveInfo;
		//		//	if( item . Tag as DirectoryInfo != null)
		//		//		dd . Directory = item . Tag as DirectoryInfo;
		//		//	if ( item . Tag is DriveInfo )
		//		//	{
		//		//		dd . Directory = item . Tag as DirectoryInfo;
		//		//		path = dd . Drive . ToString ( );
		//		//		try
		//		//		{
		//		//			string [ ] files;
		//		//			path = dd . Drive . ToString ( );
		//		//			files = System . IO . Directory . GetDirectories( path );
		//		//			foreach ( var file in files )
		//		//			{
		//		//				//item . Items . Add ( CreateTreeItem ( file) );
		//		//				TreeViewItem tv = new TreeViewItem ( );
		//		//				//The actual Drive/Folder/File Name as a string
		//		//				tv . Header = file;
		//		//				//Save the object to the Tag (Drive/Folder/File) so we can identify it later 
		//		//				string [ ] strings = file . Split ( '\\' );
		//		//				int selection = strings . Length - 1;
		//		//				tv . Tag = strings [ selection ];
		//		//				item . Items . Add ( tv . Tag );

		//		//			}
		//		//		}
		//		//		catch { }
		//		//	}
		//		//	if ( item . Tag is DirectoryInfo )
		//		//	{
		//		//		dd . Directory = item . Tag as DirectoryInfo;
		//		//		path = dd . Drive . ToString ( ) + dd . Directory . ToString ( );

		//		//		try
		//		//		{
		//		//			string [ ] files;
		//		//			path = dd . Drive . ToString ( ) + item.Tag. ToString ( );
		//		//			files = System . IO . Directory . GetFiles ( path );
		//		//			foreach ( var file in files )
		//		//			{
		//		//				//item . Items . Add ( CreateTreeItem ( file) );
		//		//				TreeViewItem tv = new TreeViewItem ( );
		//		//				//The actual Drive/Folder/File Name as a string
		//		//				tv . Header = file;
		//		//				//Save the object to the Tag (Drive/Folder/File) so we can identify it later 
		//		//				string [ ] strings = file . Split ( '\\' );
		//		//				int selection = strings . Length - 1;
		//		//				tv . Tag = strings [ selection ];
		//		//				item . Items . Add ( tv . Tag );

		//		//			}
		//		//		}
		//		//		catch { }
		//		//	}
		//	}
		}

		private void foldersItem_SelectedItemChanged ( object sender, RoutedPropertyChangedEventArgs<object> e )
		{
			int index = 0;
			if ( foldersItem . Items . Count >= 0 )
			{
				var tree = sender as TreeView;
				if ( tree . SelectedValue != null )
				{
					index++;
					TreeViewItem item = tree . SelectedItem as TreeViewItem;
					Debug . WriteLine ( $"Name={item . Name} at index {index}" );
					ItemsControl parent = ItemsControl . ItemsControlFromItemContainer ( item );
					//while ( parent != null && parent . GetType ( ) == typeof ( TreeViewItem ) )
					//{
					//	index++;
					//	parent = ItemsControl . ItemsControlFromItemContainer ( parent );
					//	Debug . WriteLine ( $"Parent={parent . Name} at index {index}" );
					//}
				}
			}

		}


		//private void TrvStructure_SelectedItemChanged ( object sender, RoutedPropertyChangedEventArgs<object> e )
		//{
		//	var s = sender as TreeView;
		//	int count = s . Items . Count;
		//	var v = s . SelectedItem . ToString ( );
		//	var z = e . Source;
		//	Debug . WriteLine ( $"Selection = {e . Source}" );
		//}

		//private void TrvStructure_Collapsed ( object sender, RoutedEventArgs e )
		//{

		//	if ( dd . Drive!= null )
		//	{
		//		// we are closing Directory tree
		//		dd . Drive = null;
		//	}
		//	else if ( dd . Directory != null )
		//	{
		//		// we are closing FILES list
		//		dd . Directory = null;
		//	}
		//}
		//END - STAGE3 example - works


		//var drives = DriveInfo . GetDrives ( )
		//	. Select ( x => new DriveInfo ( x . ToString ( ) ) );

		//var dirs = Directory . GetDirectories ( path )
		//		    . Select ( x => new DirectoryInfo ( x ) )
		//		    . Select ( x => new FileViewModel ( )
		//		    {
		//			    Name = x . Name,
		//			    Path = x . FullName,
		//			    IsFile = false,

		//		    } );

		//var files = Directory . GetFiles ( path )
		//		       . Select ( x => new FileInfo ( x ) )
		//		       . Select ( x => new FileViewModel ( )
		//		       {
		//			       Name = x . Name,
		//			       Path = x . FullName,
		//			       Size = x . Length,
		//			       IsFile = true,
		//		       } );

		//DataContext = dirs . Concat ( files ) . ToList ( );
		//this . LoadDirectories ( );
		//TestTree . ItemsSource = DataContext;
		//this.ExpandSubtree ( );



		private TreeViewItem GetItem ( DirectoryInfo directory )
		{
			var item = new TreeViewItem
			{
				Header = directory . Name,
				DataContext = directory,
				Tag = directory
			};
//			this . AddDummy ( item );
			//item . Expanded += new RoutedEventHandler ( item_Expanded );
			return item;
		}

		private TreeViewItem GetItem ( FileInfo file )
		{
			var item = new TreeViewItem
			{
				Header = file . Name,
				DataContext = file,
				Tag = file
			};
			return item;
		}


		private void ExploreDirectories ( TreeViewItem item )
		{
			var directoryInfo = ( DirectoryInfo ) null;
			if ( item . Tag is DriveInfo )
			{
				directoryInfo = ( ( DriveInfo ) item . Tag ) . RootDirectory;
			}
			else if ( item . Tag is DirectoryInfo )
			{
				directoryInfo = ( DirectoryInfo ) item . Tag;
			}
			else if ( item . Tag is FileInfo )
			{
				directoryInfo = ( ( FileInfo ) item . Tag ) . Directory;
			}
			if ( object . ReferenceEquals ( directoryInfo, null ) )
				return;
			foreach ( var directory in directoryInfo . GetDirectories ( ) )
			{
				var isHidden = ( directory . Attributes & FileAttributes . Hidden ) == FileAttributes . Hidden;
				var isSystem = ( directory . Attributes & FileAttributes . System ) == FileAttributes . System;
				if ( !isHidden && !isSystem )
				{
					item . Items . Add ( this . GetItem ( directory ) );
				}
			}
		}

		private void ExploreFiles ( TreeViewItem item )
		{
			var directoryInfo = ( DirectoryInfo ) null;
			if ( item . Tag is DriveInfo )
			{
				directoryInfo = ( ( DriveInfo ) item . Tag ) . RootDirectory;
			}
			else if ( item . Tag is DirectoryInfo )
			{
				directoryInfo = ( DirectoryInfo ) item . Tag;
			}
			else if ( item . Tag is FileInfo )
			{
				directoryInfo = ( ( FileInfo ) item . Tag ) . Directory;
			}
			if ( object . ReferenceEquals ( directoryInfo, null ) )
				return;
			foreach ( var file in directoryInfo . GetFiles ( ) )
			{
				var isHidden = ( file . Attributes & FileAttributes . Hidden ) == FileAttributes . Hidden;
				var isSystem = ( file . Attributes & FileAttributes . System ) == FileAttributes . System;
				if ( !isHidden && !isSystem )
				{
					item . Items . Add ( this . GetItem ( file ) );
				}
			}
		}

		private void TextBlock_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			//_startPoint = e . GetPosition ( null );
			// Make sure the left mouse button is pressed down so we are really moving a record
			if(e.ClickCount == 2)
			{
				// Double clicked				
				TextBlock tvi = sender as TextBlock;
				string s = DrvString + "\\" + tvi . Text as string;
				// Call function  to execute this program/File as relevant
				SupportMethods . ProcessExecuteRequest ( this, null, null, s);
				return;
			}
		}

		private void TextBlock_PreviewMouseLeftButtonUp ( object sender, MouseButtonEventArgs e )
		{
		}

		private void TextBlock_PreviewMouseMove ( object sender, MouseEventArgs e )
		{
			Brush brs = Utils . GetDictionaryBrush ( "Red3" );
			Background = brs;
		}

		private void StackPanel_PreviewMouseMove ( object sender, MouseEventArgs e )
		{
			Brush brs = Utils . GetDictionaryBrush ( "Red3" );
			this.Background = brs;

		}
	}
	//======================================================================
	public class FileViewModel : INotifyPropertyChanged
	{
		private bool _isSelected;
		public bool IsSelected
		{
			get
			{
				return _isSelected;
			}
			set
			{
				_isSelected = value;
				OnPropertyChanged ( "IsSelected" );
			}
		}


		public string Name
		{
			get; set;
		}
		public long Size
		{
			get; set;
		}
		public string Path
		{
			get; set;
		}
		public bool IsFile
		{
			get; set;
		}
		public ImageSource Image
		{
			get; set;
		}
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged ( string propertyName )
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if ( handler != null )
				handler ( this, new PropertyChangedEventArgs ( propertyName ) );
		}
	}
}
