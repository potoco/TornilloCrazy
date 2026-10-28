using DataServicio.Modelo;
using DataServicio.Tabla;
using Microsoft.EntityFrameworkCore;

namespace DataServicio.Servicio;

public class UsuarioService
{
    public sealed record UsuarioDetalle(
        int UsuarioId,
        int PersonaId,
        string LoginUser,
        int Estado,
        string Nombre,
        string? ApePaterno,
        string? ApeMaterno,
        string? DNI,
        string? CUIT,
        List<DireccionTbl> Direcciones,
        List<TelefonoTbl> Telefonos,
        List<EmailTbl> Emails,
        List<RolDto> Roles);

    private readonly FerreteriaDbContext _dbContext;
    public UsuarioService(FerreteriaDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<UsuarioTbl?> ObtenerUsuarioPorNombreAsync(string nombreUsuario)
    {
        return await _dbContext.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.LoginUser == nombreUsuario);
    }

    public async Task<List<UsuarioDto>> ObtenerUsuariosAsync()
    {
        return await _dbContext.Usuarios
            .AsNoTracking()
            .Include(u => u.Persona)
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
            .OrderBy(u => u.Persona.ApePaterno)
                .ThenBy(u => u.Persona.Nombre)
            .Select(u => new UsuarioDto(
                u.UsuarioId,
                u.PersonaId,
                u.LoginUser,
                u.Persona.Nombre,
                u.Persona.ApePaterno,
                u.Persona.ApeMaterno,
                u.Estado,
                u.UsuarioRoles
                    .Where(ur => ur.Rol.Activo)
                    .OrderBy(ur => ur.Rol.Nombre)
                    .Select(ur => ur.Rol.Nombre)
                    .ToList()))
            .ToListAsync();
    }

    public async Task<UsuarioDetalle?> ObtenerDetalleUsuarioAsync(int usuarioId)
    {
        return await _dbContext.Usuarios
            .AsNoTracking()
            .Where(u => u.UsuarioId == usuarioId)
            .Select(u => new UsuarioDetalle(
                u.UsuarioId,
                u.PersonaId,
                u.LoginUser,
                u.Estado,
                u.Persona.Nombre,
                u.Persona.ApePaterno,
                u.Persona.ApeMaterno,
                u.Persona.DNI,
                u.Persona.CUIT,
                u.Persona.Direcciones!
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
                u.Persona.Telefonos!
                    .OrderBy(t => t.TelefonoId)
                    .Select(t => new TelefonoTbl
                    {
                        TelefonoId = t.TelefonoId,
                        PersonaId = t.PersonaId,
                        Telefono = t.Telefono,
                        Estado = t.Estado
                    })
                    .ToList(),
                u.Persona.Emails!
                    .OrderBy(e => e.EmailId)
                    .Select(e => new EmailTbl
                    {
                        EmailId = e.EmailId,
                        PersonaId = e.PersonaId,
                        Email = e.Email,
                        Estado = e.Estado
                    })
                    .ToList(),
                u.UsuarioRoles
                    .Where(ur => ur.Rol.Activo)
                    .OrderBy(ur => ur.Rol.Nombre)
                    .Select(ur => new RolDto(ur.RolId, ur.Rol.Nombre))
                    .ToList()))
            .FirstOrDefaultAsync();
    }

    public async Task<UsuarioTbl> CrearUsuarioAsync(string loginUser, string loginPwd, string nombre, string? apePaterno, string? apeMaterno, string? dni, string? cuit)
    {
        var loginNormalizado = loginUser.Trim();
        var existe = await _dbContext.Usuarios.AnyAsync(u => u.LoginUser == loginNormalizado);
        if (existe)
            throw new InvalidOperationException($"Ya existe un usuario con el login '{loginNormalizado}'.");

        var persona = new PersonaTbl
        {
            Nombre = nombre.Trim(),
            ApePaterno = Limpiar(apePaterno),
            ApeMaterno = Limpiar(apeMaterno),
            DNI = Limpiar(dni),
            CUIT = Limpiar(cuit),
            Estado = 1
        };

        _dbContext.Personas.Add(persona);
        await _dbContext.SaveChangesAsync();

        var usuario = new UsuarioTbl
        {
            LoginUser = loginNormalizado,
            LoginPwd = loginPwd.Trim(),
            Estado = 1,
            PersonaId = persona.PersonaId
        };

        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync();

        return usuario;
    }

    public async Task ActualizarUsuarioAsync(int usuarioId, string loginUser, string? loginPwd, string nombre, string? apePaterno, string? apeMaterno, string? dni, string? cuit)
    {
        var usuario = await _dbContext.Usuarios
            .Include(u => u.Persona)
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

        if (usuario is null)
            throw new KeyNotFoundException("Usuario no encontrado.");

        var loginNormalizado = loginUser.Trim();
        var loginOcupado = await _dbContext.Usuarios
            .AnyAsync(u => u.LoginUser == loginNormalizado && u.UsuarioId != usuarioId);
        if (loginOcupado)
            throw new InvalidOperationException($"Ya existe otro usuario con el login '{loginNormalizado}'.");

        usuario.LoginUser = loginNormalizado;
        if (!string.IsNullOrWhiteSpace(loginPwd))
            usuario.LoginPwd = loginPwd.Trim();

        usuario.Persona.Nombre = nombre.Trim();
        usuario.Persona.ApePaterno = Limpiar(apePaterno);
        usuario.Persona.ApeMaterno = Limpiar(apeMaterno);
        usuario.Persona.DNI = Limpiar(dni);
        usuario.Persona.CUIT = Limpiar(cuit);

        await _dbContext.SaveChangesAsync();
    }

    public async Task BajaLogicaUsuarioAsync(int usuarioId)
    {
        var usuario = await _dbContext.Usuarios
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

        if (usuario is null)
            throw new KeyNotFoundException("Usuario no encontrado.");

        usuario.Estado = 0;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<RolDto>> ObtenerRolesActivosAsync()
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .Where(r => r.Activo)
            .OrderBy(r => r.Nombre)
            .Select(r => new RolDto(r.RolId, r.Nombre))
            .ToListAsync();
    }

    public async Task AsignarRolAsync(int usuarioId, int rolId)
    {
        var usuarioExiste = await _dbContext.Usuarios.AnyAsync(u => u.UsuarioId == usuarioId);
        if (!usuarioExiste)
            throw new KeyNotFoundException("Usuario no encontrado.");

        var rolExiste = await _dbContext.Roles.AnyAsync(r => r.RolId == rolId && r.Activo);
        if (!rolExiste)
            throw new KeyNotFoundException("Rol no encontrado.");

        var yaAsignado = await _dbContext.UsuarioRoles
            .AnyAsync(ur => ur.UsuarioId == usuarioId && ur.RolId == rolId);
        if (yaAsignado)
            return;

        _dbContext.UsuarioRoles.Add(new UsuarioRolTbl { UsuarioId = usuarioId, RolId = rolId });
        await _dbContext.SaveChangesAsync();
    }

    public async Task QuitarRolAsync(int usuarioId, int rolId)
    {
        var relacion = await _dbContext.UsuarioRoles
            .FirstOrDefaultAsync(ur => ur.UsuarioId == usuarioId && ur.RolId == rolId);

        if (relacion is null)
            throw new KeyNotFoundException("El usuario no tiene asignado ese rol.");

        _dbContext.UsuarioRoles.Remove(relacion);
        await _dbContext.SaveChangesAsync();
    }

    private static string? Limpiar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        return valor.Trim();
    }
}

