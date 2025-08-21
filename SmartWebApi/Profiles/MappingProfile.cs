using AutoMapper;
using SmartWebApi.Models.DTOs;
using SmartWebApi.Models.Identity;

namespace SmartWebApi.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
    }
}