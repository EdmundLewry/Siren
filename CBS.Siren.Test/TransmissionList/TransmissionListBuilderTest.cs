using CBS.Siren.Device;
using CBS.Siren.Time;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CBS.Siren.Test
{
    public class TransmissionListBuilderTest
    {
        enum TimingStrategyType{
            Fixed,
            Sequential
        }

        private PlaylistEvent GenerateTestPlaylistEvent(TimingStrategyType strategyType = TimingStrategyType.Fixed)
        {
            List<IEventFeature> features = new List<IEventFeature>() {
                new VideoPlaylistEventFeature(Guid.Empty, new FeaturePropertiesFactory(), new MediaInstance("", new TimeSpan()))
            };
            IEventTimingStrategy timingStrategy = strategyType switch
            {
                TimingStrategyType.Fixed => new FixedStartEventTimingStrategy(TimeSource.Now),
                TimingStrategyType.Sequential => new SequentialStartEventTimingStrategy(),
                _ => null
            };
            return new PlaylistEvent(features, timingStrategy);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GivenAPlaylist_BuildTransmissionList_ReturnsAListWithTheSameCount()
        {
            PlaylistEvent event1 = GenerateTestPlaylistEvent();
            PlaylistEvent event2 = GenerateTestPlaylistEvent();
            IPlaylist playlist = new Playlist(new List<PlaylistEvent>(){event1, event2});
            TransmissionList transmissionList = TransmissionListBuilder.BuildFromPlaylist(playlist, null, null);

            Assert.Equal(playlist.Events.Count, transmissionList.Events.Count);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GivenAPlaylist_BuildTransmissionList_ReturnsEventsInTheSameOrder()
        {
            PlaylistEvent event1 = GenerateTestPlaylistEvent();
            PlaylistEvent event2 = GenerateTestPlaylistEvent();
            IPlaylist playlist = new Playlist(new List<PlaylistEvent>(){event1, event2});
            TransmissionList transmissionList = TransmissionListBuilder.BuildFromPlaylist(playlist, null, null);

            Assert.Equal(event1.Id, transmissionList.Events[0].RelatedPlaylistEvent.Id);
            Assert.Equal(event2.Id, transmissionList.Events[1].RelatedPlaylistEvent.Id);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GivenAPlaylist_BuildTransmissionList_CreatesTransmissionEventsWithTheSameFeatures()
        {
            PlaylistEvent event1 = GenerateTestPlaylistEvent();
            IPlaylist playlist = new Playlist(new List<PlaylistEvent>(){event1});
            TransmissionList transmissionList = TransmissionListBuilder.BuildFromPlaylist(playlist, null, null);

            Assert.True(event1.EventFeatures.SequenceEqual<IEventFeature>(transmissionList.Events[0].EventFeatures));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GivenAPlaylist_BuildTransmissionList_CreatesTransmissionEventsWithAReferenceToAPlaylistEvent()
        {
            PlaylistEvent event1 = GenerateTestPlaylistEvent();
            IPlaylist playlist = new Playlist(new List<PlaylistEvent>(){event1});
            TransmissionList transmissionList = TransmissionListBuilder.BuildFromPlaylist(playlist, null, null);

            Assert.Equal(event1.Id, transmissionList.Events[0].RelatedPlaylistEvent.Id);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GivenAPlaylist_BuildTransmissionList_CreatesTransmissionEventWithSameTiming()
        {
            PlaylistEvent event1 = GenerateTestPlaylistEvent();
            PlaylistEvent event2 = GenerateTestPlaylistEvent(TimingStrategyType.Sequential);
            IPlaylist playlist = new Playlist(new List<PlaylistEvent>() { event1, event2 });
            TransmissionList transmissionList = TransmissionListBuilder.BuildFromPlaylist(playlist, null, null);

            Assert.Equal(event1.EventTimingStrategy, transmissionList.Events[0].EventTimingStrategy);
            Assert.Equal(event2.EventTimingStrategy, transmissionList.Events[1].EventTimingStrategy);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GivenAPlaylistAndVideoChain_BuildTransmissionList_CreatesTransmissionFeatureWithDevices()
        {
            var device = new Mock<IDevice>();
            var videoChain = new Mock<IVideoChain>();
            videoChain.Setup(mock => mock.ChainDevices).Returns(new List<IDevice>() { device.Object });

            PlaylistEvent event1 = GenerateTestPlaylistEvent();
            IPlaylist playlist = new Playlist(new List<PlaylistEvent>() { event1 });
            TransmissionList transmissionList = TransmissionListBuilder.BuildFromPlaylist(playlist, videoChain.Object, null);

            Assert.Equal(device.Object, transmissionList.Events[0].EventFeatures[0].Device);
        }
    }
}