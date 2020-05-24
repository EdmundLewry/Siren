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
                                config => config.MapFrom(list => Enum.GetName(typeof(TransmissionListEventState.Status), list.EventState.CurrentStatus)))
                        .ForMember(dto => dto.EventTimingStrategy,
                                config => config.MapFrom(list => list.EventTimingStrategy.StrategyType))
                        .ForMember(dto => dto.EventFeatureCount,
                                config => config.MapFrom(list => list.EventFeatures.Count))
                        .ForMember(dto => dto.RelatedPlaylistEvent,
                            config => config.MapFrom(list => list.RelatedPlaylistEvent.Id))
                        .ForMember(dto => dto.RelatedDeviceListEventCount,
                            config => config.MapFrom(list => list.RelatedDeviceListEvents.Count));
        }
    }
}