using DataServicio.Servicio;
using System.Globalization;

namespace TornilloWeb.Servicio;

public class LaburandoService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    public LaburandoService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var formato = "dd/MM/yyyy";
        var cultura = CultureInfo.InvariantCulture;
        var tarea1 = ReActivar(ct => ProcesarListaPrecio(formato, cultura, ct), stoppingToken);
        await Task.WhenAll(tarea1);
    }

    private async Task ReActivar(Func<CancellationToken, Task> revivir, CancellationToken ct)
    {

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await revivir(ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            await Task.Delay(5000, ct);
        }
    }

    private async Task ProcesarListaPrecio(string formato, CultureInfo cultura, CancellationToken ct)
    {
        int contador = 0;
        while (!ct.IsCancellationRequested)
        {
            using var sco = _serviceProvider.CreateAsyncScope();
            var procesarListaPrecioServicio = sco.ServiceProvider.GetRequiredService<ProcesarListaPrecioServicio>();
            var proveedorServicio = sco.ServiceProvider.GetRequiredService<ProveedorService>();

            contador++;
            if(contador == 3)
            {
                contador = 0;
                throw new Exception("El contador ha llegado a 3");
            }
            await Task.Delay(3000, ct); // Simulación de trabajo
        }
    }

}
