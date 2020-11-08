using AngleSharp.Html.Parser;
using Nicome.Utils;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Store = Nicome.Store;
using WatchPage = Nicome.WWW.API.Types.WatchPage;

namespace Nicome.WWW
{
    interface INicoContext
    {
        Task<bool> GetContent(string id, string message);
        VideoInfo GetVideoInfo();
    }
    class NicoContext : INicoContext, IDisposable
    {

        public WWW.NicoHttp? NicoClient;
        public WatchPage::BaseJson? ApiData { get; private set; }
        private Uri referrerUri;
        private string moduleName = "WWW.NicoContext";
        private Store::Types.StoreRoot store;
        private string GeneralID="";


        /// <summary>
        /// ストリームの状態
        /// </summary>
        public bool IsOpen { get; private set; } = false;

        public NicoContext()
        {
            store = new Store::Store().GetData();
            NicoClient = new WWW.NicoHttp();
            referrerUri = new Uri(store.GetNicoBaseAddress());
            IsOpen = true;
        }

        /// <summary>
        /// ページを取得する
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> GetContent(string id, string message = "ダウンロード")
        {
            GeneralID = id;
            var logger = NicoLogger.GetLogger();
            referrerUri = new Uri(store.GetPageAddress(id));

            logger.Debug("niconicoとの通信を開始", moduleName);
            logger.Debug("コンテンツの取得・解析を開始", moduleName);
            try
            {
                await NicoClient.LoginAsync(store.GetUserName(), store.GetPassWord());
                ApiData = await GetApiData(id);
                var video = GetVideoInfo();
                logger.Log($"[{video.Id}] 「 {video.Title} 」の{message}を開始します。");
            }
            catch (Exception e)
            {
                logger.Error($"{e.Message}");
                logger.Error(e.StackTrace == null ? "there's no stack_trace..." : e.StackTrace);
                return false;
            }
            logger.Debug("コンテンツの取得・解析が完了", moduleName);
            return true;
        }
        /// <summary>
        /// APIデータを取得する
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<WatchPage::BaseJson> GetApiData(string id)
        {
            string response = await NicoClient.GetPageAsync(id);

            //デシリアライズJSON
            return LoadHTML(response);
        }

        /// <summary>
        /// 視聴ページのJSONをデシリアライズする
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private WatchPage::BaseJson LoadHTML(string html)
        {
            var logger = NicoLogger.GetLogger();
            logger.Debug("htmlの解析を開始", moduleName);

            var htmlParser = new HtmlParser();
            var document = htmlParser.ParseDocument(html);

            WatchPage::BaseJson data = JsonSerializer.Deserialize<WatchPage::BaseJson>(document.QuerySelector("#js-initial-watch-data").GetAttribute("data-api-data"));

            logger.Debug("htmlの解析が完了", moduleName);
            return data;
        }

        /// <summary>
        /// 動画情報を取得するv
        /// </summary>
        /// <returns></returns>
        public VideoInfo GetVideoInfo()
        {
            if (ApiData != null && ApiData.video != null && ApiData.video.title != null && ApiData.video.id != null&&ApiData.video.postedDateTime!=null)
            {
                return new VideoInfo(ApiData.video.title, ApiData.owner == null ? "" : ApiData.owner.nickname == null ? "" : ApiData.owner.nickname, GeneralID, ApiData.video.postedDateTime);
            }
            else
            {
                throw new Exception("動画情報の生成に必要な項目がnullです。");
            }
        }
        /// <summary>
        /// 破棄
        /// </summary>
        public void Dispose()
        {
            var logger = NicoLogger.GetLogger();
            logger.Debug("niconicoとの通信を終了",moduleName);
            IsOpen = false;
            NicoClient = null;
        }
    }

    class VideoInfo
    {

        public VideoInfo(string _title, string _user, string _id,string _dt)
        {
            Title = _title;
            User = _user;
            Id = _id;
            try
            {
                this.PostedDateTime = DateTime.Parse(_dt);
            }
            catch
            {
                throw new Exception("動画の投稿日時の解析に失敗しました");
            }
        }

        public string Title { get; set; }
        public string User { get; set; }
        public string Id { get; set; }
        public DateTime PostedDateTime { get; set; }
    }
}
