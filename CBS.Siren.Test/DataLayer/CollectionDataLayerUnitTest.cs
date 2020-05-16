using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CBS.Siren.Data;
using Xunit;

namespace CBS.Siren.Test
{
    public class CollectionDataLayerUnitTests
    {
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddUpdateTransmissionLists_WhenListDoesNotExist_CreatesNewList()
        {
            TransmissionList initialList = new TransmissionList(new List<TransmissionListEvent>(), null);
            CollectionDataLayer codeUnderTest = new CollectionDataLayer();
            await codeUnderTest.AddUpdateTransmissionLists(initialList);

            TransmissionList expectedList = new TransmissionList(new List<TransmissionListEvent>(), null);
            await codeUnderTest.AddUpdateTransmissionLists(expectedList);

            List<TransmissionList> lists = (await codeUnderTest.TransmissionLists()).ToList(); 
            Assert.Equal(2, lists.Count);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddUpdateTransmissionLists_WhenListExists_UpdatesList()
        {
            TransmissionList initialList = new TransmissionList(new List<TransmissionListEvent>(), null);
            CollectionDataLayer codeUnderTest = new CollectionDataLayer();
            await codeUnderTest.AddUpdateTransmissionLists(initialList);

            TransmissionList changedList = (await codeUnderTest.TransmissionLists()).First();
            changedList.SourceList = new Playlist(new List<PlaylistEvent>());

            await codeUnderTest.AddUpdateTransmissionLists(changedList);

            List<TransmissionList> lists = (await codeUnderTest.TransmissionLists()).ToList(); 
            Assert.Single(lists);

            Assert.NotNull(lists[0].SourceList);
        }
    }
}