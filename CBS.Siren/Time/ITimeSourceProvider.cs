using System;

namespace CBS.Siren.Time
{
    public interface ITimeSourceProvider
    {
        public DateTimeOffset Now { get; }
    }
}