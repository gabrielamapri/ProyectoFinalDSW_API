using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using CentroTerapia.API;
using CentroTerapia.API.Middleware;
using Microsoft.OpenApi.Models;
// Data seeding removed per user request
using CentroTerapia.Application;
using CentroTerapia.Infrastructure;

var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (!File.Exists(envPath))
{
    envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
}

if (File.Exists(envPath))
{
    Env.Load(envPath);
    Console.WriteLine($"Loaded .env from: {envPath}");

    // Build ConnectionStrings__DefaultConnection from DB_* env vars if not provided
    var existingConn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
    if (string.IsNullOrWhiteSpace(existingConn))
    {
        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
        var name = Environment.GetEnvironmentVariable("DB_NAME");
        var user = Environment.GetEnvironmentVariable("DB_USER");
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

        if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(user))
        {
            var conn = $"server={host};port={port};database={name};user={user};password={password};";
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", conn);
            Console.WriteLine("Constructed ConnectionStrings__DefaultConnection from DB_* env vars.");
        }
    }
}

var builder = WebApplication.CreateBuilder(args);
// Force the app to listen on localhost:5192 unless overridden by environment
builder.WebHost.UseUrls("http://localhost:5192");

// Configurar CORS

var allowOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
 ?? new [] {
    "http://localhost:5173",
    "http://localhost:5178",
    "http://localhost:5182", // Vite dev server
    "http://localhost:5183", // Vite alternate dev port
    "https://localhost:7057",
    "http://localhost:5291",
    "https://localhost:5001",
    "http://localhost:5000"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // In Development allow any localhost origin to avoid editing the list each time Vite uses a different port.
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(origin =>
            {
                try
                {
                    var u = new Uri(origin);
                    // Allow loopback and localhost hostnames
                    return u.IsLoopback || string.Equals(u.Host, "localhost", StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(allowOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

builder.Services.AddInfrastructure();
builder.Services.AddApplication();

// Authentication - JWT
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
        };
    });

builder.Services.AddControllers( options => 
{
    options.Filters.Add<ValidatorFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

// Automatic seeding disabled

app.UseMiddleware<GlobalExceptionMiddleware>();


// Usar CORS antes de otros middlewares
app.UseCors("AllowFrontend");

// Always enable Swagger so reviewers can see API docs after cloning.
app.UseSwagger();
app.UseSwaggerUI();

// Only enforce HTTPS redirection outside Development so local HTTP (swagger) works when developing
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Development-only: ensure seeded admin has a known password for testing (resets to "123")
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        if (app.Environment.IsDevelopment())
        {
            var uow = services.GetRequiredService<CentroTerapia.Domain.Ports.Out.IUnitOfWork>();
            var logger = services.GetRequiredService<ILogger<Program>>();
            var admin = uow.Users.GetByEmailAsync("admin@centro.local").GetAwaiter().GetResult();
            if (admin != null)
            {
                admin.HashContrasena = BCrypt.Net.BCrypt.HashPassword("123");
                uow.Users.UpdateAsync(admin).GetAwaiter().GetResult();
                uow.SaveChangesAsync().GetAwaiter().GetResult();
                logger.LogInformation("Admin password reset to '123' in Development environment.");
            }
            else
            {
                logger.LogWarning("Admin user not found when attempting to reset password.");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetService<ILogger<Program>>();
        logger?.LogError(ex, "Error while attempting to reset admin password");
    }
}

app.Run();
