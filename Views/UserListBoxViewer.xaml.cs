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

namespace WPFPages . Views
{

	public class ExpandedState : Expander
	{
		public ExpandedState ( ) : base ( ) { }
		public bool IsItemExpanded
		{
			get
			{
				return ( bool ) this . GetValue ( IsItemExpandedProperty );
			}
			set
			{
				this . SetValue ( IsItemExpandedProperty, value );
			}
		}

		public static readonly DependencyProperty
			IsItemExpandedProperty = DependencyProperty .
			Register ( "IsItemExpanded", typeof ( bool ), typeof ( Expander ), new PropertyMetadata ( true ) );

	}

	/// <summary>
	/// Interaction logic for UserListBoxViewer.xaml
	/// </summary>
	public partial class UserListBoxViewer : Window
	{
		// Declare all 3 of the local Db pointers
		public BankCollection SqlBankcollection = new BankCollection ( );
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
			if ( e . CallerType == "SELECTEDDATA" )
			{
				//Pass the data  to the UserControl to load into the ListBox
				UCListbox . ItemsSource = e . DataSource as BankCollection;
				datagrid . ItemsSource = e . DataSource as BankCollection;
				UCListbox . SelectedIndex = 0;
				Mouse . OverrideCursor = System . Windows . Input . Cursors . Arrow;
			}
		}

		private void Window_Closed ( object sender, EventArgs e )
		{
			//DbListWindowControl . SelectedParams -= DbListbox_SelectedParams;
			//DbListWindowControl . ClearBtnPressed -= DbListWindowControl_ClearBtnPressed;
			//DbListWindowControl . LoadBtnPressed -= DbListWindowControl_LoadBtnPressed;

		}

		private async void DbList_LoadBtnPressed ( object sender, RoutedEventArgs e )
		{
			int min = 0, max = 0, tot = 0;
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
			//			UCListbox . ItemsSource = SqlBankcollection;
			//ActiveType = "BANKACCOUNT";
			//			UCListbox . SelectedIndex = 0;
			//BankRecord = DbListbox . UCListbox . SelectedItem as BankAccountViewModel;
			Mouse . OverrideCursor = System . Windows . Input . Cursors . Arrow;
		}

		#region DATA LOAD FUNCTIONS
		public static BankCollection LoadBankTest ( BankCollection temp, DataTable dtBank )
		{

			//try
			//{
			//	for ( int i = 0 ; i < dtBank . Rows . Count ; i++ )
			//	{
			//		temp . Add ( new BankAccountViewModel
			//		{
			//			Id = Convert . ToInt32 ( dtBank . Rows [ i ] [ 0 ] ), BankNo = dtBank . Rows [ i ] [ 1 ] . ToString ( ),
			//			CustNo = dtBank . Rows [ i ] [ 2 ] . ToString ( ), AcType = Convert . ToInt32 ( dtBank . Rows [ i ] [ 3 ] ),
			//			Balance = Convert . ToDecimal ( dtBank . Rows [ i ] [ 4 ] ), IntRate = Convert . ToDecimal ( dtBank . Rows [ i ] [ 5 ] ),
			//			ODate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 6 ] ), CDate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 7 ] ),
			//		} );
			//	}

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
				//lbi.
				ItemExpanded = true;
				lbi . Refresh ( );
				UCListbox . Refresh ( );
				//if ( expander . IsExpanded = true )
				//lbi.ItemExpanded = false;
				//				Expander
				//UCListbox.Items.
				//ListBoxItem lbi = new ListBoxItem();
				//lbi.lbiExpander
				AreItemsExpanded = true;
				//BankDataTemplate.lbiExpander . IsExpanded = true;
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
			if ( e . Key == Key . Enter || e . Key == Key . Tab )
			{
				BankAccountViewModel bvm = new BankAccountViewModel ( );
				bvm = UCListbox . SelectedItem as BankAccountViewModel;
				BankCollection . UpdateBankDb ( bvm, "BANKACCOUNT" );
				//e.Handled = true;
				//Focus = false;
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
				//				UCListbox . SelectedItem = ListSelection;
			}
		}

