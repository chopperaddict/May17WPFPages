using System;
using System . Collections;
using System . Collections . Generic;
using System . Data;
using System . Data . SqlClient;
using System . Diagnostics;
using System . Linq;
using System . Reflection;
using System . Runtime . CompilerServices;
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
using WPFPages . Views;

using static System . Net . Mime . MediaTypeNames;

namespace WPFPages . Views
{

	//public class ExpandedState : Expander
	//{
	//	public ExpandedState ( ) : base ( ) { }
	//	public bool IsItemExpanded
	//	{
	//		get
	//		{
	//			return ( bool ) this . GetValue ( IsItemExpandedProperty );
	//		}
	//		set
	//		{
	//			this . SetValue ( IsItemExpandedProperty, value );
	//		}
	//	}

	//	public static readonly DependencyProperty
	//		IsItemExpandedProperty = DependencyProperty .
	//		Register ( "IsItemExpanded", typeof ( bool ), typeof ( Expander ), new PropertyMetadata ( true ) );

	//}

	/// <summary>
	/// Interaction logic for UserListBoxViewer.xaml
	/// </summary>
	public partial class UserListBoxViewer : Window
	{
		// Declare all 3 of the local Db pointers
		public BankCollection SqlBankcollection = new BankCollection ( );
		public BankCollection BackupBankcollection = new BankCollection ( );
		public CustCollection SqlCustcollection = new CustCollection ( );
		public DetCollection SqlDetcollection = new DetCollection ( );
		public static List<BankAccountViewModel> BankList = new List<BankAccountViewModel> ( );
		public List<CustomerViewModel> CustList = new List<CustomerViewModel> ( );
		public List<DetailsViewModel> DetList = new List<DetailsViewModel> ( );

		private bool ExpandAll = false;

		// DEFAULT Background colors
		private Brush UnselectedBackColor = new SolidColorBrush ( Color . FromArgb ( 0xFF, 0xFF, 0xFF, 0xFF ) );        //White
		private Brush SelectedBackColor = new SolidColorBrush ( Color . FromArgb ( 0xFF, 0x70, 0x09, 0xef ) );  // selected background
															//CURRENT Foreground color
		private Brush CurrentForeColor = new SolidColorBrush ( Color . FromArgb ( 0xFF, 0x00, 0x00, 0x00 ) );  // current foreground =black
		private Brush CurrentBackColor = new SolidColorBrush ( Color . FromArgb ( 0xFF, 0xFF, 0xFF, 0xFF ) );  // current foreground =White
		private string CurrentCellName = "";
		private bool areItemsExpanded;
		public bool AreItemsExpanded
		{
			get
			{
				return areItemsExpanded;
			}
			set
			{
				areItemsExpanded = value;
			}
		}

		private string tbBalance = "";
		public string TbBalance
		{
			get
			{
				return tbBalance;
			}
			set
			{
				tbBalance = value;
			}
		}
		// CURRENT Background color
		private Brush tbCurrentBrush = new SolidColorBrush ( Color . FromArgb ( 0xFF, 0xFF, 0xFF, 0xFF ) );
		public Brush TbCurrentBrush
		{
			get
			{
				return tbCurrentBrush;
			}
			set
			{
				tbCurrentBrush = value;
			}
		}

		private int currentindex;
		public int CurrentIndex
		{
			get
			{
				return currentindex;
			}
			set
			{
				currentindex = value;
			}
		}

		private bool isdirty;
		public bool IsDirty
		{
			get
			{
				return isdirty;
			}
			set
			{
				isdirty = value;
			}
		}
		private int gridSelection;
		public int GridSelection
		{
			get
			{
				return gridSelection;
			}
			set
			{
				gridSelection = value;
			}
		}
		private int listSelection;
		public int ListSelection
		{
			get
			{
				return listSelection;
			}
			set
			{
				listSelection = value;
			}
		}

		private bool itemExpanded = true;
		public bool ItemExpanded
		{
			get
			{
				return itemExpanded;
			}
			set
			{
				itemExpanded = value;
			}
		}
		private bool isSelected;
		public bool IsSelected
		{
			get
			{
				return isSelected;
			}
			set
			{
				isSelected = value;
			}
		}

		private TextBox tb;

		// Drag & Drop stuff
		private bool IsLeftButtonDown = false;
		private static Point _startPoint
		{
			get; set;
		}
		private static bool ScrollBarMouseMove
		{
			get; set;
		}

