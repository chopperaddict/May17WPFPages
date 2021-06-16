#define USEDRAGVIEWMODEL
using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . IO;
using System . Linq;
using System . Runtime . CompilerServices;
using System . Runtime . Serialization;
using System . Text;
using System . Threading;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Imaging;
using System . Windows . Shapes;
using WPFPages . ViewModels;
using WPFPages . Views;

namespace WPFPages . Views
{

	// Delegate to allow a selection of optional search locations for the internal Execute file method
	public delegate string QualifyingFileLocations (string filename );
//	public delegate string QualifyingFileLocations ( string [ ] possiblefolders, string searchfilename );

	/// <summary>
	/// Interaction logic for DragDropClient.xaml
	/// </summary>
	/// 
	public class DragviewModel : BankAccountViewModel
	{
		public string RecordType { get; set; }
		public override string ToString ( )
		{
			//-WORKING WELL 13 / 6 / 21
			return RecordType + ", " + CustNo + ", " + BankNo + ", " + AcType . ToString ( ) + ", " + IntRate . ToString ( ) + ", " + Balance . ToString ( ) + ", " + ODate . ToString ( ) + ", " + CDate . ToString ( );
			//return base . ToString ( );
		}

		public DragviewModel ( )
		{
		}
	}
	public partial class DragDropClient : Window
	{
		//-WORKING WELL 13 / 6 / 21
		private DragviewModel bv = new DragviewModel ( );
		private List<DragviewModel> bvm = new List<DragviewModel> ( );
		public bool addCr { get; set; }
		private string Savepath = @"C:\Users\Ianch\Documents\";
		private bool CopyGridToText { get; set; }
		private bool SaveBoth { get; set; }
		private int Selectindex { get; set; }
		private Point _startPoint { get; set; }


		// CONSTRUCTOR
		public DragDropClient ( )
		{
			InitializeComponent ( );
			this . Topmost = false;
			dataGrid . ItemsSource = bvm;
			addCr = true;
			SavePrompt . Visibility = Visibility . Collapsed;
			dataGrid . Visibility = Visibility . Visible;
			textBox . Visibility = Visibility . Visible;
			ExecuteFile . Visibility = Visibility . Collapsed;
			this . MouseDown += delegate { DoDragMove ( ); };
			AddToText . IsChecked = true;
			CopyGridToText = true;
			Flags . DragDropViewer = this;
			OntopChkbox . IsChecked = true;
			this . Topmost = true;
		}


		private void DoDragMove ( )
		{//Handle the button NOT being the left mouse button
		 // which will crash the DragMove Fn.....
			try
			{ this . DragMove ( ); }
			catch { return; }
		}


		private void CancelBtn_Click ( object sender, RoutedEventArgs e )
		{
			SavePrompt . Visibility = Visibility . Collapsed;
			dataGrid . Visibility = Visibility . Visible;
			textBox . Visibility = Visibility . Visible;
		}

