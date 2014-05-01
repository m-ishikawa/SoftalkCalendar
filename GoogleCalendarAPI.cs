using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;

class GoogleCalendarAPI
{
	CalendarService _service;

	public class CalendarData
	{
		public CalendarListEntry CalendarListEntry { get; private set; }
		public IList<Event> Events { get; private set; }

		public CalendarData(CalendarListEntry calendarListEntry, IList<Event> events)
		{
			this.CalendarListEntry = calendarListEntry;
			this.Events = events;
		}
	}

	public GoogleCalendarAPI(string appName, UserCredential credential)
	{
		// Create the calendar service using an initializer instance
		BaseClientService.Initializer initializer = new BaseClientService.Initializer();
		initializer.HttpClientInitializer = credential;
		initializer.ApplicationName = appName;
		_service = new CalendarService(initializer);
	}


	/// <summary>
	/// 今日のカレンダーを取得する
	/// </summary>
	/// <returns></returns>
	public IEnumerable<CalendarData> Fetch()
	{
		// Fetch the list of calendar list
		try
		{
			var items = _service.CalendarList.List().Execute().Items;
			List<CalendarData> calList = new List<CalendarData>();

			foreach (var item in items)
			{
				var requeust = _service.Events.List(item.Id);
				// Set MaxResults and TimeMin with sample values
				requeust.MaxResults = 100;
				var timeSpan = new TimeSpan(0, 0, 0);	// 一日を朝４時始まりにする
				var today = new DateTime(DateTime.Today.Ticks, DateTimeKind.Utc);	// UTCに変更しないとズレる
				requeust.TimeMin = today + timeSpan;
				requeust.TimeMax = today.AddDays(1) + timeSpan;

				// Fetch the list of events
				var events = requeust.Execute().Items.ToArray();

				calList.Add(new CalendarData(item, events));
			}
			return calList;
		}
		catch (Exception e)
		{
			return null;
		}
	}

}

