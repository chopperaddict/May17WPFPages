using System;
using System . Collections . Generic;
using System . ComponentModel;
using System . IO;
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

using WPFPages . ViewModels;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for TreeView1.xaml
	/// </summary>
	public partial class TreeView1 : Window
	{
		private DependencyObject _LastDependencyObject;
		public DependencyObject LastDependencyObject
		{
			get
			{
				return _LastDependencyObject;
			}
			set
			{
				_LastDependencyObject = value;
			}
		}
		public TreeView1 ( )
		{
			InitializeComponent ( );

			var path = @"C:\";

			var dirs = Directory . GetDirectories ( path )
					    . Select ( x => new DirectoryInfo ( x ) )
					    . Select ( x => new FileViewModel ( )
					    {
						    Name = x . Name,
						    Path = x . FullName,
						    IsFile = false,

					    } );

			var files = Directory . GetFiles ( path )
					       . Select ( x => new FileInfo ( x ) )
					       . Select ( x => new FileViewModel ( )
					       {
						       Name = x . Name,
						       Path = x . FullName,
						       Size = x . Length,
						       IsFile = true,
					       } );


			DataContext = dirs . Concat ( files ) . ToList ( );
		}

		private void ListView_Loaded ( object sender, RoutedEventArgs e )
		{
			//ListViewItem lvi = new ListViewItem ( );
			//ListView lv = sender as ListView;
			//for ( int i = 0 ; i < lv . Items . Count ; i++ )
			//{
			//	if ( i % 2 == 0 )
			//	{
			//		lvi . ContentTemplate = ( DataTemplate ) this . FindResource ( "myFirstItemTemplate" );
			//	}
			//	else
			//		lvi . ContentTemplate = ( DataTemplate ) this . FindResource ( "mySecondItemTemplate" );
			//}

		}

		private void LV1_Selected ( object sender, RoutedEventArgs e )
		{
			//ListViewItem lvi = new ListViewItem ( );
			//ListView lv = sender as ListView;

			//if ( lvi . ContentTemplate == ( DataTemplate ) this . FindResource ( "myFirstItemTemplate" ) )
			//{
			//	lvi . ContentTemplate = ( DataTemplate ) this . FindResource ( "mySecondItemTemplate" );
			//}
			//else
			//	lvi . ContentTemplate = ( DataTemplate ) this . FindResource ( "myFirstItemTemplate" );
		}

		private void LV1_PreviewMouseDown ( object sender, MouseButtonEventArgs e )
		{
			//ListView lv = sender as ListView;
			//Point aP = e . GetPosition ( lv );
			//IInputElement obj = lv . InputHitTest ( aP );
			//DependencyObject target = obj as DependencyObject;

			//while ( target != null )
			//{
			//	if ( target is Border )
			//	{
			//		Border b = new Border ( );
			//		b = target as Border;
			//		Color c = Color . FromRgb (
			//			Convert . ToByte ( 100),
			//			Convert . ToByte ( 100),
			//			Convert . ToByte ( 255) );

			//		b . Background = ( Brush ) new SolidColorBrush (c );
			//		b.BorderBrush = ( Brush ) FindResource ( "Red3" );
			//		break;
			//	}
			//	target = VisualTreeHelper . GetParent ( target );
			//}

			//if ( lvi . ContentTemplate == ( DataTemplate ) this . FindResource ( "myFirstItemTemplate" ) )
			//{
			//	lvi . ContentTemplate = ( DataTemplate ) this . FindResource ( "mySecondItemTemplate" );
			//}
			//else
			//	lvi . ContentTemplate = ( DataTemplate ) this . FindResource ( "myFirstItemTemplate" );

		}

		private void LV1_PreviewMouseMove ( object sender, MouseEventArgs e )
		{
			ListView lv = sender as ListView;
			Point aP = e . GetPosition ( lv );
			IInputElement obj = lv . InputHitTest ( aP );
			DependencyObject target = obj as DependencyObject;
//			while ( target != null )
//			{
//				if ( target is Border )
//				{
//					Border b = new Border ( );
//					b = target as Border;
//					Color c = Color . FromRgb (
//						Convert . ToByte ( 100 ),
//						Convert . ToByte ( 100 ),
//						Convert . ToByte ( 255 ) );

//					b . Background = ( Brush ) new SolidColorBrush ( c );
//					b . BorderBrush = ( Brush ) FindResource ( "Red3" );
////					LastDependencyObject = target as DependencyObject;
//					break;
//				}
//				else if ( target is TextBlock )
//				{
//					TextBlock tb = new TextBlock ( );
//					tb = target as TextBlock;
//					Color c = Color . FromRgb (
//						Convert . ToByte ( 250 ),
//						Convert . ToByte ( 100 ),
//						Convert . ToByte ( 125 ) );

//					tb . Background = ( Brush ) new SolidColorBrush ( c );
//					tb . Foreground = ( Brush ) FindResource ( "Black1" );
////					LastDependencyObject = target as DependencyObject;
//					break;
//				}
//				target = VisualTreeHelper . GetParent ( target );
//			}
			///SetBinding LAST object to Transparent
			//Border br = new Border ( );
			//if ( dp . GetType ( ) == br . GetType ( ) )
			//{
			//	Color c = Color . FromArgb (
			//		Convert . ToByte ( 255 ),
			//		Convert . ToByte ( 0 ),
			//		Convert . ToByte ( 0 ),
			//		Convert . ToByte ( 0 ) );
			//	br. Background = ( Brush ) new SolidColorBrush ( c );
			//}

		}
	}
	public class FileViewModel : INotifyPropertyChanged
	{
		private bool _isSelected;
		public bool IsSelected
		{
			get
			{
				return _isSelected;
			}
			set
			{
				_isSelected = value;
				OnPropertyChanged ( "IsSelected" );
			}
		}


		public string Name
		{
			get; set;
		}
		public long Size
		{
			get; set;
		}
		public string Path
		{
			get; set;
		}
		public bool IsFile
		{
			get; set;
		}
		public ImageSource Image
		{
			get; set;
		}
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged ( string propertyName )
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if ( handler != null )
				handler ( this, new PropertyChangedEventArgs ( propertyName ) );
		}
	}
}
