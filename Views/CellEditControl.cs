using System;
using System . Collections . Generic;
using System . Linq;
using System . Security . Cryptography;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Xml . Linq;
using WPFPages . ViewModels;

namespace WPFPages . Views
{
	public class RowData
	{
		public int _CustNo = 0;
		public int _BankNo = 0;
		public int _AcType = 0;
		public decimal _IntRate = 0M;
		public decimal _Balance = 0M;
		public DateTime _ODate;
		public DateTime _CDate;
		public RowData ( )
		{
		}
	}

	public class CustRowData
	{
		public int _CustNo = 0;
		public int _BankNo = 0;
		public int _AcType = 0;
		public string _FName = "";
		public string _LName = "";
		public string _Addr1= "";
		public string _Addr2 = "";
		public string _Town = "";
		public string _County = "";
		public string _PCode = "";
		public string _Phone = "";
		public string _Mobile = "";
		public DateTime _Dob;
		public DateTime _CDate;
		public DateTime _ODate;
		public CustRowData ( )
		{
		}
	}
	public class CellEditControl
	{
		// These MAINTAIN setting values across instances !!!
		public static int bindex { get; set; }
		public static int cindex { get; set; }
		public static int dindex { get; set; }
		public static DependencyProperty TagProperty { get; private set; }

		#region DATA EDIT CONTROL METHODS

	#region BANK HANDLERS
		/// <summary>
		///  DATA EDIT CONTROL METHODS
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static RowData BankGrid_EditStart ( RowData bvmCurrent, DataGridBeginningEditEventArgs e )
		{
			// Save  the current data for checking later on when we exit editing
			// but first, check to see if we already have one being saved !
			if ( bvmCurrent == null )
			{
				//// Get a new instance of our temp class
				bvmCurrent = new RowData ( );
				// Nope, so create a new one and get on with the edit process
				BankAccountViewModel tmp = new BankAccountViewModel ( );
				tmp = e . Row . Item as BankAccountViewModel;
				// save data to our temp class
				bvmCurrent . _CustNo = int . Parse ( tmp . CustNo );
				bvmCurrent . _BankNo = int . Parse ( tmp . BankNo );
				bvmCurrent . _AcType = tmp . AcType;
				bvmCurrent . _IntRate = tmp . IntRate;
				bvmCurrent . _Balance = tmp . Balance;
				bvmCurrent . _ODate = tmp . ODate;
				bvmCurrent . _CDate = tmp . CDate;
				return bvmCurrent;
			}
			return bvmCurrent;
		}

		/// <summary>
		/// does nothing at all because it is called whenver any single cell is exited
		///     and not just when ENTER is hit to save any changes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static bool BankGrid_EditEnding ( RowData bvmCurrent, DataGrid BankGrid, DataGridCellEditEndingEventArgs e )
		{
			//			bool ClearData = false;
			if ( bvmCurrent == null ) return false;

			// Has Data been changed in one of our rows. ?
			BankAccountViewModel bvm = BankGrid . SelectedItem as BankAccountViewModel;
			bvm = e . Row . Item as BankAccountViewModel;

			// This block get the data from the selected cell as a string - and Works well, but see lower down vfor an easier way
			var dgci = BankGrid . SelectedCells [ e . Column . DisplayIndex ];
			var dat = dgci . Column . GetCellContent ( dgci . Item );
			var val = dat . ToString ( );

			var columnName = e . Column . Header . ToString ( );
			// Only returns full depedecy name
			//var x = getDataGridValueAt ( BankGrid, columnName );
//			int ColumnIndex = BankGrid . CurrentColumn . DisplayIndex;

			// This actually get the data from the selected cell as a string - and Works well
			// Except dates will crash it
//			TextBox  CellContent = (BankGrid . SelectedCells [ ColumnIndex ] . Column . GetCellContent ( BankGrid . SelectedItem )) as TextBox;
//			string val2= CellContent . Text;

			if ( columnName == "Customer #" )
			{
				if ( val == bvmCurrent . _CustNo . ToString ( ) ){return true;}
			}
			else if ( columnName == "Bank #" )
			{
				if ( val == bvmCurrent . _BankNo . ToString ( ) ){return true;}
			}
			else if ( columnName == "Interest" )
			{
				if ( val == bvmCurrent . _IntRate . ToString ( ) ){return true;}
			}
			else if ( columnName == "Type" )
			{
				if ( val == bvmCurrent . _AcType . ToString ( ) ){return true;}
			}
			else if ( columnName == "Balance" )
			{
				if ( val == bvmCurrent . _Balance . ToString ( ) ){return true;}
			}
			else if ( columnName == "Open Date" )
			{	// Dates do NOT Work
				if ( val .Contains(bvmCurrent . _ODate . ToString ( )) ){return true;}
			}
			else if ( columnName == "Close Date" )
			{       // Dates do NOT Work
				if ( val == bvmCurrent . _CDate . ToString ( ) ){return true;}
			}
			return false;
		}

