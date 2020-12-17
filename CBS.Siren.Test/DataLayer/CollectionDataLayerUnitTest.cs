using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CBS.Siren.Data;
using CBS.Siren.Device;
using Xunit;

namespace CBS.Siren.Test
{
    public class CollectionDataLayerUnitTests
    {
        #region TransmissionList
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
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddUpdateTransmissionLists_WhenListExists_EnsuresAllEventsHaveAnId()
        {
            TransmissionList initialList = new TransmissionList(new List<TransmissionListEvent>(), null);
            CollectionDataLayer codeUnderTest = new CollectionDataLayer();
            await codeUnderTest.AddUpdateTransmissionLists(initialList);
            
            TransmissionList changedList = (await codeUnderTest.TransmissionLists()).First();
            changedList.Events = new List<TransmissionListEvent>() { new TransmissionListEvent(null, null), new TransmissionListEvent(null, null) };

            await codeUnderTest.AddUpdateTransmissionLists(changedList);

            List<TransmissionList> lists = (await codeUnderTest.TransmissionLists()).ToList(); 
            Assert.Single(lists);

            Assert.NotEqual(0, lists[0].Events[0].Id);
            Assert.NotEqual(0, lists[0].Events[1].Id);
        }
        #endregion

        #region Device

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddUpdateDevices_WhenDeviceDoesNotExist_CreatesNewDevice()
        {
            DeviceModel initialDevice = new DeviceModel(){ Name = "Test" };
            CollectionDataLayer codeUnderTest = new CollectionDataLayer();
            await codeUnderTest.AddUpdateDevices(initialDevice);

            DeviceModel expectedDevice = new DeviceModel() { Name = "Test2" };
            await codeUnderTest.AddUpdateDevices(expectedDevice);

            List<DeviceModel> devices = (await codeUnderTest.Devices()).ToList(); 
            Assert.Equal(2, devices.Count);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddUpdateDevices_WhenDeviceExists_UpdatesDevice()
        {
            DeviceModel initialDevice = new DeviceModel() { Name = "Test" };
            CollectionDataLayer codeUnderTest = new CollectionDataLayer();
            await codeUnderTest.AddUpdateDevices(initialDevice);
            
            string updatedName = "Updated";
            DeviceModel changedDevice = new DeviceModel() { Id = initialDevice.Id, Name = updatedName };

            await codeUnderTest.AddUpdateDevices(changedDevice);

            List<DeviceModel> devices = (await codeUnderTest.Devices()).ToList();
            Assert.Single(devices);

            Assert.Equal(updatedName, devices[0].Name);
        }

        #endregion

        #region MediaInstance
        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddUpdateMediaInstances_WhenInstanceDoesNotExist_CreatesNewInstance()
        {
            MediaInstance initialInstance = new MediaInstance("Test", new TimeSpan(0,0,30));
            CollectionDataLayer codeUnderTest = new CollectionDataLayer();
            await codeUnderTest.AddUpdateMediaInstances(initialInstance);

            MediaInstance expectedInstance = new MediaInstance("Test2", new TimeSpan(0,0,10));
            await codeUnderTest.AddUpdateMediaInstances(expectedInstance);

            List<MediaInstance> instances = (await codeUnderTest.MediaInstances()).ToList();
            Assert.Equal(2, instances.Count);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddUpdateMediaInstances_WhenInstanceExists_UpdatesInstance()
        {
            MediaInstance initialInstance = new MediaInstance("Test", new TimeSpan(0, 0, 30));
            CollectionDataLayer codeUnderTest = new CollectionDataLayer();
            await codeUnderTest.AddUpdateMediaInstances(initialInstance);

            string updatedName = "Updated";
            MediaInstance changedInstance = new MediaInstance(updatedName, initialInstance.Duration) { Id = initialInstance.Id };

            await codeUnderTest.AddUpdateMediaInstances(changedInstance);

            List<MediaInstance> instances = (await codeUnderTest.MediaInstances()).ToList();
            Assert.Single(instances);

            Assert.Equal(updatedName, instances[0].Name);
        }
        #endregion

        #region Channel

        [Fact]
        [Trait("TestType", "UnitTest")]
        public async Task AddUpdateChannel_WhenChannelDoesNotExist_CreatesNewChannel()
        {
            Channel channel = new Channel() { 
                Name = "FirstChannel"
            };
            CollectionDataLayer codeUnderTest = new CollectionDataLayer();
            _ = await codeUnderTest.AddUpdateChannels(channel);

            Channel expectedChannel = new Channel()
            {
                Name = "SecondChannel"
            };
            _ = await codeUnderTest.AddUpdateChannels(expectedChannel);

            List<Channel> channels = (await codeUnderTest.Channels()).ToList();
            Assert.Equal(2, channels.Count);
        }

        #endregion
    }
}