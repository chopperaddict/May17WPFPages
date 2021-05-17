using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WPFPages
{
	public class FormatServicePeriodConvertor : IValueConverter
	{
		public FormatServicePeriodConvertor()
		{

		}
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			double v = (double)value;
			if(v < 12) 
				return v.ToString("#.##") + " Year";
			double dv = (double)v / 12.0;
			//Return the value with 2 decimal places
			string s = dv.ToString("#.##") + " Years";
			return s;	
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

}

