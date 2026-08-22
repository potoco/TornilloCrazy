using DataServicio.Modelo;
using DataServicio.Tabla;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;

namespace DataServicio.Servicio;

public class ProveedorService
{
    private readonly FerreteriaDbContext _dbContext;

    public ProveedorService(FerreteriaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProveedorDto> CrearProveedorAsync(string cuit, string razonSocial)
    {
        var persona = new PersonaTbl
        {
            CUIT = cuit.Trim(),
            Nombre = razonSocial.Trim()
        };

        _dbContext.Personas.Add(persona);
        await _dbContext.SaveChangesAsync();

        var proveedor = new ProveedorTbl
        {
            PersonaId = persona.PersonaId
        };

        _dbContext.Proveedores.Add(proveedor);
        await _dbContext.SaveChangesAsync();

        return new ProveedorDto(
            proveedor.ProveedorId,
            persona.PersonaId,
            persona.CUIT,
            persona.Nombre);
    }

    public async Task<List<ProveedorDto>> ObtenerProveedoresAsync()
    {
        return await _dbContext.Proveedores
            .AsNoTracking()
            .Include(p => p.Persona)
            .Select(p => new ProveedorDto(
                p.ProveedorId,
                p.PersonaId,
                p.Persona.CUIT,
                p.Persona.Nombre))
            .ToListAsync();
    }

    public async Task<ProveedorDto?> ObtenerProveedorPorIdAsync(int proveedorId)
    {
        return await _dbContext.Proveedores
            .AsNoTracking()
            .Include(p => p.Persona)
            .Where(p => p.ProveedorId == proveedorId)
            .Select(p => new ProveedorDto(
                p.ProveedorId,
                p.PersonaId,
                p.Persona.CUIT,
                p.Persona.Nombre))
            .FirstOrDefaultAsync();
    }

    public async Task<ListaPrecioArchivoDto> RegistrarListaPrecioAsync(int proveedorId,string nombreArchivoGuid,string nombreArchivoOriginal)
    {
        var proveedorExiste = await _dbContext.Proveedores.AnyAsync(p => p.ProveedorId == proveedorId);
        if (!proveedorExiste)
            throw new KeyNotFoundException("Proveedor no encontrado.");

        var registro = new ListaPrecioProveedorTbl
        {
            ProveedorId = proveedorId,
            FecIngreso = DateTime.Now,
            NombreArchivo = nombreArchivoGuid,
            NombreArchivoOriginal = nombreArchivoOriginal
        };

        _dbContext.ListaPrecioProveedores.Add(registro);
        await _dbContext.SaveChangesAsync();

        return new ListaPrecioArchivoDto(
            registro.ListaPrecioProveedorId,
            proveedorId,
            registro.FecIngreso,
            registro.NombreArchivo,
            registro.NombreArchivoOriginal);
    }

    public async Task AgregarItemPrecioAsync(int listaPrecioProveedorId, string descripcion, string lineaCruda)
    {
        if (listaPrecioProveedorId <= 0)
            throw new ArgumentOutOfRangeException(nameof(listaPrecioProveedorId), "La lista de precio es obligatoria.");

        var listaExiste = await _dbContext.ListaPrecioProveedores
            .AnyAsync(lp => lp.ListaPrecioProveedorId == listaPrecioProveedorId);

        if (!listaExiste)
            throw new KeyNotFoundException("La lista de precio no existe.");

        var item = new LisPreProDataTbl
        {
            Descripcion1 = descripcion.Trim(),
            LineaCruda = lineaCruda.Trim(),
            ListaPrecioProveedorId = listaPrecioProveedorId
        };

        _dbContext.LisPreProData.Add(item);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<ListaPrecioProveedorTbl>> ObtenerListaPrecioSinProcesarAsync()
    {
        return await _dbContext.ListaPrecioProveedores
            .AsNoTracking()
            .Where(item => item.FecProcesamiento == null)
            .OrderBy(item => item.FecIngreso)
            .ToListAsync();
    }

    public async Task MarcarListaPrecioComoProcesadaAsync(int idPrecioLista)
    {
        var lista = await _dbContext.ListaPrecioProveedores
            .FirstOrDefaultAsync(lp => lp.ListaPrecioProveedorId == idPrecioLista);

        if (lista == null)
            throw new KeyNotFoundException("La lista de precio no existe.");

        lista.FecProcesamiento = DateTime.Now;
        await _dbContext.SaveChangesAsync();    
    }


    public async Task<List<ProveedorMaestroTbl>> ObtenerItemsProveedor(int cantidad=50)
    {
        return await _dbContext.ProveedorMaestros
            .AsNoTracking()
            .Where(item => item.EstadoRevisionIA == 0)
            .OrderBy(item => item.ProveedorMaestroId)
            .Take(cantidad)
            .ToListAsync();
    }

    public async Task GuardarItemMaestroAsync(string descripcion, int proveedorId)
    {
        var existe = await _dbContext.ProveedorMaestros.AnyAsync(item => item.Descripcion1.ToLower() == descripcion.ToLower() && item.ProveedorId == proveedorId);
        if (!existe)
        {
            var maestro = new ProveedorMaestroTbl
            {
                NombreCanonico = "",
                Descripcion1 = descripcion.ToUpper().Trim(),
                ProveedorId = proveedorId
            };
            _dbContext.ProveedorMaestros.Add(maestro);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task ActualizaryMarcarComoProcesados(List<ProveedorMaestroTbl> proveedorMaestroTbls)
    {
        var ids = proveedorMaestroTbls.Select(x => x.ProveedorMaestroId).ToList();

        var entidadesDb = await _dbContext.ProveedorMaestros
            .Where(pm => ids.Contains(pm.ProveedorMaestroId))
            .ToListAsync();

        foreach (var entidad in entidadesDb)
        {
            var origen = proveedorMaestroTbls
                .First(x => x.ProveedorMaestroId == entidad.ProveedorMaestroId);

            entidad.EstadoRevisionIA = 2;
            entidad.FechaRevisionIA = DateTime.Now;
            entidad.NombreCanonico = origen.NombreCanonico;
            entidad.RubrosJson = origen.RubrosJson;
            entidad.AtributosJson = origen.AtributosJson;
            entidad.SinonimosJson = origen.SinonimosJson;
            entidad.UsosJson = origen.UsosJson;
            entidad.JsonRaw = origen.JsonRaw;
            entidad.TextoVectorial = origen.TextoVectorial;
            entidad.VectorEmbedding = origen.VectorEmbedding;
        }
        await _dbContext.SaveChangesAsync();
    }
    public async Task MarcarItemsComoProcesados(List<ProveedorMaestroTbl> proveedorMaestroTbls)
    {
        var ids = proveedorMaestroTbls.Select(x => x.ProveedorMaestroId).ToList();

        await _dbContext.ProveedorMaestros
            .Where(pm => ids.Contains(pm.ProveedorMaestroId))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(pm => pm.EstadoRevisionIA, 2)
                .SetProperty(pm => pm.FechaRevisionIA, DateTime.Now)
            );
    }
    public async Task MarcarItemsEnProceso(List<ProveedorMaestroTbl> proveedorMaestroTbls)
    {
        var ids = proveedorMaestroTbls.Select(x => x.ProveedorMaestroId).ToList();

        await _dbContext.ProveedorMaestros
            .Where(pm => ids.Contains(pm.ProveedorMaestroId))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(pm => pm.EstadoRevisionIA, 1)
            );
    }

    public async Task<List<CandidatoDuplicadoDto>> BuscarProductoProveedor(string textoUsuario, float[] textoUsuarioVectorial, int top = 5)
    {
        var vectorConsulta = new SqlVector<float>(textoUsuarioVectorial);
        var candidatos = await _dbContext.ProveedorMaestros
            .AsNoTracking()
            .Where(p => p.VectorEmbedding != null)
            .Select(p => new CandidatoDuplicadoDto
            {
                ProveedorId = p.ProveedorId,
                NombreCanonico = p.NombreCanonico,
                AtributosJson = p.AtributosJson,
                // EF.Functions.VectorDistance traduce a VECTOR_DISTANCE('cosine', ...)
                Distancia = EF.Functions.VectorDistance("cosine", p.VectorEmbedding.Value, vectorConsulta)
            })
            .OrderBy(c => c.Distancia)
            .Take(top)
            .ToListAsync();
        return candidatos;

    }

}

