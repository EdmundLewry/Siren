using System.Collections.Generic;
using System.Threading.Tasks;

namespace CBS.Siren.Data
{
    public interface IDataLayer
    {
        Task<IEnumerable<TransmissionList>> TransmissionLists();
        Task AddUpdateTransmissionLists(params TransmissionList[] lists);

        Task<IEnumerable<MediaInstance>> MediaInstances();
    }
}