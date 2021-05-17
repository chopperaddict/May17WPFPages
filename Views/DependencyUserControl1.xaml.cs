
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFPages
{
	/// <summary>
	/// We are creating a special Property named ShowText here for our userControl (only)
	/// Interaction logic for DependencyUserControl1.xaml
	/// </summary>
	public partial class DependencyUserControl1 : UserControl
	{
		public DependencyUserControl1()
		{
			InitializeComponent();
		}
#region SetText dependency
		//Define the new SetText property
		public static readonly DependencyProperty SetTextProperty =
			DependencyProperty.Register("SetText", 
				typeof(string), 
				typeof(UserControl), 
				new PropertyMetadata("", 
					new PropertyChangedCallback(OnSetTextChanged)));
		public string SetText { 
			get { return (string)GetValue(SetTextProperty); }
			set { SetValue(SetTextProperty, value); }
		}

		private static void OnSetTextChanged(DependencyObject d,
			DependencyPropertyChangedEventArgs e)
		{ 
			DependencyUserControl1 UserControl1 = d as DependencyUserControl1;
			UserControl1.OnSetTextChanged(e);
		}
		private void OnSetTextChanged(DependencyPropertyChangedEventArgs e)
		{
			tbTest.Text = e.NewValue.ToString();
		}
		#endregion

		//Define the new SetBackground roperty
		public static readonly DependencyProperty SetBackgroundProperty =
			DependencyProperty.Register("SetBackground", 
				typeof(string), 
				typeof(UserControl), 
				new PropertyMetadata("", 
					new PropertyChangedCallback(OnSetBackgroundChanged)));
		public string SetBackground
		{ 
			get { return (string)GetValue(SetBackgroundProperty); }
			set { SetValue(SetBackgroundProperty, value); }
		}

		private static void OnSetBackgroundChanged(DependencyObject d,
			DependencyPropertyChangedEventArgs e)
		{ 
			DependencyUserControl1 UserControl1 = d as DependencyUserControl1;
			UserControl1.OnSetBackgroundChanged(e);
		}
		private void OnSetBackgroundChanged(DependencyPropertyChangedEventArgs e)
		{
			//this is how to convert a string of a color received to be able to use it as a Brush
			if ((string)e.NewValue == "") return;
			try
			{
				ColorConverter Convertor = new ColorConverter();
				Color color = (Color)Convertor.ConvertFromInvariantString(e.NewValue.ToString());
				SolidColorBrush brush = new SolidColorBrush(color);
				tbTest.Background = brush;
			}
			catch { 
			
			}
		}
	}
	}
