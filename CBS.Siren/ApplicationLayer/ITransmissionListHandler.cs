using System.Collections.Generic;
using System.Threading.Tasks;
using CBS.Siren.DTO;

namespace CBS.Siren.Application
{
    public interface ITransmissionListHandler
    {
        Task<IEnumerable<TransmissionList>> GetAllLists();
        Task<IEnumerable<TransmissionListEvent>> GetListEvents(string id);
        Task<TransmissionListEvent> AddEvent(string id, TransmissionListEventCreationDTO listEvent);
        Task RemoveEvent(string listId, string eventId);
    }
}