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
}

public sealed record ProveedorDto(int ProveedorId, int PersonaId, string? CUIT, string RazonSocial);