		private void DataGrid_PreviewDrop ( object sender, DragEventArgs e )
		{
			//			char ch = ',';
			string dataString = "";

			// Works for all 3 types of data 13/6/21
			if ( e . Data . GetDataPresent ( "DETAILS" ) )
			{
				DetailsViewModel dvm = new DetailsViewModel ( );
				dvm = ( DetailsViewModel ) e . Data . GetData ( "DETAILS" );
				DragviewModel ddc = new DragviewModel ( );
				// Massage data from Details record to Drag record
				ddc . RecordType = "DETAILS";
				ddc . CustNo = dvm . CustNo;
				ddc . BankNo = dvm . BankNo;
				ddc . AcType = dvm . AcType;
				ddc . IntRate = dvm . IntRate;
				ddc . Balance = dvm . Balance;
				ddc . ODate = dvm . ODate;
				ddc . CDate = dvm . CDate;

				dataGrid . ItemsSource = null;
				bvm . Add ( ddc );
				dataGrid . ItemsSource = bvm;
				// position on newly added record !!
				dataGrid . SelectedIndex = dataGrid . Items . Count - 1;
				Utils . ScrollRecordIntoView ( dataGrid, dataGrid . SelectedIndex );
				dataGrid . SelectedItem = dataGrid . SelectedIndex;
				dataGrid . Refresh ( );

				if ( CopyGridToText )
				{
					dataString = Utils . CreateFullCsvTextFromRecord ( null, dvm, null, true );
					textBox . Text += dataString + "\n";
					int textlen = textBox . Text . Length;
					textBox . CaretIndex = textlen - dataString . Length - 1;
					int line = textBox . LineCount - 1;
					//					Thread . Sleep ( 350 );
					textBox . ScrollToLine ( line );
					textBox . SelectionLength = dataString . Length;
					textBox . Refresh ( );
					// This ADDS a duplicate to the original dragged DataObject ??
					//e . Data. SetData ( "DETAILS", dvm, true);
					var result = e . Data . GetDataPresent ( "", true );
					result = e . Data . GetDataPresent ( "FileDrop", true );
					dataGrid . Focus ( );
				}
				e . Handled = true;
			}
			else if ( e . Data . GetDataPresent ( DataFormats . StringFormat ) )
			{
				//				int Elements = 0;
				string [ ] items;
				// Yes, so check what it is by verifying the Flag we put inside it in the Drag creation
				dataString = ( string ) e . Data . GetData ( DataFormats . StringFormat );
				// our datagrid ONLY accepts BANK view model data, so we massage the received data into that (BANK) format
				int currsel = 0;
				// Get Details record that has been dblclicked on from TOPGRID
				items = dataString . Split ( ',' );
#if USEDRAGVIEWMODEL
				bv = Utils . CreateGridRecordFromString ( dataString );
#else
if ( dataString . Contains ( "CUSTOMER" ) )
					bv = Utils . CreateBankRecordFromString ( "CUSTOMER", dataString );
				else if ( dataString . Contains ( "BANK" ) )
					bv = Utils . CreateBankRecordFromString ( "BANK", dataString );
				else if ( dataString . Contains ( "DETAILS" ) )
					bv = Utils . CreateBankRecordFromString ( "DETAILS", dataString );

#endif
				currsel = dataGrid . SelectedIndex;
				dataGrid . ItemsSource = null;
				bvm . Add ( bv );
				dataGrid . ItemsSource = bvm;
				// position on newly added record !!
				dataGrid . SelectedIndex = dataGrid . Items . Count - 1;
				Utils . ScrollRecordIntoView ( dataGrid, dataGrid . SelectedIndex );
				dataGrid . SelectedItem = dataGrid . SelectedIndex;
				dataGrid . Refresh ( );
				if ( CopyGridToText )
				{
					if ( addCr )
						textBox . Text += dataString + "\n";
					else
						textBox . Text += dataString;
					int textlen = textBox . Text . Length;
					textBox . CaretIndex = textlen - dataString . Length - 1;
					int line = textBox . LineCount - 1;
					textBox . ScrollToLine ( line );
					textBox . SelectionLength = dataString . Length;
					textBox . Refresh ( );
					dataGrid . Focus ( );
				}
				e . Handled = true;
			}
			else
			{
				Console . Beep ( 600, 300 );
				MessageBox . Show ( $"Data dropped onto this grid is not in the correct format !\nYou can drop various data types on the Text Box below....", "Invalid Data format" );
				e . Handled = true;
			}
		}


		/// <summary>
		/// This is a TextBox that accepts dropped data and handles adding newlines as required
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Textbox_Drop ( object sender, DragEventArgs e )
		{
			// This is  the method that actually adds the Dropped data to the TextBox
			int Elements = 0;
			string [ ] items;
			char ch = ',';
			string dataString = "";

			if ( e . Data . GetDataPresent ( "DETAILS" ) )
			{
				DetailsViewModel dvm = new DetailsViewModel ( );
				dvm = ( DetailsViewModel ) e . Data . GetData ( "DETAILS" );
				dataString = Utils . CreateFullCsvTextFromRecord ( null, dvm, null, true );
				if ( addCr )
					textBox . Text += dataString + "\n";
				else
					textBox . Text += dataString;
				int textlen = textBox . Text . Length;
				textBox . CaretIndex = textlen - dataString . Length - 1;
				int line = textBox . LineCount - 1;
				textBox . ScrollToLine ( line );
				textBox . SelectionLength = dataString . Length - 1;
				dataGrid . Focus ( );
				textBox . Refresh ( );
				e . Handled = true;
			}
			else if ( e . Data . GetDataPresent ( DataFormats . StringFormat ) )
			{
				// get the data sent in  the drag message so we can verify what it is 
				dataString = ( string ) e . Data . GetData ( DataFormats . StringFormat );
				if ( addCr )
					textBox . Text += dataString + "\n";
				else
					textBox . Text += dataString;
				int textlen = textBox . Text . Length;
				textBox . CaretIndex = textlen;
				textBox . Focus ( );
				e . Handled = true;
			}
			else if ( e . Data . GetDataPresent ( DataFormats . Text ) )
			{
				// get the data sent in  the drag message so we can verify what it is 
				dataString = ( string ) e . Data . GetData ( DataFormats . Serializable );
				textBox . Text += dataString;
				int textlen = textBox . Text . Length;
				textBox . CaretIndex = textlen;
				textBox . Focus ( );
				e . Handled = true;
			}
			else
			{
				//				dropped on textbox 15/6/21 OK
				ListPasteOtherformats ( e );
				// Reset flag as We have now got the dragged data
				e . Handled = true;
			}
			e . Handled = true;
		}
		private void AddCr_Click ( object sender, RoutedEventArgs e )
		{
			if ( AddCr . IsChecked == true )
				addCr = true;
			else
				addCr = false;
		}

