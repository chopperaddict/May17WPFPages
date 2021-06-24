
using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Dynamic;
using System . IO;
using System . Linq;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Input;
using System . Windows . Media;
using WPFPages . ViewModels;

namespace WPFPages . Views
{

	/// <summary>
	/// Interaction logic for GetExportRecords.xaml
	/// </summary>
	public partial class GetExportRecords : Window
	{
		public List<BankAccountViewModel> BankRecords = new List<BankAccountViewModel> ( );
		public List<DetailsViewModel> DetRecords = new List<DetailsViewModel> ( );
		public BankAccountViewModel bankmodel = new BankAccountViewModel ( );
		public DetailsViewModel detailsmodel = new DetailsViewModel ( );
		//		public List<BankAccountViewModel> Detailsrecords = new List<BankAccountViewModel> ( );
		//public static List<CustomerViewModel> Custrecords = new List<CustomerViewModel> ( );
		//public List<CustomerViewModel> selectedCustData = new List<CustomerViewModel> ( );
		//public List<DetailsViewModel> selectedDetData = new List<DetailsViewModel> ( );
		public string CurrentDb = "";
		public bool DraggingInProgress { get; set; }
		public bool LeftMouseButtonIsDown { get; set; }
		public int currseltop { get; set; }
		public int currselbottom { get; set; }
		public string topGridIdentity { get; set; }
		private static Point _startPoint { get; set; }
		//List<BankAccountViewModel> BankMoved = new List<BankAccountViewModel> ( );
		//List<DetailsViewModel> DetMoved = new List<DetailsViewModel> ( );

		public GetExportRecords ( string currentDb,
						ref List<BankAccountViewModel> Bankrecordreceived,
						ref List<DetailsViewModel> Detrecordreceived )
		{
			// We receive pointers to both a Bank and a Details collection
			InitializeComponent ( );
			Mouse . OverrideCursor = Cursors . Wait;
			CurrentDb = currentDb;

			this . Background = SetupBackgroundGradient ( );
			if ( CurrentDb == "BANKACCOUNT" )
			{
				// Setup internal pointers to our Collections
				topGridIdentity = "BANK";
				BankRecords = Bankrecordreceived;
				DetRecords = Detrecordreceived;
				TopGrid . Items . Clear ( );
				BottomGrid . Items . Clear ( );
				TopGrid . ItemsSource = BankRecords;
				BottomGrid . ItemsSource = DetRecords;
				UpperGridName . Text = "Bank Account Database";
				LowerGridName . Text = "Details Account Database";
				TopGrid . AlternatingRowBackground = Utils . GetDictionaryBrush ( "Cyan4" );
				BottomGrid . AlternatingRowBackground = Utils . GetDictionaryBrush ( "Green3" );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				// Setup internal pointers to our Collections
				topGridIdentity = "DET";
				BankRecords = Bankrecordreceived;
				DetRecords = Detrecordreceived;
				TopGrid . Items . Clear ( );
				BottomGrid . Items . Clear ( );

				TopGrid . ItemsSource = DetRecords;
				BottomGrid . ItemsSource = BankRecords;
				LowerGridName . Text = "Bank Account Database";
				UpperGridName . Text = "Details Account Database";
				TopGrid . AlternatingRowBackground = Utils . GetDictionaryBrush ( "Green3" );
				BottomGrid . AlternatingRowBackground = Utils . GetDictionaryBrush ( "Cyan3" );
			}
			TopGrid . SelectedIndex = 0;
			TopGrid . SelectedItem = 0;
			TopGrid . Refresh ( );
			BottomGrid . Refresh ( );
			if ( topGridIdentity == "BANK" )
			{
				uppercount . Text = BankRecords . Count . ToString ( );
				lowercount . Text = DetRecords . Count . ToString ( );
			}
			else
			{
				lowercount . Text = BankRecords . Count . ToString ( );
				uppercount . Text = DetRecords . Count . ToString ( );
			}
			TopMostOption . IsChecked = true;
			this . Topmost = true;
			this . MouseDown += delegate { DoDragMove ( ); };
		}
		private void Cancelbutton_Click ( object sender, RoutedEventArgs e )
		{
			// Clear databases so no update occurs on return To caller function
			BankRecords = null;
			DetRecords = null;
			Close ( );
		}

		private void Gobutton_Click ( object sender, RoutedEventArgs e )
		{
			//			DbManipulation . OutputSelectedRecords ( CurrentDb, Detailsrecords, selectedCustData, selectedDetData );
		}

		private void OnLoaded ( object sender, RoutedEventArgs e )
		{
			SetButtonGradientBackground ( Upbutton );
			SetButtonGradientBackground ( Dnbutton );
			SetButtonGradientBackground ( cancelbutton );
			Mouse . OverrideCursor = Cursors . Arrow;
		}

