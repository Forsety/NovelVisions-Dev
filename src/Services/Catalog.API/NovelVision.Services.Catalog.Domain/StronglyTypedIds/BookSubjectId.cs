// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/StronglyTypedIds/BookSubjectId.cs
using NovelVision.BuildingBlocks.SharedKernel.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.StronglyTypedIds;

/// <summary>
/// Строго типізований ID для BookSubject
/// </summary>
public sealed record BookSubjectId(Guid Value) : GuidStronglyTypedId(Value)
{
    public static BookSubjectId Create() => new(Guid.NewGuid());
    public static BookSubjectId From(Guid value) => new(value);
    public static BookSubjectId From(string value) => new(Guid.Parse(value));
}