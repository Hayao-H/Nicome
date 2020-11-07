using System;
using System.Collections.Generic;
using System.Text;

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
}
