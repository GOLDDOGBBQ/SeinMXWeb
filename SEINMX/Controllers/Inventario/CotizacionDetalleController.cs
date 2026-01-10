using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEINMX.Clases;
using SEINMX.Context;
using SEINMX.Context.Database;


namespace SEINMX.Controllers.Inventario
{
    [Authorize]
    public class CotizacionDetalleController : ApplicationController
    {
        private readonly AppDbContext _db;

        public CotizacionDetalleController(AppDbContext db)
        {
            _db = db;
        }

        // ======================================================
        // 5.1 AGREGAR PRODUCTO
        // ======================================================
        [HttpPost]
        public async Task<IActionResult> Agregar([FromBody] CotizacionDetalle model)
        {
            try
            {
                if (model.IdCotizacion == 0)
                    return BadRequest("La cotización no existe.");

                // Cálculos
                model.Total = model.Cantidad * model.PrecioCliente;

                // Auditoría
                model.CreadoPor = GetApiName();
                model.FchReg = DateTime.Now;
                model.UsrReg = GetUserId();

                _db.CotizacionDetalles.Add(model);
                await _db.SaveChangesAsync();

                return Ok(new { ok = true, id = model.IdCotizacionDetalle });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        // ======================================================
        // 5.2 ACTUALIZAR PRODUCTO
        // ======================================================
        [HttpPost]
        public async Task<IActionResult> Editar([FromBody] CotizacionDetalle model)
        {
            try
            {
                var item = await _db.CotizacionDetalles
                    .FirstOrDefaultAsync(x => x.IdCotizacionDetalle == model.IdCotizacionDetalle);

                if (item == null)
                    return NotFound("El producto no existe.");

                // Actualizar campos
                item.IdProducto = model.IdProducto;
                item.Cantidad = model.Cantidad;
                item.PrecioCliente = model.PrecioCliente;
                item.Total = model.Cantidad * model.PrecioCliente;
                item.Observaciones = model.Observaciones;

                // Auditoría
                item.ModificadoPor = GetApiName();
                item.FchAct = DateTime.Now;
                item.UsrAct = GetUserId();

                await _db.SaveChangesAsync();

                return Ok(new { ok = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        // ======================================================
        // 5.3 ELIMINAR PRODUCTO
        // ======================================================
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var item = await _db.CotizacionDetalles
                    .FirstOrDefaultAsync(x => x.IdCotizacionDetalle == id);

                if (item == null)
                    return NotFound("No existe el producto que intenta eliminar.");

                _db.CotizacionDetalles.Remove(item);
                await _db.SaveChangesAsync();

                return Ok(new { ok = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }
    }
}
