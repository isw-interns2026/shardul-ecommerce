using ECommerce.Models.Domain.Entities;
using ECommerce.Models.Domain.Exceptions;
using ECommerce.Services.Implementations;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ECommerce.Tests.Services;

public class DomainExceptionHandlerTests
{
    private readonly DomainExceptionHandler _handler = new();

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<ProblemDetails?> ReadProblemDetails(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        return await JsonSerializer.DeserializeAsync<ProblemDetails>(response.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // #116 — B1: non-domain exception → false
    [Fact]
    public async Task TryHandle_NullReferenceException_ReturnsFalse()
    {
        var context = CreateHttpContext();

        var result = await _handler.TryHandleAsync(context, new NullReferenceException(), CancellationToken.None);

        result.Should().BeFalse();
    }

    // #117 — B1: non-domain exception → false
    [Fact]
    public async Task TryHandle_InvalidOperationException_ReturnsFalse()
    {
        var context = CreateHttpContext();

        var result = await _handler.TryHandleAsync(context, new InvalidOperationException(), CancellationToken.None);

        result.Should().BeFalse();
    }

    // #118 — B2: DuplicateEmailException (409)
    [Fact]
    public async Task TryHandle_DuplicateEmailException_ReturnsTrue_With409()
    {
        var context = CreateHttpContext();

        var result = await _handler.TryHandleAsync(context, new DuplicateEmailException(), CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(409);
    }

    // #119 — B2: InsufficientStockException (422)
    [Fact]
    public async Task TryHandle_InsufficientStockException_ReturnsTrue_With422()
    {
        var context = CreateHttpContext();
        var product = new Product { Name = "Widget", CountInStock = 5, ReservedCount = 5 };

        var result = await _handler.TryHandleAsync(context, new InsufficientStockException(product), CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(422);
    }

    // #120 — B2: ProductNotFoundException (404)
    [Fact]
    public async Task TryHandle_ProductNotFoundException_ReturnsTrue_With404()
    {
        var context = CreateHttpContext();

        var result = await _handler.TryHandleAsync(context, new ProductNotFoundException(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(404);
    }

    // #121 — B2: verify ProblemDetails body
    [Fact]
    public async Task TryHandle_DomainException_WritesProblemDetailsWithTitleAndDetail()
    {
        var context = CreateHttpContext();
        var exception = new DuplicateEmailException();

        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        var body = await ReadProblemDetails(context.Response);
        body.Should().NotBeNull();
        body!.Title.Should().Be(nameof(DuplicateEmailException));
        body.Detail.Should().Be(exception.Message);
        body.Status.Should().Be(409);
    }
}
