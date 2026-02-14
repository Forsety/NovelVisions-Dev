// src/BuildingBlocks/SharedKernel/NovelVision.BuildingBlocks.SharedKernel/Results/Result.cs
namespace NovelVision.BuildingBlocks.SharedKernel.Results;

/// <summary>
/// Represents the result of an operation
/// </summary>
public class Result
{
    /// <summary>
    /// Gets the list of errors
    /// </summary>
    public List<Error> Errors { get; }

    /// <summary>
    /// Gets the first error or null if no errors
    /// </summary>
    public Error? Error => Errors.FirstOrDefault();

    /// <summary>
    /// Indicates if the operation succeeded (alias for IsSucceeded)
    /// </summary>
    public bool IsSuccess => !Errors.Any();

    /// <summary>
    /// Indicates if the operation succeeded
    /// </summary>
    public bool IsSucceeded => !Errors.Any();

    /// <summary>
    /// Indicates if the operation failed (alias for IsFailed)
    /// </summary>
    public bool IsFailure => Errors.Any();

    /// <summary>
    /// Indicates if the operation failed
    /// </summary>
    public bool IsFailed => Errors.Any();

    protected Result()
    {
        Errors = new List<Error>();
    }

    protected Result(Error error)
    {
        Errors = new List<Error> { error };
    }

    protected Result(IEnumerable<Error> errors)
    {
        Errors = errors?.ToList() ?? new List<Error>();
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result Success()
    {
        return new Result();
    }

    /// <summary>
    /// Creates a failed result with an error
    /// </summary>
    public static Result Failure(Error error)
    {
        return new Result(error);
    }

    /// <summary>
    /// Creates a failed result with an error message
    /// </summary>
    public static Result Failure(string errorMessage)
    {
        return new Result(Error.Failure(errorMessage));
    }

    /// <summary>
    /// Creates a failed result with multiple errors
    /// </summary>
    public static Result Failure(IEnumerable<Error> errors)
    {
        return new Result(errors);
    }

    /// <summary>
    /// Implicit conversion from Error to Result
    /// </summary>
    public static implicit operator Result(Error error)
    {
        return Failure(error);
    }
}

/// <summary>
/// Represents the result of an operation with a return value
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;

    /// <summary>
    /// Gets the value if the operation succeeded
    /// </summary>
    public T Value
    {
        get
        {
            if (IsFailed)
                throw new InvalidOperationException("Cannot access Value when Result has failed.");

            return _value!;
        }
    }

    protected Result(T value) : base()
    {
        _value = value;
    }

    protected Result(Error error) : base(error)
    {
        _value = default;
    }

    protected Result(IEnumerable<Error> errors) : base(errors)
    {
        _value = default;
    }

    /// <summary>
    /// Creates a successful result with a value
    /// </summary>
    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    /// <summary>
    /// Creates a failed result with an error
    /// </summary>
    public new static Result<T> Failure(Error error)
    {
        return new Result<T>(error);
    }

    /// <summary>
    /// Creates a failed result with an error message
    /// </summary>
    public new static Result<T> Failure(string errorMessage)
    {
        return new Result<T>(Error.Failure(errorMessage));
    }

    /// <summary>
    /// Creates a failed result with multiple errors
    /// </summary>
    public new static Result<T> Failure(IEnumerable<Error> errors)
    {
        return new Result<T>(errors);
    }

    /// <summary>
    /// Implicit conversion from value to Result<T>
    /// </summary>
    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }

    /// <summary>
    /// Implicit conversion from Error to Result<T>
    /// </summary>
    public static implicit operator Result<T>(Error error)
    {
        return Failure(error);
    }
}