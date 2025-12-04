using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEINMX.Clases;
using SEINMX.Context;
using SEINMX.Context.Database;
using SEINMX.Models.Inventario;

[Authorize]
public class CotizacionController : ApplicationController
{
    private readonly AppDbContext _db;

    public CotizacionController(AppDbContext db)
    {
        _db = db;
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


    [HttpPost]
    public async Task<IActionResult> Guardar(CotizacionViewModel model)
    {
        /*if (!ModelState.Cotizacion.IsValid)
        {
            ViewBag.Error = "El formulario contiene errores.";
            return View("Editar", model);
        }*/

        if (model.Cotizacion is null || model.Cotizacion is { IdCotizacion: 0 })
        {
            ViewBag.Error = "No se recibio el numero de cotizacion";
            return View("Editar", model);
        }


        try
        {
            // Validar que exista
            var cot = await _db.Cotizacions
                .FirstOrDefaultAsync(x => model.Cotizacion != null && x.IdCotizacion == model.Cotizacion.IdCotizacion);

            if (cot == null)
            {
                ViewBag.Error = "La cotización no existe o fue eliminada.";
                return View("Editar", model);
            }

            // === ACTUALIZAR CAMPOS EDITABLES ===
            cot.Fecha = model.Cotizacion?.Fecha;
            cot.TipoCambio = model.Cotizacion.TipoCambio;
            cot.Tarifa = model.Cotizacion.Tarifa;
            cot.PorcentajeIva = model.Cotizacion.PorcentajeIva;
            cot.Descuento = model.Cotizacion.Descuento;
            cot.Observaciones = model.Cotizacion.Observaciones;

            cot.UsuarioResponsable = model.Cotizacion.UsuarioResponsable;
            cot.IdCliente = model.Cotizacion.IdCliente;
            cot.IdClienteContacto = model.Cotizacion.IdClienteContacto;
            cot.IdClienteRazonSolcial = model.Cotizacion.IdClienteRazonSolcial;

            // === CAMPOS AUTOMÁTICOS DE MODIFICACIÓN ===
            cot.ModificadoPor = GetApiName();
            cot.FchAct = DateTime.Now;
            cot.UsrAct = GetUserId();

            await _db.SaveChangesAsync();

            TempData["Success"] = "Los datos fueron guardados correctamente.";
            return RedirectToAction("Editar", new { id = cot.IdCotizacion });
        }
        catch (Exception ex)
        {
            // En caso de error, regresar al formulario SIN PERDER DATOS
            ViewBag.Error = "Error al guardar: " + ex.Message;
            return View("Editar", model);
        }
    }


    // =====================================================
    // PANEL PRINCIPAL (CREAR / EDITAR)
    // =====================================================

    public async Task<IActionResult> Panel(int? id)
    {
        if (id == null)
        {
            // Modo CREAR – La vista mostrará botón "Crear Cotización"
            return View("Panel", null);
        }

        // Modo EDITAR
        var model = await _db.VsCotizacions
            .FirstOrDefaultAsync(x => x.IdCotizacion == id);

        if (model == null)
            return NotFound();

        return View("Panel", model);
    }


    // =====================================================
    // GUARDAR ENCABEZADO
    // =====================================================
    [HttpPost]
    public async Task<IActionResult> GuardarEncabezado([FromBody] Cotizacion model)
    {
        var item = await _db.Cotizacions.FindAsync(model.IdCotizacion);
        if (item == null) return NotFound();

        item.UsuarioResponsable = model.UsuarioResponsable;
        item.IdCliente = model.IdCliente;
        item.IdClienteContacto = model.IdClienteContacto;
        item.IdClienteRazonSolcial = model.IdClienteRazonSolcial;
        item.Observaciones = model.Observaciones;
        item.Tarifa = model.Tarifa;
        item.TipoCambio = model.TipoCambio;
        item.PorcentajeIva = model.PorcentajeIva;
        item.Descuento = model.Descuento;

        item.ModificadoPor = User.Identity?.Name ?? "";
        item.FchAct = DateTime.Now;
        item.UsrAct = User.Identity?.Name ?? "";

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }


    // =====================================================
    // GUARDAR PRODUCTO (CREATE / UPDATE)
    // =====================================================
    [HttpPost]
    public async Task<IActionResult> GuardarProducto([FromBody] CotizacionDetalle model)
    {
        CotizacionDetalle item;

        if (model.IdCotizacionDetalle == 0)
        {
            item = model;
            item.CreadoPor = User.Identity?.Name ?? "";
            item.UsrReg = User.Identity?.Name ?? "";
            item.FchReg = DateTime.Now;
            _db.CotizacionDetalles.Add(item);
        }
        else
        {
            item = await _db.CotizacionDetalles.FindAsync(model.IdCotizacionDetalle);
            if (item == null) return NotFound();

            item.IdProducto = model.IdProducto;
            item.Cantidad = model.Cantidad;
            item.PrecioCliente = model.PrecioCliente;
            item.Total = model.Total;
            item.Observaciones = model.Observaciones;
            item.PrecioListaMxn = model.PrecioListaMxn;
            item.PrecioProveedor = model.PrecioProveedor;
            item.GananciaProveedor = model.GananciaProveedor;
            item.PorcentajeProveedor = model.PorcentajeProveedor;
            item.PorcentajeProveedorGanancia = model.PorcentajeProveedorGanancia;
            item.IdMoneda = model.IdMoneda;
            item.PrecioSein = model.PrecioSein;

            item.ModificadoPor = User.Identity?.Name ?? "";
            item.UsrAct = User.Identity?.Name ?? "";
            item.FchAct = DateTime.Now;
        }

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }


    // =====================================================
    // ELIMINAR PRODUCTO
    // =====================================================
    [HttpDelete]
    public async Task<IActionResult> EliminarProducto(int id)
    {
        var item = await _db.CotizacionDetalles.FindAsync(id);
        if (item == null) return NotFound();

        _db.CotizacionDetalles.Remove(item);
        await _db.SaveChangesAsync();

        return Ok(new { ok = true });
    }

    // =====================================================
    // LISTA PRODUCTOS DE LA COTIZACION
    // =====================================================
    public async Task<IActionResult> Productos(int idCotizacion)
    {
        var data = await _db.CotizacionDetalles
            .Where(x => x.IdCotizacion == idCotizacion)
            .ToListAsync();

        return PartialView("_Productos", data);
    }
}