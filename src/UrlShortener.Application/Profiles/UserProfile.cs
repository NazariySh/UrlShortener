using AutoMapper;
using UrlShortener.Application.DTOs.Users;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Models;

namespace UrlShortener.Application.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>();

        CreateMap<PagedList<User>, PagedList<UserDto>>();

        CreateMap<RegisterDto, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));
    }
}