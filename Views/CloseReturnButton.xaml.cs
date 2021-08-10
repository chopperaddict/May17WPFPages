using System;
using System . Collections . Generic;
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

		public CloseReturnButton ( )
		{
			InitializeComponent ( );
			// Needed to allow referencing of other control names in the Xaml
			//this . DataContext = this;
			ThisWin = this;
			//force background fill to 
			Ellipse9 . UpdateDefaultStyle ( );
			Ellipse9 . Fill = FillColor as SolidColorBrush;
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
				return ( Brush ) GetValue ( FillProperty );
			}
			set
			{
				SetValue ( FillProperty, value );
			}
		}

		public static readonly DependencyProperty FillProperty =
			DependencyProperty . Register ( "FillColor",
			typeof ( Brush ),
			typeof ( CloseReturnButton ),
			new PropertyMetadata ( Brushes . White,
			new PropertyChangedCallback ( SetFillColor ) ) );

		private static void SetFillColor ( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
//			var b = ( CloseReturnButton ) d;
//			b . Ellipse9 . Fill = e . NewValue as Brush;
		}

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
				SetValue ( MouseOverColorProperty, value );
			}
		}

		public static readonly DependencyProperty MouseOverColorProperty =
			DependencyProperty . Register ( "MouseOverColor",
			typeof ( Brush ),
			typeof ( CloseReturnButton ),
			new PropertyMetadata ( default,
			new PropertyChangedCallback ( SetMouseOverColor) ) );

		private static void SetMouseOverColor ( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
//			var b = ( CloseReturnButton ) d;
//			b . Ellipse9 . Fill = e . NewValue as Brush;
		}

		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

		//// Button's standard text color
		public static readonly DependencyProperty TextColorProperty =
			DependencyProperty . Register ( "TextColor",
				typeof ( Brush ),
				typeof ( CloseReturnButton ),
				new PropertyMetadata ( default, new PropertyChangedCallback ( OnTextColorChanged ) )
				);
		private static void OnTextColorChanged ( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			CloseReturnButton Control1 = d as CloseReturnButton;
			Control1 . TextColorChanged ( e );
		}
		private void TextColorChanged ( DependencyPropertyChangedEventArgs e )
		{
//			TextColor = e . NewValue as Brush;
//			BtnText . Foreground = e . NewValue as Brush;
		}
		public Brush TextColor
		{
			get
			{
				return ( Brush ) GetValue ( TextColorProperty );
			}
			set
			{
				SetValue ( TextColorProperty, value );
			}
		}

		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

		//// Ellipse Button's text color on mouseover
		public static readonly DependencyProperty MouseoverTextColorProperty =
			DependencyProperty . Register ( "MouseoverTextColor",
				typeof ( Brush ),
				typeof ( CloseReturnButton ),
				new PropertyMetadata ( default, new PropertyChangedCallback ( OnMouseoverTextColorChanged ) )
				);
		private static void OnMouseoverTextColorChanged ( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			CloseReturnButton Control1 = d as CloseReturnButton;
			Control1 . MouseoverTextColorChanged ( e );
		}
		private void MouseoverTextColorChanged ( DependencyPropertyChangedEventArgs e )
		{
//			MouseoverTextColor = e . NewValue as Brush;
//			BtnText . Foreground = e . NewValue as Brush;
		}
		public Brush MouseoverTextColor
		{
			get
			{
				return ( Brush ) GetValue ( MouseoverTextColorProperty );
			}
			set
			{
				SetValue ( MouseoverTextColorProperty, value );
			}
		}

		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

		//// Enclosing Grid Background color
		public static readonly DependencyProperty ColorBackgroundProperty =
			DependencyProperty . Register ( "ColorBackground",
				typeof ( Brush ),
				typeof ( CloseReturnButton ),
				new UIPropertyMetadata ( Brushes . Cyan ,
				ColorBackgroundCallback ));

		private static void  ColorBackgroundCallback ( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
//			var b = ( CloseReturnButton ) d;
//			b . EllipseGrid . Background= e . NewValue as Brush;
		}

		public Brush ColorBackground
		{
			get
			{return ( Brush ) GetValue ( ColorBackgroundProperty );}
			set
			{SetValue ( ColorBackgroundProperty, value );}
		}

		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

		//// Ellipse Button's text
		public static readonly DependencyProperty ButtonTextProperty =
			DependencyProperty . Register ( "ButtonText",
				typeof ( string ),
				typeof ( CloseReturnButton ),
				new PropertyMetadata ( default , new PropertyChangedCallback(OnButtonTextChanged)) 
				);
		private static void OnButtonTextChanged ( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			CloseReturnButton UserControl1Control = d as CloseReturnButton;
			UserControl1Control. SetTextChanged ( e );
		}
		private void SetTextChanged ( DependencyPropertyChangedEventArgs e )
		{
//			ButtonText = e . NewValue . ToString ( );
//			BtnText.Text = e . NewValue . ToString ( );
		}
		public string ButtonText
		{
			get
			{
				return ( string ) GetValue ( ButtonTextProperty );
			}
			set
			{
				SetValue ( ButtonTextProperty, value );
			}
		}


		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

		//// Ellipse Button's text
		public static readonly DependencyProperty MouseoverButtonTextProperty =
			DependencyProperty . Register ( "MouseoverButtonText",
				typeof ( string ),
				typeof ( CloseReturnButton ),
				new PropertyMetadata ( default, new PropertyChangedCallback ( OnMouseoverButtonTextChanged ) )
				);
		private static void OnMouseoverButtonTextChanged ( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			CloseReturnButton UserControl1Control = d as CloseReturnButton;
			UserControl1Control . SetMouseoverTextChanged ( e );
		}
		private void SetMouseoverTextChanged ( DependencyPropertyChangedEventArgs e )
		{
//			MouseoverButtonText = e . NewValue . ToString ( );
//			BtnText . Text = e . NewValue . ToString ( );
		}
		public string MouseoverButtonText
		{
			get
			{
				return ( string ) GetValue ( MouseoverButtonTextProperty );
			}
			set
			{
				SetValue ( MouseoverButtonTextProperty, value );
			}
		}

		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

		//// Ellipse Button's text color on mouseover
		public static readonly DependencyProperty MouseOverTextColorProperty =
			DependencyProperty . Register ( "MouseOverTextColor",
				typeof ( Brush ),
				typeof ( CloseReturnButton ),
				new PropertyMetadata ( default, new PropertyChangedCallback ( OnMouseOverTextColorChanged ) )
				);
		private static void OnMouseOverTextColorChanged ( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			CloseReturnButton Control1 = d as CloseReturnButton;
			Control1 . MouseOverTextColorChanged ( e );
		}
		private void MouseOverTextColorChanged ( DependencyPropertyChangedEventArgs e )
		{
//			MouseOverTextColor = e . NewValue as Brush;
//			BtnText . Foreground = e . NewValue as Brush;
		}
		public Brush MouseOverTextColor
		{
			get
			{
				return ( Brush ) GetValue ( MouseOverTextColorProperty );
			}
			set
			{
				SetValue ( MouseOverTextColorProperty, value );
			}
		}
		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
		////// Ellipse Border color
		public static readonly DependencyProperty BorderColorProperty =
			DependencyProperty . Register ( "BorderColor",
				typeof ( Brush ),
				typeof ( CloseReturnButton ),
				new PropertyMetadata ( default, new PropertyChangedCallback ( OnBorderColorChanged ) )
				);
		private static void OnBorderColorChanged ( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			CloseReturnButton UserControl1Control = d as CloseReturnButton;
			UserControl1Control . BorderColorChanged ( e );
		}
		private void BorderColorChanged ( DependencyPropertyChangedEventArgs e )
		{
//			BorderColor = e . NewValue as Brush;
//			Ellipse9 . Stroke =  e . NewValue as Brush;
		}
		public Brush BorderColor
		{
			get
			{
				return GetValue ( BorderColorProperty ) as Brush;
			}
			set
			{
				SetValue ( BorderColorProperty, value as Brush);
			}
		}

		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

		////// Ellipse Border width
		public static readonly DependencyProperty StrokeWidthProperty =
			DependencyProperty . Register ( "StrokeWidth",
				typeof ( double),
				typeof ( CloseReturnButton ),
				new PropertyMetadata ( (double)1, new PropertyChangedCallback ( OnStrokeWidthChanged ) )
				);
		private static void OnStrokeWidthChanged ( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			CloseReturnButton UserControl = d as CloseReturnButton;
			UserControl . StrokeWidthChanged ( e );
		}
		private void StrokeWidthChanged ( DependencyPropertyChangedEventArgs e )
		{
//			double d = ( double ) e . NewValue;
//			StrokeWidth =**** d;
//			Ellipse9 . StrokeThickness = d;
		}
		public double StrokeWidth
		{
			get
			{
				return (double)GetValue ( StrokeWidthProperty ) ;
			}
			set
			{
				SetValue ( StrokeWidthProperty, (double) value);
			}
		}

		//$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
	#endregion Dependency Properties

		private void CloseButton_Click ( object sender, MouseButtonEventArgs e )
		{
			//NavigationService ns = NavigationService . GetNavigationService ( this );
			//			ns.Navigate(MainWindow._Page1);

		}

		private void EllipseGrid_MouseMove ( object sender, MouseEventArgs e )
		{
			SolidColorBrush sb = new SolidColorBrush ( );
			if ( IsmouseOver )
			{
				sb = MouseOverColor as SolidColorBrush;
				Ellipse9 . Fill = sb;
				BtnText . Foreground = MouseoverTextColor;
				BtnText . Text = MouseoverButtonText;
			}
			else
			{
				sb = FillColor as SolidColorBrush;
				Ellipse9 . Fill = sb;
				BtnText . Foreground = TextColor;
				BtnText . Text = ButtonText;
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
			SolidColorBrush  sb = FillColor as SolidColorBrush;
			Ellipse9 . Fill = sb;
			BtnText . Text = ButtonText;
			Ellipse9 . UpdateLayout ( );
		}
	}
}
