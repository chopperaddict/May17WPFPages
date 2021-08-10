using System;
using System . Collections . Generic;
using System . Linq;
using System . Reflection;
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

using WPFPages . UserControls;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for SysColors.xaml
	/// </summary>
	public partial class SysColors : Window
	{
                public class SysColorItem
                {
                        public string Name
                        {get; set;}
                        public Color Color
                        {get; set;}
                }
                public SysColors ( )
		{
			InitializeComponent ( );
                        LoadSystemColors ( );
                        Utils . SetupWindowDrag ( this );
                        this . DataContext = this;
                }
                public void LoadSystemColors ( )
                {
                        List<SysColorItem> sysColorList = new List<SysColorItem> ( );
                        Type t = typeof ( System . Windows . SystemColors );
                        PropertyInfo [ ] propInfo = t . GetProperties ( );
                        foreach ( PropertyInfo p in propInfo )
                        {
                                if ( p . PropertyType == typeof ( Color ) )
                                {
                                        SysColorItem list = new SysColorItem ( );
                                        list . Color = ( Color ) p . GetValue ( new Color ( ),
                                            BindingFlags . GetProperty, null, null, null );
                                        list . Name = p . Name;

                                        sysColorList . Add ( list );
                                }
                                else if ( p . PropertyType == typeof ( SolidColorBrush ) )
                                {
                                        SysColorItem list = new SysColorItem ( );
                                        list . Color = ( ( SolidColorBrush ) p . GetValue ( new SolidColorBrush ( ),
                                            BindingFlags . GetProperty, null, null, null ) ) . Color;
                                        list . Name = p . Name;

                                        sysColorList . Add ( list );
                                }
                        }
                        grdSysColorList . ItemsSource = sysColorList;
                }

		private void CloseReturnButton_PreviewMouseLeftButtonDown ( object sender, MouseButtonEventArgs e )
		{
                        Close ( );
		}

		private void Window_Loaded ( object sender, RoutedEventArgs e )
		{
                        ExitBtn . FillColor = ExitBtn . FindResource ( "Yellow1" ) as Brush;
                        ExitBtn . Refresh ( );
                }

		private void ColorCode_PreviewMouseRightButtonDown ( object sender, MouseButtonEventArgs e )
		{
                        // Save color code to ClipBoard
                        Clipboard . SetText(ColorCode . Text);
                        ClipPopup . IsOpen = true;
                }

		private void ShowColorsWindow_Click ( object sender, RoutedEventArgs e )
		{
                        string args = "";
                        DataGrid dg = grdSysColorList ;                       
                        SysColorItem sci = grdSysColorList . SelectedItem as SysColorItem;
                        if ( sci != null )
                        {
                                ColorCode . Text = sci . Color . ToString ( );
                                args = ColorCode . Text;
                        }

                        ColorsSelector cs = new ColorsSelector ( args );
                        cs . Tag= args;
                        cs . Show ( );
		}

		private void GrdSysColorList_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
                        DataGrid dg = sender as DataGrid;
                        SysColorItem sci = dg . SelectedItem as SysColorItem;
                        ColorCode . Text = sci . Color.ToString();                         
		}

		private void Button_Click ( object sender, RoutedEventArgs e )
		{
                        ClipPopup . IsOpen = false;
                        ClipPopup . Focus ( );
                        ClosePopup . Focus ( );
		}

		private void Window_PreviewKeyDown ( object sender, KeyEventArgs e )
		{
                        if ( e .Key == Key . Escape )
                                ClipPopup . IsOpen = false;
		}
	}

}