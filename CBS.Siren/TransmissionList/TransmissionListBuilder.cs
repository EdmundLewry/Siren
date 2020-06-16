using CBS.Siren.Data;
using CBS.Siren.Device;
using System.Collections.Generic;
using System.Linq;

namespace CBS.Siren
{
    public static class TransmissionListBuilder
    {
        public static TransmissionList BuildFromPlaylist(IPlaylist list, IVideoChain videoChain, IDataLayer dataLayer)
        {
            List<TransmissionListEvent> createdEvents = new List<TransmissionListEvent>();
            list.Events.ForEach((playlistEvent) => createdEvents.Add(TransmissionListEventFactory.BuildTransmissionListEvent(playlistEvent, videoChain, dataLayer)));

            return new TransmissionList(createdEvents, list);
        }
    }
}