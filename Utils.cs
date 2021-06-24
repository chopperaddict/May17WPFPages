#define SHOWWINDOWDATA
using System;
using System . Collections . Generic;
using System . Configuration;
using System . Diagnostics;
using System . IO;
using System . Runtime . InteropServices . WindowsRuntime;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Controls . Primitives;
using System . Windows . Forms . VisualStyles;
using System . Windows . Input;
using System . Windows . Media;
using Microsoft . Win32;
using Newtonsoft . Json;
using WPFPages . Properties;
using WPFPages . ViewModels;
using WPFPages . Views;
using System . Text;
using Newtonsoft . Json . Linq;

namespace WPFPages
{
	/// <summary>
	/// Class to handle various utility functions such as fetching 
	/// Style/Templates/Brushes etc to Set/Reset control styles 
	/// from various Dictionary sources for use in "code behind"
	/// </summary>
	public class Utils
	{
		public static Action<DataGrid, int> GridInitialSetup = Utils . SetUpGridSelection;
		//		public static Func<bool, BankAccountViewModel, CustomerViewModel, DetailsViewModel> IsMatched = CheckRecordMatch; 
		public static Func<object, object, bool> IsRecordMatched = Utils . CompareDbRecords;

		public struct bankrec
		{
			public string custno { get; set; }
			public string bankno { get; set; }
			public int actype { get; set; }
			public decimal intrate { get; set; }
			public decimal balance { get; set; }
			public DateTime odate { get; set; }
			public DateTime cdate { get; set; }
		}


		// Declare the first few notes of the song, "Mary Had A Little Lamb".
		// Define the frequencies of notes in an octave, as well as
		// silence (rest).
		protected enum Tone
		{
			REST = 0,
			GbelowC = 196,
			A = 220,
			Asharp = 233,
			B = 247,
			C = 262,
			Csharp = 277,
			D = 294,
			Dsharp = 311,
			E = 330,
			F = 349,
			Fsharp = 370,
			G = 392,
			Gsharp = 415,
		}

		// Define the duration of a note in units of milliseconds.
		protected enum Duration
		{
			WHOLE = 1600,
			HALF = WHOLE / 2,
			QUARTER = HALF / 2,
			EIGHTH = QUARTER / 2,
			SIXTEENTH = EIGHTH / 2,
		}

		protected struct Note
		{
			Tone toneVal;
			Duration durVal;

			// Define a constructor to create a specific note.
			public Note ( Tone frequency, Duration time )
			{
				toneVal = frequency;
				durVal = time;
			}

			// Define properties to return the note's tone and duration.
			public Tone NoteTone { get { return toneVal; } }
			public Duration NoteDuration { get { return durVal; } }
		}
		public static void PlayMary ( )
		{
			Note [ ] Mary =
			{
					new Note(Tone.B, Duration.QUARTER),
					new Note(Tone.A, Duration.QUARTER),
					new Note(Tone.GbelowC, Duration.QUARTER),
					new Note(Tone.A, Duration.QUARTER),
					new Note(Tone.B, Duration.QUARTER),
					new Note(Tone.B, Duration.QUARTER),
					new Note(Tone.B, Duration.HALF),
					new Note(Tone.A, Duration.QUARTER),
					new Note(Tone.A, Duration.QUARTER),
					new Note(Tone.A, Duration.HALF),
					new Note(Tone.B, Duration.QUARTER),
					new Note(Tone.D, Duration.QUARTER),
					new Note(Tone.D, Duration.HALF)
			};
			// Play the song
			Play ( Mary );
		}
		// Play the notes in a song.
		protected static void Play ( Note [ ] tune )
		{
			foreach ( Note n in tune )
			{
				if ( n . NoteTone == Tone . REST )
					Thread . Sleep ( ( int ) n . NoteDuration );
				else
					Console . Beep ( ( int ) n . NoteTone, ( int ) n . NoteDuration );
			}
		}
		// Define a note as a frequency (tone) and the amount of
		//// time (duration) the note plays.
		//public static Task DoBeep ( int freq = 180, int count = 300, bool swap = false )
		//{
		//	int tone = freq;
		//	int duration = count;
		//	int x = 0;
		//	Task t = new Task ( ( ) => x = 1 );
		//	if ( Flags . UseBeeps )
		//	{
		//		if ( swap )
		//		{
		//			tone = ( tone / 4 ) * 3;
		//			duration = ( count * 5 ) / 2;
		//			t = Task . Factory . StartNew ( ( ) => Console . Beep ( freq, count ) )
		//				. ContinueWith ( Action => Console . Beep ( tone, duration ) );
		//			Thread . Sleep ( 500 );
		//		}
		//		else
		//		{
		//			tone = ( tone / 4 ) * 3;
		//			duration = ( count * 5 ) / 2;
		//			t = Task . Factory . StartNew ( ( ) => Console . Beep ( tone, duration ) )
		//				. ContinueWith ( Action => Console . Beep ( freq, count ) );
		//			Thread . Sleep ( 500 );
		//		}
		//	}
		//	else
		//	{
		//		Task task = Task . Factory . StartNew ( ( ) => Console . WriteLine ( ) );
		//		t = task ,TaskScheduler . FromCurrentSynchronizationContext ( );
		//			}