		private void SaveBoth_Click ( object sender, RoutedEventArgs e )
		{
			// SaveCombined (3) button 
			// Save Grid and Text in a single compound file
			//-WORKING WELL 13 / 6 / 21
			if ( CombinedSaveName . Text . Contains ( "Enter Name for" ) || CombinedSaveName . Text == "" )
			{
				MessageBox . Show ( "You must enter a name for the Combined data to be saved !", "Save dragged data Utility" );
				return;
			}
			SaveAll_Click ( sender, e );
			//SaveBoth = true;
			//SaveText_Click ( sender, e );
			//SaveGrid_Click ( sender, e );
			//SaveBoth = false;
		}

		private void SaveData_Click ( object sender, RoutedEventArgs e )
		{
			// Main Save menu option , just displays the file name entry panel (Canvas)
			dataGrid . Visibility = Visibility . Collapsed;
			textBox . Visibility = Visibility . Collapsed;
			promptmessage . Visibility = Visibility . Collapsed;

			SavePrompt . Visibility = Visibility . Visible;
			CombinedSaveName . Text = "Enter Name for the saved combined Data...";
			SavePrompt . BringIntoView ( );
			CombinedSaveName . Focus ( );
		}
		private void SaveAll_Click ( object sender, RoutedEventArgs e )
		{
			string path = "";
			if ( CombinedSaveName . Text . Contains ( "Enter Name for" ) || CombinedSaveName . Text == "" )
			{
				MessageBox . Show ( "You must enter a name for the combined data to be saved to!", "Save dragged data Utility" );
				return;
			}
			// Save grid data here ->>
			string output = "";
			DragviewModel bvm = new DragviewModel ( );
			foreach ( var item in dataGrid . Items )
			{
				bvm = item as DragviewModel;
				output += Utils . CreateDragDataFromRecord ( bvm );
			}
			if ( CombinedSaveName . Text . ToUpper ( ) . Contains ( ".CSV" ) == false )
				path = Savepath + CombinedSaveName . Text + ".DATA.CSV";
			else if ( CombinedSaveName . Text . Contains ( "." ) )
			{
				char ch = '.';
				string [ ] data = CombinedSaveName . Text . Split ( ch );
				string tmp = data [ 0 ] + ".ALLDATA.CSV";
				path = tmp;
			}
			else
				path = Savepath + CombinedSaveName . Text;
			output += "\n$$$\n";
			output += textBox . Text;
			try
			{
				//Save the data to disk file
				File . WriteAllText ( path, output );
				MessageBox . Show ( $"The file [{path}] \nhas been saved successfully", "File Save" );
				SavePrompt . Visibility = Visibility . Collapsed;
				dataGrid . Visibility = Visibility . Visible;
				textBox . Visibility = Visibility . Visible;
				promptmessage . Visibility = Visibility . Visible;
			}
			catch {
				MessageBox . Show ( $"The file [{path}] \nas entered does not appear to be valid.\nPlease correct chack and correct it & try again", "File Save Error" );
			}
	//		e . Handled = true;
		}
		private void PreviewKeyDownCombo ( object sender, KeyEventArgs e )
		{
			if ( e . Key == Key . Enter )
			{
				SaveAll_Click ( sender, null );
				e . Handled = true;
			}
			else if ( e . Key == Key . Escape )
			{
				CancelBtn_Click ( sender, null );
				e . Handled = true;
			}
			else
			{
				// Clear Text name fields as user  types into it
				if ( CombinedSaveName . Text . Contains ( "Enter Name for " ) )
					CombinedSaveName . Text = "";
				e . Handled = false;
			}
		}
		private void CloseBtn_Click ( object sender, RoutedEventArgs e )
		{
			//-WORKING WELL 13 / 6 / 21
			Flags . DragDropViewer = null;
			Close ( );
		}
		private void AddToText_Click ( object sender, RoutedEventArgs e )
		{
			//-WORKING WELL 13 / 6 / 21
			if ( AddToText . IsChecked == true )
				CopyGridToText = true;
			else
				CopyGridToText = false;
		}
		public void RemoteLoadGrid ( )
		{
			//-WORKING WELL 13 / 6 / 21
			ReloadText_Click ( null, null );
		}
		private void ReloadText_Click ( object sender, RoutedEventArgs e )
		{
			//-WORKING WELL 13 / 6 / 21
			// Reload data from CSV (Text output format)
			// Handles BAD text in the text fields
			bool iscombo = false;
			string [ ] data;
			int index = 1;
			string input = "";
			char ch;
			string [ ] splitdata = { };
			string path = Utils . GetImportFileName ( ".CSV" );
			if ( path . Length == 0 ) return;
			input = File . ReadAllText ( path );
			try
			{
				//1st check - Is it a combined data source file
				if ( input . Contains ( "\n$$$\n" ) )
				{
					string [ ] tempgriddata;
					// got a combo file  Grid 1st with no \n then delim text with \n between lines
					splitdata = input . Split ( '$' );
					string griddata = splitdata [ 0 ];
					textBox . Text = splitdata [ 3 ] . Substring ( 1 );
					input = splitdata [ 0 ];
					int textlen = textBox . Text . Length;
					textBox . CaretIndex = textlen;
					textBox . Refresh ( );
					iscombo = true;
					input = splitdata [ 0 ];
					//data = griddata;


					ch = ',';
					data = input . Split ( ch );
					index = 0;
					bvm . Clear ( );
					while ( true )
					{
						DragviewModel bv = new DragviewModel ( );
						if ( index >= data . Length - 1 ) break;
						bv . RecordType = data [ index++ ];
						bv . Id = int . Parse ( data [ index++ ] );
						bv . CustNo = data [ index++ ];
						bv . BankNo = data [ index++ ];
						bv . AcType = int . Parse ( data [ index++ ] );
						bv . IntRate = decimal . Parse ( data [ index++ ] );
						bv . Balance = decimal . Parse ( data [ index++ ] );
						// gotta remove the quote marks
						string tmp1 = data [ index++ ] . ToString ( );
						string tmp2 = tmp1 . Substring ( 1 );
						char ch2 = '\'';
						string [ ] tmp3;
						tmp3 = tmp2 . Split ( ch2 );
						bv . ODate = Convert . ToDateTime ( tmp3 [ 0 ] );

						tmp1 = data [ index++ ] . ToString ( );
						tmp2 = tmp1 . Substring ( 1 );
						tmp3 = tmp2 . Split ( ch2 );
						bv . CDate = Convert . ToDateTime ( tmp3 [ 0 ] );
						bvm . Add ( bv );

					}
					dataGrid . ItemsSource = null;
					dataGrid . ItemsSource = bvm;
					dataGrid . SelectedIndex = 0;
					dataGrid . SelectedItem = 0;
					dataGrid . CurrentItem = 0;
					Utils . ScrollRecordIntoView ( dataGrid, 0 );
				}
				else
				{
					MessageBox . Show ( $"Error encountered loading data from [{path}]\nPlease check  this data file for possible corruption!\nor the file may NOT be in  the correct format for this application", "Incompatible Data Identified" );
				}
			}
			catch ( Exception ex )
			{
				MessageBox . Show ( $"Error encountered loading data from [{path}]\nPlease check  this data file for possible corruption!\nor the file may NOT be in  the correct format for this application", "Incompatible Data Identified" );
			}
		}

