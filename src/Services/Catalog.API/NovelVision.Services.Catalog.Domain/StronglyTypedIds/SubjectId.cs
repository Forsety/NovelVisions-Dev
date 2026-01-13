// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/StronglyTypedIds/SubjectId.cs
using NovelVision.BuildingBlocks.SharedKernel.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.StronglyTypedIds;

/// <summary>
/// Strongly-typed ID для Subject entity
/// </summary>
public sealed record SubjectId(Guid Value) : GuidStronglyTypedId(Value)
{
    public static SubjectId Create() => new(Guid.NewGuid());
    public static SubjectId From(Guid value) => new(value);
    public static SubjectId From(string value) => new(Guid.Parse(value));
}