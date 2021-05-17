using System . Windows;

namespace WPFPages
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		//		public static int GridWindows = 0;
		//public DataGrid CurrentGrid;
		//public DateTime LoadTime;
#if USEDETAILEDEXCEPTIONHANDLER
		WindowExceptionHandler _exceptionHandler;
#endif

		public App ( )
		{
			//			new BindingTracer (msg => Debugger.Break ());
#if USEDETAILEDEXCEPTIONHANDLER
			_exceptionHandler = new WindowExceptionHandler ();
#endif
		}

#pragma MVVM TODO

		#region MVVM STUFF

		// In App.xaml.cs
		protected override void OnStartup ( StartupEventArgs e )
		{
			base . OnStartup ( e );
			//MainWindow window = new MainWindow ();
			// Create the ViewModel to which
			// the main window binds.
			//			string path = "Data/customers.xml";
			//?			var viewModel = new MainWindowViewModel (path);
			// When the ViewModel asks to be closed,
			// close the window.
			//?			viewModel.RequestClose += delegate { window.Close (); };
			// Allow all controls in the window to
			// bind to the ViewModel by setting the
			// DataContext, which propagates down
			// the element tree.
			//?			window.DataContext = viewModel;

			//			window.Show ();
		}

		#endregion MVVM STUFF

		// These are used to try to force Textbox's to always select
		// all the content in the field, rahter than the default
		// // of putting the cursor at end of the current content
		//protected override void OnStartup(StartupEventArgs e) {
		//	EventManager.RegisterClassHandler(typeof(TextBox),
		//	    TextBox.GotFocusEvent,
		//	    new RoutedEventHandler(TextBox_GotFocus));

		//	FrameworkElement.LanguageProperty.OverrideMetadata(
		//	 typeof(FrameworkElement),
		//	 new FrameworkPropertyMetadata(
		//	 System.Windows.Markup.XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag)));

		//	base.OnStartup(e);
		//}
	}
}
