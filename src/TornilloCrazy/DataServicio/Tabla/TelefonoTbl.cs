using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataServicio.Tabla;

[Table("Telefono")]
public class TelefonoTbl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TelefonoId { get; set; }
    [MaxLength(20)] public string Telefono { get; set; } = "";
    public int Estado { get; set; } = 0;
    public int PersonaId { get; set; }
    public PersonaTbl Persona { get; set; } = null!;

}
