using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WpfUI;

namespace WPFPages.ViewModels
{
	public class MainViewModel : ObservableCollection<Person>
     {
          public event PropertyChangedEventHandler PropertyChanged;

          public string NameToDisplay { get; set; }
          public List<Person> ListOfPersons { get; set; }
          public ObservableCollection<Person> People = new ObservableCollection<Person>();

          public MainViewModel()
          {
               NameToDisplay = "Hello Ian & Olwen";
               //Assign the List of Persons to a public List<Person> named ListOfPersons
               ListOfPersons = GetListOfPersons();
       
          }

          private List<Person> GetListOfPersons()
          {
               Person fabianPerson = GetPerson("Fabian", 29);
               Person evePerson = GetPerson("Eve", 100);
               Person jPerson = GetPerson("John", 60);
               Person kPerson = GetPerson("Kinder", 88);
               Person yPerson = GetPerson("Yvonne", 23);
               People.Add(fabianPerson);
               People.Add(evePerson);
               People.Add(jPerson);
               People.Add(kPerson);
               People.Add(yPerson);
               return new List<Person> { fabianPerson, evePerson,jPerson, kPerson,yPerson};
          }

          private Person GetPerson(string name, int age)
          {
               return new Person();
          }
          private void OnPropertyChanged(string propertyName)
          {
               PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
          }
          private void RaisePropertyChanged(string propName)
          {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}
     }
}
