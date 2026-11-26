using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VetCare.Abstracciones.Modelos;

namespace VetCare.DA;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Especie> Especies => Set<Especie>();
    public DbSet<Mascota> Mascotas => Set<Mascota>();
    public DbSet<Veterinario> Veterinarios => Set<Veterinario>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<CitaProducto> CitaProductos => Set<CitaProducto>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Relaciones con Usuario (Identity). Los modelos solo guardan UsuarioId,
        // así Abstracciones no depende de Identity.
        b.Entity<Cliente>().HasOne<ApplicationUser>().WithMany().HasForeignKey(c => c.UsuarioId);   // 0..1 a N
        b.Entity<Veterinario>().HasOne<ApplicationUser>().WithMany().HasForeignKey(v => v.UsuarioId);

        // Relación entre productos y proveedores
        b.Entity<Producto>().HasOne(p => p.Proveedor).WithMany(p => p.Productos).HasForeignKey(p => p.ProveedorId);

        b.Entity<Cliente>().HasIndex(c => c.Cedula).IsUnique();
        b.Entity<Cliente>().HasIndex(c => c.UsuarioId);   // NO único: un usuario registra muchos clientes
        b.Entity<Veterinario>().HasIndex(v => v.CodigoColegiado).IsUnique();
        b.Entity<Veterinario>().HasIndex(v => v.UsuarioId).IsUnique();   // una cuenta por veterinario
        b.Entity<Producto>().HasIndex(p => new { p.Nombre, p.Categoria }).IsUnique();

        b.Entity<CitaProducto>().HasKey(cp => new { cp.CitaId, cp.ProductoId });

        b.Entity<Producto>().Property(p => p.Precio).HasPrecision(18, 2);
        b.Entity<Mascota>().Property(m => m.PesoKg).HasPrecision(4, 1);
        b.Entity<Cita>().Property(c => c.Estado).HasConversion<string>().HasMaxLength(20);
        b.Entity<Producto>().Property(p => p.Categoria).HasConversion<string>().HasMaxLength(20);

        // Eliminación restringida (RN-06), solo en las tablas del proyecto, no en las de Identity
        foreach (var fk in b.Model.GetEntityTypes()
                     .Where(e => e.ClrType.Namespace == typeof(Cliente).Namespace)
                     .SelectMany(e => e.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;
    }
}