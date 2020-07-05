using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.Siren.Data
{
    public class CollectionDataLayer : IDataLayer
    {
        private long nextListId = 0;
        private List<TransmissionList> StoredTransmissionLists { get; set; } = new List<TransmissionList>();
        private List<MediaInstance> StoredMediaInstances { get; set; } = new List<MediaInstance>();

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
                    /* For an in memory collection, we don't actually need to do anything
                     * if the user is updating the original object, since they would have updated the
                     * object reference.
                     
                    Note: Not sure exactly what I want this to behave like. Should Get pass back a new object?
                     */

                    foundList.SourceList = list.SourceList ?? foundList.SourceList;
                    foundList.Events = list.Events ?? foundList.Events;
                    continue;
                }

                list.Id = nextListId++.ToString();
                StoredTransmissionLists.Add(list);
            }

            return Task.CompletedTask;
        }

        private TransmissionList GetTransmissionListById(string id)
        {
            return StoredTransmissionLists.FirstOrDefault((list) => list.Id == id);
        }

        public Task<IEnumerable<MediaInstance>> MediaInstances()
        {
            return Task.FromResult<IEnumerable<MediaInstance>>(StoredMediaInstances);
        }
    }
}