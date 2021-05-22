using System;
using System . Collections . Generic;
using System . IO;
using System . Linq;
using System . Text;
using System . Threading . Tasks;

namespace WPFPages . Views
{
	public class FileHandling
	{
		public static string ReadFile ( string file )
		{
			string Output = "";
			FileInfo fi = new FileInfo ( file );
			FileStream fs = fi . Open ( FileMode . OpenOrCreate, FileAccess . Read, FileShare . Read );
			StreamReader sr = new StreamReader ( fs );
			Output = sr . ReadToEnd ( );
			sr . Close ( );
			fs . Close ( );
			return Output;
		}
		public static bool WriteFile ( string file )
		{
			bool result = false;
			FileInfo fi = new FileInfo ( file );
			//Open file for Read\Write
			FileStream fs = fi . Open ( FileMode . OpenOrCreate, FileAccess . Write, FileShare . Read );
			//Create StreamWriter object to write string to FileSream
			StreamWriter sw = new StreamWriter ( fs );
			sw . WriteLine ( file );
			sw . Close ( );
			result = true;
			return result;
		}
	}
}
