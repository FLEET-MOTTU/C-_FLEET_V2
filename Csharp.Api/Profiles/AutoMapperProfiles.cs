using AutoMapper;
using Csharp.Api.DTOs;
using Csharp.Api.Entities;

namespace Csharp.Api.Profiles
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Moto: Create
            CreateMap<CreateMotoDto, Moto>()
                .ForMember(d => d.DataCriacaoRegistro, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.Placa, o => o.MapFrom(s => s.Placa != null ? s.Placa.ToUpperInvariant() : null));

            // Moto: Update
            CreateMap<UpdateMotoDto, Moto>()
                .ForMember(d => d.Placa, o => o.MapFrom(s => s.Placa != null ? s.Placa.ToUpperInvariant() : null))
                .ForAllMembers(o => o.Condition((_, __, srcMember) => srcMember != null));

            // Moto → View
            CreateMap<Moto, MotoViewDto>()
                .ForMember(d => d.Modelo, o => o.MapFrom(s => s.Modelo.ToString()))
                .ForMember(d => d.StatusMoto, o => o.MapFrom(s => s.StatusMoto.ToString()));

            // TagBLE criada junto com a moto
            CreateMap<CreateMotoDto, TagBle>()
                .ForMember(d => d.CodigoUnicoTag, o => o.MapFrom(s => s.CodigoUnicoTagParaNovaTag.ToUpperInvariant()));

            CreateMap<TagBle, TagBleViewDto>();

            // Beacon
            CreateMap<CreateBeaconDto, Beacon>()
                .ForMember(d => d.BeaconId, o => o.MapFrom(s => s.BeaconId.ToUpperInvariant()));
            CreateMap<UpdateBeaconDto, Beacon>()
                .ForAllMembers(o => o.Condition((_, __, src) => src != null));
            CreateMap<Beacon, BeaconDto>();

            // Mapeia a entidade de sync 'Zona' para a 'ZonaDto' de resposta
            CreateMap<Entities.Zona, ZonaDto>();

            // Mapeia a entidade de sync 'Pateo' para a 'PateoDetailDto' de resposta
            CreateMap<Entities.Pateo, PateoDetailDto>()
                .ForMember(dest => dest.Zonas, opt => opt.MapFrom(src => src.Zonas));
        }
    }
}
