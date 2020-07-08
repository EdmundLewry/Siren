using CBS.Siren.Data;
using CBS.Siren.Device;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace CBS.Siren.Test
{
    public class DeviceManagerUnitTest
    {
        //DeviceManager on creation should create a device process for each device in the data layer
        //DeviceManager on destruction should end all device processes
        //GetDeviceById when id doesn't exist should throw an exception
        //GetDeviceById when id exists, should return a valid Device Object
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void DeviceManager_OnConstruction_CreatesADeviceForEachDeviceModel()
        {
        }

        #region Add
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void AddDevice_WhenEmptyNameProvided_ThrowsException()
        {
            var dataLayer = new Mock<IDataLayer>();
            var logger = new Mock<ILogger>();
            var loggerFactory = new Mock<ILoggerFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            DeviceManager codeUnderTest = new DeviceManager(dataLayer.Object, logger.Object, loggerFactory.Object, deviceFactory.Object);

            Assert.ThrowsAny<ArgumentException>(() => codeUnderTest.AddDevice(""));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void AddDevice_WhenCalled_AddsDeviceToDataLayer()
        {
            var dataLayer = new Mock<IDataLayer>();
            var logger = new Mock<ILogger>();
            var loggerFactory = new Mock<ILoggerFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var mockDevice = new Mock<IDevice>();
            deviceFactory.Setup(mock => mock.CreateDemoDevice(It.IsAny<DeviceModel>(), It.IsAny<ILoggerFactory>())).Returns(mockDevice.Object);

            DeviceManager codeUnderTest = new DeviceManager(dataLayer.Object, logger.Object, loggerFactory.Object, deviceFactory.Object);

            codeUnderTest.AddDevice("Test");
            
            dataLayer.Verify(mock => mock.AddUpdateDevices(It.IsAny<DeviceModel[]>()), Times.Once);
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void AddDevice_WhenCalled_StartDeviceProcess()
        {
            var dataLayer = new Mock<IDataLayer>();
            var logger = new Mock<ILogger>();
            var loggerFactory = new Mock<ILoggerFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();

            string name = "Test";

            var mockDevice = new Mock<IDevice>();
            mockDevice.Setup(mock => mock.Model).Returns(new DeviceModel() { Name = name });
            deviceFactory.Setup(mock => mock.CreateDemoDevice(It.IsAny<DeviceModel>(), It.IsAny<ILoggerFactory>())).Returns(mockDevice.Object);
            DeviceManager codeUnderTest = new DeviceManager(dataLayer.Object, logger.Object, loggerFactory.Object, deviceFactory.Object);

            codeUnderTest.AddDevice(name);

            Assert.Single(codeUnderTest.Devices);
            Assert.Equal(name, codeUnderTest.Devices[0].Device.Model.Name);
        }
        #endregion
    }
}
