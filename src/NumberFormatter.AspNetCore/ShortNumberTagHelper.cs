using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;

namespace NumberFormatter.AspNetCore;

/// <summary>
/// Tag helper for formatting numbers in Razor views
/// </summary>
[HtmlTargetElement("short-number")]
public class ShortNumberTagHelper : TagHelper
{
    [HtmlAttributeName("value")]
    public decimal Value { get; set; }

    [HtmlAttributeName("format")]
    public string? Format { get; set; }

    [HtmlAttributeName("decimal-places")]
    public int DecimalPlaces { get; set; } = 2;

    [HtmlAttributeName("currency-code")]
    public string? CurrencyCode { get; set; }

    [HtmlAttributeName("css-class")]
    public string? CssClass { get; set; }

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