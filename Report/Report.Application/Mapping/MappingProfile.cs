using AutoMapper;
using Report.Contracts.DTOs;
using Report.Contracts.Queries;

namespace Report.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<GetReportDto, GetReportQuery>();
    }
}
