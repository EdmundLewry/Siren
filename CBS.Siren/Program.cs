using System;
using System.Collections.Generic;
using CBS.Siren.Time;

namespace CBS.Siren
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("*** Beginning program. Generating playlist. ***");
            
            MediaInstance demoMedia = CreateDemoMediaInstance();

            DateTime startTime = DateTime.Now.AddSeconds(30);
            List<PlaylistEvent> events = GeneratePlaylistEvents(demoMedia, startTime, 3);
            
            Playlist list = new Playlist(events);
            PrintPlaylistContent(list);
            
            Console.WriteLine("\n*** Generating Transmission List from Playlist ***\n");
            
            //TODO - Create TransmissionListService - The thing that actually works on a transmission list
            //Generate TransmissionList from playlist
            TransmissionList transmissionList = GenerateTransmissionList(list);
            PrintTransmissionListContent(transmissionList);

            //Dictionary<IDevice, DeviceList> playoutLists = DeviceListGenerationService.GenerateDeviceLists(demoChannel.GeneratedList);
            //DeliverPlayoutListsToDevices(playoutLists);
        }

        private static TransmissionList GenerateTransmissionList(Playlist list)
        {
            return TransmissionListBuilder.BuildFromPlaylist(list);
        }

        private static void PrintPlaylistContent(Playlist list)
        {
            Console.WriteLine("Playlist contains the following events:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(list.ToString());
            Console.ResetColor();
        }

        private static MediaInstance CreateDemoMediaInstance()
        {
            const int FPS = 25;
            const int mediaDurationSeconds = 30;
            const int secondsAsFrames = FPS * mediaDurationSeconds;
            const String mediaName = "DemoMedia1";
            const String mediaPath = "C:\\Media\\DemoMedia1.txt";
            return new MediaInstance(mediaName, secondsAsFrames, mediaPath, FileType.TEXT);
        }

        private static List<PlaylistEvent> GeneratePlaylistEvents(MediaInstance demoMedia, DateTime startTime, int eventCount)
        {
            List<PlaylistEvent> events = new List<PlaylistEvent>();
            for (int i = 0; i < eventCount; ++i)
            {
                int additionalSeconds = (demoMedia.Duration / TimeSource.SOURCE_FRAMERATE) * i;
                FixedStartEventTimingStrategy timingStrategy = new FixedStartEventTimingStrategy(startTime.AddSeconds(additionalSeconds));
                VideoPlaylistEventFeature videoFeature = new VideoPlaylistEventFeature(new FeaturePropertiesFactory(), demoMedia);
                PlaylistEvent playlistEvent = new PlaylistEvent(new List<IEventFeature>() { videoFeature }, timingStrategy);
                events.Add(playlistEvent);
            }

            return events;
        }

        private static void PrintTransmissionListContent(TransmissionList list)
        {
            Console.WriteLine("Transmission List contains the following events:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(list.ToString());
            Console.ResetColor();
        }

        private static Channel GenerateChannel(Playlist list)
        {
            //TODO:3 Will need to configure this with a list
            List<IDevice> devices = new List<IDevice>();
            devices.Add(new DemoDevice("DemoDevice1"));
            VideoChain chainConfiguration = new VideoChain(devices);

            return new Channel(chainConfiguration);
        }

        private static void DeliverPlayoutListsToDevices(Dictionary<IDevice, DeviceList> playoutLists)
        {
            throw new NotImplementedException();
        }
    }
}
