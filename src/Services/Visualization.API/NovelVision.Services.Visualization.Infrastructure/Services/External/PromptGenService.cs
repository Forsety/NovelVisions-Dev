// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Services/External/PromptGenService.cs

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.Enums;

namespace NovelVision.Services.Visualization.Infrastructure.Services.External;

/// <summary>
/// HTTP клиент для взаимодействия с PromptGen.API (Python FastAPI)
/// </summary>
public sealed class PromptGenService : IPromptGenService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PromptGenService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public PromptGenService(
        HttpClient httpClient,
        ILogger<PromptGenService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<PromptGenResponseDto>> GeneratePromptAsync(
        string originalText,
        AIModelProvider targetModel,
        Guid bookId,
        string? style = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Generating prompt for BookId: {BookId}, Model: {Model}, TextLength: {Length}",
                bookId, targetModel.ApiName, originalText.Length);

            var request = new GeneratePromptsRequest
            {
                BookId = bookId.ToString(),
                PageContent = originalText,
                TargetModel = targetModel.ApiName,
                Style = style,
                MaintainConsistency = true,
                MaxPrompts = 1
            };

            var response = await _httpClient.PostAsJsonAsync(
                "/api/v1/visualization/generate-prompts",
                request,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "PromptGen API error. Status: {Status}, Error: {Error}",
                    response.StatusCode, errorContent);

                return Result<PromptGenResponseDto>.Failure(
                    Error.Failure($"PromptGen API error: {response.StatusCode}"));
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<PromptGenApiResponse>(
                JsonOptions, cancellationToken);

            if (apiResponse?.Data == null)
            {
                return Result<PromptGenResponseDto>.Failure(
                    Error.Failure("Invalid response from PromptGen.API"));
            }

            var promptData = apiResponse.Data;
            var firstPrompt = promptData.Prompts?.FirstOrDefault();

            var result = new PromptGenResponseDto
            {
                EnhancedPrompt = firstPrompt?.Prompt ?? originalText,
                NegativePrompt = firstPrompt?.NegativePrompt,
                TargetModel = targetModel.ApiName,
                Style = style,
                SceneDescription = firstPrompt?.SceneDescription,
                Characters = firstPrompt?.Characters ?? new List<string>(),
                Parameters = firstPrompt?.Parameters ?? new Dictionary<string, object>(),
                ProcessingTimeMs = promptData.ProcessingTimeMs
            };

            _logger.LogInformation(
                "Generated prompt for BookId: {BookId}. Processing time: {Time}ms",
                bookId, promptData.ProcessingTimeMs);

            return Result<PromptGenResponseDto>.Success(result);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("PromptGen request timed out for BookId: {BookId}", bookId);
            return Result<PromptGenResponseDto>.Failure(
                Error.Failure("PromptGen request timed out"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating prompt for BookId: {BookId}", bookId);
            return Result<PromptGenResponseDto>.Failure(Error.Failure(ex.Message));
        }
    }

    public async Task<Result<CharacterConsistencyDto>> GetCharacterConsistencyAsync(
        Guid bookId,
        string characterName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug(
                "Getting character consistency for BookId: {BookId}, Character: {Character}",
                bookId, characterName);

            var encodedName = Uri.EscapeDataString(characterName);
            var response = await _httpClient.GetAsync(
                $"/api/v1/visualization/character-consistency/{bookId}/{encodedName}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Персонаж не найден - возвращаем пустой DTO
                    return Result<CharacterConsistencyDto>.Success(new CharacterConsistencyDto
                    {
                        CharacterName = characterName,
                        BookId = bookId,
                        IsEstablished = false
                    });
                }

                return Result<CharacterConsistencyDto>.Failure(
                    Error.Failure($"Failed to get character consistency: {response.StatusCode}"));
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<PromptGenApiResponse<CharacterConsistencyApiDto>>(
                JsonOptions, cancellationToken);

            if (apiResponse?.Data == null)
            {
                return Result<CharacterConsistencyDto>.Failure(
                    Error.Failure("Invalid response from PromptGen.API"));
            }

            var data = apiResponse.Data;
            var result = new CharacterConsistencyDto
            {
                CharacterName = data.CharacterName,
                BookId = bookId,
                Appearance = data.Appearance,
                Clothing = data.Clothing,
                DistinguishingFeatures = data.DistinguishingFeatures,
                PromptFragment = data.PromptFragment,
                IsEstablished = data.IsEstablished,
                GenerationCount = data.GenerationCount
            };

            return Result<CharacterConsistencyDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting character consistency for BookId: {BookId}, Character: {Character}",
                bookId, characterName);
            return Result<CharacterConsistencyDto>.Failure(Error.Failure(ex.Message));
        }
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                "/api/v1/visualization/health",
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    #region Private Request/Response DTOs

    private sealed record GeneratePromptsRequest
    {
        public string BookId { get; init; } = string.Empty;
        public string? ChapterId { get; init; }
        public string? PageId { get; init; }
        public string PageContent { get; init; } = string.Empty;
        public int? PageNumber { get; init; }
        public string TargetModel { get; init; } = "dalle3";
        public string? Style { get; init; }
        public bool MaintainConsistency { get; init; } = true;
        public int MaxPrompts { get; init; } = 1;
    }

    private sealed record PromptGenApiResponse
    {
        public string? Message { get; init; }
        public GeneratePromptsResponseData? Data { get; init; }
    }

    private sealed record PromptGenApiResponse<T>
    {
        public string? Message { get; init; }
        public T? Data { get; init; }
    }

    private sealed record GeneratePromptsResponseData
    {
        public string BookId { get; init; } = string.Empty;
        public string? ChapterId { get; init; }
        public string? PageId { get; init; }
        public int? PageNumber { get; init; }
        public List<GeneratedPromptData>? Prompts { get; init; }
        public Dictionary<string, object>? Analysis { get; init; }
        public Dictionary<string, object>? CharacterContext { get; init; }
        public string TargetModel { get; init; } = string.Empty;
        public string? Style { get; init; }
        public int ProcessingTimeMs { get; init; }
    }

    private sealed record GeneratedPromptData
    {
        public string Prompt { get; init; } = string.Empty;
        public string? NegativePrompt { get; init; }
        public string? SceneDescription { get; init; }
        public string? SceneType { get; init; }
        public string Importance { get; init; } = "medium";
        public List<string> Characters { get; init; } = new();
        public List<string> Objects { get; init; } = new();
        public string? Location { get; init; }
        public Dictionary<string, object> Parameters { get; init; } = new();
    }

    private sealed record CharacterConsistencyApiDto
    {
        public string CharacterName { get; init; } = string.Empty;
        public string? Appearance { get; init; }
        public string? Clothing { get; init; }
        public List<string>? DistinguishingFeatures { get; init; }
        public string? PromptFragment { get; init; }
        public bool IsEstablished { get; init; }
        public int GenerationCount { get; init; }
    }

    #endregion
}