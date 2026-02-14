// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Mappings/AuthorMappingProfile.cs
using System.Linq;
using AutoMapper;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Aggregates.AuthorAggregate;
using NovelVision.Services.Catalog.Domain.ValueObjects;

namespace NovelVision.Services.Catalog.Application.Mappings;

public class AuthorMappingProfile : Profile
{
    public AuthorMappingProfile()
    {
        // =====================================================
        // Author -> AuthorDto (FULL)
        // =====================================================
        CreateMap<Author, AuthorDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Biography, opt => opt.MapFrom(src => src.Biography))
            .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src => src.IsVerified))
            .ForMember(dest => dest.VerifiedAt, opt => opt.MapFrom(src => src.VerifiedAt))
            .ForMember(dest => dest.BookCount, opt => opt.MapFrom(src => src.BookCount))
            .ForMember(dest => dest.BookIds, opt => opt.MapFrom(src => src.BookIds.Select(b => b.Value).ToList()))
            .ForMember(dest => dest.SocialLinks, opt => opt.MapFrom(src => src.SocialLinks.ToDictionary(x => x.Key, x => x.Value)))
            // NEW: Life info
            .ForMember(dest => dest.BirthYear, opt => opt.MapFrom(src => src.BirthYear))
            .ForMember(dest => dest.DeathYear, opt => opt.MapFrom(src => src.DeathYear))
            .ForMember(dest => dest.LifeSpan, opt => opt.MapFrom(src => src.LifeSpan))
            .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.Nationality))
            .ForMember(dest => dest.IsAlive, opt => opt.MapFrom(src => src.IsAlive))
            .ForMember(dest => dest.IsHistorical, opt => opt.MapFrom(src => src.IsHistorical))
            // NEW: External IDs
            .ForMember(dest => dest.ExternalIds, opt => opt.MapFrom(src => src.ExternalIds))
            .ForMember(dest => dest.IsFromExternalSource, opt => opt.MapFrom(src => src.IsFromExternalSource));

        // =====================================================
        // Author -> AuthorListDto
        // =====================================================
        CreateMap<Author, AuthorListDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
            .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src => src.IsVerified))
            .ForMember(dest => dest.BookCount, opt => opt.MapFrom(src => src.BookCount))
            .ForMember(dest => dest.LifeSpan, opt => opt.MapFrom(src => src.LifeSpan))
            .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.Nationality))
            .ForMember(dest => dest.IsHistorical, opt => opt.MapFrom(src => src.IsHistorical))
            .ForMember(dest => dest.AvatarUrl, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        // =====================================================
        // Author -> AuthorDetailDto
        // =====================================================
        CreateMap<Author, AuthorDetailDto>()
            .IncludeBase<Author, AuthorDto>()
            .ForMember(dest => dest.Books, opt => opt.Ignore()); // Set separately

        // =====================================================
        // ExternalAuthorIdentifiers -> ExternalAuthorIdentifiersDto
        // =====================================================
        CreateMap<ExternalAuthorIdentifiers, ExternalAuthorIdentifiersDto>()
            .ForMember(dest => dest.OpenLibraryUrl, opt => opt.MapFrom(src => src.OpenLibraryUrl));
    }
}