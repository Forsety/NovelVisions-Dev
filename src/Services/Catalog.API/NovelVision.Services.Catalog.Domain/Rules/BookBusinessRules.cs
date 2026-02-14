namespace NovelVision.Services.Catalog.Domain.Rules;

/// <summary>
/// Business rules and constants for the Book domain
/// </summary>
public static class BookBusinessRules
{
    public const int MinTitleLength = 1;
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 2000;
    public const int MaxChaptersPerBook = 100;
    public const int MaxPagesPerChapter = 500;
    public const int MaxGenresPerBook = 5;
    public const int MaxTagsPerBook = 20;
    public const int MaxVisualizationPromptsPerPage = 5;
    public const int MinPagesForPublishing = 1;
    public const int MaxBookSizeMB = 50;
    
    // Reading speed assumptions
    public const int AverageWordsPerMinute = 250;
    public const int AverageWordsPerPage = 250;
    
    // Visualization rules
    public const int MaxImagesPerBook = 500;
    public const int MaxImageGenerationRetries = 3;
    public static readonly TimeSpan ImageGenerationTimeout = TimeSpan.FromMinutes(2);
    
    // Cache durations
    public static readonly TimeSpan BookCacheDuration = TimeSpan.FromHours(1);
    public static readonly TimeSpan ImageCacheDuration = TimeSpan.FromDays(7);
}
