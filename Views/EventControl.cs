using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows;
using System . Windows . Controls;

namespace WPFPages . Views
{
	#region KNOWN DELEGATES IN USE

	public delegate bool DbReloaded ( object sender , DataLoadedArgs args );

	public delegate void DbUpdated ( SqlDbViewer sender , DataGrid Grid , DataChangeArgs args );

	public delegate void EditDbGridSelectionChanged ( int ChangeType , int value , string caller );

	public delegate void EditDbDataChanged ( int EditDbChangeType , int row , string CurentDb );

	public delegate void NotifyViewer ( int status , string info , SqlDbViewer NewSqlViewer );

	public delegate void SQLViewerSelectionChanged ( int ChangeType , int row , string CurrentDb );

	public delegate void SqlSelectedRowChanged ( int ChangeType , int row , string CurentDb );

	public delegate void SqlViewerNotify ( int status , string info , SqlDbViewer NewSqlViewer );

	public delegate void DeletionHandler ( string Source , string bankno , string custno , int CurrrentRow );


	#endregion KNOWN DELEGATES IN USE

	public class EventControl
	{
		#region ALL NEW EVENTS

		// THIS IS  HOW  TO HANDLE EVENTS RIGHT NOW  - WORKING WELL 4/5/21//
		// We no longer need to delcare a Delegate  to do this
		//Event CallBack for when Asynchronous data loading has been completed in the Various ViewModel classes

		// Main Event handlersfor when DB's load data from disk
		public static  event EventHandler<LoadedEventArgs> BankDataLoaded;
		public static  event EventHandler<LoadedEventArgs> CustDataLoaded;
		public static  event EventHandler<LoadedEventArgs> DetDataLoaded;

		public static event EventHandler<IndexChangedArgs> ViewerIndexChanged;
		public static event EventHandler<IndexChangedArgs> EditIndexChanged;
		public static event EventHandler<IndexChangedArgs> MultiViewerIndexChanged;
		public static event EventHandler<IndexChangedArgs> ForceEditDbIndexChanged;

		// Used to broadcast to everyone
		public static event EventHandler<LoadedEventArgs> ViewerDataUpdated;
		public static event EventHandler<LoadedEventArgs> EditDbDataUpdated;
		public static event EventHandler<LoadedEventArgs> MultiViewerDataUpdated;

		public static event EventHandler<LoadedEventArgs> DataUpdated;
		// Event we TRIGGER to notify SqlViewer of  a selectedindex change
		// uses delegate : public delegate void EditDbDataChanged ( int EditDbChangeType , int row , string CurentDb );
//		public static event EditDbDataChanged ViewerDataHasBeenChanged;

		// Uses Delegate : public delegate void DbUpdated ( SqlDbViewer sender , DataGrid Grid , DataChangeArgs args );
		public static event DbUpdated NotifyOfDataChange;

		//	create a record deletion delegate handle to use through the code
		// to assing methods to dynamically
		// used delegate : public delegate void DeletionHandler ( string Source , string bankno , string custno , int CurrrentRow );
		public static event DeletionHandler RecordDeleted;



		#endregion ALL NEW EVENTS


		public EventControl ( )
		{
		}

		/// <summary>
		/// Central point for TRIGGERING this event
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="e"></param>

//dummy to stop error only
		public static void TriggerDataUpdated ( object obj, LoadedEventArgs e )
		{
			DataUpdated?.Invoke ( obj, e );
			Console . WriteLine ( $"DEBUG : In EventControl : Sending  DataUpdated EVENT trigger" );
		}

