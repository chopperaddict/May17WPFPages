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
using WPFPages.ViewModels;

namespace WPFPages . Views
{
	/// <summary>
	/// Interaction logic for RowInfoPopup.xaml
	/// </summary>
	public partial class RowInfoPopup : Window
	{
		public RowInfoPopup (string callerType )
		{
			InitializeComponent ( );
			if ( callerType == "BANKACCOUNT" )
			{
				BankLabels . Visibility = Visibility . Visible;
				BankData . Visibility = Visibility . Visible;
				CustLabels . Visibility = Visibility . Hidden;
				CustData . Visibility = Visibility . Hidden;
			}
			if ( callerType == "CUSTOMER" )
			{
				CustData . Visibility = Visibility . Visible;
				CustLabels . Visibility = Visibility . Visible;
				BankLabels . Visibility = Visibility . Hidden;
				BankData . Visibility = Visibility . Hidden;
			}
			if ( callerType == "DETAILS" )
			{
				BankLabels . Visibility = Visibility . Visible;
				BankData . Visibility = Visibility . Visible;
				CustLabels . Visibility = Visibility . Hidden;
				CustData . Visibility = Visibility . Hidden;
			}
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
