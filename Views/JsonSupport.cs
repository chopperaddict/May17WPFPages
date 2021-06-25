using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . IO;
using System . Linq;
using System . Text;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Input;
using Newtonsoft . Json;
using Newtonsoft . Json . Converters;
using WPFPages . ViewModels;

namespace WPFPages . Views
{
	class JsonSupport
	{
		// Class to handle JSN formatting of data
		// 22/6/21 OK
		#region JSON SUPPORT
		/// <summary>
		/// Save in Json format any form of Dbdata (revd as object) to file name as passed in to the Fn
		/// </summary>
		/// <param name="path"></param>
		/// <param name="DbData"></param>
		/// <returns></returns>
		public static string CreateDbToJsonString(string path, object DbData)
		{
			string output = "";

			if ( DbData == null || path == "" ) return "";

			JsonSerializer serializer = new JsonSerializer ( );
			serializer . Converters . Add ( new JavaScriptDateTimeConverter ( ) );
			serializer . NullValueHandling = NullValueHandling . Ignore;

			using ( StreamWriter sw = new StreamWriter ( path ) )
			using ( JsonWriter writer = new JsonTextWriter ( sw ) )
			{
				serializer . Serialize ( writer, DbData );
			}

			return output;

		}

		public static void JsonSerialize ( object obj, string filePath )
		{
			var serializer = new JsonSerializer ( );

			using ( var sw = new StreamWriter ( filePath ) )
			using ( JsonWriter writer = new JsonTextWriter ( sw ) )
			{
				serializer . Serialize ( writer, obj );
			}
		}

		public static object JsonDeserialize ( string path )
		{
			var serializer = new JsonSerializer ( );

			using ( var sw = new StreamReader ( path ) )
			using ( var reader = new JsonTextReader ( sw ) )
			{
				return serializer . Deserialize ( reader );
			}
		}

		public static string CreateFormattedJsonOutput ( string jsonInput, string Title , Progressbar pbar = null)
		{
			int rows = 0;
			int fiddle = 0, maxrows = 0;
			double current = 0; maxrows = 0; 
			string Output = "";
			// using a stringbuilder improves speed by around 1000 % - honestly !!!
			// it is now almost instant
			StringBuilder sb = new StringBuilder ( );
			string temp = "";
			string [ ] tmp1;
			string [ ] tmp2;
			string [ ] tmp3;
			string [ ] tmp4;
			//split by comas first
			tmp1 = jsonInput . Split ( '{' );
			if(pbar != null )
			{
				pbar .Progbar. Value = rows;
			}
			if ( tmp1 . Length == 3 )
			{
				//Single record output only !! OK - 23/6/21
				temp = tmp1 [ 2 ];
				tmp3 = temp . Split ( ',' );
				Output += "{";
				Output += "\n\"{Title}\":{\n";
				for ( int i = 1 ; i < tmp3 . Length ; i++ )
				{
					if ( tmp3 [ i ] . Length == 0 ) continue;
					if ( i == tmp3 . Length - 1 )
					{
						Output += tmp3 [ i ] . Substring ( 0, tmp3 [ i ] . Length - 2 );
						Output += "\n}\r\n";
					}
					else
						Output += tmp3 [ i ] + "\n\t";

				}
				return Output;
			}
			//Multi record file output
			temp = tmp1 [ 2 ];
//			Output = "\t";
			tmp2 = temp . Split ( ',' );
			maxrows = tmp1 . Length;
			// get 1% value
			fiddle = maxrows / 100; // eg 4900 recs =490		
			for ( int outer = 0 ; outer < maxrows - 1 ; outer++ )
			{
				temp = tmp1 [ outer ];
				tmp3 = temp . Split ( ',' );
				if ( tmp3 . Length <= 1 ) continue;
				if(outer == 1) Output += "{" + $"\n\t\"{Title}\": [\n";
				Output += "\t{\n";
//				Output += "\n";
				Output += $"\t\t{tmp3 [ 0 ]},\n";
//				if ( outer >= 1 ) Output += "\t";
				for ( int i = 1 ; i < tmp3 . Length ; i++ )
				{
					if ( tmp3 [ i ] . Length == 0 ) continue;
					if ( i == tmp3 . Length - 2 )
					{
						Output += "\t\t" + tmp3 [ i ] . Substring ( 0, tmp3 [ i ] . Length - 1 );
						Output += "\t\n";
					}
					else
						Output += "\t\t" + tmp3 [ i ] + ",\n";
				}
				if (rows + 3 < maxrows)
					Output += "\t},\n";
				else
					Output += "\t}\n";
				sb. Append ( Output );
				Output = "";
				rows++;
				current = rows;
				// so rows / 490 = x / 100ths of total 
				if ( pbar != null && rows % fiddle == 0 )
				{
					// every 250 lines
					// We are ONE percent further on - update progressbar
					double val = (int)(rows / fiddle) ;
					pbar . Progbar.Value = (int)val;
					pbar . Percent . Text = $"{val} %";
					pbar . Refresh ( );
					pbar . UpdateLayout (  );
				}
			}
			if(Output.Length > 0)
				Output = Output . Substring ( 0, Output . Length - 3 ) + "\t]\n}\n";
			else
				Output =  "\t]\n}\r\n";
			sb . Append ( Output );
			return sb . ToString ( ); ;
		}