		private async void Select_Click ( object sender, RoutedEventArgs e )
		{
			string filename = Utils . GetExportFileName ( "*.CSV" );
			string lineout = "", output = "";
			if ( topGridIdentity == "BANK" )
			{
				// Top Grid is BANK, so our bottom grid must be DETAILS
				DetailsViewModel dvm = new DetailsViewModel ( );
				// looping thru the ROWS
				for ( int i = 0 ; i < BottomGrid . Items . Count ; i++ )
				{
					// iterate through each column in our dataset
					dvm = BottomGrid . Items [ i ] as DetailsViewModel;
					/*
					 * 		private string CreateTextFromRecord ( BankAccountViewModel bvm,
											DetailsViewModel dvm,
											bool IncludeType = true,
											bool includeDateQuotemarks = false,
											bool UseShortDate = false , 
											bool IncludeIdField = true)

					 * */
					lineout = CreateTextFromRecord ( null, dvm, null, false, true, true, false );
					output += lineout + "\n";
					lineout = "";
				}
			}
			else
			{
				// Top Grid is DETAILS, so our bottom grid must be BANK
				BankAccountViewModel bvm = new BankAccountViewModel ( );
				// looping thru the ROWS
				for ( int i = 0 ; i < BottomGrid . Items . Count ; i++ )
				{
					// iterate through each column in our dataset
					bvm = BottomGrid . Items [ i ] as BankAccountViewModel;
					lineout = CreateTextFromRecord ( bvm, null, null, false, true, true, false );
					output += lineout + "\n";
					lineout = "";
				}
			}
			// write the data to disk
			File . WriteAllText ( filename, output );
			Console . WriteLine ( $"All the selected data has been Exported Successfully to  [{filename}]" );
			MessageBox . Show ( $"All the selected data has been Exported Successfully to  [{filename}]", "Data Save" );
			if ( CurrentDb == "BANKACCOUNT" )
			{
				//	EventControl . TriggerTransferDataUpdated ( this, new LoadedEventArgs
				//	{
				//		DataSource = DetRecords,
				//		CallerType = "Updatemultiaccounts",
				//		CallerDb = CurrentDb
				//	} ); ;
			}
			Close ( );
		}

