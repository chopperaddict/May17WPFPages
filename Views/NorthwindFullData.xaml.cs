using System;
using System . Collections . Generic;
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
using WPFPages . ViewModels;
using System . ComponentModel;
using static WPFPages . NorthWind;
using System . Collections . ObjectModel;
using System . IO;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for NorthwindFullData.xaml
	/// </summary>
	public partial class NorthwindFullData : Window
	{

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

		//private int customersTotal;
		//private int productsTotal;
		//private int ordersTotal;
		//private int orderdetailsTotal;
		//private int categoriesTotal;
		//private int customerCurrent;
		//private int orderCurrent;
		//public int CustomersTotal
		//{
		//	get
		//	{
		//		return customersTotal;
		//	}
		//	set
		//	{
		//		customersTotal = value;
		//		OnPropertyChanged ( nameof ( CustomersTotal ) );
		//	}
		//}
		//public int ProductsTotal
		//{
		//	get
		//	{
		//		return productsTotal;
		//	}
		//	set
		//	{
		//		productsTotal = value;
		//		OnPropertyChanged ( nameof ( ProductsTotal ) );
		//	}
		//}
		//public int OrdersTotal
		//{
		//	get
		//	{
		//		return ordersTotal;
		//	}
		//	set
		//	{
		//		ordersTotal = value;
		//		OnPropertyChanged ( nameof ( OrdersTotal ) );
		//	}
		//}
		//public int OrderDetailsTotal
		//{
		//	get
		//	{
		//		return orderdetailsTotal;
		//	}
		//	set
		//	{
		//		orderdetailsTotal = value;
		//		OnPropertyChanged ( nameof ( OrderDetailsTotal ) );
		//	}
		//}
		//public int CategoriesTotal
		//{
		//	get
		//	{
		//		return categoriesTotal;
		//	}
		//	set
		//	{
		//		categoriesTotal = value;
		//		OnPropertyChanged ( nameof ( CategoriesTotal ) );
		//	}
		//}
		//public int CustomerCurrent
		//{
		//	get
		//	{
		//		return customerCurrent;
		//	}
		//	set
		//	{
		//		customerCurrent = value;
		//		OnPropertyChanged ( nameof ( CustomerCurrent ) );
		//	}
		//}
		//public int OrderCurrent
		//{
		//	get
		//	{
		//		return this . orderCurrent;
		//	}
		//	set
		//	{
		//		this . orderCurrent = value;
		//		OnPropertyChanged ( nameof ( OrderCurrent ) );
		//	}
		//}
		#endregion Full Properties

		public DataSet ds = new DataSet ( );

		public nwcustomer NwCust = new nwcustomer ( );
//		public CustCollection NwCustCollection = new CustCollection ( );
		public NwOrderCollection NwOrdCollection = new NwOrderCollection ( "" );
		public NwOrderDetails NwOdCollection = new NwOrderDetails ( -1 );
		public NwProductCollection NwProdCollection = new NwProductCollection ( -1 );
		public NwCatCollection NwcatCollection = new NwCatCollection ( -1 );

		public ObservableCollection<nwcustomer> nwc;


		public NorthwindFullData ( )
		{
			InitializeComponent ( );
			nwc = new ObservableCollection<nwcustomer> ( );
			nwc = NwCust . Loadcustomers ( );
			CustomersGrid . ItemsSource = nwc;
			NwCust . CustomersTotal = CustomersGrid . Items . Count;
			CustomersTot . Text = CustomersGrid.Items.Count.ToString ( );

			DataContext = NwCust;
			//			CustomersGrid . DataContext = NwCust;
			//			this .  nwc;
			EventControl . NwCustomerSelected += EventControl_NwCustomerSelected;

		}

		private void EventControl_NwCustomerSelected ( object sender, NwGridArgs e )
		{
			//handle 
			string srchterm = e . ArgumentParameter;

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
			//			CustomersGrid . DataContext = nwc;
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
		public void FillAllGrids ( string caller, int index )
		{
			bool isworking = false;
			//if ( caller == "Main" )
			//{
			//	isworking = true;
			//	IsLoading = true;
			//	this . CustomersGrid . SelectedIndex = index;
			//	NwCustomerCollection NwCustCollection = new NwCustomerCollection ( true );
			//	this . CustomersGrid . ItemsSource = NwCustCollection;
			//	this . CustomersGrid . DataContext = NwCustCollection;
			//	this . CustomersGrid . SelectedIndex = index;
			//	this . CustomersGrid . SelectedItem = index;
			//	Utils . SetGridRowSelectionOn ( CustomersGrid, index );
			//}
			//Now get The selected Customer item and use it to load the OrdersGrid with correct set of matching records
			if ( caller == "Customer" || isworking )
			{
				isworking = true;
				IsLoading = true;
				var srchitem1 = CustomersGrid . SelectedItem as nwcustomer;
				if ( srchitem1 != null )
				{
					int rec = FindMatchingRecord ( srchitem1 as object, OrdersGrid );
					OrdersGrid . SelectedIndex = rec;
					OrdersGrid . SelectedItem = rec;
					Utils . SetGridRowSelectionOn ( OrdersGrid, rec );
				}
			}
			//Now get The selected OrderId item and use it to load the OrderDetailsGrid with correct set of matching records
			// This is the master grid to allow access to 2 way access to Product and Order tables !!
			if ( caller == "Order" || isworking )
			{
				isworking = true;
				IsLoading = true;
				var v2 = OrdersGrid . SelectedItem as nworder;
				if ( v2 != null )
				{
					NwOrderDetails NwOdCollection = new NwOrderDetails ( v2 . OrderId );
					//					OrderDetailsGrid . DataContext = NwOdCollection;
					OrderDetailsGrid . ItemsSource = NwOdCollection;
					OrderDetailsGrid . SelectedIndex = 0;
					OrderDetailsGrid . SelectedItem = 0;
					Utils . SetGridRowSelectionOn ( OrderDetailsGrid, 0 );
				}
			}

			//Now get The selected ProductId item and use it to load the ProductsGrid with correct set of matching records
			if ( caller == "OrderDetail" || isworking )
			{
				isworking = true;
				IsLoading = true;
				var v4 = OrderDetailsGrid . SelectedItem as nworderdetail;
				if ( v4 != null )
				{
					NwProductCollection NwProdCollection = new NwProductCollection ( v4 . ProductId );
					//					ProductsGrid . DataContext = NwProdCollection;
					ProductsGrid . ItemsSource = NwProdCollection;
					ProductsGrid . SelectedIndex = 0;
					ProductsGrid . SelectedItem = 0;
					Utils . SetGridRowSelectionOn ( ProductsGrid, 0 );
				}
			}

			if ( caller == "Product" || isworking )
			{
				isworking = true;
				IsLoading = true;
				var srchitem4 = ProductsGrid . SelectedItem as nwproduct;
				if ( srchitem4 != null )
				{
					NwCatCollection NwcatCollection = new NwCatCollection ( srchitem4 . CategoryId );
					//					CategoriesGrid . DataContext = NwcatCollection;
					CategoriesGrid . ItemsSource = NwcatCollection;
					CategoriesGrid . SelectedIndex = 0;
					CategoriesGrid . SelectedItem = 0;
					Utils . SetGridRowSelectionOn ( CategoriesGrid, 0 );
				}
			}
			isworking = false;
			IsLoading = false;
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
					if ( SearchType == "ORDERID" )
					{
						if ( cvm . OrderId == Convert . ToInt32 ( srchitem ) )
						{
							break;
						}
					}
					else if ( SearchType == "CUSTOMERID" )
					{
						if ( (string)cvm . CustomerId ==  (string)srchitem )
						{
							break;
						}
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
			//			DataTable dt = new DataTable ( "Products" );
			string ConString = ( string ) Properties . Settings . Default [ "NorthwindConnectionString" ];
			//int Ordervalue = Convert . ToInt32 ( orderId );
			string CmdString = string . Empty;
			using ( SqlConnection con = new SqlConnection ( ConString ) )
			{
				CmdString = $"SELECT *  FROM [Customers];" +
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
			//DataTable dt1 = ds . Tables [ "Table" ];
			//DataTable dt2 = ds . Tables [ "Orders" ];
			//DataTable dt3 = ds . Tables [ "OrderDetails" ];
			//DataTable dt4 = ds . Tables [ "Products" ];
			//DataTable dt5 = ds . Tables [ "Categories" ];
			// This is how to extract a DataTable from a DataSet
			DataTable dt1 = ds . Tables [ 0 ] . DataSet . Tables [ 0 ];
			DataTable dt2 = ds . Tables [ 0 ] . DataSet . Tables [ 1 ];
			DataTable dt3 = ds . Tables [ 0 ] . DataSet . Tables [ 2 ];
			DataTable dt4 = ds . Tables [ 0 ] . DataSet . Tables [ 3 ];
			DataTable dt5 = ds . Tables [ 0 ] . DataSet . Tables [ 4 ];
			LoadFullDataGrids ( );
			CustomersTot . Text = NwCust.CustomersTotal . ToString ( );
			OrdersTot . Text = NwCust . OrdersTotal . ToString ( );
			ProductsTot . Text = NwCust . ProductTotal . ToString ( );
			OrderDetailsTot . Text = NwCust . OrderDetailsTotal . ToString ( );
			CategoriesTot . Text = NwCust . CategoriesTotal . ToString ( );
			this . Refresh ( );
			return ds;
		}
		private CustCollection LoadCustomerData ( )
		{
			int count = 0;
			DataTable dtCust = ds . Tables [ 0 ];
			CustCollection Custinternalcollection = new CustCollection ( );
			try
			{
				for ( int i = 0 ; i < dtCust . Rows . Count ; i++ )
				{
					Custinternalcollection . Add ( new CustomerViewModel
					{
						Id = Convert . ToInt32 ( dtCust . Rows [ i ] [ 0 ] ),
						CustNo = dtCust . Rows [ i ] [ 1 ] . ToString ( ),
						BankNo = dtCust . Rows [ i ] [ 2 ] . ToString ( ),
						AcType = Convert . ToInt32 ( dtCust . Rows [ i ] [ 3 ] ),
						FName = dtCust . Rows [ i ] [ 4 ] . ToString ( ),
						LName = dtCust . Rows [ i ] [ 5 ] . ToString ( ),
						Addr1 = dtCust . Rows [ i ] [ 6 ] . ToString ( ),
						Addr2 = dtCust . Rows [ i ] [ 7 ] . ToString ( ),
						Town = dtCust . Rows [ i ] [ 8 ] . ToString ( ),
						County = dtCust . Rows [ i ] [ 9 ] . ToString ( ),
						PCode = dtCust . Rows [ i ] [ 10 ] . ToString ( ),
						Phone = dtCust . Rows [ i ] [ 11 ] . ToString ( ),
						Mobile = dtCust . Rows [ i ] [ 12 ] . ToString ( ),
						Dob = Convert . ToDateTime ( dtCust . Rows [ i ] [ 13 ] ),
						ODate = Convert . ToDateTime ( dtCust . Rows [ i ] [ 14 ] ),
						CDate = Convert . ToDateTime ( dtCust . Rows [ i ] [ 15 ] )
					} );
					count = i;
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"CUSTOMERS : ERROR {ex . Message} + {ex . Data} ...." );
				MessageBox . Show ( $"CUSTOMERS : ERROR :\n		Error was  : [{ex . Message}] ...." );
			}
			Flags . CustCollection = Custinternalcollection;
			return Custinternalcollection;
		}

		private void LoadFullDataGrids ( )
		{
			IsLoading = true;

			//			CustomersGrid . Items . Clear ( );
			//			DataTable dt = ds . Tables [ 0 ];
			//			LoadCustomerData ( );
			//			CustomersGrid . ItemsSource = NwCustCollection;
			//			CustomersGrid . SelectedIndex = 0;
			//			CustomersGrid . SelectedItem = 0;
			//			Utils . SetGridRowSelectionOn ( CustomersGrid, 0 );
			//			CustomersTotal = CustomersGrid . Items . Count;
			//Force the selected row to be FULLY selected
			Utils . SetUpGridSelection ( this . CustomersGrid, 0);

			OrdersGrid . Items . Clear ( );
			OrdersGrid . ItemsSource = NwOrdCollection;
			OrdersGrid . SelectedIndex = 0;
			OrdersGrid . SelectedItem = 0;
			Utils . SetGridRowSelectionOn ( this . OrdersGrid, 0 );
			NwCust . OrdersTotal = this . OrdersGrid . Items . Count;

			OrderDetailsGrid . ItemsSource = NwOdCollection;
			OrderDetailsGrid . SelectedIndex = 0;
			OrderDetailsGrid . SelectedItem = 0;
			Utils . SetGridRowSelectionOn ( OrderDetailsGrid, 0 );
			NwCust . OrderDetailsTotal = OrderDetailsGrid . Items . Count;

			ProductsGrid . ItemsSource = NwProdCollection;
			ProductsGrid . SelectedIndex = 0;
			ProductsGrid . SelectedItem = 0;
			Utils . SetGridRowSelectionOn ( ProductsGrid, 0 );
			NwCust . ProductTotal = ProductsGrid . Items . Count;

			CategoriesGrid . ItemsSource = NwcatCollection;
			CategoriesGrid . SelectedIndex = 0;
			CategoriesGrid . SelectedItem = 0;
			Utils . SetGridRowSelectionOn ( CategoriesGrid, 0 );
			NwCust . CategoriesTotal = CategoriesGrid . Items . Count;
			IsLoading = false;

		}
		private void CustomerGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( IsLoading )
				return;
			var v = CustomersGrid . SelectedItem as nwcustomer;
			if ( v == null )
				return;
			Debug . WriteLine ( $"this.DataContext = {this . DataContext}" );
			// NB This next line triggers the actual update on screen
			NwCust . CurrentCustomer = CustomersGrid . SelectedIndex;
			CustCurrent . Refresh ( );
			//Find 1st match in Orders Db Grid and select it
			IsLoading = true;
			int rec = FindMatchingRecord ( v . CustomerId, OrdersGrid, "CUSTOMERID" );
//			OrdersGrid . SelectedIndex = rec;
			Utils . SetGridRowSelectionOn ( OrdersGrid, rec );

			// NB This next line triggers the actual update on screen
			NwCust . CurrentOrder = OrdersGrid . SelectedIndex;

			var v2 = OrdersGrid . SelectedItem as nworder;
			if ( v2 == null )
			{
				IsLoading = false;
				return;
			}

			IsLoading = true;
			//Find 1st match in Orders Db Grid and select it
			rec = FindMatchingRecord ( v2 . OrderId, OrderDetailsGrid );
			OrderDetailsGrid . SelectedIndex = rec;
			Utils . SetGridRowSelectionOn ( OrderDetailsGrid, rec );
			NwCust . OrderCurrent = rec;
			var v3 = OrderDetailsGrid . SelectedItem as nworderdetail;
			if ( v3 == null )
			{
				IsLoading = false;
				return;
			}
			IsLoading = true;
			// NB This next line triggers the actual update on screen
			NwCust . CurrentOrderDetail = OrderDetailsGrid . SelectedIndex;
			//Find 1st match in Products Db Grid and select it
			rec = FindMatchingRecord ( v3 . ProductId, ProductsGrid );
			ProductsGrid . SelectedIndex = rec;
			Utils . SetGridRowSelectionOn ( ProductsGrid, rec );

			// NB This next line triggers the actual update on screen
			NwCust . CurrentProduct = ProductsGrid . SelectedIndex;
			ProductsGrid . Refresh ( );

			var v4 = ProductsGrid . SelectedItem as nwproduct;
			if ( v4 == null )
			{
				IsLoading = false;
				return;
			}
			IsLoading = true;
			// NB We MUST set the datacontext to the current selected Customer Record for XAML updates to work
			//this . DataContext = v4;
			//Find 1st match in Products Db Grid and select it
			rec = FindMatchingRecord ( v4 . CategoryId, CategoriesGrid );
			Utils . SetGridRowSelectionOn ( CategoriesGrid, rec );
			CategoriesGrid . SelectedIndex = rec;

			// NB This next line triggers the actual update on screen
			NwCust . CurrentCategory = CategoriesGrid . SelectedIndex;

			CustomersTot . Text = NwCust . CustomersTotal . ToString ( );
			//CustCurrent . Text = CustomerCurrent . ToString ( );
			
			IsLoading = false;
			
			if ( Flags . NwSelectionWindow != null )
			{
				EventControl . TriggerNwCustomerSelected ( this, new NwGridArgs { ArgumentParameter = v . CustomerId } );
			}
		}

		private void OrdersGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( IsLoading )
				return;
			var v = OrdersGrid . SelectedItem as nworder;
			if ( v == null )
				return;
			//			this . DataContext = v;

			// NB This next line triggers the actual update on screen
			NwCust . CurrentOrder = OrdersGrid . SelectedIndex;

			//Find 1st match in Orders Db Grid and select it
			IsLoading = true;
			int rec = FindMatchingRecord ( v . OrderId, OrderDetailsGrid);
			Utils . SetGridRowSelectionOn ( OrderDetailsGrid, rec );
			OrderDetailsGrid . SelectedIndex = rec;

			// NB This next line triggers the actual update on screen
			NwCust . CurrentOrderDetail = OrderDetailsGrid . SelectedIndex;

			var v3 = OrderDetailsGrid . SelectedItem as nworderdetail;
			if ( v3 == null )
			{
				IsLoading = false;
				return;
			}

			IsLoading = true;
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
			// NB This next line triggers the actual update on screen
			NwCust . CurrentProduct = ProductsGrid . SelectedIndex;

			IsLoading = true;
			//Find 1st match in Products Db Grid and select it
			rec = FindMatchingRecord ( v4 . CategoryId, CategoriesGrid );
			Utils . SetGridRowSelectionOn ( CategoriesGrid, rec );

			// NB This next line triggers the actual update on screen
			NwCust . CurrentCategory = CategoriesGrid . SelectedIndex;

			IsLoading = false;
		}
		private void ProductsGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( IsLoading )
				return;
			var v = ProductsGrid . SelectedItem as nwproduct;
			if ( v == null )
				return;
			// NB This next line triggers the actual update on screen
			NwCust . CurrentProduct = ProductsGrid . SelectedIndex;
			IsLoading = true;

			//Find 1st match in Orders Db Grid and select it
			int rec = FindMatchingRecord ( v . CategoryId, CategoriesGrid );
			Utils . SetGridRowSelectionOn ( CategoriesGrid, rec );
			//Find ProductId in Product Db Grid use it to find OrderId in OrderDetails grid
			IsLoading = true;
			rec = FindMatchingRecord ( v . ProductId, OrderDetailsGrid, "PRODUCTID" );
			Utils . SetGridRowSelectionOn ( OrderDetailsGrid, rec );
			// NB This next line triggers the actual update on screen
			NwCust . CurrentOrderDetail = OrderDetailsGrid . SelectedIndex;
			IsLoading = false;

		}
		private void CategoriesGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( IsLoading )
				return;
			var v = CategoriesGrid . SelectedItem as nwcategory;
			if ( v == null )
				return;
			// NB This next line triggers the actual update on screen
			NwCust . CurrentCategory = CategoriesGrid . SelectedIndex;
			IsLoading = true;
			//Find 1st match in Products Db Grid and select it
			int rec = FindMatchingRecord ( v . CategoryId, ProductsGrid, "PRODUCTID" );
			Utils . SetGridRowSelectionOn ( ProductsGrid, rec );
			// NB This next line triggers the actual update on screen
			NwCust . CurrentProduct = ProductsGrid . SelectedIndex;
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
			int rec = FindMatchingRecord ( v . OrderId, OrdersGrid, "ORDERID" );
			OrdersGrid . SelectedIndex = rec;
			Utils . SetGridRowSelectionOn ( OrdersGrid, rec );
			IsLoading = true;

			// NB This next line triggers the actual update on screen
			NwCust . CurrentOrder = OrdersGrid . SelectedIndex;

			//Find 1st match in Products Db Grid and select it
			rec = FindMatchingRecord ( v . ProductId, ProductsGrid );
			Utils . SetGridRowSelectionOn ( ProductsGrid, rec );
			ProductsGrid . Refresh ( );

			var v2 = ProductsGrid . SelectedItem as nwproduct;
			if ( v2 == null )
			{
				IsLoading = false;
				return;
			}
			IsLoading = true;

			// NB This next line triggers the actual update on screen
			NwCust . CurrentProduct = ProductsGrid . SelectedIndex;

			//Find 1st match in Products Db Grid and select it
			rec = FindMatchingRecord ( v2 . CategoryId, CategoriesGrid );
			Utils . SetGridRowSelectionOn ( CategoriesGrid, rec );
			// NB This next line triggers the actual update on screen
			NwCust . CurrentCategory = CategoriesGrid . SelectedIndex;
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

		private void Export_Click ( object sender, RoutedEventArgs e )
		{
			//Export Orders  data to CSV
			StringBuilder sb = new StringBuilder ( );
			string Output = "";
			foreach ( var item in OrdersGrid . Items )
			{
				nworder nwo = new nworder ( );
				nwo = item as nworder;
				sb . Append ( nwo . OrderId . ToString ( ) + "," );
				sb . Append ( nwo . CustomerId . ToString ( ) + "," );
				sb . Append ( nwo . EmployeeId . ToString ( ) + "," );
				sb . Append ( nwo . OrderDate . ToString ( ) + "," );
				sb . Append ( nwo . RequiredDate . ToString ( ) + "," );
				sb . Append ( nwo . ShippedDate . ToString ( ) + "," );
				sb . Append ( nwo . ShipVia . ToString ( ) + "\n" );
				sb . Append ( nwo . Freight . ToString ( ) + "," );
				sb . Append ( nwo . ShipName . ToString ( ) + "," );
				sb . Append ( nwo . ShipAddress . ToString ( ) + "," );
				sb . Append ( nwo . ShipCity . ToString ( ) + "," );
				sb . Append ( nwo . ShipRegion . ToString ( ) + "," );
				sb . Append ( nwo . ShipPostalCode . ToString ( ) + "," );
				sb . Append ( nwo . ShipCountry . ToString ( ) + "," );
			}
			Output = sb . ToString ( );
			File . WriteAllText ( @"C:\users\ianch\Documents\nworders.csv", Output );
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
			SelectedNwDetails nwg = new SelectedNwDetails ( );
			nwg . Show ( );
		}

		private void Window_Closing ( object sender, CancelEventArgs e )
		{
			EventControl . NwCustomerSelected -= EventControl_NwCustomerSelected;
		}
		private void ShowOrderDetailedView_Click ( object sender, RoutedEventArgs e )
		{
			nwcustomer cg = new nwcustomer ( );
			cg = CustomersGrid . SelectedItem as nwcustomer;
			EventControl . TriggerNwCustomerSelected ( this, new NwGridArgs { ArgumentParameter = cg . CustomerId } );
		}

		private void ShowDetailedView_Click ( object sender, RoutedEventArgs e )
		{

			nwcustomer cg = new nwcustomer ( );
			cg = CustomersGrid . SelectedItem as nwcustomer;
			EventControl . TriggerNwCustomerSelected ( this, new NwGridArgs { ArgumentParameter = cg . CustomerId } );
		}

		private void NwStandard_PreviewMouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{
			ContextMenu cm = this . FindResource ( "NwDetailsContextMenu" ) as ContextMenu;
			cm . PlacementTarget = this . CustomersGrid as DataGrid;
			cm . IsOpen = true;
		}

		private void ShowSelectedView_Click ( object sender, RoutedEventArgs e )
		{
			string arg = "";
			nwcustomer nwc = new nwcustomer ( );
			nwc = CustomersGrid . SelectedItem as nwcustomer;
			SelectedNwDetails snd = new SelectedNwDetails ( nwc . CustomerId );
			snd . Show ( );
		}

		private void CloseReturnButton_Loaded ( object sender, RoutedEventArgs e )
		{
			//			UserControl uc = sender as UserControl;
			//			var b = Utils . BrushFromColors ( Colors . Red );
			//			Txtblk.Background = b;
			//			b = Utils.BrushFromHashString ( "FF00FF00" );
			Foreground= (Brush)FindResource("Red5");
		}

		private void CloseReturnButton_PreviewMouseMove ( object sender, MouseEventArgs e )
		{
			UserControl uc = sender as UserControl;
			
		}
	}
}

