using NovelVision.BuildingBlocks.SharedKernel.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Domain.StronglyTypedIds;

/// <summary>
/// Strongly-typed ID для сгенерированного изображения
/// </summary>
public sealed record GeneratedImageId(Guid Value) : GuidStronglyTypedId(Value)
{
    public static GeneratedImageId Create() => new(Guid.NewGuid());
    public static GeneratedImageId From(Guid value) => new(value);
    public static GeneratedImageId From(string value) => new(Guid.Parse(value));
}
