using System;

namespace CBS.Siren.Time
{
    public class UtcTimeSourceProvider : ITimeSourceProvider
    {
        public DateTimeOffset Now => DateTimeOffset.UtcNow;
    }
}