using System;
using System.Threading.Tasks;
using System.Text.Json;
using Nicome.Utils;
using System.Collections.Generic;
using WatchPage = Nicome.WWW.API.Types.WatchPage;
using ComApi = Nicome.WWW.API.Types.Comment;
using NicoEnums = Nicome.Enums;
using System.Net.Http;
using System.Xml.Serialization;
using System.Linq;
using System.Xml;
using System.Text;
using NicoComment = Nicome.Comment;
using System.Text.RegularExpressions;

namespace Nicome.WWW.Comment
{
    class Comment
    {
        public async Task<NicoEnums::GenelicErrorCode> DownloadComment()
        {
            var logger = NicoLogger.GetLogger();
            var storeData = new Store.Store().GetData();

            bool fileExists = Utils.IO.Exists(storeData.GetNicoID());
            if (!storeData.DoOverWrite()&&fileExists)
            {
                while (true)
                {
                    logger.Warn($"id:{storeData.GetNicoID()}は既に保存済です。上書きしますか？(Y/N)");
                    string continueOrNot = Console.ReadLine();
                    if (Regex.IsMatch(continueOrNot, "[yY]"))
                    {
                        break;
                    } else if (Regex.IsMatch(continueOrNot, "[nN]"))
                    {
                        logger.Log("処理をスキップします。");
                        return NicoEnums::GenelicErrorCode.OK;
                    } else
                    {
                        logger.Error("YまたはNで答えて下さい。");
                        continue;
                    }
                }
            }

            VideoInfo video;
            List<ComApi.CommentBody.Json.JsonComment> comment;

            using (var context = new NicoContext())
            {
                if (!await context.GetContent(storeData.GetNicoID(), "コメントのダウンロード"))
                {
                    logger.Error("セッションの確立に失敗しました。");
                    return NicoEnums::GenelicErrorCode.ERROR;
                }

                //nullチェック
                if (context.ApiData == null)
                {
                    logger.Error("APIデータの取得に失敗しました。");
                    return NicoEnums.GenelicErrorCode.ERROR;
                }

                //動画情報
                video = context.GetVideoInfo();

                //投稿日時を指定
                storeData.SetPostDate(video.PostedDateTime);

                var comCliet = new CommentClient(context);
                try
                {
                    comment = await comCliet.GetComment(context.ApiData, context, storeData.DoDownloadCommentLog());
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                    return NicoEnums.GenelicErrorCode.ERROR;
                }
            }


            //保存
            var save = new Nicome.IO.Save(video);
            bool ioResult = save.TryWriteComment(comment);
            if (!ioResult)
            {
                return NicoEnums::GenelicErrorCode.ERROR;
            }

            logger.Log("コメントの保存が完了しました。");

            return NicoEnums.GenelicErrorCode.OK;
        }
    }

    class CommentRequestInfo
    {
        public string? Threadkey { get; set; }
        public string? Force184 { get; set; }
        public string? WayBackKey { get; set; }
        public int ComRequestCount { get; set; } = 0;
        public int ComCommandCount { get; set; } = 0;
        public int LastResNo { get; private set; } = 0;
        public int CommentBlockNo { get; private set; }
        public void SetLastResNo(int lastres)
        {
            LastResNo = lastres;
            double div = lastres / 4000;
            CommentBlockNo = (int)Math.Floor(div);
        }
    }

    class CommentClient
    {
        private NicoContext context;
        private CommentRequestInfo crInfo = new CommentRequestInfo();
        private string moduleName = "WWW.Comment.Commentclient";

        public CommentClient(NicoContext _context)
        {
            context = _context;
        }

        /// <summary>
        /// コメントを取得する(エントリー・ポイント)
        /// </summary>
        /// <returns></returns>
        public async Task<List<ComApi.CommentBody.Json.JsonComment>> GetComment(WatchPage::BaseJson data, NicoContext context, bool kako = false)
        {

            var logger = NicoLogger.GetLogger();

            logger.Debug($"過去ログ: {kako}", moduleName);


            //threadkeyを取得
            (crInfo.Threadkey, crInfo.Force184) = await GetThreadKey(data.video.dmcInfo.thread.thread_id.ToString(), context);


            //最古コメントを取得
            logger.Log("現行コメントをダウンロード中...");
            var comments = new CommentList();
            comments.Add(await GetCommentData(data));
            ComApi::CommentBody.Json.Chat firstComment = comments.GetFirstComment();



            if (kako)
            {
                crInfo.WayBackKey = await GetWayBackKey(data.video.dmcInfo.thread.thread_id, context);
                //初期whenデータを作成
                int _when = comments.GetFirstComment().date;
                int i = 1;
                do
                {
                    var kacomments = new CommentList();
                    logger.Log($"過去ログをダウンロード中...({i}件目、no.{firstComment.no}まで取得完了)");
                    kacomments.Add(await GetCommentData(data, _when));
                    try
                    {
                        firstComment = kacomments.GetFirstComment();
                    }
                    catch
                    {
                        break;
                    }
                    _when = firstComment.GetPrevDate();
                    comments.Merge(kacomments);
                    ++i;
                    if (comments.IsMax(i)) break;
                }
                while (firstComment.no > 1);

            }
            logger.Log("ダウンロードしたコメントの整合性をチェックしています。");
            comments.FormatComments();

            return comments.Comments;
        }

