using HumanNumbers;
using HumanNumbers.AspNetCore;
using HumanNumbers.AspNetCore.Financial;
using HumanNumbers.Currencies;
using HumanNumbers.Suffixes;
using HumanNumbers.Formatting;

var builder = WebApplication.CreateBuilder(args);

// 1. Register custom currencies globally
CurrencyRegistry.RegisterCurrency("ETB", "ብር", "ET");
CurrencyRegistry.RegisterCurrency("CNY", "¥", "CN");

// Add services with proper ASP.NET Core integration using the new unified setup
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
    });

// 2. Configure HumanNumbers with advanced policies
builder.Services.AddHumanNumbersDefaults(options =>
{
    // Chinese (10,000-based scaling / 万 system)
    options.AddPolicy("Chinese", new HumanNumberFormatOptions
    {
        CachedCustomSuffixes = new MagnitudeSuffix[]
        {
            new(100_000_000m, "亿"),
            new(10_000m, "万"),
            new(1m, "")
        },
        Threshold = 10000m,
        DecimalPlaces = 1,
        CurrencyPosition = CurrencyPosition.Before
    });

    // Amharic (1,000-based with custom suffix strings)
    options.AddPolicy("Amharic", new HumanNumberFormatOptions
    {
        CustomSuffixes = new[] { "ኩዊ", "ኳድ", "ት", "ቢ", "ሚ", "ሺ" },
        Threshold = 1000m,
        DecimalPlaces = 2,
        CurrencyPosition = CurrencyPosition.Before
    });

    // Indian (Lac/Crore scaling)
    options.AddPolicy("Indian", new HumanNumberFormatOptions
    {
        CachedCustomSuffixes = new MagnitudeSuffix[]
        {
            new(100_00_000m, "Cr"),
            new(100_000m, "Lac"),
            new(1m, "")
        },
        Threshold = 100000m,
        DecimalPlaces = 2
    });

    options.DefaultPolicyName = "Dashboard";
});

// 3. Add financial formatters (already included in Defaults, but shown for clarity)
builder.Services.AddHumanFinancialFormatters();

// Add Razor pages for tag helpers
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles(); // for eventual CSS/JS
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers(); // map API controllers

// Add a Minimal API example endpoint
app.MapGet("/api/demo/minimal", () => Results.Extensions.HumanOk(new { Revenue = 1500000m }));

app.Run();