	#endregion BANK HANDLERS

	#region CUSTOMER HANDLERS
		/// <summary>
		///  DATA EDIT CONTROL METHODS
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static CustRowData CustGrid_EditStart ( CustRowData cvmCurrent, DataGridBeginningEditEventArgs e )
		{
			// Save  the current data for checking later on when we exit editing
			// but first, check to see if we already have one being saved !
			if ( cvmCurrent == null )
			{
				//// Get a new instance of our temp class
				cvmCurrent = new CustRowData ( );
				// Nope, so create a new one and get on with the edit process
				CustomerViewModel tmp = new CustomerViewModel ( );
				tmp = e . Row . Item as CustomerViewModel;
				// save data to our temp class
				cvmCurrent . _CustNo = int . Parse ( tmp . CustNo );
				cvmCurrent . _BankNo = int . Parse ( tmp . BankNo );
				cvmCurrent . _AcType = tmp . AcType;
				cvmCurrent . _FName= tmp . FName;
				cvmCurrent . _LName = tmp . LName;
				cvmCurrent . _Addr1= tmp . Addr1;
				cvmCurrent . _Addr2 = tmp . Addr2;
				cvmCurrent . _Town = tmp . Town;
				cvmCurrent . _County = tmp . County;
				cvmCurrent . _PCode = tmp . PCode;
				cvmCurrent . _Phone = tmp . Phone;
				cvmCurrent . _Mobile = tmp . Mobile;
				cvmCurrent . _Dob = tmp . Dob;
				cvmCurrent . _ODate = tmp . ODate;
				cvmCurrent . _CDate = tmp . CDate;
				return cvmCurrent;
			}
			return cvmCurrent;
		}

		/// <summary>
		/// does nothing at all because it is called whenver any single cell is exited
		///     and not just when ENTER is hit to save any changes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static bool CustGrid_EditEnding ( CustRowData cvmCurrent, DataGrid CustGrid, DataGridCellEditEndingEventArgs e )
		{
			//			bool ClearData = false;
			if ( cvmCurrent == null ) return false;

			// Has Data been changed in one of our rows. ?
			CustomerViewModel cvm = CustGrid . SelectedItem as CustomerViewModel;
			cvm = e . Row . Item as CustomerViewModel;
			var dgci = CustGrid . SelectedCells [ e . Column . DisplayIndex ];
			var dat = dgci . Column . GetCellContent ( dgci . Item );
			var val = dat . ToString ( );

			var columnName = e . Column . Header . ToString ( );
			if ( columnName == "Customer #" )
			{
				if ( val == cvmCurrent . _CustNo . ToString ( ) ) { return true; }
			}
			else if ( columnName == "Bank #" )
			{
				if ( val == cvmCurrent . _BankNo . ToString ( ) ) { return true; }
			}
			else if ( columnName == "Type" )
			{
				if ( val != cvmCurrent . _AcType . ToString ( ) ) { return true; }
			}
			else if ( columnName == "First Name" )
			{
				if ( val == cvmCurrent . _FName. ToString ( ) ) { return true; }
			}
			else if ( columnName == "Last Name" )
			{
				if ( val == cvmCurrent . _LName . ToString ( ) ) { return true; }
			}
			else if ( columnName == "Address 1" )
			{
				if ( val == cvmCurrent . _Addr1 . ToString ( ) ) { return true; }
			}
			else if ( columnName == "Address 2" )
			{
				if ( val == cvmCurrent . _Addr2 . ToString ( ) ) { return true; }
			}
			else if ( columnName == "Town" )
			{
				if ( val == cvmCurrent . _Town . ToString ( ) ) { return true; }
			}
			else if ( columnName == "County" )
			{
				if ( val == cvmCurrent . _County . ToString ( ) ) { return true; }
			}
			else if ( columnName == "Post code" )
			{
				if ( val == cvmCurrent . _PCode . ToString ( ) ) { return true; }
			}
			else if ( columnName == "Phone #" )
			{
				if ( val == cvmCurrent . _Phone . ToString ( ) ) { return true; }
			}
			else if ( columnName == "Mobile #" )
			{
				if ( val == cvmCurrent . _Mobile . ToString ( ) ) { return true; }
			}
			else if ( columnName == "D.o.B" )
			{
				if ( val == cvmCurrent . _Dob . ToString ( ) ) { return true; }
			}
			else if ( columnName == "Open Date" )
			{
				if ( val == cvmCurrent . _ODate . ToString ( ) ) { return true; }
			}
			else if ( columnName == "Close Date" )
			{
				if ( val == cvmCurrent . _CDate . ToString ( ) ) { return true; }
			}
			return false;
		}
	#endregion CUSTOMER HANDLERS

