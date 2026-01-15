// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/Services/Import/GutenbergTextParser.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace NovelVision.Services.Catalog.Infrastructure.Services.Import;

/// <summary>
/// Парсер текстов книг из Project Gutenberg
/// </summary>
public class GutenbergTextParser
{
    private readonly ILogger<GutenbergTextParser> _logger;

    // Паттерны для определения начала и конца основного текста Gutenberg
    private static readonly Regex StartPatterns = new(
        @"\*\*\*\s*START\s+OF\s+(THE|THIS)\s+PROJECT\s+GUTENBERG\s+EBOOK|" +
        @"END\s+OF\s+(THE|THIS)\s+PROJECT\s+GUTENBERG\s+EBOOK\s+LICENSE|" +
        @"START\s+OF\s+THE\s+PROJECT\s+GUTENBERG|" +
        @"Produced\s+by",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex EndPatterns = new(
        @"\*\*\*\s*END\s+OF\s+(THE|THIS)\s+PROJECT\s+GUTENBERG\s+EBOOK|" +
        @"End\s+of\s+(the|this)\s+Project\s+Gutenberg|" +
        @"End\s+of\s+Project\s+Gutenberg",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Паттерны для определения глав
    private static readonly Regex ChapterPatterns = new(
        @"^(?:CHAPTER|BOOK|PART|VOLUME|SECTION|ACT|SCENE)\s+[IVXLCDM0-9]+\.?\s*[-–—]?\s*|" +
        @"^(?:Chapter|Book|Part|Volume|Section|Act|Scene)\s+[IVXLCDMivxlcdm0-9]+\.?\s*[-–—]?\s*|" +
        @"^[IVXLCDM]+\.\s+|" +
        @"^[0-9]+\.\s+[A-Z]",
        RegexOptions.Multiline | RegexOptions.Compiled);

    public GutenbergTextParser(ILogger<GutenbergTextParser> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Извлекает основной текст книги, удаляя заголовки и футеры Gutenberg
    /// </summary>
    public string ExtractMainText(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return string.Empty;

        var lines = rawText.Split('\n');
        var startIndex = 0;
        var endIndex = lines.Length - 1;

        // Поиск начала основного текста
        for (int i = 0; i < lines.Length; i++)
        {
            if (StartPatterns.IsMatch(lines[i]))
            {
                startIndex = i + 1;
                break;
            }
        }

        // Поиск конца основного текста
        for (int i = lines.Length - 1; i >= startIndex; i--)
        {
            if (EndPatterns.IsMatch(lines[i]))
            {
                endIndex = i - 1;
                break;
            }
        }

        // Собираем текст
        var mainText = string.Join("\n", lines.Skip(startIndex).Take(endIndex - startIndex + 1));

        _logger.LogDebug("Extracted text from line {Start} to {End}", startIndex, endIndex);

        return mainText.Trim();
    }

    /// <summary>
    /// Разбивает текст на главы
    /// </summary>
    public List<ParsedChapter> ParseChapters(string text, bool parseChapters)
    {
        var chapters = new List<ParsedChapter>();

        if (string.IsNullOrWhiteSpace(text))
            return chapters;

        var lines = text.Split('\n');
        var currentChapter = new ParsedChapter { Title = "Prologue", OrderIndex = 0 };
        var chapterContent = new List<string>();
        var chapterIndex = 1;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (ChapterPatterns.IsMatch(trimmedLine) && trimmedLine.Length < 100)
            {
                // Сохраняем предыдущую главу
                if (chapterContent.Any())
                {
                    currentChapter.Content = string.Join("\n", chapterContent).Trim();
                    if (!string.IsNullOrWhiteSpace(currentChapter.Content))
                    {
                        chapters.Add(currentChapter);
                    }
                }

                // Начинаем новую главу
                currentChapter = new ParsedChapter
                {
                    Title = ExtractChapterTitle(trimmedLine),
                    OrderIndex = chapterIndex++
                };
                chapterContent.Clear();
            }
            else
            {
                chapterContent.Add(line);
            }
        }

        // Добавляем последнюю главу
        if (chapterContent.Any())
        {
            currentChapter.Content = string.Join("\n", chapterContent).Trim();
            if (!string.IsNullOrWhiteSpace(currentChapter.Content))
            {
                chapters.Add(currentChapter);
            }
        }

        // Если глав не найдено, создаём одну главу со всем текстом
        if (chapters.Count == 0)
        {
            chapters.Add(new ParsedChapter
            {
                Title = "Content",
                Content = text,
                OrderIndex = 1
            });
        }

        _logger.LogInformation("Parsed {Count} chapters from text", chapters.Count);

        return chapters;
    }

    /// <summary>
    /// Разбивает текст на страницы
    /// </summary>
    public List<ParsedPage> ParsePages(string text, int wordsPerPage = 300)
    {
        var pages = new List<ParsedPage>();

        if (string.IsNullOrWhiteSpace(text))
            return pages;

        var words = text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var pageNumber = 1;
        var currentPageWords = new List<string>();

        foreach (var word in words)
        {
            currentPageWords.Add(word);

            if (currentPageWords.Count >= wordsPerPage)
            {
                pages.Add(new ParsedPage
                {
                    PageNumber = pageNumber++,
                    Content = string.Join(" ", currentPageWords),
                    WordCount = currentPageWords.Count
                });
                currentPageWords.Clear();
            }
        }

        // Добавляем остаток
        if (currentPageWords.Any())
        {
            pages.Add(new ParsedPage
            {
                PageNumber = pageNumber,
                Content = string.Join(" ", currentPageWords),
                WordCount = currentPageWords.Count
            });
        }

        _logger.LogInformation("Parsed {Count} pages ({TotalWords} words)", pages.Count, words.Length);

        return pages;
    }

    /// <summary>
    /// Подсчёт слов в тексте
    /// </summary>
    public int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private string ExtractChapterTitle(string line)
    {
        // Очищаем заголовок главы
        var title = line
            .Replace("CHAPTER", "Chapter")
            .Replace("BOOK", "Book")
            .Replace("PART", "Part")
            .Replace("VOLUME", "Volume")
            .Replace("SECTION", "Section")
            .Trim();

        // Ограничиваем длину
        if (title.Length > 100)
            title = title.Substring(0, 97) + "...";

        return title;
    }
    public ParsedBookText Parse(string rawText, bool parseChapters, int wordsPerPage = 300)
    {
        var mainText = ExtractMainText(rawText);
        var totalWords = CountWords(mainText);

        // 1) Главы
        var chapters = ParseChapters(mainText, parseChapters);

        // 2) Страницы + summary
        var resultChapters = new List<ParsedChapterData>(chapters.Count);

        foreach (var ch in chapters)
        {
            var pages = ParsePages(ch.Content, wordsPerPage)
                .Select(p => p.Content)
                .ToList();

            resultChapters.Add(new ParsedChapterData
            {
                Title = string.IsNullOrWhiteSpace(ch.Title) ? $"Chapter {ch.OrderIndex}" : ch.Title,
                Summary = BuildSummary(ch.Content),
                Pages = pages
            });
        }

        return new ParsedBookText
        {
            Chapters = resultChapters,
            TotalWordCount = totalWords
        };
    }

    // Для совместимости с текущим BookImportService (если он вызывает ParseBookText)
    public ParsedBookText ParseBookText(string rawText, bool parseChapters, int wordsPerPage = 300)
        => Parse(rawText, parseChapters, wordsPerPage);

    private static string? BuildSummary(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;

        // Очень простой “summary”: первые ~250 символов без лишних пробелов.
        var normalized = string.Join(" ", content
            .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries));

        const int max = 250;
        if (normalized.Length <= max) return normalized;

        return normalized.Substring(0, max).Trim() + "…";
    }

}

/// <summary>
/// Разобранная глава
/// </summary>
public class ParsedChapter
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int WordCount => Content?.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
}

/// <summary>
/// Разобранная страница
/// </summary>
public class ParsedPage
{
    public int PageNumber { get; set; }
    public string Content { get; set; } = string.Empty;
    public int WordCount { get; set; }
}
public sealed class ParsedBookText
{
    public List<ParsedChapterData> Chapters { get; init; } = new();
    public int TotalWordCount { get; init; }
}

public sealed class ParsedChapterData
{
    public string Title { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public List<string> Pages { get; init; } = new();
}
