using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataServicio.Tabla;

[Table("Usuario")]
public class UsuarioTbl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UsuarioId { get; set; }
    [MaxLength(15)] public string LoginUser { get; set; } = "";
    [MaxLength(15)] public string LoginPwd { get; set; } = "";
    public int Estado { get; set; } = 0;
    public int PersonaId { get; set; }
    public PersonaTbl Persona { get; set; } = new();
}
