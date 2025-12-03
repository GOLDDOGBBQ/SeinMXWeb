using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEINMX.Clases;
using SEINMX.Context;
using SEINMX.Context.Database;

[Authorize]
public class ClienteController : ApplicationController
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
// GUARDAR CLIENTE COMPLETO (Cliente + Contactos + Razones)
// =====================================================
    [HttpPost]
    public async Task<IActionResult> GuardarClienteCompleto([FromBody] Cliente model)
    {
        try
        {
            Cliente entity;

            // ================================================
            // NUEVO CLIENTE
            // ================================================
            if (model.IdCliente == 0)
            {
                entity = new Cliente
                {
                    Nombre = model.Nombre,
                    Direccion = model.Direccion,
                    Observaciones = model.Observaciones,
                    Tarifa = model.Tarifa,
                    IdTipo = model.IdTipo,
                    FchReg = DateTime.Now,
                    CreadoPor = GetApiName(),
                    UsrReg = GetUserId()
                };

                _db.Clientes.Add(entity);
                await _db.SaveChangesAsync(); // Necesario para generar ID
            }
            else
            {
                // ================================================
                // CLIENTE EXISTENTE
                // ================================================
                entity = await _db.Clientes
                    .Include(c => c.ClienteContactos)
                    .Include(c => c.ClienteRazonSolcials)
                    .FirstOrDefaultAsync(c => c.IdCliente == model.IdCliente);

                if (entity == null) return NotFound();

                entity.Nombre = model.Nombre;
                entity.Direccion = model.Direccion;
                entity.Observaciones = model.Observaciones;
                entity.Tarifa = model.Tarifa;
                entity.IdTipo = model.IdTipo;
                entity.ModificadoPor = GetApiName();
                entity.UsrAct = GetUserId();
                entity.FchAct = DateTime.Now;
            }

            // =====================================================
            // PROCESAR CONTACTOS
            // =====================================================
            var enviadosContactos = model.ClienteContactos;
            var existentesContactos = entity.ClienteContactos.ToList();

            // ELIMINAR CONTACTOS REMOVIDOS
            foreach (var item in existentesContactos)
            {
                if (enviadosContactos.All(c => c.IdClienteContacto != item.IdClienteContacto))
                    _db.ClienteContactos.Remove(item);
            }

            // AGREGAR / ACTUALIZAR CONTACTOS
            foreach (var c in enviadosContactos)
            {
                if (c.IdClienteContacto == 0)
                {
                    // Nuevo contacto
                    c.IdCliente = entity.IdCliente;
                    c.FchReg = DateTime.Now;
                    c.CreadoPor = GetApiName();
                    c.UsrReg = GetUserId();
                    _db.ClienteContactos.Add(c);
                }
                else
                {
                    // Actualizar contacto
                    var ct = existentesContactos
                        .FirstOrDefault(x => x.IdClienteContacto == c.IdClienteContacto);

                    if (ct != null)
                    {
                        ct.Nombre = c.Nombre;
                        ct.Telefono = c.Telefono;
                        ct.Correo = c.Correo;

                        ct.FchAct = DateTime.Now;
                        ct.ModificadoPor = GetApiName();
                        ct.UsrAct = GetUserId();
                    }
                }
            }

            // =====================================================
            // PROCESAR RAZONES SOCIALES
            // =====================================================
            var enviadosRazones = model.ClienteRazonSolcials;
            var existentesRazones = entity.ClienteRazonSolcials.ToList();

            // ELIMINAR RAZONES REMOVIDAS
            foreach (var item in existentesRazones)
            {
                if (enviadosRazones.All(r => r.IdClienteRazonSolcial != item.IdClienteRazonSolcial))
                    _db.ClienteRazonSolcials.Remove(item);
            }

            // AGREGAR / ACTUALIZAR RAZONES
            foreach (var r in enviadosRazones)
            {
                if (r.IdClienteRazonSolcial == 0)
                {
                    r.IdCliente = entity.IdCliente;
                    r.FchReg = DateTime.Now;
                    r.CreadoPor = GetApiName();
                    r.UsrReg = GetUserId();
                    _db.ClienteRazonSolcials.Add(r);
                }
                else
                {
                    var rz = existentesRazones
                        .FirstOrDefault(x => x.IdClienteRazonSolcial == r.IdClienteRazonSolcial);

                    if (rz != null)
                    {
                        rz.Rfc = r.Rfc;
                        rz.RazonSocial = r.RazonSocial;
                        rz.Domicilio = r.Domicilio;
                        rz.EsPublicoGeneral = r.EsPublicoGeneral;
                        rz.Observaciones = r.Observaciones;

                        rz.FchAct = DateTime.Now;
                        rz.ModificadoPor = GetApiName();
                        rz.UsrAct = GetUserId();

                    }
                }
            }

            // =====================================================
            // GUARDAR TODO
            // =====================================================
            await _db.SaveChangesAsync();

            return Ok(new
            {
                ok = true,
                cliente = new
                {
                    idCliente = entity.IdCliente,
                    nombre = entity.Nombre,
                    tarifa = entity.Tarifa,
                    idTipo = entity.IdTipo
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                ok = false,
                error = ex.Message,
                stack = ex.StackTrace
            });
        }
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
            model.CreadoPor = GetApiName();
            model.UsrReg = GetUserId();
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
            item.ModificadoPor = GetApiName();
            item.FchAct = DateTime.Now;
            item.UsrAct = GetUserId();
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
            model.CreadoPor = GetApiName();
            model.UsrReg = GetUserId();
            _db.ClienteContactos.Add(model);
        }
        else
        {
            var item = await _db.ClienteContactos.FindAsync(model.IdClienteContacto);
            if (item == null) return NotFound();

            item.Nombre = model.Nombre;
            item.Telefono = model.Telefono;
            item.Correo = model.Correo;
            item.ModificadoPor = GetApiName();
            item.FchAct = DateTime.Now;
            item.UsrAct = GetUserId();
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


    [HttpDelete]
    public async Task<IActionResult> BorrarContacto(int id)
    {
        var item = await _db.ClienteContactos.FindAsync(id);
        if (item == null) return NotFound();

        item.Eliminado = true;
        item.ModificadoPor = GetApiName();
        item.UsrAct = GetUserId();
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
            model.CreadoPor = GetApiName();
            model.UsrReg = GetUserId();
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
            item.ModificadoPor = GetApiName();
            item.FchAct = DateTime.Now;
            item.UsrAct = GetUserId();
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
        item.ModificadoPor = GetApiName();
        item.UsrAct = GetUserId();
        await _db.SaveChangesAsync();
        return Ok();
    }

    public async Task<JsonResult> Dropdown(int page = 0, int pageSize = 30, int? id = default, string search = "")
    {


        var lista = _db.Clientes.Where(x => x.Eliminado == false);


        if (!string.IsNullOrWhiteSpace(search))
        {
            lista.Where(x => x.Nombre.Contains(search));
        }

        if (id.HasValue)
        {
            lista = lista.Where(c => c.IdCliente == id.Value);
        }

        var count = await lista.CountAsync();
        var data = await lista.Skip(page * pageSize).Take(pageSize)
            .Select(drmCliente => new { display = drmCliente.Nombre, value = drmCliente.IdCliente.ToString() })
            .ToListAsync();
        var remaining = Math.Max(count - (page * pageSize) - data.Count, 0);

        return Json(new
        {
            moreToLoad = remaining > 0,
            data
        });
    }
}