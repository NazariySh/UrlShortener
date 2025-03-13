using AutoMapper;
using UrlShortener.Application.DTOs.ShortenedUrl;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Models;

namespace UrlShortener.Application.Profiles;

public class ShortenedUrlProfile : Profile
{
    public ShortenedUrlProfile()
    {
        CreateMap<ShortenedUrl, ShortenedUrlDto>().ReverseMap();

        CreateMap<PagedList<ShortenedUrl>, PagedList<ShortenedUrlDto>>();
    }
}