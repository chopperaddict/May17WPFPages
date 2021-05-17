using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WPFPages
{
	public class FormatIntValuesConvertor : IValueConverter
	{
		public FormatIntValuesConvertor()
		{

		}
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int v = (int)value;
			string s = v.ToString().PadLeft(4, ' ');
			return s;
			//if (v >999)
			//	return v.ToString();
			//else if (v > 99)
			//	return " " + v.ToString();
			//else if (v > 9)
			//	return "   " + v.ToString();
			//else 
			//	return "     " + v.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
