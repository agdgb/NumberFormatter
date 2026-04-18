using System;
using HumanNumbers.Formatting;

namespace HumanNumbers.AspNetCore;

/// <summary>
/// Configuration options for HumanNumbers ASP.NET Core integration.
/// </summary>
public class HumanNumbersOptions
{
    private HumanNumberFormatOptions _coreOptions = new();

    /// <summary>
    /// Default number of decimal places for formatting.
    /// </summary>
    public int DefaultDecimalPlaces { get => _coreOptions.DecimalPlaces; set => _coreOptions.DecimalPlaces = value; }

    /// <summary>
    /// Advanced configuration for the core HumanNumbers library.
    /// </summary>
    public HumanNumberFormatOptions CoreOptions { get => _coreOptions; set => _coreOptions = value; }

    /// <summary>
    /// Whether to automatically log formatting errors to the ASP.NET Core ILogger.
    /// Default: <see langword="true" />.
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Whether to enable automatic response formatting via filters.
    /// </summary>
    public bool EnableAutoFormatting { get; set; } = false;

    /// <summary>
    /// The default auto-formatting mode.
    /// </summary>
    public AutoFormatMode AutoFormatMode { get; set; } = AutoFormatMode.OptInAttributeOnly;

    /// <summary>
    /// Whether to register standard JSON converters.
    /// </summary>
    public bool EnableJsonConverters { get; set; } = true;

    /// <summary>
    /// Whether to respect data annotations like [DisplayFormat].
    /// </summary>
    public bool RespectDataAnnotations { get; set; } = true;

    /// <summary>
    /// The name of the default formatting policy.
    /// </summary>
    public string DefaultPolicyName { get; set; } = "Default";
}

/// <summary>
/// Modes for automatic number formatting in API responses.
/// </summary>
public enum AutoFormatMode
{
    /// <summary>
    /// Auto-formatting is disabled.
    /// </summary>
    Off,

    /// <summary>
    /// Only properties marked with [HumanNumber] are formatted.
    /// </summary>
    OptInAttributeOnly,

    /// <summary>
    /// All numeric properties are formatted unless marked with [NoHumanFormat].
    /// </summary>
    OptOutAttribute,

    /// <summary>
    /// All numeric properties are formatted globally.
    /// </summary>
    Global
}
