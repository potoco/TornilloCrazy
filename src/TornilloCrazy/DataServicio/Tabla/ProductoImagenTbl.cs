using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataServicio.Tabla;

[Table("ProductoImagen")]
public class ProductoImagenTbl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductoImagenId { get; set; }
    public int ProductoId { get; set; }
    [MaxLength(200)] public string NombreArchivo { get; set; } = "";
    public bool EsPrincipal { get; set; } = false;
    [ForeignKey("ProductoId")]
    public virtual ProductoTbl? Producto { get; set; }
}