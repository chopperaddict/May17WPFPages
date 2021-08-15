using System;
using System . Collections . Generic;
using System . ComponentModel;
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
	/// Interaction logic for ThreeDeeBtnControl.xaml
	/// </summary>
	public partial class ThreeDeeBtnControl : UserControl
	{
		public LinearGradientBrush lgb { get; set; }
		private GradientDisplay gw = null;
		public bool Loading = true;
		private LinearGradientBrush brush1;
		public static Ellipse H1;
		public static Ellipse H2;
		public static Ellipse H3;

		#region   colors for use in system
		public LinearGradientBrush Brush1
		{
			get { return brush1; }
			set { brush1 = value; OnPropertyChanged ( "Brush1" ); }
		}
		private LinearGradientBrush brush2;
		public LinearGradientBrush Brush2
		{
			get { return brush2; }
			set { brush2 = value; OnPropertyChanged ( "Brush2" ); }
		}
		private LinearGradientBrush brush3;
		public LinearGradientBrush Brush3
		{
			get { return brush3; }
			set { brush3 = value; OnPropertyChanged ( "Brush3" ); }
		}
		private LinearGradientBrush brush4;
		public LinearGradientBrush Brush4
		{
			get { return brush4; }
			set { brush4 = value; OnPropertyChanged ( "Brush4" ); }
		}
		private LinearGradientBrush brush5;
		public LinearGradientBrush Brush5
		{
			get { return brush5; }
			set { brush5 = value; OnPropertyChanged ( "Brush5" ); }
		}

		public Brush BorderGreen;
		public Brush BorderYellow;
		public Brush BorderOrange;
		public Brush BorderBlue;
		public Brush BorderBlack;
		
		#endregion   colors for use in system
		
		public ThreeDeeBtnControl ( )
		{
			this . DataContext = this;
			InitializeComponent ( );
		}

		private void ThreeDBttn_Loaded ( object sender , RoutedEventArgs e )
		{
			this . DataContext = this;
			lgb = new LinearGradientBrush ( );
			Brush1 = ( LinearGradientBrush ) FindResource ( "Greenbackground" );
			BorderGreen = new SolidColorBrush ( Colors . Green );
			// Startup Setup
			Ellipse  dp =  GetTemplateChild ( "H1" ) as Ellipse;

			FillTop = Brush1;
			FillSide = BorderGreen;
			FillHole = BorderBlack;
			ThreeDBtn .Foreground = BtnTextColor;
			ThreeDBtn . FontSize = TextSize;
			H1 = new Ellipse ( );
			H1Bak = H1;
			H2 = new Ellipse ( );
			H2Bak = H2;
			H3 = new Ellipse ( );
			H3Bak = H3; 
			H1 . Fill = FillTop;
			 H2 . Fill = FillSide;
			 H3 . Fill = FillHole;
			//			ThreeDBtn . FontStyle = FontDecoration;
		}

		#region internal variables
		public int GradientStyle { get; set; }
		private double GradientValue { get; set; }
		private int ActivePane { get; set; }
		private Color ActiveColor { get; set; }
		public Color Colors1 { get; set; }
		public Color Colors2 { get; set; }
		public Color Colors3 { get; set; }
		public Color Colors4 { get; set; }
		#endregion internal variables

		#region Dependency properties

		/*
		 FillTop
		FillSide
		FillHole
		Ellip1Height
		Ellip2Height
		Ellip3Height
		Ellip1Width
		Ellip2Width
		Ellip3Width
		BtnText
		BtnTextColor
		TextSize
		ControlHeight
		ControlWidth
		EllipHeight1
		EllipHeight2
		EllipHeight3
		EllipWidth1
		EllipWidth2
		EllipWidth3	
		FontHeight
		  */

		#region Control Settings   - UNUSED RIGHT NOW

		//==================================================================================================//
		//public double ControlHeight
		//{
		//	get { return ( double ) GetValue ( ControlHeightProperty ); }
		//	set
		//	{
		//		SetValue ( ControlHeightProperty , value );
		//		//SetValue ( Ellip1HeightProperty , value - 30 );
		//		//SetValue ( Ellip2HeightProperty , value - 10);
		//		//SetValue ( Ellip3HeightProperty , value  );
		//	}
		//}
		//public static readonly DependencyProperty ControlHeightProperty =
		//	DependencyProperty . Register ( "ControlHeight",
		//	typeof ( double),
		//	typeof ( ThreeDeeBtnControl),
		//	new PropertyMetadata ( default) );
		//public double ControlWidth
		//{
		//	get { return ( double ) GetValue ( ControlWidthProperty ); }
		//	set
		//	{
		//		SetValue ( ControlWidthProperty , value );
		//		//SetValue ( Ellip1WidthProperty , value - 10 );
		//		//SetValue ( Ellip2WidthProperty , value  - 10);
		//		//SetValue ( Ellip3WidthProperty , value - 10 );
		//	}
		//}
		//public static readonly DependencyProperty ControlWidthProperty =
		//	DependencyProperty . Register ( "ControlWidth",
		//	typeof ( double),
		//	typeof ( ThreeDeeBtnControl),
		//	new PropertyMetadata ( default) );
		////==================================================================================================//

		//#region Ellipse Height/Width  Control Dependecnies

		//public double Ellip1Height
		//{
		//	get { return ( double ) GetValue ( Ellip1HeightProperty ); }
		//	set
		//	{
		//		SetValue ( Ellip1HeightProperty , value  );
		//		//SetValue ( Ellip2HeightProperty , Ellip1Height );
		//		//SetValue ( Ellip3HeightProperty , Ellip1Height + 10 );
		//	}
		//}
		//public static readonly DependencyProperty Ellip1HeightProperty =
		//	DependencyProperty . Register ( "Ellip1Height",
		//	typeof ( double),
		//	typeof ( ThreeDeeBtnControl),
		//	new PropertyMetadata ( default) );
		//public double Ellip2Height
		//{
		//	get { return ( double ) GetValue ( Ellip2HeightProperty ); }
		//	set { SetValue ( Ellip2HeightProperty , value ); }
		//	//				 SetValue ( Ellip1HeightProperty , Ellip1Height - 8 ); }
		//}
		//public static readonly DependencyProperty Ellip2HeightProperty =
		//	DependencyProperty . Register ( "Ellip2Height",
		//	typeof ( double ),
		//	typeof ( ThreeDeeBtnControl),
		//	new PropertyMetadata ( default) );
		//public double Ellip3Height
		//{
		//	get { return ( double ) GetValue ( Ellip3HeightProperty ); }
		//	set { SetValue ( Ellip3HeightProperty , value ); }
		//}
		//public static readonly DependencyProperty Ellip3HeightProperty =
		//	DependencyProperty . Register ( "Ellip3Height",
		//	typeof ( double ),
		//	typeof ( ThreeDeeBtnControl),
		//	new PropertyMetadata ( default) );
		//public double Ellip1Width
		//{
		//	get { return ( double ) GetValue ( Ellip1WidthProperty ); }
		//	set
		//	{
		//		SetValue ( Ellip1WidthProperty , value );
		//		//SetValue ( Ellip2WidthProperty , Ellip1Width + 8 );
		//		//SetValue ( Ellip3WidthProperty , Ellip1Width + 10 );
		//	}
		//}
		//public static readonly DependencyProperty Ellip1WidthProperty =
		//	DependencyProperty . Register ( "Ellip1Width",
		//	typeof ( double ),
		//	typeof ( ThreeDeeBtnControl),
		//	new PropertyMetadata ( default) );
		//public double Ellip2Width
		//{
		//	get { return ( double ) GetValue ( Ellip2WidthProperty ); }
		//	set { SetValue ( Ellip2WidthProperty , value ); }
		//}
		//public static readonly DependencyProperty Ellip2WidthProperty =
		//	DependencyProperty . Register ( "Ellip2Width",
		//	typeof ( double ),
		//	typeof ( ThreeDeeBtnControl),
		//	new PropertyMetadata ( default) );
		//public double Ellip3Width
		//{
		//	get { return ( double ) GetValue ( Ellip3WidthProperty ); }
		//	set { SetValue ( Ellip3WidthProperty , value ); }
		//}
		//public static readonly DependencyProperty Ellip3WidthProperty =
		//	DependencyProperty . Register ( "Ellip3Width",
		//	typeof ( double ),
		//	typeof ( ThreeDeeBtnControl),
		//	new PropertyMetadata ( default) );
		//#endregion Ellipse Height/Width  Control Dependecnies

		#endregion Control Settings

		#region Color Setting Dependencies
		public Brush FillTop
		{
			get { return ( Brush ) GetValue ( FillTopProperty ); }
			set { }
		}
		public static readonly DependencyProperty FillTopProperty =
			DependencyProperty . Register ( "FillTop",
			typeof ( Brush ),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata (default));

		private static void SetTopColor ( DependencyObject d , DependencyPropertyChangedEventArgs e )
		{
		}
		public Brush FillSide
		{
			get { return ( Brush ) GetValue ( FillSideProperty ); }
			set { }
		}
		public static readonly DependencyProperty FillSideProperty =
			DependencyProperty . Register ( "FillSide",
			typeof ( Brush ),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( default) );
		public Brush FillHole
		{
			get { return ( Brush ) GetValue ( FillholeProperty ); }
			set { }
		}
		public static readonly DependencyProperty FillholeProperty =
			DependencyProperty . Register ( "FillHole",
			typeof ( Brush ),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( default) );

		#endregion Color Setting Dependencies

		#region Text Dependency settings 

		public int TextSize
		{
			get { return ( int ) GetValue ( TextSizeProperty ); }
//			set { SetValue ( TextSizeProperty , value ); }
			set { }
		}
		public static readonly DependencyProperty TextSizeProperty =
			DependencyProperty . Register ( "TextSize",
			typeof ( int),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( default) );

		public string BtnText
		{
			get { return ( string ) GetValue ( BtnTextProperty ); }
			//		set { SetValue ( BtnTextProperty , value ); }
			set { }
		}
		public static readonly DependencyProperty BtnTextProperty =
			DependencyProperty . Register ( "BtnText",
			typeof ( string ),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( "") );
		public Brush BtnTextColor
		{
			get { return ( Brush ) GetValue ( BtnTextColorProperty ); }
			set { SetValue ( BtnTextColorProperty , value ); }
		}
		public static readonly DependencyProperty BtnTextColorProperty =
			DependencyProperty . Register ( "BtnTextColor",
			typeof ( Brush),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( default) );

		public string FontDecoration
		{
			get { return ( string ) GetValue ( FontDecorationProperty ); }
			set { SetValue ( FontDecorationProperty , value ); }
		}
		public static readonly DependencyProperty FontDecorationProperty =
			DependencyProperty . Register ( "FontDecoration",
			typeof ( string),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( default) );
		#endregion Text Dependecy settings 


		#region Elipse Dependencies
		public Ellipse H1Bak
		{
			get { return ( Ellipse ) GetValue ( H1BakProperty ); }
			set { SetValue ( H1BakProperty , value ); }
			//			set { SetValue ( TextSizeProperty, value); }
		}
		public static readonly DependencyProperty H1BakProperty =
			DependencyProperty . Register ( "H1Bak",
			typeof ( Ellipse),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( default) );
		public Ellipse H2Bak
		{
			get { return ( Ellipse ) GetValue ( H2BakProperty ); }
			set { SetValue ( H2BakProperty , value ); }
			//			set { SetValue ( TextSizeProperty, value); }
		}
		public static readonly DependencyProperty H2BakProperty =
			DependencyProperty . Register ( "H2Bak",
			typeof ( Ellipse),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( default) );
		public Ellipse H3Bak
		{
			get { return ( Ellipse ) GetValue ( H3BakProperty ); }
			set { SetValue ( H3BakProperty , value ); }
			//			set { SetValue ( TextSizeProperty, value); }
		}
		public static readonly DependencyProperty H3BakProperty =
			DependencyProperty . Register ( "H3Bak",
			typeof ( Ellipse),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( default) );

		#endregion Elipse Dependencies

		#endregion Dependency properties

		#region PropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged ( string propertyName )
		{
			PropertyChanged?.Invoke ( this , new PropertyChangedEventArgs ( propertyName ) );
			OnApplyTemplate ( );
			//	this . VerifyPropertyName ( propertyName );

			if ( this . PropertyChanged != null )
			{
				//				var e = new PropertyChangedEventArgs ( propertyName );
				//			this . PropertyChanged ( this , e );
			}
			#endregion PropertyChanged
		}

		private void Button_Click ( object sender , RoutedEventArgs e )
		{

		}

		private void ThreeDBtn_PreviewMouseMove ( object sender , MouseEventArgs e )
		{
			DependencyObject dp =  this . GetTemplateChild ( "H1" );
			//			var t = ThreeDBtn.ThreeDBtnTemplate;
			//			Ellipse el = (Ellipse)FindResource("H1");
			//		el . Height = el . Height - 20;
			//			el.H1.Height = el . H1.Height - 25;
			H1 . Fill = new SolidColorBrush(Colors.Brown);
		}
		public override void OnApplyTemplate ( )
		{
			base . OnApplyTemplate ( );
			if ( Template != null )
			{
				DependencyObject dp =  GetTemplateChild ( "H1" );

				Ellipse partImage = Template.FindName("H1", this) as Ellipse;
				if ( partImage != null )
				{
					//if ( String . IsNullOrEmpty ( Ellipse) )
					//{
					//	partImage . Visibility = Visibility . Hidden;
					//	partImage . Width = 0;
					//}
					//else
					//{
					//	partImage . Visibility = Visibility . Visible;
					//	partImage . Width = 16;
					//}
					partImage . Height -= 15;
				}
			}
		}
	}
}
