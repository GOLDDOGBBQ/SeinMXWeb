using Microsoft.AspNetCore.Mvc.Rendering;

namespace SEINMX.Models.Inventario;

public static class CotizacionComboStatus
{
    public static List<SelectListItem> GetComboStatus(int? Status, bool isFilter )
    {
       var lista = new List<SelectListItem>
        {
            new SelectListItem("CREADA", "1", Status == 1),
            new SelectListItem("COTIZADA", "2", Status == 2),
            new SelectListItem("AUTORIZADA", "3", Status == 3),
            new SelectListItem("PAGADA", "4", Status == 4),
            new SelectListItem("EN PROCESO", "5", Status == 5),
            new SelectListItem("CERRADA", "6", Status == 6)
        };

       if (isFilter)
           lista.Insert(0, new SelectListItem("Todos", "", Status is null or 0  ));

        return lista;
    }
}