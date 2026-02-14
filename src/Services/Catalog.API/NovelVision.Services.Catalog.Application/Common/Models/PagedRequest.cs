namespace NovelVision.Services.Catalog.Application.Common.Models;

public abstract class PagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public int Skip => (PageNumber - 1) * PageSize;
}