	#region DETAILS HANDLERS

		/// <summary>
		///  DATA EDIT CONTROL METHODS
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static RowData DetGrid_EditStart ( RowData  bvmCurrent, DataGridBeginningEditEventArgs e)
		{
			// Save  the current data for checking later on when we exit editing
			// but first, check to see if we already have one being saved !
			if ( bvmCurrent == null )
			{
				//// Get a new instance of our temp class
				bvmCurrent = new RowData ( );
				// Nope, so create a new one and get on with the edit process
				DetailsViewModel tmp = new DetailsViewModel ( );
				tmp = e . Row . Item as DetailsViewModel;
				// save data to our temp class
				bvmCurrent . _CustNo = int . Parse ( tmp . CustNo );
				bvmCurrent . _BankNo = int . Parse ( tmp . BankNo );
				bvmCurrent . _AcType = tmp . AcType;
				bvmCurrent . _IntRate = tmp . IntRate;
				bvmCurrent . _Balance = tmp . Balance;
				bvmCurrent . _ODate = tmp . ODate;
				bvmCurrent . _CDate = tmp . CDate;
				return bvmCurrent;
			}
			return null;
		}

		/// <summary>
		/// does nothing at all because it is called whenver any single cell is exited
		///     and not just when ENTER is hit to save any changes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static bool  DetGrid_EditEnding (  RowData bvmCurrent, DataGrid DetGrid, DataGridCellEditEndingEventArgs e )
		{
			//			bool ClearData = false;
			if ( bvmCurrent == null ) return false;

			bool updated = false;
			//if we get here, make sure we have been NOT been told to EsCAPE out
			DataGridEditAction dgea = e . EditAction;
			// Commit (1) is Confirm, otherwise we wil get Cancle (0) and we just exit here
			//			if ( dgea == 0 )
			//				ClearData = true; 

			// Has Data been changed in one of our rows. ?
			DetailsViewModel bvm = DetGrid . SelectedItem as DetailsViewModel;
			bvm = e . Row . Item as DetailsViewModel;
			var dgci = DetGrid . SelectedCells [ e . Column . DisplayIndex ];
			var dat = dgci . Column . GetCellContent ( dgci . Item );
			var val = dat . ToString ( );

			var columnName = e . Column . Header . ToString ( );
			if ( columnName == "Customer #" )
			{
				if ( val == bvmCurrent . _CustNo . ToString ( ) ) {return true;}
			}
			else if ( columnName == "Bank #" )
			{
				if ( val == bvmCurrent . _BankNo . ToString ( ) ) {return true;}
			}
			else if ( columnName == "Interest" )
			{
				if ( val == bvmCurrent . _IntRate . ToString ( ) ) {return true;}
			}
			else if ( columnName == "Type" )
			{
				if ( val == bvmCurrent . _AcType . ToString ( ) ) {return true;}
			}
			else if ( columnName == "Balance" )
			{
				if ( val == bvmCurrent . _Balance . ToString ( ) ) {return true;}
			}
			else if ( columnName == "Open Date" )
			{
				if ( val == bvmCurrent . _ODate . ToString ( ) ) {return true;}
			}
			else if ( columnName == "Close Date" )
			{
				if ( val == bvmCurrent . _CDate . ToString ( ) ) {return true;}
			}
			// Commit (1) is Confirm, otherwise we wil get Cancel (0) and we just exit here
			//if ( dgea == 0 )
			//{
			//				bvmCurrent = null;
			//				await DetailCollection . LoadDet ( DetViewerDbcollection, "DETAILSDBVIEW", 2, true );
			//				return false;
			//}
			
			// Data HAS CHANGED, so retur true
			return true;
		}

