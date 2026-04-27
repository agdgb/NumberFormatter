using System.Collections.Concurrent;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using HumanNumbers.Currencies;
using HumanNumbers.Formatting;
using HumanNumbers.Suffixes;

namespace HumanNumbers
{
    /// <summary>
    /// Provides culture-aware human-readable number formatting extensions.
    /// This is the primary entry point for the HumanNumbers platform.
    /// </summary>
    public static class HumanNumber
    {
        private static readonly MagnitudeSuffix[] DefaultSuffixes = StandardSuffixSets.Default;
        private static readonly ConcurrentDictionary<string, NumberFormatInfo> FormatCache = new();
        
        // Suffix multipliers used for parsing
        private static readonly Dictionary<string, decimal> SuffixMultipliers = new(StringComparer.OrdinalIgnoreCase)
        {
            ["k"] = 1000m,
            ["K"] = 1000m,
            ["M"] = 1000000m,
            ["B"] = 1000000000m,
            ["T"] = 1000000000000m,
            ["Qa"] = 1000000000000000m,
            ["Qi"] = 1000000000000000000m
        };

        #region Public API

        /// <summary>
        /// Configures the global default settings for the HumanNumbers platform.
        /// </summary>
        public static void Configure(Action<HumanNumbersConfig> configure)
        {
            configure?.Invoke(HumanNumbersConfig.Instance);
        }

        #region Fluent API

        /// <summary>
        /// Begins a fluent formatting pipeline for the specified value.
        /// </summary>
        public static FormattingContext Format(decimal value) => new FormattingContext(value);

        /// <summary>
        /// Context for fluent formatting.
        /// </summary>
        public readonly struct FormattingContext
        {
            private readonly decimal _value;
            private readonly HumanNumberFormatOptions? _options;
            private readonly CultureInfo? _culture;

            internal FormattingContext(decimal value, HumanNumberFormatOptions? options = null, CultureInfo? culture = null)
            {
                _value = value;
                _options = options;
                _culture = culture;
            }

            /// <summary>
            /// Applies specific formatting options to the context.
            /// </summary>
            public FormattingContext UsingOptions(HumanNumberFormatOptions options)
            {
                return new FormattingContext(_value, options, _culture);
            }

            /// <summary>
            /// Applies a specific culture to the formatting context.
            /// </summary>
            public FormattingContext UsingCulture(CultureInfo culture)
            {
                return new FormattingContext(_value, _options, culture);
            }

            /// <summary>
            /// Applies a predefined named policy to the formatting context.
            /// </summary>
            public FormattingContext UsingPolicy(string policyName)
            {
                if (HumanNumbersConfig.Instance.TryGetPolicy(policyName, out var options))
                {
                    return new FormattingContext(_value, options, _culture);
                }
                return this; // Fallback to default if policy not found
            }

            /// <summary>
            /// Switches to strict numeric precision where suffixes are only promoted 
            /// when the absolute value exactly reaches the next magnitude.
            /// </summary>
            public FormattingContext Strict()
            {
                var options = _options ?? HumanNumbersConfig.Instance.GlobalOptions;
                options.PromotionThreshold = 1.0m;
                return new FormattingContext(_value, options, _culture);
            }

            /// <summary>
            /// Executes the formatting and returns a human-readable string.
            /// </summary>
            public string ToHuman()
            {
                var options = _options ?? HumanNumbersConfig.Instance.GlobalOptions;
                if (!HumanNumber.TryFormatNumber(_value, options, _culture, out var result))
                {
                    if (options.ErrorMode == HumanNumbersErrorMode.Strict)
                        throw new FormatException($"Failed to format value {_value} to human readable format.");
                    return _value.ToString(CultureInfo.InvariantCulture); // Safe fallback
                }
                return result;
            }

            /// <summary>
            /// Executes the formatting as a currency and returns a human-readable string.
            /// </summary>
            public string ToHumanCurrency(string? currencyCode = null)
            {
                var options = _options ?? HumanNumbersConfig.Instance.GlobalOptions;
                
                if (!string.IsNullOrEmpty(currencyCode))
                {
                    options.CurrencySymbol = GetCurrencySymbol(currencyCode!);
                }
                else if (_culture != null && _culture.Name != "")
                {
                    options.CurrencySymbol = _culture.NumberFormat.CurrencySymbol;
                }
                else if (options.CurrencySymbol == null)
                {
                    var activeCulture = _culture ?? CultureInfo.CurrentCulture;
                    if (activeCulture.Name != "")
                        options.CurrencySymbol = activeCulture.NumberFormat.CurrencySymbol;
                    else
                        options.CurrencySymbol = new CultureInfo("en-US").NumberFormat.CurrencySymbol;
                }

                if (!HumanNumber.TryFormatNumber(_value, options, _culture, out var result))
                {
                    if (options.ErrorMode == HumanNumbersErrorMode.Strict)
                        throw new FormatException($"Failed to format currency value {_value} to human readable format.");
                    return _value.ToString(CultureInfo.InvariantCulture); // Safe fallback
                }
                return result;
            }
        }

        #endregion

