// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Aggregates/AuthorAggregate/Author.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Domain.Events;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;
using NovelVision.Services.Catalog.Domain.ValueObjects;

namespace NovelVision.Services.Catalog.Domain.Aggregates.AuthorAggregate;

/// <summary>
/// Агрегат автора книг
/// </summary>
public sealed class Author : AggregateRoot<AuthorId>
{
    private string _displayName = string.Empty;
    private string _email = string.Empty;
    private string? _biography;
    private readonly HashSet<BookId> _bookIds = new();
    private readonly Dictionary<string, string> _socialLinks = new();

    #region Constructors

    /// <summary>
    /// Private parameterless constructor for EF Core
    /// </summary>
    private Author() : base(default!)
    {
        // EF Core will set all properties via reflection
        // Инициализируем поля значениями по умолчанию для избежания CS8618/CS8625
        _displayName = string.Empty;
        _email = string.Empty;
        _bookIds = new HashSet<BookId>();
        _socialLinks = new Dictionary<string, string>();
    }

    private Author(
        AuthorId id,
        string displayName,
        string email,
        string? biography = null) : base(id)
    {
        _displayName = displayName;
        _email = email;
        _biography = biography;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Отображаемое имя автора
    /// </summary>
    public string DisplayName => _displayName;

    /// <summary>
    /// Email автора
    /// </summary>
    public string Email => _email;

    /// <summary>
    /// Биография автора
    /// </summary>
    public string? Biography => _biography;

    /// <summary>
    /// Подтвержден ли автор
    /// </summary>
    public bool IsVerified { get; private set; }

    /// <summary>
    /// Дата подтверждения
    /// </summary>
    public DateTime? VerifiedAt { get; private set; }

    /// <summary>
    /// ID книг автора
    /// </summary>
    public IReadOnlySet<BookId> BookIds => _bookIds;

    /// <summary>
    /// Социальные ссылки автора
    /// </summary>
    public IReadOnlyDictionary<string, string> SocialLinks => _socialLinks;

    #endregion

    #region Identity Integration Properties

    /// <summary>
    /// ID пользователя из Identity (ApplicationUser.Id)
    /// Связывает профиль автора с учетной записью пользователя
    /// </summary>
    public string? UserId { get; private set; }

    /// <summary>
    /// Привязан ли профиль автора к пользователю
    /// </summary>
    public bool IsLinkedToUser => !string.IsNullOrEmpty(UserId);

    #endregion

    #region Additional Profile Properties

    /// <summary>
    /// Год рождения автора (для Gutenberg авторов)
    /// </summary>
    public int? BirthYear { get; private set; }

    /// <summary>
    /// Год смерти автора (для Gutenberg авторов)
    /// </summary>
    public int? DeathYear { get; private set; }

    /// <summary>
    /// URL аватара автора
    /// </summary>
    public string? AvatarUrl { get; private set; }

    /// <summary>
    /// Национальность/страна автора
    /// </summary>
    public string? Nationality { get; private set; }

    /// <summary>
    /// Ссылка на Wikipedia
    /// </summary>
    public string? WikipediaUrl { get; private set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Количество книг автора
    /// </summary>
    public int BookCount => _bookIds.Count;

    /// <summary>
    /// Есть ли у автора книги
    /// </summary>
    public bool HasBooks => _bookIds.Any();

    /// <summary>
    /// Жив ли автор (если известны годы жизни)
    /// </summary>
    public bool? IsAlive => DeathYear.HasValue ? false : (BirthYear.HasValue ? null : null);

    /// <summary>
    /// Период жизни в формате "1828-1910"
    /// </summary>
    public string? LifeSpan => BirthYear.HasValue
        ? DeathYear.HasValue
            ? $"{BirthYear}-{DeathYear}"
            : $"{BirthYear}-"
        : null;

    /// <summary>
    /// Является ли исторический автор (из Gutenberg)
    /// </summary>
    public bool IsHistorical => BirthYear.HasValue || DeathYear.HasValue;



    /// </summary>
    public ExternalAuthorIdentifiers ExternalIds { get; private set; } = ExternalAuthorIdentifiers.Empty;

    /// <summary>
    /// Был ли автор импортирован из внешнего источника
    /// </summary>
    public bool IsFromExternalSource => ExternalIds.HasAnyId;

    #endregion

    #region Factory Methods

    /// <summary>
    /// Создает нового автора
    /// </summary>
    public static Result<Author> Create(
        string displayName,
        string email,
        string? biography = null)
    {
        try
        {
            Guard.Against.NullOrWhiteSpace(displayName, nameof(displayName));
            Guard.Against.NullOrWhiteSpace(email, nameof(email));

            // Basic email validation
            if (!email.Contains('@') || !email.Contains('.'))
            {
                return Result<Author>.Failure(Error.Validation("Invalid email format"));
            }

            var author = new Author(
                AuthorId.Create(),
                displayName,
                email.ToLowerInvariant(),
                biography);

            author.AddDomainEvent(new AuthorCreatedEvent(author.Id, displayName, email));

            return Result<Author>.Success(author);
        }
        catch (Exception ex)
        {
            return Result<Author>.Failure(Error.Validation(ex.Message));
        }
    }

    /// <summary>
    /// Создает автора с привязкой к пользователю
    /// </summary>
    public static Result<Author> CreateWithUser(
        string displayName,
        string email,
        string userId,
        string? biography = null)
    {
        var result = Create(displayName, email, biography);
        if (result.IsFailed)
            return result;

        result.Value.LinkToUser(userId);
        return result;
    }

    /// <summary>
    /// Создает автора из Gutenberg (исторический автор)
    /// </summary>
    public static Result<Author> CreateFromGutenberg(
        string displayName,
        int? birthYear,
        int? deathYear,
        string? wikipediaUrl = null)
    {
        try
        {
            Guard.Against.NullOrWhiteSpace(displayName, nameof(displayName));

            // Для Gutenberg авторов генерируем email-заглушку
            var email = $"gutenberg.{Guid.NewGuid():N}@novelvision.local";

            var author = new Author(
                AuthorId.Create(),
                displayName,
                email,
                null);

            author.BirthYear = birthYear;
            author.DeathYear = deathYear;
            author.WikipediaUrl = wikipediaUrl;

            author.AddDomainEvent(new AuthorCreatedEvent(author.Id, displayName, email));

            return Result<Author>.Success(author);
        }
        catch (Exception ex)
        {
            return Result<Author>.Failure(Error.Validation(ex.Message));
        }
    }

    #endregion

    #region Profile Management

    /// <summary>
    /// Обновляет профиль автора
    /// </summary>
    public void UpdateProfile(string displayName, string? biography)
    {
        Guard.Against.NullOrWhiteSpace(displayName, nameof(displayName));

        _displayName = displayName;
        _biography = biography;
        IncrementVersion();

        AddDomainEvent(new AuthorProfileUpdatedEvent(Id, displayName, biography));
    }

    /// <summary>
    /// Обновляет email автора
    /// </summary>
    public Result<bool> UpdateEmail(string newEmail)
    {
        Guard.Against.NullOrWhiteSpace(newEmail, nameof(newEmail));

        if (!newEmail.Contains('@') || !newEmail.Contains('.'))
        {
            return Result<bool>.Failure(Error.Validation("Invalid email format"));
        }

        var oldEmail = _email;
        _email = newEmail.ToLowerInvariant();
        IncrementVersion();

        AddDomainEvent(new AuthorEmailChangedEvent(Id, newEmail));

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Устанавливает аватар автора
    /// </summary>
    public void SetAvatar(string avatarUrl)
    {
        Guard.Against.NullOrWhiteSpace(avatarUrl, nameof(avatarUrl));
        AvatarUrl = avatarUrl;
        IncrementVersion();
    }

    /// <summary>
    /// Устанавливает национальность автора
    /// </summary>
    public void SetNationality(string? nationality)
    {
        Nationality = nationality;
        IncrementVersion();
    }

    /// <summary>
    /// Устанавливает годы жизни автора
    /// </summary>
    public void SetLifeYears(int? birthYear, int? deathYear)
    {
        BirthYear = birthYear;
        DeathYear = deathYear;
        IncrementVersion();
    }

    /// <summary>
    /// Привязывает профиль автора к пользователю
    /// </summary>
    public void LinkToUser(string userId)
    {
        Guard.Against.NullOrWhiteSpace(userId, nameof(userId));

        if (IsLinkedToUser)
        {
            throw new InvalidOperationException("Author is already linked to a user");
        }

        UserId = userId;
        IncrementVersion();

        AddDomainEvent(new AuthorLinkedToUserEvent(Id, userId));
    }

    /// <summary>
    /// Отвязывает профиль автора от пользователя
    /// </summary>
    public void UnlinkFromUser()
    {
        if (!IsLinkedToUser)
        {
            throw new InvalidOperationException("Author is not linked to any user");
        }

        var oldUserId = UserId;
        UserId = null;
        IncrementVersion();

        AddDomainEvent(new AuthorUnlinkedFromUserEvent(Id, oldUserId!));
    }

    #endregion

    #region Verification

    /// <summary>
    /// Подтверждает автора
    /// </summary>
    public void Verify()
    {
        if (IsVerified)
        {
            throw new InvalidOperationException("Author is already verified");
        }

        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
        IncrementVersion();

        AddDomainEvent(new AuthorVerifiedEvent(Id));
    }

    /// <summary>
    /// Снимает подтверждение с автора
    /// </summary>
    public void Unverify()
    {
        if (!IsVerified)
        {
            throw new InvalidOperationException("Author is not verified");
        }

        IsVerified = false;
        VerifiedAt = null;
        IncrementVersion();
    }

    #endregion

    #region Book Management

    /// <summary>
    /// Добавляет книгу автору
    /// </summary>
    public void AddBook(BookId bookId)
    {
        Guard.Against.Null(bookId, nameof(bookId));

        if (_bookIds.Add(bookId))
        {
            IncrementVersion();
            AddDomainEvent(new BookAddedToAuthorEvent(Id, bookId));
        }
    }

    /// <summary>
    /// Удаляет книгу у автора
    /// </summary>
    public void RemoveBook(BookId bookId)
    {
        Guard.Against.Null(bookId, nameof(bookId));

        if (_bookIds.Remove(bookId))
        {
            IncrementVersion();
            AddDomainEvent(new BookRemovedFromAuthorEvent(Id, bookId));
        }
    }

    #endregion

    #region Social Links Management

    /// <summary>
    /// Добавляет социальную ссылку
    /// </summary>
    public void AddSocialLink(string platform, string url)
    {
        Guard.Against.NullOrWhiteSpace(platform, nameof(platform));
        Guard.Against.NullOrWhiteSpace(url, nameof(url));

        if (_socialLinks.Count >= 10)
        {
            throw new InvalidOperationException("Maximum 10 social links allowed");
        }

        _socialLinks[platform.ToLowerInvariant()] = url;
        IncrementVersion();
    }

    /// <summary>
    /// Удаляет социальную ссылку
    /// </summary>
    public void RemoveSocialLink(string platform)
    {
        Guard.Against.NullOrWhiteSpace(platform, nameof(platform));

        if (_socialLinks.Remove(platform.ToLowerInvariant()))
        {
            IncrementVersion();
        }
    }

    /// <summary>
    /// Очищает все социальные ссылки
    /// </summary>
    public void ClearSocialLinks()
    {
        _socialLinks.Clear();
        IncrementVersion();
    }

    #endregion
}