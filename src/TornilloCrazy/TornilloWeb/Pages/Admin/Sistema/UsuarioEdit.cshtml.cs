using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using DataServicio.Modelo;
using DataServicio.Servicio;

namespace TornilloWeb.Pages.Admin.Sistema
{
    public class UsuarioEditModel : PageModel
    {
        public sealed class UsuarioForm
        {
            [Required, StringLength(15)]
            [Display(Name = "Usuario")]
            public string LoginUser { get; set; } = string.Empty;

            [StringLength(15)]
            [Display(Name = "Contraseña")]
            public string? LoginPwd { get; set; }

            [Required, StringLength(130)]
            public string Nombre { get; set; } = string.Empty;

            [StringLength(25)]
            [Display(Name = "Apellido paterno")]
            public string? ApePaterno { get; set; }

            [StringLength(25)]
            [Display(Name = "Apellido materno")]
            public string? ApeMaterno { get; set; }

            [StringLength(15)]
            public string? DNI { get; set; }

            [StringLength(30)]
            public string? CUIT { get; set; }
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

        private readonly UsuarioService _usuarioService;
        private readonly PersonaService _personaService;

        public UsuarioEditModel(UsuarioService usuarioService, PersonaService personaService)
        {
            _usuarioService = usuarioService;
            _personaService = personaService;
        }

        [BindProperty(SupportsGet = true)]
        public int? UsuarioId { get; set; }

        [BindProperty]
        public UsuarioForm UsuarioInput { get; set; } = new();

        [BindProperty]
        public DireccionForm DireccionInput { get; set; } = new();

        [BindProperty]
        public TelefonoForm TelefonoInput { get; set; } = new();

        [BindProperty]
        public EmailForm EmailInput { get; set; } = new();

        public List<UsuarioDto> Usuarios { get; private set; } = [];
        public UsuarioService.UsuarioDetalle? UsuarioSeleccionado { get; private set; }
        public List<RolDto> RolesDisponibles { get; private set; } = [];
        public bool EsEdicion => UsuarioSeleccionado is not null;

