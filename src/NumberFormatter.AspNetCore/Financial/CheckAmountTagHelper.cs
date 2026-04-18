using HumanNumbers.Financial;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HumanNumbers.AspNetCore.Financial;

/// <summary>
/// A tag helper that formats a decimal value into its spelled-out, check-friendly representation.
/// </summary>
[HtmlTargetElement("hn-check")]
public class HumanCheckAmountTagHelper : TagHelper
{
    /// <summary>
    /// The decimal value to format.
    /// </summary>
    [HtmlAttributeName("value")]
    public decimal Value { get; set; }

    /// <summary>
    /// The major currency name to use (plural). Default is "Dollars".
    /// </summary>
    [HtmlAttributeName("major-currency")]
    public string MajorCurrency { get; set; } = "Dollars";

    /// <summary>
    /// The major currency name to use (singular). Default is "Dollar".
    /// </summary>
    [HtmlAttributeName("major-currency-singular")]
    public string MajorCurrencySingular { get; set; } = "Dollar";

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";

        var words = Value.ToCheckWords(MajorCurrency, MajorCurrencySingular);

        output.Content.SetContent(words);
    }
}
