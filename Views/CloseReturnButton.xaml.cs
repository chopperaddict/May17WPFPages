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
using System . Windows . Ink;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Imaging;
using System . Windows . Navigation;
using System . Windows . Shapes;

using Newtonsoft . Json . Linq;

namespace WPFPages . UserControls
{
	/// <summary>
	/// Interaction logic for CloseReturnButton.xaml
	/// </summary>
	public partial class CloseReturnButton : UserControl	
	{
		private CloseReturnButton ThisWin ;
		static private Color c = Color . FromArgb ( 0xff , 0xff , 0x00 , 0x00 ) ;
		static private SolidColorBrush b1 =  new SolidColorBrush(c);
		public CloseReturnButton ( )
		{
			InitializeComponent ( );
			this . DataContext = this;
			// Needed to allow referencing of other control names in the Xaml
			//this . DataContext = this;
			ThisWin = this;
			//force background fill to 
//			Ellipse9 . UpdateDefaultStyle ( );
//			Ellipse9 . Fill = FillColor as SolidColorBrush;
//			FillColor = b1;
//			Ellipse9 . UpdateLayout ( );
		}

		#region Declarations
		// This is how to declare standard properties for
		// binding by end user of this control
		public double ButtonHeight
		{
			get
			{
				return Height;
			}
			set
			{
				Height = value;
			}
		}
		public double ButtonWidth
		{
			get
			{
				return Width;
			}
			set
			{
				Width= value;
			}
		}
		private bool ismouseOver;
		public bool IsmouseOver
		{
			get
			{
				return ismouseOver;
			}
			set
			{
				ismouseOver = value;
			}
		}

		#endregion Declarations

		/// All my own Dependency properties are :

		/// Colors stuff

		/// FillColor - ellipse fill color
		/// MouseOverColor - Color of Fill for Ellipse
		/// TextColor - Color of standard text 
		/// MouseoverTextColor - Color of text on Mouseover
		/// ColorBackground - Holding grid background (Little used)

		/// Text stuff

		/// ButtonText - Text on the Button
		/// /// MouseoverButtonText - text on Mouseover

		/// Border stuff

		/// StrokeWidth - Thickness of Elipse border 
		/// BorderColor - Stroke property (Border color)

		#region Dependency Properties

		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
		/// <summary>
		/// Dependency property = Fill() for the the Ellipse !
		/// </summary>
		public Brush FillColor
		{
			get
			{
				return ( Brush ) GetValue ( FillColorProperty );
			}
			set
			{
//				SetValue ( FillColorProperty , value );
//				OnPropertyChanged ( "FillColor" );
//				Ellipse9 . Fill = FillColor;
//				BtnText . Foreground = MouseoverTextColor;
//				BtnText . Text = MouseoverButtonText;
//				Ellipse9 . UpdateLayout ( );
			}
		}

		public static readonly DependencyProperty FillColorProperty =
			DependencyProperty . RegisterAttached ( "FillColor",
			typeof ( Brush ),
			typeof ( CloseReturnButton ),
			new PropertyMetadata ( default) );

		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

		/// <summary>
		/// Dependency property = Fill() for the the Ellipse !
		/// </summary>
		public Brush MouseOverColor
		{
			get
			{
				return ( Brush ) GetValue ( MouseOverColorProperty );
			}
			set
			{
				//SetValue ( MouseOverColorProperty, value );
				//OnPropertyChanged ( "MouseOverColor" );
//				Ellipse9 . Fill = MouseOverColor;
				//BtnText . Foreground = MouseoverTextColor;
				//BtnText . Text = MouseoverButtonText;
//				Ellipse9 . UpdateLayout ( );
			}
		}

		public static readonly DependencyProperty MouseOverColorProperty =
			DependencyProperty . RegisterAttached ( "MouseOverColor",
			typeof ( Brush ),
			typeof ( CloseReturnButton ),
			new PropertyMetadata ( default) );

		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

		//// Button's standard text color
		public Brush TextColor
		{
			get
			{
				return ( Brush ) GetValue ( TextColorProperty );
			}
			set
			{
				SetValue ( TextColorProperty , value );
				//OnPropertyChanged ( "TextColor" );
//				SolidColorBrush sb = MouseOverColor as SolidColorBrush;
				                        //Ellipse9 . Fill = sb;
//								BtnText . Foreground = TextColor;
//				BtnText . Text = MouseoverButtonText;
//				Ellipse9 . UpdateLayout ( );
			}
		}    
		public static readonly DependencyProperty TextColorProperty =
			DependencyProperty . RegisterAttached ( "TextColor", 
				typeof(Brush), 
				typeof(CloseReturnButton), 
				new PropertyMetadata(b1), 
				validateValueCallback );

