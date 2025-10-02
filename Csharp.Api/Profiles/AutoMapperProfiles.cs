using AutoMapper;
using Csharp.Api.DTOs;
using Csharp.Api.Entities;
using Csharp.Api.Entities.Enums;

namespace Csharp.Api.Profiles
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<CreateMotoDto, Moto>()
                .ForMember(dest => dest.DataCriacaoRegistro, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Placa, opt => opt.MapFrom(src => src.Placa != null ? src.Placa.ToUpper() : null));
            
            CreateMap<UpdateMotoDto, Moto>()
                .ForMember(dest => dest.Placa, opt => opt.MapFrom(src => src.Placa != null ? src.Placa.ToUpper() : null))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Moto, MotoViewDto>()
                .ForMember(dest => dest.Modelo, opt => opt.MapFrom(src => src.Modelo.ToString()))
                .ForMember(dest => dest.StatusMoto, opt => opt.MapFrom(src => src.StatusMoto.ToString()));
            
            CreateMap<CreateMotoDto, TagBle>()
                .ForMember(dest => dest.CodigoUnicoTag, opt => opt.MapFrom(src => src.CodigoUnicoTagParaNovaTag));

            CreateMap<TagBle, TagBleViewDto>();

            CreateMap<CreateBeaconDto, Beacon>();
            CreateMap<UpdateBeaconDto, Beacon>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Beacon, BeaconDto>();
        }
    }
}