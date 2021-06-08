using System;
using System . Collections . Generic;
using System . Linq;
using System . Linq . Expressions;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Controls . Primitives;
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
	/// Interaction logic for GetExportRecords.xaml
	/// </summary>
	public partial class GetExportRecords : Window
	{
		public static List<BankAccountViewModel >records;
		public GetExportRecords (List<BankAccountViewModel  >recordsarray )
		{
			InitializeComponent ( );
			records = recordsarray;
               }

                private void Cancelbutton_Click ( object sender, RoutedEventArgs e )
		{Close ( );}

		private void Gobutton_Click ( object sender, RoutedEventArgs e )
		{

		}

		private void OnLoaded ( object sender, RoutedEventArgs e )
		{
			dataGrid . ItemsSource = records;
			dataGrid . SelectedIndex = 0;
			dataGrid . SelectedItem = 0;
			dataGrid . Refresh ( );

		}

		//private bool IsTheMouseOnTargetRow (Visual theTarget, GetDragDropPosition pos)
		//{
		//	Rect posBounds = VisualTreeHelper . GetDescendantBounds ( theTarget );
		//	Point theMousePos = pos ( ( IInputElement) theTarget);
		//	return posBounds . Contains ( theMousePos );
		//}

		//private DataGridRow GetDataGridRowItem(int index )
		//{
		//	if(records.Item)
		//}







		//private void SelectedGrid_PreviewDrop ( object sender, DragEventArgs e )
		//{
		//	int x = 0;
		//}

		//private void DataGrid_DragEnter ( object sender, DragEventArgs e )
		//{
		//	int x = 0;

		//}

		//private void DataGrid_PreparingCellForEdit ( object sender, DataGridPreparingCellForEditEventArgs e )
		//{

		//}
	}
}
