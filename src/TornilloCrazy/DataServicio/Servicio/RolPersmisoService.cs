using DataServicio.Tabla;
using Microsoft.EntityFrameworkCore;

namespace DataServicio.Servicio;

public class RolPersmisoService
{
    public sealed record RolDetalle(
        int RolId,
        string Nombre,
        string? Descripcion,
        bool Activo,
        int CantidadUsuarios,
        IReadOnlyList<int> PermisoIds);

    private readonly FerreteriaDbContext _dbContext;
    public RolPersmisoService(FerreteriaDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<List<RolPermisoTbl>> ObtenerPermisosPorRolAsync(int rolId)
    {
        return await _dbContext.RolPermisos
            .AsNoTracking()
            .Where(rp => rp.RolId == rolId)
            .Include(rp => rp.Permiso)
            .OrderBy(rp => rp.Permiso.Nombre)
            .ToListAsync();
    }

    public async Task<List<RolTbl>> ObtenerRolesAsync(bool soloActivos = false)
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .Where(r => !soloActivos || r.Activo)
            .OrderBy(r => r.Nombre)
            .ToListAsync();
    }

    public async Task<RolDetalle?> ObtenerRolDetalleAsync(int rolId)
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .Where(r => r.RolId == rolId)
            .Select(r => new RolDetalle(
                r.RolId,
                r.Nombre,
                r.Descripcion,
                r.Activo,
                r.UsuarioRoles.Count,
                r.RolPermisos.Select(rp => rp.PermisoId).ToList()))
            .FirstOrDefaultAsync();
    }

    public async Task<RolTbl> CrearRolAsync(string nombre, string? descripcion)
    {
        var nombreNormalizado = nombre.Trim();
        var existe = await _dbContext.Roles.AnyAsync(r => r.Nombre == nombreNormalizado);
        if (existe)
            throw new InvalidOperationException($"Ya existe un rol con el nombre '{nombreNormalizado}'.");

        var rol = new RolTbl
        {
            Nombre = nombreNormalizado,
            Descripcion = Limpiar(descripcion),
            Activo = true
        };

        _dbContext.Roles.Add(rol);
        await _dbContext.SaveChangesAsync();
        return rol;
    }

    public async Task ActualizarRolAsync(int rolId, string nombre, string? descripcion, bool activo)
    {
        var rol = await _dbContext.Roles.FirstOrDefaultAsync(r => r.RolId == rolId);
        if (rol is null)
            throw new KeyNotFoundException("Rol no encontrado.");

        var nombreNormalizado = nombre.Trim();
        var nombreOcupado = await _dbContext.Roles
            .AnyAsync(r => r.Nombre == nombreNormalizado && r.RolId != rolId);
        if (nombreOcupado)
            throw new InvalidOperationException($"Ya existe otro rol con el nombre '{nombreNormalizado}'.");

        rol.Nombre = nombreNormalizado;
        rol.Descripcion = Limpiar(descripcion);
        rol.Activo = activo;
        await _dbContext.SaveChangesAsync();
    }

    public async Task CambiarEstadoRolAsync(int rolId, bool activo)
    {
        var rol = await _dbContext.Roles.FirstOrDefaultAsync(r => r.RolId == rolId);
        if (rol is null)
            throw new KeyNotFoundException("Rol no encontrado.");

        rol.Activo = activo;
        await _dbContext.SaveChangesAsync();
    }

    public async Task GuardarPermisosDeRolAsync(int rolId, IReadOnlyCollection<int> permisoIds)
    {
        var rolExiste = await _dbContext.Roles.AnyAsync(r => r.RolId == rolId);
        if (!rolExiste)
            throw new KeyNotFoundException("Rol no encontrado.");

        var actuales = await _dbContext.RolPermisos
            .Where(rp => rp.RolId == rolId)
            .ToListAsync();

        var actualesIds = actuales.Select(rp => rp.PermisoId).ToHashSet();
        var nuevosIds = permisoIds.ToHashSet();

        var aQuitar = actuales.Where(rp => !nuevosIds.Contains(rp.PermisoId)).ToList();
        if (aQuitar.Count > 0)
            _dbContext.RolPermisos.RemoveRange(aQuitar);

        var aAgregar = nuevosIds
            .Where(id => !actualesIds.Contains(id))
            .Select(id => new RolPermisoTbl { RolId = rolId, PermisoId = id })
            .ToList();
        if (aAgregar.Count > 0)
            _dbContext.RolPermisos.AddRange(aAgregar);

        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<PermisoTbl>> ObtenerPermisosAsync(bool soloActivos = false)
    {
        return await _dbContext.Permisos
            .AsNoTracking()
            .Where(p => !soloActivos || p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<PermisoTbl> CrearPermisoAsync(string codigo, string nombre, string? descripcion)
    {
        var codigoNormalizado = codigo.Trim().ToUpperInvariant();
        var existe = await _dbContext.Permisos.AnyAsync(p => p.Codigo == codigoNormalizado);
        if (existe)
            throw new InvalidOperationException($"Ya existe un permiso con el código '{codigoNormalizado}'.");

        var permiso = new PermisoTbl
        {
            Codigo = codigoNormalizado,
            Nombre = nombre.Trim(),
            Descripcion = Limpiar(descripcion),
            Activo = true
        };

        _dbContext.Permisos.Add(permiso);
        await _dbContext.SaveChangesAsync();
        return permiso;
    }

    public async Task ActualizarPermisoAsync(int permisoId, string codigo, string nombre, string? descripcion, bool activo)
    {
        var permiso = await _dbContext.Permisos.FirstOrDefaultAsync(p => p.PermisoId == permisoId);
        if (permiso is null)
            throw new KeyNotFoundException("Permiso no encontrado.");

        var codigoNormalizado = codigo.Trim().ToUpperInvariant();
        var codigoOcupado = await _dbContext.Permisos
            .AnyAsync(p => p.Codigo == codigoNormalizado && p.PermisoId != permisoId);
        if (codigoOcupado)
            throw new InvalidOperationException($"Ya existe otro permiso con el código '{codigoNormalizado}'.");

        permiso.Codigo = codigoNormalizado;
        permiso.Nombre = nombre.Trim();
        permiso.Descripcion = Limpiar(descripcion);
        permiso.Activo = activo;
        await _dbContext.SaveChangesAsync();
    }

    public async Task CambiarEstadoPermisoAsync(int permisoId, bool activo)
    {
        var permiso = await _dbContext.Permisos.FirstOrDefaultAsync(p => p.PermisoId == permisoId);
        if (permiso is null)
            throw new KeyNotFoundException("Permiso no encontrado.");

        permiso.Activo = activo;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<RolTbl>> ObtenerRolesConPermisosAsync()
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .Include(r => r.RolPermisos)
                .ThenInclude(rp => rp.Permiso)
            .OrderBy(r => r.Nombre)
            .ToListAsync();
    }

    public async Task<PermisoTbl?> ObtenerPermisoAsync(int permisoId)
    {
        return await _dbContext.Permisos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PermisoId == permisoId);
    }

    public async Task EliminarRolAsync(int rolId)
    {
        var rol = await _dbContext.Roles
            .Include(r => r.RolPermisos)
            .FirstOrDefaultAsync(r => r.RolId == rolId);
        if (rol is null)
            throw new KeyNotFoundException("Rol no encontrado.");

        var tieneUsuarios = await _dbContext.UsuarioRoles.AnyAsync(ur => ur.RolId == rolId);
        if (tieneUsuarios)
            throw new InvalidOperationException("No se puede eliminar un rol que tiene usuarios asignados. Desactívalo en su lugar.");

        _dbContext.RolPermisos.RemoveRange(rol.RolPermisos);
        _dbContext.Roles.Remove(rol);
        await _dbContext.SaveChangesAsync();
    }

    public async Task EliminarPermisoAsync(int permisoId)
    {
        var permiso = await _dbContext.Permisos
            .Include(p => p.RolPermisos)
            .FirstOrDefaultAsync(p => p.PermisoId == permisoId);
        if (permiso is null)
            throw new KeyNotFoundException("Permiso no encontrado.");

        _dbContext.RolPermisos.RemoveRange(permiso.RolPermisos);
        _dbContext.Permisos.Remove(permiso);
        await _dbContext.SaveChangesAsync();
    }

    public async Task AsignarPermisoAsync(int rolId, int permisoId)
    {
        var existeAsignacion = await _dbContext.RolPermisos
            .AnyAsync(rp => rp.RolId == rolId && rp.PermisoId == permisoId);
        if (existeAsignacion)
            return;

        var rolExiste = await _dbContext.Roles.AnyAsync(r => r.RolId == rolId);
        if (!rolExiste)
            throw new KeyNotFoundException("Rol no encontrado.");

        var permisoExiste = await _dbContext.Permisos.AnyAsync(p => p.PermisoId == permisoId);
        if (!permisoExiste)
            throw new KeyNotFoundException("Permiso no encontrado.");

        _dbContext.RolPermisos.Add(new RolPermisoTbl { RolId = rolId, PermisoId = permisoId });
        await _dbContext.SaveChangesAsync();
    }

    public async Task QuitarPermisoAsync(int rolId, int permisoId)
    {
        var asignacion = await _dbContext.RolPermisos
            .FirstOrDefaultAsync(rp => rp.RolId == rolId && rp.PermisoId == permisoId);
        if (asignacion is null)
            return;

        _dbContext.RolPermisos.Remove(asignacion);
        await _dbContext.SaveChangesAsync();
    }

    private static string? Limpiar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        return valor.Trim();
    }
}

