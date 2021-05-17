using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace WPFPages
{
     public class OurStaff :INotifyPropertyChanged
     {
          private string forename;
          private string surname;
          private double monthsemployed;
          private int age;
          private double salary;
          private string fullname;

          public event PropertyChangedEventHandler PropertyChanged;
          public string Forename{
			get { return forename; }
			set {  forename = value;
                    fullname = forename + " " + surname;
                    OnPropertyChanged("Forename"); OnPropertyChanged("Fullname"); }
          }

          public string Surname {
               get { return surname; }
               set { surname = value;
                    fullname = forename + " " + surname;
                    OnPropertyChanged("Surname"); OnPropertyChanged("Fullname"); }
          }
          public double MonthsEmployed {
               get { return monthsemployed; }
               set {
                    monthsemployed = value; OnPropertyChanged("MonthsEmployed");
               }
          }
          public int Age {
               get { return age; }
               set { age = value; OnPropertyChanged("Age"); }
          }
          public double Salary {
               get {return salary; } 
               set { salary = value; OnPropertyChanged("Salary");
               }
          }
          public string Fullname {
			get { Console.WriteLine($"Fullname requested"); return (string)forename + " " + surname;  }
               set { fullname = forename + " " + surname; Console.WriteLine($"Fullname reset {fullname}");  OnPropertyChanged("Fullname"); }
          }
          public BitmapImage UserPic {get; set;}

		private void OnPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

	}
}
