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

 


    }


}
