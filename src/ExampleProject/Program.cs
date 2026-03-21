using Microsoft.AspNetCore.Mvc;
using NumberFormatter;
using NumberFormatter.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services with proper ASP.NET Core integration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // This will apply short number formatting to all numeric properties
        options.JsonSerializerOptions.Converters.Add(new ShortNumberJsonConverterFactory());
    });

builder.Services.AddNumberFormatter(); // Add formatter service

// Add Razor pages for tag helpers
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles(); // for eventual CSS/JS
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers(); // still map API controllers

app.Run();