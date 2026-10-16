using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();

    // Surface XML doc comments (generated via GenerateDocumentationFile) in Swagger.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// EF Core DbContext: SQL Server when a connection string is configured.
// Without one, Development/Testing falls back to InMemory; Production fails fast.
var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(defaultConn))
{
    builder.Services.AddDbContext<RaceDay.Data.ApplicationDbContext>(options =>
        options.UseSqlServer(defaultConn));
}
else if (!builder.Environment.IsProduction())
{
    builder.Services.AddDbContext<RaceDay.Data.ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("RaceDayDev"));
}
else
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is not configured.");
}

// Session-based auth
// NOTE: the in-memory cache loses sessions on restart and doesn't work across multiple
// instances. For production, swap for Redis (AddStackExchangeRedisCache) or SQL Server cache.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.IdleTimeout = TimeSpan.FromHours(8);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<RaceDay.Services.IAuthService, RaceDay.Services.AuthService>();
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<RaceDay.Models.User>, Microsoft.AspNetCore.Identity.PasswordHasher<RaceDay.Models.User>>();

// Custom session authentication so [Authorize] and Roles work based on session
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = RaceDay.Services.SessionAuthenticationHandler.SchemeName;
    options.DefaultChallengeScheme = RaceDay.Services.SessionAuthenticationHandler.SchemeName;
}).AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, RaceDay.Services.SessionAuthenticationHandler>(
    RaceDay.Services.SessionAuthenticationHandler.SchemeName, _ => { });

builder.Services.AddAuthorization();

// Rate limiting for brute-force protection. Apply with [EnableRateLimiting("login")]
// on the login action/controller.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Make the implicit Program class visible for WebApplicationFactory in tests
public partial class Program { }