        /// <summary>
        /// コメントデータを取得する
        /// </summary>
        private async Task<List<ComApi.CommentBody.Json.JsonComment>> GetCommentData(WatchPage::BaseJson data, int? _when = null)
        {
            var logger = NicoLogger.GetLogger();
            var serializer = new CommentSerializer();
            var chandler = new CommentHandller();

            List<ComApi.RequestItems> comRequest = GetCommentRequest(data, _when);

            //コメントを取得する
            logger.Debug("コメントの取得を開始", moduleName);
            HttpClient client = context.NicoClient.Client;
            var comBody = await client.PostAsync(new Uri(@"http://nmsg.nicovideo.jp/api.json/"), new StringContent(serializer.SerializeJson(comRequest)));

            //エラーハンドリング
            if (!comBody.IsSuccessStatusCode)
            {
                throw new Exception($"コメントの取得に失敗しました。 (status_code: {(int)comBody.StatusCode})");
            }
            else
            {
                logger.Debug($"コメントデータの取得が完了", moduleName);
            }

            //取得したコメントをデシリアライズ
            string comJson = await comBody.Content.ReadAsStringAsync();

            return serializer.DeserializeJson<List<ComApi.CommentBody.Json.JsonComment>>(comJson);
        }

        /// <summary>
        /// リクエストデータを取得する
        /// </summary>
        private List<ComApi.RequestItems> GetCommentRequest(WatchPage::BaseJson data, int? _when)
        {
            var logger = NicoLogger.GetLogger();
            logger.Debug("コメントリクエストデータの作成を開始", moduleName);


            var request = new List<ComApi.RequestItems>();

            void insertStart(string type = "ps")
            {
                var s = new ComApi.RequestItems();
                s.ping = new ComApi.CommentItems.Ping();
                switch (type)
                {
                    case "ps":
                        s.ping.content = $"ps:{crInfo.ComCommandCount}";
                        break;
                    case "rs":
                        s.ping.content = $"rs:{crInfo.ComRequestCount}";
                        break;
                }
                request.Add(s);
            }

            void insertEnd(string type = "pf")
            {
                var e = new ComApi.RequestItems();
                e.ping = new ComApi.CommentItems.Ping();
                switch (type)
                {
                    case "pf":
                        e.ping.content = $"pf:{crInfo.ComCommandCount}";
                        ++crInfo.ComCommandCount;
                        break;
                    case "rf":
                        e.ping.content = $"rf:{crInfo.ComRequestCount}";
                        ++crInfo.ComRequestCount;
                        break;
                }
                request.Add(e);
            }

            //rs
            insertStart("rs");

            foreach (var thread in data.commentComposite.threads)
            {
                if (!thread.isActive) continue;

                insertStart(); if (_when == null)
                {
                    request.Add(GetThread(thread, data));
                }
                else
                {
                    request.Add(GetThread(thread, data, (int)_when));
                }
                insertEnd();

                if (thread.isLeafRequired)
                {
                    insertStart();
                    if (_when == null)
                    {
                        request.Add(GetLeaf(thread, data));
                    }
                    else
                    {
                        request.Add(GetLeaf(thread, data, (int)_when));
                    }
                    insertEnd();
                }
            }

            //rf
            insertEnd("rf");
            logger.Debug("コメントリクエストデータの作成が完了", moduleName);

            return request;
        }

        /// <summary>
        /// threadを取得
        /// </summary>
        private ComApi.RequestItems GetThread(WatchPage::CommentCompositeItems.Thread thread, WatchPage::BaseJson data)
        {
            var command = new ComApi.RequestItems();
            command.thread = new ComApi.CommentItems.Thread();
            command.thread.thread = $"{thread.id}";
            command.thread.fork = thread.fork;
            command.thread.user_id = $"{data.video.dmcInfo.user.user_id}";

            //公式動画・ユーザー動画で分岐
            if (thread.isThreadkeyRequired)
            {
                command.thread.threadkey = crInfo.Threadkey;
                command.thread.force_184 = crInfo.Force184;
            }
            else
            {
                command.thread.userkey = data.context.userkey;
            }

            return command;
        }

