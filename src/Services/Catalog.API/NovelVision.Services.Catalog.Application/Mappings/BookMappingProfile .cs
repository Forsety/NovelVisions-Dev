using System;
using System.Linq;
using AutoMapper;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Aggregates.BookAggregate;
using NovelVision.Services.Catalog.Domain.ValueObjects;

namespace NovelVision.Services.Catalog.Application.Mappings;

public class BookMappingProfile : Profile
{
    public BookMappingProfile()
    {
        // =====================================================
        // Book -> BookDto (Full)
        // =====================================================
        CreateMap<Book, BookDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Metadata.Title))
            .ForMember(dest => dest.Subtitle, opt => opt.MapFrom(src => src.Metadata.Subtitle))
            .ForMember(dest => dest.OriginalTitle, opt => opt.MapFrom(src => src.Metadata.OriginalTitle))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Metadata.Description))
            .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Metadata.Language))
            .ForMember(dest => dest.LanguageCode, opt => opt.MapFrom(src => src.Metadata.Language))
            .ForMember(dest => dest.PageCount, opt => opt.MapFrom(src => src.Metadata.PageCount))
            .ForMember(dest => dest.WordCount, opt => opt.MapFrom(src => src.Metadata.WordCount))
            .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.AuthorId.Value))
            .ForMember(dest => dest.AuthorName, opt => opt.Ignore())
            .ForMember(dest => dest.ISBN, opt => opt.MapFrom(src => src.ISBN != null ? src.ISBN.Value : null))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name))
            .ForMember(dest => dest.CopyrightStatus, opt => opt.MapFrom(src => src.CopyrightStatus.Name))
            .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.IsPublished))
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.Genres.ToList()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.ToList()))
            .ForMember(dest => dest.CoverImageUrl, opt => opt.MapFrom(src => src.CoverImage != null ? src.CoverImage.Url : null))
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.CoverImage != null ? src.CoverImage.ThumbnailUrl : null))
            .ForMember(dest => dest.Publisher, opt => opt.MapFrom(src => src.PublicationInfo != null ? src.PublicationInfo.Publisher : null))
            .ForMember(dest => dest.PublicationDate, opt => opt.MapFrom(src => src.PublicationInfo != null ? src.PublicationInfo.PublicationDate : null))
            .ForMember(dest => dest.Edition, opt => opt.MapFrom(src => src.PublicationInfo != null ? src.PublicationInfo.Edition : null))
            .ForMember(dest => dest.ChapterCount, opt => opt.MapFrom(src => src.Chapters.Count))
            .ForMember(dest => dest.VisualizationMode, opt => opt.MapFrom(src => src.VisualizationMode.Name))
            .ForMember(dest => dest.VisualizationSettings, opt => opt.MapFrom(src => src.VisualizationSettings))
            .ForMember(dest => dest.HasVisualization, opt => opt.MapFrom(src => src.VisualizationMode.Value > 0))
            .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source.Name))
            .ForMember(dest => dest.ExternalSource, opt => opt.MapFrom(src => src.ExternalIds != null ? src.ExternalIds.SourceType.Name : null))
            .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalIds != null ? src.ExternalIds.ExternalId : null))
            .ForMember(dest => dest.ExternalUrl, opt => opt.MapFrom(src => src.ExternalIds != null ? src.ExternalIds.SourceUrl : null))
            .ForMember(dest => dest.DownloadCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.DownloadCount : 0))
            .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.ViewCount : 0))
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.AverageRating : 0))
            .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.ReviewCount : 0))
            .ForMember(dest => dest.FavoriteCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.FavoriteCount : 0))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

        // =====================================================
        // Book -> BookListDto (Short)
        // =====================================================
        CreateMap<Book, BookListDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Metadata.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom<DescriptionResolver>())
            .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Metadata.Language))
            .ForMember(dest => dest.PageCount, opt => opt.MapFrom(src => src.Metadata.PageCount))
            .ForMember(dest => dest.WordCount, opt => opt.MapFrom(src => src.Metadata.WordCount))
            .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.AuthorId.Value))
            .ForMember(dest => dest.AuthorName, opt => opt.Ignore())
            .ForMember(dest => dest.CoverImageUrl, opt => opt.MapFrom(src =>
                src.CoverImage != null
                    ? (src.CoverImage.ThumbnailUrl ?? src.CoverImage.Url)
                    : null))
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.Genres.Take(3).ToList()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Take(5).ToList()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name))
            .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.IsPublished))
            .ForMember(dest => dest.DownloadCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.DownloadCount : 0))
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.AverageRating : 0))
            .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.ReviewCount : 0))
            .ForMember(dest => dest.HasVisualization, opt => opt.MapFrom(src => src.VisualizationMode.Value > 0))
            .ForMember(dest => dest.IsFree, opt => opt.MapFrom(src => src.CopyrightStatus.Value <= 1))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        // =====================================================
        // Book -> BookDetailDto (полный маппинг без IncludeBase)
        // =====================================================
        CreateMap<Book, BookDetailDto>()
            // Core Properties
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Metadata.Title))
            .ForMember(dest => dest.Subtitle, opt => opt.MapFrom(src => src.Metadata.Subtitle))
            .ForMember(dest => dest.OriginalTitle, opt => opt.MapFrom(src => src.Metadata.OriginalTitle))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Metadata.Description))
            .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Metadata.Language))
            // Cover
            .ForMember(dest => dest.CoverImageUrl, opt => opt.MapFrom(src => src.CoverImage != null ? src.CoverImage.Url : null))
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.CoverImage != null ? src.CoverImage.ThumbnailUrl : null))
            // Author
            .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.AuthorId.Value))
            .ForMember(dest => dest.AuthorName, opt => opt.Ignore())
            .ForMember(dest => dest.Author, opt => opt.Ignore())
            // Publication
            .ForMember(dest => dest.ISBN, opt => opt.MapFrom(src => src.ISBN != null ? src.ISBN.Value : null))
            .ForMember(dest => dest.Publisher, opt => opt.MapFrom(src => src.PublicationInfo != null ? src.PublicationInfo.Publisher : null))
            .ForMember(dest => dest.PublicationDate, opt => opt.MapFrom(src => src.PublicationInfo != null ? src.PublicationInfo.PublicationDate : null))
            .ForMember(dest => dest.PublicationYear, opt => opt.MapFrom(src => src.PublicationInfo != null && src.PublicationInfo.PublicationDate.HasValue
                ? src.PublicationInfo.PublicationDate.Value.Year : (int?)null))
            .ForMember(dest => dest.OriginalPublicationYear, opt => opt.MapFrom(src => src.OriginalPublicationYear))
            .ForMember(dest => dest.Edition, opt => opt.MapFrom(src => src.PublicationInfo != null ? src.PublicationInfo.Edition : null))
            // Status
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name))
            .ForMember(dest => dest.CopyrightStatus, opt => opt.MapFrom(src => src.CopyrightStatus.Name))
            .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.IsPublished))
            // Content
            .ForMember(dest => dest.PageCount, opt => opt.MapFrom(src => src.Metadata.PageCount))
            .ForMember(dest => dest.WordCount, opt => opt.MapFrom(src => src.WordCount))
            .ForMember(dest => dest.ChapterCount, opt => opt.MapFrom(src => src.Chapters.Count))
            .ForMember(dest => dest.Chapters, opt => opt.MapFrom(src => src.Chapters))
            // Categories
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.Genres.ToList()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.ToList()))
            .ForMember(dest => dest.Subjects, opt => opt.Ignore())
            // Statistics
            .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.ViewCount : 0))
            .ForMember(dest => dest.DownloadCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.DownloadCount : 0))
            .ForMember(dest => dest.FavoriteCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.FavoriteCount : 0))
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.AverageRating : 0))
            .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.ReviewCount : 0))
            // Visualization
            .ForMember(dest => dest.VisualizationMode, opt => opt.MapFrom(src => src.VisualizationMode.Name))
            .ForMember(dest => dest.VisualizationSettings, opt => opt.MapFrom(src => src.VisualizationSettings))
            .ForMember(dest => dest.HasVisualization, opt => opt.MapFrom(src => src.VisualizationMode.Value > 0))
            // External
            .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source.Name))
            .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalIds != null ? src.ExternalIds.ExternalId : null))
            .ForMember(dest => dest.ExternalUrl, opt => opt.MapFrom(src => src.ExternalIds != null ? src.ExternalIds.SourceUrl : null))
            .ForMember(dest => dest.FullTextUrl, opt => opt.MapFrom(src => src.FullTextUrl))
            .ForMember(dest => dest.HasFullText, opt => opt.MapFrom(src => src.HasFullText))
            // Timestamps
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

        // =====================================================
        // VisualizationSettings -> VisualizationSettingsDto
        // =====================================================
        CreateMap<VisualizationSettings, VisualizationSettingsDto>()
            .ForMember(dest => dest.Mode, opt => opt.MapFrom(src => src.PrimaryMode.Name))
            .ForMember(dest => dest.AllowReaderChoice, opt => opt.MapFrom(src => src.AllowReaderChoice))
            .ForMember(dest => dest.AllowedModes, opt => opt.MapFrom(src => src.AllowedModes.Select(m => m.Name).ToList()))
            .ForMember(dest => dest.PreferredStyle, opt => opt.MapFrom(src => src.PreferredStyle))
            .ForMember(dest => dest.PreferredProvider, opt => opt.MapFrom(src => src.PreferredProvider))
            .ForMember(dest => dest.MaxImagesPerPage, opt => opt.MapFrom(src => src.MaxImagesPerPage))
            .ForMember(dest => dest.AutoGenerateOnPublish, opt => opt.MapFrom(src => src.AutoGenerateOnPublish))
            .ForMember(dest => dest.IsEnabled, opt => opt.MapFrom(src => src.IsEnabled));
    }
}

/// <summary>
/// Resolver для Description с truncation
/// </summary>
public class DescriptionResolver : IValueResolver<Book, BookListDto, string?>
{
    private const int MaxLength = 200;

    public string? Resolve(Book source, BookListDto destination, string? destMember, ResolutionContext context)
    {
        var description = source.Metadata?.Description;

        if (string.IsNullOrEmpty(description))
            return null;

        if (description.Length <= MaxLength)
            return description;

        return description.Substring(0, MaxLength - 3) + "...";
    }
}
