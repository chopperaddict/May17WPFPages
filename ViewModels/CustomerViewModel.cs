//#define PERSISTENTDATA
using System;
using System . ComponentModel;
using System . Data;
using System . Diagnostics;
using System . Windows . Controls;

using WPFPages . ViewModels;

namespace WPFPages . Views
{
	//===========================
	//CUSTOMER VIEW MODEL CLASS
	//===========================
	public partial class CustomerViewModel :INotifyPropertyChanged
	{
		#region CONSTRUCTORS

		//==================
		// BASIC CONSTRUCTOR
		//==================
		public CustomerViewModel ( )
		{
		}

		#endregion CONSTRUCTORS
		#region PropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private new void OnPropertyChanged ( string propertyName )
		{
			//PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( propertyName ) );
			if ( Flags . SqlCustActive == false )
//				this . VerifyPropertyName ( propertyName );

			if ( this . PropertyChanged != null )
			{
				var e = new PropertyChangedEventArgs ( propertyName );
				this . PropertyChanged ( this, e );
			}
		}
		/// <summary>
		/// Warns the developer if this object does not have
		/// a public property with the specified name. This
		/// method does not exist in a Release build.
		/// </summary>
		[Conditional ( "DEBUG" )]
		[DebuggerStepThrough]
		public virtual void VerifyPropertyName ( string propertyName )
		{
			// Verify that the property name matches a real,
			// public, instance property on this object.
			if ( TypeDescriptor . GetProperties ( this ) [ propertyName ] == null )
			{
				string msg = "Invalid property name: " + propertyName;

				if ( this . ThrowOnInvalidPropertyName )
					throw new Exception ( msg );
				else
					Debug . Fail ( msg );
			}
		}

		/// <summary>
		/// Returns whether an exception is thrown, or if a Debug.Fail() is used
		/// when an invalid property name is passed to the VerifyPropertyName method.
		/// The default value is false, but subclasses used by unit tests might
		/// override this property's getter to return true.
		/// </summary>
		protected virtual bool ThrowOnInvalidPropertyName
		{
			get; private set;
		}

		#endregion PropertyChanged


		#region PRIVATE Variables declarations

		private int id;
		private string custno;
		private string bankno;
		private int actype;
		private string fname;
		private string lname;
		private string addr1;
		private string addr2;
		private string town;
		private string county;
		private string pcode;
		private string phone;
		private string mobile;
		private DateTime dob;
		private DateTime odate;
		private DateTime cdate;

		private static bool loaded = false;

		//public bool FilterResult = false;
		//public bool isMultiMode = false;

		#endregion PRIVATE Variables declarations

		#region PROPERTY SETTERS

		public int Id
		{
			get { return id; }
			set { id = value; OnPropertyChanged ( Id . ToString ( ) ); }
		}

		public string CustNo
		{
			get { return custno; }
			set { custno = value; OnPropertyChanged ( CustNo . ToString ( ) ); }
		}

		public string BankNo
		{
			get { return bankno; }
			set { bankno = value; OnPropertyChanged ( BankNo . ToString ( ) ); }
		}

		public int AcType
		{
			get { return actype; }
			set { actype = value; OnPropertyChanged ( AcType . ToString ( ) ); }
		}

		public string FName
		{
			get { return fname; }
			set { fname = value; OnPropertyChanged ( FName . ToString ( ) ); }
		}

		public string LName
		{
			get { return lname; }
			set { lname = value; OnPropertyChanged ( LName . ToString ( ) ); }
		}

		public string Addr1
		{
			get { return addr1; }
			set { addr1 = value; OnPropertyChanged ( Addr1 . ToString ( ) ); }
		}

		public string Addr2
		{
			get { return addr2; }
			set { addr2 = value; OnPropertyChanged ( Addr2 . ToString ( ) ); }
		}

		public string Town
		{
			get { return town; }
			set { town = value; OnPropertyChanged ( Town . ToString ( ) ); }
		}

		public string County
		{
			get { return county; }
			set { county = value; OnPropertyChanged ( County . ToString ( ) ); }
		}

		public string PCode
		{
			get { return pcode; }
			set { pcode = value; OnPropertyChanged ( PCode . ToString ( ) ); }
		}

		public string Phone
		{
			get { return phone; }
			set { phone = value; OnPropertyChanged ( Phone . ToString ( ) ); }
		}

		public string Mobile
		{
			get { return mobile; }
			set { mobile = value; OnPropertyChanged ( Mobile . ToString ( ) ); }
		}

		public DateTime Dob
		{
			get { return dob; }
			set { dob = value; OnPropertyChanged ( Dob . ToString ( ) ); }
		}

		public DateTime ODate
		{
			get { return odate; }
			set { odate = value; OnPropertyChanged ( ODate . ToString ( ) ); }
		}

		public DateTime CDate
		{
			get { return cdate; }
			set { cdate = value; OnPropertyChanged ( CDate . ToString ( ) ); }
		}

		#endregion PROPERTY SETTERS

		#region PUBLIC & STATIC DECLARATIONS

		public static BankAccountViewModel bvm = MainWindow . bvm;
		public static CustomerViewModel cvm = MainWindow . cvm;
		public static DetailsViewModel dvm = MainWindow . dvm;

		#endregion PUBLIC & STATIC DECLARATIONS

	}
}
