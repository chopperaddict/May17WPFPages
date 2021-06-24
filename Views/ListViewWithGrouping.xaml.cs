using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Linq;
using System . Net;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Forms;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Imaging;
using System . Windows . Shapes;
using WPFPages . ViewModels;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for ListViewWithGrouping.xaml
	/// </summary>
	/// 
	public class testclass
	{
		string Company { get; set; }
		string Url { get; set; }
		string Phone { get; set; }
		string Size { get; set; }
		public testclass (string Company, string Url, string Phone, string Size )
		{
			this . Company = Company;
			this . Url = Url;
			this . Phone = Phone;
			this . Size = Size;
		}
	}
	public partial class ListViewWithGrouping : Window
	{
		private BankCollection SqlBankcollection { get; set; }
		CollectionView view { get; set; }

		public ListViewWithGrouping ( )
		{
			InitializeComponent ( );
//			EventControl . BankDataLoaded += EventControl_BankDataLoaded;
			Utils . SetupWindowDrag ( this );
			List<testclass> businesses = new List<testclass> ( );
			businesses . Add ( new ( "Microsoft", "http://www.microsoft.com", "123-421-1231", "Enterprise" ) );
			businesses . Add ( new  ( "SkyXoft", "http://www.skyxoft.com", "123-321-1231", "SMB" ) );
			businesses . Add ( new  ( "LicenseSpot", "http://www.licensespot.com", "123-312-3212", "SMB" ) );
			BankListView . ItemsSource = businesses;
			CollectionView view = ( CollectionView ) CollectionViewSource . GetDefaultView ( BankListView . ItemsSource );
			PropertyGroupDescription groupDescription = new PropertyGroupDescription ( "Size" );
			view . GroupDescriptions . Add ( groupDescription );
			//			DownloadData ( );
	
		}

		private void BankListView_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			int x = 0;
			Debug . WriteLine ($"{e.Source.ToString()}, {sender.ToString()}");
//			Debug . WriteLine ($"Custno = {testclass.Company}");
		}
		//private async void DownloadData ( )
		//{
		//	await BankCollection . LoadBank ( SqlBankcollection, "BANKLISTVIEW", 1, true );
		//}

		//private async void EventControl_BankDataLoaded ( object sender, LoadedEventArgs e )
		//{
		//	if ( e . CallerType != "BANKLISTVIEW" )
		//	{
		//		TaskFactory task = new TaskFactory ( );// () =>  LoadGrids (e ) );
		//		await task . StartNew ( ( ) => LoadGrids ( e ) );
		//		if ( SqlBankcollection . Count > 0 ) return;
		//		CollectionView view = ( CollectionView ) CollectionViewSource . GetDefaultView ( SqlBankcollection );
		//		if ( view != null )
		//		{
		//			PropertyGroupDescription groupDescription = new PropertyGroupDescription ( "MultiBank" );
		//			view . GroupDescriptions . Add ( groupDescription );
		//		}
		//	}
		//}
		//private async Task<bool> LoadGrids ( LoadedEventArgs e )
		//{
		//	SqlBankcollection = e . DataSource as BankCollection;
		//	BankListView . ItemsSource = SqlBankcollection;
		//	BankListView . Refresh ( );
		//	return true;
		//}

	}
}

