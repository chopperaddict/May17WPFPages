using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Input;
using WPFPages.ViewModels;


namespace WPFPages
{
	public class BankAccount: INotifyPropertyChanged
	{
		//This is the Class that holds and handles our SQL data
		//It receives updates etc on the fly via OnPropertyChanged events
		//but is just a skeleton basically

		public static List<BankAccountViewModel> BankList = new List<BankAccountViewModel> ();
		private ObservableCollection<BankAccountViewModel> _bankAccountObs = new ObservableCollection<BankAccountViewModel> (BankList);

		public static ObservableCollection<BankAccountViewModel> BankAccountObs { get; set; }


		//#region properties setup
		//private int id;
		//private string bankno;
		//private string custno;
		//private int actype;
		//private decimal balance;
		//private decimal intrate;
		//private DateTime odate;
		//private DateTime cdate;
		//private int selectedItem;
		//private int selectedIndex;
		//private int currentItem;
		//private int selectedRow;
		//public int Id
		//{
		//	get { return id; }
		//	set { id = value; OnPropertyChanged (Id.ToString ()); }
		//}
		//public string BankNo
		//{
		//	get { return bankno; }
		//	set { bankno = value; OnPropertyChanged (BankNo.ToString ()); }
		//}
		//public string CustNo
		//{
		//	get { return custno; }
		//	set { custno = value; OnPropertyChanged (CustNo.ToString ()); }
		//}
		//public int AcType
		//{
		//	get { return actype; }
		//	set { actype = value; OnPropertyChanged (AcType.ToString ());
		//		OnPropertyChanged ("BankAccountObs");
		//	}
		//}
		//public decimal Balance
		//{
		//	get { return balance; }
		//	set
		//	{
		//		balance = value;
		//		OnPropertyChanged (Id.ToString (Balance.ToString ()));
		//	}
		//}
		//public decimal IntRate
		//{
		//	get { return intrate; }
		//	set { intrate = value; OnPropertyChanged (IntRate.ToString ()); }
		//}
		//public DateTime ODate
		//{
		//	get { return odate; }
		//	set { odate = value; OnPropertyChanged (ODate.ToString ()); }
		//}
		//public DateTime CDate
		//{
		//	get { return cdate; }
		//	set { cdate = value; OnPropertyChanged (CDate.ToString ()); }
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
		//public int SelectedIndex
		//{
		//	get { return selectedIndex; }
		//	set
		//	{
		//		selectedIndex = value;
		//		OnPropertyChanged (SelectedIndex.ToString ());
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
		//public int CurrentItem
		//{
		//	get { return currentItem; }
		//	set
		//	{
		//		currentItem = value;
		//		OnPropertyChanged (currentItem.ToString ());
		//	}
		//}

		////public  void BankAccountObs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		////{
		////	OnPropertyChanged ("BankAccountObs");
		
		////}
		//#endregion properties setup

		//#region SQL data loading

		//public async void LoadBankCollection (DataTable dtBank)
		//{
		//	Mouse.OverrideCursor = Cursors.Wait;
		//	//			BankAccount Bank = new BankAccount();
		//	await FillBankAccountDataGrid (dtBank);
		//	LoadBankAccountIntoList (BankAccountViewModel.BankList, dtBank);
		//	BankAccountObs = new ObservableCollection<BankAccountViewModel> (BankList);
		//	Mouse.OverrideCursor = Cursors.Arrow;
		//}

		/////<summary>
		///// fill DataTable with data from SQL BankAccount database
		///// </summary>
		//public async Task  FillBankAccountDataGrid (DataTable dtBank)
		//{
		//	//clear the datatable first as we are only showing a subset
		//	//			dtBank.Clear ();
		//	dtBank = LoadSqlData (dtBank);
		//	// dtBank should be fully loaded here
			
		//}


		///// <summary>
		///// Load SQL Db from SQL Server
		///// </summary>
		///// <param name="dt"></param>
		///// <returns></returns>
		//public DataTable LoadSqlData (DataTable dt)
		//{
		//	try
		//	{
		//		SqlConnection con;
		//		string ConString = (string)Properties.Settings.Default["BankSysConnectionString"];
		//		//			@"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = 'C:\USERS\IANCH\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\IAN1.MDF'; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
		//		using (con = new SqlConnection (ConString))
		//		{
		//			SqlCommand cmd = new SqlCommand ("Select * from BankAccount", con);
		//			SqlDataAdapter sda = new SqlDataAdapter (cmd);
		//			sda.Fill (dt);
		//			return dt;
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		Console.WriteLine ($"Failed to load Bank Details - {ex.Message}");
		//		return (DataTable)null;
		//	}
		//	return (DataTable)null;
		//}
		//public bool LoadBankAccountIntoList (List<BankAccountViewModel> Banklist, DataTable dtBank)
		//{
		//	//This DOES access the Bank/Account Class properties !!!!!
		//	for (int i = 0; i < dtBank.Rows.Count; ++i)
		//	{
		//		Banklist.Add (new BankAccountViewModel 
		//		{
		//			Id = Convert.ToInt32 (dtBank.Rows[i][0]),
		//			BankNo = dtBank.Rows[i][1].ToString (),
		//			CustNo = dtBank.Rows[i][2].ToString (),
		//			AcType = Convert.ToInt32 (dtBank.Rows[i][3]),
		//			Balance = Convert.ToDecimal (dtBank.Rows[i][4]),
		//			IntRate = Convert.ToDecimal (dtBank.Rows[i][5]),
		//			ODate = Convert.ToDateTime (dtBank.Rows[i][6]),
		//			CDate = Convert.ToDateTime (dtBank.Rows[i][7]),
		//		});
		//	}
		//	return true;
		//}

		//#endregion SQL data loading

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;


		protected void OnPropertyChanged (string PropertyName)
		{
			if (null != PropertyChanged)
			{
				PropertyChanged (this,
					new PropertyChangedEventArgs (PropertyName));
			}
		}
		#endregion
	}
}