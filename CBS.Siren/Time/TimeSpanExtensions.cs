using System;
using System.Text.RegularExpressions;

namespace CBS.Siren.Time
{
    public static class TimeSpanExtensions
    {
        private const string TIMECODE_REGEX = @"\b(?:(?<days>[0-9]{2,3}):)?(?<hours>[0-1][0-9]|2[0-3]):(?<minutes>[0-5][0-9]):(?<seconds>[0-5][0-9])(?<seperator>[:;])(?<frames>[0-1][0-9]|2[0-9])\b";
        public static TimeSpan FromTimecodeString(string timecode) => FromTimecodeString(timecode, TimeSource.SourceFrameRate);
        public static TimeSpan FromTimecodeString(string timecode, FrameRate frameRate)
        {
            Regex timeRegex = new Regex(TIMECODE_REGEX);
            Match match = timeRegex.Match(timecode);

            ValidateTimeString(match, timecode, frameRate);

            bool hasDays = match.Groups["days"].Success;

            int days = hasDays ? int.Parse(match.Groups["days"].Value) : 0;
            int hours = int.Parse(match.Groups["hours"].Value);
            int minutes = int.Parse(match.Groups["minutes"].Value);
            int seconds = int.Parse(match.Groups["seconds"].Value);
            int milliseconds = TimeUtilities.FramesToMiliseconds(int.Parse(match.Groups["frames"].Value), frameRate);

            return new TimeSpan(days, hours, minutes, seconds, milliseconds);
        }

        private static void ValidateTimeString(Match match, string timecode, FrameRate frameRate)
        {
            if (!match.Success)
            {
                throw new ArgumentException($"Timecode (Value: {timecode}) string does not match accepted format", "timecode");
            }

            if(frameRate.IsDropFrame())
            {
                if(!match.Groups["seperator"].Value.Equals(";"))
                    throw new ArgumentException($"Timecode (Value: {timecode}) string was expected to be dropframe, but does not include DF seperator", "timecode");
            }
            else
            {
                if (!match.Groups["seperator"].Value.Equals(":"))
                    throw new ArgumentException($"Timecode (Value: {timecode}) string was expected not to be dropframe, but includes the DF seperator", "timecode");
            }

            int frames = int.Parse(match.Groups["frames"].Value);
            if (frames >= frameRate.FrameCount())
            {
                throw new ArgumentException($"Timecode (Value: {timecode}) frames is greater than the frame rate {Enum.GetName(typeof(FrameRate), frameRate)}", "timecode");
            }
        }

        public static string ToTimecodeString(this TimeSpan timeSpan) => ToTimecodeString(timeSpan, TimeSource.SourceFrameRate);
        public static string ToTimecodeString(this TimeSpan timeSpan, FrameRate frameRate)
        {
            string timecode = timeSpan.Days > 0 ? $"{timeSpan.Days.ToString().PadLeft(3,'0')}:" : "";
            
            string hours = timeSpan.Hours.ToString().PadLeft(2,'0');
            string minutes = timeSpan.Minutes.ToString().PadLeft(2,'0');
            string seconds = timeSpan.Seconds.ToString().PadLeft(2,'0');
            string seperator = frameRate.IsDropFrame() ? ";" : ":";
            string frames = TimeUtilities.MillisecondsToFrames(timeSpan.Milliseconds, frameRate).ToString().PadLeft(2,'0');

            return timecode + $"{hours}:{minutes}:{seconds}{seperator}{frames}";
        }

        public static long TotalFrames(this TimeSpan timeSpan) => TotalFrames(timeSpan, TimeSource.SourceFrameRate);
        public static long TotalFrames(this TimeSpan timeSpan, FrameRate frameRate)
        {
            return timeSpan.TotalMilliseconds.MillisecondsToFrames(frameRate);
        }
    }
}
