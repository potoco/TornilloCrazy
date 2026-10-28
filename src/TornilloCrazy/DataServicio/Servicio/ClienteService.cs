using DataServicio.Modelo;
using DataServicio.Tabla;
using Microsoft.EntityFrameworkCore;

namespace DataServicio.Servicio;

public class ClienteService
{
    public sealed record ClientePersonaDetalle(
        int ClienteId,
        int PersonaId,
        string? Cuit,
        string Nombre,
        List<DireccionTbl> Direcciones,
        List<TelefonoTbl> Telefonos,
        List<EmailTbl> Emails,
        decimal DescuentoPorcentaje);

    private readonly FerreteriaDbContext _dbContext;

    public ClienteService(FerreteriaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ClientePersonaDetalle?> ObtenerDetalleClienteAsync(int clienteId)
    {
        return await _dbContext.Clientes
            .AsNoTracking()
            .Where(p => p.ClienteId == clienteId && p.Estado >= 0)
            .Select(p => new ClientePersonaDetalle(
                p.ClienteId,
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
                    .ToList(), p.DescuentoPorcentaje))
            .FirstOrDefaultAsync();
    }

    public async Task<ClienteDto> CrearClienteAsync(string cuit, string razonSocial, decimal descuentoPorcentaje = 0)
    {
        ValidarDescuento(descuentoPorcentaje);
        var persona = new PersonaTbl
        {
            CUIT = cuit.Trim(),
            Nombre = razonSocial.Trim(),
            Estado = 1
        };

        var cliente = new ClienteTbl { Persona = persona, Estado = 1, DescuentoPorcentaje = descuentoPorcentaje };
        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();

        return new ClienteDto(
            cliente.ClienteId,
            persona.PersonaId,
            persona.CUIT,
            persona.Nombre, cliente.DescuentoPorcentaje);
    }

    public async Task<List<ClienteDto>> ObtenerClientesAsync()
    {
        return await _dbContext.Clientes
            .AsNoTracking()
            .Where(p => p.Estado >= 0)
            .Include(p => p.Persona)
            .OrderBy(p => p.Persona.Nombre)
            .Select(p => new ClienteDto(
                p.ClienteId,
                p.PersonaId,
                p.Persona.CUIT,
                p.Persona.Nombre, p.DescuentoPorcentaje))
            .ToListAsync();
    }

    public async Task<ClienteDto?> ObtenerClientePorIdAsync(int clienteId)
    {
        return await _dbContext.Clientes
            .AsNoTracking()
            .Include(p => p.Persona)
            .Where(p => p.ClienteId == clienteId && p.Estado >= 0)
            .Select(p => new ClienteDto(
                p.ClienteId,
                p.PersonaId,
                p.Persona.CUIT,
                p.Persona.Nombre, p.DescuentoPorcentaje))
            .FirstOrDefaultAsync();
    }

    public async Task ActualizarClienteAsync(int clienteId, string cuit, string razonSocial, decimal descuentoPorcentaje)
    {
        ValidarDescuento(descuentoPorcentaje);
        var cliente = await _dbContext.Clientes.Include(c => c.Persona)
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId && c.Estado >= 0);
        if (cliente is null) throw new KeyNotFoundException("Cliente no encontrado.");
        cliente.Persona.CUIT = cuit.Trim();
        cliente.Persona.Nombre = razonSocial.Trim();
        cliente.DescuentoPorcentaje = descuentoPorcentaje;
        await _dbContext.SaveChangesAsync();
    }

    private static void ValidarDescuento(decimal porcentaje)
    {
        if (porcentaje < 0 || porcentaje > 100 || decimal.Round(porcentaje, 2) != porcentaje)
            throw new ArgumentOutOfRangeException(nameof(porcentaje), "El descuento debe estar entre 0 y 100%, con hasta dos decimales.");
    }

    public async Task BajaLogicaClienteAsync(int clienteId)
    {
        var cliente = await _dbContext.Clientes
            .FirstOrDefaultAsync(p => p.ClienteId == clienteId);

        if (cliente is null)
            throw new KeyNotFoundException("Cliente no encontrado.");

        cliente.Estado = -1;
        await _dbContext.SaveChangesAsync();
    }

}
