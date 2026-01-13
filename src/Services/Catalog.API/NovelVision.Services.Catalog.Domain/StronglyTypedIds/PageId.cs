using NovelVision.BuildingBlocks.SharedKernel.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.StronglyTypedIds;

public sealed record PageId(Guid Value) : GuidStronglyTypedId(Value)
{
    public static PageId Create() => new(Guid.NewGuid());
    public static PageId From(Guid value) => new(value);
    public static PageId From(string value) => new(Guid.Parse(value));
}
