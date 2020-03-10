using System;
using System.Collections.Generic;

namespace PBS.Siren
{
    public class SimpleChannelScheduler : IScheduler
    {
        public SimpleChannelScheduler()
        {
            
        }

        public TransmissionList GenerateChannelList(IPlaylist list, IPlayoutChainConfiguration channelConfig)
        {
            //Work through each event in the list and find the devices in the config that will need to be controlled
            //given the source and playout strategies
            List<TransmissionListEvent> events = new List<TransmissionListEvent>();

            list.Events.ForEach((PlaylistEvent e) => {
                TransmissionListEvent channelEvent = TranslateListEvent(e, channelConfig);
                if(channelEvent!=null)
                {
                    events.Add(channelEvent);
                }
            });

            CalculateListTimings(list, channelConfig);
            return new TransmissionList(events, list, null); 
        }

        private TransmissionListEvent TranslateListEvent(PlaylistEvent e, IPlayoutChainConfiguration channelConfig)
        {
            IDevice deviceForPlayout = FindDeviceForEvent(e, channelConfig);
            if(deviceForPlayout == null)
            {
                return null;
            }
            return new TransmissionListEvent(e, deviceForPlayout);
        }

        private IDevice FindDeviceForEvent(PlaylistEvent e, IPlayoutChainConfiguration channelConfig)
        {
            //Should use Linq for this eventually
            if(channelConfig.ChainDevices.Count == 0)
            {
                return null;
            }

            return channelConfig.ChainDevices[0];
        }

        private void CalculateListTimings(IPlaylist list, IPlayoutChainConfiguration channelConfig)
        {
            list.Events.ForEach((PlaylistEvent e) => e.EventTimingStrategy.CalculateStartTime());
        }
    }
}