		//	TaskScheduler . FromCurrentSynchronizationContext ( ));
		//	return t;
		//}
		public static Task DoSingleBeep ( int freq = 280, int count = 300, int repeat = 1 )
		{
			int x = 0;
			Task t = new Task ( ( ) => x = 1 );
			if ( Flags . UseBeeps )
			{
				for ( int i = 0 ; i < repeat ; i++ )
				{
					Console . Beep ( freq, count );
					//t = Task . Factory . StartNew ( ( ) => Console . Beep ( freq, count ) );
					//					Thread . Sleep ( 100 );
				}
				Thread . Sleep ( 200 );
			}
			//else
			//	t = Task . Factory . StartNew ( ( ) => Console . WriteLine ( ) );
			return t;
		}
		public static Task DoErrorBeep ( int freq = 280, int count = 100, int repeat = 3 )
		{
			int x = 0;
			Task t = new Task ( ( ) => x = 1 );
			if ( Flags . UseBeeps )
			{
				for ( int i = 0 ; i < repeat ; i++ )
				{
					Console . Beep ( freq, count );
				}
				Thread . Sleep ( 100 );
			}
			return t;
		}

		public static BankAccountViewModel CreateBankRecordFromString ( string type, string input )
		{
			int index = 0;
			BankAccountViewModel bvm = new BankAccountViewModel ( );
			char [ ] s = { ',' };
			string [ ] data = input . Split ( s );
			string donor = data [ 0 ];
			try
			{
				DateTime dt;
				if ( type == "BANK" || type == "DETAILS" )
				{
					// This WORKS CORRECTLY 12/6/21 when called from n SQLDbViewer DETAILS grid entry && BANK grid entry					
					// this test confirms the data layout by finding the Odate field correctly
					// else it drops thru to the Catch branch
					dt = Convert . ToDateTime ( data [ 7 ] );
					//We can have any type of record in the string recvd
					index = 1;  // jump the data type string
					bvm . Id = int . Parse ( data [ index++ ] );
					bvm . CustNo = data [ index++ ];
					bvm . BankNo = data [ index++ ];
					bvm . AcType = int . Parse ( data [ index++ ] );
					bvm . IntRate = decimal . Parse ( data [ index++ ] );
					bvm . Balance = decimal . Parse ( data [ index++ ] );
					bvm . ODate = Convert . ToDateTime ( data [ index++ ] );
					bvm . CDate = Convert . ToDateTime ( data [ index ] );
					return bvm;
				}
				else if ( type == "CUSTOMER" )
				{
					// this test confirms the data layout by finding the Odate field correctly
					// else it drops thru to the Catch branch
					dt = Convert . ToDateTime ( data [ 5 ] );
					// We have a customer record !!
					//Check to see if the data includes the data type in it
					//As we have to parse it diffrently if not - see index....
					index = 1;
					bvm . Id = int . Parse ( data [ index++ ] );
					bvm . CustNo = data [ index++ ];
					bvm . BankNo = data [ index++ ];
					bvm . AcType = int . Parse ( data [ index++ ] );
					bvm . ODate = Convert . ToDateTime ( data [ index++ ] );
					bvm . CDate = Convert . ToDateTime ( data [ index ] );
				}
				return bvm;
			}
			catch
			{
				//Check to see if the data includes the data type in it
				//As we have to parse it diffrently if not - see index....
				index = 0;
				try
				{
					int x = int . Parse ( donor );
					// if we get here, it IS a NUMERIC VALUE
					index = 0;
				}
				catch ( Exception ex )
				{
					//its probably the Data Type string, so ignore it for our Data creation processing
					index = 1;
				}
				//We have a CUSTOMER record
				bvm . Id = int . Parse ( data [ index++ ] );
				bvm . CustNo = data [ index++ ];
				bvm . BankNo = data [ index++ ];
				bvm . AcType = int . Parse ( data [ index++ ] );
				bvm . ODate = Convert . ToDateTime ( data [ index++ ] );
				bvm . CDate = Convert . ToDateTime ( data [ index ] );
				return bvm;
			}
		}
		public static DragviewModel CreateGridRecordFromString ( string input )
		{
			int index = 0;
			string type = "";
			//			BankAccountViewModel bvm = new BankAccountViewModel ( );
			DragviewModel bvm = new DragviewModel ( );
			char [ ] s = { ',' };
			string [ ] data = input . Split ( s );
			string donor = data [ 0 ];
			try
			{
				DateTime dt;
				type = data [ 0 ];
				if ( type == "BANKACCOUNT" || type == "BANK" || type == "DETAILS" )
				{
					// This WORKS CORRECTLY 12/6/21 when called from n SQLDbViewer DETAILS grid entry && BANK grid entry					
					// this test confirms the data layout by finding the Odate field correctly
					// else it drops thru to the Catch branch
					dt = Convert . ToDateTime ( data [ 7 ] );
					//We can have any type of record in the string recvd
					index = 1;  // jump the data type string
					bvm . RecordType = type;
					bvm . Id = int . Parse ( data [ index++ ] );
					bvm . CustNo = data [ index++ ];
					bvm . BankNo = data [ index++ ];
					bvm . AcType = int . Parse ( data [ index++ ] );
					bvm . IntRate = decimal . Parse ( data [ index++ ] );
					bvm . Balance = decimal . Parse ( data [ index++ ] );
					bvm . ODate = Convert . ToDateTime ( data [ index++ ] );
					bvm . CDate = Convert . ToDateTime ( data [ index ] );
					return bvm;
				}
				else if ( type == "CUSTOMER" )
				{
					// this test confirms the data layout by finding the Odate field correctly
					// else it drops thru to the Catch branch
					dt = Convert . ToDateTime ( data [ 5 ] );
					// We have a customer record !!
					//Check to see if the data includes the data type in it
					//As we have to parse it diffrently if not - see index....
					index = 1;
					bvm . RecordType = type;
					bvm . Id = int . Parse ( data [ index++ ] );
					bvm . CustNo = data [ index++ ];
					bvm . BankNo = data [ index++ ];
					bvm . AcType = int . Parse ( data [ index++ ] );
					bvm . ODate = Convert . ToDateTime ( data [ index++ ] );
					bvm . CDate = Convert . ToDateTime ( data [ index ] );
				}
				return bvm;
			}
			catch
			{
				//Check to see if the data includes the data type in it
				//As we have to parse it diffrently if not - see index....
				index = 0;
				try
				{
					int x = int . Parse ( donor );
					// if we get here, it IS a NUMERIC VALUE
					index = 0;
				}
				catch ( Exception ex )
				{
					//its probably the Data Type string, so ignore it for our Data creation processing
					index = 1;
				}
				//We have a CUSTOMER record
				bvm . RecordType = type;
				bvm . Id = int . Parse ( data [ index++ ] );
				bvm . CustNo = data [ index++ ];
				bvm . BankNo = data [ index++ ];
				bvm . AcType = int . Parse ( data [ index++ ] );
				bvm . ODate = Convert . ToDateTime ( data [ index++ ] );
				bvm . CDate = Convert . ToDateTime ( data [ index ] );
				return bvm;
			}
		}
		public static CustomerViewModel CreateCustomerRecordFromString ( string input )
		{
			int index = 0;
			CustomerViewModel cvm = new CustomerViewModel ( );
			char [ ] s = { ',' };
			string [ ] data = input . Split ( s );
			string donor = data [ 0 ];
			//Check to see if the data includes the data type in it
			//As we have to parse it diffrently if not - see index....
			if ( donor . Length > 3 )
				index = 1;
			//We have the sender type in the string recvd
			cvm . Id = int . Parse ( data [ index++ ] );
			cvm . CustNo = data [ index++ ];
			cvm . BankNo = data [ index++ ];
			cvm . AcType = int . Parse ( data [ index++ ] );
			cvm . ODate = DateTime . Parse ( data [ index++ ] );
			cvm . CDate = DateTime . Parse ( data [ index ] );
			return cvm;
		}
		public static DetailsViewModel CreateDetailsRecordFromString ( string input )
		{
			int index = 0;
			DetailsViewModel bvm = new DetailsViewModel ( );
			char [ ] s = { ',' };
			string [ ] data = input . Split ( s );
			string donor = data [ 0 ];
			//Check to see if the data includes the data type in it
			//As we have to parse it diffrently if not - see index....
			if ( donor . Length > 3 )
				index = 1;
			bvm . Id = int . Parse ( data [ index++ ] );
			bvm . CustNo = data [ index++ ];
			bvm . BankNo = data [ index++ ];
			bvm . AcType = int . Parse ( data [ index++ ] );
			bvm . IntRate = decimal . Parse ( data [ index++ ] );
			bvm . Balance = decimal . Parse ( data [ index++ ] );
			bvm . ODate = DateTime . Parse ( data [ index++ ] );
			bvm . CDate = DateTime . Parse ( data [ index ] );
			return bvm;
		}
		public static string CreateFullCsvTextFromRecord ( BankAccountViewModel bvm, DetailsViewModel dvm, CustomerViewModel cvm = null, bool IncludeType = true )
		{
			if ( bvm == null && cvm == null && dvm == null ) return "";
			string datastring = "";
			if ( bvm != null )
			{
				// Handle a BANK Record
				if ( IncludeType ) datastring = "BANKACCOUNT";
				datastring += bvm . Id + ",";
				datastring += bvm . CustNo + ",";
				datastring += bvm . BankNo + ",";
				datastring += bvm . AcType . ToString ( ) + ",";
				datastring += bvm . IntRate . ToString ( ) + ",";
				datastring += bvm . Balance . ToString ( ) + ",";
				datastring += "'" + bvm . CDate . ToString ( ) + "',";
				datastring += "'" + bvm . ODate . ToString ( ) + "',";
			}
			else if ( dvm != null )
			{
				if ( IncludeType ) datastring = "DETAILS,";
				datastring += dvm . Id + ",";
				datastring += dvm . CustNo + ",";
				datastring += dvm . BankNo + ",";
				datastring += dvm . AcType . ToString ( ) + ",";
				datastring += dvm . IntRate . ToString ( ) + ",";
				datastring += dvm . Balance . ToString ( ) + ",";
				datastring += "'" + dvm . CDate . ToString ( ) + "',";
				datastring += dvm . ODate . ToString ( ) + ",";
			}
			else if ( cvm != null )
			{
				if ( IncludeType ) datastring = "CUSTOMER,";
				datastring += cvm . Id + ",";
				datastring += cvm . CustNo + ",";
				datastring += cvm . BankNo + ",";
				datastring += cvm . AcType . ToString ( ) + ",";
				datastring += "'" + cvm . CDate . ToString ( ) + "',";
				datastring += cvm . ODate . ToString ( ) + ",";
			}
			return datastring;
		}
		public static string CreateDragDataFromRecord ( DragviewModel bvm )
		{
			if ( bvm == null ) return "";
			string datastring = "";
			datastring = bvm . RecordType + ",";
			datastring += bvm . Id + ",";
			datastring += bvm . CustNo + ",";
			datastring += bvm . BankNo + ",";
			datastring += bvm . AcType . ToString ( ) + ",";
			datastring += bvm . IntRate . ToString ( ) + ",";
			datastring += bvm . Balance . ToString ( ) + ",";
			datastring += "'" + bvm . CDate . ToString ( ) + "',";
			datastring += "'" + bvm . ODate . ToString ( ) + "',";
			return datastring;
		}

