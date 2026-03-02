using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEINMX.Clases;
using SEINMX.Context;
using SEINMX.Context.Database;
using SEINMX.Models.Finanzas;

[Authorize]
public class MovimientoFinancieroController : ApplicationController
{
    private readonly AppDbContext _db;

    public MovimientoFinancieroController(AppDbContext db) => _db = db;

    // =====================================================
    // INDEX
    // =====================================================
    public async Task<IActionResult> Index(MovimientoFinancieroFilterViewModel filter)
    {
        if (!GetIsAdmin())
        {
            return Unauthorized();
        }


        var query = _db.MovimientoFinancieros
            .Where(x => !x.Eliminado)
            .Include(x => x.IdProveedorNavigation)
            .AsQueryable();

        if (DateOnly.TryParseExact(filter.FechaDesde, "yyyy-MM-dd", null,
                System.Globalization.DateTimeStyles.None, out var fechaDesde))
            query = query.Where(x => x.Fecha >= fechaDesde);

        if (DateOnly.TryParseExact(filter.FechaHasta, "yyyy-MM-dd", null,
                System.Globalization.DateTimeStyles.None, out var fechaHasta))
            query = query.Where(x => x.Fecha <= fechaHasta);

        if (!string.IsNullOrWhiteSpace(filter.ProveedorNombre))
        {
            var pat = $"%{filter.ProveedorNombre.Trim()}%";
            query = query.Where(x => x.IdProveedorNavigation != null &&
                                     EF.Functions.Like(x.IdProveedorNavigation.Nombre, pat));
        }

        if (filter.MontoMin.HasValue)
            query = query.Where(x => x.Monto >= filter.MontoMin.Value);

        if (filter.MontoMax.HasValue)
            query = query.Where(x => x.Monto <= filter.MontoMax.Value);

        if (filter.PendienteFacturar.HasValue)
            query = query.Where(x => x.PendienteFacturar == filter.PendienteFacturar.Value);

        if (filter.Tipo.HasValue && filter.Tipo.Value > 0)
            query = query.Where(x => x.Tipo == filter.Tipo.Value);

        filter.Items = await query
            .OrderBy(x => x.Orden)
            .Select(x => new MovimientoFinancieroRowViewModel
            {
                IdMovimientoFinanciero = x.IdMovimientoFinanciero,
                Tipo                   = x.Tipo,
                Fecha                  = x.Fecha,
                Descripcion            = x.Descripcion,
                Monto                  = x.Monto,
                IdProveedor            = x.IdProveedor,
                ProveedorNombre        = x.IdProveedorNavigation != null ? x.IdProveedorNavigation.Nombre : null,
                Factura                = x.Factura,
                PendienteFacturar      = x.PendienteFacturar,
                Orden                  = x.Orden
            })
            .ToListAsync();

        ViewBag.Proveedores = await _db.Proveedors
            .Where(p => !p.Eliminado)
            .OrderBy(p => p.Nombre)
            .Select(p => new { id = p.IdProveedor, nombre = p.Nombre })
            .ToListAsync();

        return View(filter);
    }

    // =====================================================
    // GUARDAR (CREATE + UPDATE)
    // =====================================================
    [HttpPost]
    public async Task<IActionResult> Guardar([FromBody] MovimientoFinancieroSaveRequest? req)
    {
        if (req is null)
            return BadRequest(new { ok = false, msg = "Modelo nulo" });

        if (!DateOnly.TryParseExact(req.FechaStr, "yyyy-MM-dd", null,
                System.Globalization.DateTimeStyles.None, out var fecha))
            return BadRequest(new { ok = false, msg = "Fecha inválida" });

        try
        {
            MovimientoFinanciero entity;

            if ((req.IdMovimientoFinanciero ?? 0) == 0)
            {
                var maxOrden = await _db.MovimientoFinancieros
                    .Where(x => !x.Eliminado)
                    .MaxAsync(x => (int?)x.Orden) ?? 0;

                entity = new MovimientoFinanciero
                {
                    Tipo              = req.Tipo,
                    Fecha             = fecha,
                    Descripcion       = req.Descripcion.Trim(),
                    Monto             = req.Monto,
                    IdProveedor       = req.IdProveedor,
                    Factura           = (req.Factura ?? "").Trim(),
                    PendienteFacturar = req.PendienteFacturar,
                    Orden             = maxOrden + 1,
                    FchReg            = DateTime.Now,
                    CreadoPor         = GetApiName(),
                    UsrReg            = GetUserId()
                };

                _db.MovimientoFinancieros.Add(entity);
            }
            else
            {
                entity = await _db.MovimientoFinancieros.FindAsync(req.IdMovimientoFinanciero)
                    ?? throw new InvalidOperationException("Registro no encontrado");

                entity.Tipo              = req.Tipo;
                entity.Fecha             = fecha;
                entity.Descripcion       = req.Descripcion.Trim();
                entity.Monto             = req.Monto;
                entity.IdProveedor       = req.IdProveedor;
                entity.Factura           = (req.Factura ?? "").Trim();
                entity.PendienteFacturar = req.PendienteFacturar;
                entity.FchAct            = DateTime.Now;
                entity.ModificadoPor     = GetApiName();
                entity.UsrAct            = GetUserId();
            }

            await _db.SaveChangesAsync();

            string? proveedorNombre = null;
            if (entity.IdProveedor.HasValue)
                proveedorNombre = await _db.Proveedors
                    .Where(p => p.IdProveedor == entity.IdProveedor)
                    .Select(p => p.Nombre)
                    .FirstOrDefaultAsync();

            return Ok(new
            {
                ok = true,
                data = new
                {
                    entity.IdMovimientoFinanciero,
                    entity.Tipo,
                    Fecha             = entity.Fecha.ToString("yyyy-MM-dd"),
                    entity.Descripcion,
                    entity.Monto,
                    entity.IdProveedor,
                    ProveedorNombre   = proveedorNombre,
                    entity.Factura,
                    entity.PendienteFacturar,
                    entity.Orden
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ok = false, msg = ex.Message });
        }
    }

    // =====================================================
    // ELIMINAR
    // =====================================================
    [HttpDelete]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var item = await _db.MovimientoFinancieros.FindAsync(id);
            if (item == null)
                return Json(new { ok = false, msg = "Registro no encontrado" });

            item.Eliminado    = true;
            item.ModificadoPor = GetApiName();
            item.UsrAct       = GetUserId();
            item.FchAct       = DateTime.Now;

            await _db.SaveChangesAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ok = false, msg = ex.Message });
        }
    }

    // =====================================================
    // REORDENAR (DRAG & DROP)
    // =====================================================
    [HttpPost]
    public async Task<IActionResult> Reordenar([FromBody] ReordenarRequest? req)
    {
        if (req?.Ids == null || !req.Ids.Any())
            return BadRequest(new { ok = false, msg = "Lista de IDs vacía" });

        try
        {
            var entities = await _db.MovimientoFinancieros
                .Where(x => req.Ids.Contains(x.IdMovimientoFinanciero))
                .ToListAsync();

            for (int i = 0; i < req.Ids.Count; i++)
            {
                var e = entities.FirstOrDefault(x => x.IdMovimientoFinanciero == req.Ids[i]);
                if (e == null) continue;
                e.Orden          = i + 1;
                e.FchAct         = DateTime.Now;
                e.ModificadoPor  = GetApiName();
                e.UsrAct         = GetUserId();
            }

            await _db.SaveChangesAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ok = false, msg = ex.Message });
        }
    }
}