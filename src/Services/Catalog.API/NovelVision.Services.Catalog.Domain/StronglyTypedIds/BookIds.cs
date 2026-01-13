using NovelVision.BuildingBlocks.SharedKernel.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.StronglyTypedIds;

public sealed record BookId(Guid Value) : GuidStronglyTypedId(Value)
{
    public static BookId Create() => new(Guid.NewGuid());
    public static BookId From(Guid value) => new(value);
    public static BookId From(string value) => new(Guid.Parse(value));
}
