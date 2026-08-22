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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProveedorTbl>()
            .HasMany(p => p.Rubros)
            .WithMany(r => r.Proveedores)
            .UsingEntity(j => j.ToTable("RubroFerreteriaRelacion"));
    }


}