		private void ClearGrid_Click ( object sender, RoutedEventArgs e )
		{
			//-WORKING WELL 13 / 6 / 21
			dataGrid . ItemsSource = null;
			dataGrid . Items . Clear ( );
			bvm . Clear ( );
		}

		private void ClearText_Click ( object sender, RoutedEventArgs e )
		{
			//-WORKING WELL 13 / 6 / 21
			textBox . Text = "";
			textBox . Refresh ( );
		}

		private void ShowToString_Click ( object sender, RoutedEventArgs e )
		{
			//-WORKING WELL 13 / 6 / 21
			string output = "";
			string input;
			char [ ] ch = { ',' };
			if ( dataGrid . SelectedItem == null ) return;
			input = dataGrid . SelectedItem . ToString ( );
			string [ ] str = input . Split ( ch );
			foreach ( var item in str )
			{
				output += item + "\n		";
			}
			MessageBox . Show ( output );
		}

		private void OntopChkbox_Click ( object sender, RoutedEventArgs e )
		{
			if ( OntopChkbox . IsChecked == true )
				this . Topmost = true;
			else
				this . Topmost = false;

		}

		private void TextBox_PreviewDragOver ( object sender, DragEventArgs e )
		{
			//if ( !e . Data . GetDataPresent ( "DETAILS" ) &&
			//	!e . Data . GetDataPresent ( "StringFormat" ) )
			//{
			//	e . Effects = DragDropEffects . None;
			//}
			//else
			e . Effects = DragDropEffects . Copy;
			//this line is CRITICAL - witohut it the TextBo does  not recognize a drop in progress
			e . Handled = true;
		}

