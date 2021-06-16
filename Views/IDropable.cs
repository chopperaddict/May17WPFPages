using System;

namespace WPFPages . Views
{
	internal interface IDropable
	{
		Type DataType { get; }

		void Drop ( object data, string strdata = "");
	}
}