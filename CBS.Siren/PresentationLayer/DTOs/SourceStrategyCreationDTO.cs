using System;

namespace CBS.Siren.DTO
{
    public class SourceStrategyCreationDTO
    {
        public string StrategyType { get; set; }
        public TimeSpan SOM { get; set; }
        public TimeSpan EOM { get; set; }
        public int MediaInstanceId { get; set; }
    }
}