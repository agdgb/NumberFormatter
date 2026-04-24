namespace HumanNumbers;

/// <summary>
/// Defines how HumanNumbers handles formatting errors during execution.
/// </summary>
public enum HumanNumbersErrorMode
{
    /// <summary>
    /// Default. Prevents crashes by returning the raw unformatted string representation of the value.
    /// Triggers the optional OnFormattingError hook if configured.
    /// </summary>
    SafeFallback = 0,

    /// <summary>
    /// Strict correctness mode. Throws an exception if formatting fails.
    /// Recommended for backend pipelines and financial calculations where silent failure is unacceptable.
    /// </summary>
    Strict = 1
}
