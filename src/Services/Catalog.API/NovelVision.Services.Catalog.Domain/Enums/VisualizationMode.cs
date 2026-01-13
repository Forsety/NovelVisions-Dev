// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Enums/VisualizationMode.cs
using Ardalis.SmartEnum;

namespace NovelVision.Services.Catalog.Domain.Enums;

public sealed class VisualizationMode : SmartEnum<VisualizationMode>
{
    public static readonly VisualizationMode None = new(nameof(None), 0, "No visualization");
    public static readonly VisualizationMode PerPage = new(nameof(PerPage), 1, "Visualize each page");
    public static readonly VisualizationMode PerChapter = new(nameof(PerChapter), 2, "Visualize each chapter");
    public static readonly VisualizationMode UserSelected = new(nameof(UserSelected), 3, "User selects scenes");
    public static readonly VisualizationMode AuthorDefined = new(nameof(AuthorDefined), 4, "Author defines scenes");

    private VisualizationMode(string name, int value, string description)
        : base(name, value)
    {
        Description = description;
    }

    public string Description { get; }
}