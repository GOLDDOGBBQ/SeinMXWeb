using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEINMX.Clases;
using SEINMX.Context;
using SEINMX.Models.Inventario;
using Microsoft.AspNetCore.Authorization;
using SEINMX.Context.Database;


namespace SEINMX.Controllers.Inventario;
[Authorize]
public class ProductoController : ApplicationController
{
    private readonly AppDbContext _db;
    private readonly AppClassContext _ClasContext;

    public ProductoController(AppDbContext db, AppClassContext clasContext)
    {
        _db = db;
        _ClasContext = clasContext;
    }


    // GET
    public async Task<IActionResult> Index(ProductoBuscadorViewModel model)
    {
        var query = _db.VsProductos.Where(x => x.Eliminado == false);

        if (model.IdProducto != null)
        {
            query = query.Where(x => x.IdProducto == model.IdProducto);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(model.Descripcion))
                query = query.Where(x => x.Descripcion.Contains(model.Descripcion));


            if (!string.IsNullOrWhiteSpace(model.Codigo))
                query = query.Where(x => x.Codigo.Contains(model.Codigo));
        }

        var lista = await query
            .OrderByDescending(x => x.IdProducto)
            .ToListAsync();

        model.Productos = lista;

        return View(model);
    }


    public async Task<IActionResult> Crear()
    {
        return RedirectToAction("Editar");
    }

    // [HttpGet("EditarProducto/{id?}")]
    public async Task<IActionResult> Editar(int? id)
    {
        if (id == null)
        {
            ProductoViewModel modelEmpy = new();
            return View("Editar", modelEmpy);
        }

        var entity = await _db.VsProductos
            .FirstOrDefaultAsync(x => x.IdProducto == id);

        if (entity == null)
            return NotFound();

        var model = new ProductoViewModel
        {
            IdProducto = entity.IdProducto,
            IdProveedor = entity.IdProveedor,
            IdMoneda = entity.IdMoneda,

            Codigo = entity.Codigo,
            CodigoProveedor = entity.CodigoProveedor,
            Descripcion = entity.Descripcion,

            PrecioLista = entity.PrecioLista,

            ClaveUnidadSAT = entity.ClaveUnidadSat,
            Observaciones = entity.Observaciones
        };


        return View("Editar", model);
    }

    [HttpPost]
    public async Task<IActionResult> Guardar(ProductoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["toast-error"] = "Revise los campos marcados.";
            return View("Editar", model);
        }


        try
        {
            if (model.IdProducto is null or 0)
            {
                var item = new Producto
                {
                    IdProveedor = model.IdProveedor,
                    Codigo = model.Codigo,
                    CodigoProveedor = model.CodigoProveedor,
                    IdMoneda = model.IdMoneda,
                    Descripcion = model.Descripcion,
                    Observaciones = model.Observaciones ?? "",
                    ClaveUnidadSat = model.ClaveUnidadSAT ?? "",
                    PrecioLista = model.PrecioLista,
                    Eliminado = false,

                    FchReg = DateTime.Now,
                    CreadoPor = GetApiName(),
                    UsrReg = GetUserId()
                };

                _db.Productos.Add(item);
            }
            else
            {
                var item = await _db.Productos.FindAsync(model.IdProducto);
                if (item == null) return NotFound();

                item.IdProveedor = model.IdProveedor;
                item.Codigo = model.Codigo;
                item.CodigoProveedor = model.CodigoProveedor;
                item.IdMoneda = model.IdMoneda;
                item.Descripcion = model.Descripcion;
                item.Observaciones = model.Observaciones ?? "";
                item.ClaveUnidadSat = model.ClaveUnidadSAT ?? "";
                item.PrecioLista = model.PrecioLista;

                item.ModificadoPor = GetApiName();
                item.FchAct = DateTime.Now;
                item.UsrAct = GetUserId();
            }

            await _db.SaveChangesAsync();

            TempData["toast-success"] = "Los datos fueron guardados correctamente.";
            return View("Editar", model);

        }
        catch (Exception ex)
        {
            TempData["toast-error"] = "Error del servidor." + ex.Message;
            return View("Editar", model);
        }
    }
    [HttpDelete]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var item = await _db.Productos.FindAsync(id);
            if (item == null) return Json(new { ok = false, mensaje = "Producto no encontrado" });

            item.Eliminado = true;
            item.ModificadoPor = GetApiName();
            item.UsrAct = GetUserId();
            item.FchAct = DateTime.Now;
            await _db.SaveChangesAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, mensaje = ex.Message });
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

}