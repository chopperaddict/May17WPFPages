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
using System . Windows . Shapes;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for LinqResults.xaml
	/// </summary>
	public partial class LinqResults : Window
	{
		public LinqResults ( )
		{
			InitializeComponent ( );
			this . MouseDown += delegate { DoDragMove ( ); };

		}
		private void DoDragMove ( )
		{//Handle the button NOT being the left mouse button
		 // which will crash the DragMove Fn.....
			try
			{ this . DragMove ( ); }
			catch { return; }
		}

		private void Close_Click ( object sender, RoutedEventArgs e )
		{
			Close ( );
		}
	}
}
