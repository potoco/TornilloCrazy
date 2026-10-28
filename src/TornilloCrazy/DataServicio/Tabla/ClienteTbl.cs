using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataServicio.Tabla;

[Table("Cliente")]
public class ClienteTbl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ClienteId { get; set; }
    public int Estado { get; set; } = 0;
    public decimal DescuentoPorcentaje { get; set; } = 0;
    public int PersonaId { get; set; }
    public PersonaTbl Persona { get; set; } = null!;
}