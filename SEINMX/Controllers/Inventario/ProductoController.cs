using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEINMX.Clases;
using SEINMX.Context;
using SEINMX.Models.Inventario;
using System.Text.Json;
using SEINMX.Context.Database;


namespace SEINMX.Controllers.Inventario;

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
        var query = _db.VsProductos.OrderByDescending(x => x.IdProducto).AsQueryable();

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
}