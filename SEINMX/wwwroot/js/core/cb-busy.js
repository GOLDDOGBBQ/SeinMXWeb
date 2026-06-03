/**
 * CB_BUSY — Estado "ocupado" reutilizable para botones y contenedores.
 *
 * Resuelve el problema de la sensación de "click sin respuesta" cuando una acción
 * asíncrona (guardar, eliminar, procesar, abrir Offcanvas con fetch, etc.) tarda
 * en responder. Centraliza el patrón "deshabilitar + spinner + restaurar" para
 * eliminar duplicación en cada módulo de área.
 *
 * ── API pública (window.CB_BUSY) ──────────────────────────────────────────────
 *
 *   // Botones — patrón básico
 *   CB_BUSY.start(btn, { texto?: 'Guardando...', mantenerAncho?: true });
 *   CB_BUSY.stop(btn);
 *
 *   // Botones — patrón recomendado (try/finally + anti doble-click)
 *   await CB_BUSY.run(btn, async () => { ... }, { texto?: 'Procesando...' });
 *
 *   // Contenedores (Offcanvas, Modal, Card) — overlay con backdrop
 *   CB_BUSY.lock(elemento, { texto?: 'Procesando...', delay?: 120 });
 *   CB_BUSY.unlock(elemento);
 *   await CB_BUSY.runIn(elemento, async () => { ... }, opts);
 *
 *   // Combinado — botón + overlay del contenedor en el mismo try/finally
 *   await CB_BUSY.runWith(btn, contenedor, async () => { ... }, opts);
 *
 * ── Comportamiento ────────────────────────────────────────────────────────────
 *
 *   - Botones: guarda innerHTML/disabled/width originales en `_cbBusy`, fija
 *     `aria-busy="true"`, inserta spinner (icono o icono + texto). Auto-restaura.
 *   - Botones sólo-icono: el spinner reemplaza el icono manteniendo el ancho fijo
 *     para evitar layout shift en filas de tabla.
 *   - Anti doble-click: `run()` retorna inmediatamente si el botón ya está busy.
 *   - Contenedores: overlay absoluto con backdrop semi-transparente + spinner.
 *     Forza `position: relative` (clase `cb-busy-host`) cuando el contenedor es
 *     `static`. Usa `aria-busy` para accesibilidad.
 *   - Anti-parpadeo: el overlay se muestra tras `delay` ms (default 120). Si la
 *     operación termina antes, NUNCA aparece. El botón sí se deshabilita al instante.
 *
 * Sin dependencias externas (sólo Bootstrap CSS para `.spinner-border`).
 * Debe cargarse ANTES de gestor-crud.js y modal-confirmacion.js.
 *
 * Archivo:  wwwroot/js/core/cb-busy.js
 * Versión:  1.0.0  |  Fecha: 2026-04-22
 * Cambios:  1.0.0 — Versión inicial. Botones (start/stop/run) + overlay
 *                   (lock/unlock/runIn) + combinado (runWith).
 */
