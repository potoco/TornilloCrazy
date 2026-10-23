using DataServicio.Modelo;
using DataServicio.Tabla;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DataServicio.Servicio;

public class ProductoService
{
    private readonly FerreteriaDbContext _context;

    public ProductoService(FerreteriaDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductoTbl>> ListarUltimosProductosAsync(int cantidad = 8)
    {
        return await _context.Productos
            .AsNoTracking()
            .Where(p => p.Estado >= 0)
            .OrderByDescending(p => p.ProductoId)
            .Take(cantidad)
            .ToListAsync();
    }

    public async Task<List<ProductoTbl>> BuscarProductosPorNombreAsync(string textoBuscar, int top = 50)
    {
        if (string.IsNullOrWhiteSpace(textoBuscar))
            return await ListarUltimosProductosAsync(8);

        var texto = textoBuscar.Trim();

        return await _context.Productos
            .AsNoTracking()
            .Where(p => p.Estado >= 0 &&
                (EF.Functions.Like(p.Nombre, $"%{texto}%") ||
                 (p.Codigo != null && EF.Functions.Like(p.Codigo, $"%{texto}%"))))
            .OrderBy(p => p.Nombre)
            .Take(top)
            .ToListAsync();
    }

    public async Task<List<ProductoImagenTbl>> ObtenerImagenesProductoAsync(int productoId)
    {
        return await _context.ProductoImagenes
            .AsNoTracking()
            .Where(i => i.ProductoId == productoId)
            .OrderByDescending(i => i.EsPrincipal)
            .ThenByDescending(i => i.ProductoImagenId)
            .ToListAsync();
    }

    public async Task<ProductoImagenTbl> AgregarImagenProductoAsync(int productoId, string nombreArchivo, bool esPrincipal)
    {
        if (esPrincipal)
        {
            await _context.ProductoImagenes
                .Where(i => i.ProductoId == productoId && i.EsPrincipal)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.EsPrincipal, false));
        }

        var imagen = new ProductoImagenTbl
        {
            ProductoId = productoId,
            NombreArchivo = nombreArchivo,
            EsPrincipal = esPrincipal
        };

        _context.ProductoImagenes.Add(imagen);
        await _context.SaveChangesAsync();
        return imagen;
    }

    public async Task<string?> EliminarImagenProductoAsync(int productoId, int productoImagenId)
    {
        var imagen = await _context.ProductoImagenes
            .FirstOrDefaultAsync(i => i.ProductoImagenId == productoImagenId && i.ProductoId == productoId);

        if (imagen is null)
            return null;

        _context.ProductoImagenes.Remove(imagen);
        await _context.SaveChangesAsync();
        return imagen.NombreArchivo;
    }

    public async Task MarcarImagenPrincipalAsync(int productoId, int productoImagenId)
    {
        await _context.ProductoImagenes
            .Where(i => i.ProductoId == productoId && i.EsPrincipal)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.EsPrincipal, false));

        await _context.ProductoImagenes
            .Where(i => i.ProductoImagenId == productoImagenId && i.ProductoId == productoId)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.EsPrincipal, true));
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
            where EF.Functions.VectorDistance("cosine", prd.VectorEmbedding.Value, vectorConsulta) <= 0.3
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
                    string jsonRaw = prov.JsonRaw ?? "{}"; // Valor por defecto si es null
                    productoFinal = new ProductoTbl
                    {
                        Nombre = prov.NombreCanonico,
                        AtributosJson = prov.AtributosJson,
                        RubrosJson = prov.RubrosJson,
                        UsosJson = prov.UsosJson,
                        TextoVectorial = prov.TextoVectorial,
                        SinonimosJson = prov.SinonimosJson,
                        JsonRaw = jsonRaw,
                        DescripcionDetallada = prov.DescripcionDetallada,
                        VectorEmbedding = prov.VectorEmbedding,
                        PrecioUnitario = ExtraerPrecioUnitario(jsonRaw), // Valor por defecto
                        Rubros = ExtraerArrayAString(jsonRaw, "rubro"),
                        Sinonimos = ExtraerArrayAString(jsonRaw, "sinonimos")
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


    private decimal ExtraerPrecioUnitario(string? atributosJson)
    {
        if (string.IsNullOrWhiteSpace(atributosJson))
            return 0;

        try
        {
            using var document = JsonDocument.Parse(atributosJson);

            if (!document.RootElement.TryGetProperty("precio_minimo", out var precioElement))
                return 0;

            if (precioElement.ValueKind == JsonValueKind.Number &&
                precioElement.TryGetDecimal(out var precio))
            {
                return precio;
            }

            if (precioElement.ValueKind == JsonValueKind.String &&
                decimal.TryParse(
                    precioElement.GetString(),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out precio))
            {
                return precio;
            }

            return 0;
        }
        catch (JsonException)
        {
            return 0;
        }
    }

    private string ExtraerArrayAString(string? atributosJson, string propiedad)
    {
        if (string.IsNullOrWhiteSpace(atributosJson))
            return string.Empty;

        try
        {
            using var document = JsonDocument.Parse(atributosJson);

            if (!document.RootElement.TryGetProperty(propiedad, out var arrayElement))
                return string.Empty;

            if (arrayElement.ValueKind != JsonValueKind.Array)
                return string.Empty;

            return string.Join(", ",
                arrayElement.EnumerateArray()
                        .Where(x => x.ValueKind == JsonValueKind.String)
                        .Select(x => x.GetString())
                        .Where(x => !string.IsNullOrWhiteSpace(x)));
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }
}