		//===========================
		#region DRAG AND DROP STUFF
		//===========================
		/// <summary>
		/// Handles doubleclick on a record by copying it AND any other records
		/// with the same CustNo and FIRSTLY copies them into the lower grid.
		/// 
		/// It then iterates through the top list and removes them from there before 
		/// redisplaying both grids
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BottomGrid_MouseDoubleClick ( object sender, MouseButtonEventArgs e )
		{
			int selindex = 0, delindex = 0;
			string custno = "", bankno = "", DataRecord = "";
			List<int> deleted = new List<int> ( );
			if ( topGridIdentity == "DET" )
			{
				// we are moving BANK record to DETAILS Grid
				// LOWER BANK -> TOP DETAILS  
				DetailsViewModel dvm = new DetailsViewModel ( );
				BankAccountViewModel bvm = new BankAccountViewModel ( );

				// Get Details record that has been dblclicked on from TOPGRID
				selindex = BottomGrid . SelectedIndex;
				bvm = BottomGrid . SelectedItem as BankAccountViewModel;
				// Store Custno as our key
				custno = bvm . CustNo;

				// Create a DETAILS record & add it to TOP DETAILS grid
				dvm = CreateDetailsRecord ( bvm );
				DetRecords . Add ( dvm );

				// now iterate thru BANK lower grid and delete the record we have moved to upper grid
				for ( int i = 0 ; i < TopGrid . Items . Count - 1 ; i++ )
				{
					bvm = BottomGrid . Items [ i ] as BankAccountViewModel;
					if ( bvm . CustNo == custno )
					{
						BankRecords . Remove ( bvm );
						break;
					}
				}
				// Refresh both grids
				TopGrid . ItemsSource = null;
				TopGrid . ItemsSource = DetRecords;
				TopGrid . Refresh ( );
				BottomGrid . ItemsSource = null;
				BottomGrid . ItemsSource = BankRecords;
				BottomGrid . Refresh ( );
			}
			else if ( topGridIdentity == "BANK" )
			{
				// we are moving DETAILS records to BANK Grid
				// LOWER DETAILS  -> TOP BANK
				DetailsViewModel dvm = new DetailsViewModel ( );
				BankAccountViewModel bvm = new BankAccountViewModel ( );

				// Get BANK record that has been dblclicked on from BOTTOMGRID
				selindex = BottomGrid . SelectedIndex;
				dvm = BottomGrid . SelectedItem as DetailsViewModel;
				// Store Custno as our key
				custno = dvm . CustNo;
				// Convert DETAILS record into BANK Record and add  to UPPER DETAILS GRID
				bvm = CreateBankRecord ( dvm );
				// Add DETAILS record to UPPER grid
				BankRecords . Add ( bvm );

				// iterate thru lower  BANK grid and delete the record we have moved to upper BANK grid
				for ( int i = 0 ; i < BottomGrid . Items . Count ; i++ )
				{
					dvm = BottomGrid . Items [ i ] as DetailsViewModel;
					if ( dvm . CustNo == custno )
					{
						DetRecords . Remove ( dvm );
						break;
					}
				}
				// Refresh both grid
				TopGrid . ItemsSource = null;
				TopGrid . ItemsSource = BankRecords;
				TopGrid . Refresh ( );
				BottomGrid . ItemsSource = null;
				BottomGrid . ItemsSource = DetRecords;
				BottomGrid . Refresh ( );
			}
			if ( topGridIdentity == "BANK" )
			{
				uppercount . Text = BankRecords . Count . ToString ( );
				lowercount . Text = DetRecords . Count . ToString ( );
			}
			else
			{
				lowercount . Text = BankRecords . Count . ToString ( );
				uppercount . Text = DetRecords . Count . ToString ( );
			}
		}
		private void TopGrid_MouseDoubleClick ( object sender, MouseButtonEventArgs e )
		{
			int selindex = 0, delindex = 0;
			string custno = "", bankno = "", DataRecord = "";
			List<int> deleted = new List<int> ( );
			if ( topGridIdentity == "BANK" )
			{
				// we are moving BANK records to DETAILS Grid
				// TOP BANK -> LOWER DETAILS 
				DetailsViewModel dvm = new DetailsViewModel ( );
				BankAccountViewModel bvm = new BankAccountViewModel ( );

				// Get Details record that has been dblclicked on from TOPGRID
				selindex = TopGrid . SelectedIndex;
				bvm = TopGrid . SelectedItem as BankAccountViewModel;
				// Store Custno as our key
				custno = bvm . CustNo;
				//Get all records matching this customer No 
				BankAccountViewModel tmp = new BankAccountViewModel ( );
				DataRecord = CreateTextFromRecord ( bvm, null );
				dvm = Utils.CreateDetailsRecordFromString ( DataRecord );
				DetRecords . Add ( dvm );

				// iterate thru BANK upper grid and delete the record we have moded to lower grid
				for ( int i = 0 ; i < TopGrid . Items . Count - 1 ; i++ )
				{
					bvm = TopGrid . Items [ i ] as BankAccountViewModel;
					if ( dvm . CustNo == custno )
						BankRecords . Remove ( bvm );
				}
				// Refresh both grid
				TopGrid . ItemsSource = null;
				TopGrid . ItemsSource = BankRecords;
				TopGrid . Refresh ( );
				BottomGrid . ItemsSource = null;
				BottomGrid . ItemsSource = DetRecords;
				BottomGrid . Refresh ( );
			}
			else if ( topGridIdentity == "DET" )
			{
				// we are moving DETAILS records to BANK Grid
				// TOP DETAILS -> LOWER BANK
				DetailsViewModel dvm = new DetailsViewModel ( );
				BankAccountViewModel bvm = new BankAccountViewModel ( );

				// Get Details record that has been dblclicked on from TOPGRID
				selindex = TopGrid . SelectedIndex;
				dvm = TopGrid . SelectedItem as DetailsViewModel;
				// Store Custno as our key
				custno = dvm . CustNo;
				//Get all records matching this customer No 
				//BankAccountViewModel tmp = new BankAccountViewModel ( );
				//DataRecord = CreateTextFromRecord ( null, dvm );
				bvm = CreateBankRecord ( dvm );
				BankRecords . Add ( bvm );

				// iterate thru upper grid and delete the record we have moded to lower grid
				for ( int i = 0 ; i < TopGrid . Items . Count - 1 ; i++ )
				{
					dvm = TopGrid . Items [ i ] as DetailsViewModel;
					if ( dvm . CustNo == custno )
						DetRecords . Remove ( dvm );
				}
				// Refresh both grid
				TopGrid . ItemsSource = null;
				TopGrid . ItemsSource = DetRecords;
				TopGrid . Refresh ( );
				BottomGrid . ItemsSource = null;
				BottomGrid . ItemsSource = BankRecords;
				BottomGrid . Refresh ( );
			}
			if ( topGridIdentity == "BANK" )
			{
				uppercount . Text = BankRecords . Count . ToString ( );
				lowercount . Text = DetRecords . Count . ToString ( );
			}
			else
			{
				lowercount . Text = BankRecords . Count . ToString ( );
				uppercount . Text = DetRecords . Count . ToString ( );
			}
		}
		private void BottomGrid_PreviewMouseMove ( object sender, System . Windows . Input . MouseEventArgs e )
		{
			if ( LeftMouseButtonIsDown == false )
			{
				// Dragging must have neded as well if button is up
				DraggingInProgress = false;
				return;
			}
			if ( DraggingInProgress )
				return;

			// Make sure the left mouse button is pressed down so we are really moving a record
			if ( e . LeftButton == MouseButtonState . Pressed )
			{
				if ( BottomGrid . SelectedItem != null )
				{
					if ( topGridIdentity == "BANK" )
					{
						// We are dragging from the DETAILS grid
						//Working string version
						DraggingInProgress = true;
						string str = CreateTextFromRecord ( null, BottomGrid . SelectedItem as DetailsViewModel );
						string dataFormat = DataFormats . UnicodeText;
						DataObject dataObject = new DataObject ( dataFormat, str );
						DragDrop . DoDragDrop (
						TopGrid,
						dataObject,
						DragDropEffects . Move );
					}
					else if ( topGridIdentity == "DET" )
					{
						// We are dragging from the BANK grid
						// We are dragging from the lower BANK grid in this case
						DraggingInProgress = true;
						string str = CreateTextFromRecord ( BottomGrid . SelectedItem as BankAccountViewModel, null );
						string dataFormat = DataFormats . UnicodeText;
						DataObject dataObject = new DataObject ( dataFormat, str );
						DragDrop . DoDragDrop (
						TopGrid,
						dataObject,
						DragDropEffects . Move );
					}
				}
			}
		}
		private void TopGrid_PreviewMouseMove ( object sender, MouseEventArgs e )
		{
			if ( LeftMouseButtonIsDown == false )
			{
				// Dragging must have neded as well if button is up
				DraggingInProgress = false;
				return;
			}
			if ( DraggingInProgress )
				return;

			// Make sure the left mouse button is pressed down so we are really moving a record
			if ( e . LeftButton == MouseButtonState . Pressed )
			{
				// ensure we have a record selected in Top Grid
				if ( TopGrid . SelectedItem != null )
				{
					if ( topGridIdentity == "BANK" )
					{
						// We are dragging from the upper BANK grid
						// to the lower DETAILS grid
						DraggingInProgress = true;
						string str = CreateTextFromRecord ( TopGrid . SelectedItem as BankAccountViewModel, null );
						string dataFormat = DataFormats . UnicodeText;
						DataObject dataObject = new DataObject ( dataFormat, str );
						DragDrop . DoDragDrop (
						TopGrid,
						dataObject,
						DragDropEffects . Move );
					}
					else if ( topGridIdentity == "DET" )
					{
						// We are dragging from the upper DETAILS grid
						// to the lower BANK grid
						DraggingInProgress = true;
						string str = CreateTextFromRecord ( null, TopGrid . SelectedItem as DetailsViewModel );
						string dataFormat = DataFormats . UnicodeText;
						DataObject dataObject = new DataObject ( dataFormat, str );
						DragDrop . DoDragDrop (
						TopGrid,
						dataObject,
						DragDropEffects . Move );
					}
				}
			}
		}
		private void BottomGrid_Drop ( object sender, DragEventArgs e )
		{
			int selindex = 0;
			string custno = "0";
			string dataString = "";
			if ( DraggingInProgress == false )
				return;

			// Have we got any data from a drag ?
			if ( e . Data . GetDataPresent ( DataFormats . StringFormat ) )
			{
				// Yes, so check what it is by verifying the Flag we put inside it in the Drag creation
				dataString = ( string ) e . Data . GetData ( DataFormats . StringFormat );
				// Yes, there is some Dragged data available
				if ( topGridIdentity == "BANK" )
				{       // working 11/6/21

					//Window is a child of a BANK  SqlDbViewer
					// We are dropping into the BOTTOM DETAILS grid, so data MUST be a BANK record
					if ( dataString . Contains ( "DETAILS" ) )
					{
						// WRONG - It's from our own grid, so ignore it
						DraggingInProgress = false;
						return;
					}

					// We are dropping an upper grid BANK record into ;lower DETAILS grid

					// Reset flag as We have now got the dragged data
					e . Handled = true;

					//Sanity check do we have a Details Record or not ?
					if ( dataString . Contains ( "BANKACCOUNT" ) == false )
					{
						DraggingInProgress = false;
						return;
					}

					// Get Details record that has been dblclicked on from TOPGRID
					selindex = TopGrid . SelectedIndex;
					BankAccountViewModel bvm = new BankAccountViewModel ( );
					bvm = TopGrid . SelectedItem as BankAccountViewModel;
					// Store Custno as our key
					custno = bvm . CustNo;
					DetailsViewModel dvm = new DetailsViewModel ( );
					dvm = CreateDetailsRecord ( bvm );
					DetRecords . Add ( dvm );

					// iterate thru BANK upper grid and delete the record we have moded to lower grid
					for ( int i = 0 ; i < TopGrid . Items . Count - 1 ; i++ )
					{
						bvm = TopGrid . Items [ i ] as BankAccountViewModel;
						if ( dvm . CustNo == custno )
						{
							BankRecords . Remove ( bvm );
							break;
						}
					}
					// Refresh both grid
					TopGrid . ItemsSource = null;
					TopGrid . ItemsSource = BankRecords;
					TopGrid . Refresh ( );
					BottomGrid . ItemsSource = null;
					BottomGrid . ItemsSource = DetRecords;
					BottomGrid . Refresh ( );
				}
				else if ( topGridIdentity == "DET" )
				{       // working 11/6/21
					//Window is a child of a DETAILS SqlDbViewer
					// We are dropping into the BOTTOM BANK grid, so data MUST be a DETAILS record
					if ( dataString . Contains ( "BANK" ) )
					{
						// It's from our own grid, so ignore it
						DraggingInProgress = false;
						return;
					}

					// We are dropping an upper grid DETAIL record into lower BANK grid

					// Get the dragged data with this call (I massaged it into a string format for easier usage overall)
					//					 dataString = ( string ) e . Data . GetData ( DataFormats . StringFormat );
					// Reset flag as We have now got the dragged data
					e . Handled = true;
					//Sanity check do we have a Details  Record or not ?

					selindex = TopGrid . SelectedIndex;
					// Massage it into a DetailsViewModel record so we can add it to lower DETAILS GRID
					BankAccountViewModel bvm = new BankAccountViewModel ( );
					bvm = Utils . CreateBankRecordFromString ( "BANK", dataString );
					// Store Custno as our key
					custno = bvm . CustNo;
					BankRecords . Add ( bvm );

					DetailsViewModel dvm = new DetailsViewModel ( );
					// iterate thru upper grid and delete the record we have moded to lower grid
					for ( int i = 0 ; i < TopGrid . Items . Count ; i++ )
					{
						dvm = TopGrid . Items [ i ] as DetailsViewModel;
						if ( dvm == null ) return;
						if ( dvm . CustNo == custno )
						{
							DetRecords . Remove ( dvm );
							break;
						}
					}
					// Refresh both grid
					// Get a temporary new List sorted by BankNo within CustNo to go back into our TOP list
					List<DetailsViewModel> temp = DetRecords . OrderBy ( DetailsViewModel => DetailsViewModel . CustNo )
						. ThenBy ( DetailsViewModel => DetailsViewModel . BankNo )
						. ToList ( );
					DetRecords = temp;
					TopGrid . ItemsSource = null;
					TopGrid . ItemsSource = DetRecords;
					TopGrid . SelectedIndex = selindex;
					TopGrid . SelectedItem = selindex;
					TopGrid . Refresh ( );
					BottomGrid . ItemsSource = null;
					BottomGrid . ItemsSource = BankRecords;
					BottomGrid . Refresh ( );
					// Clear global dragging flag so a new one can start
				}
				// Clear global dragging flag so a new one can start
				DraggingInProgress = false;
				if ( topGridIdentity == "BANK" )
				{
					uppercount . Text = BankRecords . Count . ToString ( );
					lowercount . Text = DetRecords . Count . ToString ( );
				}
				else
				{
					lowercount . Text = BankRecords . Count . ToString ( );
					uppercount . Text = DetRecords . Count . ToString ( );
				}
			}
		}
		private void TopGrid_Drop ( object sender, DragEventArgs e )
		{
			// Not dragging, so we are outta here
			if ( DraggingInProgress == false )
				return;

			if ( e . Data . GetDataPresent ( DataFormats . StringFormat ) )
			{
				// get the data sent in  the drag message so we can verify what it is 
				string dataString = ( string ) e . Data . GetData ( DataFormats . StringFormat );
				if ( topGridIdentity == "BANK" )
				{
					// We are dropping into the TOP grid, which is a BANK grid, so data MUST be a DETAILS record
					if ( dataString . Contains ( "BANKACCOUNT" ) )
					{
						// It's from our own grid, so ignore it
						DraggingInProgress = false;
						return;
					}

					// We are dropping an lower grid DETAILS record into upper BANK grid
					// we have recieved a DETAILS  data record to drop back into our BANK grid
					BankAccountViewModel bvm = new BankAccountViewModel ( );

					// massage DETAILS data received from a string to a Bank record that we can add then to our bank list
					bvm = Utils . CreateBankRecordFromString ("DETAILS",  dataString );
					BankRecords . Add ( bvm );
					string custno = bvm . CustNo . ToString ( );
					int currsel = TopGrid . SelectedIndex;
					TopGrid . ItemsSource = null;
					TopGrid . ItemsSource = BankRecords;
					TopGrid . SelectedIndex = currsel;
					TopGrid . Refresh ( );
					// Recipient BANK  grid now has  the record added (back into) it, so now lets sort out the lower donor Details grid
					// and remove it from there, iterate thru and delete current record from the lower bank grid
					DetailsViewModel tmp = new DetailsViewModel ( );
					for ( int i = 0 ; i < BottomGrid . Items . Count ; i++ )
					{
						tmp = BottomGrid . Items [ i ] as DetailsViewModel;
						if ( tmp . CustNo == custno )
						{
							DetRecords . Remove ( tmp );
							break;
						}
					}
					DraggingInProgress = false;
					BottomGrid . ItemsSource = null;
					BottomGrid . ItemsSource = DetRecords;
					BottomGrid . Refresh ( );
				}
				else if ( topGridIdentity == "DET" )
				{
					// We are dropping an lower BANK grid record into upper DETAILS grid
					// we have recieved a BANK data record to drop back into our DETAILS upper grid 
					DetailsViewModel dvm = new DetailsViewModel ( );

					dvm = Utils . CreateDetailsRecordFromString ( dataString );
					DetRecords . Add ( dvm );
					int currsel = TopGrid . SelectedIndex;
					TopGrid . ItemsSource = null;
					TopGrid . ItemsSource = DetRecords;
					TopGrid . SelectedIndex = currsel;
					TopGrid . Refresh ( );
					// iterate thru and delete current record from the upper grid
					BankAccountViewModel tmp = new BankAccountViewModel ( );
					for ( int i = 0 ; i < BottomGrid . Items . Count ; i++ )
					{
						tmp = BottomGrid . Items [ i ] as BankAccountViewModel;
						if ( tmp . CustNo == dvm . CustNo )
						{
							BankRecords . Remove ( tmp );
							break;
						}
					}
					DraggingInProgress = false;
					BottomGrid . ItemsSource = null;
					BottomGrid . ItemsSource = BankRecords;
					BottomGrid . Refresh ( );
				}
			}
			if ( topGridIdentity == "BANK" )
			{
				uppercount . Text = BankRecords . Count . ToString ( );
				lowercount . Text = DetRecords . Count . ToString ( );
			}
			else
			{
				lowercount . Text = BankRecords . Count . ToString ( );
				uppercount . Text = DetRecords . Count . ToString ( );
			}
		}

