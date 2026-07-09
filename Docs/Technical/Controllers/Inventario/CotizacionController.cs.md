# Documento técnico

## Archivo fuente

`Controllers/Inventario/CotizacionController.cs`

## Responsabilidad

Controller MVC (`[Authorize]`) para el CRUD de `Cotizacion` y de su detalle de productos
(`CotizacionDetalle`): listado/búsqueda, alta/edición del encabezado, y endpoints AJAX (JSON) que alimentan la
tabla dinámica de productos de la vista `Editar`.

## Dónde se usa

Consumido por `Views/Cotizacion/Editar.cshtml` (fetch a `GetDetalles` y a los endpoints de alta/edición/borrado
de detalle) y por `Views/Cotizacion/Index.cshtml` (listado/búsqueda).

## Dependencias

- `AppDbContext` (`_db`): acceso a `VsCotizacionDetalles` (vista de BD con precios ya calculados),
  `CotizacionDetalles` (tabla base, usada para borrado), `VsCotizacions`.
- `AppClassContext` (`_clasContext`) y `RazorViewToStringRenderer` (`_razorRenderer`): usados por otras acciones
  del controller (no relacionadas con `ObtenerDetallesRaw`).
- `ApplicationController` (clase base): expone `GetIsAdmin()` y `GetUserId()` a partir de los claims de la
  cookie de sesión.

## Flujo interno / comportamiento

### `GetDetalles(int idCotizacion)` — `[HttpGet]`

Devuelve como JSON el resultado de `ObtenerDetallesRaw(idCotizacion)`. Sin envoltura `{ ok, data }` (a
diferencia de otros endpoints del proyecto) — el front (`cargarTabla()`) espera directamente un array.

### `ObtenerDetallesRaw(int idCotizacion, int? idDetalle = null)` — privado

Consulta `_db.VsCotizacionDetalles` filtrando por `IdCotizacion` (y opcionalmente por `IdCotizacionDetalle`,
usado tras alta/edición de un solo renglón). Proyecta a un objeto anónimo distinto según el rol:

- **Admin** (`GetIsAdmin() == true`): incluye, además de los campos comunes, `precioListaMxn`, `monedaLista`,
  `precioLista` (precio de lista original y su moneda, antes de la conversión a MXN — agregados para que el
  front pueda resaltar y mostrar el valor original cuando la moneda de lista no es MXN), `porcentajeProveedor`,
  `precioProveedor`, `porcentajeProveedorGanancia`, `gananciaProveedor`, `precioSein`.
- **No-admin**: solo `idCotizacionDetalle`, `cantidad`, `idProducto`, `codigo`, `descripcion`, `precioCliente`,
  `claveUnidadSat`, `total`, `observaciones`.

Ambas ramas comparten los nombres de campo (camelCase) para los datos comunes, de modo que el JS del front
(`cargarTabla`, `actualizarFilaDesdeModelo`) puede usar los mismos accesos (`d.total`, `d.cantidad`, etc.) sin
importar el rol.

## Contratos y efectos secundarios

- El shape del JSON devuelto por `ObtenerDetallesRaw` está acoplado 1:1 a los selectores CSS/propiedades que lee
  `Editar.cshtml` (`d.precioListaMxn`, `d.monedaLista`, `d.precioLista`, etc.). Cambiar un nombre de campo aquí
  sin actualizar la vista rompe la tabla en tiempo de ejecución (sin error de compilación, porque el front es
  JS dinámico sobre un `object` anónimo).

## Consideraciones para modificarlo

- Si se agrega un campo nuevo que también deba verse en la rama no-admin, hay que añadirlo explícitamente en
  ambos `Select` — no hay una proyección compartida.
- `VsCotizacionDetalle` es una vista de BD (`Context/Database/VsCotizacionDetalle.cs`); cualquier campo nuevo
  debe existir ya en esa vista antes de poder proyectarse aquí.

## Registro de cambios del documento

- **2026-07-08** — Se agregaron `monedaLista` y `precioLista` a la proyección admin de `ObtenerDetallesRaw`,
  para soportar el resaltado en rojo + tooltip de precio original en `Editar.cshtml` cuando la moneda de lista
  no es MXN.
