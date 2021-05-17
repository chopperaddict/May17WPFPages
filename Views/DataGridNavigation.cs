using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace WPFPages.Views
{
	public static class DataGridNavigation
	{
		#region DataGrid positioning code

		//public static void SelectRowByIndex (DataGrid dataGrid, int rowIndex, int GetCellindex)
		//{
		//	if (!dataGrid.SelectionUnit.Equals (DataGridSelectionUnit.FullRow))
		//	//Add dbselector call in here somewhere
		//	{
		//		Console.WriteLine ("The SelectionUnit of the DataGrid must be set to FullRow.");
		//		return;
		//	}

		//	if (dataGrid.Items.Count == 0)
		//		return;
		//	if (rowIndex < 0 || rowIndex > (dataGrid.Items.Count - 1))
		//	{
		//		Console.WriteLine (string.Format ("Positioning error - {0} is an invalid row index.", rowIndex));
		//		return;
		//	}
		//	//Crashes if the grid is set to single selecton only
		//	/* set the SelectedItem property */
		//	object item = dataGrid.Items[rowIndex]; // = Product X
		//	dataGrid.SelectedItem = item;

		//	DataGridRow row = dataGrid.ItemContainerGenerator.ContainerFromIndex (rowIndex) as DataGridRow;
		//	if (row == null)
		//	{
		//		/* bring the data item (Product object) into view
		//		 * in case it has been virtualized away */
		//		dataGrid.ScrollIntoView (item);

		//		//if dataGrid = "DataGrid1" we are handling EditDb DataGrid
		//		// else it is "BankGrid" or CustomerGrid or DetailsGrid in SQLDbViewer
		//		row = dataGrid.ItemContainerGenerator.ContainerFromIndex (rowIndex) as DataGridRow;

		//	}
		//	if (GetCellindex != -1)
		//	{
		//		DataGridCell cell = GetCell (dataGrid, row, GetCellindex);
		//		if (cell != null)
		//			cell.Focus ();
		//		//				TODO: Retrieve and focus a DataGridCell object
		//	}
		//}
		//public static DataGridCell GetCell (DataGrid dataGrid, DataGridRow rowContainer, int column)
		//{
		//	if (rowContainer != null)
		//	{
		//		DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter> (rowContainer);
		//		if (presenter == null)
		//		{
		//			/* if the row has been virtualized away, call its ApplyTemplate() method 
		//			 * to build its visual tree in order for the DataGridCellsPresenter
		//			 * and the DataGridCells to be created */
		//			rowContainer.ApplyTemplate ();
		//			presenter = FindVisualChild<DataGridCellsPresenter> (rowContainer);
		//		}
		//		if (presenter != null)
		//		{
		//			DataGridCell cell = presenter.ItemContainerGenerator.ContainerFromIndex (column) as DataGridCell;
		//			if (cell == null)
		//			{
		//				/* bring the column into view
		//				 * in case it has been virtualized away */
		//				dataGrid.ScrollIntoView (rowContainer, dataGrid.Columns[column]);
		//				cell = presenter.ItemContainerGenerator.ContainerFromIndex (column) as DataGridCell;
		//			}
		//			return cell;
		//		}
		//	}
		//	return null;
		//}

		public static void SelectRowByIndex (DataGrid dataGrid, int rowIndex, int GetCellindex)
		{
			DataGrid caller = null;
//			DataGrid slaveGrid = null;
			if (!dataGrid.SelectionUnit.Equals (DataGridSelectionUnit.FullRow))
			{
				Console.WriteLine ("The SelectionUnit of the DataGrid must be set to FullRow.");
				return;
			}
			//			if(dataGrid == MainWindow.DgControl.CurrentSqlGrid)
			//			dataGrid = MainWindow.DgControl.SelChangeCallerGrid;
			Console.WriteLine ($"SelectRowByIndex: Caller = {dataGrid.Name}, RowToFind = {rowIndex}");
			if (dataGrid == null) return;
			caller = dataGrid;
			if (dataGrid.Items.Count == 0) return;
			if (rowIndex < 0 || rowIndex > (dataGrid.Items.Count - 1))
			{
				Console.WriteLine (string.Format ("Positioning error - {0} is an invalid row index.", rowIndex)); return;
			}
			//Crashes if the grid is set to single selecton only
			/* set the SelectedItem property */
			object item = dataGrid.Items[rowIndex];
			dataGrid.SelectedItem = item;

			DataGridRow row = dataGrid.ItemContainerGenerator.ContainerFromIndex (rowIndex) as DataGridRow;
			if (row == null)
			{
				/* bring the data item (Product object) into view
				 * in case it has been virtualized away */
				dataGrid.ScrollIntoView (item);

				//if dataGrid = "DataGrid1" we are handling EditDb DataGrid
				// else it is "BankGrid" or CustomerGrid or DetailsGrid in SQLDbViewer
				//row = dataGrid.ItemContainerGenerator.ContainerFromIndex (rowIndex) as DataGridRow;

			}
			if (GetCellindex != -1)
			{
				DataGridCell cell = GetCell (dataGrid, row, GetCellindex);
				if (cell != null)
					cell.Focus ();
				//				TODO: Retrieve and focus a DataGridCell object
			}
			dataGrid.SelectedItem = dataGrid.SelectedIndex;
			dataGrid.ScrollIntoView (dataGrid.SelectedItem);
			//clear flag again
			//			MainWindow.DgControl.SelChangeCallerGrid = null;
		}
		public static DataGridCell GetCell (DataGrid dataGrid, DataGridRow rowContainer, int column)
		{
			if (rowContainer != null)
			{
				DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter> (rowContainer);
				if (presenter == null)
				{
					/* if the row has been virtualized away, call its ApplyTemplate() method 
					 * to build its visual tree in order for the DataGridCellsPresenter
					 * and the DataGridCells to be created */
					rowContainer.ApplyTemplate ();
					presenter = FindVisualChild<DataGridCellsPresenter> (rowContainer);
				}
				if (presenter != null)
				{
					DataGridCell cell = presenter.ItemContainerGenerator.ContainerFromIndex (column) as DataGridCell;
					if (cell == null)
					{
						/* bring the column into view
						 * in case it has been virtualized away */
						dataGrid.ScrollIntoView (rowContainer, dataGrid.Columns[column]);
						cell = presenter.ItemContainerGenerator.ContainerFromIndex (column) as DataGridCell;
					}
					return cell;
				}
			}
			return null;
		}

		public static T FindVisualChild<T> (DependencyObject obj) where T : DependencyObject
		{
			if ( VisualTreeHelper . GetChildrenCount ( obj ) > 0 )
			{
				Decorator border = VisualTreeHelper.GetChild(obj, 0) as Decorator;
				if ( border != null )
				{
					Decorator borderChild = border.Child as Decorator;
					if ( borderChild != null )
					{
						ScrollViewer scroll = borderChild.Child as ScrollViewer;
						if ( scroll != null )
						{
							scroll . ScrollChanged += new ScrollChangedEventHandler ( Flags.CurrentSqlViewer.Scroll_ScrollChanged );
						}
					}
				}
			}
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount (obj); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild (obj, i);
				if (child != null && child is T)   
					return (T)child;
				else
				{
					T childOfChild = FindVisualChild<T> (child);
					if (childOfChild != null)
						return childOfChild;
				}
			}
			return null;
		}

		//public static T FindVisualChild2<T> ( DependencyObject obj ) where T : DependencyObject
		//{
			// Different version of above ?????

			//if ( VisualTreeHelper . GetChildrenCount ( obj ) > 0 )
			//{
			//	Decorator border = VisualTreeHelper.GetChild(obj, 0) as Decorator;
			//	if ( border != null )
			//	{
			//		Decorator borderChild = border.Child as Decorator;
			//		if ( borderChild != null )
			//		{
			//			ScrollViewer scroll = borderChild.Child as ScrollViewer;
			//			if ( scroll != null )
			//			{
			//				scroll . ScrollChanged += new ScrollChangedEventHandler ( Flags . CurrentSqlViewer . Scroll_ScrollChanged );
			//			}
			//		}
			//	}
			//}
			//DependencyObject child = null;
			//for ( int i = 0 ; i < VisualTreeHelper . GetChildrenCount ( obj ) ; i++ )
			//{
			//	child = VisualTreeHelper . GetChild ( obj , i );
			//	if ( child != null && child is T )
			//	{
			//		break;
			//		//return ( T ) child;
			//	}
			//	else
			//	{
			//		T childOfChild = FindVisualChild<T> (child);
			//		if ( childOfChild != null )
			//		{
			//			child = childOfChild;
			//			break;
			//			//return childOfChild;
			//		}
			//	}
			//}
			//return ( T ) child;
//		}


		public static void SelectCellByIndex (DataGrid dataGrid, int rowIndex, int columnIndex)
		{
			if (!dataGrid.SelectionUnit.Equals (DataGridSelectionUnit.Cell))
				throw new ArgumentException ("The SelectionUnit of the DataGrid must be set to Cell.");

			if (rowIndex < 0 || rowIndex > (dataGrid.Items.Count - 1))
				throw new ArgumentException (string.Format ("{0} is an invalid row index.", rowIndex));

			if (columnIndex < 0 || columnIndex > (dataGrid.Columns.Count - 1))
				throw new ArgumentException (string.Format ("{0} is an invalid column index.", columnIndex));

			dataGrid.SelectedCells.Clear ();

			object item = dataGrid.Items[rowIndex]; //=Product X
			DataGridRow row = dataGrid.ItemContainerGenerator.ContainerFromIndex (rowIndex) as DataGridRow;
			if (row == null)
			{
				dataGrid.ScrollIntoView (item);
				row = dataGrid.ItemContainerGenerator.ContainerFromIndex (rowIndex) as DataGridRow;
			}
			if (row != null)
			{
				DataGridCell cell = GetCell (dataGrid, row, columnIndex);
				if (cell != null)
				{
					DataGridCellInfo dataGridCellInfo = new DataGridCellInfo (cell);
					dataGrid.SelectedCells.Add (dataGridCellInfo);
					cell.Focus ();
				}
			}
		}
		public static string CellData (DataGridCellInfo cell)
		{
			return $"{ cell.Column.Header}, {cell.Item}";
		}
	}
	#endregion DataGrid positioning code
}
