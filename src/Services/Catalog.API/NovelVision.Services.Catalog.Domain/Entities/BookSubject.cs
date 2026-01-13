// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Entities/BookSubject.cs
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.Entities;

/// <summary>
/// Зв'язок many-to-many між Book та Subject
/// </summary>
public sealed class BookSubject : Entity<BookSubjectId>
{
    private BookSubject() : base(default!) { }

    private BookSubject(
        BookSubjectId id,
        BookId bookId,
        SubjectId subjectId,
        bool isPrimary,
        int displayOrder) : base(id)
    {
        BookId = bookId;
        SubjectId = subjectId;
        IsPrimary = isPrimary;
        DisplayOrder = displayOrder;
    }

    public BookId BookId { get; private set; } = null!;
    public SubjectId SubjectId { get; private set; } = null!;
    public bool IsPrimary { get; private set; }
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Навігаційна властивість до Subject
    /// </summary>
    public Subject? Subject { get; private set; }

    public static BookSubject Create(
        BookId bookId,
        SubjectId subjectId,
        bool isPrimary = false,
        int displayOrder = 0)
    {
        return new BookSubject(
            BookSubjectId.Create(),
            bookId,
            subjectId,
            isPrimary,
            displayOrder);
    }

    public void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
        UpdateTimestamp();
    }

    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
        UpdateTimestamp();
    }
}