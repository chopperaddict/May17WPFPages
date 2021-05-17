using System;
using System . Diagnostics;
using System . Runtime . CompilerServices;
using System . Windows . Controls;
using System . Windows . Threading;

using WPFPages . ViewModels;

namespace WPFPages . Views
{
	#region chime
	public delegate void ChimeEventHandler ( );

	public class ClockTower
	{
		// declare event based on delegate above
		public static event ChimeEventHandler Chime;

		public static void sChimeFivePm ( )
		{
			Console . WriteLine ( $"" );
			Chime ( );
		}

		public static void sChimeSixAm ( )
		{
			Console . WriteLine ( $"" );
			Chime ( );
		}

		public void ChimeFivePm ( )
		{
			Console . WriteLine ( $"" );
			Chime ( );
		}

		public void ChimeSixAm ( )
		{
			Console . WriteLine ( $"" );
			Chime ( );
		}
	}
	#endregion chime


	public class EventHandlers
	{
		#region DELEGATES IN USE

		//Delegates I AM USING
//		public static NotifyViewer SendViewerCommand;

		#endregion DELEGATES IN USE

		public MainWindow mw = null;

		/// ONLY CONSTRUCTOR (Specialised)
		/// <param name="edb"> This is the Caller class</param>
		/// <param name="dg"> This is the (Active) Datagrid in the above class</param>
		public EventHandlers ( DataGrid dg , string CallerName , out EventHandlers thisWin )
		{
			thisWin = this;
			if ( !BankAccountViewModel . ShowSubscribeData )
				return;
			//			int count = 0;
			//			int count2 = 0;
			//if ( Flags . EventHandlerDebug )
			//{
			Console . WriteLine ( $"EventHandler.EventHandlers(51) : In Constructor - CallerName = {CallerName}." );
		}

