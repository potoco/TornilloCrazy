using DataServicio;
using DataServicio.Servicio;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using TornilloWeb.Servicio;

const string errorLogTemplate =
    "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] ({SourceContext}){NewLine}" +
    "Mensaje: {Message:lj}{NewLine}" +
    "{Exception}" +
    "--------------------------------------------------------------------------------{NewLine}";

string ObtenerRutaEscrituraDisponible(params string[] rutas)
{
    foreach (var ruta in rutas.Where(r => !string.IsNullOrWhiteSpace(r)))
    {
        try
        {
            Directory.CreateDirectory(ruta);
            var archivoPrueba = Path.Combine(ruta, $"permiso-{Guid.NewGuid():N}.tmp");
            File.WriteAllText(archivoPrueba, "ok");
            File.Delete(archivoPrueba);
            return ruta;
        }
        catch
        {
        }
    }

    return Path.GetTempPath();
}

var bootstrapPath = ObtenerRutaEscrituraDisponible(
    Path.Combine(AppContext.BaseDirectory, "AppData", "Logs"),
    Path.Combine(Path.GetTempPath(), "TornilloWeb", "AppData", "Logs"));

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(bootstrapPath, "bootstrap-.log"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: errorLogTemplate,
        retainedFileCountLimit: 15)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        var appDataLogsPath = ObtenerRutaEscrituraDisponible(
            Path.Combine(context.HostingEnvironment.ContentRootPath, "AppData", "Logs"),
            Path.Combine(Path.GetTempPath(), "TornilloWeb", "AppData", "Logs"),
            bootstrapPath);

        var classLogsRootPath = ObtenerRutaEscrituraDisponible(
            Path.Combine(context.HostingEnvironment.ContentRootPath, "Logs"),
            Path.Combine(Path.GetTempPath(), "TornilloWeb", "Logs"),
            bootstrapPath);

        configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                path: Path.Combine(appDataLogsPath, "general-.log"),
                restrictedToMinimumLevel: LogEventLevel.Warning,
                rollingInterval: RollingInterval.Day,
                outputTemplate: errorLogTemplate,
                retainedFileCountLimit: 30)
            .WriteTo.Map(
                keyPropertyName: "SourceContext",
                defaultKey: "General",
                configure: (sourceContext, wt) =>
                {
                    var className = sourceContext?.Split('.').Last() ?? "General";
                    var safeClassName = string.Concat(className.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
                    if (string.IsNullOrWhiteSpace(safeClassName))
                    {
                        safeClassName = "General";
                    }

                    wt.File(
                        path: Path.Combine(classLogsRootPath, safeClassName, "log-.txt"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: errorLogTemplate,
                        retainedFileCountLimit: 15);
                },
                sinkMapCountLimit: 30
            );
    });

    builder.Services
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Login";
            options.AccessDeniedPath = "/Login";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });

    builder.Services.AddAuthorization();

    builder.Services.AddRazorPages(options =>
    {
        options.Conventions.AuthorizeFolder("/Admin");
        options.Conventions.AllowAnonymousToPage("/Login");
    });
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddDbContext<FerreteriaDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddScoped<ProveedorService>();
    builder.Services.AddScoped<PersonaService>();
    builder.Services.AddScoped<ProductoService>();
    builder.Services.AddScoped<ProcesarListaPrecioServicio>();
    builder.Services.AddScoped<InstruccionesTextoHelper>();
    builder.Services.AddSingleton<ClienteWebPotoco>();
    builder.Services.AddHostedService<LaburandoService>();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapStaticAssets();
    app.MapRazorPages().WithStaticAssets();
    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó por una excepción no controlada.");
}
finally
{
    Log.CloseAndFlush();
}
