using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;


namespace WPFPages
{
	/// <summary>
	/// AllStaff Class 
	/// 
	/// This lives in the /Views Folder
	/// 
	/// An ObservableCollection class Wrapper to provide access to various view that provide
	///  a collection of /ViewModels/OurStaff Objects to Views that require it.
	/// </summary>
	public class AllStaff : ObservableCollection<OurStaff>
	{
		public AllStaff() {
			//NB This is also how to load relevant images on the fly.....
			Add(new OurStaff {
				Forename = "ian",
				Surname = "Turner",
				Age = 77,
				MonthsEmployed = 120,
				Salary = 35000L,
				UserPic = new BitmapImage(new Uri("/images/ian.jpg", UriKind.Relative))
			});
			Add(new OurStaff {
				Forename = "Olwen",
				Surname = "Turner",
				Age = 72,
				MonthsEmployed = 960,
				Salary = 31000L,
				UserPic = new BitmapImage(new Uri("/images/olwen.jpg", UriKind.Relative))
			});
			Add(new OurStaff {
				Forename = "John",
				Surname = "Wilson",
				Age = 67,
				MonthsEmployed = 11,
				Salary = 63500L,
				UserPic = new BitmapImage(new Uri("/images/ian.jpg", UriKind.Relative))
			});

		}
	}
}