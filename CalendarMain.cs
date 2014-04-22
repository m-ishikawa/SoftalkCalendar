using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Util.Store;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Json;

class CalendarMain
{
	public const string FileDataStore = "SofTalkCalendar";		// Oauth認証のキャッシュ名（%HOMEPATH%\AppData\Roaming に出来る）
	public const string ApplicationName = "GoogleCalendarAPITest";
	public Settings Settings { get; set; }

	Thread _thread;
	GoogleCalendarAPI _googleCalendarAPI;
	
	public CalendarMain()
	{
		// アプリバージョンなど表示
		System.Diagnostics.FileVersionInfo verInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
		Console.WriteLine(verInfo.ProductName + " Ver" + verInfo.FileVersion);

		// 設定ファイルを読み込む
		using (FileStream stream = new FileStream("settings.json", FileMode.Open, FileAccess.Read))
		{
			Settings = AppSettings.Load(stream).Settings;
			Console.WriteLine("SoftalkPath=" + Settings.SoftalkPath);
		}

		// googleにサインインするための情報をファイルから読み込む
		UserCredential credential = default(UserCredential);
		using (FileStream stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
		{
			credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
				GoogleClientSecrets.Load(stream).Secrets,
				new[] { CalendarService.Scope.Calendar },
				"user",
				CancellationToken.None,
				new FileDataStore("SofTalkCalendar")
			).Result;
		}

		_googleCalendarAPI = new GoogleCalendarAPI(ApplicationName, credential);
	}

	public void Run()
	{
		_thread = new Thread(new ThreadStart(ThreadMain));
		_thread.Start();
	}

	public void Dispose()
	{
		if (_thread != null)
		{
			_thread.Abort();
			_thread = null;
		}
	}


	void ThreadMain()
	{
		var now = DateTime.Now;
		IEnumerable<GoogleCalendarAPI.CalendarData> calendars = null;
		var calEvt = new CalendarEvent(Settings);

		while (true)
		{
			var cal = FetchCalendars();
			if (cal != null)
			{
				cal = cal.Concat(new []{CreateMorningMeeting()});
				calendars = cal;
			}

			// イベントの時刻をチェックしてしゃべるよ
			if (calendars != null)
				calEvt.EventUpdate(calendars);

			Thread.Sleep(5 * 1000);		// ５秒でポーリング
		}

	}

	/// <summary>
	/// 朝会をイベントに足す
	/// </summary>
	GoogleCalendarAPI.CalendarData CreateMorningMeeting()
	{
		var e = new Event();
		e.Summary = Settings.MorningMeetingMessage;
		e.Start = new EventDateTime();
		e.Start.DateTime = DateTime.Parse(Settings.MorningMeetingTime);
		e.Id = null;	// 朝会識別用特殊ID

		return new GoogleCalendarAPI.CalendarData(null, new []{e});
	}


	DateTime? lastFetchDate;

	/// <summary>
	/// 定期的にカレンダーを更新する
	/// </summary>
	/// <returns></returns>
	public IEnumerable<GoogleCalendarAPI.CalendarData> FetchCalendars()
	{
		var now = DateTime.Now;
		IEnumerable<GoogleCalendarAPI.CalendarData> calendars = null;

		var span = new TimeSpan(0, 5, 0);	// カレンダーの更新間隔（10分）
		if (!lastFetchDate.HasValue || now > lastFetchDate + span)
		{
			//Console.WriteLine("カレンダー更新するよ 前回更新=" + lastFetchDate);
			lastFetchDate = now;
			calendars = _googleCalendarAPI.Fetch();

			// 取得したカレンダー表示
			//DisplayCalendarEvents(calendars);
		}
		return calendars;
	}

	void DisplayCalendarEvents(IEnumerable<GoogleCalendarAPI.CalendarData> calendars)
	{
		foreach (var d in calendars)
		{
			Console.WriteLine("カレンダー:" + d.CalendarListEntry.Summary);

			if (d.Events == null || d.Events.Count == 0)
			{
				Console.WriteLine(" イベント無し");
			}
			else
			{
				Console.WriteLine(" イベント一覧:");
				DisplayEvents(d.Events);
			}
			Console.WriteLine(Environment.NewLine);
		}
	}
		
	void DisplayEvents(IList<Event> events)
	{
		foreach (var calendarEvent in events)
		{
			Console.WriteLine(" " + calendarEvent.Summary);
			Console.WriteLine("  開始:" + GetDateString(calendarEvent.Start));
			Console.WriteLine("  終了:" + GetDateString(calendarEvent.End));
			Console.WriteLine("  説明:" + calendarEvent.Description);
			Console.WriteLine("  場所:" + calendarEvent.Location);
		}
	}

	string GetDateString(EventDateTime eventDateTime)
	{
		string startDate = "Unspecified";
		if (((eventDateTime != null)))
		{
			if (((eventDateTime.Date != null)))
			{
				startDate = eventDateTime.Date.ToString();
			}
			else if (eventDateTime.DateTime != null)
			{
				startDate = eventDateTime.DateTime.ToString();
			}
		}
		return startDate;
	}
}

