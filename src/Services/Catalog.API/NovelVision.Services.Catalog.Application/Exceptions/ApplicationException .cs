using FluentValidation.Results;
using System;

namespace NovelVision.Services.Catalog.Application.Exceptions;

public class ApplicationException : Exception
{
    /// <summary>
    /// Initializes a new instance of ApplicationException
    /// </summary>
    public ApplicationException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of ApplicationException with message
    /// </summary>
    public ApplicationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of ApplicationException with message and inner exception
    /// </summary>
    public ApplicationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}


public class NotFoundException : Exception
{
    /// <summary>
    /// Gets the name of the entity
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// Gets the key of the entity
    /// </summary>
    public object Key { get; }

    /// <summary>
    /// Initializes a new instance of NotFoundException
    /// </summary>
    public NotFoundException() : base()
    {
        EntityName = string.Empty;
        Key = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of NotFoundException with message
    /// </summary>
    public NotFoundException(string message) : base(message)
    {
        EntityName = string.Empty;
        Key = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of NotFoundException with message and inner exception
    /// </summary>
    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
        EntityName = string.Empty;
        Key = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of NotFoundException for a specific entity
    /// </summary>
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
        EntityName = name;
        Key = key;
    }
}



public class BadRequestException : ApplicationException
{
    public BadRequestException(string message) : base(message)
    {
    }
}

public class ValidationException : Exception
{
    /// <summary>
    /// Gets the validation failures
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Initializes a new instance of ValidationException
    /// </summary>
    public ValidationException() : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Initializes a new instance of ValidationException with failures
    /// </summary>
    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    /// <summary>
    /// Initializes a new instance of ValidationException with message
    /// </summary>
    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Initializes a new instance of ValidationException with property and message
    /// </summary>
    public ValidationException(string propertyName, string errorMessage) : this()
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }
}
public class UnauthorizedException : Exception
{
    /// <summary>
    /// Initializes a new instance of UnauthorizedException
    /// </summary>
    public UnauthorizedException() : base("User is not authorized.")
    {
    }

    /// <summary>
    /// Initializes a new instance of UnauthorizedException with message
    /// </summary>
    public UnauthorizedException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of UnauthorizedException with message and inner exception
    /// </summary>
    public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}


public class ForbiddenException : Exception
{
    /// <summary>
    /// Initializes a new instance of ForbiddenException
    /// </summary>
    public ForbiddenException() : base("Access is forbidden.")
    {
    }

    /// <summary>
    /// Initializes a new instance of ForbiddenException with message
    /// </summary>
    public ForbiddenException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of ForbiddenException with message and inner exception
    /// </summary>
    public ForbiddenException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
public class ConflictException : Exception
{
    /// <summary>
    /// Initializes a new instance of ConflictException
    /// </summary>
    public ConflictException() : base("A conflict has occurred.")
    {
    }

    /// <summary>
    /// Initializes a new instance of ConflictException with message
    /// </summary>
    public ConflictException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of ConflictException with message and inner exception
    /// </summary>
    public ConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
public class BusinessRuleException : Exception
{
    /// <summary>
    /// Gets the business rule that was violated
    /// </summary>
    public string Rule { get; }

    /// <summary>
    /// Gets the error code
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Initializes a new instance of BusinessRuleException
    /// </summary>
    public BusinessRuleException() : base("A business rule has been violated.")
    {
        Rule = string.Empty;
        Code = "BUSINESS_RULE_VIOLATION";
    }

    /// <summary>
    /// Initializes a new instance of BusinessRuleException with message
    /// </summary>
    public BusinessRuleException(string message) : base(message)
    {
        Rule = message;
        Code = "BUSINESS_RULE_VIOLATION";
    }

    /// <summary>
    /// Initializes a new instance of BusinessRuleException with rule and code
    /// </summary>
    public BusinessRuleException(string rule, string code, string message) : base(message)
    {
        Rule = rule;
        Code = code;
    }
}
