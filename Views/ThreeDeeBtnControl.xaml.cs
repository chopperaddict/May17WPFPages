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
		public ThreeDeeBtnControl ( )
		{
			InitializeComponent ( );
			this . DataContext = this;
		}

		private void ThreeDBttn_Loaded ( object sender , RoutedEventArgs e )
		{
			lgb = new LinearGradientBrush ( );
			Brush1 = ( LinearGradientBrush ) FindResource ( "Greenbackground" );
			BorderGreen = new SolidColorBrush(Colors.Green);
			// Startup Setup
			ControlWidth = 120;
			ControlHeight = 80;
			FillTop = Brush1;
			FillSide = BorderGreen;
			FillHole = BorderBlack;
			TextSize= 16;
			FontWeight = FontWeights . Normal;
			FontDecoration = "Normal";
			BtnTextColor = new SolidColorBrush ( Colors . Black );
			TextSize = 16;
			}

		#region internal variables
		public int GradientStyle{get; set;}
		private double GradientValue { get; set; }
		private int ActivePane {get; set;}
		private Color ActiveColor {get; set;}
		public Color Colors1 {get; set;}
		public Color Colors2 {get; set;}
		public Color Colors3 {get; set;}
		public Color Colors4 {get; set;}
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
		#region Control Settings
		//==================================================================================================//
		public int ControlHeight
		{
			get { return ( int ) GetValue ( ControlHeightProperty ); }
			set
			{	SetValue ( ControlHeightProperty , value );
				SetValue ( Ellip1HeightProperty , value );
				SetValue ( Ellip2HeightProperty ,Convert.ToInt32( Ellip1Height * 1.1 ));
				SetValue ( Ellip3HeightProperty , Convert . ToInt32 (  Ellip1Height + 10));}
		}
		public static readonly DependencyProperty ControlHeightProperty =
			DependencyProperty . Register ( "ControlHeight",
			typeof ( int),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( default) );
		public int ControlWidth
		{
			get { return ( int ) GetValue ( ControlWidthProperty ); }
			set
			{	SetValue ( ControlWidthProperty , value );
				SetValue ( Ellip1WidthProperty ,  value  );
				SetValue ( Ellip2WidthProperty , ( value / 150 ) + 155  );
				SetValue ( Ellip3WidthProperty , ( Ellip1Width) + 6 );}
		}
		public static readonly DependencyProperty ControlWidthProperty =
			DependencyProperty . Register ( "ControlWidth",
			typeof ( int),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( default) );
		//==================================================================================================//
		#endregion Control Settings

		#region Color Setting Dependencies
		public Brush FillTop
		{
			get { return ( Brush ) GetValue ( FillTopProperty ); }
			set{ }
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

		#region Text Dependecy settings 

		public int TextSize
		{
			get { return ( int ) GetValue ( TextSizeProperty ); }
			set { SetValue ( TextSizeProperty , value ); }
		}
		public static readonly DependencyProperty TextSizeProperty =
			DependencyProperty . Register ( "TextSize",
			typeof ( int),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( default) );

		public string BtnText
		{
			get { return ( string ) GetValue ( BtnTextProperty ); }
			set { SetValue ( BtnTextProperty , value ); }
		}
		public static readonly DependencyProperty BtnTextProperty =
			DependencyProperty . Register ( "BtnText",
			typeof ( string ),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( "") );
		public Brush BtnTextColor
		{
			get { return ( Brush) GetValue ( BtnTextColorProperty ); }
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

		#region Ellipse Height/Width

		public int Ellip1Height {
			get { return ( int) GetValue ( Ellip1HeightProperty); }
			set { }
		}
		public static readonly DependencyProperty Ellip1HeightProperty =
			DependencyProperty . Register ( "Ellip1Height",
			typeof ( int),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( default) );
		public int Ellip2Height {
			get { return ( int ) GetValue ( Ellip2HeightProperty ); }
			set { }
		}
		public static readonly DependencyProperty Ellip2HeightProperty =
			DependencyProperty . Register ( "Ellip2Height",
			typeof ( int),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( default) );
		public int Ellip3Height {
			get { return ( int ) GetValue ( Ellip3HeightProperty ); }
			set { }
		}
		public static readonly DependencyProperty Ellip3HeightProperty =
			DependencyProperty . Register ( "Ellip3Height",
			typeof ( int),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( default) );
		public int Ellip1Width {
			get { return ( int ) GetValue ( Ellip1WidthProperty ); }
			set { }
		}
		public static readonly DependencyProperty Ellip1WidthProperty =
			DependencyProperty . Register ( "Ellip1Width",
			typeof ( int),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( default) );
		public int Ellip2Width {
			get { return ( int ) GetValue ( Ellip2WidthProperty ); }
			set {  }
		}
		public static readonly DependencyProperty Ellip2WidthProperty =
			DependencyProperty . Register ( "Ellip2Width",
			typeof ( int),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( default) );
		public int Ellip3Width {
			get { return ( int ) GetValue ( Ellip3WidthProperty ); }
			set { }
		}
		public static readonly DependencyProperty Ellip3WidthProperty =
			DependencyProperty . Register ( "Ellip3Width",
			typeof ( int),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( default) );
		#endregion Ellipse Height/Width

		#endregion Dependency properties

		#region PropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged ( string propertyName )
		{
			PropertyChanged?.Invoke ( this , new PropertyChangedEventArgs ( propertyName ) );
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
	}
}
