using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nicome.Utils
{
    class DateTimeUtils
    {
        public static long ToUnixTime(DateTime dt)
        {
            var dtOffset = new DateTimeOffset(dt.Ticks, new TimeSpan(09, 00, 00));
            return dtOffset.ToUnixTimeSeconds();
        }
    }

    class XmlUtils
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
