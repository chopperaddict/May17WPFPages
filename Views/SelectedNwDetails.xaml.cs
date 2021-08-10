using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . ComponentModel;
using System . Data . SqlClient;
using System . Data;
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
using System . IO;
using static WPFPages . NorthWind;
using WPFPages . UserControls;
using System . Net;
using System . Threading;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for SelectedNwDetails.xaml
	/// </summary>
	public partial class SelectedNwDetails : Window
	{

		// Declare public pointers to our data classes
		public nwcustomer NwCustomer = new nwcustomer ( );
		public ObservableCollection<nwcustomer> nwc2;
		private string argument = "";

		public  SelectedNwDetails ( string arg = "" )
		{
			Thread t1;
			InitializeComponent ( );
			argument = arg;
			// start our linkage monitor
			//t1 = new Thread ( LoadNorthWindData );
			//t1 . IsBackground = true;
			//t1 . Priority = ThreadPriority . Lowest;
			//t1 . Start ( );
			//Debug . WriteLine ( t1 . ThreadState . ToString ( ) );
			LoadNorthWindData ( );
		}
		private  void  LoadNorthWindData ( )
		{
			nwc2 = new ObservableCollection<nwcustomer> ( );
			nwc2 =  NwCustomer . LoadSpecificCustomers ( argument );
			CustomersGrid . ItemsSource = nwc2;
			CustomersGrid . DataContext = NwCustomer;
			EventControl . NwCustomerSelected += EventControl_NwCustomerSelected;
			Flags . NwSelectionWindow = this;
//			return true;
		}
		public void SwitchCustomer (string arg )
		{
			argument = arg;
			nwc2 = new ObservableCollection<nwcustomer> ( );
			nwc2 = NwCustomer . LoadSpecificCustomers ( arg );
			CustomersGrid . ItemsSource = null;
			CustomersGrid . Items . Clear ( );
			CustomersGrid . ItemsSource = nwc2;
			CustomersGrid . SelectedIndex= 0;

		}

		#region PropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged ( string PropertyName )
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if ( handler != null )
			{
				handler ( this, new PropertyChangedEventArgs ( PropertyName ) );
			}
		}

		#endregion PropertyChanged	

		#region Full Properties

		private int customersTotal;
		private int productsTotal;
		private int ordersTotal;
		private int orderdetailsTotal;
		private int categoriesTotal;
		private int customerCurrent;
		private int orderCurrent;
		public int CustomersTotal
		{
			get
			{
				return customersTotal;
			}
			set
			{
				customersTotal = value;
				OnPropertyChanged ( nameof ( CustomersTotal ) );
			}
		}
		public int ProductsTotal
		{
			get
			{
				return productsTotal;
			}
			set
			{
				productsTotal = value;
				OnPropertyChanged ( nameof ( ProductsTotal ) );
			}
		}
		public int OrdersTotal
		{
			get
			{
				return ordersTotal;
			}
			set
			{
				ordersTotal = value;
				OnPropertyChanged ( nameof ( OrdersTotal ) );
			}
		}
		public int OrderDetailsTotal
		{
			get
			{
				return orderdetailsTotal;
			}
			set
			{
				orderdetailsTotal = value;
				OnPropertyChanged ( nameof ( OrderDetailsTotal ) );
			}
		}
		public int CategoriesTotal
		{
			get
			{
				return categoriesTotal;
			}
			set
			{
				categoriesTotal = value;
				OnPropertyChanged ( nameof ( CategoriesTotal ) );
			}
		}
		public int CustomerCurrent
		{
			get
			{
				return customerCurrent;
			}
			set
			{
				customerCurrent = value;
				OnPropertyChanged ( nameof ( CustomerCurrent ) );
			}
		}
		public int OrderCurrent
		{
			get
			{
				return this . orderCurrent;
			}
			set
			{
				this . orderCurrent = value;
				OnPropertyChanged ( nameof ( OrderCurrent ) );
			}
		}
		#endregion Full Properties


		public nwcustomer NwCust = new nwcustomer ( );
		public NwOrderCollection NwOrdCollection = new NwOrderCollection ( "" );
		public NwOrderDetails NwOdCollection = new NwOrderDetails ( -1 );
		public NwProductCollection NwProdCollection = new NwProductCollection ( -1 );
		public NwCatCollection NwcatCollection = new NwCatCollection ( -1 );

		public ObservableCollection<nwcustomer> nwc;


		private void EventControl_NwCustomerSelected ( object sender, NwGridArgs e )
		{
			//handle 

			//CustomersGrid . Items . Clear ( );
			IsLoading = true;
			CustomersGrid . ItemsSource = null;
			CustomersGrid . Items.Clear( );			
			string srchterm = e . ArgumentParameter;
			SwitchCustomer ( srchterm );
			IsLoading = false;
			LoadFullDataGrids ( );
		}

		// Declare public pointers to our data classes
		public bool IsLoading
		{
			get; set;
		}
		private void Window_Loaded ( object sender, RoutedEventArgs e )
		{
			IsLoading = true;
			//Load the Customers MAIN database and all others as required
			//			FillAllGrids ( "Main", 0 );
			IsLoading = false;
			// This creates a DataSet containing ALL data
			FillMultiTables ( );
			Utils . SetupWindowDrag ( this );
			CustomersGrid . Focus ( );
			IsLoading = false;
			//CustomersGrid . DataContext = nwc;
			this . Refresh ( );
		}
		/// <summary>
		/// This method cascades down from whatever starting level is requesting by the 1st paramter
		/// The 2nd parameter is the spource record index for it to trigger the cascade from
		/// The Cascade sequence is
		///  CUSTOMERS -> ORDERS
		/// ORDERS -> ORDER DETAILS
		/// ORDER DETAILS -> PRODUCTS
		/// PRODUCTS -> CATEGORIES
		/// 
		// UNUSED
		public void FillAllGrids ( string caller, int index )
		{
			//			bool isworking = false;
			//			if ( caller == "Main" )
			//			{
			//				isworking = true;
			//				IsLoading = true;
			//				this . CustomersGrid . SelectedIndex = index;
			////				nwc = new nw
			//				this . CustomersGrid . ItemsSource = nwc;
			//				this . CustomersGrid . DataContext = nwc;
			//				this . CustomersGrid . SelectedIndex = index;
			//				this . CustomersGrid . SelectedItem = index;
			//				Utils . SetGridRowSelectionOn ( CustomersGrid, index );
			//			}
			//			//Now get The selected Customer item and use it to load the OrdersGrid with correct set of matching records
			//			if ( caller == "Customer" || isworking )
			//			{
			//				isworking = true;
			//				IsLoading = true;
			//				var srchitem1 = CustomersGrid . SelectedItem as nwcustomer;
			//				if ( srchitem1 != null )
			//				{
			//					int rec = FindMatchingRecord ( srchitem1 as object, OrdersGrid );
			//					OrdersGrid . SelectedIndex = rec;
			//					OrdersGrid . SelectedItem = rec;
			//					Utils . SetGridRowSelectionOn ( OrdersGrid, rec );
			//				}
			//			}
			//			//Now get The selected OrderId item and use it to load the OrderDetailsGrid with correct set of matching records
			//			// This is the master grid to allow access to 2 way access to Product and Order tables !!
			//			if ( caller == "Order" || isworking )
			//			{
			//				isworking = true;
			//				IsLoading = true;
			//				var v2 = OrdersGrid . SelectedItem as nworder;
			//				if ( v2 != null )
			//				{
			//					NwOrderDetails NwOdCollection = new NwOrderDetails ( v2 . OrderId );
			//					OrderDetailsGrid . DataContext = NwOdCollection;
			//					OrderDetailsGrid . ItemsSource = NwOdCollection;
			//					OrderDetailsGrid . SelectedIndex = 0;
			//					OrderDetailsGrid . SelectedItem = 0;
			//					Utils . SetGridRowSelectionOn ( OrderDetailsGrid, 0 );
			//				}
			//			}

			//			//Now get The selected ProductId item and use it to load the ProductsGrid with correct set of matching records
			//			if ( caller == "OrderDetail" || isworking )
			//			{
			//				isworking = true;
			//				IsLoading = true;
			//				var v4 = OrderDetailsGrid . SelectedItem as nworderdetail;
			//				if ( v4 != null )
			//				{
			//					NwProductCollection NwProdCollection = new NwProductCollection ( v4 . ProductId );
			//					ProductsGrid . DataContext = NwProdCollection;
			//					ProductsGrid . ItemsSource = NwProdCollection;
			//					ProductsGrid . SelectedIndex = 0;
			//					ProductsGrid . SelectedItem = 0;
			//					Utils . SetGridRowSelectionOn ( ProductsGrid, 0 );
			//				}
			//			}

			//			if ( caller == "Product" || isworking )
			//			{
			//				isworking = true;
			//				IsLoading = true;
			//				var srchitem4 = ProductsGrid . SelectedItem as nwproduct;
			//				if ( srchitem4 != null )
			//				{
			//					NwCatCollection NwcatCollection = new NwCatCollection ( srchitem4 . CategoryId );
			//					CategoriesGrid . DataContext = NwcatCollection;
			//					CategoriesGrid . ItemsSource = NwcatCollection;
			//					CategoriesGrid . SelectedIndex = 0;
			//					CategoriesGrid . SelectedItem = 0;
			//					Utils . SetGridRowSelectionOn ( CategoriesGrid, 0 );
			//				}
			//			}
			//			isworking = false;
			//			IsLoading = false;
		}

		private int FindMatchingRecord ( object srchitem, DataGrid dGrid, string SearchType = "" )
		{
			int index = 0;
			if ( dGrid == OrdersGrid )
			{
				string search = srchitem as string;
				foreach ( var item in dGrid . Items )
				{
					nworder cvm = item as nworder;
					if ( cvm == null )
						break;
					if ( cvm . CustomerId == search )
					{
						break;
					}
					index++;
				}
				return index;
			}
			else if ( dGrid == OrderDetailsGrid )
			{
				int search = Convert . ToInt32 ( srchitem );
				foreach ( var item in dGrid . Items )
				{
					nworderdetail cvm = item as nworderdetail;
					if ( cvm == null )
						break;
					if ( SearchType == "PRODUCTID" )
					{
						if ( cvm . ProductId == search )
						{
							break;
						}
					}
					else
					{
						if ( cvm . OrderId == search )
						{
							break;
						}
					}
					//if ( cvm . OrderId == search )
					//{
					//	break;
					//}
					index++;
				}
				return index;
			}
			else if ( dGrid == ProductsGrid )
			{
				int search = Convert . ToInt32 ( srchitem );
				foreach ( var item in dGrid . Items )
				{
					nwproduct cvm = item as nwproduct;
					if ( cvm == null )
						break;
					if ( SearchType == "PRODUCTID" )
					{
						if ( cvm . CategoryId == search )
						{
							break;
						}
					}
					else
					{
						if ( cvm . ProductId == search )
						{
							break;
						}
					}
					index++;
				}
				return index;
			}
			else if ( dGrid == OrdersGrid )
			{
				string search = srchitem as string;
				foreach ( var item in dGrid . Items )
				{
					nworder cvm = item as nworder;
					if ( cvm == null )
						break;
					if ( cvm . CustomerId == search )
					{
						break;
					}
					index++;
				}
				return index;
			}
			else if ( dGrid == CategoriesGrid )
			{
				int search = Convert . ToInt32 ( srchitem );
				foreach ( var item in dGrid . Items )
				{
					nwcategory cvm = item as nwcategory;
					if ( cvm == null )
						break;
					if ( cvm . CategoryId == search )
					{
						break;
					}
					index++;
				}
				return index;
			}
			return index;
		}
		public DataSet FillMultiTables ( )
		{
			DataSet ds = new DataSet ( );
			DataTable dt = new DataTable ( "Products" );
			string ConString = ( string ) Properties . Settings . Default [ "NorthwindConnectionString" ];
			//int Ordervalue = Convert . ToInt32 ( orderId );
			string CmdString = string . Empty;
			using ( SqlConnection con = new SqlConnection ( ConString ) )
			{
				if ( argument != "" )
					CmdString = $"SELECT *  FROM [Customers] where CustomerId = '{argument}';" +
							" Select * from [Orders]; " +
							" select * from [Order Details]; " +
							" Select * from [Products]; " +
							" Select * from [Categories]";
				else
					CmdString = $"SELECT *  FROM [Customers]; " +
							" Select * from [Orders]; " +
							" select * from [Order Details]; " +
							" Select * from [Products]; " +
							" Select * from [Categories]";
				SqlCommand cmd = new SqlCommand ( CmdString, con );

				SqlDataAdapter sda = new SqlDataAdapter ( cmd );
				sda . TableMappings . Add ( "Table", "Customers" );
				sda . TableMappings . Add ( "Table1", "Orders" );
				sda . TableMappings . Add ( "Table2", "OrderDetails" );
				sda . TableMappings . Add ( "Table3", "Products" );
				sda . TableMappings . Add ( "Table4", "Categories" );
				try
				{
					sda . Fill ( ds, "AllData" );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"Data={ex . Data}, {ex . Message}\n[{CmdString}]" );
				}
			}
			// This is how to extract a DataTable from a DataSet
			DataTable dt1 = ds . Tables [ 0 ] . DataSet . Tables [ 0 ];
			DataTable dt2 = ds . Tables [ 0 ] . DataSet . Tables [ 1 ];
			DataTable dt3 = ds . Tables [ 0 ] . DataSet . Tables [ 2 ];
			DataTable dt4 = ds . Tables [ 0 ] . DataSet . Tables [ 3 ];
			DataTable dt5 = ds . Tables [ 0 ] . DataSet . Tables [ 4 ];
			LoadFullDataGrids ( );
			this . Refresh ( );
			return ds;
		}

		private void LoadFullDataGrids ( )
		{
			IsLoading = true;

			CustomersGrid . SelectedIndex = 0;
			CustomersGrid . SelectedItem = 0;
			Utils . SetGridRowSelectionOn ( CustomersGrid, 0 );
			CustomersTotal = CustomersGrid . Items . Count;

			var v = CustomersGrid . SelectedItem as nwcustomer;
			if ( v == null )
				return;
			//Find 1st match in Orders Db Grid and select it
			IsLoading = true;
			NwOrderCollection nwc = new NwOrderCollection ( v . CustomerId );
			OrdersGrid . ItemsSource = nwc;
			int rec = FindMatchingRecord ( v . CustomerId, OrdersGrid );
			Utils . SetGridRowSelectionOn ( OrdersGrid, rec );
			var v2 = OrdersGrid . SelectedItem as nworder;
			if ( v2 == null )
			{
				IsLoading = false;
				return;
			}

			//Find 1st match in Orders Db Grid and select it
			IsLoading = true;
			NwOrderDetails nwd = new NwOrderDetails ( v2 . OrderId );
			OrderDetailsGrid . ItemsSource = nwd;
			rec = FindMatchingRecord ( v2 . OrderId, OrderDetailsGrid );
			Utils . SetGridRowSelectionOn ( OrderDetailsGrid, rec );
			OrderCurrent = rec;
			var v3 = OrderDetailsGrid . SelectedItem as nworderdetail;
			if ( v3 == null )
			{
				IsLoading = false;
				return;
			}
			//Find 1st match in Products Db Grid and select it
			IsLoading = true;
			NwProductCollection nwp = new NwProductCollection ( v3 . ProductId );
			ProductsGrid . ItemsSource = nwp;
			rec = FindMatchingRecord ( v3 . ProductId, ProductsGrid );
			Utils . SetGridRowSelectionOn ( ProductsGrid, rec );
			ProductsGrid . Refresh ( );

			var v4 = ProductsGrid . SelectedItem as nwproduct;
			if ( v4 == null )
			{
				IsLoading = false;
				return;
			}
			//Find 1st match in Products Db Grid and select it
			IsLoading = true;
			NwCatCollection ncc = new NwCatCollection ( v4 . CategoryId );
			CategoriesGrid . ItemsSource = ncc;
			rec = FindMatchingRecord ( v4 . CategoryId, CategoriesGrid );
			Utils . SetGridRowSelectionOn ( CategoriesGrid, rec );
			CustomerCurrent = CustomersGrid . SelectedIndex;
			IsLoading = false;

		}
		private void CustomerGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			int rec = 0;
			if ( IsLoading )
				return;
			var v = CustomersGrid . SelectedItem as nwcustomer;
			if ( v == null )
				return;
			//Find 1st match in Orders Db Grid and select it
			IsLoading = true;
			rec = FindMatchingRecord ( v . CustomerId, OrdersGrid );
			Utils . SetGridRowSelectionOn ( OrdersGrid, rec );

			var v2 = OrdersGrid . SelectedItem as nworder;
			if ( v2 == null )
			{
				IsLoading = false;
				return;
			}
			//Find 1st match in Orders Db Grid and select it
			rec = FindMatchingRecord ( v2 . OrderId, OrderDetailsGrid );
			Utils . SetGridRowSelectionOn ( OrderDetailsGrid, rec );
			OrderCurrent = rec;
			var v3 = OrderDetailsGrid . SelectedItem as nworderdetail;
			if ( v3 == null )
			{
				IsLoading = false;
				return;
			}
			//Find 1st match in Products Db Grid and select it
			rec = FindMatchingRecord ( v3 . ProductId, ProductsGrid );
			Utils . SetGridRowSelectionOn ( ProductsGrid, rec );
			ProductsGrid . Refresh ( );

			var v4 = ProductsGrid . SelectedItem as nwproduct;
			if ( v4 == null )
			{
				IsLoading = false;
				return;
			}
			//Find 1st match in Products Db Grid and select it
			rec = FindMatchingRecord ( v4 . CategoryId, CategoriesGrid );
			Utils . SetGridRowSelectionOn ( CategoriesGrid, rec );
			CustomerCurrent = CustomersGrid . SelectedIndex;
			IsLoading = false;
		}

		private void OrdersGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( IsLoading )
				return;
			var v = OrdersGrid . SelectedItem as nworder;
			if ( v == null )
				return;
			//Find 1st match in Order Details Db Grid and select it
			IsLoading = true;
			NwOrderDetails nwd = new NwOrderDetails ( v . OrderId );
			OrderDetailsGrid . ItemsSource = nwd;
			int rec = FindMatchingRecord ( v . OrderId, OrderDetailsGrid );
			Utils . SetGridRowSelectionOn ( OrderDetailsGrid, rec );

			var v3 = OrderDetailsGrid . SelectedItem as nworderdetail;
			if ( v3 == null )
			{
				IsLoading = false;
				return;
			}
			IsLoading = true;
			NwProductCollection npc = new NwProductCollection ( v3 . ProductId );
			ProductsGrid . ItemsSource = npc;
			//Find 1st match in Products Db Grid and select it
			rec = FindMatchingRecord ( v3 . ProductId, ProductsGrid );
			Utils . SetGridRowSelectionOn ( ProductsGrid, rec );
			ProductsGrid . Refresh ( );

			var v4 = ProductsGrid . SelectedItem as nwproduct;
			if ( v4 == null )
			{
				IsLoading = false;
				return;
			}
			IsLoading = true;
			//Find 1st match in Products Db Grid and select it
			NwCatCollection ncc = new NwCatCollection ( v4 . CategoryId);
			CategoriesGrid . ItemsSource = ncc;
			rec = FindMatchingRecord ( v4 . CategoryId, CategoriesGrid );
			Utils . SetGridRowSelectionOn ( CategoriesGrid, rec );
			this . OrderCurrent = this . OrdersGrid . SelectedIndex;

			IsLoading = false;
		}
		private void ProductsGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( IsLoading )
				return;
			var v = ProductsGrid . SelectedItem as nwproduct;
			if ( v == null )
				return;
			IsLoading = true;

			//Find 1st match in Orders Db Grid and select it
			int rec = FindMatchingRecord ( v . CategoryId, CategoriesGrid );
			Utils . SetGridRowSelectionOn ( CategoriesGrid, rec );
			//Find ProductId in Product Db Grid use it to find OrderId in OrderDetails grid
			IsLoading = true;
			rec = FindMatchingRecord ( v . ProductId, OrderDetailsGrid, "PRODUCTID" );
			Utils . SetGridRowSelectionOn ( OrderDetailsGrid, rec );
			IsLoading = false;

		}
		private void CategoriesGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( IsLoading )
				return;
			var v = CategoriesGrid . SelectedItem as nwcategory;
			if ( v == null )
				return;
			IsLoading = true;
			//Find 1st match in Products Db Grid and select it
			int rec = FindMatchingRecord ( v . CategoryId, ProductsGrid, "PRODUCTID" );
			Utils . SetGridRowSelectionOn ( ProductsGrid, rec );
			IsLoading = false;
		}
		private void OrderDetailsGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( IsLoading )
				return;
			var v = OrderDetailsGrid . SelectedItem as nworderdetail;
			if ( v == null )
				return;
			IsLoading = true;
			//Find 1st match in Products Db Grid and select it
			NwProductCollection nwp = new NwProductCollection ( v . ProductId );
			ProductsGrid . ItemsSource = nwp;
			int rec = FindMatchingRecord ( v . ProductId, ProductsGrid );
			Utils . SetGridRowSelectionOn ( ProductsGrid, rec );
			ProductsGrid . Refresh ( );

			var v2 = ProductsGrid . SelectedItem as nwproduct;
			if ( v2 == null )
			{
				IsLoading = false;
				return;
			}
			IsLoading = true;
			//Find 1st match in Products Db Grid and select it
			rec = FindMatchingRecord ( v2 . CategoryId, CategoriesGrid );
			Utils . SetGridRowSelectionOn ( CategoriesGrid, rec );

			var v4 = ProductsGrid . SelectedItem as nwproduct;
			if ( v4 == null )
			{
				IsLoading = false;
				return;
			}
			//Find 1st match in Products Db Grid and select it
			IsLoading = true;
			NwCatCollection ncc = new NwCatCollection ( v4 . CategoryId );
			CategoriesGrid . ItemsSource = ncc;
			rec = FindMatchingRecord ( v4 . CategoryId, CategoriesGrid );
			Utils . SetGridRowSelectionOn ( CategoriesGrid, rec );

			IsLoading = false;

		}
		public static void SelectRowByIndex ( DataGrid dataGrid, int rowIndex )
		{
			if ( !dataGrid . SelectionUnit . Equals ( DataGridSelectionUnit . FullRow ) )
				throw new ArgumentException ( "The SelectionUnit of the DataGrid must be set to FullRow." );

			if ( rowIndex < 0 || rowIndex > ( dataGrid . Items . Count - 1 ) )
				throw new ArgumentException ( string . Format ( "{0} is an invalid row index.", rowIndex ) );

			if ( dataGrid . Items . Count == 0 )
				return;
			//			dataGrid . SelectedItem. Clear ( );
			/* set the SelectedItem property */
			dataGrid . SelectedIndex = rowIndex;

			if ( dataGrid . Name == "CustomersGrid" )
			{
				nwcustomer selitem = dataGrid . SelectedItem as nwcustomer;
				dataGrid . SelectedItem = selitem;
				dataGrid . ScrollIntoView ( selitem );
				dataGrid . Focus ( );
			}
			else if ( dataGrid . Name == "OrdersGrid" )
			{
				nworder selitem = dataGrid . SelectedItem as nworder;
				dataGrid . SelectedItem = selitem;
				dataGrid . ScrollIntoView ( selitem );
			}
			else if ( dataGrid . Name == "OrderDetailsGrid" )
			{
				nworderdetail selitem = dataGrid . SelectedItem as nworderdetail;
				dataGrid . SelectedItem = selitem;
				dataGrid . ScrollIntoView ( selitem );
			}
			else if ( dataGrid . Name == "ProductsGrid" )
			{
				nwproduct selitem = dataGrid . SelectedItem as nwproduct;
				dataGrid . SelectedItem = selitem;
				dataGrid . ScrollIntoView ( selitem );
			}
			else if ( dataGrid . Name == "CategoriesGrid" )
			{
				nwcategory selitem = dataGrid . SelectedItem as nwcategory;
				dataGrid . SelectedItem = selitem;
				dataGrid . ScrollIntoView ( selitem );
			}

			return;
		}

		private void CloseReturnButton_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			Close ( );
		}

		private void CategoriesGrid_DataContextChanged ( object sender, DependencyPropertyChangedEventArgs e )
		{
			int x = 0;
		}

		private void CustomersGrid_DataContextChanged ( object sender, DependencyPropertyChangedEventArgs e )
		{
			int x = 0;
		}

		private void OrdersGrid_DataContextChanged ( object sender, DependencyPropertyChangedEventArgs e )
		{
			int x = 0;
		}

		private void ProductsGrid_DataContextChanged ( object sender, DependencyPropertyChangedEventArgs e )
		{
			int x = 0;
		}

		private void OrderDetailsGrid_DataContextChanged ( object sender, DependencyPropertyChangedEventArgs e )
		{
			int x = 0;
		}

		private void Button_Click ( object sender, RoutedEventArgs e )
		{
			//Export Orders  data to CSV
			StringBuilder sb = new StringBuilder ( );
			string Output = "";
			//foreach ( var item in OrdersGrid . Items )
			//{
			//	nworder nwo = new nworder ( );
			//	nwo = item as nworder;
			//	sb . Append ( nwo . OrderId . ToString ( ) + "," );
			//	sb . Append ( nwo . CustomerId . ToString ( ) + "," );
			//	sb . Append ( nwo . EmployeeId . ToString ( ) + "," );
			//	sb . Append ( nwo . OrderDate . ToString ( ) + "," );
			//	sb . Append ( nwo . RequiredDate . ToString ( ) + "," );
			//	sb . Append ( nwo . ShippedDate . ToString ( ) + "," );
			//	sb . Append ( nwo . ShipVia . ToString ( ) + "\n" );
			//	sb . Append ( nwo . Freight . ToString ( ) + "," );
			//	sb . Append ( nwo . ShipName . ToString ( ) + "," );
			//	sb . Append ( nwo . ShipAddress . ToString ( ) + "," );
			//	sb . Append ( nwo . ShipCity . ToString ( ) + "," );
			//	sb . Append ( nwo . ShipRegion . ToString ( ) + "," );
			//	sb . Append ( nwo . ShipPostalCode . ToString ( ) + "," );
			//	sb . Append ( nwo . ShipCountry . ToString ( ) + "," );
			//}
			//Output = sb . ToString ( );
			//File . WriteAllText ( @"C:\users\ianch\Documents\nworders.csv", Output );
		}

		private void Import_Click ( object sender, RoutedEventArgs e )
		{
			string Output = "", line = "";
			string [ ] lines, fields;
			int x = 0;
			nworder nw = new nworder ( );
			Output = File . ReadAllText ( @"C:\users\ianch\Documents\nworders.csv" );
			while ( true )
			{
				lines = Output . Split ( '\n' );
				fields = lines [ x ] . Split ( ',' );
				nw . OrderId = Convert . ToInt32 ( fields [ 0 ] );
			}

		}

		private void Std_Click ( object sender, RoutedEventArgs e )
		{
			//			NorthWindGrid nwg = new NorthWindGrid ( );
			//nwg . Show ( );
		}

		private void MenuItem_Click ( object sender, RoutedEventArgs e )
		{

		}


		private void Window_Closing ( object sender, CancelEventArgs e )
		{
			EventControl . NwCustomerSelected -= EventControl_NwCustomerSelected;
			Flags . NwSelectionWindow = null;
		}
		private void ShowOrderDetailedView_Click ( object sender, RoutedEventArgs e )
		{
			if ( Flags . NwSelectionWindow != null )
			{
				nwcustomer cg = new nwcustomer ( );
				cg = CustomersGrid . SelectedItem as nwcustomer;
				EventControl . TriggerNwCustomerSelected ( this, new NwGridArgs { ArgumentParameter = cg . CustomerId } );
			}
			else
			{
			
			}
		}

		private void ShowDetailedView_Click ( object sender, RoutedEventArgs e )
		{
			nwcustomer cg = new nwcustomer ( );
			cg = CustomersGrid . SelectedItem as nwcustomer;
			EventControl . TriggerNwCustomerSelected ( this, new NwGridArgs { ArgumentParameter = cg . CustomerId } );
		}

		private void NwStandard_PreviewMouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{
			ContextMenu cm = this . FindResource ( "NwContextMenu" ) as ContextMenu;
			cm . PlacementTarget = this . CustomersGrid as DataGrid;
			cm . IsOpen = true;
		}

		private void CloseReturnButton_PreviewMouseMove ( object sender, MouseEventArgs e )
		{
			SolidColorBrush scb = new SolidColorBrush ( );
			CloseReturnButton cb = sender as CloseReturnButton;
//			scb = cb .Ellipse9.Fill as SolidColorBrush;
//			Color c = scb . Color;
//			scb = this . Background as SolidColorBrush;
		}

		private void CloseReturnButton_Loaded ( object sender, RoutedEventArgs e )
		{

		}
	}

}

