// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/Services/External/TextParsingService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.Interfaces;

namespace NovelVision.Services.Catalog.Infrastructure.Services.External;

public class TextParsingService : ITextParsingService
{
    private readonly ILogger<TextParsingService> _logger;

    // Regex patterns for chapter detection
    private static readonly Regex[] ChapterPatterns = new[]
    {
        new Regex(@"^CHAPTER\s+(\d+|[IVXLCDM]+)\.?\s*(.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline),
        new Regex(@"^Chapter\s+(\d+|[IVXLCDM]+)\.?\s*(.*)$", RegexOptions.Multiline),
        new Regex(@"^BOOK\s+(\d+|[IVXLCDM]+)\.?\s*(.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline),
        new Regex(@"^Part\s+(\d+|[IVXLCDM]+)\.?\s*(.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline),
        new Regex(@"^(\d+)\.\s+(.+)$", RegexOptions.Multiline),
        new Regex(@"^\*\*\*\s*(.+)\s*\*\*\*$", RegexOptions.Multiline)
    };

    public TextParsingService(ILogger<TextParsingService> logger)
    {
        _logger = logger;
    }

    public Task<Result<List<ParsedChapter>>> ParseTextIntoChaptersAsync(
        string text,
        TextFormat format,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Task.FromResult(Result<List<ParsedChapter>>.Failure("Text is empty"));
            }

            // Clean up text based on format
            var cleanedText = format switch
            {
                TextFormat.Html => StripHtml(text),
                _ => text
            };

            // Remove Gutenberg header/footer
            cleanedText = RemoveGutenbergBoilerplate(cleanedText);

            var chapters = new List<ParsedChapter>();
            var chapterMatches = FindChapterBoundaries(cleanedText);

            if (chapterMatches.Count == 0)
            {
                // No chapters found - treat entire text as single chapter
                chapters.Add(new ParsedChapter
                {
                    Title = "Chapter 1",
                    Content = cleanedText.Trim(),
                    OrderIndex = 1,
                    Pages = new List<ParsedPage>()
                });
            }
            else
            {
                for (var i = 0; i < chapterMatches.Count; i++)
                {
                    var match = chapterMatches[i];
                    var startIndex = match.Index + match.Length;
                    var endIndex = i < chapterMatches.Count - 1
                        ? chapterMatches[i + 1].Index
                        : cleanedText.Length;

                    var chapterContent = cleanedText.Substring(startIndex, endIndex - startIndex).Trim();
                    var chapterTitle = ExtractChapterTitle(match);

                    chapters.Add(new ParsedChapter
                    {
                        Title = chapterTitle,
                        Content = chapterContent,
                        OrderIndex = i + 1,
                        Pages = new List<ParsedPage>()
                    });
                }
            }

            _logger.LogInformation("Parsed {Count} chapters from text", chapters.Count);
            return Task.FromResult(Result<List<ParsedChapter>>.Success(chapters));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing text into chapters");
            return Task.FromResult(Result<List<ParsedChapter>>.Failure($"Parse error: {ex.Message}"));
        }
    }

    public Task<Result<List<ParsedPage>>> SplitChapterIntoPagesAsync(
        string chapterContent,
        int wordsPerPage = 300,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pages = new List<ParsedPage>();
            var words = chapterContent.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var pageNumber = 1;

            for (var i = 0; i < words.Length; i += wordsPerPage)
            {
                var pageWords = words.Skip(i).Take(wordsPerPage).ToArray();
                var pageContent = string.Join(" ", pageWords);

                pages.Add(new ParsedPage
                {
                    PageNumber = pageNumber++,
                    Content = pageContent,
                    WordCount = pageWords.Length
                });
            }

            return Task.FromResult(Result<List<ParsedPage>>.Success(pages));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<List<ParsedPage>>.Failure($"Error splitting into pages: {ex.Message}"));
        }
    }

    public async Task<Result<ParsedBook>> ParseBookAsync(
        string text,
        TextFormat format,
        int wordsPerPage = 300,
        CancellationToken cancellationToken = default)
    {
        var chaptersResult = await ParseTextIntoChaptersAsync(text, format, cancellationToken);
        if (chaptersResult.IsFailed)
        {
            return Result<ParsedBook>.Failure(chaptersResult.Errors.First().Message);
        }

        var chapters = chaptersResult.Value;
        var totalWordCount = 0;
        var totalPageCount = 0;

        foreach (var chapter in chapters)
        {
            var pagesResult = await SplitChapterIntoPagesAsync(chapter.Content, wordsPerPage, cancellationToken);
            if (pagesResult.IsSucceeded)
            {
                chapter.Pages = pagesResult.Value;
                totalWordCount += pagesResult.Value.Sum(p => p.WordCount);
                totalPageCount += pagesResult.Value.Count;
            }
        }

        var metadata = await ExtractMetadataAsync(text, cancellationToken);

        return Result<ParsedBook>.Success(new ParsedBook
        {
            Chapters = chapters,
            TotalWordCount = totalWordCount,
            TotalPageCount = totalPageCount,
            Metadata = metadata.IsSucceeded ? metadata.Value : null
        });
    }

    public Task<Result<ExtractedMetadata>> ExtractMetadataAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            var metadata = new ExtractedMetadata();

            // Try to extract title from Gutenberg format
            var titleMatch = Regex.Match(text, @"Title:\s*(.+)", RegexOptions.IgnoreCase);
            if (titleMatch.Success)
            {
                metadata.Title = titleMatch.Groups[1].Value.Trim();
            }

            // Try to extract author
            var authorMatch = Regex.Match(text, @"Author:\s*(.+)", RegexOptions.IgnoreCase);
            if (authorMatch.Success)
            {
                metadata.Author = authorMatch.Groups[1].Value.Trim();
            }

            // Try to extract language
            var langMatch = Regex.Match(text, @"Language:\s*(.+)", RegexOptions.IgnoreCase);
            if (langMatch.Success)
            {
                metadata.Language = langMatch.Groups[1].Value.Trim();
            }

            // Try to extract release date
            var dateMatch = Regex.Match(text, @"Release Date:\s*(.+)", RegexOptions.IgnoreCase);
            if (dateMatch.Success)
            {
                metadata.ReleaseDate = dateMatch.Groups[1].Value.Trim();
            }

            return Task.FromResult(Result<ExtractedMetadata>.Success(metadata));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<ExtractedMetadata>.Failure($"Error extracting metadata: {ex.Message}"));
        }
    }

    public int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public TimeSpan EstimateReadingTime(int wordCount, int wordsPerMinute = 250)
    {
        return TimeSpan.FromMinutes((double)wordCount / wordsPerMinute);
    }

    private static string StripHtml(string html)
    {
        // Remove script and style blocks
        html = Regex.Replace(html, @"<(script|style)[^>]*>.*?</\1>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        // Remove HTML tags
        html = Regex.Replace(html, @"<[^>]+>", " ");
        // Decode HTML entities
        html = System.Net.WebUtility.HtmlDecode(html);
        // Normalize whitespace
        html = Regex.Replace(html, @"\s+", " ");
        return html.Trim();
    }

    private static string RemoveGutenbergBoilerplate(string text)
    {
        // Remove header (everything before "*** START OF")
        var startMatch = Regex.Match(text, @"\*\*\*\s*START OF (THE|THIS) PROJECT GUTENBERG.*?\*\*\*", RegexOptions.IgnoreCase);
        if (startMatch.Success)
        {
            text = text.Substring(startMatch.Index + startMatch.Length);
        }

        // Remove footer (everything after "*** END OF")
        var endMatch = Regex.Match(text, @"\*\*\*\s*END OF (THE|THIS) PROJECT GUTENBERG", RegexOptions.IgnoreCase);
        if (endMatch.Success)
        {
            text = text.Substring(0, endMatch.Index);
        }

        return text.Trim();
    }

    private static List<Match> FindChapterBoundaries(string text)
    {
        var matches = new List<Match>();

        foreach (var pattern in ChapterPatterns)
        {
            var patternMatches = pattern.Matches(text);
            if (patternMatches.Count > 2) // Need at least 3 chapters to consider it valid
            {
                matches = patternMatches.Cast<Match>().ToList();
                break;
            }
        }

        return matches.OrderBy(m => m.Index).ToList();
    }

    private static string ExtractChapterTitle(Match match)
    {
        var groups = match.Groups;

        // Try to get chapter number and title
        if (groups.Count >= 3 && !string.IsNullOrWhiteSpace(groups[2].Value))
        {
            return $"Chapter {groups[1].Value}: {groups[2].Value.Trim()}";
        }
        else if (groups.Count >= 2)
        {
            return $"Chapter {groups[1].Value}";
        }

        return match.Value.Trim();
    }
}