using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using DataServicio.Modelo;
using DataServicio.Servicio;

namespace TornilloWeb.Pages.Admin.Sistema;

public class ClienteEditModel : PageModel
{
    public sealed class ClienteForm : IValidatableObject
    {
        [Required, StringLength(19)]
        public string CUIT { get; set; } = string.Empty;

        [Required, StringLength(130)]
        [Display(Name = "Nombre / Razón social")]
        public string RazonSocial { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "100"), Display(Name = "Descuento (%)")]
        public decimal DescuentoPorcentaje { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (decimal.Round(DescuentoPorcentaje, 2) != DescuentoPorcentaje)
                yield return new ValidationResult("El descuento admite hasta dos decimales.", [nameof(DescuentoPorcentaje)]);
        }
    }

    public sealed class DireccionForm
    {
        [Required, StringLength(25)]
        public string Calle { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Numero { get; set; }

        [StringLength(10)]
        public string? Piso { get; set; }

        [StringLength(10)]
        public string? Depto { get; set; }
    }

    public sealed class TelefonoForm
    {
        [Required, StringLength(20)]
        public string Telefono { get; set; } = string.Empty;
    }

    public sealed class EmailForm
    {
        [Required, StringLength(90), EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    private readonly ClienteService _clienteService;
    private readonly PersonaService _personaService;

    public ClienteEditModel(ClienteService clienteService, PersonaService personaService)
    {
        _clienteService = clienteService;
        _personaService = personaService;
    }

    [BindProperty(SupportsGet = true)]
    public int? ClienteId { get; set; }

    [BindProperty]
    public ClienteForm ClienteInput { get; set; } = new();

    [BindProperty]
    public DireccionForm DireccionInput { get; set; } = new();

    [BindProperty]
    public TelefonoForm TelefonoInput { get; set; } = new();

    [BindProperty]
    public EmailForm EmailInput { get; set; } = new();

    public List<ClienteDto> Clientes { get; private set; } = [];
    public ClienteService.ClientePersonaDetalle? ClienteSeleccionado { get; private set; }
    public bool EsEdicion => ClienteSeleccionado is not null;

    [TempData]
    public string? MensajeOk { get; set; }

    [TempData]
    public string? MensajeError { get; set; }

    public async Task OnGetAsync()
    {
        await CargarPaginaAsync(ClienteId);
    }

    public async Task<IActionResult> OnPostCrearAsync()
    {
        ModelState.Clear();
        if (!TryValidateModel(ClienteInput, nameof(ClienteInput)))
        {
            await CargarPaginaAsync(null);
            return Page();
        }

        var creado = await _clienteService.CrearClienteAsync(ClienteInput.CUIT, ClienteInput.RazonSocial, ClienteInput.DescuentoPorcentaje);
        MensajeOk = "Cliente creado correctamente.";
        return RedirectToPage(new { clienteId = creado.ClienteId });
    }

    public async Task<IActionResult> OnPostActualizarAsync(int clienteId)
    {
        var cliente = await _clienteService.ObtenerDetalleClienteAsync(clienteId);
        if (cliente is null) return NotFound();
        var personaId = cliente.PersonaId;
        ModelState.Clear();
        if (!TryValidateModel(ClienteInput, nameof(ClienteInput)))
        {
            await CargarPaginaAsync(clienteId, cargarInput: false);
            return Page();
        }

        await _clienteService.ActualizarClienteAsync(clienteId, ClienteInput.CUIT, ClienteInput.RazonSocial, ClienteInput.DescuentoPorcentaje);
        MensajeOk = "Cliente actualizado correctamente.";
        return RedirectToPage(new { clienteId });
    }

    public async Task<IActionResult> OnPostEliminarClienteAsync(int clienteId)
    {
        if (await _clienteService.ObtenerClientePorIdAsync(clienteId) is null) return NotFound();
        await _clienteService.BajaLogicaClienteAsync(clienteId);
        MensajeOk = "Cliente desactivado correctamente.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAgregarDireccionAsync(int clienteId)
    {
        var cliente = await _clienteService.ObtenerDetalleClienteAsync(clienteId);
        if (cliente is null) return NotFound();
        var personaId = cliente.PersonaId;
        ModelState.Clear();
        if (!TryValidateModel(DireccionInput, nameof(DireccionInput)))
        {
            await CargarPaginaAsync(clienteId);
            return Page();
        }

        await _personaService.AgregarDireccionAsync(personaId, DireccionInput.Calle, DireccionInput.Numero, DireccionInput.Piso, DireccionInput.Depto);
        MensajeOk = "Dirección agregada correctamente.";
        return RedirectToPage(new { clienteId });
    }

    public async Task<IActionResult> OnPostActualizarDireccionAsync(int clienteId, int direccionId, string calle, string? numero, string? piso, string? depto)
    {
        var cliente = await _clienteService.ObtenerDetalleClienteAsync(clienteId);
        if (cliente is null) return NotFound();
        var personaId = cliente.PersonaId;
        ModelState.Clear();
        if (!TryValidateModel(new DireccionForm { Calle = calle, Numero = numero, Piso = piso, Depto = depto }, nameof(DireccionInput)))
        {
            MensajeError = "Revise los datos de la dirección.";
            await CargarPaginaAsync(clienteId);
            return Page();
        }

        await _personaService.ActualizarDireccionAsync(personaId, direccionId, calle, numero, piso, depto);
        MensajeOk = "Dirección actualizada correctamente.";
        return RedirectToPage(new { clienteId });
    }

    public async Task<IActionResult> OnPostEliminarDireccionAsync(int clienteId, int direccionId)
    {
        var cliente = await _clienteService.ObtenerDetalleClienteAsync(clienteId);
        if (cliente is null) return NotFound();
        var personaId = cliente.PersonaId;
        await _personaService.EliminarDireccionAsync(personaId, direccionId);
        MensajeOk = "Dirección eliminada correctamente.";
        return RedirectToPage(new { clienteId });
    }

    public async Task<IActionResult> OnPostAgregarTelefonoAsync(int clienteId)
    {
        var cliente = await _clienteService.ObtenerDetalleClienteAsync(clienteId);
        if (cliente is null) return NotFound();
        var personaId = cliente.PersonaId;
        ModelState.Clear();
        if (!TryValidateModel(TelefonoInput, nameof(TelefonoInput)))
        {
            await CargarPaginaAsync(clienteId);
            return Page();
        }

        await _personaService.AgregarTelefonoAsync(personaId, TelefonoInput.Telefono);
        MensajeOk = "Teléfono agregado correctamente.";
        return RedirectToPage(new { clienteId });
    }

    public async Task<IActionResult> OnPostActualizarTelefonoAsync(int clienteId, int telefonoId, string telefono)
    {
        var cliente = await _clienteService.ObtenerDetalleClienteAsync(clienteId);
        if (cliente is null) return NotFound();
        var personaId = cliente.PersonaId;
        ModelState.Clear();
        if (!TryValidateModel(new TelefonoForm { Telefono = telefono }, nameof(TelefonoInput)))
        {
            MensajeError = "Revise el teléfono ingresado.";
            await CargarPaginaAsync(clienteId);
            return Page();
        }

        await _personaService.ActualizarTelefonoAsync(personaId, telefonoId, telefono);
        MensajeOk = "Teléfono actualizado correctamente.";
        return RedirectToPage(new { clienteId });
    }

    public async Task<IActionResult> OnPostEliminarTelefonoAsync(int clienteId, int telefonoId)
    {
        var cliente = await _clienteService.ObtenerDetalleClienteAsync(clienteId);
        if (cliente is null) return NotFound();
        var personaId = cliente.PersonaId;
        await _personaService.EliminarTelefonoAsync(personaId, telefonoId);
        MensajeOk = "Teléfono eliminado correctamente.";
        return RedirectToPage(new { clienteId });
    }

    public async Task<IActionResult> OnPostAgregarEmailAsync(int clienteId)
    {
        var cliente = await _clienteService.ObtenerDetalleClienteAsync(clienteId);
        if (cliente is null) return NotFound();
        var personaId = cliente.PersonaId;
        ModelState.Clear();
        if (!TryValidateModel(EmailInput, nameof(EmailInput)))
        {
            await CargarPaginaAsync(clienteId);
            return Page();
        }

        await _personaService.AgregarEmailAsync(personaId, EmailInput.Email);
        MensajeOk = "Email agregado correctamente.";
        return RedirectToPage(new { clienteId });
    }

    public async Task<IActionResult> OnPostActualizarEmailAsync(int clienteId, int emailId, string email)
    {
        var cliente = await _clienteService.ObtenerDetalleClienteAsync(clienteId);
        if (cliente is null) return NotFound();
        var personaId = cliente.PersonaId;
        ModelState.Clear();
        if (!TryValidateModel(new EmailForm { Email = email }, nameof(EmailInput)))
        {
            MensajeError = "Revise el email ingresado.";
            await CargarPaginaAsync(clienteId);
            return Page();
        }

        await _personaService.ActualizarEmailAsync(personaId, emailId, email);
        MensajeOk = "Email actualizado correctamente.";
        return RedirectToPage(new { clienteId });
    }

    public async Task<IActionResult> OnPostEliminarEmailAsync(int clienteId, int emailId)
    {
        var cliente = await _clienteService.ObtenerDetalleClienteAsync(clienteId);
        if (cliente is null) return NotFound();
        var personaId = cliente.PersonaId;
        await _personaService.EliminarEmailAsync(personaId, emailId);
        MensajeOk = "Email eliminado correctamente.";
        return RedirectToPage(new { clienteId });
    }

    private async Task CargarPaginaAsync(int? clienteId, bool cargarInput = true)
    {
        Clientes = await _clienteService.ObtenerClientesAsync();
        ClienteId = clienteId;

        if (clienteId.HasValue)
        {
            ClienteSeleccionado = await _clienteService.ObtenerDetalleClienteAsync(clienteId.Value);
            if (ClienteSeleccionado is not null && cargarInput)
            {
                ClienteInput = new ClienteForm
                {
                    CUIT = ClienteSeleccionado.Cuit ?? string.Empty,
                    RazonSocial = ClienteSeleccionado.Nombre,
                    DescuentoPorcentaje = ClienteSeleccionado.DescuentoPorcentaje
                };
            }
        }
    }
}
