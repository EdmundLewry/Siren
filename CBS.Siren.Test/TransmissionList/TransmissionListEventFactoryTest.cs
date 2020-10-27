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
            PrimaryVideoPlayoutStrategy playoutStrategy = new PrimaryVideoPlayoutStrategy();

            TimeSpan som = TimeSpan.Zero;
            TimeSpan eom = TimeSpan.FromSeconds(30);
            TimeSpan duration = TimeSpan.FromSeconds(40);
            MediaSourceStrategy sourceStrategy = new MediaSourceStrategy(instance, som, eom);
            VideoPlaylistEventFeature expectedFeature = new VideoPlaylistEventFeature(playoutStrategy, sourceStrategy, duration);

            Assert.Single(createdEvent.EventFeatures);
            Assert.Equal(expectedFeature, createdEvent.EventFeatures[0]);
        }
    }
}