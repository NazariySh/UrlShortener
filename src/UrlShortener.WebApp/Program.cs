using Microsoft.AspNetCore.Identity;
using UrlShortener.Application.Extensions;
using UrlShortener.Infrastructure.Extensions;
using UrlShortener.WebApp.Extensions;
using UrlShortener.WebApp.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<CustomExceptionFilter>();
});

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddSingleton<CustomExceptionFilter>();

var app = builder.Build();

await app.InitializeAsync();

app.UseExceptionHandler("/Home/Error");
app.UseHsts();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=User}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();