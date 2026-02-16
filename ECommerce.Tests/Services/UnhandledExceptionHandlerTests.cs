using ECommerce.Services.Implementations;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text.Json;

namespace ECommerce.Tests.Services;

public class UnhandledExceptionHandlerTests
{
    private readonly ILogger<UnhandledExceptionHandler> _logger = Substitute.For<ILogger<UnhandledExceptionHandler>>();
    private readonly UnhandledExceptionHandler _handler;

    public UnhandledExceptionHandlerTests()
    {
        _handler = new UnhandledExceptionHandler(_logger);
    }

    private static HttpContext CreateHttpContext(string method = "POST", string path = "/test")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<ProblemDetails?> ReadProblemDetails(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        return await JsonSerializer.DeserializeAsync<ProblemDetails>(response.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // #122
    [Fact]
    public async Task TryHandle_AnyException_ReturnsTrue()
    {
        var context = CreateHttpContext();

        var result = await _handler.TryHandleAsync(context, new InvalidOperationException("boom"), CancellationToken.None);

        result.Should().BeTrue();
    }

    // #123
    [Fact]
    public async Task TryHandle_SetsStatusCodeTo500()
    {
        var context = CreateHttpContext();

        await _handler.TryHandleAsync(context, new Exception("fail"), CancellationToken.None);

        context.Response.StatusCode.Should().Be(500);
    }

    // #124
    [Fact]
    public async Task TryHandle_ResponseBodyContainsGenericMessage()
    {
        var context = CreateHttpContext();

        await _handler.TryHandleAsync(context, new Exception("secret internal error"), CancellationToken.None);

        var body = await ReadProblemDetails(context.Response);
        body.Should().NotBeNull();
        body!.Detail.Should().Be("An unexpected error occurred. Please try again later.");
    }

    // #125
    [Fact]
    public async Task TryHandle_ResponseBodyDoesNotContainExceptionMessage()
    {
        var context = CreateHttpContext();
        var secretMessage = "NullRef at StockReservationService line 42";

        await _handler.TryHandleAsync(context, new NullReferenceException(secretMessage), CancellationToken.None);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var rawBody = await reader.ReadToEndAsync();

        rawBody.Should().NotContain(secretMessage);
    }

    // #126
    [Fact]
    public async Task TryHandle_ResponseBodyDoesNotContainStackTrace()
    {
        var context = CreateHttpContext();
        Exception captured;
        try { throw new InvalidOperationException("boom"); }
        catch (Exception ex) { captured = ex; }

        await _handler.TryHandleAsync(context, captured, CancellationToken.None);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var rawBody = await reader.ReadToEndAsync();

        rawBody.Should().NotContain("at ECommerce");
        rawBody.Should().NotContain("StackTrace");
    }

    // #127
    [Fact]
    public async Task TryHandle_LogsErrorWithException()
    {
        var context = CreateHttpContext("GET", "/buyer/cart");
        var exception = new InvalidOperationException("something broke");

        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
