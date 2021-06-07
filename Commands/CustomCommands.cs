using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Input;

namespace WPFPages . Commands
{
	public  class CustomCommands 
	{



		public ICommand SaveCommand
		{
			get; internal set;
		}
		private bool CanExecuteSaveCommand ( )
		{
			return Flags . MultiViewer != null;
		}

		private void CreateSaveCommand ( )
		{
//			SaveCommand = new RelayCommand ( SaveExecute, CanExecuteSaveCommand );
		}

		public void SaveExecute ( )
		{
			Debug . WriteLine ($"SaveExecute triggered ...");
		}



		//		public event EventHandler CanExecuteChanged;
		//		CustomCommands _InfoCommand = new CustomCommands ( );
		//This is all from one Web source (BlackWasp)
		public bool CanExecute ( object parameter )
		{
			return true;
		}
		public bool CanExecuteChanged ( object parameter )
		{
			return true;
		}

		public void Execute ( object parameter )
		{
			Debug . WriteLine ( $"_InfoCommand triggered ....." );
		}
		//end of all from one Web source (BlackWasp)


		//public static readonly RoutedCommand myCommand = new RoutedCommand ( );

		//// performs logic for ClearCommand
		//public static void ExecutedmyCommand ( object sender, ExecutedRoutedEventArgs e )
		//{
		//	Debug . WriteLine ($"myCommand triggered");
		//}


		//// only returns true if the textbox has text.
		//public static void CanExecutemyCommand ( object sender, CanExecuteRoutedEventArgs e )
		//{
		//	//if ( textBox1 . Text . Length > 0 )
		//	//{
		//		e . CanExecute = true;
		//	//}
		//	//else
		//	//{
		//	//	e . CanExecute = false;
		//	//}
		//}



		//(
		//	"Exit",
		//	"Exit",
		//	typeof ( CustomCommands ),
		//	new InputGestureCollection ( )
		//	{
		//		new KeyGesture(Key.F4, ModifierKeys.Alt)
		//	}
		//);
		//public static readonly RoutedUICommand Escape = new RoutedUICommand
		//(
		//	"Escape",
		//	"Escape",
		//	typeof ( CustomCommands ),
		//	new InputGestureCollection ( )
		//	{
		//		new KeyGesture(Key.Escape, ModifierKeys.None)
		//	}
		//);
		//public static readonly RoutedUICommand LoadMultiViewer = new RoutedUICommand
		//(
		//	"Load Multiviewer",
		//	"LoadMultiviewer",
		//	typeof ( CustomCommands ),
		//	new InputGestureCollection ( )
		//	{
		//		new KeyGesture(Key.F8, ModifierKeys.Control)
		//	}
		//);
	}
}
