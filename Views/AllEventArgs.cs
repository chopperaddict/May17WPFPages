using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Controls;

namespace WPFPages . Views
{
	#region EventArg Declarations
	public class DeletionEventArgs : EventArgs	{		
		public string Sender { get; set; }
		public string Bankno { get; set; }
		public string Custno { get; set; }
	}
	public class LoadedEventArgs :EventArgs
	{
		public int CurrSelection { get; set; }
		public string CallerDb { get; set; }
		public object DataSource { get; set; }
		public int RowCount { get; set; }
	}
	public class NotifyAllViewersOfUpdateEventArgs : EventArgs
	{
		public string CurrentDb { get; set; }
	}
	public class DataUpdatedEventArgs : EventArgs
	{
		public string CurrentDb { get; set; }
		public int Row { get; set; }
	}

	//Inherited Event Args  for Callbacks
	public class DataChangeArgs : EventArgs
	{
		public string SenderName { get; set; }
		public string DbName { get; set; }
	}

	public class DataLoadedArgs : EventArgs
	{
		public DataGrid Grid { get; set; }
		public string DbName { get; set; }
		public int CurrentIndex { get; set; }
	}

	#endregion EventArg Declarations

	class AllEventArgs
	{

	}
}
