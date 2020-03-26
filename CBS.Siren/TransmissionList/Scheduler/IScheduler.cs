using CBS.Siren.Device;
using System.Collections.Generic;

namespace CBS.Siren
{
    /*
    A Scheduler takes a Transmision List and a Playout Chain Configuration and applies logic to it to
    generate a Channel List, which defines what devices we expect to be required in order to be able to
    play each Transmission Event in the Transmission List.

    The Scheduler is also responsible for handling manual intervention in the Channel, via Channel Actions
    and will trigger Event Timing Strategies on the Transmission Events.
    */
    public interface IScheduler
    {
        Dictionary<IDevice, DeviceList> ScheduleTransmissionList(TransmissionList transmissionList);
    }
}