		private static bool validateValueCallback ( object value )
		{
			Brush b = value  as Brush;
			if ( b == ( Brush ) null )
				return false;
			else
				return true;
		}

		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

		//// Ellipse Button's text color on mouseover
		public Brush MouseoverTextColor
		{
			get
			{
				return ( Brush ) GetValue ( MouseoverTextColorProperty );
			}
			set
			{
				//SetValue ( MouseoverTextColorProperty , value );
				//OnPropertyChanged ( "MouseOverTextColor" );

				//SolidColorBrush sb = MouseOverColor as SolidColorBrush;
				//Ellipse9 . Fill = sb;
//				BtnText . Foreground = MouseoverTextColor;
				//BtnText . Text = MouseoverButtonText;
//				Ellipse9 . UpdateLayout ( );
			}
		}
		public static readonly DependencyProperty MouseoverTextColorProperty =
			DependencyProperty . RegisterAttached ( "MouseoverTextColor",
				typeof ( Brush ),
				typeof ( CloseReturnButton ),
				new PropertyMetadata ( default )
				);
		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

		public Brush ColorBackground
		{
			get
			{ return ( Brush ) GetValue ( ColorBackgroundProperty ); }
			set
			{
				//SetValue ( ColorBackgroundProperty , value );
				//OnPropertyChanged ( "ColorBackground" );
				//SolidColorBrush sb = MouseOverColor as SolidColorBrush;
				//Ellipse9 . Fill = sb;
				//BtnText . Foreground = MouseoverTextColor;
				//BtnText . Text = MouseoverButtonText;
				//Ellipse9 . UpdateLayout ( );
			}
		}
		//// Enclosing Grid Background color
		public static readonly DependencyProperty ColorBackgroundProperty =
			DependencyProperty . RegisterAttached ( "ColorBackground",
				typeof ( Brush ),
				typeof ( CloseReturnButton ),
				new UIPropertyMetadata ( default ));


		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

		public string ButtonText
		{
			get
			{
				return ( string ) GetValue ( ButtonTextProperty );
			}
			set
			{
				//SetValue ( ButtonTextProperty , value );
				//OnPropertyChanged ( "ButtonText" );
				////SolidColorBrush sb = MouseOverColor as SolidColorBrush;
				//Ellipse9 . Fill = sb;
				//BtnText . Foreground = MouseoverTextColor;
//				BtnText . Text = ButtonText;
//				Ellipse9 . UpdateLayout ( );
			}
		}
		//// Ellipse Button's text
		public static readonly DependencyProperty ButtonTextProperty =
			DependencyProperty . RegisterAttached ( "ButtonText",
				typeof ( string ),
				typeof ( CloseReturnButton ),
				new PropertyMetadata ( default) 
				);

		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

		public string MouseoverButtonText
		{
			get
			{
				return ( string ) GetValue ( MouseoverButtonTextProperty );
			}
			set
			{
				//SetValue ( MouseoverButtonTextProperty , value );
				//OnPropertyChanged ( "MouseoverButtonText" );
				//SolidColorBrush sb = MouseOverColor as SolidColorBrush;
				//Ellipse9 . Fill = sb;
				//BtnText . Foreground = MouseoverTextColor;
				//BtnText . Text = MouseoverButtonText;
				//Ellipse9 . UpdateLayout ( );
			}
		}
		//// Ellipse Button's text
		public static readonly DependencyProperty MouseoverButtonTextProperty =
			DependencyProperty . RegisterAttached ( "MouseoverButtonText",
				typeof ( string ),
				typeof ( CloseReturnButton ),
				new PropertyMetadata ( default )
				);
	
		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
		////// Ellipse Border color
		public Brush BorderColor
		{
			get
			{
				return GetValue ( BorderColorProperty ) as Brush;
			}
			set
			{
				//SetValue ( BorderColorProperty , value as Brush );
				//OnPropertyChanged ( "BorderColor" );
				//	SolidColorBrush sb = MouseOverColor as SolidColorBrush;
				//	Ellipse9 . Fill = sb;
				//	BtnText . Foreground = MouseoverTextColor;
				//	BtnText . Text = MouseoverButtonText;
				//	Ellipse9 . UpdateLayout ( );
			}
		}
		public static readonly DependencyProperty BorderColorProperty =
			DependencyProperty . RegisterAttached ( "BorderColor",
				typeof ( Brush ),
				typeof ( CloseReturnButton ),
				new PropertyMetadata ( default)
				);
		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

