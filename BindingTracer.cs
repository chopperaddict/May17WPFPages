using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFPages
{
	public class BindingTracer: TraceListener
	{

		private Action<string> _errorHandler;

		public  BindingTracer (Action<string> errorHandler)
		{
			_errorHandler = errorHandler;
			TraceSource bindingTrace = PresentationTraceSources
			    .DataBindingSource;

			bindingTrace.Listeners.Add (this);
			bindingTrace.Switch.Level = SourceLevels.Error;
		}

		public override void WriteLine (string message)
		{
			_errorHandler?.Invoke (message);
		}

		public override void Write (string message)
		{
		}
	}
}
