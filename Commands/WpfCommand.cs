using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Input;

namespace WPFPages . Commands
{
	//Generic Command setup we can use  to call other commands ?
	public class WpfCommand : ICommand
	{
		// uses delegates to make it universal !!
		Action<object> executeMethod;
		Func<object, bool> canexecuteMethod;
		
		public event EventHandler CanExecuteChanged;

		public WpfCommand ( Action<object> executeMethod, Func<object, bool> canexecuteMethod )
		{
			this . executeMethod = executeMethod;
			this . canexecuteMethod = canexecuteMethod;
		}
		
		public bool CanExecute ( object parameter )
		{
			return true;
		}

		public void Execute ( object parameter )
		{
			// open the window
			executeMethod ( parameter );
		}
	}
}
