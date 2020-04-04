using System.Collections.Generic;

namespace CBS.Siren
{
    public class TransmissionList
    {
        public IPlaylist SourceList { get; set; }
        public List<TransmissionListEvent> Events { get; }
        
        public TransmissionList(List<TransmissionListEvent> events, IPlaylist list)
        {
            SourceList = list;
            Events = events;
        }

        public override string ToString()
        {
            string result = "";
            for (int i = 0; i < Events.Count; ++i)
            {
                result = $"{result}\nEvent #{i} - {Events[i].ToString()}\n";
            }

            return result;
        }
    }
}