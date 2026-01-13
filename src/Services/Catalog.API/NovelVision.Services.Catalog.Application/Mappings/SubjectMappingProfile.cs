// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Mappings/SubjectMappingProfile.cs
using AutoMapper;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Entities;

namespace NovelVision.Services.Catalog.Application.Mappings;

public class SubjectMappingProfile : Profile
{
    public SubjectMappingProfile()
    {
        // Subject -> SubjectDto
        CreateMap<Subject, SubjectDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.Name))
            .ForMember(dest => dest.TypeDescription, opt => opt.MapFrom(src => src.Type.Description))
            .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId != null ? src.ParentId.Value : (Guid?)null))
            .ForMember(dest => dest.ParentName, opt => opt.Ignore()) // Set separately
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Slug))
            .ForMember(dest => dest.BookCount, opt => opt.MapFrom(src => src.BookCount))
            .ForMember(dest => dest.ExternalMapping, opt => opt.MapFrom(src => src.ExternalMapping))
            .ForMember(dest => dest.IsRoot, opt => opt.MapFrom(src => src.IsRoot));

        // Subject -> SubjectListDto
        CreateMap<Subject, SubjectListDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.Name))
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Slug))
            .ForMember(dest => dest.BookCount, opt => opt.MapFrom(src => src.BookCount))
            .ForMember(dest => dest.HasChildren, opt => opt.Ignore()); // Set separately
    }
}