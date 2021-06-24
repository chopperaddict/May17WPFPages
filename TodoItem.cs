namespace WPFPages.Views
{
	public class TodoItem
	{
		private string title;
		private int completion;

		public string Title {
			get { return title; }
			set { title = value; }
		}

		public int Completion {
			get { return completion; }
			set { completion = value; }
		}

	}
}
