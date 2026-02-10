using PokerAPI.Services;
using PokerAPI.Services.Interfaces;
using PokerAPI.Hubs;
using Serilog;
// using PokerAPI.Logging; // Removed old logging
using PokerAPI.Infrastructure.Logging;

var builder = WebApplication.CreateBuilder(args);
// SerilogConfiguration.Configure(); // Removed old static config
// builder.Host.UseSerilog(); // This was already here but let's make sure we use the new way if needed, or just keep it. 
// actually the new way is to configure it via the host builder.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());
// =======================
// Services
// =======================

builder.Services.AddControllers();
builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();  
builder.Services.AddSignalR(); 
// --- Tambahkan CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhostUI", policy =>
    {
        policy.WithOrigins("http://localhost:5148","http://localhost:5071") 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
              
    });
});

var app = builder.Build();

// =======================
// Middleware
// =======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalhostUI");

app.UseAuthorization();

//app.MapControllers();
//addPokerHubforSignalR
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<PokerHub>("/pokerHub");
});

// =======================
// Run (HARUS TERAKHIR)
// =======================
app.Run();
