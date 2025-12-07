using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEINMX.Clases;
using SEINMX.Context;
using SEINMX.Context.Database;
using SEINMX.Models.Inventario;
using System.Text.Json;

[Authorize]
public class CotizacionController : ApplicationController
{
    private readonly AppDbContext _db;
    private readonly AppClassContext _ClasContext;

    public CotizacionController(AppDbContext db, AppClassContext clasContext)
    {
        _db = db;
        _ClasContext = clasContext;
    }


    public IActionResult Nueva()
    {
        return RedirectToAction("Editar");
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
        CotizacionViewModel vm = new();

        if (id == null)
        {
            // Mostrar botón "Crear Cotización" en la vista
            return View("Editar", vm);
        }

        vm = await GetModelView((int)id);

        if (vm.Cotizacion == null)
            return NotFound();


        return View("Editar", vm);
    }

    private async Task<CotizacionViewModel> GetModelView(int idCotizacion)
    {
        CotizacionViewModel vm = new()
        {
            // Buscar vista
            Cotizacion = await _db.VsCotizacions
                .FirstOrDefaultAsync(x => x.IdCotizacion == idCotizacion),
            // Cargar detalles
            Detalles = await _db.CotizacionDetalles
                .Where(x => x.IdCotizacion == idCotizacion)
                .ToListAsync()
        };

        return vm;
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
        if (model.Cotizacion is null || model.Cotizacion is { IdCotizacion: 0 })
        {
            ViewBag.Error = "No se recibio el numero de cotizacion";
            return View("Editar", model);
        }


        try
        {
            var result = _ClasContext
                .SpCotizacionNuevoResults
                .FromSqlInterpolated($@"
                EXEC [INV].[GP_CotizacionNuevo]
                     @json = {JsonSerializer.Serialize(model.Cotizacion)},
                     @UserId = {GetUserId()},
                     @ProgName = {GetApiName()}
            ")
                .AsEnumerable()
                .FirstOrDefault();

            // Si el SP no regresó nada (caso muy raro)
            if (result == null)
            {
                ViewBag.Error = "No se recibió respuesta del servidor.";
                return View("Editar", model);
            }

            // Si el SP reporta error
            if (result.IdError != 0)
            {
                ViewBag.Error = "La cotización no existe o fue eliminada.";
                return View("Editar", model);
            }

            TempData["Success"] = "Los datos fueron guardados correctamente.";
            return RedirectToAction("Editar", new { id = result.IdCotizacion });
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error del servidor." + ex.Message;
            return View("Editar", model);
        }
    }

    public async Task<JsonResult> DropdownProducto(int page = 0, int pageSize = 30, int? id = null, string search = "")
    {
        var lista = _db.Productos.Where(x => x.Eliminado == false);


        if (!string.IsNullOrWhiteSpace(search))
        {
            lista.Where(x => x.Descripcion.Contains(search));
        }

        if (id.HasValue)
        {
            lista = lista.Where(c => c.IdProducto == id.Value);
        }

        var count = await lista.CountAsync();
        var data = await lista.Skip(page * pageSize).Take(pageSize)
            .Select(drmCliente => new { display = drmCliente.Descripcion, value = drmCliente.IdProducto.ToString() })
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


    public async Task<IActionResult> TablaProductos(int id)
    {
        var lista = await _db.CotizacionDetalles
            .Where(x => x.IdCotizacion == id)
            .ToListAsync();

        return PartialView("_TablaProductos", lista);
    }
}