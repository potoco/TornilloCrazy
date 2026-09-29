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
        int top = 15)
    {
        // 1. Convertir el array de floats generado por la IA al tipo vectorial nativo
        var vectorConsulta = new SqlVector<float>(vectorNuevoProducto);

        // 2. Consulta LINQ con búsqueda vectorial por Distancia Coseno
        var candidatos = await _context.Productos
            .AsNoTracking()
            .Include(p => p.ProveedorMaestros) // Incluir la relación con ProveedorMaestros
            .Where(p => p.VectorEmbedding != null)
            .Select(p => new CandidatoDuplicadoDto
            {
                ProductoId = p.ProductoId,
                NombreCanonico = p.Nombre,
                AtributosJson = p.AtributosJson,
                
                Distancia = EF.Functions.VectorDistance("cosine", p.VectorEmbedding.Value, vectorConsulta)
            })
            .OrderBy(c => c.Distancia)
            .Take(top)
            .ToListAsync();

        return candidatos;
    }


    public async Task<List<ProductoResultadoDto>?> ObtenerProductoPorVectorAsync(float[] vectorNuevoProducto, int top = 60)
    {
        var vectorConsulta = new SqlVector<float>(vectorNuevoProducto);

        var resultado = await (
            from prd in _context.Productos
            join pm in _context.ProveedorMaestros on prd.ProductoId equals pm.ProductoId
            join lp in _context.ListaPrecioProveedores on pm.ProveedorId equals lp.ProveedorId
            join datap in _context.LisPreProData on lp.ListaPrecioProveedorId equals datap.ListaPrecioProveedorId
            join p in _context.Proveedores on pm.ProveedorId equals p.ProveedorId
            join per in _context.Personas on p.PersonaId equals per.PersonaId
            where EF.Functions.VectorDistance("cosine", prd.VectorEmbedding.Value, vectorConsulta) <= 0.6
               && pm.Descripcion1 == datap.Descripcion1
            select new ProductoResultadoDto
            {
                NombreProducto = prd.Nombre,
                ProductoId = pm.ProductoId.Value,
                ProveedorMaestroId = pm.ProveedorMaestroId,
                NombreCanonico = pm.NombreCanonico,
                Proveedor = per.Nombre,
                CodigoLista = lp.ListaPrecioProveedorId,
                FecIngreso = lp.FecIngreso.Value,
                LisPreProDataId = datap.LisPreProDataId,
                LineaCruda = datap.LineaCruda,
                TextoVectorial = prd.TextoVectorial,
                DescripcionDetallada = prd.DescripcionDetallada,
                Distancia = EF.Functions.VectorDistance("cosine", prd.VectorEmbedding.Value, vectorConsulta)

            }
        )
        .OrderBy(c => c.Distancia)
        .Take(top)
        .ToListAsync();

        return resultado;

    }



    public async Task<List<ProductoResultadoDto>?> ObtenerProductoPorTextoAsync(string textoBuscar, int top = 15)
    {

        var resultado = await (
            from prd in _context.Productos
            join pm in _context.ProveedorMaestros on prd.ProductoId equals pm.ProductoId
            join lp in _context.ListaPrecioProveedores on pm.ProveedorId equals lp.ProveedorId
            join datap in _context.LisPreProData on lp.ListaPrecioProveedorId equals datap.ListaPrecioProveedorId
            join p in _context.Proveedores on pm.ProveedorId equals p.ProveedorId
            join per in _context.Personas on p.PersonaId equals per.PersonaId
            where (EF.Functions.Like(prd.Nombre, $"%{textoBuscar}%")
                || EF.Functions.Like(prd.TextoVectorial, $"%{textoBuscar}%"))
               && pm.Descripcion1 == datap.Descripcion1
            select new ProductoResultadoDto
            {
                NombreProducto = prd.Nombre,
                ProductoId = pm.ProductoId.Value,
                ProveedorMaestroId = pm.ProveedorMaestroId,
                NombreCanonico = pm.NombreCanonico,
                Proveedor = per.Nombre,
                CodigoLista = lp.ListaPrecioProveedorId,
                FecIngreso = lp.FecIngreso.Value,
                LisPreProDataId = datap.LisPreProDataId,
                TextoVectorial = prd.TextoVectorial,
                LineaCruda = datap.LineaCruda,
                Distancia = 100
            }
        )
        .Take(top)
        .ToListAsync();

        return resultado;

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
                        DescripcionDetallada = prov.DescripcionDetallada,
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