		public static void SaveProperty ( string setting, string value )
		{
			try
			{
				if ( value . ToUpper ( ) . Contains ( "TRUE" ) )
					Settings . Default [ setting ] = true;
				else if ( value . ToUpper ( ) . Contains ( "FALSE" ) )
					Settings . Default [ setting ] = false;
				else
					Settings . Default [ setting ] = value;
				Settings . Default . Save ( );
				Settings . Default . Upgrade ( );
				ConfigurationManager . RefreshSection ( setting );
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Unable to save property {setting} of [{value}]\nError was {ex . Data}, {ex . Message}, Stack trace = \n{ex . StackTrace}" );
			}
		}
		public static string GetExportFileName ( string filespec )
		// opens  the common file open dialog
		{
			OpenFileDialog ofd = new OpenFileDialog ( );
			ofd . InitialDirectory = @"C:\Users\ianch\Documents\";
			ofd . CheckFileExists = false;
			ofd . AddExtension = true;
			ofd . Title = "Select name for Exported data file.";
			if ( filespec . ToUpper ( ) . Contains ( "XL" ) )
				ofd . Filter = "Excel Spreadsheets (*.xl*) | *.xl*";
			else if ( filespec . ToUpper ( ) . Contains ( "CSV" ) )
				ofd . Filter = "Comma seperated data (*.csv) | *.csv";
			else if ( filespec . ToUpper ( ) . Contains ( "*.*" ) )
				ofd . Filter = "All Files (*.*) | *.*";
			else if ( filespec == "" )
			{
				ofd . Filter = "All Files (*.*) | *.*";
				ofd . DefaultExt = ".CSV";
			}
			//			string initfolder = ofd . InitialDirectory;
			//			if ( initfolder != "" && fnameonly != "" )
			//				filespec = initfolder + fnameonly;
			ofd . FileName = filespec;
			//if ( filespec == "" )
			//	ofd . DefaultExt = ".XLS*" ;
			//else
			//	ofd . DefaultExt = $".{filespec . ToUpper ( )}" ;
			ofd . ShowDialog ( );
			string fnameonly = ofd . SafeFileName;
			return ofd . FileName;
		}

