using CBS.Siren.Data;
using System.Collections.Generic;

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