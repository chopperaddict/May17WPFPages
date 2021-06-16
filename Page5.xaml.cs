using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace WPFPages.Views
{
	/// <summary>
	/// Interaction logic for Page5.xaml
	/// </summary>
	public partial class Page5 : Page
	{
		public List<TodoItem> items;
		ObservableCollection<TodoItem> ItemsCollection = new ObservableCollection<TodoItem>();
		public Page5() {
			InitializeComponent();
			//The following code is needed to allow binding WITHOUT 
			//using any form of the the XAML markup ItemsSource=items 
			items = new List<TodoItem>();
			items.Add(new TodoItem() { Title = "2Complete this WPF tutorial", Completion = 45 });
			items.Add(new TodoItem() { Title = "2Learn C#", Completion = 80 });
			items.Add(new TodoItem() { Title = "2Wash the car", Completion = 0 });
			lb2.ItemsSource = items; // This binds our List<> to a specific control
//			lb2.DisplayMemberPath;
//			this.DataContext = items;
		}

		private void Button0_Click(object sender, RoutedEventArgs e) {

		}
		private void Button_Click_1(object sender, RoutedEventArgs e) {

		}

		private void Button_Click_2(object sender, RoutedEventArgs e) {

		}

	}
}
