using Nicome.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NicoComment = Nicome.WWW.API.Types.Comment.CommentBody.Json;
using NicoUtils = Nicome.Utils;

namespace Nicome.Comment
{
    interface ICommentNg
    {
        bool JudgeAll(NicoComment::JsonComment c);
    }
    public class CommentNg : ICommentNg
    {
        private string moduleName = "Nicome.Comment.CommentNg";
        /// <summary>
        /// NG処理
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        public bool JudgeAll(NicoComment::JsonComment comment)
        {
            var data = new Store.Store().GetData();

            if (IsTimeNg(comment, data.GetNgTime())) return true;
            if (IsCommandNg(comment, data.GetNgCommand())) return true;
            if (IsUserNg(comment, data.GetNgUser())) return true;
            if (IsWordNg(comment, data.GetNgWord())) return true;
            return false;
        }

        /// <summary>
        /// 時間制NGにひっかかるかどうか
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        private bool IsTimeNg(NicoComment::JsonComment comment, List<NicoUtils.CommentTime.CommentTimeSpan> ngData)
        {
            if (comment.chat == null)
            {
                throw new ArgumentException("comment must be a chat, not a thread.");
            }

            DateTime cDatetime = NicoUtils.DateTimeUtils.FromUnixTime(comment.chat.date);

            foreach (var ng in ngData)
            {
                if (ng.From.ToDatetime(cDatetime) < cDatetime && cDatetime < ng.To.ToDatetime(cDatetime))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// コマンドNG
        /// </summary>
        /// <param name="content"></param>
        /// <param name="ngData"></param>
        /// <returns></returns>
        private bool IsCommandNg(NicoComment::JsonComment comment, List<string> ngData)
        {
            if (comment.chat == null)
            {
                throw new ArgumentException("comment must be a chat, not a thread.");
            }
            foreach (var ng in ngData)
            {
                if (comment.chat.mail != null && comment.chat.mail.Contains(ng)) return true;
            }

            return false;
        }

        /// <summary>
        /// NGユーザー
        /// </summary>
        /// <param name="content"></param>
        /// <param name="ngData"></param>
        /// <returns></returns>
        private bool IsUserNg(NicoComment::JsonComment comment, List<string> ngData)
        {
            if (comment.chat == null)
            {
                throw new ArgumentException("comment must be a chat, not a thread.");
            }
            foreach (var ng in ngData)
            {
                if (comment.chat.mail != null && comment.chat.user_id == ng) return true;
            }

            return false;
        }

        /// <summary>
        /// NGワード
        /// </summary>
        /// <param name="content"></param>
        /// <param name="ngData"></param>
        /// <returns></returns>
        private bool IsWordNg(NicoComment::JsonComment comment, List<string> ngData)
        {
            if (comment.chat == null)
            {
                throw new ArgumentException("comment must be a chat, not a thread.");
            }
            foreach (var ng in ngData)
            {
                if (comment.chat.content != null && comment.chat.content.Contains(ng)) return true;
            }

            return false;
        }
    }
}
