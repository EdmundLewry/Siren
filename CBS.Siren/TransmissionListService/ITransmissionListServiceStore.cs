using System;

namespace CBS.Siren
{
    public interface ITransmissionListServiceStore
    {
        ITransmissionListService GetTransmissionListServiceByListId(int transmissionListId);
    }
}
