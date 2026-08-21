using DataServicio.Tabla;
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

    /*
SELECT top 50 pp.* from [LisPreProData] pp
where pp.EstadoRevisionIA=0
order by LisPreProDataId
     */
    public async Task<List<LisPreProDataTbl>> ObtenerItemsPreciosSinProcesarAsync(int cantidad=50)
    {
        return await _dbContext.LisPreProData
            .AsNoTracking()
            .Where(item => item.EstadoRevisionIA == 0)
            .OrderBy(item => item.LisPreProDataId)
            .Take(cantidad)
            .ToListAsync();
    } 
}

public sealed record ProveedorDto(int ProveedorId, int PersonaId, string? CUIT, string RazonSocial);

public sealed record ListaPrecioArchivoDto(
    int ListaPrecioProveedorId,
    int ProveedorId,
    DateTime? FecIngreso,
    string? NombreArchivo,
    string? NombreArchivoOriginal);