		// Not used if DEBUG is UnDefined
		[Conditional ( "DEBUG" )]
		public static void ShowSubscribersCount ( )
		{
			int count = -1;
			int count2 = -1;
			int count3 = -1;
			int count4 = -1;
			int count5 = -1;
			int count6 = -1;
			int count7 = -1;
			int count8 = -1;
			int count9 = -1;
			int count10 =-1;
			int count11 =-1;
			if ( !BankAccountViewModel . ShowSubscribeData )
				return;
			//ViewerDataHasBeenChanged
			Delegate [ ] dg = EventControl . GetEventCount ( );
			if ( dg != null ) count = dg . Length;
			// NotifyOfDataChange
			dg = EventControl . GetEventCount2 ( );
			if ( dg != null ) count2 = dg . Length;
			// NotifyOfDataLoaded
			dg = EventControl . GetEventCount3 ( );
			if ( dg != null ) count3 = dg . Length;
			// DetCollection. BankDataLoaded
			dg = EventControl . GetEventCount6 ( );
			if ( dg != null ) count6 = dg . Length;
			// CustCollection. CustDataLoaded 
			dg = EventControl . GetEventCount7 ( );
			if ( dg != null ) count7 = dg . Length;
			// CustCollection. DetDataLoaded 
			dg = EventControl . GetEventCount8 ( );
			if ( dg != null ) count8 = dg . Length;
			//SQLHandlers. DataUpdated
			dg = EditDb . GetEventCount9 ( );
			if ( dg != null ) count9 = dg . Length;
			//AllViewersUpdate
			dg = EditDb . GetEventCount9 ( );
			if ( dg != null ) count10 = dg . Length;

			//RecordDeleted
			dg = EventControl . GetEventCount11 ( );
			if ( dg != null ) count11 = dg . Length;

			Console . WriteLine ( $"\n *** Currently Subscribed Events  ***" );
			if ( count < 0 )
				Console . WriteLine ( $"ViewerDataHasBeenChanged=  " );
			else
				Console . WriteLine ( $"ViewerDataHasBeenChanged		= {count} " );
			if ( count2 < 0 )
				Console . WriteLine ( $"NotifyOfDataChange				= " );
			else
				Console . WriteLine ( $"NotifyOfDataChange				= {count2}" );
			if ( count3 < 0 )
				Console . WriteLine ( $"NotifyOfDataLoaded				= " );
			else
				Console . WriteLine ( $"NotifyOfDataLoaded				= {count3}" );
			if ( count6 < 0 )
				Console . WriteLine ( $"BankCollection. BankDataLoaded	= " );
			else
				Console . WriteLine ( $"BankCollection. BankDataLoaded	= {count6}" );
			if ( count7 < 0 )
				Console . WriteLine ( $"CustCollection. CustDataLoaded	= " );
			else
				Console . WriteLine ( $"CustCollection. CustDataLoaded	= {count7}" );
			if ( count8 < 0 )
				Console . WriteLine ( $"DetCollection. DetDataLoaded	= " );
			else
				Console . WriteLine ( $"DetCollection. DetDataLoaded	= {count8}" );
			if ( count9 < 0 )
				Console . WriteLine ( $"SQLHandler.DataUpdated			= " );
			else
				Console . WriteLine ( $"SQLHandler.DataUpdated			= {count9}" );
			if ( count10 < 0 )
				Console . WriteLine ( $"DbEdit.AllViewersUpdate			= " );
			else
				Console . WriteLine ( $"DbEdit.AllViewersUpdate			= {count10}" );
			if ( count11 < 0 )
				Console . WriteLine ( $"RecordDeleted					= " );
			else
				Console . WriteLine ( $"RecordDeleted					= {count11}" );

			bool first = true;
			Delegate [ ] dglist2 = EventControl . GetEventCount ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Console . WriteLine ( $"=====================================================================================" ); first = false;
				}
				Console . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Console . WriteLine ( );
					//item . CurrentDb;
					Console . WriteLine ( $"Delegate : VIEWERDATAHASBEENCHANGED :\n >>> {item . Target}\nMethod = {item . Method . Name . ToString ( )}" );
					cnt++;
				}
			}
			dglist2 = EventControl . GetEventCount2 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Console . WriteLine ( $"=====================================================================================" ); first = false;
				}
				first = true;
				Console . WriteLine ( $"=====================================================================================" );
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Console . WriteLine ( );
					Console . WriteLine ( $"Delegate : NOTIFYOFDATACHANGE : \n >>> {item . Target . ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					cnt++;
				}
			}
			dglist2 = EventControl . GetEventCount3 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Console . WriteLine ( $"=====================================================================================" ); first = false;
				}
				first = true;
				Console . WriteLine ( $"=====================================================================================" );
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Console . WriteLine ( );
					Console . WriteLine ( $"Delegate : NOTIFYOFDATALOADED: \n >>> {item . Target . ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					cnt++;
				}
			}
			dglist2 = EventControl . GetEventCount6 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Console . WriteLine ( $"=====================================================================================" ); first = false;
				}
				Console . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Console . WriteLine ( );
					if ( item . Target != null )
						Console . WriteLine ( $"Delegate : BankCollection. BANKDATALOADED:\n >>> {item . Target?.ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					else
						Console . WriteLine ( $"Delegate : BankCollection. BANKDATALOADED:\n >>> \nMethod = {item . Method . ToString ( )}" );
					cnt++;
				}
			}
			dglist2 = EventControl . GetEventCount7 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Console . WriteLine ( $"=====================================================================================" ); first = false;
				}
				Console . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Console . WriteLine ( );
					if ( item . Target != null )
						Console . WriteLine ( $"Delegate : CustCollection. CUSTDATALOADED:\n >>> {item . Target?.ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					else
						Console . WriteLine ( $"Delegate : CustCollection. CUSTDATALOADED:\n >>> \nMethod = {item . Method . ToString ( )}" );
					cnt++;
				}
			}
			dglist2 = EventControl . GetEventCount8 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Console . WriteLine ( $"=====================================================================================" ); first = false;
				}
				Console . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Console . WriteLine ( );
					if ( item . Target != null )
						Console . WriteLine ( $"Delegate : DetCollection. DETDATALOADED:\n >>> {item . Target?.ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					else
						Console . WriteLine ( $"Delegate : DetCollection. DETDATALOADED:\n >>> \nMethod = {item . Method . ToString ( )}" );
					cnt++;
				}
			}
			dglist2 = EditDb . GetEventCount9 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Console . WriteLine ( $"=====================================================================================" ); first = false;
				}
				Console . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Console . WriteLine ( );
					if ( item . Target != null )
						Console . WriteLine ( $"Delegate : SQLHandlers.DATAUPDATED:\n >>> {item . Target?.ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					else
						Console . WriteLine ( $"Delegate : SQLHandlers.DATAUPDATED::\n >>> \nMethod = {item . Method . ToString ( )}" );
					cnt++;
				}
			}
			dglist2 = EditDb . GetEventCount10 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Console . WriteLine ( $"=====================================================================================" ); first = false;
				}
				Console . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Console . WriteLine ( );
					if ( item . Target != null )
						Console . WriteLine ( $"Delegate : DbEdit.ALLVIEWERSUPDATE :\n >>> {item . Target?.ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					else
						Console . WriteLine ( $"Delegate : DbEdit.ALLVIEWERSUPDATE :\n >>> \nMethod = {item . Method . ToString ( )}" );
					cnt++;
				}
			}
			dglist2 = EventControl . GetEventCount11 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Console . WriteLine ( $"=====================================================================================" ); first = false;
				}
				Console . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Console . WriteLine ( );
					if ( item . Target != null )
						Console . WriteLine ( $"Delegate : RECORDDELETED:\n >>> {item . Target?.ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					else
						Console . WriteLine ( $"Delegate : RECORDDELETED:\n >>> \nMethod = {item . Method . ToString ( )}" );
					cnt++;
				}
			}

			Console . WriteLine ( $"+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++" );
		}
	}

	public static class DispatcherExtensions
		{
			public static SwitchToUiAwaitable SwitchToUi ( this Dispatcher dispatcher )
			{
				return new SwitchToUiAwaitable ( dispatcher );
			}

			public struct SwitchToUiAwaitable : INotifyCompletion
			{
				private readonly Dispatcher _dispatcher;

				public SwitchToUiAwaitable ( Dispatcher dispatcher )
				{
					_dispatcher = dispatcher;
				}

				public SwitchToUiAwaitable GetAwaiter ( )
				{
					return this;
				}

				public void GetResult ( )
				{
				}

				public bool IsCompleted => _dispatcher . CheckAccess ( );

				public void OnCompleted ( Action continuation )
				{
					_dispatcher . BeginInvoke ( continuation );
				}
			}
		}

} // End namespace
