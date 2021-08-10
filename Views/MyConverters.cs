using System;
using System . Collections . Generic;
using System . ComponentModel;
using System . Diagnostics;
using System . Globalization;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Data;
using System . Windows . Media;

using Newtonsoft . Json . Linq;

using WPFPages . ViewModels;

namespace WPFPages . MyConvertors
{

	/*
	Contents : 
	Actype2Name					Converts Numeric value 1-4  to A/C type name
	DateTimeToShortStringConverter		Converts Any DateTime format to Short Date format
	NumericString2ShortDateConverter 	Converts a string '01022020' to string "01/02/2020'
	DateTime2ShortDateConvertor 		Converts DateTime to DateTime.ShortDate()
	Date2UTCConverter 				Converts DateTime to International 'YYYY/MM/DD' format
	Int2BrushConverter				Converts an int 1-4 to the specified brush as per the parameter received
	Resource2BrushConverter			Converts Resource Name to relevant Brush

	UniversalValueConverter			Converts 
	*/
	public class Actype2Name : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			// Receives an int value 1-4 
			// Returns the AC type as a word "Checking A/c"
			if ( value  == DependencyProperty . UnsetValue )
				return DependencyProperty . UnsetValue;
			try
			{
				int val = System . Convert . ToInt32 ( value );
				if ( val == 0 )
					return "Unknown";
				else if ( val == 1 )
					return "Checking A/C";
				else if ( val == 2 )
					return "Deposit A/C";
				else if ( val == 3 )
					return "Savings A/C";
				else if ( val == 4 )
					return "Business A/C";
				else
					return ( object ) null;
			}
			catch ( Exception ex )
			{
				return ( object ) null;

			}
		}
		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return null as object;
		}
	}

	// ALL CONVERTERS ARE ACTUALLY WORKING 10/7/21
	public class DateTimeToShortStringConverter : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value == DependencyProperty . UnsetValue ) 
				return DependencyProperty . UnsetValue;
			// Receives a FULL date with time = "01/01/1933 12:13:54"
			// Returns just the date part = "01/01/1933"
			string date = value . ToString ( );
			char [ ] ch = { ' ' };
			string [ ] dateonly = date . Split ( ch [ 0 ] );
			return ( string ) dateonly [ 0 ];
		}
		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return null as object;
		}
	}
	public class NumericString2ShortDateConverter : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value == DependencyProperty . UnsetValue  )
				return DependencyProperty . UnsetValue;

			// Assumes receiving a date as "10041932" or similar
			string date = value . ToString ( );
			string Output = "";
			Output = date [ 0 ] + date [ 1 ] + "/";
			Output += date [ 2 ] + date [ 3 ] + "/";
			Output += date [ 4 ] + date [ 5 ];
			Output += date [ 6 ] + date [ 7 ];
			return Output;
		}
		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return null as object;
		}
	}
	public class DateTime2ShortDateConvertor : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value == DependencyProperty . UnsetValue ) 
				return DependencyProperty . UnsetValue;
			// Receives full date/Time string and returns just the date part of it as a string
			return value . ToString ( ) . Substring ( 0, 10 );
		}

		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return ( object ) null;
		}

	}

	public class Date2UTCConverter : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value == DependencyProperty . UnsetValue ) 
				return DependencyProperty . UnsetValue;
			// Assumes receiving a date as "10/04/1932" or similar
			// Returns "1932/04/10"
			string date = value . ToString ( );
			string Output = "";
			Output += date [ 6 ];
			Output += date [ 7 ];
			Output += date [ 8 ];
			Output += date [ 9 ];
			Output += "/";
			Output += date [ 3 ];
			Output += date [ 4 ];
			Output += "/";
			Output += date [ 0 ];
			Output += date [ 1 ];
			return Output;
		}
		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return null as object;
		}
	}

	public class Int2BrushConverter : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value == DependencyProperty . UnsetValue ) 
				return DependencyProperty . UnsetValue;
			// Assumes receiving an int value and Returns a Brush
			// Used by AcType to color each datagrid row to match the AcType of the account
			Brush br = null;
			string s = parameter . ToString ( );
			try
			{
				int val = System . Convert . ToInt32 ( s );
				br = ConverterUtils . GetBrushFromInt ( val );
			}
			catch ( Exception ex ) { Debug . WriteLine ( $"{ex . Message}, {ex . Data}" ); }

			return br;
		}
		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return null as object;
		}
	}

	public class Resource2BrushConverter : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value == DependencyProperty . UnsetValue ) 
				return DependencyProperty . UnsetValue;
			// We receive a Resource name and Return a Brush
			return ( Brush ) Utils . GetDictionaryBrush ( parameter . ToString ( ) );
		}
		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return null as object;
		}
	}


	#region UNIVERSAL CONVERTOR !!!!
	// Universal Convertor stub ???
	public class UniversalValueConverter : IValueConverter
	{
		public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value == DependencyProperty . UnsetValue ) 
				return DependencyProperty . UnsetValue;
			// obtain the conveter for the target type
			TypeConverter converter = TypeDescriptor . GetConverter ( targetType );
			try
			{
				// determine if the supplied value is of a suitable type
				if ( converter . CanConvertFrom ( value . GetType ( ) ) )
				{
					// return the converted value
					return converter . ConvertFrom ( value );
				}
				else
				{
					Type t = targetType . GetType ( );
					var val = value . GetType ( );
					if ( val . FullName == "System.DateTime" )
					{
						//Convert from DateTime to  something else as specified in the  argument "parameter"
						if ( parameter == "YYYYMMDD" )
						{

						}
					}
					else if ( val . FullName . Contains ( "BankAccountViewModel" ) 
						|| val . FullName . Contains ( "DetailsViewModel" ) 
						|| val . FullName . Contains ( "CustomersViewModel" ) )
					{
						return ( Brush ) Utils . GetDictionaryBrush ( parameter . ToString ( ) );
					}

					if ( val . FullName == "System.Int32"
						|| val . FullName == "System.Int16"
						|| val . FullName == "System.Int64"
						|| val . FullName == "System.Double" )
					{
						return ConverterUtils . GetBrushFromInt ( System . Convert . ToInt32 ( val ) );
					}

					// try to convert from the string representation
					return converter . ConvertFrom ( value . ToString ( ) );
				}
			}
			catch ( Exception )
			{
				return ( Brush ) null;
			}
			return ( Brush ) null;
		}

		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return null;
		}
	}

	#endregion UNIVERSAL CONVERTOR !!!!

	// Utilities to support converters
	#region UTILITIES
	public class ConverterUtils
	{

		public static Brush GetBrushFromInt ( int value )
		{
			switch ( value )
			{
				case 0:
					return ( Brushes . White );
					break;
				case 1:
					return ( Brushes . Yellow );
					break;
				case 2:
					return ( Brushes . Orange );
					break;
				case 3:
					return ( Brushes . Red );
					break;
				case 4:
					return ( Brushes . Magenta );
					break;
				case 5:
					return ( Brushes . Gray );
					break;
				case 6:
					return ( Brushes . Aqua );
					break;
				case 7:
					return ( Brushes . Azure );
					break;
				case 8:
					return ( Brushes . Brown );
					break;
				case 9:
					return ( Brushes . Crimson );
					break;
				case 10:
					return ( Brushes . Transparent );
					break;
			}
			return ( Brush ) null;
		}
		public static Brush GetBrush ( string parameter )
		{
			if ( parameter == "BLUE" )
				return Brushes . Blue;
			else if ( parameter == "RED" )
				return Brushes . Red;
			else if ( parameter == "GREEN" )
				return Brushes . Green;
			else if ( parameter == "CYAN" )
				return Brushes . Cyan;
			else if ( parameter == "MAGENTA" )
				return Brushes . Magenta;
			else if ( parameter == "YELLOW" )
				return Brushes . Yellow;
			else if ( parameter == "WHITE" )
				return Brushes . White;
			else
			{
				//We appear to have received a Brushes Resource Name, so return that Brushes value
				Brush b = ( Brush ) Utils . GetDictionaryBrush ( parameter . ToString ( ) );
				return b;
			}
		}
	}
	#endregion UTILITIES
}

