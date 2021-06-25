#define TASK1


namespace WPFPages.ViewModels
{
	public partial class DetailsViewModel// : Observable
	{
		#region CONSTRUCTORS

		public  class Internalclass
		{
			public int Age { get; set; }
			public int Rating { get; set; }
			public string Hobby { get; set; }
		}
		#endregion PropertyChanged


	}
}


//**************************************************************************************************************************************************************//

/*
 *
#if USETASK
{
			try
			{
			// THIS ALL WORKS PERFECTLY - THANKS TO VIDEO BY JEREMY CLARKE OF JEREMYBYTES YOUTUBE CHANNEL
				int? taskid = Task.CurrentId;
				Task<DataTable> DataLoader = LoadSqlData ();
				DataLoader.ContinueWith
				(
					task =>
					{
						LoadDetailsObsCollection();
					},
					TaskScheduler.FromCurrentSynchronizationContext ()
				);
				Console.WriteLine ($"Completed AWAITED task to load Details Data via Sql\n" +
					$"task =Id is [{taskid}], Completed status  [{DataLoader.IsCompleted}] in {(DateTime.Now - start).Ticks} ticks]\n");
			}
			catch (Exception ex)
			{ Console.WriteLine ($"Task error {ex.Data},\n{ex.Message}"); }
			Mouse.OverrideCursor = Cursors.Arrow;
			// WE NOW HAVE OUR DATA HERE - fully loaded into Obs >?
}
#else * */
