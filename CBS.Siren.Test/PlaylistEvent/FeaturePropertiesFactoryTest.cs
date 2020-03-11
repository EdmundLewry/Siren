using System;
using Xunit;

namespace CBS.Siren.Test
{
    public class FeaturePropertiesFactoryTest
    {

        [Fact]
        public void CreateSourceStrategy_WithMedia_ReturnsAMediaSourceStrategy()
        {
            IFeaturePropertiesFactory featurePropertiesFactory = new FeaturePropertiesFactory();

            MediaInstance mediaInstance = new MediaInstance("test");
            Assert.IsType<MediaSourceStrategy>(featurePropertiesFactory.CreateMediaSourceStrategy(mediaInstance));
        }
        
        [Fact]
        public void CreatePlayoutStrategy_WithPrimaryVideo_ReturnsAPrimaryVideoPlayoutStrategy()
        {
            IFeaturePropertiesFactory featurePropertiesFactory = new FeaturePropertiesFactory();

            Assert.IsType<PrimaryVideoPlayoutStrategy>(featurePropertiesFactory.CreatePrimaryVideoPlayoutStrategy());
        }
    }
}
