using CBS.Siren.Device;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CBS.Siren.Test
{
    public class TransmissionListBuilderTest
    {
        private PlaylistEvent GenerateTestPlaylistEvent()
        {
            List<IEventFeature> features = new List<IEventFeature>() {
                new VideoPlaylistEventFeature(new FeaturePropertiesFactory(), new MediaInstance())
            };
            return new PlaylistEvent(features, new FixedStartEventTimingStrategy(DateTime.Now));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GivenAPlaylist_BuildTransmissionList_ReturnsAListWithTheSameCount()
        {
            PlaylistEvent event1 = GenerateTestPlaylistEvent();
            PlaylistEvent event2 = GenerateTestPlaylistEvent();
            IPlaylist playlist = new Playlist(new List<PlaylistEvent>(){event1, event2});
            TransmissionList transmissionList = TransmissionListBuilder.BuildFromPlaylist(playlist, null);

            Assert.Equal(playlist.Events.Count, transmissionList.Events.Count);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GivenAPlaylist_BuildTransmissionList_ReturnsEventsInTheSameOrder()
        {
            PlaylistEvent event1 = GenerateTestPlaylistEvent();
            PlaylistEvent event2 = GenerateTestPlaylistEvent();
            IPlaylist playlist = new Playlist(new List<PlaylistEvent>(){event1, event2});
            TransmissionList transmissionList = TransmissionListBuilder.BuildFromPlaylist(playlist, null);

            Assert.Equal(event1.Id, transmissionList.Events[0].RelatedPlaylistEvent.Id);
            Assert.Equal(event2.Id, transmissionList.Events[1].RelatedPlaylistEvent.Id);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GivenAPlaylist_BuildTransmissionList_CreatesTransmissionEventsWithTheSameFeatures()
        {
            PlaylistEvent event1 = GenerateTestPlaylistEvent();
            IPlaylist playlist = new Playlist(new List<PlaylistEvent>(){event1});
            TransmissionList transmissionList = TransmissionListBuilder.BuildFromPlaylist(playlist, null);

            Assert.True(event1.EventFeatures.SequenceEqual<IEventFeature>(transmissionList.Events[0].EventFeatures));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GivenAPlaylist_BuildTransmissionList_CreatesTransmissionEventsWithAReferenceToAPlaylistEvent()
        {
            PlaylistEvent event1 = GenerateTestPlaylistEvent();
            IPlaylist playlist = new Playlist(new List<PlaylistEvent>(){event1});
            TransmissionList transmissionList = TransmissionListBuilder.BuildFromPlaylist(playlist, null);

            Assert.Equal(event1.Id, transmissionList.Events[0].RelatedPlaylistEvent.Id);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GivenAPlaylist_BuildTransmissionList_CreatesTransmissionEventWithSameTiming()
        {
            PlaylistEvent event1 = GenerateTestPlaylistEvent();
            IPlaylist playlist = new Playlist(new List<PlaylistEvent>() { event1 });
            TransmissionList transmissionList = TransmissionListBuilder.BuildFromPlaylist(playlist, null);

            Assert.Equal(event1.EventTimingStrategy, transmissionList.Events[0].EventTimingStrategy);
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
            TransmissionList transmissionList = TransmissionListBuilder.BuildFromPlaylist(playlist, videoChain.Object);

            Assert.Equal(device.Object, transmissionList.Events[0].EventFeatures[0].Device);
        }
    }
}