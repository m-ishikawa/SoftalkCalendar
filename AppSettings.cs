using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Util.Store;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Json;

class Settings
{
	[Newtonsoft.Json.JsonProperty("softalk_path")]
	public string SoftalkPath { get; set; }
	[Newtonsoft.Json.JsonProperty("morning_meeting_time")]
	public string MorningMeetingTime { get; set; }
	[Newtonsoft.Json.JsonProperty("morning_meeting_message")]
	public string MorningMeetingMessage { get; set; }
	[Newtonsoft.Json.JsonProperty("update_interval")]
	public string UpdateInterval { get; set; }
}

class AppSettings
{
	[Newtonsoft.Json.JsonProperty("settings")]
	private Settings _settings = null;

	public Settings Settings { get { return _settings; } }

	public static AppSettings Load(Stream stream)
	{
		return NewtonsoftJsonSerializer.Instance.Deserialize<AppSettings>(stream);
	}

}