		private void DataGrid_ColumnReordered ( object sender, DataGridColumnEventArgs e )
		{
			DragviewModel dvm = new DragviewModel ( );
			dvm = dataGrid . SelectedItem as DragviewModel;
			string custno = dvm . CustNo;
			string bankno = dvm . BankNo;
			int indx = Utils . FindMatchingRecord ( custno, bankno, dataGrid );
			Utils . ScrollRecordIntoView ( dataGrid, indx );
		}

		private void DataGrid_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			Selectindex = dataGrid . SelectedIndex;
		}
		/// <summary>
		/// sets cursor to a hand if dragged over the dataGrid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void textBox_GiveFeedback ( object sender, GiveFeedbackEventArgs e )
		{
			if ( e . Effects == DragDropEffects . Copy )
			{
				e . UseDefaultCursors = false;
				Mouse . SetCursor ( Cursors . Hand );
			}
			else
				e . UseDefaultCursors = true;

			e . Handled = true;
		}

		private void TextBox_DragEnter ( object sender, DragEventArgs e )
		{
			if ( !e . Data . GetDataPresent ( "DETAILS" ) ||
				!e . Data . GetDataPresent ( "StringFormat" ) ||
				sender == e . Source )
			{
				e . Effects = DragDropEffects . None;
			}
			else
				e . Effects = DragDropEffects . Copy;
			e . Handled = true;
		}
		/// <summary>
		/// Method to output the 
		/// format of any dropped data in this window, including adding txt file contents to textBox
		/// otherewise the file name dragged is listed in the same TextBox
		/// </summary>
		/// <param name="e"></param>
		public void ListPasteOtherformats ( DragEventArgs e )
		{
			try
			{
				if ( e . Data . GetDataPresent ( DataFormats . StringFormat ) )
				{
					Debug . WriteLine ( $"Received a dragged STRINGFORMAT record..." );
					string filenames = ( string ) e . Data . GetData ( DataFormats . StringFormat );
					textBox . Text += filenames + "\n";
				}
				if ( e . Data . GetData ( DataFormats . Serializable ) != null )
				{
					Debug . WriteLine ( $"Received a dragged SERIALIZABLE record..." );
					string filenames = ( string ) e . Data . GetData ( DataFormats . Serializable );
					textBox . Text += filenames + "\n";
				}
				if ( e . Data . GetData ( DataFormats . CommaSeparatedValue ) != null )
				{
					Debug . WriteLine ( $"Received a dragged COMMASEPARATEDVALUE record..." );
					string filenames = ( string ) e . Data . GetData ( DataFormats . StringFormat );
					textBox . Text += filenames + "\n";
				}

				if ( e . Data . GetData ( DataFormats . FileDrop ) != null )
				{
					Debug . WriteLine ( $"Received a dragged FILEDROP record..." );
					string [ ] filenames = ( string [ ] ) e . Data . GetData ( DataFormats . FileDrop );

					List<string> files = new List<string> ( );
					files . Add ( filenames [ 0 ] );
					if ( files [ 0 ] . ToUpper ( ) . Contains ( ".TXT" ) )
					{
						var v = File . ReadAllText ( filenames [ 0 ] );
						textBox . Text += v + "\n";
					}
					else
						textBox . Text += files [ 0 ] + "\n";
				}
				if ( e . Data . GetData ( DataFormats . SymbolicLink ) != null )
				{
					Debug . WriteLine ( $"Received a dragged SYMBOLICLINK record..." );
					string filenames = ( string ) e . Data . GetData ( DataFormats . SymbolicLink );
					textBox . Text += filenames + "\n";
				}
				if ( e . Data . GetData ( DataFormats . Rtf ) != null )
				{
					Debug . WriteLine ( $"Received a dragged RTF record..." );
					string filenames = ( string ) e . Data . GetData ( DataFormats . Rtf );
					textBox . Text += filenames + "\n";
				}
				int textlen = textBox . Text . Length;
				textBox . CaretIndex = textlen;
				int line = textBox . LineCount - 1;
				textBox . ScrollToLine ( line );
				textBox . SelectionLength = textlen;
				textBox . Refresh ( );
				textBox . Focus ( );
				//if ( e . Data . GetData ( "DETAILS" ) != null )
				//{
				//	Debug . WriteLine ( $"Received a dragged DETAILS record..." );
				//	string filenames = ( string ) e . Data . GetData ( DataFormats . StringFormat );
				//	textBox . Text += filenames + "\n";
				//}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Parse error unpcking dragged data Format :-\n{ex . Message}, {ex . Data}" );
			}
		}

