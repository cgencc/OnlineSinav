using Microsoft.AspNetCore.Authentication.Cookies;
using OnlineSinav.MVC.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITokenStorage, CookieTokenStorage>();
builder.Services.AddScoped<ApiService>();

// IHttpClientFactory: Authorization header'ý redirect sonrasý düþürmemek için
// DangerousAcceptAnyServerCertificateValidator yerine redirect'i kapatýyoruz
builder.Services.AddHttpClient("API", client =>
{
    // BaseAddress burada set etmiyoruz, ApiService'de dinamik set ediliyor
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // HTTPS redirect sonrasý Authorization header'ýnýn düþmesini önle
    AllowAutoRedirect = false
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Shared/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(3);
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// HTTPS redirect'i kapat - API ile HTTP üzerinden konuþuyoruz, redirect auth header'ý düþürür
// app.UseHttpsRedirection(); // DEVRE DIÞI

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();