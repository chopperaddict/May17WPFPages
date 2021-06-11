
using System;
using System . Collections . Generic;
using System . Diagnostics;
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
		public int currselbottom{ get; set; }
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
			if ( CurrentDb == "BANKACCOUNT" )
			{
				EventControl . TriggerTransferDataUpdated ( this, new LoadedEventArgs
				{
					DataSource = DetRecords,
					CallerType = "Updatemultiaccounts",
					CallerDb = CurrentDb
				} ); ;
			}
			else
			{
				EventControl . TriggerTransferDataUpdated ( this, new LoadedEventArgs
				{
					DataSource = BankRecords,
					CallerType = "Updatemultiaccounts",
					CallerDb = CurrentDb
				} );
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
			if ( CurrentDb == "BANKACCOUNT" )
			{
				// Parent		= BANK
				// TOPGRID		= BANK   	- List<BankAccountViewModel>	BANKRECORDS
				//BOTTOMGRID	= DETAILS	- List<DetailsViewModel>			DETRECORDS
				// File Handling Bank moving records to Details
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
				dvm = CreateDetailsRecord( bvm);
				DetRecords . Add ( dvm );

				// iterate thru BANK upper grid and delete the record we have moded to lower grid
				for ( int i = 0 ; i < TopGrid . Items . Count - 1 ; i++ )
				{
					bvm = TopGrid . Items [ i ] as BankAccountViewModel;
					if ( dvm . CustNo == custno )
						BankRecords . Remove ( bvm );
				}
				// Refresh both grids
				TopGrid . ItemsSource = null;
				TopGrid . ItemsSource = BankRecords;
				TopGrid . Refresh ( );
				BottomGrid . ItemsSource = null;
				BottomGrid . ItemsSource = DetRecords;
				BottomGrid . Refresh ( );
			}
			if ( CurrentDb == "DETAILS" )
			{
				// WORKING CORRECTLY 11/6/21
				// Parent		= DETAILS
				// TOPGRID		= DETAILS	-List<DetailsViewModel>			DETRECORDS
				//BOTTOMGRID	= BANK		- List<BankAccountViewModel>	BANKRECORDS
				// File Handling Details moving records to Bank
				DetailsViewModel dvm = new DetailsViewModel ( );
				BankAccountViewModel bvm = new BankAccountViewModel ( );

				// Get BANK record that has been dblclicked on from BOTTOMGRID
				selindex = BottomGrid . SelectedIndex;
				bvm = BottomGrid . SelectedItem as BankAccountViewModel;
				// Store Custno as our key
				custno = bvm . CustNo;
				// Convert BANK record into DETAILS Record and add  to UPPER DETAILS GRID
				DetailsViewModel tmp = CreateDetailsRecord ( bvm );
				// Add DETAILS record to UPPER grid
				DetRecords . Add ( tmp );

				// iterate thru lower  BANK grid and delete the record we have moved to upper BANK grid
				for ( int i = 0 ; i < BottomGrid . Items . Count - 1 ; i++ )
				{
					bvm = BottomGrid . Items [ i ] as BankAccountViewModel;
					if ( bvm . CustNo == custno )
						BankRecords . Remove ( bvm );
				}
				// Refresh both grid
				TopGrid . ItemsSource = null;
				TopGrid . ItemsSource = DetRecords;
				TopGrid . Refresh ( );
				BottomGrid . ItemsSource = null;
				BottomGrid . ItemsSource = BankRecords;
				BottomGrid . Refresh ( );
			}
		}
		private void TopGrid_MouseDoubleClick ( object sender, MouseButtonEventArgs e )
		{
			int selindex = 0, delindex = 0;
			string custno = "", bankno = "", DataRecord = "";
			List<int> deleted = new List<int> ( );
			if ( CurrentDb == "BANKACCOUNT" )
			{
				// Parent		= BANK
				// TOPGRID		= BANK   	- List<BankAccountViewModel>	BANKRECORDS
				//BOTTOMGRID	= DETAILS	- List<DetailsViewModel>			DETRECORDS
				// File Handling Bank moving records to Details
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
				dvm = CreateDetailsRecordFromString ( DataRecord );
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
			if ( CurrentDb == "DETAILS" )
			{
				// WORKING CORRECTLY 11/6/21
				// Parent		= DETAILS
				// TOPGRID		= DETAILS	-List<DetailsViewModel>			DETRECORDS
				//BOTTOMGRID	= BANK		- List<BankAccountViewModel>	BANKRECORDS
				// File Handling Details moving records to Bank
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
				bvm = CreateBankRecord( dvm);
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
		}
		private void BottomGrid_PreviewMouseMove ( object sender, System . Windows . Input . MouseEventArgs e )
		{
			if ( LeftMouseButtonIsDown == false ) { 
				// Dragging must have neded as well if button is up
				DraggingInProgress = false;  
					return; 
			}
			if ( DraggingInProgress ) return;

			if ( e . LeftButton == MouseButtonState . Pressed )
			{
				if ( BottomGrid . SelectedItem != null )
				{
					if ( CurrentDb == "BANKACCOUNT" )
					{
						//Working string version
						string str = CreateTextFromRecord ( null, TopGrid . SelectedItem as DetailsViewModel );
						string dataFormat = DataFormats . UnicodeText;
						DataObject dataObject = new DataObject ( dataFormat, str );
						DragDrop . DoDragDrop (
						TopGrid,
						dataObject,
						DragDropEffects . Move );
					}
					else
					{
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
			if ( DraggingInProgress ) return;

			if ( e . LeftButton == MouseButtonState . Pressed )
			{
				if ( TopGrid . SelectedItem != null )
				{
					if ( TopGrid . ItemsSource == BankRecords )
					{
						DraggingInProgress = true;
						string str = CreateTextFromRecord ( TopGrid . SelectedItem as BankAccountViewModel, null );
						string dataFormat = DataFormats . UnicodeText;
						DataObject dataObject = new DataObject ( dataFormat, str );
						DragDrop . DoDragDrop (
						TopGrid,
						dataObject,
						DragDropEffects . Move );
					}
					else if ( TopGrid . ItemsSource == DetRecords )
					{
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
			string DataRecord = "", custno = "0", bankno = "0";

			if ( DraggingInProgress == false ) return;

			if ( e . Data . GetDataPresent ( DataFormats . StringFormat ) )
			{
				// Yes, there is some Dragged data available, but do NOT do it if the door grid is this grid itself
				if ( CurrentDb == "BANKACCOUNT"  )
				{
					// Parent		= BANK
					// TOPGRID		= BANK   	- List<BankAccountViewModel>	BANKRECORDS
					//BOTTOMGRID	= DETAILS	- List<DetailsViewModel>			DETRECORDS
					// File Handling Bank moving records to Details
					DetailsViewModel dvm = new DetailsViewModel ( );
					BankAccountViewModel bvm = new BankAccountViewModel ( );

					string dataString = ( string ) e . Data . GetData ( DataFormats . StringFormat );
					// Reset flag as We have now got the dragged data
					e . Handled = true;

					//Sanity check do we have a Details Record or not ?
					if ( dataString . Contains ( "DETAILS" ) == false )
					{
						DraggingInProgress = false;
						return;
					}

					// Get Details record that has been dblclicked on from TOPGRID
					selindex = TopGrid . SelectedIndex;
					bvm = TopGrid . SelectedItem as BankAccountViewModel;
					// Store Custno as our key
					custno = bvm . CustNo;
					dvm = CreateDetailsRecordFromString ( dataString );
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
					DraggingInProgress = false;

				}
				if ( CurrentDb == "DETAILS"  )
				{
					// WORKING CORRECTLY 11/6/21 (in mouse dblclick)
					// Parent		= DETAILS
					// TOPGRID		= DETAILS	-List<DetailsViewModel>			DETRECORDS
					//BOTTOMGRID	= BANK		- List<BankAccountViewModel>	BANKRECORDS
					// File Handling Details moving records to Bank
					BankAccountViewModel bvm = new BankAccountViewModel ( );

					// Get the dragged data with this call (I massaged it into a string format for easier usage overall)
					string dataString = ( string ) e . Data . GetData ( DataFormats . StringFormat );
					// Reset flag as We have now got the dragged data
					e . Handled = true;
					//Sanity check do we have a Details  Record or not ?
					if ( dataString. Contains ( "DETAILS" ) == false )
					{
						DraggingInProgress = false;
						return;
					}

					selindex = TopGrid . SelectedIndex;
					// Massage it into a DetailsViewModel record so we can add it to lower DETAILS GRID
					bvm = CreateBankRecordFromString ( dataString );
					// Store Custno as our key
					custno = bvm . CustNo;
					BankRecords . Add ( bvm );

					DetailsViewModel dvm = new DetailsViewModel ( );
					// iterate thru upper grid and delete the record we have moded to lower grid
					for ( int i = 0 ; i < TopGrid . Items . Count - 1 ; i++ )
					{
						dvm = TopGrid . Items [ i ] as DetailsViewModel;
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
					// Clear global dragging flag so a new one can start
					DraggingInProgress = false;
					TopGrid . ItemsSource = null;
					TopGrid . ItemsSource = DetRecords;
					TopGrid . SelectedIndex = selindex;
					TopGrid . SelectedItem = selindex;
					TopGrid . Refresh ( );
					BottomGrid . ItemsSource = null;
					BottomGrid . ItemsSource = BankRecords;
					BottomGrid . Refresh ( );
				}
			}
		}
		private void TopGrid_Drop ( object sender, DragEventArgs e )
		{
			//if ( LeftMouseButtonIsDown == false )
			//{
			//	// Dragging must have neded as well if button is up
			//	DraggingInProgress = false;
			//	return;
			//}
			// Not dragging, so we are outta here
			if ( DraggingInProgress == false ) return;

			if ( e . Data . GetDataPresent ( DataFormats . StringFormat ) )
			{
				if ( CurrentDb == "BANKACCOUNT" )
				{
					// we have recieved a BANK data record to drop back into our DETAILS grid
					BankAccountViewModel bvm = new BankAccountViewModel ( );
					string dataString = ( string ) e . Data . GetData ( DataFormats . StringFormat );

					//Sanity check do we have a Bank Record or not ?
					if ( dataString . Contains ( "BANKACCOUNT" ) == false )
					{
						DraggingInProgress = false;
						return;
					}

					// massage data from a string to a Bank record that we can add then to our bank list
					bvm = CreateBankRecordFromString ( dataString );
					BankRecords . Add ( bvm );
					int currsel = TopGrid . SelectedIndex;
					TopGrid . ItemsSource = null;
					TopGrid . ItemsSource = BankRecords;
					TopGrid . SelectedIndex = currsel;
					TopGrid . Refresh ( );
					// Recipient Bank  grid now has  the record added (back into) it, so now lets sort out the lower donor Details grid
					// and remove it from there, iterate thru from end working backwards and delete current record from the bank grid
					DetailsViewModel tmp = new DetailsViewModel ( );
					BankAccountViewModel bvm2 = new BankAccountViewModel ( );
//					return;
					for ( int i = BottomGrid . Items . Count - 2 ; i >= 0 ; i-- )
					{
						bvm2 = BottomGrid . Items [ i ] as BankAccountViewModel;
						if ( tmp . CustNo == bvm2 . CustNo )
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

				if ( CurrentDb == "DETAILS" )
				{
					if ( e . Data . GetDataPresent ( DataFormats . StringFormat ) )
					{
						// Called when we are dropping onto the upper DETAILS grid
						// we have received a BANK record, so first we massage it into a DETAILS record and add it to TOP list
						DetailsViewModel dvm = new DetailsViewModel ( );
						var  dataString = ( string ) e . Data . GetData ( DataFormats . StringFormat );

						//Sanity check do we have a Bank Record or not ?
						if ( dataString . Contains ( "DETAILS" ) )
						{
							DraggingInProgress = false;
							return;
						}

						dvm = CreateDetailsRecordFromString ( dataString );
						DetRecords . Add ( dvm );
						int currsel = TopGrid . SelectedIndex;
						TopGrid . ItemsSource = null;
						TopGrid . ItemsSource = DetRecords;
						TopGrid . SelectedIndex = currsel;
						TopGrid . Refresh ( );
						// iterate thru from end working backwards and delete current record from the upper grid
						BankAccountViewModel tmp = new BankAccountViewModel ( );
						for ( int i = 0 ; i < BottomGrid . Items . Count - 1 ; i++ )
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
			}
		}
		private string CreateTextFromRecord ( BankAccountViewModel bvm, DetailsViewModel dvm )
		{
			if ( bvm == null && dvm == null ) return "";
			string datastring = "";
			if ( bvm != null )
			{
				datastring = "BANKACCOUNT,"; 
				datastring += bvm . Id + ",";
				datastring += bvm . CustNo + ",";
				datastring += bvm . BankNo + ",";
				datastring += bvm . AcType + ",";
				datastring += bvm . IntRate + ",";
				datastring += bvm . Balance + ",";
				datastring += bvm . ODate + ",";
				datastring += bvm . CDate;
			}
			else
			{
				datastring = "DETAILS,";
				datastring += dvm . Id + ",";
				datastring += dvm . CustNo + ",";
				datastring += dvm . BankNo + ",";
				datastring += dvm . AcType + ",";
				datastring += dvm . IntRate + ",";
				datastring += dvm . Balance + ",";
				datastring += dvm . ODate + ",";
				datastring += dvm . CDate;
			}
			return datastring;
		}
		private BankAccountViewModel CreateBankRecordFromString ( string input )
		{
			BankAccountViewModel bvm = new BankAccountViewModel ( );
			char [ ] s = { ',' };
			string [ ] data = input . Split ( s );
			string donor = data [ 0 ];
			if ( donor != "DETAILS" ) return null;
			bvm . Id = int . Parse ( data [ 1 ] );
			bvm . CustNo = data [ 2 ];
			bvm . BankNo = data [ 3 ];
			bvm . AcType = int . Parse ( data [ 4 ] );
			bvm . IntRate = decimal . Parse ( data [ 5 ] );
			bvm . Balance = decimal . Parse ( data [ 6 ] );
			bvm . ODate = DateTime . Parse ( data [ 7 ] );
			bvm . CDate = DateTime . Parse ( data [ 8 ] );
			return bvm;
		}
		private DetailsViewModel CreateDetailsRecordFromString ( string input )
		{
			DetailsViewModel bvm = new DetailsViewModel ( );
			char [ ] s = { ',' };
			string [ ] data = input . Split ( s );
			string donor  = data [ 0 ];
			if ( donor != "BANKACCOUNT" ) return null;
			bvm . Id = int . Parse ( data [ 1 ] );
			bvm . CustNo = data [ 2 ];
			bvm . BankNo = data [ 3 ];
			bvm . AcType = int . Parse ( data [ 4 ] );
			bvm . IntRate = decimal . Parse ( data [ 5 ] );
			bvm . Balance = decimal . Parse ( data [ 6 ] );
			bvm . ODate = DateTime . Parse ( data [ 7 ] );
			bvm . CDate = DateTime . Parse ( data [ 8 ] );
			return bvm;
		}
		/// <summary>
		/// Creates a BankAccountViewModel record from a DetailsViewModel record directly
		/// </summary>
		/// <param name="dvm"></param>
		/// <returns></returns>
		/// 
		private BankAccountViewModel CreateBankRecord ( DetailsViewModel dvm )
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
		private DetailsViewModel CreateDetailsRecord ( BankAccountViewModel bvm )
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

		private void DoDragMove ( )
		{//Handle the button NOT being the left mouse button
		 // which will crash the DragMove Fn.....
			try
			{ this . DragMove ( ); }
			catch { return; }
		}

		private void TopGrid_PreviewLeftMouseButtonUp ( object sender, MouseButtonEventArgs e )
		{
			// Just clear our drag/drop flag as left button is up
			DraggingInProgress = false;
		}
		private void TopGrid_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			// set a flag os our drag knows the mousemove is OK as left button is also down
			if ( e . LeftButton == MouseButtonState . Pressed )
				LeftMouseButtonIsDown = true;
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
			Debug . WriteLine ($"TopGrid = {currseltop}");
		}

		//===========================
		#endregion DRAG AND DROP STUFF
		//===========================

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
			if ( btn == Upbutton  || btn == Dnbutton)
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

	}
}