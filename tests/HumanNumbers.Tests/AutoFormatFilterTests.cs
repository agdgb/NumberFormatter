using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using HumanNumbers.AspNetCore;
using Xunit;
using System.Dynamic;

namespace HumanNumbers.Tests;

public class AutoFormatFilterTests
{
    [Fact]
    public async Task Filter_WithGlobalMode_FormatsAllNumericProperties()
    {
        // Arrange
        var options = new HumanNumbersOptions
        {
            EnableAutoFormatting = true,
            AutoFormatMode = AutoFormatMode.Global
        };
        var filter = new HumanNumberAutoFormatFilter(options);
        
        var model = new TestModel { Salary = 5000, Bonus = 1000, Name = "Alice" };
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        var objectResult = new ObjectResult(model);
        var executingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), null!);
        
        var executedContext = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), null!)
        {
            Result = objectResult
        };

        ActionExecutionDelegate next = () => Task.FromResult(executedContext);

        // Act
        await filter.OnActionExecutionAsync(executingContext, next);

        // Assert
        var resultValue = objectResult.Value;
        Assert.IsType<ExpandoObject>(resultValue);
        var dict = (IDictionary<string, object?>)resultValue;
        
        Assert.Equal("5.00K", dict["Salary"]);
        Assert.Equal("1.00K", dict["Bonus"]);
        Assert.Equal("Alice", dict["Name"]);
    }

    [Fact]
    public async Task Filter_WithOptInMode_OnlyFormatsAttributedProperties()
    {
        // Arrange
        var options = new HumanNumbersOptions
        {
            EnableAutoFormatting = true,
            AutoFormatMode = AutoFormatMode.OptInAttributeOnly
        };
        var filter = new HumanNumberAutoFormatFilter(options);
        
        var model = new TestModel { Salary = 5000, Bonus = 1000, Name = "Alice" };
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        var objectResult = new ObjectResult(model);
        var executedContext = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), null!) { Result = objectResult };
        ActionExecutionDelegate next = () => Task.FromResult(executedContext);

        // Act
        await filter.OnActionExecutionAsync(null!, next);

        // Assert
        var dict = (IDictionary<string, object?>)objectResult.Value!;
        Assert.Equal("5.00K", dict["Salary"]); // Has [HumanNumber]
        Assert.Equal(1000m, dict["Bonus"]);    // No attribute
    }

    [Fact]
    public async Task Filter_HandlesNestedObjectsAndCollections()
    {
        // Arrange
        var options = new HumanNumbersOptions { EnableAutoFormatting = true, AutoFormatMode = AutoFormatMode.Global };
        var filter = new HumanNumberAutoFormatFilter(options);
        
        var model = new ParentModel 
        { 
            Children = new List<TestModel> 
            { 
                new TestModel { Salary = 2000 } 
            },
            PrimaryChild = new TestModel { Salary = 3000 }
        };
        
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        var objectResult = new ObjectResult(model);
        var executedContext = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), null!) { Result = objectResult };
        ActionExecutionDelegate next = () => Task.FromResult(executedContext);

        // Act
        await filter.OnActionExecutionAsync(null!, next);

        // Assert
        var dict = (IDictionary<string, object?>)objectResult.Value!;
        
        var primaryChild = (IDictionary<string, object?>)dict["PrimaryChild"]!;
        Assert.Equal("3.00K", primaryChild["Salary"]);

        var children = (List<object?>)dict["Children"]!;
        var firstChild = (IDictionary<string, object?>)children[0]!;
        Assert.Equal("2.00K", firstChild["Salary"]);
    }

    public class TestModel
    {
        public string Name { get; set; } = "";
        
        [HumanNumber]
        public decimal Salary { get; set; }
        
        public decimal Bonus { get; set; }
    }

    public class ParentModel
    {
        public List<TestModel> Children { get; set; } = new();
        public TestModel PrimaryChild { get; set; } = null!;
    }

    [Fact]
    public async Task Filter_HandlesCircularReferences_WithoutStackOverflow()
    {
        // Arrange
        var options = new HumanNumbersOptions { EnableAutoFormatting = true, AutoFormatMode = AutoFormatMode.Global };
        var filter = new HumanNumberAutoFormatFilter(options);

        var model = new CircularModel { Salary = 5000 };
        model.Self = model; // Set up circular reference

        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        var objectResult = new ObjectResult(model);
        var executedContext = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), null!) { Result = objectResult };
        ActionExecutionDelegate next = () => Task.FromResult(executedContext);

        // Act & Assert
        // This would throw StackOverflowException if cyclic checks were absent.
        var exception = await Record.ExceptionAsync(() => filter.OnActionExecutionAsync(null!, next));
        Assert.Null(exception);

        var dict = (IDictionary<string, object?>)objectResult.Value!;
        Assert.Equal("5.00K", dict["Salary"]);
        Assert.Same(model, dict["Self"]);
    }

    [Fact]
    public void HumanNumbersOptions_SetProperties_DoesNotMutateSharedCoreOptions()
    {
        // Arrange
        var options = new HumanNumbersOptions();
        var originalCore = options.CoreOptions;

        // Act
        options.DefaultDecimalPlaces = 4;
        options.DefaultErrorMode = HumanNumbersErrorMode.Strict;

        // Assert
        Assert.NotSame(originalCore, options.CoreOptions); // Must have created a new record via with-expression
        Assert.Equal(2, originalCore.DecimalPlaces); // Original shared record remains unmutated
        Assert.Equal(HumanNumbersErrorMode.SafeFallback, originalCore.ErrorMode);

        Assert.Equal(4, options.CoreOptions.DecimalPlaces);
        Assert.Equal(HumanNumbersErrorMode.Strict, options.CoreOptions.ErrorMode);
    }

    public class CircularModel
    {
        public decimal Salary { get; set; }
        public CircularModel Self { get; set; } = null!;
    }
}
