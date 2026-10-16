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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public const string SchemeName = "Session";

        public SessionAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IHttpContextAccessor httpContextAccessor)
            : base(options, logger, encoder, clock)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var http = _httpContextAccessor.HttpContext;
            if (http == null)
                return Task.FromResult(AuthenticateResult.NoResult());

            var session = http.Session;
            var userId = session.GetInt32("UserId");
            var role = session.GetString("UserRole");

            if (!userId.HasValue || string.IsNullOrEmpty(role))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, SchemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
