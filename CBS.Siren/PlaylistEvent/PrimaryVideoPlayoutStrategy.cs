using System;
using System.Diagnostics.CodeAnalysis;

namespace CBS.Siren
{
    public class PrimaryVideoPlayoutStrategy : IPlayoutStrategy, IEquatable<PrimaryVideoPlayoutStrategy>
    {
        public PrimaryVideoPlayoutStrategy()
        {
            
        }

        public bool Equals([AllowNull] PrimaryVideoPlayoutStrategy other)
        {
            return other != null;
        }

        public override string ToString()
        {
            return "PrimaryVideoPlayoutStrategy";
        }
    }
}