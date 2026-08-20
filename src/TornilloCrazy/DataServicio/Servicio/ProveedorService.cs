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

}

public sealed record ProveedorDto(int ProveedorId, int PersonaId, string? CUIT, string RazonSocial);

public sealed record ListaPrecioArchivoDto(
    int ListaPrecioProveedorId,
    int ProveedorId,
    DateTime? FecIngreso,
    string? NombreArchivo,
    string? NombreArchivoOriginal);
