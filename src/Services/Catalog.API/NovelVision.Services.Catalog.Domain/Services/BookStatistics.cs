namespace NovelVision.Services.Catalog.Domain.Services;

public sealed record BookStatistics(
    int ChapterCount,
    int PageCount,
    int WordCount,
    TimeSpan EstimatedReadingTime,
    Dictionary<string, int> ChapterWordCounts,
    double AverageWordsPerPage,
    double AverageWordsPerChapter)
{
    public string ReadingTimeFormatted => 
        $"{(int)EstimatedReadingTime.TotalHours}h {EstimatedReadingTime.Minutes}min";
}
