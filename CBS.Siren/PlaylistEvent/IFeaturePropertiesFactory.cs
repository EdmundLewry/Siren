using System;
using System.Collections.Generic;
using System.Text;

namespace CBS.Siren
{
    public interface IFeaturePropertiesFactory
    {
        ISourceStrategy CreateSourceStrategy(string type);
        IPlayoutStrategy CreatePlayoutStrategy(string type);
    }
}
