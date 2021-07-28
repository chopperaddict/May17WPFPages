using System . Data . SqlClient;
using System . Data;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Input;
using System;
using System . Diagnostics;
using System . Windows . Media;
using System . Collections . ObjectModel;
using System . Windows . Controls . Primitives;
using System . Collections . Generic;
using WPFPages . Commands;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for NorthWindGrid.xaml
	/// </summary>
	public partial class NorthWindGrid : Window
	{
		// Declare public pointers to our data classes
		public nwcustomer NwCust = new nwcustomer ( );
		public ObservableCollection<nwcustomer> nwc;
//		public ICommand LoadNwCommand {get; set;}

		public bool IsLoading
		{
			get; set;
		}
		public NorthWindGrid ( )
		{
			InitializeComponent ( );
			nwc = new ObservableCollection<nwcustomer> ( );
			nwc = NwCust . Loadcustomers ( );
			CustomersGrid . ItemsSource = nwc;
			CustomersGrid . DataContext = NwCust;
			EventControl . NwCustomerSelected += EventControl_NwCustomerSelected;
			// Initialise the "LoadNwCommand" Command
			//			LoadNwCommand = new Command(ExecuteMethod, canExecuteMethod );
		}
		//private bool canExecuteMethod (object parameter ){return true;}
		//private void ExecuteMethod ( object parameter )
		//{
		//	//handle the actual command code here
		//	MessageBox . Show ( "Command is run..." );

		//}

		private void EventControl_NwCustomerSelected ( object sender, NwGridArgs e )
		{
			//Find CustomerGrid_SelectionChanged  passed to us from the Full Datawindow
			int x = 0;
		}

		private void Window_Loaded ( object sender, RoutedEventArgs e )
		{
			IsLoading = true;
			//Load the Customers MAIN database and all others as required
			FillAllGrids ( "Customer", 0 );
			IsLoading = false;
			// This creates a DataSet containing ALL data
			FillMultiTables ( );
			Utils . SetupWindowDrag ( this );
			CustomersGrid . Focus ( );
			IsLoading = false;
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
			if ( caller == "Main" )
			{
				//isworking = true;
				//IsLoading = true;
				//NwCust NwOrdCollection = new NwOrderCollection ( srchitem1 . CustomerId );
				//CustomersGrid . SelectedIndex = index;
				//CustomersGrid . ItemsSource = NwCustCollection;
				//this . CustomersGrid . DataContext = NwCustCollection;
				//this . CustomersGrid . SelectedIndex = index;
				//this . CustomersGrid . SelectedItem = index;
				//Utils . SetGridRowSelectionOn ( CustomersGrid, index );
			}
			//Now get The selected Customer item and use it to load the OrdersGrid with correct set of matching records
			if ( caller == "Customer" || isworking )
			{
				isworking = true;
				IsLoading = true;
				var srchitem1 = CustomersGrid . SelectedItem as nwcustomer;
				if ( srchitem1 != null )
				{
					//Get the selected CustomerId item and use it to load the OrdersGrid  with correct set of matching records
					NwOrderCollection NwOrdCollection = new NwOrderCollection( srchitem1 . CustomerId );
//					NwOrderCollection NwOrdCollection = new NwOrderCollection ( srchitem1 . CustomerId );
					OrdersGrid . DataContext = NwOrdCollection;
					OrdersGrid . ItemsSource = NwOrdCollection;
					OrdersGrid . SelectedIndex = 0;
					OrdersGrid . SelectedItem = 0;
					Utils . SetGridRowSelectionOn ( OrdersGrid, index );
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
					OrderDetailsGrid . DataContext = NwOdCollection;
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
//					NwProductCollection NwProdCollection = new NwProductCollection ( v4 . ProductId );
					NwProductCollection NwProdCollection = new NwProductCollection (-1);
					ProductsGrid . DataContext = NwProdCollection;
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
					CategoriesGrid . DataContext = NwcatCollection;
					CategoriesGrid . ItemsSource = NwcatCollection;
					CategoriesGrid . SelectedIndex = 0;
					CategoriesGrid . SelectedItem = 0;
					Utils . SetGridRowSelectionOn ( CategoriesGrid, 0 );
				}
			}
			isworking = false;
			IsLoading = false;
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
				CmdString = $"SELECT *  FROM [Customers];" + 
						" Select * from [Orders]; " + 
						" select * from [Order Details]; " + 
						" Select * from [Products]; " + 
						" sele	ct * from [Categories]";
				SqlCommand cmd = new SqlCommand ( CmdString, con );
				SqlDataAdapter sda = new SqlDataAdapter ( cmd );
				//sda . TableMappings . Add ( "Table", "Customers" );
				//sda . TableMappings . Add ( "Table1", "Orders" );
				//sda . TableMappings . Add ( "Table2", "OrderDetails" );
				//sda . TableMappings . Add ( "Table3", "Products" );
				//sda . TableMappings . Add ( "Table4", "Categories" );
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
			// This is how tp extract a DataTable from a DataSet
			//DataTable dt1 = ds . Tables [ 0 ] . DataSet . Tables [ 0 ];
			//DataTable dt2 = ds . Tables [ 0 ] . DataSet . Tables [ 1 ];
			//DataTable dt3 = ds . Tables [ 0 ] . DataSet . Tables [ 2 ];
			//DataTable dt4 = ds . Tables [ 0 ] . DataSet . Tables [ 3 ];
			//DataTable dt5 = ds . Tables [ 0 ] . DataSet . Tables [ 4 ];
			//showdatasetContentCounts ( "dt1", dt1 );
			//showdatasetContentCounts ( "dt2", dt2 );
			//showdatasetContentCounts ( "dt3", dt3 );
			//showdatasetContentCounts ( "dt4", dt4 );
			//showdatasetContentCounts ( "dt5", dt5 );
			return ds;
		}

		private void showdatasetContentCounts (string dtname, DataTable dt1 )
		{
			int counter = 0;
			var nwc = dt1 . AsEnumerable ( );
			foreach ( var item in nwc )
			{
				var x = item [ 0 ];
				counter++;
			}
			Debug . WriteLine ( $"{dtname}.Count = {counter}" );

		}
		private void CustomerGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( IsLoading )
				return;
			FillAllGrids ( "Customer", CustomersGrid . SelectedIndex );
		}


		private void OrdersGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( IsLoading )
				return;

			FillAllGrids ( "Order", OrdersGrid . SelectedIndex );
		}

		private void ProductsGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( IsLoading )
				return;

			FillAllGrids ( "Product", ProductsGrid . SelectedIndex );
		}

		private void CategoriesGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( IsLoading )
				return;
			FillAllGrids ( "Category", CategoriesGrid . SelectedIndex );
			return;

		}


		private void OrderDetailsGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			if ( IsLoading )
				return;
			//Now get The selected Customer item and use it to load the OrdersGrid with correct set of matching records

			FillAllGrids ( "OrderDetail", 0 );
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


			//DataGridRow row = dataGrid . ItemContainerGenerator . ContainerFromIndex ( rowIndex ) as DataGridRow;
			//// if NOT selected, make it selected
			//if ( row == null )
			//{
			//	/* bring the data item (Product object) into view
			//	 * in case it has been virtualized away */
			//	dataGrid . ScrollIntoView ( item );
			//	row = dataGrid . ItemContainerGenerator . ContainerFromIndex ( rowIndex ) as DataGridRow;
			//}
			//TODO: Retrieve and focus a DataGridCell object
			//			dataGrid . SelectedIndex = rowIndex;
			//			dataGrid . SelectedItem = rowIndex;
		}

		private void CloseReturnButton_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			Close ( );
		}

		private void CustomersGrid_Selected ( object sender, RoutedEventArgs e )
		{
			Brush br = Utils . GetDictionaryBrush ( "Magenta5" );
			CustomersGrid . Background = br;
		}

		private void CategoriesGrid_DataContextChanged ( object sender, DependencyPropertyChangedEventArgs e )
		{
			int x = 0;
		}

		private void Selected_Click ( object sender, RoutedEventArgs e )
		{
			SelectedNwDetails nwfd = new SelectedNwDetails ( );
			nwfd . Show ( );
		}

		private void Window_Closing ( object sender, System . ComponentModel . CancelEventArgs e )
		{
			EventControl . NwCustomerSelected -= EventControl_NwCustomerSelected;
		}

		private void MenuItem_Click ( object sender, RoutedEventArgs e )
		{

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
			ContextMenu cm = this . FindResource ( "NwContextMenu" ) as ContextMenu;
			cm . PlacementTarget = this . CustomersGrid as DataGrid;
			cm . IsOpen = true;
		}


	}
}
