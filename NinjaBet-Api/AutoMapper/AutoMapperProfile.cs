using AutoMapper;
using NinjaBet_Api.Models.Caixa;
using NinjaBet_Application.DTOs.Caixa;

namespace NinjaBet_Api.AutoMapper
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CaixaFiltroModel, CaixaFiltroDto>();
            CreateMap<CaixaFiltroDto, CaixaFiltroModel>();
        }
    }
}
