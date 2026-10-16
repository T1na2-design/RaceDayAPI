using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RaceDay.Services
{
    public class SessionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "Session";
        public const string SessionUserIdKey = "UserId";
        public const string SessionUserRoleKey = "UserRole";

        // ISystemClock is obsolete in modern .NET; this constructor overload doesn't need it.
        public SessionAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var session = Context.Session;

            // Load asynchronously (matters for Redis / SQL Server session stores).
            await session.LoadAsync();

            var userId = session.GetInt32(SessionUserIdKey);
            var role = session.GetString(SessionUserRoleKey);

            if (!userId.HasValue || string.IsNullOrEmpty(role))
                return AuthenticateResult.NoResult();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            return AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName));
        }
    }
}
