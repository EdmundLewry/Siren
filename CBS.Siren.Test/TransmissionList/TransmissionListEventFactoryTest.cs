using System.Collections.Generic;
using CBS.Siren.DTO;
using CBS.Siren.Time;
using System;
using Xunit;
using Moq;
using CBS.Siren.Data;
using CBS.Siren.Device;

namespace CBS.Siren.Test
{
    public class TransmissionListEventFactoryTest
    {
        private PlaylistEvent GeneratePlaylistEvent(IEventTimingStrategy timingStrategy, MediaInstance mediaInstance)
        {
            VideoPlaylistEventFeature videoFeature = new VideoPlaylistEventFeature(Guid.Empty, new FeaturePropertiesFactory(), mediaInstance);
            return new PlaylistEvent(new List<IEventFeature>() { videoFeature }, timingStrategy);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void BuildTransmissionListEvent_WithFixedTimingData_CreatesEventWithFixedTiming()
        {
            DateTimeOffset targetStartTime = DateTimeOffset.Parse("2020-03-22 00:00:10");
            TimingStrategyUpsertDTO timingData = new TimingStrategyUpsertDTO(){
                StrategyType = "fixed",
                TargetStartTime = targetStartTime.ToTimecodeString()
            };

            TransmissionListEvent createdEvent = TransmissionListEventFactory.BuildTransmissionListEvent(timingData, new List<ListEventFeatureUpsertDTO>(), null, new Mock<IDataLayer>().Object);

            FixedStartEventTimingStrategy expectedStrategy = new FixedStartEventTimingStrategy(targetStartTime);
            Assert.Equal(expectedStrategy, createdEvent.EventTimingStrategy);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void BuildTransmissionListEvent_WithSequentialTimingData_CreatesEventWithSequentialTiming()
        {
            TimingStrategyUpsertDTO timingData = new TimingStrategyUpsertDTO(){
                StrategyType = "sequential"
            };

            TransmissionListEvent createdEvent = TransmissionListEventFactory.BuildTransmissionListEvent(timingData, new List<ListEventFeatureUpsertDTO>(), null, new Mock<IDataLayer>().Object);

            SequentialStartEventTimingStrategy expectedStrategy = new SequentialStartEventTimingStrategy();
            Assert.Equal(expectedStrategy, createdEvent.EventTimingStrategy);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void BuildTransmissionListEvent_WithVideoFeature_CreatesEventWithVideoFeature()
        {
            Guid uid = Guid.NewGuid();

            ListEventFeatureUpsertDTO featureData = new ListEventFeatureUpsertDTO(){
                Uid = uid.ToString(),
                FeatureType = "video",
                PlayoutStrategy = new PlayoutStrategyCreationDTO() { StrategyType = "primaryVideo" },
                SourceStrategy = new SourceStrategyCreationDTO() {
                    StrategyType = "mediaSource",
                    SOM = "00:00:00:00",
                    EOM = "00:00:30:00",
                    MediaName = "TestInstance"
                },
                Duration = "00:00:40:00"
            };

            var mockVideoChain = new Mock<IVideoChain>();
            mockVideoChain.Setup(mock => mock.ChainDevices).Returns(new List<IDevice>(){new Mock<IDevice>().Object});

            MediaInstance instance = new MediaInstance("TestInstance", TimeSpan.FromSeconds(30));
            var mockDataLayer = new Mock<IDataLayer>();
            mockDataLayer.Setup(mock => mock.MediaInstances()).ReturnsAsync(new List<MediaInstance>(){instance});

            TransmissionListEvent createdEvent = TransmissionListEventFactory.BuildTransmissionListEvent(new TimingStrategyUpsertDTO(), 
                                                                                                        new List<ListEventFeatureUpsertDTO>(){featureData}, 
                                                                                                        mockVideoChain.Object,
                                                                                                        mockDataLayer.Object);
            PrimaryVideoPlayoutStrategy playoutStrategy = new PrimaryVideoPlayoutStrategy();

            TimeSpan som = TimeSpan.Zero;
            TimeSpan eom = TimeSpan.FromSeconds(30);
            TimeSpan duration = TimeSpan.FromSeconds(40);
            MediaSourceStrategy sourceStrategy = new MediaSourceStrategy(instance, som, eom);
            VideoPlaylistEventFeature expectedFeature = new VideoPlaylistEventFeature(uid, playoutStrategy, sourceStrategy, duration);

            Assert.Single(createdEvent.EventFeatures);
            Assert.Equal(expectedFeature, createdEvent.EventFeatures[0]);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void BuildTransmissionListEvent_WithFeatureWithNoUid_CreatesFeatureWithNewUid()
        {
            ListEventFeatureUpsertDTO featureData = new ListEventFeatureUpsertDTO(){
                FeatureType = "video",
                PlayoutStrategy = new PlayoutStrategyCreationDTO() { StrategyType = "primaryVideo" },
                SourceStrategy = new SourceStrategyCreationDTO() {
                    StrategyType = "mediaSource",
                    SOM = "00:00:00:00",
                    EOM = "00:00:30:00",
                    MediaName = "TestInstance"
                },
                Duration = "00:00:30:00"
            };

            var mockVideoChain = new Mock<IVideoChain>();
            mockVideoChain.Setup(mock => mock.ChainDevices).Returns(new List<IDevice>(){new Mock<IDevice>().Object});

            MediaInstance instance = new MediaInstance("TestInstance", TimeSpan.FromSeconds(30));
            var mockDataLayer = new Mock<IDataLayer>();
            mockDataLayer.Setup(mock => mock.MediaInstances()).ReturnsAsync(new List<MediaInstance>(){instance});

            TransmissionListEvent createdEvent = TransmissionListEventFactory.BuildTransmissionListEvent(new TimingStrategyUpsertDTO(), 
                                                                                                        new List<ListEventFeatureUpsertDTO>(){featureData}, 
                                                                                                        mockVideoChain.Object,
                                                                                                        mockDataLayer.Object);

            Assert.Single(createdEvent.EventFeatures);
            Assert.NotNull(createdEvent.EventFeatures[0].Uid);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void BuildTransmissionListEvent_FromPlaylistEvent_WithFixedTimingData_CreatesEventWithFixedTiming()
        {
            DateTimeOffset targetStartTime = DateTimeOffset.Parse("2020-03-22 00:00:10");
            FixedStartEventTimingStrategy fixedStart = new FixedStartEventTimingStrategy(targetStartTime);
            MediaInstance instance = new MediaInstance("TestInstance", TimeSpan.FromSeconds(30));
            PlaylistEvent playlistEvent = GeneratePlaylistEvent(fixedStart, instance);

            TransmissionListEvent createdEvent = TransmissionListEventFactory.BuildTransmissionListEvent(playlistEvent, null, new Mock<IDataLayer>().Object);

            FixedStartEventTimingStrategy expectedStrategy = new FixedStartEventTimingStrategy(targetStartTime);
            Assert.Equal(expectedStrategy, createdEvent.EventTimingStrategy);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void BuildTransmissionListEvent_FromPlaylistEvent_WithSequentialTimingData_CreatesEventWithSequentialTiming()
        {
            SequentialStartEventTimingStrategy sequentialStart = new SequentialStartEventTimingStrategy();
            MediaInstance instance = new MediaInstance("TestInstance", TimeSpan.FromSeconds(30));
            PlaylistEvent playlistEvent = GeneratePlaylistEvent(sequentialStart, instance);

            TransmissionListEvent createdEvent = TransmissionListEventFactory.BuildTransmissionListEvent(playlistEvent, null, new Mock<IDataLayer>().Object);

            SequentialStartEventTimingStrategy expectedStrategy = new SequentialStartEventTimingStrategy();
            Assert.Equal(expectedStrategy, createdEvent.EventTimingStrategy);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void BuildTransmissionListEvent_FromPlaylistEvent_WithVideoFeature_CreatesEventWithVideoFeature()
        {
            var mockVideoChain = new Mock<IVideoChain>();
            mockVideoChain.Setup(mock => mock.ChainDevices).Returns(new List<IDevice>() { new Mock<IDevice>().Object });

            MediaInstance instance = new MediaInstance("TestInstance", TimeSpan.FromSeconds(30));
            var mockDataLayer = new Mock<IDataLayer>();
            mockDataLayer.Setup(mock => mock.MediaInstances()).ReturnsAsync(new List<MediaInstance>() { instance });

            SequentialStartEventTimingStrategy sequentialStart = new SequentialStartEventTimingStrategy();
            PlaylistEvent playlistEvent = GeneratePlaylistEvent(sequentialStart, instance);
            Guid uid = Guid.NewGuid();
            playlistEvent.EventFeatures[0].Uid = uid;
            playlistEvent.EventFeatures[0].Duration = TimeSpan.FromSeconds(40);

            TransmissionListEvent createdEvent = TransmissionListEventFactory.BuildTransmissionListEvent(playlistEvent,
                                                                                                        mockVideoChain.Object,
                                                                                                        mockDataLayer.Object);
            PrimaryVideoPlayoutStrategy playoutStrategy = new PrimaryVideoPlayoutStrategy();

            TimeSpan som = TimeSpan.Zero;
            TimeSpan eom = TimeSpan.FromSeconds(30);
            TimeSpan duration = TimeSpan.FromSeconds(40);
            MediaSourceStrategy sourceStrategy = new MediaSourceStrategy(instance, som, eom);
            VideoPlaylistEventFeature expectedFeature = new VideoPlaylistEventFeature(uid, playoutStrategy, sourceStrategy, duration);

            Assert.Single(createdEvent.EventFeatures);
            Assert.Equal(expectedFeature, createdEvent.EventFeatures[0]);
        }
    }
}