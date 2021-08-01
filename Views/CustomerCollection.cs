using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . Linq;
using System . Text;
using System . Threading . Tasks;

namespace WPFPages . Views
{
	public class CustomerCollection : ObservableCollection<nwcustomer>
	{
		private nwcustomer customerptr = new nwcustomer ( );
		public CustomerCollection ( )
		{
		}
		public CustomerCollection  GetCustCollection( )
		{
			return customerptr . Loadcustomers ( ) as CustomerCollection;
		}
	}
}
