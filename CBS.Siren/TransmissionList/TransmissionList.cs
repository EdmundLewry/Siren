using System.Collections.Generic;
using System.Linq;

namespace CBS.Siren
{
    public class TransmissionList
    {
        public int Id { get; set; } = 0;
        public IPlaylist SourceList { get; set; }
        public List<TransmissionListEvent> Events { get; set;  }
        public int? CurrentEventId { get; set; } = null;
        public TransmissionListState State { get; set; } = TransmissionListState.Stopped;

        public TransmissionList() : this(new List<TransmissionListEvent>()) {}

        public TransmissionList(List<TransmissionListEvent> events, IPlaylist list = null)
        {
            SourceList = list;
            Events = events;
            CurrentEventId = events.Any() ? events[0].Id : CurrentEventId;
        }

        public override string ToString()
        {
            string result = $"Id: {Id}";
            for (int i = 0; i < Events.Count; ++i)
            {
                result = $"{result}\nEvent #{i} - {Events[i]}\n";
            }

            return result;
        }
    }
}