using System;
using System.Diagnostics.CodeAnalysis;

namespace CBS.Siren
{
    public class PrimaryVideoPlayoutStrategy : IPlayoutStrategy
    {
        public string StrategyType => "primaryVideo";

        public virtual bool Equals([AllowNull] IPlayoutStrategy other)
        {
            return other is PrimaryVideoPlayoutStrategy;
        }

        public override string ToString()
        {
            return "PrimaryVideoPlayoutStrategy";
        }
    }
}