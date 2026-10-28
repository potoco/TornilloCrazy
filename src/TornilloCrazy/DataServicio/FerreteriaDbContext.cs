using DataServicio.Tabla;
using Microsoft.EntityFrameworkCore;

namespace DataServicio;


public class FerreteriaDbContext(DbContextOptions<FerreteriaDbContext> options) : DbContext(options)
{
    public DbSet<PersonaTbl> Personas => Set<PersonaTbl>();
    public DbSet<DireccionTbl> Direcciones => Set<DireccionTbl>();
    public DbSet<EmailTbl> Emails => Set<EmailTbl>();
    public DbSet<ProveedorTbl> Proveedores => Set<ProveedorTbl>();
    public DbSet<LisPreProConfiguracionTbl> LisPreProConfiguraciones => Set<LisPreProConfiguracionTbl>();
    public DbSet<LisPreProDataTbl> LisPreProData => Set<LisPreProDataTbl>();
    public DbSet<ListaPrecioProveedorTbl> ListaPrecioProveedores => Set<ListaPrecioProveedorTbl>();
    public DbSet<ProductoTbl> Productos => Set<ProductoTbl>();
    public DbSet<TelefonoTbl> Telefonos => Set<TelefonoTbl>();
    public DbSet<UsuarioTbl> Usuarios => Set<UsuarioTbl>();
    public DbSet<ProveedorMaestroTbl> ProveedorMaestros => Set<ProveedorMaestroTbl>();
    public DbSet<TipoAlteracionStockTbl> TipoAlteracionStocks => Set<TipoAlteracionStockTbl>();
    public DbSet<StockControlTbl> StockControls => Set<StockControlTbl>();
    public DbSet<ProductoCostoTbl> ProductoCostos => Set<ProductoCostoTbl>();
    public DbSet<ProductoImagenTbl> ProductoImagenes => Set<ProductoImagenTbl>();
    public DbSet<RolTbl> Roles => Set<RolTbl>();
    public DbSet<PermisoTbl> Permisos => Set<PermisoTbl>();
    public DbSet<UsuarioRolTbl> UsuarioRoles => Set<UsuarioRolTbl>();
    public DbSet<RolPermisoTbl> RolPermisos => Set<RolPermisoTbl>();
    public DbSet<VentaTbl> Ventas => Set<VentaTbl>();
    public DbSet<ClienteTbl> Clientes => Set<ClienteTbl>();
    public DbSet<VentaStockTbl> VentaStocks => Set<VentaStockTbl>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProveedorTbl>()
            .HasMany(p => p.Rubros)
            .WithMany(r => r.Proveedores)
            .UsingEntity(j => j.ToTable("RubroFerreteriaRelacion"));


        modelBuilder.Entity<TipoAlteracionStockTbl>().HasData(
            new { TipoAlteracionStockId=1, Nombre = "Control de Stock", CodigoAlteracion = "CS", Descripcion = "Es el evento que marca un punto de partida" },
            new { TipoAlteracionStockId = 2, Nombre = "Ingreso de stock", CodigoAlteracion = "II", Descripcion = "Es el evento que marca un ingreso de stock" },
            new { TipoAlteracionStockId = 3, Nombre = "Salida de stock", CodigoAlteracion = "EE", Descripcion = "Es el evento que marca EGRESO o SALIDA del stock" }
        );


        modelBuilder.Entity<RolTbl>().HasData(
            new { RolId = 1, Nombre = "Administrador", Activo=true },
            new { RolId = 2, Nombre = "Vendedor", Activo=true },
            new { RolId = 3, Nombre = "Consulta", Activo=true },
            new { RolId = 4, Nombre = "Supervisor", Activo=true }
        );

