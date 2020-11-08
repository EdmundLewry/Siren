using System;

namespace CBS.Siren
{
    public static class TransmissionListExtensions
    {
        public static int GetEventPositionById(this TransmissionList transmissionList, int eventId)
        {
            //This is a fairly expensive way to do this. We may want to add positional data 
            //to the events and maintain it in some other way
            //We'll probably need that so that we can order them when they come out of the db anyway
            int listEventPositionIndex = transmissionList.Events.FindIndex((listEvent) => listEvent.Id == eventId);
            if (listEventPositionIndex == -1)
            {
                throw new ArgumentException($"Unable to find list event with id: {eventId}", nameof(eventId));
            }
            return listEventPositionIndex;
        }
    }
}
