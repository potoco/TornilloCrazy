using DataServicio.Tabla;
using Microsoft.EntityFrameworkCore;

namespace DataServicio.Servicio;

public class PersonaService
{
    public sealed record ProveedorPersonaDetalle(
        int ProveedorId,
        int PersonaId,
        string? Cuit,
        string Nombre,
        List<DireccionTbl> Direcciones,
        List<TelefonoTbl> Telefonos,
        List<EmailTbl> Emails);

    private readonly FerreteriaDbContext _dbContext;

    public PersonaService(FerreteriaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProveedorPersonaDetalle?> ObtenerDetalleProveedorAsync(int proveedorId)
    {
        return await _dbContext.Proveedores
            .AsNoTracking()
            .Where(p => p.ProveedorId == proveedorId && p.Estado == 1)
            .Select(p => new ProveedorPersonaDetalle(
                p.ProveedorId,
                p.PersonaId,
                p.Persona.CUIT,
                p.Persona.Nombre,
                p.Persona.Direcciones!
                    .OrderBy(d => d.DireccionId)
                    .Select(d => new DireccionTbl
                    {
                        DireccionId = d.DireccionId,
                        PersonaId = d.PersonaId,
                        Calle = d.Calle,
                        Numero = d.Numero,
                        Piso = d.Piso,
                        Depto = d.Depto
                    })
                    .ToList(),
                p.Persona.Telefonos!
                    .OrderBy(t => t.TelefonoId)
                    .Select(t => new TelefonoTbl
                    {
                        TelefonoId = t.TelefonoId,
                        PersonaId = t.PersonaId,
                        Telefono = t.Telefono,
                        Estado = t.Estado
                    })
                    .ToList(),
                p.Persona.Emails!
                    .OrderBy(e => e.EmailId)
                    .Select(e => new EmailTbl
                    {
                        EmailId = e.EmailId,
                        PersonaId = e.PersonaId,
                        Email = e.Email,
                        Estado = e.Estado
                    })
                    .ToList()))
            .FirstOrDefaultAsync();
    }

    public async Task ActualizarDatosPersonaAsync(int personaId, string cuit, string nombre)
    {
        var persona = await _dbContext.Personas.FirstOrDefaultAsync(p => p.PersonaId == personaId);
        if (persona is null)
            throw new KeyNotFoundException("Persona no encontrada.");

        persona.CUIT = cuit.Trim();
        persona.Nombre = nombre.Trim();
        await _dbContext.SaveChangesAsync();
    }

    public async Task AgregarDireccionAsync(int personaId, string calle, string? numero, string? piso, string? depto)
    {
        await ValidarPersonaExiste(personaId);

        var direccion = new DireccionTbl
        {
            PersonaId = personaId,
            Calle = calle.Trim(),
            Numero = Limpiar(numero),
            Piso = Limpiar(piso),
            Depto = Limpiar(depto)
        };

        _dbContext.Direcciones.Add(direccion);
        await _dbContext.SaveChangesAsync();
    }

    public async Task ActualizarDireccionAsync(int personaId, int direccionId, string calle, string? numero, string? piso, string? depto)
    {
        var direccion = await _dbContext.Direcciones
            .FirstOrDefaultAsync(d => d.DireccionId == direccionId && d.PersonaId == personaId);

        if (direccion is null)
            throw new KeyNotFoundException("Dirección no encontrada.");

        direccion.Calle = calle.Trim();
        direccion.Numero = Limpiar(numero);
        direccion.Piso = Limpiar(piso);
        direccion.Depto = Limpiar(depto);
        await _dbContext.SaveChangesAsync();
    }

    public async Task EliminarDireccionAsync(int personaId, int direccionId)
    {
        var direccion = await _dbContext.Direcciones
            .FirstOrDefaultAsync(d => d.DireccionId == direccionId && d.PersonaId == personaId);

        if (direccion is null)
            throw new KeyNotFoundException("Dirección no encontrada.");

        _dbContext.Direcciones.Remove(direccion);
        await _dbContext.SaveChangesAsync();
    }

    public async Task AgregarTelefonoAsync(int personaId, string telefono)
    {
        await ValidarPersonaExiste(personaId);

        var registro = new TelefonoTbl
        {
            PersonaId = personaId,
            Telefono = telefono.Trim(),
            Estado = 1
        };

        _dbContext.Telefonos.Add(registro);
        await _dbContext.SaveChangesAsync();
    }

    public async Task ActualizarTelefonoAsync(int personaId, int telefonoId, string telefono)
    {
        var registro = await _dbContext.Telefonos
            .FirstOrDefaultAsync(t => t.TelefonoId == telefonoId && t.PersonaId == personaId);

        if (registro is null)
            throw new KeyNotFoundException("Teléfono no encontrado.");

        registro.Telefono = telefono.Trim();
        registro.Estado = 1;
        await _dbContext.SaveChangesAsync();
    }

    public async Task EliminarTelefonoAsync(int personaId, int telefonoId)
    {
        var registro = await _dbContext.Telefonos
            .FirstOrDefaultAsync(t => t.TelefonoId == telefonoId && t.PersonaId == personaId);

        if (registro is null)
            throw new KeyNotFoundException("Teléfono no encontrado.");

        _dbContext.Telefonos.Remove(registro);
        await _dbContext.SaveChangesAsync();
    }

    public async Task AgregarEmailAsync(int personaId, string email)
    {
        await ValidarPersonaExiste(personaId);

        var registro = new EmailTbl
        {
            PersonaId = personaId,
            Email = email.Trim(),
            Estado = 1
        };

        _dbContext.Emails.Add(registro);
        await _dbContext.SaveChangesAsync();
    }

    public async Task ActualizarEmailAsync(int personaId, int emailId, string email)
    {
        var registro = await _dbContext.Emails
            .FirstOrDefaultAsync(e => e.EmailId == emailId && e.PersonaId == personaId);

        if (registro is null)
            throw new KeyNotFoundException("Email no encontrado.");

        registro.Email = email.Trim();
        registro.Estado = 1;
        await _dbContext.SaveChangesAsync();
    }

    public async Task EliminarEmailAsync(int personaId, int emailId)
    {
        var registro = await _dbContext.Emails
            .FirstOrDefaultAsync(e => e.EmailId == emailId && e.PersonaId == personaId);

        if (registro is null)
            throw new KeyNotFoundException("Email no encontrado.");

        _dbContext.Emails.Remove(registro);
        await _dbContext.SaveChangesAsync();
    }

    private async Task ValidarPersonaExiste(int personaId)
    {
        var existe = await _dbContext.Personas.AnyAsync(p => p.PersonaId == personaId);
        if (!existe)
            throw new KeyNotFoundException("Persona no encontrada.");
    }

    private static string? Limpiar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        return valor.Trim();
    }
}

