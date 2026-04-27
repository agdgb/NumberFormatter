using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;

namespace HumanNumbers.Currencies;

/// <summary>
/// Provides extensible mapping between ISO currency codes, their symbols, and region information.
/// </summary>
public static class CurrencyRegistry
{
    private static readonly ConcurrentDictionary<string, string> SymbolCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, RegionInfo?> RegionCache = new(StringComparer.OrdinalIgnoreCase);
    
    // Core fallback map for symbols not easily discovered by CultureInfo
    private static readonly ConcurrentDictionary<string, string> FallbackSymbolMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["USD"] = "$", ["EUR"] = "€", ["GBP"] = "£", ["JPY"] = "¥",
        ["CAD"] = "$", ["AUD"] = "$", ["CHF"] = "CHF", ["CNY"] = "¥",
        ["SEK"] = "kr", ["NZD"] = "$", ["MXN"] = "$", ["SGD"] = "$",
        ["HKD"] = "$", ["NOK"] = "kr", ["KRW"] = "₩", ["TRY"] = "₺",
        ["INR"] = "₹", ["RUB"] = "₽", ["BRL"] = "R$", ["ZAR"] = "R",
        ["DKK"] = "kr", ["PLN"] = "zł", ["TWD"] = "NT$", ["THB"] = "฿",
        ["MYR"] = "RM"
    };

    // Mapping for when we need to reconstruct a CultureInfo primarily based on Currency
    private static readonly ConcurrentDictionary<string, string> FallbackRegionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["USD"] = "US", ["EUR"] = "FR", ["GBP"] = "GB", ["JPY"] = "JP",
        ["CAD"] = "CA", ["AUD"] = "AU", ["CHF"] = "CH", ["CNY"] = "CN",
        ["SEK"] = "SE", ["NZD"] = "NZ", ["MXN"] = "MX", ["SGD"] = "SG",
        ["HKD"] = "HK", ["NOK"] = "NO", ["KRW"] = "KR", ["TRY"] = "TR",
        ["INR"] = "IN", ["RUB"] = "RU", ["BRL"] = "BR", ["ZAR"] = "ZA",
        ["DKK"] = "DK", ["PLN"] = "PL", ["TWD"] = "TW", ["THB"] = "TH",
        ["MYR"] = "MY"
    };

    /// <summary>
    /// Registers or overrides a currency mapping globally.
    /// </summary>
    /// <param name="currencyCode">The 3-letter ISO code (e.g., USD).</param>
    /// <param name="symbol">The visual symbol (e.g., $).</param>
    /// <param name="regionCode">Optional 2-letter ISO region code (e.g., US) for fallback culture resolution.</param>
    public static void RegisterCurrency(string currencyCode, string symbol, string? regionCode = null)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new ArgumentNullException(nameof(currencyCode));
            
        FallbackSymbolMap[currencyCode] = symbol ?? throw new ArgumentNullException(nameof(symbol));
        
        if (regionCode != null)
        {
            FallbackRegionMap[currencyCode] = regionCode;
        }

        // Invalidate caches
        SymbolCache.TryRemove(currencyCode, out _);
        RegionCache.TryRemove(currencyCode, out _);
    }

    /// <summary>
    /// Gets the visual symbol for an ISO currency code. Returns the code itself if unknown.
    /// </summary>
    public static string GetSymbol(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return string.Empty;

        return SymbolCache.GetOrAdd(currencyCode, code =>
        {
            // 1. Try explicit mapping
            if (FallbackSymbolMap.TryGetValue(code, out var symbol))
                return symbol;

            // 2. Try OS region discovery
            foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                try
                {
                    var region = new RegionInfo(culture.Name);
                    if (string.Equals(region.ISOCurrencySymbol, code, StringComparison.OrdinalIgnoreCase))
                    {
                        return region.CurrencySymbol;
                    }
                }
                catch
                {
                    // Ignore invalid cultures
                }
            }

            // 3. Fallback to just using the code (e.g. USD)
            return code.ToUpperInvariant();
        });
    }

    /// <summary>
    /// Attempts to find an appropriate OS RegionInfo for a given currency code.
    /// </summary>
    public static RegionInfo? GetRegion(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return null;

        return RegionCache.GetOrAdd(currencyCode, code =>
        {
            if (FallbackRegionMap.TryGetValue(code, out var explicitRegion))
            {
                try
                {
                    return new RegionInfo(explicitRegion);
                }
                catch { }
            }

            foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                try
                {
                    var region = new RegionInfo(culture.Name);
                    if (string.Equals(region.ISOCurrencySymbol, code, StringComparison.OrdinalIgnoreCase))
                    {
                        return region;
                    }
                }
                catch { }
            }

            return null;
        });
    }
}
