using System;
using System . Collections;
using System . Collections . Generic;
using System . Globalization;
using System . IO;
using System . Linq;
using System . Runtime . Serialization . Formatters . Binary;
using System . Text;
using System . Threading . Tasks;
using System.Windows.Navigation;
using DocumentFormat.OpenXml.Spreadsheet;

namespace WPFPages
{
	public static class Library
	{

		public static string PrettyDate ( DateTime StartDate )
		{
			// Formats a DateTime (Date section) so it has 2 digits ("0x" for days and months < 10
			string tmp = "";
			string daystr = StartDate . Day . ToString ( );
			string monthstr = StartDate . Month . ToString ( );
			string yearstr = StartDate . Year . ToString ( );
			if ( daystr . Length == 1 ) { tmp = "0" + daystr; daystr = tmp; }
			if ( monthstr . Length == 1 ) { tmp = "0" + monthstr; monthstr = tmp; }
			string startstring = yearstr + "/" + monthstr + "/" + daystr;
			return startstring;
		}

		//-------------------------------------------------------------------------------------------------------//
		public static StringBuilder GetDataFromDiskFile ( StringBuilder stringBuilder, string File )
		{
			StringBuilder SB = new StringBuilder ( );
			BinaryFormatter formatter = new BinaryFormatter ( );
			FileStream fs = new FileStream ( File, FileMode . Open );
			string v = formatter . Deserialize ( fs ) . ToString ( );
			SB . Append ( v );
			fs . Close ( );
			return SB;
		}
		//-------------------------------------------------------------------------------------------------------//

		public static double stringToDouble ( string str )
		{
			double dbl;
			// This is how to convert anything to anything else basically.
			_ = new NumberFormatInfo
			{
				NumberDecimalSeparator = ".",
				NumberGroupSeparator = ",",
				NumberGroupSizes = new int [ ] { 2 }
			};
			// Finally we can now do the conversion
			dbl = Convert . ToDouble ( str );
			return dbl;
		}
		//-------------------------------------------------------------------------------------------------------//
		public static decimal stringToDecimal ( string str )
		{
			decimal dec;
			// This is how to convert anything to anything else basically.
			NumberFormatInfo numberFormatInfo1 = new NumberFormatInfo
			{
				NumberDecimalSeparator = ".",
				NumberGroupSeparator = ",",
				NumberGroupSizes = new int [ ] { 2 }
			};
			// Finally we can now do the conversion
			dec = Convert . ToDecimal ( str );
			return dec;
		}
		//-------------------------------------------------------------------------------------------------------//
		public static Int16 stringToInt16 ( string str )
		{
			Int16 int16;
			// This is how to convert anything to anything else basically.
			NumberFormatInfo reformat = new NumberFormatInfo
			{
				NumberDecimalSeparator = ".",
				NumberGroupSeparator = ",",
				NumberGroupSizes = new int [ ] { 2 }
			};
			// Finally we can now do the conversion
			int16 = Convert . ToInt16 ( str, reformat );
			return int16;
		}
		//-------------------------------------------------------------------------------------------------------//
		public static int stringToInt32 ( string str )
		{
			int intval;
			// This is how to convert anything to anything else basically.
			NumberFormatInfo reformat = new NumberFormatInfo
			{
				NumberDecimalSeparator = ".",
				NumberGroupSeparator = ",",
				NumberGroupSizes = new int [ ] { 2 }
			};
			// Finally we can now do the conversion
			intval = Convert . ToInt32 ( str, reformat );
			return intval;
		}
		//-------------------------------------------------------------------------------------------------------//
		public static Int64 stringToInt64 ( string str )
		{
			Int64 int64;
			// This is how to convert anything to anything else basically.
			NumberFormatInfo reformat = new NumberFormatInfo
			{
				NumberDecimalSeparator = ".",
				NumberGroupSeparator = ",",
				NumberGroupSizes = new int [ ] { 2 }
			};
			// Finally we can now do the conversion
			int64 = Convert . ToInt64 ( str, reformat );
			return int64;
		}
		//-------------------------------------------------------------------------------------------------------//
		//-------------------------------------------------------------------------------------------------------//

