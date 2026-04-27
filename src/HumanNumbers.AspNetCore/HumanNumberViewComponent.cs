using Microsoft.AspNetCore.Mvc;
using HumanNumbers;

namespace HumanNumbers.AspNetCore;

/// <summary>
/// View component for formatting numbers as human-readable strings.
/// </summary>
public class HumanNumberViewComponent : ViewComponent
{
    private readonly HumanNumbersOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="HumanNumberViewComponent"/> class.
    /// </summary>
    public HumanNumberViewComponent(HumanNumbersOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Invokes the view component to format a number.
    /// </summary>
    /// <param name="value">The numeric value to format.</param>
    /// <param name="decimalPlaces">Number of decimal places to include.</param>
    /// <param name="isCurrency">Whether the value is a currency amount.</param>
    /// <param name="currencyCode">Optional ISO currency code to use.</param>
    /// <returns>The formatted string as a view component result.</returns>
    public IViewComponentResult Invoke(decimal value, int? decimalPlaces = null, bool isCurrency = false, string? currencyCode = null)
    {
        var places = decimalPlaces ?? _options.DefaultDecimalPlaces;
        
        var formatted = isCurrency
            ? currencyCode != null
                ? value.ToHumanCurrency(currencyCode, places)
                : value.ToHumanCurrency(places)
            : value.ToHuman(places);

        return Content(formatted);
    }
}