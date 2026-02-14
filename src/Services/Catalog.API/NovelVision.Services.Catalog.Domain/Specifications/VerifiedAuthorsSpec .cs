using Ardalis.Specification;
using NovelVision.Services.Catalog.Domain.Aggregates.AuthorAggregate;

namespace NovelVision.Services.Catalog.Domain.Specifications;

public sealed class VerifiedAuthorsSpec : Specification<Author>
{
    public VerifiedAuthorsSpec()
    {
        Query
            .Where(author => author.IsVerified)
            .OrderBy(author => author.DisplayName);
    }
}

public sealed class AuthorsWithBooksSpec : Specification<Author>
{
    public AuthorsWithBooksSpec()
    {
        Query
            .Where(author => author.BookCount > 0)
            .OrderByDescending(author => author.BookCount);
    }
}

public sealed class AuthorByEmailSpec : Specification<Author>, ISingleResultSpecification<Author>
{
    public AuthorByEmailSpec(string email)
    {
        Query.Where(author => author.Email == email.ToLowerInvariant());
    }
}