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

            Assert.IsType<MediaSourceStrategy>(featurePropertiesFactory.CreateSourceStrategy("media"));
        }
        
        [Fact]
        public void CreateSourceStrategy_WithAnInvalidType_ReturnsNull()
        {
            IFeaturePropertiesFactory featurePropertiesFactory = new FeaturePropertiesFactory();

            Assert.Null(featurePropertiesFactory.CreateSourceStrategy("invalid"));
        }
        
        [Fact]
        public void CreatePlayoutStrategy_WithPrimaryVideo_ReturnsAPrimaryVideoPlayoutStrategy()
        {
            IFeaturePropertiesFactory featurePropertiesFactory = new FeaturePropertiesFactory();

            Assert.IsType<PrimaryVideoPlayoutStrategy>(featurePropertiesFactory.CreatePlayoutStrategy("primaryVideo"));
        }

        [Fact]
        public void CreatePlayoutStrategy_WithAnInvalidType_ReturnsNull()
        {
            IFeaturePropertiesFactory featurePropertiesFactory = new FeaturePropertiesFactory();

            Assert.Null(featurePropertiesFactory.CreatePlayoutStrategy("invalid"));
        }
    }
}