		////This is how to declare properties for binding
		//public TextBlock BalanceTextBlock
		//{
		//	get { return ( TextBlock ) GetValue ( BalanceTextblockProperty ); }
		//	set { SetValue ( BalanceTextblockProperty , (TextBlock)value ); }
		//}

		//// Using a DependencyProperty as the backing store for BaseDataText.  This enables animation, styling, binding, etc...
		//public static readonly DependencyProperty BalanceTextblockProperty =
		//	DependencyProperty . Register ( "BalanceTextBlock", typeof ( TextBox), typeof ( UserListBoxViewer), new PropertyMetadata ( default ) );


		public UserListBoxViewer ( )
		{
			InitializeComponent ( );
			//			ExpandedState es = new ExpandedState ( );
			//			es. IsItemExpanded= false;
			//			CheckTypes ( );
		}
		private void Window_Loaded ( object sender, RoutedEventArgs e )
		{
			this . Show ( );
			Utils . SetupWindowDrag ( this );
			EventControl . BankDataLoaded += EventControl_BankDataLoaded;
			Flags . SqlBankActive  = true;
			BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );

			//Events declared by my ListBox UserControl : DbListWindowControl 
			//DbListWindowControl . SelectedParams += DbListbox_SelectedParams;
			//DbListWindowControl . ClearBtnPressed += DbListWindowControl_ClearBtnPressed;
			//DbListWindowControl . LoadBtnPressed += DbListWindowControl_LoadBtnPressed;
		}


		private void Select ( int low, int high, int total )
		{
			DataTable dtBank = new DataTable ( );
			//DbListbox . UCListbox . ItemsSource = null;
			//DbListbox . UCListbox . Items . Clear ( );
			//DbListbox . UCListbox . Refresh ( );
			//DbListbox . UCListbox . UpdateLayout ( );
			//Mouse . OverrideCursor = System . Windows . Input . Cursors . Wait;
			//SqlBankcollection . Clear ( );
			//dtBank = DbListWindowControl . LoadBankData ( low, high, total );
			//SqlBankcollection = DbListWindowControl . LoadBankTest ( SqlBankcollection, dtBank );
			//DbListbox . UCListbox . ItemsSource = SqlBankcollection;
			//DbListbox . UCListbox . DataContext = SqlBankcollection;
			//DbListbox . ActiveType = "BANKACCOUNT";
			//DbListbox . UCListbox . SelectedIndex = 63;
			//DbListbox . BankRecord = DbListbox . UCListbox . SelectedItem as BankAccountViewModel;
			//Mouse . OverrideCursor = System . Windows . Input . Cursors . Arrow;
		}

		private void EventControl_BankDataLoaded ( object sender, LoadedEventArgs e )
		{
			// Only load if it was US that triggered the request
			// So we do not close all Expanders on the user via an auto load of new data
			//			if ( e . CallerType == "SELECTEDDATA" )
			//			{
			//Pass the data  to the UserControl to load into the ListBox
//			if ( sender == null )
//				return;
			Debug . WriteLine ( $"\n*** Loading Bank data in UserListboxViewer after BankDataLoaded trigger\n" );
			UCListbox . ItemsSource = e . DataSource as BankCollection;
			datagrid . ItemsSource = e . DataSource as BankCollection;
			UCListbox . SelectedIndex = 0;
			//				BackupBankcollection = e.DataSource as BankCollection;
			SqlBankcollection = e . DataSource as BankCollection;
			UCListbox . ItemsSource = SqlBankcollection;
			Mouse . OverrideCursor = System . Windows . Input . Cursors . Arrow;
			//			}
		}

		private void Window_Closed ( object sender, EventArgs e )
		{
			//DbListWindowControl . SelectedParams -= DbListbox_SelectedParams;
			//DbListWindowControl . ClearBtnPressed -= DbListWindowControl_ClearBtnPressed;
			//DbListWindowControl . LoadBtnPressed -= DbListWindowControl_LoadBtnPressed;

		}

		private async void DbList_LoadBtnPressed ( object sender, RoutedEventArgs e )
		{
		}

