using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataServicio.Tabla;

[Table("Usuario")]
public class UsuarioTbl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UsuarioId { get; set; }
    [MaxLength(15)] public required string LoginUser { get; set; } = "";
    [MaxLength(15)] public required string LoginPwd { get; set; } = "";
    public int Estado { get; set; } = 0;
    public int PersonaId { get; set; }
    public PersonaTbl Persona { get; set; } = null!;
    public ICollection<UsuarioRolTbl> UsuarioRoles { get; set; }
        = new List<UsuarioRolTbl>();
}

[Table("Rol")]
public class RolTbl
{
    public int RolId { get; set; }

    public required string Nombre { get; set; }

    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;


    // Navegación
    public ICollection<UsuarioRolTbl> UsuarioRoles { get; set; }
        = new List<UsuarioRolTbl>();

    public ICollection<RolPermisoTbl> RolPermisos { get; set; }
        = new List<RolPermisoTbl>();
}

[Table("Permiso")]
public class PermisoTbl
{
    public int PermisoId { get; set; }

    public required string Codigo { get; set; }

    public required string Nombre { get; set; }

    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;


    // Navegación
    public ICollection<RolPermisoTbl> RolPermisos { get; set; }
        = new List<RolPermisoTbl>();
}


[Table("UsuarioRol")]
public class UsuarioRolTbl
{
    public int UsuarioId { get; set; }

    public int RolId { get; set; }


    // Navegación
    public UsuarioTbl Usuario { get; set; } = null!;

    public RolTbl Rol { get; set; } = null!;
}

[Table("RolPermiso")]
public class RolPermisoTbl
{
    public int RolId { get; set; }

    public int PermisoId { get; set; }


    // Navegación
    public RolTbl Rol { get; set; } = null!;

    public PermisoTbl Permiso { get; set; } = null!;
}
