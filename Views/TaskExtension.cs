using System;

using System.Threading.Tasks;
using System.Timers;

namespace WPFPages
{
	public static class TaskExtensions
	{
//		public Timer timer = new Timer ();
		public async static void Await (this Task task, Action action)
		{
			try
			{
				 System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch ();
				sw.Start();
				Console.WriteLine ("Calling AWAITED task");
				await task;
				sw.Stop ();
				Console.WriteLine ($"AWAITED task completed in {sw.Elapsed} milliseconds");
				action.Invoke ();

			}
			catch { }
		}

	}
}

