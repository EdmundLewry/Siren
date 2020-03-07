using System;
using System.Collections.Generic;

namespace PBS.Siren
{
    public class PlayoutListGenerationService
    {
        /*public PlayoutListGenerationService(ChannelList channelList)
        {

        }*/

        public static Dictionary<IDevice, PlayoutList> GeneratePlayoutLists(ChannelList channelList)
        {
            //We do this in two steps so that we can maintain immutability for the Playout Lists
            Dictionary<IDevice, List<PlayoutListEvent>> playoutEventLists = ConstructListsOfEvents(channelList); 

            Dictionary<IDevice, PlayoutList> generatedLists = new Dictionary<IDevice, PlayoutList>();

            //Probably a nicer way to do this in C#
            foreach(KeyValuePair<IDevice, List<PlayoutListEvent>> entry in playoutEventLists)
            {
                generatedLists[entry.Key] = new PlayoutList(entry.Value);
            }

            return generatedLists;
        }

        private static Dictionary<IDevice, List<PlayoutListEvent>> ConstructListsOfEvents(ChannelList eventList)
        {
            Dictionary<IDevice, List<PlayoutListEvent>> playoutEventLists = new Dictionary<IDevice, List<PlayoutListEvent>>();

            eventList.Events.ForEach((ChannelListEvent channelEvent) => {
                if(!playoutEventLists.ContainsKey(channelEvent.Device))
                {
                    playoutEventLists[channelEvent.Device] = new List<PlayoutListEvent>();
                }

                playoutEventLists[channelEvent.Device].Add(new PlayoutListEvent(TransmissionEventTranslationService.TranslateToString(channelEvent.RelatedTransmissionEvent)));
            });
            
            return playoutEventLists;
        }
    }
}