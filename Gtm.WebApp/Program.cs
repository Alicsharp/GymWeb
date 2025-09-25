using Gtm.Application;
using Gtm.InfraStructure;
using Gtm.WebApp.Models;
using Gtm.WebApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Utility.Appliation.Auth;
using Utility.Appliation.FileService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var connectionstring = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddTransient<IFileService,FileService>();
builder.Services.AddTransient<IAuthService,AuthService>();
builder.Services.AddInfra(connectionstring).AddApplication();
builder.Services.AddHttpContextAccessor(); builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    x.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(x =>
    {
        x.ExpireTimeSpan = TimeSpan.FromDays(30);
        x.LoginPath = "/Auth/Login";
        x.LogoutPath = "/Auth/Logout";
        x.AccessDeniedPath = "/Auth/AccessDenied";
    });

builder.Services.Configure<SiteData>(builder.Configuration.GetSection("SiteData"));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(x =>
{
    x.MapAreaControllerRoute(
        name: "Admin",
        pattern: "Admin/{controller=Home}/{action=Index}/{id?}",
        areaName: "Admin");

    x.MapAreaControllerRoute(
        name: "UserPanel",
        pattern: "UserPanel/{controller=Home}/{action=Index}/{id?}",
        areaName: "UserPanel"
        );


    x.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}/{slug?}");
});



app.Run();
