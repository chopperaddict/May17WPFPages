using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Input;
using System . Windows . Media;

namespace WPFPages . Views
{
	public class Filtering
	{
		private string columnToFilterOn = "";
		private string filtervalue1 = "";
		private string filtervalue2 = "";
		private string operand = "";
		public bool FilterResult = false;
		private string IsFiltered = "";
		private string FilterCommand = "";
		private string CurrentDb = "";

		public string  DoFilters ( object obj , string currentDb , int mode = 0 )
		{
			string filterstring = "";
			CurrentDb = currentDb;
			Button Filters = new Button();
			// Make sure this window has it's pointer "Registered" cos we can
			// Click the button before the window has had focus set
			if ( Flags . CurrentSqlViewer != null )
			{
				Flags . CurrentSqlViewer = obj as SqlDbViewer;
				Filters = Flags . CurrentSqlViewer . Filters;
				Filters . Content = "Filtering";
			}
			else
			{
				if ( mode == 0 )
					Filters . Content = "Reset";
				else
					Filters . Content = "";
			}
			// Call up the Filtering Window to select
			// the filtering conditions required
			//			Window_GotFocus ( sender , null );

			//if ( CurrentDb == "" )
			//{
			//	MessageBox . Show ( "You need to have loaded one of the data tables\r\nbefore you can access the filtering system" );
			//	return;
			//}
			//if ( Filters . Content == "Reset" )
			//{
			//	Filters . Content = "Filter";
			//	// clear any previous filter command line data
			//	Flags . FilterCommand = "";
			//	if ( Flags . CurrentSqlViewer != null )
			//	{
			//		if ( CurrentDb == "BANKACCOUNT" )
			//			Flags . CurrentSqlViewer . ShowBank_Click ( null , null );
			//		else if ( CurrentDb == "CUSTOMER" )
			//			Flags . CurrentSqlViewer . ShowCust_Click ( null , null );
			//		else if ( CurrentDb == "DETAILS" )
			//			Flags . CurrentSqlViewer . ShowDetails_Click ( null , null );
			//		ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
			//		Filters . Template = tmp;
			//		Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
			//		Filters . Background = br;
			//		Filters . Content = "Filtering";
			//		//Tidy up general flags
			//		Flags . IsFiltered = false;
			//		Flags . FilterCommand = "";
			//		Flags . CurrentSqlViewer . ParseButtonText ( true );
			//	}
			//	Mouse . OverrideCursor = Cursors . Arrow;

			//	return;
			//}
			SQLFilter sf = new SQLFilter ( null );
			//// filter any table
			if ( CurrentDb == "BANKACCOUNT" )
			{
				sf . FilterList . Items . Clear ( );
				sf . FilterList . Items . Add ( "ID" );
				sf . FilterList . Items . Add ( "BANKNO" );
				sf . FilterList . Items . Add ( "CUSTNO" );
				sf . FilterList . Items . Add ( "ACTYPE" );
				sf . FilterList . Items . Add ( "BALANCE" );
				sf . FilterList . Items . Add ( "INTRATE" );
				sf . FilterList . Items . Add ( "ODATE" );
				sf . FilterList . Items . Add ( "CDATE" );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				sf . FilterList . Items . Clear ( );
				sf . FilterList . Items . Add ( "Id" );
				sf . FilterList . Items . Add ( "ID" );
				sf . FilterList . Items . Add ( "BANKNO" );
				sf . FilterList . Items . Add ( "CUSTNO" );
				sf . FilterList . Items . Add ( "ACTYPE" );
				sf . FilterList . Items . Add ( "FNAME" );
				sf . FilterList . Items . Add ( "LNAME" );
				sf . FilterList . Items . Add ( "ADDR1" );
				sf . FilterList . Items . Add ( "ADDR2" );
				sf . FilterList . Items . Add ( "TOWN" );
				sf . FilterList . Items . Add ( "COUNTY" );
				sf . FilterList . Items . Add ( "PCODE" );
				sf . FilterList . Items . Add ( "PHONE" );
				sf . FilterList . Items . Add ( "MOBILE" );
				sf . FilterList . Items . Add ( "DOB" );
				sf . FilterList . Items . Add ( "ODATE" );
				sf . FilterList . Items . Add ( "CDATE" );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				sf . FilterList . Items . Clear ( );
				sf . FilterList . Items . Add ( "ID" );
				sf . FilterList . Items . Add ( "BANKNO" );
				sf . FilterList . Items . Add ( "CUSTNO" );
				sf . FilterList . Items . Add ( "ACTYPE" );
				sf . FilterList . Items . Add ( "BALANCE" );
				sf . FilterList . Items . Add ( "INTRATE" );
				sf . FilterList . Items . Add ( "ODATE" );
				sf . FilterList . Items . Add ( "CDATE" );
			}
			sf . Operand . Items . Add ( "Equal to" );
			sf . Operand . Items . Add ( "Not Equal to" );
			sf . Operand . Items . Add ( "Greater than or Equal to" );
			sf . Operand . Items . Add ( "Less than or Equal to" );
			sf . Operand . Items . Add ( ">= value1 AND <= value2" );
			sf . Operand . Items . Add ( "> value1 AND < value2" );
			sf . Operand . Items . Add ( "< value1 OR > value2" );
			sf . Operand . SelectedIndex = 0;
			//			}
			sf . currentDb = CurrentDb;
			sf . FilterResult = false;
			sf . ShowDialog ( );
			if ( sf . FilterResult )
			{
				columnToFilterOn = sf . ColumnToFilterOn;
				filtervalue1 = sf . FilterValue . Text;
				filtervalue2 = sf . FilterValue2 . Text;
				operand = sf . operand;
				filterstring = DoFilter ( obj, null );
				Flags . FilterCommand = filterstring;
				//if ( Flags . CurrentSqlViewer != null )
				//{
				//	Flags . CurrentSqlViewer . StatusBar . Text = $"Filtered Results are shown above. Column = {columnToFilterOn}, Condition = {operand}, Value(s) = {filtervalue1}, {filtervalue2} ";
				//	Flags . CurrentSqlViewer . Filters . IsEnabled = false;
				//	Flags . CurrentSqlViewer . Filters . Content = "Reset";
				//	Flags . CurrentSqlViewer . Filters . IsEnabled = true;
				//	ControlTemplate ctmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGreen" );
				//	Flags . CurrentSqlViewer . Filters . Template = ctmp;
				//	Brush brs = Utils . GetDictionaryBrush ( "HeaderBrushGreen" );
				//	Flags . CurrentSqlViewer . Filters . Background = brs;
				//}
			}
			else
			{
				ControlTemplate tmp = Utils . GetDictionaryControlTemplate ( "HorizontalGradientTemplateGray" );
				if ( Flags . CurrentSqlViewer != null )
				{
					Flags . CurrentSqlViewer . Filters . Template = tmp;
					Brush br = Utils . GetDictionaryBrush ( "HeaderBrushGray" );
					Flags . CurrentSqlViewer . Filters . Background = br;
					Flags . CurrentSqlViewer . Filters . Content = "Filtering";
				}
			}
			return filterstring;

		}
		public string DoFilter ( object sender , MouseButtonEventArgs e )
		{
			// carry out the filtering operation
			string Commandline1 = "";
			string Commandline = "";
			if ( CurrentDb == "BANKACCOUNT" )
			{
				Commandline1 = $"Select * from BankAccount where ";
				BankCollection . dtBank . Clear ( );
			}
			else if ( CurrentDb == "CUSTOMER" )
			{
				Commandline1 = $"Select * from Customer where ";
				CustCollection . dtCust . Clear ( );
			}
			else if ( CurrentDb == "DETAILS" )
			{
				Commandline1 = $"Select * from SecAccounts where ";
				DetCollection . dtDetails . Clear ( );
			}

			if ( operand . Contains ( "Not Equal" ) )
				Commandline = Commandline1 + $" {columnToFilterOn} <> '{filtervalue1}'";
			else if ( operand . Contains ( "Greater than" ) )
				Commandline = Commandline1 + $" {columnToFilterOn} >= '{filtervalue1}'";
			else if ( operand . Contains ( "Less than" ) )
				Commandline = Commandline1 + $" {columnToFilterOn} <= '{filtervalue1}'";
			else if ( operand . Contains ( "Equal to" ) )
				Commandline = Commandline1 + $" {columnToFilterOn} = '{filtervalue1}'";
			else
			{
				if ( filtervalue1 == "" || filtervalue2 == "" )
				{
					MessageBox . Show ( "The Filter you have selected needs TWO seperate Values.\r\nUnable to continue with Filtering process..." );
					return "";
				}
				else if ( operand . Contains ( ">= value1 AND <= value2" ) )
					Commandline = Commandline1 + $" {columnToFilterOn} >= {filtervalue1} AND {columnToFilterOn} <= '{filtervalue2}'";
				else if ( operand . Contains ( "> value1 AND < value2" ) )
					Commandline = Commandline1 + $" {columnToFilterOn} > {filtervalue1} AND {columnToFilterOn} < '{filtervalue2}'";
				else if ( operand . Contains ( "< value1 OR > value2" ) )
					Commandline = Commandline1 + $" {columnToFilterOn} < {filtervalue1} OR {columnToFilterOn} > '{filtervalue2}'";
			}

			Commandline += " Order  by ";
			Commandline = Utils . GetDataSortOrder ( Commandline );
			Flags . FilterCommand = Commandline;
			Flags . IsFiltered = true;
			//	set file wide filter command line
			FilterCommand = Commandline;
			//if ( sender != null )
			//{
			//	if ( CurrentDb == "BANKACCOUNT" )
			//	{
			//		IsFiltered = "BANKACCOUNT";
			//		Flags.CurrentSqlViewer.ShowBank_Click ( null , null );
			//	}
			//	else if ( CurrentDb == "CUSTOMER" )
			//	{
			//		IsFiltered = "CUSTOMER";
			//		Flags . CurrentSqlViewer . ShowCust_Click ( null , null );
			//	}
			//	else if ( CurrentDb == "DETAILS" )
			//	{
			//		IsFiltered = "DETAILS";
			//		Flags . CurrentSqlViewer . ShowDetails_Click ( null , null );
			//	}

			//	Flags . CurrentSqlViewer . UpdateAuxilliaries ( "" );
			//}
			Mouse . OverrideCursor = Cursors . Arrow;
			return FilterCommand;
		}

	}
}