		public static StringBuilder CreateJsonFileFromJsonObject ( object JsonObject, out string output )
		{
			BankAccountViewModel bvm = new BankAccountViewModel ( );
			StringBuilder sb = new StringBuilder ( );
			//			JObject obj = JObject . FromObject ( JsonObject );
			string s = JsonConvert . SerializeObject ( new { JsonObject } );
			sb . Append ( s );
			output = s;
			return sb;
		}

		public static void CreateShowJsonText ( string CurrentDb, object collection, string Title = "" )
		{
			object DbData = new object ( );
			string resultString = "", path = "";
			string jsonresult = "";

			Progressbar pbar = new Progressbar ( );
			pbar . Show ( );
			Mouse . OverrideCursor = Cursors . Wait;

			//We need to save current Collectionview as a Json (binary) data to disk
			// this is the best way to save persistent data in Json format
			//using tmp folder for interim file that we will then display
			if ( CurrentDb == "BANKACCOUNT" )
			{
				path = @"C:\\tmp\\BankTempdata.json";
				jsonresult = JsonConvert . SerializeObject ( collection );
				JsonSupport . JsonSerialize ( jsonresult, path );
				if (Title == "") Title = "BankAccount";
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				path = @"C:\\tmp\\CustomerTempdata.json";
				jsonresult = JsonConvert . SerializeObject ( collection );
				JsonSupport . JsonSerialize ( jsonresult, path );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				path = @"C:\\tmp\\DetailsTempdata.json";
				jsonresult = JsonConvert . SerializeObject ( collection );
				JsonSupport . JsonSerialize ( jsonresult, path );
			}

			//Now Create JSON file in PRETTY FORMAT ??
			resultString = JsonSupport . CreateFormattedJsonOutput ( jsonresult, Title, pbar );

			// remove tmp file
			File . Delete ( path );
			path = @"C:\tmp\dboutput.json";
			File . WriteAllText ( path, resultString );
			Mouse . OverrideCursor = Cursors . Arrow;
			pbar . Close ( );
			/// Finally - Use the default viewer to Display the data we have generated...

			// Get default text files viewer application from App resources
			string program = ( string ) Properties . Settings . Default [ "DefaultTextviewer" ];

			Process ExternalProcess = new Process ( );
			ExternalProcess . StartInfo . FileName = program . Trim ( );
			ExternalProcess . StartInfo . Arguments = path . Trim ( );
			try
			{
				ExternalProcess . Start ( );
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"ExternalProcess error : {ex . Message}, {ex . Data}" );
				if ( ex . Message . Contains ( "cannot find the file" ) )
				{
					if ( Flags . SingleSearchPath != "" )
						MessageBox . Show ( $"Error executing the (Full specified command) \n \"{ program}\"\n\nThe System was unable to Execute this file.", "File Execution Error !" );
					else
						MessageBox . Show ( $"Error executing the (command) shown below\n \"{ program} {path}\"\n\nThe System was unable to Execute this file.", "File Execution Error !" );
				}
				else
					MessageBox . Show ( $"Error executing the (Command) shown below\n \"{ program} {path}\"\n\nSee the Debug output for more information.", "File Execution Error !" );

			}
			finally
			{
				ExternalProcess . Close ( );
			}
		}
		#endregion JSON SUPPORT

	}
}
