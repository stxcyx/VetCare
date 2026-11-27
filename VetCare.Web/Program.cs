using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VetCare.DA;
using VetCare.Web.Proxies;

var builder = WebApplication.CreateBuilder(args);

var cultura = new CultureInfo("es-CR");
cultura.NumberFormat.NumberDecimalSeparator = ".";
cultura.NumberFormat.NumberGroupSeparator = ",";
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

// La Web usa la base solo para Identity.
var cadena = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Falta la cadena de conexión 'DefaultConnection'.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(cadena));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
        options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Clientes HTTP hacia la API
var apiUrl = builder.Configuration["ApiUrl"]
    ?? throw new InvalidOperationException(
        "Falta 'ApiUrl' en appsettings.json.");

builder.Services.AddHttpClient<ClienteProxy>(
    c => c.BaseAddress = new Uri(apiUrl));

builder.Services.AddHttpClient<MascotaProxy>(
    c => c.BaseAddress = new Uri(apiUrl));

builder.Services.AddHttpClient<VeterinarioProxy>(
    c => c.BaseAddress = new Uri(apiUrl));

builder.Services.AddHttpClient<IProductoProxy, ProductoProxy>(
    c => c.BaseAddress = new Uri(apiUrl));

builder.Services.AddHttpClient<IDashboardProxy, DashboardProxy>(
    c => c.BaseAddress = new Uri(apiUrl));

builder.Services.AddHttpClient<IReporteProxy, ReporteProxy>(
    c => c.BaseAddress = new Uri(apiUrl));

builder.Services.AddHttpClient<IAtencionProxy, AtencionProxy>(
    c => c.BaseAddress = new Uri(apiUrl));

// Cliente para comprobar la disponibilidad del API
builder.Services.AddHttpClient("ApiDisponibilidad", c =>
{
    c.BaseAddress = new Uri(apiUrl);
    c.Timeout = TimeSpan.FromSeconds(3);
});

// HU-03: todas las páginas exigen sesión y uno de los tres roles
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AuthorizeFilter(
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireRole("Administrador", "Recepcionista", "Veterinario")
            .Build())));

builder.Services.ConfigureApplicationCookie(options =>
    options.AccessDeniedPath = "/Home/AccesoDenegado");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Estado/{0}");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

// Esperar automáticamente a que el API esté disponible
if (app.Environment.IsDevelopment())
{
    var factory = app.Services
        .GetRequiredService<IHttpClientFactory>();

    using var http = factory.CreateClient("ApiDisponibilidad");

    var apiDisponible = false;

    for (var intento = 1; intento <= 30; intento++)
    {
        try
        {
            using var respuesta = await http.GetAsync("/health");

            if (respuesta.IsSuccessStatusCode)
            {
                apiDisponible = true;

                app.Logger.LogInformation(
                    "El API de VetCare está disponible.");

                break;
            }
        }
        catch (HttpRequestException)
        {
            // El API todavía está iniciando.
        }
        catch (TaskCanceledException)
        {
            // El API todavía no responde.
        }

        app.Logger.LogInformation(
            "Esperando el API de VetCare. Intento {Intento}/30",
            intento);

        await Task.Delay(1000);
    }

    if (!apiDisponible)
    {
        throw new InvalidOperationException(
            "No se pudo establecer comunicación con el API de VetCare.");
    }
}

app.Run();