		/// <summary>
		///<para> Specialist Fn to create a CSV style comma delimited string for any BANK or DETAILS record
		/// It provides (optional) arguments to control  the output format:</para>
		/// <list type="bullet">
		/// <item>
		/// <description>Include Data Type - True/False.</description>
		/// </item>
		/// <item>
		/// <description>wrap dates in single quotemarks - True/False</description>
		/// </item>
		/// <item>
		/// <description>Use Short Date format - True/False</description>
		/// </item>
		/// <item>
		/// <description>Include ID field - True/False</description>
		/// </item>
		/// </list>
		/// </summary>
		/// <param name="bvm"></param>
		/// <param name="dvm"></param>
		/// <param name="IncludeType"></param>
		/// <param name="includeDateQuotemarks"></param>
		/// <param name="UseShortDate"></param>
		/// <param name="IncludeIdField"></param>
		/// <returns></returns>
		public static  string CreateTextFromRecord ( BankAccountViewModel bvm,DetailsViewModel dvm,CustomerViewModel cvm = null,
					bool IncludeType = true, bool includeDateQuotemarks = true, bool UseShortDate = false,bool IncludeIdField = true)
		{
			if ( bvm == null && cvm == null && dvm == null ) return "";
			string datastring = "";
			string odate = "", cdate = "";
			if ( bvm != null )
			{
				// Handle a BANK Record
				if ( IncludeType ) datastring = "BANKACCOUNT,";
				if ( IncludeIdField )
					datastring += bvm . Id .ToString().Trim() + ",";
				datastring += bvm . CustNo+ ",";
				datastring += bvm . BankNo+ ",";
				datastring += bvm . AcType + ",";
				datastring += bvm . IntRate + ",";
				datastring += bvm . Balance + ",";
				odate = Utils . ConvertInputDate ( bvm . ODate . ToString ( ) . Trim ( ) );
				cdate = Utils . ConvertInputDate ( bvm . CDate . ToString ( ) . Trim ( ) );
				if ( includeDateQuotemarks )
				{
					if ( UseShortDate )
						datastring += "'" + odate + "',";
					else
						datastring += "'" + odate + "',";
					if ( UseShortDate )
						datastring += "'" + dvm . CDate . ToShortDateString ( ) + "'";
					else
						datastring += "'" + cdate + "'";
				}
				else
				{// Nonquotemarks
					if ( UseShortDate )
						datastring += odate + ",";
					else
						datastring += odate + ",";
					if ( UseShortDate )
						datastring += cdate;
					else
						datastring += cdate;
				}
			}
			else if ( dvm != null )
			{
				//This works - 12/6/21
				if ( IncludeType ) datastring = "DETAILS,";
				if ( IncludeIdField )
					datastring += dvm . Id + ",";
				datastring += dvm . CustNo+ ",";
				datastring += dvm . BankNo+ ",";
				datastring += dvm . AcType + ",";
				datastring += dvm . IntRate+ ",";
				datastring += dvm . Balance  + ",";
				odate = Utils . ConvertInputDate (dvm . ODate . ToString ( ) . Trim ( ) );
				cdate = Utils . ConvertInputDate ( dvm . CDate . ToString ( ) . Trim ( ) );
				if ( includeDateQuotemarks )
				{
					if ( UseShortDate )
						datastring += "'" + odate + "',";
					else
						datastring += "'" + odate + "',";
					if ( UseShortDate )
						datastring += "'" + cdate + "'";
					else
						datastring += "'" + cdate + "'" ;
				}
				else
				{// Nonquotemarks
					if ( UseShortDate )
						datastring +=  dvm . ODate . ToShortDateString ( ) . Trim ( ) + ",";
					else
						datastring += dvm . ODate . ToString ( ) . Trim ( ) + ",";
					if ( UseShortDate )
						datastring += dvm . CDate . ToShortDateString ( );
					else
						datastring += dvm . CDate . ToString ( ) . Trim ( );
				}
			}
			else if ( cvm != null )
			{
				if ( IncludeType ) datastring = "CUSTOMER,";
				if ( IncludeIdField )
					datastring += cvm . Id . ToString ( ) . Trim ( ) + ",";
				datastring += cvm . CustNo . ToString ( ) . Trim ( ) + ",";
				datastring += cvm . BankNo . ToString ( ) . Trim ( ) + ",";
				datastring += cvm . AcType . ToString ( ) . Trim ( ) + ",";
				odate = Utils . ConvertInputDate (cvm . ODate . ToString ( ) . Trim ( ) );
				cdate = Utils . ConvertInputDate ( cvm . CDate . ToString ( ) . Trim ( ) );
				if ( includeDateQuotemarks )
				{
					if ( UseShortDate )
						datastring += "'" + odate + "',";
					else
						datastring += "'" + odate + "',";
					if ( UseShortDate )
						datastring += "'" + dvm . CDate . ToShortDateString ( ) + "'";
					else
						datastring += "'" + cdate + "'";
				}
				else
				{// Nonquotemarks
					if ( UseShortDate )
						datastring += odate + ",";
					else
						datastring += odate + ",";
					if ( UseShortDate )
						datastring += cdate;
					else
						datastring += cdate;
				}
			}
			return datastring;
		}
		/// <summary>
		/// Creates a BankAccountViewModel record from a DetailsViewModel record directly
		/// </summary>
		/// <param name="dvm"></param>
		/// <returns></returns>
		/// 
		public static  BankAccountViewModel CreateBankRecord ( DetailsViewModel dvm )
		{
			BankAccountViewModel bvm = new BankAccountViewModel ( );
			bvm . Id = dvm . Id;
			bvm . CustNo = dvm . CustNo;
			bvm . BankNo = dvm . BankNo;
			bvm . AcType = dvm . AcType;
			bvm . IntRate = dvm . IntRate;
			bvm . Balance = dvm . Balance;
			bvm . ODate = dvm . ODate;
			bvm . CDate = dvm . CDate;
			return bvm;
		}
		/// <summary>
		/// Creates a DetailsViewModel record from a BankAccountViewModel record directly
		/// </summary>
		/// <param name="dvm"></param>
		/// <returns></returns>
		/// 
		public static DetailsViewModel CreateDetailsRecord ( BankAccountViewModel bvm )
		{
			DetailsViewModel dvm = new DetailsViewModel ( );
			dvm . Id = bvm . Id;
			dvm . CustNo = bvm . CustNo;
			dvm . BankNo = bvm . BankNo;
			dvm . AcType = bvm . AcType;
			dvm . IntRate = bvm . IntRate;
			dvm . Balance = bvm . Balance;
			dvm . ODate = bvm . ODate;
			dvm . CDate = bvm . CDate;
			return dvm;
		}

