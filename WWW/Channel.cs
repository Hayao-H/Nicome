using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;

namespace Nicome.WWW
{
    class Channel
    {
        /// <summary>
        /// IDのリストを取得する
        /// </summary>
        /// <param name="channnel"></param>
        /// <returns></returns>
        public async Task<List<string>> GetIdList(string channnel)
        {
            var parser = new HtmlParser();
            string html = await this.GetHtml(channnel);
            var document = parser.ParseDocument(html);
            return document.QuerySelectorAll(".g-video-link").Select(e => this.GetIdFromAddress(e.GetAttribute("href"))).Distinct().ToList();
        }

        /// <summary>
        /// ページアドレスからIDを取得する
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private string GetIdFromAddress(string address)
        {
            string[] addressArray = address.Split('/');
            return addressArray[addressArray.Length - 1];
        }

        /// <summary>
        /// チャンネルページのHTMlを取得する
        /// </summary>
        /// <param name="channnel"></param>
        /// <returns></returns>
        public async Task<string> GetHtml(string channnel)
        {
            var client = new NicoHttp();
            HttpResponseMessage response = await client.Client.GetAsync($"https://ch.nicovideo.jp/{channnel}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            } else
            {
                throw new HttpRequestException($"チャンネル「{channnel}」のページ取得に失敗しました。(status: {(int)response.StatusCode})");
            }
        }
    }
}
