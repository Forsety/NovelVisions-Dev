using System;
using System.Linq;
using Ardalis.Specification;
using NovelVision.Services.Catalog.Domain.Aggregates.BookAggregate;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;
using NovelVision.Services.Catalog.Domain.ValueObjects;

namespace NovelVision.Services.Catalog.Domain.Specifications;

public sealed class BookByStatusSpec : Specification<Book>
{
    public BookByStatusSpec(BookStatus status)
    {
        Query
            .Where(book => book.Status == status)
            .OrderByDescending(book => book.CreatedAt);
    }
}

public sealed class PublishedBooksSpec : Specification<Book>
{
    public PublishedBooksSpec()
    {
        Query
            .Where(book => book.Status == BookStatus.Published)
            .OrderByDescending(book => book.CreatedAt)
            .Include(book => book.Chapters);
    }
}

public sealed class BooksByAuthorSpec : Specification<Book>
{
    public BooksByAuthorSpec(AuthorId authorId)
    {
        Query
            .Where(book => book.AuthorId == authorId)
            .OrderByDescending(book => book.CreatedAt);
    }
}

public sealed class BooksByGenreSpec : Specification<Book>
{
    public BooksByGenreSpec(string genre)
    {
        Query
            .Where(book => book.Genres.Contains(genre))
            .Where(book => book.Status == BookStatus.Published)
            .OrderByDescending(book => book.CreatedAt);
    }
}

public sealed class BooksByLanguageSpec : Specification<Book>
{
    public BooksByLanguageSpec(Language language)
    {
        Query
            .Where(book => book.Metadata.Language == language.ToString())
            .Where(book => book.Status == BookStatus.Published);
    }
}

public sealed class BooksWithVisualizationSpec : Specification<Book>
{
    public BooksWithVisualizationSpec()
    {
        Query
            .Where(book => book.VisualizationMode != VisualizationMode.None)
            .Where(book => book.Status == BookStatus.Published)
            .OrderByDescending(book => book.CreatedAt);
    }
}

// Композитная спецификация для поиска
public sealed class BookSearchSpec : Specification<Book>
{
    public BookSearchSpec(
        string? searchTerm = null,
        BookStatus? status = null,
        Language? language = null,
        string? genre = null,
        int? minPages = null,
        int? maxPages = null)
    {
        Query.Where(book => true); // Start with all books

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            Query.Where(book => 
                book.Metadata.Title.Contains(searchTerm) ||
                (book.Metadata.Description != null && book.Metadata.Description.Contains(searchTerm)));
        }

        if (status != null)
        {
            Query.Where(book => book.Status == status);
        }

        if (language != null)
        {
            Query.Where(book => book.Metadata.Language == language.ToString());
        }

        if (!string.IsNullOrWhiteSpace(genre))
        {
            Query.Where(book => book.Genres.Contains(genre));
        }

        if (minPages.HasValue)
        {
            Query.Where(book => book.Metadata.PageCount >= minPages.Value);
        }

        if (maxPages.HasValue)
        {
            Query.Where(book => book.Metadata.PageCount <= maxPages.Value);
        }

        Query.OrderByDescending(book => book.CreatedAt);
    }
}
