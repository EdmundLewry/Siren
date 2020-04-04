using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CBS.Siren.Device;
using CBS.Siren.Logging;
using CBS.Siren.Time;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace CBS.Siren
{
    class Program
    {
        private static ILogger _logger;

        protected Program()
        {

        }

        static void Main(string[] args)
        {
            LoggingManager.ConfigureLogging();

            ILoggerFactory logFactory = LoggerFactory.Create((builder) => builder.AddNLog());
            _logger = logFactory.CreateLogger<Program>();

            _logger.LogInformation("*** Beginning program. Generating playlist. ***");

            bool playoutComplete = false;
            EventHandler<DeviceStatusEventArgs> statusEventHandler = new EventHandler<DeviceStatusEventArgs>((sender, args) => {
                if(args.NewStatus == IDevice.DeviceStatus.STOPPED)
                {
                    playoutComplete = true;
                }
            });

            IDeviceFactory deviceFactory = new DeviceFactory();
            using IDevice device = deviceFactory.CreateDemoDevice("DemoDevice1", logFactory);
            device.OnDeviceStatusChanged += statusEventHandler;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Thread deviceThread = new Thread(async () => await device.Run(cancellationTokenSource.Token));
            deviceThread.Start();

            MediaInstance demoMedia = CreateDemoMediaInstance();

            DateTime startTime = DateTime.Now.AddSeconds(3);
            List<PlaylistEvent> events = GeneratePlaylistEvents(demoMedia, startTime, 3);
            
            Playlist list = new Playlist(events);
            PrintPlaylistContent(list);

            _logger.LogInformation("\n*** Generating Transmission List from Playlist ***\n");

            Channel channel = GenerateChannel(device);

            TransmissionListService transmissionListService = new TransmissionListService(logFactory.CreateLogger<TransmissionListService>());
            
            //Generate TransmissionList from playlist
            TransmissionList transmissionList = GenerateTransmissionList(list, channel.ChainConfiguration);
            PrintTransmissionListContent(transmissionList);

            _logger.LogInformation("\n*** Generating Device Lists from Transmission List ***\n");

            SimpleScheduler scheduler = new SimpleScheduler();
            Dictionary<IDevice, DeviceList> deviceLists = scheduler.ScheduleTransmissionList(transmissionList);

            PrintDeviceListsContent(deviceLists);

            DeliverDeviceLists(deviceLists);

            Task inputTask = Task.Run(() => Console.ReadLine());
            while(!playoutComplete && !inputTask.IsCompleted)
            {
                Task.Delay(1000).Wait();
            }

            _logger.LogInformation("*** Completed Siren Program ***");

            device.OnDeviceStatusChanged -= statusEventHandler;
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
            _logger.LogInformation("Playlist contains the following events:");
            Console.ForegroundColor = ConsoleColor.Green;
            _logger.LogInformation(list.ToString());
            Console.ResetColor();
        }

        private static MediaInstance CreateDemoMediaInstance()
        {
            const int FPS = 25;
            const int mediaDurationSeconds = 5;
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
            _logger.LogInformation("Transmission List contains the following events:");
            Console.ForegroundColor = ConsoleColor.Green;
            _logger.LogInformation(list.ToString());
            Console.ResetColor();
        }

        private static void PrintDeviceListsContent(Dictionary<IDevice, DeviceList> deviceLists)
        {
            foreach (KeyValuePair<IDevice, DeviceList> deviceListPair in deviceLists)
            {
                _logger.LogInformation($"Device:{deviceListPair.Key.Name} will have a Device List containing the following events:");
                Console.ForegroundColor = ConsoleColor.Green;
                _logger.LogInformation(deviceListPair.Value.ToString());
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

        private static void DeliverDeviceLists(Dictionary<IDevice, DeviceList> deviceLists)
        {
            deviceLists.ToList().ForEach((pair) => pair.Key.SetDeviceList(pair.Value));
        }
    }
}
