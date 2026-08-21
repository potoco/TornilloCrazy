using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlTypes;
using DataServicio.Modelo;
using DataServicio.Tabla;

namespace DataServicio.Servicio;

public class ProductoService
{
    private readonly FerreteriaDbContext _context;

    public ProductoService(FerreteriaDbContext context)
    {
        _context = context;
    }
    public async Task<List<CandidatoDuplicadoDto>> BuscarCandidatosAsync(
        float[] vectorNuevoProducto,
        int top = 5)
    {
        // 1. Convertir el array de floats generado por la IA al tipo vectorial nativo
        var vectorConsulta = new SqlVector<float>(vectorNuevoProducto);

        // 2. Consulta LINQ con búsqueda vectorial por Distancia Coseno
        var candidatos = await _context.Productos
            .AsNoTracking()
            .Where(p => p.VectorEmbedding != null)
            .Select(p => new CandidatoDuplicadoDto
            {
                ProductoId = p.ProductoId,
                NombreCanonico = p.Nombre,
                AtributosJson = p.AtributosJson,
                // EF.Functions.VectorDistance traduce a VECTOR_DISTANCE('cosine', ...)
                Distancia = EF.Functions.VectorDistance("cosine", p.VectorEmbedding.Value, vectorConsulta)
            })
            .OrderBy(c => c.Distancia)
            .Take(top)
            .ToListAsync();

        return candidatos;
    }

    public async Task<ProductoTbl> CrearProductoAsync(string nombre, string? atributosJson, float[] vectorEmbedding)
    {
        var producto = new ProductoTbl
        {
            Nombre = nombre,
            AtributosJson = atributosJson,
            VectorEmbedding = new SqlVector<float>(vectorEmbedding)
        };
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return producto;
    }
}
