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
            decimal msPerFrame = 1000 / (decimal)frameRate.FrameCount();
            decimal frames = (decimal)msCount / msPerFrame;
            return (long)Math.Floor(frames);
        }

        public static long FramesToMiliseconds(long frames) => FramesToMiliseconds(frames, TimeSource.SourceFrameRate);
        public static long FramesToMiliseconds(long frames, FrameRate frameRate)
        {
            double frameRateCount = frameRate.FrameCount();
            long totalFrames = frames;

            if (frameRate.IsDropFrame())
            {
                double droppedFramesPerMinute = Math.Round(frameRateCount * 0.06, 1);

                double framesPerMinute = frameRateCount * 60 - droppedFramesPerMinute;
                int frameCountPerMinute = (int)Math.Round(framesPerMinute);
                int frameCountPer10Minutes = (int)Math.Round(framesPerMinute * 10);

                droppedFramesPerMinute = Math.Round(droppedFramesPerMinute);

                long tenMinuteCount = totalFrames / frameCountPer10Minutes;
                long framesOver10Minutes = totalFrames % frameCountPer10Minutes;

                totalFrames += (long)(droppedFramesPerMinute * 9 * tenMinuteCount);

                if(framesOver10Minutes > frameCountPerMinute)
                {
                    totalFrames += (long)(droppedFramesPerMinute * Math.Floor((framesOver10Minutes - droppedFramesPerMinute) / frameCountPerMinute));
                }
            }

            double milliseconds = (totalFrames * 1000) / frameRateCount;
            return (long)Math.Round(milliseconds);
        }

        public static long DifferenceInFrames(this DateTimeOffset lhs, DateTimeOffset rhs)
        {
            long ticksDifference = rhs.Ticks - lhs.Ticks;
            TimeSpan timeSpanDifference = new TimeSpan(ticksDifference);

            long framesDifference = timeSpanDifference.TotalMilliseconds.MillisecondsToFrames();

            return framesDifference;
        }
    }
}
