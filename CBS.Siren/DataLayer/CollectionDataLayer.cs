using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.Siren.Data
{
    public class CollectionDataLayer : IDataLayer
    {
        private static long nextId = 0;
        private List<TransmissionList> StoredTransmissionLists { get; set; } = new List<TransmissionList>();

        public Task<IEnumerable<TransmissionList>> TransmissionLists()
        {
            return Task.FromResult<IEnumerable<TransmissionList>>(StoredTransmissionLists);
        }
        
        public Task AddUpdateTransmissionLists(params TransmissionList[] lists)
        {
            foreach (var list in lists)
            {
                TransmissionList foundList = GetTransmissionListById(list.Id);
                if(foundList != null)
                {
                    foundList = list;
                    continue;
                }

                list.Id = nextId++.ToString();
                StoredTransmissionLists.Add(list);
            }

            return Task.CompletedTask;
        }

        private TransmissionList GetTransmissionListById(string id)
        {
            return StoredTransmissionLists.FirstOrDefault((list) => list.Id == id);
        }
    }
}