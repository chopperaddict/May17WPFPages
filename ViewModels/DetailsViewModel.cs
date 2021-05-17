//if NOT set, dtDetails is persistent
//#define PERSISTENTDATA
#define TASK1

using System;
using System . Threading . Tasks;
using System . Windows . Controls;

using WPFPages . Views;



namespace WPFPages . ViewModels
{
	public class DetailsViewModel : Observable
	{
		#region CONSTRUCTORS

		// CONSTRUCTOR
		public DetailsViewModel ( )
		{
		}

		#endregion CONSTRUCTORS

		/// Callback for db change notifications
		/// </summary>
		/// <param name="sender"></param>
		public void DbHasChangedHandler ( SqlDbViewer sender , DataGrid Grid , DataChangeArgs args )
		{
			if ( Flags . SqlBankViewer != null )
				Flags . SqlBankViewer . ReloadBankOnUpdateNotification ( sender , Grid , args );
			if ( Flags . SqlCustViewer != null )
				Flags . SqlCustViewer . ReloadCustomerOnUpdateNotification ( sender , Grid , args );
			if ( Flags . SqlDetViewer != null )
				Flags . SqlDetViewer . ReloadDetailsOnUpdateNotification ( sender , Grid , args );
			return;
		}

		public static bool SqlUpdating = false;
		public static int CurrentSelectedIndex = 0;
	
		#region properties

		private int id;
		private string bankno;
		private string custno;
		private int actype;
		private decimal balance;
		private decimal intrate;
		private DateTime odate;
		private DateTime cdate;
		private int selectedItem;
		private int selectedRow;

		public int Id
		{
			get { return id; }
			set { id = value; OnPropertyChanged ( Id . ToString ( ) ); }
		}

		public string CustNo
		{
			get { return custno; }
			set { custno = value; OnPropertyChanged ( CustNo . ToString ( ) ); }
		}

		public string BankNo
		{
			get { return bankno; }
			set { bankno = value; OnPropertyChanged ( BankNo . ToString ( ) ); }
		}

		public int AcType
		{
			get { return actype; }
			set { actype = value; OnPropertyChanged ( AcType . ToString ( ) ); }
		}

