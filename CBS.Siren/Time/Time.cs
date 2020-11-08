using System;

namespace CBS.Siren.Time
{
    public static class TimeSource
    {
        public const int SOURCE_FRAMERATE = 25;
        public static FrameRate SourceFrameRate { get; set; } = FrameRate.FPS25;

        public static ITimeSourceProvider TimeProvider { get; set; } = new UtcTimeSourceProvider();

        public static DateTimeOffset Now { get => TimeProvider.Now; }
    }
}
