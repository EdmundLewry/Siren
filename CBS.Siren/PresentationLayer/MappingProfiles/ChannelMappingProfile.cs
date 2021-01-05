using AutoMapper;
using CBS.Siren.PresentationLayer.DTOs;

namespace CBS.Siren.DTO
{
    public class ChannelMappingProfile : Profile
    {
        public ChannelMappingProfile()
        {
            CreateMap<Channel, ChannelDTO>()
                .ForMember(dto => dto.ListCount,
                           config => config.MapFrom(channel => channel.TransmissionLists.Count))
                .ForMember(dto => dto.HealthyListCount,
                           config => config.MapFrom(channel => channel.TransmissionLists.Count));

            CreateMap<Channel, ChannelDetailsDTO>();
        }
    }
}
