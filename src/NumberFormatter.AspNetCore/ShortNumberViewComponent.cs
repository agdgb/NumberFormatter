using Microsoft.AspNetCore.Mvc;

namespace NumberFormatter.AspNetCore;

/// <summary>
/// View component for short number formatting
/// </summary>
public class ShortNumberViewComponent : ViewComponent
{
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