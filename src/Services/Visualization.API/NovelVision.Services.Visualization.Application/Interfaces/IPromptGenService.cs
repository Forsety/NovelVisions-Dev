using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Domain.Enums;

namespace NovelVision.Services.Visualization.Application.Interfaces;

/// <summary>
/// Интерфейс для взаимодействия с PromptGen.API
/// </summary>
public interface IPromptGenService
{
    /// <summary>
    /// Сгенерировать улучшенный промпт из текста
    /// </summary>
    /// <param name="originalText">Исходный текст из книги</param>
    /// <param name="targetModel">Целевая AI модель</param>
    /// <param name="bookId">ID книги для контекста персонажей</param>
    /// <param name="style">Желаемый стиль (опционально)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<Result<PromptGenResponseDto>> GeneratePromptAsync(
        string originalText,
        AIModelProvider targetModel,
        Guid bookId,
        string? style = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить consistency данные для персонажа
    /// </summary>
    Task<Result<CharacterConsistencyDto>> GetCharacterConsistencyAsync(
        Guid bookId,
        string characterName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить доступность сервиса
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}