        /// <summary>
        /// threadを取得(過去ログ)
        /// </summary>
        private ComApi.RequestItems GetThread(WatchPage::CommentCompositeItems.Thread thread, WatchPage::BaseJson data, int _when)
        {
            var command = new ComApi.RequestItems();
            command.thread = new ComApi.CommentItems.Thread();
            command.thread.thread = $"{thread.id}";
            command.thread.fork = thread.fork;
            command.thread.user_id = $"{data.video.dmcInfo.user.user_id}";

            //公式動画・ユーザー動画で分岐
            if (thread.isThreadkeyRequired)
            {
                command.thread.threadkey = crInfo.Threadkey;
                command.thread.force_184 = crInfo.Force184;
            }
            else
            {
                //command.thread.userkey = data.context.userkey;
            }

            command.thread.waybackkey = crInfo.WayBackKey;
            command.thread.whencom = _when;

            return command;
        }

        /// <summary>
        /// Leafを取得
        /// </summary>
        private ComApi.RequestItems GetLeaf(WatchPage::CommentCompositeItems.Thread thread, WatchPage::BaseJson data)
        {
            var command = new ComApi.RequestItems();
            command.thread_leaves = new ComApi.CommentItems.Leaf();
            command.thread_leaves.thread = $"{thread.id}";
            command.thread_leaves.user_id = $"{data.video.dmcInfo.user.user_id}";
            int minute = (int)Math.Ceiling(data.video.duration / 60);
            command.thread_leaves.content = $"0-{minute}:100,1000,nicoru:100";

            if (thread.isThreadkeyRequired)
            {
                command.thread_leaves.threadkey = crInfo.Threadkey;
                command.thread_leaves.force_184 = crInfo.Force184;
            }
            else
            {
                command.thread_leaves.userkey = data.context.userkey;
            }

            return command;
        }

        /// <summary>
        /// Leafを取得(過去ログ)
        /// </summary>
        private ComApi.RequestItems GetLeaf(WatchPage::CommentCompositeItems.Thread thread, WatchPage::BaseJson data, int _when)
        {
            var command = new ComApi.RequestItems();
            command.thread_leaves = new ComApi.CommentItems.Leaf();
            command.thread_leaves.thread = $"{thread.id}";
            command.thread_leaves.user_id = $"{data.video.dmcInfo.user.user_id}";
            int minute = (int)Math.Ceiling(data.video.duration / 60);
            command.thread_leaves.content = $"0-{minute}:100,1000,nicoru:100";

            if (thread.isThreadkeyRequired)
            {
                command.thread_leaves.threadkey = crInfo.Threadkey;
                command.thread_leaves.force_184 = crInfo.Force184;
            }
            else
            {
                //command.thread_leaves.userkey = data.context.userkey;
            }

            command.thread_leaves.waybackkey = crInfo.WayBackKey;
            command.thread_leaves.whencom = _when;

            return command;
        }

        /// <summary>
        /// threadkeyを取得
        /// </summary>
        private async Task<(string, string)> GetThreadKey(string thread, NicoContext context)
        {
            var logger = NicoLogger.GetLogger();
            var client = context.NicoClient.Client;

            logger.Debug($"threadkeyの取得を開始(thread_id: {thread})", moduleName);
            var res = await client.GetAsync(new Uri($"http://flapi.nicovideo.jp/api/getthreadkey?thread={thread}"));
            if (res.IsSuccessStatusCode)
            {
                var resContent = await res.Content.ReadAsStringAsync();
                logger.Debug($"threadkeyの取得が完了(response: {resContent})", moduleName);
                var query = System.Web.HttpUtility.ParseQueryString(resContent);
                return (query["threadkey"], query["force_184"]);
            }
            else
            {
                throw new Exception("threadkeyの取得に失敗しました。");
            }
        }

        /// <summary>
        /// waybackkeyを取得
        /// </summary>
        private async Task<string> GetWayBackKey(int thread, NicoContext context)
        {
            var client = context.NicoClient.Client;
            var res = await client.GetAsync($"https://flapi.nicovideo.jp/api/getwaybackkey?thread={thread}");

            if (!res.IsSuccessStatusCode) throw new HttpRequestException("waybackkeyの取得に失敗しました。");

            var responseData = await res.Content.ReadAsStringAsync();
            var query = System.Web.HttpUtility.ParseQueryString(responseData);

            return query["waybackkey"];
        }
    }

