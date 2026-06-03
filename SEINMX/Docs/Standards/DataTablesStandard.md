# Estándar de DataTables — CBAppWeb

> Guía completa y autosuficiente para implementar DataTables en cualquier proyecto
> con la misma arquitectura, funcionalidades y estructura que CBAppWeb.
>
> **Motor:** DataTables 3.0.0-beta.2 + ColumnControl + ColReorder + Select  
> **Wrapper:** `CB_DATATABLES` (fachada propia del proyecto)  
> **Regla de motor:** DataTables → todas las tablas planas. Tabulator → solo árboles (treeview).

---

## Tabla de Contenidos

1. [Stack y versiones](#1-stack-y-versiones)
2. [Archivos del proyecto](#2-archivos-del-proyecto)
3. [Estructura HTML estándar de una vista con tabla](#3-estructura-html-estándar-de-una-vista-con-tabla)
4. [Partial `_TablaControles`](#4-partial-_tablacontroles)
5. [CSS personalizado — `datatables-cb.css`](#5-css-personalizado--datatables-cbcss)
6. [Fachada `CB_DATATABLES` — API pública](#6-fachada-cb_datatables--api-pública)
7. [Inicialización: `crearTabla`](#7-inicialización-crearTabla)
8. [Definición de columnas](#8-definición-de-columnas)
9. [ColumnControl — menú ☰ por columna](#9-columncontrol--menú--por-columna)
10. [Operaciones CRUD en la tabla](#10-operaciones-crud-en-la-tabla)
11. [Persistencia en localStorage](#11-persistencia-en-localstorage)
12. [Fachada `CB_TABLA` — motor agnóstico](#12-fachada-cb_tabla--motor-agnóstico)
13. [Patrón completo — módulo JS de vista](#13-patrón-completo--módulo-js-de-vista)
14. [Assets y scripts en la vista Razor](#14-assets-y-scripts-en-la-vista-razor)
15. [Reglas y restricciones críticas](#15-reglas-y-restricciones-críticas)

---

## 1. Stack y versiones

| Componente | Versión | Rol |
|---|---|---|
| DataTables | 3.0.0-beta.2 | Motor de tabla principal |
| ColumnControl | 2.0.0-beta.1 | Menú ☰ por columna (filtros avanzados) |
| ColReorder | 3.0.0-beta.1 | Drag & drop de columnas |
| Select | 4.0.0-beta.1 | Selección de filas |
| Buttons | 4.0.0-beta.1 | Exportar CSV / PDF / Excel |
| Responsive | 4.0.0-beta.1 | Adaptación mobile |
| FixedHeader | 5.0.0-beta.1 | Header fijo al hacer scroll |
| FixedColumns | 6.0.0-beta.1 | Columnas fijas al scroll horizontal |
| Bootstrap | 5.x | UI framework |
| jQuery | **NO requerido** | DT 3.x usa su propia librería DOM |

> **Importante:** El bundle local (`datatables.min.js`) incluye todos los
> componentes anteriores en un solo archivo. No se usan CDN en producción.

---

## 2. Archivos del proyecto

```
wwwroot/
├── lib/datatables/
│   ├── css/datatables.min.css    ← Bundle CSS (DT + Bootstrap 5 + extensiones)
│   └── js/datatables.min.js     ← Bundle JS (DT 3.x + todas las extensiones)
├── css/
│   └── datatables-cb.css        ← Overrides de selección, animaciones, ColumnControl
└── js/core/
    ├── datatables-cb.js         ← Fachada CB_DATATABLES (window.CB_DATATABLES)
    ├── cb-tabla.js              ← Fachada CB_TABLA — motor agnóstico
    ├── gestor-crud.js           ← GestorCrud (resaltarFila, guardarYActualizar)
    ├── modal-confirmacion.js    ← ModalConfirmacion
    ├── cb-busy.js               ← CB_BUSY — feedback visual
    └── notify.js                ← Notify — toasts

Views/Shared/
└── _TablaControles.cshtml       ← Partial de botones de control de tabla
```

---

## 3. Estructura HTML estándar de una vista con tabla

El patrón de una vista Index con DataTables sigue esta estructura:

```cshtml
@* Sección Styles: assets DataTables SIEMPRE en @section Styles *@
@section Styles {
    <link rel="stylesheet" href="~/lib/datatables/css/datatables.min.css" asp-append-version="true"/>
    <link rel="stylesheet" href="~/css/datatables-cb.css" asp-append-version="true"/>
}

@* FiltroPanel — búsqueda GET con formulario *@
<div id="panel-filtros" class="card card-outline card-secondary mb-3">
    <div class="card-header">
        <h3 class="card-title"><i class="fa-solid fa-filter me-1"></i>Filtros</h3>
        <div class="card-tools">
            <button type="button" class="btn btn-tool" data-lte-toggle="card-collapse">
                <i class="fa-solid fa-minus"></i>
            </button>
        </div>
    </div>
    <div class="card-body">
        <form id="form-filtros"
              asp-area="MiArea" asp-controller="MiEntidad" asp-action="Index"
              method="get">
            <div class="row g-3 align-items-end">
                <div class="col-md-4">
                    <label class="form-label form-label-sm">Descripcion contiene</label>
                    <input type="text" name="TextoBuscar"
                           class="form-control form-control-sm"
                           value="@Model.Filtro.TextoBuscar" />
                </div>
                <div class="col-md-auto">
                    <button type="submit" class="btn btn-primary btn-sm">
                        <i class="fa-solid fa-magnifying-glass me-1"></i>Buscar
                    </button>
                    <button type="button" id="btn-limpiar-filtros" class="btn btn-secondary btn-sm ms-1">
                        <i class="fa-solid fa-broom me-1"></i>Limpiar
                    </button>
                </div>
            </div>
        </form>
    </div>
</div>

@* ActionBar — acciones principales *@
<div class="card mb-2">
    <div class="card-body py-2 px-3 d-flex justify-content-between align-items-center flex-wrap gap-2">
        <div class="d-flex gap-2 flex-wrap align-items-center" id="action-bar-izquierda">
            <button type="button" id="btn-nuevo" class="btn btn-success btn-sm">
                <i class="fa-solid fa-plus me-1"></i>Nuevo
            </button>
        </div>
        @* Zona BULK (oculta hasta que el usuario seleccione filas) *@
        <div class="d-none gap-2 align-items-center" id="zona-bulk">
            <span class="text-muted small" id="lbl-seleccionados"></span>
            <button type="button" class="btn btn-secondary btn-sm" id="btn-deseleccionar">
                <i class="fa-solid fa-xmark me-1"></i>Deseleccionar
            </button>
            <button type="button" class="btn btn-danger btn-sm" id="btn-eliminar-bulk">
                <i class="fa-solid fa-trash me-1"></i>Eliminar seleccionados
            </button>
        </div>
    </div>
</div>

@* Tabla — el <table> tiene data-cb-persistencia obligatorio *@
<div class="card">
    <div class="card-header">
        <h3 class="card-title">
            <i class="fa-solid fa-list me-1"></i>Mi Entidad
            <span class="badge bg-secondary ms-2">@Model.TotalRegistros</span>
        </h3>
        <div class="card-tools d-flex gap-1">
            @* Botones de control: exportar, limpiar filtros, recordar, restablecer *@
            <partial name="_TablaControles" view-data='@(new ViewDataDictionary(ViewData) {
                { "TablaSelector",  "#tblMiEntidad" },
                { "NombreCsv",      "mi-entidad.csv" },
                { "Motor",          "datatables" }
            })' />
        </div>
    </div>
    <div class="card-body p-0">
        @* data-cb-persistencia: ID ÚNICO por tabla — nunca duplicar en la solución *@
        <table id="tblMiEntidad"
               class="table table-sm table-hover align-middle w-100"
               data-cb-persistencia="MiArea_tblMiEntidad_YYYYMMDD">
            <thead>
                <tr>
                    <th class="text-center">Acciones</th>
                    <th>Nombre</th>
                    <th>Descripcion</th>
                    <th class="text-center">Activo</th>
                </tr>
            </thead>
        </table>
    </div>
</div>

@* Modal de confirmación (obligatorio para eliminar) *@
<partial name="_ModalConfirmacion" />

@* Offcanvas — formulario de alta/edición *@
<div class="offcanvas offcanvas-end" tabindex="-1"
     id="offcanvas-formulario" data-bs-backdrop="false" style="width: 480px;">
    <div class="offcanvas-header border-bottom py-2">
        <h5 class="offcanvas-title h6" id="offcanvas-titulo">Nueva entidad</h5>
        <button type="button" class="btn-close btn-sm" data-bs-dismiss="offcanvas"></button>
    </div>
    <div class="offcanvas-body">
        @Html.AntiForgeryToken()
        <input type="hidden" id="form-modo" value="nuevo" />
        <input type="hidden" id="campo-id-oculto" name="IdMiEntidad" value="0" data-grupo="grabar" />
        <form id="form-registro" novalidate>
            <div class="alert alert-danger d-none" id="form-errores" role="alert"></div>
            @* campos del formulario *@
        </form>
    </div>
    <div class="offcanvas-footer border-top p-3 d-flex gap-2">
        <button type="button" class="btn btn-primary btn-sm" id="btn-guardar">
            <i class="fa-solid fa-save me-1"></i> Guardar
        </button>
        <button type="button" class="btn btn-outline-secondary btn-sm" id="btn-limpiar">
            <i class="fa-solid fa-eraser me-1"></i> Limpiar
        </button>
    </div>
</div>

@* Scripts: SIEMPRE en @section Scripts, después del layout *@
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
    <script src="~/lib/datatables/js/datatables.min.js" asp-append-version="true"></script>
    <script src="~/js/core/datatables-cb.js" asp-append-version="true"></script>
    @* Nota: cb-tabla.js, gestor-crud.js, notify.js, modal-confirmacion.js, cb-busy.js
             deben estar incluidos en el layout o antes de mi módulo JS *@

    @* Inyección de configuración desde el servidor (evita inline JS con lógica) *@
    <script>
        window.MI_CONFIG = {
            data: @Html.Raw(System.Text.Json.JsonSerializer.Serialize(
                                Model.Registros,
                                new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = null })),
            urls: {
                guardar:  '@Url.Action("Guardar",  "MiEntidad", new { area = "MiArea" })',
                eliminar: '@Url.Action("Eliminar", "MiEntidad", new { area = "MiArea" })'
            }
        };
    </script>
    <script src="~/js/areas/mia-area/mi-entidad.js" asp-append-version="true"></script>
}
```

---

## 4. Partial `_TablaControles`

El partial `Views/Shared/_TablaControles.cshtml` genera el contenedor `div[data-cb-controles]`
con los botones declarativos de la tabla. Es compatible con DataTables y Tabulator.

### Parámetros via ViewData

| Parámetro | Tipo | Requerido | Descripción |
|---|---|---|---|
| `TablaSelector` | string | ✅ | Selector CSS del `<table>` (ej. `"#tblMiEntidad"`) |
| `NombreCsv` | string | ✅ | Nombre del archivo CSV al exportar (ej. `"entidades.csv"`) |
| `Motor` | string | ✅ | `"datatables"` para DataTables |
| `MostrarRecordar` | bool | ❌ | Muestra toggle "Recordar filtros" (default: `true`) |
| `MostrarImprimir` | bool | ❌ | Muestra botón Imprimir (default: `false` en el proyecto) |
| `MostrarArbol` | bool | ❌ | Solo Tabulator — no aplica en DataTables |
| `MostrarFiltros` | bool | ❌ | Solo Tabulator — siempre `false` en DataTables (ColumnControl ya expone ☰) |

### Invocación estándar para DataTables

```cshtml
<partial name="_TablaControles" view-data='@(new ViewDataDictionary(ViewData) {
    { "TablaSelector",  "#tblMiEntidad" },
    { "NombreCsv",      "mi-entidad.csv" },
    { "Motor",          "datatables" }
})' />
```

### HTML que genera (referencia)

```html
<div data-cb-controles
     data-cb-motor="datatables"
     data-cb-tabla="#tblMiEntidad"
     data-cb-csv="mi-entidad.csv"
     class="d-flex gap-1 align-items-center">

    <!-- G1: toggle recordar filtros -->
    <button type="button" data-cb-accion="recordar-filtros"
            class="btn btn-outline-secondary btn-sm" title="Recordar filtros al recargar la página"
            aria-pressed="false">
        <i class="fa-solid fa-thumbtack"></i>
    </button>

    <span class="vr align-self-stretch mx-1 opacity-25"></span>

    <!-- G2: exportar -->
    <button type="button" data-cb-accion="exportar"
            class="btn btn-secondary btn-sm" title="Exportar CSV">
        <i class="fa-solid fa-file-csv"></i>
    </button>

    <span class="vr align-self-stretch mx-1 opacity-25"></span>

    <!-- G3: limpiar y restablecer -->
    <button type="button" data-cb-accion="limpiar"
            class="btn btn-secondary btn-sm" title="Limpiar filtros de tabla">
        <i class="fa-solid fa-filter-circle-xmark"></i>
    </button>
    <button type="button" data-cb-accion="restablecer"
            class="btn btn-warning btn-sm" title="Restablecer tabla">
        <i class="fa-solid fa-rotate-left"></i>
    </button>
</div>
```

### Comportamiento de cada acción declarativa

| `data-cb-accion` | Comportamiento |
|---|---|
| `exportar` | Descarga CSV con datos visibles (excluye columnas Acciones/checkbox/auditoría) |
| `limpiar` | Limpia búsqueda global + búsqueda por columna DT + filtros ColumnControl. El botón se vuelve azul (`cb-filtro-activo`) cuando hay filtros activos |
| `restablecer` | Limpia toda la persistencia (localStorage) y recarga la página |
| `recordar-filtros` | Toggle: activa/desactiva persistencia de estado de filtros en localStorage |

> **Nota:** `filtros` (ocultar/mostrar columnas de filtro) no aplica en DataTables.
> ColumnControl ya expone su menú ☰ siempre visible por columna.

---

## 5. CSS personalizado — `datatables-cb.css`

Este archivo corrige comportamientos del bundle oficial y define el estilo CargoBaja.
Debe cargarse **siempre después** de `datatables.min.css` en `@section Styles`.

### Contenido completo

```css
/* datatables-cb.css — v1.5.0 */

/* ── Animación de fila guardada (highlight verde 2.5s) ── */
@keyframes cb-row-guardado {
    0%   { background-color: rgba(25, 135, 84, .28); }
    60%  { background-color: rgba(25, 135, 84, .18); }
    100% { background-color: transparent; }
}
@keyframes cb-row-guardado-dark {
    0%   { background-color: rgba(25, 135, 84, .45); }
    60%  { background-color: rgba(25, 135, 84, .25); }
    100% { background-color: transparent; }
}
.dt-container table tbody tr.cb-row-guardado,
.dt-container table tbody tr.cb-row-guardado td {
    animation: cb-row-guardado 2.5s ease-out forwards;
}
[data-bs-theme="dark"] .dt-container table tbody tr.cb-row-guardado,
[data-bs-theme="dark"] .dt-container table tbody tr.cb-row-guardado td {
    animation: cb-row-guardado-dark 2.5s ease-out forwards;
}
.dtcc { pointer-events: auto; }

/* ── ColumnControl: ícono ☰ siempre al extremo derecho ── */
table.dataTable thead > tr > th  div.dt-column-header,
table.dataTable thead > tr > td  div.dt-column-header,
table.dataTable tfoot > tr > th  div.dt-column-footer,
table.dataTable tfoot > tr > td  div.dt-column-footer {
    flex-direction: row !important;
}

/* ── Botón "Limpiar" activo cuando hay filtros ── */
[data-cb-accion="limpiar"].cb-filtro-activo {
    color: #fff !important;
    background-color: var(--bs-primary) !important;
    border-color: var(--bs-primary) !important;
}

/* ── Ajustes generales de UI ── */
.dt-container { margin-top: 0; }
table.dataTable thead th.dt-orderable-asc,
table.dataTable thead th.dt-orderable-desc { cursor: pointer; }
.dt-paging .pagination { margin-bottom: 0; }

/* ── Paginación activa — color primario del proyecto ── */
.dt-paging .pagination {
    --bs-pagination-active-bg:           var(--bs-primary);
    --bs-pagination-active-border-color: var(--bs-primary);
    --bs-pagination-active-color:        #fff;
    --bs-pagination-hover-color:         var(--bs-primary);
    --bs-pagination-hover-border-color:  var(--bs-primary-border-subtle);
}
.dt-info { font-size: 0.875rem; color: var(--bs-secondary-color); padding-top: 0.5rem; }
table.dataTable tbody td.dt-select {
    padding-left: 0.5rem; padding-right: 0.5rem; vertical-align: middle;
}

/* ── Selección de fila — color primario del proyecto ── */
/* DT 3.x usa variables RGB sin rgb() en --dt_background-selected */
:root {
    --dt_background-selected  : 168, 213, 186;  /* verde-200 */
    --dt_color-selected       : 33, 37, 41;
    --dt_link_color-selected  : 0, 121, 53;
}
:root[data-bs-theme=dark] {
    --dt_background-selected  : 0, 61, 27;       /* verde-900 */
    --dt_color-selected       : 255, 255, 255;
    --dt_link_color-selected  : 77, 203, 126;
}
/* Refuerzo directo en celdas (variables compartidas con Tabulator) */
table.dataTable tbody tr.selected > td {
    background-color : var(--cb-seleccion-bg)   !important;
    color            : var(--cb-seleccion-text) !important;
}
@media (hover: hover) and (pointer: fine) {
    table.dataTable tbody tr.selected:hover > td {
        background-color : var(--cb-seleccion-bg-hover) !important;
    }
}
[data-bs-theme="dark"] table.dataTable tbody tr.selected > td {
    background-color : var(--cb-seleccion-bg)   !important;
    color            : var(--cb-seleccion-text) !important;
}
@media (hover: hover) and (pointer: fine) {
    [data-bs-theme="dark"] table.dataTable tbody tr.selected:hover > td {
        background-color : var(--cb-seleccion-bg-hover) !important;
        color            : var(--cb-seleccion-text)    !important;
    }
}

/* ── Botones <a class="btn"> en filas seleccionadas — preservar color ── */
table.dataTable tbody tr.selected > td a.btn {
    color : var(--bs-btn-color) !important;
}

/* ── ColumnControl dropdown — tema Bootstrap ── */
.dtcc-dropdown {
    border: 1px solid var(--bs-border-color);
    border-radius: 0.375rem;
    box-shadow: 0 4px 16px rgba(0,0,0,.12);
    background: var(--bs-body-bg);
    min-width: 220px;
    z-index: 1050;
}
.dtcc-dropdown .dtcc-button:hover { background-color: var(--bs-tertiary-bg); }
.dtcc-list-search {
    width: 100%; padding: 4px 8px;
    border: 1px solid var(--bs-border-color);
    border-radius: 0.25rem; font-size: 0.8125rem;
    background: var(--bs-body-bg); color: var(--bs-body-color);
}
.dtcc-button_dropdown[aria-expanded="true"] .dtcc-button-icon svg {
    stroke: var(--bs-primary);
}
```

### Variables CSS del sitio necesarias

Estas variables deben estar definidas globalmente (ej. en `site.css`):

```css
:root {
    --cb-seleccion-bg:       #e0f5e9;  /* verde-100 light */
    --cb-seleccion-bg-hover: #a8d5ba;  /* verde-200 light */
    --cb-seleccion-text:     #212529;
}
[data-bs-theme="dark"] {
    --cb-seleccion-bg:       #003d1b;  /* verde-900 dark */
    --cb-seleccion-bg-hover: #004d24;
    --cb-seleccion-text:     #ffffff;
}
```

---

## 6. Fachada `CB_DATATABLES` — API pública

La fachada `window.CB_DATATABLES` encapsula toda la configuración base y provee
las operaciones CRUD sobre la instancia DataTables. **Nunca llamar `new DataTable()`
directamente** — usar siempre `CB_DATATABLES.crearTabla`.

```
API pública (window.CB_DATATABLES):
  crearTabla(selector, opciones)          → DataTables.Api
  placeholderHtml(icono, mensaje)         → string (HTML del estado vacío)
  resaltarFila(dt, pkValor)              → void   (scroll + animación verde 2.5s)
  addRow(dt, registro, alInicio)         → void   (insertar fila)
  updateRow(dt, registro)               → void   (actualizar fila por PK)
  deleteRow(dt, pkValor)                → void   (eliminar fila por PK)
```

### Configuración base aplicada automáticamente

```javascript
// BASE_CFG aplicado en toda tabla creada con crearTabla:
{
    paging         : true,
    pageLength     : 25,
    lengthMenu     : [[10, 25, 50, 100, -1], [10, 25, 50, 100, 'Todos']],
    pagingType     : 'full_numbers',
    ordering       : true,
    searching      : true,
    info           : true,
    autoWidth      : false,
    responsive     : true,
    stateSave      : true,          // con callbacks condicionales al toggle "Recordar filtros"
    language       : LANG_ES,       // localización completa en español
    select         : { style: 'single', selector: 'tr' },   // sin checkbox: clic en fila
    layout: {
        topStart   : 'pageLength',
        topEnd     : 'search',
        bottomStart: 'info',
        bottomEnd  : 'paging'
    },
    // ColumnControl activado por defecto:
    columnControl  : [
        ['search', 'orderAsc', 'orderDesc', 'orderClear']  // array anidado = botón ☰
    ],
    // ColReorder activado por defecto:
    colReorder     : true   // o { columns: ':gt(0)' } con checkbox (bloquea col 0)
}
```

---

## 7. Inicialización: `crearTabla`

### Firma recomendada (v1.4.0+)

```javascript
// El persistenceId se lee automáticamente del atributo data-cb-persistencia del <table>
var tabla = CB_DATATABLES.crearTabla('#tblMiEntidad', opciones);
```

### Opciones disponibles

| Opción | Tipo | Default | Descripción |
|---|---|---|---|
| `rowId` | string | — | Campo PK del objeto de datos (ej. `'IdEntidad'`). Requerido para CRUD por PK |
| `data` | Array | — | Datos iniciales JS (del servidor). Alternativa a `ajax` |
| `ajax` | string/Object | — | URL para carga AJAX. Alternativa a `data` |
| `columns` | Array | — | Definición de columnas (ver §8) |
| `columnDefs` | Array | — | Reglas de columna por selector de índice |
| `order` | Array | — | Orden inicial: `[[colIdx, 'asc'/'desc']]` |
| `pageLength` | number | 25 | Filas por página inicial |
| `checkboxCol` | bool | `false` | Agrega columna checkbox al inicio para selección |
| `multiSelect` | bool | `false` | Permite selección múltiple de filas |
| `columnControl` | bool/Array | `true` | `false` desactiva ColumnControl en toda la tabla |
| `colReorder` | bool/Object | `true` | `false` desactiva drag & drop de columnas |
| `onSelect` | Function | — | Callback `fn(count, rows)` cuando cambia la selección |
| `drawCallback` | Function | — | Callback ejecutado en cada redibujado (para re-bind de botones) |
| `initComplete` | Function | — | Callback ejecutado al terminar la inicialización |
| `scrollX` | bool | — | Habilita scroll horizontal |
| `scrollY` | string | — | Altura fija con scroll vertical (ej. `'400px'`) |
| `placeholderIcono` | string | `'fa-solid fa-inbox'` | Ícono FA del estado vacío |
| `placeholderMensaje` | string | `'Sin registros.'` | Texto del estado vacío |

### Ejemplo completo de inicialización

```javascript
var tabla = CB_DATATABLES.crearTabla('#tblMiEntidad', {
    rowId      : 'IdMiEntidad',
    checkboxCol: false,             // sin checkbox (default — selección por clic en fila)
    multiSelect: false,
    data       : window.MI_CONFIG.data,
    order      : [[1, 'asc']],

    columns: [
        {
            data      : null,
            title     : 'Acciones',
            orderable : false,
            searchable: false,
            columnControl: [],       // sin ColumnControl en esta columna
            className : 'text-center',
            width     : '110px',
            render    : _renderAcciones
        },
        { data: 'Nombre',      title: 'Nombre'      },
        { data: 'Descripcion', title: 'Descripcion' },
        {
            data     : 'Activo',
            title    : 'Activo',
            className: 'text-center',
            render   : function (v, type) {
                if (type !== 'display') return v;
                return v
                    ? "<span class='badge bg-success'>Sí</span>"
                    : "<span class='badge bg-secondary'>No</span>";
            }
        }
    ],

    onSelect: function (count) {
        var zonaBulk = document.getElementById('zona-bulk');
        if (!zonaBulk) return;
        if (count > 0) {
            zonaBulk.classList.remove('d-none');
            zonaBulk.classList.add('d-flex');
            document.getElementById('lbl-seleccionados').textContent =
                count + (count === 1 ? ' seleccionado' : ' seleccionados');
        } else {
            zonaBulk.classList.add('d-none');
            zonaBulk.classList.remove('d-flex');
        }
    },

    drawCallback: function () {
        _bindBotonesTabla();     // re-vincular botones en cada redibujado
    }
});
```

---

## 8. Definición de columnas

### Estructura de columna completa

```javascript
{
    data      : 'NombreCampo',   // campo del objeto de datos. null = columna sin datos
    title     : 'Título',        // texto del <th>. Obligatorio para CSV limpio
    type      : 'string',        // 'string' | 'num' | 'date' | 'html' | 'html-num'
    orderable : true,            // permite ordenar (default: true)
    searchable: true,            // participa en búsqueda global (default: true)
    visible   : true,            // visibilidad (default: true)
    className : '',              // clases CSS de las celdas
    width     : '',              // ancho CSS
    defaultContent: '',          // valor si data es null/undefined
    columnControl : [],          // [] desactiva ColumnControl en esta columna
    render    : function (data, type, row, meta) {
        // type: 'display' | 'filter' | 'sort' | 'type'
        if (type !== 'display') return data;  // retornar valor limpio para sort/filter
        return '<strong>' + data + '</strong>';
    }
}
```

### Columna de acciones (patrón estándar)

```javascript
{
    data      : null,
    title     : 'Acciones',
    orderable : false,
    searchable: false,
    columnControl: [],   // sin menú ☰ en la columna de acciones
    className : 'text-center',
    width     : '110px', // ajustar según cantidad de botones
    render    : function (data, type, row) {
        if (type !== 'display') return '';
        var id     = row.IdMiEntidad;
        var nombre = (row.Nombre || '').replace(/"/g, '&quot;');
        return (
            "<button type='button'" +
            " class='btn btn-success btn-sm js-editar'" +
            " data-id='" + id + "'" +
            " title='Editar'>" +
            "<i class='fa-solid fa-pencil'></i></button>" +

            "<button type='button'" +
            " class='btn btn-danger btn-sm ms-1 js-eliminar'" +
            " data-id='" + id + "'" +
            " data-nombre='" + nombre + "'" +
            " title='Eliminar'>" +
            "<i class='fa-solid fa-trash'></i></button>"
        );
    }
}
```

### Columna numérica (con formato de moneda)

```javascript
{
    data     : 'Precio',
    title    : 'Precio',
    type     : 'num',
    className: 'text-end dt-type-numeric',
    render   : function (data) {
        return '$\u00A0' + Number(data).toLocaleString('es-MX', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }
}
```

### Columna de fecha

```javascript
{
    data     : 'FechaCreacion',
    title    : 'Fecha',
    type     : 'date',
    className: 'dt-type-date',
    render   : function (data, type) {
        if (type !== 'display' || !data) return data || '';
        return new Date(data).toLocaleDateString('es-MX', {
            day: '2-digit', month: '2-digit', year: 'numeric'
        });
    }
}
```

### Columna badge/estado

```javascript
{
    data     : 'Estado',
    title    : 'Estado',
    className: 'text-center',
    render   : function (data, type) {
        if (type !== 'display') return data;
        var cls = data === 'Activo' ? 'success' : 'secondary';
        return "<span class='badge text-bg-" + cls + "'>" + data + "</span>";
    }
}
```

### Deshabilitar ColumnControl en columnas específicas

```javascript
// Via columnDefs (por índice):
columnDefs: [
    {
        targets: [0],                 // columna 0 (Acciones)
        columnControl: [],            // sin menú ☰
        orderable: false,
        searchable: false
    }
]

// Via columns (por definición de columna):
{ data: null, title: 'Acciones', columnControl: [], orderable: false, ... }
```

---

## 9. ColumnControl — menú ☰ por columna

ColumnControl se activa automáticamente en `crearTabla`. Agrega un botón ☰ a cada
columna que despliega un menú con filtros avanzados.

### Configuración activa por defecto

```javascript
// En CB_DATATABLES, el columnControl global es:
columnControl: [
    ['search', 'orderAsc', 'orderDesc', 'orderClear']
    // array anidado = botón ☰ con:
    //   'search'     → filtro inteligente por tipo (texto/número/fecha)
    //   'orderAsc'   → botón "Orden ascendente"
    //   'orderDesc'  → botón "Orden descendente"
    //   'orderClear' → botón "Quitar orden"
]
```

### Tipos de filtro por columna (auto-detectados)

| Tipo de datos | Filtros disponibles en el ☰ |
|---|---|
| Texto (`string`) | Contiene, No contiene, Igual a, Diferente a, Empieza con, Termina en, Vacío, No vacío |
| Numérico (`num`) | =, ≠, >, ≥, <, ≤, Vacío, No vacío |
| Fecha (`date`) | Igual a, Diferente a, Después de, Antes de, Vacío, No vacío |

### ⚠️ Reglas críticas de ColumnControl

```javascript
// ✅ CORRECTO — array anidado = botón ☰ (dropdown)
columnControl: ['order', ['searchList', 'orderAsc', 'orderDesc', 'orderClear']]

// ✅ CORRECTO — solo filtro avanzado en ☰ (sin botón 'order' extra)
columnControl: [['search', 'orderAsc', 'orderDesc', 'orderClear']]

// ✅ CORRECTO — forma expandida (requiere 'target')
columnControl: { target: 0, content: ['order', ['searchList']] }

// ❌ INCORRECTO — 'content' / 'header' / 'dropdown' sin 'target' → TypeError
columnControl: { header: [...], content: [...] }
columnControl: { header: [...], dropdown: [...] }
```

> **Nota sobre doble ícono de orden:** Si incluyes `'order'` a nivel raíz del
> `columnControl` Y `ordering: true` en la configuración base, aparecerán dos
> íconos de orden. La fachada `CB_DATATABLES` ya maneja esto: no incluye `'order'`
> al nivel raíz; las flechas nativas de DataTables cubren ese rol.

---

## 10. Operaciones CRUD en la tabla

### Vinculación de botones en filas (evento delegado)

```javascript
// Patrón estándar: delegación de eventos en el contenedor de la tabla
// (evita re-vincular en cada drawCallback)
document.getElementById('tblMiEntidad').addEventListener('click', function (e) {
    var btnEditar = e.target.closest('.js-editar');
    if (btnEditar) {
        _cargarEnOffcanvas(parseInt(btnEditar.dataset.id, 10));
        return;
    }

    var btnEliminar = e.target.closest('.js-eliminar');
    if (btnEliminar && !btnEliminar.disabled) {
        var id     = parseInt(btnEliminar.dataset.id, 10);
        var nombre = btnEliminar.dataset.nombre || 'este registro';

        ModalConfirmacion.mostrar({
            titulo    : 'Eliminar registro',
            mensaje   : '¿Deseas eliminar "' + nombre + '"? Esta acción no se puede deshacer.',
            etiquetaOk: 'Eliminar',
            onConfirm : async function () {
                var token = document.querySelector('[name="__RequestVerificationToken"]')?.value || '';
                await CB_BUSY.run(btnEliminar, async function () {
                    var response = await fetch(window.MI_CONFIG.urls.eliminar + '/' + id, {
                        method : 'POST',
                        headers: {
                            'X-Requested-With'        : 'XMLHttpRequest',
                            'RequestVerificationToken': token
                        }
                    });
                    var resultado = {};
                    try { resultado = await response.json(); } catch (_) {}

                    if (!response.ok || resultado.IsSuccess === false) {
                        ErrorResponse.manejar(response, resultado);
                        return;
                    }
                    CB_TABLA.deleteRow(tabla, id);   // eliminar de la tabla sin recargar
                    Notify.success(resultado.Message || 'Registro eliminado correctamente.');
                });
            }
        });
    }
});
```

### Insertar fila (addRow)

```javascript
// Después de un POST exitoso al servidor:
var nuevoRegistro = resultado.Registro; // objeto con los datos del nuevo registro

CB_DATATABLES.addRow(tabla, nuevoRegistro, true);
// true = navegar a la página que contiene el nuevo registro
// Si filtros activos ocultan el registro → se muestra toast con enlace "Limpiar filtros"

GestorCrud.resaltarFila(tabla, nuevoRegistro.IdMiEntidad);
// Scroll + animación verde 2.5s sobre la fila
```

### Actualizar fila (updateRow)

```javascript
var registroActualizado = resultado.Registro;

CB_DATATABLES.updateRow(tabla, registroActualizado);
// Reemplaza los datos de la fila con PK = registroActualizado.IdMiEntidad
// Requiere rowId configurado en crearTabla

GestorCrud.resaltarFila(tabla, registroActualizado.IdMiEntidad);
```

### Eliminar fila (deleteRow)

```javascript
CB_DATATABLES.deleteRow(tabla, id);
// Elimina la fila con PK = id del DOM y de los datos internos de DT
// Requiere rowId configurado en crearTabla
```

### Leer datos de una fila (para cargar en Offcanvas)

```javascript
function _cargarEnOffcanvas(id) {
    // Opción 1: leer desde DataTables
    var fila = tabla.row('#' + id);
    if (fila.any()) {
        var datos = fila.data();
        // poblar campos del formulario
        document.getElementById('NombreCampo').value = datos.NombreCampo || '';
        // ...
    }

    // Mostrar offcanvas
    bootstrap.Offcanvas.getOrCreateInstance(
        document.getElementById('offcanvas-formulario')
    ).show();
}
```

### Patrón completo de guardado con GestorCrud

```javascript
async function _guardar() {
    var modo  = document.getElementById('form-modo').value; // 'nuevo' | 'editar'
    var token = document.querySelector('#offcanvas-formulario [name="__RequestVerificationToken"]')?.value
             || document.querySelector('[name="__RequestVerificationToken"]')?.value;

    await GestorCrud.guardarYActualizar({
        url       : window.MI_CONFIG.urls.guardar,
        formId    : 'form-registro',
        token     : token,
        modulo    : 'MiArea',
        control   : 'btn-guardar',
        btnSubmit : '#btn-guardar',
        contenedor: '#offcanvas-formulario .offcanvas-body',
        textoBusy : 'Guardando...',
        onExito   : async function (resultado) {
            // 1) Limpiar UI
            if (modo === 'nuevo') {
                FormStateManager.limpiar('form-registro');
                document.getElementById('campo-id-oculto').value = '0';
                document.getElementById('form-errores')?.classList.add('d-none');
            } else {
                bootstrap.Offcanvas.getInstance(
                    document.getElementById('offcanvas-formulario')
                )?.hide();
            }

            // 2) Actualizar tabla por PK
            if (resultado.Registro) {
                var pk = resultado.Registro.IdMiEntidad;
                if (modo === 'nuevo') {
                    await CB_TABLA.addRow(tabla, resultado.Registro, true);
                } else {
                    await CB_TABLA.updateRow(tabla, resultado.Registro);
                }
                CB_TABLA.resaltarFila(tabla, pk);
            }
        }
    });
}
```

---

## 11. Persistencia en localStorage

La fachada `CB_DATATABLES` persiste automáticamente las preferencias del usuario.

### Claves de localStorage por tabla

| Clave | Cuándo se guarda | Descripción |
|---|---|---|
| `cb-dt-state-{pid}` | Solo si "Recordar filtros" está ON | Estado completo: filtros, búsqueda, orden |
| `cb-dt-recordar-{pid}` | Al activar/desactivar el toggle | `'true'` o `'false'` |
| `cb-dt-filtros-{pid}` | (Legacy — ya no se usa activamente) | Visibilidad del panel de filtros |
| `cb-dt-colorder-{pid}` | Siempre, al reordenar columnas | Array del orden de columnas (drag & drop) |
| `cb-dt-paging-{pid}` | Siempre, al cambiar página o tamaño | `{ length: 25, start: 0 }` |

> **Persistencia independiente por tipo:**
> - **Layout** (orden de columnas, tamaño de página): se persiste siempre.
> - **Filtros** (búsqueda, orden de datos): solo si el usuario activa el toggle "Recordar filtros".

### ID de persistencia

El `persistenceId` se define en el atributo `data-cb-persistencia` del `<table>`.

```html
<table id="tblMiEntidad"
       data-cb-persistencia="MiArea_tblMiEntidad_20260601">
```

**Reglas:**
- Es único por tabla en toda la solución.
- Incluye el área, el nombre de la tabla y la fecha (YYYYMMDD) para invalidar
  cuando cambia la estructura de columnas.
- Nunca duplicar entre tablas distintas.

---

## 12. Fachada `CB_TABLA` — motor agnóstico

`window.CB_TABLA` es una fachada que detecta el motor (Tabulator o DataTables)
por duck-typing y delega a la API correspondiente. Usar siempre `CB_TABLA`
en el código de módulos de vista para que cambiar el motor no requiera tocar el JS.

```javascript
// API pública:
CB_TABLA.esDataTables(tabla)           → boolean
CB_TABLA.esTabulator(tabla)            → boolean
CB_TABLA.addRow(tabla, registro, alInicio)     → void
CB_TABLA.updateRow(tabla, registro)           → void
CB_TABLA.deleteRow(tabla, pkValor)            → void
CB_TABLA.resaltarFila(tabla, pkValor)         → void
CB_TABLA.getSelectedData(tabla)               → Array
CB_TABLA.clearFilters(tabla)                  → void
CB_TABLA.redraw(tabla)                        → void
```

---

## 13. Patrón completo — módulo JS de vista

Estructura IIFE estándar para un módulo de vista con DataTables:

```javascript
/**
 * mi-entidad.js — Módulo JS para MiArea/MiEntidad/Index
 *
 * Dependencias: Bootstrap 5, CB_DATATABLES, CB_TABLA, GestorCrud,
 *               ModalConfirmacion, CB_BUSY, FormValidator, Notify
 *
 * Archivo: wwwroot/js/areas/mi-area/mi-entidad.js
 * Versión: 1.0.0
 */
(function () {
    'use strict';

    var CFG   = window.MI_CONFIG || {};
    var tabla = null;

    document.addEventListener('DOMContentLoaded', function () {
        if (!document.getElementById('tblMiEntidad')) return;
        _initTabla();
        _initFiltros();
        _initOffcanvas();
        _initBulkActions();
    });

    // ── TABLA ─────────────────────────────────────────────────────────────────

    function _initTabla() {
        tabla = CB_DATATABLES.crearTabla('#tblMiEntidad', {
            rowId : 'IdMiEntidad',
            data  : CFG.data || [],
            order : [[1, 'asc']],

            columns: [
                {
                    data: null, title: 'Acciones',
                    orderable: false, searchable: false, columnControl: [],
                    className: 'text-center', width: '110px',
                    render: _renderAcciones
                },
                { data: 'Nombre',      title: 'Nombre'      },
                { data: 'Descripcion', title: 'Descripcion' },
                {
                    data: 'Activo', title: 'Activo',
                    className: 'text-center',
                    render: function (v, type) {
                        if (type !== 'display') return v;
                        return v
                            ? "<span class='badge bg-success'>Sí</span>"
                            : "<span class='badge bg-secondary'>No</span>";
                    }
                }
            ],

            onSelect: _actualizarBulk,

            // delegacion: botones dentro de filas
            drawCallback: function () {}  // no necesario con delegación en contenedor
        });

        // Delegación de eventos — vincular UNA VEZ en el contenedor de la tabla
        document.getElementById('tblMiEntidad').addEventListener('click', function (e) {
            var btnEditar = e.target.closest('.js-editar');
            if (btnEditar) {
                _cargarEnOffcanvas(parseInt(btnEditar.dataset.id, 10));
            }

            var btnEliminar = e.target.closest('.js-eliminar');
            if (btnEliminar && !btnEliminar.disabled) {
                _confirmarEliminar(btnEliminar);
            }
        });
    }

    function _renderAcciones(data, type, row) {
        if (type !== 'display') return '';
        var id     = row.IdMiEntidad;
        var nombre = (row.Nombre || '').replace(/"/g, '&quot;');
        return (
            "<button type='button' class='btn btn-success btn-sm js-editar'" +
            " data-id='" + id + "' title='Editar'>" +
            "<i class='fa-solid fa-pencil'></i></button>" +

            "<button type='button' class='btn btn-danger btn-sm ms-1 js-eliminar'" +
            " data-id='" + id + "' data-nombre='" + nombre + "' title='Eliminar'>" +
            "<i class='fa-solid fa-trash'></i></button>"
        );
    }

    // ── FILTROS GET ───────────────────────────────────────────────────────────

    function _initFiltros() {
        document.getElementById('btn-limpiar-filtros')?.addEventListener('click', function () {
            window.location.href = CFG.urls.index; // redirige sin parámetros de filtro
        });
    }

    // ── OFFCANVAS ─────────────────────────────────────────────────────────────

    function _initOffcanvas() {
        if (!document.getElementById('offcanvas-formulario')) return;

        FormValidator.crearDesdeModelo({
            formularioId: 'form-registro',
            onSubmit: async function () { await _guardar(); }
        });

        // Btn Nuevo
        document.getElementById('btn-nuevo')?.addEventListener('click', function () {
            FormStateManager.limpiar('form-registro');
            document.getElementById('form-modo').value       = 'nuevo';
            document.getElementById('campo-id-oculto').value = '0';
            document.getElementById('offcanvas-titulo').textContent = 'Nueva entidad';
            document.getElementById('form-errores')?.classList.add('d-none');
            bootstrap.Offcanvas.getOrCreateInstance(
                document.getElementById('offcanvas-formulario')
            ).show();
        });

        // Btn Limpiar — EST-01: confirmación obligatoria
        document.getElementById('btn-limpiar')?.addEventListener('click', function () {
            ModalConfirmacion.mostrar({
                titulo    : 'Limpiar formulario',
                mensaje   : '¿Seguro que deseas limpiar el formulario?',
                variante  : 'warning',
                etiquetaOk: 'Sí, limpiar',
                onConfirm : function () {
                    FormStateManager.limpiar('form-registro');
                    document.getElementById('form-modo').value       = 'nuevo';
                    document.getElementById('campo-id-oculto').value = '0';
                    document.getElementById('form-errores')?.classList.add('d-none');
                }
            });
        });

        // Btn Guardar → dispara submit → FormValidator → _guardar
        document.getElementById('btn-guardar')?.addEventListener('click', function () {
            document.getElementById('form-registro')?.dispatchEvent(
                new Event('submit', { bubbles: true, cancelable: true })
            );
        });
    }

    function _cargarEnOffcanvas(id) {
        var fila = tabla.row('#' + id);
        var datos = fila.any() ? fila.data() : (CFG.data || []).find(function (d) {
            return d.IdMiEntidad == id;
        });
        if (!datos) return;

        document.getElementById('form-modo').value          = 'editar';
        document.getElementById('campo-id-oculto').value    = id;
        document.getElementById('offcanvas-titulo').textContent = 'Editar: ' + (datos.Nombre || '');
        document.getElementById('Nombre').value      = datos.Nombre      || '';
        document.getElementById('Descripcion').value = datos.Descripcion || '';

        ['Nombre', 'Descripcion'].forEach(function (n) {
            var el = document.getElementById(n);
            if (el) el.classList.remove('is-invalid', 'is-valid');
        });
        document.getElementById('form-errores')?.classList.add('d-none');

        bootstrap.Offcanvas.getOrCreateInstance(
            document.getElementById('offcanvas-formulario')
        ).show();
    }

    function _confirmarEliminar(btn) {
        var id     = parseInt(btn.dataset.id, 10);
        var nombre = btn.dataset.nombre || 'este registro';

        ModalConfirmacion.mostrar({
            titulo    : 'Eliminar registro',
            mensaje   : '¿Deseas eliminar "' + nombre + '"? Esta acción no se puede deshacer.',
            etiquetaOk: 'Eliminar',
            onConfirm : async function () {
                var token = document.querySelector('[name="__RequestVerificationToken"]')?.value || '';
                await CB_BUSY.run(btn, async function () {
                    var response = await fetch(CFG.urls.eliminar + '/' + id, {
                        method : 'POST',
                        headers: {
                            'X-Requested-With'        : 'XMLHttpRequest',
                            'RequestVerificationToken': token
                        }
                    });
                    var resultado = {};
                    try { resultado = await response.json(); } catch (_) {}

                    if (!response.ok || resultado.IsSuccess === false) {
                        ErrorResponse.manejar(response, resultado);
                        return;
                    }
                    CB_TABLA.deleteRow(tabla, id);
                    Notify.success(resultado.Message || 'Registro eliminado.');
                });
            }
        });
    }

    async function _guardar() {
        var modo  = document.getElementById('form-modo').value;
        var token = document.querySelector('#offcanvas-formulario [name="__RequestVerificationToken"]')?.value
                 || document.querySelector('[name="__RequestVerificationToken"]')?.value;

        await GestorCrud.guardarYActualizar({
            url       : CFG.urls.guardar,
            formId    : 'form-registro',
            token     : token,
            modulo    : 'MiArea',
            control   : 'btn-guardar',
            btnSubmit : '#btn-guardar',
            contenedor: '#offcanvas-formulario .offcanvas-body',
            textoBusy : 'Guardando...',
            onExito   : async function (resultado) {
                if (modo === 'nuevo') {
                    FormStateManager.limpiar('form-registro');
                    document.getElementById('campo-id-oculto').value = '0';
                    document.getElementById('form-errores')?.classList.add('d-none');
                } else {
                    bootstrap.Offcanvas.getInstance(
                        document.getElementById('offcanvas-formulario')
                    )?.hide();
                }

                if (resultado.Registro) {
                    var pk = resultado.Registro.IdMiEntidad;
                    if (modo === 'nuevo') {
                        await CB_TABLA.addRow(tabla, resultado.Registro, true);
                    } else {
                        await CB_TABLA.updateRow(tabla, resultado.Registro);
                    }
                    CB_TABLA.resaltarFila(tabla, pk);
                }
            }
        });
    }

    // ── ACCIONES BULK ─────────────────────────────────────────────────────────

    function _initBulkActions() {
        document.getElementById('btn-deseleccionar')?.addEventListener('click', function () {
            tabla.rows().deselect();
        });

        document.getElementById('btn-eliminar-bulk')?.addEventListener('click', function () {
            var ids = tabla.rows({ selected: true }).data().toArray()
                .map(function (r) { return r.IdMiEntidad; });
            if (!ids.length) return;

            ModalConfirmacion.mostrar({
                titulo  : 'Eliminar ' + ids.length + ' registros',
                mensaje : '¿Deseas eliminar los ' + ids.length + ' registros seleccionados?',
                onConfirm: function () {
                    tabla.rows({ selected: true }).remove().draw();
                    // En módulo real: fetch DELETE → luego remove().draw()
                }
            });
        });
    }

    function _actualizarBulk(count) {
        var zonaBulk = document.getElementById('zona-bulk');
        var lbl      = document.getElementById('lbl-seleccionados');
        if (!zonaBulk) return;
        if (count > 0) {
            zonaBulk.classList.remove('d-none');
            zonaBulk.classList.add('d-flex');
            if (lbl) lbl.textContent = count + (count === 1 ? ' seleccionado' : ' seleccionados');
        } else {
            zonaBulk.classList.add('d-none');
            zonaBulk.classList.remove('d-flex');
        }
    }

})();
```

---

## 14. Assets y scripts en la vista Razor

### Orden de carga (crítico)

```cshtml
@* En @section Styles — SIEMPRE después del layout *@
@section Styles {
    <link rel="stylesheet" href="~/lib/datatables/css/datatables.min.css" asp-append-version="true"/>
    <link rel="stylesheet" href="~/css/datatables-cb.css" asp-append-version="true"/>
    @* datatables-cb.css DEBE ir después de datatables.min.css para sobreescribir correctamente *@
}

@* En @section Scripts — SIEMPRE después del layout *@
@section Scripts {
    <partial name="_ValidationScriptsPartial" />

    @* 1. Bundle DataTables (incluye DT 3.x + ColumnControl + ColReorder + Select + más) *@
    <script src="~/lib/datatables/js/datatables.min.js" asp-append-version="true"></script>

    @* 2. Fachada CB_DATATABLES *@
    <script src="~/js/core/datatables-cb.js" asp-append-version="true"></script>

    @* 3. Fachadas core (si no están en el layout) *@
    @* <script src="~/js/core/cb-tabla.js"          asp-append-version="true"></script> *@
    @* <script src="~/js/core/gestor-crud.js"       asp-append-version="true"></script> *@
    @* <script src="~/js/core/modal-confirmacion.js" asp-append-version="true"></script> *@
    @* <script src="~/js/core/cb-busy.js"           asp-append-version="true"></script> *@
    @* <script src="~/js/core/notify.js"            asp-append-version="true"></script> *@

    @* 4. Config inyectada desde servidor *@
    <script>
        window.MI_CONFIG = {
            data: @Html.Raw(jsonData),
            urls: {
                guardar:  '@Url.Action("Guardar",  "MiEntidad", new { area = "MiArea" })',
                eliminar: '@Url.Action("Eliminar", "MiEntidad", new { area = "MiArea" })'
            }
        };
    </script>

    @* 5. Módulo JS de la vista (último) *@
    <script src="~/js/areas/mi-area/mi-entidad.js" asp-append-version="true"></script>
}
```

### Inyección de datos desde el servidor

```csharp
// En el Controller — serializar los datos para la vista
@{
    var jsonData = System.Text.Json.JsonSerializer.Serialize(
        Model.Registros,
        new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = null }
    );
}

// En @section Scripts:
window.MI_CONFIG = {
    data: @Html.Raw(jsonData),   // ← datos sin serialización camelCase (null = PascalCase)
    urls: { ... }
};
```

> **Importante:** `PropertyNamingPolicy = null` mantiene los nombres de propiedades
> tal como están en C# (PascalCase). Las columnas en DataTables deben coincidir:
> `{ data: 'NombrePascalCase' }`.

---

## 15. Reglas y restricciones críticas

### ✅ Reglas obligatorias

| Regla | Descripción |
|---|---|
| **R-01** | Siempre usar `CB_DATATABLES.crearTabla` — nunca `new DataTable()` directamente |
| **R-02** | El `data-cb-persistencia` del `<table>` debe ser único en toda la solución |
| **R-03** | Declarar `rowId` si se usan `addRow` / `updateRow` / `deleteRow` / `resaltarFila` |
| **R-04** | La columna de Acciones debe tener `columnControl: []`, `orderable: false`, `searchable: false` |
| **R-05** | En `render`, retornar el valor limpio cuando `type !== 'display'` (para sort/filter correcto) |
| **R-06** | Los assets deben ir en `@section Styles` y `@section Scripts` — nunca inline en el body |
| **R-07** | `datatables-cb.css` DEBE cargarse después de `datatables.min.css` (sobreescribe variables) |
| **R-08** | Usar delegación de eventos en el contenedor `<table>` — no re-vincular en `drawCallback` |
| **R-09** | El botón Limpiar de formularios Offcanvas DEBE tener confirmación via `ModalConfirmacion` (EST-01) |
| **R-10** | Usar `CB_BUSY.run(btn, fn)` en acciones asíncronas de botones de fila (Eliminar, etc.) |

### ❌ Errores comunes

| Error | Consecuencia | Solución |
|---|---|---|
| `columnControl: { header: [...], content: [...] }` sin `target` | `TypeError: this._c.content.forEach is not a function` | Usar array anidado: `[['search', ...]]` |
| Duplicar `data-cb-persistencia` entre tablas | Preferencias de una tabla sobrescriben a la otra | Usar IDs únicos con fecha: `Area_tblNombre_YYYYMMDD` |
| Renderizar `TempData` en la vista | Mensajes duplicados | El layout los muestra; no agregarlos en la vista |
| `data` en camelCase del servidor | Columnas DT no encuentran datos | Usar `PropertyNamingPolicy = null` en la serialización |
| `ordenar/buscar` con HTML en la celda | Datos de sort/filter contienen HTML | En `render`, retornar `data` limpio cuando `type !== 'display'` |
| Olvidar `rowId` en `crearTabla` | `row('#id')` no resuelve; CRUD por PK falla silenciosamente (solo `console.warn`) | Declarar `rowId: 'IdEntidad'` siempre |
| Incluir `'order'` en nivel raíz de `columnControl` y `ordering: true` | Doble ícono de orden en cada columna | Quitar `'order'` del nivel raíz; `CB_DATATABLES` ya lo gestiona |

### Convenciones de nomenclatura

| Elemento | Patrón | Ejemplo |
|---|---|---|
| `id` del `<table>` | `tbl{Entidad}` | `tblCatalogos` |
| `data-cb-persistencia` | `{Area}_tbl{Entidad}_{YYYYMMDD}` | `Configuracion_tblCatalogos_20260424` |
| Variable JS de la tabla | `tabla` (local, dentro del IIFE) | `var tabla = null;` |
| Config global del módulo | `window.{MODULO}_CONFIG` | `window.MI_CONFIG` |
| Archivo JS del módulo | `wwwroot/js/areas/{area}/{entidad}.js` | `js/areas/configuracion/catalogos.js` |
| Clases CSS de botones en fila | `js-{accion}-{entidad}` | `.js-editar-catalogo`, `.js-eliminar` |

---

## Registro de cambios del documento

| Fecha | Cambio | Por qué |
|---|---|---|
| 2026-06-02 | Creación inicial — guía completa de implementación DataTables | Transferir el conocimiento acumulado en CBAppWeb al estándar portable para otros proyectos |

---

*Documentación generada para uso offline por agentes de IA y equipos de desarrollo.*  
*Versión: 1.0.0 · Fecha: 2026-06-02*  
*Basada en: `datatables-cb.js` v1.5.0, `cb-tabla.js` v1.0.0, `datatables-cb.css` v1.5.0, `_TablaControles.cshtml` v1.3.0*

