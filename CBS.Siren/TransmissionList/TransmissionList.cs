using System.Collections.Generic;

namespace CBS.Siren
{
    public class TransmissionList
    {
        public string Id { get; set; }
        public IPlaylist SourceList { get; set; }
        public List<TransmissionListEvent> Events { get; set;  }
        
        public TransmissionList() : this(new List<TransmissionListEvent>()) {}
        public TransmissionList(List<TransmissionListEvent> events, IPlaylist list = null)
        {
            SourceList = list;
            Events = events;
        }

        public override string ToString()
        {
            string result = $"Id: {Id}";
            for (int i = 0; i < Events.Count; ++i)
            {
                result = $"{result}\nEvent #{i} - {Events[i].ToString()}\n";
            }

            return result;
        }
    }
}