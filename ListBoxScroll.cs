﻿using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Controls;

namespace WPFPages
{
	public class ListBoxScroll : ListBox
	{
		public ListBoxScroll ( ) : base ( )
		{
			SelectionChanged += new SelectionChangedEventHandler ( ListBoxScroll_SelectionChanged );
		}

		void ListBoxScroll_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			ScrollIntoView ( SelectedItem );
		}
	}
}
