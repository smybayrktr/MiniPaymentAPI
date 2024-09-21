using AutoMapper;
using Payment.Contracts.Commands;
using Payment.Contracts.Queries;
using Payment.Contracts.DTOs;

namespace Payment.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<PayTransactionDto, PayTransactionCommand>();
        CreateMap<SearchPaymentDto, SearchPaymentQuery>();
    }
}
