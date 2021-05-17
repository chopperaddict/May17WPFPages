
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using WpfUI;

using static WpfUI.People;
using static WpfUI.Person;

namespace WPFPages
{

	/// <summary>
	/// Author:	Ian Turner
	/// Date: $time$
	/// Interaction logic for Page2.xaml
	/// Version: 1.0.0.0
	/// </summary>
	/// 



	public partial class Page2 : Page
	{
		//SqlDbViewer tw = null;

		public Page2()
		{
			InitializeComponent();

			//The binding of this class is to the People Class

			//Create a grouping  so we can layout by Sex - don't know how this works really
			//but it works well.  It reads the list of items in the Control (a ListView in this case)
			// and creates another form of Collection, a Collectionview()
			CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lview3.ItemsSource);
			if (view != null)
			{
				PropertyGroupDescription groupDescription = new PropertyGroupDescription("Sex");
				view.GroupDescriptions.Add(groupDescription);
			}
		}
		private void Page1_Click(object sender, RoutedEventArgs e)
		{
//			this.NavigationService.Navigate(MainWindow._Page1);
		}
		private void Page2_Click(object sender, RoutedEventArgs e)
		{
//			this.NavigationService.Navigate(MainWindow._Page2);
		}
		private void Page3_Click(object sender, RoutedEventArgs e)
		{
//			this.NavigationService.Navigate(MainWindow._Page3);
		}
		private void ExitButton_Click(object sender, RoutedEventArgs e)
		{ Application.Current.Shutdown(); }
		void Page1Button_Click(object sender, RoutedEventArgs e)
		{
//			this.NavigationService.Navigate(MainWindow._Page1);
		}

		private void Page2Button_Click(object sender, RoutedEventArgs e)
		{
//			this.NavigationService.Navigate(MainWindow._Page3);

		}

		private void Page4_Click(object sender, RoutedEventArgs e)
		{
//			this.NavigationService.Navigate(MainWindow._Page4);

		}
		private void Page5_Click(object sender, RoutedEventArgs e)
		{
			SqlDbViewer tw = new SqlDbViewer(-1);
			tw.Show();
		}
		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			this.NavigationService.Navigate(MainWindow._Page0);

		}

	}
}
