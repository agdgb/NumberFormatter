using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;
using HumanNumbers;

namespace HumanNumbers.AspNetCore.TagHelpers;

/// <summary>
/// Tag helper for formatting numbers in Razor views as human-readable strings.
/// </summary>
[HtmlTargetElement("hn-number")]
public class HumanNumberTagHelper : TagHelper
{
    private readonly HumanNumbersOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="HumanNumberTagHelper"/> class.
    /// </summary>
    public HumanNumberTagHelper(HumanNumbersOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// The numeric value to format.
    /// </summary>
    [HtmlAttributeName("value")]
    public decimal Value { get; set; }

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

        var formatted = Value.ToHuman(DecimalPlaces ?? _options.DefaultDecimalPlaces);

        output.Content.SetContent(formatted);
    }
}