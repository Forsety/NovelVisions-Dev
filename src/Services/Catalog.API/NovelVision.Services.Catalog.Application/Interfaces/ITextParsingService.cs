// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Interfaces/ITextParsingService.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NovelVision.BuildingBlocks.SharedKernel.Results;

namespace NovelVision.Services.Catalog.Application.Interfaces;

public interface ITextParsingService
{
    Task<Result<List<ParsedChapter>>> ParseTextIntoChaptersAsync(
        string text,
        TextFormat format,
        CancellationToken cancellationToken = default);

    Task<Result<List<ParsedPage>>> SplitChapterIntoPagesAsync(
        string chapterContent,
        int wordsPerPage = 300,
        CancellationToken cancellationToken = default);

    Task<Result<ParsedBook>> ParseBookAsync(
        string text,
        TextFormat format,
        int wordsPerPage = 300,
        CancellationToken cancellationToken = default);

    Task<Result<ExtractedMetadata>> ExtractMetadataAsync(
        string text,
        CancellationToken cancellationToken = default);

    int CountWords(string text);
    TimeSpan EstimateReadingTime(int wordCount, int wordsPerMinute = 250);
}

public enum TextFormat
{
    PlainText,
    Html,
    Markdown
}

public class ParsedBook
{
    public List<ParsedChapter> Chapters { get; set; } = new();
    public int TotalWordCount { get; set; }
    public int TotalPageCount { get; set; }
    public ExtractedMetadata? Metadata { get; set; } = null;
}

public class ParsedChapter
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public List<ParsedPage> Pages { get; set; } = new();
}

public class ParsedPage
{
    public int PageNumber { get; set; }
    public string Content { get; set; } = string.Empty;
    public int WordCount { get; set; }
}

public class ExtractedMetadata
{
    public string? Title { get; set; } = null;
    public string? Author { get; set; } = null;
    public string? Language { get; set; } = null;
    public string? ReleaseDate { get; set; } = null;
    public string? Subject { get; set; } = null;
}