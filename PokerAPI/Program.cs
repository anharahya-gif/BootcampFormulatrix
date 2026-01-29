using PokerAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Services
// =======================
builder.Services.AddControllers();
builder.Services.AddSingleton<GameController>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseAuthorization();

app.MapControllers();

// =======================
// Run (HARUS TERAKHIR)
// =======================
app.Run();
