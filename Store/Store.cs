using NicoUtl = Nicome.Utils;
using Nicome.WWW.API.Types.WatchPage;
using System;
using System.Collections.Generic;
using System.IO;
using Enums = Nicome.Enums;
using System.Data;
using System.Diagnostics;

namespace Nicome.Store
{

    abstract class StoreBase
    {
        abstract public Types.StoreRoot GetData(bool r);
    }

    class Store : StoreBase
    {

        private static Types.StoreRoot? data;

        public Store()
        {
            if (Store.data == null) throw new NoNullAllowedException("data is not set.");
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public Store(CLI.Parser parser)
        {
            Store.data = new Types.StoreRoot();

            //ID
            if (parser.Contains("nicoid"))
            {
                CLI.CLICommand? id;
                parser.TryGetOption("nicoid", out id);
                if (id == null || id.Parameter == null) throw new NoNullAllowedException("動画IDが指定されていません。");
                Store.data.Download.ID = id.Parameter;
            }
            else
            {
                throw new NoNullAllowedException("動画IDが指定されていません。");
            }

            //パスワード
            if (parser.Contains("pass"))
            {
                CLI.CLICommand? pass;
                parser.TryGetOption("pass", out pass);
                if (pass == null || pass.Parameter == null) throw new NoNullAllowedException("パスワードが指定されていません。");
                Store.data.User.Password = pass.Parameter;
            }
            else
            {
                throw new NoNullAllowedException("パスワードが指定されていません。");
            }

            //パスワード
            if (parser.Contains("user"))
            {
                CLI.CLICommand? user;
                parser.TryGetOption("user", out user);
                if (user == null || user.Parameter == null) throw new NoNullAllowedException("ユーザー名が指定されていません。");
                Store.data.User.UserName = user.Parameter;
            }
            else
            {
                throw new NoNullAllowedException("ユーザー名が指定されていません。");
            }

            //フォルダー
            if (parser.Contains("foldername"))
            {
                CLI.CLICommand? folder;
                parser.TryGetOption("foldername", out folder);
                if (folder != null && folder.Parameter != null)
                {
                    Store.data.Files.FolderName = folder.Parameter;
                }
            }

            //ファイル形式
            if (parser.Contains("format"))
            {
                CLI.CLICommand? arg;
                parser.TryGetOption("format", out arg);
                if (arg != null && arg.Parameter != null)
                {
                    Store.data.Files.Format = arg.Parameter;
                }
            }

            //ログレベル
            if (parser.Contains("loglevel"))
            {
                CLI.CLICommand? arg;
                parser.TryGetOption("loglevel", out arg);
                if (arg != null && arg.Parameter != null)
                {
                    Store.data.Log.LogLevel = arg.Parameter;
                }
            }

            //過去ログ
            if (parser.Contains("comlog"))
            {
                Store.data.Download.CommentLog = true;

            }
        }

        /// <summary>
        /// データを取得する
        /// </summary>
        /// <param name="refresh"></param>
        /// <returns></returns>
        public override Types.StoreRoot GetData(bool refresh = false)
        {
            return data;
        }
    }

    namespace Types
    {

        abstract class StoreRootBase
        {
            abstract public string GetNicoID();
            abstract public string GetPassWord();
            abstract public string GetUserName();
            abstract public string GetVideoFilePath();
            abstract public string GetVideoFileFormat();
            abstract public string GetPageAddress(string i);
            abstract public string GetNicoBaseAddress();
            abstract public string GetNicoLoginAddress();
            abstract public bool DoDownloadCommentLog();
            abstract public bool IsStartFromPostDate();
            abstract public void SetPostDate(DateTime p);
            abstract public List<NicoUtl.CommentTime.CommentTimeSpan> GetNgTime();
            abstract public Enums::LOGLEVEL GetLogLevel();
        }
        class StoreRoot : StoreRootBase
        {
            public override string GetNicoID()
            {
                return Download.ID;
            }
            /// <summary>
            /// パスワードを取得する
            /// </summary>
            /// <returns></returns>
            public override string GetPassWord()
            {
                return User.Password;
            }

            /// <summary>
            /// ユーザー名を取得する
            /// </summary>
            /// <returns></returns>
            public override string GetUserName()
            {
                return User.UserName;
            }

