using Ardalis.SmartEnum;

namespace NovelVision.Services.Catalog.Domain.ValueObjects;

public sealed class Language : SmartEnum<Language>
{
    public static readonly Language English = new(nameof(English), 1, "en", "English");
    public static readonly Language Russian = new(nameof(Russian), 2, "ru", "Русский");
    public static readonly Language Ukrainian = new(nameof(Ukrainian), 3, "uk", "Українська");
    public static readonly Language Polish = new(nameof(Polish), 4, "pl", "Polski");
    public static readonly Language Spanish = new(nameof(Spanish), 5, "es", "Español");
    public static readonly Language French = new(nameof(French), 6, "fr", "Français");
    public static readonly Language German = new(nameof(German), 7, "de", "Deutsch");

    private Language(string name, int value, string code, string displayName) 
        : base(name, value)
    {
        Code = code;
        DisplayName = displayName;
    }

    public string Code { get; }
    public string DisplayName { get; }

    public static Language FromCode(string code)
    {
        return List.FirstOrDefault(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Language with code '{code}' not found");
    }
}
