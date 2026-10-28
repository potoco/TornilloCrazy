namespace DataServicio.Modelo;

public sealed record ClienteDto(int ClienteId, int PersonaId, string? CUIT, string RazonSocial, decimal DescuentoPorcentaje = 0);
