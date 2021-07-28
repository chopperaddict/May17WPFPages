using System;
using System . Collections . Generic;
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

using Newtonsoft . Json . Linq;

namespace WPFPages . UserControls
{
	/// <summary>
	/// Interaction logic for CloseReturnButton.xaml
	/// </summary>
	public partial class CloseReturnButton : UserControl
	{
		public CloseReturnButton ( )
		{
			InitializeComponent ( );
			// Needed to allow referencing of other control names in the Xaml
			this . DataContext = this;
		}

		private void CloseButton_Click ( object sender, MouseButtonEventArgs e )
		{
			NavigationService ns = NavigationService . GetNavigationService ( this );
			//			ns.Navigate(MainWindow._Page1);

		}

		//This is how to declare properties for binding
		//Text for the Ellipse
		public static readonly DependencyProperty ButtonTextProperty =
			DependencyProperty . Register ( "ButtonText", typeof ( string ), typeof ( CloseReturnButton ), new PropertyMetadata ( string . Empty ) );
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


		public static readonly DependencyProperty FillProperty = DependencyProperty . Register ( "Fill", 
			typeof ( Brush ),
			typeof ( CloseReturnButton ), 
			new PropertyMetadata ( Brushes . White ) );

		public Brush Fill
		{
			get
			{
				return ( Brush ) GetValue ( ColorBackgroundProperty );
			}
			set
			{
				SetValue ( ColorBackgroundProperty, value );
			}
		}

		//Background for the Ellipse
		public static readonly DependencyProperty ColorBackgroundProperty =
			DependencyProperty . Register ( "ColorBackground",
				typeof ( Brush ), 
				typeof ( CloseReturnButton ), 
				new PropertyMetadata ( Brushes . Cyan ) );
		public Brush ColorBackground
		{
			get
			{
				return ( Brush ) GetValue ( ColorBackgroundProperty );
			}
			set
			{
				SetValue ( ColorBackgroundProperty, value );
			}
		}
		//Color for button's text
		public static readonly DependencyProperty TextColorProperty =
			DependencyProperty . Register ( "TextColor", 
				typeof ( string ), 
				typeof ( CloseReturnButton ), 
				new PropertyMetadata ( default ) );

		public string TextColor
		{
			get
			{
				return ( string ) GetValue ( TextColorProperty );
			}
			set
			{
				SetValue ( TextColorProperty, value );
			}
		}
		//Color for button's text
		public static readonly DependencyProperty BorderColorProperty =
			DependencyProperty . Register ( "BorderColor", typeof ( Brush ), typeof ( CloseReturnButton ), new PropertyMetadata ( default ) );
		public string BorderColor
		{
			get
			{
				return ( string ) GetValue ( BorderColorProperty );
			}
			set
			{
				SetValue ( BorderColorProperty, value );
			}
		}


	}
}
