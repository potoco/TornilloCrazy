namespace DataServicio.Modelo;

public sealed record ProveedorDto(int ProveedorId, int PersonaId, string? CUIT, string RazonSocial);
