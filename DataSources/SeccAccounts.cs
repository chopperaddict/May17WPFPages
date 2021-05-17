using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFPages.DataSources
{
	public class SecAccounts : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		//private int id;
		//private string bankno;
		//private string custno;
		//private int actype;
		//private decimal balance;
		//private decimal intrate;
		//private DateTime odate;
		//private DateTime cdate;
		//private int selectedItem;
		//private int selectedRow;

		//public int Id {
		//	get { return id; }
		//	set { id = value; OnPropertyChanged(Id.ToString()); }
		//}
		//public string CustNo {
		//	get { return custno; }
		//	set { custno = value; OnPropertyChanged(CustNo.ToString()); }
		//}
		//public string BankNo {
		//	get { return bankno; }
		//	set { bankno = value; OnPropertyChanged(BankNo.ToString()); }
		//}
		//public int AcType {
		//	get { return actype; }
		//	set { actype = value; OnPropertyChanged(AcType.ToString()); }
		//}
		//public decimal Balance {
		//	get { return balance; }
		//	set { balance = value; OnPropertyChanged(Balance.ToString()); }
		//}
		//public decimal IntRate {
		//	get { return intrate; }
		//	set { intrate = value; OnPropertyChanged(IntRate.ToString()); }
		//}
		//public DateTime ODate {
		//	get { return odate; }
		//	set { odate = value; OnPropertyChanged(ODate.ToString()); }
		//}
		//public DateTime CDate {
		//	get { return cdate; }
		//	set { cdate = value; OnPropertyChanged(CDate.ToString()); }
		//}
		//public int SelectedItem
		//{
		//	get { return selectedItem; }
		//	set
		//	{
		//		selectedItem = value;
		//		OnPropertyChanged (SelectedItem.ToString ());
		//	}
		//}
		//public int SelectedRow
		//{
		//	get { return selectedRow; }
		//	set
		//	{
		//		selectedRow = value;
		//		OnPropertyChanged (selectedRow.ToString ());
		//	}
		//}
		#region INotifyProp		
		protected void OnPropertyChanged (string PropertyName)
		{
			if (null != PropertyChanged)
			{
				PropertyChanged (this,
					new PropertyChangedEventArgs (PropertyName));
			}
		}
		#endregion INotifyProp		
	}
}
