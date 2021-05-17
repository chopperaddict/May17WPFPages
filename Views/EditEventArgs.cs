using System;

//using System.Windows.Forms;

namespace WPFPages
{
	public class EditEventArgs : EventArgs
	{
		public string Caller { set; get; }
		public object DataType { get; set; }
		public int CurrentIndex { get; set; }

		//Constructor
		public EditEventArgs ( )
		{
		}
	}
}