        modelBuilder.Entity<PermisoTbl>().HasData(
        new { PermisoId = 1, Codigo = "PRODUCTOS.VER",      Nombre="PRODUCTOS.VER"   , Activo=true   },
        new { PermisoId = 2, Codigo = "PRODUCTOS.EDITAR",   Nombre="PRODUCTOS.EDITAR"   , Activo=true },
        new { PermisoId = 3, Codigo = "PRODUCTOS.ELIMINAR", Nombre="PRODUCTOS.ELIMINAR" , Activo=true },
        new { PermisoId = 4, Codigo = "CLIENTES.VER",       Nombre="CLIENTES.VER"       , Activo=true },
        new { PermisoId = 5, Codigo = "CLIENTES.EDITAR",    Nombre="CLIENTES.EDITAR"    , Activo=true },
        new { PermisoId = 6, Codigo = "USUARIOS.VER",       Nombre="USUARIOS.VER"       , Activo=true },
        new { PermisoId = 7, Codigo = "USUARIOS.EDITAR",    Nombre= "USUARIOS.EDITAR" , Activo=true });

        modelBuilder.Entity<RolPermisoTbl>().HasData(
            new { RolId = 1, PermisoId = 1, Activo=true },
            new { RolId = 1, PermisoId = 2, Activo=true },
            new { RolId = 1, PermisoId = 3, Activo=true },
            new { RolId = 1, PermisoId = 4, Activo=true },
            new { RolId = 1, PermisoId = 5, Activo=true },
            new { RolId = 1, PermisoId = 6, Activo=true },
            new { RolId = 1, PermisoId = 7, Activo=true },
            new { RolId = 2, PermisoId = 1, Activo=true },
            new { RolId = 2, PermisoId = 2, Activo=true },
            new { RolId = 2, PermisoId = 4, Activo=true },
            new { RolId = 2, PermisoId = 5, Activo=true },
            new { RolId = 3, PermisoId = 1, Activo=true },
            new { RolId = 3, PermisoId = 4, Activo=true }
        );


        modelBuilder.Entity<UsuarioRolTbl>().HasData(
            new { RolId = 1, UsuarioId = 1 }
        );


        ConfigurarRol(modelBuilder);
        ConfigurarPermiso(modelBuilder);
        ConfigurarUsuarioRol(modelBuilder);
        ConfigurarRolPermiso(modelBuilder);
        ConfigurarVentaStock(modelBuilder);


    }

    private static void ConfigurarRol(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolTbl>(entity =>
        {
            entity.HasKey(x => x.RolId);

            entity.Property(x => x.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Descripcion)
                .HasMaxLength(300);

            entity.HasIndex(x => x.Nombre)
                .IsUnique();
        });
    }
    private static void ConfigurarPermiso(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PermisoTbl>(entity =>
        {
            entity.HasKey(x => x.PermisoId);

            entity.Property(x => x.Codigo)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Nombre)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Descripcion)
                .HasMaxLength(300);

            entity.HasIndex(x => x.Codigo)
                .IsUnique();
        });
    }
    private static void ConfigurarUsuarioRol(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UsuarioRolTbl>(entity =>
        {
            entity.HasKey(x => new
            {
                x.UsuarioId,
                x.RolId
            });

            entity.HasOne(x => x.Usuario)
                .WithMany(x => x.UsuarioRoles)
                .HasForeignKey(x => x.UsuarioId);

            entity.HasOne(x => x.Rol)
                .WithMany(x => x.UsuarioRoles)
                .HasForeignKey(x => x.RolId);
        });
    }
    private static void ConfigurarRolPermiso(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolPermisoTbl>(entity =>
        {
            entity.HasKey(x => new
            {
                x.RolId,
                x.PermisoId
            });

            entity.HasOne(x => x.Rol)
                .WithMany(x => x.RolPermisos)
                .HasForeignKey(x => x.RolId);

            entity.HasOne(x => x.Permiso)
                .WithMany(x => x.RolPermisos)
                .HasForeignKey(x => x.PermisoId);
        });
    }
    private static void ConfigurarVentaStock(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VentaStockTbl>(entity =>
        {
            entity.HasKey(x => new
            {
                x.VentaId,
                x.StockControlId
            });

            entity.HasOne(x => x.Ventas)
                .WithMany(x => x.VentaStocks)
                .HasForeignKey(x => x.VentaId);

            entity.HasOne(x => x.StockControl)
                .WithMany(x => x.VentaStocks)
                .HasForeignKey(x => x.StockControlId);
        });
    }

}
