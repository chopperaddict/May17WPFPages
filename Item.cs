using System.ComponentModel;

namespace WPFPages
{
	public class Item : INotifyPropertyChanged
     {
          private double _value;

          public double Value {
               get { return _value; }
               set { _value = value; OnPropertyChanged("Value"); }
          }

          #region INotifyPropertyChanged Members

          public event PropertyChangedEventHandler PropertyChanged;

          #endregion

          protected void OnPropertyChanged(string PropertyName) {
               if (null != PropertyChanged) {
                    PropertyChanged(this,
                         new PropertyChangedEventArgs(PropertyName));
               }
          }
     }


}