		public decimal Balance
		{
			get { return balance; }
			set { balance = value; OnPropertyChanged ( Balance . ToString ( ) ); }
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

		public int SelectedItem
		{
			get { return selectedItem; }

			set
			{
				selectedItem = value;
				OnPropertyChanged ( SelectedItem . ToString ( ) );
			}
		}

		public int SelectedRow
		{
			get { return selectedRow; }

			set
			{
				selectedRow = value;
				OnPropertyChanged ( selectedRow . ToString ( ) );
			}
		}

		#endregion properties
	}
}

//#region INotifyProp
//protected void OnPropertyChanged ( string PropertyName )
//{
//	if ( null != PropertyChanged )
//	{
//		PropertyChanged ( this,
//			new PropertyChangedEventArgs ( PropertyName ) );
//	}
//}
//#endregion INotifyProp

//public static void UpdateBankDb ( string CallerDb )
//{
//	if ( CallerDb == "DETAILS" )
//	{
//		//				string Cmd = "Update Bankaccount, "
//	}
//}

//public async Task<bool> LoadDetailsTaskInSortOrderAsync ( bool IsOriginator, int mode = -1 )
//{
//	//THIS Fn HANDLES SPAWNING THE TASK/AWAIT
//	//and handles the Broadcast Notification
//	Console . WriteLine ( $"Calling Details Loading Task (task).." );
//	try
//	{
//		List<Task<bool>> tasks = new List<Task<bool>> ( );
//		tasks . Add ( LoadDetailsTask ( true, 0 ) );
//		var Results = await Task . WhenAll ( tasks );
//	}
//	catch ( Exception ex)
//	{
//		Console . WriteLine ( $"SQl Load error : {ex . Message},  {ex . Data}" );
//	}
//	Console . WriteLine ( $"Task returned .." );

//	DataLoadedArgs args = new DataLoadedArgs ( );
//	args . DbName = "DETAILS";
//	args . CurrentIndex = 0;
//	// Notify all interested parties that the full Details data is loaded and available for them in -  dtDetails & DetailsObs at least
//	if ( !IsOriginator )
//		SqlDbViewer . SendDBLoadedMsg ( null, args );
//	return true;
//}

//**************************************************************************************************************************************************************//
/// <summary>
/// // Only called by LoadDetailsTaskInSortOrderAsync()
/// </summary>
/// <param> </param>
//		public async Task<bool> LoadDetailsTask ( bool isOriginator , int mode = -1 )
//		{
//Mouse . OverrideCursor = Cursors . Wait;
//// load SQL data in DataTable
//if ( dtDetails == null ) DetailsViewModel . dtDetails = new DataTable ( );
//else dtDetails . Clear ( );
//try
//{ if ( DetailsObs != null && DetailsObs . Count > 0 ) DetailsObs . Clear ( ); }
//catch ( Exception ex )
//{ Console . WriteLine ( $"DetailsObs Exception [{ex . Data}\r\n" ); }

//try
//{
//	Console . WriteLine ( $"Load of DataTable dtDetails starting..." );
//	await LoadSqlData ( 0, Flags . IsMultiMode );
//	Console . WriteLine ( $"DataTable dtDetails Loaded ..." );
//	Console . WriteLine ( $"Collection DetailsObs Load starting..." );

//	await LoadDetailsObsCollection ( );

//	Console . WriteLine ( $"Collection DetailsObs loaded ..." );
//}
//catch ( Exception ex )
//{ Console . WriteLine ( $"Task error {ex . Data},\n{ex . Message}" ); }
//Mouse . OverrideCursor = Cursors . Arrow;
//// WE NOW HAVE OUR DATA HERE - fully loaded into O
//return true;
//		}

/// Handles the actual conneciton ot SQL to load the Details Db data required
/// </summary>
/// <returns></returns>
//		public async static Task<bool> LoadSqlData ( int mode = -1, bool isMultiMode = false )
//		{
//			try
//			{
//				if ( dtDetails . Rows . Count > 0 )
//					dtDetails . Clear ( );
//				SqlConnection con;
//				string ConString = "";
//				string commandline = "";
//				ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];
//				con = new SqlConnection ( ConString );
//				using ( con )
//				{
//					if ( Flags.IsMultiMode)
//					{
//						// Create a valid Query Command string including any active sort ordering
//						commandline = $"SELECT * FROM SECACCOUNTS WHERE CUSTNO IN "
//							+ $"(SELECT CUSTNO FROM SECACCOUNTS  "
//							+ $" GROUP BY CUSTNO"
//							+ $" HAVING COUNT(*) > 1) ORDER BY ";
//						commandline = Utils . GetDataSortOrder ( commandline );
//					}
//					else if (Flags.FilterCommand != "")
//					{
//						commandline = Flags.FilterCommand;
//					}
//					else
//					{
//						// Create a valid Query Command string including any active sort ordering
//						commandline = "Select * from SecAccounts  order by ";
//						commandline = Utils . GetDataSortOrder ( commandline );
//					}
//					SqlCommand cmd = new SqlCommand ( commandline, con );
//					SqlDataAdapter sda = new SqlDataAdapter ( cmd );
//					sda . Fill ( dtDetails );
////					Console . WriteLine ( $"Sql data loaded into Details DataTable [{dtDetails . Rows . Count}] row(s) ...." );
//					return true;
//				}
//			}
//			catch ( Exception ex )
//			{
//				Console . WriteLine ( $"Failed to load Details Details - {ex . Message}, {ex . Data}" );
//				return false;
//			}
//			return true;
//		}
//		//**************************************************************************************************************************************************************//
// Loads data from DataTable into Observable Collection
//public async Task<bool> LoadDetailsObsCollection ( )
//{
//			try
//			{
//				//Load the data into our ObservableCollection BankAccounts
//				if ( DetailsObs . Count > 0 )
//				{ DetailsObs . Clear ( ); }

//				for ( int i = 0 ; i < DetailsViewModel . dtDetails . Rows . Count ; ++i )
//					DetailsObs . Add ( new DetailsViewModel
//					{
//						Id = Convert . ToInt32 ( dtDetails . Rows [ i ] [ 0 ] ),
//						BankNo = dtDetails . Rows [ i ] [ 1 ] . ToString ( ),
//						CustNo = dtDetails . Rows [ i ] [ 2 ] . ToString ( ),
//						AcType = Convert . ToInt32 ( dtDetails . Rows [ i ] [ 3 ] ),
//						Balance = Convert . ToDecimal ( dtDetails . Rows [ i ] [ 4 ] ),
//						IntRate = Convert . ToDecimal ( dtDetails . Rows [ i ] [ 5 ] ),
//						ODate = Convert . ToDateTime ( dtDetails . Rows [ i ] [ 6 ] ),
//						CDate = Convert . ToDateTime ( dtDetails . Rows [ i ] [ 7 ] )
//					} );
//				// WE NOW HAVE OUR DATA HERE - fully loaded into Obs
////				Console . WriteLine ( $"Sql data loaded into Details Collection [{DetailsObs . Count}] ...." );
//return true;
//			}
//catch ( Exception ex )
//{
//	Console . WriteLine ( $"Error loading Details Data {ex . Message}" );
//	return false;
//}
//		}

//**************************************************************************************************************************************************************//

/*
 *
#if USETASK
{
			try
			{
			// THIS ALL WORKS PERFECTLY - THANKS TO VIDEO BY JEREMY CLARKE OF JEREMYBYTES YOUTUBE CHANNEL
				int? taskid = Task.CurrentId;
				Task<DataTable> DataLoader = LoadSqlData ();
				DataLoader.ContinueWith
				(
					task =>
					{
						LoadDetailsObsCollection();
					},
					TaskScheduler.FromCurrentSynchronizationContext ()
				);
				Console.WriteLine ($"Completed AWAITED task to load Details Data via Sql\n" +
					$"task =Id is [{taskid}], Completed status  [{DataLoader.IsCompleted}] in {(DateTime.Now - start).Ticks} ticks]\n");
			}
			catch (Exception ex)
			{ Console.WriteLine ($"Task error {ex.Data},\n{ex.Message}"); }
			Mouse.OverrideCursor = Cursors.Arrow;
			// WE NOW HAVE OUR DATA HERE - fully loaded into Obs >?
}
#else * */