		public static int Add ( int val1, int val2 )
		{ return ( ( int ) val1 + ( int ) val2 ); }
		//		public static Int32 Add ( Int32 val1, Int32 val2 )
		//		{ return val1 + val2; }
		public static Int64 Add ( Int64 val1, Int64 val2 )
		{ return val1 + val2; }
		public static decimal Add ( decimal val1, decimal val2 )
		{ return val1 + val2; }
		public static double Add ( double val1, double val2 )
		{ return val1 + val2; }
		public static double Add ( float val1, float val2 )
		{ return val1 + val2; }
		public static string Add ( string val1, string val2 )
		{ return val1 + val2; }
		public static int Divide ( int val1, int val2 )
		{ return val1 / val2; }
		public static Int64 Divide ( Int64 val1, Int64 val2 )
		{ return val1 / val2; }
		public static decimal Divide ( decimal val1, decimal val2 )
		{ return val1 / val2; }
		public static double Divide ( double val1, double val2 )
		{ return val1 / val2; }
		public static float Divide ( float val1, float val2 )
		{ return val1 / val2; }
		//		public static Int16 Multiply ( Int16 val1, Int16 val2 )
		//		{ return val1 * val2; }
		public static Int32 Multiply ( Int32 val1, Int32 val2 )
		{ return val1 * val2; }
		public static Int64 Multiply ( Int64 val1, Int64 val2 )
		{ return val1 * val2; }
		public static decimal Multiply ( decimal val1, decimal val2 )
		{ return val1 * val2; }
		public static double Multiply ( double val1, double val2 )
		{ return val1 * val2; }
		public static float Multiply ( float val1, float val2 )
		{ return val1 * val2; }
		public static string Concat ( string val1, string val2 )
		{ return val1 + val2; }

		//-------------------------------------------------------------------------------------------------------//
		public static string StripFilenameFromString ( string val1 )
		{   // worked well in testing !!!
			string input = val1;
			char c = '\\';
			string [ ] strs;
			string fname = "";
			strs = input . Split ( c );
			if ( strs . Length == 1 )
			{
				if ( val1 . Contains ( "." ) ) return val1; // nothing to do
				else return "";
			}
			else
			{
				fname = strs [ strs . Length - 1 ]; // the last entry is the file name part
				if ( fname . Contains ( "." ) ) return fname; // nothing to do}
				else return "";
			}
		}
		//-------------------------------------------------------------------------------------------------------//
		public static string FitCommas ( char [ ] chars )
		{
			//			FindControl fc = new FindControl ( );
			//			Control . ControlCollection ctrls;//  = new Control.ControlCollection();

			/*			if ( Bank. ActiveForm . Controls . Contains ( Output2 ) )
						{

						}
				*/
			//		((TextBox) fc . Ctrl ( fc . TheForm ( "Bank" ), "Output2" )).Text = "Output2 identified";
			string output = "";
			if ( chars . Length == 4 )
			{
				output += chars [ 0 ]; output += chars [ 1 ]; output += chars [ 2 ]; output += ','; output += chars [ 3 ];
			}
			else if ( chars . Length == 5 )
			{
				output += chars [ 0 ]; output += chars [ 1 ]; output += chars [ 2 ];
				output += ','; output += chars [ 3 ]; output += chars [ 4 ];
			}
			else if ( chars . Length == 6 )
			{
				output += chars [ 0 ]; output += chars [ 1 ]; output += chars [ 2 ];
				output += ','; output += chars [ 3 ]; output += chars [ 4 ]; output += chars [ 5 ];
			}
			else if ( chars . Length == 7 )
			{
				output += chars [ 0 ]; output += chars [ 1 ]; output += chars [ 2 ];
				output += ','; output += chars [ 3 ]; output += chars [ 4 ]; output += chars [ 5 ];
				output += ','; output += chars [ 6 ];
			}
			else if ( chars . Length == 8 )
			{
				output += chars [ 0 ]; output += chars [ 1 ]; output += chars [ 2 ];
				output += ','; output += chars [ 3 ]; output += chars [ 4 ]; output += chars [ 5 ];
				output += ','; output += chars [ 6 ]; output += chars [ 7 ];
			}
			else if ( chars . Length == 9 )
			{
				output += chars [ 0 ]; output += chars [ 1 ]; output += chars [ 2 ];
				output += ','; output += chars [ 3 ]; output += chars [ 4 ]; output += chars [ 5 ];
				output += ','; output += chars [ 6 ]; output += chars [ 7 ]; output += chars [ 8 ];
			}
			else if ( chars . Length == 10 )
			{
				output += chars [ 0 ]; output += chars [ 1 ]; output += chars [ 2 ];
				output += ','; output += chars [ 3 ]; output += chars [ 4 ]; output += chars [ 5 ];
				output += ','; output += chars [ 6 ]; output += chars [ 7 ]; output += chars [ 8 ];
				output += ','; output += chars [ 9 ];
			}
			else if ( chars . Length == 11 )
			{
				output += chars [ 0 ]; output += chars [ 1 ]; output += chars [ 2 ];
				output += ','; output += chars [ 3 ]; output += chars [ 4 ]; output += chars [ 5 ];
				output += ','; output += chars [ 6 ]; output += chars [ 7 ]; output += chars [ 8 ];
				output += ','; output += chars [ 9 ]; output += chars [ 10 ];
			}
			else if ( chars . Length == 12 )
			{
				output += chars [ 0 ]; output += chars [ 1 ]; output += chars [ 2 ];
				output += ','; output += chars [ 3 ]; output += chars [ 4 ]; output += chars [ 5 ];
				output += ','; output += chars [ 6 ]; output += chars [ 7 ]; output += chars [ 8 ];
				output += ','; output += chars [ 9 ]; output += chars [ 10 ]; output += chars [ 11 ];
			}
			else
			{
				// just return it as is
				output = BuildStringFromChar ( chars );
			}
			return ReverseString ( output );
		}
		//-------------------------------------------------------------------------------------------------------//
		public static string BuildStringFromChar ( char [ ] input )
		{
			string output = "";
			for ( int i = 0 ; i < input . Length ; i++ )
			{
				output += input [ i ];
			}
			return output;
		}
		//-------------------------------------------------------------------------------------------------------//
		public static string AddCommas ( char [ ] chars )
		{   // we only received the Integer part to process, not the decimal part
			string output = "";
			char [ ] ch = { '.' };
			char [ ] data = null;
			// string is reversed, process it for commas
			if ( chars . Length > 3 )
			{
				string temp = BuildStringFromChar ( chars );
				string rev = ReverseString ( temp );
				//                string temp = rev . ToString ( );
				//                string[] d = temp . Split ( ch );
				data = rev . ToCharArray ( );
				// pass the reversed integer part as a char[] 
				output = FitCommas ( data );
				//				output += ".00";
			}
			else if ( chars . Length == 2 )
			{   // we HAVE  decimal point in it
				return chars . ToString ( );
			}
			else
			{
				return "";
			}
			return output;
		}// Works OK

