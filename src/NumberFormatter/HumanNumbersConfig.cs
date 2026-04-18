using HumanNumbers.Formatting;

namespace HumanNumbers;

/// <summary>
/// Provides global configuration for the HumanNumbers platform.
/// </summary>
public class HumanNumbersConfig
{
    private static HumanNumbersConfig _instance = new();

    /// <summary>
    /// Gets the singleton configuration instance.
    /// </summary>
    public static HumanNumbersConfig Instance
    {
        get => _instance;
        internal set => _instance = value;
    }

    /// <summary>
    /// Obsolete access property pointing to the singleton's default options.
    /// Maintained for temporary backwards compatibility.
    /// </summary>
    public static HumanNumberFormatOptions Default
    {
        get => Instance.GlobalOptions;
        set => Instance.GlobalOptions = value;
    }

    /// <summary>
    /// Gets or sets the global default formatting options.
    /// </summary>
    public HumanNumberFormatOptions GlobalOptions { get; set; } = new();
}
