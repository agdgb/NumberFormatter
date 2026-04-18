using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace HumanNumbers.AspNetCore;

/// <summary>
/// Resolves the best culture for formatting based on request context and configuration.
/// </summary>
public static class CultureResolver
{
    private const string ContextItemKey = "HumanNumbers_Culture";

    /// <summary>
    /// Resolves the culture for the current request.
    /// Priority: 1. Explicit override in HttpContext.Items 2. RequestLocalization 3. Accept-Language 4. Thread default.
    /// </summary>
    public static CultureInfo Resolve(HttpContext context)
    {
        // 1. Explicit override in HttpContext.Items
        if (context.Items.TryGetValue(ContextItemKey, out var item) && item is CultureInfo explicitCulture)
        {
            return explicitCulture;
        }

        // 2. RequestLocalization Middleware
        var requestCultureFeature = context.Features.Get<IRequestCultureFeature>();
        if (requestCultureFeature != null)
        {
            return requestCultureFeature.RequestCulture.Culture;
        }

        // 3. Thread default / system default
        return CultureInfo.CurrentCulture;
    }

    /// <summary>
    /// Sets an explicit culture override for the current request.
    /// </summary>
    public static void SetCulture(HttpContext context, CultureInfo culture)
    {
        context.Items[ContextItemKey] = culture;
    }
}
