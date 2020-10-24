using System;
using System.Linq;
using AutoMapper;
using CBS.Siren.Device;
using CBS.Siren.Time;

namespace CBS.Siren.DTO
{
    public class TransmissionListMappingProfile : Profile
    {
        public TransmissionListMappingProfile()
        {
            CreateTransmissionListMappings();
            CreateTransmissionListEventMappings();
        }

        private void CreateTransmissionListMappings()
        {
            CreateMap<TransmissionList, TransmissionListDTO>()
                        .ForMember(dto => dto.EventCount,
                                   config => config.MapFrom(list => list.Events.Count))
                        .ForMember(dto => dto.ListState,
                                    config => config.MapFrom(list => Enum.GetName(typeof(TransmissionListState), list.State)));

            CreateMap<TransmissionList, TransmissionListDetailDTO>()
                        .ForMember(dto => dto.ListState,
                                    config => config.MapFrom(list => Enum.GetName(typeof(TransmissionListState), list.State)));
        }

        private void CreateTransmissionListEventMappings()
        {
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

            CreateMap<TransmissionListEvent, TransmissionListEventDetailDTO>()
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
                        .ForMember(dto => dto.RelatedDeviceListEventCount,
                            config => config.MapFrom(listEvent => listEvent.EventFeatures.Count(feature => feature.DeviceListEventId.HasValue)));

            CreateMap<IEventTimingStrategy, TimingStrategyDTO>()
                        .ForMember(dto => dto.TargetStartTime,
                                    config => config.MapFrom(timingStrategy => timingStrategy.TargetStartTime.ToTimecodeString()));

            CreateMap<IEventFeature, ListEventFeatureDTO>();
            CreateMap<IPlayoutStrategy, PlayoutStrategyDTO>();
            CreateMap<ISourceStrategy, SourceStrategyDTO>()
                    .ForMember(dto => dto.SOM,
                                config => config.MapFrom(sourceStrategy => sourceStrategy.SOM.ToTimecodeString()))
                    .ForMember(dto => dto.EOM,
                                config => config.MapFrom(sourceStrategy => sourceStrategy.EOM.ToTimecodeString()));

            CreateMap<PlaylistEvent, PlaylistEventDTO>()
                    .ForMember(dto => dto.FeatureCount,
                                config => config.MapFrom(playlistEvent => playlistEvent.EventFeatures.ToList().Count));

            CreateMap<IDevice, DeviceDTO>()
                .ForMember(dto => dto.Id, config => config.MapFrom(device => device.Model.Id))
                .ForMember(dto => dto.Name, config => config.MapFrom(device => device.Model.Name))
                .ForMember(dto => dto.CurrentStatus, config => config.MapFrom(device => Enum.GetName(typeof(IDevice.DeviceStatus), device.CurrentStatus)));
        }
    }
}