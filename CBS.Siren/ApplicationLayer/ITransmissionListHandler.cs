using System.Collections.Generic;
using System.Threading.Tasks;
using CBS.Siren.DTO;

namespace CBS.Siren.Application
{
    public interface ITransmissionListHandler
    {
        Task<IEnumerable<TransmissionList>> GetAllLists();
        Task<IEnumerable<TransmissionListEvent>> GetListEvents(int id);
        Task<TransmissionListEvent> AddEvent(int id, TransmissionListEventCreationDTO listEvent);
        Task RemoveEvent(int listId, int eventId);
        Task ClearList(int id);

        Task PlayTransmissionList(int id);
        Task PauseTransmissionList(int id);
        Task NextTransmissionList(int id);
    }
}