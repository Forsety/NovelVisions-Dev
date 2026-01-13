// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/StronglyTypedIds/VisualizationJobId.cs
using System;

namespace NovelVision.Services.Catalog.Domain.StronglyTypedIds;

/// <summary>
/// Strongly-typed ID for visualization jobs (used for cross-service references)
/// </summary>
public readonly record struct VisualizationJobId : IEquatable<VisualizationJobId>
{
    public Guid Value { get; }

    private VisualizationJobId(Guid value)
    {
        Value = value;
    }

    public static VisualizationJobId Create() => new(Guid.NewGuid());

    public static VisualizationJobId From(Guid value) => new(value);

    public static VisualizationJobId? FromNullable(Guid? value) =>
        value.HasValue ? new VisualizationJobId(value.Value) : null;

    public static implicit operator Guid(VisualizationJobId id) => id.Value;

    public override string ToString() => Value.ToString();
}