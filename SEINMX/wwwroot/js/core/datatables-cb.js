/**
 * datatables-cb.js  —  Módulo central DataTables para CBAppWeb
 *
 * Proporciona configuración base, localización español, ColumnControl y
 * utilidades para todas las tablas DataTables del proyecto (tablas planas).
 *
 * REGLA DE USO:
 *   - Tabulator  → tablas con estructura de árbol (treeview)
 *   - DataTables → todas las demás tablas planas
 *
 * API pública  (window.CB_DATATABLES):
 *   crearTabla(selector, persistenceId, opciones)   → DataTables instance
 *   placeholderHtml(icono, mensaje)                 → string
 *   resaltarFila(dt, pkValor)                       → void
 *   addRow(dt, registro, alInicio)                  → void   (v1.0.7)
 *   updateRow(dt, registro)                         → void   (v1.0.7)
 *   deleteRow(dt, pkValor)                          → void   (v1.0.7)
 *
 * Botones declarativos — patrón data-cb-* (mismo que tabulator-cb.js):
 *   Contenedor: div[data-cb-controles] con atributos:
 *     data-cb-tabla="#selector"         — selector CSS de la tabla
 *     data-cb-persistencia="id"         — ID de persistencia en localStorage
 *     data-cb-csv="nombre.csv"          — nombre del archivo CSV al exportar
 *   Botones dentro del contenedor con data-cb-accion:
 *     exportar | imprimir | limpiar | restablecer | recordar-filtros
 *   Notas:
 *     - 'imprimir' añadido en v1.0.7 (paridad con tabulator-cb.js).
 *     - 'filtros' NO se implementa: ColumnControl ya expone su menú ☰ siempre visible,
 *       el toggle de ocultar/mostrar filtros no aplica a DataTables.
 *     - 'expandir'/'colapsar' no aplican (los árboles se manejan con Tabulator).
 *
 * Persistencia:
 *   Prefijo localStorage: "cb-dt-{persistenceId}"
 *   Preferencias de UI:     "cb-dt-filtros-{pid}" / "cb-dt-recordar-{pid}"
 *   Orden de columnas:      "cb-dt-colorder-{pid}"   (siempre persistente)
 *   Estado completo DT:     "cb-dt-state-{pid}"      (solo si recordar-filtros ON)
 *
 * ColumnControl (integrado en el bundle local):
 *   Se activa automáticamente en crearTabla a menos que opciones.columnControl === false.
 *   Dropdown ☰ por columna: lista de valores únicos + búsqueda interna,
 *   Select All / Deselect, y acciones de orden Asc/Desc/Clear dentro del menú.
 *   Las flechas ▲▼ de ordenamiento las provee DataTables nativo (ordering:true).
 *
 * ColReorder (integrado en el bundle local):
 *   Activo por defecto. Se desactiva con opciones.colReorder === false.
 *   Drag & drop de columnas desde el <th>. La columna 0 (checkbox) queda fija
 *   mediante la opción `columns: ':gt(0)'` de ColReorder 3.x.
 *   El orden se persiste vía stateSave (solo si el usuario activa "Recordar filtros").
 *
 * Dependencias:
 *   - wwwroot/lib/datatables/js/datatables.min.js  (bundle: DT 3.0.0-beta.2 + ColReorder
 *     3.0.0-beta.1 + ColumnControl 2.0.0-beta.1 + Buttons 4.0.0-beta.1 + Select
 *     4.0.0-beta.1 + Responsive 4.0.0-beta.1 + Moment 2.29.4 + pdfmake 0.2.7
 *     + FixedColumns 6.0.0-beta.1 + FixedHeader 5.0.0-beta.1)
 *   - wwwroot/lib/datatables/css/datatables.min.css
 *   - wwwroot/css/datatables-cb.css  (animaciones cb-row-guardado para DataTables)
 *   - NO requiere jQuery — DT 3.x usa su propia librería DOM interna
 *
 * Archivo:   wwwroot/js/core/datatables-cb.js
 * Versión:   1.5.0  |  Fecha: 2026-05-04
 * Cambios:   1.5.0 — Migración a DataTables 3.0.0-beta.2.
 *                    Breaking changes corregidos:
 *                    (1) ColReorder 3.x: fixedColumnsLeft eliminado → columna fija
 *                        se configura ahora con { columns: ':gt(0)' } que permite
 *                        reordenar SOLO columnas con índice > 0 (la 0 queda bloqueada).
 *                    (2) DT 3.x: settings._bInitComplete eliminado → reemplazado
 *                        por settings.initDone (bool).
 *                    (3) DT 3.x: API interna de columnas renombrada:
 *                        settings.aoColumns → settings.columns
 *                        col.mData          → col.data
 *                        col.sTitle         → col.title
 *                        Afecta a _descargarCsv.
 *                    NOTA: No son breaking changes (siguen funcionando sin cambios):
 *                        pagingType: 'full_numbers' → compatible vía compatMap de DT 3.x.
 *                        select.selector            → Select 4.x mantiene opción selector.
 *                        colReorder.order(arr,true) → misma firma en ColReorder 3.x.
 *            1.4.0 — Estándar data-cb-persistencia: crearTabla acepta firma corta
 *                    crearTabla(selector, opciones) leyendo persistenceId desde
 *                    data-cb-persistencia del elemento tabla. Firma larga sigue
 *                    funcionando por compatibilidad. _autoBindParaTabla usa el
 *                    registro interno como fuente única (no lee data-cb-persistencia
 *                    del contenedor de controles).
 *            1.3.0 — checkboxCol default cambiado de true → false.
 *                    Ahora la columna checkbox solo se agrega si checkboxCol:true
 *                    o multiSelect:true (consistente con Tabulator selectableRows:1).
 *                    selector de selección: 'td:first-child' con checkbox,
 *                    'tr' sin checkbox (clic en cualquier celda de la fila).
 *            1.2.5 — Fix timing en "Limpiar filtros" del toast de registro oculto.
 *                    Patrón callback _cbDtLimpiar_{timestamp} en window con closure
 *                    sobre dt/pkValor/btnLimpiarId. Orden: (1) limpiar filtros,
 *                    (2) dt.one('draw.dt', resaltarFila), (3) toast.hide(),
 *                    (4) delete window[cbKey]. Soluciona toast no cerrable y fila
 *                    sin resaltar por ejecución antes del draw.
 *            1.2.4 — _mostrarToastRegistroOculto migrada a Notify.warning — elimina
 *                    dependencia de #cb-toast-container (removido del layout).
 *                    Dependencia: wwwroot/js/core/notify.js.
 *            1.2.3 — Fix limpiar filtros persistentes: el botón "Limpiar filtros"
 *                    ahora borra `cb-dt-state-{pid}` de localStorage además de
 *                    limpiar los filtros en memoria, para que al recargar la página
 *                    los filtros eliminados no se vuelvan a aplicar.
 *            1.2.2 — Fix REQ-01: addRow detecta si el registro queda oculto por
 *                    filtros activos tras el draw. Si queda oculto, muestra aviso
 *                    al usuario (ver 1.2.4 para la migración a Notify).
 *                    No se limpian los filtros automáticamente.
 *                    Fix REQ-02: botón "Limpiar filtros" se activa correctamente al
 *                    cargar con filtros persistentes — _actualizarBtnLimpiar se llama
 *                    en init.dt además de en search.dt/draw.dt.
 *            1.2.1 — Fix addRow: el registro nuevo siempre es visible al usuario.
 *                    El bug raíz era que `dt.order()` SIEMPRE devuelve el orden
 *                    activo (nunca vacío), por lo que `hayOrdenUsuario` era siempre
 *                    true y el registro se agregaba al final. Solución: insertar con
 *                    `row.add().draw(false)` y luego navegar automáticamente a la
 *                    página que contiene el nuevo registro mediante `_irAPaginaDeRegistro`,
 *                    calculando su posición en el display ordenado actual.
 *            1.2.0 — (1) Fix recordar-filtros: stateSave siempre activo con callbacks
 *                    condicionales — al activar el toggle guarda inmediatamente con
 *                    dt.state.save(), sin necesidad de recargar la página.
 *                    (2) Fix botón limpiar: detecta filtros ColumnControl (search.fixed
 *                    con clave 'dtcc') además de dt.search() y dt.columns().search().
 *                    (3) Fix CSV: cabeceras limpias desde settings.columns[i].title
 *                    (sin texto "More..." del ColumnControl); filas solo con columnas
 *                    de datos (data !== null), excluye Acciones/checkbox/auditoria.
 *                    (4) Elimina acción 'imprimir' — no se usa en el proyecto.
 *                    (5) Nuevo estándar: columna PK (#) obligatoria después de Acciones.
 *            1.1.1 — Fix: traducciones de operadores de ColumnControl en inglés.
 *                    La estructura plana de LANG_ES.columnControl (contains,
 *                    notContains…) no coincidía con las rutas de i18n del bundle
 *                    (columnControl.search.text.contains, etc.).
 *                    Corregido usando estructura anidada:
 *                      columnControl.search.text.*     → operadores de texto
 *                      columnControl.search.number.*   → operadores numéricos
 *                      columnControl.search.datetime.* → operadores de fecha
 *            1.1.0 — Filtros por operador en el dropdown ☰ (ColumnControl).
 *                    'searchList' → 'search' (auto-detección de tipo por columna):
 *                      Texto     → searchText  : contiene, no contiene, igual,
 *                                   diferente, empieza con, termina en, vacío,
 *                                   no vacío.
 *                      Numérico  → searchNumber: =, ≠, >, ≥, <, ≤, vacío, no vacío.
 *                      Fecha     → searchDateTime: igual, diferente, después,
 *                                   antes, vacío, no vacío.
 *                    Para forzar un tipo específico en una columna concreta,
 *                    usar `columnControl: [['searchNumber', ...]]` en la
 *                    definición de esa columna.
 *            1.0.9 — Persistencia independiente de paginación (página actual +
 *                    `pageLength`). Nueva clave `cb-dt-paging-{pid}` que se
 *                    guarda en los eventos `page.dt` y `length.dt`, y se
 *                    restaura al inicializar. Independiente del toggle
 *                    "Recordar filtros" — la preferencia de cuántas filas
 *                    ver por página y en qué página se estaba es parte del
 *                    layout (como el orden de columnas), no de los filtros.
 *                    El botón "Restablecer" también limpia esta clave.
 *            1.0.8 — Fix crítico: addRow() ya NO manipula `settings.data`
 *                    directamente (pop/unshift). Esa mutación rompía los
 *                    índices internos (`display`, caché `_aSortData`) y producía:
 *                       TypeError: can't access property "_aSortData",
 *                                  f[t] is null
 *                    al intentar re-dibujar con ColumnControl/ColReorder
 *                    activos. Ahora `alInicio` se implementa vía
 *                    `clear() + rows.add([nuevo, ...antiguos])` SOLO cuando
 *                    no hay ordenamiento aplicado por el usuario; si hay
 *                    sort activo, el motor ya coloca la fila en la posición
 *                    correcta y `resaltarFila` se encarga de la visibilidad.
 *            1.0.7 — Paridad CRUD con CB_TABULATOR y nueva acción 'imprimir'.
 *                    + addRow(dt, registro, alInicio) — equivalente a tabla.addRow().
 *                    + updateRow(dt, registro)        — equivalente a tabla.updateData([...]).
 *                    + deleteRow(dt, pkValor)         — equivalente a tabla.deleteRow().
 *                    Todos requieren `opts.rowId` configurado en crearTabla (se emite
 *                    console.warn en runtime si falta, análogo a `index` en Tabulator).
 *                    + Nueva acción declarativa data-cb-accion="imprimir" que usa
 *                      dt.button('print:name') con fallback a window.print().
 *                    + Se elimina el caso 'filtros' de _vincularBoton: ColumnControl
 *                      ya expone el menú ☰ siempre visible, no tiene sentido un toggle.
 *            1.0.6 — Persistencia independiente del orden de columnas (ColReorder).
 *                    Nueva clave `cb-dt-colorder-{pid}` que guarda el arreglo
 *                    `dt.colReorder.order()` en el evento `column-reorder`.
 *                    Se restaura automáticamente al crear la tabla (después de
 *                    `init.dt`), sin depender del toggle "Recordar filtros".
 *                    Motivo: el orden de columnas es una preferencia de layout,
 *                    no de filtros; debe persistir siempre.
 *                    El botón "Restablecer" también elimina esta clave.
 *            1.0.5 — Activación de ColReorder (drag & drop de columnas).
 *                    Habilitado por defecto; se desactiva con `opts.colReorder === false`.
 *                    Cuando hay columna checkbox (`checkboxCol: true`) la col. 0
 *                    queda fija vía `columns: ':gt(0)'` (ColReorder 3.x) para no
 *                    romper la selección.
 *                    El orden de columnas se persiste automáticamente vía `stateSave`
 *                    cuando el usuario activa "Recordar filtros". El botón
 *                    "Restablecer" limpia también este estado (ya cubierto por
 *                    `cb-dt-state-{pid}` → location.reload()).
 *            1.0.4 — Fix ícono de orden duplicado en el header.
 *                    Se elimina 'order' del nivel raíz de columnControl; las
 *                    flechas nativas de DataTables (ordering:true) ya cubren
 *                    ese rol, evitando dos toggles de orden en el mismo <th>.
 *                    El menú ☰ conserva orderAsc/orderDesc/orderClear.
 *            1.0.3 — Fix definitivo de ColumnControl v1.2.1.
 *                    La API real usa ARRAYS ANIDADOS para crear el dropdown (☰),
 *                    no objetos con claves 'header'/'content'/'dropdown'.
 *                    Configuración correcta:
 *                      columnControl: ['order', ['searchList', 'orderAsc', 'orderDesc', 'orderClear']]
 *                    Tanto { header, content } como { header, dropdown } causaban:
 *                      "this._c.content.forEach is not a function"
 *                    Referencias: Datatables.md §16.3, §16.14 (Revisión 1.2)
 *            1.0.2 — Fix incompleto: usaba { header:[], content:[] } que también falla.
 *            1.0.1 — Fix incompleto: usaba content:[] sin header. Mismo error.
 *            1.0.0 — Versión inicial.
 */
