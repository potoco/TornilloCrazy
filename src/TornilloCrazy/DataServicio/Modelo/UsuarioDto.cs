namespace DataServicio.Modelo;

public sealed record UsuarioDto(
    int UsuarioId,
    int PersonaId,
    string LoginUser,
    string Nombre,
    string? ApePaterno,
    string? ApeMaterno,
    int Estado,
    IReadOnlyList<string> Roles);

public sealed record RolDto(int RolId, string Nombre);
