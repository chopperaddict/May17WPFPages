using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Windows.Data;
using System;

namespace WPFPages
{

	/// <summary>
	///  This class is a method from the Web to access the selected Cell in any DataGrid
	///  We need to call this from the MouseRightButtonUp Handler
	///  
	/// 		private void DataGrid_MouseRightButtonUp (object sender,	MouseButtonEventArgs e)
	/// </summary>
	public class DataGridSupport
	{
		public static int GetDataGridRowFromTree ( MouseButtonEventArgs e, out DataGridRow RowData )
		{
			int currentRow = -1;
			DependencyObject dep = ( DependencyObject ) e . OriginalSource;
			dep = VisualTreeHelper . GetParent ( dep );

			//I have found that we can still get the current row
			{
				DataGridCell cell = dep as DataGridCell;
				// navigate further up the tree
				while ( ( dep != null ) && !( dep is DataGridRow ) )
				{
					dep = VisualTreeHelper . GetParent ( dep );
				}
				DataGridRow row = dep as DataGridRow;
				currentRow = FindRowIndex ( row );
				RowData = row;
				return currentRow;
			}

		}

		public static object GetCellContent (object sender, MouseButtonEventArgs e,   string CurrentDb, out int currentRow, out int currentColumn, out string columnName, out object rowdata)
		{
			currentRow = -1;
			currentColumn = -1;
			columnName = "";
			rowdata = null;
			object returnData = null;
			DependencyObject dep = (DependencyObject)e.OriginalSource;
			DataGridCell cell = null;
			// iteratively traverse the visual tree
			//while ((dep != null) &&
			//!(dep is DataGridCell) &&
			//!(dep is DataGridColumnHeader))
			////Orignal code block
			//{

			//My trial version to handle datePicker
			while ((dep != null) &&
			!(dep is DataGridCell) &&
			!(dep is DatePicker) &&
			!(dep is DataGridRow) &&
			!(dep is DataGridColumnHeader))
			//Orignal code block
			{
				//This is called when there is a problem such as a DatePicker column  being received
				dep = VisualTreeHelper.GetParent (dep);

				//I have found that we can still get the current row
				//{
				//	DataGridCell cell = dep as DataGridCell;
				//	// navigate further up the tree
				//	while ((dep != null) && !(dep is DataGridRow))
				//	{
				//		dep = VisualTreeHelper.GetParent (dep);
				//	}
				//	DataGridRow row = dep as DataGridRow;
				//	currentRow = FindRowIndex (row);
				//	break;
				//}
			}

			if (dep == null)
				return returnData;

			if (dep is DataGridColumnHeader)
			//Orignal code block
			{
				DataGridColumnHeader columnHeader = dep as DataGridColumnHeader;
				// do something
				return columnHeader as object;
			}
			else 
			//***********************************//
			//MY TEST code block
			{
				if (dep is DatePicker)
				{
					//Never hit.....
					DataGridRow dgr = dep as DataGridRow;
					return dgr.Item;
				}
				else if (dep is DataGridCell)
				{
					// loo0ks like a DatePicker was clicked on - we cant handle these
//					DependencyProperty dp = new DependencyProperty ();
					object dgcell = dep.GetType();
					string g = dgcell.ToString ();
					//this is done later on anyway
					//cell = dep as DataGridCell;
				}
				else if (dep is DataGridRow)
				{
					// looks like a DatePicker was clicked on - we cant handle these
					DataGridRow dgr = dep as DataGridRow;
					return dgr.Item;
				}
				DataGridColumn dgc = dep as DataGridColumn;
				cell = dep as DataGridCell;
			}
			//***********************************//
			if (dep is DataGridCell)
			//Orignal code block
			{
				cell = dep as DataGridCell;
				DataGridColumn  dgc = cell.Column;
				//Get Header's Column text
				string colheader = dgc.Header as string;
				columnName = colheader;
				//Get column offset
				int colindex = dgc.DisplayIndex;
				Type t = dgc.GetType ();
//				dgc.GetValue ();
				// navigate further up the tree
				while ((dep != null) && !(dep is DataGridRow))
				{
					dep = VisualTreeHelper.GetParent (dep);
					if (dep is DataGridCell)
					{
						cell = dep as DataGridCell;
					}
//					if (dep is TextBlock)
//					{
//						cell = dep as TextBlock;
//					}
				}

				DataGridRow row = dep as DataGridRow;
			
				object cellData = ExtractBoundValue (row, cell, out  currentColumn);
				return (object)cellData;
			}
			return (object)null;
		}
		private static int FindRowIndex (DataGridRow row)
		{
			DataGrid dataGrid = ItemsControl.ItemsControlFromItemContainer (row) as DataGrid;

			int index = dataGrid.ItemContainerGenerator.IndexFromContainer (row);
			return index;
		}
		//private static int FindColumnIndex (DataGridColumn col)
		//{
		//	DataGrid dataGrid = ItemsControl.ItemsControlFromItemContainer (col) as DataGrid;

		//	int index = dataGrid.ItemContainerGenerator.IndexFromContainer (col);
		//	return index;
		//}

		private static object ExtractBoundValue (DataGridRow row, DataGridCell cell, out int column)
		{
			column = -1;
			// find the column that this cell belongs to
			DataGridBoundColumn col =   cell.Column as DataGridBoundColumn;
			if (col == null) return null;
			column = cell.Column.DisplayIndex;
		       // find the property that this column is bound to
		       Binding binding = col.Binding as Binding;
			string boundPropertyName = binding.Path.Path;

			// find the object that is related to this row
			object data = row.Item;
			if (data == null) return null;
			// extract the property value
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties (data);
			if (properties == null) return null;

			PropertyDescriptor property = properties[boundPropertyName];
			object value = property.GetValue (data);

			return value;
		}
	}

}
