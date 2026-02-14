using NovelVision.BuildingBlocks.SharedKernel.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Domain.StronglyTypedIds;

/// <summary>
/// Strongly-typed ID для задания визуализации
/// </summary>
public sealed record VisualizationJobId(Guid Value) : GuidStronglyTypedId(Value)
{
    public static VisualizationJobId Create() => new(Guid.NewGuid());
    public static VisualizationJobId From(Guid value) => new(value);
    public static VisualizationJobId From(string value) => new(Guid.Parse(value));
}
