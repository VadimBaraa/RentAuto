using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentAutoWeb.Models;
using RentAutoWeb.Services;
using Microsoft.AspNetCore.Diagnostics;
using Stripe;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Добавляем DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавляем Identity
builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Добавляем SignInManager
builder.Services.AddHostedService<RentalExpirationService>();
builder.Services.AddScoped<YandexLocatorService>();
builder.Services.AddScoped<GenerateRandomCoordinates>();
builder.Services.AddHttpClient<WeatherService>();
builder.Services.AddDistributedMemoryCache(); // Кэш, который используем для сессий
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Устанавливаем тайм-аут сессии
    options.Cookie.HttpOnly = true; // Защита куки
    options.Cookie.IsEssential = true; // Cookie обязательны для работы
});
// Добавляем логирование
builder.Services.AddLogging();

var stripeSettings = builder.Configuration.GetSection("Stripe").Get<StripeSettings>();
StripeConfiguration.ApiKey = stripeSettings.SecretKey;

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(3); // или сколько хочешь
    options.LoginPath = "/Login/Login";
    options.LogoutPath = "/Login/Logout";
    options.SlidingExpiration = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var random = new Random();

    foreach (var car in db.Cars.ToList())
    {
        car.Latitude = 51.655 + random.NextDouble() * (51.670 - 51.655);
        car.Longitude = 39.180 + random.NextDouble() * (39.210 - 39.180);
    }

    db.SaveChanges(); // Сохраняем изменения
}

if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
            var exception = errorFeature?.Error;

            // Просто выводим текст ошибки без стека
            await context.Response.WriteAsync("Произошла ошибка: " + exception?.Message);
        });
    });
    app.UseHsts();
}

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    var watch = System.Diagnostics.Stopwatch.StartNew();

    await next.Invoke();

    watch.Stop();
    var logMessage = $"[{DateTime.Now}] {context.Request.Method} {context.Request.Path} took {watch.ElapsedMilliseconds} ms";
    Console.WriteLine(logMessage);
});
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
