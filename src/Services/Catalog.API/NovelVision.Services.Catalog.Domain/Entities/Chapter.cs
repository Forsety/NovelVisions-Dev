using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.Entities;

public sealed class Chapter : Entity<ChapterId>
{
    private readonly List<Page> _pages = new();
    private string _title = string.Empty;
    private string? _summary;

    // Private parameterless constructor for EF Core
    private Chapter() : base(default!)
    {
    }


    private Chapter(
        ChapterId id,
        string title,
        int orderIndex,
        BookId bookId) : base(id)
    {
        _title = title;
        OrderIndex = orderIndex;
        BookId = bookId;
    }

    public string Title => _title;
    public string? Summary => _summary;
    public int OrderIndex { get; private set; }
    public BookId BookId { get; private set; } = null!; // Will be set by EF Core
    public IReadOnlyList<Page> Pages => _pages.AsReadOnly();

    // Computed properties
    public int PageCount => _pages.Count;
    public int TotalWordCount => _pages.Sum(p => p.WordCount);
    public TimeSpan EstimatedReadingTime => TimeSpan.FromMinutes(TotalWordCount / 250.0);

    public static Chapter Create(
        string title,
        int orderIndex,
        BookId bookId,
        string? summary = null)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NegativeOrZero(orderIndex, nameof(orderIndex));
        Guard.Against.Null(bookId, nameof(bookId));

        var chapter = new Chapter(
            ChapterId.Create(),
            title,
            orderIndex,
            bookId);

        if (!string.IsNullOrWhiteSpace(summary))
        {
            chapter._summary = summary;
        }

        return chapter;
    }

    public Page AddPage(string content)
    {
        var pageNumber = _pages.Count + 1;
        var page = Page.Create(pageNumber, content, Id);

        _pages.Add(page);
        UpdateTimestamp();

        return page;
    }

    public void RemovePage(PageId pageId)
    {
        var page = _pages.FirstOrDefault(p => p.Id == pageId);
        if (page is null)
        {
            throw new InvalidOperationException($"Page with ID {pageId} not found in chapter");
        }

        _pages.Remove(page);

        // Reorder remaining pages
        for (var i = 0; i < _pages.Count; i++)
        {
            _pages[i].GetType()
                .GetProperty(nameof(Page.PageNumber))?
                .SetValue(_pages[i], i + 1);
        }

        UpdateTimestamp();
    }

    public void UpdateTitle(string newTitle)
    {
        Guard.Against.NullOrWhiteSpace(newTitle, nameof(newTitle));

        _title = newTitle;
        UpdateTimestamp();
    }

    public void UpdateSummary(string? summary)
    {
        _summary = summary;
        UpdateTimestamp();
    }

    public void ReorderPages(List<PageId> orderedPageIds)
    {
        Guard.Against.Null(orderedPageIds, nameof(orderedPageIds));

        if (orderedPageIds.Count != _pages.Count)
        {
            throw new ArgumentException("Ordered page IDs count doesn't match pages count");
        }

        var reorderedPages = new List<Page>();
        for (var i = 0; i < orderedPageIds.Count; i++)
        {
            var page = _pages.FirstOrDefault(p => p.Id == orderedPageIds[i])
                ?? throw new InvalidOperationException($"Page {orderedPageIds[i]} not found");

            page.GetType()
                .GetProperty(nameof(Page.PageNumber))?
                .SetValue(page, i + 1);

            reorderedPages.Add(page);
        }

        _pages.Clear();
        _pages.AddRange(reorderedPages);
        UpdateTimestamp();
    }
    public void UpdateOrderIndex(int newOrderIndex)
    {
        if (newOrderIndex < 1)
            throw new ArgumentException("Order index must be positive", nameof(newOrderIndex));

        OrderIndex = newOrderIndex;
        UpdateTimestamp();
    }

}

