namespace HumanNumbers.Suffixes
{
    /// <summary>
    /// Provides predefined arrays of <see cref="MagnitudeSuffix"/> instances for different formatting contexts.
    /// Arrays are ordered from largest to smallest magnitude for efficient suffix selection.
    /// Used internally by <see cref="HumanNumbers.HumanNumber"/> for short number formatting.
    /// </summary>
    public static class StandardSuffixSets
    {
        /// <summary>
        /// Default short number suffixes used by <see cref="HumanNumbers.HumanNumber"/>.
        /// Covers: Quintillion (Qi), Quadrillion (Qa), Trillion (T), Billion (B), Million (M), Thousand (K).
        /// </summary>
        /// <remarks>
        /// Thresholds: 10¹⁸, 10¹⁵, 10¹², 10⁹, 10⁶, 10³, 1.
        /// Array intentionally includes final <c>new(1m, "")</c> entry for no-suffix fallback.
        /// </remarks>
        public static readonly MagnitudeSuffix[] Default =
        {
            new(1_000_000_000_000_000_000m, "Qi"), // Quintillion (10¹⁸)
            new(1_000_000_000_000_000m, "Qa"),     // Quadrillion (10¹⁵)  
            new(1_000_000_000_000m, "T"),          // Trillion (10¹²)
            new(1_000_000_000m, "B"),              // Billion (10⁹)
            new(1_000_000m, "M"),                  // Million (10⁶)
            new(1_000m, "K"),                      // Thousand (10³)
            new(1m, "")                            // No suffix (absolute fallback)
        };

        /// <summary>
        /// Financial-style suffixes matching <see cref="Default"/> exactly.
        /// Provided for semantic clarity in financial contexts.
        /// </summary>
        public static readonly MagnitudeSuffix[] Financial =
        {
            new(1_000_000_000_000_000_000m, "Qi"), // Quintillion
            new(1_000_000_000_000_000m, "Qa"),     // Quadrillion
            new(1_000_000_000_000m, "T"),          // Trillion
            new(1_000_000_000m, "B"),              // Billion
            new(1_000_000m, "M"),                  // Million
            new(1_000m, "K"),                      // Thousand
            new(1m, "")                            // No suffix
        };

        /// <summary>
        /// Scientific notation prefixes following SI standards.
        /// Uses: Exa (E), Peta (P), Tera (T), Giga (G), Mega (M), kilo (k).
        /// </summary>
        /// <remarks>
        /// Note lowercase "k" for kilo per SI convention (vs uppercase "K" in <see cref="Default"/>).
        /// Thresholds identical to other sets: 10¹⁸, 10¹⁵, 10¹², 10⁹, 10⁶, 10³, 1.
        /// </remarks>
        public static readonly MagnitudeSuffix[] Scientific =
        {
            new(1_000_000_000_000_000_000m, "E"), // Exa (10¹⁸)
            new(1_000_000_000_000_000m, "P"),     // Peta (10¹⁵)
            new(1_000_000_000_000m, "T"),         // Tera (10¹²)
            new(1_000_000_000m, "G"),             // Giga (10⁹)
            new(1_000_000m, "M"),                 // Mega (10⁶)
            new(1_000m, "k"),                     // Kilo (10³)
            new(1m, "")                           // No suffix
        };
    }
}
