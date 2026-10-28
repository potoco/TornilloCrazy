using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using DataServicio.Servicio;
using DataServicio.Tabla;

namespace TornilloWeb.Pages.Admin.Sistema;

public class RolPermisoEditModel : PageModel
{
    public sealed class RolForm
    {
        [Required(ErrorMessage = "El nombre del rol es obligatorio."), StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(300)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Rol activo")]
        public bool Activo { get; set; } = true;
    }

    public sealed class PermisoForm
    {
        [Required(ErrorMessage = "El código es obligatorio."), StringLength(100)]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio."), StringLength(150)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(300)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Permiso activo")]
        public bool Activo { get; set; } = true;
    }

    private readonly RolPersmisoService _rolPermisoService;
    private readonly ILogger<RolPermisoEditModel> _logger;

    public RolPermisoEditModel(RolPersmisoService rolPermisoService, ILogger<RolPermisoEditModel> logger)
    {
        _rolPermisoService = rolPermisoService;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public int? RolId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PermisoId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? AsignacionRolId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Tab { get; set; } = "roles";

    [BindProperty]
    public RolForm RolInput { get; set; } = new();

    [BindProperty]
    public PermisoForm PermisoInput { get; set; } = new();

    [BindProperty]
    public List<int> PermisosSeleccionados { get; set; } = [];

    public List<RolTbl> Roles { get; private set; } = [];
    public List<PermisoTbl> Permisos { get; private set; } = [];
    public RolPersmisoService.RolDetalle? RolSeleccionado { get; private set; }
    public PermisoTbl? PermisoSeleccionado { get; private set; }
    public RolPersmisoService.RolDetalle? RolAsignacion { get; private set; }
    public bool EsEdicionRol => RolSeleccionado is not null;
    public bool EsEdicionPermiso => PermisoSeleccionado is not null;

    [TempData]
    public string? MensajeOk { get; set; }

    [TempData]
    public string? MensajeError { get; set; }

    public async Task OnGetAsync()
    {
        await CargarPaginaAsync();
    }

    public async Task<IActionResult> OnPostCrearRolAsync()
    {
        Tab = "roles";
        ModelState.Clear();
        if (!TryValidateModel(RolInput, nameof(RolInput)))
            return await VolverAlFormularioAsync();

        try
        {
            var rol = await _rolPermisoService.CrearRolAsync(RolInput.Nombre, RolInput.Descripcion);
            MensajeOk = $"Rol '{rol.Nombre}' creado correctamente.";
            return RedirectToPage(new { tab = "roles", rolId = rol.RolId });
        }
        catch (InvalidOperationException ex)
        {
            MensajeError = ex.Message;
            return await VolverAlFormularioAsync();
        }
    }

    public async Task<IActionResult> OnPostActualizarRolAsync(int rolId)
    {
        Tab = "roles";
        RolId = rolId;
        ModelState.Clear();
        if (!TryValidateModel(RolInput, nameof(RolInput)))
            return await VolverAlFormularioAsync();

        try
        {
            await _rolPermisoService.ActualizarRolAsync(rolId, RolInput.Nombre, RolInput.Descripcion, RolInput.Activo);
            MensajeOk = "Rol actualizado correctamente.";
            return RedirectToPage(new { tab = "roles", rolId });
        }
        catch (InvalidOperationException ex)
        {
            MensajeError = ex.Message;
            return await VolverAlFormularioAsync();
        }
        catch (KeyNotFoundException ex)
        {
            MensajeError = ex.Message;
            return RedirectToPage(new { tab = "roles" });
        }
    }

    public async Task<IActionResult> OnPostCambiarEstadoRolAsync(int rolId, bool activo)
    {
        try
        {
            await _rolPermisoService.CambiarEstadoRolAsync(rolId, activo);
            MensajeOk = activo ? "Rol activado correctamente." : "Rol desactivado correctamente.";
        }
        catch (KeyNotFoundException ex)
        {
            MensajeError = ex.Message;
        }

        return RedirectToPage(new { tab = "roles" });
    }

    public async Task<IActionResult> OnPostEliminarRolAsync(int rolId)
    {
        try
        {
            await _rolPermisoService.EliminarRolAsync(rolId);
            MensajeOk = "Rol eliminado correctamente.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            MensajeError = ex.Message;
        }

        return RedirectToPage(new { tab = "roles" });
    }

    public async Task<IActionResult> OnPostCrearPermisoAsync()
    {
        Tab = "permisos";
        ModelState.Clear();
        if (!TryValidateModel(PermisoInput, nameof(PermisoInput)))
            return await VolverAlFormularioAsync();

        try
        {
            var permiso = await _rolPermisoService.CrearPermisoAsync(PermisoInput.Codigo, PermisoInput.Nombre, PermisoInput.Descripcion);
            MensajeOk = $"Permiso '{permiso.Codigo}' creado correctamente.";
            return RedirectToPage(new { tab = "permisos", permisoId = permiso.PermisoId });
        }
        catch (InvalidOperationException ex)
        {
            MensajeError = ex.Message;
            return await VolverAlFormularioAsync();
        }
    }

    public async Task<IActionResult> OnPostActualizarPermisoAsync(int permisoId)
    {
        Tab = "permisos";
        PermisoId = permisoId;
        ModelState.Clear();
        if (!TryValidateModel(PermisoInput, nameof(PermisoInput)))
            return await VolverAlFormularioAsync();

        try
        {
            await _rolPermisoService.ActualizarPermisoAsync(permisoId, PermisoInput.Codigo, PermisoInput.Nombre, PermisoInput.Descripcion, PermisoInput.Activo);
            MensajeOk = "Permiso actualizado correctamente.";
            return RedirectToPage(new { tab = "permisos", permisoId });
        }
        catch (InvalidOperationException ex)
        {
            MensajeError = ex.Message;
            return await VolverAlFormularioAsync();
        }
        catch (KeyNotFoundException ex)
        {
            MensajeError = ex.Message;
            return RedirectToPage(new { tab = "permisos" });
        }
    }

    public async Task<IActionResult> OnPostCambiarEstadoPermisoAsync(int permisoId, bool activo)
    {
        try
        {
            await _rolPermisoService.CambiarEstadoPermisoAsync(permisoId, activo);
            MensajeOk = activo ? "Permiso activado correctamente." : "Permiso desactivado correctamente.";
        }
        catch (KeyNotFoundException ex)
        {
            MensajeError = ex.Message;
        }

        return RedirectToPage(new { tab = "permisos" });
    }

    public async Task<IActionResult> OnPostEliminarPermisoAsync(int permisoId)
    {
        try
        {
            await _rolPermisoService.EliminarPermisoAsync(permisoId);
            MensajeOk = "Permiso eliminado correctamente.";
        }
        catch (KeyNotFoundException ex)
        {
            MensajeError = ex.Message;
        }

        return RedirectToPage(new { tab = "permisos" });
    }

    public async Task<IActionResult> OnPostGuardarAsignacionAsync(int asignacionRolId)
    {
        Tab = "asignacion";
        AsignacionRolId = asignacionRolId;

        try
        {
            await _rolPermisoService.GuardarPermisosDeRolAsync(asignacionRolId, PermisosSeleccionados);
            MensajeOk = "Permisos asignados correctamente.";
        }
        catch (KeyNotFoundException ex)
        {
            MensajeError = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar permisos del rol {RolId}", asignacionRolId);
            MensajeError = "Ocurrió un error al guardar la asignación.";
        }

        return RedirectToPage(new { tab = "asignacion", asignacionRolId });
    }

    private async Task<IActionResult> VolverAlFormularioAsync()
    {
        await CargarPaginaAsync(conservarEntradas: true);
        return Page();
    }

    private async Task CargarPaginaAsync(bool conservarEntradas = false)
    {
        if (string.IsNullOrWhiteSpace(Tab) || Tab is not ("roles" or "permisos" or "asignacion"))
        {
            if (PermisoId is not null)
                Tab = "permisos";
            else if (AsignacionRolId is not null)
                Tab = "asignacion";
            else
                Tab = "roles";
        }

        Roles = await _rolPermisoService.ObtenerRolesAsync();
        Permisos = await _rolPermisoService.ObtenerPermisosAsync();

        if (RolId is not null)
        {
            RolSeleccionado = await _rolPermisoService.ObtenerRolDetalleAsync(RolId.Value);
            if (RolSeleccionado is not null && !conservarEntradas)
            {
                RolInput = new RolForm
                {
                    Nombre = RolSeleccionado.Nombre,
                    Descripcion = RolSeleccionado.Descripcion,
                    Activo = RolSeleccionado.Activo
                };
            }
        }

        if (PermisoId is not null)
        {
            PermisoSeleccionado = await _rolPermisoService.ObtenerPermisoAsync(PermisoId.Value);
            if (PermisoSeleccionado is not null && !conservarEntradas)
            {
                PermisoInput = new PermisoForm
                {
                    Codigo = PermisoSeleccionado.Codigo,
                    Nombre = PermisoSeleccionado.Nombre,
                    Descripcion = PermisoSeleccionado.Descripcion,
                    Activo = PermisoSeleccionado.Activo
                };
            }
        }

        if (AsignacionRolId is null && Roles.Count > 0)
        {
            AsignacionRolId = RolId ?? Roles[0].RolId;
        }

        if (AsignacionRolId is not null)
        {
            RolAsignacion = await _rolPermisoService.ObtenerRolDetalleAsync(AsignacionRolId.Value);
            if (RolAsignacion is not null && !conservarEntradas)
                PermisosSeleccionados = RolAsignacion.PermisoIds.ToList();
        }
    }
}
