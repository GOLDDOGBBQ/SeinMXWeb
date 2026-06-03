/**
 * dt-common.js — Inicialización estándar de DataTables para SEINMX
 * Incluye: ColReorder, filtros por columna, botones (ColVis / Limpiar / Resetear), stateSave
 */

/* ── Inyectar estilos para la fila de filtros (sobreescribe table-dark del thead) ── */
(function () {
    if (document.getElementById('dt-common-styles')) return;
    const s = document.createElement('style');
    s.id = 'dt-common-styles';
    s.textContent = `
        /* ── Iconos de orden: invertir color para que sean visibles en headers oscuros ── */
        .table-dark thead th .dt-column-order {
            filter: invert(1) brightness(2);
        }
        /* Fallback con variables CSS de DT */
        .table-dark thead th {
            --dt-order-arrow_color:         rgb(255, 255, 255);
            --dt-order-arrow_color-current: rgb(255, 255, 255);
            --dt-order-arrow_opacity:         0.6;
            --dt-order-arrow_opacity-current: 1;
            --dt-order-header_outline-hover:  2px solid rgba(255,255,255,0.4);
            --dt-order-icon-color: rgba(255,255,255,0.7);
        }
        .table-dark thead th.dt-ordering-asc .dt-column-order::before,
        .table-dark thead th.dt-ordering-desc .dt-column-order::after,
        .table-dark thead th.dt-orderable-asc .dt-column-order::before,
        .table-dark thead th.dt-orderable-desc .dt-column-order::after {
            opacity: 1 !important;
            color: #fff !important;
        }

        /* ── Fila de filtros: fondo claro aunque esté dentro de table-dark ── */
        thead tr.dt-filter-row th {
            background-color: #f1f3f5 !important;
            color: #212529     !important;
            padding: 4px 6px   !important;
        }
        thead tr.dt-filter-row input.form-control {
            background-color: #ffffff !important;
            color: #212529            !important;
            border-color: #ced4da    !important;
        }
        thead tr.dt-filter-row input.form-control::placeholder {
            color: #6c757d !important;
        }
    `;
    document.head.appendChild(s);
}());

const DT_LANG_ES = {
    decimal:           ",",
    emptyTable:        "No hay datos disponibles",
    info:              "Mostrando _START_ a _END_ de _TOTAL_ registros",
    infoEmpty:         "Mostrando 0 a 0 de 0 registros",
    infoFiltered:      "(filtrado de _MAX_ registros totales)",
    infoPostFix:       "",
    thousands:         ".",
    lengthMenu:        "Mostrar _MENU_ registros",
    loadingRecords:    "Cargando...",
    processing:        "Procesando...",
    search:            "",
    searchPlaceholder: "Buscar en tabla...",
    zeroRecords:       "No se encontraron resultados",
    paginate: {
        first:    "Primero",
        last:     "Último",
        next:     "Siguiente",
        previous: "Anterior"
    },
    aria: {
        sortAscending:  ": activar para ordenar ascendente",
        sortDescending: ": activar para ordenar descendente"
    }
};

/**
 * Limpia todos los filtros de una tabla.
 * @param {string}          tableSelector  Selector CSS de la tabla
 * @param {DataTables.Api}  dt             Instancia DataTables
 */
function dtLimpiarFiltros(tableSelector, dt) {
    // Limpiar búsqueda global y por columna
    dt.search('').columns().search('').draw();

    // Limpiar los inputs del header de filtros
    $(tableSelector)
        .find('thead tr.dt-filter-row input')
        .val('');
}

/**
 * Inicializa una tabla con DataTables con todas las funciones estándar.
 *
 * @param {string} tableSelector  Selector CSS (ej: '#miTabla')
 * @param {object} overrides      Opciones extra: order, columnDefs, etc.
 * @returns {DataTables.Api}
 */
