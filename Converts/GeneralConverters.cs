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
	public class GeneralConverters : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException ( );
		}

		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException ( );
		}
	}
	class DummyDebugConverter : IValueConverter
	{
		object IValueConverter.Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			Debugger . Break ( );
			return value;
		}

		object IValueConverter.ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			Debugger . Break ( );
			return value;
		}
	}
}
