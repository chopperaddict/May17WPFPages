using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


using WPFPages;

namespace WpfUI
{
	/// <summary>
	/// Interaction logic for Page1.xaml
	/// </summary>
	public partial class Page1 : Page
	{
		private Brush newbrush = null;
		private Brush currentbrush = null;
//		private Brush MouseoverBrush = Brushes.Green;
		private bool imagechanged = false;
		SqlDbViewer tw = null;
		public Page1()
		{
			InitializeComponent();
		}
		private void Page1_Click(object sender, RoutedEventArgs e)
		{
			//Button btn = (Button)sender;
			//btn.FontSize = 28;
//			this.NavigationService.Navigate(MainWindow._Page1);
		}
		private void Page2_Click(object sender, RoutedEventArgs e)
		{
//			this.NavigationService.Navigate(MainWindow._Page2);
		}
		private void Page3_Click(object sender, RoutedEventArgs e)
		{
///			this.NavigationService.Navigate(MainWindow._Page3);
		}
		private void Page4_Click(object sender, RoutedEventArgs e)
		{
//			this.NavigationService.Navigate(MainWindow._Page4);
		}

		private void Page5_Click(object sender, RoutedEventArgs e)
		{
			//			testwin tw = new testwin();
			//			tw.Show();
			if (tw != null) {
				tw.BringIntoView();
				return;
			}
			tw = new SqlDbViewer(-1);
			tw.Show();
		}
		private void ExitButton_Click(object sender, RoutedEventArgs e)
		{ Application.Current.Shutdown(); }

		private void MainWindowLoaded(object sender, RoutedEventArgs e)
		{
			//frame.NavigationService.Navigate(new Page2());
		}

		private void Button_MouseEnter(object sender, MouseEventArgs e)
		{
			Button b = (Button)sender;
			b.Background = Brushes.Green;
			e.Handled = true;

		}

		private void Button_MouseLeave(object sender, MouseEventArgs e)
		{
			Button b = (Button)sender;
			b.Background = Brushes.Gold;
			e.Handled = true;

		}
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);
		}
//		private static bool btn1state;
//		private static bool btn2state;
		private void Button_Click1(object sender, RoutedEventArgs e)
		{
			//if (btn2.IsEnabled)
			//	btn2.IsEnabled = false;
			//else
			//	btn2.IsEnabled = true;
		}

		private void Button_Click2(object sender, RoutedEventArgs e)
		{
			//if (btn1.IsEnabled)
			//	btn1.IsEnabled = false;
			//else
			//	btn1.IsEnabled = true;
		}

		private void Ellipse_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("There ya go !");
		}

		private void LoadButtonStack_Click(object sender, RoutedEventArgs e)
		{
			//this.NavigationService.Navigate(MainWindow.ButtonStackPanel);

		}

		private void PreviewMouseDown_Click(object sender, MouseButtonEventArgs e)
		{
			MessageBox.Show("This is spawned via a PreviewMouseDown trap");
		}

		private void ChangeColor_Click(object sender, MouseButtonEventArgs e)
		{
			if (currentbrush == null)
			{
				newbrush = new SolidColorBrush(Color.FromArgb(255, (byte)255, (byte)123, (byte)123));
				currentbrush = TextblockOne.Background;
			}
			if (TextblockOne.Background == newbrush)
			{
				TextblockOne.Background = currentbrush;
				TextblockOne.Text = "Set Check back to original !!";
			}
			else
			{
				TextblockOne.Background = newbrush;
				TextblockOne.Text = "Changed the checkbox status !!";
			}
			//Change the checked status of the Checkboc CheckMark as well.
			chkBox.IsChecked = !chkBox.IsChecked;

		}

		private void Mousedn_Click(object sender, MouseButtonEventArgs e)
		{
			double Sizereduce = 1.7;
			double Sizeincrease = 1.7;
			Image im = Vignette;
			BitmapImage img1 = new BitmapImage();
			img1.BeginInit();
			img1.UriSource = new Uri(@"C:\Users\ianch\source\repos\timcorey\WPFPages\Images\ian.jpg");
//			img1.Rotation = Rotation.Rotate90;
			img1.EndInit();
			BitmapImage img2 = new BitmapImage();
			img2.BeginInit();
			img2.UriSource = new Uri(@"C:\Users\ianch\source\repos\timcorey\WPFPages\Images\olwen.jpg");
			img2.EndInit();
			if (!imagechanged)
			{
				imagechanged = true;
				img2.Rotation = Rotation.Rotate90;
				im.Source = img1;
				VignetteText.Text = "Hi Ian";
				VignetteText.Foreground = Brushes.Red;
				VignetteText.FontStretch = FontStretches.UltraExpanded;
				VignetteText.FontSize = 36;
				ImageButton.Width /= Sizereduce;
				ImageButton.Height /= Sizereduce;


			}
			else
			{
				imagechanged = false;
				im.Source = img2;
				VignetteText.Text = "Hi Olwen";
				VignetteText.Foreground = Brushes.White;
				VignetteText.FontStretch = FontStretches.Normal;
				VignetteText.FontSize = 22;
				ImageButton.Width *= Sizeincrease;
				ImageButton.Height *= Sizeincrease;

			}
			//im.LayoutTransform = 

		}
		private void Mouseup_Click(object sender, MouseButtonEventArgs e)
		{
			//Image im = Vignette;
			//BitmapImage img = new BitmapImage();
			//img.BeginInit();
			//img.UriSource = new Uri(@"C:\Users\ianch\source\repos\timcorey\WPFPages\olwen.jpg");
			//img.EndInit();

			//im.Source = img;

		}

		private void CloseButton_Click(object sender, MouseButtonEventArgs e)
		{
			this.NavigationService.Navigate(MainWindow._Page0);

		}

		private void btn10_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Hmmmmmmmm");
		}
	}
}