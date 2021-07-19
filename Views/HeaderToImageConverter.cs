using System;
using System . Collections . Generic;
using System . Globalization;
using System . Linq;
using System . Text;
using System . Threading . Tasks;
using System . Windows . Data;
using System . Windows . Media . Imaging;

namespace WPFPages . Views
{
	[ValueConversion ( typeof ( string ), typeof ( bool ) )]
	public class HeaderToImageConverter : IValueConverter
	{
                public static HeaderToImageConverter Instance =
                         new HeaderToImageConverter ( );

                public object Convert ( object value, Type targetType,
                    object parameter, CultureInfo culture )
                {
                        if ( ( value as string ) . Contains ( @"\" ) )
                        {
                                //Drive
                                Uri uri = new Uri
                                ( "pack://application:,,,/Icons/folder1.png" );
                                BitmapImage source = new BitmapImage ( uri );
                                return source;
                        }
                        else if ( ( value as string ) . Contains ( "." ) )
                        {
                                // Sub folder
                                Uri uri = new Uri
                                ( "pack://application:,,/Icons/rss.png" );
                                BitmapImage source = new BitmapImage ( uri );
                                return source;
                        }
                        else
                        {
                                //File alone
                                Uri uri = new Uri ( "pack://application:,,,/Icons/mailbox.png" );
                                BitmapImage source = new BitmapImage ( uri );
                                return source;
                        }
                }

                public object ConvertBack ( object value, Type targetType,
                    object parameter, CultureInfo culture )
                {
                        throw new NotSupportedException ( "Cannot convert back" );
                }
        }
}
