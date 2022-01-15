using System;

namespace EzAspDotNet.Util
{
    public static class DateTimeUtil
    {
        public static long GetCurrentTimeStamp()
        {
            DateTime dt = DateTime.Now;
            return ((DateTimeOffset)dt).ToUnixTimeSeconds();
        }

        public static long ToTimeStamp(this DateTime value)
        {
            return ((DateTimeOffset)value).ToUnixTimeSeconds();
        }

        public static DateTime ToDateTime(this long value)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddSeconds(value).ToLocalTime();
            return dt;
        }
    }
}
