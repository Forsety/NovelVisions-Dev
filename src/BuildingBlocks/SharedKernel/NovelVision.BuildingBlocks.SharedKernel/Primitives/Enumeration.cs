using Ardalis.SmartEnum;

namespace NovelVision.BuildingBlocks.SharedKernel.Primitives;

/// <summary>
/// Базовый класс для Enumeration (вместо обычных enum)
/// Позволяет добавлять поведение к перечислениям
/// </summary>
public abstract class Enumeration<TEnum> : SmartEnum<TEnum>
    where TEnum : SmartEnum<TEnum, int>
{
    protected Enumeration(string name, int value) : base(name, value)
    {
    }
}