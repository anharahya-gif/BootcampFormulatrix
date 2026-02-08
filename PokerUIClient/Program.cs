using PokerUIClient.Services;
var builder = WebApplication.CreateBuilder(args);

// Tambahkan services
builder.Services.AddRazorPages();
builder.Services.AddHttpClient<ApiService>();

// Tambahkan session kalau pakai session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

// gunakan session
app.UseSession();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();


app.Run();