(function (global) {
    'use strict';

    // ── Registro interno de tablas ────────────────────────────────────────────
    var _tablas = {};

    // Duracion de la animacion CSS cb-row-guardado (debe coincidir con datatables-cb.css)
    var ROW_HIGHLIGHT_MS = 2500;

    // ── Localización español completa ─────────────────────────────────────────
    var LANG_ES = {
        processing    : 'Procesando\u2026',
        search        : 'Buscar:',
        lengthMenu    : '_MENU_ por p\u00E1gina',
        info          : 'Mostrando _START_\u2013_END_ de _TOTAL_ registros',
        infoEmpty     : 'Mostrando 0 registros',
        infoFiltered  : '(filtrado de _MAX_ registros totales)',
        infoPostFix   : '',
        loadingRecords: 'Cargando\u2026',
        zeroRecords   : 'No se encontraron resultados',
        emptyTable    : '', // Se sobreescribe con placeholderHtml en crearTabla
        paginate: {
            first   : '\u00AB',
            previous: '\u2039',
            next    : '\u203A',
            last    : '\u00BB'
        },
        aria: {
            orderable: 'Ordenar por esta columna',
            orderableReverse: 'Orden inverso',
            orderableRemove : 'Quitar orden',
            paginate: {
                first   : 'Primera p\u00E1gina',
                previous: 'P\u00E1gina anterior',
                next    : 'P\u00E1gina siguiente',
                last    : '\u00FAltima p\u00E1gina'
            },
            select: {
                rows: {
                    _  : '%d filas seleccionadas',
                    0  : '',
                    1  : '1 fila seleccionada'
                }
            }
        },
        select: {
            rows: {
                _: '%d seleccionados',
                0: '',
                1: '1 seleccionado'
            }
        },
        buttons: {
            copy      : 'Copiar',
            csv       : 'CSV',
            excel     : 'Excel',
            pdf       : 'PDF',
            print     : 'Imprimir',
            colvis    : 'Columnas',
            collection: 'Exportar'
        },
        columnControl: {
            search: {
                text: {
                    contains   : 'Contiene',
                    notContains: 'No contiene',
                    equal      : 'Igual a',
                    notEqual   : 'Diferente a',
                    starts     : 'Empieza con',
                    ends       : 'Termina en',
                    empty      : 'Vacío',
                    notEmpty   : 'No vacío'
                },
                number: {
                    equal         : 'Igual a',
                    notEqual      : 'Diferente a',
                    greater       : 'Mayor que',
                    greaterOrEqual: 'Mayor o igual',
                    less          : 'Menor que',
                    lessOrEqual   : 'Menor o igual',
                    empty         : 'Vacío',
                    notEmpty      : 'No vacío'
                },
                datetime: {
                    equal   : 'Igual a',
                    notEqual: 'Diferente a',
                    greater : 'Después de',
                    less    : 'Antes de',
                    empty   : 'Vacío',
                    notEmpty: 'No vacío'
                }
            }
        }
    };

    // ── Configuración base ────────────────────────────────────────────────────
    var BASE_CFG = {
        paging         : true,
        pageLength     : 25,
        lengthMenu     : [[10, 25, 50, 100, -1], [10, 25, 50, 100, 'Todos']],
        pagingType     : 'full_numbers',
        ordering       : true,
        searching      : true,
        info           : true,
        autoWidth      : false,
        responsive     : true,
        stateSave      : false,   // controlado por preferencia del usuario
        language       : LANG_ES,
        // Select: una fila a la vez (mismo comportamiento que Tabulator selectableRows:1)
        select         : { style: 'single', selector: 'td:first-child' },
        layout: {
            topStart   : 'pageLength',
            topEnd     : 'search',
            bottomStart: 'info',
            bottomEnd  : 'paging'
        }
    };

    // =========================================================================
    // placeholderHtml
    // =========================================================================
    /**
     * Genera HTML para el estado vacío estándar de DataTables.
     * Sigue el mismo estándar visual que tabulator-cb.js.
     *
     * @param {string} [icono]   Clases FontAwesome (ej. "fa-solid fa-inbox")
     * @param {string} [mensaje] Texto (default: "Sin registros.")
     * @returns {string}
     */
    function placeholderHtml(icono, mensaje) {
        var ic  = icono   || 'fa-solid fa-inbox';
        var msg = mensaje || 'Sin registros.';
        return '<div class="d-flex flex-column align-items-center py-4 text-muted">' +
               '<i class="' + ic + ' fa-2x mb-2 opacity-25"></i>' +
               '<span>' + msg + '</span>' +
               '</div>';
    }

    // =========================================================================
    // crearTabla
    // =========================================================================
    /**
     * Crea una instancia de DataTables con la configuración base de CBAppWeb.
     * Registra la tabla internamente para que los controles declarativos
     * data-cb-* puedan vincularse automáticamente.
     *
     * Acepta dos firmas:
     *   crearTabla(selector, opciones)               ← NUEVA (recomendada)
     *   crearTabla(selector, persistenceId, opciones) ← legado, sigue funcionando
     *
     * En la firma nueva el persistenceId se lee del atributo data-cb-persistencia
     * del elemento tabla (fuente única de verdad — estándar v1.4.0).
     *
     * @param {string}        selector       CSS selector de la <table> (ej. "#tblMiTabla")
     * @param {string|Object} persistenceId  ID único de persistencia O opciones (firma corta)
     * @param {Object}        [opciones]     Opciones adicionales de DataTables.
     *   @param {string}  [opciones.rowId]           Campo PK para row ID (ej. 'IdEntidad').
     *                                                Permite resaltarFila + deleteRow.
     *   @param {boolean} [opciones.multiSelect]     true → selección múltiple (default: false)
 *   @param {boolean} [opciones.columnControl]   false → desactiva ColumnControl (default: true)
 *   @param {boolean} [opciones.checkboxCol]     true → agrega columna checkbox al inicio (default: false — sin checkbox)
 *   @param {boolean} [opciones.colReorder]      false → desactiva drag & drop de columnas (default: true)
 *   @param {boolean} [opciones.stateSave]       true → activa persistencia de estado (default: false)
     *   @param {Function}[opciones.onSelect]        Callback cuando cambia la selección: fn(count, rows)
     *
     * @returns {DataTables.Api} Instancia de DataTables
     */
    function crearTabla(selector, persistenceId, opciones) {
        // ── Auto-detección de firma corta: crearTabla(selector, opciones) ─────
        if (persistenceId !== null && persistenceId !== undefined &&
                typeof persistenceId === 'object') {
            opciones      = persistenceId;
            persistenceId = null;
        }
        var opts = opciones || {};

        // ── Leer persistenceId del elemento si no se proporcionó ──────────────
        // Fuente única: atributo data-cb-persistencia en el elemento tabla.
        if (!persistenceId) {
            var elPid = document.querySelector(selector);
            persistenceId = (elPid && elPid.getAttribute('data-cb-persistencia')) || '';
        }

        // ── Leer preferencias de persistencia ─────────────────────────────────
        var claveRecordar = 'cb-dt-recordar-' + persistenceId;
        var claveFiltros  = 'cb-dt-filtros-'  + persistenceId;
        var recordarFiltros  = localStorage.getItem(claveRecordar) === 'true';
        var filtrosVisibles  = localStorage.getItem(claveFiltros)  === 'true';

        // ── Construir configuración ────────────────────────────────────────────
        var cfg = _mezclar({}, BASE_CFG);

        // Lenguaje — placeholder HTML en estado vacío
        cfg.language = _mezclar({}, LANG_ES, {
            emptyTable: placeholderHtml(opts.placeholderIcono, opts.placeholderMensaje)
        });

        // Row ID (para resaltarFila y operaciones por PK)
        if (opts.rowId) {
            cfg.rowId = opts.rowId;
        }

        // Columna checkbox — solo si se solicita explícitamente o con multiSelect.
        // Default: false — sin checkbox (consistente con Tabulator selectableRows:1).
        // Usar checkboxCol:true solo cuando la tabla requiere acciones masivas.
        var conCheckbox = opts.checkboxCol === true || opts.multiSelect === true;

        // Selección: single (default) o múltiple.
        // Con checkbox    → selector 'td:first-child': solo el clic en la celda checkbox activa la selección.
        // Sin checkbox    → selector 'tr':             cualquier clic en la fila la selecciona visualmente.
        cfg.select = {
            style   : opts.multiSelect ? 'multi' : 'single',
            selector: conCheckbox ? 'td:first-child' : 'tr'
        };

        // Persistencia de estado — siempre activa con callbacks condicionales.
        // El saveCallback comprueba en tiempo real si recordarFiltros está ON,
        // lo que permite que el toggle tome efecto inmediato sin recargar la página.
        cfg.stateSave         = true;
        cfg.stateSaveCallback = _makeStateSaveCallback(persistenceId);
        cfg.stateLoadCallback = _makeStateLoadCallback(persistenceId);


        // ColumnControl 1.2.1 — API real (arrays anidadas):
        //
        //   columnControl: [ ['searchList', 'orderAsc', 'orderDesc', 'orderClear'] ]
        //
        //   - Un array anidado [ ] se convierte automáticamente en el botón ☰ (dropdown)
        //   - Los strings dentro del array anidada son los controles del panel flotante
        //   - NO se incluye 'order' a nivel raíz: DataTables ya renderiza las flechas
        //     nativas ▲▼ en el <th> cuando `ordering: true` (BASE_CFG). Incluir 'order'
        //     producía un segundo toggle duplicado en el header.
        //
        // ⚠️  INCORRECTO — causa "this._c.content.forEach is not a function":
        //     { header: [...], content: [...] }   ← 'header'/'content' sin 'target' = error
        //     { header: [...], dropdown: [...] }  ← claves inexistentes en v1.x = error
        //
        // ✅ CORRECTO — forma expandida SOLO si se necesita 'target':
        //     { target: 0, content: ['searchList', 'orderAsc', 'orderDesc', 'orderClear'] }
        if (opts.columnControl !== false) {
            cfg.columnControl = [
                ['search', 'orderAsc', 'orderDesc', 'orderClear'] // array anidado = botón ☰
            ];
        }

        // ColReorder 3.0.0-beta.1 — drag & drop de columnas desde el header.
        //   - Habilitado por defecto. Se desactiva con `opts.colReorder === false`.
        //   - `columns: ':gt(0)'` permite reordenar SOLO columnas con índice > 0,
        //     bloqueando la columna 0 (checkbox) para no romper la selección.
        //     Equivale al antiguo `fixedColumnsLeft: 1` de ColReorder 2.x, pero
        //     la opción fue eliminada en ColReorder 3.x.
        //   - Persistencia: el orden se guarda en `cb-dt-colorder-{pid}` en el
        //     evento `column-reorder` y se restaura al inicializar la tabla
        //     (hook después de `new DataTable`). Es INDEPENDIENTE del toggle
        //     "Recordar filtros" porque el orden de columnas es preferencia de
        //     layout, no de búsqueda.
        //   - El botón "Restablecer" elimina la clave `cb-dt-colorder-{pid}`.
        if (opts.colReorder !== false) {
            cfg.colReorder = conCheckbox
                ? { columns: ':gt(0)' }   // bloquea col 0 (checkbox) — reemplaza fixedColumnsLeft
                : true;
        }

        // Opciones adicionales del caller (sobreescriben BASE_CFG pero no columnControl)
        var extraKeys = ['data', 'ajax', 'columns', 'columnDefs', 'order', 'pageLength',
                         'serverSide', 'processing', 'scrollX', 'scrollY', 'layout',
                         'fixedHeader', 'responsive', 'rowCallback', 'drawCallback',
                         'initComplete', 'createdRow'];
        for (var i = 0; i < extraKeys.length; i++) {
            if (opts[extraKeys[i]] !== undefined) {
                cfg[extraKeys[i]] = opts[extraKeys[i]];
            }
        }

        // ── Restaurar paginación persistida (pageLength) ───────────────────────
        // Se aplica ANTES de instanciar la tabla para que `pageLength` inicial
        // sea el preferido por el usuario. La página actual se restaura después
        // (requiere la tabla ya creada).
        var pagingGuardado = _leerPaginacion(persistenceId);
        if (pagingGuardado && typeof pagingGuardado.length === 'number') {
            cfg.pageLength = pagingGuardado.length;
        }

        // Columna checkbox — se antepone a columnDefs si aplica
        if (conCheckbox && cfg.columns) {
            cfg.columnDefs = cfg.columnDefs || [];
            // Agregar columna checkbox como primera columna si no existe ya
            var yaExiste = cfg.columns.some(function (c) {
                return c.data === null && c.className && c.className.indexOf('dt-select') !== -1;
            });
            if (!yaExiste) {
                cfg.columns = [_columnaCheckbox()].concat(cfg.columns);
            }
        }

        // ── Inicializar DataTables ─────────────────────────────────────────────
        var dt = new DataTable(selector, cfg);

        // ── Persistencia del orden de columnas (ColReorder) ───────────────────
        // Clave propia, independiente de stateSave. Se aplica solo si la
        // extensión ColReorder está activa en esta tabla.
        if (opts.colReorder !== false) {
            _configurarPersistenciaColReorder(dt, persistenceId);
        }

        // ── Persistencia de paginación (siempre activa) ───────────────────────
        // Clave propia `cb-dt-paging-{pid}`. Se hidrata aquí (página actual) y
        // se engancha a `page.dt` y `length.dt` para guardar cambios.
        _configurarPersistenciaPaginacion(dt, persistenceId, pagingGuardado);

        // ── Registrar en _tablas ───────────────────────────────────────────────
        _tablas[selector] = {
            instancia      : dt,
            persistenceId  : persistenceId,
            filtrosVisibles: filtrosVisibles,
            recordarFiltros: recordarFiltros,
            rowId          : opts.rowId || null
        };

        // ── Estado visual inicial: clase cb-dt-filtros-visibles ───────────────
        if (filtrosVisibles) {
            var el = document.querySelector(selector);
            if (el) el.classList.add('cb-dt-filtros-visibles');
        }

        // ── Vincular botones declarativos ─────────────────────────────────────
        _autoBindParaTabla(selector);

        // ── Callback de selección → ActionBar BULK ────────────────────────────
        if (typeof opts.onSelect === 'function') {
            dt.on('select deselect', function () {
                var count = dt.rows({ selected: true }).count();
                var rows  = dt.rows({ selected: true }).data().toArray();
                opts.onSelect(count, rows);
            });
        }

        return dt;
    }

    // =========================================================================
    // resaltarFila
    // =========================================================================
    /**
     * Desplaza la tabla al registro con el pkValor dado y aplica animación
     * verde de 2.5 s. Requiere que crearTabla haya recibido opciones.rowId.
     *
     * GestorCrud.resaltarFila() delega aquí cuando detecta una instancia DataTables.
     *
     * @param {DataTables.Api} dt       Instancia de DataTables
     * @param {*}              pkValor  Valor de la PK (mismo que rowId)
     */
    function resaltarFila(dt, pkValor) {
        if (!dt || pkValor == null) return;

        // Buscar el <tr> por row ID ('#' + pkValor, establecido por opciones.rowId)
        var rowNode = null;
        try {
            var rowApi = dt.row('#' + pkValor);
            if (rowApi && rowApi.node()) {
                rowNode = rowApi.node();
            }
        } catch (e) { /* row no encontrado */ }

        // Fallback: buscar en todos los datos por valor exacto de pkValor
        if (!rowNode) {
            dt.rows().every(function () {
                var data = this.data();
                if (!data) return;
                var vals = typeof data === 'object' ? Object.values(data) : [data];
                if (vals.some(function (v) { return v == pkValor; })) {
                    rowNode = this.node();
                    return false; // break
                }
            });
        }

        if (!rowNode) return;

        // Scroll hacia la fila
        rowNode.scrollIntoView({ behavior: 'smooth', block: 'center' });

        // Animación verde
        rowNode.classList.remove('cb-row-guardado');
        void rowNode.offsetWidth; // reflow para reiniciar la animación CSS
        rowNode.classList.add('cb-row-guardado');
        setTimeout(function () { rowNode.classList.remove('cb-row-guardado'); }, ROW_HIGHLIGHT_MS + 150);
    }

    // =========================================================================
    // _columnaCheckbox
    // =========================================================================
    /**
     * Genera la definición de columna checkbox para selección.
     * Columna sin datos, no buscable, no ordenable, con selector de DataTables.
     * @private
     */
    function _columnaCheckbox() {
        return {
            data          : null,
            title         : '',
            orderable     : false,
            searchable    : false,
            className     : 'dt-select text-center',
            width         : '32px',
            render        : function (data, type) {
                if (type === 'display') {
                    return '<input type="checkbox" class="form-check-input" />';
                }
                return '';
            }
        };
    }

    // =========================================================================
    // Persistencia en localStorage
    // =========================================================================
    function _makeStateSaveCallback(pid) {
        return function (settings, data) {
            // Solo guardar si el usuario activó "recordar filtros"
            if (localStorage.getItem('cb-dt-recordar-' + pid) !== 'true') return;
            try {
                localStorage.setItem('cb-dt-state-' + pid, JSON.stringify(data));
            } catch (e) { /* storage lleno — ignorar */ }
        };
    }

    function _makeStateLoadCallback(pid) {
        return function (settings) {
            // Solo restaurar si el usuario activó "recordar filtros"
            if (localStorage.getItem('cb-dt-recordar-' + pid) !== 'true') return null;
            try {
                var raw = localStorage.getItem('cb-dt-state-' + pid);
                return raw ? JSON.parse(raw) : null;
            } catch (e) {
                return null;
            }
        };
    }

    // =========================================================================
    // _configurarPersistenciaColReorder
    // =========================================================================
    /**
     * Persiste el orden de columnas (ColReorder) en `cb-dt-colorder-{pid}`,
     * independiente del stateSave global. Se guarda en el evento
     * `column-reorder` y se restaura una sola vez tras la inicialización.
     *
     * @param {DataTables.Api} dt   Instancia de DataTables
     * @param {string}         pid  persistenceId
     * @private
     */
    function _configurarPersistenciaColReorder(dt, pid) {
        var clave = 'cb-dt-colorder-' + pid;

        // Restaurar orden guardado (una sola vez, al terminar la inicialización).
        try {
            var raw = localStorage.getItem(clave);
            if (raw) {
                var orden = JSON.parse(raw);
                if (Array.isArray(orden) && dt.colReorder && typeof dt.colReorder.order === 'function') {
                    // Validar que el tamaño coincida con la cantidad actual de columnas;
                    // si la tabla cambió (añadida/eliminada columna), descartar el orden previo.
                    if (orden.length === dt.columns().count()) {
                        dt.colReorder.order(orden, true); // true = original indexes
                    } else {
                        localStorage.removeItem(clave);
                    }
                }
            }
        } catch (e) { /* JSON inválido o ColReorder no disponible — ignorar */ }

        // Guardar cada vez que el usuario reordene columnas.
        dt.on('column-reorder.dt', function () {
            try {
                var ord = dt.colReorder.order();
                localStorage.setItem(clave, JSON.stringify(ord));
            } catch (e) { /* storage lleno — ignorar */ }
        });
    }

    // =========================================================================
    // Persistencia de paginación  (cb-dt-paging-{pid})
    // =========================================================================
    /**
     * Lee la paginación guardada para un persistenceId dado.
     * @returns {{ length:number, start:number }|null}
     * @private
     */
    function _leerPaginacion(pid) {
        try {
            var raw = localStorage.getItem('cb-dt-paging-' + pid);
            if (!raw) return null;
            var obj = JSON.parse(raw);
            if (!obj || typeof obj !== 'object') return null;
            return {
                length: typeof obj.length === 'number' ? obj.length : null,
                start : typeof obj.start  === 'number' ? obj.start  : 0
            };
        } catch (e) { return null; }
    }

    /**
     * Restaura la página actual (`start`) una vez que la tabla ya está creada
     * y engancha los eventos `page.dt` / `length.dt` para persistir cambios.
     *
     * La preferencia de paginación es parte del *layout* del usuario (al
     * igual que el orden de columnas), no de los filtros — por eso persiste
     * siempre e independiente del toggle "Recordar filtros".
     *
     * @param {DataTables.Api} dt       Instancia de DataTables
     * @param {string}         pid      persistenceId
     * @param {Object|null}    guardado Resultado de _leerPaginacion (o null)
     * @private
     */
    function _configurarPersistenciaPaginacion(dt, pid, guardado) {
        var clave = 'cb-dt-paging-' + pid;

        // Restaurar página actual tras la inicialización (pageLength ya fue
        // aplicado en cfg antes de `new DataTable`).
        if (guardado && guardado.start > 0) {
            try {
                var info    = dt.page.info();
                var pagina  = Math.floor(guardado.start / (info.length || dt.page.len()));
                if (pagina > 0) dt.page(pagina).draw(false);
            } catch (e) { /* registro nuevo o datos reducidos — ignorar */ }
        }

        // Guardar cambios de página y de longitud
        var _guardar = function () {
            try {
                var info = dt.page.info();
                localStorage.setItem(clave, JSON.stringify({
                    length: info.length,
                    start : info.start
                }));
            } catch (e) { /* storage lleno — ignorar */ }
        };
        dt.on('page.dt',   _guardar);
        dt.on('length.dt', _guardar);
    }

    // =========================================================================
    // _autoBindParaTabla  —  auto-bind declarativo por data-cb-*
    // =========================================================================
    /**
     * Busca contenedores [data-cb-controles] cuyo data-cb-tabla coincida con
     * el selector dado y vincula cada botón [data-cb-accion] a la tabla.
     * Mismo patrón que tabulator-cb.js — compatibilidad total de HTML.
     *
     * @param {string} selector  CSS selector de la tabla (ej. "#tblMiTabla")
     * @private
     */
    function _autoBindParaTabla(selector) {
        var registro = _tablas[selector];
        if (!registro) return;

        var dt            = registro.instancia;
        var persistenceId = registro.persistenceId;

        var contenedores = document.querySelectorAll('[data-cb-controles][data-cb-tabla="' + selector + '"]');

        for (var i = 0; i < contenedores.length; i++) {
            var contenedor = contenedores[i];
            var nombreCsv  = contenedor.getAttribute('data-cb-csv') || 'tabla.csv';
            // persistenceId viene del registro interno (_tablas) — fuente única de verdad.
            // El contenedor de controles ya no necesita data-cb-persistencia (estándar 1.4.0).
            var pid        = persistenceId;

            var botones = contenedor.querySelectorAll('[data-cb-accion]');
            for (var j = 0; j < botones.length; j++) {
                _vincularBoton(botones[j], dt, pid, nombreCsv, selector);
            }

            // Estado visual inicial de toggles
            _inicializarToggle(contenedor, 'filtros',          registro.filtrosVisibles);
            _inicializarToggle(contenedor, 'recordar-filtros', registro.recordarFiltros);

            // Botón limpiar: azul cuando hay búsqueda global activa
            var btnLimpiar = contenedor.querySelector('[data-cb-accion="limpiar"]');
            if (btnLimpiar) {
                dt.on('search.dt', function () { _actualizarBtnLimpiar(dt, btnLimpiar); });
                dt.on('draw.dt',   function () { _actualizarBtnLimpiar(dt, btnLimpiar); });
                // REQ-02: activar el botón inmediatamente si hay filtros persistentes
                // al cargar la página (la tabla ya está inicializada en este punto).
                dt.one('init.dt', function () { _actualizarBtnLimpiar(dt, btnLimpiar); });
                // Fallback: si init.dt ya disparó antes del bind, evaluar ahora mismo.
                // DT 3.x: settings.initDone reemplaza a settings._bInitComplete de DT 2.x.
                try {
                    if (dt.settings()[0].initDone) {
                        _actualizarBtnLimpiar(dt, btnLimpiar);
                    }
                } catch (e) { /* ignorar */ }
            }
        }
    }

    /**
     * Vincula un botón individual según su data-cb-accion.
     * @private
     */
    function _vincularBoton(btn, dt, pid, nombreCsv, selector) {
        var accion = btn.getAttribute('data-cb-accion');

        switch (accion) {
            case 'exportar':
                btn.addEventListener('click', function () {
                    // Usar siempre el fallback propio para garantizar CSV limpio:
                    // cabeceras sin "More..." (ColumnControl) y filas sin campos de auditoría.
                    _descargarCsv(dt, nombreCsv);
                });
                break;

            case 'limpiar':
                btn.addEventListener('click', function () {
                    // Limpiar búsqueda global, columnas DT y fixed searches de ColumnControl
                    dt.search('').columns().search('');
                    try {
                        dt.columns().every(function () {
                            try { this.search.fixed('dtcc', ''); } catch (e) { /* ignorar */ }
                        });
                    } catch (e) { /* ignorar si API no disponible */ }
                    dt.draw();
                    _actualizarBtnLimpiar(dt, btn);

                    // Borrar estado persistido para que al recargar la página los
                    // filtros limpiados no se vuelvan a aplicar.
                    try { localStorage.removeItem('cb-dt-state-' + pid); } catch (e) { /* ignorar */ }
                });
                break;

            case 'restablecer':
                btn.addEventListener('click', function () {
                    // Limpiar toda la persistencia de este ID (estado DT,
                    // preferencias UI, orden de columnas y paginación).
                    Object.keys(localStorage)
                        .filter(function (k) {
                            return k === 'cb-dt-state-'    + pid ||
                                   k === 'cb-dt-recordar-' + pid ||
                                   k === 'cb-dt-filtros-'  + pid ||
                                   k === 'cb-dt-colorder-' + pid ||
                                   k === 'cb-dt-paging-'   + pid;
                        })
                        .forEach(function (k) { localStorage.removeItem(k); });
                    location.reload();
                });
                break;

            // NOTA: 'filtros' (toggle ocultar/mostrar filtros de columna) se eliminó
            // en v1.0.7. ColumnControl 1.2.1 ya expone un menú ☰ siempre visible por
            // columna, haciendo redundante el toggle. La clave legacy
            // `cb-dt-filtros-{pid}` sigue siendo limpiada por 'restablecer' por
            // compatibilidad con instalaciones previas.
            // NOTA: 'imprimir' eliminado en v1.2.0 — no se usa en el proyecto.

            case 'colvis':
                (function () {
                    var panelId = 'cb-colvis-' + selector.replace(/[^a-zA-Z0-9]/g, '_');

                    btn.addEventListener('click', function (e) {
                        e.stopPropagation();
                        var panel = document.getElementById(panelId);

                        if (!panel) {
                            panel = _construirColvisPanel(panelId, dt);
                        }

                        if (panel.style.display === 'none' || !panel.style.display) {
                            // Actualizar estados de checkboxes antes de mostrar
                            panel.querySelectorAll('[data-col-idx]').forEach(function (cb) {
                                cb.checked = dt.column(parseInt(cb.getAttribute('data-col-idx'))).visible();
                            });
                            // Posicionar debajo del botón
                            var rect = btn.getBoundingClientRect();
                            panel.style.left    = (window.scrollX + rect.left) + 'px';
                            panel.style.top     = (window.scrollY + rect.bottom + 4) + 'px';
                            panel.style.display = 'block';
                        } else {
                            panel.style.display = 'none';
                        }
                    });

                    // Cerrar al hacer clic fuera del panel
                    document.addEventListener('click', function (e) {
                        var panel = document.getElementById(panelId);
                        if (panel && !panel.contains(e.target) && e.target !== btn) {
                            panel.style.display = 'none';
                        }
                    });
                })();
                break;

            case 'recordar-filtros':
                btn.addEventListener('click', function () {
                    var reg = _tablas[selector];
                    if (!reg) return;
                    reg.recordarFiltros = !reg.recordarFiltros;
                    localStorage.setItem('cb-dt-recordar-' + pid, reg.recordarFiltros);

                    if (!reg.recordarFiltros) {
                        // Desactivar: borrar estado guardado
                        localStorage.removeItem('cb-dt-state-' + pid);
                    } else {
                        // Activar: guardar estado actual de inmediato sin esperar un redibujado.
                        // stateSaveCallback ya verifica recordarFiltros antes de guardar,
                        // por lo que dt.state.save() escribe en localStorage ahora mismo.
                        try { dt.state.save(); } catch (e) { /* ignorar si stateSave no está activo */ }
                    }

                    _actualizarToggle(btn, reg.recordarFiltros);
                });
                break;
        }
    }

    /**
     * Establece el estado visual inicial de un botón toggle (clase active).
     * @private
     */
    function _inicializarToggle(contenedor, accion, activo) {
        var btn = contenedor.querySelector('[data-cb-accion="' + accion + '"]');
        if (!btn) return;
        btn.classList.toggle('active', !!activo);
        btn.setAttribute('aria-pressed', String(!!activo));
    }

    /**
     * Actualiza las clases CSS y aria-pressed de un botón toggle.
     * @private
     */
    function _actualizarToggle(btn, activo) {
        btn.classList.toggle('active', activo);
        btn.setAttribute('aria-pressed', String(activo));
    }

    /**
     * Pone el botón "limpiar" en color primario si hay búsqueda activa.
     * @private
     */
    function _actualizarBtnLimpiar(dt, btn) {
        var activo = false;

        // 1. Búsqueda global DT
        if (dt.search() !== '') { activo = true; }

        if (!activo) {
            // 2. Búsquedas por columna (DT nativo) + filtros ColumnControl (search.fixed 'dtcc')
            dt.columns().every(function () {
                if (this.search() !== '') { activo = true; return false; }
                // ColumnControl registra sus filtros activos como search.fixed con clave 'dtcc'
                try {
                    var sf = this.search.fixed('dtcc');
                    if (sf !== '' && sf != null) { activo = true; return false; }
                } catch (e) { /* API no disponible en esta versión — ignorar */ }
            });
        }

        btn.classList.toggle('cb-filtro-activo', activo);
    }

    /**
     * Construye el panel flotante de visibilidad de columnas (ColVis).
     * Se crea una vez y se reutiliza en aperturas posteriores.
     *
     * @param {string}         panelId  ID único del panel en el DOM
     * @param {DataTables.Api} dt       Instancia de DataTables
     * @returns {HTMLElement}  El elemento panel creado y añadido al body
     * @private
     */
    function _construirColvisPanel(panelId, dt) {
        var panel = document.createElement('div');
        panel.id        = panelId;
        panel.className = 'cb-colvis-panel card shadow border p-2';
        panel.style.cssText = 'position:absolute;z-index:1050;min-width:180px;display:none;max-height:320px;overflow-y:auto;';

        var settings = dt.settings()[0];
        dt.columns().every(function () {
            var idx = this.index();
            // Leer el título desde settings (fuente limpia, sin texto añadido por ColumnControl).
            // DT 3.x: settings.columns[i].title  (equivalente a col.sTitle en DT 2.x).
            // Se quita HTML residual (iconos <i class="fas ..."></i>) con replace de etiquetas.
            var rawTitle = (settings.columns[idx] && settings.columns[idx].title) || '';
            var title    = rawTitle.replace(/<[^>]*>/g, '').replace(/\s+/g, ' ').trim();
            if (!title) return; // omitir columnas sin título (checkbox, acciones sin texto)

            var label = document.createElement('label');
            label.className = 'd-flex align-items-center gap-2 px-2 py-1 rounded cb-colvis-item';
            label.style.cursor = 'pointer';

            var cb = document.createElement('input');
            cb.type      = 'checkbox';
            cb.className = 'form-check-input m-0 flex-shrink-0';
            cb.setAttribute('data-col-idx', String(idx));
            cb.checked   = this.visible();

            // Al cambiar el checkbox, actualizar visibilidad de la columna
            cb.addEventListener('change', function () {
                dt.column(parseInt(this.getAttribute('data-col-idx'))).visible(this.checked);
            });

            var texto = document.createElement('span');
            texto.textContent = title;
            texto.style.fontSize = '0.875rem';

            label.appendChild(cb);
            label.appendChild(texto);
            panel.appendChild(label);
        });

        document.body.appendChild(panel);
        return panel;
    }

    /**
     * Genera y descarga un CSV con los datos visibles de la tabla.
     * Fallback cuando el botón Buttons de DataTables no está disponible.
     * @private
     */
    function _descargarCsv(dt, nombreArchivo) {
        var settings = dt.settings()[0];
        // DT 3.x: settings.aoColumns → settings.columns
        var columnas  = settings.columns;

        // Seleccionar solo columnas con campo de datos reales (excluye Acciones/checkbox con data:null)
        // y que estén visibles. Esto elimina el "More..." del ColumnControl y los campos de auditoría.
        // DT 3.x: col.mData → col.data  |  col.sTitle → col.title
        var colsExportar = [];
        for (var i = 0; i < columnas.length; i++) {
            var col = columnas[i];
            // Omitir columnas sin campo de datos (Acciones, checkbox)
            if (col.data === null || col.data === undefined) continue;
            // Omitir columnas ocultas
            if (!dt.column(i).visible()) continue;
            colsExportar.push({
                idx    : i,
                titulo : (col.title || '').replace(/<[^>]*>/g, '').trim(), // quitar HTML residual
                dataSrc: col.data
            });
        }

        // Cabeceras limpias desde settings.columns[i].title (sin texto del ColumnControl)
        var cabeceras = colsExportar
            .map(function (c) { return '"' + c.titulo.replace(/"/g, '""') + '"'; })
            .join(',');

        // Filas — valores crudos del objeto de datos (sin render HTML, sin campos de auditoría)
        var filas = dt.rows({ search: 'applied' }).data().toArray().map(function (fila) {
            return colsExportar.map(function (c) {
                var val = '';
                if (typeof c.dataSrc === 'string') {
                    val = fila[c.dataSrc];
                } else if (typeof c.dataSrc === 'number') {
                    val = Array.isArray(fila) ? fila[c.dataSrc] : '';
                } else if (typeof c.dataSrc === 'function') {
                    try { val = c.dataSrc(fila, 'sort', '', {}); } catch (e) { val = ''; }
                }
                return '"' + String(val == null ? '' : val).replace(/"/g, '""') + '"';
            }).join(',');
        });

        var contenido = '\uFEFF' + cabeceras + '\n' + filas.join('\n'); // BOM para Excel
        var blob = new Blob([contenido], { type: 'text/csv;charset=utf-8;' });
        var url  = URL.createObjectURL(blob);
        var a    = document.createElement('a');
        a.href = url; a.download = nombreArchivo; a.style.display = 'none';
        document.body.appendChild(a);
        a.click();
        setTimeout(function () { URL.revokeObjectURL(url); a.remove(); }, 1000);
    }

    // =========================================================================
    // _mezclar  —  Object.assign superficial compatible con ES5
    // =========================================================================
    function _mezclar(destino) {
        for (var i = 1; i < arguments.length; i++) {
            var src = arguments[i];
            if (!src) continue;
            for (var k in src) {
                if (Object.prototype.hasOwnProperty.call(src, k)) {
                    destino[k] = src[k];
                }
            }
        }
        return destino;
    }

    // =========================================================================
    // addRow / updateRow / deleteRow  (v1.0.7 — paridad CRUD con CB_TABULATOR)
    // =========================================================================
    /**
     * Inserta un registro en la tabla. Equivalente a `tabla.addRow(registro, alInicio)`
     * de Tabulator.
     *
     * Cuando `alInicio=true` (default), después de insertar navega automáticamente
     * a la página que contiene el nuevo registro (determinada por el sort activo),
     * garantizando que el usuario lo vea sin importar la paginación.
     * `resaltarFila` puede llamarse a continuación para el scroll + animación.
     *
     * @param {DataTables.Api} dt        Instancia de DataTables
     * @param {Object}         registro  Objeto con los datos del nuevo registro
     * @param {boolean}        [alInicio=true]  true → navega a la página del registro;
     *                                          false → agrega sin cambiar la página
     */
    function addRow(dt, registro, alInicio) {
        if (!dt || !registro) return;
        _advertirSinRowId(dt, 'addRow');

        // Agregar la fila y redibujar respetando el sort/filtro activos.
        dt.row.add(registro).draw(false);

        // Cuando alInicio=true (default), intentar navegar a la página del registro.
        // Si el registro queda oculto por filtros activos, _irAPaginaDeRegistro
        // devuelve false y mostramos un toast de aviso al usuario (REQ-01).
        if (alInicio !== false) {
            var visible = _irAPaginaDeRegistro(dt, registro);
            if (!visible) {
                _mostrarToastRegistroOculto(dt, registro);
            }
        }
    }

    /**
     * Navega a la página de DataTables que contiene el registro indicado.
     * Usa rowId para localizar la fila en el display ordenado/filtrado actual.
     *
     * @returns {boolean} true si el registro es visible en el display filtrado;
     *                    false si fue excluido por los filtros activos.
     * @private
     */
    function _irAPaginaDeRegistro(dt, registro) {
        try {
            var settings   = dt.settings()[0];
            var rowIdField = settings && settings.rowId ? settings.rowId : null;
            if (!rowIdField || registro[rowIdField] == null) return true; // sin rowId, asumir visible

            var pk  = registro[rowIdField];
            var row = dt.row('#' + pk);
            if (!row || !row.any()) return false;

            // Índice de la fila dentro del display ordenado/filtrado actual.
            // Si posicion < 0, la fila existe en aiDisplayMaster pero los filtros la ocultan.
            var rowIdx     = row.index();
            var allIndexes = dt.rows({ search: 'applied', order: 'applied' }).indexes().toArray();
            var posicion   = allIndexes.indexOf(rowIdx);
            if (posicion < 0) return false; // oculta por filtros activos

            var pageLen     = dt.page.len();
            var targetPage  = Math.floor(posicion / pageLen);
            var currentPage = dt.page.info().page;

            if (targetPage !== currentPage) {
                dt.page(targetPage).draw(false);
            }
            return true;
        } catch (e) { return true; /* error inesperado — no bloquear */ }
    }

    /**
     * Muestra una notificación warning informando que el registro nuevo existe en la tabla
     * pero está oculto por los filtros activos. Delega en el módulo Notify.
     * Al hacer clic en "Limpiar filtros": limpia filtros, espera el draw, resalta la fila
     * y cierra el toast.
     * @param {DataTables.Api} dt
     * @param {Object}         registro  Datos del registro insertado (para resaltarFila tras limpiar)
     * @private
     */
    function _mostrarToastRegistroOculto(dt, registro) {
        // Buscar el botón limpiar asociado a esta tabla
        var btnLimpiarId = null;
        var selector = null;
        try {
            for (var sel in _tablas) {
                if (_tablas[sel].instancia === dt) { selector = sel; break; }
            }
            if (selector) {
                var btnLimpiar = document.querySelector(
                    '[data-cb-controles][data-cb-tabla="' + selector + '"] [data-cb-accion="limpiar"]'
                );
                if (btnLimpiar) {
                    if (!btnLimpiar.id) {
                        btnLimpiar.id = 'cb-btn-limpiar-' + Date.now();
                    }
                    btnLimpiarId = btnLimpiar.id;
                }
            }
        } catch (e) { /* ignorar */ }

        // Extraer PK del registro para resaltarFila después de limpiar
        var pkValor = null;
        try {
            var settings   = dt.settings()[0];
            var rowIdField = settings && settings.rowId ? settings.rowId : null;
            if (rowIdField && registro) pkValor = registro[rowIdField];
        } catch (e) { /* ignorar */ }

        // Registrar función callback con ID único para que el onclick inline pueda
        // invocarla con acceso a dt, pkValor y el elemento toast.
        var cbKey = '_cbDtLimpiar_' + Date.now();
        window[cbKey] = function (enlaceEl) {
            // 1. Obtener referencia al toast ANTES de limpiar (el DOM aún está intacto)
            var toastEl = enlaceEl ? enlaceEl.closest('.toast') : null;

            // 2. Limpiar filtros — dispara draw.dt sincrónicamente en DataTables
            var btnEl = document.getElementById(btnLimpiarId);
            if (btnEl) btnEl.click();

            // 3. Resaltar fila tras el próximo draw — dt.one garantiza ejecución única
            if (pkValor != null) {
                dt.one('draw.dt', function () {
                    resaltarFila(dt, pkValor);
                });
            }

            // 4. Cerrar toast
            if (toastEl) {
                try {
                    var instancia = bootstrap.Toast.getInstance(toastEl);
                    if (instancia) instancia.hide();
                } catch (e) { /* ignorar */ }
            }

            // 5. Limpiar referencia global
            try { delete window[cbKey]; } catch (e) { /* ignorar */ }
        };

        var accionHtml = btnLimpiarId
            ? ' <a href="#" class="alert-link" onclick="window[\'' + cbKey + '\'](this);return false;">Limpiar filtros</a>'
            : '';

        Notify.warning(
            'El registro fue guardado pero los filtros activos lo ocultan.' + accionHtml,
            { title: 'Registro guardado', html: true }
        );
    }

    /**
     * Actualiza una fila existente localizada por PK. Equivalente a
     * `tabla.updateData([registro])` de Tabulator.
     *
     * Requiere que crearTabla haya recibido `opts.rowId` para que `row('#'+pk)`
     * pueda resolver el registro.
     *
     * @param {DataTables.Api} dt        Instancia de DataTables
     * @param {Object}         registro  Objeto con los datos actualizados
     */
    function updateRow(dt, registro) {
        if (!dt || !registro) return;
        var rowIdField = _advertirSinRowId(dt, 'updateRow');
        if (!rowIdField) return;

        var pk = registro[rowIdField];
        if (pk == null) {
            console.warn('[CB_DATATABLES.updateRow] El registro no contiene el campo PK "' + rowIdField + '".');
            return;
        }

        try {
            var row = dt.row('#' + pk);
            if (row && row.any()) {
                row.data(registro).draw(false);
            } else {
                console.warn('[CB_DATATABLES.updateRow] No se encontró la fila con PK=' + pk);
            }
        } catch (e) {
            console.warn('[CB_DATATABLES.updateRow] Error actualizando fila:', e);
        }
    }

    /**
     * Elimina una fila por PK. Equivalente a `tabla.deleteRow(pkValor)` de Tabulator.
     *
     * @param {DataTables.Api} dt       Instancia de DataTables
     * @param {*}              pkValor  Valor de la PK del registro a eliminar
     */
    function deleteRow(dt, pkValor) {
        if (!dt || pkValor == null) return;
        _advertirSinRowId(dt, 'deleteRow');

        try {
            var row = dt.row('#' + pkValor);
            if (row && row.any()) {
                row.remove().draw(false);
            } else {
                console.warn('[CB_DATATABLES.deleteRow] No se encontró la fila con PK=' + pkValor);
            }
        } catch (e) {
            console.warn('[CB_DATATABLES.deleteRow] Error eliminando fila:', e);
        }
    }

    /**
     * Advierte en consola si la tabla fue creada sin `rowId`, condición necesaria
     * para que `dt.row('#'+pk)` resuelva correctamente. Equivalente al requisito
     * `index` de Tabulator (CrudStandard §6).
     *
     * @param {DataTables.Api} dt      Instancia de DataTables
     * @param {string}         metodo  Nombre de la función llamadora (para el log)
     * @returns {string|null}  El nombre del campo rowId si está configurado; null si no.
     * @private
     */
    function _advertirSinRowId(dt, metodo) {
        try {
            var s = dt.settings()[0];
            var rowIdField = s && s.rowId ? s.rowId : null;
            if (!rowIdField) {
                console.warn(
                    '[CB_DATATABLES.' + metodo + '] La tabla no tiene `rowId` configurado. ' +
                    'Declara `rowId: "IdEntidad"` en las opciones de crearTabla ' +
                    'para habilitar operaciones CRUD por PK. Ver CrudStandard §6.'
                );
            }
            return rowIdField;
        } catch (e) { return null; }
    }

    // ── API pública ────────────────────────────────────────────────────────────
    global.CB_DATATABLES = Object.freeze({
        crearTabla     : crearTabla,
        placeholderHtml: placeholderHtml,
        resaltarFila   : resaltarFila,
        addRow         : addRow,
        updateRow      : updateRow,
        deleteRow      : deleteRow
    });

}(window));

