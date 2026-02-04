using PokerAPI.Services;
using PokerAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Services
// =======================
builder.Services.AddControllers();
builder.Services.AddSingleton<GameController>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();  
builder.Services.AddSignalR(); 
// --- Tambahkan CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhostUI", policy =>
    {
        policy.WithOrigins("http://localhost:5148") 
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
