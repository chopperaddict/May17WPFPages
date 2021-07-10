using System;
using System . Collections . Generic;
using System . Globalization;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Imaging;
using System . Windows . Shapes;

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
			int r = (int)RedValue;
			int g = ( int ) GreenValue;
			int b = ( int ) BlueValue;
			int o = Convert.ToInt16(opacity * 255);
			string output = "";
			output =  o . ToString ( "X2" );
			output += r . ToString ( "X2" );
			output += g . ToString ( "X2" );
			output += b . ToString ( "X2" );
			opRed.Text = r . ToString ( "X2" );
			opGreen . Text = g . ToString ( "X2" );
			opBlue . Text = b . ToString ( "X2" );
			//////////////opVal . Text = o . ToString ( "X2" );
			opAll . Text = output;
			return output;
		}
		public ColorsSelector ( )
		{
			InitializeComponent ( );
			opacity = 1;
			OpacitySlider . Value = 1;
			//RedSlider = Red;
			//GreenSlider = Green;
			//BlueSlider = Blue;
			RedValue = 112;
			RedSlider . Value = 112;
			GreenValue = 158;
			GreenSlider . Value = 158;
			BlueValue = 218;
			BlueSlider . Value = 218;
			//this . DataContext = this;
			Output . Opacity = opacity;
			Output . Refresh ( );
			Loading = false;
			//Binding binding = new Binding (RedValue.ToString() );
			//binding . Source = RedValue;
			//opRed . SetBinding ( TextBlock . TextProperty, binding );
			//binding = new Binding ( "Text" );
			//binding . Source = GreenValue;
			//opGreen. SetBinding ( TextBlock . TextProperty, binding );
			//binding = new Binding ( "Text" );
			//binding . Source = BlueValue;
			//opBlue . SetBinding ( TextBlock . TextProperty, binding );
			//binding = new Binding ( "Text" );
			//binding . Source = opacity;
			//opVal. SetBinding ( TextBlock . TextProperty, binding );
			Color c = Color . FromRgb ( 
				Convert . ToByte ( RedValue ),
				Convert . ToByte ( GreenValue ),
				Convert . ToByte ( BlueValue ) );
			Output . Fill = new SolidColorBrush ( c );
			Output . Refresh ( );
			//opRed . Text = Convert . ToInt32 ( RedValue ) . ToString ( );
			//opGreen . Text = Convert . ToInt32 ( GreenValue ) . ToString ( );
			//opBlue . Text = Convert . ToInt32 ( BlueValue ) . ToString ( );
			//opVal . Text = Convert . ToInt32 ( opacity * 100) . ToString ( );
			SetRgbValue ( );
		}


		private void RedSlider_ValueChanged ( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			if ( Loading )
				return;
			RedValue = e . OldValue;
			Color c = Color . FromRgb (
				Convert . ToByte ( RedSlider . Value ),
				Convert . ToByte ( GreenSlider . Value ),
				Convert . ToByte ( BlueSlider . Value ) );
			Output . Fill = new SolidColorBrush ( c);
			Output . Opacity = opacity;
			Output . Refresh ( );
			SetRgbValue ( );
			//			opRed. Text = RedSlider . Value . ToString ( );
		}

		private void GreenSlider_ValueChanged ( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			if ( Loading )
				return;
			GreenValue = e . OldValue;
			Color c = Color . FromRgb (
				Convert . ToByte ( RedSlider . Value ),
				Convert . ToByte ( GreenSlider . Value ),
				Convert . ToByte ( BlueSlider . Value ) );
			Output . Fill = new SolidColorBrush ( c );
			Output . Opacity = opacity;
			Output . Refresh ( );
			SetRgbValue ( );
		}
		private void BlueSlider_ValueChanged ( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			if ( Loading )
				return;
			BlueValue = e . OldValue;
			Color c = Color . FromRgb (
				Convert . ToByte ( RedSlider . Value ),
				Convert . ToByte ( GreenSlider . Value ),
				Convert . ToByte ( BlueSlider . Value ) );
			Output . Fill = new SolidColorBrush ( c );
			Output . Opacity = opacity;
			Output . Refresh ( );
			SetRgbValue ( );
		}

		private void OpacitySlider_ValueChanged ( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			if ( Loading )
				return;
			opacity = e . OldValue;
			//opacity = OpacitySlider . Value;
			Color c = Color . FromRgb (
				Convert . ToByte ( RedSlider . Value ),
				Convert . ToByte ( GreenSlider . Value ),
				Convert . ToByte ( BlueSlider . Value ) );
			Output . Fill = new SolidColorBrush ( c );
			Output . Opacity = opacity;
			Output . Refresh ( );
//			opVal. Text = Convert . ToInt32 ( opacity * 100 ) . ToString ( );
			SetRgbValue ( );
		}

		private void ClipSave_Click ( object sender, RoutedEventArgs e )
		{
			Clipboard . SetText ( opAll . Text );
		}
	}
}
