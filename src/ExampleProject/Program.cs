using Microsoft.AspNetCore.Mvc;
using HumanNumbers;
using HumanNumbers.AspNetCore;
using HumanNumbers.AspNetCore.Financial;

var builder = WebApplication.CreateBuilder(args);

// Add services with proper ASP.NET Core integration using the new unified setup
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
    });

// 1-line setup for JSON, MVC extensions, DI, and Financial formatters
builder.Services.AddHumanNumbersDefaults(options => 
{
    options.DefaultPolicyName = "Dashboard";
});

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