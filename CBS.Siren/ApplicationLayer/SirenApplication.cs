using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CBS.Siren.Device;
using CBS.Siren.Time;
using Microsoft.Extensions.Logging;

namespace CBS.Siren.Application
{
    /* This is a temporary application layer that places the domain logic together so that we can see
    how the pieces might interact. It is triggered via the Demo API controller, but both will be removed
    as the application layer is implemented.
     */
    public class SirenApplication
    {
        private readonly ILogger<SirenApplication> _logger;
        private readonly ILoggerFactory _logFactory;

        public SirenApplication(ILoggerFactory logFactory)
        {
            _logFactory = logFactory;
            _logger = _logFactory.CreateLogger<SirenApplication>();
            
        }
        public async Task RunApplication()
        {
            _logger.LogInformation("*** Beginning program. Generating playlist. ***");

            bool playoutComplete = false;
            EventHandler<DeviceStatusEventArgs> statusEventHandler = new EventHandler<DeviceStatusEventArgs>((sender, args) =>
            {
                if (args.NewStatus == IDevice.DeviceStatus.STOPPED)
                {
                    playoutComplete = true;
                }
            });

            IDeviceFactory deviceFactory = new DeviceFactory();
            IDeviceListEventStore deviceListEventStore = new DeviceListEventStore(_logFactory.CreateLogger<DeviceListEventStore>());
            using IDevice device = deviceFactory.CreateDemoDevice(new DeviceModel() { Name = "DemoDevice1" }, _logFactory, deviceListEventStore);
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

            TransmissionList transmissionList = TransmissionListBuilder.BuildFromPlaylist(list, channel.ChainConfiguration, null);
            PrintTransmissionListContent(transmissionList);

            _logger.LogInformation("\n*** Generating Device Lists from Transmission List ***\n");

            using ITransmissionListService transmissionListService = new TransmissionListService(new SimpleScheduler(), new DeviceListEventWatcher(), deviceListEventStore, _logFactory.CreateLogger<TransmissionListService>())
            {
                TransmissionList = transmissionList
            };

            transmissionListService.PlayTransmissionList();

            PrintDeviceListsContent(channel.ChainConfiguration);

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs args)
            {
                args.Cancel = true;
                cancellationTokenSource.Cancel();
            };

            while (!playoutComplete && !cancellationTokenSource.IsCancellationRequested)
            {
                await Task.Delay(1000);
            }

            if (playoutComplete)
            {
                _logger.LogInformation($"Playout of triggered list, has completed.");
            }
            _logger.LogInformation("*** Completed Siren Program ***");

            device.OnDeviceStatusChanged -= statusEventHandler;
            cancellationTokenSource.Cancel();
            deviceThread.Join();
        }

        private void PrintPlaylistContent(Playlist list)
        {
            _logger.LogInformation("Playlist contains the following events:");
            _logger.LogInformation(list.ToString());
        }

        private MediaInstance CreateDemoMediaInstance()
        {
            TimeSpan duration = TimeSpanExtensions.FromTimecodeString("00:00:05:00", FrameRate.FPS25);
            const String mediaName = "DemoMedia1";
            const String mediaPath = "\\Media\\DemoMedia1.txt";
            return new MediaInstance(mediaName, duration, mediaPath, FileType.TEXT);
        }

        private List<PlaylistEvent> GeneratePlaylistEvents(MediaInstance demoMedia, DateTime startTime, int eventCount)
        {
            List<PlaylistEvent> events = new List<PlaylistEvent>();
            FixedStartEventTimingStrategy fixedStart = new FixedStartEventTimingStrategy(startTime);
            events.Add(GeneratePlaylistEvent(fixedStart, demoMedia));
            for (int i = 0; i < eventCount; ++i)
            {                
                events.Add(GeneratePlaylistEvent(new SequentialStartEventTimingStrategy(), demoMedia));
            }

            return events;
        }

        private PlaylistEvent GeneratePlaylistEvent(IEventTimingStrategy timingStrategy, MediaInstance mediaInstance)
        {
                VideoPlaylistEventFeature videoFeature = new VideoPlaylistEventFeature(new FeaturePropertiesFactory(), mediaInstance);
                return new PlaylistEvent(new List<IEventFeature>() { videoFeature }, timingStrategy);
        }

        private void PrintTransmissionListContent(TransmissionList list)
        {
            _logger.LogInformation("Transmission List contains the following events:");
            _logger.LogInformation(list.ToString());
        }

        private void PrintDeviceListsContent(IVideoChain videoChain)
        {
            videoChain.ChainDevices.ForEach((device) =>
            {
                _logger.LogInformation($"Device:{device.Model.Name} will have a Device List containing the following events:");
                _logger.LogInformation(device.ActiveList.ToString());
            });
        }

        private Channel GenerateChannel(IDevice device)
        {
            //TODO:3 Will need to configure this with a list
            List<IDevice> devices = new List<IDevice>() { device };
            VideoChain chainConfiguration = new VideoChain(devices);

            return new Channel(chainConfiguration);
        }
    }
}