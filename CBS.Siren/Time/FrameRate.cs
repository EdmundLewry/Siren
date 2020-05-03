using System;

namespace CBS.Siren.Time
{
    public enum FrameRate
    {
        FPS24,
        FPS25,
        DF30,
        FPS30
    }

    public static class FrameRateExtensions
    {
        public static double FrameCount(this FrameRate frameRate)
        {
            return frameRate switch
            {
                FrameRate.FPS24 => 24,
                FrameRate.FPS25 => 25,
                FrameRate.DF30 => 29.97,
                FrameRate.FPS30 => 30,
                _ => throw new ArgumentException("Invalid framerate")
            };
        }

        public static bool IsDropFrame(this FrameRate frameRate)
        {
            switch(frameRate)
            {
                case FrameRate.FPS24:
                case FrameRate.FPS25:
                case FrameRate.FPS30:
                    return false;
                case FrameRate.DF30:
                    return true;
            }

            throw new ArgumentException("Invalid framerate");
        }
    }
}
