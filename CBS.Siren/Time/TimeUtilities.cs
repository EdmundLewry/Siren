using System;

namespace CBS.Siren.Time
{
    public static class TimeUtilities
    {
        public static int FramesToSeconds(this int frameCount) => FramesToSeconds(frameCount, TimeSource.SourceFrameRate);
        //Should implement this in full sometimes
        public static int FramesToSeconds(this int frameCount, FrameRate frameRate) => frameCount / (int)Math.Round(frameRate.FrameCount());

        public static long MillisecondsToFrames(this double msCount) => MillisecondsToFrames(msCount, TimeSource.SourceFrameRate);
        public static long MillisecondsToFrames(this double msCount, FrameRate frameRate)
        {
            double msPerFrame = 1000 / frameRate.FrameCount();
            return (long)Math.Round(msCount / msPerFrame);
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

            long framesDifference = timeSpanDifference.TotalMilliseconds.MillisecondsToFrames();

            return framesDifference;
        }
    }
}
