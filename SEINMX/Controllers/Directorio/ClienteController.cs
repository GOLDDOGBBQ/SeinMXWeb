using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEINMX.Clases;
using SEINMX.Context;
using SEINMX.Context.Database;
using SEINMX.Models.Directorio;

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
    public async Task<IActionResult> Index(ClienteBuscadorViewModel model)
    {
        var query = _db.Clientes
            .Where(x => !x.Eliminado)
            .AsQueryable();


        if (model.IdCliente != null)
        {
            query = query.Where(x => x.IdCliente == model.IdCliente);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(model.Nombre))
                query = query.Where(x => x.Nombre.Contains(model.Nombre));


            if ((model.IdTipo ?? 0) > 0)
                query = query.Where(x => x.IdTipo == model.IdTipo);
        }

        var lista = await query
            .OrderByDescending(x => x.IdCliente)
            .ToListAsync();

        model.Clientes = lista;

        return View(model);
    }


    public async Task<IActionResult> Crear()
    {
        var model = new ClienteViewModel(); // modelo vacío
        return View("Editar", model);
    }

    // =====================================================
    // PANEL DETALLE (CLIENTE + CONTACTOS + RAZONES SOC.)
    // =====================================================
    public async Task<IActionResult> Editar(int id)
    {
        var cliente = await _db.Clientes
            .Include(x => x.ClienteContactos.Where(c => !c.Eliminado))
            .Include(x => x.ClienteRazonSolcials.Where(r => !r.Eliminado))
            .FirstOrDefaultAsync(x => x.IdCliente == id);

        if (cliente == null)
            return NotFound();

        var model = new ClienteViewModel
        {
            IdCliente = cliente.IdCliente,
            Nombre = cliente.Nombre,
            Direccion = cliente.Direccion,
            Observaciones = cliente.Observaciones,
            Tarifa = cliente.Tarifa,
            IdTipo = cliente.IdTipo,
            TarifaGanancia = cliente.TarifaGanancia,
            ClienteContactos = cliente.ClienteContactos.Select(m => new ClienteContactoViewModel
                { IdClienteContacto = m.IdClienteContacto, Nombre = m.Nombre, Telefono = m.Telefono , Correo = m.Correo}).ToList(),
            ClienteRazonSolcials = cliente.ClienteRazonSolcials.Select(m => new ClienteRazonSolcialViewModel
                { IdClienteRazonSolcial = m.IdClienteRazonSolcial, RFC = m.Rfc, RazonSocial = m.RazonSocial, Domicilio = m.Domicilio, EsPublicoGeneral = m.EsPublicoGeneral , Observaciones = m.Observaciones  }).ToList()
        };

        return View("Editar", model);
    }


    // =====================================================
