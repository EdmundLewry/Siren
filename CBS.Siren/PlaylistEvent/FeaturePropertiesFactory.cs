using System;
using System.Collections.Generic;
using System.Text;

namespace CBS.Siren
{
    public class FeaturePropertiesFactory : IFeaturePropertiesFactory
    {
        const string PRIMARY_VIDEO_PLAYOUT_STRATEGY = "primaryVideo";
        const string MEIDA_SOURCE_STRATEGY = "media";

        public FeaturePropertiesFactory()
        {
            
        }

        public IPlayoutStrategy CreatePlayoutStrategy(string type)
        {
            return type switch
            {
                PRIMARY_VIDEO_PLAYOUT_STRATEGY => new PrimaryVideoPlayoutStrategy(),
                _ => null
            };
        }

        public ISourceStrategy CreateSourceStrategy(string type)
        {
            return type switch
            {
                MEIDA_SOURCE_STRATEGY => new MediaSourceStrategy(new MediaInstance(), 0, 0),
                _ => null
            };
        }
    }
}
