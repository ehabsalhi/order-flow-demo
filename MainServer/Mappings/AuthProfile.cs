using AutoMapper;
using MainServer.DTOs.Auth;
using MainServer.Entities;

namespace MainServer.Mappings;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<User, UserResponse>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
    }
}
