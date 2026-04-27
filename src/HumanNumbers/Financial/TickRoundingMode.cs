namespace HumanNumbers.Financial
{
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
}

namespace NumberFormatter.Financial
{
    /// <summary>
    /// Obsolete alias for <see cref="HumanNumbers.Financial.TickRoundingMode"/>.
    /// </summary>
    [System.Obsolete("Use HumanNumbers.Financial.TickRoundingMode instead. This alias will be removed in a future version.")]
    public enum TickRoundingMode
    {
        /// <summary>Nearest tick.</summary>
        Nearest = HumanNumbers.Financial.TickRoundingMode.Nearest,
        /// <summary>Next tick.</summary>
        Up = HumanNumbers.Financial.TickRoundingMode.Up,
        /// <summary>Previous tick.</summary>
        Down = HumanNumbers.Financial.TickRoundingMode.Down
    }
}
