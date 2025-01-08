using ASP_P22.Services.Hash;
using ASP_P22.Services.Random;
using ASP_P22.Services.Time;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// реєструємо власні сервіси у контейнері builder.Services
builder.Services.AddSingleton<IRandomService, AbcRandomService>();
builder.Services.AddSingleton<IHashService, Md5HashService>();
builder.Services.AddSingleton<ITimeService, TimeService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
