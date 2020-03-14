using System.Collections.Generic;

namespace CBS.Siren
{
    public static class TransmissionListBuilder
    {
        public static TransmissionList BuildFromPlaylist(IPlaylist list)
        {
            return new TransmissionList(new List<TransmissionListEvent>(), list, new SimpleChannelScheduler());
        }
    }
}