using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Data;
using System . Windows . Documents;
using System . Windows . Input;
using System . Windows . Media;
using System . Windows . Media . Imaging;
using System . Windows . Shapes;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for Listviews.xaml
	/// </summary>
	public partial class Listviews : Window
	{
		//SqlDbViewer tw = null;
		private BankCollection SqlBankcollection { get; set; }
		CollectionView view { get; set; }
		public Listviews ( )
		{
			InitializeComponent ( );

			//The binding of this class is to the Bank Class

			//Create a grouping  so we can layout by Sex - don't know how this works really
			//but it works well.  It reads the list of items in the Control (a ListView in this case)
			// and creates another form of Collection, a Collectionview()
			EventControl . BankDataLoaded += EventControl_BankDataLoaded;
			BankCollection . LoadBank ( SqlBankcollection, "BANKLISTVIEW", 1, true );
			Utils.SetupWindowDrag ( this );
		}

		private async Task<bool> LoadGrids ( LoadedEventArgs e )
		{
			SqlBankcollection = e . DataSource as BankCollection;
			//lview . ItemsSource = SqlBankcollection;
			//lview . DataContext = SqlBankcollection;
			//view = ( CollectionView ) CollectionViewSource . GetDefaultView ( SqlBankcollection );

			lview . Refresh ( );
			lview2 . ItemsSource = SqlBankcollection;
//			lview2 . DataContext = SqlBankcollection;
			lview2 . Refresh ( );

			lview3 . ItemsSource = SqlBankcollection;
//			lview3 . DataContext = SqlBankcollection;
			lview3 . Refresh ( );

			//Emplistbox7 . ItemsSource = view;
			//Emplistbox7 . DataContext = view;
			//Emplistbox7 . Refresh ( );

			//Emplistbox8 . ItemsSource = view;
			//Emplistbox8 . DataContext = view;
			//Emplistbox8 . Refresh ( );

			//Emplistbox9 . ItemsSource = view;
			//Emplistbox9 . DataContext = view;
			//Emplistbox9 . Refresh ( );

			return true ;
		}
		private async void EventControl_BankDataLoaded ( object sender, LoadedEventArgs e )
		{
			if ( e . CallerType != "BANKLISTVIEW" )
			{
				TaskFactory task = new TaskFactory ( );// () =>  LoadGrids (e ) );
				await task.StartNew( () => LoadGrids ( e ) );

				CollectionView view = ( CollectionView ) CollectionViewSource . GetDefaultView ( SqlBankcollection);
				if ( view != null )
				{
					PropertyGroupDescription groupDescription = new PropertyGroupDescription ( "CustNo" );
					view . GroupDescriptions . Add ( groupDescription );
				}
				//SqlBankcollection = e . DataSource as BankCollection;
				//lview . ItemsSource = SqlBankcollection;
				//lview . DataContext = SqlBankcollection;
				//view = ( CollectionView ) CollectionViewSource . GetDefaultView ( SqlBankcollection );

				////lview . Refresh ( );
				////lview2 . ItemsSource = SqlBankcollection;
				////lview2 . DataContext = SqlBankcollection;
				////lview2 . Refresh ( );

				////lview3 . ItemsSource = SqlBankcollection;
				////lview3 . DataContext = SqlBankcollection;
				////lview3 . Refresh ( );

				////Emplistbox7 . ItemsSource = view;
				////Emplistbox7 . DataContext = view;
				////Emplistbox7 . Refresh ( );

				//Emplistbox8 . ItemsSource = view;
				//Emplistbox8 . DataContext = view;
				//Emplistbox8 . Refresh ( );

				////Emplistbox9 . ItemsSource = view;
				////Emplistbox9 . DataContext = view;
				////Emplistbox9 . Refresh ( );
			}
		}

		private void Page1_Click ( object sender, RoutedEventArgs e )
		{
			//			this.NavigationService.Navigate(MainWindow._Page1);
		}
		private void Page2_Click ( object sender, RoutedEventArgs e )
		{
			//			this.NavigationService.Navigate(MainWindow._Page2);
		}
		private void Page3_Click ( object sender, RoutedEventArgs e )
		{
			//			this.NavigationService.Navigate(MainWindow._Page3);
		}
		private void ExitButton_Click ( object sender, RoutedEventArgs e )
		{ Application . Current . Shutdown ( ); }
		void Page1Button_Click ( object sender, RoutedEventArgs e )
		{
			//			this.NavigationService.Navigate(MainWindow._Page1);
		}

		private void Page2Button_Click ( object sender, RoutedEventArgs e )
		{
			//			this.NavigationService.Navigate(MainWindow._Page3);

		}

		private void Page4_Click ( object sender, RoutedEventArgs e )
		{
			//			this.NavigationService.Navigate(MainWindow._Page4);

		}
		private void Page5_Click ( object sender, RoutedEventArgs e )
		{
			SqlDbViewer tw = new SqlDbViewer ( true );
			tw . Show ( );
		}
		private void CloseButton_Click ( object sender, RoutedEventArgs e )
		{
			this . Close ( );
		}

	}
}
