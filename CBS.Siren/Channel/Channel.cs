namespace CBS.Siren
{
    /*
    A channel is a logical combination of a particular transmission list, 1 or more Payout Chain
    configurations, and a scheduler which is responsible for combining the Playout Chain Configurations
    and transmission list into a Channel List, and triggering event timings to update in the transmission list
     */
    public class Channel
    {
        public IVideoChain ChainConfiguration { get; set; }
        
        public Channel(IVideoChain channelConfig)
        {
            ChainConfiguration = channelConfig;
        }
    }
}