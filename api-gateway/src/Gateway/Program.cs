using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── YARP Reverse Proxy ────────────────────────────────────────────────────────
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var secret   = jwtSection["Secret"]   ?? throw new InvalidOperationException("Jwt:Secret no configurado.");
var issuer   = jwtSection["Issuer"]   ?? throw new InvalidOperationException("Jwt:Issuer no configurado.");
var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience no configurado.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = issuer,
            ValidAudience            = audience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ClockSkew                = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── Rate Limiting (Fixed Window — 100 req/min por IP) ─────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("gateway-policy", limiterOptions =>
    {
        limiterOptions.AutoReplenishment = true;
        limiterOptions.PermitLimit       = 100;
        limiterOptions.Window            = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit        = 10;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── CORS ──────────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:3000", "http://localhost:5173"];

builder.Services.AddCors(options =>
    options.AddPolicy("FrontendPolicy", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

// ── Health Checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseRateLimiter();
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapReverseProxy()
   .RequireRateLimiting("gateway-policy");

await app.RunAsync().ConfigureAwait(false);
