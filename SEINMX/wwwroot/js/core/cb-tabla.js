/**
 * cb-tabla.js  —  Fachada unificada para Tabulator y DataTables
 *
 * Proporciona una API agnóstica del motor para las operaciones CRUD
 * frecuentes desde módulos de vista (onExito de GestorCrud, handlers de
 * botones, etc.). Detecta el motor por duck-typing y delega a
 * CB_TABULATOR o CB_DATATABLES según corresponda.
 *
 * Regla de uso (ver Docs/Standards/ViewsStandard.md §9.5):
 *   - Tabulator  → tablas con estructura de árbol (treeview).
 *   - DataTables → todas las demás tablas planas.
 *
 * API pública (window.CB_TABLA):
 *   esDataTables(tabla)                   → boolean
 *   esTabulator(tabla)                    → boolean
 *   addRow(tabla, registro, alInicio)     → Promise|void
 *   updateRow(tabla, registro)            → Promise|void
 *   deleteRow(tabla, pkValor)             → Promise|void
 *   resaltarFila(tabla, pkValor)          → void   (delega a GestorCrud.resaltarFila)
 *   getSelectedData(tabla)                → Array
 *   clearFilters(tabla)                   → void
 *   redraw(tabla)                         → void
 *
 * Contrato de intercambiabilidad:
 *   Si un módulo usa exclusivamente CB_TABLA + data-cb-* (partial _TablaControles)
 *   + ModalConfirmacion declarativo, cambiar el motor de la tabla NO requiere
 *   tocar el JS del módulo — solo la inicialización (CB_TABULATOR.crearTabla o
 *   CB_DATATABLES.crearTabla) y la carga de scripts/estilos de la vista.
 *
 * Dependencias:
 *   - CB_TABULATOR  (wwwroot/js/core/tabulator-cb.js)  — cuando la tabla es Tabulator
 *   - CB_DATATABLES (wwwroot/js/core/datatables-cb.js) — cuando la tabla es DataTables
 *   - GestorCrud    (wwwroot/js/core/gestor-crud.js)   — para resaltarFila
 *
 * Ubicación: wwwroot/js/core/cb-tabla.js
 * Versión:   1.0.0  |  Fecha: 2026-04-17
 */
(function (global) {
    'use strict';

    // ── Duck-typing de detección de motor ─────────────────────────────────────
    function esTabulator(tabla) {
        return !!(tabla &&
                  typeof tabla.getRow      === 'function' &&
                  typeof tabla.scrollToRow === 'function');
    }
    function esDataTables(tabla) {
        return !!(tabla &&
                  !esTabulator(tabla) &&
                  typeof tabla.row      === 'function' &&
                  typeof tabla.draw     === 'function' &&
                  typeof tabla.settings === 'function');
    }

    // ── addRow ────────────────────────────────────────────────────────────────
    function addRow(tabla, registro, alInicio) {
        if (!tabla || !registro) return;
        if (esTabulator(tabla)) {
            return tabla.addRow(registro, alInicio !== false);
        }
        if (esDataTables(tabla) && global.CB_DATATABLES) {
            global.CB_DATATABLES.addRow(tabla, registro, alInicio !== false);
        }
    }

    // ── updateRow ─────────────────────────────────────────────────────────────
    function updateRow(tabla, registro) {
        if (!tabla || !registro) return;
        if (esTabulator(tabla)) {
            return tabla.updateData([registro]);
        }
        if (esDataTables(tabla) && global.CB_DATATABLES) {
            global.CB_DATATABLES.updateRow(tabla, registro);
        }
    }

    // ── deleteRow ─────────────────────────────────────────────────────────────
    function deleteRow(tabla, pkValor) {
        if (!tabla || pkValor == null) return;
        if (esTabulator(tabla)) {
            return tabla.deleteRow(pkValor);
        }
        if (esDataTables(tabla) && global.CB_DATATABLES) {
            global.CB_DATATABLES.deleteRow(tabla, pkValor);
        }
    }

    // ── resaltarFila ──────────────────────────────────────────────────────────
    function resaltarFila(tabla, pkValor) {
        if (global.GestorCrud && typeof global.GestorCrud.resaltarFila === 'function') {
            global.GestorCrud.resaltarFila(tabla, pkValor);
            return;
        }
        if (esDataTables(tabla) && global.CB_DATATABLES) {
            global.CB_DATATABLES.resaltarFila(tabla, pkValor);
        }
    }

    // ── getSelectedData ───────────────────────────────────────────────────────
    function getSelectedData(tabla) {
        if (!tabla) return [];
        if (esTabulator(tabla)) {
            return tabla.getSelectedData();
        }
        if (esDataTables(tabla)) {
            try { return tabla.rows({ selected: true }).data().toArray(); }
            catch (e) { return []; }
        }
        return [];
    }

    // ── clearFilters ──────────────────────────────────────────────────────────
    function clearFilters(tabla) {
        if (!tabla) return;
        if (esTabulator(tabla)) {
            try { tabla.clearHeaderFilter(); } catch (e) { /* sin filter */ }
            try { tabla.clearFilter(true);   } catch (e) { /* sin filter */ }
            return;
        }
        if (esDataTables(tabla)) {
            try { tabla.search('').columns().search('').draw(); } catch (e) { /* ignorar */ }
        }
    }

    // ── redraw ────────────────────────────────────────────────────────────────
    function redraw(tabla) {
        if (!tabla) return;
        if (esTabulator(tabla)) {
            try { tabla.redraw(true); } catch (e) { /* ignorar */ }
            return;
        }
        if (esDataTables(tabla)) {
            try { tabla.draw(false); } catch (e) { /* ignorar */ }
        }
    }

    // ── API pública ───────────────────────────────────────────────────────────
    global.CB_TABLA = Object.freeze({
        esTabulator    : esTabulator,
        esDataTables   : esDataTables,
        addRow         : addRow,
        updateRow      : updateRow,
        deleteRow      : deleteRow,
        resaltarFila   : resaltarFila,
        getSelectedData: getSelectedData,
        clearFilters   : clearFilters,
        redraw         : redraw
    });

}(window));

