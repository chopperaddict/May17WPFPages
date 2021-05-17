using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

using WpfUI;

namespace WPFPages
{
	/// <summary>
	/// Interaction logic for Page3.xaml
	/// </summary>
	public partial class Page3 : Page
	{
		private enum SelectedShape { None, Circle, Rectangle }
		SqlDbViewer tw = null as SqlDbViewer;

		//		private SelectedShape Shape1 = SelectedShape.None;
		public Page3() {
			InitializeComponent();
		}

		private void Page1_Click(object sender, RoutedEventArgs e) {
			this.NavigationService.Navigate(MainWindow._Page1);
		}
		private void Page2_Click(object sender, RoutedEventArgs e) {
			this.NavigationService.Navigate(MainWindow._Page2);
		}
		private void Page3_Click(object sender, RoutedEventArgs e) {
			this.NavigationService.Navigate(MainWindow._Page3);
		}
		private void Page4_Click(object sender, RoutedEventArgs e) {
			this.NavigationService.Navigate(MainWindow._Page4);
		}

		private void Page1Button_Click(object sender, RoutedEventArgs e) {
			this.NavigationService.Navigate(MainWindow._Page1);

		}
		private void Page2Button_Click(object sender, RoutedEventArgs e) {
			this.NavigationService.Navigate(MainWindow._Page2);
		}
		private void Page3Button_Click(object sender, RoutedEventArgs e) {
			this.NavigationService.Navigate(MainWindow._Page3);
		}
		private void Page4Button_Click(object sender, RoutedEventArgs e) {
			this.NavigationService.Navigate(MainWindow._Page4);
		}
		//private void Page5Button_Click(object sender, RoutedEventArgs e)
		//{
		//	testwin tw = new testwin();
		//	tw.Show();
		//}
		private void Page5Button_Click(object sender, RoutedEventArgs e) {
			if (tw != null) {
				tw.BringIntoView();
				return;
			}

			tw = new SqlDbViewer(-1);
			tw.Show();
		}
		private void ExitButton_Click(object sender, RoutedEventArgs e) { Application.Current.Shutdown(); }



		private void Button_PreviewMouseMove(object sender, MouseEventArgs e) {
			this.Background = Brushes.Green;
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			Window main = Application.Current.MainWindow;
			//			main.NavigationService.Navigate(MainWindow._Page1);
		}

		private void CloseButton_Click(object sender, MouseButtonEventArgs e) {
			this.NavigationService.Navigate(MainWindow._Page0);

		}

		private void canvasArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			//Show a circle
			// NB:  canvasPosition is thge x:Name of the Canvas
			DrawShape("CIRCLE", canvasPosition, e);
		}

		private void canvasArea_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
			//Show a rectangle
			DrawShape("RECTANGLE", canvasPosition, e);
		}

		private void DrawShape(string Type, Canvas canvas, MouseButtonEventArgs e) 
		{
			Shape Rendershape = null;
			RadialGradientBrush brush = new RadialGradientBrush();

			if (Type == "CIRCLE") {
				SelectedShape Shape1 = SelectedShape.Circle;
				Rendershape = new Ellipse() { Height = 40, Width = 40 };
				brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#f23207"), 0.250));
				brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#f57b5f"), 0.500));
				brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#f7b7a8"), 1));
				Rendershape.Fill = brush;
			}
			else {
				SelectedShape Shape1 = SelectedShape.Rectangle;
				Rendershape = new Rectangle() { Height = 40, Width = 40 };
				brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#0d4091"), 0.250));
				brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#366bbf"), 0.500));
				brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#80acf2"), 1));
				Rendershape.Fill = brush;
			}

			double px = (double)e.GetPosition(canvas).X;
			double currentX = Rendershape.Height;
			double py = (double)e.GetPosition(canvas).Y;
			double currentY = Rendershape.Width;
			double framewidth = canvas.ActualWidth;
			double frameheight = canvas.ActualHeight;
			Point position = canvas.PointToScreen(new Point(0d, 0d)),
				controlPosition = this.PointToScreen(new Point(0d, 0d));

			position.X -= controlPosition.X;
			position.Y -= controlPosition.Y;
			if (px + 45L > framewidth) {
				double newX = framewidth - 45;
				px = newX;
			}
			if (py + 45L > frameheight) {
				double newY = frameheight - 45;
				py = newY;
			}
			Canvas.SetLeft(Rendershape, px);
			Canvas.SetTop(Rendershape, py);
			canvasPosition.Children.Add(Rendershape);
		}
		private void Button_Click_1(object sender, RoutedEventArgs e) {
			Button b = new Button();
//			b.IsPressed
		}
		private void customControl_Click(object sender, RoutedEventArgs e) {
			txtBlock.Text = "You have just click your custom control";
		}

	}
}
