# NovelVision.Services.Visualization.Application

## 📖 Описание

Application слой микросервиса визуализации. Содержит бизнес-логику приложения, реализованную через паттерн CQRS с использованием MediatR.

## 🏗️ Структура

### 📂 Commands (Write Operations)
- **CreateVisualizationRequest** - Создание запроса на визуализацию
- **AddTextSelection** - Добавление выделенного текста
- **StartVisualization** - Запуск процесса визуализации
- **CancelVisualization** - Отмена визуализации
- **PauseVisualization** - Приостановка визуализации
- **ResumeVisualization** - Возобновление визуализации
- **ApproveImage** - Одобрение сгенерированного изображения
- **RejectImage** - Отклонение изображения

### 📂 Queries (Read Operations)
- **GetVisualizationRequest** - Получение запроса визуализации
- **GetVisualizationStatus** - Получение статуса визуализации
- **GetGeneratedImages** - Получение сгенерированных изображений
- **GetUserStatistics** - Получение статистики пользователя
- **GetVisualizationsByUser** - Получение визуализаций пользователя
- **GetVisualizationsByBook** - Получение визуализаций книги

### 📂 DTOs (Data Transfer Objects)
- `VisualizationRequestDto`
- `VisualizationTaskDto`
- `GeneratedImageDto`
- `TextSelectionDto`
- `VisualizationSettingsDto`
- И другие...

### 📂 Interfaces
- **Repositories**: `IVisualizationRequestRepository`, `IVisualizationTaskRepository`, etc.
- **External Services**: `IPromptGenerationService`, `IImageGenerationService`, `ICatalogService`
- **Infrastructure**: `IUnitOfWork`, `ICacheService`, `IImageStorageService`

### 📂 Mapping
- **VisualizationMappingProfile** - AutoMapper профиль для маппинга Domain <-> DTOs

### 📂 Validators
- FluentValidation валидаторы для всех Commands

### 📂 Behaviors
- **ValidationBehavior** - Автоматическая валидация запросов
- **LoggingBehavior** - Логирование всех операций
- **PerformanceBehavior** - Мониторинг производительности

### 📂 Exceptions
- Application-специфичные исключения

## 🔗 Зависимости

### NuGet Packages
- **MediatR** (12.2.0) - CQRS implementation
- **AutoMapper** (13.0.1) - Object mapping
- **FluentValidation** (11.9.0) - Input validation
- **Microsoft.Extensions.Logging.Abstractions** (8.0.0)
- **Microsoft.Extensions.Options** (8.0.0)

### Project References
- **NovelVision.Services.Visualization.Domain** - Domain layer
- **NovelVision.BuildingBlocks.SharedKernel** - Shared primitives

## 🚀 Использование

### Регистрация в DI Container

```csharp
services.AddApplicationServices();
```

### Пример использования Command

```csharp
public class VisualizationController : ControllerBase
{
    private readonly IMediator _mediator;

    public VisualizationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateVisualization(
        [FromBody] CreateVisualizationRequestCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}
```

### Пример использования Query

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetVisualization(Guid id)
{
    var query = new GetVisualizationRequestQuery 
    { 
        RequestId = id,
        IncludeDetails = true 
    };
    
    var result = await _mediator.Send(query);
    
    if (!result.IsSuccess)
    {
        return NotFound(result.Error);
    }

    return Ok(result.Value);
}
```

## 📝 Паттерны и принципы

### CQRS (Command Query Responsibility Segregation)
- **Commands** - изменяют состояние системы
- **Queries** - только читают данные

### Mediator Pattern
- Все операции проходят через MediatR
- Уменьшает связанность между компонентами

### Pipeline Behaviors
- Валидация - перед выполнением handler
- Логирование - вокруг каждого handler
- Performance monitoring - измерение времени выполнения

### Result Pattern
- Все операции возвращают `Result<T>`
- Избегаем exceptions для бизнес-логики
- Явная обработка ошибок

## 🎯 Workflow визуализации

```
1. CreateVisualizationRequestCommand
   └─> Создает VisualizationRequest в Domain
   └─> Сохраняет через Repository
   └─> Возвращает VisualizationRequestDto

2. AddTextSelectionCommand (опционально)
   └─> Добавляет TextSelection к Request
   └─> Валидирует по типу Selection

3. StartVisualizationCommand
   └─> Получает контент из Catalog API
   └─> Создает VisualizationTask для каждой страницы/главы
   └─> Запускает background job для обработки

4. Background Processing
   └─> Генерирует промпты (PromptGen API)
   └─> Генерирует изображения (AI Service)
   └─> Сохраняет в Storage
   └─> Обновляет статус Tasks

5. ApproveImageCommand / RejectImageCommand
   └─> Модерация сгенерированных изображений
   └─> Публикация одобренных

6. GetVisualizationStatusQuery
   └─> Отслеживание прогресса
   └─> Real-time updates через SignalR
```

## 🔐 Валидация

Все Commands проходят через FluentValidation:

```csharp
public class CreateVisualizationRequestCommandValidator 
    : AbstractValidator<CreateVisualizationRequestCommand>
{
    public CreateVisualizationRequestCommandValidator()
    {
        RuleFor(x => x.BookId)
            .NotEmpty()
            .WithMessage("BookId is required");

        RuleFor(x => x.Mode)
            .Must(BeValidMode)
            .WithMessage("Invalid visualization mode");
    }
}
```

## 📊 Маппинг

AutoMapper автоматически конвертирует Domain entities в DTOs:

```csharp
public class VisualizationMappingProfile : Profile
{
    public VisualizationMappingProfile()
    {
        CreateMap<VisualizationRequest, VisualizationRequestDto>()
            .ForMember(dest => dest.Id, 
                opt => opt.MapFrom(src => src.Id.Value));
    }
}
```

## 🧪 Тестирование

Application слой должен быть покрыт:
- **Unit Tests** - для Handlers
- **Integration Tests** - для полного flow
- **Validation Tests** - для Validators

## 📄 License

MIT License © 2025 NovelVision
