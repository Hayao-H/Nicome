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


        public static List<CommentTimeSpan> ParseDateTime(string time)
        {
            var timeList = new List<CommentTimeSpan>();
            string[] timesArray = time.Split(',');

            foreach (var t in timesArray)
            {
                timeList.Add(new CommentTimeSpan(t));
            }

            return timeList;
        }
    }

    public class CommentTimeSpan:IEquatable<CommentTimeSpan>
    {
        public CommentTimeSpan(string time)
        {
            if (!time.Contains('-'))
            {
                throw new ArgumentException(this.ErrorMessage);
            }
            else if (time.Where(c => c == '-').Count() > 1)
            {
                throw new ArgumentException(this.ErrorMessage);
            }

            string[] fromto = time.Split('-');
            this.From = new TimeInfo(fromto[0]);
            this.To = new TimeInfo(fromto[1]);

        }

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        private string ErrorMessage = "不正な時間指定の形式です。(\"-\"は一文字だけです)";

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        private string ErrorMessageNoSplitChar = "不正な時間指定の形式です。(時間は範囲を指定して下さい)";

        public TimeInfo From { get; private set; }

        public TimeInfo To { get; private set; }

        public bool Equals(CommentTimeSpan other)
        {
            bool fromeq = this.From.Hour == other.From.Hour && this.From.Minute == other.From.Minute;
            bool toeq = this.To.Hour == other.To.Hour && this.To.Minute == other.To.Minute;
            return true;
        }

        public override int GetHashCode()
        {
            //return this.From.Hour.GetHashCode()^this.From.Minute.GetHashCode();
            return 1;
        }
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
                this.Hour = int.Parse(hourminite[1]);
            }
            catch(ArgumentException)
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