(function (global) {
    'use strict';

    var DELAY_OVERLAY_MS = 120;
    var KEY_BTN          = '_cbBusy';
    var KEY_HOST         = '_cbBusyHost';

    // ── Helpers ───────────────────────────────────────────────────────────────

    function _resolverEl(ref) {
        if (!ref) return null;
        if (typeof ref === 'string') return document.querySelector(ref);
        return ref;
    }

    function _spinnerHtml(texto) {
        var sp = "<span class='spinner-border spinner-border-sm' role='status' aria-hidden='true'></span>";
        return texto ? sp + " <span class='ms-1'>" + texto + "</span>" : sp;
    }

    // ── BOTONES ───────────────────────────────────────────────────────────────

    /**
     * Marca un botón como "ocupado": guarda estado, inserta spinner, deshabilita.
     * Idempotente — segunda llamada sobre el mismo botón no hace nada.
     *
     * @param {HTMLElement|string} ref
     * @param {object}             [opts]
     * @param {string}             [opts.texto]            — Texto opcional junto al spinner.
     * @param {boolean}            [opts.mantenerAncho=true] — Fija el ancho del botón al original.
     */
    function start(ref, opts) {
        var btn = _resolverEl(ref);
        if (!btn || btn[KEY_BTN]) return;
        opts = opts || {};

        var ancho = opts.mantenerAncho !== false ? btn.getBoundingClientRect().width : null;

        btn[KEY_BTN] = {
            html    : btn.innerHTML,
            disabled: btn.disabled,
            width   : btn.style.width
        };

        if (ancho) btn.style.width = ancho + 'px';
        btn.disabled = true;
        btn.setAttribute('aria-busy', 'true');
        btn.innerHTML = _spinnerHtml(opts.texto);
    }

    /**
     * Restaura un botón al estado previo a `start()`.
     * Idempotente — sin efecto si el botón no estaba marcado.
     *
     * @param {HTMLElement|string} ref
     */
    function stop(ref) {
        var btn = _resolverEl(ref);
        if (!btn || !btn[KEY_BTN]) return;

        var prev = btn[KEY_BTN];
        btn.innerHTML  = prev.html;
        btn.disabled   = prev.disabled;
        btn.style.width = prev.width || '';
        btn.removeAttribute('aria-busy');
        btn[KEY_BTN] = null;
    }

    /**
     * Ejecuta `fn` envuelto en `start`/`stop` con anti doble-click.
     * Retorna el resultado de `fn` (o undefined si el botón ya estaba busy).
     *
     * @param {HTMLElement|string} ref
     * @param {Function}           fn      — async function
     * @param {object}             [opts]
     */
    async function run(ref, fn, opts) {
        var btn = _resolverEl(ref);
        if (btn && btn[KEY_BTN]) return; // anti doble-click

        start(btn, opts);
        try {
            return await fn();
        } finally {
            stop(btn);
        }
    }

    // ── CONTENEDORES (overlay con backdrop) ───────────────────────────────────

    /**
     * Bloquea un contenedor con un overlay semi-transparente + spinner.
     * El overlay aparece tras `delay` ms (default 120) para evitar parpadeo en
     * respuestas rápidas. Idempotente.
     *
     * @param {HTMLElement|string} ref
     * @param {object}             [opts]
     * @param {string}             [opts.texto='Procesando...']
     * @param {number}             [opts.delay=120]  — ms antes de mostrar el overlay.
     * @param {boolean}            [opts.spinner=true]
     */
    function lock(ref, opts) {
        var host = _resolverEl(ref);
        if (!host || host[KEY_HOST]) return;
        opts = opts || {};

        host[KEY_HOST] = { timer: null, overlay: null, posPrev: null };
        host.setAttribute('aria-busy', 'true');

        var delay = (typeof opts.delay === 'number') ? opts.delay : DELAY_OVERLAY_MS;
        var crear = function () {
            // Si el contenedor es static, forzar position relative para anclar el overlay
            var pos = getComputedStyle(host).position;
            if (pos === 'static') {
                host[KEY_HOST].posPrev = host.style.position;
                host.classList.add('cb-busy-host');
            }

            var overlay = document.createElement('div');
            overlay.className = 'cb-busy-overlay';
            overlay.setAttribute('role', 'status');
            overlay.setAttribute('aria-live', 'polite');

            var texto = (opts.texto != null) ? opts.texto : 'Procesando...';
            var spinner = (opts.spinner !== false)
                ? "<div class='spinner-border text-primary' role='status' aria-hidden='true'></div>"
                : '';
            overlay.innerHTML =
                "<div class='cb-busy-overlay__content'>" +
                spinner +
                (texto ? "<div class='cb-busy-overlay__texto small mt-2'>" + texto + "</div>" : '') +
                "</div>";

            host.appendChild(overlay);
            host[KEY_HOST].overlay = overlay;
        };

        if (delay > 0) {
            host[KEY_HOST].timer = setTimeout(crear, delay);
        } else {
            crear();
        }
    }

    /**
     * Quita el overlay y restaura el contenedor. Idempotente.
     * @param {HTMLElement|string} ref
     */
    function unlock(ref) {
        var host = _resolverEl(ref);
        if (!host || !host[KEY_HOST]) return;

        var st = host[KEY_HOST];
        if (st.timer) clearTimeout(st.timer);
        if (st.overlay && st.overlay.parentNode) st.overlay.parentNode.removeChild(st.overlay);
        if (st.posPrev !== null) host.classList.remove('cb-busy-host');

        host.removeAttribute('aria-busy');
        host[KEY_HOST] = null;
    }

    /**
     * Ejecuta `fn` envuelto en `lock`/`unlock`. Retorna el resultado de `fn`.
     */
    async function runIn(ref, fn, opts) {
        var host = _resolverEl(ref);
        lock(host, opts);
        try {
            return await fn();
        } finally {
            unlock(host);
        }
    }

    /**
     * Combinado: bloquea botón + overlay del contenedor en el mismo try/finally.
     * Cualquiera de los dos puede ser null (sólo se aplica al que se proporcione).
     *
     * @param {HTMLElement|string|null} btnRef
     * @param {HTMLElement|string|null} contRef
     * @param {Function}                fn
     * @param {object}                  [opts]   { texto, contenedorTexto, delay }
     */
    async function runWith(btnRef, contRef, fn, opts) {
        opts = opts || {};
        var btn  = _resolverEl(btnRef);
        var cont = _resolverEl(contRef);

        if (btn && btn[KEY_BTN]) return; // anti doble-click

        if (btn)  start(btn, { texto: opts.texto });
        if (cont) lock(cont, { texto: opts.contenedorTexto || opts.texto || 'Procesando...', delay: opts.delay });
        try {
            return await fn();
        } finally {
            if (cont) unlock(cont);
            if (btn)  stop(btn);
        }
    }

    // ── API pública ───────────────────────────────────────────────────────────
    global.CB_BUSY = Object.freeze({
        start  : start,
        stop   : stop,
        run    : run,
        lock   : lock,
        unlock : unlock,
        runIn  : runIn,
        runWith: runWith
    });

}(window));