		public static string GetImportFileName ( string filespec )
		// opens  the common file open dialog
		{
			OpenFileDialog ofd = new OpenFileDialog ( );
			ofd . InitialDirectory = @"C:\Users\ianch\Documents\";
			ofd . CheckFileExists = true;
			if ( filespec . ToUpper ( ) . Contains ( "XL" ) )
				ofd . Filter = "Excel Spreadsheets (*.xl*) | *.xl*";
			else if ( filespec . ToUpper ( ) . Contains ( "CSV" ) )
				ofd . Filter = "Comma seperated data (*.csv) | *.csv";
			else if ( filespec . ToUpper ( ) . Contains ( "*.*" ) || filespec == "" )
				ofd . Filter = "All Files (*.*) | *.*";
			ofd . AddExtension = true;
			//if ( filespec == "" )
			//	ofd . DefaultExt = ".XLS*" ;
			//else
			//	ofd . DefaultExt = $".{filespec . ToUpper ( )}" ;
			ofd . ShowDialog ( );
			return ofd . FileName;
		}
		public static string ConvertInputDate ( string datein )
		{
			string YYYMMDD = "";
			string [ ] datebits;
			// This filter will strip off the "Time" section of an excel date
			// and return us a valid YYYY/MM/DD string
			char [ ] ch = { '/', ' ' };
			datebits = datein . Split ( ch );
			if ( datebits . Length < 3 ) return datein;

			// check input to see if it needs reversing ?
			if ( datebits [ 0 ] . Length == 4 )
				YYYMMDD = datebits [ 0 ] + "/" + datebits [ 1 ] + "/" + datebits [ 2 ];
			else
				YYYMMDD = datebits [ 2 ] + "/" + datebits [ 1 ] + "/" + datebits [ 0 ];
			return YYYMMDD;
		}


