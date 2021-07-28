using System;
using System . Collections . Generic;
using System . Data . SqlClient;
using System . Data;
using System . Diagnostics;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Controls;
using System . Collections . ObjectModel;
using WPFPages . ViewModels;
using WPFPages . Libraries;
using System . Windows;
using System . ComponentModel;

namespace WPFPages . Views
{
	
	public class nwcustomer : INotifyPropertyChanged
	{
//		public nwcustomer NwCustomer = new nwcustomer ( );
//		public ObservableCollection<nwcustomer> nwCustCollection;


		//public event PropertyChangedEventHandler PropertyChanged;

		//protected virtual void OnPropertyChanged ( String propertyName )
		//{
		//	if ( ( this . PropertyChanged != null ) )
		//	{
		//		this . PropertyChanged ( this, new PropertyChangedEventArgs ( propertyName ) );
		//	}
		//}
		#region PropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		//protected void OnPropertyChanged ( string PropertyName )
		//{
		//	if ( null != PropertyChanged )
		//	{
		//		PropertyChanged ( this,
		//			new PropertyChangedEventArgs ( PropertyName ) );
		//	}
		//}
		private void OnPropertyChanged ( string propertyName )
		{
			PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( propertyName ) );
		}
		#endregion PropertyChanged

		#region declarations

		private string customerId;
		private string companyName;
		private string contactName;
		private string contactTitle;
		private string address;
		private string fax;
		private string phone;
		private string country;
		private string postalCode;
		private string region;
		private string city;
		private int currentSelection;
		private int customerCurrent;
		private int orderCurrent;

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
				return orderCurrent;
			}
			set
			{
				orderCurrent = value;
				OnPropertyChanged ( nameof ( OrderCurrent ) );
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
		public string CustomerId
		{
			get
			{
				return customerId;
			}
			set
			{
				customerId = value;
				OnPropertyChanged ( "CustomerId" );
			}
		}

		public string CompanyName
		{
			get
			{
				return companyName;
			}
			set
			{
				companyName = value;
				OnPropertyChanged ( "CompanyName" );
			}
		}

		public string ContactName
		{
			get
			{
				return contactName;
			}
			set
			{
				contactName = value;
				OnPropertyChanged ( "ContactName" );
			}
		}


		public string ContactTitle
		{
			get
			{
				return contactTitle;
			}
			set
			{
				contactTitle = value;
				OnPropertyChanged ( "ContactTitle" );
			}
		}

		public string Address
		{
			get
			{
				return address;
			}
			set
			{
				address = value;
				OnPropertyChanged ( "Address" );
			}
		}

		public string City
		{
			get
			{
				return city;
			}
			set
			{
				city = value;
				OnPropertyChanged ( "City" );
			}
		}

		public string Region
		{
			get
			{
				return region;
			}
			set
			{
				region = value;
				OnPropertyChanged ( "Region" );
			}
		}

		public string PostalCode
		{
			get
			{
				return postalCode;
			}
			set
			{
				OnPropertyChanged ( "PostalCode" );
				postalCode = value;
			}
		}

		public string Country
		{
			get
			{
				return country;
			}
			set
			{
				country = value;
				OnPropertyChanged ( "Country" );
			}
		}

		public string Phone
		{
			get
			{
				return phone;
			}
			set
			{
				phone = value;
				OnPropertyChanged ( "Phone" );
			}
		}

		public string Fax
		{
			get
			{
				return fax;
			}
			set
			{
				fax = value;
				OnPropertyChanged ( "Fax" );
			}
		}

		#endregion declarations

		public ObservableCollection<nwcustomer> nwc = new ObservableCollection<nwcustomer> ( );

		public nwcustomer ( )
		{
		}
		public nwcustomer (string arg )
		{
			nwc = new ObservableCollection<nwcustomer> ( );
			LoadSpecificCustomers ( arg );
		}
		public ObservableCollection<nwcustomer> Loadcustomers ( )
		{
			DataTable dt = new DataTable ( "Customers" );
			string ConString = ( string ) Properties . Settings . Default [ "NorthwindConnectionString" ];

			string CmdString = string . Empty;
			try
			{
				using ( SqlConnection con = new SqlConnection ( ConString ) )
				{
					CmdString = $"SELECT *  FROM Customers ";
					SqlCommand cmd = new SqlCommand ( CmdString, con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dt );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Data={ex . Data}, {ex . Message}\n[{CmdString}]" );
			}
			CreateCustCollection ( dt );
			return nwc;
		}
		public ObservableCollection<nwcustomer> LoadSpecificCustomers (string arg )
		{
			DataTable dt = new DataTable ( "Customers" );
			string ConString = ( string ) Properties . Settings . Default [ "NorthwindConnectionString" ];

			string CmdString = string . Empty;
			try
			{
				using ( SqlConnection con = new SqlConnection ( ConString ) )
				{
					if ( arg == "" )
						CmdString = $"SELECT *  FROM [Customers] ";
					else
						CmdString = $"SELECT * from [Customers] where CustomerId ='{arg}'";
					SqlCommand cmd = new SqlCommand ( CmdString, con );
					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
					sda . Fill ( dt );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Data={ex . Data}, {ex . Message}\n[{CmdString}]" );
			}
			nwc = new ObservableCollection<nwcustomer> ( );
			CreateCustCollection ( dt );
			return nwc;
		}
		public bool CreateCustCollection ( DataTable dt )
		{
			int count = 0;
			try
			{
				for ( int i = 0 ; i < dt . Rows . Count ; i++ )
				{
					nwc . Add ( new nwcustomer
					{
						CustomerId = dt . Rows [ i ] [ 0 ] . ToString ( ),
						CompanyName = dt . Rows [ i ] [ 1 ] . ToString ( ),
						ContactName = dt . Rows [ i ] [ 2 ] . ToString ( ),
						ContactTitle = dt . Rows [ i ] [ 3 ] . ToString ( ),
						Address = dt . Rows [ i ] [ 4 ] . ToString ( ),
						Fax = dt . Rows [ i ] [ 10 ] . ToString ( ),
						Region = dt . Rows [ i ] [ 6 ] . ToString ( ),
						Country = dt . Rows [ i ] [ 8 ] . ToString ( ),
						PostalCode = dt . Rows [ i ] [ 7 ] . ToString ( ),
						Phone = dt . Rows [ i ] [ 9 ] . ToString ( ),
						City = dt . Rows [ i ] [ 5 ] . ToString ( ),
					} );
					count = i;
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"DETAILS : ERROR in  LoadDetCollection() : loading Details into ObservableCollection \"DetCollection\" : [{ex . Message}] : {ex . Data} ...." );
				MessageBox . Show ( $"DETAILS : ERROR in  LoadDetCollection() : loading Details into ObservableCollection \"DetCollection\" : [{ex . Message}] : {ex . Data} ...." );
				return false;
			}
			finally
			{
			}
			return true;
		}
	}
}

