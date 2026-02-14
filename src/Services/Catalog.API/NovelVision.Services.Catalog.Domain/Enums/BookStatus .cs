using Ardalis.SmartEnum;

namespace NovelVision.Services.Catalog.Domain.Enums;

public sealed class BookStatus : SmartEnum<BookStatus>
{
    public static readonly BookStatus Draft = new(nameof(Draft), 1);
    public static readonly BookStatus Published = new(nameof(Published), 2);
    public static readonly BookStatus Archived = new(nameof(Archived), 3);
    public static readonly BookStatus Suspended = new(nameof(Suspended), 4);

    private BookStatus(string name, int value) : base(name, value)
    {
    }

    public bool CanEdit => this == Draft;
    public bool IsVisible => this == Published;
    public bool CanPublish => this == Draft;
}
