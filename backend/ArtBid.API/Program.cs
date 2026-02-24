using ArtBid.Application.Services;
using ArtBid.Application.Repositories;
using ArtBid.Application.Interfaces;
using ArtBid.Infrastructure.Repositories;
using ArtBid.Infrastructure.Persistence;
using ArtBid.Infrastructure.Services;
using ArtBid.Infrastructure.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Conexión a DB
builder.Services.AddDbContext<AuctionDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT y autenticación
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT Secret is not configured in appsettings.json");
}

var key = Encoding.ASCII.GetBytes(jwtSecret);

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        // Importante: Mapear los claims correctamente
        NameClaimType = "name",  // Mapea el claim "name" a User.Identity.Name
        RoleClaimType = "role"   // Si usas roles
    };
    
    // Eventos mejorados para debugging y SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/auctionHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            
            // Log todos los claims para debugging
            if (context.Principal != null)
            {
                foreach (var claim in context.Principal.Claims)
                {
                    Console.WriteLine($"  Claim Type: '{claim.Type}' = '{claim.Value}'");
                }
            }
            
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            return Task.CompletedTask;
        }
    };
});

// Repositorios
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();

// Servicios de aplicación
builder.Services.AddScoped<AuthService>(provider => 
    new AuthService(
        provider.GetRequiredService<IUserRepository>(),
        jwtSecret
    ));

builder.Services.AddScoped<AuctionService>();
builder.Services.AddScoped<DemoScriptService>();

// SignalR
builder.Services.AddSignalR();
builder.Services.AddScoped<IAuctionNotifier, SignalRAuctionNotifier>();

// Background service para cerrar subastas
builder.Services.AddHostedService<AuctionClosingService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Añadir cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

// Asegurar que la base de datos existe
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
    dbContext.Database.EnsureCreated();

    // Seed inicial
    DbSeeder.Seed(dbContext);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();  // Primero autenticación
app.UseAuthorization();   // Luego autorización

// Mapear hubs de SignalR
app.MapHub<AuctionHub>("/auctionHub");

app.MapControllers();

app.Run();

