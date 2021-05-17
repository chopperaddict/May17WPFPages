using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;

namespace WPFPages . Views
{
	public class ViewEditdb : Observable
	{

		EditDb Editdb;
		SqlDbViewer SqlDbviewer;
		string CurrentDb;
		// SelectedItem
		object Item;
		// SelectedIndex
		int Index;
		public ViewEditdb ( string currentDb, int index, object item, SqlDbViewer sqldb )
		{
			CurrentDb = currentDb;
			SqlDbviewer = sqldb;
			Item = item;
			Index = index;

			// open a new instance of our EditDb Window
			Editdb = new EditDb ( currentDb, 0,  item, sqldb );
			Editdb . Show ( );
		}
	}
}
