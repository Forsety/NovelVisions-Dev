namespace NovelVision.BuildingBlocks.SharedKernel.StronglyTypedIds;

/// <summary>
/// Базовый класс для Strongly Typed IDs
/// </summary>
public abstract record StronglyTypedId<T>(T Value) where T : notnull
{
    public override string ToString() => Value.ToString() ?? string.Empty;
    
    public static implicit operator T(StronglyTypedId<T> typedId) => typedId.Value;
}

/// <summary>
/// Guid-based Strongly Typed ID
/// </summary>
public abstract record GuidStronglyTypedId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    protected GuidStronglyTypedId() : this(Guid.NewGuid()) { }
}
