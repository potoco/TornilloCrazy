using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DataServicio.Tabla;


[Table("Proveedor")]
public class ProveedorTbl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProveedorId { get; set; }
    public int Estado { get; set; } = 0;
    public int PersonaId { get; set; }
    public PersonaTbl Persona { get; set; } = null!;
    public string? Observaciones { get; set; }
    public ICollection<RubroFerreteriaTbl> Rubros { get; set; } = new List<RubroFerreteriaTbl>();
}

[Table("RubroFerreteria")]
public class RubroFerreteriaTbl
{
    [Key]
    [Required]
    public int RubroFerreteriaId { get; set; }
    public string Titulo { get; set; }=string.Empty;
    public ICollection<ProveedorTbl> Proveedores { get; set; } = new List<ProveedorTbl>();
}

