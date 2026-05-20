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

    private static readonly Dictionary<string, string> InitialFallbackSymbols = new(StringComparer.OrdinalIgnoreCase)
    {
        ["USD"] = "$", ["EUR"] = "€", ["GBP"] = "£", ["JPY"] = "¥",
        ["CAD"] = "$", ["AUD"] = "$", ["CHF"] = "CHF", ["CNY"] = "¥",
        ["SEK"] = "kr", ["NZD"] = "$", ["MXN"] = "$", ["SGD"] = "$",
        ["HKD"] = "$", ["NOK"] = "kr", ["KRW"] = "₩", ["TRY"] = "₺",
        ["INR"] = "₹", ["RUB"] = "₽", ["BRL"] = "R$", ["ZAR"] = "R",
        ["DKK"] = "kr", ["PLN"] = "zł", ["TWD"] = "NT$", ["THB"] = "฿",
        ["MYR"] = "RM"
    };

    private static readonly Dictionary<string, string> InitialFallbackRegions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["USD"] = "US", ["EUR"] = "FR", ["GBP"] = "GB", ["JPY"] = "JP",
        ["CAD"] = "CA", ["AUD"] = "AU", ["CHF"] = "CH", ["CNY"] = "CN",
        ["SEK"] = "SE", ["NZD"] = "NZ", ["MXN"] = "MX", ["SGD"] = "SG",
        ["HKD"] = "HK", ["NOK"] = "NO", ["KRW"] = "KR", ["TRY"] = "TR",
        ["INR"] = "IN", ["RUB"] = "RU", ["BRL"] = "BR", ["ZAR"] = "ZA",
        ["DKK"] = "DK", ["PLN"] = "PL", ["TWD"] = "TW", ["THB"] = "TH",
        ["MYR"] = "MY"
    };

    private static readonly ConcurrentDictionary<string, string> FallbackSymbolMap = new(InitialFallbackSymbols, StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, string> FallbackRegionMap = new(InitialFallbackRegions, StringComparer.OrdinalIgnoreCase);

    // Pre-cache tables for O(1) OS region and symbol resolution
    private static readonly Lazy<Dictionary<string, string>> OsSymbolCache = new(() =>
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            try
            {
                var region = new RegionInfo(culture.Name);
                var code = region.ISOCurrencySymbol;
                if (!string.IsNullOrEmpty(code) && !map.ContainsKey(code))
                {
                    map[code] = region.CurrencySymbol;
                }
            }
            catch
            {
                // Ignore invalid cultures
            }
        }
        return map;
    });

    private static readonly Lazy<Dictionary<string, RegionInfo>> OsRegionCache = new(() =>
    {
        var map = new Dictionary<string, RegionInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            try
            {
                var region = new RegionInfo(culture.Name);
                var code = region.ISOCurrencySymbol;
                if (!string.IsNullOrEmpty(code) && !map.ContainsKey(code))
                {
                    map[code] = region;
                }
            }
            catch { }
        }
        return map;
    });

    /// <summary>
    /// Resets the currency registry and caches to their initial/default state.
    /// Useful for test isolation.
    /// </summary>
    public static void Reset()
    {
        SymbolCache.Clear();
        RegionCache.Clear();

        FallbackSymbolMap.Clear();
        foreach (var kvp in InitialFallbackSymbols)
        {
            FallbackSymbolMap[kvp.Key] = kvp.Value;
        }

        FallbackRegionMap.Clear();
        foreach (var kvp in InitialFallbackRegions)
        {
            FallbackRegionMap[kvp.Key] = kvp.Value;
        }
    }

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

            // 2. Try OS region discovery via pre-cached table
            if (OsSymbolCache.Value.TryGetValue(code, out var osSymbol))
                return osSymbol;

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

            if (OsRegionCache.Value.TryGetValue(code, out var region))
            {
                return region;
            }

            return null;
        });
    }
}
