using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFPages.Views
{
	/// <summary>
	/// Create a new Observable object of the TodoItems Class
	/// to be used in Page 5
	/// </summary>
	class ItemsCollection : ObservableCollection<TodoItem>
	{
		public ItemsCollection() {
			Add(new TodoItem() { Title = "Complete this WPF tutorial", Completion = 45 });
			Add(new TodoItem() { Title = "Learn C#", Completion = 80 });
			Add(new TodoItem() { Title = "Wash the car", Completion = 0 });
		}
	}
}