		private void DbListbox_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			ListBoxItem lbi = new ListBoxItem ( );
			ListBox lv = sender as ListBox;
			int sel = lv . SelectedIndex;
			lbi = ( ListBoxItem ) UCListbox . ItemContainerGenerator . ContainerFromIndex ( UCListbox . SelectedIndex );
			ListSelection = UCListbox . SelectedIndex;
			GridSelection = ListSelection;
			datagrid . SelectedIndex = GridSelection;
			datagrid . ScrollIntoView ( datagrid . SelectedItem );
			datagrid . Refresh ( );
			int count = 0;
			//foreach ( var item in lv.Items )
			//{
			//	lv . SelectedIndex = count;
			//	count++;
			//	lv . Refresh ( );
			//}

			//			ListSelection = UCListbox . SelectedIndex;
		}

		private void Datagrid_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			GridSelection = datagrid . SelectedIndex;
			ListSelection = GridSelection;
			UCListbox . SelectedIndex = ListSelection;

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
			ListBox lb = new ListBox ( );
			lb = sender as ListBox;
			ListSelection = lb . SelectedIndex;
			GridSelection = ListSelection;
			if ( lb . SelectedItem != null )
				datagrid . ScrollIntoView ( lb . SelectedItem );
		}

		private void datagrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			DataGrid dg = new DataGrid ( );
			dg = e . Source as DataGrid;
			var dgr = dg . Items . CurrentItem;
			//Brush br = dg . RowBackground;
			GridSelection = dg . SelectedIndex;
			ListSelection = GridSelection;
			var template = datagrid;
			TextBlock tb = ( TextBlock ) this . datagrid . FindName ( "custno2" );
			Brush newbrush = new SolidColorBrush ( Color . FromArgb ( 255, ( byte ) 255, ( byte ) 255, ( byte ) 255 ) );
			//			BalanceTextBlock . Foreground= newbrush;
			//var myControl = ( MyControl ) template . FindName ( "MyControlName", MyList );
			//custno . Foreground = newbrush;
			//Brush b = SolidColorBrush ( Colors.Chocolate); 
			//dg
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
			// Default the brush to Unselected (Alternate) colored background row
			//if ( CurrentCellName == "balancefield" )
			//{
			//	TextBlock tb = new TextBlock ( );
			//	tb = ( TextBlock ) sender as TextBlock;


			//	if ( tb != null )
			//	{
			//		Brush newbrush = new SolidColorBrush ( Color . FromArgb ( 0xFF, 0x11, 0x71, 0xe6 ) );
			//		//Save current background color
			//		// Set the Font Weight value
			//		tb . FontWeight = FontWeight . FromOpenTypeWeight ( 600 );

			//		// Set the ForeGround color
			//		newbrush = new SolidColorBrush ( Color . FromArgb ( 0xFF, 0xFF, 0x00, 0x00 ) );
			//		tb . Foreground = newbrush;
			//		// Set the BackGround color
			//		newbrush = new SolidColorBrush ( Color . FromArgb ( 0xFF, 0x70, 0x9e, 0xFe ) );
			//		tb . Background = newbrush;
			//	}
			//}
		}

		private void Balancefield_PreviewMouseLeave ( object sender, MouseEventArgs e )
		{
			//Brush tempbrush;
			//TextBlock tb = new TextBlock ( );
			//tb = sender as TextBlock;
			//if ( CurrentCellName == "balancefield" )
			//{
			//	tb . Background = CurrentBackColor;
			//	tb . Foreground = CurrentForeColor;
			//	tb . FontWeight = FontWeight . FromOpenTypeWeight ( 200 );
			//}
		}

		private void Balancefield_MouseMove ( object sender, MouseEventArgs e )
		{
			//TextBlock tb = new TextBlock ( );
			//tb = sender as TextBlock;
			//if ( TbBalance == "balancefield" )
			//{
			//	TbBalance = tb . Name;
			//	// set Foreground to Red
			//	Brush newbrush = new SolidColorBrush ( Color . FromArgb ( 0xFF, 0xFF, 0x00, 0x00 ) );
			//	tb . Foreground = newbrush;
			//	//Set Background to Highlight blue
			//	newbrush = new SolidColorBrush ( Color . FromArgb ( 0xFF, 0xDF, 0xE9, 0xAD ) );
			//	tb . Background = newbrush;
			//}
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
				Debug . WriteLine ( $"Name={CurrentCellName }, B = {CurrentBackColor}, F = {CurrentForeColor}" );
			}
		}
	}
}
