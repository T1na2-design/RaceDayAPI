using System;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
});

// EF Core DbContext - use SQL Server if a connection string is provided; otherwise fall back to InMemory for tests/dev
var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(defaultConn))
{
    builder.Services.AddDbContext<RaceDay.Data.ApplicationDbContext>(options =>
        options.UseSqlServer(defaultConn));
}
else
{
    // Use InMemory only when EF Core InMemory provider is available (tests/local).
    // The provider is included in test projects or when running locally.
    // Register DbContext without a provider here so tests or local dev can replace it with an InMemory provider.
    builder.Services.AddDbContext<RaceDay.Data.ApplicationDbContext>();
}

// Session-based auth
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Make the implicit Program class visible for WebApplicationFactory in tests
public partial class Program { }
