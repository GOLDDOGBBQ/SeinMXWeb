# Contexto de Feature

## Propósito

Gestión de cotizaciones (`Cotizacion` + `CotizacionDetalle`): alta/edición de una cotización para un cliente,
con un detalle de productos (tabla dinámica cargada vía fetch) que calcula precios en distintos niveles
(proveedor, Sein, cliente) y totales.

## Alcance actual

- `CotizacionController` (`Controllers/Inventario/CotizacionController.cs`): listado (`Index`), edición
  (`Editar`/`Guardar`), y endpoints AJAX para el detalle de productos (`GetDetalles`, alta/edición/borrado de
  `CotizacionDetalle`).
- Vista `Views/Cotizacion/Editar.cshtml`: formulario de datos generales de la cotización + tabla `#tablaDetalle`
  con los productos agregados, poblada 100% por JS (`cargarTabla()`), sin partial views para las filas.
- La visibilidad de columnas y campos de precio de la tabla depende del rol (`UserClaimsHelper.IsAdmin(User)` /
  `GetIsAdmin()` en el controller): un admin ve columnas de costo/proveedor/ganancia además de precio cliente;
  un usuario no-admin solo ve precio cliente, cantidad, total y observaciones.

## Reglas de negocio y flujo actual

- `ObtenerDetallesRaw` (privado, usado por `GetDetalles` y por las respuestas de alta/edición de detalle)
  proyecta el detalle desde la vista de BD `VsCotizacionDetalle` (no la tabla base `CotizacionDetalle`), porque
  esa vista ya trae los cálculos de precio/proveedor/ganancia resueltos en SQL.
- Rama admin de `ObtenerDetallesRaw` expone: `precioListaMxn`, `monedaLista`, `precioLista`, `porcentajeProveedor`,
  `precioProveedor`, `porcentajeProveedorGanancia`, `gananciaProveedor`, `precioSein`, `precioCliente`,
  `claveUnidadSat`, `total`, `observaciones`. `monedaLista`/`precioLista` se agregaron para poder mostrar en el
  front el precio de lista **original** (antes de convertir a MXN) cuando la lista del producto está en otra
  moneda.
- Cuando `MonedaLista != "MXN"`, la celda "Precio Lista" (columna solo-admin) se resalta en rojo/negrita
  (`text-danger fw-bold`) y expone un tooltip (Bootstrap `Tooltip`, `trigger: "hover focus"` + `tabindex="0"`)
  con el precio original formateado en su propia moneda — para que el admin identifique de un vistazo qué
  productos tienen lista en divisa distinta a MXN antes de la conversión.
- Filas de la tabla soportan edición inline (`data-mode="view|edit"`, patrón `span.valor` + `input.d-none`) para
  `cantidad` y `observaciones`; el resto de columnas de precio son de solo lectura y se recalculan en servidor.

## UI/UX actual

- Tabla `#tablaDetalle` (Bootstrap `table table-bordered table-hover`), filas construidas por `cargarTabla()` con
  template literals (una rama de HTML para admin, otra para no-admin).
- Patrón "placeholder vacío + relleno post-inserción": celdas cuyo contenido depende de lógica JS adicional
  (acciones de fila, precio de lista) se insertan vacías (`<td class="acciones"></td>`, `<td class="precio-lista"></td>`)
  y se rellenan justo después de insertar el `<tr>` en el DOM (`renderAcciones(tr)`, `aplicarPrecioLista(td, d)`).
- Único uso de componentes JS de Bootstrap en el proyecto antes de este cambio: `bootstrap.Toast` en
  `_Layout.cshtml` (función `showToast`). El tooltip de precio de lista es el primer uso de `bootstrap.Tooltip`
  en el código propio del proyecto.

## Integraciones y dependencias

- EF Core sobre la vista de BD `VsCotizacionDetalle` (`Context/Database/VsCotizacionDetalle.cs`), mapeada en
  `AppDbContext`.
- `bootstrap.bundle.min.js` (incluye Popper), cargado globalmente en `Views/Shared/_Layout.cshtml`, requerido por
  `bootstrap.Tooltip`.
- `UserClaimsHelper.IsAdmin(User)` (vista, para ocultar/mostrar columnas) y `GetIsAdmin()`
  (`ApplicationController`, para el controller) — misma noción de rol, dos puntos de verificación (vista +
  servidor) que deben mantenerse en sincronía si cambian las columnas visibles.

## Riesgos / consideraciones

- `monedaLista`/`precioLista` solo se exponen en la rama admin de `ObtenerDetallesRaw`; si en el futuro se
  necesita ese dato en la rama no-admin (o en otro endpoint que use la misma vista), hay que agregarlo ahí
  también — no hay una función compartida de proyección entre ambas ramas.
- Cada llamada a `aplicarPrecioLista` destruye (`dispose()`) cualquier instancia previa de `bootstrap.Tooltip`
  sobre la celda antes de recrearla, para evitar tooltips huérfanos al refrescar la fila tras una edición
  (`actualizarFilaDesdeModelo`). Si se agregan más columnas con tooltip a futuro, replicar este mismo manejo de
  ciclo de vida (dispose antes de recrear).

## Registro de cambios del contexto

- **2026-07-08** — Se agregó a `ObtenerDetallesRaw` (rama admin) la proyección de `monedaLista` y `precioLista`
  desde `VsCotizacionDetalle`, y en `Editar.cshtml` se agregó `aplicarPrecioLista(td, d)`: resalta en rojo/negrita
  la celda "Precio Lista" cuando `monedaLista != "MXN"` y muestra el precio original vía tooltip de Bootstrap
  (hover o click). Motivo: el admin no tenía forma de saber si un precio de lista mostrado en MXN venía de una
  conversión desde otra moneda.
