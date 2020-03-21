using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CBS.Siren.Logging;
using CBS.Siren.Time;
using NLog;

namespace CBS.Siren
{
    class Program
    {
        private static ILogger _logger = LoggingManager.GetLogger(typeof(Program).FullName);

        static void Main(string[] args)
        {
            LoggingManager.ConfigureLogging();

            _logger.Info("*** Beginning program. Generating playlist. ***");

            IDevice device = new DemoDevice("DemoDevice1");
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Thread deviceThread = new Thread(() => device.Run(cancellationTokenSource.Token));
            deviceThread.Start();

            MediaInstance demoMedia = CreateDemoMediaInstance();

            DateTime startTime = DateTime.Now.AddSeconds(30);
            List<PlaylistEvent> events = GeneratePlaylistEvents(demoMedia, startTime, 3);
            
            Playlist list = new Playlist(events);
            PrintPlaylistContent(list);

            _logger.Info("\n*** Generating Transmission List from Playlist ***\n");

            Channel channel = GenerateChannel(device);
            //TODO - Create TransmissionListService - The thing that actually works on a transmission list
            //Generate TransmissionList from playlist
            TransmissionList transmissionList = GenerateTransmissionList(list, channel.ChainConfiguration);
            PrintTransmissionListContent(transmissionList);

            _logger.Info("\n*** Generating Device Lists from Transmission List ***\n");

            SimpleScheduler scheduler = new SimpleScheduler();
            Dictionary<IDevice, DeviceList> deviceLists = scheduler.ScheduleTransmissionList(transmissionList);

            PrintDeviceListsContent(deviceLists);

            _logger.Info("*** Completed Siren Program ***");

            Task.Delay(5000).Wait();

            cancellationTokenSource.Cancel();
            deviceThread.Join();
            LoggingManager.Shutdown();
        }

        private static TransmissionList GenerateTransmissionList(Playlist list, IVideoChain videoChain)
        {
            return TransmissionListBuilder.BuildFromPlaylist(list, videoChain);
        }

        private static void PrintPlaylistContent(Playlist list)
        {
            _logger.Info("Playlist contains the following events:");
            Console.ForegroundColor = ConsoleColor.Green;
            _logger.Info(list.ToString());
            Console.ResetColor();
        }

        private static MediaInstance CreateDemoMediaInstance()
        {
            const int FPS = 25;
            const int mediaDurationSeconds = 30;
            const int secondsAsFrames = FPS * mediaDurationSeconds;
            const String mediaName = "DemoMedia1";
            const String mediaPath = "\\Media\\DemoMedia1.txt";
            return new MediaInstance(mediaName, secondsAsFrames, mediaPath, FileType.TEXT);
        }

        private static List<PlaylistEvent> GeneratePlaylistEvents(MediaInstance demoMedia, DateTime startTime, int eventCount)
        {
            List<PlaylistEvent> events = new List<PlaylistEvent>();
            for (int i = 0; i < eventCount; ++i)
            {
                int additionalSeconds = demoMedia.Duration.FramesToSeconds() * i;
                FixedStartEventTimingStrategy timingStrategy = new FixedStartEventTimingStrategy(startTime.AddSeconds(additionalSeconds));
                VideoPlaylistEventFeature videoFeature = new VideoPlaylistEventFeature(new FeaturePropertiesFactory(), demoMedia);
                PlaylistEvent playlistEvent = new PlaylistEvent(new List<IEventFeature>() { videoFeature }, timingStrategy);
                events.Add(playlistEvent);
            }

            return events;
        }

        private static void PrintTransmissionListContent(TransmissionList list)
        {
            _logger.Info("Transmission List contains the following events:");
            Console.ForegroundColor = ConsoleColor.Green;
            _logger.Info(list.ToString());
            Console.ResetColor();
        }

        private static void PrintDeviceListsContent(Dictionary<IDevice, DeviceList> deviceLists)
        {
            foreach (KeyValuePair<IDevice, DeviceList> deviceListPair in deviceLists)
            {
                _logger.Info($"Device:{deviceListPair.Key.Name} will have a Device List containing the following events:");
                Console.ForegroundColor = ConsoleColor.Green;
                _logger.Info(deviceListPair.Value.ToString());
                Console.ResetColor();
            }
        }

        private static Channel GenerateChannel(IDevice device)
        {
            //TODO:3 Will need to configure this with a list
            List<IDevice> devices = new List<IDevice>() { device };
            VideoChain chainConfiguration = new VideoChain(devices);

            return new Channel(chainConfiguration);
        }
    }
}
