using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEINMX.Clases;
using SEINMX.Context;
using SEINMX.Context.Database;
using SEINMX.Models.Inventario;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using SEINMX.Clases.Utilerias;

[Authorize]
public class CotizacionController : ApplicationController
{
    private readonly AppDbContext _db;
    private readonly AppClassContext _ClasContext;
    private readonly RazorViewToStringRenderer _razorRenderer;

    public CotizacionController(AppDbContext db, AppClassContext clasContext, RazorViewToStringRenderer razorRenderer)
    {
        _db = db;
        _ClasContext = clasContext;
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



    public async Task<IActionResult> Crear()
    {
        var nueva = new Cotizacion
        {
            Fecha = DateOnly.FromDateTime(DateTime.Now),
            Status = 1, // CREADA
            IdCliente = 1,
            Tarifa = 20,
            TipoCambio = 1,
            PorcentajeIva = 16,
            UsuarioResponsable = GetUserId(),
            CreadoPor = GetApiName(),
            FchReg = DateTime.Now,
            UsrReg = GetUserId()
        };

        _db.Cotizacions.Add(nueva);
        await _db.SaveChangesAsync();

        return RedirectToAction("Editar", new { id = nueva.IdCotizacion });
    }

    [HttpGet("Editar/{id?}")]
    public async Task<IActionResult> Editar(int? id)
    {
        if (id == null)
        {
            CotizacionViewModel modelEmpy = new();
            return View("Editar", modelEmpy);
        }

        var entity = await _db.VsCotizacions
            .FirstOrDefaultAsync(x => x.IdCotizacion == id);

        if (entity == null)
            return NotFound();

        var model = new CotizacionViewModel
        {
            IdCotizacion = entity.IdCotizacion,
            Fecha = entity.Fecha,
            TipoCambio = entity.TipoCambio,
            Tarifa = entity.Tarifa,
            PorcentajeIVA = entity.PorcentajeIva,
            Descuento = entity.Descuento,
            UsuarioResponsable = entity.UsuarioResponsable,
            IdCliente = entity.IdCliente,
            IdClienteContacto = entity.IdClienteContacto,
            IdClienteRazonSolcial = entity.IdClienteRazonSolcial,
            Status = entity.Status ?? 1,
            Observaciones = entity.Observaciones,
            SubTotal = entity.SubTotal,
            Iva = entity.Iva,
            Total = entity.Total,

            // Agrega todos los campos adicionales que tenga tu modelo
        };


        ViewBag.UsuariosResponsables = await ObtenerUsuariosResponsablesAsync();

        return View("Editar", model);
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
            Iva = item.Iva,
            SubTotal = item.SubTotal,
            Total = item.Total,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Guardar(CotizacionViewModel model)
    {
        ViewBag.UsuariosResponsables = await ObtenerUsuariosResponsablesAsync();

        if (!ModelState.IsValid)
        {
            TempData["toast-error"] = "Revise los campos marcados.";
            return View("Editar", model);
        }


        try
        {
            var result = _ClasContext
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
                return View("Editar", model);
            }

            // Si el SP reporta error
            if (result.IdError != 0)
            {
                TempData["toast-error"] = result.MensajeError + " " + result.MensajeErrorDev;
                return View("Editar", model);
            }

            TempData["toast-success"] = "Los datos fueron guardados correctamente.";

            return View("Editar", model);
        }
        catch (Exception ex)
        {
            TempData["toast-error"] = "Error del servidor." + ex.Message;
            return View("Editar", model);
        }
    }

    public async Task<JsonResult> DropdownProducto(int page = 0, int pageSize = 30, int? id = null, string search = "")
    {
        var lista = _db.Productos.Where(x => x.Eliminado == false);


        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            lista = lista.Where(x => EF.Functions.Like(x.Descripcion, pattern)||  EF.Functions.Like(x.Codigo, pattern) );

        }

        if (id.HasValue)
        {
            lista = lista.Where(c => c.IdProducto == id.Value);
        }

        var count = await lista.CountAsync();
        var data = await lista.Skip(page * pageSize).Take(pageSize)
            .Select(drmCliente => new { display = $"{drmCliente.Codigo} - {drmCliente.Descripcion}"  ,  value = drmCliente.IdProducto.ToString() })
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
            var result = _ClasContext
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
        var detalles = await _db.VsCotizacionDetalles
            .Where(x => x.IdCotizacion == idCotizacion)
            .Select(x => new
            {
                x.IdCotizacionDetalle,
                x.Cantidad,
                x.IdProducto,
                x.Codigo,
                x.Descripcion,
                x.PrecioListaMxn,
                x.PorcentajeProveedor,
                x.PrecioProveedor,
                x.PorcentajeProveedorGanancia,
                x.GananciaProveedor,
                x.PrecioSein,
                x.PrecioCliente,
                x.Total,
                x.Observaciones
            }).ToListAsync();

        return Json(detalles);
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