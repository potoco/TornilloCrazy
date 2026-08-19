using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataServicio.Tabla;

[Table("Persona")]
public class PersonaTbl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PersonaId { get; set; }
    [MaxLength(30)] public string Nombre { get; set; } = "";
    [MaxLength(12)] public string? ApePaterno { get; set; }
    [MaxLength(12)] public string? ApeMaterno { get; set; }
    [MaxLength(15)] public string? DNI { get; set; }
    [MaxLength(19)] public string? CUIT { get; set; }
    public int Estado { get; set; } = 0;
    public ICollection<EmailTbl>? Emails { get; set; }
    public ICollection<TelefonoTbl>? Telefonos { get; set; }
    public ICollection<DireccionTbl>? Direcciones { get; set; }
    [NotMapped] public string FullName { get => $"{Nombre} {ApePaterno} {ApeMaterno}"; }
    [NotMapped] public string FullName2 { get => $"{ApePaterno}, {Nombre}"; }
}