		// INDEX CHANGE EVENTS
		public static void TriggerForceEditDbIndexChanged ( object obj, IndexChangedArgs e )
		{
			Console . WriteLine ( $"DEBUG : In EventControl : Sending  ForceEditDbIndexChanged EVENT trigger" );
			ForceEditDbIndexChanged?.Invoke ( obj, e );
		}
		public static void TriggerEditDbIndexChanged ( object obj, IndexChangedArgs e )
		{
			Console . WriteLine ( $"DEBUG : In EventControl : Sending  EditIndexChanged EVENT trigger" );
			EditIndexChanged?.Invoke ( obj, e );
		}
		public static void TriggerViewerIndexChanged ( object obj, IndexChangedArgs e )
		{
			Console . WriteLine ( $"DEBUG : In EventControl : Sending  ViewerIndexChanged EVENT trigger (from {obj?.ToString ( )})" );
			ViewerIndexChanged?.Invoke ( obj, e );
		}
		public static void TriggerMultiViewerIndexChanged ( object obj, IndexChangedArgs e )
		{
			Console . WriteLine ( $"DEBUG : In EventControl : Sending  MultiViewerIndexChanged EVENT trigger (from{obj?.ToString ( )})" );
			MultiViewerIndexChanged?.Invoke ( obj, e );
		}
		// DATA CHANGE EVENTS
		public static void TriggerViewerDataUpdated ( object obj, LoadedEventArgs e )
		{
			Console . WriteLine ( $"DEBUG : In EventControl : Sending  ViewerDataUpdated EVENT trigger (from{obj? . ToString ( )})" );
			ViewerDataUpdated?.Invoke ( obj, e );
		}
		public static void TriggerEditDbDataUpdated ( object obj, LoadedEventArgs e )
		{
			Console . WriteLine ( $"DEBUG : In EventControl : Sending  EditDbDataUpdated EVENT trigger (from{obj? . ToString ( )})" );
			EditDbDataUpdated?.Invoke ( obj, e );
		}
		public static void TriggerMultiViewerDataUpdated ( object obj, LoadedEventArgs e )
		{
			Console . WriteLine ( $"DEBUG : In EventControl : Sending  MultiViewerDataUpdated EVENT trigger (from{obj? . ToString ( )})" );
			MultiViewerDataUpdated?.Invoke ( obj, e );
		}
		public static void TriggerBankDataLoaded ( object obj , LoadedEventArgs e )
		{
			Console . WriteLine ( $"DEBUG : In EventControl : Sending  BankDataLoaded EVENT trigger (from{obj? . ToString ( )})" );
			BankDataLoaded?.Invoke ( obj, e );
		}
		public static void TriggerCustDataLoaded ( object obj , LoadedEventArgs e )
		{
			Console . WriteLine ( $"DEBUG : In EventControl : Sending  CustDataLoaded EVENT trigger (from{obj ?. ToString ( )})" );
			CustDataLoaded?.Invoke ( obj, e );
		}
		public static void TriggerDetDataLoaded ( object obj , LoadedEventArgs e )
		{
			Console . WriteLine ( $"DEBUG : In EventControl : Sending  DetDataLoaded EVENT trigger (from{obj? . ToString ( )})" );
			DetDataLoaded?.Invoke ( obj, e );
		}
		//public static void TriggerViewerDataChanged ( int EditDbChangeType , int row , string CurentDb )
		//{
		//	ViewerDataHasBeenChanged?.Invoke ( EditDbChangeType , row , CurentDb );
		//}
		public static void TriggerNotifyOfDataChange ( SqlDbViewer sender , DataGrid Grid , DataChangeArgs args )
		{
			NotifyOfDataChange?.Invoke ( sender , Grid , args );
		}

		public static void TriggerRecordDeleted ( string Source , string bankno , string custno , int CurrrentRow )
		{
			RecordDeleted?. Invoke ( Source , bankno , custno , CurrrentRow );
		}

		#region DEBUG utilities
		//public static Delegate [ ] GetEventCount ( )
		//{
		//	Delegate [ ] dglist2 = null;
		//	if ( ViewerDataHasBeenChanged != null )
		//		dglist2 = ViewerDataHasBeenChanged?.GetInvocationList ( );
		//	return dglist2;
		//}

		//public static Delegate [ ] GetEventCount2 ( )
		//{
		//	Delegate [ ] dglist2 = null;
		//	if ( NotifyOfDataChange != null )
		//		dglist2 = NotifyOfDataChange?.GetInvocationList ( );
		//	return dglist2;
		//}

		//public static Delegate [ ] GetEventCount3 ( )
		//{
		//	Delegate [ ] dglist2 = null;
		//	if ( ViewerDataHasBeenChanged != null )
		//		dglist2 = ViewerDataHasBeenChanged?.GetInvocationList ( );
		//	return dglist2;
		//}

		public static Delegate [ ] GetEventCount4 ( )
		{
			Delegate [ ] dglist2 = null;
			if ( EditIndexChanged != null )
				dglist2 = EditIndexChanged?.GetInvocationList ( );
			return dglist2;
		}

		public static Delegate [ ] GetEventCount5 ( )
		{
			Delegate [ ] dglist2 = null;
			if ( ViewerIndexChanged != null )
				dglist2 = ViewerIndexChanged?.GetInvocationList ( );
			return dglist2;
		}
		
		public static Delegate [ ] GetEventCount6 ( )
		{
			Delegate [ ] dglist2 = null;
			if ( EventControl . BankDataLoaded != null )
				dglist2 = BankDataLoaded?.GetInvocationList ( );
			return dglist2;
		}

		public static Delegate [ ] GetEventCount7 ( )
		{
			Delegate [ ] dglist2 = null;
			if ( CustDataLoaded != null )
				dglist2 = CustDataLoaded?.GetInvocationList ( );
			return dglist2;
		}

		public static Delegate [ ] GetEventCount8 ( )
		{
			Delegate [ ] dglist2 = null;
			if ( DetDataLoaded != null )
				dglist2 = DetDataLoaded?.GetInvocationList ( );
			return dglist2;
		}

		public static Delegate [ ] GetEventCount9 ( )
		{
			Delegate [ ] dglist2 = null;
			if ( RecordDeleted != null )
				dglist2 = RecordDeleted?.GetInvocationList ( );
			return dglist2;
		}
		public static Delegate [ ] GetEventCount10 ( )
		{
			Delegate [ ] dglist2 = null;
			if ( ViewerIndexChanged != null )
				dglist2 = ViewerIndexChanged?.GetInvocationList ( );
			return dglist2;
		}
		public static Delegate [ ] GetEventCount11 ( )
		{
			Delegate [ ] dglist2 = null;
			if ( DataUpdated != null )
				dglist2 = DataUpdated?.GetInvocationList ( );
			return dglist2;
		}

		#endregion DEBUG utilities

	}
}
