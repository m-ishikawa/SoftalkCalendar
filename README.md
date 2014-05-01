SoftalkCalendar
=
Googleカレンダーに登録してあるイベントを、時間になったらゆっくりが教えてくれるツールです。

#### 機能
+ イベントの開始時間と終了時間にゆっくりがしゃべります
+ 朝会の時間に一日にスケジュールをゆっくりがしゃべります

使い方
-
+ [カレンダー](https://www.google.com/calendar/)にイベントを登録する
+ [API Console](https://code.google.com/apis/console/)の APIs から Calendar API を ON にする
+ 同サイトの Consent screen から 自分のメールアドレスを設定する
+ 同様に credentials から Client ID と Client secret を取得し client_secrets.json ファイルを書き換える
+ [SofTalk](http://www35.atwiki.jp/softalk/)をダウンロードし　`c:\softalk\SofTalk.exe` に解凍する 
+ アプリを起動

設定ファイル
-
実行時に、カレントに有る settings.json ファイルを読み込みます。  
ファイルを変更した場合はアプリを再起動して下さい。

+ `softalk_path`: SofTalk のインストール Path
+ `morning_meeting_time`: 朝会の時間
+ `morning_meeting_message`: 朝会開始時の一言
+　`update_interval`: カレンダーの更新間隔

関連情報
-
[Google Calendar API V3](https://developers.google.com/google-apps/calendar/)  
[SofTalk](http://www35.atwiki.jp/softalk/)
