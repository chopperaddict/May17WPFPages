using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . Data;
using System . Data . Linq;
using System . Data . SqlClient;
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
using System . Windows . Navigation;
using System . Windows . Shapes;
using System . Windows . Threading;

using Newtonsoft . Json . Linq;

using WPFPages . ViewModels;

namespace WPFPages . Views
{
	//public class LoadArgs : EventArgs
	//{
	//	public int LowValue
	//	{
	//		get; set;
	//	}
	//	public int HighValue
	//	{
	//		get; set;
	//	}
	//	public int TotalRecords
	//	{
	//		get; set;
	//	}
	//}       /// <summary>
		/// Interaction logic for UserDbListWindow.xaml
		/// </summary>
	public partial class listboxes : Window
	{
		//		ObservableCollection<BankAccountViewModel> Bankdata = new ObservableCollection<BankAccountViewModel> ( );
		public static event EventHandler<List<int>> SelectedParams;
		public static event EventHandler<bool> ClearBtnPressed;
		public static event EventHandler<LoadArgs> LoadBtnPressed;
		private bool isMouseOver;
		public bool IsMouseOver
		{
			get
			{
				return isMouseOver;
			}
			set
			{
				isMouseOver = value;
			}
		}
		private int min = 0;
		private int max = 0;
		private int tot = 0;
		public listboxes( )
		{
			InitializeComponent ( );
		}


		//public void LoadBank ( BankCollection bankcollection )
		//{

		//	if ( bankcollection . Count > 0 )
		//	{
		//		foreach ( var item in bankcollection )
		//		{
		//			Bankdata . Add ( item );
		//		}
		//		SelectedListbox . ItemsSource = Bankdata;
		//	}
		//}


		//PRIVATE DEPENDENCIES
		/*
		 * int		ListCount
		 * int		Selected
		 * string	SelectionColor
		 * string	MsgTextOne
		 * string	MsgTextTwo
		 * string	ActiveType
		 * bool	IsUptoDate
		 * bool	IsSelected
		 * object	BankRecord
		 * object	CustRecord
		 * object	DetRecord
		 * DataContext CurrentDataContext
		 * */
		#region Dependencies

		//ListBox load parameters required
		public int lowValue
		{
			get
			{
				return ( int ) GetValue ( lowValueProperty );
			}
			set
			{
				SetValue ( lowValueProperty, value );
			}
		}
		public static readonly DependencyProperty lowValueProperty =
		    DependencyProperty . Register ( "lowValue", typeof ( int ), typeof ( DbListWindowControl ), new PropertyMetadata ( 0 ) );

		public int highValue
		{
			get
			{
				return ( int ) GetValue ( highValueProperty );
			}
			set
			{
				SetValue ( highValueProperty, value );
			}
		}
		public static readonly DependencyProperty highValueProperty =
		    DependencyProperty . Register ( "highValue", typeof ( int ), typeof ( DbListWindowControl ), new PropertyMetadata ( 0 ) );

		public new int Height
		{
			get
			{
				return ( int ) GetValue ( HeightProperty );
			}
			set
			{
				SetValue ( HeightProperty, value );
			}
		}
		public static readonly DependencyProperty HeightProperty =
		    DependencyProperty . Register ( "Height", typeof ( int ), typeof ( DbListWindowControl ), new PropertyMetadata ( 150 ) );

		public new int Width
		{
			get
			{
				return ( int ) GetValue ( WidthProperty );
			}
			set
			{
				SetValue ( WidthProperty, value );
			}
		}
		public static readonly DependencyProperty WidthProperty =
		    DependencyProperty . Register ( "Width", typeof ( int ), typeof ( DbListWindowControl ), new PropertyMetadata ( 250 ) );

		//END - ListBox load parameters required


		public double ItemOpacity
		{
			get
			{
				return ( double ) GetValue ( ItemOpacityProperty );
			}
			set
			{
				SetValue ( ItemOpacityProperty, value );
			}
		}
		public static readonly DependencyProperty ItemOpacityProperty =
		    DependencyProperty . Register ( "SelectedItem", typeof ( double ), typeof ( ListBoxItem ), new PropertyMetadata ( ) );

