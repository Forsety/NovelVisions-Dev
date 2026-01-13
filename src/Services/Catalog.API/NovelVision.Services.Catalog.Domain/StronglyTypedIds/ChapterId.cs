using NovelVision.BuildingBlocks.SharedKernel.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.StronglyTypedIds;

public sealed record ChapterId(Guid Value) : GuidStronglyTypedId(Value)
{
    public static ChapterId Create() => new(Guid.NewGuid());
    public static ChapterId From(Guid value) => new(value);
    public static ChapterId From(string value) => new(Guid.Parse(value));
}
