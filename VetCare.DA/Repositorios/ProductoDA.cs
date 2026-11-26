using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VetCare.Abstracciones.Comunes;
using VetCare.Abstracciones.Interfaces;
using VetCare.Abstracciones.Modelos;

namespace VetCare.DA.Repositorios;

public class ProductoDA : IProductoDA
{
    private readonly ApplicationDbContext _context;

    public ProductoDA(ApplicationDbContext context) => _context = context;

    public async Task<PaginaResultado<Producto>> ListarAsync(
        string? buscar, int pagina, int tamano)
    {
        var elementos = new List<Producto>();
        var total = 0;

        var conn = (SqlConnection)_context.Database.GetDbConnection();

        await _context.Database.OpenConnectionAsync();

        try
        {
            await using var cmd = new SqlCommand("SP_Producto_Listar", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Buscar",
                string.IsNullOrWhiteSpace(buscar) ? DBNull.Value : (object)buscar);

            cmd.Parameters.AddWithValue("@Pagina", pagina);
            cmd.Parameters.AddWithValue("@Tamano", tamano);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                elementos.Add(LeerProducto(reader));
            }

            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                total = reader.GetInt32(0);
            }
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }

        return new PaginaResultado<Producto>
        {
            Elementos = elementos,
            Pagina = pagina,
            TotalRegistros = total,
            TotalPaginas = (int)Math.Ceiling(total / (double)tamano)
        };
    }

    public async Task<Producto?> ObtenerAsync(int id)
    {
        var conn = (SqlConnection)_context.Database.GetDbConnection();

        await _context.Database.OpenConnectionAsync();

        try
        {
            await using var cmd = new SqlCommand("SP_Producto_Obtener", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ProductoId", id);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                return LeerProducto(reader);

            return null;
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<bool> ExisteProductoAsync(
        string nombre, CategoriaProducto categoria,
        int? excluirProductoId = null)
    {
        var conn = (SqlConnection)_context.Database.GetDbConnection();

        await _context.Database.OpenConnectionAsync();

        try
        {
            await using var cmd = new SqlCommand("SP_Producto_Existe", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Nombre", nombre);
            cmd.Parameters.AddWithValue("@Categoria", categoria.ToString());

            cmd.Parameters.AddWithValue("@ExcluirProductoId",
                (object?)excluirProductoId ?? DBNull.Value);

            var resultado = await cmd.ExecuteScalarAsync();

            return resultado is bool existe && existe;
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<bool> ExisteProveedorAsync(int proveedorId)
    {
        var conn = (SqlConnection)_context.Database.GetDbConnection();

        await _context.Database.OpenConnectionAsync();

        try
        {
            await using var cmd = new SqlCommand("SP_Proveedor_Existe", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ProveedorId", proveedorId);

            var resultado = await cmd.ExecuteScalarAsync();

            return resultado is bool existe && existe;
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<Producto> AgregarAsync(Producto producto)
    {
        var conn = (SqlConnection)_context.Database.GetDbConnection();

        await _context.Database.OpenConnectionAsync();

        try
        {
            await using var cmd = new SqlCommand("SP_Producto_Agregar", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            AgregarParametros(cmd, producto);

            var resultado = await cmd.ExecuteScalarAsync();

            producto.ProductoId = Convert.ToInt32(resultado);

            return producto;
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<bool> ActualizarAsync(Producto producto)
    {
        var conn = (SqlConnection)_context.Database.GetDbConnection();

        await _context.Database.OpenConnectionAsync();

        try
        {
            await using var cmd = new SqlCommand("SP_Producto_Actualizar", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ProductoId", producto.ProductoId);

            AgregarParametros(cmd, producto);

            var resultado = await cmd.ExecuteScalarAsync();

            return resultado is bool actualizado && actualizado;
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var conn = (SqlConnection)_context.Database.GetDbConnection();

        await _context.Database.OpenConnectionAsync();

        try
        {
            await using var cmd = new SqlCommand("SP_Producto_Eliminar", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ProductoId", id);

            var resultado = await cmd.ExecuteScalarAsync();

            return resultado is bool eliminado && eliminado;
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    private static void AgregarParametros(SqlCommand cmd, Producto producto)
    {
        cmd.Parameters.AddWithValue("@Nombre", producto.Nombre);
        cmd.Parameters.AddWithValue("@Categoria", producto.Categoria.ToString());
        cmd.Parameters.AddWithValue("@ProveedorId", producto.ProveedorId);

        var precio = cmd.Parameters.Add("@Precio", SqlDbType.Decimal);
        precio.Precision = 18;
        precio.Scale = 2;
        precio.Value = producto.Precio;

        cmd.Parameters.AddWithValue("@Stock", producto.Stock);
        cmd.Parameters.AddWithValue("@StockMinimo", producto.StockMinimo);

        cmd.Parameters.Add("@FechaVencimiento", SqlDbType.DateTime2).Value =
            (object?)producto.FechaVencimiento ?? DBNull.Value;

        cmd.Parameters.AddWithValue("@Activo", producto.Activo);
    }

    private static Producto LeerProducto(SqlDataReader reader)
    {
        return new Producto
        {
            ProductoId = reader.GetInt32(reader.GetOrdinal("ProductoId")),

            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),

            Categoria = Enum.Parse<CategoriaProducto>(
                reader.GetString(reader.GetOrdinal("Categoria"))),

            ProveedorId = reader.GetInt32(reader.GetOrdinal("ProveedorId")),

            Precio = reader.GetDecimal(reader.GetOrdinal("Precio")),

            Stock = reader.GetInt32(reader.GetOrdinal("Stock")),

            StockMinimo = reader.GetInt32(reader.GetOrdinal("StockMinimo")),

            FechaVencimiento = reader.IsDBNull(
                reader.GetOrdinal("FechaVencimiento"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("FechaVencimiento")),

            Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),

            Proveedor = new Proveedor
            {
                ProveedorId = reader.GetInt32(reader.GetOrdinal("ProveedorId")),
                Nombre = reader.GetString(reader.GetOrdinal("NombreProveedor"))
            }
        };
    }
}