		public int Selected
		{
			get
			{
				return ( int ) GetValue ( SelectedProperty );
			}
			set
			{
				SetValue ( SelectedProperty, value );
			}
		}
		public static readonly DependencyProperty SelectedProperty =
		    DependencyProperty . Register ( "Selected", typeof ( int ), typeof ( DbListWindowControl ), new PropertyMetadata ( ) );
		public int ListCount
		{
			get
			{
				return ( int ) GetValue ( ListCountProperty );
			}
			set
			{
				SetValue ( ListCountProperty, value );
			}
		}
		public static readonly DependencyProperty ListCountProperty =
		    DependencyProperty . Register ( "ListCount", typeof ( int ), typeof ( DbListWindowControl ), new PropertyMetadata ( ) );
		public string SelectionColor
		{
			get
			{
				return ( string ) GetValue ( SelectionColorProperty );
			}
			set
			{
				SetValue ( SelectionColorProperty, value );
			}
		}
		public static readonly DependencyProperty SelectionColorProperty =
		    DependencyProperty . Register ( "SelectionColor", typeof ( string ), typeof ( DbListWindowControl ), new PropertyMetadata ( ) );
		public bool IsUptoDate
		{
			get
			{
				return ( bool ) GetValue ( IsUptoDateProperty );
			}
			set
			{
				SetValue ( IsUptoDateProperty, value );
			}
		}
		public static readonly DependencyProperty IsUptoDateProperty =
		    DependencyProperty . Register ( "IsIsUptoDate", typeof ( bool ), typeof ( DbListWindowControl ), new PropertyMetadata ( ) );
		public bool IsSelected
		{
			get
			{
				return ( bool ) GetValue ( IsSelectedProperty );
			}
			set
			{
				SetValue ( IsSelectedProperty, value );
			}
		}
		public static readonly DependencyProperty IsSelectedProperty =
		    DependencyProperty . Register ( "IsSelectedProperty", typeof ( bool ), typeof ( DbListWindowControl ), new PropertyMetadata ( ) );
		public string ActiveType
		{
			get
			{
				return ( string ) GetValue ( ActiveTypeProperty );
			}
			set
			{
				SetValue ( ActiveTypeProperty, value );
			}
		}
		public static readonly DependencyProperty ActiveTypeProperty =
		    DependencyProperty . Register ( "ActiveType", typeof ( string ), typeof ( DbListWindowControl ), new PropertyMetadata ( ) );
		public string MsgTextTwo
		{
			get
			{
				return ( string ) GetValue ( MsgTextTwoProperty );
			}
			set
			{
				SetValue ( MsgTextTwoProperty, value );
			}
		}
		public static readonly DependencyProperty MsgTextTwoProperty =
		    DependencyProperty . Register ( "MsgTextTwo", typeof ( string ), typeof ( DbListWindowControl ), new PropertyMetadata ( ) );
		public BankAccountViewModel BankRecord
		{
			get
			{
				return ( BankAccountViewModel ) GetValue ( BankRecordProperty );
			}
			set
			{
				SetValue ( BankRecordProperty, value );
			}
		}
		public static readonly DependencyProperty BankRecordProperty =
		    DependencyProperty . Register ( "BankRecord", typeof ( BankAccountViewModel ), typeof ( DbListWindowControl ), new PropertyMetadata ( ) );
		public CustomerViewModel CustRecord
		{
			get
			{
				return ( CustomerViewModel ) GetValue ( CustRecordProperty );
			}
			set
			{
				SetValue ( CustRecordProperty, value );
			}
		}
		public static readonly DependencyProperty CustRecordProperty =
		    DependencyProperty . Register ( "CustRecord", typeof ( CustomerViewModel ), typeof ( DbListWindowControl ), new PropertyMetadata ( ) );
		public DetailsViewModel DetRecord
		{
			get
			{
				return ( DetailsViewModel ) GetValue ( DetRecordProperty );
			}
			set
			{
				SetValue ( DetRecordProperty, value );
			}
		}
		public static readonly DependencyProperty DetRecordProperty =
			    DependencyProperty . Register ( "DetRecord", typeof ( DetailsViewModel ), typeof ( DbListWindowControl ), new PropertyMetadata ( ) );
		//public DataContext CurrentDataContext
		//{
		//	get { return ( DataContext ) GetValue ( CurrentDataContextProperty ); }
		//	set { SetValue ( CurrentDataContextProperty, value ); }
		//}
		//public static readonly DependencyProperty CurrentDataContextProperty =
		//    DependencyProperty . Register ( "CurrentDataContext", typeof ( object ), typeof ( DataContext), new PropertyMetadata ( ) );

		// END - Private Dependencies
		#endregion Dependencies

		private void ListBox_UpdateCount ( int count )
		{
			if ( ListCount == -1 )
				ListCount = 0;
			else
				ListCount = count;
		}

		private void ListBox_PreviewMouseEnter ( object sender, MouseEventArgs e )
		{
			IsMouseOver = true;
			var brush = ( Brush ) new BrushConverter ( ) . ConvertFromString ( SelectionColor );
			this . UCListbox . Foreground = brush;
		}

