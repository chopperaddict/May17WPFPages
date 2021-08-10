#undef USEIMAGE
using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . ComponentModel;
using System . Data . SqlClient;
using System . Data;
using System . Diagnostics;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Controls;

using System . Windows;
using System . IO;

namespace WPFPages . Views
{
	public class NwCatCollection : ObservableCollection<nwcategory>
	{
		public NwCatCollection ( )
		{
		}
		public NwCatCollection ( int categoryId )
		{
			LoadCatDetails ( categoryId );
		}
		public NwCatCollection LoadCatDetails ( int categoryId = -1 )
		{
			//if ( grid == null )
			//	return null;
			DataTable dt = new DataTable ( "Categories" );
			string ConString = ( string ) Properties . Settings . Default [ "NorthwindConnectionString" ];
			//int Ordervalue = Convert . ToInt32 ( orderId );
			string CmdString = string . Empty;
			using ( SqlConnection con = new SqlConnection ( ConString ) )
			{
				if ( categoryId != -1 )
					CmdString = $"SELECT *  FROM [Categories] where CategoryId = {categoryId}";
				else
					CmdString = $"SELECT *  FROM [Categories]";
				SqlCommand cmd = new SqlCommand ( CmdString, con );
				SqlDataAdapter sda = new SqlDataAdapter ( cmd );
				try
				{
					sda . Fill ( dt );
				}
				catch ( Exception ex )
				{
					Debug . WriteLine ( $"Data={ex . Data}, {ex . Message}\n[{CmdString}]" );
				}
			}
#if USEIMAGE
			SqlConnection sqlConnection1 = new SqlConnection ( ConString );
			try
			{
				List<Image> images = new List<Image> ( );
				SqlCommand cmdSelect = new SqlCommand ( "select Picture from Categories where Picture=@Picture", sqlConnection1 );
				cmdSelect . Parameters . Add ( "@Picture", SqlDbType . Int, 4 );
				cmdSelect . Parameters [ "@Picture" ] . Value = "image.bmp";

				sqlConnection1 . Open ( );
				byte [ ] barrImg = ( byte [ ] ) cmdSelect . ExecuteScalar ( );
				string strfn = Convert . ToString ( DateTime . Now . ToFileTime ( ) );
				FileStream fs = new FileStream ( strfn,
						  FileMode . CreateNew, FileAccess . Write );
				fs . Write ( barrImg, 0, barrImg . Length );
				fs . Flush ( );
				fs . Close ( );
				//images.Add( strfn ));
			}
			catch ( Exception ex )
			{
				MessageBox . Show ( ex . Message );
			}
			finally
			{
				sqlConnection1 . Close ( );
			}
#endif


			return CreateCatCollection ( dt );
		}
		public NwCatCollection CreateCatCollection ( DataTable dt )
		{
			int count = 0;
			try
			{
				for ( int i = 0 ; i < dt . Rows . Count ; i++ )
				{

					this . Add ( new nwcategory
					{
						CategoryId = Convert . ToInt32 ( dt . Rows [ i ] [ 0 ] ),
						CategoryName = dt . Rows [ i ] [ 1 ] . ToString ( ),
						Description = dt . Rows [ i ] [ 2 ] . ToString ( ),
						Picture = dt . Rows [ i ] [ 3 ] as ImageClass
					} );
					count = i;
					//					Picture = Convert . To ( dt . Rows [ i ] [ 2 ] ),

				}
			}
			catch ( Exception ex )
			{
				Debug . WriteLine ( $"NWCATEGORIES: ERROR in  CreateCatCollection() : loading data into ObservableCollection : [{ex . Message}] : {ex . Data} ...." );
				MessageBox . Show ( $"NWCATEGORIES: ERROR in  CreateCatCollection() : loading data into ObservableCollection : [{ex . Message}] : {ex . Data} ...." );
				return null;
			}
			finally
			{
			}
			return this;
		}

	}
	public class nwcategory : System . ComponentModel.INotifyPropertyChanged
	{
		public nwcategory ( )
		{
		}
		#region Declarations
		private int categoryId;
		private string categoryName;
		private string description;
		private ImageClass picture;
		private int currentSelection;
		public event PropertyChangedEventHandler PropertyChanged;

		#region PropertyChanged

		private void OnPropertyChanged ( string propertyName )
		{
			//PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( propertyName ) );
//			this . VerifyPropertyName ( propertyName );

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

		public int CurrentSelection
		{
			get
			{
				return currentSelection;
			}
			set
			{
				currentSelection = value;
				OnPropertyChanged ( "CurrentSelection" );
			}
		}
		public int CategoryId
		{
			get
			{
				return categoryId;
			}
			set
			{
				categoryId = value;
				OnPropertyChanged ( "CategoryId" );
			}
		}
		public string CategoryName
		{
			get
			{
				return categoryName;
			}
			set
			{
				categoryName = value;
				OnPropertyChanged ( "CategoryName" );
			}
		}
		public string Description
		{
			get
			{
				return description;
			}
			set
			{
				description = value;
				OnPropertyChanged ( "Description" );
			}
		}
		public ImageClass Picture
		{
			get
			{
				return picture;
			}
			set
			{
				picture = value;
				OnPropertyChanged ( "Picture" );
			}
		}
		#endregion Declarations
	}
	public class ImageClass
	{
		public int Id
		{
			get; set;
		}
		public string ImagePath
		{
			get; set;
		}
		public byte [ ] ImageInBytes
		{
			get; set;
		}
	}
}
