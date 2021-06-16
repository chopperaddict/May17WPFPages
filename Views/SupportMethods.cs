using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . IO;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Input;

namespace WPFPages . Views
{
	public class SupportMethods
	{
		/// <summary>
		/// Implementation of a Delegate provided to pass to FindExecutePath() 
		/// below to extend the search process in Utils.ProcessExecuteRequest() as required. it includes a List[string]
		/// holding various  different paths to be searched in addition to the defaults
		/// providedby [Environment . SpecialFolder. xxxxxx ]in the application
		/// </summary>
		/// <param name="possiblefolders"></param>
		/// <param name="filename"></param>
		/// <returns></returns>
		public static string qualifiers ( string Filename )
		{
			string result = "", temp="";
			// Get the search paths from disk file
			List<string> AdditionalFolders = new List<string> ( );
			// Get searchpaths files own path
			string Str = ( string ) Properties . Settings . Default [ "SearchPathStringFileName" ];

			//Read in our search strings list into string[]
			string [ ] paths = File . ReadAllLines ( Str + @"\searchpaths.dat" );
			// now put them into List<string> collection
			foreach ( var item in paths )
			{
				char tmp = item [ item . Length - 1 ];
				if(tmp != '\\')
					AdditionalFolders . Add ( item+ "@\\" );
				else
					AdditionalFolders . Add ( item );
			}

			// Finally - Search these paths for our selected file to be executed
			foreach ( var path in AdditionalFolders )
			{
				if ( File . Exists ( path + $"{Filename}" ) )
				{
					result = path + $"{Filename}";
					break;
				}
			}
			return result;
		}

		/// <summary>
		/// Executes any file path/name double clicked in the Textbox of the DRag/Drop Window
		/// or from a string (command) received if possible.
		/// All Strings passed directly need to include a fully qualified path where relevant.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <param name="command"></param>
		/// <returns></returns>
		/// The normal input control is a WPF Textbox, but you can pass a String as a 
		/// command line instead of the method processing the Textbox to obtain 
		/// the selected string for execution
		/// So only sender and either Textbox or Command string are needed
		public static bool ProcessExecuteRequest ( object sender, MouseButtonEventArgs e = null, TextBox control = null, string command = "" )
		{
			bool result = false;
			int linelen = 0;
			string controlcontent = "";
			string currentline = "";
			string linetext = "";
			TextBox textBox = control;
			//Using a TextBox as input ?
			if ( command == "" )
			{
				// YES - so Parse the line that was dbl.clicked in the Textbox to get the command to be executed
				int caretpos = textBox . CaretIndex;
				var line = textBox . GetLineIndexFromCharacterIndex ( caretpos );
				linelen = textBox . GetLineLength ( line );
				// This gets me the complete line, including \n
				linetext = textBox . GetLineText ( line );
				linetext = linetext . Substring ( 0, linelen - 1 );
				// Check for line endings \r and \n and remove them if found
				if ( linetext . Contains ( "\r" ) )
					linetext = linetext . Substring ( 0, linetext . Length - 1 );
				if ( linetext . Contains ( "\n" ) )
					linetext = linetext . Substring ( 0, linetext . Length - 1 );
				controlcontent = textBox . Text;
				currentline = controlcontent;
				// We should now have a clean, complete command line to extract from the TextBox text 
				for ( int indx = 0 ; indx < controlcontent . Length - 1 ; indx++ )
				{
					currentline = controlcontent . Substring ( indx );
					if ( currentline . Contains ( linetext ) == false )
					{
						currentline = controlcontent . Substring ( 0, ( indx - 2 ) < 0 ? 0 : indx - 2 );
						if ( currentline . Length == 0 )
							currentline = controlcontent;
						// now got the full command string alone
						textBox . CaretIndex = indx - 1;
						textBox . SelectionLength = linelen;
						break;
					}
				}
			}
			else
			{
				//No - so use the string sent to us
				int caretpos = 0;
				var line = command;
				linelen = command . Length;
				// This gets me the complete line, including \n
				linetext = command;
				if ( linetext . Contains ( "\r" ) )
					linetext = linetext . Substring ( 0, linetext . Length - 1 );
				if ( linetext . Contains ( "\n" ) )
					linetext = linetext . Substring ( 0, linetext . Length - 1 );
				currentline = linetext;
				controlcontent = linetext;
			}
			if ( currentline . Length == 0 ) return false;

			// how to save the user specified search paths to settings.settings
			//Utils . SaveProperty ( "SearchPathStringFileName", Environment . GetFolderPath ( Environment . SpecialFolder . MyDocuments ) );
			//ConfigurationManager . RefreshSection ( "SearchPathStringFileName" );

			// Get searchpaths files own path
			string Str = ( string ) Properties . Settings . Default [ "SearchPathStringFileName" ];


			//Read in our search strings list into string[]
			string [ ] paths = File . ReadAllLines ( Str + @"\searchpaths.dat" );
			//List<string> tmp = new List<string> ( );
			//foreach ( var item in paths )
			//{
			//	if(item.Length > 4)
			//	tmp . Add ( item );
			//}

			//Get this apps current directory
			string fullPath = GetCurrentApplicationFullPath ( );
			//Add current App Path to our list as well.  gotta increase the array by 1 element first !
			Array . Resize ( ref paths, paths . Length + 1 );
			paths [ paths . Length-1 ] = fullPath;
			//tmp . Add ( fullPath );

			//Finally, convert it back to an array so wecan access it ore easily.
			//paths = tmp . ToArray ( );

			var test = linetext;
			//check for path backslashes. If none found, we use our path search
			//function to try to identify the path automaticlly - Clever eh ?
			if ( linetext . Contains ( "\\" ) == false )
			{
				// Add it to our search string collection
				try
				{
					//Setup our delegate
					QualifyingFileLocations FindPathHandler = SupportMethods . qualifiers;
					// pass the delegate method thru to our search for executable path method
					// It contains all the specialist paths we want to have searched
					// WORKS VERY WELL 15/6/21
					test = FindExecutePath ( linetext, SupportMethods . qualifiers );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"Failure in Utils.FindExecutePath()\n{ex . Message}, {ex . Data}" );
					return false;
				}
			}
			// if only original string is returned, we just execute that and hope....
			// otherwise, we use the qualified path we have found for the user
			if ( test == "" )
				test = linetext;

