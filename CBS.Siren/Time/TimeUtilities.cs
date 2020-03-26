using System;

namespace CBS.Siren.Time
{
    public static class TimeUtilities
    {
        public static int FramesToSeconds(this int frameCount) => frameCount / TimeSource.SOURCE_FRAMERATE;
        public static long MsToFrames(this double msCount)
        {
            int msPerFrame = 1000 / TimeSource.SOURCE_FRAMERATE; //Doesn't account for drop frame
            return (long)msCount / msPerFrame;
        }

        public static long DifferenceInFrames(this DateTime lhs, DateTime rhs)
        {
            long ticksDifference = rhs.Ticks - lhs.Ticks;
            TimeSpan timeSpanDifference = new TimeSpan(ticksDifference);

            double framesDifference = timeSpanDifference.TotalMilliseconds.MsToFrames();

            return (long)framesDifference;
        }
    }
}
