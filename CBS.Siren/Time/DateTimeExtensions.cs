using System;
using System.Text.RegularExpressions;

namespace CBS.Siren.Time
{
    public static class DateTimeExtensions
    {
        private const string TIMECODE_MATCH = @"(?<hour>[0-1][0-9]|2[0-3]):(?<minute>[0-5][0-9]):(?<second>[0-5][0-9])(?<seperator>[:;])(?<frame>[0-1][0-9]|2[0-9])";
        private const string DATE_MATCH = @"(?<year>[0-9]{4})-(?<month>0[0-9]|1[0-2])-(?<day>[0-2][0-9]|3[01])";
        private const string DATE_REGEX = "\\b"+ DATE_MATCH + "\\b";
        private const string DATETIME_REGEX = "\\b" + DATE_MATCH + "T" + TIMECODE_MATCH + "\\b";

        public static DateTime FromTimecodeString(string timecode) => FromTimecodeString(timecode, TimeSource.SourceFrameRate);
        public static DateTime FromTimecodeString(string timecode, FrameRate frameRate)
        {
            Regex timeRegex = new Regex(DATETIME_REGEX);
            Match match = timeRegex.Match(timecode);

            if(!match.Success)
            {
                timeRegex = new Regex(DATE_REGEX);
                match = timeRegex.Match(timecode);
            }

            ValidateTimeString(match, timecode, frameRate);

            int year = int.Parse(match.Groups["year"].Value);
            int month = int.Parse(match.Groups["month"].Value);
            int day = int.Parse(match.Groups["day"].Value);
            int hour = match.Groups["hour"].Success ? int.Parse(match.Groups["hour"].Value) : 0;
            int minute = match.Groups["minute"].Success ? int.Parse(match.Groups["minute"].Value) : 0;
            int second = match.Groups["second"].Success ? int.Parse(match.Groups["second"].Value) : 0;
            int millisecond = match.Groups["frame"].Success ? TimeUtilities.FramesToMiliseconds(int.Parse(match.Groups["frame"].Value), frameRate) : 0;

            return new DateTime(year, month, day, hour, minute, second, millisecond);
        }

        private static void ValidateTimeString(Match match, string timecode, FrameRate frameRate)
        {
            if(!match.Success)
            {
                throw new ArgumentException($"Timecode (Value: {timecode}) string does not match accepted format", "timecode");
            }

            Group seperator = match.Groups["seperator"];
            if (!seperator.Success)
            {
                return;
            }

            if (frameRate.IsDropFrame())
            {
                if (!seperator.Value.Equals(";"))
                    throw new ArgumentException($"Timecode (Value: {timecode}) string was expected to be dropframe, but does not include DF seperator", "timecode");
            }
            else
            {
                if (!seperator.Value.Equals(":"))
                    throw new ArgumentException($"Timecode (Value: {timecode}) string was expected not to be dropframe, but includes the DF seperator", "timecode");
            }

            int frames = int.Parse(match.Groups["frame"].Value);
            if (frames >= frameRate.FrameCount())
            {
                throw new ArgumentException($"Timecode (Value: {timecode}) frames is greater than the frame rate {Enum.GetName(typeof(FrameRate), frameRate)}", "timecode");
            }
        }

        public static string ToTimecodeString(this DateTime dateTime) => ToTimecodeString(dateTime, TimeSource.SourceFrameRate);
        public static string ToTimecodeString(this DateTime dateTime, FrameRate frameRate)
        {
            string timecode = dateTime.ToString("s");
            string seperator = frameRate.IsDropFrame() ? ";" : ":";
            string frame = TimeUtilities.MillisecondsToFrames(dateTime.Millisecond, frameRate).ToString().PadLeft(2, '0');

            return timecode + $"{seperator}{frame}";
        }
    }
}
