using System;
using System . Collections . Generic;
using System . Data . SqlClient;
using System . Data;
using System . Diagnostics;
using System . Linq;
using System . Security . Policy;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Controls;
using System . Collections . ObjectModel;
using System . Windows;
using System . ComponentModel;

namespace WPFPages . Views
{
	public class NwProductCollection : ObservableCollection<nwproduct>
	{
		//		public ObservableCollection<nwproduct> NwProducts;

		public NwProductCollection ( int arg )
		{
			LoadProductDetails ( arg );
		}
		public static DataTable LoadProducts ( )
		{
			DataTable dt = new DataTable ( "Products" );
			string ConString = ( string ) Properties . Settings . Default [ "NorthwindConnectionString" ];
			string CmdString = string . Empty;
			using ( SqlConnection con = new SqlConnection ( ConString ) )
			{
				CmdString = $"SELECT *  FROM [Products]";
				SqlCommand cmd = new SqlCommand ( CmdString, con );
				SqlDataAdapter sda = new SqlDataAdapter ( cmd );
				try
				{
					sda . Fill ( dt );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"Data={ex . Data}, {ex . Message}\n[{CmdString}]" );
				}
			}
			return dt;
		}
		public NwProductCollection LoadProductDetails ( int orderId = -1 )
		{
			//if ( grid == null )
			//	return null;
			DataTable dt = new DataTable ( "Products" );
			string ConString = ( string ) Properties . Settings . Default [ "NorthwindConnectionString" ];
			//int Ordervalue = Convert . ToInt32 ( orderId );
			string CmdString = string . Empty;
			try
			{
				using ( SqlConnection con = new SqlConnection ( ConString ) )
				{
					if ( orderId != -1 )
						CmdString = $"SELECT *  FROM [Products] where ProductID = {orderId}";
					else
						CmdString = $"SELECT *  FROM [Products]";
					SqlCommand cmd = new SqlCommand ( CmdString, con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dt );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Data={ex . Data}, {ex . Message}\n[{CmdString}]" );
			}
			return CreateProductCollection ( dt );
		}
		public NwProductCollection CreateProductCollection ( DataTable dt )
		{
			int count = 0;
			try
			{
				for ( int i = 0 ; i < dt . Rows . Count ; i++ )
				{
					this . Add ( new nwproduct
					{
						ProductId = Convert . ToInt32 ( dt . Rows [ i ] [ 0 ] ),
						ProductName = dt . Rows [ i ] [ 1 ] . ToString ( ),
						SupplierId = Convert . ToInt32 ( dt . Rows [ i ] [ 2 ] ),
						CategoryId = Convert . ToInt32 ( dt . Rows [ i ] [ 3 ] ),
						QuantityPerUnit = dt . Rows [ i ] [ 4 ] . ToString ( ),
						UnitPrice = Convert . ToDecimal ( dt . Rows [ i ] [ 5 ] ),
						UnitsInStock = Convert . ToInt32 ( dt . Rows [ i ] [ 6 ] ),
						UnitsOnOrder = Convert . ToInt32 ( dt . Rows [ i ] [ 7 ] ),
						ReorderLevel = Convert . ToInt32 ( dt . Rows [ i ] [ 8 ] ),
						Discontinued = Convert . ToBoolean ( dt . Rows [ i ] [ 9 ] )
					} );
					count = i;
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"NWPRODUCTS: ERROR in  CreateProductCollection() : loading data into ObservableCollection : [{ex . Message}] : {ex . Data} ...." );
				MessageBox . Show ( $"NWPRODUCTS: ERROR in  CreateProductCollection() : loading data into ObservableCollection : [{ex . Message}] : {ex . Data} ...." );
				return null;
			}
			finally
			{
			}
			return this;
		}
	}
	public class nwproduct : INotifyPropertyChanged
	{
		public nwproduct ( )
		{
		}

		#region declarations
		private int productId;
		private string productName;
		private int supplierId;
		private int categoryId;
		private int unitsInStock;
		private decimal unitPrice;
		private string quantityPerUnit;
		private int unitsOnOrder;
		private int reorderLevel;
		private bool discontinued;
		private  int currentSelection;

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged ( String propertyName )
		{
			if ( ( this . PropertyChanged != null ) )
			{
				this . PropertyChanged ( this, new PropertyChangedEventArgs ( propertyName ) );
			}
		}

		public int CurrentSelection
		{
			get
			{
				return currentSelection;
			}
			set
			{
				currentSelection = value;
				OnPropertyChanged ( "CurrentSelection" );
			}
		}

		public int ProductId
		{
			get
			{
				return productId;
			}
			set
			{
				productId = value;
				OnPropertyChanged ( "ProductId" );
			}
		}

		public string ProductName
		{
			get
			{
				return productName;
			}
			set
			{
				productName = value;
				OnPropertyChanged ( "ProductName" );
			}
		}

		public int SupplierId
		{
			get
			{
				return supplierId;
			}
			set
			{
				supplierId = value;
				OnPropertyChanged ( "SupplierId" );
			}
		}

		public int CategoryId
		{
			get
			{
				return categoryId;
			}
			set
			{
				categoryId = value;
				OnPropertyChanged ( "CategoryId" );
			}
		}


		public string QuantityPerUnit
		{
			get
			{
				return quantityPerUnit;
			}
			set
			{
				quantityPerUnit = value;
				OnPropertyChanged ( "QuantityPerUnit" );
			}
		}

		public decimal UnitPrice
		{
			get
			{
				return unitPrice;
			}
			set
			{
				unitPrice = value;
				OnPropertyChanged ( "UnitPrice" );
			}
		}

		public int UnitsInStock
		{
			get
			{
				return unitsInStock;
			}
			set
			{
				unitsInStock = value;
				OnPropertyChanged ( "UnitsInStock" );
			}
		}

		public int UnitsOnOrder
		{
			get
			{
				return unitsOnOrder;
			}
			set
			{
				unitsOnOrder = value;
				OnPropertyChanged ( "UnitsOnOrder" );
			}
		}


		public int ReorderLevel
		{
			get
			{
				return reorderLevel;
			}
			set
			{
				reorderLevel = value;
				OnPropertyChanged ( "ReOrderLevel" );
			}
		}

		public bool Discontinued
		{
			get
			{
				return discontinued;
			}
			set
			{
				discontinued = value;
				OnPropertyChanged ( "Discontinued" );
			}
		}
		#endregion declarations

	}
}
