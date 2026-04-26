using HumanNumbers.Formatting;

namespace HumanNumbers;

/// <summary>
/// Provides global configuration for the HumanNumbers platform.
/// </summary>
public class HumanNumbersConfig
{
    private static HumanNumbersConfig _instance = new();

    private HumanNumbersConfig()
    {
        // Register default system policies
        AddPolicy("Strict", new HumanNumberFormatOptions { PromotionThreshold = 1.0m });
    }

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

    /// <summary>
    /// Gets or sets the global error handling mode. This is a shortcut for <see cref="GlobalOptions"/> ErrorMode.
    /// </summary>
    public HumanNumbersErrorMode ErrorMode
    {
        get => GlobalOptions.ErrorMode;
        set
        {
            var options = GlobalOptions;
            options.ErrorMode = value;
            GlobalOptions = options;
        }
    }

    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, HumanNumberFormatOptions> _policies = new(System.StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a named formatting policy.
    /// </summary>
    public void AddPolicy(string name, HumanNumberFormatOptions options)
    {
        _policies[name] = options;
    }

    /// <summary>
    /// Gets a registered formatting policy by name. Returns false if not found.
    /// </summary>
    public bool TryGetPolicy(string name, out HumanNumberFormatOptions options)
    {
        return _policies.TryGetValue(name, out options);
    }

    /// <summary>
    /// Gets the names of all currently registered formatting policies.
    /// </summary>
    public System.Collections.Generic.IEnumerable<string> GetPolicyNames()
    {
        return _policies.Keys;
    }

    /// <summary>
    /// Gets all registered policies and their configurations.
    /// </summary>
    public System.Collections.Generic.IReadOnlyDictionary<string, HumanNumberFormatOptions> GetPolicies()
    {
        return _policies;
    }
}
