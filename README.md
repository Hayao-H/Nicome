# Nicome
ニコニコ動画のコメントをダウンロードします

# 使用方法
## 実行方法
nicome <オプションキー> <値> <オプションキー> <値>...
## 例
```nicome -u nico@example.com -p 0000 -i sm9 -f おんみょーじ```
## オプション
オプションキー | オプションキー(冗長表記) | 内容 | デフォルト値
----| ---- | ---- | :----:
-i | --id | **<<必須>>** ニコニコ動画におけるID | -
-u | --username | **<<必須>>** niconicoアカウントのユーザー名。 | -
-p | --password | **<<必須>>** niconicoアカウントのパスワード。 | -
-f | --folder | 保存フォルダー名。実行ファイルからの相対パス | 保存したコメント
-k | --kako | 過去ログ取得フラグ。値は不要。| (取得しない)
-l | --loglevel | quiet/error/warn/log/info/debug | log
# 動作環境
- Windows(Win10 homeで動作確認済)
# 開発環境
- .Net Core 3.1
- Visual Studio Community 2019
- VS code