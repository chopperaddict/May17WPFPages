using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . Data . SqlClient;
using System . Data;
using System . Diagnostics;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . ComponentModel;

namespace WPFPages . Views
{
	public class NwOrderDetails : ObservableCollection<nworderdetail>
	{
		public NwOrderDetails ( )
		{
		}

		//		public ObservableCollection<nworderdetails> NworderDetails;

		public NwOrderDetails ( int arg )
		{
			LoadOrderDetails ( arg );
		}
		public NwOrderDetails LoadOrderDetails ( int arg = -1 )
		{
			DataTable dt = new DataTable ( "SelectedOrders" );
			string ConString = ( string ) Properties . Settings . Default [ "NorthwindConnectionString" ];

			string CmdString = string . Empty;
			try
			{
				using ( SqlConnection con = new SqlConnection ( ConString ) )
				{
					if ( arg != -1 )
						CmdString = $"SELECT *  FROM [Order Details] where OrderId = {arg}";
					else
						CmdString = $"SELECT *  FROM [Order Details]";
					SqlCommand cmd = new SqlCommand ( CmdString, con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dt );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Data={ex . Data}, {ex . Message}\n[{CmdString}]" );
			}
			return CreateDetOrdersCollection ( dt );
		}
		public NwOrderDetails CreateDetOrdersCollection ( DataTable dt )
		{
			int count = 0;
			try
			{
				for ( int i = 0 ; i < dt . Rows . Count ; i++ )
				{
					this . Add ( new nworderdetail
					{
						OrderId = Convert . ToInt32 ( dt . Rows [ i ] [ 0 ] ),
						ProductId = Convert . ToInt32 ( dt . Rows [ i ] [ 1 ] ),
						UnitPrice = Convert . ToDecimal ( dt . Rows [ i ] [ 2 ] ),
						Quantity = Convert . ToInt32 ( dt . Rows [ i ] [ 3 ] ),
						Discount = Convert . ToDouble ( dt . Rows [ i ] [ 4 ] )
					} );
					count = i;
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"NWORDERDETAILS: ERROR in  CreateDetOrdersCollection() : loading data into ObservableCollection : [{ex . Message}] : {ex . Data} ...." );
				MessageBox . Show ( $"NWORDERDETAILS: ERROR in  CreateDetOrdersCollection() : loading data into ObservableCollection : [{ex . Message}] : {ex . Data} ...." );
				return null;
			}
			finally
			{
			}
			return this;
		}

	}
	public class nworderdetail : System . ComponentModel.INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		#region PropertyChanged

		private void OnPropertyChanged ( string propertyName )
		{
			//PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( propertyName ) );
//			this . VerifyPropertyName ( propertyName );

			if ( this . PropertyChanged != null )
			{
				var e = new PropertyChangedEventArgs ( propertyName );
				this . PropertyChanged ( this, e );
			}
		}
		/// <summary>
		/// Warns the developer if this object does not have
		/// a public property with the specified name. This
		/// method does not exist in a Release build.
		/// </summary>
		[Conditional ( "DEBUG" )]
		[DebuggerStepThrough]
		public virtual void VerifyPropertyName ( string propertyName )
		{
			// Verify that the property name matches a real,
			// public, instance property on this object.
			if ( TypeDescriptor . GetProperties ( this ) [ propertyName ] == null )
			{
				string msg = "Invalid property name: " + propertyName;

				if ( this . ThrowOnInvalidPropertyName )
					throw new Exception ( msg );
				else
					Debug . Fail ( msg );
			}
		}

		/// <summary>
		/// Returns whether an exception is thrown, or if a Debug.Fail() is used
		/// when an invalid property name is passed to the VerifyPropertyName method.
		/// The default value is false, but subclasses used by unit tests might
		/// override this property's getter to return true.
		/// </summary>
		protected virtual bool ThrowOnInvalidPropertyName
		{
			get; private set;
		}

		#endregion PropertyChanged
		private int orderId;
		private int productId;
		private decimal unitPrice;
		private int quantity;
		private double discount;
		private  int currentSelection;
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

		public int OrderId
		{
			get
			{
				return orderId;
			}
			set
			{
				orderId = value;
				OnPropertyChanged ( "OrderId" );
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
		public int Quantity
		{
			get
			{
				return quantity;
			}
			set
			{
				quantity = value;
				OnPropertyChanged ( "Quantity" );
			}
		}
		public double Discount
		{
			get
			{
				return discount;
			}
			set
			{
				discount = value;
				OnPropertyChanged ( "Discount" );
			}
		}

	}
}
