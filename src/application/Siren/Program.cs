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

            const int FPS = 25;
            const int mediaDurationSeconds = 30;
            const int secondsAsFrames = FPS * mediaDurationSeconds;
            const String mediaName = "DemoMedia1";
            const String mediaPath = "C:\\Media\\DemoMedia1.txt";
            MediaInstance instance = new MediaInstance(mediaName, secondsAsFrames, mediaPath, FileType.TEXT);

            MediaSourceStrategy sourceStrategy = new MediaSourceStrategy();
            PrimaryVideoPlayoutStrategy playoutStrategy = new PrimaryVideoPlayoutStrategy();
            FixedStartEventTimingStrategy timingStrategy = new FixedStartEventTimingStrategy();
            TransmissionEvent transmissionEvent = new TransmissionEvent(sourceStrategy, playoutStrategy, timingStrategy);

            List<TransmissionEvent> events = new List<TransmissionEvent>();
            events.Add(transmissionEvent);

            TransmissionList list = new TransmissionList(events);

            DemoDevice demoDevice = new DemoDevice();
            PlayoutChainConfiguration chainConfiguration = new PlayoutChainConfiguration();

            SimpleChannelScheduler scheduler = new SimpleChannelScheduler();

            Channel demoChannel = new Channel(chainConfiguration, list, scheduler);

            Console.WriteLine("Channel Created");
            PrintChannelListContent(demoChannel);
        }

        private static void PrintChannelListContent(Channel demoChannel)
        {
            ChannelList list = demoChannel.GeneratedList;
            list.Events.ForEach(Console.WriteLine);
        }
    }
}
