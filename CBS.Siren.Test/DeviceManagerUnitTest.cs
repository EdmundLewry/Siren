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
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void DeviceManager_OnConstruction_CreatesADeviceForEachDeviceModel()
        {
            var dataLayer = new Mock<IDataLayer>();
            List<DeviceModel> deviceModels = new List<DeviceModel>()
            {
                new DeviceModel(){ Id = 1, Name = "1" },
                new DeviceModel(){ Id = 2, Name = "2" },
                new DeviceModel(){ Id = 3, Name = "3" }
            };
            dataLayer.Setup(mock => mock.Devices()).Returns(Task.FromResult<IEnumerable<DeviceModel>>(deviceModels));

            var logger = new Mock<ILogger<DeviceManager>>();
            var loggerFactory = new Mock<ILoggerFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var mockDevice = new Mock<IDevice>();
            deviceFactory.Setup(mock => mock.CreateDemoDevice(It.IsAny<DeviceModel>(), It.IsAny<ILoggerFactory>())).Returns(mockDevice.Object);
            DeviceManager codeUnderTest = new DeviceManager(dataLayer.Object, logger.Object, loggerFactory.Object, deviceFactory.Object);

            Assert.Equal(3, codeUnderTest.Devices.Count);
        }        

        #region Add
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void AddDevice_WhenEmptyNameProvided_ThrowsException()
        {
            var dataLayer = new Mock<IDataLayer>();
            dataLayer.Setup(mock => mock.AddUpdateDevices(It.IsAny<DeviceModel[]>())).ReturnsAsync(new List<DeviceModel>() { new DeviceModel() { Id = 0, Name = "Test" } });
            var logger = new Mock<ILogger<DeviceManager>>();
            var loggerFactory = new Mock<ILoggerFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var mockDevice = new Mock<IDevice>();
            deviceFactory.Setup(mock => mock.CreateDemoDevice(It.IsAny<DeviceModel>(), It.IsAny<ILoggerFactory>())).Returns(mockDevice.Object);
            DeviceManager codeUnderTest = new DeviceManager(dataLayer.Object, logger.Object, loggerFactory.Object, deviceFactory.Object);

            Assert.ThrowsAny<ArgumentException>(() => codeUnderTest.AddDevice(""));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void AddDevice_WhenCalled_AddsDeviceToDataLayer()
        {
            var dataLayer = new Mock<IDataLayer>();
            dataLayer.Setup(mock => mock.AddUpdateDevices(It.IsAny<DeviceModel[]>())).ReturnsAsync(new List<DeviceModel>() { new DeviceModel() { Id = 0, Name = "Test" } });
            var logger = new Mock<ILogger<DeviceManager>>();
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
            string name = "Test";

            var dataLayer = new Mock<IDataLayer>();
            dataLayer.Setup(mock => mock.AddUpdateDevices(It.IsAny<DeviceModel[]>())).ReturnsAsync(new List<DeviceModel>() { new DeviceModel() { Id = 0, Name = name } });
            var logger = new Mock<ILogger<DeviceManager>>();
            var loggerFactory = new Mock<ILoggerFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();


            var mockDevice = new Mock<IDevice>();
            mockDevice.Setup(mock => mock.Model).Returns(new DeviceModel() { Name = name });
            deviceFactory.Setup(mock => mock.CreateDemoDevice(It.IsAny<DeviceModel>(), It.IsAny<ILoggerFactory>())).Returns(mockDevice.Object);
            DeviceManager codeUnderTest = new DeviceManager(dataLayer.Object, logger.Object, loggerFactory.Object, deviceFactory.Object);

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
            var dataLayer = new Mock<IDataLayer>();
            var logger = new Mock<ILogger<DeviceManager>>();
            var loggerFactory = new Mock<ILoggerFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var mockDevice = new Mock<IDevice>();
            deviceFactory.Setup(mock => mock.CreateDemoDevice(It.IsAny<DeviceModel>(), It.IsAny<ILoggerFactory>())).Returns(mockDevice.Object);
            DeviceManager codeUnderTest = new DeviceManager(dataLayer.Object, logger.Object, loggerFactory.Object, deviceFactory.Object);

            Assert.ThrowsAny<ArgumentException>(() => codeUnderTest.GetDevice(100));
        }
        
        [Fact]
        [Trait("TestType", "UnitTest")]
        public void GetDevice_WhenIdExists_ReturnsDevice()
        {
            string expectedName = "Test";

            var dataLayer = new Mock<IDataLayer>();
            DeviceModel deviceModel = new DeviceModel() { Id = 1, Name = expectedName };
            dataLayer.Setup(mock => mock.Devices()).Returns(Task.FromResult<IEnumerable<DeviceModel>>(new List<DeviceModel>() { deviceModel }));

            var logger = new Mock<ILogger<DeviceManager>>();
            var loggerFactory = new Mock<ILoggerFactory>();
            var deviceFactory = new Mock<IDeviceFactory>();
            var mockDevice = new Mock<IDevice>();
            mockDevice.Setup(mock => mock.Model).Returns(deviceModel);
            deviceFactory.Setup(mock => mock.CreateDemoDevice(It.IsAny<DeviceModel>(), It.IsAny<ILoggerFactory>())).Returns(mockDevice.Object);

            DeviceManager codeUnderTest = new DeviceManager(dataLayer.Object, logger.Object, loggerFactory.Object, deviceFactory.Object);

            IDevice device = codeUnderTest.GetDevice(1);
            Assert.Equal(expectedName, device.Model.Name);
        }
        #endregion
    }
}
