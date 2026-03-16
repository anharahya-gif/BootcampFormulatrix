using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PokerAPIMultiplayerWithDB.Data;
using PokerAPIMultiplayerWithDB.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Ensure Database folder exists
System.IO.Directory.CreateDirectory("Database");

// Connection string
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=Database/poker.db";
builder.Services.AddDbContext<PokerDbContext>(options => options.UseSqlite(connectionString));

// JWT
var jwtKey = Environment.GetEnvironmentVariable("DOTNET_JWT_KEY");
if (string.IsNullOrEmpty(jwtKey))
{
    Console.WriteLine("WARNING: DOTNET_JWT_KEY not set. Authentication will fail without a secret.");
}
var key = Encoding.ASCII.GetBytes(jwtKey ?? "default_secret_should_be_overridden_in_env_vars");

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
        ValidateLifetime = true
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Allow token from query for SignalR
            var accessToken = context.Request.Query["access_token"].FirstOrDefault();
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/lobbyHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true));
});

// Register application services
builder.Services.AddScoped<PokerAPIMultiplayerWithDB.Services.IJwtTokenService, PokerAPIMultiplayerWithDB.Services.JwtTokenService>();
builder.Services.AddScoped<PokerAPIMultiplayerWithDB.Services.IHandRankEvaluator, PokerAPIMultiplayerWithDB.Services.HandRankEvaluator>();
builder.Services.AddSingleton<PokerAPIMultiplayerWithDB.Services.ITableGameService, PokerAPIMultiplayerWithDB.Services.TableGameService>();

var app = builder.Build();

// Enable Swagger for all environments (for development/testing)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "PokerAPIMultiplayerWithDB v1");
    options.RoutePrefix = string.Empty; // Serve Swagger UI at root
});

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<LobbyHub>("/lobbyHub");
app.MapHub<PokerAPIMultiplayerWithDB.Hubs.GameHub>("/gameHub");

app.Run();
