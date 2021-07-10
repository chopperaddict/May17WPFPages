using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Linq;
using System . Reflection;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Imaging;
using System . Windows . Navigation;
using System . Windows . Shapes;
using System . Windows . Threading;

namespace WPFPages . Views
	{
	/// <summary>
	/// Interaction logic for RefreshButton.xaml
	/// </summary>
	public partial class RefreshButton : UserControl
		{
		public static event EventHandler<bool> RefreshTimer;
		//public static readonly DependencyProperty RefreshMessageProperty;
		//private const string msg = "";
		private bool isMouseOver;

		public bool IsMouseOver
			{
			get { return isMouseOver; }
			set { isMouseOver = value; }
			}

		//PRIVATE DEPENDENCIES
		public int Counter
			{
			get { return ( int ) GetValue ( CounterProperty ); }
			set { SetValue ( CounterProperty, value ); }
			}
		public static readonly DependencyProperty CounterProperty =
			    DependencyProperty . Register ( "Counter", typeof ( int ), typeof ( RefreshButton ), new PropertyMetadata ( ) );

		public string CtrlBackGround
			{
			get { return ( string ) GetValue ( CtrlBackGroundProperty ); }
			set { SetValue ( CtrlBackGroundProperty, value ); }
			}
		public static readonly DependencyProperty CtrlBackGroundProperty =
			    DependencyProperty . Register ( "CtrlBackGround", typeof ( string ), typeof ( RefreshButton ), new PropertyMetadata ( ) );
		public string MouseoverColor
			{
			get { return ( string ) GetValue ( MouseoverColorProperty ); }
			set { SetValue ( MouseoverColorProperty, value ); }
			}
		public static readonly DependencyProperty MouseoverColorProperty =
			    DependencyProperty . Register ( "Mouseover", typeof ( string ), typeof ( RefreshButton ), new PropertyMetadata ( ) );

		public bool IsRefreshVisible
			{
			get { return ( bool ) GetValue ( IsRefreshVisibleProperty ); }
			set { SetValue ( IsRefreshVisibleProperty, value ); }
			}
		public static readonly DependencyProperty IsRefreshVisibleProperty =
			    DependencyProperty . Register ( "IsRefreshvisible", typeof ( bool ), typeof ( RefreshButton ), new PropertyMetadata ( ) );

		private bool isFlashing;
		public bool IsFlashing
			{
			get { return ( bool ) GetValue ( IsFlashingProperty ); }
			set { SetValue ( IsFlashingProperty, value ); }
			}
		public static readonly DependencyProperty IsFlashingProperty =
			    DependencyProperty . Register ( "IsFlashing", typeof ( bool ), typeof ( Border ), new PropertyMetadata ( ) );

		public string TextOn
			{
			get { return ( string ) GetValue ( TextOnProperty ); }
			set { SetValue ( TextOnProperty, value ); }
			}
		public static readonly DependencyProperty TextOnProperty =
			    DependencyProperty . Register ( "TextOn", typeof ( string ), typeof ( RefreshButton ), new PropertyMetadata ( ) );

		public string TextOff
			{
			get { return ( string ) GetValue ( TextOffProperty ); }
			set { SetValue ( TextOffProperty, value ); }
			}
		public static readonly DependencyProperty TextOffProperty =
			    DependencyProperty . Register ( "TextOff", typeof ( string ), typeof ( RefreshButton ), new PropertyMetadata ( ) );


		//END - PRIVATE DEPENDENCIES

		//private static void Trigger_RefreshTimer(object sender, bool e)
		//{
		//	if(RefreshTimer != null)
		//		RefreshTimer?.Invoke(sender, e);
		//}

		//private static object CoerceValue(DependencyObject element, object value)
		//{
		//	string msg = "";
		//	RefreshButton RefreshMessage = (RefreshButton)element;
		//	return RefreshMessage;
		//}
		//private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		//{
		//	RefreshButton control = (RefreshButton)obj;
		//	RoutedPropertyChangedEventArgs<string> e = new RoutedPropertyChangedEventArgs<string>(
		//		(string)args.OldValue, (string)args.NewValue, ValueChangedEvent);
		//	control.OnValueChanged(e);

		//	//control.OnValueChanged(e);
		//}

		//public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
		//   "ValueChanged", RoutingStrategy.Bubble,
		//	   typeof(RoutedPropertyChangedEventHandler<string>), typeof(RefreshButton));

		///// <summary>
		///// Occurs when the Value property changes.
		///// </summary>
		//public event RoutedPropertyChangedEventHandler<string> ValueChanged
		//{
		//	add { AddHandler(ValueChangedEvent, value); }
		//	remove { RemoveHandler(ValueChangedEvent, value); }
		//}

		///// <summary>
		///// Raises the ValueChanged event.
		///// </summary>
		///// <param name="args">Arguments associated with the ValueChanged event.</param>
		//protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<string> args)
		//{
		//	RaiseEvent(args);
		//}

		//private static string Refreshmessage { get; set; }
		//public string RefreshMessage
		//{
		//	get { return (string)GetValue(RefreshMessageProperty); }
		//	set { SetValue(RefreshMessageProperty, value.ToString()); }
		//}

		//public void OnRefreshMessage()
		//{
		//	this.Content = RefreshMessage;
		//}

		public RefreshButton ( )
			{
			InitializeComponent ( );

			//			Binding b = new Binding("msg");
			Binding b = new Binding ( "RefreshBorder" );
			b . Source = this;
			b . Mode = BindingMode . TwoWay;
			//			RefreshBtn.SetBinding(RefreshButton.RefreshMessageProperty, b);
			Counter = -1;
			this . DataContext = this;
			this . IsFlashing = true;
			//			RefreshTimer += RefreshButton_RefreshTimer;
			DispatcherTimer dispatcherTimer = new DispatcherTimer ( TimeSpan . FromSeconds ( 1 ), DispatcherPriority . Normal,
				delegate
				{
					if ( this . CtrlBackGround != null )
						{
						try
							{
							if ( this . IsMouseOver )
								{
								if ( this . MouseoverColor != null )
									{
									var brush = ( Brush ) new BrushConverter ( ) . ConvertFromString ( MouseoverColor );
									this . RefreshBorder . Background = brush;
									}
								}
							else
								{
								if ( this . CtrlBackGround != null )
									{
									var brush = ( Brush ) new BrushConverter ( ) . ConvertFromString ( CtrlBackGround );
									this . RefreshBorder . Background = brush;
									}
								}
							}
						catch ( Exception ex )
							{ }
						}
					if ( this . IsRefreshVisible == false && this . Visibility == Visibility . Visible )
						this . Visibility = Visibility . Hidden;
					else if ( this . IsRefreshVisible == true && this . Visibility == Visibility . Hidden )
						{
						Brush tp = new SolidColorBrush ( Colors . Transparent );
						Refresh_Content . Background = tp;
						RefreshBtn . Background = tp;
						this . Refreshbutton . Background = new SolidColorBrush ( Colors . Transparent );
						this . Visibility = Visibility . Visible;
						}
					//						this.BringIntoView();
					if ( IsFlashing )
						{
						if ( Refresh_Content . Text == TextOn )
							Refresh_Content . Text = TextOff;
						else
							Refresh_Content . Text = TextOn;
						}
					}
				, Dispatcher );
			}

		private void RefreshButton_RefreshTimer ( object sender, bool e )
			{
			if ( Counter == -1 )
				Counter = 0;
			else
				Counter = -1;
			}

		private void RefreshBorder_PreviewMouseEnter ( object sender, MouseEventArgs e )
			{
			IsMouseOver = true;
			var brush = ( Brush ) new BrushConverter ( ) . ConvertFromString ( MouseoverColor );
			this . RefreshBorder . Background = brush;
			}

		private void RefreshBorder_MouseLeave ( object sender, MouseEventArgs e )
			{
			IsMouseOver = false;
			var brush = ( Brush ) new BrushConverter ( ) . ConvertFromString ( CtrlBackGround . ToString ( ) );

			//var brush = (Brush)new BrushConverter().ConvertFromString(CtrlBackGround.ToString()); 
			this . RefreshBorder . Background = brush;

			}
		}
	}
