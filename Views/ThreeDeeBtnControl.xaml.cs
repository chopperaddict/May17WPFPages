using System;
using System . Collections . Generic;
using System . ComponentModel;
using System . Diagnostics;
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

		//#############################
		#region Size CONTROL Variables
		//#############################

		public int height0 =105;
		public int height1 = 105;
		public int height2=105;
		public int height3=105;
		public int height4=105;

		public int width0 = 235;
		public int width1 = 235;
		public int width2 = 245;
		public int width3 = 235;
		public int width4 = 235;
		public bool Startup = false;
		public int Height0
		{
			get { return height0; }
			set { height0 = value;
				calculateEllipses ( 0 , value , "Height0" );
			}
		}
		public int Height1
		{
			get { return height1; }
			set { height1 = value; }
		}
		public int Height2
		{
			get { return height2; }
			set { height2 = value; }
		}
		public int Height3
		{
			get { return height3; }
			set { height3 = value; }
		}
		public int Height4
		{
			get { return height4; }
			set { height4 = value; }
		}
		public int Width0
		{
			get { return width0; }
			set { width0 = value;
				calculateEllipses ( 1 , value , "Width0" );
			}
		}
		public int Width1
		{
			get { return width1; }
			set { width1 = value; }
		}
		public int Width2
		{
			get { return width2; }
			set { width2 = value; }
		}

		public int Width3
		{
			get { return width3; }
			set { width3 = value; }
		}
		public int Width4
		{
			get { return width4; }
			set { width4 = value; }
		}
		private void calculateEllipses ( int type , object arg1 , string Caller )
		{
			int arg = Convert.ToInt32(arg1);
			if ( type == 0 )  // Height parameter
			{
				if ( arg == 0 ) arg = 100;
				if ( arg != CtrlHeight )
				{
					//				Height0 = arg;
					Height1 = arg + 25;
					Height2 = arg - 10;
					Height3 = arg + 36;
					Height4 = arg + 65;
					Console . WriteLine ( $"Calculation of Height : {Caller} = {Height1}" );
				}
			}
			else
			{     //Width
				if ( arg == 0 ) arg = 230;
				if ( arg != CtrlWidth )
				{
					//				Width0 = arg;
					Width1 = arg + 10;
					Width2 = arg;
					Width3 = arg + 10;
					Width4 = arg + 10;
					Console . WriteLine ( $"Calculation of Width : {Caller} = {Width1}" );
				}
			}
		}
		//#############################
		#endregion Size CONTROL Variables
		//#############################

		//#############################
		#region   colors for use in system
		//#############################
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

		//#############################
		#endregion   colors for use in system
		//#############################


		//#############################
		#region internal 'color' variables
		//#############################

		public int GradientStyle { get; set; }
		private double GradientValue { get; set; }
		private int ActivePane { get; set; }
		private Color ActiveColor { get; set; }
		public Color Colors1 { get; set; }
		public Color Colors2 { get; set; }
		public Color Colors3 { get; set; }
		public Color Colors4 { get; set; }

		//#############################
		#endregion internal 'Color' variables
		//#############################

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
			BorderBlack = new SolidColorBrush ( Colors . Black );
			SetStartupDefaults ( );
			{
				ReportStatus ( );
				DataContext = this;
			}
		}

		private void SetStartupDefaults ( )
		{
			// Startup Setup
			//FillTop = new SolidColorBrush(Brushes.Transparent);
			//FillSide = new SolidColorBrush ( Brushes . Transparent );
			//FillHole = new SolidColorBrush(Brushes.Transparent);
			ThreeDBtn . Foreground = BtnTextColor;
			ThreeDBtn . FontSize = TextSize == 0 ? 22 : TextSize;
			H1 = new Ellipse ( );
			H2 = new Ellipse ( );
			H3 = new Ellipse ( );
			H1 . Fill = FillTop;
			H2 . Fill = FillSide;
			H3 . Fill = FillHole;
			ThreeDBttn . Height = Height;
			ThreeDBttn . Width= Width;
			//Height = CtrlHeight = 100;
			//Height1 += 10;
			//Height2 += 35;
			//Height3 += 20;
			//Height4 += 50;
			//Width = CtrlWidth = 230;
			//Width1 += 10;
			//Width2 += 0;
			//Width3 += 10;
			//Width4 += 10;
		}



		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$//

		/*																									
		BtnText				Ellip2Height		EllipHeight2		FontSize				   H1Bak		
		BtnTextColor			Ellip3Heigh		EllipHeight3		FillTop					   H1Bak
		ControlHeight		Ellip1Width		EllipWidth1		FillSide				   H2Bak
		ControlWidth		Ellip2Width		EllipWidth2		FillHole				   H3Bak
		CurrentWidth		Ellip3Width		EllipWidth3		FontDecoration		 TextSize
		Ellip1Height			EllipHeight1	
		  */

		// Using a DependencyProperty as the backing store for CurrentWidth.  This enables animation, styling, binding, etc...

		//#############################
		#region CTRLxxxx Sizes Dependency objects
		//#############################

		public int CtrlHeight
		{
			get { return ( int ) GetValue ( CtrlHeightProperty ); }
			set { calculateEllipses ( 0 , value , "CtrlHeight" ); }
			//set { }
		}
		private static readonly DependencyProperty CtrlHeightProperty =
			DependencyProperty . Register ( "CtrlHeight" , typeof ( int ) , typeof ( ThreeDeeBtnControl ) , new PropertyMetadata ( (int)default ), OnCtrlHeightChanged);
		private static bool OnCtrlHeightChanged ( object value )
		{
			return value != null ? true : false;

		}
		//	&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&//

		public int CtrlWidth
		{
			get { return ( int ) GetValue ( CtrlWidthProperty ); }
			set { calculateEllipses ( 1 , value , "CtrlWidth" ); }
			//set { }
		}
		public static readonly DependencyProperty CtrlWidthProperty =
			DependencyProperty.Register("CtrlWidth", typeof(int), typeof(ThreeDeeBtnControl), new PropertyMetadata(default), OnCtrlWidthChanged);
		private static bool OnCtrlWidthChanged ( object value )
		{
			return value != null ? true : false;
		}
		//#############################
		#endregion CTRLxxxx Sizes Dependency objects
		//#############################

		//#############################
		#region Color Setting Dependencies
		//#############################
		public Brush FillTop
		{
			get { return ( Brush ) GetValue ( FillTopProperty ); }
			set { SetValue ( FillTopProperty , value ); }
			//set { }
		}
		public static readonly DependencyProperty FillTopProperty =
			DependencyProperty . Register ( "FillTop",
			typeof ( Brush ),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata (new SolidColorBrush(Colors.Transparent)), OnFillTopChanged);
		private static bool OnFillTopChanged ( object value )
		{
			Console . WriteLine ( $"FillTop Property changed to [{ value}]" );
			return value != null ? true : false;
		}
		public Brush FillSide
		{
			get { return ( Brush ) GetValue ( FillSideProperty ); }
			//set { }
			set { SetValue ( FillSideProperty , value ); }
		}
		public static readonly DependencyProperty FillSideProperty =
			DependencyProperty . Register ( "FillSide",
			typeof ( Brush ),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata (new SolidColorBrush(Colors.Transparent)), OnFillSideChanged );
		private static bool OnFillSideChanged ( object value )
		{
			Console . WriteLine ( $"FillSide Property changed to [{ value}]" );
			return value != null ? true : false;
		}
		public Brush FillHole
		{
			get { return ( Brush ) GetValue ( FillHoleProperty ); }
			//set { }
			set { SetValue ( FillHoleProperty , value ); }
		}
		public static readonly DependencyProperty FillHoleProperty =
			DependencyProperty . Register ( "FillHole",
			typeof ( Brush ),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata (new SolidColorBrush(Colors.Transparent)), OnFillHoleChanged );
		private static bool OnFillHoleChanged ( object value )
		{
			Console . WriteLine ( $"FillHoleProperty changed to [{ value}]" );
			return value != null ? true : false;
		}

		//#############################
		#endregion Color Setting Dependencies
		//#############################


		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
		private static bool OnTextOffsetChanged ( object value )
		{
			Console . WriteLine ( $"TextOffset Property changed to [{ value}]" );
			return value != null ? true : false;
		}
		private string textOffset;


		private int CalcTextOffset(int value )
		{
			ContentPresenter cp = FindName ( "Btn6Content" ) as ContentPresenter;
			string temp = cp.Content.ToString();
			string TextOffsetMargin = "";
			return value;
		}

		//#############################
		#region Text Dependency settings 
		//#############################

		public int TextOffset
		{
			get { return ( int ) GetValue ( TextOffsetProperty ); }
			set { CalcTextOffset ( value ); }
			//set { }
		}
		public static readonly DependencyProperty TextOffsetProperty =
			DependencyProperty . Register ( "TextOffset",
			typeof ( int  ),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata (18), OnTextOffsetChanged);

		/// <summary>
		/// Size of the button text
		/// </summary>
		public int TextSize
		{
			get { return ( int ) GetValue ( TextSizeProperty ); }
			//set { SetValue ( TextSizeProperty , value ); }
			set { }
		}
		public static readonly DependencyProperty TextSizeProperty =
			DependencyProperty . Register ( "TextSize",
			typeof ( int  ),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata (18));

		/// <summary>
		/// The Text to be displayed on the button at rest (See Button Down option below)
		/// </summary>
		public string BtnText
		{
			get { return ( string ) GetValue ( BtnTextProperty ); }
			//set { SetValue ( BtnTextProperty , value ); }
			set { }
		}
		public static readonly DependencyProperty BtnTextProperty =
			DependencyProperty . Register ( "BtnText",
			typeof ( string ),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ( ""),OnBtnTextChanged );
		private static bool OnBtnTextChanged ( object value )
		{
			Console . WriteLine ( $"BtnText Property changed to [{ value}]" );
			return value != null ? true : false;
		}
		/// <summary>
		/// Text for button when it has  the Mouse over it
		/// </summary>
		public string BtnTextDown
		{
			get { return ( string ) GetValue ( BtnTextDownProperty ); }
			//			set { SetValue ( BtnTextColorProperty , value ); }
			set { }
		}
		public static readonly DependencyProperty BtnTextDownProperty =
			DependencyProperty . Register ( "BtnTextDown",
			typeof ( string),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata (""));

		/// <summary>
		/// Color of the border around the top surface of the button
		/// </summary>
		public Brush BtnBorder
		{
			get { return ( Brush ) GetValue ( BtnBorderProperty ); }
			//			set { SetValue ( BtnTextColorProperty , value ); }
			set { }
		}
		public static readonly DependencyProperty BtnBorderProperty =
			DependencyProperty . Register ( "BtnBorder",
			typeof ( Brush),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata (new SolidColorBrush(Colors.Transparent)));

		/// <summary>
		/// Width of the border line around the top of the button
		/// </summary>
		public int BorderWidth
		{
			get { return ( int ) GetValue ( BorderWidthProperty ); }
			//set { SetValue ( TextSizeProperty , value ); }
			set { }
		}
		public static readonly DependencyProperty BorderWidthProperty =
			DependencyProperty . Register ( "BorderWidth",
			typeof ( int  ),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata (1));

		/// <summary>
		/// Color pf the text with button at rest
		/// </summary>
		public Brush BtnTextColor
		{
			get { return ( Brush ) GetValue ( BtnTextColorProperty ); }
			//			set { SetValue ( BtnTextColorProperty , value ); }
			set { }
		}
		public static readonly DependencyProperty BtnTextColorProperty =
			DependencyProperty . Register ( "BtnTextColor",
			typeof ( Brush),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata (new SolidColorBrush(Colors.Black)));
		
		/// <summary>
		/// Color of the button Text when it is depressed
		/// </summary>
		public Brush BtnTextColorDown
		{
			get { return ( Brush ) GetValue ( BtnTextColorDownProperty ); }
			//			set { SetValue ( BtnTextColorProperty , value ); }
			set { }
		}
		public static readonly DependencyProperty BtnTextColorDownProperty =
			DependencyProperty . Register ( "BtnTextColorDown",
			typeof ( Brush),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata (new SolidColorBrush(Colors.Black)));
		
		
		/// <summary>
		/// Font Styling option
		/// </summary>
		public string FontDecoration
		{
			get { return ( string ) GetValue ( FontDecorationProperty ); }
			//			set { SetValue ( FontDecorationProperty , value ); }
			set { }
		}
		public static readonly DependencyProperty FontDecorationProperty =
			DependencyProperty . Register ( "FontDecoration",
			typeof ( string),
			typeof ( ThreeDeeBtnControl),
			new PropertyMetadata ("Normal"), OnFontDecorationChanged );
		private static bool OnFontDecorationChanged ( object value )
		{
			Console . WriteLine ( $"FontDecoration Property changed to [{ value}]" );
			return value != null ? true : false;
		}

		//#############################
		#endregion Text Dependency settings 
		//#############################

		//#############################
		#region PropertyChanged
		//#############################

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
		}
		#endregion PropertyChanged

		private void Button_Click ( object sender , RoutedEventArgs e )
		{
			Ellipse elip = sender as Ellipse;
			//Console . WriteLine ( $"Button Clicked [{ elip.ToString()}]" );

		}

		private void ThreeDBtn_PreviewMouseMove ( object sender , MouseEventArgs e )
		{
			//Button  elip = sender as Button;
			//Border b = elip . Parent as Border;
			//			Console . WriteLine ( $"Mouse over in outer Border named :  ['{ b.Name}']" );
			//			Console . WriteLine ( $"Current DataContext = { DataContext . ToString ( )}");
			//			DependencyObject dp =  this . GetTemplateChild ( "H1" );
			//			var t = ThreeDBtn.ThreeDBtnTemplate;
			//			Ellipse el = (Ellipse)FindResource("H1");
			//		el . Height = el . Height - 20;
			//			el.H1.Height = el . H1.Height - 25;
			//		H1 . Fill = new SolidColorBrush ( Colors . Brown );
		}
		public override void OnApplyTemplate ( )
		{
			base . OnApplyTemplate ( );
			return;


			if ( Template != null )
			{
				//				DependencyObject dp =  GetTemplateChild ( "H1" );
				//			ContentPresenter  cp =  GetTemplateChild ( "Btn6Content" ) as ContentPresenter;
				//				Console . WriteLine ( $"Contentpresenter = {cp}" );
				//				cp = Template . FindName ( "Btn6Content" , this ) as ContentPresenter;
				//				Console . WriteLine ( $"Contentpresenter = {cp}" );
				//			Ellipse partImage = Template.FindName("H1", this) as Ellipse;
				//		if ( partImage != null )
				//	{
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
				//	partImage . Height -= 15;
				//				}
			}
		}
		private void ReportStatus ( )
		{
			Console . WriteLine ( $"REPORT On Loading : \n"
				+ $" BtnText		: {BtnText}\n"
				+ $" BtnTextColor	: {BtnTextColor}\n"
				+ $" FontSize		: {FontSize}\n"
				+ $" FontDecoration	:	{FontDecoration}\n" +
				   $" FillTop	: {FillTop . ToString ( )}\n" +
				   $" FillSide	: {FillSide . ToString ( )}\n" +
				   $" FillHole	: {FillHole?.ToString ( )}\n" +
				   $" CtrlHeight	: {CtrlHeight} | {CtrlWidth}...\n" +
				   $" Height1	: {Height1} | {Width1}...\n" +
				   $" Height2	: {Height2} | {Width2}...\n" +
				   $" Height3	: {Height3} | {Width3}..\n" +
				   $" Height4	: {Height4} | {Width4}...\n"
				   );

			Console . WriteLine ( $"CtrlHeight = { CtrlHeight}" );
			Console . WriteLine ( $"CtrlWidth = { CtrlWidth}\n" );
			Console . WriteLine ( $"Height1 = { Height1}" );
			Console . WriteLine ( $"Width1 = { Width1}\n" );
			Console . WriteLine ( $"Height2 = {Height2}" );
			Console . WriteLine ( $"Width2 = { Width2}\n" );
			Console . WriteLine ( $"Height3 = {Height3}" );
			Console . WriteLine ( $"Width3 = { Width3}\n" );
			Console . WriteLine ( $"Height4 = {Height4}" );
			Console . WriteLine ( $"Width4 = { Width4}\n" );

		}

		private void H1_PreviewMouseLeftButtonDown ( object sender , MouseButtonEventArgs e )
		{
			//Debugger . Break ( );
		}

		private void ThreeDBtn_MouseEnter ( object sender , MouseEventArgs e )
		{
			ThreeDBtn . Foreground = BtnTextColorDown;
			ThreeDBtn . Content= BtnTextDown != "" ? BtnTextDown : BtnText; ;
		}

		private void ThreeDBtn_MouseLeave ( object sender , MouseEventArgs e )
		{
			ThreeDBtn . Foreground = BtnTextColor;
			ThreeDBtn . Content = BtnText;
		}
	}
}