        /// <summary>
        /// Formats a number to a human-readable string (e.g., 1.23M, 5.68B)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToHuman(
            this decimal value,
            int? decimalPlaces = null,
            CultureInfo? culture = null)
        {
            var globalOptions = HumanNumbersConfig.Instance.GlobalOptions;
            if (decimalPlaces == null || decimalPlaces == globalOptions.DecimalPlaces)
            {
                if (TryFormatNumber(value, globalOptions, culture, out var result)) return result;
            }
            else
            {
                var options = globalOptions with { DecimalPlaces = decimalPlaces.Value };
                if (TryFormatNumber(value, options, culture, out var result)) return result;
            }

            return value.ToString(culture ?? CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Attempts to format a number to a human-readable string.
        /// </summary>
        public static bool TryToHuman(
            this decimal value,
            out string result,
            int? decimalPlaces = null,
            CultureInfo? culture = null)
        {
            var globalOptions = HumanNumbersConfig.Instance.GlobalOptions;
            if (decimalPlaces == null || decimalPlaces == globalOptions.DecimalPlaces)
            {
                return TryFormatNumber(value, globalOptions, culture, out result);
            }
            
            var options = globalOptions with { DecimalPlaces = decimalPlaces.Value };
            return TryFormatNumber(value, options, culture, out result);
        }

        /// <summary>
        /// Attempts to format a number to a human-readable string with custom options.
        /// </summary>
        public static bool TryToHuman(
            this decimal value,
            HumanNumberFormatOptions options,
            out string result,
            CultureInfo? culture = null)
        {
            return TryFormatNumber(value, options, culture, out result);
        }

#if !NETSTANDARD2_0
        /// <summary>
        /// Formats a number into a character span.
        /// </summary>
        public static bool ToHuman(
            this decimal value,
            Span<char> destination,
            out int charsWritten,
            int? decimalPlaces = null,
            CultureInfo? culture = null)
        {
            return TryFormat(value, destination, out charsWritten, decimalPlaces, culture);
        }

        /// <summary>
        /// Attempts to format a number into a character span.
        /// </summary>
        public static bool TryFormat(
            this decimal value,
            Span<char> destination,
            out int charsWritten,
            int? decimalPlaces = null,
            CultureInfo? culture = null)
        {
            var globalOptions = HumanNumbersConfig.Instance.GlobalOptions;
            
            // Critical check: avoid record cloning if possible
            if (!decimalPlaces.HasValue || decimalPlaces.Value == globalOptions.DecimalPlaces)
            {
                return TryFormatNumber(value, globalOptions, culture, destination, out charsWritten);
            }

            // Fallback for custom precision
            var options = globalOptions with { DecimalPlaces = decimalPlaces.Value };
            return TryFormatNumber(value, options, culture, destination, out charsWritten);
        }

        /// <summary>
        /// Attempts to format a number into a character span with custom options.
        /// </summary>
        public static bool TryFormat(
            this decimal value,
            Span<char> destination,
            out int charsWritten,
            HumanNumberFormatOptions options,
            CultureInfo? culture = null)
        {
            return TryFormatNumber(value, options, culture, destination, out charsWritten);
        }
#endif

        /// <summary>
        /// Formats a number to a human-readable string with custom options
        /// </summary>
        public static string ToHuman(
            this decimal value,
            HumanNumberFormatOptions options,
            CultureInfo? culture = null)
        {
            if (TryFormatNumber(value, options, culture, out var result)) return result;
            if (options.ErrorMode == HumanNumbersErrorMode.Strict) throw new FormatException($"Failed to format value {value}");
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Attempts to format a number as currency in short form.
        /// </summary>
        public static bool TryToHumanCurrency(
            this decimal value,
            out string result,
            int? decimalPlaces = null,
            CultureInfo? culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            if (culture.Name == "") culture = new CultureInfo("en-US");

            var globalOptions = HumanNumbersConfig.Instance.GlobalOptions;
            var symbol = culture.NumberFormat.CurrencySymbol;

            if ((decimalPlaces == null || decimalPlaces == globalOptions.DecimalPlaces) && symbol == globalOptions.CurrencySymbol)
            {
                return TryFormatNumber(value, globalOptions, culture, out result);
            }

            var options = globalOptions with
            {
                DecimalPlaces = decimalPlaces ?? globalOptions.DecimalPlaces,
                CurrencySymbol = symbol
            };
            return TryFormatNumber(value, options, culture, out result);
        }

        /// <summary>
        /// Formats a number as currency using specified options.
        /// </summary>
        public static string ToHumanCurrency(
            this decimal value,
            HumanNumberFormatOptions options,
            CultureInfo? culture = null)
        {
            if (TryFormatNumber(value, options, culture, out var result)) return result;
            if (options.ErrorMode == HumanNumbersErrorMode.Strict) throw new FormatException($"Failed to format currency value {value}");
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a number as currency in short form
        /// </summary>
        public static string ToHumanCurrency(
            this decimal value,
            int? decimalPlaces = null,
            CultureInfo? culture = null)
        {
            if (TryToHumanCurrency(value, out var result, decimalPlaces, culture)) return result;
            var options = HumanNumbersConfig.Instance.GlobalOptions;
            if (options.ErrorMode == HumanNumbersErrorMode.Strict) throw new FormatException($"Failed to format currency value {value}");
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Attempts to format a number as currency with explicit currency code.
        /// </summary>
        public static bool TryToHumanCurrency(
            this decimal value,
            string currencyCode,
            out string result,
            int? decimalPlaces = null,
            CultureInfo? culture = null)
        {
            var symbol = GetCurrencySymbol(currencyCode);
            var globalOptions = HumanNumbersConfig.Instance.GlobalOptions;

            if ((decimalPlaces == null || decimalPlaces == globalOptions.DecimalPlaces) && symbol == globalOptions.CurrencySymbol)
            {
                return TryFormatNumber(value, globalOptions, culture, out result);
            }

            var options = globalOptions with
            {
                DecimalPlaces = decimalPlaces ?? globalOptions.DecimalPlaces,
                CurrencySymbol = symbol
            };
            return TryFormatNumber(value, options, culture, out result);
        }

        /// <summary>
        /// Formats a number as currency with explicit currency code
        /// </summary>
        public static string ToHumanCurrency(
            this decimal value,
            string currencyCode,
            int? decimalPlaces = null,
            CultureInfo? culture = null)
        {
            if (TryToHumanCurrency(value, currencyCode, out var result, decimalPlaces, culture)) return result;
            var options = HumanNumbersConfig.Instance.GlobalOptions;
            if (options.ErrorMode == HumanNumbersErrorMode.Strict) throw new FormatException($"Failed to format currency value {value}");
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Attempts to format any numeric type to a human-readable string.
        /// </summary>
#if NET7_0_OR_GREATER
        public static bool TryToHuman<T>(
            this T value,
            out string result,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : INumber<T>
#else
        public static bool TryToHuman<T>(
            this T value,
            out string result,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct
#endif
        {
            try
            {
                var decimalValue = Convert.ToDecimal(value);
                return TryToHuman(decimalValue, out result, decimalPlaces, culture);
            }
            catch (Exception ex)
            {
                HumanNumbersConfig.Instance.GlobalOptions.OnFormattingError?.Invoke(ex);
                result = value.ToString() ?? string.Empty;
                return false;
            }
        }

        /// <summary>
        /// Generic version for any numeric type
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NET7_0_OR_GREATER
        public static string ToHuman<T>(
            this T value,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : INumber<T>
#else
        public static string ToHuman<T>(
            this T value,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct
#endif
        {
            if (TryToHuman(value, out var result, decimalPlaces, culture)) return result;
            if (HumanNumbersConfig.Instance.GlobalOptions.ErrorMode == HumanNumbersErrorMode.Strict) throw new FormatException($"Failed to format generic value {value}");
            return value.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Attempts to format any numeric type as currency.
        /// </summary>
#if NET7_0_OR_GREATER
        public static bool TryToHumanCurrency<T>(
            this T value,
            out string result,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : INumber<T>
#else
        public static bool TryToHumanCurrency<T>(
            this T value,
            out string result,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct
#endif
        {
            try
            {
                var decimalValue = Convert.ToDecimal(value);
                return TryToHumanCurrency(decimalValue, out result, decimalPlaces, culture);
            }
            catch (Exception ex)
            {
                HumanNumbersConfig.Instance.GlobalOptions.OnFormattingError?.Invoke(ex);
                result = value.ToString() ?? string.Empty;
                return false;
            }
        }

        /// <summary>
        /// Formats a number as currency in short form.
        /// Generic version for any numeric type.
        /// </summary>
#if NET7_0_OR_GREATER
        public static string ToHumanCurrency<T>(
            this T value,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : INumber<T>
#else
        public static string ToHumanCurrency<T>(
            this T value,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct
#endif
        {
            if (TryToHumanCurrency(value, out var result, decimalPlaces, culture)) return result;
            if (HumanNumbersConfig.Instance.GlobalOptions.ErrorMode == HumanNumbersErrorMode.Strict) throw new FormatException($"Failed to format generic currency value {value}");
            return value.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Attempts to format any numeric type as currency with explicit currency code.
        /// </summary>
#if NET7_0_OR_GREATER
        public static bool TryToHumanCurrency<T>(
            this T value,
            string currencyCode,
            out string result,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : INumber<T>
#else
        public static bool TryToHumanCurrency<T>(
            this T value,
            string currencyCode,
            out string result,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct
#endif
        {
            try
            {
                var decimalValue = Convert.ToDecimal(value);
                return TryToHumanCurrency(decimalValue, currencyCode, out result, decimalPlaces, culture);
            }
            catch (Exception ex)
            {
                HumanNumbersConfig.Instance.GlobalOptions.OnFormattingError?.Invoke(ex);
                result = value.ToString() ?? string.Empty;
                return false;
            }
        }

        /// <summary>
        /// Formats a number as currency with explicit currency code.
        /// Generic version for any numeric type.
        /// </summary>
#if NET7_0_OR_GREATER
        public static string ToHumanCurrency<T>(
            this T value,
            string currencyCode,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : INumber<T>
#else
        public static string ToHumanCurrency<T>(
            this T value,
            string currencyCode,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct
#endif
        {
            if (TryToHumanCurrency(value, currencyCode, out var result, decimalPlaces, culture)) return result;
            if (HumanNumbersConfig.Instance.GlobalOptions.ErrorMode == HumanNumbersErrorMode.Strict) throw new FormatException($"Failed to format generic currency value {value}");
            return value.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Generic version for any nullable numeric type. Returns string.Empty if null.
        /// </summary>
#if NET7_0_OR_GREATER
        public static string ToHumanCurrency<T>(
            this T? value,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct, INumber<T>
#else
        public static string ToHumanCurrency<T>(
            this T? value,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct
#endif
        {
            return value.HasValue ? ToHumanCurrency(value.Value, decimalPlaces, culture) : string.Empty;
        }

        /// <summary>
        /// Generic version for any nullable numeric type and currency code. Returns string.Empty if null.
        /// </summary>
#if NET7_0_OR_GREATER
        public static string ToHumanCurrency<T>(
            this T? value,
            string currencyCode,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct, INumber<T>
#else
        public static string ToHumanCurrency<T>(
            this T? value,
            string currencyCode,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct
#endif
        {
            return value.HasValue ? ToHumanCurrency(value.Value, currencyCode, decimalPlaces, culture) : string.Empty;
        }
        /// <summary>
        /// Formats a nullable decimal to a human-readable string. Returns string.Empty if null.
        /// </summary>
        public static string ToHuman(
            this decimal? value,
            int? decimalPlaces = null,
            CultureInfo? culture = null)
        {
            return value.HasValue ? ToHuman(value.Value, decimalPlaces, culture) : string.Empty;
        }

        /// <summary>
        /// Formats a nullable number to a human-readable string with custom options. Returns string.Empty if null.
        /// </summary>
        public static string ToHuman(
            this decimal? value,
            HumanNumberFormatOptions options,
            CultureInfo? culture = null)
        {
            return value.HasValue ? ToHuman(value.Value, options, culture) : string.Empty;
        }

        /// <summary>
        /// Formats a nullable number as currency in short form. Returns string.Empty if null.
        /// </summary>
        public static string ToHumanCurrency(
            this decimal? value,
            int? decimalPlaces = null,
            CultureInfo? culture = null)
        {
            return value.HasValue ? ToHumanCurrency(value.Value, decimalPlaces, culture) : string.Empty;
        }

        /// <summary>
        /// Formats a nullable number as currency with explicit currency code. Returns string.Empty if null.
        /// </summary>
        public static string ToHumanCurrency(
            this decimal? value,
            string currencyCode,
            int? decimalPlaces = null,
            CultureInfo? culture = null)
        {
            return value.HasValue ? ToHumanCurrency(value.Value, currencyCode, decimalPlaces, culture) : string.Empty;
        }

        /// <summary>
        /// Generic version for any nullable numeric type. Returns string.Empty if null.
        /// </summary>
#if NET7_0_OR_GREATER
        public static string ToHuman<T>(
            this T? value,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct, INumber<T>
#else
        public static string ToHuman<T>(
            this T? value,
            int? decimalPlaces = null,
            CultureInfo? culture = null) where T : struct
#endif
        {
            return value.HasValue ? ToHuman(value.Value, decimalPlaces, culture) : string.Empty;
        }

        #region Obsolete Aliases (Backward Compatibility)

        /// <summary>
        /// Obsolete. Use <see cref="ToHuman(decimal, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHuman instead. This alias will be removed in a future version.")]
        public static string ToShortString(this decimal value, int decimalPlaces = 2, CultureInfo? culture = null)
            => ToHuman(value, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="ToHuman(decimal, HumanNumberFormatOptions, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHuman instead. This alias will be removed in a future version.")]
        public static string ToShortString(this decimal value, HumanNumberFormatOptions options, CultureInfo? culture = null)
            => ToHuman(value, options, culture);

        /// <summary>
        /// Obsolete. Use <see cref="ToHumanCurrency(decimal, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHumanCurrency instead. This alias will be removed in a future version.")]
        public static string ToShortCurrencyString(this decimal value, int decimalPlaces = 2, CultureInfo? culture = null)
            => ToHumanCurrency(value, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="ToHumanCurrency(decimal, string, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHumanCurrency instead. This alias will be removed in a future version.")]
        public static string ToShortCurrencyString(this decimal value, string currencyCode, int decimalPlaces = 2, CultureInfo? culture = null)
            => ToHumanCurrency(value, currencyCode, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="ToHuman{T}(T, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHuman instead. This alias will be removed in a future version.")]
#if NET7_0_OR_GREATER
        public static string ToShortString<T>(this T value, int decimalPlaces = 2, CultureInfo? culture = null) where T : INumber<T>
#else
        public static string ToShortString<T>(this T value, int decimalPlaces = 2, CultureInfo? culture = null) where T : struct
#endif
            => ToHuman(value, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="ToHuman(decimal?, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHuman instead. This alias will be removed in a future version.")]
        public static string ToShortString(this decimal? value, int decimalPlaces = 2, CultureInfo? culture = null)
            => ToHuman(value, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="ToHuman(decimal?, HumanNumberFormatOptions, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHuman instead. This alias will be removed in a future version.")]
        public static string ToShortString(this decimal? value, HumanNumberFormatOptions options, CultureInfo? culture = null)
            => ToHuman(value, options, culture);

        /// <summary>
        /// Obsolete. Use <see cref="ToHumanCurrency(decimal?, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHumanCurrency instead. This alias will be removed in a future version.")]
        public static string ToShortCurrencyString(this decimal? value, int decimalPlaces = 2, CultureInfo? culture = null)
            => ToHumanCurrency(value, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="ToHumanCurrency(decimal?, string, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHumanCurrency instead. This alias will be removed in a future version.")]
        public static string ToShortCurrencyString(this decimal? value, string currencyCode, int decimalPlaces = 2, CultureInfo? culture = null)
            => ToHumanCurrency(value, currencyCode, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="ToHuman{T}(T?, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use ToHuman instead. This alias will be removed in a future version.")]
#if NET7_0_OR_GREATER
        public static string ToShortString<T>(this T? value, int decimalPlaces = 2, CultureInfo? culture = null) where T : struct, INumber<T>
#else
        public static string ToShortString<T>(this T? value, int decimalPlaces = 2, CultureInfo? culture = null) where T : struct
#endif
            => ToHuman(value, decimalPlaces, culture);

        #endregion

        /// <summary>
        /// Parses a formatted short numeric string (e.g., "$1.5M", "50K") back to a decimal.
        /// Uses CultureInfo.InvariantCulture by default.
        /// </summary>
        public static bool TryParse(string? value, out decimal result)
        {
            return TryParse(value, CultureInfo.CurrentCulture, out result);
        }

        /// <summary>
        /// Parses a formatted short numeric string (e.g., "$1.5M", "50K") back to a decimal using the specified culture.
        /// </summary>
        public static bool TryParse(string? value, CultureInfo? culture, out decimal result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            culture ??= CultureInfo.InvariantCulture;
            var nfi = culture.NumberFormat;
            var decimalSeparator = nfi.NumberDecimalSeparator[0];
            var groupSeparator = nfi.NumberGroupSeparator[0];

            var span = value.AsSpan().Trim();

            // Strip non-numeric prefixes (like currency symbols)
            while (span.Length > 0 && !char.IsDigit(span[0]) && span[0] != '+' && span[0] != '-' && span[0] != decimalSeparator)
            {
                span = span.Slice(1).TrimStart();
            }

            // Find where the number ends
            int numEnd = 0;
            while (numEnd < span.Length && (char.IsDigit(span[numEnd]) || span[numEnd] == decimalSeparator || span[numEnd] == groupSeparator || span[numEnd] == '+' || span[numEnd] == '-'))
            {
                numEnd++;
            }

            var numberPart = span.Slice(0, numEnd);
            var suffixPart = span.Slice(numEnd).Trim();

            // Strip trailing non-numeric/non-letter characters (like trailing currency symbols)
            int suffixEnd = suffixPart.Length;
            while (suffixEnd > 0 && !char.IsLetter(suffixPart[suffixEnd - 1]))
            {
                suffixEnd--;
            }
            suffixPart = suffixPart.Slice(0, suffixEnd);

#if !NETSTANDARD2_0
            if (decimal.TryParse(numberPart, NumberStyles.Any, culture, out var number))
#else
            if (decimal.TryParse(numberPart.ToString(), NumberStyles.Any, culture, out var number))
#endif
            {
                if (suffixPart.Length > 0)
                {
                    if (TryGetMultiplier(suffixPart, out var multiplier))
                    {
                        result = number * multiplier;
                        return true;
                    }
                }
                else
                {
                    result = number;
                    return true;
                }
            }

            // Fallback: Try parsing the whole string natively
            return decimal.TryParse(value, NumberStyles.Any, culture, out result);
        }

        private static bool TryGetMultiplier(ReadOnlySpan<char> suffix, out decimal multiplier)
        {
            multiplier = 1;
            if (suffix.Length == 0) return true;

            // Common short suffixes
            if (suffix.Length == 1)
            {
                char c = char.ToUpperInvariant(suffix[0]);
                switch (c)
                {
                    case 'K': multiplier = 1_000m; return true;
                    case 'M': multiplier = 1_000_000m; return true;
                    case 'B': multiplier = 1_000_000_000m; return true;
                    case 'T': multiplier = 1_000_000_000_000m; return true;
                    case 'P': multiplier = 1_000_000_000_000_000m; return true;
                    case 'E': multiplier = 1_000_000_000_000_000_000m; return true;
                }
            }

            // Fallback to dictionary for custom/longer suffixes
            return SuffixMultipliers.TryGetValue(suffix.ToString(), out multiplier);
        }

        /// <summary>
        /// Parses a formatted short numeric string, throwing an exception if invalid.
        /// Uses CultureInfo.InvariantCulture by default.
        /// </summary>
        public static decimal Parse(string? value)
        {
            return Parse(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parses a formatted short numeric string using the specified culture, throwing an exception if invalid.
        /// </summary>
        public static decimal Parse(string? value, CultureInfo? culture)
        {
            if (TryParse(value, culture, out var result))
                return result;
            throw new FormatException($"Input string '{value}' was not in a correct format.");
        }

        #endregion

        #region Core Formatting Logic

        /// <summary>
        /// Core formatting logic for short number conversion inside a Try pattern.
        /// </summary>
        internal static bool TryFormatNumber(
            decimal value,
            HumanNumberFormatOptions options,
            CultureInfo? culture,
            out string result)
        {
#if !NETSTANDARD2_0
            // Optimized path for modern .NET using stack-allocated buffer
            Span<char> buffer = stackalloc char[128];
            if (TryFormatNumber(value, options, culture, buffer, out var charsWritten))
            {
                result = new string(buffer.Slice(0, charsWritten));
                return true;
            }
            
            options.OnFormattingError?.Invoke(new FormatException("Failed to format into buffer."));
            result = value.ToString(culture ?? CultureInfo.InvariantCulture);
            return false;
#else
            // Legacy path for .NET Standard 2.0
            try
            {
                culture ??= options.Culture ?? CultureInfo.CurrentCulture;
                var numberFormat = GetNumberFormatInfo(culture);

                if (value == 0 && !options.AlwaysShowSuffix)
                {
                    result = options.CurrencySymbol != null
                        ? FormatCurrencyNumber(0, "", options, numberFormat)
                        : "0";
                    return true;
                }

                var isNegative = value < 0;
                var absValue = Math.Abs(value);
                var suffixes = options.CachedCustomSuffixes ?? DefaultSuffixes;
                var (divisor, suffix) = GetSuffixAndDivisor(absValue, suffixes, options.Threshold, options.PromotionThreshold, options.AlwaysShowSuffix);

                var scaledValue = absValue / divisor;
                var roundedValue = Math.Round(scaledValue, options.DecimalPlaces, MidpointRounding.AwayFromZero);

                while (divisor > 0)
                {
                    var reconstructedValue = roundedValue * divisor;
                    var (newDivisor, newSuffix) = GetSuffixAndDivisor(reconstructedValue, suffixes, options.Threshold, 1.0m, options.AlwaysShowSuffix);
                    if (newDivisor <= divisor) break;
                    divisor = newDivisor;
                    suffix = newSuffix;
                    scaledValue = absValue / divisor;
                    roundedValue = Math.Round(scaledValue, options.DecimalPlaces, MidpointRounding.AwayFromZero);
                }

                var precision = (options.SuppressDefaultDecimals && divisor == 1m && string.IsNullOrEmpty(suffix) && roundedValue == Math.Truncate(roundedValue))
                    ? 0
                    : options.DecimalPlaces;

                var formattedNumber = roundedValue.ToString($"N{precision}", numberFormat);

                if (options.CurrencySymbol != null)
                {
                    result = FormatCurrencyNumber(roundedValue, suffix, options, numberFormat);
                }
                else
                {
                    result = options.ShowPlusSign && !isNegative && value > 0
                        ? $"+{formattedNumber}{suffix}"
                        : $"{formattedNumber}{suffix}";
                }

                if (isNegative) 
                    result = FormatNegativeNumber(result, options, numberFormat);

                return true;
            }
            catch (Exception ex)
            {
                options.OnFormattingError?.Invoke(ex);
                result = value.ToString(CultureInfo.InvariantCulture);
                return false;
            }
#endif
        }

#if !NETSTANDARD2_0
        /// <summary>
        /// Zero-allocation core formatting logic writing directly to a span.
        /// </summary>
        internal static bool TryFormatNumber(
            decimal value,
            HumanNumberFormatOptions options,
            CultureInfo? culture,
            Span<char> destination,
            out int charsWritten)
        {
            charsWritten = 0;
            try
            {
                culture ??= options.Culture ?? CultureInfo.CurrentCulture;
                var numberFormat = GetNumberFormatInfo(culture);

                if (value == 0 && !options.AlwaysShowSuffix)
                {
                    if (options.CurrencySymbol != null)
                    {
                        return TryFormatCurrency(0, ReadOnlySpan<char>.Empty, options, numberFormat, destination, out charsWritten);
                    }
                    if (destination.Length > 0)
                    {
                        destination[0] = '0';
                        charsWritten = 1;
                        return true;
                    }
                    return false;
                }

                var isNegative = value < 0;
                var absValue = Math.Abs(value);
                var suffixes = options.CachedCustomSuffixes ?? DefaultSuffixes;

                var (divisor, suffix) = GetSuffixAndDivisor(absValue, suffixes, options.Threshold, options.PromotionThreshold, options.AlwaysShowSuffix);
                var scaledValue = absValue / divisor;
                var roundedValue = Math.Round(scaledValue, options.DecimalPlaces, MidpointRounding.AwayFromZero);

                // Magnitude promotion check
                while (divisor > 0)
                {
                    var reconstructedValue = roundedValue * divisor;
                    var (newDivisor, newSuffix) = GetSuffixAndDivisor(reconstructedValue, suffixes, options.Threshold, 1.0m, options.AlwaysShowSuffix);
                    if (newDivisor <= divisor) break;
                    divisor = newDivisor;
                    suffix = newSuffix;
                    scaledValue = absValue / divisor;
                    roundedValue = Math.Round(scaledValue, options.DecimalPlaces, MidpointRounding.AwayFromZero);
                }

                var precision = (options.SuppressDefaultDecimals && divisor == 1m && string.IsNullOrEmpty(suffix) && roundedValue == Math.Truncate(roundedValue))
                    ? 0
                    : options.DecimalPlaces;

                // Build parts without allocations
                if (options.CurrencySymbol != null)
                {
                    return TryFormatCurrency(roundedValue, suffix.AsSpan(), options, numberFormat, destination, out charsWritten, isNegative);
                }
                else
                {
                    return TryFormatGeneric(roundedValue, precision, suffix.AsSpan(), options, numberFormat, destination, out charsWritten, isNegative);
                }
            }
            catch (Exception ex)
            {
                options.OnFormattingError?.Invoke(ex);
                return value.TryFormat(destination, out charsWritten, default, CultureInfo.InvariantCulture);
            }
        }

        private static bool TryFormatGeneric(
            decimal roundedValue,
            int precision,
            ReadOnlySpan<char> suffix,
            HumanNumberFormatOptions options,
            NumberFormatInfo numberFormat,
            Span<char> destination,
            out int charsWritten,
            bool isNegative)
        {
            charsWritten = 0;
            var pos = 0;

            // 1. Sign
            if (isNegative)
            {
                if (options.NegativePattern == "-n")
                {
                    if (pos >= destination.Length) return false;
                    destination[pos++] = '-';
                }
                else if (options.NegativePattern == "(n)")
                {
                    if (pos >= destination.Length) return false;
                    destination[pos++] = '(';
                }
            }
            else if (options.ShowPlusSign && roundedValue > 0)
            {
                if (pos >= destination.Length) return false;
                destination[pos++] = '+';
            }

            // 2. Number
            Span<char> format = stackalloc char[8];
            "N".AsSpan().CopyTo(format);
            precision.TryFormat(format.Slice(1), out var fLen);
            var formatSpan = format.Slice(0, 1 + fLen);

            if (!roundedValue.TryFormat(destination.Slice(pos), out var numWritten, formatSpan, numberFormat))
                return false;
            
            pos += numWritten;

            // 3. Suffix
            if (!suffix.IsEmpty)
            {
                if (pos + suffix.Length > destination.Length) return false;
                suffix.CopyTo(destination.Slice(pos));
                pos += suffix.Length;
            }

            // 4. Closing decorators
            if (isNegative)
            {
                if (options.NegativePattern == "(n)")
                {
                    if (pos >= destination.Length) return false;
                    destination[pos++] = ')';
                }
                else if (options.NegativePattern == "n-")
                {
                    if (pos >= destination.Length) return false;
                    destination[pos++] = '-';
                }
            }

            charsWritten = pos;
            return true;
        }

        private static bool TryFormatCurrency(
            decimal roundedValue,
            ReadOnlySpan<char> suffix,
            HumanNumberFormatOptions options,
            NumberFormatInfo numberFormat,
            Span<char> destination,
            out int charsWritten,
            bool isNegative = false)
        {
            charsWritten = 0;
            var pos = 0;
            var symbol = (options.CurrencySymbol ?? numberFormat.CurrencySymbol).AsSpan();

            // Handle negative patterns for currency
            if (isNegative && options.NegativePattern == "(n)")
            {
                if (pos >= destination.Length) return false;
                destination[pos++] = '(';
            }
            else if (isNegative && options.NegativePattern == "-n")
            {
                if (pos >= destination.Length) return false;
                destination[pos++] = '-';
            }

            // Position: Before
            if (options.CurrencyPosition == CurrencyPosition.Before || options.CurrencyPosition == CurrencyPosition.BeforeWithSpace)
            {
                if (pos + symbol.Length > destination.Length) return false;
                symbol.CopyTo(destination.Slice(pos));
                pos += symbol.Length;

                if (options.CurrencyPosition == CurrencyPosition.BeforeWithSpace)
                {
                    if (pos >= destination.Length) return false;
                    destination[pos++] = ' ';
                }
            }

            // Number
            Span<char> format = stackalloc char[8];
            "N".AsSpan().CopyTo(format);
            options.DecimalPlaces.TryFormat(format.Slice(1), out var fLen);
            
            if (!roundedValue.TryFormat(destination.Slice(pos), out var numWritten, format.Slice(0, 1 + fLen), numberFormat))
                return false;
            pos += numWritten;

            // Suffix
            if (!suffix.IsEmpty)
            {
                if (pos + suffix.Length > destination.Length) return false;
                suffix.CopyTo(destination.Slice(pos));
                pos += suffix.Length;
            }

            // Position: After
            if (options.CurrencyPosition == CurrencyPosition.After || options.CurrencyPosition == CurrencyPosition.AfterWithSpace)
            {
                if (options.CurrencyPosition == CurrencyPosition.AfterWithSpace)
                {
                    if (pos >= destination.Length) return false;
                    destination[pos++] = ' ';
                }

                if (pos + symbol.Length > destination.Length) return false;
                symbol.CopyTo(destination.Slice(pos));
                pos += symbol.Length;
            }

            if (isNegative)
            {
                if (options.NegativePattern == "(n)")
                {
                    if (pos >= destination.Length) return false;
                    destination[pos++] = ')';
                }
                else if (options.NegativePattern == "n-")
                {
                    if (pos >= destination.Length) return false;
                    destination[pos++] = '-';
                }
            }

            charsWritten = pos;
            return true;
        }
#endif

        private static string FormatCurrencyNumber(
            decimal value,
            string suffix,
            HumanNumberFormatOptions options,
            NumberFormatInfo numberFormat)
        {
            var symbol = options.CurrencySymbol ?? numberFormat.CurrencySymbol;
            var formattedValue = value.ToString($"F{options.DecimalPlaces}", numberFormat);
            var numberWithSuffix = $"{formattedValue}{suffix}";

            return options.CurrencyPosition switch
            {
                CurrencyPosition.Before => $"{symbol}{numberWithSuffix}",
                CurrencyPosition.After => $"{numberWithSuffix}{symbol}",
                CurrencyPosition.BeforeWithSpace => $"{symbol} {numberWithSuffix}",
                CurrencyPosition.AfterWithSpace => $"{numberWithSuffix} {symbol}",
                _ => $"{symbol}{numberWithSuffix}"
            };
        }

        private static string FormatNegativeNumber(
            string formattedNumber,
            HumanNumberFormatOptions options,
            NumberFormatInfo numberFormat)
        {
            return options.NegativePattern switch
            {
                "(n)" => $"({formattedNumber})",
                "n-" => $"{formattedNumber}-",
                _ => $"-{formattedNumber}" // "-n" is default
            };
        }

        private static (decimal Divisor, string Suffix) GetSuffixAndDivisor(
            decimal value,
            MagnitudeSuffix[] suffixes,
            decimal threshold,
            decimal promotionThreshold,
            bool alwaysShowSuffix)
        {
            if (value < threshold && !alwaysShowSuffix)
                return (1m, "");

            foreach (var current in suffixes)
            {
                // If alwaysShowSuffix is true, we skip the "no-suffix" entry (empty string)
                // to force selection of a higher magnitude suffix.
                if (string.IsNullOrEmpty(current.Suffix) && alwaysShowSuffix)
                    continue;

                if (value >= current.Threshold * promotionThreshold)
                    return (current.Threshold, current.Suffix);
            }

            if (alwaysShowSuffix && suffixes.Length > 1)
            {
                // Return the smallest non-empty suffix (usually "K")
                // Suffixes are ordered largest to smallest, so it's the one before the empty one.
                var smallestSuffix = suffixes[suffixes.Length - 2];
                return (smallestSuffix.Threshold, smallestSuffix.Suffix);
            }

            return (1m, "");
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Creates a collection of magnitude suffixes from an ordered array of strings.
        /// Useful for building custom scaling rules (e.g., 10k-based systems).
        /// </summary>
        public static MagnitudeSuffix[] CreateCustomSuffixes(string[] suffixes)
        {
            var result = new List<MagnitudeSuffix>();
            var multiplier = 1m;

            for (int i = suffixes.Length - 1; i >= 0; i--)
            {
                if (i > 0)
                {
                    multiplier *= 1000;
                }
                result.Add(new MagnitudeSuffix(multiplier, suffixes[i]));
            }

            return result.OrderByDescending(x => x.Threshold).ToArray();
        }

        private static NumberFormatInfo GetNumberFormatInfo(CultureInfo culture)
        {
            var key = culture.Name;
            return FormatCache.GetOrAdd(key, _ => culture.NumberFormat);
        }

        private static string GetCurrencySymbol(string currencyCode)
        {
            return CurrencyRegistry.GetSymbol(currencyCode);
        }

        #endregion
    }
}

namespace NumberFormatter
{
    using System;
    using System.Globalization;
    using HumanNumbers.Formatting;

    /// <summary>
    /// Obsolete alias for <see cref="HumanNumbers.HumanNumber"/>.
    /// </summary>
    [Obsolete("Use HumanNumbers.HumanNumber instead. This alias will be removed in a future version.")]
    public static class NumberFormatter
    {
        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.HumanNumber.ToHuman(decimal, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.HumanNumber.ToHuman instead.")]
        public static string ToShortString(this decimal value, int decimalPlaces = 2, CultureInfo? culture = null)
            => HumanNumbers.HumanNumber.ToHuman(value, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.HumanNumber.ToHuman(decimal, HumanNumbers.Formatting.HumanNumberFormatOptions, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.HumanNumber.ToHuman instead.")]
        public static string ToShortString(this decimal value, ShortNumberFormatOptions options, CultureInfo? culture = null)
            => HumanNumbers.HumanNumber.ToHuman(value, options, culture);

        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.HumanNumber.ToHuman{T}(T, int?, System.Globalization.CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.HumanNumber.ToHuman instead.")]
#if NET7_0_OR_GREATER
        public static string ToShortString<T>(this T value, int decimalPlaces = 2, CultureInfo? culture = null) where T : INumber<T>
#else
        public static string ToShortString<T>(this T value, int decimalPlaces = 2, CultureInfo? culture = null) where T : struct
#endif
            => HumanNumbers.HumanNumber.ToHuman(value, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.HumanNumber.ToHumanCurrency(decimal, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.HumanNumber.ToHumanCurrency instead.")]
        public static string ToShortCurrencyString(this decimal value, int decimalPlaces = 2, CultureInfo? culture = null)
            => HumanNumbers.HumanNumber.ToHumanCurrency(value, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.HumanNumber.ToHumanCurrency(decimal, string, int?, CultureInfo?)"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.HumanNumber.ToHumanCurrency instead.")]
        public static string ToShortCurrencyString(this decimal value, string currencyCode, int decimalPlaces = 2, CultureInfo? culture = null)
            => HumanNumbers.HumanNumber.ToHumanCurrency(value, currencyCode, decimalPlaces, culture);

        /// <summary>
        /// Obsolete. Use <see cref="HumanNumbers.HumanNumber.TryParse(string?, out decimal)"/> instead.
        /// </summary>
        [Obsolete("Use HumanNumbers.HumanNumber.TryParse instead.")]
        public static bool TryParse(string? value, out decimal result)
            => HumanNumbers.HumanNumber.TryParse(value, out result);
    }
}