		#endregion DETAILS HANDLERS
		#endregion DATA EDIT CONTROL METHODS

	#region utils ??
		public static string GetSelectedCellValue ( DataGrid Grid )
	{
			DataGridCellInfo cellInfo = Grid . SelectedCells [ 0 ];
			if ( cellInfo == null ) return null;

			DataGridBoundColumn column = cellInfo . Column as DataGridBoundColumn;
			if ( column == null ) return null;

			FrameworkElement element = new FrameworkElement ( ) { DataContext = cellInfo . Item };
			BindingOperations . SetBinding ( element, TagProperty, column . Binding );

			return element . Tag . ToString ( );
		}

		/// <summary>
		/// Take a value from a the selected row of a DataGrid
		/// ATTENTION : The column's index is absolute : if the DataGrid is reorganized by the user,
		/// the index must change
		/// </summary>
		/// <param name="dGrid">The DataGrid where we take the value</param>
		/// <param name="columnIndex">The value's line index</param>
		/// <returns>The value contained in the selected line or an empty string if nothing is selected</returns>
		public static string getDataGridValueAt ( DataGrid dGrid, int columnIndex )
		{
			if ( dGrid . SelectedItem == null )
				return "";
			string str = dGrid . SelectedItem . ToString ( ); // Take the selected line
			str = str . Replace ( "}", "" ) . Trim ( ) . Replace ( "{", "" ) . Trim ( ); // Delete useless characters
			if ( columnIndex < 0 || columnIndex >= str . Split ( ',' ) . Length ) // case where the index can't be used 
				return "";
			str = str . Split ( ',' ) [ columnIndex ] . Trim ( );
			str = str . Split ( '=' ) [ 1 ] . Trim ( );
			return str;
		}

		/// <summary>
		/// Take a value from a the selected row of a DataGrid
		/// </summary>
		/// <param name="dGrid">The DataGrid where we take the value.</param>
		/// <param name="columnName">The column's name of the searched value. Be careful, the parameter must be the same as the shown on the dataGrid</param>
		/// <returns>The value contained in the selected line or an empty string if nothing is selected or if the column doesn't exist</returns>
		public static string getDataGridValueAt ( DataGrid dGrid, string columnName )
		{
			if ( dGrid . SelectedItem == null )
				return "";
			for ( int i = 0 ; i < columnName . Length ; i++ )
				if ( columnName . ElementAt ( i ) == '_' )
				{
					columnName = columnName . Insert ( i, "_" );
					i++;
				}
			string str = dGrid . SelectedItem . ToString ( ); // Get the selected Line
			str = str . Replace ( "}", "" ) . Trim ( ) . Replace ( "{", "" ) . Trim ( ); // Remove useless characters
			for ( int i = 0 ; i < str . Split ( ',' ) . Length ; i++ )
				if ( str . Split ( ',' ) [ i ] . Trim ( ) . Split ( '=' ) [ 0 ] . Trim ( ) == columnName ) // Check if the searched column exists in the dataGrid.
					return str . Split ( ',' ) [ i ] . Trim ( ) . Split ( '=' ) [ 1 ] . Trim ( );
			return str;
		}
		#endregion utils ??
	}
}
