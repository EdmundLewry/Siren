using System;
using System.Collections.Generic;
using CBS.Siren.Time;

namespace CBS.Siren
{
    class Program
    {
        static void Main(string[] args)
        {

            /*
            1: Create a Media Instance based on a file
            2: Create Transmission Event with a source strategy that uses that media instance, which plays out as primary video and
                which has a fixed start
            3: Create a Transmission List containing that event
            4: Create a Demo Device
            5: Create a Chain Configuration which uses that device for playing events with a Media Source
            6: Create a Scheduler
            7: Create a Channel with the Transmission List, Scheduler, and the Chain Configuration
            @ This point the scheduler should generate a Channel List by triggering the event timing strategies on the events
            @ A Playout List Generation Service should then generate a Playout List (this will eventually be triggered by cue/uncue)
            @ The Device should be passed this Playlist
            @ The Device will then report playing the media at the Fixed Start Time, with the positional properties required
            */

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
                int additionalSeconds = (demoMedia.Duration / TimeSource.SYSTEM_FRAMERATE) * i;
                FixedStartEventTimingStrategy timingStrategy = new FixedStartEventTimingStrategy(startTime.AddSeconds(additionalSeconds));
                VideoPlaylistEventFeature videoFeature = new VideoPlaylistEventFeature(new FeaturePropertiesFactory(), demoMedia);
                PlaylistEvent playlistEvent = new PlaylistEvent(new List<IEventFeature>() { videoFeature }, timingStrategy);
                events.Add(playlistEvent);
            }

            return events;
        }

        private static Channel GenerateChannel(Playlist list)
        {
            //TODO:3 Will need to configure this with a list
            List<IDevice> devices = new List<IDevice>();
            devices.Add(new DemoDevice("DemoDevice1"));
            VideoChain chainConfiguration = new VideoChain(devices);

            return new Channel(chainConfiguration);
        }

        private static void PrintTransmissionListContent(TransmissionList list)
        {
            Console.WriteLine("Transmission List contains the following events:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(list.ToString());
            Console.ResetColor();
        }
        private static void DeliverPlayoutListsToDevices(Dictionary<IDevice, DeviceList> playoutLists)
        {
            throw new NotImplementedException();
        }
    }
}
