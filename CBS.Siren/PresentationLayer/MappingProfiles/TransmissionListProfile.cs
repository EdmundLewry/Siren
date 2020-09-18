using System;
using System.Linq;
using AutoMapper;
using CBS.Siren.Time;

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
                        .ForMember(dto => dto.ExpectedDuration,
                                config => config.MapFrom(listEvent => listEvent.ExpectedDuration.ToTimecodeString()))
                        .ForMember(dto => dto.ExpectedStartTime,
                                config => config.MapFrom(listEvent => listEvent.ExpectedStartTime.ToTimecodeString()))
                        .ForMember(dto => dto.ActualStartTime,
                                config => config.MapFrom(listEvent => listEvent.ActualStartTime.HasValue ? listEvent.ActualStartTime.Value.ToTimecodeString() : ""))
                        .ForMember(dto => dto.ActualEndTime,
                                config => config.MapFrom(listEvent => listEvent.ActualEndTime.HasValue ? listEvent.ActualEndTime.Value.ToTimecodeString() : ""))
                        .ForMember(dto => dto.EventTimingStrategy,
                                config => config.MapFrom(listEvent => listEvent.EventTimingStrategy.StrategyType))
                        .ForMember(dto => dto.EventFeatureCount,
                                config => config.MapFrom(listEvent => listEvent.EventFeatures.Count))
                        .ForMember(dto => dto.RelatedPlaylistEvent,
                            config => config.MapFrom(listEvent => listEvent.RelatedPlaylistEvent.Id))
                        .ForMember(dto => dto.RelatedDeviceListEventCount,
                            config => config.MapFrom(listEvent => listEvent.EventFeatures.Count(feature => feature.DeviceListEventId.HasValue)));
        }
    }
}