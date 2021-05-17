using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WPFPages.Views
{
	/// <summary>
	///  a class to help control of SqlDbViewer and EditDb DataGrid handling
	/// </summary>
	public  class DataGridController
	{
		public object SelectedItem { get; set; }
		public  int SelectedIndex { get; set; }
		public DataGrid SelectedGrid { get; set; }
		public DataGrid CurrentSqlGrid { get; set; }
		public DataGrid CurrentEditGrid { get; set; }
		public bool SqlSelChange { get; set; }
		public bool EditSelChange { get; set; }
		public SqlDbViewer CurrentEditDbViewer { get; set; }
		public int SelectionChangeInitiator = -1;


		public DataGridController () { }

		#region INotifyPropertyChanged Members

		#endregion

	}
}
