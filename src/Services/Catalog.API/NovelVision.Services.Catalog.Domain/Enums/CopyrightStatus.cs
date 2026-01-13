// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Enums/CopyrightStatus.cs
using Ardalis.SmartEnum;

namespace NovelVision.Services.Catalog.Domain.Enums;

/// <summary>
/// Статус авторских прав книги
/// </summary>
public sealed class CopyrightStatus : SmartEnum<CopyrightStatus>
{
    /// <summary>
    /// Авторские права неизвестны
    /// </summary>
    public static readonly CopyrightStatus Unknown = new(nameof(Unknown), 0, "Copyright status unknown");

    /// <summary>
    /// Книга в публичном домене (свободна для использования)
    /// </summary>
    public static readonly CopyrightStatus PublicDomain = new(nameof(PublicDomain), 1, "Public domain - free to use");

    /// <summary>
    /// Книга защищена авторским правом
    /// </summary>
    public static readonly CopyrightStatus Copyrighted = new(nameof(Copyrighted), 2, "Protected by copyright");

    /// <summary>
    /// Открытая лицензия (Creative Commons и т.д.)
    /// </summary>
    public static readonly CopyrightStatus OpenLicense = new(nameof(OpenLicense), 3, "Open license (CC, etc.)");

    private CopyrightStatus(string name, int value, string description)
        : base(name, value)
    {
        Description = description;
    }

    /// <summary>
    /// Описание статуса
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Можно ли свободно использовать контент
    /// </summary>
    public bool IsFreeToUse => this == PublicDomain || this == OpenLicense;

    /// <summary>
    /// Требуется ли указание автора
    /// </summary>
    public bool RequiresAttribution => this == OpenLicense;
}