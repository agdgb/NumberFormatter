using System;
using System.Collections.Generic;
using System.Text;

namespace NumberFormatter;

/// <summary>
/// Immutable record struct representing a numeric magnitude threshold
/// and its associated suffix (for example, 1_000m with suffix "K").
/// </summary>
/// <param name="Threshold">
/// The minimum absolute value at which this suffix becomes applicable.
/// </param>
/// <param name="Suffix">
/// The string suffix to append when the threshold is met (e.g. "K", "M", "B").
/// </param>
internal readonly record struct NumberSuffix(decimal Threshold, string Suffix)
{
    /// <summary>
    /// Determines whether this suffix applies to the specified value
    /// based on its absolute magnitude and this instance's threshold.
    /// </summary>
    /// <param name="value">The numeric value to test.</param>
    /// <returns>
    /// <see langword="true"/> if the absolute value is greater than or equal
    /// to <see cref="Threshold"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsApplicable(decimal value) => Math.Abs(value) >= Threshold;
}