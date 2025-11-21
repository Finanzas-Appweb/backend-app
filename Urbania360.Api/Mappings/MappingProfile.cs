using AutoMapper;
using Urbania360.Api.DTOs.Auth;
using Urbania360.Api.DTOs.Banks;
using Urbania360.Api.DTOs.Clients;
using Urbania360.Api.DTOs.Properties;
using Urbania360.Api.DTOs.Simulations;
using Urbania360.Api.DTOs.Users;
using Urbania360.Domain.Entities;
using Urbania360.Domain.Enums;

namespace Urbania360.Api.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        // RegisterRequest -> User mapping not needed (se crea manualmente en AuthController con rol User por defecto)

        CreateMap<User, UserInfo>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

        CreateMap<User, UserResponse>()
            .ForMember(dest => dest.DefaultCurrency, opt => opt.MapFrom(src => src.UserPreference != null ? src.UserPreference.DefaultCurrency : (Currency?)null))
            .ForMember(dest => dest.DefaultRateType, opt => opt.MapFrom(src => src.UserPreference != null ? src.UserPreference.DefaultRateType : (RateType?)null));

        // Client mappings
        CreateMap<ClientCreateRequest, Client>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore());

        CreateMap<ClientUpdateRequest, Client>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore());

        CreateMap<Client, ClientResponse>()
            .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser.FullName));

        // Property mappings
        CreateMap<Property, PropertyResponse>()
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedByUser.FullName))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.PropertyImages))
            .ForMember(dest => dest.ConsultsCount, opt => opt.MapFrom(src => src.PropertyConsults.Count));

        CreateMap<PropertyImage, PropertyImageResponse>();

        // Bank mappings
        CreateMap<Bank, BankResponse>();

        // Simulation mappings
        CreateMap<LoanSimulation, SimulationResponse>()
            .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => $"{src.Client.FirstName} {src.Client.LastName}"))
            .ForMember(dest => dest.PropertyTitle, opt => opt.MapFrom(src => src.Property != null ? src.Property.Title : null))
            .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.Bank != null ? src.Bank.Name : null))
            .ForMember(dest => dest.AmortizationSchedule, opt => opt.MapFrom(src => src.AmortizationItems.OrderBy(a => a.Period)));

        CreateMap<LoanSimulation, SimulationSummaryResponse>()
            .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => $"{src.Client.FirstName} {src.Client.LastName}"))
            .ForMember(dest => dest.PropertyTitle, opt => opt.MapFrom(src => src.Property != null ? src.Property.Title : null));

        CreateMap<AmortizationItem, AmortizationItemResponse>();
    }
}