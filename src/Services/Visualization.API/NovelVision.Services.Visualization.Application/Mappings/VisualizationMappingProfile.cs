using AutoMapper;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Domain.Aggregates.VisualizationJobAggregate;
using NovelVision.Services.Visualization.Domain.ValueObjects;

namespace NovelVision.Services.Visualization.Application.Mappings;

/// <summary>
/// AutoMapper профиль для Visualization
/// </summary>
public sealed class VisualizationMappingProfile : Profile
{
    public VisualizationMappingProfile()
    {
        // VisualizationJob -> VisualizationJobDto
        CreateMap<VisualizationJob, VisualizationJobDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.Trigger, opt => opt.MapFrom(src => src.Trigger.Name))
            .ForMember(dest => dest.TriggerDisplayName, opt => opt.MapFrom(src => src.Trigger.DisplayName))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name))
            .ForMember(dest => dest.StatusDisplayName, opt => opt.MapFrom(src => src.Status.DisplayName))
            .ForMember(dest => dest.PreferredProvider, opt => opt.MapFrom(src => src.PreferredProvider.Name))
            .ForMember(dest => dest.PreferredProviderDisplayName, opt => opt.MapFrom(src => src.PreferredProvider.DisplayName))
            .ForMember(dest => dest.ProcessingTimeSeconds, opt => opt.MapFrom(src => 
                src.ProcessingTime.HasValue ? src.ProcessingTime.Value.TotalSeconds : (double?)null))
            .ForMember(dest => dest.ImageCount, opt => opt.MapFrom(src => src.Images.Count(i => !i.IsDeleted)))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.Where(i => !i.IsDeleted)))
            .ForMember(dest => dest.SelectedImage, opt => opt.MapFrom(src => src.SelectedImage))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        // VisualizationJob -> VisualizationJobSummaryDto
        CreateMap<VisualizationJob, VisualizationJobSummaryDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Name))
            .ForMember(dest => dest.StatusDisplayName, opt => opt.MapFrom(src => src.Status.DisplayName))
            .ForMember(dest => dest.Trigger, opt => opt.MapFrom(src => src.Trigger.Name))
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => 
                src.SelectedImage != null ? src.SelectedImage.Metadata.ThumbnailUrl : null))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        // GeneratedImage -> GeneratedImageDto
        CreateMap<GeneratedImage, GeneratedImageDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.JobId, opt => opt.MapFrom(src => src.JobId.Value))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Metadata.Url))
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.Metadata.ThumbnailUrl))
            .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.Metadata.Width))
            .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Metadata.Height))
            .ForMember(dest => dest.AspectRatio, opt => opt.MapFrom(src => src.Metadata.AspectRatio))
            .ForMember(dest => dest.FileSizeBytes, opt => opt.MapFrom(src => src.Metadata.FileSizeBytes))
            .ForMember(dest => dest.FileSizeFormatted, opt => opt.MapFrom(src => src.Metadata.FileSizeFormatted))
            .ForMember(dest => dest.Format, opt => opt.MapFrom(src => src.Metadata.Format.Name))
            .ForMember(dest => dest.MimeType, opt => opt.MapFrom(src => src.Metadata.Format.MimeType))
            .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src.Provider.Name))
            .ForMember(dest => dest.ProviderDisplayName, opt => opt.MapFrom(src => src.Provider.DisplayName));

        // GeneratedImage -> GeneratedImageSummaryDto
        CreateMap<GeneratedImage, GeneratedImageSummaryDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Metadata.Url))
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.Metadata.ThumbnailUrl))
            .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.Metadata.Width))
            .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Metadata.Height));

        // PromptData -> PromptDataDto
        CreateMap<PromptData, PromptDataDto>()
            .ForMember(dest => dest.TargetModel, opt => opt.MapFrom(src => src.TargetModel.Name));

        // TextSelection -> TextSelectionDto
        CreateMap<TextSelection, TextSelectionDto>()
            .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Length));

        // GenerationParameters -> GenerationParametersDto
        CreateMap<GenerationParameters, GenerationParametersDto>();

        // GenerationParametersDto -> GenerationParameters (reverse)
        CreateMap<GenerationParametersDto, GenerationParameters>()
            .ConstructUsing(src => GenerationParameters.Create(
                src.Size,
                src.Quality,
                src.AspectRatio,
                src.Seed,
                src.Steps,
                src.CfgScale,
                src.Sampler,
                src.Upscale));
    }
}
