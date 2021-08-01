using System;
using System . Collections . Generic;
using System . Diagnostics;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using WPFPages . ViewModels;

namespace WPFPages . Views
{
	public partial class LinqResults
	{
//		public BankCollection SqlViewerBankcollection = BankCollection . SqlViewerBankcollection;
//		public CustCollection SqlViewerCustcollection = CustCollection . SqlViewerCustcollection;
//		public DetCollection SqlViewerDetcollection = DetCollection . SqlViewerDetcollection;

		public LinqResults ( )
		{

		}

		//#region LINQ methods
		////*************************************************************************************************************//
		//public void Linq1_Query ( SqlDbViewer sender,  string CurrentDb, DataGrid dataGrid )
		//{
		//	//select items;
		//	if ( sender . GetType ( ) == typeof ( SqlDbViewer ) )
		//	{
		//		if ( CurrentDb == "BANKACCOUNT" )
		//		{
		//			var accounts = from items in SqlViewerBankcollection
		//				       where ( items . AcType == 1 )
		//				       orderby items . CustNo
		//				       select items;
		//			dataGrid . ItemsSource = accounts;
		//		}
		//		else if ( CurrentDb == "CUSTOMER" )
		//		{
		//			var accounts = from items in SqlViewerCustcollection
		//				       where ( items . AcType == 1 )
		//				       orderby items . CustNo
		//				       select items;
		//			dataGrid . ItemsSource = accounts;
		//		}
		//		else if ( CurrentDb == "DETAILS" )
		//		{
		//			var accounts = from items in SqlViewerDetcollection
		//				       where ( items . AcType == 1 )
		//				       orderby items . CustNo
		//				       select items;
		//			dataGrid . ItemsSource = accounts;
		//		}
		//	}
		//	else if ( sender . GetType ( ) == typeof ( MultiViewer ) )
		//	{
		//		if ( CurrentDb == "BANKACCOUNT" )
		//		{
		//			var accounts = from items in SqlViewerBankcollection
		//				       where ( items . AcType == 1 )
		//				       orderby items . CustNo
		//				       select items;
		//			dataGrid . ItemsSource = accounts;
		//		}
		//		else if ( CurrentDb == "CUSTOMER" )
		//		{
		//			var accounts = from items in SqlViewerCustcollection
		//				       where ( items . AcType == 1 )
		//				       orderby items . CustNo
		//				       select items;
		//			dataGrid . ItemsSource = accounts;
		//		}
		//		else if ( CurrentDb == "DETAILS" )
		//		{
		//			var accounts = from items in SqlViewerDetcollection
		//				       where ( items . AcType == 1 )
		//				       orderby items . CustNo
		//				       select items;
		//			dataGrid . ItemsSource = accounts;
		//		}
		//	}
		//}

		////*************************************************************************************************************//
		//public void Linq2_Query ( object sender, string CurrentDb, DataGrid dataGrid )
		//{
		//	//select items;
		//	if ( CurrentDb == "BANKACCOUNT" )
		//	{
		//		var accounts = from items in SqlViewerBankcollection
		//			       where ( items . AcType == 2 )
		//			       orderby items . CustNo
		//			       select items;
		//		dataGrid . ItemsSource = accounts;
		//	}
		//	else if ( CurrentDb == "CUSTOMER" )
		//	{
		//		var accounts = from items in SqlViewerCustcollection
		//			       where ( items . AcType == 2 )
		//			       orderby items . CustNo
		//			       select items;
		//		dataGrid . ItemsSource = accounts;
		//	}
		//	else if ( CurrentDb == "DETAILS" )
		//	{
		//		var accounts = from items in SqlViewerDetcollection
		//			       where ( items . AcType == 2 )
		//			       orderby items . CustNo
		//			       select items;
		//		dataGrid . ItemsSource = accounts;
		//	}
		//}

		////*************************************************************************************************************//
		//public void Linq3_Query ( object sender, string CurrentDb, DataGrid dataGrid )
		//{
		//	//select items;
		//	if ( CurrentDb == "BANKACCOUNT" )
		//	{
		//		var accounts = from items in SqlViewerBankcollection
		//			       where ( items . AcType == 3 )
		//			       orderby items . CustNo
		//			       select items;
		//		dataGrid . ItemsSource = accounts;
		//	}
		//	else if ( CurrentDb == "CUSTOMER" )
		//	{
		//		var accounts = from items in SqlViewerCustcollection
		//			       where ( items . AcType == 3 )
		//			       orderby items . CustNo
		//			       select items;
		//		dataGrid . ItemsSource = accounts;
		//	}
		//	else if ( CurrentDb == "DETAILS" )
		//	{
		//		var accounts = from items in SqlViewerDetcollection
		//			       where ( items . AcType == 3 )
		//			       orderby items . CustNo
		//			       select items;
		//		dataGrid . ItemsSource = accounts;
		//	}
		//}

