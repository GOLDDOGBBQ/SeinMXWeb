using Microsoft.AspNetCore.Mvc.Rendering;

namespace SEINMX.Models.Inventario;

public static class CombosFijos
{
    public static List<SelectListItem> GetComboStatus(int? status, bool isFilter)
    {
        var lista = new List<SelectListItem>
        {
            new SelectListItem("CREADA", "1", status == 1),
            new SelectListItem("COTIZADA", "2", status == 2),
            new SelectListItem("AUTORIZADA", "3", status == 3),
            new SelectListItem("PAGADA", "4", status == 4),
            new SelectListItem("EN PROCESO", "5", status == 5),
            new SelectListItem("CERRADA", "6", status == 6)
        };

        if (isFilter)
            lista.Insert(0, new SelectListItem("Todos", "0", status is null or 0));

        return lista;
    }


    public static List<SelectListItem> GetComboMoneda(int? idMoneda, bool isFilter)
    {
        var lista = new List<SelectListItem>
        {
            new SelectListItem("MXN", "1", idMoneda == 1),
            new SelectListItem("USD", "2", idMoneda == 2)
        };

        if (isFilter)
            lista.Insert(0, new SelectListItem("Todos", "0", idMoneda is null or 0));

        return lista;
    }
}