		//-------------------------------------------------------------------------------------------------------//
		public static string FormatNumberWithCommas ( string input )
		{
			char [ ] chars;
			string output = "";
			chars = input . ToCharArray ( );
			output = AddCommas ( chars );
			return output;
		}
		//-------------------------------------------------------------------------------------------------------//
		public static string ReverseString ( string input )
		{
			string output = null;
			string s = input . Trim ( );
			input = s;
			char [ ] chars = input . ToCharArray ( );
			int Inlen = chars . Length;
			char [ ] outch = new char [ Inlen ];
			for ( int i = chars . Length - 1 ; i >= 0 ; i-- )
				output += chars [ i ];
			return output;
		}
		//-------------------------------------------------------------------------------------------------------//
		public static string GetCurrencyString ( string amount )
		{
			if ( amount . Length == 0 ) return "";
			System . Globalization . CultureInfo CI = new System . Globalization . CultureInfo ( "en-GB" );
			string str = ToFormattedCurrencyString ( Convert . ToDecimal ( amount ), "£", CI );
			return str;
		}
		//-------------------------------------------------------------------------------------------------------//
		public static string GetFieldCurrencyString ( string amount )
		{
			if ( amount . Length == 0 ) return "";
			System . Globalization . CultureInfo CI = new System . Globalization . CultureInfo ( "en-GB" );
			string str = ToFormattedCurrencyString ( Convert . ToDecimal ( amount ), "", CI );
			return str;
		}
		//-------------------------------------------------------------------------------------------------------//
		public static string ProcessAnyInt ( string input )
		{
			string [ ] parts = null;
			string output = "";
			parts = FormatCheck1 ( input, 1 );
			int partscount = parts . Length;

			if ( partscount == 1 )
			{
				// only got the int part, so
				// just return the string value
				if ( parts [ 0 ] . Length <= 3 )
				{
					output = parts [ 0 ] . ToString ( ) + ".00";
				}
				else
				{   // string of more than 3 digits so
					output = input;
					//					output = AddCommas ( chars );
					return output;
				}
			}
			else
			{   // call the double handler
				double d = Convert . ToDouble ( input );
				output = FormatToDecimal ( d );
			}
			return output;
		}
		//-------------------------------------------------------------------------------------------------------//
		public static string FormatToDecimal ( string inval )
		{
			// 1st we do a standard check to see what we got
			string output = "";
			string validstr = "0123456789.";
			decimal d = 0.0M;
			// check for non numeric  characters
			char [ ] array = inval . ToCharArray ( );
			char [ ] checkarray = { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', };
			int count = 0;

			foreach ( char c in array )
			{   // ensure we have only vlaid numbers, comma or decimal point
				if ( !validstr . Contains ( c . ToString ( ) ) )
					return "";
				else if ( c != ',' )
					checkarray [ count++ ] = c;
			}

			//convert it back  to a string again, trimming empty chars off the end
			output = BuildStringFromChar ( checkarray );
			output . TrimEnd ( ' ' );
			// convert to a valid numeric value to check it is a vlaid numeric value
			if ( output . Contains ( "." ) )
			{
				d = Convert . ToDecimal ( output );
				return FormatToDecimal ( d );
			}
			else
			{
				Convert . ToInt32 ( output );
				return FormatToDecimal ( output );
			}

		}
		//-------------------------------------------------------------------------------------------------------//
		public static string FormatToDecimal ( Int16 inval )
		{
			// 1st we do a standard check to see what we got
			string output = "";
			string input = inval . ToString ( );
			output = ProcessAnyInt ( input );
			string [ ] parts = output . Split ( '.' );
			char [ ] chars = parts [ 0 ] . ToCharArray ( );
			output = AddCommas ( chars );
			if ( output . Length == 0 )
				output = input; // no change
			if ( !output . Contains ( ".00" ) )
				output += ".00";
			return output;
		}
		//-------------------------------------------------------------------------------------------------------//
		public static string FormatToDecimal ( Int32 inval )
		{
			// 1st we do a standard check to see what we got
			string output = "";
			string input = inval . ToString ( );
			output = ProcessAnyInt ( input );
			char [ ] chars = output . ToCharArray ( );
			output = AddCommas ( chars );
			if ( !output . Contains ( ".00" ) )
				output += ".00";
			return output;
		}
		//-------------------------------------------------------------------------------------------------------//
		public static string FormatToDecimal ( Int64 inval )
		{
			// 1st we do a standard check to see what we got
			string output = "";
			string input = inval . ToString ( );
			output = ProcessAnyInt ( input );
			char [ ] chars = output . ToCharArray ( );
			output = AddCommas ( chars );
			if ( !output . Contains ( ".00" ) )
				output += ".00";
			return output;
		}
		//-------------------------------------------------------------------------------------------------------//
		public static string FormatToDecimal ( decimal inval )
		{
			char [ ] ch = { '.' };
			string input = inval . ToString ( );
			string output = "";
			// handle the 'integer part 1st - same as othe rInts
			string [ ] parts = input . Split ( ch );
			char [ ] chars = parts [ 0 ] . ToCharArray ( );
			input = BuildStringFromChar ( chars );
			output = ProcessAnyInt ( input );
			chars = output . ToCharArray ( );
			output = AddCommas ( chars );

			// handle the decimal part - if we have oine ?
			if ( parts . Length > 1 )
			{
				if ( parts [ 1 ] . Length == 1 )
				{   // got a decimal point, just make it 2 digits                
					output += parts [ 0 ] + "." + parts [ 1 ] + "0";
				}
				else if ( parts [ 1 ] . Length == 2 )
					output = parts [ 0 ] + "." + parts [ 1 ];
			}
			return output;
		}
		//-------------------------------------------------------------------------------------------------------//
		public static string FormatToDecimal ( double inval )
		{
			char [ ] ch = { '.' };
			string input = inval . ToString ( );
			string output = "";
			// handle the 'integer part 1st - same as othe rInts
			string [ ] parts = input . Split ( ch );
			char [ ] chars = parts [ 0 ] . ToCharArray ( );
			input = BuildStringFromChar ( chars );
			output = ProcessAnyInt ( input );
			chars = output . ToCharArray ( );
			// this return the Int part with commas as needed, but no . or deimal part
			output = AddCommas ( chars );

			// handle the decimal part - if we have oine ?
			if ( parts . Length > 1 )
			{
				if ( parts [ 1 ] . Length == 1 )
				{   // got a decimal point, just make it 2 digits                
					output += parts [ 1 ] + "." + parts [ 1 ] + "0";
				}
				else if ( parts [ 1 ] . Length == 2 )
					output = parts [ 0 ] + "." + parts [ 1 ];
			}
			return output;
		}
		//-------------------------------------------------------------------------------------------------------//
		public static string [ ] FormatCheck1 ( string instr, int type )
		{   // do initial check of what we got
		    // type is 1-Int16, 2 - Int32, 3 - Int64, 4 - double, 5 - decimal, 6 - 
			string input = "", output = "", intstr = "", decstr = "";
			string [ ] parts;

			char [ ] ch = { '.' };
			input = instr;
			// convert to string array of 2 parts (integer and decimal parts)
			parts = input . Split ( ch );
			if ( parts . Length == 1 )
			{
				// just an integer part
				intstr = parts [ 0 ];
				output = parts [ 0 ] . ToString ( );
			}
			else
			{
				// integer and decimal parts
				intstr = parts [ 0 ];
				decstr = parts [ 1 ];
			}
			return parts;
		}
		//-------------------------------------------------------------------------------------------------------//
		// CALL TO USE THIS IN CODE IS 
		//string CurrencyAmount = Utils.GetCurrencyString(string amount);
		//-------------------------------------------------------------------------------------------------------//
		public static string ToFormattedCurrencyString ( this decimal currencyAmount, string isoCurrencyCode, CultureInfo userCulture )
		{
			string userCurrencyCode = new RegionInfo ( userCulture . Name ) . ISOCurrencySymbol;
			userCulture = new CultureInfo ( "en-GB", false );
			if ( userCurrencyCode == isoCurrencyCode )
			{
				return currencyAmount . ToString ( "C", userCulture );
			}

			return string . Format ( "{0} {1}", isoCurrencyCode, currencyAmount . ToString ( "N2", userCulture ) );
		}


		/*
		 * This first one is an int incrementer as an Enumerable 
		 * but could be any data type
		 *  To Call this :-
		 *  var myEnumerable = new MyEnumerable();
		 *  foreach(var x in myEnumerable)
		 *  CW (x);
		 * 
		 See below for a string enumerator and another for BankAccounts and one for Customers
		 */
		//*******************************************************************************************//
		public class EnumerateInt : IEnumerable<int>
		{
			private int [ ] myarray;
			public EnumerateInt ( int [ ] input )
			{ myarray = input; }
			public IEnumerator GetEnumerator ( )
			{ return new MyEnumerator ( myarray ); }
			IEnumerator<int> IEnumerable<int>.GetEnumerator ( )
			{ return new MyEnumerator ( myarray ); }
			//----------------------------------------------------------------------------------------------------------------------------------------------------------//
			public class MyEnumerator : IEnumerator<int>
			{
				private int [ ] mValues;
				private int mindex = -1;
				// We HAVE to set Current to the data we receive in to iterate thru
				public int Current => mValues [ mindex ];
				// This is also mandatory, & clearly points to our data, but not sure why we do this ??
				object IEnumerator.Current => Current;
				public MyEnumerator ( int [ ] values )
				{ mValues = values; }
				public void Dispose ( )
				{ }
				public bool MoveNext ( )
				{
					mindex++;
					return mindex < mValues . Length;
				}
				public void Reset ( )
				{ mindex = 0; }
			}
		}

		//*******************************************************************************************//
		//*******************************************************************************************//
		// Now a string enumerator ??
		public class EnumerateString : IEnumerable<string>
		{
			private string [ ] myarray;
			public EnumerateString ( string [ ] input )
			{ myarray = input; }
			public IEnumerator GetEnumerator ( )
			{ return new MystringEnumerator ( myarray ); }
			IEnumerator<string> IEnumerable<string>.GetEnumerator ( )
			{ return new MystringEnumerator ( myarray ); }
		}
		//----------------------------------------------------------------------------------------------------------------------------------------------------------//
		public class MystringEnumerator : IEnumerator<string>
		{
			private string [ ] mValues;
			private int mindex = -1;
			// We HAVE to set Current to the data we receive in to iterate thru
			public string Current => mValues [ mindex ];
			// This is also mandatory, & clearly points to our data, but not sure why we do this ??
			object IEnumerator.Current => Current;
			public MystringEnumerator ( string [ ] values )
			{ mValues = values; }
			public void Dispose ( )
			{ }
			public bool MoveNext ( )
			{
				mindex++;
				return mindex < mValues . Length;
			}
			public void Reset ( )
			{ mindex = 0; }
		}
		//*******************************************************************************************//
		//*******************************************************************************************//
		// Now a double  enumerator ??
		public class EnumerateDouble : IEnumerable<double>
		{
			private double [ ] myarray;
			public EnumerateDouble ( double [ ] input )
			{ myarray = input; }
			public IEnumerator GetEnumerator ( )
			{ return new MyDoubleEnumerator ( myarray ); }
			IEnumerator<double> IEnumerable<double>.GetEnumerator ( )
			{ return new MyDoubleEnumerator ( myarray ); }
		}
		//----------------------------------------------------------------------------------------------------------------------------------------------------------//
		public class MyDoubleEnumerator : IEnumerator<double>
		{
			private double [ ] mValues;
			private int mindex = -1;
			// We HAVE to set Current to the data we receive in to iterate thru
			public double Current => mValues [ mindex ];
			// This is also mandatory, & clearly points to our data, but not sure why we do this ??
			object IEnumerator.Current => Current;
			public MyDoubleEnumerator ( double [ ] values )
			{ mValues = values; }
			public void Dispose ( )
			{ }
			public bool MoveNext ( )
			{
				mindex++;
				return mindex < mValues . Length;
			}
			public void Reset ( )
			{ mindex = 0; }
		}

		//*******************************************************************************************//
		//*******************************************************************************************//
		// Now a Float's enumerator ??
		public class Enumeratefloat : IEnumerable<float>
		{
			private float [ ] myarray;
			public Enumeratefloat ( float [ ] input )
			{ myarray = input; }
			public IEnumerator GetEnumerator ( )
			{ return new floatEnumerator ( myarray ); }
			IEnumerator<float> IEnumerable<float>.GetEnumerator ( )
			{ return new floatEnumerator ( myarray ); }
		}
		//----------------------------------------------------------------------------------------------------------------------------------------------------------//
		public class floatEnumerator : IEnumerator<float>
		{
			private float [ ] mValues;
			private int mindex = -1;
			// We HAVE to set Current to the data we receive in to iterate thru
			public float Current => mValues [ mindex ];
			// This is also mandatory, & clearly points to our data, but not sure why we do this ??
			object IEnumerator.Current => Current;
			public floatEnumerator ( float [ ] values )
			{ mValues = values; }
			public void Dispose ( )
			{ }
			public bool MoveNext ( )
			{
				mindex++;
				return mindex < mValues . Length;
			}
			public void Reset ( )
			{ mindex = 0; }
		}

		//*******************************************************************************************//
		//*******************************************************************************************//
		// Now a Object's enumerator ??
		public class EnumerateObject : IEnumerable<object>
		{
			private object [ ] myarray;
			public EnumerateObject ( object [ ] input )
			{ myarray = input; }
			public IEnumerator GetEnumerator ( )
			{ return new ObjectEnumerator ( myarray ); }
			IEnumerator<object> IEnumerable<object>.GetEnumerator ( )
			{ return new ObjectEnumerator ( myarray ); }
		}
		//----------------------------------------------------------------------------------------------------------------------------------------------------------//
		public class ObjectEnumerator : IEnumerator<object>
		{
			private object [ ] mValues;
			private int mindex = -1;
			// We HAVE to set Current to the data we receive in to iterate thru
			public object Current => mValues [ mindex ];
			// This is also mandatory, & clearly points to our data, but not sure why we do this ??
			object IEnumerator.Current => Current;
			public ObjectEnumerator ( object [ ] values )
			{ mValues = values; }
			public void Dispose ( )
			{ }
			public bool MoveNext ( )
			{
				mindex++;
				return mindex < mValues . Length;
			}
			public void Reset ( )
			{ mindex = 0; }
		}
	}
}
