using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Imaging;
using System . Windows . Shapes;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for DelegateSelection.xaml
	/// </summary>
	public partial class DelegateSelection : Window
	{
		int result = 0;
		public DelegateSelection ( )
		{
			InitializeComponent ( );
			checkBox1 . IsChecked = true;
		}

		private void Button_Click ( object sender , RoutedEventArgs e )
		{
			if ( checkBox1 . IsChecked == true )
				result = 1;
			else if ( checkBox2 . IsChecked == true )
				result = 2;
			else if ( checkBox3 . IsChecked == true )
				result = 3;
			else if ( checkBox4 . IsChecked == true )
				result = 4;
			else if ( checkBox5 . IsChecked == true )
				result = 5;
			else if ( checkBox6 . IsChecked == true )
				result = 6;
			else if ( checkBox7 . IsChecked == true )
				result = 7;
			else if ( checkBox8 . IsChecked == true )
				result = 8;
			else if ( checkBox9 . IsChecked == true )
				result = 9;
			// Store resuolt in local variable in Viewer itself
			SqlDbViewer.DelegateSelection = result;
			this . DialogResult = true;
			Close ( );
		}

		private void Cancel_Click ( object sender , RoutedEventArgs e )
		{
			DialogResult = false;
			Close ( );
		}

		private void SelectDelegate_PreviewKeyUp ( object sender , KeyEventArgs e )
		{
			if ( e . Key == Key . Enter )
			{
				DialogResult = true;
				Close ( );
			}
			if ( e . Key == Key . Escape )
			{
				DialogResult = false;
				Close ( );
			}
		}
	}
}
