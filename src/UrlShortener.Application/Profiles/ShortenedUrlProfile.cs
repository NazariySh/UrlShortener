using AutoMapper;
using UrlShortener.Application.DTOs;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Profiles;

public class ShortenedUrlProfile : Profile
{
    public ShortenedUrlProfile()
    {
        CreateMap<ShortenedUrl, ShortenedUrlDto>();
    }
}