using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PokerAPIMPwDB.Infrastructure.Persistence;
using PokerAPIMPwDB.API.Controllers;
using PokerAPIMPwDB.Infrastructure.Services;
using PokerAPIMPwDB.Hubs;
using PokerAPIMPwDB.Services;
using Microsoft.AspNetCore.Identity;
using PokerAPIMPwDB.Infrastructure.Persistence.Entities;
using PokerAPIMPwDB.Infrastructure.Persistence.Repositories;
using PokerAPIMPwDB.Infrastructure.Mappings;

var builder = WebApplication.CreateBuilder(args);

// EF Core SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=poker.db");
});

// Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Controllers
builder.Services.AddControllers();
// Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ITableRepository, TableRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IPlayerServiceTable, PlayerServiceTable>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<LobbyService>(); 
builder.Services.AddSingleton<GameManager>();
builder.Services.AddSignalR();



// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Add JWT support in Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? "DefaultSecretKeyThatIsAtLeast32CharactersLong"))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// ======================
// Middleware pipeline
// ======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Map controllers (AuthController, etc)
app.MapControllers();
app.MapHub<PokerHub>("/pokerHub");

app.Run();
