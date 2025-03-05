using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace FullstackDotNetCore.Authorization
{
    public class DevelopmentAuthHandlerBackup : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public DevelopmentAuthHandlerBackup(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            TimeProvider timeProvider)
            : base(options, logger, encoder, (ISystemClock)timeProvider)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "mock-user-id"),
                new Claim(ClaimTypes.Name, "mock@example.com"),
                new Claim(ClaimTypes.Role, "Developer")
            };

            var identity = new ClaimsIdentity(claims, "Development");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "DevelopmentScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
} 