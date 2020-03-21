using System;
namespace CBS.Siren
{
    /*
    A Source Strategy is the series of data that defines what the input of the event is.

    This could be a Media Source, used to play out some kind of Media File(s). It could be a Live Source,
    used to route a particular input to our playout output.
    */
    public interface ISourceStrategy : IEquatable<ISourceStrategy>
    {
        string StrategyType { get; }
        int GetDuration();
        object BuildStrategyData();
    }
}