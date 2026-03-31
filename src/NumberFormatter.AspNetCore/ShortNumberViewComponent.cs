using Microsoft.AspNetCore.Mvc;

namespace NumberFormatter.AspNetCore;

/// <summary>
/// View component for short number formatting
/// </summary>
public class ShortNumberViewComponent : ViewComponent
{
    /// <summary>
    /// Invokes the view component to format a number.
    /// </summary>
    /// <param name="value">The numeric value to format.</param>
    /// <param name="decimalPlaces">Number of decimal places to include.</param>
    /// <param name="isCurrency">Whether the value is a currency amount.</param>
    /// <param name="currencyCode">Optional ISO currency code to use.</param>
    /// <returns>The formatted string as a view component result.</returns>
    public IViewComponentResult Invoke(decimal value, int decimalPlaces = 2, bool isCurrency = false, string? currencyCode = null)
    {
        var formatted = isCurrency
            ? currencyCode != null
                ? value.ToShortCurrencyString(currencyCode, decimalPlaces)
                : value.ToShortCurrencyString(decimalPlaces)
            : value.ToShortString(decimalPlaces);

        return Content(formatted);
    }
}