		private void TextBox_IsMouseCaptureWithinChanged ( object sender, DependencyPropertyChangedEventArgs e )
		{
			//int x = textBox . CaretIndex;
			//var  line = textBox.GetLineIndexFromCharacterIndex(x);
			//var linestring = textBox . GetLineLength ( line);
			//textBox . CaretIndex -= x;
			//// This gets me the complete line, including \n
			//var txt = textBox . GetLineText ( line);
			//var commd = txt . Split ( '\n' );
			//var commandstring = commd [ 0 ];
			////		File . Open ( commd[0], FileMode .Open );
			//Process ExternalProcess = new Process ( );
			//ExternalProcess . StartInfo . FileName = commandstring;
			//if ( commandstring . Length == 0 ) return;
			//ExternalProcess . StartInfo . WindowStyle = ProcessWindowStyle .Normal;
			//ExternalProcess . EnableRaisingEvents = false;
			//try
			//{
			//	ExternalProcess . Start ( );
			//}
			//catch(Exception ex )
			//{
			//	Debug . WriteLine ($"ExternalProcess error : {ex.Message}, {ex.Data}");
			//}
			//finally
			//{
			//	ExternalProcess . Close ( );
			//}
			//textBox.IsMou
		}

		private void TextBox_MouseDoubleClick ( object sender, MouseButtonEventArgs e )
		{
			SupportMethods. ProcessExecuteRequest ( sender, e, textBox );
//			Utils . ProcessExecuteRequest ( this,null,null, "msconfig.exe");	// test line for a command line entry
		}
		#region AUTO PATH SEARCH METHODS

		#endregion AUTO PATH SEARCH METHODS

		private void SavePrompt_RequestBringIntoView ( object sender, RequestBringIntoViewEventArgs e )
		{
			//SavePrompt . BringIntoView (  );
			//SavePrompt . Focus ( );
		}

		private void textBox_TextChanged ( object sender, TextChangedEventArgs e )
		{

		}

		private void ClearAll_Click ( object sender, RoutedEventArgs e )
		{
			//-WORKING WELL 13 / 6 / 21
			dataGrid . ItemsSource = null;
			dataGrid . Items . Clear ( );
			textBox . Text = "";
			textBox . Refresh ( );
			bvm . Clear ( );
			dataGrid . Refresh ( );
		}

		private void searchpaths_Click ( object sender, RoutedEventArgs e )
		{
			// modify paths used by (QualifyingFileLocations ) delegate
			RunSearchPaths rsp = new RunSearchPaths (  );
			rsp.ShowDialog();
		}

		private void Execute_Click ( object sender, RoutedEventArgs e )
		{
			ExecuteFile . Visibility = Visibility . Visible;
			//Trying to disablespellchecker - doesn't work though
			SpellCheck sc = execName . SpellCheck;
			sc = null;
			ExecuteFile . BringIntoView ( );
			execName . Focus ( );

		}

		private void Exec_Click ( object sender, RoutedEventArgs e )
		{
			SupportMethods . ProcessExecuteRequest ( this, null, null, execName.Text );
		}

		private void scratch_Click ( object sender, RoutedEventArgs e )
		{
			ExecuteFile . Visibility = Visibility . Collapsed;
		}
	}
}


