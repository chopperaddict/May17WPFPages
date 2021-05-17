using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFPages.DataSources
{
	public class Customer : INotifyPropertyChanged
	{


	public event PropertyChangedEventHandler PropertyChanged;

		//private int id;
		//private string custno;
		//private string bankno;
		//private int actype;
		//private string fname;
		//private string lname;
		//private string addr1;
		//private string addr2;
		//private string town;
		//private string county;
		//private string pcode;
		//private string phone;
		//private string mobile;
		//private DateTime dob;
		//private DateTime odate;
		//private DateTime cdate;
		//private int selectedItem;
		//private int selectedRow;


		//#region INotifyPropertyChanged Members
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
		//public string FName {
		//	get { return fname; }
		//	set { fname = value; OnPropertyChanged(FName.ToString()); }
		//}

		//public string LName {
		//	get { return lname; }
		//	set { lname = value; OnPropertyChanged(LName.ToString()); }
		//}
		//public string Addr1 {
		//	get { return addr1; }
		//	set { addr1 = value; OnPropertyChanged(Addr1.ToString()); }
		//}
		//public string Addr2 {
		//	get { return addr2; }
		//	set { addr2 = value; OnPropertyChanged(Addr2.ToString()); }
		//}
		//public string Town{
		//	get { return town; }
		//	set { town= value; OnPropertyChanged(Town.ToString()); }
		//}
		//public string County{
		//	get { return county; }
		//	set { county = value; OnPropertyChanged(County.ToString()); }
		//}
		//public string PCode{
		//	get { return pcode; }
		//	set { pcode = value; OnPropertyChanged(PCode.ToString()); }
		//}
		//public string Phone {
		//	get { return phone; }
		//	set { phone= value; OnPropertyChanged(Phone.ToString()); }
		//}
		//public string Mobile{
		//	get { return mobile; }
		//	set { mobile = value; OnPropertyChanged(Mobile.ToString()); }
		//}
		//public DateTime  Dob {
		//	get { return dob; }
		//	set { dob = value; OnPropertyChanged(Dob.ToString()); }
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
		//#endregion INotifyPropertyChanged Members

		#region INotifyProp
		protected void OnPropertyChanged(string PropertyName) {
		if (null != PropertyChanged) {
			PropertyChanged(this,
				new PropertyChangedEventArgs(PropertyName));
		}
	}
		#endregion
	}

}
