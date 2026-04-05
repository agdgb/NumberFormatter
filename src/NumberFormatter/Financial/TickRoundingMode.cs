namespace NumberFormatter.Financial;

/// <summary>
/// Defines the rounding behavior when snapping to a tick size.
/// </summary>
public enum TickRoundingMode
{
    /// <summary>
    /// Rounds to the nearest tick. Standard rounding.
    /// </summary>
    Nearest,
    
    /// <summary>
    /// Always rounds up to the next tick value (Ceiling).
    /// </summary>
    Up,
    
    /// <summary>
    /// Always rounds down to the previous tick value (Floor).
    /// </summary>
    Down
}
