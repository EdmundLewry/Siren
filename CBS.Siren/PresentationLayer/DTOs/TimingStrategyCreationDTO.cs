using System;

namespace CBS.Siren.DTO
{
    public class TimingStrategyCreationDTO
    {
        public string StrategyType {get; set;}
        public DateTimeOffset? TargetStartTime { get; set; }
    }
}