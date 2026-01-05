using CargoBajaLib.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SEINMX.Clases.Tools;
using SEINMX.Clases.Utilerias;
using SEINMX.Config;
using SEINMX.Context;

var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString(builder.Environment.IsDevelopment() ? "ConnectionUpadate" : "DefaultConnection");

// =======================
// Configurar DbContext
// =======================
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.EnableSensitiveDataLogging();
});


builder.Services.AddDbContext<AppClassContext>(options =>

{
    options.UseSqlServer(connectionString);
    options.EnableSensitiveDataLogging();
});


// =======================
// Configurar autenticación por cookies
// =======================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<RazorViewToStringRenderer>();
//builder.Services.AddSingleton<BlazorRenderer>();
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<CronConfigServiceProvider>();
    builder.Services.AddHttpClient<ExchangeRateService>();
    builder.ConfigureBackgroundServices();
}

// =======================
// MVC
// =======================
builder.Services.AddControllersWithViews();


builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// =======================
// Build
// =======================
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
SEINMX.Clases.Helpers.UrlExtensions.Configure(httpContextAccessor);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // Antes de Authorization
app.UseAuthorization();


// =======================
// Rutas MVC
// =======================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();