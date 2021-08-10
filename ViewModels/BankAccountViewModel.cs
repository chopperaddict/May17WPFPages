// if set, Datatable is cleared and reloaded, otherwise it is not reloaded
//#define PERSISTENTDATA
#define USETASK
#undef USETASK

using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . ComponentModel;
using System . Diagnostics;
using System . Windows . Controls;

using WPFPages . Views;

/// <summary>
///  this is a mirror image of the original BankAccount.cs file
/// </summary>
namespace WPFPages . ViewModels
{
	public partial class BankAccountViewModel : INotifyPropertyChanged
	{
		#region PropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged ( string propertyName )
		{
			if ( Flags . SqlBankActive == false )
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

		#region CONSTRUCTORS

		// CONSTRUCTOR
		//**************************************************************************************************************************************************************//

		public BankAccountViewModel ( )
		{
		}
		#endregion CONSTRUCTORS

		public static DataGrid ActiveEditDbViewer = null;

		#region STANDARD CLASS PROPERTIES SETUP

		private int id;
		private string bankno;
		private string custno;
		private int actype;
		private decimal balance;
		private decimal intrate;
		private DateTime odate;
		private DateTime cdate;

		public int Id
		{
			get
			{
				return id;
			}
			set
			{
				id = value;
				OnPropertyChanged ( Id . ToString ( ) );
			}
		}

		public string BankNo
		{
			get
			{
				return bankno;
			}
			set
			{
				bankno = value;
				OnPropertyChanged ( BankNo );
			}
		}

		public string CustNo
		{
			get
			{
				return custno;
			}
			set
			{
				custno = value;
				OnPropertyChanged ( CustNo );
			}
		}

		public int AcType
		{
			get
			{
				return actype;
			}

			set
			{
				actype = value;
				OnPropertyChanged ( AcType . ToString ( ) );
			}
		}

		public decimal Balance
		{
			get
			{
				return balance;
			}

			set
			{
				balance = value;
				OnPropertyChanged ( Balance . ToString ( ) );
			}
		}

		public decimal IntRate
		{
			get
			{
				return intrate;
			}
			set
			{
				intrate = value;
				OnPropertyChanged ( IntRate . ToString ( ) );
			}
		}

		public DateTime ODate
		{
			get
			{
				return odate;
			}
			set
			{
				odate = value;
				OnPropertyChanged ( ODate . ToString ( ) );
			}
		}

		public DateTime CDate
		{
			get
			{
				return cdate;
			}
			set
			{
				cdate = value;
				OnPropertyChanged ( CDate . ToString ( ) );
			}
		}

		public string ToString ( bool full = false )
		{
			//			if ( full )
			//				return CustNo + ", " + BankNo + ", " + AcType + ", " + IntRate + ", " + Balance + ", " + ODate + ", " + CDate;
			//			else
			return base . ToString ( );
		}
		public override string ToString ( )
		{
			return base . ToString ( );
		}
		#endregion STANDARD CLASS PROPERTIES SETUP

		#region SETUP/DECLARATIONS



		#endregion SETUP/DECLARATIONS
	}
}

/*
 *
 #if USETASK
			{
				int? taskid = Task.CurrentId;
				DateTime start = DateTime.Now;
				Task<bool> DataLoader = FillBankAccountDataGrid ();
				DataLoader.ContinueWith
				(
					task =>
					{
						LoadBankAccountIntoList (dtBank);
					},
					TaskScheduler.FromCurrentSynchronizationContext ()
				);
				Console.WriteLine ($"Completed AWAITED task to load BankAccount  Data via Sql\n" +
					$"task =Id is [ {taskid}], Completed status  [{DataLoader.IsCompleted}] in {(DateTime.Now - start)} Ticks\n");
			}
#else
			{
* */
