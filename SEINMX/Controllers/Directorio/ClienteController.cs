using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SEINMX.Clases;
using SEINMX.Context;
using SEINMX.Context.Database;
using SEINMX.Models.Directorio;
using SEINMX.Models.Inventario;

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
        var query = _db.VsClientes
            .Where(x => !x.Eliminado)
            .AsQueryable();


        if (!GetIsAdmin())
        {
            var usr = GetUserId();
            query = query.Where(x => x.UsrReg == usr);
        }


        if (model.IdCliente != null)
        {
            query = query.Where(x => x.IdCliente == model.IdCliente);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(model.Nombre))
                query = query.Where(x => x.Nombre.Contains(model.Nombre));
        }

        var lista = await query
            .OrderByDescending(x => x.IdCliente)
            .ToListAsync();

        model.Clientes = lista;

        ViewBag.Perfiles = await ObtenerPerfilesAsync(true);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Crear([FromQuery] bool? esCotizable)
    {
        var model = new ClienteViewModel
        {
            esCotizable = esCotizable ?? false
        };

        ViewBag.Perfiles = await ObtenerPerfilesAsync();
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
            IdPerfil = cliente.IdPerfil,
            ClienteContactos = cliente.ClienteContactos.Select(m => new ClienteContactoViewModel
            {
                IdClienteContacto = m.IdClienteContacto, Nombre = m.Nombre, Telefono = m.Telefono, Correo = m.Correo
            }).ToList(),
            ClienteRazonSolcials = cliente.ClienteRazonSolcials.Select(m => new ClienteRazonSolcialViewModel
            {
                IdClienteRazonSolcial = m.IdClienteRazonSolcial, RFC = m.Rfc, RazonSocial = m.RazonSocial,
                Domicilio = m.Domicilio, CodigoPostal = m.CodigoPostal, EsPublicoGeneral = m.EsPublicoGeneral, Observaciones = m.Observaciones
            }).ToList()
        };

        ViewBag.Perfiles = await ObtenerPerfilesAsync();

        return View("Editar", model);
    }

    // =====================================================
    // GUARDAR CLIENTE COMPLETO (Cliente + Contactos + Razones)
    // =====================================================
    [HttpPost]
    public async Task<IActionResult> GuardarCompleto([FromBody] ClienteViewModel? model)
    {
        if (model is null)
            return BadRequest("Modelo nulo");

        if (!ModelState.IsValid)
        {
            var errores = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value?.Errors)
                .Select(e => e.ErrorMessage)
                .Distinct();

            string mensajeError = string.Join(" | ", errores);

            return BadRequest(new
            {
                ok = false,
                error = mensajeError,
            });
        }

        bool esCotizable = model.esCotizable;

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
                    IdPerfil = model.IdPerfil,
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
                    .FirstOrDefaultAsync(c => c.IdCliente == model.IdCliente) ?? throw new InvalidOperationException();

                entity.Nombre = model.Nombre;
                entity.Direccion = model.Direccion ?? "";
                entity.Observaciones = model.Observaciones ?? "";
                entity.IdPerfil = model.IdPerfil;
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
                        RazonSocial = r.RazonSocial.ToUpper(),
                        Domicilio = r.Domicilio ?? "",
                        CodigoPostal = r.CodigoPostal ?? "",
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
                        rz.RazonSocial = r.RazonSocial.ToUpper();
                        rz.Domicilio = r.Domicilio ?? "";
                        rz.CodigoPostal = r.CodigoPostal ?? "";
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
                    esCotizable = esCotizable,
                    nombre = entity.Nombre,
                    idPerfil = entity.IdPerfil
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

    [HttpDelete]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var item = await _db.Clientes.FindAsync(id);
            if (item == null) return Json(new { ok = false, error = "Cliente no encontrado" });

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
            lista = lista.Where(x => x.RazonSocial.Contains(search));
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

    private async Task<List<SelectListItem>> ObtenerPerfilesAsync(bool isFilter = false)
    {
        var lista = await _db.Perfils
            .Where(p => p.Eliminado == false)
            .Select(u => new SelectListItem
            {
                Value = u.IdPerfil.ToString(),
                Text = u.Perfil1
            })
            .OrderBy(u => u.Text)
            .ToListAsync();

        if (isFilter)
            lista.Insert(0, new SelectListItem("Todos", "0"));

        return lista;
    }
}