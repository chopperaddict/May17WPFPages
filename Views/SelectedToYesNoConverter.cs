using System;
using System . Collections . Generic;
using System . Globalization;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Data;

namespace WPFPages . Views
{
//	[ValueConversion ( typeof ( string), typeof ( bool) )]
	public  class SelectedToYesNoConverter : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			string retval = "xxx";
			var s = value . ToString ( );
			//bool b = System.Convert.ToBoolean(value);
			if ( value  is bool )
			{
				if ( value is true )
					retval = "Yes";
				else
					
					retval = "No";
			}
			return retval;
		}

		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			bool retval = false;
			if ( value is bool )
			{
				if ( value is true )
					retval = true;
				else 
					retval = false;
			}
			return retval;
		}
	}
}
