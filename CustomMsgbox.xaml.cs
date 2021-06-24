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
using static System . Environment;

namespace WPFPages
{
	/// <summary>
	/// Interaction logic for CustomMsgbox.xaml
	/// </summary>
	public partial class CustomMsgbox : Window
	{
		string [ ] drive;
		bool lboolResult = false;
		public CustomMsgbox ( )
		{
			InitializeComponent ( );
			Utils . SetupWindowDrag ( this );
			this . Topmost = true;
			label1 . Content = "Current Folder	: " + Environment . CurrentDirectory;
			label2 . Content = "Machine Name	: " + Environment . MachineName;
			OperatingSystem os =  Environment.OSVersion;
			label3 . Text=         $"O/S Version	 : {os}\t" +
				os . ServicePack;			

			label4 . Content = "My Documents	:" + Environment . GetFolderPath ( SpecialFolder . MyDocuments );
			string s = "Drives : "; 
			drive = Environment . GetLogicalDrives ( );
			foreach ( var item in drive )
			{
				s += item + ",  ";
			}
			label5 . Content = s . Substring ( 0, s . Length - 1 );
		}


		private void BtnYes_Click ( object sender, RoutedEventArgs e )
		{
			Close ( );
		}

		private void BtnNo_Click ( object sender, RoutedEventArgs e )
		{
			Close ( );

		}
	}
}
