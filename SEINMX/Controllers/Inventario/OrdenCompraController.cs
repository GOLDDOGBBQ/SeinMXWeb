using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEINMX.Clases;
using SEINMX.Context;
using SEINMX.Models.Inventario;
using System.Text.Json;
using Microsoft.Data.SqlClient;
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

    public async Task<IActionResult> Index(OrdenCompraBuscadorViewModel model, int? idCotizacion)
    {
        if (!GetIsAdmin())
        {
            return Unauthorized();
        }

        model.Status ??= 1;

        if (idCotizacion is not null)
        {
            model.IdCotizacion = idCotizacion.Value;
        }

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

    [HttpGet]
    public async Task<IActionResult> GenerarPdf(int? id, bool file = false)
    {
        var header = await _db.VsOrdenCompras
            .FirstOrDefaultAsync(x => x.IdOrdenCompra == id);

        if (header == null)
            return NotFound();

        var detalles = await _db.VsOrdenCompraDetalles
            .Where(x => x.IdOrdenCompra == id)
            .OrderBy(x => x.IdOrdenCompraDetalle)
            .ToListAsync();


        var model = new OrdenCompraPdfModel(
            IdOrdenCompra: header.IdOrdenCompra,
            Fecha: header.Fecha,
            TipoCambio: header.TipoCambio,
            CondicionPago: header.CondicionPago,
            Proveedor: header.Proveedor,
            ProveedorRfc: header.ProveedorRfc,
            ProveedorRazonSocial: header.ProveedorRazonSocial,
            StatusDesc: header.StatusDesc,
            Observaciones: header.Observaciones,
            SubTotal: header.SubTotal,
            Iva: header.Iva,
            Total: header.Total,
            Detalles: detalles
        );


        if (file)
        {
            var pdf = await _razorRenderer.RenderViewToPdfAsync(
                "~/Views/OrdenCompra/Rp_OrdenCompra.cshtml",
                model,
                "/img/PiePagina.webp"
            );


            return File(pdf, "application/pdf", $"OrdenCompra-{model.IdOrdenCompra}.pdf");
        }
        else
        {
            return View("Rp_OrdenCompra", model);
        }
    }


    [HttpPost]
    public IActionResult Crear(OrdenCompraNuevaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["toast-error"] = "Revise los campos marcados.";
            return View("Nueva", model);
        }


        var proveedor = _db.Proveedors.Find(model.IdProveedor);
        var porcentajeProveedor = proveedor?.Tarifa ?? 0;
        var porcentajeProveedorGanancia = proveedor?.TarifaGanancia ?? 0;

        var nueva = new OrdenCompraNuevaViewModel
        {
            Fecha = DateOnly.FromDateTime(DateTime.Now),
            Status = 1,
            IdCotizacion = model.IdCotizacion,
            IdProveedor = model.IdProveedor,
            PorcentajeProveedor = porcentajeProveedor,
            PorcentajeProveedorGanancia = porcentajeProveedorGanancia,
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
                IdCotizacion = entity.IdCotizacion,
                Cotizacion = entity.Cotizacion,
                Fecha = entity.Fecha,
                Cliente = entity.Cliente,
                Proveedor = entity.Proveedor,
                TipoCambio = entity.TipoCambio,
                PorcentajeProveedor = entity.PorcentajeProveedor,
                PorcentajeProveedorGanancia = entity.PorcentajeProveedorGanancia,
                CondicionPago = entity.CondicionPago,
                Status = entity.Status ?? 1,
                Observaciones = entity.Observaciones,
                SubTotal = entity.SubTotal,
                Iva = entity.Iva,
                Total = entity.Total,
                Detalles = await ObtenerDetalleAsync(entity.IdCotizacion, entity.IdOrdenCompra)
            };

            return model;
        }
        else
        {
            refresh.Detalles = await ObtenerDetalleAsync(entity.IdCotizacion, entity.IdOrdenCompra);
            refresh.SubTotal = entity.SubTotal;
            refresh.Iva = entity.Iva;
            refresh.Total = entity.Total;
            return refresh;
        }
    }


    public async Task<List<CotizacionOrdenDetalleViewModel>> ObtenerDetalleAsync(
        int idCotizacion,
        int idOrdenCompra,
        int? idOrdenCompraDetalle = null,
        int? idCotizacionDetalle = null
    )
    {
        string sWhere = "";
        var nLen = idOrdenCompraDetalle is null ? 2 : 3;

        if (idCotizacionDetalle is not null)
        {
            nLen += 1;
        }

        object[] arrParametros = new object[nLen];

        arrParametros[0] = new SqlParameter("@IdCotizacion", idCotizacion);

        arrParametros[1] = new SqlParameter("@IdOrdenCompra", idOrdenCompra);

        if (idOrdenCompraDetalle is not null)
        {
            arrParametros[2] = new SqlParameter("@IdOrdenCompraDetalle", idOrdenCompraDetalle);
            sWhere = " WHERE ocd.IdOrdenCompraDetalle = @IdOrdenCompraDetalle";
        }

        if (idCotizacionDetalle is not null)
        {
            nLen -= 1;
            arrParametros[nLen] = new SqlParameter("@IdCotizacionDetalle", idCotizacionDetalle);
            sWhere = " WHERE c.IdCotizacionDetalle = @IdCotizacionDetalle";
        }

        var sql = $@"
            WITH CTE_Productos AS (SELECT cd.IdCotizacionDetalle,
                                          cd.IdCotizacion,
                                          p.Codigo,
                                          p.CodigoProveedor,
                                          p.Descripcion,
                                          cd.Cantidad,
                                          PrecioListaMXN  = IIF(p.IdMoneda = 1, p.PrecioLista, (p.PrecioLista * OC.TipoCambio)),
                                          PrecioProveedor = (IIF(p.IdMoneda = 1, p.PrecioLista, (p.PrecioLista * OC.TipoCambio)) -
                                                             (IIF(p.IdMoneda = 1, p.PrecioLista, (p.PrecioLista * OC.TipoCambio)) *
                                                              (OC.PorcentajeProveedor / 100))),

                                          PrecioSein      =((IIF(p.IdMoneda = 1, p.PrecioLista, (p.PrecioLista * OC.TipoCambio)) -
                                                             (IIF(p.IdMoneda = 1, p.PrecioLista, (p.PrecioLista * OC.TipoCambio)) *
                                                              (OC.PorcentajeProveedor / 100)))
                                              / (1 - (OC.PorcentajeProveedorGanancia / 100))),

                                          OC.PorcentajeProveedorGanancia,
                                          OC.PorcentajeProveedor
                                   FROM INV.CotizacionDetalle    cd
                                            JOIN INV.OrdenCompra OC ON cd.IdCotizacion = OC.IdCotizacion
                                            JOIN INV.Producto    p
                                                 ON cd.IdProducto = p.IdProducto AND OC.IdProveedor = p.IdProveedor
                                   WHERE OC.IdOrdenCompra = @IdOrdenCompra),
                 CTE_Asignados AS (SELECT ocd.IdCotizacionDetalle,
                                          SUM(ocd.Cantidad) AS Asignados
                                   FROM INV.OrdenCompraDetalle   ocd
                                            JOIN INV.OrdenCompra oc ON ocd.IdOrdenCompra = oc.IdOrdenCompra
                                   WHERE oc.IdCotizacion = @IdCotizacion
                                     AND oc.IdOrdenCompra <> @IdOrdenCompra
                                   GROUP BY ocd.IdCotizacionDetalle)
            SELECT c.IdCotizacionDetalle,
                   c.IdCotizacion,
                   c.Codigo,
                   c.CodigoProveedor,
                   c.Descripcion,
                   ocd.IdOrdenCompraDetalle,
                   ocd.IdOrdenCompra,
                   CantidadCotizada            = c.Cantidad,
                   Cantidad                    = ISNULL(ocd.Cantidad, 0),
                   CantidadDisponible          = c.Cantidad
                       - ISNULL(a.Asignados, 0)
                       - ISNULL(ocd.Cantidad, 0),
                   PrecioListaMXN              = COALESCE(ocd.PrecioListaMXN, c.PrecioListaMXN),
                   PrecioProveedor             = COALESCE(ocd.PrecioProveedor, c.PrecioProveedor),
                   PrecioSein                  = COALESCE(ocd.PrecioSein, c.PrecioSein),
                   PorcentajeProveedor         = COALESCE(ocd.PorcentajeProveedor, c.PorcentajeProveedor),
                   PorcentajeProveedorGanancia = COALESCE(ocd.PorcentajeProveedorGanancia, c.PorcentajeProveedorGanancia),
                   Total                       = (COALESCE(ocd.PrecioSein, c.PrecioSein) * ISNULL(ocd.Cantidad, 0))
            FROM CTE_Productos                        c
                     LEFT JOIN CTE_Asignados          a ON c.IdCotizacionDetalle = a.IdCotizacionDetalle
                     LEFT JOIN INV.OrdenCompraDetalle ocd
                               ON c.IdCotizacionDetalle = ocd.IdCotizacionDetalle AND ocd.IdOrdenCompra = @IdOrdenCompra
            {sWhere}

    ";


        return await _clasContext
            .CotizacionOrdenDetalleViewModels
            .FromSqlRaw(sql,
                arrParametros)
            .AsNoTracking()
            .ToListAsync();
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


    [HttpPost]
    public async Task<IActionResult> GuardarDetalle([FromBody] OrdenCompraDetalleRequest request)
    {
        try
        {
            var result = _clasContext
                .SpGenericResults
                .FromSqlInterpolated($@"
                EXEC [INV].[GP_OrdenCompraDetalleNuevo]
                     @IdOrdenCompra = {request.IdOrdenCompra},
                     @IdOrdenCompraDetalle = {request.IdOrdenCompraDetalle},
                     @IdCotizacionDetalle = {request.IdCotizacionDetalle},         
                     @Cantidad = {request.Cantidad},  
                     @UserId = {GetUserId()},
                     @ProgName = {GetApiName()}
            ")
                .AsEnumerable()
                .FirstOrDefault();

            // Si el SP no regresó nada (caso muy raro)
            if (result is null)
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
                IdOrdenCompraDetalle = result.Id,
                data = await ObtenerDetalleAsync(request.IdCotizacion, request.IdOrdenCompra, result.Id)
            });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, msg = ex.Message });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> EliminarDetalle(int id)
    {
        try
        {
            var item = await _db.OrdenCompraDetalles.FindAsync(id);
            if (item == null) return NotFound();

            int idOrdenCompra = item.IdOrdenCompra;
            int idCotizacionDetalle = item.IdCotizacionDetalle;

            var itemOC = await _db.OrdenCompras.FindAsync(idOrdenCompra);
            if (itemOC == null) return NotFound();

            int idCotizacion = itemOC.IdCotizacion;


            _db.OrdenCompraDetalles.Remove(item);
            await _db.SaveChangesAsync();

            return Json(new
            {
                ok = true,
                data = await ObtenerDetalleAsync(idCotizacion, idOrdenCompra, null, idCotizacionDetalle)
            });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, msg = ex.Message });
        }
    }
}