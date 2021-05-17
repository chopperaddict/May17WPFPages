using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;


namespace WPFPages
{
	/// <summary>
	/// Interaction logic for Page0.xaml
	/// </summary>
	public partial class Page0 : Page
	{
		private GridViewColumnHeader listViewSortCol = null;
		private SortAdorner listViewSortAdorner = null;

		public Page0() {
			InitializeComponent();
			AllStaff os = new AllStaff();
//			TestListView.ItemsSource = os;
		}

		private void ListBox_MouseDown(object sender, MouseButtonEventArgs e) {
			ItemsControl l = (ItemsControl)sender;
			//int sel = l.g;
			Console.WriteLine($"Item selected is {l}");
		}
		private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ListView l = (ListView)sender;
			string sel = l.SelectedItem.ToString();
			Console.WriteLine($"Item selected is {sel}");
			if (l.Items.CurrentItem == null) return;
			Console.WriteLine($"Item selected is {l.Items.CurrentItem.ToString()}");
		}
		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ListBox l = (ListBox)sender;
			string sel = l.SelectedItem.ToString();
			Console.WriteLine($"Item selected is {sel}");
			if (l.Items.CurrentItem == null) return;
			Console.WriteLine($"Item selected is {l.Items.CurrentItem.ToString()}");
		}

		private void GridViewColumnHeader_Click(object sender, System.Windows.RoutedEventArgs e) {
			//How we go about Sorting the columns of a Listview
			GridViewColumnHeader column = (sender as GridViewColumnHeader);
			string sortBy = column.Tag.ToString();
			if (listViewSortCol != null) {
				AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
				lLabelView.Items.SortDescriptions.Clear();
			}

			ListSortDirection newDir = ListSortDirection.Ascending;
			if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
				newDir = ListSortDirection.Descending;

			listViewSortCol = column;
			listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
			AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
			lLabelView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
		}
	}
	public class SortAdorner : Adorner
	{
		//This Class simply draws the Sort "Triangle" in the header block
		private static Geometry ascGeometry =
			Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");

		private static Geometry descGeometry =
			Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");

		public ListSortDirection Direction { get; private set; }

		public SortAdorner(UIElement element, ListSortDirection dir)
			: base(element) {
			this.Direction = dir;
		}

		protected override void OnRender(DrawingContext drawingContext) {
			base.OnRender(drawingContext);

			if (AdornedElement.RenderSize.Width < 20)
				return;

			TranslateTransform transform = new TranslateTransform
				(
					AdornedElement.RenderSize.Width - 15,
					(AdornedElement.RenderSize.Height - 5) / 2
				);
			drawingContext.PushTransform(transform);

			Geometry geometry = ascGeometry;
			if (this.Direction == ListSortDirection.Descending)
				geometry = descGeometry;
			drawingContext.DrawGeometry(Brushes.Black, null, geometry);

			drawingContext.Pop();
		}
	}
}
