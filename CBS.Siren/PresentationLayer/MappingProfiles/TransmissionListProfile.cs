using System;
using AutoMapper;

namespace CBS.Siren.DTO
{
    public class TransmissionListProfile : Profile
    {
        public TransmissionListProfile()
        {
            CreateMap<TransmissionList, TransmissionListDTO>()
                        .ForMember(dto => dto.EventCount,
                                   config => config.MapFrom(list => list.Events.Count));
            
            CreateMap<TransmissionListEvent, TransmissionListEventDTO>()
                        .ForMember(dto => dto.EventState,
                                config => config.MapFrom(listEvent => Enum.GetName(typeof(TransmissionListEventState.Status), listEvent.EventState.CurrentStatus)))
                        .ForMember(dto => dto.EventTimingStrategy,
                                config => config.MapFrom(listEvent => listEvent.EventTimingStrategy.StrategyType))
                        .ForMember(dto => dto.EventFeatureCount,
                                config => config.MapFrom(listEvent => listEvent.EventFeatures.Count))
                        .ForMember(dto => dto.RelatedPlaylistEvent,
                            config => config.MapFrom(listEvent => listEvent.RelatedPlaylistEvent.Id))
                        .ForMember(dto => dto.RelatedDeviceListEventCount,
                            config => config.MapFrom(listEvent => listEvent.RelatedDeviceListEvents.Count));
        }
    }
}