﻿using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Controls;

namespace WPFPages . Views
{
	#region EventArg Declarations

	public class LinkageChangedArgs : EventArgs
	{
		public object sender
		{
			get; set;
		}
		public bool LinkToMulti
		{
			get; set;
		}
		public bool LinkToBankViewer
		{
			get; set;
		}
		public bool LinkToCustomerViewer
		{
			get; set;
		}
		public bool LinkToDetailsViewer
		{
			get; set;
		}
		public bool LinkToAll
		{
			get; set;
		}
	}
	public class IndexChangedArgs : EventArgs
	{
		public object Senderviewer
		{
			get; set;
		}
		public string SenderId
		{
			get; set;
		}
		public string Sender
		{
			get; set;
		}
		public string Custno
		{
			get; set;
		}
		public string Bankno
		{
			get; set;
		}
		public int Row
		{
			get; set;
		}
		public DataGrid dGrid
		{
			get; set;
		}
	}
	public class DeletionEventArgs : EventArgs
	{
		public string Sender
		{
			get; set;
		}
		public string Bankno
		{
			get; set;
		}
		public string Custno
		{
			get; set;
		}
	}
	public class GlobalEventArgs : EventArgs
	{
		public string CallerType
		{
			get; set;
		}
		public string AccountType
		{
			get; set;
		}
		public string SenderGuid
		{
			get; set;
		}
	}
	public class LoadedEventArgs : EventArgs
	{
		public string CallerType
		{
			get; set;
		}
		public string Custno
		{
			get; set;
		}
		public string Bankno
		{
			get; set;
		}
		public string SenderGuid
		{
			get; set;
		}
		public int CurrSelection
		{
			get; set;
		}
		public string CallerDb
		{
			get; set;
		}
		public object DataSource
		{
			get; set;
		}
		public int RowCount
		{
			get; set;
		}
	}
	public class NotifyAllViewersOfUpdateEventArgs : EventArgs
	{
		public string CurrentDb
		{
			get; set;
		}
	}
	public class DataUpdatedEventArgs : EventArgs
	{
		public string CurrentDb
		{
			get; set;
		}
		public int Row
		{
			get; set;
		}
	}

	//Inherited Event Args  for Callbacks
	public class DataChangeArgs : EventArgs
	{
		public string SenderName
		{
			get; set;
		}
		public string DbName
		{
			get; set;
		}
	}

	public class DataLoadedArgs : EventArgs
	{
		public DataGrid Grid
		{
			get; set;
		}
		public string DbName
		{
			get; set;
		}
		public int CurrentIndex
		{
			get; set;
		}
	}
	public class NwGridArgs : EventArgs
	{
		public DataGrid Grid
		{
			get; set;
		}
		public string ArgumentParameter
		{
			get; set;
		}
	}

	#endregion EventArg Declarations

	class AllEventArgs
	{

	}
}
