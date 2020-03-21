namespace CBS.Siren.Time
{
    public static class TimeSource
    {
        public const int SOURCE_FRAMERATE = 25;

        public static int FramesToSeconds(this int frameCount)
        {
            return frameCount / SOURCE_FRAMERATE;
        }
    }
}
