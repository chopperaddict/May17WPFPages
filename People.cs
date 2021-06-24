using System;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

using static WpfUI.Person;

namespace WpfUI
{

	/// <summary>
	/// People Class 
	/// 
	/// This lives in the /Views Folder
	/// 
	/// An ObservableCollection class Wrapper to provide access to various view that provide
	///  a collection of /ViewModels/Person Objects to Views that require it.
	/// </summary>
	public class People : ObservableCollection<Person>
	{
		public SexType Sex;

		public People()
		{
			//NB This is also how to load relevant images on the fly.....
			Add(new Person { Sex = SexType.Male, EmployeeId = 1, EmployeeFirstName = "Ronald", EmployeeLastName = "Jones", ServicePeriod=27,
				UserPic = new BitmapImage(new Uri("/images/ian.jpg", UriKind.Relative)) });
			Add(new Person { Sex=SexType.Male, EmployeeId = 2, EmployeeFirstName = "Jimmiey", EmployeeLastName = "Smith", ServicePeriod=15,
				UserPic = new BitmapImage(new Uri("/images/ian.jpg", UriKind.Relative)) });
			Add(new Person { Sex=SexType.Female, EmployeeId = 3, EmployeeFirstName = "Jane", EmployeeLastName = "Turner",ServicePeriod = 79,
				UserPic = new BitmapImage(new Uri("/images/olwen.jpg", UriKind.Relative)) });
			Add(new Person { Sex=SexType.Male, EmployeeId = 44, EmployeeFirstName = "Prunto", EmployeeLastName = "Wilson",ServicePeriod = 420,
				UserPic = new BitmapImage(new Uri("/images/ian.jpg", UriKind.Relative)) });
			Add(new Person { Sex=SexType.Female, EmployeeId = 5, EmployeeFirstName = "Susan", EmployeeLastName = "McConnell",ServicePeriod = 11,
				UserPic = new BitmapImage(new Uri("/images/olwen.jpg", UriKind.Relative)) });
			Add(new Person { Sex=SexType.Male, EmployeeId = 6, EmployeeFirstName = "Rowe", EmployeeLastName = "Baker",ServicePeriod = 22,
				UserPic = new BitmapImage(new Uri("/images/ian.jpg", UriKind.Relative)) });
			Add(new Person { Sex=SexType.Male, EmployeeId = 7, EmployeeFirstName = "Jackson", EmployeeLastName = "Smythe",ServicePeriod = 45.53,
				UserPic = new BitmapImage(new Uri("/images/ian.jpg", UriKind.Relative)) });
			Add(new Person { Sex=SexType.Male, EmployeeId = 8, EmployeeFirstName = "Jamien", EmployeeLastName = "Cuthbertson", ServicePeriod = 35,
				UserPic = new BitmapImage(new Uri("/images/ian.jpg", UriKind.Relative)) });
			Add(new Person { Sex=SexType.Male, EmployeeId = 9, EmployeeFirstName = "Proctor", EmployeeLastName = "Williams",ServicePeriod = 12,
				UserPic = new BitmapImage(new Uri("/images/ian.jpg", UriKind.Relative)) });
			Add(new Person { Sex = SexType.Female, EmployeeId = 10, EmployeeFirstName = "Olwen", EmployeeLastName = "McFarlane",ServicePeriod = 5,
				UserPic = new BitmapImage(new Uri("/images/olwen.jpg", UriKind.Relative)) });
			Add(new Person {Sex = SexType.Female,EmployeeId = 10,EmployeeFirstName = "Janine",EmployeeLastName = "Curtiss",ServicePeriod = 73,
				UserPic = new BitmapImage(new Uri("/images/olwen.jpg", UriKind.Relative))});
			Add(new Person {	Sex = SexType.Female,EmployeeId = 10,EmployeeFirstName = "Joanne",EmployeeLastName = "Simpson",ServicePeriod = 1,
				UserPic = new BitmapImage(new Uri("/images/olwen.jpg", UriKind.Relative))});
		}


	}
}