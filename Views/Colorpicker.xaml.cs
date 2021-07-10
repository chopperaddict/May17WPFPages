using System;
using System . Collections . Generic;
using System . IO . Ports;
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
using System . Windows . Navigation;
using System . Windows . Shapes;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for Colorpicker.xaml
	/// </summary>
	public partial class Colorpicker : UserControl
	{
		public bool Loading = true;
		private double oPacity;

		public double opacity
		{
			get
			{
				return  oPacity;
			}
			set
			{
				 oPacity = value;
			}
		}

		public Colorpicker ( )
		{
			InitializeComponent ( );
			OpacitySlider . Value = 1;
			//RedSlider = Red;
			//GreenSlider = Green;
			//BlueSlider = Blue;
			RedSlider . Value = 128;
			GreenSlider . Value = 128;
			BlueSlider . Value = 128;
//			this . DataContext = this;
			Output . Refresh ( );
			Loading = false;
		}

		private void GreenSlider_ValueChanged ( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			if ( Loading )
				return;
			Color fill = Color . FromArgb ( Convert . ToByte ( OpacitySlider . Value ), 
				Convert . ToByte ( RedSlider . Value ),
				Convert . ToByte ( GreenSlider . Value ),
				Convert . ToByte ( BlueSlider . Value ) );
			Output . Fill = new SolidColorBrush ( fill );
			Output . Refresh ( );
		}

		private void RedSlider_ValueChanged ( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			if ( Loading )
				return;
			Color fill = Color . FromArgb ( Convert . ToByte ( OpacitySlider . Value ), 
				Convert . ToByte ( RedSlider . Value ),
				Convert . ToByte ( GreenSlider . Value ),
				Convert . ToByte ( BlueSlider . Value ) );
			Output . Fill = new SolidColorBrush ( fill );
			Output . Refresh ( );
		}

		private void BlueSlider_ValueChanged ( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			if ( Loading )return;
			Color fill = Color . FromArgb ( Convert . ToByte ( OpacitySlider . Value ), 				
				Convert . ToByte ( RedSlider . Value ),
				Convert . ToByte ( GreenSlider . Value ),
				Convert . ToByte ( BlueSlider . Value ) );
			Output . Fill = new SolidColorBrush ( fill );
			Output . Refresh ( );
		}

		private void OpacitySlider_ValueChanged ( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
		if ( Loading )
				return;
			opacity = OpacitySlider . Value;
			Color fill = Color . FromArgb ( Convert . ToByte ( opacity), Convert . ToByte ( RedSlider . Value ),
				Convert . ToByte ( GreenSlider . Value ),
				Convert . ToByte ( BlueSlider . Value ) );
			if ( Output == null )
			{
				Output . Fill = new SolidColorBrush ( fill );
				Output . Refresh ( );
			}
		}
	}
}
