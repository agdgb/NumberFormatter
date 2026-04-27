using System;

namespace HumanNumbers.Suffixes
{
    /// <summary>
    /// Represents a numeric suffix for a specific magnitude threshold.
    /// </summary>
    /// <param name="Threshold">The numeric threshold where this suffix starts being used.</param>
    /// <param name="Suffix">The string suffix (e.g. "K", "M").</param>
    public record MagnitudeSuffix(decimal Threshold, string Suffix)
    {
        /// <summary>
        /// Determines whether this suffix applies to the specified value.
        /// </summary>
        public bool IsApplicable(decimal value) => Math.Abs(value) >= Threshold;
    }
}