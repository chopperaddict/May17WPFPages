using System;
using System . Collections . Generic;
using System . Linq;
using System . Text;
using System . Threading . Tasks;

namespace WPFPages . ViewModels
{
	public class LinqSupport
	{
		List<object> storedProcedures;
		private object ProcedureStore { get; set; }
		public List<object> GetStoredMethods ( )
		{
			return storedProcedures;
		}
	}
}
