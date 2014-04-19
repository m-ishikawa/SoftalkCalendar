using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Util.Store;
using Google.Apis.Calendar.v3.Data;
using System.Diagnostics;
using System.Linq;

class CalendarEvent
{
	class EventData
	{
		public CalendarListEntry CalendarListEntry { get; private set; }
		public Event Event { get; private set; }
		public bool Start { get; private set; }
		public EventData(CalendarListEntry calendarListEntry, Event evt, bool start)
		{
			CalendarListEntry = calendarListEntry;
			Event = evt;
			Start = start;
		}
	}

	DateTime lastUpdateDate;
	Settings _settings;

	public CalendarEvent(Settings settings)
	{
		lastUpdateDate = DateTime.Now;
		_settings = settings;
	}


	/// <summary>
	/// カレンダーに設定されているイベントの開始時間と終了時間に喋らせる
	/// </summary>
	/// <param name="calendars"></param>
	public void EventUpdate(IEnumerable<GoogleCalendarAPI.CalendarData> calendars)
	{
		var now = DateTime.Now;
		List<EventData> events = new List<EventData>();
		var message = "";

		foreach (var cal in calendars)
		{
			if (cal.Events == null || cal.Events.Count == 0)
				continue;

			foreach (var item in cal.Events)
			{
				if (!item.Start.DateTime.HasValue)
					continue;


				// 開始時間の判定
				var dt = item.Start.DateTime.Value;
				if (dt > lastUpdateDate && dt <= now)
				{
					// 朝会特殊イベントチェック
					if (string.IsNullOrEmpty(item.Id))
					{
						message += SayMorningMeeting(calendars, item.Summary, now);
						continue;
					}
					else
					{
						events.Add(new EventData(cal.CalendarListEntry, item, true));
					}
				}

				// 終了時間の判定
				if (item.End != null)
				{
					dt = item.End.DateTime.Value;
					if (dt > lastUpdateDate && dt <= now)
					{
						events.Add(new EventData(cal.CalendarListEntry, item, false));
					}
				}
			}
		}
		lastUpdateDate = now;

		message += SayEvents(events, now);

		if (!string.IsNullOrEmpty(message))
		{
			Console.WriteLine("message=" + message);

			// softalkを呼び出す
			var softalk = _settings.SoftalkPath;
			Process.Start(softalk, " /W:" + message);
			//Process.Start(softalk, " /close");
		}
	}

	/// <summary>
	/// 指定の時間になった時のセリフ作成
	/// </summary>
	/// <param name="events">イベント</param>
	/// <param name="now">セリフを言う時の時間</param>
	/// <returns></returns>
	string SayEvents(IEnumerable<EventData> events, DateTime now)
	{
		if (events == null || events.Count() == 0)
			return "";
			
		var message = "";
		message += now.Hour + "時" + now.Minute + "分になったよ。";

		foreach (var item in events)
		{
			var d = item.Event;

			message += item.CalendarListEntry.Summary + "。";

			if (item.Start)
			{
				message += (d.Summary ?? "無題の予定") + " 開始だよ。";
				if (d.End.DateTime.HasValue)
				{
					var endDt = d.End.DateTime.Value;
					message += "終わるのは、"  + endDt.Hour + "時" + endDt.Minute + "分だよ。";
				}
			}
			else
			{
				message += d.Summary + " 終わりだよ。";
			}


			if (!string.IsNullOrEmpty(d.Location))
				message += "場所は、" + d.Location + "だよ。";
		}

		return message;
	}

	/// <summary>
	/// 朝会で今日のスケジュールをしゃべる
	/// </summary>
	/// <param name="calendars"></param>
	/// <param name="mes"></param>
	/// <param name="now"></param>
	/// <returns></returns>
	string SayMorningMeeting(IEnumerable<GoogleCalendarAPI.CalendarData> calendars, string mes, DateTime now)
	{
		var message = mes;

		foreach (var d in calendars)
		{
			if (d.Events == null || d.Events.Count == 0)
				continue;

			message += d.CalendarListEntry.Summary + "。";

			for (int i = 0; i < d.Events.Count; i++)
			{
				var item = d.Events[i];
				if (string.IsNullOrEmpty(item.Id))
					continue;

				if (item.Start == null || !item.Start.DateTime.HasValue)
				{
					// 終日のイベント
					message += "終日、";
				}
				else
				{
					// 開始時間の判定
					var dt = item.Start.DateTime.Value;
					message += dt.Hour + "時" + dt.Hour + "分から、";

					// 終了時間の判定
					if (item.End != null)
					{
						dt = item.End.DateTime.Value;
						message += dt.Hour + "時" + dt.Hour + "分まで、";
					}
				}
				message += (item.Summary ?? "無題の予定") + "。";
			}
		}

		if (string.IsNullOrEmpty(message))
		{
			message += "なにもないよ。";
		}
		else
		{
			message += "以上だよ。";
		}
		message += "ゆっくりしていってね！";

		return message;
	}


}

