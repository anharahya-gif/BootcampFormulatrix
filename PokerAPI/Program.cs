using PokerAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Services
// =======================
builder.Services.AddControllers();
builder.Services.AddSingleton<GameController>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// --- Tambahkan CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhostUI", policy =>
    {
        policy.WithOrigins("http://localhost:5148") // ganti dengan port UI kamu
              .AllowAnyHeader()
              .AllowAnyMethod();
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

app.MapControllers();

// =======================
// Run (HARUS TERAKHIR)
// =======================
app.Run();
