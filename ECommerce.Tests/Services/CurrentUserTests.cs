using ECommerce.Services.Implementations;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Security.Claims;

namespace ECommerce.Tests.Services;

public class CurrentUserTests
{
    private static IHttpContextAccessor CreateAccessor(ClaimsPrincipal? user = null, bool nullHttpContext = false)
    {
        var accessor = Substitute.For<IHttpContextAccessor>();

        if (nullHttpContext)
        {
            accessor.HttpContext.Returns((HttpContext?)null);
            return accessor;
        }

        var httpContext = new DefaultHttpContext();
        if (user != null)
            httpContext.User = user;

        accessor.HttpContext.Returns(httpContext);
        return accessor;
    }

    private static ClaimsPrincipal CreatePrincipal(string? userId = null, string? role = null)
    {
        var claims = new List<Claim>();
        if (userId != null)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        if (role != null)
            claims.Add(new Claim(ClaimTypes.Role, role));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    // #151 — B4: all valid
    [Fact]
    public void Constructor_ValidClaims_SetsUserIdAndRole()
    {
        var userId = Guid.NewGuid();
        var accessor = CreateAccessor(CreatePrincipal(userId.ToString(), "Buyer"));

        var currentUser = new CurrentUser(accessor);

        currentUser.UserId.Should().Be(userId);
        currentUser.Role.Should().Be("Buyer");
    }

    // #152 — B1: HttpContext null
    [Fact]
    public void Constructor_NullHttpContext_ThrowsUnauthorizedAccessException()
    {
        var accessor = CreateAccessor(nullHttpContext: true);

        var act = () => new CurrentUser(accessor);

        act.Should().Throw<UnauthorizedAccessException>();
    }

    // #153 — B1: User is null (DefaultHttpContext has a non-null User by default, 
    //        but without claims the NameIdentifier will be null — tested in #154)
    //        Test with no authenticated identity
    [Fact]
    public void Constructor_NoAuthenticatedIdentity_ThrowsOnMissingClaim()
    {
        var accessor = CreateAccessor(new ClaimsPrincipal());

        var act = () => new CurrentUser(accessor);

        // FindFirstValue returns null when no claims → Guid.Parse(null!) throws
        act.Should().Throw<ArgumentNullException>();
    }

    // #154 — B2: NameIdentifier claim missing
    [Fact]
    public void Constructor_MissingNameIdentifierClaim_Throws()
    {
        var principal = CreatePrincipal(userId: null, role: "Buyer");
        var accessor = CreateAccessor(principal);

        var act = () => new CurrentUser(accessor);

        act.Should().Throw<ArgumentNullException>();
    }

    // #155 — B2: NameIdentifier has invalid GUID
    [Fact]
    public void Constructor_InvalidGuidInNameIdentifier_ThrowsFormatException()
    {
        var principal = CreatePrincipal(userId: "not-a-guid", role: "Buyer");
        var accessor = CreateAccessor(principal);

        var act = () => new CurrentUser(accessor);

        act.Should().Throw<FormatException>();
    }
}
