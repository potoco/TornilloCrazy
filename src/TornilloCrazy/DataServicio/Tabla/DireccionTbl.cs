using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataServicio.Tabla;

[Table("Direccion")]
public class DireccionTbl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int DireccionId { get; set; }

    [MaxLength(25)]
    public string Calle { get; set; } = "";

    [MaxLength(10)]
    public string? Numero { get; set; }

    [MaxLength(10)]
    public string? Piso { get; set; }

    [MaxLength(10)]
    public string? Depto { get; set; }

    public int PersonaId { get; set; }
    public PersonaTbl Persona { get; set; } = null!;

}