		private void ListBox_MouseLeave ( object sender, MouseEventArgs e )
		{
			IsMouseOver = false;
			//			var brush = ( Brush ) new BrushConverter ( ) . ConvertFromString ( CtrlBackGround . ToString ( ) );

			//			this . RefreshBorder . Background = brush;

		}
		private void SelectedListbox_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			int x = 0;
		}

		//public void DoSelect ( int low, int high, int total )
		//{
		//	List<int> list = new List<int> ( );
		//	list . Add ( low );
		//	list . Add ( high );
		//	list . Add ( total );
		//	// reload listbox
		//	if ( SelectedParams != null )
		//	{
		//		SelectedParams?.Invoke ( null, list );
		//	}
		//}

		private void DoSelect_Click ( object sender, RoutedEventArgs e )
		{
			// Trigger the event to notify caller of the parameters for the Data loading
			min = Convert . ToInt32 ( MinValue . Text );
			max = Convert . ToInt32 ( MaxValue . Text );
			tot = Convert . ToInt32 ( MaxRecords . Text );
			if ( LoadBtnPressed != null )
				LoadBtnPressed . Invoke ( sender, new LoadArgs
				{
					LowValue = Convert . ToInt32 ( min ),
					HighValue = Convert . ToInt32 ( max ),
					TotalRecords = Convert . ToInt32 ( tot )
				} );
		}
		private void ListBox_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			ListBox l = new ListBox ( );
			l = sender as ListBox;
			if ( l . Items . Count > 0 )
				( ( ListBox ) sender ) . ScrollIntoView ( e . AddedItems [ 0 ] );

		}

		private void Border_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			//Border of the button controlhas been clicked on  : Just Sends True  to clients application
			if ( ClearBtnPressed != null )
				ClearBtnPressed . Invoke ( sender, true );
		}

		#region DATA LOAD FUNCTIONS
		public static DataTable LoadBankData ( int Min, int Max, int Tot )
		{
			DataTable dt = new DataTable ( );
			SqlConnection con;
			string ConString = "";
			string commandline = "";
			ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
			Debug . WriteLine ( $"Making new SQL connection in BANKCOLLECTION" );
			con = new SqlConnection ( ConString );
			try
			{
				Debug . WriteLine ( $"Using new SQL connection in BANKCOLLECTION" );
				using ( con )
				{
					// Create a valid Query Command string including any active sort ordering
					commandline = $"Select Top {Tot} Id, BankNo, CustNo, AcType, Balance, IntRate, ODate, CDate  from BankAccount  " +
						$" where CustNo > {Min} AND CustNo < {Max} order by CustNo + BankNo ";
					SqlCommand cmd = new SqlCommand ( commandline, con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dt );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"BANKACCOUNT : ERROR in LoadBankDataSql(): Failed to load Bank Details - {ex . Message}, {ex . Data}" );
				MessageBox . Show ( $"BANKACCOUNT: ERROR in LoadBankDataSql(): Failed to load Bank Details - {ex . Message}, {ex . Data}" );
			}
			finally
			{
				con . Close ( );
			}
			return dt;
		}
		public static BankCollection LoadBankTest ( BankCollection temp, DataTable dtBank )
		{

			try
			{
				for ( int i = 0 ; i < dtBank . Rows . Count ; i++ )
				{
					temp . Add ( new BankAccountViewModel
					{
						Id = Convert . ToInt32 ( dtBank . Rows [ i ] [ 0 ] ), BankNo = dtBank . Rows [ i ] [ 1 ] . ToString ( ),
						CustNo = dtBank . Rows [ i ] [ 2 ] . ToString ( ), AcType = Convert . ToInt32 ( dtBank . Rows [ i ] [ 3 ] ),
						Balance = Convert . ToDecimal ( dtBank . Rows [ i ] [ 4 ] ), IntRate = Convert . ToDecimal ( dtBank . Rows [ i ] [ 5 ] ),
						ODate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 6 ] ), CDate = Convert . ToDateTime ( dtBank . Rows [ i ] [ 7 ] ),
					} );
				}

			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"BANK : SQL Error in BankCollection load function : {ex . Message}, {ex . Data}" );
				MessageBox . Show ( $"BANK : SQL Error in BankCollection load function : {ex . Message}, {ex . Data}" );
			}
			finally
			{
				Debug . WriteLine ( $"BANK : Completed load into Bankcollection :  {temp . Count} records loaded successfully ...." );
			}
			return temp;
		}
		#endregion DATA LOAD FUNCTIONS

	}
}


