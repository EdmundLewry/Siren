using System.Collections.Generic;
using System.Threading.Tasks;

namespace CBS.Siren.Application
{
    public interface ITransmissionListHandler
    {
        Task<IEnumerable<TransmissionList>> GetAllLists();
    }
}