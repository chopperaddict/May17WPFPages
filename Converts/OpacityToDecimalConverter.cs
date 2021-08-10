using System;
using System . Collections . Generic;
using System . Globalization;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Data;

namespace WPFPages . Converts
{
	public class OpacityToDecimalConvert : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			double temp = System . Convert . ToDouble ( value );
			return ( double ) value;
		}

		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			double temp = System . Convert . ToDouble ( value );
			return ( double ) temp;
		}
	}
}
