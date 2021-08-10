using System . ComponentModel;

namespace WPFPages
{
	public class INotifyPropertyChanged : System . ComponentModel.INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		#region INotifyProp		
		protected void OnPropertyChanged ( string PropertyName )
		{
			if ( null != PropertyChanged )
			{
				PropertyChanged ( this,
					new PropertyChangedEventArgs ( PropertyName ) );
			}
		}
		#endregion INotifyProp		

	}

}