		////// Ellipse Border width
		public double StrokeWidth
		{
			get
			{
				return ( double ) GetValue ( StrokeWidthProperty );
			}
			set
			{
//				SetValue ( StrokeWidthProperty , ( double ) value );
//				OnPropertyChanged ( "StrokeWidth" );
				//SolidColorBrush sb = MouseOverColor as SolidColorBrush;
				//Ellipse9 . Fill = sb;
				//BtnText . Foreground = MouseoverTextColor;
				//BtnText . Text = MouseoverButtonText;
				//Ellipse9 . UpdateLayout ( );
			}
		}
		public static readonly DependencyProperty StrokeWidthProperty =
			DependencyProperty . RegisterAttached ( "StrokeWidth",
				typeof ( double),
				typeof ( CloseReturnButton ),
				new PropertyMetadata ( (double)1 )
				);

		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

		////// Ellipse Font Size
		public int FontHeight
		{
			get
			{
				return ( int ) GetValue ( FontHeightProperty );
			}
			set
			{
				//				SetValue ( StrokeWidthProperty , ( double ) value );
				//				OnPropertyChanged ( "StrokeWidth" );
				//SolidColorBrush sb = MouseOverColor as SolidColorBrush;
				//Ellipse9 . Fill = sb;
				//BtnText . Foreground = MouseoverTextColor;
				//BtnText . Text = MouseoverButtonText;
				//Ellipse9 . UpdateLayout ( );
			}
		}
		public static readonly DependencyProperty FontHeightProperty=
			DependencyProperty . RegisterAttached ( "FontHeight",
				typeof ( int),
				typeof ( CloseReturnButton ),
				new PropertyMetadata ( default)
				);

		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
		////// Ellipse Width
		public int BtnWidth
		{
			get
			{
				return ( int ) GetValue ( BtnWidthProperty );
			}
			set
			{
				//				SetValue ( StrokeWidthProperty , ( double ) value );
				//				OnPropertyChanged ( "StrokeWidth" );
				//SolidColorBrush sb = MouseOverColor as SolidColorBrush;
				//Ellipse9 . Fill = sb;
				//BtnText . Foreground = MouseoverTextColor;
				//BtnText . Text = MouseoverButtonText;
				//Ellipse9 . UpdateLayout ( );
			}
		}
		public static readonly DependencyProperty BtnWidthProperty=
			DependencyProperty . RegisterAttached ( "BtnWidth",
				typeof ( int),
				typeof ( CloseReturnButton ),
				new PropertyMetadata ( default)
				);

		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
		////// Ellipse Width
		 public  int  BorderWidth
		{
			get
			{
				return ( int ) GetValue ( BorderWidthProperty );
			}
			set
			{
				//				SetValue ( StrokeWidthProperty , ( double ) value );
				//				OnPropertyChanged ( "StrokeWidth" );
				//SolidColorBrush sb = MouseOverColor as SolidColorBrush;
				//Ellipse9 . Fill = sb;
				//BtnText . Foreground = MouseoverTextColor;
				//BtnText . Text = MouseoverButtonText;
				//Ellipse9 . UpdateLayout ( );
			}
		}
		public static readonly DependencyProperty BorderWidthProperty =
			DependencyProperty . RegisterAttached ( "BorderWidth",
				typeof ( int),
				typeof ( CloseReturnButton ),
				new PropertyMetadata ( default)
				);
		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

		#endregion Dependency Properties

		private void CloseButton_Click ( object sender, MouseButtonEventArgs e )
		{
			//NavigationService ns = NavigationService . GetNavigationService ( this );
			//			ns.Navigate(MainWindow._Page1);

		}
			private void EllipseGrid_MouseMove ( object sender, MouseEventArgs e )
		{
			if ( IsmouseOver )
			{
				Ellipse9 . Fill = MouseOverColor;
				BtnText . Foreground = MouseoverTextColor;
				BtnText . Text = MouseoverButtonText;
				Ellipse9 . UpdateLayout ( );
			}
			else
			{
				Ellipse9 . Fill = FillColor;
				BtnText . Foreground = TextColor;
				BtnText . Text = ButtonText;
				Ellipse9 . UpdateLayout ( );
			}
		}

		private void Ellipse9_MouseEnter ( object sender, MouseEventArgs e )
		{
			IsmouseOver = true;
			EllipseGrid_MouseMove ( sender, e );
		}

		private void Ellipse9_MouseLeave ( object sender, MouseEventArgs e )
		{
			IsmouseOver = false;
			EllipseGrid_MouseMove ( sender, e );
		}

		private void CloseReturnBtn_Loaded ( object sender, RoutedEventArgs e )
		{
			//force button background to be the "ForeGround", not mouseover...
			//Ellipse9 . Fill = FillColor;
			//BtnText . Text = ButtonText;
			//Ellipse9 . UpdateLayout ( );
		}
		#region PropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged ( string propertyName )
		{
			PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( propertyName ) );
			//	this . VerifyPropertyName ( propertyName );

			if ( this . PropertyChanged != null )
			{
//				var e = new PropertyChangedEventArgs ( propertyName );
	//			this . PropertyChanged ( this , e );
			}
			#endregion PropertyChanged
		}
	}
}
