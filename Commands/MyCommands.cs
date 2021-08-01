using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Input;

namespace WPFPages.Commands
{
	public static class MyCommands
	{
		public static readonly RoutedUICommand Exit = new RoutedUICommand
			( "Exit", "Exit", typeof ( MyCommands ) );

		public static readonly RoutedUICommand ShowMessage = new RoutedUICommand
			( "Show Message", "Show_Message", typeof ( MyCommands ) );

		public static readonly RoutedUICommand CloseWin = new RoutedUICommand
			( "Close", "CloseWin", typeof ( MyCommands ) );

		public static readonly RoutedCommand Hello = new RoutedCommand ( );
		public static readonly RoutedCommand Bye = new RoutedCommand ( );

	}
}
