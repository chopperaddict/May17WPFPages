#define USEDRAGVIEWMODEL
using System;
using System . Collections . Generic;
using System . IO;
using System . Linq;
using System . Runtime . CompilerServices;
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

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for DragDropClient.xaml
	/// </summary>
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
		//		private BankAccountViewModel bv = new BankAccountViewModel ( );
		//		private List<BankAccountViewModel> bvm = new List<BankAccountViewModel> ( );
		public bool addCr { get; set; }
		private string Savepath = @"C:\Users\Ianch\Documents\";
		private bool CopyGridToText { get; set; }
		public DragDropClient ( )
		{
			InitializeComponent ( );
			this . Topmost = false;
			dataGrid . ItemsSource = bvm;
			addCr = true;
			SavePrompt . Visibility = Visibility . Collapsed;
			this . MouseDown += delegate { DoDragMove ( ); };
			//			textBox . MouseDown += delegate { DoDragMove ( ); };
			//			SavePrompt . MouseDown += delegate { DoDragMove ( ); };
			AddToText . IsChecked = true;
			CopyGridToText = true;
			Flags . DragDropViewer = this;
			//AddCr. MouseDown += delegate { DoDragMove ( ); };
			//CancelBtn . MouseDown += delegate { DoDragMove ( ); };
			//SaveTextBtn . MouseDown += delegate { DoDragMove ( ); };
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
		}

		/// <summary>
		/// Set the cursor as relevant for data to be dropped on this control
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TextBox_PreviewDragEnter ( object sender, DragEventArgs e )
		{
			e . Effects = ( DragDropEffects ) DragDropEffects . Move;
		}

		/// <summary>
		/// This is a TextBox that accepts dropped data and handles adding newlines as required
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Textbox_PreviewDrop ( object sender, DragEventArgs e )
		{
			// This is  the method that actually adds the Dropped data to the TextBox

			if ( e . Data . GetDataPresent ( DataFormats . StringFormat ) )
			{
				// get the data sent in  the drag message so we can verify what it is 
				string dataString = ( string ) e . Data . GetData ( DataFormats . StringFormat );
				if ( addCr )
					textBox . Text += dataString + "\n";
				else
					textBox . Text += dataString;
				int textlen = textBox . Text . Length;
				textBox . CaretIndex = textlen;
				textBox . Focus ( );
				e . Handled = true;
			}
		}

		private void DataGrid_PreviewDrop ( object sender, DragEventArgs e )
		{
			int Elements = 0;
			string [ ] items;
			char ch = ',';
			string dataString = "";

			// Works for all 3 types of data 13/6/21
			if ( e . Data . GetDataPresent ( DataFormats . StringFormat ) )
			{
				// Yes, so check what it is by verifying the Flag we put inside it in the Drag creation
				dataString = ( string ) e . Data . GetData ( DataFormats . StringFormat );
				// Reset flag as We have now got the dragged data
				e . Handled = true;

				// our datagrid ONLY accepts BANK view model data, so we massage the received data into that (BANK) format
				int currsel = 0;
				// Get Details record that has been dblclicked on from TOPGRID
				items = dataString . Split ( ch );
				Elements = items . Length;
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
					//					Thread . Sleep ( 350 );
					textBox . ScrollToLine ( line );
					textBox . SelectionLength = dataString . Length;
					textBox . Refresh ( );
					dataGrid . Focus ( );
				}
			}
		}
		private void AddCr_Click ( object sender, RoutedEventArgs e )
		{
			if ( AddCr . IsChecked == true )
				addCr = true;
			else
				addCr = false;
		}

		private void SaveGrid_Click ( object sender, RoutedEventArgs e )
		{
			// Working 13/6/21
			string path = "";
			if ( GridSaveName . Text . Contains ( " Enter Name for" ) || GridSaveName . Text == "" )
			{
				MessageBox . Show ( "You must enter a name for the Grid data to be saved !", "Save dragged data Utility" );
				return;
			}
			// Save grid data here ->>
			string output = "";
			BankAccountViewModel bvm = new BankAccountViewModel ( );
			foreach ( var item in dataGrid . Items )
			{
				bvm = item as BankAccountViewModel;
				output += Utils . CreateFullCsvTextFromRecord ( bvm, null, null, true );
			}
			if ( GridSaveName . Text . Length > 0 )
			{
				if ( GridSaveName . Text . ToUpper ( ) . Contains ( ".CSV" ) == false )
					path = Savepath + GridSaveName . Text + ".CSV";
				else
					path = Savepath + GridSaveName . Text;
				File . WriteAllText ( path, output );
				MessageBox . Show ( $"The file [{path}] \nhas been saved successfully", "File Save" );
			}
			else
			{
				MessageBox . Show ( "The file name does not appear to be correctly formatted ?", "File Save Error" );
				return;
			}
			return;
		}

		private void SaveData ( object sender, RoutedEventArgs e )
		{
			//-WORKING WELL 13 / 6 / 21
			SavePrompt . Visibility = Visibility . Visible;
			SavePrompt . BringIntoView ( );
		}

		private void SaveData_Click ( object sender, RoutedEventArgs e )
		{
			// Main Save button on main window, displays the data entry panel (Canvas)
			SavePrompt . Visibility = Visibility . Visible;
			SavePrompt . BringIntoView ( );
			GridSaveName . Focus ( );
		}
		private void SaveAll_Click ( object sender, RoutedEventArgs e )
		{
			// Display the file name "dialog" panel (SavePrompt)
			dataGrid . Visibility = Visibility . Collapsed;
			textBox . Visibility = Visibility . Collapsed;
			SavePrompt . Visibility = Visibility . Visible;
			SavePrompt . BringIntoView ( );
		}

		private void SaveText_Click ( object sender, RoutedEventArgs e )
		{
			// Working 13/6/21
			if ( TextSaveName . Text . Contains ( "Enter Name for" ) || TextSaveName . Text == "" )
			{

				MessageBox . Show ( "You must enter a name for the Text data to be saved !", "Save dragged data Utility" );
				return;
			}
			// Save grid data here ->>
			// Save grid data here ->>
			string output = textBox . Text;
			if ( output . Length > 0 )
			{
				string path = "";
				if ( TextSaveName . Text . ToUpper ( ) . Contains ( ".CSV" ) == false )
					path = Savepath + TextSaveName . Text + ".CSV";
				else
					path = Savepath + TextSaveName . Text;

				File . WriteAllText ( path, output );
				MessageBox . Show ( $"The file [{path}]\n has been saved successfully", "File Save" );

			}
			else
			{
				MessageBox . Show ( "The file name does not appear to be correctly formatted\nor the Text field is empty ?", "File Save Error" );

				return;
			}
			SavePrompt . Visibility = Visibility . Collapsed;
			dataGrid . Visibility = Visibility . Visible;
			textBox . Visibility = Visibility . Visible;
			return;
		}

		private void PreviewKeyDownText ( object sender, KeyEventArgs e )
		{
			// Clear Text name fields as user  types into it
			if ( TextSaveName . Text . Contains ( "Enter Name for the saved Text Data..." ) )
				TextSaveName . Text = "";

		}
		private void PreviewKeyDownGrid ( object sender, KeyEventArgs e )
		{
			// Clear Grid name fields as user  types into it
			if ( GridSaveName . Text . Contains ( "Enter Name for the saved Grid Data..." ) )
				GridSaveName . Text = "";
		}
		private void CloseBtn_Click ( object sender, RoutedEventArgs e )
		{
			//-WORKING WELL 13 / 6 / 21
			Flags . DragDropViewer = null;
			Close ( );
		}
		private void SaveBoth_Click ( object sender, RoutedEventArgs e )
		{
			//-WORKING WELL 13 / 6 / 21
			SaveText_Click ( sender, e );
			SaveGrid_Click ( sender, e );
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

			string [ ] data;
			char ch = '\n';
			int index = 1;
			string input = "";
			string path = Utils . GetImportFileName ( ".CSV" );
			if ( path . Length == 0 ) return;
			input = File . ReadAllText ( path );
			// 1st, check for data type format (Text/Grid)
			data = input . Split ( ch );
			if ( data . Length == 1 && data [ 0 ] . Contains ( "\n" ) == false )
			{
				// we have a Grid style csv !   - WORKING WELL 13/6/21
				bvm . Clear ( );
				ch = ',';
				data = input . Split ( ch );
				textBox . Text = "";
				index = 0;
				foreach ( var item in data )
				{
					//					bv = new BankAccountViewModel ( );
					//					DragviewModel bv = new DragviewModel ( );
					// //Jump the first field as it is the type string
					bv . RecordType = data [ 0 ];
					// //Jump the first field as it is the type string
					textBox . Text += data [ index ] + ",";
					index++;
					if ( index >= data . Length ) break;
					textBox . Text += data [ index ] + ",";
					bv . Id = int . Parse ( data [ index++ ] );
					textBox . Text += data [ index ] + ",";
					bv . CustNo = data [ index++ ];
					textBox . Text += data [ index ] + ",";
					bv . BankNo = data [ index++ ];
					textBox . Text += data [ index ] + ",";
					bv . AcType = int . Parse ( data [ index++ ] );
					textBox . Text += data [ index ] + ",";
					bv . IntRate = decimal . Parse ( data [ index++ ] );
					textBox . Text += data [ index ] + ",";
					bv . Balance = decimal . Parse ( data [ index++ ] );
					// gotta remove the quote marks
					textBox . Text += data [ index ] + ",";
					string tmp1 = data [ index++ ];
					string tmp2 = tmp1 . Substring ( 1 );
					char ch2 = '\'';
					string [ ] tmp3;
					tmp3 = tmp2 . Split ( ch2 );
					bv . ODate = Convert . ToDateTime ( tmp3 [ 0 ] );
					textBox . Text += data [ index ];
					tmp1 = data [ index++ ];
					tmp2 = tmp1 . Substring ( 1 );
					tmp3 = tmp2 . Split ( ch2 );
					bv . CDate = Convert . ToDateTime ( tmp3 [ 0 ] );
					// Add  the new record to our List<bvm>
					bvm . Add ( bv );
					textBox . Text += "\n";
				}
			}
			else
			{
				//Text style input file  - WORKING WELL 13/6/21
				ch = ',';
				textBox . Text = input;
				int textlen = textBox . Text . Length;
				textBox . CaretIndex = textlen;
				// Now create grid data

				string [ ] rowdata;
				char cr = '\n';
				// split input data into record length strings
				rowdata = input . Split ( cr );

				// this data in input is a single stream of data with no \n, so we need to parse it out into individuaol records first
				bvm . Clear ( );
				try
				{

					foreach ( var item in rowdata )
					{
						bv = new DragviewModel ( );
						//					bv = new BankAccountViewModel ( );
						data = item . Split ( ch );

						// //Jump the first field as it is the type string
						if ( data . Length < 2 ) break;
						if ( data [ 0 ] == "CUSTOMER" )
						{
							bv . RecordType = data [ 0 ];
							index = 1;
							bv . Id = int . Parse ( data [ index++ ] );
							bv . CustNo = data [ index++ ];
							bv . BankNo = data [ index++ ];
							bv . AcType = int . Parse ( data [ index++ ] );
							bv . IntRate = 0.00M;
							bv . Balance = 0.00M;
							// gotta remove the quote marks
							string tmp1 = data [ index++ ];
							//string tmp2 = tmp1 . Substring ( 1 );
							//char ch2 = '\'';
							//string [ ] tmp3;
							//tmp3 = tmp2 . Split ( ch2 );
							bv . ODate = Convert . ToDateTime ( tmp1 );
							tmp1 = data [ index++ ];
							bv . CDate = Convert . ToDateTime ( tmp1 );

						}
						else
						{
							if ( data [ 0 ] == "DETAILS"  || data [ 0 ] == "BANKACCOUNT" )
							{
								bv . RecordType = data [ 0 ];
								index = 1;
							}
							else
								index = 0;

							bv . Id = int . Parse ( data [ index++ ] );
							bv . CustNo = data [ index++ ];
							bv . BankNo = data [ index++ ];
							bv . AcType = int . Parse ( data [ index++ ] );
							bv . IntRate = decimal . Parse ( data [ index++ ] );
							bv . Balance = decimal . Parse ( data [ index++ ] );
							// gotta remove the quote marks
							string tmp1 = data [ index++ ];
							string tmp2 = tmp1 . Substring ( 1 );
							char ch2 = '\'';
							string [ ] tmp3;
							tmp3 = tmp2 . Split ( ch2 );
							bv . ODate = Convert . ToDateTime ( tmp3 [ 0 ] );
							tmp1 = data [ index++ ];
							tmp2 = tmp1 . Substring ( 1 );
							tmp3 = tmp2 . Split ( ch2 );
							bv . CDate = Convert . ToDateTime ( tmp3 [ 0 ] );
						}                               // Add  the new record to our List<bvm>
						bvm . Add ( bv );
					}
				}
				catch ( Exception ex )
				{
					MessageBox . Show ( $"Error encountered loading data from [{path}]\nPlease check  this data file for possible corruption!\nor the file may NOT be in  the correct format for this application", "Incompatible Data Identified" );
				}
			}
			dataGrid . ItemsSource = null;
			dataGrid . Items . Clear ( );
			dataGrid . ItemsSource = bvm;

			textBox . Focus ( );
		}

		private void ClearGrid_Click ( object sender, RoutedEventArgs e )
		{
			//-WORKING WELL 13 / 6 / 21
			dataGrid . ItemsSource = null;
			dataGrid . Items . Clear ( );
			bvm . Clear (  );
		}

		private void ClearText_Click ( object sender, RoutedEventArgs e )
		{
			//-WORKING WELL 13 / 6 / 21
			textBox . Text = "";
			textBox . Refresh ( );
		}

		private void LoadDragDrop ( object sender, RoutedEventArgs e )
		{

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
	}
}

