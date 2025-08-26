using AutoMapper;
using Veritheia.Data.Entities;
using Veritheia.Data.DTOs;
using Veritheia.Data.Services;

namespace veritheia.Web.Mapping;

/// <summary>
/// AutoMapper profile for mapping between entities and DTOs
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Journey mappings
        CreateMap<Journey, JourneyDto>();
        CreateMap<JourneyDto, Journey>();

        // Persona mappings
        CreateMap<Persona, PersonaDto>();
        CreateMap<PersonaDto, Persona>();

        // User mappings
        CreateMap<User, UserDto>();
        CreateMap<UserDto, User>();

        // Process execution mappings
        CreateMap<ProcessExecution, ProcessExecutionDto>();
        CreateMap<ProcessExecutionDto, ProcessExecution>();

        // Statistics mappings
        CreateMap<JourneyStatistics, JourneyStatisticsDto>();
        CreateMap<RecentActivityItem, RecentActivityItemDto>();
    }
}