using System;
using System.Collections.Generic;

namespace PBS.Siren
{
    public class SimpleChannelScheduler : IScheduler
    {
        public SimpleChannelScheduler()
        {
            
        }

        public ChannelList GenerateChannelList(ITransmissionList list, IPlayoutChainConfiguration channelConfig)
        {
            //Work through each event in the list and find the devices in the config that will need to be controlled
            //given the source and playout strategies
            List<ChannelListEvent> events = new List<ChannelListEvent>();

            list.Events.ForEach((TransmissionEvent e) => {
                ChannelListEvent channelEvent = TranslateListEvent(e, channelConfig);
                if(channelEvent!=null)
                {
                    events.Add(channelEvent);
                }
            });

            CalculateListTimings(list, channelConfig);
            return new ChannelList(events); 
        }

        private ChannelListEvent TranslateListEvent(TransmissionEvent e, IPlayoutChainConfiguration channelConfig)
        {
            IDevice deviceForPlayout = FindDeviceForEvent(e, channelConfig);
            if(deviceForPlayout == null)
            {
                return null;
            }
            return new ChannelListEvent(e, deviceForPlayout);
        }

        private IDevice FindDeviceForEvent(TransmissionEvent e, IPlayoutChainConfiguration channelConfig)
        {
            //Should use Linq for this eventually
            if(channelConfig.ChainDevices.Count == 0)
            {
                return null;
            }

            return channelConfig.ChainDevices[0];
        }

        private void CalculateListTimings(ITransmissionList list, IPlayoutChainConfiguration channelConfig)
        {
            list.Events.ForEach((TransmissionEvent e) => e.EventTimingStrategy.CalculateStartTime());
        }
    }
}