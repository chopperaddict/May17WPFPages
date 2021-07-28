using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Input;

namespace WPFPages
{
	public static class MyCommands
	{

		private static readonly RoutedUICommand showMessage = new RoutedUICommand ("Show Message", "MessageShow" ,typeof(void));
		public static RoutedUICommand ShowMessage
		{
			get
			{
				return showMessage;
			}
		}
	}
}
