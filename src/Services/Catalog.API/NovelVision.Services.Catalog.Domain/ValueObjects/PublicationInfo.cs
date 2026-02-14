using System;
using System.Collections.Generic;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;

namespace NovelVision.Services.Catalog.Domain.ValueObjects;

public sealed class PublicationInfo : ValueObject
{
    private PublicationInfo(
        string? publisher,
        DateTime? publicationDate,
        string? edition)
    {
        Publisher = publisher;
        PublicationDate = publicationDate;
        Edition = edition;
    }

    public string? Publisher { get; }
    public DateTime? PublicationDate { get; }
    public string? Edition { get; }
    public int? PublicationYear => PublicationDate?.Year;

    public static PublicationInfo Create(
        string? publisher = null,
        DateTime? publicationDate = null,
        string? edition = null)
    {
        if (publicationDate.HasValue && publicationDate.Value > DateTime.UtcNow)
        {
            throw new ArgumentException("Publication date cannot be in the future");
        }

        return new PublicationInfo(publisher, publicationDate, edition);
    }
    public static PublicationInfo Empty => Create();
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Publisher;
        yield return PublicationDate;
        yield return Edition;
    }
}
