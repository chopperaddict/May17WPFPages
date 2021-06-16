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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace WPFPages
{
	/// <summary>
	/// Interaction logic for Page4.xaml
	/// </summary>
	public partial class Page4 : Page
	{
//		public SqlDbViewer tw = null;
		public Page4()
		{
			InitializeComponent();
			//DataContext = this;
		}
		private void Page1_Click(object sender, RoutedEventArgs e)
		{
			//Button btn = (Button)sender;
			//btn.FontSize = 28;
			this.NavigationService.Navigate(MainWindow._Page1);
		}
		private void Page2_Click(object sender, RoutedEventArgs e)
		{
			this.NavigationService.Navigate(MainWindow._Page2);
		}
		private void Page3_Click(object sender, RoutedEventArgs e)
		{
			this.NavigationService.Navigate(MainWindow._Page3);
		}
		private void Page4_Click(object sender, RoutedEventArgs e)
		{
			//if(tw != null) { tw.BringIntoView(); tw.Focus(); }
			//SqlDbViewer tw = new SqlDbViewer();
			//tw.Show();
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
		private void ExitButton_Click(object sender, RoutedEventArgs e)
		{ Application.Current.Shutdown(); }

		private void PrintFlowDoc_Click(object sender, RoutedEventArgs e)
		{
			//Print the contents of the FlowDocument named FlowDoc
			// Works very well indeed
			var str = XamlWriter.Save(FlowDoc);
			var stringReader = new System.IO.StringReader(str);
			var xmlReader = XmlReader.Create(stringReader);
			var CloneDoc = XamlReader.Load(xmlReader) as FlowDocument;

			//Now print using PrintDialog
			var pd = new PrintDialog();

			if (pd.ShowDialog().Value)
			{
				CloneDoc.PageHeight = pd.PrintableAreaHeight;
				CloneDoc.PageWidth = pd.PrintableAreaWidth;
				IDocumentPaginatorSource idocument = CloneDoc as IDocumentPaginatorSource;

				pd.PrintDocument(idocument.DocumentPaginator, "Printing FlowDocument");
			}
		}

		private void PrintFlowDocImage_Click(object sender, RoutedEventArgs e)
		{
			// Print  the entire FlowDoc object as vieed on the screen
			PrintDialog printDlg = new System.Windows.Controls.PrintDialog();

			if (printDlg.ShowDialog() == true)

			{

				printDlg.PrintVisual(this, "WPF Print");

			}

		}
		//public void OnSizeChangedEventHandler(object sender, SizeChangedEventArgs e)
		// Doesn't get called
		//{
		//	double h = RightGrid.Height;
		//	Size s = new Size();
		//	s = e.NewSize;
		//	FlowReader.Height = (double)s.Height;
			
		//}
	}
}
