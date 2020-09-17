using System;

namespace MeshBackend.Helpers
{
    public static
        class TimeStampConvertHelper
    {
        public static long ConvertToTimeStamp(DateTime time)
        {
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            var timeStamp = (long) (time - startTime).TotalMilliseconds;
            return timeStamp;
        }
    }
}