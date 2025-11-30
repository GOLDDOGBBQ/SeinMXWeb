using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEINMX.Context;
using SEINMX.Context.Database;

[Authorize]
public class ClienteController : Controller
{
    private readonly AppDbContext _db;

    public ClienteController(AppDbContext db)
    {
        _db = db;
    }

    // =====================================================
    // INDEX (LISTA + BUSCADOR MANUAL)
    // =====================================================
    public async Task<IActionResult> Index(int? idCliente, string? nombre, int? idTipo)
    {
        var query = _db.Clientes
            .Where(x => !x.Eliminado)
            .AsQueryable();

        // FILTRO # CLIENTE
        if (idCliente != null)
            query = query.Where(x => x.IdCliente == idCliente);

        // FILTRO NOMBRE
        if (!string.IsNullOrWhiteSpace(nombre))
            query = query.Where(x => x.Nombre.Contains(nombre));

        // FILTRO TIPO (0 cliente, 1 proveedor)
        if (idTipo != null)
            query = query.Where(x => x.IdTipo == idTipo);

        var lista = await query
            .OrderBy(x => x.Nombre)
            .ToListAsync();

        return View(lista);
    }


    public async Task<IActionResult> Crear()
    {
        var model = new Cliente(); // modelo vacío
        return PartialView("_PanelCliente", model);
    }

    // =====================================================
    // PANEL DETALLE (CLIENTE + CONTACTOS + RAZONES SOC.)
    // =====================================================
    public async Task<IActionResult> Detalle(int id)
    {
        var cliente = await _db.Clientes
            .Include(x => x.ClienteContactos.Where(c => !c.Eliminado))
            .Include(x => x.ClienteRazonSolcials.Where(r => !r.Eliminado))
            .FirstOrDefaultAsync(x => x.IdCliente == id);

        if (cliente == null)
            return NotFound();

        return PartialView("_PanelCliente", cliente);
    }


    // =====================================================
    // GUARDAR CLIENTE
    // =====================================================
    [HttpPost]
    public async Task<IActionResult> GuardarCliente([FromBody] Cliente model)
    {
        if (model.IdCliente == 0)
        {
            model.FchReg = DateTime.Now;
            model.CreadoPor = User.Identity?.Name ?? "";
            model.UsrReg = User.Identity?.Name ?? "";
            _db.Clientes.Add(model);
        }
        else
        {
            var item = await _db.Clientes.FindAsync(model.IdCliente);
            if (item == null) return NotFound();

            item.Nombre = model.Nombre;
            item.Direccion = model.Direccion;
            item.Observaciones = model.Observaciones;
            item.Tarifa = model.Tarifa;
            item.IdTipo = model.IdTipo;
            item.ModificadoPor = User.Identity?.Name ?? "";
            item.FchAct = DateTime.Now;
            item.UsrAct = User.Identity?.Name ?? "";
        }

        await _db.SaveChangesAsync();
        return Ok(new
        {
            ok = true,
            cliente = new
            {
                idCliente = model.IdCliente,
                nombre = model.Nombre,
                tarifa = model.Tarifa,
                idTipo = model.IdTipo
            }
        });
    }

    // =====================================================
    // CONTACTOS
    // =====================================================
    [HttpPost]
    public async Task<IActionResult> GuardarContacto([FromBody] ClienteContacto model)
    {
        if (model.IdClienteContacto == 0)
        {
            model.FchReg = DateTime.Now;
            model.CreadoPor = User.Identity?.Name ?? "";
            model.UsrReg = User.Identity?.Name ?? "";
            _db.ClienteContactos.Add(model);
        }
        else
        {
            var item = await _db.ClienteContactos.FindAsync(model.IdClienteContacto);
            if (item == null) return NotFound();

            item.Nombre = model.Nombre;
            item.Telefono = model.Telefono;
            item.Correo = model.Correo;
            item.ModificadoPor = User.Identity?.Name ?? "";
            item.FchAct = DateTime.Now;
            item.UsrAct = User.Identity?.Name ?? "";
        }

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }


    [HttpDelete]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var item = await _db.Clientes.FindAsync(id);
            if (item == null) return Json(new { ok = false, mensaje = "Cliente no encontrado" });

            item.Eliminado = true;
            item.UsrAct = User.Identity?.Name ?? "";
            item.FchAct = DateTime.Now;
            await _db.SaveChangesAsync();
            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, mensaje = ex.Message });
        }
    }





    [HttpDelete]
    public async Task<IActionResult> BorrarContacto(int id)
    {
        var item = await _db.ClienteContactos.FindAsync(id);
        if (item == null) return NotFound();

        item.Eliminado = true;
        await _db.SaveChangesAsync();
        return Ok();
    }

    // =====================================================
    // RAZONES SOCIALES
    // =====================================================
    [HttpPost]
    public async Task<IActionResult> GuardarRazon([FromBody] ClienteRazonSolcial model)
    {
        if (model.IdClienteRazonSolcial == 0)
        {
            model.FchReg = DateTime.Now;
            model.CreadoPor = User.Identity?.Name ?? "";
            model.UsrReg = User.Identity?.Name ?? "";
            _db.ClienteRazonSolcials.Add(model);
        }
        else
        {
            var item = await _db.ClienteRazonSolcials.FindAsync(model.IdClienteRazonSolcial);
            if (item == null) return NotFound();

            item.Rfc = model.Rfc;
            item.RazonSocial = model.RazonSocial;
            item.EsPublicoGeneral = model.EsPublicoGeneral;
            item.Domicilio = model.Domicilio;
            item.Observaciones = model.Observaciones;
            item.ModificadoPor = User.Identity?.Name ?? "";
            item.FchAct = DateTime.Now;
            item.UsrAct = User.Identity?.Name ?? "";
        }

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }

    [HttpDelete]
    public async Task<IActionResult> BorrarRazon(int id)
    {
        var item = await _db.ClienteRazonSolcials.FindAsync(id);
        if (item == null) return NotFound();

        item.Eliminado = true;
        await _db.SaveChangesAsync();
        return Ok();
    }

    // =====================================================
    // VISTAS BÁSICAS (para evitar errores 404)
    // =====================================================
    public IActionResult Create() => View();
    public IActionResult Edit(int id) => View();
    public IActionResult Details(int id) => View();
    public IActionResult Delete(int id) => View();
}