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
	/// Interaction logic for testwin.xaml
	/// </summary>
	public partial class testwin : Window
	{
		public testwin()
		{
			InitializeComponent();
		}
		private void Border_Loaded(object sender, RoutedEventArgs e)
		{
			ShowBorderSize();
		}
		private void ShowBorderSize()
		{
//			double width = DemoBorder.ActualWidth;
			//double height = DemoBorder.ActualHeight;
//			SizeLabel.Content = string.Format("({0},{1})", width, height);
		}
		private void Border_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			ShowBorderSize();
		}
		/// <summary>  
		/// This method creates a dynamic FlowDocument. You can add anything to this  
		/// FlowDocument that you would like to send to the printer  
		/// </summary>  
		/// <returns></returns>  
		private FlowDocument CreateFlowDocument()
		{
			// Create a FlowDocument  
			FlowDocument doc = new FlowDocument();
			// Create a Section  
			Section sec = new Section();
			// Create first Paragraph  
			Paragraph p1 = new Paragraph();
			// Create and add a new Bold, Italic and Underline  
			Bold bld = new Bold();
			bld.Inlines.Add(new Run("First Paragraph"));
			Italic italicBld = new Italic();
			italicBld.Inlines.Add(bld);
			Underline underlineItalicBld = new Underline();
			underlineItalicBld.Inlines.Add(italicBld);
			// Add Bold, Italic, Underline to Paragraph  
			p1.Inlines.Add(underlineItalicBld);
			// Add Paragraph to Section  
			sec.Blocks.Add(p1);
			// Add Section to FlowDocument  
			doc.Blocks.Add(sec);
			return doc;
		}
		private void PrintSimpleTextButton_Click(object sender, RoutedEventArgs e)
		{
			// Create a PrintDialog  
			PrintDialog printDlg = new PrintDialog();
			//Show the Printer Selection Dialog
			//You can bypass this and the default printer will be used
			printDlg.ShowDialog();
			// Create a FlowDocument dynamically.  
			FlowDocument doc = CreateFlowDocument();
			//You need to pass the name of the FlowDocument defined in the xaml here
			//To have it print the entire contents of the FlowDocument
			//Youcan specify just certain paragraphs etc if required
			doc.Name = "FlowDoc";
			// Create IDocumentPaginatorSource from FlowDocument  
			IDocumentPaginatorSource idpSource = Flowdoc;
			// Call PrintDocument method to send document to printer  
			printDlg.PrintDocument(idpSource.DocumentPaginator, "Hello WPF Printing.");
		}

		private void PrintWindow_Click(object sender, RoutedEventArgs e)
		{
			PrintDialog printDlg = new PrintDialog();
			//Show the Printer Selection Dialog
			//You can bypass this and the default printer will be used
			printDlg.ShowDialog();
			//PrintVisual lets us print any Control as it appears on screen !!!!
			printDlg.PrintVisual(this, "Window Printing.");
		}

		private void ClosePanel_Click(object sender, RoutedEventArgs e)
		{
			//			NavigationService ns = NavigationService.GetNavigationService(dependencyObject: );
			//			MainWindow .MainPageHolder.NavigationService.Navigate(MainWindow._Page1);
			this.Close();
		}
		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
		//	int x;
		//	if (e.PreviousSize.Width == 0 && e.PreviousSize.Height == 0)
		//	{
		//		Testwin.Width = 800;
		//	}
		}

		private void Testwin_Loaded(object sender, RoutedEventArgs e)
		{
//			Testwin.Width = 800;
		}
	}
}
