using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;

namespace NovelVision.Services.Catalog.Domain.ValueObjects;

public sealed class BookISBN : ValueObject
{
    private static readonly Regex IsbnRegex = new(
        @"^(?:ISBN(?:-1[03])?:?\s)?(?=[0-9X]{10}$|(?=(?:[0-9]+[-\s]){3})[-\s0-9X]{13}$|97[89][0-9]{10}$|(?=(?:[0-9]+[-\s]){4})[-\s0-9]{17}$)(?:97[89][-\s]?)?[0-9]{1,5}[-\s]?[0-9]+[-\s]?[0-9]+[-\s]?[0-9X]$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private BookISBN(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static BookISBN Create(string isbn)
    {
        Guard.Against.NullOrWhiteSpace(isbn, nameof(isbn));
        
        var normalizedIsbn = isbn.Replace("-", "").Replace(" ", "").ToUpperInvariant();
        
        if (!IsValidISBN(normalizedIsbn))
        {
            throw new ArgumentException($"Invalid ISBN format: {isbn}");
        }

        return new BookISBN(normalizedIsbn);
    }

    public static BookISBN? TryCreate(string? isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            return null;

        try
        {
            return Create(isbn);
        }
        catch
        {
            return null;
        }
    }

    private static bool IsValidISBN(string isbn)
    {
        if (isbn.Length == 10)
        {
            return ValidateISBN10(isbn);
        }
        
        if (isbn.Length == 13)
        {
            return ValidateISBN13(isbn);
        }

        return false;
    }

    private static bool ValidateISBN10(string isbn)
    {
        var sum = 0;
        for (var i = 0; i < 9; i++)
        {
            if (!char.IsDigit(isbn[i]))
                return false;
            sum += (isbn[i] - '0') * (10 - i);
        }

        var lastChar = isbn[9];
        sum += lastChar == 'X' ? 10 : (lastChar - '0');

        return sum % 11 == 0;
    }

    private static bool ValidateISBN13(string isbn)
    {
        if (!isbn.All(char.IsDigit))
            return false;

        var sum = 0;
        for (var i = 0; i < 13; i++)
        {
            var digit = isbn[i] - '0';
            sum += (i % 2 == 0) ? digit : digit * 3;
        }

        return sum % 10 == 0;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
