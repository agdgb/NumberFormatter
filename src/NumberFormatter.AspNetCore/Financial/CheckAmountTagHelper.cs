using Microsoft.AspNetCore.Razor.TagHelpers;
using NumberFormatter.Financial;

namespace NumberFormatter.AspNetCore.Financial;

/// <summary>
/// A tag helper that formats a decimal value into its spelled-out, check-friendly representation.
/// </summary>
[HtmlTargetElement("check-amount")]
public class CheckAmountTagHelper : TagHelper
{
    [HtmlAttributeName("value")]
    public decimal Value { get; set; }

    [HtmlAttributeName("major-currency")]
    public string MajorCurrency { get; set; } = "Dollars";

    [HtmlAttributeName("major-currency-singular")]
    public string MajorCurrencySingular { get; set; } = "Dollar";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span"; // Render as a span by default
        
        var words = Value.ToCheckWords(MajorCurrency, MajorCurrencySingular);
        
        output.Content.SetContent(words);
    }
}
