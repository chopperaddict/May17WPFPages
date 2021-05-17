using System.ComponentModel;
using System.Windows.Media.Imaging;

using static System.Net.Mime.MediaTypeNames;

namespace WpfUI
{
	public class Person :INotifyPropertyChanged
	{
		/// <summary>
		/// Person Class 
		/// 
		/// This lives in /ViewModels
		/// 
		/// is a Standard style of backing Class for the People ObservableCollection, which
		/// has several Data Properties including images used mostly for testing Binding
		/// and is getting more complex over time...
		/// </summary>

		public enum SexType
		{
			Male,
			Female
		}
		public event PropertyChangedEventHandler PropertyChanged;

		private string _employeeFirstName;
		private string _employeeLastName;
		private string _fullName;
		private int _employeeId;
		private double _servicePeriod;
		private SexType _sex;
		private BitmapImage _userPic;

		public string EmployeeFirstName {
			get { return _employeeFirstName; }
			set { _employeeFirstName = value; _fullName = _employeeFirstName + " " + _employeeLastName; OnPropertyChanged("EmployeeFirstName"); OnPropertyChanged("FullName"); }
		}
		public string EmployeeLastName { 
			get { return _employeeLastName; }
			set { _employeeLastName = value; _fullName = _employeeFirstName + " " + _employeeLastName; OnPropertyChanged("FullName"); OnPropertyChanged("EmployeeLastName"); }
		}
		public string FullName { 
			get { return _fullName;  }
			set { _fullName = value; OnPropertyChanged("FullName"); }
		}
		public int EmployeeId {
			get { return _employeeId; }
			set { _employeeId = value; OnPropertyChanged("EmployeeId"); }
		}
		public double ServicePeriod { 
			get { return _servicePeriod; }
			set { _servicePeriod = value; OnPropertyChanged("ServicePeriod"); }
		}
		public SexType Sex {
			get { return _sex; }
			set { _sex= value; OnPropertyChanged("Sex"); }
		}
		public BitmapImage UserPic { 
			get { return _userPic; }
			set { _userPic = value; OnPropertyChanged("UserPic");}
		}


private void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

	}
}