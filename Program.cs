using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Futebol.Api.Dtos;
using Futebol.Api.Endpoints;
using Futebol.Api.Infrastructure;
using Futebol.Api.Infrastructure.Email;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configuração do EF Core
/* builder.Services.AddDbContext<FutebolDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))); */

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// AutoDetect tenta conectar no MySQL; durante design-time (dotnet ef) falha se o host não estiver acessível.
// Fixamos explicitamente a versão para permitir gerar migrations offline.
var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

builder.Services.AddDbContext<FutebolDbContext>(options =>
    options.UseMySql(
        connectionString,
        serverVersion
    )
);

// Configure JSON to use camelCase so frontend (camelCase) matches API responses
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// JWT
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// Configure Email Settings
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>();
builder.Services.AddSingleton(emailSettings ?? new EmailSettings());
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:5173").WithOrigins("https://www.softjack.com.br")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Caso não exista o banco, cria e aplica migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FutebolDbContext>();
    db.Database.Migrate();
}

SorteiosEndpoints.Map(app);
PartidasEndpoints.Map(app);
OrganizadoresEndpoints.Map(app);
JogadoresEndpoints.Map(app);
UsersEndpoints.Map(app);
AuthEndpoints.Map(app);
app.Run();
