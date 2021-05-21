using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static WPFPages.Utils;

namespace WPFPages
{
	/// <summary>
	/// Interaction logic for SQLFilter.xaml
	/// </summary>
	public partial class SQLFilter : Window
	{
		public string currentDb = "";
		public string operand = "";
		public string ColumnToFilterOn = "";
		public bool FilterResult = false;
		private SqlDbViewer parent = null;
		public SQLFilter(SqlDbViewer Viewparent)
{
			parent = Viewparent;
			InitializeComponent();

		}


		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this.MouseDown += delegate { DoDragMove(); };
		}
		private void DoDragMove()
		{
			//Handle the button NOT being the left mouse button
			// which will crash the DragMove Fn.....
			try
			{this.DragMove();}
			catch{return;}
		}
		private void SetFilter_Click(object sender, RoutedEventArgs e)
		{
			operand = Operand.SelectedItem?.ToString();
			ColumnToFilterOn = FilterList.SelectedItem?.ToString();
			if (ColumnToFilterOn == null)
			{
				System.Windows.MessageBox.Show("Invalid data - Please select the field you wish to filter on ?");
				FilterList.Focus();
				return;
			}
			if (FilterValue.Text == "")
			{
				System.Windows.MessageBox.Show("Invalid data - Please enter the Search Term to be used for filtering on ?");
				FilterValue.Focus();
				return;
			}
			FilterResult = true;
			Close();
		}

		private void ExitFilter_Click(object sender, RoutedEventArgs e)
		{

			FilterResult = false;
			Close ();

		}

		private void Filter_Select(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			string filterString = FilterList.SelectedItem.ToString();
		}

		private void checkEscape_click(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				FilterResult = false;
				Close ();

			}
			else if ( e . Key == Key . Enter )
			{
				
				FilterResult = true;
				SetFilter_Click ( this, null );
				Close ( );
			}
		}

		private void MultiAccounts_Click(object sender, RoutedEventArgs e)
		{
			// get all Customers accounts that have more than 1 Bank A/c's

		}

		private void Window_Closing (object sender, System.ComponentModel.CancelEventArgs e)
		{
			if ( parent == null ) return;
			//ControlTemplate tmp = GetDictionaryControlTemplate ("HorizontalGradientTemplateGray");
			//parent.Filters.Template = tmp;
			//Brush br = GetDictionaryBrush ("HeaderBrushGray");
			//parent.Filters.Background = br;
			//parent.Filters.Content = "Filtering";


		}
	}
}
