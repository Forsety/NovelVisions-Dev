// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Extensions/VisualizationStatusConverter.cs
// Общий конвертер для преобразования доменного VisualizationStatus в DTO PageVisualizationStatus
using NovelVision.Services.Catalog.Application.DTOs;
using DomainVisualizationStatus = NovelVision.Services.Catalog.Domain.Enums.VisualizationStatus;

namespace NovelVision.Services.Catalog.Application.Extensions;

/// <summary>
/// Конвертер статусов визуализации между Domain и Application слоями
/// </summary>
public static class VisualizationStatusConverter
{
    /// <summary>
    /// Конвертирует доменный VisualizationStatus в DTO PageVisualizationStatus
    /// </summary>
    public static PageVisualizationStatus ToDto(DomainVisualizationStatus? domainStatus)
    {
        if (domainStatus is null)
            return PageVisualizationStatus.None;

        // SmartEnum - используем Name для сравнения
        return domainStatus.Name switch
        {
            "None" => PageVisualizationStatus.None,
            "Pending" => PageVisualizationStatus.Pending,
            "InProgress" or "Processing" => PageVisualizationStatus.InProgress,
            "Completed" or "Success" => PageVisualizationStatus.Completed,
            "Failed" or "Error" => PageVisualizationStatus.Failed,
            _ => PageVisualizationStatus.None
        };
    }

    /// <summary>
    /// Конвертирует строку в PageVisualizationStatus
    /// </summary>
    public static PageVisualizationStatus FromString(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return PageVisualizationStatus.None;

        return status.ToLowerInvariant() switch
        {
            "none" => PageVisualizationStatus.None,
            "pending" => PageVisualizationStatus.Pending,
            "inprogress" or "processing" => PageVisualizationStatus.InProgress,
            "completed" or "success" => PageVisualizationStatus.Completed,
            "failed" or "error" => PageVisualizationStatus.Failed,
            _ => PageVisualizationStatus.None
        };
    }

    /// <summary>
    /// Конвертирует DTO PageVisualizationStatus в строку
    /// </summary>
    public static string ToString(PageVisualizationStatus status)
    {
        return status switch
        {
            PageVisualizationStatus.None => "None",
            PageVisualizationStatus.Pending => "Pending",
            PageVisualizationStatus.InProgress => "InProgress",
            PageVisualizationStatus.Completed => "Completed",
            PageVisualizationStatus.Failed => "Failed",
            _ => "None"
        };
    }
}