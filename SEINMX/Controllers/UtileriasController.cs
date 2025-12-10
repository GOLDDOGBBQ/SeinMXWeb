using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEINMX.Context;

namespace SEINMX.Controllers;

public class UtileriasController : Controller
{
    private readonly AppDbContext _db;

    public UtileriasController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetComboUsuarios(string? id, string? term)
    {
        var lista = _db.Usuarios.Where(x => x.Eliminado == false);

        if (!string.IsNullOrWhiteSpace(term))
        {
          lista = lista.Where(x => x.Nombre.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(id))
        {
          lista =  lista.Where(x => x.Usuario1 == id);
        }

        var usuarios = await lista.ToListAsync();

        return Ok(usuarios);
    }


}