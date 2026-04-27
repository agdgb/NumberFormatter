using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;
using HumanNumbers;

namespace HumanNumbers.AspNetCore.TagHelpers;

/// <summary>
/// Tag helper for formatting currency values in Razor views as human-readable strings.
/// </summary>
[HtmlTargetElement("hn-currency")]
public class HumanCurrencyTagHelper : TagHelper
{
    private readonly HumanNumbersOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="HumanCurrencyTagHelper"/> class.
    /// </summary>
    public HumanCurrencyTagHelper(HumanNumbersOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// The numeric value to format.
    /// </summary>
    [HtmlAttributeName("value")]
    public decimal Value { get; set; }

    /// <summary>
    /// The ISO currency code (e.g., "USD", "EUR"). If null, uses culture default.
    /// </summary>
    [HtmlAttributeName("currency-code")]
    public string? CurrencyCode { get; set; }

    /// <summary>
    /// The number of decimal places to include.
    /// </summary>
    [HtmlAttributeName("decimal-places")]
    public int? DecimalPlaces { get; set; }

    /// <summary>
    /// Optional CSS classes to apply to the rendered span.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";

        if (!string.IsNullOrEmpty(CssClass))
            output.Attributes.SetAttribute("class", CssClass);

        var formatted = !string.IsNullOrEmpty(CurrencyCode)
            ? Value.ToHumanCurrency(CurrencyCode, DecimalPlaces ?? _options.DefaultDecimalPlaces)
            : Value.ToHumanCurrency(DecimalPlaces ?? _options.DefaultDecimalPlaces);

        output.Content.SetContent(formatted);
    }
}
