using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Globalization;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Data;

namespace WPFPages . Converts
{
	public class ReduceEllipseHeightConverter : IValueConverter
	{	  
		public object Convert ( object value , Type targetType , object parameter , CultureInfo culture )
		{
			double d = (double)value - 25;
			return d;
		}

		public  object ConvertBack ( object value , Type targetType , object parameter , CultureInfo culture )
		{
			Debugger . Break ( );
			return value;
		}

	}
}
