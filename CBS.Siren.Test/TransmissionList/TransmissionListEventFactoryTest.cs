using System.Collections.Generic;
using System.Text.Json;
using CBS.Siren.DTO;
using CBS.Siren.Time;
using CBS.Siren.Utilities;
using System;
using Xunit;

namespace CBS.Siren.Test
{
    public class TransmissionListEventFactoryTest
    {
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void BuildTransmissionListEvent_WithFixedTimingData_CreatesEventWithFixedTiming()
        {
            TimingStrategyCreationDTO timingData = new TimingStrategyCreationDTO(){
                StrategyType = "fixed",
                TargetStartTime = "2020-03-22T00:00:10:05"
            };

            TransmissionListEvent createdEvent = TransmissionListEventFactory.BuildTransmissionListEvent(timingData, new List<ListEventFeatureCreationDTO>(), null);

            FixedStartEventTimingStrategy expectedStrategy = new FixedStartEventTimingStrategy(DateTimeExtensions.FromTimecodeString("2020-03-22T00:00:10:05"));
            Assert.Equal(expectedStrategy, createdEvent.EventTimingStrategy);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void BuildTransmissionListEvent_WithSequentialTimingData_CreatesEventWithSequentialTiming()
        {
            TimingStrategyCreationDTO timingData = new TimingStrategyCreationDTO(){
                StrategyType = "sequential"
            };

            TransmissionListEvent createdEvent = TransmissionListEventFactory.BuildTransmissionListEvent(timingData, new List<ListEventFeatureCreationDTO>(), null);

            SequentialStartEventTimingStrategy expectedStrategy = new SequentialStartEventTimingStrategy();
            Assert.Equal(expectedStrategy, createdEvent.EventTimingStrategy);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void BuildTransmissionListEvent_WithVideoFeature_CreatesEventWithVideoFeature()
        {
            ListEventFeatureCreationDTO featureData = new ListEventFeatureCreationDTO(){
                FeatureType = "video",
                PlayoutStrategy = new PlayoutStrategyCreationDTO() { StrategyType = "primaryVideo" },
                SourceStrategy = new SourceStrategyCreationDTO() {
                    StrategyType = "mediaSource",
                    SOM = new TimeSpan(0,0,0,0,0),
                    EOM = new TimeSpan(0,0,0,30,0),
                    MediaInstanceId = 0
                }
            };

            TransmissionListEvent createdEvent = TransmissionListEventFactory.BuildTransmissionListEvent(new TimingStrategyCreationDTO(), 
                                                                                                        new List<ListEventFeatureCreationDTO>(){featureData}, 
                                                                                                        null);
            PrimaryVideoPlayoutStrategy playoutStrategy = new PrimaryVideoPlayoutStrategy();

            MediaInstance instance = new MediaInstance("TestInstance", TimeSpanExtensions.FromTimecodeString("00:00:30:00"));
            TimeSpan som = TimeSpanExtensions.FromTimecodeString("00:00:00:00");
            TimeSpan eom = TimeSpanExtensions.FromTimecodeString("00:00:30:00");
            MediaSourceStrategy sourceStrategy = new MediaSourceStrategy(instance, som, eom);
            VideoPlaylistEventFeature expectedFeature = new VideoPlaylistEventFeature(playoutStrategy, sourceStrategy);

            Assert.Single(createdEvent.EventFeatures);
            Assert.Equal(expectedFeature, createdEvent.EventFeatures[0]);
        }
    }
}