            /// <summary>
            /// ログレベルを取得する
            /// </summary>
            /// <returns></returns>
            public override Enums::LOGLEVEL GetLogLevel()
            {
                switch (Log.LogLevel)
                {
                    case "quiet":
                        return Enums::LOGLEVEL.Quiet;
                    case "error":
                        return Enums::LOGLEVEL.Error;
                    case "warn":
                        return Enums::LOGLEVEL.Warn;
                    case "info":
                        return Enums::LOGLEVEL.Info;
                    case "debug":
                        return Enums::LOGLEVEL.Debug;
                    default:
                        return Enums::LOGLEVEL.Log;
                }
            }


            /// <summary>
            /// 動画ファイルの保存パスを返す
            /// </summary>
            /// <param name="foldername"></param>
            /// <returns></returns>
            public override string GetVideoFilePath()
            {
                return Path.Combine(Files.BaseDirectory, Files.FolderName);
            }

            /// <summary>
            /// 動画ファイル名のフォーマットを返す
            /// </summary>
            /// <returns></returns>
            public override string GetVideoFileFormat()
            {
                return Files.Format;
            }

            /// <summary>
            /// ニコニコ動画の基本URLを返す
            /// </summary>
            /// <returns></returns>
            public override string GetNicoBaseAddress()
            {
                return Niconico.NicoBaseUrl;
            }

            /// <summary>
            /// ニコニコ動画のページアドレスを返す
            /// </summary>
            public override string GetPageAddress(string id)
            {
                return $"{Niconico.NicoBaseUrl}{id}";
            }

            /// <summary>
            /// ニコニコ動画のログインURLを返す
            /// </summary>
            /// <returns></returns>
            public override string GetNicoLoginAddress()
            {
                return Niconico.NicoLoginAddress;
            }

            /// <summary>
            /// 過去ログをダウンロードするかどうか
            /// </summary>
            public override bool DoDownloadCommentLog()
            {
                return this.Download.CommentLog;
            }

            /// <summary>
            /// 時間帯NGデータを返す
            /// </summary>
            /// <returns></returns>
            public override List<NicoUtl.CommentTime.CommentTimeSpan> GetNgTime()
            {
                return this.Ngs.NgTimes;
            }

            /// <summary>
            /// 投稿日起点型NGであるかどうかを返す
            /// </summary>
            /// <returns></returns>
            public override bool IsStartFromPostDate()
            {
                return this.Ngs.IsStartFromPostDate;
            }

            /// <summary>
            /// 投稿日時を設定
            /// </summary>
            /// <param name="posted"></param>
            public override void SetPostDate(DateTime posted)
            {
                if (this.Ngs.IsStartFromPostDate)
                {
                    foreach (var ng in this.Ngs.NgTimes)
                    {
                        ng.SetPostDate(posted);
                    }
                }
            }

            public UserInfo User { get; set; } = new UserInfo();
            public NicoInfo Niconico { get; set; } = new NicoInfo();
            public LogConfig Log { get; set; } = new LogConfig();
            public FileConfig Files { get; set; } = new FileConfig();
            public DownloadInfo Download { get; set; } = new DownloadInfo();
            public NgInfo Ngs { get; set; } = new NgInfo();

        }

        class DownloadInfo
        {
            public string ID { get; set; } = "sm9";
            public bool CommentLog { get; set; } = false;
        }

        class UserInfo
        {
            public string Password { get; set; } = "";
            public string UserName { get; set; } = "";
        }

        class NicoInfo
        {
            public string NicoBaseUrl { get; set; } = "https://nicovideo.jp/watch/";
            public string NicoLoginAddress { get; set; } = "https://secure.nicovideo.jp/secure/login";
            public string NicoDmcAPIAddress { get; set; } = "https://api.dmc.nico:443/api/sessions?_format=json";
            public string NicoNvAPIAddress { get; set; } = "https://nvapi.nicovideo.jp/v1/2ab0cbaa/watch";
        }

        class LogConfig
        {
            public string LogLevel { get; set; } = "log";
        }

        class FileConfig
        {
            public string BaseDirectory { get; set; } = Directory.GetParent(Process.GetCurrentProcess().MainModule.FileName).FullName;
            public string FolderName { get; set; } = "保存したコメント";
            public string Format { get; set; } = "[<id>]<title>";
        }

        class NgInfo
        {
            public List<NicoUtl.CommentTime.CommentTimeSpan> NgTimes { get; set; } = new List<NicoUtl.CommentTime.CommentTimeSpan>();
            public bool IsStartFromPostDate = false;
        }
    }
}
