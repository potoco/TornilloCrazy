using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using DataServicio.Modelo;
using DataServicio.Servicio;

namespace TornilloWeb.Pages.Admin
{
    public class ProveedorCrudModel : PageModel
    {
        public sealed class ProveedorForm
        {
            [Required, StringLength(19)]
            public string CUIT { get; set; } = string.Empty;

            [Required, StringLength(130)]
            public string RazonSocial { get; set; } = string.Empty;
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

        private readonly ProveedorService _proveedorService;
        private readonly PersonaService _personaService;

        public ProveedorCrudModel(ProveedorService proveedorService, PersonaService personaService)
        {
            _proveedorService = proveedorService;
            _personaService = personaService;
        }

        [BindProperty(SupportsGet = true)]
        public int? ProveedorId { get; set; }

        [BindProperty]
        public ProveedorForm ProveedorInput { get; set; } = new();

        [BindProperty]
        public DireccionForm DireccionInput { get; set; } = new();

        [BindProperty]
        public TelefonoForm TelefonoInput { get; set; } = new();

        [BindProperty]
        public EmailForm EmailInput { get; set; } = new();

        public List<ProveedorDto> Proveedores { get; private set; } = [];
        public PersonaService.ProveedorPersonaDetalle? ProveedorSeleccionado { get; private set; }
        public bool EsEdicion => ProveedorSeleccionado is not null;

        [TempData]
        public string? MensajeOk { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public async Task OnGetAsync()
        {
            await CargarPaginaAsync(ProveedorId);
        }

        public async Task<IActionResult> OnPostCrearAsync()
        {
            ModelState.Clear();
            if (!TryValidateModel(ProveedorInput, nameof(ProveedorInput)))
            {
                await CargarPaginaAsync(null);
                return Page();
            }

            var creado = await _proveedorService.CrearProveedorAsync(ProveedorInput.CUIT, ProveedorInput.RazonSocial);
            MensajeOk = "Proveedor creado correctamente.";
            return RedirectToPage(new { proveedorId = creado.ProveedorId });
        }

        public async Task<IActionResult> OnPostActualizarAsync(int proveedorId, int personaId)
        {
            ModelState.Clear();
            if (!TryValidateModel(ProveedorInput, nameof(ProveedorInput)))
            {
                await CargarPaginaAsync(proveedorId);
                return Page();
            }

            await _personaService.ActualizarDatosPersonaAsync(personaId, ProveedorInput.CUIT, ProveedorInput.RazonSocial);
            MensajeOk = "Proveedor actualizado correctamente.";
            return RedirectToPage(new { proveedorId });
        }

        public async Task<IActionResult> OnPostEliminarProveedorAsync(int proveedorId)
        {
            await _proveedorService.BajaLogicaProveedorAsync(proveedorId);
            MensajeOk = "Proveedor desactivado correctamente.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAgregarDireccionAsync(int proveedorId, int personaId)
        {
            ModelState.Clear();
            if (!TryValidateModel(DireccionInput, nameof(DireccionInput)))
            {
                await CargarPaginaAsync(proveedorId);
                return Page();
            }

            await _personaService.AgregarDireccionAsync(personaId, DireccionInput.Calle, DireccionInput.Numero, DireccionInput.Piso, DireccionInput.Depto);
            MensajeOk = "Dirección agregada correctamente.";
            return RedirectToPage(new { proveedorId });
        }

        public async Task<IActionResult> OnPostActualizarDireccionAsync(int proveedorId, int personaId, int direccionId, string calle, string? numero, string? piso, string? depto)
        {
            if (string.IsNullOrWhiteSpace(calle))
            {
                MensajeError = "La calle es obligatoria.";
                await CargarPaginaAsync(proveedorId);
                return Page();
            }

            await _personaService.ActualizarDireccionAsync(personaId, direccionId, calle, numero, piso, depto);
            MensajeOk = "Dirección actualizada correctamente.";
            return RedirectToPage(new { proveedorId });
        }

        public async Task<IActionResult> OnPostEliminarDireccionAsync(int proveedorId, int personaId, int direccionId)
        {
            await _personaService.EliminarDireccionAsync(personaId, direccionId);
            MensajeOk = "Dirección eliminada correctamente.";
            return RedirectToPage(new { proveedorId });
        }

        public async Task<IActionResult> OnPostAgregarTelefonoAsync(int proveedorId, int personaId)
        {
            ModelState.Clear();
            if (!TryValidateModel(TelefonoInput, nameof(TelefonoInput)))
            {
                await CargarPaginaAsync(proveedorId);
                return Page();
            }

            await _personaService.AgregarTelefonoAsync(personaId, TelefonoInput.Telefono);
            MensajeOk = "Teléfono agregado correctamente.";
            return RedirectToPage(new { proveedorId });
        }

        public async Task<IActionResult> OnPostActualizarTelefonoAsync(int proveedorId, int personaId, int telefonoId, string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                MensajeError = "El teléfono es obligatorio.";
                await CargarPaginaAsync(proveedorId);
                return Page();
            }

            await _personaService.ActualizarTelefonoAsync(personaId, telefonoId, telefono);
            MensajeOk = "Teléfono actualizado correctamente.";
            return RedirectToPage(new { proveedorId });
        }

        public async Task<IActionResult> OnPostEliminarTelefonoAsync(int proveedorId, int personaId, int telefonoId)
        {
            await _personaService.EliminarTelefonoAsync(personaId, telefonoId);
            MensajeOk = "Teléfono eliminado correctamente.";
            return RedirectToPage(new { proveedorId });
        }

        public async Task<IActionResult> OnPostAgregarEmailAsync(int proveedorId, int personaId)
        {
            ModelState.Clear();
            if (!TryValidateModel(EmailInput, nameof(EmailInput)))
            {
                await CargarPaginaAsync(proveedorId);
                return Page();
            }

            await _personaService.AgregarEmailAsync(personaId, EmailInput.Email);
            MensajeOk = "Email agregado correctamente.";
            return RedirectToPage(new { proveedorId });
        }

        public async Task<IActionResult> OnPostActualizarEmailAsync(int proveedorId, int personaId, int emailId, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                MensajeError = "El email es obligatorio.";
                await CargarPaginaAsync(proveedorId);
                return Page();
            }

            await _personaService.ActualizarEmailAsync(personaId, emailId, email);
            MensajeOk = "Email actualizado correctamente.";
            return RedirectToPage(new { proveedorId });
        }

        public async Task<IActionResult> OnPostEliminarEmailAsync(int proveedorId, int personaId, int emailId)
        {
            await _personaService.EliminarEmailAsync(personaId, emailId);
            MensajeOk = "Email eliminado correctamente.";
            return RedirectToPage(new { proveedorId });
        }

        private async Task CargarPaginaAsync(int? proveedorId)
        {
            Proveedores = await _proveedorService.ObtenerProveedoresAsync();
            ProveedorId = proveedorId;

            if (proveedorId.HasValue)
            {
                ProveedorSeleccionado = await _personaService.ObtenerDetalleProveedorAsync(proveedorId.Value);
                if (ProveedorSeleccionado is not null)
                {
                    ProveedorInput = new ProveedorForm
                    {
                        CUIT = ProveedorSeleccionado.Cuit ?? string.Empty,
                        RazonSocial = ProveedorSeleccionado.Nombre
                    };
                }
            }
        }
    }
}
