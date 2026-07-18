using LinkUpPro.Application;
using LinkUpPro.Infrastructure.Identity;
using LinkUpPro.Infrastructure.Persistence;
using LinkUpPro.Infrastructure.Shared;
using LinkUpPro.WebApp;
using LinkUpPro.WebApp.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddWebAppServices();

var app = builder.Build();

await app.Services.RunIdentitySeedAsync();

// Middleware global de excepciones (ANTES de Auth)
app.UseGlobalExceptionMiddleware();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// En desarrollo, desactivar cache del navegador completamente
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "0";
        await next();
    });
}

app.UseHttpsRedirection();

// Static files: en producción con cache inmutable (los archivos usan asp-append-version → hash en URL)
var staticFileOptions = new StaticFileOptions();
if (!app.Environment.IsDevelopment())
{
    staticFileOptions.OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
    };
}
app.UseStaticFiles(staticFileOptions);

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

await app.RunAsync();
