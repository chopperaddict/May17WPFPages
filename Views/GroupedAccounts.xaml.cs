using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Linq;
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
using static System . Windows . Forms . VisualStyles . VisualStyleElement . ProgressBar;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for GroupedAccounts.xaml
	/// </summary>
	public partial class GroupedAccounts : Window
	{
		public  BankCollection SqlBankcollection { get; set; }
		CollectionView view { get; set; }
		public GroupedAccounts ( )
		{
			InitializeComponent ( );

			//Create a grouping  so we can layout by AcType - don't know how this works really
			//but it works well.  It reads the list of items in the Control (a ListView in this case)
			// and creates another form of Collection, a Collectionview()
			if ( MainWindow.TestBankcollection == null  || MainWindow . TestBankcollection . Count == 0 )
			{
				Mouse . OverrideCursor = System . Windows . Input . Cursors . Wait;
				EventControl . BankDataLoaded += EventControl_BankDataLoaded;
				BankCollection . LoadBank ( SqlBankcollection, "BANKLISTVIEW", 1, true );
			}
			else
			{
				this . Show ( );
				Mouse . OverrideCursor = System . Windows . Input . Cursors . Wait;
				if ( MainWindow . TestBankcollection . Count > 0 )
				{
					SqlBankcollection = MainWindow . TestBankcollection;
					var accounts = from items in SqlBankcollection
						       where ( items . AcType == 1 )
						       orderby items . AcType, items . CustNo, items . BankNo
						       select items;
					lview3 . Items . Clear ( );
					SqlBankcollection = new BankCollection();
					foreach ( var item in accounts )
					{
						SqlBankcollection . Add ( item );
					}
					lview3 . ItemsSource = SqlBankcollection;
				}
				else
				{
					Mouse . OverrideCursor = System . Windows . Input . Cursors . Wait;
					EventControl . BankDataLoaded += EventControl_BankDataLoaded;
					BankCollection . LoadBank ( SqlBankcollection, "BANKLISTVIEW", 1, true );

				}
				lview3 . Refresh ( );
				Mouse . OverrideCursor = System . Windows . Input . Cursors . Arrow;
			}
		}
		private async void EventControl_BankDataLoaded ( object sender, LoadedEventArgs e )
		{
			if ( e . CallerType != "BANKLISTVIEW" )
			{
				TaskFactory task = new TaskFactory ( );// () =>  LoadGrids (e ) );
				await task . StartNew ( ( ) => LoadSqlData ( e ) );

				// Create a Collection as this is what the grouping system Demands
				CollectionView view = ( CollectionView ) CollectionViewSource . GetDefaultView ( SqlBankcollection );
				if ( view != null )
				{
					PropertyGroupDescription groupDescription = new PropertyGroupDescription ( "AcType" );
					view . GroupDescriptions . Add ( groupDescription );
				}
				else
				{
					//whhoops - no view
					Debug . WriteLine ($"Failed to create collectionView");
					Console . Beep ( 300, 3); 
				}
				Mouse . OverrideCursor = System . Windows . Input . Cursors . Arrow;
			}
		}
		private async Task<bool> LoadSqlData ( LoadedEventArgs e )
		{
			BankCollection bc = new BankCollection ( );
			bc = e . DataSource as BankCollection;
			//Sort data by AcType so we can show themgrouped
			var accounts = from items in bc
				       where ( items . AcType == 1 )
				       orderby items.AcType, items.CustNo, items.BankNo
				       select items;

			// massage view and create a new BankCollection as ItemsSource
			lview3 . Items . Clear ( );
			SqlBankcollection = new BankCollection (  );
			foreach ( var item in accounts )
			{
				SqlBankcollection . Add ( item );
			}
			MainWindow . TestBankcollection = SqlBankcollection;
			lview3 . ItemsSource = SqlBankcollection;
			lview3 . Refresh ( );
			return true;
		}

		private void Expander_Drop ( object sender, System . Windows . DragEventArgs e )
		{

		}
	}
}
