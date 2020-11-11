using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Threading.Tasks;
using NicoLogger = Nicome.Utils.NicoLogger;
using Store = Nicome.Store;
using System.Linq;

namespace Nicome.WWW
{

    interface INicoHttp
    {
        Task<string> GetPageAsync(string id);
        Task OptionAsync(Uri uri);
        Task LoginAsync(string u, string p);
        HttpRequestMessage CreateHttpRequest(HttpMethod m, Uri u, Dictionary<string, string> h, Uri r);
    }

    class NicoHttp : INicoHttp
    {

        private string moduleName = "WWW.NicoHttp";
        /// <summary>
        /// ログイン状態
        /// </summary>
        public static bool IsLogin { get; set; }

        /// <summary>
        /// httpクライアント
        /// </summary>
        public HttpClient Client = Nicome.Program.Client;

        /// <summary>
        /// 非同期にページを取得する
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> GetPageAsync(string id)
        {
            var logger = NicoLogger.GetLogger();
            var store = new Store::Store().GetData();

            logger.Debug($"ページの取得を開始({id})", moduleName);
            var res = await Client.GetAsync($"{store.GetPageAddress(id)}");

            if (res.IsSuccessStatusCode)
            {
                logger.Debug("ページの取得が完了", moduleName);
                return await res.Content.ReadAsStringAsync();
            }
            else
            {
                throw new HttpRequestException($"コンテンツの取得に失敗しました。(id: {id}, status_code: {(int)res.StatusCode})");
            }
        }

        /// <summary>
        /// optionsメソッドで送信する
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task OptionAsync(Uri uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Options, uri);
            HttpResponseMessage res = await Client.SendAsync(request);

            //エラーハンドリング
            if (!res.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"アクセスに失敗しました。(status_code: {(int)res.StatusCode})");
            }
        }

        /// <summary>
        /// ログインする
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task LoginAsync(string username, string password)
        {
            //二重ログイン回避
            if (NicoHttp.IsLogin) return;

            var store = new Store::Store().GetData();
            var loginData = new Dictionary<string, string>() {
                {"mail",username },
                {"password",password }
            };

            var logger = NicoLogger.GetLogger();

            logger.Debug("ログインを試行", moduleName);
            HttpResponseMessage res = await Client.PostAsync(store.GetNicoLoginAddress(), new FormUrlEncodedContent(loginData));

            if (res.IsSuccessStatusCode&&!res.Headers.Contains("Set-Cookie"))
            {
                logger.Log("ログインに成功しました。");
                NicoHttp.IsLogin = true;
            }
            else
            {
                throw new HttpRequestException($"ログインに失敗しました。パスワード・ユーザー名を確認して下さい。(status_code: {(int)res.StatusCode})");
            }

            logger.Debug($"username: {username}　, pass: {Regex.Replace(password, ".", "●")}", moduleName);
        }

        /// <summary>
        /// リクエストメッセージを作成する
        /// </summary>
        /// <returns></returns>
        public HttpRequestMessage CreateHttpRequest(HttpMethod method, Uri uri, Dictionary<string, string> header, Uri referrer)
        {
            var request = new HttpRequestMessage(method, uri);

            foreach (var (key, val) in header)
            {
                request.Headers.Add(key, val);
            }

            request.Headers.Referrer = referrer;

            return request;
        }
    }
}
