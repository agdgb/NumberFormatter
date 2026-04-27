namespace HumanNumbers.Financial
{
    /// <summary>
    /// Defines a provider that converts numeric values into spelled-out words (e.g., for check writing).
    /// </summary>
    public interface IWordsProvider
    {
        /// <summary>
        /// Converts the integral part of a number into words.
        /// </summary>
        /// <param name="value">The absolute integral value up to decimal's maximum range.</param>
        /// <returns>The string representation in words.</returns>
        string ToWords(decimal value);

        /// <summary>
        /// Gets the word representation for a negative sign.
        /// </summary>
        string NegativeWord { get; }
        
        /// <summary>
        /// Gets the word to use to join the major and minor parts (e.g., "and").
        /// </summary>
        string ConjunctionWord { get; }
    }
}

namespace NumberFormatter.Financial
{
    /// <summary>
    /// Obsolete alias for <see cref="HumanNumbers.Financial.IWordsProvider"/>.
    /// </summary>
    [System.Obsolete("Use HumanNumbers.Financial.IWordsProvider instead. This alias will be removed in a future version.")]
    public interface IWordsProvider : HumanNumbers.Financial.IWordsProvider { }
}
