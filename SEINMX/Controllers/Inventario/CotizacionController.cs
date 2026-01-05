using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEINMX.Clases;
using SEINMX.Context;
using SEINMX.Models.Inventario;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using SEINMX.Clases.Utilerias;

[Authorize]
public class CotizacionController : ApplicationController
{
    private readonly AppDbContext _db;
    private readonly AppClassContext _clasContext;
    private readonly RazorViewToStringRenderer _razorRenderer;

    public CotizacionController(AppDbContext db, AppClassContext clasContext, RazorViewToStringRenderer razorRenderer)
    {
        _db = db;
        _clasContext = clasContext;
        _razorRenderer = razorRenderer;
    }


    public async Task<IActionResult> Index(CotizacionBuscadorViewModel model)
    {
        model.Status ??= 1;

        var query = _db.VsCotizacions.OrderByDescending(x => x.IdCotizacion).AsQueryable();

        if (model.IdCotizacion != null)
        {
            query = query.Where(x => x.IdCotizacion == model.IdCotizacion);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(model.Cliente))
                query = query.Where(x => x.Cliente!.Contains(model.Cliente));


            if (model.Status != 0)
                query = query.Where(x => x.Status == model.Status);
        }

        if (!GetIsAdmin())
        {
            var usr = GetUserId();
            query = query.Where(x => x.UsuarioResponsable == usr);
        }


        var lista = await query
            .OrderByDescending(x => x.IdCotizacion)
            .ToListAsync();

