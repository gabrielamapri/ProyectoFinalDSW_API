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

// Configurar CORS

var allowOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
 ?? new [] { "http://localhost:5173", "https://localhost:7057", "http://localhost:5291" }; // Valor por defecto si no se encuentra en la configuración

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
