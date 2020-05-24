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
        }
    }
}