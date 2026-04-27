using HumanNumbers.Formatting;

namespace HumanNumbers.AspNetCore;

/// <summary>
/// Defines a formatting policy for HumanNumbers.
/// </summary>
public interface IHumanNumbersPolicy
{
    /// <summary>
    /// The unique name of the policy.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the formatting options for this policy.
    /// </summary>
    HumanNumberFormatOptions GetOptions();
}

/// <summary>
/// Built-in formatting policies for common scenarios.
/// </summary>
public static class HumanNumbersPolicies
{
    /// <summary>
    /// Standard default policy.
    /// </summary>
    public class DefaultPolicy : IHumanNumbersPolicy
    {
        /// <inheritdoc/>
        public string Name => "Default";
        /// <inheritdoc/>
        public HumanNumberFormatOptions GetOptions() => new() { DecimalPlaces = 2 };
    }

    /// <summary>
    /// Policy optimized for dashboards (high-level summaries, 0-1 decimals).
    /// </summary>
    public class DashboardPolicy : IHumanNumbersPolicy
    {
        /// <inheritdoc/>
        public string Name => "Dashboard";
        /// <inheritdoc/>
        public HumanNumberFormatOptions GetOptions() => new()
        {
            DecimalPlaces = 0,
            PromotionThreshold = 0.9m,
            AlwaysShowSuffix = false
        };
    }

    /// <summary>
    /// Policy for financial applications (precise, 2 decimals, currency support).
    /// </summary>
    public class FinancialPolicy : IHumanNumbersPolicy
    {
        /// <inheritdoc/>
        public string Name => "Financial";
        /// <inheritdoc/>
        public HumanNumberFormatOptions GetOptions() => new()
        {
            DecimalPlaces = 2,
            Threshold = 1000m
        };
    }

    /// <summary>
    /// Strict policy for financial applications (throws on formatting errors instead of falling back).
    /// </summary>
    public class StrictFinancialPolicy : IHumanNumbersPolicy
    {
        /// <inheritdoc/>
        public string Name => "StrictFinancial";
        /// <inheritdoc/>
        public HumanNumberFormatOptions GetOptions() => new()
        {
            DecimalPlaces = 2,
            Threshold = 1000m,
            ErrorMode = HumanNumbersErrorMode.Strict
        };
    }


    /// <summary>
    /// Policy for public APIs (efficient short formatting).
    /// </summary>
    public class PublicApiPolicy : IHumanNumbersPolicy
    {
        /// <inheritdoc/>
        public string Name => "PublicApi";
        /// <inheritdoc/>
        public HumanNumberFormatOptions GetOptions() => new()
        {
            DecimalPlaces = 1,
            Threshold = 1000m
        };
    }
}
