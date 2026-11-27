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

// La Web usa la base solo para Identity (inicio de sesión y roles).
// Los datos de los módulos llegan por la API.
var cadena = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection'.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(cadena));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
        options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Clientes HTTP hacia la API (uno por módulo)
var apiUrl = builder.Configuration["ApiUrl"]
    ?? throw new InvalidOperationException("Falta 'ApiUrl' en appsettings.json.");
builder.Services.AddHttpClient<ClienteProxy>(c => c.BaseAddress = new Uri(apiUrl));
builder.Services.AddHttpClient<MascotaProxy>(c => c.BaseAddress = new Uri(apiUrl));
builder.Services.AddHttpClient<VeterinarioProxy>(c => c.BaseAddress = new Uri(apiUrl));
builder.Services.AddHttpClient<CitaProxy>(c => c.BaseAddress = new Uri(apiUrl));

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();