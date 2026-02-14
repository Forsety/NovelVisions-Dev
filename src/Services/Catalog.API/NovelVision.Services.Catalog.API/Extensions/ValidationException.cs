namespace NovelVision.Services.Catalog.API.Exceptions;

public class ValidationException : Exception
{
    public IEnumerable<ValidationFailure>? Errors { get; }

    public ValidationException(string message) : base(message) { }

    public ValidationException(IEnumerable<ValidationFailure> errors) : base("Validation failed")
    {
        Errors = errors;
    }
}

public class ValidationFailure
{
    public string PropertyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public object? AttemptedValue { get; set; }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Unauthorized") : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Access denied") : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
