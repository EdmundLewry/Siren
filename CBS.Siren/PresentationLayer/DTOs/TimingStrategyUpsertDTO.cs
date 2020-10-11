using System;

namespace CBS.Siren.DTO
{
    public class TimingStrategyUpsertDTO
    {
        public string StrategyType {get; set;}
        public DateTimeOffset? TargetStartTime { get; set; }
    }
}