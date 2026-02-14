// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/ValueObjects/PublicationInfo.cs
// ИСПРАВЛЕНИЕ: Добавлено статическое свойство Empty
using System;
using System.Collections.Generic;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;

namespace NovelVision.Services.Catalog.Domain.ValueObjects;

/// <summary>
/// Информация о публикации книги
/// </summary>
public sealed class PublicationInfoDto : ValueObject
{
    private PublicationInfoDto(
        string? publisher,
        DateTime? publicationDate,
        string? edition)
    {
        Publisher = publisher;
        PublicationDate = publicationDate;
        Edition = edition;
    }

    /// <summary>
    /// Издатель
    /// </summary>
    public string? Publisher { get; }

    /// <summary>
    /// Дата публикации
    /// </summary>
    public DateTime? PublicationDate { get; }

    /// <summary>
    /// Издание (первое, второе и т.д.)
    /// </summary>
    public string? Edition { get; }

    /// <summary>
    /// Год публикации (вычисляемое свойство)
    /// </summary>
    public int? PublicationYear => PublicationDate?.Year;

    /// <summary>
    /// Есть ли информация о публикации
    /// </summary>
    public bool HasInfo => !string.IsNullOrEmpty(Publisher) ||
                           PublicationDate.HasValue ||
                           !string.IsNullOrEmpty(Edition);

    #region Factory Methods

    /// <summary>
    /// Пустая информация о публикации
    /// </summary>
    public static PublicationInfoDto Empty => new(null, null, null);

    /// <summary>
    /// Создаёт информацию о публикации
    /// </summary>
    public static PublicationInfoDto Create(
        string? publisher = null,
        DateTime? publicationDate = null,
        string? edition = null)
    {
        if (publicationDate.HasValue && publicationDate.Value > DateTime.UtcNow)
        {
            throw new ArgumentException("Publication date cannot be in the future");
        }

        return new PublicationInfoDto(publisher?.Trim(), publicationDate, edition?.Trim());
    }

    /// <summary>
    /// Создаёт информацию о публикации с годом
    /// </summary>
    public static PublicationInfoDto CreateWithYear(
        string? publisher,
        int year,
        string? edition = null)
    {
        if (year < 1 || year > DateTime.UtcNow.Year + 1)
        {
            throw new ArgumentException($"Invalid publication year: {year}");
        }

        var publicationDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return new PublicationInfoDto(publisher?.Trim(), publicationDate, edition?.Trim());
    }

    #endregion

    #region Modification Methods

    /// <summary>
    /// Возвращает копию с изменённым издателем
    /// </summary>
    public PublicationInfoDto WithPublisher(string? publisher)
    {
        return new PublicationInfoDto(publisher?.Trim(), PublicationDate, Edition);
    }

    /// <summary>
    /// Возвращает копию с изменённой датой публикации
    /// </summary>
    public PublicationInfoDto WithPublicationDate(DateTime? publicationDate)
    {
        return new PublicationInfoDto(Publisher, publicationDate, Edition);
    }

    /// <summary>
    /// Возвращает копию с изменённым изданием
    /// </summary>
    public PublicationInfoDto WithEdition(string? edition)
    {
        return new PublicationInfoDto(Publisher, PublicationDate, edition?.Trim());
    }

    #endregion

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Publisher;
        yield return PublicationDate;
        yield return Edition;
    }
}