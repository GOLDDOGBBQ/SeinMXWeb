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
public class OrdenCompraController : ApplicationController
{
    private readonly AppDbContext _db;
    private readonly AppClassContext _clasContext;
    private readonly RazorViewToStringRenderer _razorRenderer;

    public OrdenCompraController(AppDbContext db, AppClassContext clasContext, RazorViewToStringRenderer razorRenderer)
    {
        _db = db;
        _clasContext = clasContext;
        _razorRenderer = razorRenderer;
    }

    public async Task<IActionResult> Index(OrdenCompraBuscadorViewModel model)
    {
        if (!GetIsAdmin())
        {
            return Unauthorized();
        }

        model.Status ??= 1;

        var query = _db.VsOrdenCompras.OrderByDescending(x => x.IdOrdenCompra).AsQueryable();

        if (model.IdOrdenCompra != null)
        {
            query = query.Where(x => x.IdOrdenCompra == model.IdOrdenCompra);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(model.Cliente))
                query = query.Where(x => x.Cliente!.Contains(model.Cliente));


            if (model.Status != 0)
                query = query.Where(x => x.Status == model.Status);

            if (model.IdCotizacion != null)
                query = query.Where(x => x.IdCotizacion == model.IdCotizacion);
        }

        var lista = await query
            .OrderByDescending(x => x.IdCotizacion)
            .ToListAsync();

        model.Ordenes = lista;

        return View(model);
    }

    public IActionResult Nueva()
    {
        var model = new OrdenCompraNuevaViewModel();
        return View(model);
    }


    /*
    [HttpGet]
    public async Task<IActionResult> GenerarPdf(int? idOrdenCompra, bool file = false)
    {
        var header = await _db.VsOrdenCompras
            .FirstOrDefaultAsync(x => x.IdOrdenCompra == idOrdenCompra);

        if (header == null)
            return NotFound();

        var detalles = await _db.OrdenCompraDetalles
            .Where(x => x.IdOrdenCompra == idOrdenCompra)
            .OrderBy(x => x.IdOrdenCompraDetalle)
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


        if (file)
        {
            var pdf = await _razorRenderer.RenderViewToPdfAsync(
                "~/Views/Cotizacion/Rp_OrdenCompra.cshtml",
                model
            );


            return File(pdf, "application/pdf", $"Cotizacion-{model.IdCotizacion}.pdf");
        }
        else
        {
            return View("Rp_Cotizacion", model);
        }
    }
    */

    [HttpPost]
    public IActionResult Crear(OrdenCompraNuevaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["toast-error"] = "Revise los campos marcados.";
            return View("Nueva", model);
        }

        var nueva = new OrdenCompraNuevaViewModel
        {
            Fecha = DateOnly.FromDateTime(DateTime.Now),
            Status = 1,
            IdCotizacion = model.IdCotizacion,
            IdProveedor = model.IdProveedor,
            PorcentajeProveedor = _db.Proveedors.Find(model.IdProveedor)?.Tarifa ?? 0,
            TipoCambio = _db.TipoCambioDofs.OrderByDescending(x => x.Fecha).FirstOrDefault()?.TipoCambio ?? 1,
        };

        try
        {
            var result = _clasContext
                .SpGenericResults
                .FromSqlInterpolated($@"
                EXEC [INV].[GP_OrdenCompraNuevo]
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

            TempData["toast-success"] = $"se creo la orden de compra #{result.Id} correctamente.";

            return RedirectToAction("Editar", new { id = result.Id });
        }
        catch (Exception ex)
        {
            TempData["toast-error"] = "Error del servidor." + ex.Message;
        }


        ModelState.Clear();
        return View("Nueva", model);
    }

    /*[HttpGet("OrdenCompra/Editar/{id?}")]*/
    public async Task<IActionResult> Editar(int? id)
    {
        if (id == null)
            throw new Exception("Orden de compra no recibida");

        var model = await BuscarOrden(id);

        return View("Editar", model);
    }

    private async Task<OrdenCompraViewModel> BuscarOrden(int? id, OrdenCompraViewModel? refresh = null)
    {
        var entity = await _db.VsOrdenCompras
            .FirstOrDefaultAsync(x => x.IdOrdenCompra == id);

        if (entity == null)
            throw new Exception("Orden de compra");

        if (!GetIsAdmin())
        {
            throw new Exception("No tiene permiso para editar esta Orden de compra");
        }

        if (refresh is null)
        {
            var model = new OrdenCompraViewModel
            {
                IdOrdenCompra = entity.IdOrdenCompra,
                Cotizacion = entity.Cotizacion,
                Fecha = entity.Fecha,
                Cliente = entity.Cliente,
                Proveedor = entity.Proveedor,
                TipoCambio = entity.TipoCambio,
                PorcentajeProveedor = entity.PorcentajeProveedor,
                Status = entity.Status ?? 1,
                Observaciones = entity.Observaciones,
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


    [HttpGet]
    public async Task<IActionResult> GetTotales(int idOrdenCompra)
    {
        var item = await _db.VsOrdenCompras
            .FirstOrDefaultAsync(x => x.IdOrdenCompra == idOrdenCompra);

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
    public async Task<IActionResult> Guardar(OrdenCompraViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["toast-error"] = "Revise los campos marcados.";
            return View("Editar", await BuscarOrden(model.IdOrdenCompra, model));
        }


        try
        {
            var result = _clasContext
                .SpGenericResults
                .FromSqlInterpolated($@"
             EXEC [INV].[GP_OrdenCompraNuevo]
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
                return View("Editar", await BuscarOrden(model.IdOrdenCompra, model));
            }

            // Si el SP reporta error
            if (result.IdError != 0)
            {
                TempData["toast-error"] = result.MensajeError + " " + result.MensajeErrorDev;
                ModelState.Clear();
                return View("Editar", await BuscarOrden(model.IdOrdenCompra, model));
            }

            TempData["toast-success"] = "Los datos fueron guardados correctamente.";
            ModelState.Clear();
            return View("Editar", await BuscarOrden(result.Id));
        }
        catch (Exception ex)
        {
            TempData["toast-error"] = "Error del servidor." + ex.Message;
            return View("Editar", await BuscarOrden(model.IdOrdenCompra, model));
        }
    }


    /*
    [HttpPost]
    public async Task<IActionResult> AgregarProducto([FromBody] AgregarProductoRequest request)
    {
        try
        {
            var result = _clasContext
                .SpGenericResults
                .FromSqlInterpolated($@"
                EXEC [INV].[GP_OrdenCompraDetalleNuevo]
                     @IdOrdenCompra = {request.IdOrdenCompra},
                     @IdOrdenCompraDetalle = {request.IdCotizacionDetalle},
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
    */

    /*
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
    }*/

    /*[HttpDelete]
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
    }*/
}