		//===========================
		#endregion DRAG AND DROP STUFF

		//===========================

		//===========================
		#region DRAG AND DROP UTILITY HELPERS
		//===========================

		private void TopGrid_PreviewLeftMouseButtonUp ( object sender, MouseButtonEventArgs e )
		{
			// Just clear our drag/drop flag as left button is up
			DraggingInProgress = false;
		}
		private void TopGrid_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			// Gotta make sure it is not anywhere in the Scrollbar we clicked on 
			if ( Utils . HitTestScrollBar ( sender, e ) ) return;
			if ( Utils . HitTestHeaderBar ( sender, e ) ) return;
			_startPoint = e . GetPosition ( null );
			// Make sure the left mouse button is pressed down so we are really moving a record
			if ( e . LeftButton == MouseButtonState . Pressed )
			{
				LeftMouseButtonIsDown = true;
			}
			else
				LeftMouseButtonIsDown = false;
		}

		private void Refresh_topgrid ( object sender, RoutedEventArgs e )
		{
			TopGrid . ItemsSource = null;
			if ( TopGrid . ItemsSource == BankRecords )
			{
				List<BankAccountViewModel> temp = BankRecords . OrderBy ( BankAccountViewModel => BankAccountViewModel . CustNo )
					. ThenBy ( BankAccountViewModel => BankAccountViewModel . BankNo )
					. ToList ( );
				BankRecords = temp;
				TopGrid . ItemsSource = BankRecords;
				TopGrid . SelectedIndex = currseltop;
			}
			else
			{
				List<DetailsViewModel> temp = DetRecords . OrderBy ( DetailsViewModel => DetailsViewModel . CustNo )
				. ThenBy ( DetailsViewModel => DetailsViewModel . BankNo )
				. ToList ( );
				DetRecords = temp;
				DetRecords = temp;
				TopGrid . ItemsSource = DetRecords;
				TopGrid . SelectedIndex = currseltop;
			}
			TopGrid . Refresh ( );
			if ( topGridIdentity == "BANK" )
			{
				uppercount . Text = BankRecords . Count . ToString ( );
				lowercount . Text = DetRecords . Count . ToString ( );
			}
			else
			{
				lowercount . Text = BankRecords . Count . ToString ( );
				uppercount . Text = DetRecords . Count . ToString ( );
			}
		}

