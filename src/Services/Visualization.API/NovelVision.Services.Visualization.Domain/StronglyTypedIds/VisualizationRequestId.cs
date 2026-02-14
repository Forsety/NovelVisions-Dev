using NovelVision.BuildingBlocks.SharedKernel.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Domain.StronglyTypedIds;

/// <summary>
/// Strongly-typed ID для запроса визуализации (от пользователя)
/// </summary>
public sealed record VisualizationRequestId(Guid Value) : GuidStronglyTypedId(Value)
{
    public static VisualizationRequestId Create() => new(Guid.NewGuid());
    public static VisualizationRequestId From(Guid value) => new(value);
    public static VisualizationRequestId From(string value) => new(Guid.Parse(value));
}
