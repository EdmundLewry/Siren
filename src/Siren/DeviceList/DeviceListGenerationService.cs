using System;
using System.Collections.Generic;

namespace PBS.Siren
{
    public class DeviceListGenerationService
    {
        /*public PlayoutListGenerationService(ChannelList channelList)
        {

        }*/

        public static Dictionary<IDevice, DeviceList> GenerateDeviceLists(ChannelList channelList)
        {
            //We do this in two steps so that we can maintain immutability for the Playout Lists
            Dictionary<IDevice, List<DeviceListEvent>> playoutEventLists = ConstructListsOfEvents(channelList); 

            Dictionary<IDevice, DeviceList> generatedLists = new Dictionary<IDevice, DeviceList>();

            //Probably a nicer way to do this in C#
            foreach(KeyValuePair<IDevice, List<DeviceListEvent>> entry in playoutEventLists)
            {
                generatedLists[entry.Key] = new DeviceList(entry.Value);
            }

            return generatedLists;
        }

        private static Dictionary<IDevice, List<DeviceListEvent>> ConstructListsOfEvents(ChannelList eventList)
        {
            Dictionary<IDevice, List<DeviceListEvent>> playoutEventLists = new Dictionary<IDevice, List<DeviceListEvent>>();

            eventList.Events.ForEach((ChannelListEvent channelEvent) => {
                if(!playoutEventLists.ContainsKey(channelEvent.Device))
                {
                    playoutEventLists[channelEvent.Device] = new List<DeviceListEvent>();
                }

                playoutEventLists[channelEvent.Device].Add(new DeviceListEvent(PlaylistEventTranslationService.TranslateToString(channelEvent.RelatedPlaylistEvent)));
            });
            
            return playoutEventLists;
        }
    }
}