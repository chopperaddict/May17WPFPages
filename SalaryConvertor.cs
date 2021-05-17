using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WPFPages
{
	public class SalaryConvertor : IValueConverter
	{

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			double v = (double)value;
			string s;
			if (v < 1000)
				return "£ "+ v;
			else if (v < 10000) {
				s = v.ToString("£ #,000");
				return s;
			}
			else if (v < 100000) {
				s = v.ToString("£ ##,###");
				return s;
			}
			else if (v < 1000000) {
				s = v.ToString("£ ###,###");
				return s;
			}
			else
				return v.ToString("£ #,###,###"); ;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
