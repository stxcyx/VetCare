using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetCare.Web.Models;

namespace VetCare.Web.Data;

public static class SeedData
{
    public static async Task InicializarAsync(IServiceProvider sp, IConfiguration config)
    {
        var context = sp.GetRequiredService<ApplicationDbContext>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.MigrateAsync();   // crea la base si no existe

        foreach (var rol in new[] { "Administrador", "Recepcionista", "Veterinario" })
            if (!await roleManager.RoleExistsAsync(rol))
                await roleManager.CreateAsync(new IdentityRole(rol));

        var clave = config["Seed:Password"]
            ?? throw new InvalidOperationException("Falta el secreto 'Seed:Password'. Vea el README.");

        await CrearUsuarioAsync(userManager, "admin@vetcare.test", "Administrador VetCare", "Administrador", clave);
        await CrearUsuarioAsync(userManager, "recepcion@vetcare.test", "Recepción VetCare", "Recepcionista", clave);
        var uMora = await CrearUsuarioAsync(userManager, "lmora@vetcare.test", "Dra. Laura Mora", "Veterinario", clave);
        var uPerez = await CrearUsuarioAsync(userManager, "jperez@vetcare.test", "Dr. José Pérez", "Veterinario", clave);

        if (await context.Clientes.AnyAsync()) return;   // ya hay datos de prueba

        var perro = new Especie { Nombre = "Perro" };
        var gato = new Especie { Nombre = "Gato" };
        context.Especies.AddRange(perro, gato, new Especie { Nombre = "Ave" }, new Especie { Nombre = "Otro" });

        var c1 = new Cliente
        {
            Cedula = "1-1234-0567",
            Nombre = "Ana",
            Apellidos = "Rojas Solís",
            Telefono = "88881234",
            Correo = "ana.rojas@correo.com"
        };
        var c2 = new Cliente
        {
            Cedula = "2-0456-0789",
            Nombre = "Carlos",
            Apellidos = "Vargas Mora",
            Telefono = "87776655",
            Correo = "carlos.vargas@correo.com"
        };
        context.Clientes.AddRange(c1, c2);

        var luna = new Mascota
        {
            Cliente = c1,
            Especie = perro,
            Nombre = "Luna",
            Raza = "Labrador",
            Sexo = "Hembra",
            FechaNacimiento = new DateTime(2022, 3, 15),
            PesoKg = 24.5m
        };
        var michi = new Mascota
        {
            Cliente = c1,
            Especie = gato,
            Nombre = "Michi",
            Raza = "Siamés",
            Sexo = "Macho",
            FechaNacimiento = new DateTime(2024, 1, 10),
            PesoKg = 4.2m
        };
        context.Mascotas.AddRange(luna, michi);

        var v1 = new Veterinario
        {
            Usuario = uMora,
            CodigoColegiado = "VET-1001",
            Nombre = "Dra. Laura Mora",
            Especialidad = "Medicina general",
            Telefono = "88880001",
            HoraInicio = new TimeSpan(8, 0, 0),
            HoraFin = new TimeSpan(16, 0, 0)
        };
        var v2 = new Veterinario
        {
            Usuario = uPerez,
            CodigoColegiado = "VET-1002",
            Nombre = "Dr. José Pérez",
            Especialidad = "Cirugía",
            Telefono = "88880002",
            HoraInicio = new TimeSpan(10, 0, 0),
            HoraFin = new TimeSpan(18, 0, 0)
        };
        context.Veterinarios.AddRange(v1, v2);

        var p1 = new Producto
        {
            Nombre = "Amoxicilina 250 mg",
            Categoria = CategoriaProducto.Medicamento,
            Precio = 3500m,
            Stock = 20,
            StockMinimo = 5
        };
        var p2 = new Producto
        {
            Nombre = "Vacuna antirrábica",
            Categoria = CategoriaProducto.Vacuna,
            Precio = 8000m,
            Stock = 3,
            StockMinimo = 5
        };
        var p3 = new Producto
        {
            Nombre = "Jeringa 3 ml",
            Categoria = CategoriaProducto.Insumo,
            Precio = 250m,
            Stock = 100,
            StockMinimo = 20
        };
        context.Productos.AddRange(p1, p2, p3);

        var citaA = new Cita
        {
            Mascota = luna,
            Veterinario = v1,
            FechaHora = DateTime.Today.AddDays(1).AddHours(9),
            Motivo = "Control anual",
            Estado = EstadoCita.Programada
        };
        var citaB = new Cita
        {
            Mascota = luna,
            Veterinario = v1,
            FechaHora = DateTime.Today.AddDays(-30).AddHours(10),
            Motivo = "Vacunación",
            Estado = EstadoCita.Atendida,
            Diagnostico = "Paciente sano",
            Tratamiento = "Vacuna aplicada"
        };
        var citaC = new Cita
        {
            Mascota = luna,
            Veterinario = v2,
            FechaHora = DateTime.Today.AddDays(-10).AddHours(11),
            Motivo = "Revisión",
            Estado = EstadoCita.Cancelada,
            MotivoCancelacion = "El cliente no pudo asistir"
        };
        context.Citas.AddRange(citaA, citaB, citaC);

        context.CitaProductos.Add(new CitaProducto { Cita = citaB, Producto = p1, Cantidad = 2 });

        await context.SaveChangesAsync();
    }

    private static async Task<ApplicationUser> CrearUsuarioAsync(
        UserManager<ApplicationUser> um, string correo, string nombre, string rol, string clave)
    {
        var usuario = await um.FindByEmailAsync(correo);
        if (usuario is null)
        {
            usuario = new ApplicationUser
            {
                UserName = correo,
                Email = correo,
                EmailConfirmed = true,
                NombreCompleto = nombre
            };
            var resultado = await um.CreateAsync(usuario, clave);
            if (!resultado.Succeeded)
                throw new InvalidOperationException(string.Join("; ", resultado.Errors.Select(e => e.Description)));
            await um.AddToRoleAsync(usuario, rol);
        }
        return usuario;
    }
}