// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Services/Common/DateTimeProvider.cs

using NovelVision.Services.Visualization.Application.Interfaces;

namespace NovelVision.Services.Visualization.Infrastructure.Services.Common;

/// <summary>
/// Провайдер даты/времени (для тестируемости)
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
}