		private void Refresh_bottomgrid ( object sender, RoutedEventArgs e )
		{
			BottomGrid . ItemsSource = null;
			if ( BottomGrid . ItemsSource == BankRecords )
			{
				List<BankAccountViewModel> temp = BankRecords . OrderBy ( BankAccountViewModel => BankAccountViewModel . CustNo )
					. ThenBy ( BankAccountViewModel => BankAccountViewModel . BankNo )
					. ToList ( );
				BankRecords = temp;
				BottomGrid . ItemsSource = DetRecords;
				BottomGrid . SelectedIndex = currselbottom;
			}
			else
			{
				List<DetailsViewModel> temp = DetRecords . OrderBy ( DetailsViewModel => DetailsViewModel . CustNo )
				. ThenBy ( DetailsViewModel => DetailsViewModel . BankNo )
				. ToList ( );
				DetRecords = temp;
				BottomGrid . ItemsSource = BankRecords;
				BottomGrid . SelectedIndex = currselbottom;
			}
			BottomGrid . Refresh ( );
			if ( topGridIdentity == "BANK" )
			{
				uppercount . Text = BankRecords . Count . ToString ( );
				lowercount . Text = DetRecords . Count . ToString ( );
			}
			else
			{
				lowercount . Text = BankRecords . Count . ToString ( );
				uppercount . Text = DetRecords . Count . ToString ( );
			}
		}

