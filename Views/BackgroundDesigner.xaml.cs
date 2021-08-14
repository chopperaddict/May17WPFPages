using System;
using System . Collections . Generic;
using System . Globalization;
using System . Linq;
using System . Net;
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

using WpfUI;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for ColorsSelector.xaml
	/// <summary>
	/// Interaction logic for BackgroundDesigner.xaml
	/// </summary>
	public partial class BackgroundDesigner : Window
	{
		private LinearGradientBrush lgb = new LinearGradientBrush ( );
		private GradientDisplay gw = null;
		public bool Loading = true;
		private bool DisplayWindowActive = false;
		private bool ToggleActive
		{
			get; set;
		}
		private int GradientStyle
		{
			get; set;
		}
		private double GradientValue { get; set; }

		private int ActivePane
		{
			get; set;
		}
		private Color ActiveColor
		{
			get; set;
		}
		private Point _startPoint { get; set; }
		private bool IsLeftButtonDown { get; set; }

		private Color Colors1
		{
			get; set;
		}
		private Color Colors2
		{
			get; set;
		}
		private Color Colors3
		{
			get; set;
		}
		private Color Colors4
		{
			get; set;
		}

		private Brush Brush1
		{
			get; set;
		}
		private Brush Brush2
		{
			get; set;
		}
		private Brush Brush3
		{
			get; set;
		}
		private Brush Brush4
		{
			get; set;
		}
		private double oPacity;

		public double OpacityValue
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

		private string ShowHexValues ( )
		{
			// Called when a slider chages a value
			//Save colors on a per panel basis
			int r = ( int ) RedValue;
			int g = ( int ) GreenValue;
			int b = ( int ) BlueValue;
			if ( OpacityValue == 0 )
				OpacityValue = 255;
			int o = Convert . ToInt16 ( OpacityValue );
			string output = "";
			output = o . ToString ( "X2" );
			output += r . ToString ( "X2" );
			output += g . ToString ( "X2" );
			output += b . ToString ( "X2" );
			//opVal . Text = Convert . ToInt32 ( o ) . ToString ( "X2" );
			//			opRed . Text = Convert . ToInt32 ( r ) . ToString ( "X2" );
			//opGreen . Text = Convert . ToInt32 ( g ) . ToString ( "X2" );
			//opBlue . Text = Convert . ToInt32 ( b ) . ToString ( "X2" );
			opAll . Text = output;
			return output;
		}
		public BackgroundDesigner ( string args )
		{
			string argscolor = "";
			InitializeComponent ( );
			DataContext = this;
			GradientStyle = 1;
			ToggleActive = true;
			if ( ActivePane == 0 )
				ActivePane = 1;
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
				opac = argscolor . Substring ( 0 , 2 );
				Red = argscolor . Substring ( 2 , 2 );
				Grn = argscolor . Substring ( 4 , 2 );
				Blu = argscolor . Substring ( 6 , 2 );
				int r = Convert . ToInt32 ( Red, 16 );
				int g = Convert . ToInt32 ( Grn, 16 );
				int b = Convert . ToInt32 ( Blu, 16 );
				int o = Convert . ToInt32 ( opac, 16 );
				if ( o == 0 )
					o = 255;
				OpacityValue = o;
				OpacitySlider . Value = o;
				RedValue = r;
				RedSlider . Value = r;
				GreenValue = g;
				GreenSlider . Value = g;
				BlueValue = b;
				BlueSlider . Value = b;
				Output1 . Opacity = o;

				//Create colors and brushes
				Color c = Color . FromArgb (
					Convert . ToByte ( OpacityValue ),
					Convert . ToByte ( RedValue ),
					Convert . ToByte ( GreenValue ),
					Convert . ToByte ( BlueValue ) );
				Colors1 = c;
				Brush1 = new SolidColorBrush ( Colors1 );
				GradientValue = 50;
				Loading = false;
				ActivePane = 1;
				Btn1 . Content = "Active...";
				SaveBrush ( c );
				DisplayPanel ( c );
				Show ( );
			}
			else
			{
				Show ( );
				this . Refresh ( );
				GradientValue = 50;

				CreateAllBrushes ( );
				this . Refresh ( );
				Loading = false;
				ActivePane = 1;
				Btn1 . Content = "Active...";
				ActivePane = 1;
				SetSliders ( Colors1 );
				Show ( );
			}
			// Set style to vertical grid (1) or (2)=diagonal
			GradientStyle = 1;
			Utils . SetupWindowDrag ( this );
			CreateGradient ( );
		}
		private void CreateAllBrushes ( )
		{
			OpacityValue = 255;
			RedValue = 255;
			GreenValue = 0;
			BlueValue = 0;
			//Create colors and brushes
			Color c = Color . FromArgb (
					Convert . ToByte ( OpacityValue ),
					Convert . ToByte ( RedValue ),
					Convert . ToByte ( GreenValue ),
					Convert . ToByte ( BlueValue ) );
			Colors1 = c;
			Loading = false;
			ActivePane = 1;
			SaveBrush ( Colors1 );
			PaintOutput ( Colors1 );
			this . Refresh ( );
			OpacityValue = 255;
			RedValue = 0;
			GreenValue = 255;
			BlueValue = 0;
			//Create colors and brushes
			c = Color . FromArgb (
					Convert . ToByte ( OpacityValue ) ,
					Convert . ToByte ( RedValue ) ,
					Convert . ToByte ( GreenValue ) ,
					Convert . ToByte ( BlueValue ) );
			Colors2 = c;
			Loading = false;
			ActivePane = 2;
			SaveBrush ( Colors2 );
			PaintOutput ( Colors2 );
			this . Refresh ( );
			OpacityValue = 255;
			RedValue = 0;
			GreenValue = 0;
			BlueValue = 255;
			//Create colors and brushes
			c = Color . FromArgb (
					Convert . ToByte ( OpacityValue ) ,
					Convert . ToByte ( RedValue ) ,
					Convert . ToByte ( GreenValue ) ,
					Convert . ToByte ( BlueValue ) );
			Colors3 = c;
			Loading = false;
			ActivePane = 3;
			SaveBrush ( Colors3 );
			PaintOutput ( Colors3 );
			this . Refresh ( );
		}
		private void DisplayPanel ( Color c )
		{
			SetSysColorsFromColor ( c );
			SaveBrush ( c );
			PaintOutput ( c );
			SaveSettings ( c );
			ShowHexValues ( );
			SetSliders ( c );
		}
		#region SLIDER Handlers
		private void OpacitySlider_ValueChanged ( object sender , RoutedPropertyChangedEventArgs<double> e )
		{
			Brush brush;
			int newopac = 0;
			if ( Loading )
				return;
			OpacityValue = e . NewValue;
			if ( OpacityValue == 0 )
				OpacityValue = 1;

			double pac = e . NewValue;
			newopac = Convert . ToInt32 ( pac );
			if ( newopac == 0 )
				newopac = 1;
			Output1 . Opacity = newopac;/// 255;
			opacityLevel . Text = Convert . ToInt32 ( pac ) . ToString ( );
			// Update the color with new Red Value
			Color c = Color . FromArgb ( Convert . ToByte ( OpacityValue ), Convert . ToByte ( RedValue ), Convert . ToByte ( GreenValue ), Convert . ToByte ( BlueValue ) );
			Loading = true;
			SaveBrush ( c );
			DisplayPanel ( c );
			CreateGradient ( );
			Loading = false;
			return;
		}
		private void RedSlider_ValueChanged ( object sender , RoutedPropertyChangedEventArgs<double> e )
		{
			if ( Loading )
				return;
			RedValue = e . NewValue;
			//			RedLevel . Text = Convert . ToInt32 ( e . NewValue ) . ToString ( );
			// Update the color with new Red Value
			Color c= Color . FromArgb ( Convert . ToByte ( OpacityValue), Convert . ToByte ( RedValue), Convert . ToByte ( GreenValue), Convert . ToByte ( BlueValue) );
			Loading = true;
			SaveBrush ( c );
			DisplayPanel ( c );
			CreateGradient ( );
			Loading = false;
			return;
		}

		private void GreenSlider_ValueChanged ( object sender , RoutedPropertyChangedEventArgs<double> e )
		{
			if ( Loading )
				return;
			GreenValue = e . NewValue;
			GreenLevel . Text = Convert . ToInt32 ( e . NewValue ) . ToString ( );
			Color c = Color . FromArgb ( Convert . ToByte ( OpacityValue ), Convert . ToByte ( RedValue ), Convert . ToByte ( GreenValue ), Convert . ToByte ( BlueValue ) );
			Loading = true;
			SaveBrush ( c );
			DisplayPanel ( c );
			CreateGradient ( );
			Loading = false;
			return;
		}
		private void BlueSlider_ValueChanged ( object sender , RoutedPropertyChangedEventArgs<double> e )
		{
			if ( Loading )
				return;
			BlueValue = e . NewValue;
			BlueLevel . Text = Convert . ToInt32 ( e . NewValue ) . ToString ( );
			Color c = Color . FromArgb ( Convert . ToByte ( OpacityValue ), Convert . ToByte ( RedValue ), Convert . ToByte ( GreenValue ), Convert . ToByte ( BlueValue ) );
			Loading = true;
			SaveBrush ( c );
			DisplayPanel ( c );
			CreateGradient ( );
			Loading = false;
			return;
		}
		#endregion SLIDER Handlers

		private void SaveSettings ( Color c )
		{
			if ( ActivePane == 1 )
				Colors1 = c;
			else if ( ActivePane == 2 )
				Colors2 = c;
			else if ( ActivePane == 3 )
				Colors3 = c;
			//			else if ( ActivePane == 4 )
			//				Colors4 = c;
		}

		private void ClipSave_Click ( object sender , RoutedEventArgs e )
		{
			//Clipboard . SetText ( opAll . Text );
		}

		#region Panel Activation buttons

		private void Button1_Click ( object sender , RoutedEventArgs e )
		{
			Btn1 . Content = "ACTIVE...";
			Btn2 . Content = "";
			Btn3 . Content = "";
			Btn4 . Content = "";
			ActivePane = 1;
			Color c = Colors1;
			ActiveColor = Colors1;
			SetSliders ( c );
			ShowHexValues ( );
			Output1 . Background = Brush1;
			//			Output1 . Background = new SolidColorBrush ( c );
			Output1 . Refresh ( );
		}
		private void Button2_Click ( object sender , RoutedEventArgs e )
		{
			Btn1 . Content = "";
			Btn2 . Content = "ACTIVE...";
			Btn3 . Content = "";
			Btn4 . Content = "";
			ActivePane = 2;
			Color c = Colors2;
			ActiveColor = Colors2;
			//			c = GetColorsFromRgb ( 2);
			Output2 . Background = Brush2;
			//			Output2 . Background = new SolidColorBrush ( c );
			SetSliders ( c );
			Output2 . Refresh ( );
		}

		private void Button3_Click ( object sender , RoutedEventArgs e )
		{
			Btn1 . Content = "";
			Btn2 . Content = "";
			Btn3 . Content = "ACTIVE...";
			Btn4 . Content = "";
			ActivePane = 3;
			Color c = Colors3;
			ActiveColor = Colors3;
			//			c = GetColorsFromRgb ( 3);
			Output3 . Background = Brush3;
			//			Output2 . Background = new SolidColorBrush ( c );
			//SetSliders ( c );
			//Output3 . Refresh ( );

		}
	
		#endregion Active buttons

		private void PaintOutput ( Color c )
		{
			switch ( ActivePane )
			{
				case 1:
					Colors1 = c;
					Output1 . Background = Brush1;
					break;
				case 2:
					Colors2 = c;
					Output2 . Background = Brush2;
					break;
				case 3:
					Colors3 = c;
					Output3 . Background = Brush3;
					break;
				case 4:
					Colors4 = c;
					Final . Background = new SolidColorBrush ( c );
					if ( DisplayWindowActive )
					{
						gw . Background = Final . Background;
					}
					break;
			}
		}

		#region Brush Helpers
		public void SetSysColorsFromColor ( Color color )
		{
			string s = color. ToString ( ) . Substring ( 1 );
			string os = s . Substring ( 0, 2 );
			string rs = s . Substring ( 2, 2 );
			string gs = s . Substring ( 4, 2 );
			string bs = s . Substring ( 6, 2 );
			int o = ( int ) Convert . ToInt32 ( os, 16 );
			int r = ( int ) Convert . ToInt32 ( rs, 16 );
			int g = ( int ) Convert . ToInt32 ( gs, 16 );
			int b = ( int ) Convert . ToInt32 ( bs, 16 );
			OpacityValue = o;
			RedValue = r;
			GreenValue = g;
			BlueValue = b;
		}
		public static Brush BrushFromColors ( Color color )
		{
			Brush brush = new SolidColorBrush ( color );
			return brush;
		}
		public string ColorstringFromBrush(Brush brush)
		{
			string output = "";
			output = brush . ToString ( );
			return output;
		}
		public static Brush BrushFromHashString ( string color )
		{
			//Must start with  '#'
			string s = color . ToString ( );
			if ( !s . Contains ( "#" ) )
				s = "#" + s;
			Brush brush = ( Brush ) new BrushConverter ( ) . ConvertFromString ( s );
			return brush;
		}
		#endregion Helpers

		private void SaveBrush ( Color c )
		{
			if ( ActivePane == 1 )
				Brush1 = BrushFromColors ( c );
			else if ( ActivePane == 2 )
				Brush2 = BrushFromColors ( c );
			else if ( ActivePane == 3 )
				Brush3 = BrushFromColors ( c );
			else if ( ActivePane == 4 )
				Brush4 = BrushFromColors ( c );
		}
		private void SetSliders ( Color c )
		{
			string s = c. ToString ( ) . Substring ( 1 );
			string os = s . Substring ( 0, 2 );
			string rs = s . Substring ( 2, 2 );
			string gs = s . Substring ( 4, 2 );
			string bs = s . Substring ( 6, 2 );
			int o = ( int ) Convert . ToInt32 ( os, 16 );
			int r = ( int ) Convert . ToInt32 ( rs, 16 );
			int g = ( int ) Convert . ToInt32 ( gs, 16 );
			int b = ( int ) Convert . ToInt32 ( bs, 16 );
			Loading = true;
			OpacitySlider . Value = o;
			OpacitySlider . Refresh ( );
			RedSlider . Value = r;
			RedSlider . Refresh ( );
			GreenSlider . Value = g;
			GreenSlider . Refresh ( );
			BlueSlider . Value = b;
			BlueSlider . Refresh ( );
			Gradientstop.Value = GradientValue;
			Gradientstop . Refresh ( );
			Loading = false;
		}

		#region mouse & button handlers
		private void Output_PreviewMouseLeftButtonDown ( object sender , MouseButtonEventArgs e )
		{
			ActivePane = 1;
			Button1_Click ( null , null );
			DisplayPanel ( Colors1 );
			Output1 . Refresh ( );
			Btn4 . Refresh ( );
		}
		private void Output2_PreviewMouseLeftButtonDown ( object sender , MouseButtonEventArgs e )
		{
			ActivePane = 2;
			Button2_Click ( null , null );
			DisplayPanel ( Colors2 );
			Output2 . Refresh ( );
			Btn4 . Refresh ( );
		}

		private void Output3_PreviewMouseLeftButtonDown ( object sender , MouseButtonEventArgs e )
		{
			ActivePane = 3;
			Button3_Click ( null , null );
			DisplayPanel ( Colors3 );
			Output3 . Refresh ( );
			Btn4 . Refresh ( );
		}
		private void Output4_PreviewMouseLeftButtonDown ( object sender , MouseButtonEventArgs e )
		{
			//ActivePane = 4;
			//Button4_Click ( null, null );
			//DisplayPanel (Colors4 );
			//Final. Refresh ( );
			Btn4 . Refresh ( );
		}
		private void Final_PreviewMouseRightButtonDown ( object sender , MouseButtonEventArgs e )
		{
			Clipboard . SetText ( GradientText . Text );
		}
		#endregion mouse & button handlers

		#region Drag RGB color data elsewhere
		private void OpAll_PreviewMouseLeftButtonDown ( object sender , MouseButtonEventArgs e )
		{
			_startPoint = e . GetPosition ( null );
			// Make sure the left mouse button is pressed down so we are really moving a record
			if ( e . LeftButton == MouseButtonState . Pressed )
			{
				IsLeftButtonDown = true;
			}

		}

		private void OpAll_PreviewMouseMove ( object sender , MouseEventArgs e )
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
				try
				{
					DataObject dataObject = new DataObject ( DataFormats . Text, str );
					var v = DragDrop . DoDragDrop (
				sender as TextBlock,
				dataObject,
				DragDropEffects . Copy );
				}
				catch(Exception ex )
				{
					Console . WriteLine ( "Drag Error encountered" );
				}
				IsLeftButtonDown = false;
			}
		}

		private void OpAll_PreviewDragOver ( object sender , DragEventArgs e )
		{
			//NEVER TRIGGERED
			Point mousePos = e . GetPosition ( opAll );
			Vector diff = _startPoint - mousePos;
		}

		private void OpAll_DragEnter ( object sender , DragEventArgs e )
		{
			//NEVER TRIGGERED
			e . Effects = ( DragDropEffects ) DragDropEffects . Move;
			Mouse . SetCursor ( Cursors . Hand );
		}

		private void OpAll_PreviewQueryContinueDrag ( object sender , QueryContinueDragEventArgs e )
		{
			//Mouse . SetCursor ( Cursors . Hand );
			e . Action = DragAction . Continue;
		}
		#endregion Drag RGB color data elsewhere

		private void LinkPanels_Click ( object sender , RoutedEventArgs e )
		{
			GradientStyle = 3;  // horizontal
			VerticalOption . Content = "Horizontal Active ...";
			CreateGradient ( );
			e . Handled = true;
			if ( DisplayWindowActive && gw != null )
				gw . Background = Final . Background;
		}

		private void VerticalOption_Click ( object sender , RoutedEventArgs e )
		{
			GradientStyle = 1;
			VerticalOption . Content = "Vertical Active ...";
			CreateGradient ( );
			e . Handled = true;
			if ( DisplayWindowActive && gw != null )
				gw . Background = Final . Background;
		}

		private void DiagonalOption_Click ( object sender , RoutedEventArgs e )
		{
			GradientStyle = 2;
			DiagonalOption . Content = "Diagonal Active ...";
			CreateGradient ( );
			e . Handled = true;
			if ( DisplayWindowActive && gw != null )
				gw . Background = Final . Background;
		}
		private void ToggleOption_Click ( object sender , RoutedEventArgs e )
		{
			// Reverse Final panel display
				ToggleActive = !ToggleActive;
			CreateGradient ( );
			if ( DisplayWindowActive && gw != null )
				gw . Background = Final . Background;
		}
		private void VerticalOption_PreviewMouseMove ( object sender , MouseEventArgs e )
		{
		}
		#region Gradient creation methods
		private void CreateGradient ( )
		{
			if ( GradientStyle == 1 )
			{
				Final . Background = CreateVerticalGradient ( true );
				VBtn . Background = Final . Background;
				HBtn . Background = CreateHorizontalGradient ( false );
				DBtn . Background = CreateDiagonalGradient ( false );
			}
			else if ( GradientStyle == 2 )
			{
				Final . Background = CreateDiagonalGradient ( true );
				DBtn . Background = Final . Background;
				HBtn . Background = CreateHorizontalGradient ( false );
				VBtn . Background = CreateVerticalGradient ( false );
			}
			else if ( GradientStyle == 3 )
			{
				Final . Background = CreateHorizontalGradient ( true );
				HBtn . Background = Final . Background;
				VBtn . Background = CreateVerticalGradient ( false );
				DBtn . Background = CreateDiagonalGradient ( false );
			}
			if ( DisplayWindowActive && gw != null )
				gw . Background = Final . Background; ;

		}
		private LinearGradientBrush CreateVerticalGradient ( bool isFull )
		{
			LinearGradientBrush lgb = new LinearGradientBrush ( );
			// 1st color
			string s = Colors1 . ToString ( ) . Substring ( 1 );
			string os = s . Substring ( 0, 2 );
			string rs = s . Substring ( 2, 2 );
			string gs = s . Substring ( 4, 2 );
			string bs = s . Substring ( 6, 2 );
			int o = ( int ) Convert . ToInt32 ( os, 16 );
			int r = ( int ) Convert . ToInt32 ( rs, 16 );
			int g = ( int ) Convert . ToInt32 ( gs, 16 );
			int b = ( int ) Convert . ToInt32 ( bs, 16 );

			GradientStop gstop = new GradientStop ( );
			Color c = Color . FromArgb (
				Convert . ToByte ( o ),
				Convert . ToByte ( r ),
				Convert . ToByte ( g ),
				Convert . ToByte ( b ) );
			gstop . Color = c;
			gstop . Offset = ( double ) 0.0;

			// 2nd color
			GradientStop gstop2 = new GradientStop ( );
			s = Colors2 . ToString ( ) . Substring ( 1 );
			os = s . Substring ( 0 , 2 );
			rs = s . Substring ( 2 , 2 );
			gs = s . Substring ( 4 , 2 );
			bs = s . Substring ( 6 , 2 );
			o = ( int ) Convert . ToInt32 ( os , 16 );
			r = ( int ) Convert . ToInt32 ( rs , 16 );
			g = ( int ) Convert . ToInt32 ( gs , 16 );
			b = ( int ) Convert . ToInt32 ( bs , 16 );

			c = Color . FromArgb (
				 Convert . ToByte ( o ) ,
				 Convert . ToByte ( r ) ,
				 Convert . ToByte ( g ) ,
				 Convert . ToByte ( b ) );
			gstop2 . Color = c;
			gstop2 . Offset = GradientValue / 100;

			GradientStop gstop3 = new GradientStop ( );
			s = Colors3 . ToString ( ) . Substring ( 1 );
			os = s . Substring ( 0 , 2 );
			rs = s . Substring ( 2 , 2 );
			gs = s . Substring ( 4 , 2 );
			bs = s . Substring ( 6 , 2 );
			o = ( int ) Convert . ToInt32 ( os , 16 );
			r = ( int ) Convert . ToInt32 ( rs , 16 );
			g = ( int ) Convert . ToInt32 ( gs , 16 );
			b = ( int ) Convert . ToInt32 ( bs , 16 );

			c = Color . FromArgb (
				Convert . ToByte ( o ) ,
				Convert . ToByte ( r ) ,
				Convert . ToByte ( g ) ,
				Convert . ToByte ( b ) );
			gstop3 . Color = c;
			gstop3 . Offset = ( double ) 1.0;
			if ( ToggleActive )
			{
				lgb . GradientStops . Add ( gstop );
				lgb . GradientStops . Add ( gstop2 );
				lgb . GradientStops . Add ( gstop3 );
				lgb . StartPoint = new Point ( 0 , 0.5 );
				lgb . EndPoint = new Point ( 1 , 0.5 );
			}
			else
			{
				lgb . GradientStops . Add ( gstop3 );
				lgb . GradientStops . Add ( gstop2 );
				lgb . GradientStops . Add ( gstop );
				lgb . StartPoint = new Point ( 1, 0.5 );
				lgb . EndPoint = new Point (0,  0.5  );
			}
			if ( isFull )
			{
				Btn4 . Content = "Vertical";
				Btn4 . BringIntoView ( );
				Final . Refresh ( );
			}
				CreateDefinition ( lgb );
			return lgb;
		}

		private LinearGradientBrush CreateDiagonalGradient ( bool isFull )
		{

			LinearGradientBrush lgb = new LinearGradientBrush ( );

			// 1st color
			string s = Colors1 . ToString ( ) . Substring ( 1 );
			string os = s . Substring ( 0, 2 );
			string rs = s . Substring ( 2, 2 );
			string gs = s . Substring ( 4, 2 );
			string bs = s . Substring ( 6, 2 );
			int o = ( int ) Convert . ToInt32 ( os, 16 );
			int r = ( int ) Convert . ToInt32 ( rs, 16 );
			int g = ( int ) Convert . ToInt32 ( gs, 16 );
			int b = ( int ) Convert . ToInt32 ( bs, 16 );

			GradientStop gstop = new GradientStop ( );
			Color c = Color . FromArgb (
				Convert . ToByte ( o ),
				Convert . ToByte ( r ),
				Convert . ToByte ( g ),
				Convert . ToByte ( b ) );
			gstop . Color = c;
			gstop . Offset = ( double ) 0.0;

			// 2nd color
			GradientStop gstop2 = new GradientStop ( );
			s = Colors2 . ToString ( ) . Substring ( 1 );
			os = s . Substring ( 0 , 2 );
			rs = s . Substring ( 2 , 2 );
			gs = s . Substring ( 4 , 2 );
			bs = s . Substring ( 6 , 2 );
			o = ( int ) Convert . ToInt32 ( os , 16 );
			r = ( int ) Convert . ToInt32 ( rs , 16 );
			g = ( int ) Convert . ToInt32 ( gs , 16 );
			b = ( int ) Convert . ToInt32 ( bs , 16 );

			c = Color . FromArgb (
				 Convert . ToByte ( o ) ,
				 Convert . ToByte ( r ) ,
				 Convert . ToByte ( g ) ,
				 Convert . ToByte ( b ) );
			gstop2 . Color = c;
			gstop2 . Offset = GradientValue / 100;

			GradientStop gstop3 = new GradientStop ( );
			s = Colors3 . ToString ( ) . Substring ( 1 );
			os = s . Substring ( 0 , 2 );
			rs = s . Substring ( 2 , 2 );
			gs = s . Substring ( 4 , 2 );
			bs = s . Substring ( 6 , 2 );
			o = ( int ) Convert . ToInt32 ( os , 16 );
			r = ( int ) Convert . ToInt32 ( rs , 16 );
			g = ( int ) Convert . ToInt32 ( gs , 16 );
			b = ( int ) Convert . ToInt32 ( bs , 16 );

			c = Color . FromArgb (
				Convert . ToByte ( o ) ,
				Convert . ToByte ( r ) ,
				Convert . ToByte ( g ) ,
				Convert . ToByte ( b ) );
			gstop3 . Color = c;
			gstop3 . Offset = ( double ) 1.0;

			if ( ToggleActive )
			{
				lgb . GradientStops . Add ( gstop );
				lgb . GradientStops . Add ( gstop2 );
				lgb . GradientStops . Add ( gstop3 );
				lgb . StartPoint = new Point ( 0.0 , 0.0 );
				lgb . EndPoint = new Point ( 1 , 1 );
			}
			else
			{
				lgb . GradientStops . Add ( gstop3 );
				lgb . GradientStops . Add ( gstop2 );
				lgb . GradientStops . Add ( gstop );
				lgb . StartPoint = new Point ( 1 , 1 );
				lgb . EndPoint = new Point ( 0.0 , 0.0 );
			}
			if ( isFull )
			{
				//				DBtn . Background = lgb;
				Btn4 . Content = "Diagonal";
				Btn4 . BringIntoView ( );
				Final . Refresh ( );
				CreateDefinition ( lgb );
			}
			return lgb;
		}

		private LinearGradientBrush CreateHorizontalGradient ( bool isFull )
		{

			LinearGradientBrush lgb = new LinearGradientBrush ( );

			// 1st color
			string s = Colors1 . ToString ( ) . Substring ( 1 );
			string os = s . Substring ( 0, 2 );
			string rs = s . Substring ( 2, 2 );
			string gs = s . Substring ( 4, 2 );
			string bs = s . Substring ( 6, 2 );
			int o = ( int ) Convert . ToInt32 ( os, 16 );
			int r = ( int ) Convert . ToInt32 ( rs, 16 );
			int g = ( int ) Convert . ToInt32 ( gs, 16 );
			int b = ( int ) Convert . ToInt32 ( bs, 16 );

			GradientStop gstop = new GradientStop ( );
			Color c = Color . FromArgb (
				Convert . ToByte ( o ),
				Convert . ToByte ( r ),
				Convert . ToByte ( g ),
				Convert . ToByte ( b ) );
			gstop . Color = c;
			gstop . Offset = ( double ) 0.35;

			// 2nd color
			GradientStop gstop2 = new GradientStop ( );
			s = Colors2 . ToString ( ) . Substring ( 1 );
			os = s . Substring ( 0 , 2 );
			rs = s . Substring ( 2 , 2 );
			gs = s . Substring ( 4 , 2 );
			bs = s . Substring ( 6 , 2 );
			o = ( int ) Convert . ToInt32 ( os , 16 );
			r = ( int ) Convert . ToInt32 ( rs , 16 );
			g = ( int ) Convert . ToInt32 ( gs , 16 );
			b = ( int ) Convert . ToInt32 ( bs , 16 );

			c = Color . FromArgb (
				 Convert . ToByte ( o ) ,
				 Convert . ToByte ( r ) ,
				 Convert . ToByte ( g ) ,
				 Convert . ToByte ( b ) );
			gstop2 . Color = c;
			gstop2 . Offset = GradientValue / 100;

			GradientStop gstop3 = new GradientStop ( );
			s = Colors3 . ToString ( ) . Substring ( 1 );
			os = s . Substring ( 0 , 2 );
			rs = s . Substring ( 2 , 2 );
			gs = s . Substring ( 4 , 2 );
			bs = s . Substring ( 6 , 2 );
			o = ( int ) Convert . ToInt32 ( os , 16 );
			r = ( int ) Convert . ToInt32 ( rs , 16 );
			g = ( int ) Convert . ToInt32 ( gs , 16 );
			b = ( int ) Convert . ToInt32 ( bs , 16 );

			c = Color . FromArgb (
				Convert . ToByte ( o ) ,
				Convert . ToByte ( r ) ,
				Convert . ToByte ( g ) ,
				Convert . ToByte ( b ) );
			gstop3 . Color = c;
			gstop3 . Offset = ( double ) 1;

			if ( ToggleActive )
			{
				lgb . GradientStops . Add ( gstop );
				lgb . GradientStops . Add ( gstop2 );
				lgb . GradientStops . Add ( gstop3 );
				lgb . StartPoint = new Point ( 0.5 , 1.7 );
				lgb . EndPoint = new Point ( 0.5 , 0 );
			}
			else
			{
				lgb . GradientStops . Add ( gstop3 );
				lgb . GradientStops . Add ( gstop2 );
				lgb . GradientStops . Add ( gstop );
				lgb . StartPoint = new Point ( 0.5 , 0 );
				lgb . EndPoint = new Point ( 0.5 , .8 );
			}
			if ( isFull )
			{
				Btn4 . Content = "Horizontal";
				Btn4 . BringIntoView ( );
				Final . Refresh ( );
			}
			CreateDefinition ( lgb );
			return lgb;
		}
		private void CreateDefinition ( LinearGradientBrush lgb )
		{
			string s = "";
			string [] offset1;
			string[] offset2;
			string[] offset3;
			string temp=lgb . GradientStops [ 0 ] . ToString ( );
			offset1 = temp . Split ( ',' );
			temp=lgb . GradientStops [ 1 ] . ToString ( );
			offset2 = temp . Split ( ',' );
			temp=lgb . GradientStops [ 2 ] . ToString ( );
			offset3 = temp . Split ( ',' );
			s = $"Start  : {lgb . StartPoint . ToString ( )}  : {lgb . EndPoint . ToString ( )}, " +
				$"Left : {offset1 [ 0 ]} : {offset1 [ 1 ]}, " +
				$"Mid : {offset2 [ 0 ]} : {offset2 [ 1 ]}, " +
				$"Right : {offset3 [ 0 ]}  : {offset3 [ 1 ]}\n ";
			GradientText . Text = s;
		}
		#endregion Gradient creation methods
		private void DisplayOption_Click ( object sender , RoutedEventArgs e )
		{
		}

		private void DisplayGradientOption_Click ( object sender , MouseButtonEventArgs e )
		{
			gw = new GradientDisplay ( );
			gw . Background = Final . Background;
			gw . Show ( );
			DisplayWindowActive = true;
			if ( DisplayWindowActive && gw != null )
				gw . Background = Final . Background;
		}

		private void GradientSlider_ValueChanged ( object sender , RoutedPropertyChangedEventArgs<double> e )
		{
			//Change position of splitter
			GradientValue = e . NewValue;
			CreateGradient ( );
			OffsetVal . Text = Convert.ToInt32(GradientValue).ToString();
		}

		private void GradientText_PreviewMouseRightButtonDown ( object sender , MouseButtonEventArgs e )
		{
			Clipboard . SetText ( GradientText . Text );
		}

		private void VBtn_Click ( object sender , RoutedEventArgs e )
		{
			GradientStyle = 1;
			Final . Background = VBtn . Background;
		}

		private void HBtn_Click ( object sender , RoutedEventArgs e )
		{
			GradientStyle = 3;			
			Final . Background = HBtn . Background;
		}

		private void DBtn_Click ( object sender , RoutedEventArgs e )
		{
			GradientStyle = 2;			
			Final . Background = DBtn . Background;
		}

		private void SaveToClipBoard_Click ( object sender , MouseButtonEventArgs e )
		{
			Border b = sender as Border;
			string s = ColorstringFromBrush ( b . Background );
			Clipboard . SetText ( s.Substring (1) );
		}

		private void opAll_Drop ( object sender , DragEventArgs e )
		{
			//update everything after data dropped on color
		}

		private void Rgb_PreviewDrop ( object sender , DragEventArgs e )
		{
			string dataString = ( string ) e . Data . GetData ( DataFormats . StringFormat );
			//string  b = e.ToString();
			//ColorstringFromBrush(e.Data );
			if(dataString.Contains("#"))
				opAll . Text = dataString . Substring ( 3 );
			else
				opAll . Text = dataString . Substring ( 2 );
			string o = opAll . Text .Substring(0,2);
			string r = opAll . Text .Substring(2,2);
			string g = opAll . Text .Substring(4,2);
			string b = opAll . Text .Substring(6,2);
			int io= Convert.ToInt32(o,16);
			int ir= Convert.ToInt32(r,16);
			int ig= Convert.ToInt32(g,16);
			int ib= Convert.ToInt32(b,16);
			Color c = Color.FromArgb((Byte)io, (Byte)ir, (Byte)ig, (Byte)ib );
			if ( ActivePane == 1 )
			{
				Button1_Click ( null , null );
				DisplayPanel ( c);
				Output1 . Refresh ( );
			}
			if ( ActivePane == 2 )
			{
				Button2_Click ( null , null );
				DisplayPanel ( c);
				Output3 . Refresh ( );
			}
			if ( ActivePane == 3 )
			{
				Button3_Click ( null , null );
				DisplayPanel ( Colors3 );
				Output3 . Refresh ( );
			}
			CreateGradient ( );
			Final . Refresh ( );
		}

		private void opAll_PreviewKeyDown ( object sender , KeyEventArgs e )
		{
			if ( e . Key == Key . Enter || e . Key == Key . Escape )
			{
				UpdateFromIncomingData ( opAll . Text );
			}
		}
		private void UpdateFromIncomingData (string dataString )
		{				
			//string  b = e.ToString();
			//ColorstringFromBrush(e.Data );
			if ( dataString . Contains ( "#" ) )
				opAll . Text = dataString . Substring ( 3 );
			else if ( dataString . Contains ( "\r\n" ) )
				opAll . Text = dataString . Substring ( 2 );
			else
				opAll . Text = dataString ;
			if(opAll.Text.Length < 8)
			{
				MessageBox . Show ( "The Color value entered is incompete, it MUST have 8 Digits/Characters");
				return;
			}
			try
			{
				string o = opAll . Text .Substring(0,2);
				string r = opAll . Text .Substring(2,2);
				string g = opAll . Text .Substring(4,2);
				string b = opAll . Text .Substring(6,2);
				int io= Convert.ToInt32(o,16);
				int ir= Convert.ToInt32(r,16);
				int ig= Convert.ToInt32(g,16);
				int ib= Convert.ToInt32(b,16);
				Color c = Color.FromArgb((Byte)io, (Byte)ir, (Byte)ig, (Byte)ib );
				if ( ActivePane == 1 )
				{
					Button1_Click ( null , null );
					DisplayPanel ( c );
					Output1 . Refresh ( );
				}
				if ( ActivePane == 2 )
				{
					Button2_Click ( null , null );
					DisplayPanel ( c );
					Output3 . Refresh ( );
				}
				if ( ActivePane == 3 )
				{
					Button3_Click ( null , null );
					DisplayPanel ( Colors3 );
					Output3 . Refresh ( );
				}
				CreateGradient ( );
				Final . Refresh ( );
			}
			catch(Exception ex)
			{

			}
		}
	}
}

