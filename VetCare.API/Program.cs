using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetCare.Abstracciones.Interfaces;
using VetCare.DA;
using VetCare.DA.Repositorios;
using VetCare.Flujo;
using VetCare.Reglas;

var builder = WebApplication.CreateBuilder(args);

// Base de datos
var cadena = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection'.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(cadena));

// Identity solo para crear los roles y usuarios de prueba (SeedData)
builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Inyección de dependencias: cada interfaz con su implementación, capa por capa
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

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// Crea la base, los roles, los usuarios de prueba y los datos semilla
using (var scope = app.Services.CreateScope())
{
    await SeedData.InicializarAsync(scope.ServiceProvider, app.Configuration);
}

app.Run();