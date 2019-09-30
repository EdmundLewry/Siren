namespace PBS.Siren
{
    /*
    A Scheduler takes a Transmision List and a Playout Chain Configuration and applies logic to it to
    generate a Channel List, which defines what devices we expect to be required in order to be able to
    play each Transmission Event in the Transmission List.

    The Scheduler is also responsible for handling manual intervention in the Channel, via Channel Actions
    and will trigger Event Timing Strategies on the Transmission Events.
    */
    public interface IScheduler
    {
        ChannelList GenerateChannelList(TransmissionList list, PlayoutChainConfiguration channelConfig);
    }
}