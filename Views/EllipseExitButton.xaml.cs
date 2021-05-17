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
	/// Interaction logic for EllipseExitButton.xaml
	/// </summary>
	public partial class EllipseExitButton : UserControl
	{
		public EllipseExitButton()
		{
			InitializeComponent();
		}
		private void ExitButton_Click(object sender, RoutedEventArgs e)
		{
			//			this.MainPageHolder.NavigationService.Navigate(MainWindow._Page1);
		}

		private void ExitButton_Click(object sender, MouseButtonEventArgs e)
		{
			Application.Current.Shutdown();
		}
	}
}