    class CommentList
    {
        private string moduleName = "WWW.Comment.CommentList";
        public List<ComApi.CommentBody.Json.JsonComment> Comments { get; private set; } = new List<ComApi.CommentBody.Json.JsonComment>();

        /// <summary>
        /// コメント数を取得する
        /// </summary>
        public int Count
        {
            get
            {
                return this.GetCommentCount();
            }
        }

        /// <summary>
        /// コメントを追加する
        /// </summary>
        /// <param name="items"></param>
        public void Add(List<ComApi.CommentBody.Json.JsonComment> items)
        {
            this.Comments.AddRange(items);
        }

        /// <summary>
        /// コメント数を取得する
        /// </summary>
        /// <returns></returns>
        private int GetCommentCount()
        {
            return this.Comments.Where(c => c.chat != null).Count();
        }

        /// <summary>
        /// コメントリストを整形する
        /// </summary>
        public void FormatComments()
        {
            var logger = NicoLogger.GetLogger();
            logger.Debug("重複削除処理を開始", moduleName);
            this.RemoveDupe();
            logger.Debug("重複削除処理が完了", moduleName);

            logger.Debug("ソート処理を開始", moduleName);
            this.SortByNumber();
            logger.Debug("ソート処理が完了", moduleName);

            logger.Debug("NG処理を開始", moduleName);
            this.RemoveNgComment();
            logger.Debug("NG処理が完了", moduleName);
            if (this.Count > (int)this.GetMaxComments())
            {
                logger.Debug("コメ数調節処理を開始", moduleName);
                this.RemoveOld(this.Count - (int)this.GetMaxComments());
                logger.Debug("コメ数調節処理が完了", moduleName);
            }
        }

        /// <summary>
        /// 合成する
        /// </summary>
        /// <param name="list"></param>
        public void Merge(CommentList list)
        {
            this.Add(list.Comments);
        }

        /// <summary>
        /// 最初のコメントを取得する
        /// </summary>
        /// <returns></returns>
        public ComApi::CommentBody.Json.Chat GetFirstComment()
        {
            var handler = new CommentHandller();
            return handler.GetFirstComment(this.Comments);
        }

        /// <summary>
        /// コメントをソートする
        /// </summary>
        private void SortByNumber()
        {
            this.Comments.Sort((a, b) => { return a.chat == null || b.chat == null ? 0 : a.chat.no - b.chat.no; });
        }

        /// <summary>
        /// 重複を削除する
        /// </summary>
        private void RemoveDupe()
        {
            this.Comments = this.Comments.Distinct().ToList();
        }

        /// <summary>
        /// ngコメントを削除する
        /// </summary>
        private void RemoveNgComment()
        {
            var ngHandler = new NicoComment::CommentNg();
            var logger = NicoLogger.GetLogger();

            int removed = this.Comments.RemoveAll(c => c.chat != null && ngHandler.JudgeAll(c));
            logger.Log($"{removed}件のコメントをNG処理(削除)しました。");
        }

        /// <summary>
        /// 最大コメント数に達しているかどうか
        /// </summary>
        /// <returns></returns>
        public bool IsMax(int i)
        {
            var store = new Store.Store().GetData();
            return store.IsMaxCommentSet() && this.Count > this.GetMaxComments()+1000*i;
        }

        /// <summary>
        /// 後ろから削除
        /// </summary>
        /// <param name="count"></param>
        private void RemoveOld(int count)
        {
            this.Comments.RemoveRange(this.Comments.Count - count, count);
        }

        /// <summary>
        /// 最大コメント数を取得
        /// </summary>
        /// <returns></returns>
        private uint GetMaxComments()
        {
            var store = new Store.Store().GetData();
            return store.IsMaxCommentSet() ? store.GetMaxComment() : 0;
        }
    }

    class CommentHandller
    {  /// <summary>
       /// 最古コメントを取得
       /// </summary>
        public ComApi::CommentBody.Json.Chat GetFirstComment(List<ComApi::CommentBody.Json.JsonComment> comments)
        {
            var comment = comments.FirstOrDefault(c => { return c.chat != null; });

            if (comment == null || comment.chat == null)
            {
                //return GetLastComment(comments, no - 1);
                throw new Exception($"投稿日時が一番古いコメントを発見できません。");
            }
            else
            {
                return comment.chat;
            }
        }