		public static void HandleCtrlFnKeys ( bool key1, KeyEventArgs e )
		{
			if ( key1 && e . Key == Key . F5 )
			{
				// list Flags in Console
				Utils . GetWindowHandles ( );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( key1 && e . Key == Key . F6 )  // CTRL + F6
			{
				// list various Flags in Console
				Debug . WriteLine ( $"\nCTRL + F6 pressed..." );
				Flags . UseBeeps = !Flags . UseBeeps;
				e . Handled = true;
				key1 = false;
				Debug . WriteLine ( $"Flags.UseBeeps reset to  {Flags . UseBeeps }" );
				return;
			}
			else if ( key1 && e . Key == Key . F7 )  // CTRL + F7
			{
				// list various Flags in Console
				Debug . WriteLine ( $"\nCTRL + F7 pressed..." );
				Flags . PrintDbInfo ( );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( key1 && e . Key == Key . F8 )     // CTRL + F8
			{
				Debug . WriteLine ( $"\nCTRL + F8 pressed..." );
				EventHandlers . ShowSubscribersCount ( );
				e . Handled = true;
				key1 = false;
				return;
			}
			else if ( key1 && e . Key == Key . F9 )     // CTRL + F9
			{
				Debug . WriteLine ( "\nCtrl + F9 NOT Implemented" );
				key1 = false;
				return;

			}
			else if ( key1 && e . Key == Key . System )     // CTRL + F10
			{
				// Major  listof GV[] variables (Guids etc]
				Debug . WriteLine ( $"\nCTRL + F10 pressed..." );
				Flags . ListGridviewControlFlags ( 1 );
				key1 = false;
				e . Handled = true;
				return;
			}
			else if ( key1 && e . Key == Key . F11 )  // CTRL + F11
			{
				// list various Flags in Console
				Debug . WriteLine ( $"\nCTRL + F11 pressed..." );
				Flags . PrintSundryVariables ( );
				e . Handled = true;
				key1 = false;
				return;
			}
		}
		private void CloseviewerWindow ( int index )
		{
			//Close the specified viewer
			if ( MainWindow . gv . window != null )
			{
				//Fn removes all record of it's very existence
				MainWindow . gv . window [ index ] . Close ( );
				Flags . CurrentSqlViewer = null;
				MainWindow . gv . SqlViewerWindow = null;
			}
		}

		/// <summary>
		/// A Func that takes ANY 2 (of 3 [Bank,Customer,Details] Db type records and returns true if the CustNo and Bankno match
		/// </summary>
		/// <param name="obj1"></param>
		/// <param name="obj2"></param>
		/// <returns></returns>
		public static bool CompareDbRecords ( object obj1, object obj2 )
		{
			bool result = false;
			BankAccountViewModel bvm = new BankAccountViewModel ( );
			CustomerViewModel cvm = new CustomerViewModel ( );
			DetailsViewModel dvm = new DetailsViewModel ( );
			//bvm = null;
			//cvm = null;
			//dvm = null;
			if ( obj1 == null || obj2 == null )
				return result;
			if ( obj1 . GetType ( ) == bvm . GetType ( ) )
				bvm = obj1 as BankAccountViewModel;
			if ( obj1 . GetType ( ) == cvm . GetType ( ) )
				cvm = obj1 as CustomerViewModel;
			if ( obj1 . GetType ( ) == dvm . GetType ( ) )
				dvm = obj1 as DetailsViewModel;

			if ( obj2 . GetType ( ) == bvm . GetType ( ) )
				bvm = obj2 as BankAccountViewModel;
			if ( obj2 . GetType ( ) == cvm . GetType ( ) )
				cvm = obj2 as CustomerViewModel;
			if ( obj2 . GetType ( ) == dvm . GetType ( ) )
				dvm = obj2 as DetailsViewModel;

			if ( bvm != null && cvm != null )
			{
				if ( bvm . CustNo == cvm . CustNo )
					result = true;
			}
			else if ( bvm != null && dvm != null )
			{
				if ( bvm . CustNo == dvm . CustNo )
					result = true;
			}
			else if ( cvm != null && dvm != null )
			{
				if ( cvm . CustNo == dvm . CustNo )
					result = true;
			}
			result = false;
			return result;
		}

		public static bool CheckRecordMatch (
			BankAccountViewModel bvm,
			CustomerViewModel cvm,
			DetailsViewModel dvm )
		{
			bool result = false;
			if ( bvm != null && cvm != null )
			{
				if ( bvm . CustNo == cvm . CustNo )
					result = true;
			}
			else if ( bvm != null && dvm != null )
			{
				if ( bvm . CustNo == dvm . CustNo )
					result = true;
			}
			else if ( cvm != null && dvm != null )
			{
				if ( cvm . CustNo == dvm . CustNo )
					result = true;
			}
			return result;
		}

		public static void SetSelectedItemFirstRow ( object dataGrid, object selectedItem )
		{
			//If target datagrid Empty, throw exception
			if ( dataGrid == null )
			{
				throw new ArgumentNullException ( "Target none" + dataGrid + "Cannot convert to DataGrid" );
			}
			//Get target DataGrid，If it is empty, an exception will be thrown
			System . Windows . Controls . DataGrid dg = dataGrid as System . Windows . Controls . DataGrid;
			if ( dg == null )
			{
				throw new ArgumentNullException ( "Target none" + dataGrid + "Cannot convert to DataGrid" );
			}
			//If the data source is empty, return
			if ( dg . Items == null || dg . Items . Count < 1 )
			{
				return;
			}

			dg . SelectedItem = selectedItem;
			dg . CurrentColumn = dg . Columns [ 0 ];
			dg . ScrollIntoView ( dg . SelectedItem, dg . CurrentColumn );
		}
		/// <summary>
		/// MASTER UPDATE METHOD
		/// This handles repositioning of a selected item in any grid perfectly
		/// </summary>
		/// <param name="grid"></param>
		/// <param name="row"></param>
		public static void SetUpGridSelection ( DataGrid grid, int row = 0 )
		{
			//			bool inprogress = false;
			int scrollrow = 0;
			if ( row == -1 ) row = 0;
			// This triggers the selection changed event
			grid . SelectedIndex = row;
			grid . SelectedItem = row;
			//			grid . SetDetailsVisibilityForItem ( grid . SelectedItem, Visibility . Visible );
			grid . SelectedIndex = row;
			grid . SelectedItem = row;
			Utils . ScrollRecordIntoView ( grid, row );
			grid . UpdateLayout ( );
			grid . Refresh ( );
			//			var v = grid .VerticalAlignment;
		}

		/// <summary>
		/// Metohd that almost GUARANTESS ot force a record into view in any DataGrid
		/// /// This is called by method above - MASTER Updater Method
		/// </summary>
		/// <param name="dGrid"></param>
		/// <param name="row"></param>
		public static void ScrollRecordInGrid ( DataGrid dGrid, int row )
		{
			if ( dGrid . CurrentItem == null ) return;
			dGrid . UpdateLayout ( );
			dGrid . ScrollIntoView ( dGrid . Items . Count - 1 );
			dGrid . UpdateLayout ( );
			dGrid . ScrollIntoView ( row );
			dGrid . UpdateLayout ( );
			Utils . ScrollRecordIntoView ( dGrid, row );
		}
		public static int FindMatchingRecord ( string Custno, string Bankno, DataGrid Grid, string currentDb = "" )
		{
			int index = 0;
			if ( currentDb == "BANKACCOUNT" )
			{
				foreach ( var item in Grid . Items )
				{
					BankAccountViewModel cvm = item as BankAccountViewModel;
					if ( cvm == null ) break;
					if ( cvm . CustNo == Custno && cvm . BankNo == Bankno )
					{
						break;
					}
					index++;
				}
				if ( index == Grid . Items . Count )
					index = -1;
				return index;
			}
			else if ( currentDb == "CUSTOMER" )
			{
				foreach ( var item in Grid . Items )
				{
					CustomerViewModel cvm = item as CustomerViewModel;
					if ( cvm == null ) break;
					if ( cvm . CustNo == Custno && cvm . BankNo == Bankno )
					{
						break;
					}
					index++;
				}
				if ( index == Grid . Items . Count )
					index = -1;
				return index;
			}
			else if ( currentDb == "DETAILS" )
			{
				foreach ( var item in Grid . Items )
				{
					DetailsViewModel dvm = item as DetailsViewModel;
					if ( dvm == null ) break;
					if ( dvm . CustNo == Custno && dvm . BankNo == Bankno )
					{
						break;
					}
					index++;
				}
				if ( index == Grid . Items . Count )
					index = -1;
				return index;
			}
			return -1;
		}


		public static bool DataGridHasFocus ( DependencyObject instance )
		{
			//how to fibnd out whether a datagrid has focus or not to handle key previewers
			IInputElement focusedControl = FocusManager . GetFocusedElement ( instance );
			if ( focusedControl == null ) return true;
			string compare = focusedControl . ToString ( );
			if ( compare . ToUpper ( ) . Contains ( "DATAGRID" ) )
				return true;
			else
				return false;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Dgrid"></param>
		/// <param name="CurrentRecord"></param>
		public static void ScrollRecordIntoView ( DataGrid Dgrid, int CurrentRecord )
		{
			// Works well 26/5/21
			double currentTop = 0;
			double currentBottom = 0;
			if ( CurrentRecord == -1 ) return;
			if ( Dgrid . Name == "CustomerGrid" || Dgrid . Name == "DataGrid1" )
			{
				currentTop = Flags . TopVisibleBankGridRow;
				currentBottom = Flags . BottomVisibleBankGridRow;
			}
			else if ( Dgrid . Name == "BankGrid" || Dgrid . Name == "DataGrid2" )
			{
				currentTop = Flags . TopVisibleCustGridRow;
				currentBottom = Flags . BottomVisibleCustGridRow;
			}
			else if ( Dgrid . Name == "DetailsGrid" || Dgrid . Name == "DetailsGrid" )
			{
				currentTop = Flags . TopVisibleDetGridRow;
				currentBottom = Flags . BottomVisibleDetGridRow;
			}     // Believe it or not, it takes all this to force a scrollinto view correctly

			if ( Dgrid == null || Dgrid . Items . Count == 0 || Dgrid . SelectedItem == null ) return;

			//update and scroll to bottom first
			Dgrid . SelectedIndex = ( int ) CurrentRecord;
			Dgrid . SelectedItem = ( int ) CurrentRecord;
			Dgrid . UpdateLayout ( );
			Dgrid . ScrollIntoView ( Dgrid . Items . Count - 1 );
			Dgrid . UpdateLayout ( );
			Dgrid . ScrollIntoView ( Dgrid . SelectedItem );
			Dgrid . UpdateLayout ( );
			Flags . CurrentSqlViewer?.SetScrollVariables ( Dgrid );
		}

		//		public NewFlags Flags = new NewFlags();
		//************************************************************************************//
		/// <summary>
		///  checks an Enum in Flags.cs andf appends the correct sort 
		///  order to the SQL command string it receives
		/// </summary>
		/// <param name="commandline"></param>
		/// <returns></returns>
		public static string GetDataSortOrder ( string commandline )
		{
			if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . DEFAULT )
				commandline += "Custno, BankNo";
			else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . ID )
				commandline += "ID";
			else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . BANKNO )
				commandline += "BankNo, CustNo";
			else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . CUSTNO )
				commandline += "CustNo";
			else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . ACTYPE )
				commandline += "AcType";
			else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . DOB )
				commandline += "Dob";
			else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . ODATE )
				commandline += "Odate";
			else if ( Flags . SortOrderRequested == ( int ) Flags . SortOrderEnum . CDATE )
				commandline += "Cdate";
			return commandline;
		}

		//************************************************************************************//
		public static bool CheckForExistingGuid ( Guid guid )
		{
			bool retval = false;
			for ( int x = 0 ; x < Flags . DbSelectorOpen . ViewersList . Items . Count ; x++ )
			{
				ListBoxItem lbi = new ListBoxItem ( );
				//lbi.Tag = viewer.Tag;
				lbi = Flags . DbSelectorOpen . ViewersList . Items [ x ] as ListBoxItem;
				if ( lbi . Tag == null ) return retval;
				Guid g = ( Guid ) lbi . Tag;
				if ( g == guid )
				{
					retval = true;
					break;
				}
			}
			return retval;
		}
		//************************************************************************************//
		public static void GetWindowHandles ( )
		{
#if SHOWWINDOWDATA
			Console . WriteLine ( $"Current Windows\r\n" + "===============" );
			foreach ( Window window in System . Windows . Application . Current . Windows )
			{
				if ( window . Title != "" && window . Content != "" )
				{
					Console . WriteLine ( $"Title:  {window . Title },\r\nContent - {window . Content}" );
					Console . WriteLine ( $"Name = [{window . Name}]\r\n" );
				}
			}
#endif
		}
		public static bool FindWindowFromTitle ( string searchterm, ref Window handle )
		{
			bool result = false;
			foreach ( Window window in System . Windows . Application . Current . Windows )
			{
				if ( window . Title . ToUpper ( ) . Contains ( searchterm . ToUpper ( ) ) )
				{
					handle = window;
					result = true;
					break;
				}
			}
			return result;
		}

		//************************************************************************************//
		public static Style GetDictionaryStyle ( string tempname )
		{
			Style ctmp = System . Windows . Application . Current . FindResource ( tempname ) as Style;
			return ctmp;
		}
		//************************************************************************************//
		//public static Template GetDictionaryTemplate ( string tempname )
		//{
		//	Template ctmp = System . Windows . Application . Current . FindResource ( tempname ) as Template;
		//	return ctmp;
		//}
		//************************************************************************************//
		public static ControlTemplate GetDictionaryControlTemplate ( string tempname )
		{
			ControlTemplate ctmp = System . Windows . Application . Current . FindResource ( tempname ) as ControlTemplate;
			return ctmp;
		}
		//************************************************************************************//
		public static Brush GetDictionaryBrush ( string brushname )
		{
			Brush brs = null;
			try
			{
				brs = System . Windows . Application . Current . FindResource ( brushname ) as Brush;
			}
			catch
			{

			}
			return brs;
		}
		// Utility functions for sensing scrollbars when dragging from a grid etc
		public static bool HitTestScrollBar ( object sender, MouseButtonEventArgs e )
		{
			//			HitTestResult hit = VisualTreeHelper . HitTest ( ( Visual ) sender, e . GetPosition ( ( IInputElement ) sender ) );
			//			return hit . VisualHit . GetVisualAncestor<ScrollBar> ( ) != null;
			object original = e . OriginalSource;

			if ( !original . GetType ( ) . Equals ( typeof ( ScrollBar ) ) )
			{
				if ( original . GetType ( ) . Equals ( typeof ( DataGrid ) ) )
				{
					Console . WriteLine ( "DataGrid is clicked" );
				}
				else if ( FindVisualParent<ScrollBar> ( original as DependencyObject ) != null )
				{
					//scroll bar is clicked
					return true;
				}
				return false; ;
			}
			return true;
		}
		public static bool HitTestHeaderBar ( object sender, MouseButtonEventArgs e )
		{
			//			HitTestResult hit = VisualTreeHelper . HitTest ( ( Visual ) sender, e . GetPosition ( ( IInputElement ) sender ) );
			//			return hit . VisualHit . GetVisualAncestor<ScrollBar> ( ) != null;
			object original = e . OriginalSource;

			if ( !original . GetType ( ) . Equals ( typeof ( DataGridColumnHeader ) ) )
			{
				if ( original . GetType ( ) . Equals ( typeof ( DataGrid ) ) )
				{
					Console . WriteLine ( "DataGrid is clicked" );
				}
				else if ( FindVisualParent<DataGridColumnHeader> ( original as DependencyObject ) != null )
				{
					//Header bar is clicked
					return true;
				}
				return false; ;
			}
			return true;
		}
		public static parentItem FindVisualParent<parentItem> ( DependencyObject obj ) where parentItem : DependencyObject
		{
			DependencyObject parent = VisualTreeHelper . GetParent ( obj );
			while ( parent != null && !parent . GetType ( ) . Equals ( typeof ( parentItem ) ) )
			{
				parent = VisualTreeHelper . GetParent ( parent );
			}
			return parent as parentItem;
		}

		public static string GetPrettyGridStatistics ( DataGrid Grid, int current )
		{
			string output = "";
			if ( current != -1 )
				output = $"{current} / {Grid . Items . Count}";
			else
				output = $"0 / {Grid . Items . Count}";
			return output;
		}
		/// <summary>
		/// Handles the making of any window to be draggable via a simple click/Drag inside them
		/// Very useful method
		/// </summary>
		/// <param name="inst"></param>
		public static void SetupWindowDrag ( Window inst )
		{
			inst . MouseDown += delegate
			{
				try
				{ inst . DragMove ( ); }
				catch { return; }
			};
		}

	}
}