			// Finally, lets try to execute the command received
			Process ExternalProcess = new Process ( );
			ExternalProcess . StartInfo . FileName = test . Trim ( );
			ExternalProcess . StartInfo . WindowStyle = ProcessWindowStyle . Normal;
			ExternalProcess . EnableRaisingEvents = false;
			try
			{
				ExternalProcess . Start ( );
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"ExternalProcess error : {ex . Message}, {ex . Data}" );
				if ( ex . Message . Contains ( "cannot find the file" ) )
					MessageBox . Show ( $"Error executing the (command) shown below\n \"{ linetext }\"\n\nthe System was unable to Execute this file.", "File Execution Error !" );
				else
					MessageBox . Show ( $"Error executing the (Command) shown below\n \"{ linetext }\"\n\nSee the Debug output for more information.", "File Execution Error !" );

			}
			finally
			{
				ExternalProcess . Close ( );
				result = true;
			}
			//e . Handled = true;
			return result;
		}
		public static string GetCurrentApplicationFullPath ( )
		{
			var process = Process . GetCurrentProcess ( ); // Or whatever method you are using
			string fullPath = process . MainModule . FileName;
			int len = fullPath . Length;
			var name = process . ProcessName + ".exe";
			if ( fullPath . Contains ( name ) )
			{
				fullPath = fullPath . Substring ( 0, len - ( name . Length ) );
				//We now have a full current path with trailing slash \\
			}
			return fullPath;
		}

		public static string FindExecutePath ( string filename, QualifyingFileLocations qualifiers = null )
		{
			string Fullpath = "";
			if ( qualifiers != null )
			{
				Fullpath = qualifiers ( filename );
				if ( Fullpath != "" )
					return Fullpath;
			}

			if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . CommonProgramFiles ) + $"\\{filename}" ) )
				return Environment . GetFolderPath ( Environment . SpecialFolder . CommonProgramFiles ) + $"\\{filename}";
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . Windows ) + $"\\{filename}" ) )
				return Environment . GetFolderPath ( Environment . SpecialFolder . Windows ) + $"\\{filename}";  // Current User's Application Data
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . ApplicationData ) + $"\\{filename}" ) )
				return Environment . GetFolderPath ( Environment . SpecialFolder . ApplicationData ) + $"\\{filename}";  // Current User's Application Data
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . CommonApplicationData ) + $"\\{filename}" ) ) // All User's Application Data
				return Environment . GetFolderPath ( Environment . SpecialFolder . CommonApplicationData ) + $"\\{filename}";
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . CommonProgramFiles ) + $"\\{filename}" ) ) // Program Files
				return Environment . GetFolderPath ( Environment . SpecialFolder . CommonProgramFiles ) + $"\\{filename}";
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . Cookies ) + $"\\{filename}" ) ) // Internet Cookie
				return Environment . GetFolderPath ( Environment . SpecialFolder . Cookies ) + $"\\{filename}";
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . Desktop ) + $"\\{filename}" ) )  // Logical Desktop
				return Environment . GetFolderPath ( Environment . SpecialFolder . Desktop ) + $"\\{filename}";
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . DesktopDirectory ) + $"\\{filename}" ) ) // Physical Desktop
				return Environment . GetFolderPath ( Environment . SpecialFolder . DesktopDirectory ) + $"\\{filename}";
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . Favorites ) + $"\\{filename}" ) ) // Favorites
				return Environment . GetFolderPath ( Environment . SpecialFolder . Favorites ) + $"\\{filename}";
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . MyComputer ) + $"\\{filename}" ) ) // "My Computer" Folder
				return Environment . GetFolderPath ( Environment . SpecialFolder . MyComputer ) + $"\\{filename}";
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . MyDocuments ) + $"\\{filename}" ) ) // "My Computer" Folder
				return Environment . GetFolderPath ( Environment . SpecialFolder . MyDocuments ) + $"\\{filename}";
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . MyMusic ) + $"\\{filename}" ) ) // "My Music" Folder
				return Environment . GetFolderPath ( Environment . SpecialFolder . MyMusic ) + $"\\{filename}";
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . MyPictures ) + $"\\{filename}" ) ) // "My Pictures" Folder
				return Environment . GetFolderPath ( Environment . SpecialFolder . MyPictures ) + $"\\{filename}"; // "My Pictures" Folder
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . Personal ) + $"\\{filename}" ) ) // "My Document" Folder
				return Environment . GetFolderPath ( Environment . SpecialFolder . Personal ) + $"\\{filename}"; // "My Document" Folder
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . ProgramFiles ) + $"\\{filename}" ) ) // Program files Folder
				return Environment . GetFolderPath ( Environment . SpecialFolder . ProgramFiles ) + $"\\{filename}";  // Program files Folder
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . Programs ) + $"\\{filename}" ) ) // Programs Folder
				return Environment . GetFolderPath ( Environment . SpecialFolder . Programs ) + $"\\{filename}"; // Programs Folder
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . Recent ) + $"\\{filename}" ) ) // Recent Folder
				return Environment . GetFolderPath ( Environment . SpecialFolder . Recent ) + $"\\{filename}"; // Recent Folder
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . SendTo ) + $"\\{filename}" ) ) // "Sent to" Folder
				return Environment . GetFolderPath ( Environment . SpecialFolder . SendTo ) + $"\\{filename}"; // "Sent to" Folder
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . StartMenu ) + $"\\{filename}" ) ) // Start Menu
				return Environment . GetFolderPath ( Environment . SpecialFolder . StartMenu ) + $"\\{filename}"; // Start Menu
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . Startup ) + $"\\{filename}" ) ) // Startup
				return Environment . GetFolderPath ( Environment . SpecialFolder . Startup ) + $"\\{filename}"; // Startup
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . System ) + $"\\{filename}" ) ) // System Folder
				return Environment . GetFolderPath ( Environment . SpecialFolder . System ) + $"\\{filename}"; // System Folder
			else if ( File . Exists ( Environment . GetFolderPath ( Environment . SpecialFolder . Templates ) + $"\\{filename}" ) ) // Document Templates		}
				return Environment . GetFolderPath ( Environment . SpecialFolder . Templates ) + $"\\{filename}"; // Document Templates		}
			return "";
		}


	}
}
