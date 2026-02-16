using ECommerce.Services.Implementations;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using NSubstitute;

namespace ECommerce.Tests.Filters;

public class FluentValidationFilterTests
{
    private readonly FluentValidationFilter _filter = new();

    private async Task<(bool nextCalled, ActionExecutingContext context)> ExecuteFilter(
        Dictionary<string, object?> arguments,
        IServiceProvider? serviceProvider = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = serviceProvider ?? Substitute.For<IServiceProvider>();

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            arguments,
            controller: null!);

        var nextCalled = false;
        var executedContext = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), controller: null!);

        Task<ActionExecutedContext> Next()
        {
            nextCalled = true;
            return Task.FromResult(executedContext);
        }

        await _filter.OnActionExecutionAsync(context, Next);

        return (nextCalled, context);
    }

    private static IServiceProvider CreateServiceProviderWithValidator<T>(IValidator<T> validator)
    {
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(IValidator<T>)).Returns(validator);
        return sp;
    }

    // #128 — B1→B5: null argument skipped, next() called
    [Fact]
    public async Task OnActionExecution_NullArgument_SkipsAndCallsNext()
    {
        var (nextCalled, context) = await ExecuteFilter(
            new Dictionary<string, object?> { ["dto"] = null });

        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    // #129 — B2→B5: no validator registered, next() called
    [Fact]
    public async Task OnActionExecution_NoValidatorRegistered_CallsNext()
    {
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(Arg.Any<Type>()).Returns((object?)null);

        var (nextCalled, context) = await ExecuteFilter(
            new Dictionary<string, object?> { ["dto"] = new object() },
            sp);

        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    // #130 — B4→B5: validation passes, next() called
    [Fact]
    public async Task OnActionExecution_ValidationPasses_CallsNext()
    {
        var validator = Substitute.For<IValidator<TestDto>>();
        validator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var sp = CreateServiceProviderWithValidator(validator);

        var (nextCalled, context) = await ExecuteFilter(
            new Dictionary<string, object?> { ["dto"] = new TestDto() },
            sp);

        nextCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    // #131 — B3: validation fails, short-circuits with 400
    [Fact]
    public async Task OnActionExecution_ValidationFails_ShortCircuitsWith400()
    {
        var validator = Substitute.For<IValidator<TestDto>>();
        validator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") }));

        var sp = CreateServiceProviderWithValidator(validator);

        var (nextCalled, context) = await ExecuteFilter(
            new Dictionary<string, object?> { ["dto"] = new TestDto() },
            sp);

        nextCalled.Should().BeFalse();
        context.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)context.Result!;
        badRequest.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    // #132 — B5: empty arguments, next() called directly
    [Fact]
    public async Task OnActionExecution_EmptyArguments_CallsNext()
    {
        var (nextCalled, _) = await ExecuteFilter(new Dictionary<string, object?>());

        nextCalled.Should().BeTrue();
    }

    // #133 — B4 then B3: first valid, second invalid → short-circuits on second
    [Fact]
    public async Task OnActionExecution_FirstValidSecondInvalid_ShortCircuitsOnSecond()
    {
        var validValidator = Substitute.For<IValidator<TestDto>>();
        validValidator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var invalidValidator = Substitute.For<IValidator<TestDto2>>();
        invalidValidator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Field", "Bad") }));

        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(IValidator<TestDto>)).Returns(validValidator);
        sp.GetService(typeof(IValidator<TestDto2>)).Returns(invalidValidator);

        var (nextCalled, context) = await ExecuteFilter(
            new Dictionary<string, object?>
            {
                ["first"] = new TestDto(),
                ["second"] = new TestDto2()
            },
            sp);

        nextCalled.Should().BeFalse();
        context.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // #134 — B4+B4→B5: multiple valid args, next() called
    [Fact]
    public async Task OnActionExecution_MultipleValidArgs_CallsNext()
    {
        var validator1 = Substitute.For<IValidator<TestDto>>();
        validator1.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var validator2 = Substitute.For<IValidator<TestDto2>>();
        validator2.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(IValidator<TestDto>)).Returns(validator1);
        sp.GetService(typeof(IValidator<TestDto2>)).Returns(validator2);

        var (nextCalled, _) = await ExecuteFilter(
            new Dictionary<string, object?>
            {
                ["first"] = new TestDto(),
                ["second"] = new TestDto2()
            },
            sp);

        nextCalled.Should().BeTrue();
    }

    // ── Test DTOs for the filter tests ───────────────────────
    public class TestDto { }
    public class TestDto2 { }
}
