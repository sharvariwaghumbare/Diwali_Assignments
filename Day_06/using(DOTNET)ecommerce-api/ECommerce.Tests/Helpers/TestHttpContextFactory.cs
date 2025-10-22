using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Tests.Helpers
{
    public static class TestHttpContextFactory
    {
        public static HttpContext CreateHttpContextWithUserId(Guid userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var claimsIdentity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            return httpContext;
        }
    }
}
