using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;

/// <summary>
/// An sample for the Calendar API which displays a list of calendars and events in the first calendar.
/// https://developers.google.com/google-apps/calendar/
/// </summary>
static class Program
{
	public static void Main()
	{
		// Display the header and initialize the sample.
		Console.WriteLine("Google.Apis.Calendar.v3 Sample");
		Console.WriteLine("==============================");


		var main = new CalendarMain();
		main.Run();

		Console.WriteLine("Press any key to continue...");
		Console.ReadKey();
	}

}
