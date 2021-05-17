using System;
using System.Windows;
using System.Windows.Threading;

namespace WPFPages.Views
{

	/// <summary>
	/// A MAGIC way of allowing the code to refresh() any object that it receives as the parameter
	/// Works Extremely well - so far
	/// </summary>
	public static class ExtensionMethods
	{
		private static Action EmptyDelegate = delegate () { };

		public static void Refresh (this UIElement uiElement)
		{
			uiElement.Dispatcher.Invoke (DispatcherPriority.Render, EmptyDelegate);
		}
	}
}