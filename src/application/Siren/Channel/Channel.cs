namespace PBS.Siren
{
    /*
    A channel is a logical combination of a particular transmission list, 1 or more Payout Chain
    configurations, and a scheduler which is responsible for combining the Playout Chain Configurations
    and transmission list into a Channel List, and triggering event timings to update in the transmission list
     */
    class Channel
    {
        public PlayoutChainConfiguration ChainConfiguration { get; set; }
        public TransmissionList SourceList { get; set; }
        public ChannelList GeneratedList { get; set; }
        public IScheduler Scheduler { get; set; }
        
        public Channel(PlayoutChainConfiguration channelConfig, TransmissionList list, IScheduler listScheduler)
        {
            Scheduler = listScheduler;
            SourceList = list;
            ChainConfiguration = channelConfig;
        }
    }
}