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

namespace WPFPages.UserControls
{
	/// <summary>
	/// Interaction logic for CloseReturnButton.xaml
	/// </summary>
	public partial class CloseReturnButton : UserControl
	{
		public CloseReturnButton()
		{
			InitializeComponent();
		}

		private void CloseButton_Click(object sender, MouseButtonEventArgs e)
		{
			NavigationService ns = NavigationService.GetNavigationService(this);
//			ns.Navigate(MainWindow._Page1);

		}
	}
}
