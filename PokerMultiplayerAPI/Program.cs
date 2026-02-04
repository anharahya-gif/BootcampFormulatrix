using PokerMultiplayerAPI.Infrastructure.Persistence;
using PokerMultiplayerAPI.Infrastructure.Notifications;
using PokerMultiplayerAPI.Domain.Interfaces;
using PokerMultiplayerAPI.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Domain & Infrastructure
builder.Services.AddSingleton<ITableRepository, InMemoryTableRepository>();
builder.Services.AddScoped<IGameNotifier, SignalRGameNotifier>();
builder.Services.AddScoped<IGameService, PokerGameService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<PokerHub>("/pokerHub");

app.Run();
