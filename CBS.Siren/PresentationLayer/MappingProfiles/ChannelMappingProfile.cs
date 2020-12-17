using AutoMapper;
using CBS.Siren.PresentationLayer.DTOs;

namespace CBS.Siren.DTO
{
    public class ChannelMappingProfile : Profile
    {
        public ChannelMappingProfile()
        {
            CreateMap<Channel, ChannelDTO>();
        }
    }
}
