using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Interfaces.DA;
using VetCare.DA;
using VetCare.DA.Repositorios;
using VetCare.Flujo;
using VetCare.Reglas;

var builder = WebApplication.CreateBuilder(args);

// Base de datos
var cadena = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Falta la cadena de conexión 'DefaultConnection'.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(cadena));

// Identity solo para crear los roles y usuarios de prueba (SeedData)
builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Inyección de dependencias
builder.Services.AddScoped<IClienteDA, ClienteDA>();
builder.Services.AddScoped<IClienteReglas, ClienteReglas>();
builder.Services.AddScoped<IClienteFlujo, ClienteFlujo>();

builder.Services.AddScoped<IMascotaDA, MascotaDA>();
builder.Services.AddScoped<IMascotaReglas, MascotaReglas>();
builder.Services.AddScoped<IMascotaFlujo, MascotaFlujo>();

builder.Services.AddScoped<IProductoDA, ProductoDA>();
builder.Services.AddScoped<IProductoReglas, ProductoReglas>();
builder.Services.AddScoped<IProductoFlujo, ProductoFlujo>();

builder.Services.AddScoped<IVeterinarioDA, VeterinarioDA>();
builder.Services.AddScoped<IVeterinarioReglas, VeterinarioReglas>();
builder.Services.AddScoped<IVeterinarioFlujo, VeterinarioFlujo>();
builder.Services.AddScoped<ICitaDA, CitaDA>();
builder.Services.AddScoped<ICitaReglas, CitaReglas>();
builder.Services.AddScoped<ICitaFlujo, CitaFlujo>();

builder.Services.AddScoped<IUsuarioDA, UsuarioDA>();
builder.Services.AddScoped<IUsuarioReglas, UsuarioReglas>();
builder.Services.AddScoped<IUsuarioFlujo, UsuarioFlujo>();

builder.Services.AddScoped<IDashboardDA, DashboardDA>();
builder.Services.AddScoped<IDashboardFlujo, DashboardFlujo>();

builder.Services.AddScoped<IReporteDA, ReporteDA>();
builder.Services.AddScoped<IReporteFlujo, ReporteFlujo>();

// Atención veterinaria
builder.Services.AddScoped<IAtencionDA, AtencionDA>();
builder.Services.AddScoped<IAtencionFlujo, AtencionFlujo>();

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.ReferenceHandler =
        ReferenceHandler.IgnoreCycles);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Comprobación de disponibilidad del API
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// Endpoint de disponibilidad
app.MapHealthChecks("/health");

// Crea la base, los roles, los usuarios de prueba y los datos semilla
using (var scope = app.Services.CreateScope())
{
    await SeedData.InicializarAsync(
        scope.ServiceProvider, app.Configuration);
}

app.Run();