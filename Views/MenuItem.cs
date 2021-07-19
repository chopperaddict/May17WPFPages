using System . Collections . ObjectModel;

namespace WPFPages . Views
{
	public class MenuItem
	{
		public MenuItem ( )
		{
			this . Items = new ObservableCollection<MenuItem> ( );
		}

		public string Title
		{
			get; set;
		}

		public string Name
		{
			get; set;
		}

		public ObservableCollection<MenuItem> Items
		{
			get; set;
		}
	}
}