		private void BottomGrid_SelectionChanged ( object sender, System . Windows . Controls . SelectionChangedEventArgs e )
		{
			if ( BottomGrid . SelectedIndex == -1 ) return;
			currselbottom = BottomGrid . SelectedIndex;
			Debug . WriteLine ( $"LowerGrid = {currseltop}" );
		}

		private void TopGrid_SelectionChanged ( object sender, System . Windows . Controls . SelectionChangedEventArgs e )
		{
			if ( TopGrid . SelectedIndex == -1 ) return;
			currseltop = TopGrid . SelectedIndex;
			//			Debug . WriteLine ($"TopGrid = {currseltop}");
		}
		//===========================
		#endregion DRAG AND DROP UTILITY HELPERS
		//===========================


		//===========================
		#region GENERAL UTILS
		//===========================
		private void DoDragMove ( )
		{//Handle the button NOT being the left mouse button
		 // which will crash the DragMove Fn.....
			try
			{ this . DragMove ( ); }
			catch { return; }
		}

		private void TopMost_Click ( object sender, RoutedEventArgs e )
		{
			if ( TopMostOption . IsChecked == true )
				this . Topmost = true;
			else
				this . Topmost = false;

		}
		private LinearGradientBrush SetupBackgroundGradient ( )
		{
			//Get a new LinearGradientBrush
			LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush ( );
			//Set the start and end points of the drawing
			myLinearGradientBrush . StartPoint = new Point ( .3, 0 );
			myLinearGradientBrush . EndPoint = new Point ( 1, .4 );
			if ( CurrentDb == "BANKACCOUNT" )
			// Gradient Stops below are light to dark
			{
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . PowderBlue, 1.0 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . LightSteelBlue, 0.5 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . DarkBlue, 0 ) );
				//Now paint the buttons to match
				LinearGradientBrush myLinearGradientBrush2 = new LinearGradientBrush ( );
				myLinearGradientBrush2 . StartPoint = new Point ( 0.5, 0 );
				myLinearGradientBrush2 . EndPoint = new Point ( 0, 1 );

