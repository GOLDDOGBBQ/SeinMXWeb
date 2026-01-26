using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEINMX.Clases;
using SEINMX.Context;
using SEINMX.Context.Database;
using SEINMX.Models.Directorio;

//namespace SEINMX.Controllers.Directorio;

[Authorize]
public class ProveedorController : ApplicationController
{
    private readonly AppDbContext _db;

    public ProveedorController(AppDbContext db)
    {
        _db = db;
    }

    // =====================================================
    // INDEX (LISTA + BUSCADOR MANUAL)
    // =====================================================
    public async Task<IActionResult> Index(ProveedorBuscadorViewModel model)
    {
        var query = _db.Proveedors
            .Where(x => !x.Eliminado)
            .AsQueryable();


        if (!GetIsAdmin())
        {
            var usr = GetUserId();
            query = query.Where(x => x.UsrReg == usr);
        }


        if (model.IdProveedor != null)
        {
            query = query.Where(x => x.IdProveedor == model.IdProveedor);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(model.Nombre))
                query = query.Where(x => x.Nombre.Contains(model.Nombre));
        }

        var lista = await query
            .OrderByDescending(x => x.IdProveedor)
            .ToListAsync();

        model.Proveedors = lista;

        return View(model);
    }

    public Task<IActionResult> Crear()
    {
        var model = new ProveedorViewModel(); // modelo vacío
        return Task.FromResult<IActionResult>(View("Editar", model));
    }

    // =====================================================
    // PANEL DETALLE (Proveedor + CONTACTOS + RAZONES SOC.)
    // =====================================================
    public async Task<IActionResult> Editar(int id)
    {
        var proveedor = await _db.Proveedors
            .FirstOrDefaultAsync(x => x.IdProveedor == id);

        if (proveedor == null)
            return NotFound();

        var model = new ProveedorViewModel
        {
            IdProveedor = proveedor.IdProveedor,
            Nombre = proveedor.Nombre,
            Direccion = proveedor.Direccion,
            Observaciones = proveedor.Observaciones,
            Tarifa = proveedor.Tarifa,
            TarifaGanancia = proveedor.TarifaGanancia,
            RFC = proveedor.Rfc,
            RazonSocial = proveedor.RazonSocial
        };

        return View("Editar", model);
    }

    // =====================================================
    // GUARDAR Proveedor COMPLETO (Proveedor + Contactos + Razones)
    // =====================================================
    [HttpPost]
    public async Task<IActionResult> GuardarCompleto(ProveedorViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["toast-error"] = "Revise los campos marcados.";
            return View("Editar",model);
        }

        try
        {
            Proveedor entity;

            // ================================================
            // NUEVO Proveedor
            // ================================================
            if ((model.IdProveedor ?? 0) == 0)
            {
                entity = new Proveedor
                {
                    Nombre = model.Nombre,
                    Direccion = model.Direccion ?? "",
                    Observaciones = model.Observaciones ?? "",
                    Tarifa = model.Tarifa,
                    TarifaGanancia = model.TarifaGanancia,
                    Rfc = model.RFC.ToUpper(),
                    RazonSocial = model.RazonSocial.ToUpper(),
                    FchReg = DateTime.Now,
                    CreadoPor = GetApiName(),
                    UsrReg = GetUserId()
                };

                _db.Proveedors.Add(entity);
                await _db.SaveChangesAsync(); // Necesario para generar ID
            }
            else
            {
                // ================================================
                // Proveedor EXISTENTE
                // ================================================
                entity = await _db.Proveedors
                             .FirstOrDefaultAsync(c => c.IdProveedor == model.IdProveedor) ??
                         throw new InvalidOperationException();

                entity.Nombre = model.Nombre;
                entity.Direccion = model.Direccion ?? "";
                entity.Observaciones = model.Observaciones ?? "";
                entity.Tarifa = model.Tarifa;
                entity.TarifaGanancia = model.TarifaGanancia;
                entity.Rfc = model.RFC.ToUpper();
                entity.RazonSocial = model.RazonSocial.ToUpper();

                entity.ModificadoPor = GetApiName();
                entity.UsrAct = GetUserId();
                entity.FchAct = DateTime.Now;
            }

            // =====================================================
            // GUARDAR_TODO
            // =====================================================
            await _db.SaveChangesAsync();
            TempData["toast-success"] = "Los datos fueron guardados correctamente.";

        }
        catch (Exception ex)
        {
            TempData["toast-error"] = "Error del servidor." + ex.Message;
        }

        return View("Editar",model);
    }

    [HttpDelete]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var item = await _db.Proveedors.FindAsync(id);
            if (item == null) return Json(new { ok = false, error = "Proveedor no encontrado" });

            item.Eliminado = true;
            item.ModificadoPor = GetApiName();
            item.UsrAct = GetUserId();
            item.FchAct = DateTime.Now;
            await _db.SaveChangesAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, error = ex.Message });
        }
    }


    public async Task<JsonResult> DropdownProveedors(int page = 0, int pageSize = 30, int? id = null,
        string search = "")
    {
        var lista = _db.Proveedors.Where(x => x.Eliminado == false);


        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            lista = lista.Where(x => EF.Functions.Like(x.Nombre, pattern));
        }

        if (id.HasValue)
        {
            lista = lista.Where(c => c.IdProveedor == id.Value);
        }

        var count = await lista.CountAsync();
        var data = await lista.Skip(page * pageSize).Take(pageSize)
            .Select(drmProveedor => new { display = drmProveedor.Nombre, value = drmProveedor.IdProveedor.ToString() })
            .ToListAsync();
        var remaining = Math.Max(count - (page * pageSize) - data.Count, 0);

        return Json(new
        {
            moreToLoad = remaining > 0,
            data
        });
    }

}