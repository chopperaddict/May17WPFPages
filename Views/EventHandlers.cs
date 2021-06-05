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
			Debug . WriteLine ( $"" );
			Chime ( );
		}

		public static void sChimeSixAm ( )
		{
			Debug . WriteLine ( $"" );
			Chime ( );
		}

		public void ChimeFivePm ( )
		{
			Debug . WriteLine ( $"" );
			Chime ( );
		}

		public void ChimeSixAm ( )
		{
			Debug . WriteLine ( $"" );
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

			Delegate[] dg = EventControl . GetEventCount4 ( );
			if ( dg != null ) count4 = dg . Length;
			
			/// ViewerIndexChanged
			dg = EventControl . GetEventCount5 ( );
			if ( dg != null ) count5 = dg . Length;

			// DetCollection. BankDataLoaded
			dg = EventControl . GetEventCount6 ( );
			if ( dg != null ) count6 = dg . Length;
			
			// CustCollection. CustDataLoaded 
			dg = EventControl . GetEventCount7 ( );
			if ( dg != null ) count7 = dg . Length;
			
			// CustCollection. DetDataLoaded 
			dg = EventControl . GetEventCount8 ( );
			if ( dg != null ) count8 = dg . Length;
			//RecordDeleted
			dg = EventControl . GetEventCount9 ( );
			if ( dg != null ) count9 = dg . Length;
			//MultiViewerIndexChanged
			dg = EventControl . GetEventCount10 ( );
			if ( dg != null ) count10 = dg . Length;

			/*
			 *	Active Control Events
				BankDataLoaded;
				CustDataLoaded;
				DetDataLoaded;

				 ViewerIndexChanged;
				 EditIndexChanged;
				 MultiViewerIndexChanged;
				 ForceEditDbIndexChanged;

				ViewerDataUpdated;
				EditDbDataUpdated;
				MultiViewerDataUpdated;

				DataUpdated;
				RecordDeleted;
			 */

			Debug . WriteLine ( $"\n *** Currently Subscribed Events  ***\nDATA LOADED EVENTS" );

			if ( count6 < 0 )
				Debug . WriteLine ( $"BankCollection. BankDataLoaded	= 0" );
			else
				Debug . WriteLine ( $"BankCollection. BankDataLoaded	= {count6}" );
			if ( count7 < 0 )
				Debug . WriteLine ( $"CustCollection. CustDataLoaded	= 0" );
			else
				Debug . WriteLine ( $"CustCollection. CustDataLoaded	= {count7}" );
			if ( count8 < 0 )
				Debug . WriteLine ( $"DetCollection. DetDataLoaded	= 0" );
			else
				Debug . WriteLine ( $"DetCollection. DetDataLoaded	= {count8}" );

			Debug . WriteLine ( $"\n INDEX CHANGED EVENTS" );
			if ( count4 < 0 )
				Debug . WriteLine ( $"EditIndexChanged				= 0" );
			else
				Debug . WriteLine ( $"EditIndexChanged				= {count4}" );
			if ( count5 < 0 )
				Debug . WriteLine ( $"ViewerIndexChanged				= 0" );
			else
				Debug . WriteLine ( $"ViewerIndexChanged				= {count5}" );
			if ( count10 < 0 )
				Debug . WriteLine ( $"MultiViewerIndexChanged			= 0" );
			else
				Debug . WriteLine ( $"MultiViewerIndexChanged			= {count10}" );

			Debug . WriteLine ( $"\n RECORD DELETION EVENTS" );
			if ( count9 < 0 )
				Debug . WriteLine ( $"RecordDeleted					= 0" );
			else
				Debug . WriteLine ( $"RecordDeleted					= {count9}" );

			bool first = true;

			Delegate [ ] dglist2 = EventControl . GetEventCount6 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Debug . WriteLine ( $"=====================================================================================" ); first = false;
				}
				Debug . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Debug . WriteLine ( "" );
					if ( item . Target != null )
						Debug . WriteLine ( $"Event : BANKDATALOADED:\n >>> {item . Target?.ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					else
						Debug . WriteLine ( $"Event : BANKDATALOADED:\n >>> \nMethod = {item . Method . ToString ( )}" );
					cnt++;
				}
			}
			dglist2 = EventControl . GetEventCount7 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Debug . WriteLine ( $"=====================================================================================" ); first = false;
				}
				Debug . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Debug . WriteLine ( "" );
					if ( item . Target != null )
						Debug . WriteLine ( $"Event : CUSTDATALOADED:\n >>> {item . Target?.ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					else
						Debug . WriteLine ( $"Event : CUSTDATALOADED:\n >>> \nMethod = {item . Method . ToString ( )}" );
					cnt++;
				}
			}
			dglist2 = EventControl . GetEventCount8 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Debug . WriteLine ( $"=====================================================================================" ); first = false;
				}
				Debug . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Debug . WriteLine ( "" );
					if ( item . Target != null )
						Debug . WriteLine ( $"Event : DETDATALOADED:\n >>> {item . Target?.ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					else
						Debug . WriteLine ( $"Event : DETDATALOADED:\n >>> \nMethod = {item . Method . ToString ( )}" );
					cnt++;
				}
			}

			dglist2 = EventControl . GetEventCount4 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Debug . WriteLine ( $"=====================================================================================" ); first = false;
				}
				first = true;
				Debug . WriteLine ( $"=====================================================================================" );
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Debug . WriteLine ("");
					Debug . WriteLine ( $"Event : EDITINDEXCHANGED: \n >>> {item . Target . ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					cnt++;
				}
			}
			dglist2 = EventControl . GetEventCount5 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Debug . WriteLine ( $"=====================================================================================" ); first = false;
				}
				Debug . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Debug . WriteLine ("");
					if ( item . Target != null )
						Debug . WriteLine ( $"Event : VIEWERINDEXCHANGED:\n >>> {item . Target?.ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					else
						Debug . WriteLine ( $"Event : VIEWERINDEXCHANGED:\n >>> \nMethod = {item . Method . ToString ( )}" );
					cnt++;
				}
			}
			dglist2 = EventControl . GetEventCount10 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Debug . WriteLine ( $"=====================================================================================" ); first = false;
				}
				Debug . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Debug . WriteLine ("");
					if ( item . Target != null )
						Debug . WriteLine ( $"Event : MULTIVIEWERINDEXCHANGED :\n >>> {item . Target?.ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					else
						Debug . WriteLine ( $"Event : MULTIVIEWERINDEXCHANGED :\n >>> \nMethod = {item . Method . ToString ( )}" );
					cnt++;
				}
			}
			dglist2 = EventControl.GetEventCount9 ( );
			if ( dglist2 != null )
			{
				int cnt = 0;
				if ( !first )
				{
					Debug . WriteLine ( $"=====================================================================================" ); first = false;
				}
				Debug . WriteLine ( $"=====================================================================================" );
				first = true;
				foreach ( var item in dglist2 )
				{
					if ( cnt > 0 ) Debug . WriteLine ("");
					if ( item . Target != null )
						Debug . WriteLine ( $"Event : RECORDDELETED:\n >>> {item . Target?.ToString ( )}\nMethod = {item . Method . ToString ( )}" );
					else
						Debug . WriteLine ( $"Event : RECORDDELETED:\n >>> \nMethod = {item . Method . ToString ( )}" );
					cnt++;
				}
			}

			Debug . WriteLine ( $"+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++" );
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
