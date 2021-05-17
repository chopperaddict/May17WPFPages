#undef SHOWWINDOWDATA
using System;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Input;
using System . Windows . Media;

namespace WPFPages
{
	/// <summary>
	/// Class to handle various utility functions such as fetching 
	/// Style/Templates/Brushes etc to Set/Reset control styles 
	/// from various Dictionary sources for use in "code behind"
	/// </summary>
	public class Utils
	{
		public static bool DataGridHasFocus ( DependencyObject instance )
		{
			//how to fibnd out whether a datagrid has focus or not to handle key previewers
			IInputElement focusedControl = FocusManager.GetFocusedElement(instance);
			if( focusedControl  == null)	return true;
				string compare = focusedControl.ToString();
			if ( compare . ToUpper ( ) . Contains ( "DATAGRID" ) )
				return true;
			else
				return false;
		}
		public static void ScrollRecordIntoView ( DataGrid Dgrid , int caller )
		{
			double currentTop = 0;
			double currentBottom= 0;
			double currsel = 0;
			double offset = 0;
			if ( Dgrid . Name == "CustomerGrid" )
			{
				currentTop = Flags . TopVisibleDetGridRow;
				currentBottom = Flags . BottomVisibleDetGridRow;
			}
			else if ( Dgrid . Name == "BankGrid" )
			{
				currentTop = Flags . TopVisibleDetGridRow;
				currentBottom = Flags . BottomVisibleDetGridRow;
			}
			else if ( Dgrid . Name == "DetailsGrid" )
			{
				currentTop = Flags . TopVisibleDetGridRow;
				currentBottom = Flags . BottomVisibleDetGridRow;
			}     // Believe it or not, it takes all this to force a scrollinto view correctly

			currsel = Dgrid . SelectedIndex;

			if ( Dgrid == null || Dgrid . Items . Count == 0 || Dgrid . SelectedItem == null ) return;

			//			if ( Dgrid . SelectedItem == null ) return;
			//			Dgrid . SelectedIndex = Dgrid.SelectedIndex + (int)offset;
			//update and scroll to bottom first
			Dgrid . UpdateLayout ( );
			Dgrid . ScrollIntoView ( Dgrid . Items . Count - 1 );
			Dgrid . UpdateLayout ( );
			Dgrid . ScrollIntoView ( Dgrid . SelectedItem );
			Dgrid . UpdateLayout ( );
			Dgrid . SelectedIndex = ( int ) currsel;
			if ( caller == 0 )
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
			Console.WriteLine ($"Current Windows\r\n"+"===============");
			foreach (Window window in System.Windows.Application.Current.Windows)
			{
				if (window.Title != "" && window.Content != "")
				{
					Console.WriteLine ($"Title:  {window.Title },\r\nContent - {window.Content}");
					Console.WriteLine ($"Name = [{window.Name}]\r\n");
				}
			}
#endif
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
