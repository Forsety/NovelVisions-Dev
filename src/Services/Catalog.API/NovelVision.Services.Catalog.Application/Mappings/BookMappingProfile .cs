// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Mappings/BookMappingProfile.cs
// ИСПРАВЛЕННАЯ ВЕРСИЯ - Правильный маппинг TotalPageCount и TotalWordCount
using System;
using System.Linq;
using AutoMapper;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Aggregates.BookAggregate;
using NovelVision.Services.Catalog.Domain.Entities;
using NovelVision.Services.Catalog.Domain.ValueObjects;

namespace NovelVision.Services.Catalog.Application.Mappings;

public class BookMappingProfile : Profile
{
    public BookMappingProfile()
    {
        // =====================================================
        // Book -> BookDto (основной маппинг для API)
        // =====================================================
        CreateMap<Book, BookDto>()
            // Core identity
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Metadata.Title))
            .ForMember(dest => dest.Subtitle, opt => opt.MapFrom(src => src.Metadata.Subtitle))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Metadata.Description))
            .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Metadata.Language ?? "en"))
            .ForMember(dest => dest.LanguageCode, opt => opt.MapFrom(src => src.Metadata.Language ?? "en"))

            // Cover
            .ForMember(dest => dest.CoverImageUrl, opt => opt.MapFrom(src => src.CoverImage != null ? src.CoverImage.Url : null))
            .ForMember(dest => dest.HasCover, opt => opt.MapFrom(src => src.CoverImage != null && !string.IsNullOrEmpty(src.CoverImage.Url)))

            // Author
            .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.AuthorId.Value))
            .ForMember(dest => dest.AuthorName, opt => opt.Ignore()) // Set separately or via Include

            // Status
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name))
            .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.IsPublished))
            .ForMember(dest => dest.CopyrightStatus, opt => opt.MapFrom(src => src.CopyrightStatus.Name))
            .ForMember(dest => dest.IsFreeToUse, opt => opt.MapFrom(src => src.IsPublicDomain))

            // === КРИТИЧЕСКИЙ МАППИНГ: Content Statistics ===
            // TotalPageCount -> PageCount (computed property из глав)
            .ForMember(dest => dest.PageCount, opt => opt.MapFrom(src => src.TotalPageCount))
            // TotalWordCount -> WordCount (computed property из глав)
            .ForMember(dest => dest.WordCount, opt => opt.MapFrom(src => src.TotalWordCount))
            // ChapterCount (уже называется правильно)
            .ForMember(dest => dest.ChapterCount, opt => opt.MapFrom(src => src.ChapterCount))
            // EstimatedReadingTime (computed property)
            .ForMember(dest => dest.EstimatedReadingTime, opt => opt.MapFrom(src => src.EstimatedReadingTime))
            .ForMember(dest => dest.ReadingTimeMinutes, opt => opt.MapFrom(src => (int)src.EstimatedReadingTime.TotalMinutes))

            // Categories
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.Genres.ToList()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.ToList()))

            // Statistics
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.AverageRating : 0m))
            .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.ReviewCount : 0))
            .ForMember(dest => dest.DownloadCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.DownloadCount : 0))
            .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.ViewCount : 0))
            .ForMember(dest => dest.FavoriteCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.FavoriteCount : 0))

            // External source
            .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source.Name))
            .ForMember(dest => dest.ExternalSource, opt => opt.MapFrom(src => src.ExternalIds != null ? src.ExternalIds.SourceType.Name : null))
            .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalIds != null ? src.ExternalIds.ExternalId : null))
            .ForMember(dest => dest.ExternalUrl, opt => opt.MapFrom(src => src.ExternalIds != null ? src.ExternalIds.SourceUrl : null))
            .ForMember(dest => dest.IsFromExternalSource, opt => opt.MapFrom(src => src.IsFromExternalSource))

            // Visualization
            .ForMember(dest => dest.HasVisualization, opt => opt.MapFrom(src => src.IsVisualizationEnabled))
            .ForMember(dest => dest.VisualizationMode, opt => opt.MapFrom(src => src.VisualizationMode != null ? src.VisualizationMode.Name : "None"))
            .ForMember(dest => dest.VisualizationSettings, opt => opt.MapFrom(src => src.VisualizationSettings))

            // Dates
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

        // =====================================================
        // Book -> BookListDto (для списков)
        // =====================================================
        CreateMap<Book, BookListDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Metadata.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src =>
                src.Metadata.Description != null && src.Metadata.Description.Length > 200
                    ? src.Metadata.Description.Substring(0, 200) + "..."
                    : src.Metadata.Description))
            .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Metadata.Language ?? "en"))
            .ForMember(dest => dest.CoverImageUrl, opt => opt.MapFrom(src => src.CoverImage != null ? src.CoverImage.Url : null))
            .ForMember(dest => dest.HasCover, opt => opt.MapFrom(src => src.CoverImage != null && !string.IsNullOrEmpty(src.CoverImage.Url)))
            .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.AuthorId.Value))
            .ForMember(dest => dest.AuthorName, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name))
            .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.IsPublished))
            .ForMember(dest => dest.CopyrightStatus, opt => opt.MapFrom(src => src.CopyrightStatus.Name))
            .ForMember(dest => dest.IsFree, opt => opt.MapFrom(src => src.IsPublicDomain))
            .ForMember(dest => dest.IsFreeToUse, opt => opt.MapFrom(src => src.IsPublicDomain))
            // === КРИТИЧЕСКИЙ МАППИНГ ===
            .ForMember(dest => dest.ChapterCount, opt => opt.MapFrom(src => src.ChapterCount))
            .ForMember(dest => dest.PageCount, opt => opt.MapFrom(src => src.TotalPageCount))
            .ForMember(dest => dest.WordCount, opt => opt.MapFrom(src => src.TotalWordCount))
            .ForMember(dest => dest.ReadingTimeMinutes, opt => opt.MapFrom(src => (int)src.EstimatedReadingTime.TotalMinutes))
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.Genres.ToList()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.ToList()))
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.AverageRating : 0m))
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.AverageRating : 0m))
            .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.ReviewCount : 0))
            .ForMember(dest => dest.DownloadCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.DownloadCount : 0))
            .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.ViewCount : 0))
            .ForMember(dest => dest.HasVisualization, opt => opt.MapFrom(src => src.IsVisualizationEnabled))
            .ForMember(dest => dest.VisualizationMode, opt => opt.MapFrom(src => src.VisualizationMode != null ? src.VisualizationMode.Name : "None"))
            .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source.Name))
            .ForMember(dest => dest.IsImported, opt => opt.MapFrom(src => src.IsFromExternalSource))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

        // =====================================================
        // Book -> BookDetailDto (детальный просмотр)
        // =====================================================
        CreateMap<Book, BookDetailDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Metadata.Title))
            .ForMember(dest => dest.Subtitle, opt => opt.MapFrom(src => src.Metadata.Subtitle))
            .ForMember(dest => dest.OriginalTitle, opt => opt.MapFrom(src => src.Metadata.OriginalTitle))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Metadata.Description))
            .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Metadata.Language ?? "en"))
            .ForMember(dest => dest.CoverImageUrl, opt => opt.MapFrom(src => src.CoverImage != null ? src.CoverImage.Url : null))
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.CoverImage != null ? src.CoverImage.ThumbnailUrl : null))
            .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.AuthorId.Value))
            .ForMember(dest => dest.AuthorName, opt => opt.Ignore())
            .ForMember(dest => dest.Author, opt => opt.Ignore())
            .ForMember(dest => dest.ISBN, opt => opt.MapFrom(src => src.ISBN != null ? src.ISBN.Value : null))
            .ForMember(dest => dest.Publisher, opt => opt.MapFrom(src => src.PublicationInfo != null ? src.PublicationInfo.Publisher : null))
            .ForMember(dest => dest.PublicationDate, opt => opt.MapFrom(src => src.PublicationInfo != null ? src.PublicationInfo.PublicationDate : null))
            .ForMember(dest => dest.Edition, opt => opt.MapFrom(src => src.PublicationInfo != null ? src.PublicationInfo.Edition : null))
            .ForMember(dest => dest.OriginalPublicationYear, opt => opt.MapFrom(src => src.OriginalPublicationYear))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name))
            .ForMember(dest => dest.CopyrightStatus, opt => opt.MapFrom(src => src.CopyrightStatus.Name))
            .ForMember(dest => dest.IsFreeToUse, opt => opt.MapFrom(src => src.IsPublicDomain))
            .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.IsPublished))
            // === КРИТИЧЕСКИЙ МАППИНГ ===
            .ForMember(dest => dest.ChapterCount, opt => opt.MapFrom(src => src.ChapterCount))
            .ForMember(dest => dest.PageCount, opt => opt.MapFrom(src => src.TotalPageCount))
            .ForMember(dest => dest.WordCount, opt => opt.MapFrom(src => src.TotalWordCount))
            .ForMember(dest => dest.EstimatedReadingTime, opt => opt.MapFrom(src => src.EstimatedReadingTime))
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.Genres.ToList()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.ToList()))
            .ForMember(dest => dest.Subjects, opt => opt.Ignore())
            .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source.Name))
            .ForMember(dest => dest.ExternalSource, opt => opt.MapFrom(src => src.ExternalIds != null ? src.ExternalIds.SourceType.Name : null))
            .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalIds != null ? src.ExternalIds.ExternalId : null))
            .ForMember(dest => dest.ExternalUrl, opt => opt.MapFrom(src => src.ExternalIds != null ? src.ExternalIds.SourceUrl : null))
            .ForMember(dest => dest.IsFromExternalSource, opt => opt.MapFrom(src => src.IsFromExternalSource))
            .ForMember(dest => dest.DownloadCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.DownloadCount : 0))
            .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.ViewCount : 0))
            .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.AverageRating : 0m))
            .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.ReviewCount : 0))
            .ForMember(dest => dest.FavoriteCount, opt => opt.MapFrom(src => src.Statistics != null ? src.Statistics.FavoriteCount : 0))
            .ForMember(dest => dest.VisualizationSettings, opt => opt.MapFrom(src => src.VisualizationSettings))
            .ForMember(dest => dest.HasVisualization, opt => opt.MapFrom(src => src.IsVisualizationEnabled))
            .ForMember(dest => dest.VisualizationMode, opt => opt.MapFrom(src => src.VisualizationMode != null ? src.VisualizationMode.Name : "None"))
            .ForMember(dest => dest.HasFullText, opt => opt.MapFrom(src => src.HasFullText))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

        // =====================================================
        // VisualizationSettings -> VisualizationSettingsDto
        // =====================================================
        CreateMap<VisualizationSettings, VisualizationSettingsDto>()
            .ForMember(dest => dest.Mode, opt => opt.MapFrom(src => src.PrimaryMode != null ? src.PrimaryMode.Name : "None"))
            .ForMember(dest => dest.PrimaryMode, opt => opt.MapFrom(src => src.PrimaryMode != null ? src.PrimaryMode.Name : "None"))
            .ForMember(dest => dest.AllowReaderChoice, opt => opt.MapFrom(src => src.AllowReaderChoice))
            .ForMember(dest => dest.AllowedModes, opt => opt.MapFrom(src => src.AllowedModes != null ? src.AllowedModes.Select(m => m.Name).ToList() : new System.Collections.Generic.List<string>()))
            .ForMember(dest => dest.MaxImagesPerPage, opt => opt.MapFrom(src => src.MaxImagesPerPage))
            .ForMember(dest => dest.AutoGenerateOnPublish, opt => opt.MapFrom(src => src.AutoGenerateOnPublish))
            .ForMember(dest => dest.IsEnabled, opt => opt.MapFrom(src => src.IsEnabled));

        // =====================================================
        // Chapter -> ChapterDto
        // =====================================================
        CreateMap<Chapter, ChapterDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.BookId.Value))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Summary, opt => opt.MapFrom(src => src.Summary))
            .ForMember(dest => dest.OrderIndex, opt => opt.MapFrom(src => src.OrderIndex))
            .ForMember(dest => dest.PageCount, opt => opt.MapFrom(src => src.PageCount))
            .ForMember(dest => dest.WordCount, opt => opt.MapFrom(src => src.TotalWordCount))
            .ForMember(dest => dest.EstimatedReadingTime, opt => opt.MapFrom(src => src.EstimatedReadingTime))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

        // =====================================================
        // Page -> PageDto
        // =====================================================
        CreateMap<Page, PageDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.ChapterId, opt => opt.MapFrom(src => src.ChapterId.Value))
            .ForMember(dest => dest.PageNumber, opt => opt.MapFrom(src => src.PageNumber))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.WordCount, opt => opt.MapFrom(src => src.WordCount))
            .ForMember(dest => dest.HasVisualization, opt => opt.MapFrom(src => src.HasVisualization))
            .ForMember(dest => dest.VisualizationImageUrl, opt => opt.MapFrom(src => src.VisualizationImageUrl))
            .ForMember(dest => dest.VisualizationThumbnailUrl, opt => opt.MapFrom(src => src.VisualizationThumbnailUrl))
            .ForMember(dest => dest.IsVisualizationPoint, opt => opt.MapFrom(src => src.IsVisualizationPoint))
            .ForMember(dest => dest.AuthorVisualizationHint, opt => opt.MapFrom(src => src.AuthorVisualizationHint));
    }
}