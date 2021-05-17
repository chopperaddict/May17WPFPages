using System.ComponentModel;
using System.Diagnostics;


namespace WPFPages
{
	public class ViewModelBase
	{
                // In ViewModelBase.cs 
                public event PropertyChangedEventHandler PropertyChanged;
                protected virtual void OnPropertyChanged (string propertyName)
                {
                        this.VerifyPropertyName (propertyName);
                        PropertyChangedEventHandler handler = this.PropertyChanged;
                        if (handler != null)
                        {
                                var e = new PropertyChangedEventArgs (propertyName);
                                handler (this, e);
                        }
                }
                [Conditional ("DEBUG")]
                [DebuggerStepThrough]
                public void VerifyPropertyName (string propertyName)
                {
                        // Verify that the property name matches a real, 
                        // public, instance property on this object. 
                        if (TypeDescriptor.GetProperties (this)[propertyName] == null)
                        {
                                string msg = "Invalid property name: " + propertyName;
#pragma MVVM TODO
                                //if (this.ThrowOnInvalidPropertyName)
                                //        throw new Exception (msg);
                                //else
                                Debug.Fail (msg);
                        }
                }
        }
}
