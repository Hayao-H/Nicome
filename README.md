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
-m | --max-comment | 最大コメント数(数値) | (無限)
-c | --channel | チャンネル名 | -
-y | --allow-overwrite | 確認せず上書き | (確認する)
-n | --disallow-overwrite | 確認せずスキップ | (確認する)
## NG関連のオプション(β版)
※開発中の為、動作が不安定である可能性があります。
- 時間制NG
    - 時間帯指定  
        --ng-time-from-to  
        ww:xx-yy:zzの形式  
        カンマ区切りで複数時間帯指定可能 。  
        例
        ``` 
        --ng-time-from-to 18:00-0:00,12:00-13:00
        ```
    - 投稿日時を起点にする  
        --ng-time-from-to-postdate  
        オプション。値は必要ありません。  
        NGを、投稿日時のみに適用します。  **必ず時間帯指定のオプションとセットで使用して下さい。**   
        例
        ``` 
        --ng-time-from-to-postdate
        ```
    - 投稿日時の〇日後を起点にする  
        --ng-time-from-to-delay  
        オプション。  
        NGを、投稿日時から指定日数経過した日のみに適用します。 **必ず上の2つのオプションとセットで使用して下さい。**  
        例
        ``` 
        --ng-time-from-to-delay 3
        ```

- NGコマンド  
-nc/--ng-command
カンマ区切りで複数指定可能です。  
例
    ```
    -nc device:3DS,device:Switch,shita,aka
    ```

- NGユーザー  
-nu/--ng-user  
カンマ区切りで複数指定可能です。  
例
    ```
    -nu xxxxyyyyzzzz
    ```

- NGワード  
-nw/--ng-word  
カンマ区切りで複数指定可能です。  
 例
    ```
    -nw うぽつ,草
    ```

# 動作環境
- Windows(Win10 homeで動作確認済)
# 開発環境
- .Net Core 3.1
- Visual Studio Community 2019
- VS code