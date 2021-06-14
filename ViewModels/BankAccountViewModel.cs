// if set, Datatable is cleared and reloaded, otherwise it is not reloaded
//#define PERSISTENTDATA
#define USETASK
#undef USETASK

using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . ComponentModel;
using System . Windows . Controls;

using WPFPages . Views;

/// <summary>
///  this is a mirror image of the original BankAccount.cs file
/// </summary>
namespace WPFPages . ViewModels
{
	public partial class BankAccountViewModel// : Observable
	{

		#region CONSTRUCTORS

		// CONSTRUCTOR
		//**************************************************************************************************************************************************************//

		public BankAccountViewModel ( )
		{
		}
		#endregion CONSTRUCTORS

		#region STANDARD CLASS PROPERTIES SETUP

		private int id;
		private string bankno;
		private string custno;
		private int actype;
		private decimal balance;
		private decimal intrate;
		private DateTime odate;
		private DateTime cdate;

		public int Id
		{
			get { return id; }
			set { id = value; OnPropertyChanged ( Id . ToString ( ) ); }
		}

		public string BankNo
		{
			get { return bankno; }
			set { bankno = value; OnPropertyChanged ( BankNo . ToString ( ) ); }
		}

		public  string CustNo
		{
			get { return custno; }
			set { custno = value; OnPropertyChanged ( CustNo . ToString ( ) ); }
		}

		public int AcType
		{
			get { return actype; }

			set
			{ actype = value; OnPropertyChanged ( AcType . ToString ( ) ); }
		}

		public decimal Balance
		{
			get { return balance; }

			set
			{ balance = value; OnPropertyChanged ( Balance . ToString ( ) ); }
		}

		public decimal IntRate
		{
			get { return intrate; }
			set { intrate = value; OnPropertyChanged ( IntRate . ToString ( ) ); }
		}

		public DateTime ODate
		{
			get { return odate; }
			set { odate = value; OnPropertyChanged ( ODate . ToString ( ) ); }
		}

		public DateTime CDate
		{
			get { return cdate; }
			set { cdate = value; OnPropertyChanged ( CDate . ToString ( ) ); }
		}

		public override string ToString ( )
		{
			return CustNo + ", " + BankNo + ", " + AcType + ", " + IntRate + ", " + Balance + ", " + ODate + ", " + CDate;
			//return base . ToString ( );
		}
		#endregion STANDARD CLASS PROPERTIES SETUP

		#region SETUP/DECLARATIONS

		public static EditDb EditdbWndBank = null;
		public static EditDb EditdbWndBankCust = null;
		public static EditDb EditdbWndBankDet = null;

		//**********************
		// dbEdit db viewer GLOBALS
		//**********************
		//public static List<DataGrid> CurrentEditDbViewerCustomerGridList = new List<DataGrid> ( );
		//public static List<DataGrid> CurrentEditDbViewerDetailsGridList = new List<DataGrid> ( );
		public static DataGrid ActiveEditDbViewer = null;


		#endregion SETUP/DECLARATIONS


		// MVVM TO DO STUFF/INFO
		// How to configure a RelayCommand with lambda expressions:
		#region PropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged ( string PropertyName )
		{
			if ( null != PropertyChanged )
			{
				PropertyChanged ( this,
					new PropertyChangedEventArgs ( PropertyName ) );
			}
		}
		#endregion PropertyChanged
		#region MVVMstuff

		//**************************************************************************************************************************************************************//
		//		private RelayCommand _saveCommand; public ICommand SaveCommand

		//		{
		//			get
		//			{
		//				if ( _saveCommand == null )
		//				{
		//#pragma MVVM TODO
		//					//_saveCommand = new RelayCommand (param => this.Save (),
		//					//    param => this.CanSave);
		//				}
		//				return _saveCommand;
		//			}
		//		}

		#endregion MVVMstuff
	}
}

/*
 *
 #if USETASK
			{
				int? taskid = Task.CurrentId;
				DateTime start = DateTime.Now;
				Task<bool> DataLoader = FillBankAccountDataGrid ();
				DataLoader.ContinueWith
				(
					task =>
					{
						LoadBankAccountIntoList (dtBank);
					},
					TaskScheduler.FromCurrentSynchronizationContext ()
				);
				Console.WriteLine ($"Completed AWAITED task to load BankAccount  Data via Sql\n" +
					$"task =Id is [ {taskid}], Completed status  [{DataLoader.IsCompleted}] in {(DateTime.Now - start)} Ticks\n");
			}
#else
			{
* */
