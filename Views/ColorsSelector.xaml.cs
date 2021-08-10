using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Globalization;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Markup;
using System . Windows . Media;
using System . Windows . Media . Imaging;
using System . Windows . Shapes;

using WPFPages . ViewModels;

namespace WPFPages . Views
{
	public class Color2Hex : IValueConverter
	{

		public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			string HexString = "";
			HexString = value . ToString ( );
			return HexString;
		}

		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException ( );
		}
	}       /// <summary>
		/// Interaction logic for ColorsSelector.xaml
		/// </summary>
	public partial class ColorsSelector : Window
	{
		public bool Loading = true;
		private Point _startPoint
		{
			get; set;
		}
		private bool IsLeftButtonDown
		{
			get; set;
		}

		private double oPacity;

		public double opacity
		{
			get
			{
				return oPacity;
			}
			set
			{
				oPacity = value;
			}
		}
		private double redvalue;

		public double RedValue
		{
			get
			{
				return redvalue;
			}
			set
			{
				redvalue = value;
			}
		}

		private double greenValue;

		public double GreenValue
		{
			get
			{
				return greenValue;
			}
			set
			{
				greenValue = value;
			}
		}

		private double blueValue;

		public double BlueValue
		{
			get
			{
				return blueValue;
			}
			set
			{
				blueValue = value;
			}
		}

		private string SetRgbValue ( )
		{
			int r = ( int ) RedValue;
			int g = ( int ) GreenValue;
			int b = ( int ) BlueValue;
			int o = Convert . ToInt16 ( opacity );
			string output = "";
			output = o . ToString ( "X2" );
			output += r . ToString ( "X2" );
			output += g . ToString ( "X2" );
			output += b . ToString ( "X2" );
			opVal . Text = Convert . ToInt32 ( o ) . ToString ( "X2" );
			opRed . Text = Convert . ToInt32 ( r ) . ToString ( "X2" );
			opGreen . Text = Convert . ToInt32 ( g ) . ToString ( "X2" );
			opBlue . Text = Convert . ToInt32 ( b ) . ToString ( "X2" );
			opAll . Text = output;
			//	opAll . Text = Convert . ToInt32 ( output ) . ToString ( );
			return output;
		}
		public ColorsSelector ( string args )
		{
			string argscolor = "";
			InitializeComponent ( );
			DataContext = this;
			if ( args != "" )
			{
				//				args = this . Tag . ToString ( );
				// Need to strip leading # off for conversion to work
				if ( args [ 0 ] == '#' )
					argscolor = args . Substring ( 1 );
				else
					argscolor = args;
				string opac = "";
				string Red = "";
				string Grn = "";
				string Blu = "";

				// Only works if there is NO LEADING #
				var val = Convert . ToInt32 ( argscolor, 16 );
				opac = argscolor . Substring ( 0, 2 );
				Red = argscolor . Substring ( 2, 2 );
				Grn = argscolor . Substring ( 4, 2 );
				Blu = argscolor . Substring ( 6, 2 );
				int r = Convert . ToInt32 ( Red, 16 );
				int g = Convert . ToInt32 ( Grn, 16 );
				int b = Convert . ToInt32 ( Blu, 16 );
				int o = Convert . ToInt32 ( opac, 16 );// / 255;

				opacity = o;
				OpacitySlider . Value = o;
				RedValue = r;
				RedSlider . Value = r;
				GreenValue = g;
				GreenSlider . Value = g;
				BlueValue = b;
				BlueSlider . Value = b;
				Output . Opacity = o;
				Output . UpdateLayout ( );
				Loading = false;

				Color c = Color . FromRgb (
					Convert . ToByte ( RedValue ),
					Convert . ToByte ( GreenValue ),
					Convert . ToByte ( BlueValue ) );
				Output . Background = new SolidColorBrush ( c );
				Output . UpdateLayout ( );
				SetRgbValue ( );
			}
			else
			{

				opacity = 1;
				OpacitySlider . Value = 1;
				RedValue = 112;
				RedSlider . Value = 112;
				GreenValue = 158;
				GreenSlider . Value = 158;
				BlueValue = 218;
				BlueSlider . Value = 218;
				Output . Opacity = opacity;
				Output . UpdateLayout ( );
				Loading = false;
				Color c = Color . FromArgb (
					Convert . ToByte ( Output . Opacity ),
					Convert . ToByte ( RedValue ),
					Convert . ToByte ( GreenValue ),
					Convert . ToByte ( BlueValue ) );
				Output . Background = new SolidColorBrush ( c );
				Output . UpdateLayout ( );
				SetRgbValue ( );
				RedLevel . Text = Convert . ToInt32 ( RedValue ) . ToString ( );
				GreenLevel . Text = Convert . ToInt32 ( GreenValue ) . ToString ( );
				BlueLevel . Text = Convert . ToInt32 ( BlueValue ) . ToString ( );

			}
		}


		private void RedSlider_ValueChanged ( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			if ( Loading )
				return;
			RedValue = e . OldValue;
			RedLevel . Text = Convert . ToInt32 ( e . NewValue ) . ToString ( );
			Color c = Color . FromArgb (
				Convert . ToByte ( Output . Opacity ),
				Convert . ToByte ( RedSlider . Value ),
				Convert . ToByte ( GreenSlider . Value ),
				Convert . ToByte ( BlueSlider . Value ) );
			Output . Background = new SolidColorBrush ( c );
			Output . Opacity = opacity;
			Output . UpdateLayout ( );
			SetRgbValue ( );
		}

		private void GreenSlider_ValueChanged ( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			if ( Loading )
				return;
			GreenValue = e . OldValue;
			GreenLevel . Text = Convert . ToInt32 ( e . NewValue ) . ToString ( );
			Color c = Color . FromArgb (
				Convert . ToByte ( Output . Opacity ),
				Convert . ToByte ( RedSlider . Value ),
				Convert . ToByte ( GreenSlider . Value ),
				Convert . ToByte ( BlueSlider . Value ) );
			Output . Background = new SolidColorBrush ( c );
			Output . Opacity = opacity;
			Output . UpdateLayout ( );
			SetRgbValue ( );
		}
		private void BlueSlider_ValueChanged ( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			if ( Loading )
				return;
			BlueValue = e . OldValue;
			BlueLevel . Text = Convert . ToInt32 ( e . NewValue ) . ToString ( );
			Color c = Color . FromArgb (
				Convert . ToByte ( Output . Opacity ),
				Convert . ToByte ( RedSlider . Value ),
				Convert . ToByte ( GreenSlider . Value ),
				Convert . ToByte ( BlueSlider . Value ) );
			Output . Background = new SolidColorBrush ( c );
			Output . Opacity = opacity;
			Output . UpdateLayout ( );
			SetRgbValue ( );
		}

		private void OpacitySlider_ValueChanged ( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			int newopac = 0;
			if ( Loading )
				return;
			opacity = e . OldValue;
			double pac = e . NewValue;// * 255;
			newopac = Convert . ToInt32 ( pac );
			opacityLevel . Text = Convert . ToInt32 ( newopac ) . ToString ( );
			Output . Opacity = newopac;/// 255;
			Color c = Color . FromArgb (
				Convert . ToByte ( newopac ),
				Convert . ToByte ( RedSlider . Value ),
				Convert . ToByte ( GreenSlider . Value ),
				Convert . ToByte ( BlueSlider . Value ) );
			Output . Background = new SolidColorBrush ( c );
			double d = Output . Opacity;/// 255;
			Output . UpdateLayout ( );
			SetRgbValue ( );
		}

		private void ClipSave_Click ( object sender, RoutedEventArgs e )
		{
			Clipboard . SetText ( opAll . Text );
		}

		private void Window_PreviewKeyDown ( object sender, KeyEventArgs e )
		{
			if ( e . Key == Key . Enter )
				ClipSave_Click ( sender, null );
			else if ( e . Key == Key . Escape )
				Close ( );
		}

		#region Helpers
		public static Brush BrushFromColors ( Color color )
		{
			Brush brush = new SolidColorBrush ( color );
			return brush;
		}
		public static Brush BrushFromHashString ( string color )
		{
			//Must start with  '#'
			string s = color . ToString ( );
			if ( !s . Contains ( "#" ) )
				return BrushFromColors ( Colors . Transparent );
			Brush brush = ( Brush ) new BrushConverter ( ) . ConvertFromString ( color );
			return brush;
		}
		#endregion Helpers

		private void Window_Loaded ( object sender, RoutedEventArgs e )
		{
			int x = 0;
		}

		#region Drag RGB color data elsewhere
		private void OpAll_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
			_startPoint = e . GetPosition ( null );
			// Make sure the left mouse button is pressed down so we are really moving a record
			if ( e . LeftButton == MouseButtonState . Pressed )
			{
				IsLeftButtonDown = true;
			}

		}

		private void OpAll_PreviewMouseMove ( object sender, MouseEventArgs e )
		{
			Point mousePos = e . GetPosition ( null );
			Vector diff = _startPoint - mousePos;
			if ( IsLeftButtonDown && e . LeftButton == MouseButtonState . Pressed )
			{
				// We are dragging from the RGB color code text field
				//Working string version
				string str = "COLOR:" + opAll . Text;
				// Send it as XAML so recipient can recognize it. - DOESNT get Sent ????
				//string xaml = XamlWriter . Save ( str);
				//					string dataFormat = DataFormats .Xaml;
				DataObject dataObject = new DataObject ( DataFormats . Text, str );
				var  v = DragDrop . DoDragDrop (
				sender as TextBlock,
				dataObject,
				DragDropEffects . Copy );
				IsLeftButtonDown = false;
			}
		}

		private void OpAll_PreviewDragOver ( object sender, DragEventArgs e )
		{
			//NEVER TRIGGERED
			Point mousePos = e . GetPosition ( opAll );
			Vector diff = _startPoint - mousePos;
		}
		#endregion Drag RGB color data elsewhere

		private void OpAll_DragEnter ( object sender, DragEventArgs e )
		{
			//NEVER TRIGGERED
			e . Effects = ( DragDropEffects ) DragDropEffects . Move;
			Mouse . SetCursor ( Cursors . Hand );
		}

		private void OpAll_PreviewQueryContinueDrag ( object sender, QueryContinueDragEventArgs e )
		{
			//Mouse . SetCursor ( Cursors . Hand );
			e . Action = DragAction . Continue;
		}
	}
}