		////*************************************************************************************************************//
		//public void Linq4_Query ( object sender, string CurrentDb, DataGrid dataGrid )
		//{
		//	//select items;
		//	if ( CurrentDb == "BANKACCOUNT" )
		//	{
		//		var accounts = from items in SqlViewerBankcollection
		//			       where ( items . AcType == 4 )
		//			       orderby items . CustNo
		//			       select items;
		//		dataGrid . ItemsSource = accounts;
		//	}
		//	else if ( CurrentDb == "CUSTOMER" )
		//	{
		//		var accounts = from items in SqlViewerCustcollection
		//			       where ( items . AcType == 4 )
		//			       orderby items . CustNo
		//			       select items;
		//		dataGrid . ItemsSource = accounts;
		//	}
		//	else if ( CurrentDb == "DETAILS" )
		//	{
		//		var accounts = from items in SqlViewerDetcollection
		//			       where ( items . AcType == 4 )
		//			       orderby items . CustNo
		//			       select items;
		//		dataGrid . ItemsSource = accounts;
		//	}
		//}

		////*************************************************************************************************************//
		//public void Linq5_Query ( object sender, string CurrentDb, DataGrid dataGrid )
		//{
		//	if ( CurrentDb == "BANKACCOUNT" )
		//	{
		//		//select All the items first;
		//		var accounts = from items in SqlViewerBankcollection orderby items . CustNo, items . AcType select items;
		//		//Next Group collection on Custno
		//		var grouped = accounts . GroupBy (
		//			b => b . CustNo );

		//		//Now filter content down to only those a/c's with multiple Bank A/c's
		//		var sel = from g in grouped
		//			  where g . Count ( ) > 1
		//			  select g;

		//		// Finally, iterate thru the list of grouped CustNo's matching to CustNo in the full Bankaccounts data
		//		// giving us ONLY the full records for any recordss that have > 1 Bank accounts
		//		List<BankAccountViewModel> output = new List<BankAccountViewModel> ( );
		//		foreach ( var item1 in sel )
		//		{
		//			foreach ( var item2 in accounts )
		//			{
		//				if ( item2 . CustNo . ToString ( ) == item1 . Key )
		//				{ output . Add ( item2 ); }
		//			}
		//		}
		//		dataGrid . ItemsSource = output;
		//	}
		//	if ( CurrentDb == "CUSTOMER" )
		//	{
		//		//select All the items first;
		//		var accounts = from items in SqlViewerCustcollection orderby items . CustNo, items . AcType select items;
		//		//Next Group collection on Custno
		//		var grouped = accounts . GroupBy (
		//			b => b . CustNo );

		//		//Now filter content down to only those a/c's with multiple Bank A/c's
		//		var sel = from g in grouped
		//			  where g . Count ( ) > 1
		//			  select g;

		//		// Finally, iterate thru the list of grouped CustNo's matching to CustNo in the full Bankaccounts data
		//		// giving us ONLY the full records for any recordss that have > 1 Bank accounts
		//		List<CustomerViewModel> output = new List<CustomerViewModel> ( );
		//		foreach ( var item1 in sel )
		//		{
		//			foreach ( var item2 in accounts )
		//			{
		//				if ( item2 . CustNo . ToString ( ) == item1 . Key )
		//				{ output . Add ( item2 ); }
		//			}
		//		}
		//		dataGrid . ItemsSource = output;
		//	}
		//	else if ( CurrentDb == "DETAILS" )
		//	{
		//		//select All the items first;
		//		var accounts = from items in SqlViewerDetcollection orderby items . CustNo, items . AcType select items;
		//		//Next Group collection on Custno
		//		var grouped = accounts . GroupBy (
		//			b => b . CustNo );

		//		//Now filter content down to only those a/c's with multiple Bank A/c's
		//		var sel = from g in grouped
		//			  where g . Count ( ) > 1
		//			  select g;

		//		// Finally, iterate thru the list of grouped CustNo's matching to CustNo in the full Bankaccounts data
		//		// giving us ONLY the full records for any recordss that have > 1 Bank accounts
		//		List<DetailsViewModel> output = new List<DetailsViewModel> ( );
		//		foreach ( var item1 in sel )
		//		{
		//			foreach ( var item2 in accounts )
		//			{
		//				if ( item2 . CustNo . ToString ( ) == item1 . Key )
		//				{ output . Add ( item2 ); }
		//			}
		//		}
		//		dataGrid . ItemsSource = output;
		//	}
		//}
		////*************************************************************************************************************//
		//public async void Linq6_Query ( object sender, string CurrentDb, DataGrid dataGrid )
		//{
		//	var accounts = from items in SqlViewerBankcollection orderby items . CustNo, items . AcType select items;
		//	var accounts1 = from items in SqlViewerCustcollection orderby items . CustNo, items . AcType select items;
		//	var accounts2 = from items in SqlViewerDetcollection orderby items . CustNo, items . AcType select items;
		//	SqlDbViewer sqldb = sender as SqlDbViewer;
		//	if ( dataGrid . Items . Count > 0 )
		//	{
		//		dataGrid . ItemsSource = null;
		//		BankCollection . LoadBank ( SqlViewerBankcollection, 1, true );
		//		dataGrid . Refresh ( );
		//	}
		//	else if ( dataGrid . Items . Count > 0 )
		//	{
		//		dataGrid . ItemsSource = null;
		//		await CustCollection . LoadCust ( SqlViewerCustcollection, 1, true );
		//		dataGrid . Refresh ( );
		//	}
		//	else if ( dataGrid . Items . Count > 0 )
		//	{
		//		dataGrid . ItemsSource = null;
		//		await DetCollection . LoadDet ( SqlViewerDetcollection, 1, true );
		//		dataGrid . Refresh ( );
		//	}
		//}
		//#endregion LINQ methods


	}
}
