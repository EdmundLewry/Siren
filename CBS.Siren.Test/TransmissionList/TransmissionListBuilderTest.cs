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
            TransmissionList transmissionList = TransmissionListBuilder.BuildFromPlaylist(playlist);

            Assert.Equal(playlist.Events.Count, transmissionList.Events.Count);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GivenAPlaylist_BuildTransmissionList_ReturnsEventsInTheSameOrder()
        {
            PlaylistEvent event1 = GenerateTestPlaylistEvent();
            PlaylistEvent event2 = GenerateTestPlaylistEvent();
            IPlaylist playlist = new Playlist(new List<PlaylistEvent>(){event1, event2});
            TransmissionList transmissionList = TransmissionListBuilder.BuildFromPlaylist(playlist);

            Assert.Equal(event1.Id, transmissionList.Events[0].RelatedPlaylistEvent.Id);
            Assert.Equal(event2.Id, transmissionList.Events[1].RelatedPlaylistEvent.Id);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GivenAPlaylist_BuildTransmissionList_CreatesTransmissionEventsWithTheSameFeatures()
        {
            PlaylistEvent event1 = GenerateTestPlaylistEvent();
            IPlaylist playlist = new Playlist(new List<PlaylistEvent>(){event1});
            TransmissionList transmissionList = TransmissionListBuilder.BuildFromPlaylist(playlist);

            Assert.True(event1.EventFeatures.SequenceEqual<IEventFeature>(transmissionList.Events[0].EventFeatures));
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GivenAPlaylist_BuildTransmissionList_CreatesTransmissionEventsWithAReferenceToAPlaylistEvent()
        {
            PlaylistEvent event1 = GenerateTestPlaylistEvent();
            IPlaylist playlist = new Playlist(new List<PlaylistEvent>(){event1});
            TransmissionList transmissionList = TransmissionListBuilder.BuildFromPlaylist(playlist);

            Assert.Equal(event1.Id, transmissionList.Events[0].RelatedPlaylistEvent.Id);
        }
    }
}