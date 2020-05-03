using System;
using System.Collections.Generic;
using System.Text;

namespace CBS.Siren.Time
{
    public static class DateTimeExtensions
    {
        public static DateTime FromTimecodeString(string timecode) => FromTimecodeString(timecode, TimeSource.SourceFrameRate);
        public static DateTime FromTimecodeString(string timecode, FrameRate frameRate)
        {
            //Validate string
            //Convert string
            //Build DT
            return DateTime.Now;
        }

        public static string ToTimecodeString(this DateTime dateTime) => ToTimecodeString(dateTime, TimeSource.SourceFrameRate);
        public static string ToTimecodeString(this DateTime dateTime, FrameRate frameRate)
        {
            //Convert to date
            //Convert to time from total frames
            return string.Empty;
        }
    }
}
