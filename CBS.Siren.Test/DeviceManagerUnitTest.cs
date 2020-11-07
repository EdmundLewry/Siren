using CBS.Siren.Data;
using CBS.Siren.Device;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CBS.Siren.Test
{
    public class DeviceManagerUnitTest
    {
        private readonly Mock<ILogger<DeviceManager>> _logger;
        private readonly Mock<ILoggerFactory> _loggerFactory;
        private readonly Mock<IDeviceFactory> _deviceFactory;
        private readonly Mock<IDeviceListEventStore> _deviceListEventStore;
        private readonly Mock<IDevice> _mockDevice;
        private readonly Mock<IDataLayer> _dataLayer;

        public DeviceManagerUnitTest()
        {
            _logger = new Mock<ILogger<DeviceManager>>();
            _loggerFactory = new Mock<ILoggerFactory>();
            _deviceFactory = new Mock<IDeviceFactory>();
            _deviceListEventStore = new Mock<IDeviceListEventStore>();
            _mockDevice = new Mock<IDevice>();
            _dataLayer = new Mock<IDataLayer>();
        }

        private DeviceManager CreateCodeUnderTest()
        {
            _deviceFactory.Setup(mock => mock.CreateDemoDevice(It.IsAny<DeviceModel>(), It.IsAny<ILoggerFactory>(), It.IsAny<IDeviceListEventStore>())).Returns(_mockDevice.Object);
            _dataLayer.Setup(mock => mock.AddUpdateDevices(It.IsAny<DeviceModel[]>())).ReturnsAsync(new List<DeviceModel>() { new DeviceModel() { Id = 0, Name = "Test" } });

            return new DeviceManager(_dataLayer.Object, _logger.Object, _loggerFactory.Object, _deviceFactory.Object, _deviceListEventStore.Object);
        }

        [Fact]
        [Trait("TestType", "UnitTest")]
        public void DeviceManager_OnConstruction_CreatesADeviceForEachDeviceModel()
        {
            List<DeviceModel> deviceModels = new List<DeviceModel>()
            {
                new DeviceModel(){ Id = 1, Name = "1" },
                new DeviceModel(){ Id = 2, Name = "2" },
                new DeviceModel(){ Id = 3, Name = "3" }
            };
            _dataLayer.Setup(mock => mock.Devices()).Returns(Task.FromResult<IEnumerable<DeviceModel>>(deviceModels));

            DeviceManager codeUnderTest = CreateCodeUnderTest();

            Assert.Equal(3, codeUnderTest.Devices.Count);
        }        

        #region Add
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void AddDevice_WhenEmptyNameProvided_ThrowsException()
        {
            DeviceManager codeUnderTest = CreateCodeUnderTest();

            Assert.ThrowsAny<ArgumentException>(() => codeUnderTest.AddDevice(""));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void AddDevice_WhenCalled_AddsDeviceToDataLayer()
        {
            DeviceManager codeUnderTest = CreateCodeUnderTest();

            codeUnderTest.AddDevice("Test");
            
            _dataLayer.Verify(mock => mock.AddUpdateDevices(It.IsAny<DeviceModel[]>()), Times.Once);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void AddDevice_WhenCalledWithNoProperties_AddsDeviceToDataLayerWithDefaultProperties()
        {
            DeviceManager codeUnderTest = CreateCodeUnderTest();
            DeviceModel deviceModel = null;
            _dataLayer.Setup(mock => mock.AddUpdateDevices(It.IsAny<DeviceModel[]>()))
                            .ReturnsAsync(new List<DeviceModel>() { new DeviceModel() { Id = 0, Name = "Test" } })
                            .Callback((DeviceModel[] arg) => deviceModel = arg[0]);

            codeUnderTest.AddDevice("Test");

            Assert.Equal(TimeSpan.Zero, deviceModel.DeviceProperties.Preroll);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void AddDevice_WhenCalledWithProperties_AddsDeviceToDataLayerWithGivenProperties()
        {
            DeviceManager codeUnderTest = CreateCodeUnderTest();
            DeviceModel deviceModel = null;
            _dataLayer.Setup(mock => mock.AddUpdateDevices(It.IsAny<DeviceModel[]>()))
                            .ReturnsAsync(new List<DeviceModel>() { new DeviceModel() { Id = 0, Name = "Test" } })
                            .Callback((DeviceModel[] arg) => deviceModel = arg[0]);

            DeviceProperties deviceProperties = new DeviceProperties() { Preroll = TimeSpan.FromSeconds(59)};
            codeUnderTest.AddDevice("Test", deviceProperties);

            Assert.Equal(TimeSpan.FromSeconds(59), deviceModel.DeviceProperties.Preroll);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void AddDevice_WhenCalled_StartDeviceProcess()
        {
            string name = "Test";

            _mockDevice.Setup(mock => mock.Model).Returns(new DeviceModel() { Name = name });

            DeviceManager codeUnderTest = CreateCodeUnderTest();

            codeUnderTest.AddDevice(name);

            Assert.Single(codeUnderTest.Devices);
            Assert.Equal(name, codeUnderTest.Devices[0].Device.Model.Name);
        }
        #endregion

        #region Get
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GetDevice_WhenIdDoesntExist_ThrowsException()
        {
            DeviceManager codeUnderTest = CreateCodeUnderTest();

            Assert.ThrowsAny<ArgumentException>(() => codeUnderTest.GetDevice(100));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GetDevice_WhenIdExists_ReturnsDevice()
        {
            string expectedName = "Test";
            TimeSpan expectedPreroll = TimeSpan.FromSeconds(40);

            DeviceModel deviceModel = new DeviceModel() { Id = 1, Name = expectedName, DeviceProperties = new DeviceProperties() { Preroll = expectedPreroll } };
            _dataLayer.Setup(mock => mock.Devices()).Returns(Task.FromResult<IEnumerable<DeviceModel>>(new List<DeviceModel>() { deviceModel }));

            _mockDevice.Setup(mock => mock.Model).Returns(deviceModel);

            DeviceManager codeUnderTest = CreateCodeUnderTest();

            IDevice device = codeUnderTest.GetDevice(1);
            Assert.Equal(expectedName, device.Model.Name);
            Assert.Equal(expectedPreroll, device.Model.DeviceProperties.Preroll);
        }
        #endregion
    }
}
