using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Globalization;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Data;

namespace WPFPages . Views
{
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
