using System;

namespace NovelVision.Services.Catalog.Domain.Exceptions;

/// <summary>
/// Base exception for domain-specific errors
/// </summary>
public class BookDomainException : Exception
{
    public BookDomainException(string message) : base(message)
    {
    }

    public BookDomainException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

public class BookNotFoundException : BookDomainException
{
    public BookNotFoundException(Guid bookId) 
        : base($"Book with ID {bookId} was not found")
    {
        BookId = bookId;
    }

    public Guid BookId { get; }
}

public class ChapterNotFoundException : BookDomainException
{
    public ChapterNotFoundException(Guid chapterId) 
        : base($"Chapter with ID {chapterId} was not found")
    {
        ChapterId = chapterId;
    }

    public Guid ChapterId { get; }
}

public class DuplicateISBNException : BookDomainException
{
    public DuplicateISBNException(string isbn) 
        : base($"A book with ISBN {isbn} already exists")
    {
        ISBN = isbn;
    }

    public string ISBN { get; }
}

public class InvalidBookStateException : BookDomainException
{
    public InvalidBookStateException(string message) : base(message)
    {
    }
}

public class AuthorNotFoundException : BookDomainException
{
    public AuthorNotFoundException(Guid authorId) 
        : base($"Author with ID {authorId} was not found")
    {
        AuthorId = authorId;
    }

    public Guid AuthorId { get; }
}

public class DuplicateAuthorEmailException : BookDomainException
{
    public DuplicateAuthorEmailException(string email) 
        : base($"An author with email {email} already exists")
    {
        Email = email;
    }

    public string Email { get; }
}