function initDataTable(tableSelector, overrides) {
    overrides = overrides || {};

    const dt = $(tableSelector).DataTable({
        pageLength: overrides.pageLength || 25,
        lengthMenu: overrides.lengthMenu || [10, 25, 50, 100],
        order:      overrides.order      || [[0, 'asc']],
        columnDefs: overrides.columnDefs || [],
        colReorder:    true,
        stateSave:     true,
        stateDuration: 0,             // 0 = localStorage (persiste entre sesiones)
        language:   DT_LANG_ES,

        /* ── Guardar estado incluyendo orden de columnas (ColReorder) ── */
        stateSaveCallback: function (settings, data) {
            try {
                const api = settings.api;
                if (api && api.colReorder) {
                    data.colReorderOrder = api.colReorder.order();
                }
            } catch (e) {}
            try {
                localStorage.setItem(
                    'DataTables_' + settings.unique + '_' + location.pathname,
                    JSON.stringify(data)
                );
            } catch (e) {}
        },

        /* ── Cargar estado desde localStorage ── */
        stateLoadCallback: function (settings) {
            try {
                const raw = localStorage.getItem(
                    'DataTables_' + settings.unique + '_' + location.pathname
                );
                return raw ? JSON.parse(raw) : null;
            } catch (e) { return null; }
        },

        /* ── Restaurar orden de columnas tras cargar el estado ── */
        stateLoaded: function (settings, data) {
            if (!data || !data.colReorderOrder) return;
            setTimeout(function () {
                try {
                    settings.api.colReorder.order(data.colReorderOrder);
                    // Reubicar también los inputs del filtro tras el reorder restaurado
                    settings.api.trigger('colReorder');
                } catch (e) {}
            }, 50);
        },

        /* ── Layout: fila superior con pageLength + búsqueda; fila extra con botones ── */
        layout: {
            topStart:    'pageLength',
            topEnd:      'search',
            top2:        'buttons',
            bottomStart: 'info',
            bottomEnd:   'paging'
        },

        /* ── Botones ── */
        buttons: [
            {
                extend:    'colvis',
                text:      '<i class="fas fa-eye me-1"></i>Columnas',
                className: 'btn btn-sm btn-secondary me-1'
            },
            {
                text:      '<i class="fas fa-eraser me-1"></i>Limpiar filtros',
                className: 'btn btn-sm btn-secondary me-1',
                action: function (e, dt) {
                    dtLimpiarFiltros(tableSelector, dt);
                }
            },
            {
                text:      '<i class="fas fa-rotate-left me-1"></i>Resetear tabla',
                className: 'btn btn-sm btn-warning',
                action: function (e, dt) {
                    dtLimpiarFiltros(tableSelector, dt);
                    dt.page.len(25);
                    dt.columns().visible(true);     // restaurar visibilidad de columnas
                    try { dt.colReorder.reset(); } catch (ignored) {}
                    dt.draw();
                }
            }
        ],

        /* ── Filtros por columna en el header ── */
        initComplete: function () {
            const api       = this.api();
            const thead     = $(api.table().header());
            const filterRow = $('<tr class="dt-filter-row"></tr>');

            thead.find('tr:first th').each(function (domIdx) {

                const th      = $('<th></th>');
                const attrCol = $(this).attr('data-dt-column');
                const colIdx  = (attrCol !== undefined && attrCol !== '')
                    ? parseInt(attrCol.split(',')[0], 10)
                    : domIdx;

                // Verificar si la columna es ordenable (acceso defensivo)
                let isOrderable = true;
                try {
                    const settings = api.settings();
                    const s0       = settings && settings[0];
                    if (s0 && s0.aoColumns && s0.aoColumns[colIdx] !== undefined) {
                        isOrderable = s0.aoColumns[colIdx].orderable !== false;
                    }
                } catch (e) {
                    isOrderable = true;
                }

                if (isOrderable) {
                    let col;
                    try { col = api.column(colIdx); } catch (e) { col = null; }
                    if (col) {
                        const currentSearch = col.search();

                        $('<input>', {
                            type:        'text',
                            'class':     'form-control form-control-sm',
                            placeholder: 'Filtrar…',
                            'data-col':  colIdx,
                            value:       currentSearch
                        })
                        .appendTo(th)
                        .on('input', function () {
                            const val = this.value;
                            if (col.search() !== val) {
                                col.search(val).draw();
                            }
                        });
                    }
                }

                filterRow.append(th);
            });

            thead.append(filterRow);

            /* Sincronizar filtros del header cuando ColReorder mueve columnas */
            api.on('colReorder', function () {
                const cells = filterRow.children('th').detach().get();

                thead.find('tr:first th').each(function () {
                    const attrCol = $(this).attr('data-dt-column');
                    const colIdx  = (attrCol !== undefined && attrCol !== '')
                        ? parseInt(attrCol.split(',')[0], 10)
                        : 0;

                    const match = cells.find(function (td) {
                        const inp = $(td).find('input');
                        return inp.length
                            ? parseInt(inp.attr('data-col'), 10) === colIdx
                            : !$(td).find('input').length && colIdx === -1;
                    });

                    filterRow.append(match ? match : $('<th></th>'));
                });

                thead.append(filterRow);
            });
        }
    });

    return dt;
}

