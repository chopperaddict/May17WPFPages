using System;
using System . Collections . Generic;
using System . Globalization;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Data;

namespace WPFPages . Views
{
	public class ConvertDateToShortString : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			string date = value . ToString ( );
			char [ ] ch = { ' ' };
			string [ ] dateonly = date . Split ( ch [ 0 ] );
			//			string s = Convert.ToDateTime(value).ToDate();

			return (string)dateonly [ 0 ];
		}
		//public object Convert ( object value , Type targetType , object parameter , CultureInfo culture )
		//{
		//	string date = value.ToString();
		//	char[] ch = { ' ' };
		//	string[] dateonly = date.Split(ch[0]);
		//	//			string s = Convert.ToDateTime(value).ToDate();

		//	return ( object ) dateonly [ 0 ];
		//}

		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			//if (value.ToString() != "")
			//	return (DateTime)value;
			//else
			return null as object;
		}
		//public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		//{
		//	throw new NotImplementedException ( );
		//}

		//public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		//{
		//	throw new NotImplementedException ( );
		//}
	}
}
