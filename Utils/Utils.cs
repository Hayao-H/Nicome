using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nicome.Utils
{
    public class DateTimeUtils
    {
        public static long ToUnixTime(DateTime dt)
        {
            var dtOffset = new DateTimeOffset(dt.Ticks, new TimeSpan(09, 00, 00));
            return dtOffset.ToUnixTimeSeconds();
        }

        public static DateTime FromUnixTime(long t)
        {
            return DateTimeOffset.FromUnixTimeSeconds(t).ToLocalTime().DateTime;
        }

        public static List<CommentTime.CommentTimeSpan> ParseDateTime(string time)
        {
            var timeList = new List<CommentTime.CommentTimeSpan>();
            string[] timesArray = time.Split(',');

            foreach (var t in timesArray)
            {
                timeList.Add(new CommentTime.CommentTimeSpan(t));
            }

            return timeList;
        }

        public static List<CommentTime.CommentTimeSpan> ParseDateTime(string time,string delay)
        {
            var timeList = new List<CommentTime.CommentTimeSpan>();
            string[] timesArray = time.Split(',');

            int d;
            if (!int.TryParse(delay, out d)) throw new ArgumentException("オプション\"--ng-time-from-to-delay\"は数値である必要があります");

            foreach (var t in timesArray)
            {
                timeList.Add(new CommentTime.CommentTimeSpan(t,d));
            }

            return timeList;
        }
    }

    namespace CommentTime
    {
        public class CommentTimeSpan
        {
            public CommentTimeSpan(string time)
            {
                if (!time.Contains('-'))
                {
                    throw new ArgumentException(this.ErrorMessageNoSplitChar);
                }
                else if (time.Where(c => c == '-').Count() > 1)
                {
                    throw new ArgumentException(this.ErrorMessage);
                }

                string[] fromto = time.Split('-');
                this.From = new TimeInfo(fromto[0]);
                this.To = new TimeInfo(fromto[1]);

                //日付調節
                if (this.To.IsBack(this.From))
                {
                    this.To.Day = 1;
                }

            }
            public CommentTimeSpan(string time,int delay)
            {
                if (!time.Contains('-'))
                {
                    throw new ArgumentException(this.ErrorMessageNoSplitChar);
                }
                else if (time.Where(c => c == '-').Count() > 1)
                {
                    throw new ArgumentException(this.ErrorMessage);
                }

                string[] fromto = time.Split('-');
                this.From = new TimeInfo(fromto[0]);
                this.To = new TimeInfo(fromto[1]);

                this.From.Day = delay;
                this.To.Day = delay;

                //日付調節
                if (this.To.IsBack(this.From))
                {
                    this.To.Day++;
                }

            }

            /// <summary>
            /// エラーメッセージ
            /// </summary>
            private string ErrorMessage = "不正な時間指定の形式です。(\"-\"は一文字だけです)";

            /// <summary>
            /// エラーメッセージ
            /// </summary>
            private string ErrorMessageNoSplitChar = "不正な時間指定の形式です。(時間は範囲を指定して下さい)";

            /// <summary>
            /// 投稿日時を設定
            /// </summary>
            /// <param name="postdate"></param>
            public void SetPostDate(DateTime postdate)
            {
                this.From.PostDatetime = postdate;
                this.To.PostDatetime = postdate;

                Console.WriteLine($"{this.From.ToDatetime(DateTime.Now)}-{this.To.ToDatetime(DateTime.Now)}");
            }

            public TimeInfo From { get; private set; }

            public TimeInfo To { get; private set; }
        }

        public class TimeInfo
        {
            public TimeInfo(string time)
            {
                if (!time.Contains(':'))
                {
                    throw new ArgumentException(this.ErrorMessage);
                }
                else if (time.Where(c => c == ':').Count() > 1)
                {
                    throw new ArgumentException(this.ErrorMessage);
                }

                string[] hourminite = time.Split(':');

                try
                {
                    this.Hour = int.Parse(hourminite[0]);
                    this.Minute = int.Parse(hourminite[1]);
                }
                catch (ArgumentException)
                {
                    throw new Exception(ErrorMessageNotNumber);
                }
            }
     

            /// <summary>
            /// エラーメッセージ
            /// </summary>
            private string ErrorMessage = "不正な時間指定の形式です。(\":\"は一文字だけです)";

            /// <summary>
            /// エラーメッセージ
            /// </summary>
            private string ErrorMessageNotNumber = "日時は数字である必要があります。";

            /// <summary>
            /// 時間
            /// </summary>
            public int Hour { get; set; }

            /// <summary>
            /// 分
            /// </summary>
            public int Minute { get; set; }

            /// <summary>
            /// 日付のoffset
            /// </summary>
            public int Day { get; set; }
        
            /// <summary>
            /// 投稿日時
            /// </summary>
            public DateTime? PostDatetime { get; set; }

            /// <summary>
            /// 指定した日付でDatetime型のインスタンスを返す
            /// </summary>
            /// <param name="source"></param>
            /// <returns></returns>
            public DateTime ToDatetime(DateTime source)
            {
                return new DateTime(this.PostDatetime==null? source.Year:this.PostDatetime.Value.Year,
                    this.PostDatetime == null ? source.Month : this.PostDatetime.Value.Month,
                    this.PostDatetime == null ? source.Day : this.PostDatetime.Value.Day + this.Day,
                    this.Hour, this.Minute,0);
            }

            /// <summary>
            /// インスタンス同士で比較
            /// </summary>
            /// <param name="t"></param>
            /// <returns></returns>
            public bool IsBack(TimeInfo t)
            {
                var dt = DateTime.Now;
                return this.ToDatetime(dt) < t.ToDatetime(dt);
            }
        }

    }

    public class XmlUtils
    {
        public static string FormatXMLProp(string origin)
        {
            string formated = origin;
            formated = XmlUtils.RemoveSymbols(formated);
            return formated;
        }

        public static string RemoveSymbols(string origin)
        {
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
            return Regex.Replace(origin, r, "", RegexOptions.Compiled);
        }
    }
}
