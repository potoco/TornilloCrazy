using DataServicio.Modelo;
using DataServicio.Tabla;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    async Task CrearProductoAsync_porlasdudasAca(List<ListaRevisionProductoMaestro> listaNuevo)
    {
        var ids = listaNuevo.Select(l => l.ProveedorMaestroId).ToHashSet();

        var proveedores = await _context.ProveedorMaestros
            .Where(p => ids.Contains(p.ProveedorMaestroId))
            .ToListAsync();

        foreach (var prov in proveedores) 
        {
        
            var candidato = await _context.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.VectorEmbedding != null &&
                EF.Functions.VectorDistance("cosine", p.VectorEmbedding.Value, prov.VectorEmbedding.Value) <= 0.05 );

            if(candidato != null)
            {
                var prd = new ProductoTbl
                {
                    Nombre = prov.NombreCanonico,
                    AtributosJson = prov.AtributosJson,
                    RubrosJson = prov.RubrosJson,
                    UsosJson = prov.UsosJson,
                    TextoVectorial = prov.TextoVectorial,
                    SinonimosJson = prov.SinonimosJson,
                    JsonRaw = prov.JsonRaw,
                    VectorEmbedding = prov.VectorEmbedding
                };
                _context.Productos.AddRange(prd);
                await _context.SaveChangesAsync(); // IDs generados aquí
            }
            if(!prov.ProductoId.HasValue)
                prov.ProductoId = candidato.ProductoId;

            var idSimilares = listaNuevo.FirstOrDefault(x => x.ProveedorMaestroId == prov.ProveedorId).ProveedorMaestroIdSimilares;
            if (idSimilares != null && idSimilares.Any())
            {
                var proveedoresSimilares = await _context.ProveedorMaestros
                    .Where(p => idSimilares.Contains(p.ProveedorMaestroId))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(pm => pm.ProductoId, candidato.ProductoId)
                    );
            }

        }
        await _context.SaveChangesAsync(); // IDs generados aquí
    }


    public async Task CrearProductoAsync(List<ListaRevisionProductoMaestro> listaNuevo)
    {
        var ids = listaNuevo.Select(l => l.ProveedorMaestroId).ToHashSet();

        var proveedores = await _context.ProveedorMaestros
            .Where(p => ids.Contains(p.ProveedorMaestroId))
            .ToListAsync();

        foreach (var prov in proveedores)
        {
            // 1) Buscar candidato similar
            var candidato = await _context.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.VectorEmbedding != null &&
                    prov.VectorEmbedding != null &&
                    EF.Functions.VectorDistance("cosine", p.VectorEmbedding.Value, prov.VectorEmbedding.Value) <= 0.05
                );

            ProductoTbl productoFinal;

            // 2) Si NO hay candidato → crear producto nuevo
            if (candidato == null)
            {
                productoFinal = new ProductoTbl
                {
                    Nombre = prov.NombreCanonico,
                    AtributosJson = prov.AtributosJson,
                    RubrosJson = prov.RubrosJson,
                    UsosJson = prov.UsosJson,
                    TextoVectorial = prov.TextoVectorial,
                    SinonimosJson = prov.SinonimosJson,
                    JsonRaw = prov.JsonRaw,
                    VectorEmbedding = prov.VectorEmbedding
                };

                // EF Core generará el ID cuando hagamos SaveChanges
                prov.Producto = productoFinal;
            }
            else
            {
                // Reutilizamos el producto existente
                productoFinal = candidato;

                // Asignamos el ID existente
                prov.ProductoId = candidato.ProductoId;
            }
            prov.FechaSeleccionProducto = DateTime.Now;
        }
        await _context.SaveChangesAsync();

        foreach (var prov in proveedores)
        {
            // 3) Asignar producto a proveedores similares
            var similares = listaNuevo
                .FirstOrDefault(x => x.ProveedorMaestroId == prov.ProveedorMaestroId)?
                .ProveedorMaestroIdSimilares;

            if (similares != null && similares.Any())
            {
                await _context.ProveedorMaestros
                    .Where(p => similares.Contains(p.ProveedorMaestroId))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(pm => pm.ProductoId, prov.ProductoId)
                        .SetProperty(pm => pm.FechaSeleccionProducto, prov.FechaSeleccionProducto)
                    );
            }
        }
    }



}
