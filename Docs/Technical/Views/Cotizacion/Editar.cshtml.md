# Documento técnico

## Archivo fuente

`Views/Cotizacion/Editar.cshtml`

## Responsabilidad

Vista de edición de una `Cotizacion`: formulario de datos generales (encabezado), formulario para agregar
productos al detalle, y la tabla dinámica `#tablaDetalle` que lista/edita los productos ya agregados. Toda la
lógica de la tabla (carga, edición inline, alta, borrado) vive como JS inline en esta misma vista.

## Dónde se usa

Servida por `CotizacionController.Editar` (GET). Usa los endpoints AJAX del mismo controller
(`GetDetalles`, alta/edición/borrado de `CotizacionDetalle`).

## Dependencias

- `bootstrap.bundle.min.js` (cargado globalmente en `_Layout.cshtml`) — requerido por `bootstrap.Tooltip` usado
  en `aplicarPrecioLista`.
- `UserClaimsHelper.IsAdmin(User)` — determina en el servidor (Razor) qué `<th>` se renderizan, y se refleja en
  JS como la constante `esAdmin` (línea `const esAdmin = @UserClaimsHelper.IsAdmin(User).ToString().ToLower();`)
  para elegir qué rama de `<tr>` construir en `cargarTabla()`.
- Endpoint `GetDetalles` (`CotizacionController`) — fuente de los datos que consume `cargarTabla()`.

## Flujo interno / comportamiento

### `cargarTabla()`

Hace `fetch` a `GetDetalles`, limpia el `<tbody>` y por cada registro inserta un `<tr>` (rama admin o no-admin
según `esAdmin`). Patrón usado para columnas con lógica adicional: se insertan como `<td>` vacío y se rellenan
justo después de insertar el `<tr>` en el DOM:

- `<td class="acciones"></td>` → `renderAcciones(tr)`.
- `<td class="precio-lista"></td>` (solo rama admin) → `aplicarPrecioLista(tr.querySelector(".precio-lista"), d)`.

### `actualizarFilaDesdeModelo(tr, d)`

Refresca una fila existente tras guardar una edición (cantidad/observaciones), sin recargar toda la tabla.
Para las columnas de precio usa los helpers `setMoney`/`setText`, excepto `.precio-lista`, que usa
`aplicarPrecioLista` (misma función que en `cargarTabla`) porque necesita lógica adicional (clase condicional +
tooltip), no solo formateo de moneda.

### `aplicarPrecioLista(td, d)`

Helper (junto a `setMoney`/`setText`) que centraliza el render de la celda "Precio Lista":

1. Si `td` ya tiene una instancia de `bootstrap.Tooltip` asociada (`bootstrap.Tooltip.getInstance(td)`), la
   destruye (`.dispose()`) antes de continuar — evita tooltips huérfanos al re-renderizar la celda.
2. Escribe `td.textContent` con `d.precioListaMxn` formateado en MXN.
3. Activa/desactiva `text-danger` y `fw-bold` según `d.monedaLista !== "MXN"`.
4. Si la moneda de lista no es MXN: formatea `d.precioLista` en su propia moneda (`d.monedaLista`), lo pone en
   `title` (junto con el código de moneda, para desambiguar el símbolo `$`), agrega `tabindex="0"` (para que la
   celda sea enfocable por click/teclado), y crea un `new bootstrap.Tooltip(td, { trigger: "hover focus" })`.
   Si es MXN: remueve `title`/`data-bs-toggle`/`tabindex` (sin tooltip).

## Contratos y efectos secundarios

- `aplicarPrecioLista` asume que `d.precioListaMxn` siempre viene definido (rama admin de `ObtenerDetallesRaw`),
  y que `d.monedaLista`/`d.precioLista` vienen definidos cuando la celda `.precio-lista` existe en el DOM (solo
  existe en la rama admin). No se llama en la rama no-admin (no hay columna "Precio Lista" ahí).
- Se usa `trigger: "hover focus"` (el default documentado por Bootstrap) en vez de `"hover click"`: combinar
  `hover` con `click` deja el tooltip "atascado" abierto tras un click, porque Bootstrap no limpia el estado del
  trigger `click` al hacer `mouseleave`. Con `tabindex="0"` en la celda, un click la enfoca (dispara `focus` →
  muestra el tooltip) y perder el foco al hacer click en cualquier otro lado lo cierra correctamente — cubre
  "hover o click" sin el bug de cierre.

## Consideraciones para modificarlo

- Si se agrega otra celda con tooltip a futuro, replicar el mismo ciclo de vida dispose-antes-de-crear que usa
  `aplicarPrecioLista`, para no acumular instancias de `bootstrap.Tooltip` huérfanas en el DOM.
- Cualquier cambio de nombre de campo en `ObtenerDetallesRaw` (`CotizacionController.cs`) debe reflejarse aquí
  (no hay tipado compartido entre backend y este JS).

## Registro de cambios del documento

- **2026-07-08** — Se agregó `aplicarPrecioLista(td, d)` y se cambiaron los dos puntos donde se pintaba
  `.precio-lista` (`cargarTabla`, `actualizarFilaDesdeModelo`) para usarla, en vez de renderizar el monto
  directo en el template literal / `setMoney`. Motivo: resaltar en rojo y mostrar vía tooltip el precio de lista
  original cuando `monedaLista != "MXN"`.
- **2026-07-08** — Cambiado `trigger: "hover click"` por `trigger: "hover focus"` + `tabindex="0"` en
  `aplicarPrecioLista`. Motivo: con `"hover click"` el tooltip se quedaba abierto permanentemente tras un click
  (reportado por el usuario) porque Bootstrap no resetea el estado activo del trigger `click` al salir el mouse.
