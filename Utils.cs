#define SHOWWINDOWDATA
using System;
using System . Runtime . InteropServices . WindowsRuntime;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Forms . VisualStyles;
using System . Windows . Input;
using System . Windows . Media;
using WPFPages . ViewModels;
using WPFPages . Views;

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
		//public static Task DoSingleBeep ( int freq = 280, int count = 300, int repeat = 1 )
		//{
		//	int x = 0;
		//	Task t = new Task ( ( ) => x = 1 );
		//	if ( Flags . UseBeeps )
		//	{
		//		for ( int i = 0 ; i < repeat ; i++ )
		//		{
		//			t = Task . Factory . StartNew ( ( ) => Console . Beep ( freq, count ) );
		//			Thread . Sleep ( 300 );
		//		}

		//		Thread . Sleep ( 500 );
		//	}
		//	else
		//		t = Task . Factory . StartNew ( ( ) => Console . WriteLine ( ) );
		//	return t;
		//}
		//public static Task DoErrorBeep ( int freq = 280, int count = 100, int repeat = 3 )
		//{
		//	int x = 0;
		//	Task t = new Task ( ( ) => x = 1 );
		//	if ( Flags . UseBeeps )
		//	{
		//		for ( int i = 0 ; i < repeat ; i++ )
		//		{
		//			t = Task . Factory . StartNew ( ( ) => Console . Beep ( freq, count ) );
		//		}

		//		Thread . Sleep ( 500 );
		//	}
		//	else
		//		t = Task . Factory . StartNew ( ( ) => Console . WriteLine() );
		//	return t;
		//}

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

		/// <summary>
		/// MASTER UPDATE METHOD
		/// This handles repositioning of a selected item in any grid perfectly
		/// </summary>
		/// <param name="grid"></param>
		/// <param name="row"></param>
		public static void SetUpGridSelection ( DataGrid grid, int row = -1 )
		{
			if ( row == -1 ) row = 0;
			// This triggers the selection changed event
			grid . SelectedIndex = row;
			grid . SelectedItem = row;
			grid . Refresh ( );
			grid . UpdateLayout ( );
			Utils . ScrollRecordIntoView ( grid, row );
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
		public static int FindMatchingRecord ( string Custno, string Bankno, DataGrid Grid, string CurrentDb )
		{
			int index = 0;
			if ( CurrentDb == "BANKACCOUNT" )
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
			}
			else if ( CurrentDb == "CUSTOMER" )
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
			}
			else if ( CurrentDb == "DETAILS" )
			{
				foreach ( var item in Grid . Items )
				{
					DetailsViewModel cvm = item as DetailsViewModel;
					if ( cvm == null ) break;
					if ( cvm . CustNo == Custno && cvm . BankNo == Bankno )
					{
						break;
					}
					index++;
				}
			}
			return index;
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
		public static void ScrollRecordIntoView ( DataGrid Dgrid, int CurrentRecord )
		{
			double currentTop = 0;
			double currentBottom = 0;
			double currsel = 0;
			double offset = 0;
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

			//			if ( Dgrid . SelectedItem == null ) return;
			//			Dgrid . SelectedIndex = Dgrid.SelectedIndex + (int)offset;
			//update and scroll to bottom first
			Dgrid . SelectedIndex = ( int ) CurrentRecord;
			Dgrid . SelectedItem = ( int ) CurrentRecord;
			Dgrid . UpdateLayout ( );
			Dgrid . ScrollIntoView ( Dgrid . Items . Count - 1 );
			Dgrid . UpdateLayout ( );
			Dgrid . ScrollIntoView ( Dgrid . SelectedItem );
			Dgrid . UpdateLayout ( );
			Dgrid . SelectedIndex = ( int ) CurrentRecord;
			//			if ( caller == 0 )
			Flags . CurrentSqlViewer?.SetScrollVariables ( Dgrid );

			//			Flags . TopVisibleDetGridRow = currentTop;
			//			Flags . BottomVisibleDetGridRow = currentBottom;
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
			Brush brs = System . Windows . Application . Current . FindResource ( brushname ) as Brush;
			return brs;
		}

	}
}
