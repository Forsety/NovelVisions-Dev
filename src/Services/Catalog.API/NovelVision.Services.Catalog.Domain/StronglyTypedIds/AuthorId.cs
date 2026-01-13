using NovelVision.BuildingBlocks.SharedKernel.StronglyTypedIds;


namespace NovelVision.Services.Catalog.Domain.StronglyTypedIds;

public sealed record AuthorId(Guid Value) : GuidStronglyTypedId(Value)
{
    public static AuthorId Create() => new(Guid.NewGuid());
    public static AuthorId From(Guid value) => new(value);
    public static AuthorId From(string value) => new(Guid.Parse(value));
}
