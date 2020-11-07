using CBS.Siren.Device;
using System;

namespace CBS.Siren
{
    public interface IEventFeature : IEquatable<IEventFeature>
    {
        Guid? Uid { get; set; }
        string FeatureType { get; }
        IPlayoutStrategy PlayoutStrategy { get; set; }
        ISourceStrategy SourceStrategy { get; set; }
        IDevice Device { get; set; }
        int? DeviceListEventId { get; set; }
        TimeSpan Duration { get; set; }
    }
}