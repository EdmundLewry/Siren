using System.Collections.Generic;
using System.Threading.Tasks;
using CBS.Siren.DTO;

namespace CBS.Siren.Application
{
    public interface ITransmissionListHandler
    {
        Task<IEnumerable<TransmissionList>> GetAllLists();
        Task<TransmissionList> GetListById(int id);
        Task<IEnumerable<TransmissionListEvent>> GetListEvents(int id);
        Task<TransmissionListEvent> GetListEventById(int listId, int eventId);
        Task<TransmissionListEvent> AddEvent(int id, TransmissionListEventUpsertDTO listEvent);
        Task RemoveEvent(int listId, int eventId);
        Task ClearList(int id);

        Task<TransmissionListEvent> ChangeEventPosition(int listId, int eventId, int previousPosition, int targetPosition);
        Task<TransmissionListEvent> UpdateEventDetails(int listId, int eventId, TransmissionListEventUpsertDTO listEventUpdate);

        Task PlayTransmissionList(int id);
        Task PauseTransmissionList(int id);
        Task NextTransmissionList(int id);
    }
}