        /// <summary>
        /// 次のスレッドがあるかどうかを返す
        /// </summary>
        public bool HasNextThread(List<ComApi.CommentBody.Json.JsonComment> comments, CommentRequestInfo crInfo)
        {
            var threadinfo = GetThreadInfo(comments);

            return threadinfo.last_res < crInfo.LastResNo;
        }

        /// <summary>
        /// スレッド情報を取得
        /// </summary>
        public ComApi::CommentBody.Json.Thread GetThreadInfo(List<ComApi.CommentBody.Json.JsonComment> comments)
        {
            var comment = comments.FirstOrDefault(ct => { return ct.thread != null && ct.thread.last_res != 0; });

            if (comment == null)
            {
                throw new Exception("スレッドデータが見つかりませんでした。");
            }
            else
            {
                return comment.thread;
            }
        }

    }

    class CommentSerializer
    {
        /// <summary>
        /// オブジェクトをシリアル化
        /// </summary>
        public string SerializeJson<T>(T data)
        {
            var jOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true,
                WriteIndented = true
            };
            return JsonSerializer.Serialize<T>(data, jOptions);
        }

        /// <summary>
        /// jsonをデシリアライズする
        /// </summary>
        public T DeserializeJson<T>(string input)
        {
            var jOptions = new JsonSerializerOptions()
            {
                IgnoreNullValues = true
            };
            return JsonSerializer.Deserialize<T>(input);
        }
        /// <summary>
        /// XMLをシリアライズする
        /// </summary>
        public string SerializeXML<T>(T data, bool addNamespace = false)
        {
            var stream = new StringBuilder();
            var serializer = new XmlSerializer(typeof(T));
            var xmlNamespace = new XmlSerializerNamespaces();

            if (!addNamespace)
            {
                //名前空間を無効化
                xmlNamespace.Add(string.Empty, string.Empty);
            }
            var Xsettings = new XmlWriterSettings()
            {
                OmitXmlDeclaration = true,
                Indent = true,
                IndentChars = " "
            };

            using (var writer = XmlWriter.Create(stream, Xsettings))
            {
                serializer.Serialize(writer, data, xmlNamespace);
            }



            return stream.ToString();
        }

    }

    class CommentConverter
    {

        /// <summary>
        /// XMLを取得
        /// </summary>
        /// <param name="comments"></param>
        /// <returns></returns>
        public string GetXML(List<ComApi.CommentBody.Json.JsonComment> comments)
        {
            var xmlItems = new ComApi::CommentBody.XML.packet();
            xmlItems.chat = new List<ComApi.CommentBody.XML.packetChat>();

            foreach (var comment in comments)
            {

                if (comment.thread != null)
                {
                    xmlItems.thread = GetThread(comment.thread);
                }
                else if (comment.chat != null)
                {
                    xmlItems.chat.Add(GetChat(comment.chat));
                }
            }
            var serializer = new CommentSerializer();
            return serializer.SerializeXML(xmlItems);
        }

        /// <summary>
        /// threadを取得
        /// </summary>
        private ComApi::CommentBody.XML.packetThread GetThread(ComApi::CommentBody.Json.Thread thread)
        {
            var xmlThread = new ComApi.CommentBody.XML.packetThread();

            xmlThread = new ComApi.CommentBody.XML.packetThread();
            xmlThread.thread = thread.thread == null ? 0 : int.Parse(thread.thread);
            xmlThread.resultcode = thread.resultcode;
            xmlThread.last_res = thread.last_res;
            xmlThread.revision = thread.revision;
            xmlThread.server_time = thread.server_time;
            xmlThread.ticket = thread.ticket == null ? "" : thread.ticket;

            return xmlThread;
        }

        /// <summary>
        /// chatを取得
        /// </summary>
        private ComApi::CommentBody.XML.packetChat GetChat(ComApi::CommentBody.Json.Chat chat)
        {
            var xmlChat = new ComApi::CommentBody.XML.packetChat();
            xmlChat.thread = chat.thread == null ? 0 : int.Parse(chat.thread);
            xmlChat.no = chat.no;
            xmlChat.vpos = chat.vpos;
            xmlChat.date = chat.date;
            xmlChat.date_usec = chat.date_usec;
            xmlChat.premium = chat.premium;
            xmlChat.anonymity = chat.anonymity;
            xmlChat.user_id = chat.user_id == null ? "" : chat.user_id;
            xmlChat.mail = chat.mail == null ? "" : chat.mail;
            xmlChat.leaf = chat.leaf;
            xmlChat.deleted = chat.deleted;
            xmlChat.score = chat.score;
            xmlChat.Value = chat.content == null ? "" : chat.content;

            return xmlChat;
        }
    }
}