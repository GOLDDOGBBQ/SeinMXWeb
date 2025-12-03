using System.Security.Claims;
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
            IdUsuarioResponsable = int.Parse(GetUserId()),
            CreadoPor =  GetApiName(),
            FchReg = DateTime.Now,
            UsrReg = User.Identity?.Name ?? "SYSTEM"
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

        // Buscar vista
        vm.Cotizacion = await _db.VsCotizacions
            .FirstOrDefaultAsync(x => x.IdCotizacion == id);

        if (vm.Cotizacion == null)
            return NotFound();

        // Cargar detalles
        vm.Detalles = await _db.CotizacionDetalles
            .Where(x => x.IdCotizacion == id)
            .ToListAsync();

        return View("Editar", vm);
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
    // CREAR COTIZACION
    // =====================================================
    [HttpPost]
    public async Task<IActionResult> Crear3()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var entity = new Cotizacion
        {
            Fecha = DateOnly.FromDateTime(DateTime.Now),
            Status = 1,
            IdUsuarioResponsable = int.Parse(userId),
            IdCliente = 0,
            PorcentajeIva = 0.16m,
            CreadoPor = User.Identity?.Name ?? "",
            UsrReg = User.Identity?.Name ?? "",
            FchReg = DateTime.Now
        };

        _db.Cotizacions.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(new { ok = true, idCotizacion = entity.IdCotizacion });
    }

    // =====================================================
    // GUARDAR ENCABEZADO
    // =====================================================
    [HttpPost]
    public async Task<IActionResult> GuardarEncabezado([FromBody] Cotizacion model)
    {
        var item = await _db.Cotizacions.FindAsync(model.IdCotizacion);
        if (item == null) return NotFound();

        item.IdUsuarioResponsable = model.IdUsuarioResponsable;
        item.IdCliente = model.IdCliente;
        item.IdClienteeContacto = model.IdClienteeContacto;
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
