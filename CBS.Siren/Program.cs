using System;
using System.Collections.Generic;

namespace PBS.Siren
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

            MediaInstance demoMedia = createDemoMediaInstance();

            DateTime startTime = DateTime.Now.AddSeconds(30);
            List<PlaylistEvent> events = generatePlaylistEvents(demoMedia, startTime);
            
            Playlist list = new Playlist(events);
            PrintTransmissionListContent(list);
            
            Channel demoChannel = generateChannel(list);

            Console.WriteLine("Channel Created");
            PrintChannelListContent(demoChannel);

            Dictionary<IDevice, DeviceList> playoutLists = DeviceListGenerationService.GenerateDeviceLists(demoChannel.GeneratedList);
            DeliverPlayoutListsToDevices(playoutLists);
        }

        private static void PrintTransmissionListContent(Playlist list)
        {
            list.Events.ForEach((PlaylistEvent e) => Console.WriteLine(PlaylistEventTranslationService.TranslateToString(e)));
        }

        private static MediaInstance createDemoMediaInstance()
        {
            const int FPS = 25;
            const int mediaDurationSeconds = 30;
            const int secondsAsFrames = FPS * mediaDurationSeconds;
            const String mediaName = "DemoMedia1";
            const String mediaPath = "C:\\Media\\DemoMedia1.txt";
            return new MediaInstance(mediaName, secondsAsFrames, mediaPath, FileType.TEXT);
        }

        private static List<PlaylistEvent> generatePlaylistEvents(MediaInstance demoMedia, DateTime startTime)
        {
            MediaSourceStrategy sourceStrategy = new MediaSourceStrategy(demoMedia, 0, demoMedia.Duration);
            PrimaryVideoPlayoutStrategy playoutStrategy = new PrimaryVideoPlayoutStrategy();
            FixedStartEventTimingStrategy timingStrategy = new FixedStartEventTimingStrategy(startTime);
            PlaylistEvent PlaylistEvent = new PlaylistEvent(sourceStrategy, playoutStrategy, timingStrategy);

            List<PlaylistEvent> events = new List<PlaylistEvent>();
            events.Add(PlaylistEvent);
            return events;
        }

        private static Channel generateChannel(Playlist list)
        {
            List<IDevice> devices = new List<IDevice>();
            devices.Add(new DemoDevice("DemoDevice1"));
            PlayoutChainConfiguration chainConfiguration = new PlayoutChainConfiguration(devices);

            SimpleChannelScheduler scheduler = new SimpleChannelScheduler();

            return new Channel(chainConfiguration, list);
        }

        private static void PrintChannelListContent(Channel demoChannel)
        {
            TransmissionList list = demoChannel.GeneratedList;
            list.Events.ForEach(Console.WriteLine);
        }
        private static void DeliverPlayoutListsToDevices(Dictionary<IDevice, DeviceList> playoutLists)
        {
            throw new NotImplementedException();
        }
    }
}
