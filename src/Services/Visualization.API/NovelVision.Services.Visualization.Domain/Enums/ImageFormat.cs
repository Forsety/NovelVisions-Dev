using Ardalis.SmartEnum;

namespace NovelVision.Services.Visualization.Domain.Enums;

/// <summary>
/// Формат сгенерированного изображения
/// </summary>
public sealed class ImageFormat : SmartEnum<ImageFormat>
{
    public static readonly ImageFormat Png = new(nameof(Png), 1, "image/png", ".png");
    public static readonly ImageFormat Jpeg = new(nameof(Jpeg), 2, "image/jpeg", ".jpg");
    public static readonly ImageFormat WebP = new(nameof(WebP), 3, "image/webp", ".webp");

    private ImageFormat(string name, int value, string mimeType, string extension) 
        : base(name, value)
    {
        MimeType = mimeType;
        Extension = extension;
    }

    public string MimeType { get; }
    public string Extension { get; }

    public static ImageFormat FromMimeType(string mimeType)
    {
        return List.FirstOrDefault(f => 
            f.MimeType.Equals(mimeType, StringComparison.OrdinalIgnoreCase))
            ?? Png; // Default to PNG
    }

    public static ImageFormat FromExtension(string extension)
    {
        var ext = extension.StartsWith('.') ? extension : $".{extension}";
        return List.FirstOrDefault(f => 
            f.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase))
            ?? Png;
    }
}