		#region DATA LOAD FUNCTIONS
		public static BankCollection LoadBankTest ( BankCollection temp, DataTable dtBank )
		{

			//try
			//{
			int x = dtBank . Rows . Count;
			int i = 0;
			Func<int, int, bool> action = (i,x) =>
		       {
			       while ( i++ < x )
			       {
				       temp . Add ( new BankAccountViewModel
				       {
					       Id = Convert . ToInt32 ( dtBank . Rows [ i ] [ 0 ] ), BankNo = dtBank . Rows [ i ] [ 1 ] . ToString ( ),
					       CustNo = dtBank . Rows [ i ] [ 2 ] . ToString ( ), AcType = Convert . ToInt32 ( dtBank . Rows [ i ] [ 3 ] ),
					       Balance = Convert . ToDecimal ( dtBank . Rows [ i ] [ 4 ] ), IntRate = Convert . ToDecimal ( dtBank . Rows [ i ] [ 5 ] ),
					       ODate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 6 ] ), CDate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 7 ] ),
				       } );
			       }
			       return true;
		       };
			action ( 0, x );

			//for ( int i = 0 ; i < dtBank . Rows . Count ; i++ )
			//{
			//	temp . Add ( new BankAccountViewModel
			//	{
			//		Id = Convert . ToInt32 ( dtBank . Rows [ i ] [ 0 ] ), BankNo = dtBank . Rows [ i ] [ 1 ] . ToString ( ),
			//		CustNo = dtBank . Rows [ i ] [ 2 ] . ToString ( ), AcType = Convert . ToInt32 ( dtBank . Rows [ i ] [ 3 ] ),
			//		Balance = Convert . ToDecimal ( dtBank . Rows [ i ] [ 4 ] ), IntRate = Convert . ToDecimal ( dtBank . Rows [ i ] [ 5 ] ),
			//		ODate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 6 ] ), CDate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 7 ] ),
			//	} );
			//}

			//}
			//catch ( Exception ex )
			//{
			//	Debug . WriteLine ( $"BANK : SQL Error in BankCollection load function : {ex . Message}, {ex . Data}" );
			//	MessageBox . Show ( $"BANK : SQL Error in BankCollection load function : {ex . Message}, {ex . Data}" );
			//}
			//finally
			//{
			//	Debug . WriteLine ( $"BANK : Completed load into Bankcollection :  {temp . Count} records loaded successfully ...." );
			//}
			//return temp;
			return null;
		}
		#endregion DATA LOAD FUNCTIONS

		private void ClearBtn_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			// clear listbox.
			UCListbox . ItemsSource = null;
			UCListbox . Items . Clear ( );
			DbList_LoadBtnPressed ( null, null );
			UCListbox . Refresh ( );
		}

		private void OpenAllBtn_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			// Open/Expand all entries
			ExpandAll = true;
			int count = UCListbox . Items . Count;
			ListBoxItem lbi = new ListBoxItem ( );
			for ( int item = 0 ; item < count ; item++ )
			{
				UCListbox . SelectedIndex = item;
				lbi = ( ListBoxItem ) UCListbox . ItemContainerGenerator . ContainerFromIndex ( UCListbox . SelectedIndex );
				if ( lbi != null )
					lbi . IsSelected = true;
				ItemExpanded = true;
				lbi . Refresh ( );
				UCListbox . Refresh ( );
				AreItemsExpanded = true;
			}
		}
		private void LbiExpander_Expanded ( object sender, RoutedEventArgs e )
		{
			// triggered whenever an item is expanded
			Expander ex = new Expander ( );
			AreItemsExpanded = ex . IsExpanded;

		}

		private void UCListbox_Selected ( object sender, RoutedEventArgs e )
		{
			Expander ex = new Expander ( );
			if ( ExpandAll )
			{
				AreItemsExpanded = true;
				LbiExpander_Expanded ( null, null );
			}
		}

		private void LbiExpander_Collapsed ( object sender, RoutedEventArgs e )
		{
			// triggered whenever an item is collapsed
			Expander ex = new Expander ( );
			AreItemsExpanded = ex . IsExpanded;
		}

		private void TextBox_PreviewKeyDown ( object sender, KeyEventArgs e )
		{
			BankAccountViewModel bvm = new BankAccountViewModel ( );
			// we are in a TextBlock or TextBox of the ListView
			if ( e . Key == Key . Enter || e . Key == Key . Tab )
			{
				string t = sender . GetType ( ) . ToString ( );
				if ( t . Contains ( "TextBox" ) || t . Contains ( "TextBlock" ) )
				{
					// in a listview !!
					if ( listSelection > -1 )
						bvm = UCListbox . SelectedItem as BankAccountViewModel;
				}
				if ( bvm != null )
					BankCollection . UpdateBankDb ( bvm, "BANKACCOUNT" );

				EventControl. TriggerViewerDataUpdated ( this,  new LoadedEventArgs 
				{
					CallerType = "USERLISTBOXVIEWER",
					CallerDb = "BANKACCOUNT",
					DataSource = SqlBankcollection,
					RowCount = UCListbox. SelectedIndex,
					Bankno = bvm.BankNo,
					Custno = bvm.CustNo
				} );
			}

		}

		private void ToggleBtn_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			if ( UCListbox . Visibility == Visibility . Visible )
			{
				Utils . SetUpGridSelection ( datagrid, ListSelection );
				UCListbox . Visibility = Visibility . Hidden;
				datagrid . Visibility = Visibility . Visible;
				datagrid . SelectedIndex = ListSelection;
				datagrid . SelectedItem = GridSelection;
				datagrid . ScrollIntoView ( datagrid . SelectedItem );

				datagrid . Focus ( );
				datagrid . Refresh ( );
				datagrid . SelectedIndex = GridSelection;
				datagrid . SelectedItem = GridSelection;
			}
			else
			{
				Utils . SetUpGListboxSelection ( UCListbox, GridSelection );
				datagrid . Visibility = Visibility . Hidden;
				UCListbox . Visibility = Visibility . Visible;
				UCListbox . SelectedIndex = ListSelection;
				UCListbox . SelectedItem = GridSelection;
				UCListbox . Refresh ( );
				UCListbox . ScrollIntoView ( UCListbox . SelectedItem );
				UCListbox . Focus ( );
			}
			//var currentborder = ToggleViews . Child . GetValue ( ContentProperty ) ;
			//if ( datagrid . Visibility != Visibility . Visible )
			//	ToggleViews . Child .  SetValue ( ContentProperty, "Show in Grid View" );
			//else
			//	ToggleViews . Child . SetValue ( ContentProperty, "Show in List View" );


			// Nothing changes behaviour, it just shows text in default Black and small size
			//			Brush newbrush = new SolidColorBrush ( Color . FromArgb ( 255, ( byte ) 255, ( byte ) 255, ( byte ) 255 ) );
			//			BtnText . HorizontalAlignment = HorizontalAlignment . Center;
			//			BtnText . Background = newbrush;
		}
		private void DbListbox_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			try
			{
				ListViewItem lbi = new ListViewItem ( );
				List<TextBlock> tbList = new List<TextBlock> ( );
				ListView lv = sender as ListView;
				int sel = lv . SelectedIndex;
				var v = UCListbox . ItemContainerGenerator . ContainerFromIndex ( UCListbox . SelectedIndex );
				ListSelection = UCListbox . SelectedIndex;
			}
			catch ( Exception ex )
			{

			}
		}

		private void Datagrid_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			string t = sender . GetType ( ) . ToString ( );
			if ( t . Contains ( "DataGrid" ) )
			{
				GridSelection = datagrid . SelectedIndex;
				ListSelection = GridSelection;
				UCListbox . SelectedIndex = ListSelection;
			}
			else
			{
			}
		}

		private void LbItem_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			int x = 0;
			ListSelection = UCListbox . SelectedIndex;
			GridSelection = ListSelection;
		}

		private void Expander_Expanded ( object sender, RoutedEventArgs e )
		{
			Expander exp = sender as Expander;
			exp . IsExpanded = true;
		}

		private void Expander_Collapsed ( object sender, RoutedEventArgs e )
		{
			Expander exp = sender as Expander;
			exp . IsExpanded = false;
		}

		private void UCListbox_MouseDoubleClick ( object sender, MouseButtonEventArgs e )
		{
			ListBoxItem lbi = new ListBoxItem ( );
			ListBox lv = sender as ListBox;
			int sel = lv . SelectedIndex;
			lbi = ( ListBoxItem ) UCListbox . ItemContainerGenerator . ContainerFromIndex ( UCListbox . SelectedIndex );
			ListSelection = UCListbox . SelectedIndex;
			GridSelection = ListSelection;
			int count = 0;

		}

		private void UCListbox_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			// Store in a class variable
			CurrentIndex = UCListbox . SelectedIndex;
			ListSelection = CurrentIndex;
			Debug . WriteLine ( $"Index is set to {CurrentIndex}" );
		}

		private FrameworkElement FindByName ( string name, FrameworkElement root )
		{
			Stack<FrameworkElement> tree = new Stack<FrameworkElement> ( );
			tree . Push ( root );

			while ( tree . Count > 0 )
			{
				FrameworkElement current = tree . Pop ( );
				if ( current . Name == name )
					return current;

				int count = VisualTreeHelper . GetChildrenCount ( current );
				for ( int i = 0 ; i < count ; ++i )
				{
					DependencyObject child = VisualTreeHelper . GetChild ( current, i );
					if ( child is FrameworkElement )
						tree . Push ( ( FrameworkElement ) child );
				}
			}

			return null;
		}
		private void datagrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			DataGrid dg = new DataGrid ( );
			dg = e . Source as DataGrid;
			var dgr = dg . Items . CurrentItem;
			GridSelection = dg . SelectedIndex;
			ListSelection = GridSelection;
			var template = datagrid;
			TextBlock tb = ( TextBlock ) this . datagrid . FindName ( "custno2" );
			Brush newbrush = new SolidColorBrush ( Color . FromArgb ( 255, ( byte ) 255, ( byte ) 255, ( byte ) 255 ) );
		}

		private void CheckTypes ( )
		{

			//Type [ ] types = { typeof(Example), typeof(NestedClass),
			// typeof(INested), typeof(S) };

			Type [ ] types = { typeof ( UserListBoxViewer ),
			typeof(DataGrid),
			typeof(TextBlock)};

			foreach ( var t in types )
			{
				Console . WriteLine ( "Attributes for type {0}:", t . Name );

				TypeAttributes attr = t . Attributes;

				// To test for visibility attributes, you must use the visibility mask.
				TypeAttributes visibility = attr & TypeAttributes . VisibilityMask;
				switch ( visibility )
				{
					case TypeAttributes . NotPublic:
						Console . WriteLine ( "   ...is not public" );
						break;
					case TypeAttributes . Public:
						Console . WriteLine ( "   ...is public" );
						break;
					case TypeAttributes . NestedPublic:
						Console . WriteLine ( "   ...is nested and public" );
						break;
					case TypeAttributes . NestedPrivate:
						Console . WriteLine ( "   ...is nested and private" );
						break;
					case TypeAttributes . NestedFamANDAssem:
						Console . WriteLine ( "   ...is nested, and inheritable only within the assembly" +
						   "\n         (cannot be declared in C#)" );
						break;
					case TypeAttributes . NestedAssembly:
						Console . WriteLine ( "   ...is nested and internal" );
						break;
					case TypeAttributes . NestedFamily:
						Console . WriteLine ( "   ...is nested and protected" );
						break;
					case TypeAttributes . NestedFamORAssem:
						Console . WriteLine ( "   ...is nested and protected internal" );
						break;
				}

				// Use the layout mask to test for layout attributes.
				TypeAttributes layout = attr & TypeAttributes . LayoutMask;
				switch ( layout )
				{
					case TypeAttributes . AutoLayout:
						Console . WriteLine ( "   ...is AutoLayout" );
						break;
					case TypeAttributes . SequentialLayout:
						Console . WriteLine ( "   ...is SequentialLayout" );
						break;
					case TypeAttributes . ExplicitLayout:
						Console . WriteLine ( "   ...is ExplicitLayout" );
						break;
				}

				// Use the class semantics mask to test for class semantics attributes.
				TypeAttributes classSemantics = attr & TypeAttributes . ClassSemanticsMask;
				switch ( classSemantics )
				{
					case TypeAttributes . Class:
						if ( t . IsValueType )
						{
							Console . WriteLine ( "   ...is a value type" );
						}
						else
						{
							Console . WriteLine ( "   ...is a class" );
						}
						break;
					case TypeAttributes . Interface:
						Console . WriteLine ( "   ...is an interface" );
						break;
				}

				if ( ( attr & TypeAttributes . Abstract ) != 0 )
				{
					Console . WriteLine ( "   ...is abstract" );
				}

				if ( ( attr & TypeAttributes . Sealed ) != 0 )
				{
					Console . WriteLine ( "   ...is sealed" );
				}

				Console . WriteLine ( );

			}
		}

		private void Balancefield_PreviewMouseEnter ( object sender, MouseEventArgs e )
		{
		}

		private void Balancefield_PreviewMouseLeave ( object sender, MouseEventArgs e )
		{
		}

		private void Balancefield_MouseMove ( object sender, MouseEventArgs e )
		{
		}

		private void Datagrid_PreviewMouseMove ( object sender, MouseEventArgs e )
		{
			TextBlock tb = new TextBlock ( );
			tb = sender as TextBlock;

			if ( tb != null )
			{
				CurrentBackColor = tb . Background;
				CurrentForeColor = tb . Foreground;
				CurrentCellName = tb . Name;
//				Debug . WriteLine ( $"Name={CurrentCellName }, B = {CurrentBackColor}, F = {CurrentForeColor}" );
			}
		}

		private void ListView_PreviewMouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{
			ContextMenu cm = this . FindResource ( "ContextMenu2" ) as ContextMenu;
			cm . PlacementTarget = sender as ListView;
			cm . IsOpen = true;
		}

		public void FindChildren<T> ( List<T> results, DependencyObject startNode )
		  where T : DependencyObject
		{
			int count = VisualTreeHelper . GetChildrenCount ( startNode );
			for ( int i = 0 ; i < count ; i++ )
			{
				DependencyObject current = VisualTreeHelper . GetChild ( startNode, i );
				if ( ( current . GetType ( ) ) . Equals ( typeof ( T ) ) || ( current . GetType ( ) . GetTypeInfo ( ) . IsSubclassOf ( typeof ( T ) ) ) )
				{
					T asType = ( T ) current;
					results . Add ( asType );
				}
				FindChildren<T> ( results, current );
			}
		}

		private void Style_Selected ( object sender, RoutedEventArgs e )
		{
			int c = 0;
		}

		private void _Border_PreviewMouseDown ( object sender, MouseButtonEventArgs e )
		{
			//Click inside ListView Item
			Border brdr = sender as Border;
			//			ListView lv = GetParent ( ( Visual ) e . Source );
			//CurrentIndex = lv . SelectedIndex;
			//			object o = FindName("UCListbox");
			//			ListView LV = o as ListView;
			//			CurrentIndex  = LV . FocusedItem;
			//			CurrentIndex = LV . SelectedIndex;

		}
		private ListView GetParent ( Visual v )
		{
			while ( v != null )
			{
				v = VisualTreeHelper . GetParent ( v ) as Visual;
				if ( v is ListView )
					break;
			}
			return v as ListView;
		}

		private async void DbList_LoadBtnPressed ( object sender, MouseButtonEventArgs e )
		{
			int min = 0, max = 0, tot = 0;
			//			return;
			// Reset the background of the Load Data button
			//			Border b = sender as Border;
			//			if ( b == null )
			//				return;
			//			b . Background = FindResource ( "Gray3" ) as SolidColorBrush;
			//			b . BorderBrush = FindResource ( "Red3" ) as SolidColorBrush;
			DataTable dtBank = new DataTable ( );
			UCListbox . ItemsSource = null;
			UCListbox . Items . Clear ( );
			UCListbox . Refresh ( );
			UCListbox . UpdateLayout ( );
			Mouse . OverrideCursor = System . Windows . Input . Cursors . Wait;
			SqlBankcollection . Clear ( );
			min = Convert . ToInt32 ( MinValue . Text );
			max = Convert . ToInt32 ( MaxValue . Text );
			tot = Convert . ToInt32 ( MaxRecords . Text );
			dtBank = BankCollection . LoadSelectedBankData ( min, max, tot );
			await BankCollection . LoadSelectedCollection ( SqlBankcollection, -1, dtBank, true );

			Mouse . OverrideCursor = System . Windows . Input . Cursors . Arrow;

		}

		#region LINQ methods
		private void Linq1_Click ( object sender, RoutedEventArgs e )
		{
			//			BackupBankcollection = SqlBankcollection;
			LinqResults lq = new LinqResults ( );
			var accounts = from items in SqlBankcollection
				       where ( items . AcType == 1 )
				       orderby items . CustNo
				       select items;
			BankCollection vm = new BankCollection ( );
			foreach ( var item in accounts )
			{
				vm . Add ( item );
			}
			UCListbox . ItemsSource = null;
			UCListbox . Items . Clear ( );
			UCListbox . ItemsSource = vm;
		}

		private void Linq2_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			//			BackupBankcollection = SqlBankcollection;
			var accounts = from items in SqlBankcollection
				       where ( items . AcType == 2 )
				       orderby items . CustNo
				       select items;
			BankCollection vm = new BankCollection ( );
			foreach ( var item in accounts )
			{
				vm . Add ( item );
			}
			UCListbox . ItemsSource = null;
			UCListbox . Items . Clear ( );
			UCListbox . ItemsSource = vm;
		}

		private void Linq3_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			//			BackupBankcollection = SqlBankcollection;
			var accounts = from items in SqlBankcollection
				       where ( items . AcType == 3 )
				       orderby items . CustNo
				       select items;
			BankCollection vm = new BankCollection ( );
			foreach ( var item in accounts )
			{
				vm . Add ( item );
			}
			UCListbox . ItemsSource = null;
			UCListbox . Items . Clear ( );
			UCListbox . ItemsSource = vm;
		}
		private void Linq4_Click ( object sender, RoutedEventArgs e )
		{
			//select items;
			//			BackupBankcollection = SqlBankcollection;
			var accounts = from items in SqlBankcollection
				       where ( items . AcType == 4 )
				       orderby items . CustNo
				       select items;
			BankCollection vm = new BankCollection ( );
			foreach ( var item in accounts )
			{
				vm . Add ( item );
			}
			UCListbox . ItemsSource = null;
			UCListbox . Items . Clear ( );
			UCListbox . ItemsSource = vm;
		}

		/// <summary>
		/// Create a subset that only includes those cust acs with >1 bankaccounts
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Linq5_Click ( object sender, RoutedEventArgs e )
		{
			//select All the items first;
			//			BackupBankcollection = SqlBankcollection;
			var accounts = from items in SqlBankcollection orderby items . CustNo, items . BankNo select items;
			//Next Group collection on CustNo
			var grouped = accounts . GroupBy ( b => b . CustNo );

			//Now filter content down to only those a/c's with multiple Bank A/c's
			var sel = from g in grouped
				  where g . Count ( ) > 1
				  select g;

			// Finally, iterate thru the list of grouped CustNo's matching to CustNo in the full BankAccounts data
			// giving us ONLY the full records for any records that have > 1 Bank accounts
			List<BankAccountViewModel> output = new List<BankAccountViewModel> ( );

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
			BankCollection vm = new BankCollection ( );
			foreach ( var item in output )
			{
				vm . Add ( item );
			}
			UCListbox . ItemsSource = null;
			UCListbox . Items . Clear ( );
			UCListbox . ItemsSource = vm;
		}
		//*************************************************************************************************************//
		// Turn filter OFF
		/// <summary>
		/// Reset our viewer to FULL record display by reloading  the Db from disk - JIC
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Linq6_Click ( object sender, RoutedEventArgs e )
		{
			//			BackupBankcollection = null;
			SqlBankcollection = null;
			UCListbox . ItemsSource = null;
			Flags . SqlCustActive  = true;
			BankCollection . LoadBank ( SqlBankcollection, "SQLDBVIEWER", 1, true );
			UCListbox . Refresh ( );
		}



		#endregion LINQ methods
		private void ViewJsonRecord_Click ( object sender, RoutedEventArgs e )
		{
			//============================================//
			//MENU ITEM 'Read and display JSON File'
			//============================================//
			string Output = "";
			Mouse . OverrideCursor = Cursors . Wait;
			BankAccountViewModel bvm = this . UCListbox . SelectedItem as BankAccountViewModel;
			Output = JsonSupport . CreateShowJsonText ( true, "BANKACCOUNT", bvm, "BankAccountViewModel" );
			MessageBox . Show ( Output, "Currently selected record in JSON format", MessageBoxButton . OK, MessageBoxImage . Information, MessageBoxResult . OK );
		}

		private void linq1_IsMouseDirectlyOverChanged ( object sender, DependencyPropertyChangedEventArgs e )
		{
			int x = 0;
		}




		private void datagrid_PreviewDragEnter ( object sender, DragEventArgs e )
		{
			e . Effects = ( DragDropEffects ) DragDropEffects . Move;
			//Debug . WriteLine ( $"Setting drag cursor...." );
		}

		//================================================================================================
		#region DRAG CODE
		private void datagrid_PreviewMouseLeftButtonup ( object sender, MouseButtonEventArgs e )
		{
			ScrollBarMouseMove = false;

		}

		private void datagrid_PreviewMouseLeftButtondown ( object sender, MouseButtonEventArgs e )
		{

			// Gotta make sure it is not anywhere in the Scrollbar we clicked on 
			if ( Utils . HitTestScrollBar ( sender, e ) )
			{
				ScrollBarMouseMove = true;
				return;
			}
			if ( Utils . HitTestHeaderBar ( sender, e ) )
				return;

			_startPoint = e . GetPosition ( null );
			// Make sure the left mouse button is pressed down so we are really moving a record
			if ( e . LeftButton == MouseButtonState . Pressed )
			{
				IsLeftButtonDown = true;
			}
		string t = sender . GetType ( ) . ToString ( );
			if ( t . Contains ( "DataGrid" ) )
			{
				GridSelection = datagrid . SelectedIndex;
				ListSelection = GridSelection;
				UCListbox . SelectedIndex = ListSelection;
			}
			else
			{
				ListSelection = UCListbox. SelectedIndex;
				GridSelection = ListSelection;
				datagrid . SelectedIndex = ListSelection;
			}

		}

		private void datagrid_PreviewMouseMove ( object sender, MouseEventArgs e )
		{
			object Sender;
			Point mousePos = e . GetPosition ( null );
			Vector diff = _startPoint - mousePos;
			string t1 = sender . GetType ( ) . ToString ( );
			if ( e . LeftButton == MouseButtonState . Pressed &&
			    Math . Abs ( diff . X ) > SystemParameters . MinimumHorizontalDragDistance ||
			    Math . Abs ( diff . Y ) > SystemParameters . MinimumVerticalDragDistance )
			{
				if ( IsLeftButtonDown && e . LeftButton == MouseButtonState . Pressed )
				{
					bool isvalid = false;
					if ( t1 . Contains ( "ListView" ) )
						isvalid = true;
					else if ( t1 . Contains ( "DataGrid" ) )
						isvalid = true;
					if ( isvalid )
					{
						if ( ScrollBarMouseMove )
							return;
						// We are dragging from the DETAILS grid
						//Working string version
						BankAccountViewModel bvm = new BankAccountViewModel ( );
						if ( t1 . Contains ( "ListView" ) )
							bvm = UCListbox . SelectedItem as BankAccountViewModel;
						else
							bvm = datagrid . SelectedItem as BankAccountViewModel;
						string str = GetExportRecords . CreateTextFromRecord ( bvm, null, null, true, false );
						string dataFormat = DataFormats . Text;
						DataObject dataObject = new DataObject ( dataFormat, str );
						System . Windows . DragDrop . DoDragDrop (
						datagrid,
						dataObject,
						DragDropEffects . Copy );
						IsLeftButtonDown = false;
					}
				}
			}
		}

		private void DbList_ShowDropGrid ( object sender, MouseButtonEventArgs e )
		{
			DropDataGridData ddg = new DropDataGridData ( );
			ddg . Show ( );
		}

		#endregion DRAG CODE
		//================================================================================================





	}
	public class FocusVisualTreeChanger
	{
		public static bool GetIsChanged ( DependencyObject obj )
		{
			return ( bool ) obj . GetValue ( IsChangedProperty );
		}

		public static void SetIsChanged ( DependencyObject obj, bool value )
		{
			obj . SetValue ( IsChangedProperty, value );
		}

		public static readonly DependencyProperty IsChangedProperty =
		    DependencyProperty . RegisterAttached ( "IsChanged", typeof ( bool ), typeof ( FocusVisualTreeChanger ), new FrameworkPropertyMetadata ( false, FrameworkPropertyMetadataOptions . Inherits, IsChangedCallback ) );

		private static void IsChangedCallback ( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			if ( true . Equals ( e . NewValue ) )
			{
				FrameworkContentElement contentElement = d as FrameworkContentElement;
				if ( contentElement != null )
				{
					contentElement . FocusVisualStyle = null;
					return;
				}

				FrameworkElement element = d as FrameworkElement;
				if ( element != null )
				{
					element . FocusVisualStyle = null;
				}
			}
		}
	}
	public class PremiumUserDataTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate ( object item, DependencyObject container )
		{
			FrameworkElement elemnt = container as FrameworkElement;
			int actype = ( int ) item;
			if ( actype == 1 )
				return elemnt . FindResource ( "Actype1DataTemplate" ) as DataTemplate;
			else if ( actype == 2 )
				return elemnt . FindResource ( "Actype2DataTemplate" ) as DataTemplate;
			else if ( actype == 3 )
				return elemnt . FindResource ( "Actype3DataTemplate" ) as DataTemplate;
			else if ( actype == 4 )
				return elemnt . FindResource ( "Actype4DataTemplate" ) as DataTemplate;
			return null;
		}
	}

}
