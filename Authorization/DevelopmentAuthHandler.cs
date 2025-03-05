using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace FullstackDotNetCore.Authorization
{
    public class DevelopmentAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public DevelopmentAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "dev-user-id"),
                new Claim(ClaimTypes.Name, "dev-user@example.com"),
                new Claim(ClaimTypes.Role, "Developer")
            };

            var identity = new ClaimsIdentity(claims, "Development");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "DevelopmentScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
} 