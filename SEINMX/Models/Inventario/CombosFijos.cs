using Microsoft.AspNetCore.Mvc.Rendering;

namespace SEINMX.Models.Inventario;

public static class CombosFijos
{
    public static List<SelectListItem> GetComboStatusCotizacion(int? status, bool isFilter)
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
            lista.Insert(0, new SelectListItem("TODOS", "0", status is null or 0));

        return lista;
    }

    public static List<SelectListItem> GetComboStatusOrdenCompra(int? status, bool isFilter)
    {
        var lista = new List<SelectListItem>
        {
            new SelectListItem("EN PROCESO", "1", status == 1),
            new SelectListItem("COLOCADA", "2", status == 2),
            new SelectListItem("PAGADA", "3", status == 3),
            new SelectListItem("ENTREGADA", "4", status == 4),
            new SelectListItem("CERRADA", "5", status == 5)
        };

        if (isFilter)
            lista.Insert(0, new SelectListItem("TODOS", "0", status is null or 0));

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

    public static List<SelectListItem> GetComboTipoCliente(int? idTipo, bool isFilter)
    {
        var lista = new List<SelectListItem>
        {
            new SelectListItem("Cliente", "2", idTipo == 2),
            new SelectListItem("Proveedor", "1", idTipo == 1)

        };

        if (isFilter)
            lista.Insert(0, new SelectListItem("Todos", "0", idTipo is null or 0));

        return lista;
    }

}