        [TempData]
        public string? MensajeOk { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public async Task OnGetAsync()
        {
            await CargarPaginaAsync(UsuarioId);
        }

        public async Task<IActionResult> OnPostCrearAsync()
        {
            ModelState.Clear();
            if (!TryValidateModel(UsuarioInput, nameof(UsuarioInput)))
            {
                await CargarPaginaAsync(null);
                return Page();
            }

            if (string.IsNullOrWhiteSpace(UsuarioInput.LoginPwd))
            {
                MensajeError = "La contraseña es obligatoria para crear un usuario.";
                await CargarPaginaAsync(null);
                return Page();
            }

            try
            {
                var creado = await _usuarioService.CrearUsuarioAsync(
                    UsuarioInput.LoginUser,
                    UsuarioInput.LoginPwd,
                    UsuarioInput.Nombre,
                    UsuarioInput.ApePaterno,
                    UsuarioInput.ApeMaterno,
                    UsuarioInput.DNI,
                    UsuarioInput.CUIT);

                MensajeOk = "Usuario creado correctamente.";
                return RedirectToPage(new { usuarioId = creado.UsuarioId });
            }
            catch (InvalidOperationException ex)
            {
                MensajeError = ex.Message;
                await CargarPaginaAsync(null);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostActualizarAsync(int usuarioId)
        {
            ModelState.Clear();
            if (!TryValidateModel(UsuarioInput, nameof(UsuarioInput)))
            {
                await CargarPaginaAsync(usuarioId);
                return Page();
            }

            try
            {
                await _usuarioService.ActualizarUsuarioAsync(
                    usuarioId,
                    UsuarioInput.LoginUser,
                    UsuarioInput.LoginPwd,
                    UsuarioInput.Nombre,
                    UsuarioInput.ApePaterno,
                    UsuarioInput.ApeMaterno,
                    UsuarioInput.DNI,
                    UsuarioInput.CUIT);

                MensajeOk = "Usuario actualizado correctamente.";
                return RedirectToPage(new { usuarioId });
            }
            catch (InvalidOperationException ex)
            {
                MensajeError = ex.Message;
                await CargarPaginaAsync(usuarioId);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostEliminarUsuarioAsync(int usuarioId)
        {
            await _usuarioService.BajaLogicaUsuarioAsync(usuarioId);
            MensajeOk = "Usuario desactivado correctamente.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAsignarRolAsync(int usuarioId, int rolId)
        {
            await _usuarioService.AsignarRolAsync(usuarioId, rolId);
            MensajeOk = "Rol asignado correctamente.";
            return RedirectToPage(new { usuarioId });
        }

        public async Task<IActionResult> OnPostQuitarRolAsync(int usuarioId, int rolId)
        {
            await _usuarioService.QuitarRolAsync(usuarioId, rolId);
            MensajeOk = "Rol quitado correctamente.";
            return RedirectToPage(new { usuarioId });
        }

        public async Task<IActionResult> OnPostAgregarDireccionAsync(int usuarioId, int personaId)
        {
            ModelState.Clear();
            if (!TryValidateModel(DireccionInput, nameof(DireccionInput)))
            {
                await CargarPaginaAsync(usuarioId);
                return Page();
            }

            await _personaService.AgregarDireccionAsync(personaId, DireccionInput.Calle, DireccionInput.Numero, DireccionInput.Piso, DireccionInput.Depto);
            MensajeOk = "Dirección agregada correctamente.";
            return RedirectToPage(new { usuarioId });
        }

        public async Task<IActionResult> OnPostActualizarDireccionAsync(int usuarioId, int personaId, int direccionId, string calle, string? numero, string? piso, string? depto)
        {
            if (string.IsNullOrWhiteSpace(calle))
            {
                MensajeError = "La calle es obligatoria.";
                await CargarPaginaAsync(usuarioId);
                return Page();
            }

            await _personaService.ActualizarDireccionAsync(personaId, direccionId, calle, numero, piso, depto);
            MensajeOk = "Dirección actualizada correctamente.";
            return RedirectToPage(new { usuarioId });
        }

        public async Task<IActionResult> OnPostEliminarDireccionAsync(int usuarioId, int personaId, int direccionId)
        {
            await _personaService.EliminarDireccionAsync(personaId, direccionId);
            MensajeOk = "Dirección eliminada correctamente.";
            return RedirectToPage(new { usuarioId });
        }

        public async Task<IActionResult> OnPostAgregarTelefonoAsync(int usuarioId, int personaId)
        {
            ModelState.Clear();
            if (!TryValidateModel(TelefonoInput, nameof(TelefonoInput)))
            {
                await CargarPaginaAsync(usuarioId);
                return Page();
            }

            await _personaService.AgregarTelefonoAsync(personaId, TelefonoInput.Telefono);
            MensajeOk = "Teléfono agregado correctamente.";
            return RedirectToPage(new { usuarioId });
        }

        public async Task<IActionResult> OnPostActualizarTelefonoAsync(int usuarioId, int personaId, int telefonoId, string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                MensajeError = "El teléfono es obligatorio.";
                await CargarPaginaAsync(usuarioId);
                return Page();
            }

            await _personaService.ActualizarTelefonoAsync(personaId, telefonoId, telefono);
            MensajeOk = "Teléfono actualizado correctamente.";
            return RedirectToPage(new { usuarioId });
        }

        public async Task<IActionResult> OnPostEliminarTelefonoAsync(int usuarioId, int personaId, int telefonoId)
        {
            await _personaService.EliminarTelefonoAsync(personaId, telefonoId);
            MensajeOk = "Teléfono eliminado correctamente.";
            return RedirectToPage(new { usuarioId });
        }

        public async Task<IActionResult> OnPostAgregarEmailAsync(int usuarioId, int personaId)
        {
            ModelState.Clear();
            if (!TryValidateModel(EmailInput, nameof(EmailInput)))
            {
                await CargarPaginaAsync(usuarioId);
                return Page();
            }

            await _personaService.AgregarEmailAsync(personaId, EmailInput.Email);
            MensajeOk = "Email agregado correctamente.";
            return RedirectToPage(new { usuarioId });
        }

        public async Task<IActionResult> OnPostActualizarEmailAsync(int usuarioId, int personaId, int emailId, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                MensajeError = "El email es obligatorio.";
                await CargarPaginaAsync(usuarioId);
                return Page();
            }

            await _personaService.ActualizarEmailAsync(personaId, emailId, email);
            MensajeOk = "Email actualizado correctamente.";
            return RedirectToPage(new { usuarioId });
        }

        public async Task<IActionResult> OnPostEliminarEmailAsync(int usuarioId, int personaId, int emailId)
        {
            await _personaService.EliminarEmailAsync(personaId, emailId);
            MensajeOk = "Email eliminado correctamente.";
            return RedirectToPage(new { usuarioId });
        }

        private async Task CargarPaginaAsync(int? usuarioId)
        {
            Usuarios = await _usuarioService.ObtenerUsuariosAsync();
            UsuarioId = usuarioId;

            if (usuarioId.HasValue)
            {
                UsuarioSeleccionado = await _usuarioService.ObtenerDetalleUsuarioAsync(usuarioId.Value);
                if (UsuarioSeleccionado is not null)
                {
                    RolesDisponibles = await _usuarioService.ObtenerRolesActivosAsync();
                    UsuarioInput = new UsuarioForm
                    {
                        LoginUser = UsuarioSeleccionado.LoginUser,
                        Nombre = UsuarioSeleccionado.Nombre,
                        ApePaterno = UsuarioSeleccionado.ApePaterno,
                        ApeMaterno = UsuarioSeleccionado.ApeMaterno,
                        DNI = UsuarioSeleccionado.DNI,
                        CUIT = UsuarioSeleccionado.CUIT
                    };
                }
            }
        }
    }
}