				GradientStop gs1 = new GradientStop ( );
				GradientStop gs2 = new GradientStop ( );
				GradientStop gs3 = new GradientStop ( );
				GradientStop gs4 = new GradientStop ( );
				GradientStop gs5 = new GradientStop ( );
				GradientStop gs6 = new GradientStop ( );
				gs1 . Color = Color . FromArgb ( 0xFF, 0x6B, 0x8E, 0x95 );
				gs2 . Color = Color . FromArgb ( 0xFF, 0x14, 0xA7, 0xC1 );
				gs3 . Color = Color . FromArgb ( 0xFF, 0x1E, 0x42, 0x4E );
				gs4 . Color = Color . FromArgb ( 0xFF, 0x1D, 0x48, 0x55 );
				gs5 . Color = Color . FromArgb ( 0xFF, 0x1D, 0x48, 0x55 );
				gs6 . Color = Color . FromArgb ( 0xFF, 0x19, 0x3A, 0x44 );
				gs1 . Offset = 1;
				gs2 . Offset = 0.509;
				gs3 . Offset = 0.542;
				gs4 . Offset = 0.542;
				gs5 . Offset = 0.526;
				gs6 . Offset = 0;
				myLinearGradientBrush2 . GradientStops . Add ( gs1 );
				myLinearGradientBrush2 . GradientStops . Add ( gs2 );
				myLinearGradientBrush2 . GradientStops . Add ( gs3 );
				myLinearGradientBrush2 . GradientStops . Add ( gs4 );
				myLinearGradientBrush2 . GradientStops . Add ( gs5 );
				myLinearGradientBrush2 . GradientStops . Add ( gs6 );
				return myLinearGradientBrush2;
			}
			if ( CurrentDb == "CUSTOMER" )
			{
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . White, 1.0 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . Gold, 0.3 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . DarkKhaki, 0.0 ) );
				//Now paint the buttons to match
				LinearGradientBrush myLinearGradientBrush2 = new LinearGradientBrush ( );
				myLinearGradientBrush2 . StartPoint = new Point ( 0.5, 0 );
				myLinearGradientBrush2 . EndPoint = new Point ( 0.5, 1 );

				GradientStop gs1 = new GradientStop ( );
				GradientStop gs2 = new GradientStop ( );
				GradientStop gs3 = new GradientStop ( );
				GradientStop gs4 = new GradientStop ( );
				GradientStop gs5 = new GradientStop ( );
				GradientStop gs6 = new GradientStop ( );
				//Yellow buttons
				gs1 . Color = Color . FromArgb ( 0xFF, 0x7a, 0x6f, 0x2d );
				gs2 . Color = Color . FromArgb ( 0xFF, 0xf5, 0xd8, 0x16 );
				gs3 . Color = Color . FromArgb ( 0xFF, 0x7d, 0x70, 0x15 );
				gs4 . Color = Color . FromArgb ( 0xFF, 0x5e, 0x56, 0x2a );
				gs5 . Color = Color . FromArgb ( 0xFF, 0x59, 0x50, 0x13 );
				gs6 . Color = Color . FromArgb ( 0xFF, 0x38, 0x32, 0x0c );
				gs1 . Offset = 1;
				gs2 . Offset = 0.209;
				gs3 . Offset = 0.342;
				gs4 . Offset = 0.442;
				gs5 . Offset = 0.526;
				gs6 . Offset = 0;
				myLinearGradientBrush2 . GradientStops . Add ( gs1 );
				myLinearGradientBrush2 . GradientStops . Add ( gs2 );
				myLinearGradientBrush2 . GradientStops . Add ( gs3 );
				myLinearGradientBrush2 . GradientStops . Add ( gs4 );
				myLinearGradientBrush2 . GradientStops . Add ( gs5 );
				myLinearGradientBrush2 . GradientStops . Add ( gs6 );
				return myLinearGradientBrush2;
			}
			if ( CurrentDb == "DETAILS" )
			{
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . White, 1.0 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . Green, 0.5 ) );
				myLinearGradientBrush . GradientStops . Add (
					new GradientStop ( Colors . DarkGreen, 0.25 ) );
				//Now paint the buttons to match
				LinearGradientBrush myLinearGradientBrush2 = new LinearGradientBrush ( );
				myLinearGradientBrush2 . StartPoint = new Point ( 0.2, 0 );
				myLinearGradientBrush2 . EndPoint = new Point ( 1, .8 );

				GradientStop gs1 = new GradientStop ( );
				GradientStop gs2 = new GradientStop ( );
				GradientStop gs3 = new GradientStop ( );
				GradientStop gs4 = new GradientStop ( );
				GradientStop gs5 = new GradientStop ( );
				GradientStop gs6 = new GradientStop ( );
				gs1 . Color = Color . FromArgb ( 0xFF, 0x75, 0xDD, 0x75 );
				gs2 . Color = Color . FromArgb ( 0xFF, 0x00, 0xFF, 0x00 );
				gs3 . Color = Color . FromArgb ( 0xFF, 0x33, 0x66, 0x33 );
				gs4 . Color = Color . FromArgb ( 0xFF, 0x44, 0x55, 0x44 );
				gs5 . Color = Color . FromArgb ( 0xFF, 0x33, 0x55, 0x55 );
				gs6 . Color = Color . FromArgb ( 0xff, 0x22, 0x40, 0x22 );
				gs1 . Offset = 1;
				gs2 . Offset = 0.709;
				gs3 . Offset = 0.542;
				gs4 . Offset = 0.342;
				gs5 . Offset = 0.126;
				gs6 . Offset = 0;
				myLinearGradientBrush2 . GradientStops . Add ( gs1 );
				myLinearGradientBrush2 . GradientStops . Add ( gs2 );
				myLinearGradientBrush2 . GradientStops . Add ( gs3 );
				myLinearGradientBrush2 . GradientStops . Add ( gs4 );
				myLinearGradientBrush2 . GradientStops . Add ( gs5 );
				myLinearGradientBrush2 . GradientStops . Add ( gs6 );
				return myLinearGradientBrush2;
			}
			// Use the brush to paint the rectangle.
			return myLinearGradientBrush;
		}

		public void SetButtonGradientBackground ( Button btn )
		{
			// how to change button background to a style in Code
			if ( btn == Upbutton || btn == Dnbutton )
			{
				if ( CurrentDb == "BANKACCOUNT" )
				{
					ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateBlue" );
					btn . Template = tmp;
					Brush br = Utils . GetDictionaryBrush ( "HeaderBrushBlue" );
					btn . Background = br;
					//					btn . Content = "Filter";
				}
				else
				{
					ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGreen" );
					btn . Template = tmp;
					Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGreen" );
					btn . Background = br;
				}
			}
			if ( btn == cancelbutton )
			{
				if ( CurrentDb == "BANKACCOUNT" )
				{
					ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateBlue" );
					btn . Template = tmp;
					Brush br = Utils . GetDictionaryBrush ( "HeaderBrushBlue" );
					btn . Background = br;
				}
				else
				{
					ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGreen" );
					btn . Template = tmp;
					Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGreen" );
					btn . Background = br;
				}
			}
		}
		//===========================
		#endregion GENERAL UTILS
		//===========================

	}
}