// GUARDAR CLIENTE COMPLETO (Cliente + Contactos + Razones)
// =====================================================
    [HttpPost]
    public async Task<IActionResult> GuardarClienteCompleto([FromBody] ClienteViewModel model)
    {
        if (model == null)
            return BadRequest("Modelo nulo");

        /*if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                ok = false,
                error = "El formulario no es valido",

            });
        }*/


        try
        {
            Cliente entity;

            // ================================================
            // NUEVO CLIENTE
            // ================================================
            if ((model.IdCliente ?? 0) == 0)
            {
                entity = new Cliente
                {
                    Nombre = model.Nombre,
                    Direccion = model.Direccion ?? "",
                    Observaciones = model.Observaciones ?? "",
                    Tarifa = model.Tarifa,
                    TarifaGanancia = model.TarifaGanancia,
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
                entity.Direccion = model.Direccion ?? "";
                entity.Observaciones = model.Observaciones ?? "";
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
                {
                    item.Eliminado = true;
                    item.ModificadoPor = GetApiName();
                    item.UsrAct = GetUserId();
                    item.FchAct = DateTime.Now;
                }

            }

            // AGREGAR / ACTUALIZAR CONTACTOS
            foreach (var c in enviadosContactos)
            {
                if ((c.IdClienteContacto ?? 0) == 0)
                {
                    var modelContacto = new ClienteContacto
                    {
                        IdCliente = entity.IdCliente,
                        Nombre = c.Nombre,
                        Telefono = c.Telefono ?? "",
                        Correo = c.Correo ?? "",
                        FchReg = DateTime.Now,
                        CreadoPor = GetApiName(),
                        UsrReg = GetUserId()
                    };


                    _db.ClienteContactos.Add(modelContacto);
                }
                else
                {
                    // Actualizar contacto
                    var ct = existentesContactos
                        .FirstOrDefault(x => x.IdClienteContacto == c.IdClienteContacto);

                    if (ct != null)
                    {
                        ct.Nombre = c.Nombre;
                        ct.Telefono = c.Telefono ?? "";
                        ct.Correo = c.Correo ?? "";

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
                if (enviadosRazones.All(c => c.IdClienteRazonSolcial != item.IdClienteRazonSolcial))
                {
                    item.Eliminado = true;
                    item.ModificadoPor = GetApiName();
                    item.UsrAct = GetUserId();
                    item.FchAct = DateTime.Now;
                }
            }

            // AGREGAR / ACTUALIZAR RAZONES
            foreach (var r in enviadosRazones)
            {
                if ((r.IdClienteRazonSolcial ?? 0) == 0)
                {
                    var modelClienteRazonSolcial = new ClienteRazonSolcial
                    {
                        IdCliente = entity.IdCliente,
                        Rfc = r.RFC,
                        RazonSocial = r.RazonSocial,
                        Domicilio = r.Domicilio ?? "",
                        EsPublicoGeneral = r.EsPublicoGeneral ?? false,
                        Observaciones = r.Observaciones ?? "",
                        FchReg = DateTime.Now,
                        CreadoPor = GetApiName(),
                        UsrReg = GetUserId()
                    };


                    _db.ClienteRazonSolcials.Add(modelClienteRazonSolcial);
                }
                else
                {
                    var rz = existentesRazones
                        .FirstOrDefault(x => x.IdClienteRazonSolcial == r.IdClienteRazonSolcial);

                    if (rz != null)
                    {
                        rz.RazonSocial = r.RazonSocial;
                        rz.Domicilio = r.Domicilio ?? "";
                        rz.EsPublicoGeneral = r.EsPublicoGeneral ?? false;
                        rz.Observaciones = r.Observaciones ?? "";

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

    public async Task<JsonResult> DropdownClientes(int page = 0, int pageSize = 30, int? id = null, string search = "")
    {
        var lista = _db.Clientes.Where(x => x.Eliminado == false);


        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            lista = lista.Where(x => EF.Functions.Like(x.Nombre, pattern));
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

    public async Task<JsonResult> DropdownClienteContacto(int page = 0, int pageSize = 30, int? id = null,
        string search = "", int? idCliente = null)
    {
        var lista = _db.ClienteContactos.Where(x => x.Eliminado == false);


        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            lista = lista.Where(x => EF.Functions.Like(x.Nombre, pattern));
        }

        if (id.HasValue)
        {
            lista = lista.Where(c => c.IdClienteContacto == id.Value);
        }

        if (idCliente.HasValue)
        {
            lista = lista.Where(c => c.IdCliente == idCliente.Value);
        }

        var count = await lista.CountAsync();
        var data = await lista.Skip(page * pageSize).Take(pageSize)
            .Select(drmCliente => new { display = drmCliente.Nombre, value = drmCliente.IdClienteContacto.ToString() })
            .ToListAsync();
        var remaining = Math.Max(count - (page * pageSize) - data.Count, 0);

        return Json(new
        {
            moreToLoad = remaining > 0,
            data
        });
    }

    public async Task<JsonResult> DropdownClienteRazonSolcial(int page = 0, int pageSize = 30, int? id = default,
        string search = "", int? idCliente = null)
    {
        var lista = _db.ClienteRazonSolcials.Where(x => x.Eliminado == false);


        if (!string.IsNullOrWhiteSpace(search))
        {
            lista.Where(x => x.RazonSocial.Contains(search));
        }

        if (id.HasValue)
        {
            lista = lista.Where(c => c.IdClienteRazonSolcial == id.Value);
        }

        if (idCliente.HasValue)
        {
            lista = lista.Where(c => c.IdCliente == idCliente.Value);
        }

        var count = await lista.CountAsync();
        var data = await lista.Skip(page * pageSize).Take(pageSize)
            .Select(drmCliente => new
                { display = drmCliente.RazonSocial, value = drmCliente.IdClienteRazonSolcial.ToString() })
            .ToListAsync();
        var remaining = Math.Max(count - (page * pageSize) - data.Count, 0);

        return Json(new
        {
            moreToLoad = remaining > 0,
            data
        });
    }
}