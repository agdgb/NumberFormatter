using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;

namespace NumberFormatter.AspNetCore;

/// <summary>
/// Tag helper for formatting numbers in Razor views
/// </summary>
[HtmlTargetElement("short-number")]
public class ShortNumberTagHelper : TagHelper
{
    /// <summary>
    /// The numeric value to format.
    /// </summary>
    [HtmlAttributeName("value")]
    public decimal Value { get; set; }

    /// <summary>
    /// The formatting style: optionally set to "currency".
    /// </summary>
    [HtmlAttributeName("format")]
    public string? Format { get; set; }

    /// <summary>
    /// The number of decimal places to include.
    /// </summary>
    [HtmlAttributeName("decimal-places")]
    public int DecimalPlaces { get; set; } = 2;

    /// <summary>
    /// The ISO currency code to prepend or append, if format is "currency".
    /// </summary>
    [HtmlAttributeName("currency-code")]
    public string? CurrencyCode { get; set; }

    /// <summary>
    /// Optional CSS classes to apply to the rendered span.
    /// </summary>
    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";

        if (!string.IsNullOrEmpty(CssClass))
            output.Attributes.SetAttribute("class", CssClass);

        var formatted = Format?.ToLower() switch
        {
            "currency" when !string.IsNullOrEmpty(CurrencyCode) =>
                Value.ToShortCurrencyString(CurrencyCode, DecimalPlaces),
            "currency" =>
                Value.ToShortCurrencyString(DecimalPlaces),
            _ =>
                Value.ToShortString(DecimalPlaces)
        };

        output.Content.SetContent(formatted);
    }
}