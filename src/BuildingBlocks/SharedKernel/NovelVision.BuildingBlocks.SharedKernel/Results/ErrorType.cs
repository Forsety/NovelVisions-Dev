// =============================================================================
// ФАЙЛ: src/BuildingBlocks/SharedKernel/NovelVision.BuildingBlocks.SharedKernel/Results/ErrorType.cs
// ДЕЙСТВИЕ: ЗАМЕНИТЬ весь файл
// ПРИЧИНА: Добавляем Unexpected тип ошибки
// =============================================================================

namespace NovelVision.BuildingBlocks.SharedKernel.Results;

public static class ErrorType
{
    public const string Validation = "Validation";
    public const string NotFound = "NotFound";
    public const string Conflict = "Conflict";
    public const string Unauthorized = "Unauthorized";
    public const string Forbidden = "Forbidden";
    public const string Failure = "Failure";
    public const string Unexpected = "Unexpected";
}