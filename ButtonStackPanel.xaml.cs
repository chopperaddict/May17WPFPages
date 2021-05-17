using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFPages
{
	/// <summary>
	/// Interaction logic for ButtonStackPanel.xaml
	/// </summary>
	public partial class ButtonStackPanel : Page
	{
		public ButtonStackPanel()
		{
			InitializeComponent();
		}
		private void Page1_Click(object sender, RoutedEventArgs e)
		{
			this.NavigationService.Navigate(MainWindow._Page1);
		}
	}
}
