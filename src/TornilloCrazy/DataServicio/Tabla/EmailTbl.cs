using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataServicio.Tabla;

[Table("Email")]
public class EmailTbl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EmailId { get; set; }
    [MaxLength(90)]
    public string Email { get; set; } = "";
    public int Estado { get; set; } = 0;
    public int PersonaId { get; set; }
    public PersonaTbl Persona { get; set; } = null!;
}
