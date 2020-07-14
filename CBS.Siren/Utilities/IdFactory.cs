/*
This is just a centralized implementation for the system to use before we make use of entity framework.
It allows us to centralize the id generation for the in memory collection objects.
*/
namespace CBS.Siren.Utilities
{
    public static class IdFactory
    {
        private static int _nextTransmissionListEventId = 0;
        private static int _nextDeviceListEventId = 0;
        private static int _nextPlaylistEventId = 0;

        public static int NextTransmissionListEventId() { return ++_nextTransmissionListEventId; }
        public static int NextDeviceListEventId() { return ++_nextDeviceListEventId; }
        public static int NextPlaylistEventId() { return ++_nextPlaylistEventId; }
    }
}
