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

        public static int FramesToMiliseconds(int frames, FrameRate frameRate)
        {
            double frameRateCount = frameRate.FrameCount();
            int totalFrames = frames;

            if (frameRate.IsDropFrame())
            {
                double droppedFramesPerMinute = Math.Round(frameRateCount * 0.06, 1);

                double framesPerMinute = frameRateCount * 60 - droppedFramesPerMinute;
                int frameCountPerMinute = (int)Math.Round(framesPerMinute);
                int frameCountPer10Minutes = (int)Math.Round(framesPerMinute * 10);

                droppedFramesPerMinute = Math.Round(droppedFramesPerMinute);

                int tenMinuteCount = totalFrames / frameCountPer10Minutes;
                int framesOver10Minutes = totalFrames % frameCountPer10Minutes;

                totalFrames += (int)(droppedFramesPerMinute * 9 * tenMinuteCount);

                if(framesOver10Minutes > frameCountPerMinute)
                {
                    totalFrames += (int)(droppedFramesPerMinute * Math.Floor((framesOver10Minutes - droppedFramesPerMinute) / frameCountPerMinute));
                }
            }

            double milliseconds = (totalFrames * 1000) / frameRateCount;
            return (int)Math.Round(milliseconds);
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
