using System;

namespace HumanNumbers.AspNetCore;

/// <summary>
/// Defines a contract for mapping keys to ISO currency codes.
/// Used by <see cref="CurrencyDictionaryConverter"/> to support dependency injection of custom mappings.
/// </summary>
public interface ICurrencyMappingProvider
{
    /// <summary>
    /// Maps a key to an ISO currency code.
    /// </summary>
    /// <param name="key">The key to map (e.g., "USA", "EUR").</param>
    /// <returns>The ISO currency code (e.g., "USD", "EUR").</returns>
    string MapKeyToCurrencyCode(string key);
}

/// <summary>
/// The default implementation of <see cref="ICurrencyMappingProvider"/> using hardcoded mappings.
/// </summary>
public class DefaultCurrencyMappingProvider : ICurrencyMappingProvider
{
    /// <inheritdoc />
    public string MapKeyToCurrencyCode(string key)
    {
        return key switch
        {
            "USA" => "USD",
            "EUR" => "EUR",
            "JPY" => "JPY",
            "GBP" => "GBP",
            _ => "USD" // fallback
        };
    }
}