        model.Cotizaciones = lista;

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GenerarPdf(int? idCotizacion, bool file = false)
    {
        var header = await _db.VsCotizacions
            .FirstOrDefaultAsync(x => x.IdCotizacion == idCotizacion);

        if (header == null)
            return NotFound();

        var detalles = await _db.VsCotizacionDetalles
            .Where(x => x.IdCotizacion == idCotizacion)
            .OrderBy(x => x.IdCotizacionDetalle)
            .ToListAsync();


        var model = new CotizacionPdfModel(
            IdCotizacion: header.IdCotizacion,
            Cotizacion: header.Cotizacion,
            Fecha: header.Fecha ?? DateOnly.FromDateTime(DateTime.Now),
            TipoCambio: header.TipoCambio,
            Tarifa: header.Tarifa,
            PorcentajeIVA: header.PorcentajeIva,
            Descuento: header.Descuento,
            UsuarioResponsable: header.UsuarioResponsable,
            IdCliente: header.IdCliente,
            IdClienteContacto: header.IdClienteContacto,
            IdClienteRazonSolcial: header.IdClienteRazonSolcial,
            Observaciones: header.Observaciones,
            SubTotal: header.SubTotal,
            Iva: header.Iva,
            Total: header.Total,
            DescuentoTotal: header.DescuentoTotal,
            EsIncluirEnvio: header.EsIncluirEnvio,
            Detalles: detalles,

            // Campos para PDF
            Cliente: header.Cliente,
            NombreContacto: header.NombreContacto,
            Telefono: header.Telefono
        );


        if (file)
        {
            var pdf = await _razorRenderer.RenderViewToPdfAsync(
                "~/Views/Cotizacion/Rp_Cotizacion.cshtml",
                model
            );


            return File(pdf, "application/pdf", $"Cotizacion-{model.IdCotizacion}.pdf");
        }
        else
        {
            return View("Rp_Cotizacion", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> OrdenCompra(int? idCotizacion)
    {
        var header = await _db.VsCotizacions
            .FirstOrDefaultAsync(x => x.IdCotizacion == idCotizacion);

        if (header == null)
            return NotFound();

        var detalles = await _db.VsCotizacionDetalles
            .Where(x => x.IdCotizacion == idCotizacion)
            .OrderBy(x => x.IdCotizacionDetalle)
            .ToListAsync();


        var model = new OrdenCompraPdfModel(
            IdCotizacion: header.IdCotizacion,
            Fecha: header.Fecha ?? DateOnly.FromDateTime(DateTime.Now),
            TipoCambio: header.TipoCambio,
            Tarifa: header.Tarifa,
            PorcentajeIVA: header.PorcentajeIva,
            Descuento: header.Descuento,
            UsuarioResponsable: header.UsuarioResponsable,
            IdCliente: header.IdCliente,
            IdClienteContacto: header.IdClienteContacto,
            IdClienteRazonSolcial: header.IdClienteRazonSolcial,
            Observaciones: header.Observaciones,
            SubTotal: header.SubTotal,
            Iva: header.Iva,
            Total: header.Total,
            Detalles: detalles,

            // Campos para PDF
            Cliente: header.Cliente,
            NombreContacto: header.NombreContacto,
            Telefono: header.Telefono
        );


        var pdf = await _razorRenderer.RenderViewToPdfAsync(
            "~/Views/Cotizacion/Rp_OrdenCompra.cshtml",
            model
        );


        return File(pdf, "application/pdf", $"Orden Compra - {model.IdCotizacion}.pdf");
    }


    public IActionResult Nueva(int? idCliente = null)
    {
        var model = new CotizacionNuevaViewModel
        {
            IdCliente = idCliente ?? 0
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult Crear(CotizacionNuevaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["toast-error"] = "Revise los campos marcados.";
            return View("Editar", model);
        }

        var nueva = new CotizacionViewModel
        {
            Fecha = DateOnly.FromDateTime(DateTime.Now),
            Status = 1, // CREADA
            IdCliente = model.IdCliente,
            IdClienteContacto = model.IdClienteContacto,
            IdClienteRazonSolcial = model.IdClienteRazonSolcial,
            TipoCambio = _db.TipoCambioDofs.OrderByDescending(x => x.Fecha).FirstOrDefault()?.TipoCambio ?? 1,
            PorcentajeIVA = 16,
            Descuento = 0,
            UsuarioResponsable = GetUserId(),
        };

        try
        {
            var result = _clasContext
                .SpCotizacionNuevoResults
                .FromSqlInterpolated($@"
                EXEC [INV].[GP_CotizacionNuevo]
                     @json = {JsonSerializer.Serialize(nueva)},
                     @UserId = {GetUserId()},
                     @ProgName = {GetApiName()}
            ")
                .AsEnumerable()
                .FirstOrDefault();

            // Si el SP no regresó nada (caso muy raro)
            if (result is not { IdError: 0 })
            {
                if (result is null)
                {
                    throw new ArgumentOutOfRangeException(nameof(result), "No se recibió respuesta del servidor.");
                }
                else
                {
                    throw new AggregateException(result.MensajeError + " " + result.MensajeErrorDev);
                }
            }

            TempData["toast-success"] = $"se creo la cotizacion #{result.IdCotizacion} correctamente.";

            return RedirectToAction("Editar", new { id = result.IdCotizacion });
        }
        catch (Exception ex)
        {
            TempData["toast-error"] = "Error del servidor." + ex.Message;
        }


        ModelState.Clear();
        return View("Nueva", model);
    }

    [HttpGet("Editar/{id?}")]
    public async Task<IActionResult> Editar(int? id)
    {
        if (id == null)
        {
            CotizacionViewModel modelEmpy = new();
            return View("Editar", modelEmpy);
        }

        var model = await BuscarCotizacion(id);

        ViewBag.UsuariosResponsables = await ObtenerUsuariosResponsablesAsync();

        return View("Editar", model);
    }

    private async Task<CotizacionViewModel> BuscarCotizacion(int? id, CotizacionViewModel? refresh = null)
    {
        var entity = await _db.VsCotizacions
            .FirstOrDefaultAsync(x => x.IdCotizacion == id);

        if (entity == null)
            throw new Exception("Cotización no encontrada");

        if (!GetIsAdmin())
        {
            if (entity.UsuarioResponsable != GetUserId())
            {
                throw new Exception("No tiene permiso para editar esta cotización");

            }
        }

        if (refresh is null)
        {
            var model = new CotizacionViewModel
            {
                Cotizacion = entity.Cotizacion,
                IdCotizacion = entity.IdCotizacion,
                Fecha = entity.Fecha,
                TipoCambio = entity.TipoCambio,
                Perfil = entity.Perfil,
                PorcentajeIVA = entity.PorcentajeIva,
                Descuento = entity.Descuento,
                UsuarioResponsable = entity.UsuarioResponsable,
                IdCliente = entity.IdCliente,
                Cliente = entity.Cliente,
                IdClienteContacto = entity.IdClienteContacto,
                IdClienteRazonSolcial = entity.IdClienteRazonSolcial,
                Status = entity.Status ?? 1,
                Observaciones = entity.Observaciones,
                EsIncluirEnvio = entity.EsIncluirEnvio,
                SubTotal = entity.SubTotal,
                Iva = entity.Iva,
                Total = entity.Total,
            };

            return model;
        }
        else
        {
            refresh.SubTotal = entity.SubTotal;
            refresh.Iva = entity.Iva;
            refresh.Total = entity.Total;
            return refresh;
        }
    }

    private async Task<List<SelectListItem>> ObtenerUsuariosResponsablesAsync()
    {
        return await _db.Usuarios
            .Select(u => new SelectListItem
            {
                Value = u.Usuario1,
                Text = u.Nombre
            })
            .OrderBy(u => u.Text)
            .ToListAsync();
    }

    [HttpGet]
    public async Task<IActionResult> GetTotales(int idCotizacion)
    {
        var item = await _db.VsCotizacions
            .FirstOrDefaultAsync(x => x.IdCotizacion == idCotizacion);

        if (item == null) return NotFound();


        return Json(new
        {
            ok = true,
            item.Iva,
            item.SubTotal,
            item.Total,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Guardar(CotizacionViewModel model)
    {
        ViewBag.UsuariosResponsables = await ObtenerUsuariosResponsablesAsync();

        model.Descuento ??= 0;

        if (!ModelState.IsValid)
        {
            TempData["toast-error"] = "Revise los campos marcados.";
            return View("Editar", await BuscarCotizacion(model.IdCotizacion, model));
        }


        try
        {
            var result = _clasContext
                .SpCotizacionNuevoResults
                .FromSqlInterpolated($@"
                EXEC [INV].[GP_CotizacionNuevo]
                     @json = {JsonSerializer.Serialize(model)},
                     @UserId = {GetUserId()},
                     @ProgName = {GetApiName()}
            ")
                .AsEnumerable()
                .FirstOrDefault();

            // Si el SP no regresó nada (caso muy raro)
            if (result == null)
            {
                TempData["toast-error"] = "No se recibió respuesta del servidor.";
                ModelState.Clear();
                return View("Editar", await BuscarCotizacion(model.IdCotizacion, model));
            }

            // Si el SP reporta error
            if (result.IdError != 0)
            {
                TempData["toast-error"] = result.MensajeError + " " + result.MensajeErrorDev;
                ModelState.Clear();
                return View("Editar", await BuscarCotizacion(model.IdCotizacion, model));
            }

            TempData["toast-success"] = "Los datos fueron guardados correctamente.";
            ModelState.Clear();
            return View("Editar", await BuscarCotizacion(result.IdCotizacion));
        }
        catch (Exception ex)
        {
            TempData["toast-error"] = "Error del servidor." + ex.Message;
            return View("Editar", await BuscarCotizacion(model.IdCotizacion, model));
        }
    }

    public async Task<JsonResult> DropdownProducto(int page = 0, int pageSize = 30, int? id = null, string search = "")
    {
        var lista = _db.Productos.Where(x => x.Eliminado == false);


        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            lista = lista.Where(x => EF.Functions.Like(x.Descripcion, pattern) || EF.Functions.Like(x.Codigo, pattern));
        }

        if (id.HasValue)
        {
            lista = lista.Where(c => c.IdProducto == id.Value);
        }

        var count = await lista.CountAsync();
        var data = await lista.Skip(page * pageSize).Take(pageSize)
            .Select(drmCliente => new
            {
                display = $"{drmCliente.Codigo} - {drmCliente.Descripcion}", value = drmCliente.IdProducto.ToString()
            })
            .ToListAsync();
        var remaining = Math.Max(count - (page * pageSize) - data.Count, 0);

        return Json(new
        {
            moreToLoad = remaining > 0,
            data
        });
    }

    [HttpPost]
    public async Task<IActionResult> AgregarProducto([FromBody] AgregarProductoRequest request)
    {
        try
        {
            var result = _clasContext
                .SpCotizacionDetalleNuevoResults
                .FromSqlInterpolated($@"
                EXEC [INV].[GP_CotizacionDetalleNuevo]
                     @IdCotizacion = {request.IdCotizacion},
                     @IdCotizacionDetalle = {request.IdCotizacionDetalle},
                     @IdProducto = {request.IdProducto},
                     @Cantidad = {request.Cantidad},
                     @Observaciones = {request.Observaciones},
                     @UserId = {GetUserId()},
                     @ProgName = {GetApiName()}
            ")
                .AsEnumerable()
                .FirstOrDefault();

            // Si el SP no regresó nada (caso muy raro)
            if (result == null)
            {
                return Json(new { ok = false, msg = "No se recibió respuesta del servidor." });
            }

            // Si el SP reporta error
            if (result.IdError != 0)
            {
                return Json(new
                {
                    ok = false,
                    msg = result.MensajeError,
                    dev = result.MensajeErrorDev
                });
            }

            return Json(new
            {
                ok = true,
                idCotizacionDetalle = result.IdCotizacionDetalle
            });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, msg = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDetalles(int idCotizacion)
    {
        var query = _db.VsCotizacionDetalles
            .Where(x => x.IdCotizacion == idCotizacion);

        if (GetIsAdmin())
        {
            var lista = await query
                .Select(x => new
                {
                    idCotizacionDetalle = x.IdCotizacionDetalle,
                    cantidad = x.Cantidad,
                    idProducto = x.IdProducto,
                    codigo = x.Codigo,
                    descripcion = x.Descripcion,
                    precioListaMxn = x.PrecioListaMxn,
                    porcentajeProveedor = x.PorcentajeProveedor,
                    precioProveedor = x.PrecioProveedor,
                    porcentajeProveedorGanancia = x.PorcentajeProveedorGanancia,
                    gananciaProveedor = x.GananciaProveedor,
                    precioSein = x.PrecioSein,
                    precioCliente = x.PrecioCliente,
                    total = x.Total,
                    observaciones = x.Observaciones
                })
                .ToListAsync();

            return Json(lista);
        }
        else
        {
            var lista = await query
                .Select(x => new
                {
                    idCotizacionDetalle = x.IdCotizacionDetalle,
                    cantidad = x.Cantidad,
                    idProducto = x.IdProducto,
                    codigo = x.Codigo,
                    descripcion = x.Descripcion,
                    precioCliente = x.PrecioCliente,
                    total = x.Total,
                    observaciones = x.Observaciones
                })
                .ToListAsync();

            return Json(lista);
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteProducto(int idDetalle)
    {
        try
        {
            var item = await _db.CotizacionDetalles.FindAsync(idDetalle);
            if (item == null) return NotFound();

            _db.CotizacionDetalles.Remove(item);
            await _db.SaveChangesAsync();

            return Json(new { ok = true });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, msg = ex.Message });
        }
    }
}