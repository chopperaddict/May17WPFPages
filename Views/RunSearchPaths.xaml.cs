using System;
using System . Collections . Generic;
using System . Diagnostics;
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

namespace WPFPages . Views
{
	public class Searchpath
	{
		public string Path { get; set; }

		public Searchpath ( )
		{

		}
	}
	/// <summary>
	/// Interaction logic for RunSearchPaths.xaml
	/// </summary>
	public partial class RunSearchPaths : Window
	{

		List<string> SearchPaths = new List<string> ( );
		List<Searchpath> searches = new List<Searchpath> ( );

		public bool TestSelectedPath { get; set; }
		public RunSearchPaths ( )
		{
			InitializeComponent ( );
			Flags . ExecuteViewer = this;

			SearchPaths = LoadSearchPaths ( );
			foreach ( var item in SearchPaths )
			{
				Searchpath temp = new Searchpath ( );
				temp . Path = item;
				searches . Add ( temp );
				//ListViewItem lvi = new ListViewItem ( );
				//lvi . Content = item;
				//listView . Items . Add ( lvi );
			}
			Utils . SetupWindowDrag ( this );
			//			Searchpath sp = new Searchpath ( );
			listView . Items . Clear ( );
			listView . ItemsSource = SearchPaths;
			ExecuteFile . Visibility = Visibility . Collapsed;
			listView . SelectedIndex = 0;
			listView . SelectedItem = 0;
			listView . Refresh ( );
			listView . Focus ( );
		}

		/// <summary>
		/// Save sundry search paths for execute system
		/// </summary>
		private void SaveSearchPathStrings ( )
		{
			string temp = "";
			string path = ( string ) Properties . Settings . Default [ "SearchPathFile" ];
///			path = path + @"\SearchPaths.dat";
//			StringBuilder sb = new StringBuilder ( );
			foreach ( string item in listView . Items )
			{
				// Ensure each line has a trailing slash.... So  it is standardised
				char tmp = item [ item . Length - 1 ];
				if ( tmp != '\\' )
					temp += item + @"\\n";
				else
					temp += item + "\n";
			}			
			File . WriteAllText ( path, temp );
		}

		/// <summary>
		/// Load sundry search paths for execute system into ListView
		/// </summary>
		private List<string> LoadSearchPaths ( )
		{
			SearchPaths . Clear ( );
			string path = ( string ) Properties . Settings . Default [ "SearchPathFile" ];
//			path = path + @"\SearchPaths.dat";
			string input = File . ReadAllText ( path );
			string [ ] lines1 = input . Split ( '\n' );
			int indx = 0;
			try
			{
				foreach ( var item in lines1 )
				{
					if ( item.Length > 4)
						SearchPaths . Add ( item );
				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"Error reading Search paths file [{path}]\n{ex . Message}, {ex . Data}" );
			}
			return SearchPaths;
		}

		private void CloseBtn_Click ( object sender, RoutedEventArgs e )
		{
			Flags . ExecuteViewer = null;
			Close ( );
		}

		private void Save_Click ( object sender, RoutedEventArgs e )
		{
			//save item entered in textBox
			int sel = listView . SelectedIndex;
			string temp = "";
			// make sure it has a trailing slash
			char tmp = TxtSearchPath . Text [ TxtSearchPath . Text . Length - 1 ];
			if ( tmp != '\\' )
				temp += TxtSearchPath . Text + @"\";
			else
				temp += TxtSearchPath . Text;
			SearchPaths . Add ( temp);
			listView . ItemsSource = null;
			listView . Items . Clear ( );
			listView . ItemsSource = SearchPaths;
			//ListViewItem lvi = new ListViewItem ( );
			//lvi . Content = TxtSearchPath . Text;
			//listView . Items . Add ( lvi );
			listView . SelectedIndex = sel + 1;
			listView . SelectedItem = sel + 1;
			listView . Refresh ( );
			TxtSearchPath . Text = "";
			TxtSearchPath . Focus ( );
		}

		private void Search_Click ( object sender, RoutedEventArgs e )
		{
			SupportMethods . ProcessExecuteRequest ( this, null, null, "Explorer.exe" );
		}

		private void Remove_Click ( object sender, RoutedEventArgs e )
		{
			int sel = listView . SelectedIndex;
			listView . ItemsSource = null;
			SearchPaths . RemoveAt ( sel );
			listView . Items . Remove ( listView . SelectedItem );
			listView . ItemsSource = SearchPaths;
			listView . SelectedIndex = sel - 1;
			listView . SelectedItem= sel - 1;
			TxtSearchPath . Focus ( );
		}

		private void Window_Closing ( object sender, System . ComponentModel . CancelEventArgs e )
		{
			SaveSearchPathStrings ( );
		}

		private void ListView_Selected ( object sender, RoutedEventArgs e )
		{
			this . Background = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
			this . Foreground = Utils . GetDictionaryBrush ( "HeaderBrushWhite" );
		}

		private void Test1_Click ( object sender, RoutedEventArgs e )
		{
			ExecuteFile . Visibility = Visibility . Visible;
			ExecuteFile . BringIntoView ( );
			ExecuteFile . Refresh (  );
			execName . Focus ( );
		}

		private void CancelBtn_Click ( object sender, RoutedEventArgs e )
		{
			ExecuteFile . Visibility = Visibility . Collapsed;
		}

		private void SaveBoth_Click ( object sender, RoutedEventArgs e )
		{

		}

		private void Exec_Click ( object sender, RoutedEventArgs e )
		{
			if( TestSelectedPath )

				Flags . SingleSearchPath = listView . SelectedItem?.ToString();
			SupportMethods . ProcessExecuteRequest ( this, null, null, execName . Text );
			Flags . SingleSearchPath = "";
		}

		private void scratch_Click ( object sender, RoutedEventArgs e )
		{
			ExecuteFile . Visibility = Visibility . Collapsed;
		}

		private void CheckBox_Click ( object sender, RoutedEventArgs e )
		{
			if(checkBox . IsChecked == true)
				TestSelectedPath = true;
			else
				TestSelectedPath = false;
		}

		private void TxtSearchPath_Entry ( object sender, RoutedEventArgs e )
		{
			if ( TxtSearchPath . Text . Contains ( "Enter qualified path here and" ) )
			{
				TxtSearchPath . Text = "";
				byte r = 0, g = 0, b = 0;
				SolidColorBrush sb = new SolidColorBrush ( Colors . Black );
				TxtSearchPath . Foreground = sb;
			}
		}
	}
}
