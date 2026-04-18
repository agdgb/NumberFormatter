namespace HumanNumbers.Currencies;

/// <summary>
/// Defines the possible positions for the currency symbol relative to the formatted number.
/// </summary>
public enum CurrencyPosition
{
    /// <summary>
    /// Currency symbol appears immediately before the number (e.g., $1.23K).
    /// </summary>
    Before,

    /// <summary>
    /// Currency symbol appears immediately after the number (e.g., 1.23K$).
    /// </summary>
    After,

    /// <summary>
    /// Currency symbol appears before the number with a space (e.g., $ 1.23K).
    /// </summary>
    BeforeWithSpace,

    /// <summary>
    /// Currency symbol appears after the number with a space (e.g., 1.23K $).
    /// </summary>
    AfterWithSpace
}
