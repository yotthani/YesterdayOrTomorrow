namespace StarTrekGame.Domain.SharedKernel;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// Used for operations that may have validation errors or business rule violations.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public IReadOnlyList<string> Errors { get; }

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = string.IsNullOrEmpty(error) ? Array.Empty<string>() : new[] { error };
    }

    protected Result(bool isSuccess, IEnumerable<string> errors)
    {
        IsSuccess = isSuccess;
        var errorList = errors?.ToList() ?? new List<string>();
        Errors = errorList.AsReadOnly();
        Error = errorList.FirstOrDefault() ?? string.Empty;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(IEnumerable<string> errors) => new(false, errors);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
}

/// <summary>
/// Represents the result of an operation that returns a value on success.
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T value) : base(true, string.Empty)
    {
        Value = value;
    }

    private Result(string error) : base(false, error)
    {
        Value = default;
    }

    private Result(IEnumerable<string> errors) : base(false, errors)
    {
        Value = default;
    }

    public static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(string error) => new(error);
    public new static Result<T> Failure(IEnumerable<string> errors) => new(errors);

    /// <summary>
    /// Maps the result value to a new type if successful.
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return IsSuccess 
            ? Result<TNew>.Success(mapper(Value!)) 
            : Result<TNew>.Failure(Error);
    }

    /// <summary>
    /// Chains another operation if this result is successful.
    /// </summary>
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
    {
        return IsSuccess 
            ? binder(Value!) 
            : Result<TNew>.Failure(Error);
    }

    /// <summary>
    /// Returns the value or a default if the result is a failure.
    /// </summary>
    public T GetValueOrDefault(T defaultValue = default!)
    {
        return IsSuccess ? Value! : defaultValue;
    }

    /// <summary>
    /// Implicit conversion from value to successful result.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);
}

/// <summary>
/// Represents the result of a turn processing operation.
/// </summary>
public class ProcessedTurnResult
{
    public int TurnNumber { get; init; }
    public int NewTurn { get; init; }
    public bool WasProcessed { get; init; }
    public List<string> Events { get; init; } = new();
    public List<CombatReport> CombatReports { get; init; } = new();
    public Dictionary<Guid, FactionTurnSummary> FactionSummaries { get; init; } = new();

    public static ProcessedTurnResult Success(int turnNumber, int newTurn, List<string> events)
    {
        return new ProcessedTurnResult
        {
            TurnNumber = turnNumber,
            NewTurn = newTurn,
            WasProcessed = true,
            Events = events
        };
    }

    public static ProcessedTurnResult NotReady(int turnNumber, string reason)
    {
        return new ProcessedTurnResult
        {
            TurnNumber = turnNumber,
            NewTurn = turnNumber,
            WasProcessed = false,
            Events = new List<string> { reason }
        };
    }
}

/// <summary>
/// Summary of what happened to a faction during a turn.
/// </summary>
public class FactionTurnSummary
{
    public Guid FactionId { get; init; }
    public int CreditsEarned { get; init; }
    public int CreditsSpent { get; init; }
    public int PopulationGrowth { get; init; }
    public int ShipsBuilt { get; init; }
    public int ShipsLost { get; init; }
    public int SystemsGained { get; init; }
    public int SystemsLost { get; init; }
    public List<string> NotableEvents { get; init; } = new();
}

/// <summary>
/// Report of a combat that occurred during turn processing.
/// </summary>
public class CombatReport
{
    public Guid SystemId { get; init; }
    public string SystemName { get; init; } = string.Empty;
    public Guid AttackerId { get; init; }
    public Guid DefenderId { get; init; }
    public string AttackerName { get; init; } = string.Empty;
    public string DefenderName { get; init; } = string.Empty;
    public bool AttackerWon { get; init; }
    public int AttackerShipsLost { get; init; }
    public int DefenderShipsLost { get; init; }
    public List<string> CombatLog { get; init; } = new();
}
