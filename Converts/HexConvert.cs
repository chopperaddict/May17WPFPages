using System;
using System . Collections . Generic;
using System . Globalization;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Data;
using System . Windows . Media;

namespace WPFPages . Converts
{
	[ValueConversion ( typeof ( double ), typeof ( string ) )]
	public class HexConvert : IValueConverter
	{
		public  object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			//double temp = System . Convert . ToDouble( value );
			//if ( temp <= 255 )
			//	return ( string ) System.Convert.ToInt32(value ). ToString (  );
			//else if ( temp <= 65535 )
			//	return ( string ) value . ToString ( );
			//else if ( temp <= 16777215 )
			//	return ( string ) value . ToString ();

			return ( string ) value. ToString ( );
			//return System.Convert.ToInt32(value);
		}

		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			double  temp = System . Convert . ToDouble( value );
			//if ( temp <= 255 )
			//	return ( string ) temp . ToString ( "X2" );
			//else if ( temp <= 65535 )
			//	return ( string ) temp . ToString ( "X4" );
			//else if ( temp <= 16777215 )
			//	return ( string ) temp . ToString ( "X6" );

			return ( double ) temp;
		}
	}	

}
