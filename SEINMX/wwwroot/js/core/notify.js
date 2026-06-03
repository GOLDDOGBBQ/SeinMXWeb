/**
 * Notify — Módulo unificado de notificaciones para CargoBaja.
 *
 * Reemplaza el sistema anterior de Bootstrap alert fijo en el header y el
 * contenedor de Toasts en bottom-end. Renderiza siempre en una posición
 * fixed relativa al viewport (top-end), por encima de cualquier modal,
 * offcanvas o drawer activo.
 *
 * Características:
 *   - Posición fixed top-end con z-index máximo del proyecto (9999)
 *   - Soporta: success, error, warning, info
 *   - Título opcional por notificación
 *   - Auto-cierre configurable (autohide + delay)
 *   - Cierre manual con botón ×
 *   - Compatible con modo oscuro via Bootstrap 5.3 data-bs-theme
 *   - Creación lazy del contenedor DOM (solo al primer uso)
 *   - Sin dependencias externas fuera de Bootstrap 5 + FontAwesome
 *
 * Dependencias: Bootstrap 5 (Toast), FontAwesome
 * Ubicación: wwwroot/js/core/notify.js
 * Versión: 1.0.0
 * Patrón: IIFE
 *
 * API Pública:
 *   Notify.success(mensaje, opciones?)
 *   Notify.error(mensaje, opciones?)
 *   Notify.warning(mensaje, opciones?)
 *   Notify.info(mensaje, opciones?)
 *   Notify.mostrar(tipo, mensaje, opciones?)
 *
 * Parámetros de opciones (todos opcionales):
 *   { title: string, delay: number, autohide: boolean, html: boolean }
 *
 * Ejemplos:
 *   Notify.success('Registro guardado correctamente');
 *   Notify.error('Error al procesar', { title: 'Error', autohide: false });
 *   Notify.warning('Los cambios no han sido guardados', { delay: 6000 });
 *   Notify.info('Sesión por expirar en 5 minutos');
 *
 * Historial:
 *   1.0.0 (2026-04-17) Versión inicial — reemplaza alert fijo del header + Toast bottom-end.
 */
const Notify = (function () {
    'use strict';

    // ── Constantes ────────────────────────────────────────────────────────────────

    var CONTAINER_ID  = 'cb-notify-container';
    var DEFAULT_DELAY = 8000;

    // Configuración por tipo de notificación
    var TIPOS = {
        success: {
            icono   : 'fa-solid fa-circle-check',
            cabecera: 'text-bg-success'
        },
        error: {
            icono   : 'fa-solid fa-circle-xmark',
            cabecera: 'text-bg-danger'
        },
        warning: {
            icono   : 'fa-solid fa-triangle-exclamation',
            cabecera: 'text-bg-warning'
        },
        info: {
            icono   : 'fa-solid fa-circle-info',
            cabecera: 'text-bg-info'
        }
    };

    // ── DOM ───────────────────────────────────────────────────────────────────────

    /**
     * Obtiene o crea (lazy) el contenedor fixed de notificaciones.
     * Se posiciona en top-end del viewport, por encima de cualquier overlay.
     * @returns {HTMLElement}
     */
    function _obtenerContenedor() {
        var contenedor = document.getElementById(CONTAINER_ID);
        if (contenedor) return contenedor;

        contenedor = document.createElement('div');
        contenedor.id        = CONTAINER_ID;
        contenedor.setAttribute('aria-live', 'polite');
        contenedor.setAttribute('aria-atomic', 'true');
        contenedor.className = 'toast-container position-fixed top-0 end-0 p-3';
        // z-index por encima de Bootstrap Modal (1055), Offcanvas (1045) y Toast anterior (1100)
        contenedor.style.zIndex = '9999';

        document.body.appendChild(contenedor);
        return contenedor;
    }

    // ── Lógica de presentación ────────────────────────────────────────────────────

    /**
     * Crea y muestra un toast de notificación.
     *
     * @param {string} tipo      — 'success' | 'error' | 'warning' | 'info'
     * @param {string} mensaje   — Texto del mensaje (HTML o texto plano según opts.html)
     * @param {Object} [opciones]
     * @param {string}  [opciones.title]    — Título de la cabecera (por defecto: 'CargoBaja')
     * @param {number}  [opciones.delay]    — ms antes de auto-ocultar (default: 8000)
     * @param {boolean} [opciones.autohide] — false para no auto-ocultar (default: true)
     * @param {boolean} [opciones.html]     — true para interpretar mensaje como HTML (default: false)
     */
    function mostrar(tipo, mensaje, opciones) {
        opciones = opciones || {};

        var config = TIPOS[tipo] || TIPOS.info;
        var titulo  = opciones.title    || 'CargoBaja';
        var delay   = opciones.delay    != null ? opciones.delay : DEFAULT_DELAY;
        var autohide= opciones.autohide != null ? opciones.autohide : true;
        var esHtml  = opciones.html     || false;

        var toastEl = document.createElement('div');
        toastEl.className = 'toast align-items-start border-0';
        toastEl.setAttribute('role', 'alert');
        toastEl.setAttribute('aria-live', 'assertive');
        toastEl.setAttribute('aria-atomic', 'true');

        // Cabecera con icono, título y botón cerrar
        var cabecera =
            '<div class="toast-header ' + config.cabecera + '">' +
                '<i class="' + config.icono + ' me-2 flex-shrink-0"></i>' +
                '<strong class="me-auto">' + _escapeHtml(titulo) + '</strong>' +
                '<button type="button" class="btn-close btn-close-white ms-2" ' +
                        'data-bs-dismiss="toast" aria-label="Cerrar"></button>' +
            '</div>';

        // Cuerpo del mensaje
        var cuerpo = '<div class="toast-body">';
        if (esHtml) {
            cuerpo += mensaje;
        } else {
            cuerpo += _escapeHtml(mensaje);
        }
        cuerpo += '</div>';

        toastEl.innerHTML = cabecera + cuerpo;

        var contenedor = _obtenerContenedor();
        contenedor.appendChild(toastEl);

        var toast = new bootstrap.Toast(toastEl, {
            autohide: autohide,
            delay   : delay,
            animation: true
        });
        toast.show();

        // Limpiar DOM al ocultar para no acumular elementos
        toastEl.addEventListener('hidden.bs.toast', function () {
            toastEl.remove();
        });
    }

    /**
     * Escapa caracteres HTML para evitar XSS en mensajes de texto plano.
     * @param {string} texto
     * @returns {string}
     */
    function _escapeHtml(texto) {
        if (typeof texto !== 'string') return String(texto);
        return texto
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    // ── API Pública ───────────────────────────────────────────────────────────────

    return Object.freeze({
        /**
         * Muestra una notificación de éxito (verde).
         * @param {string} mensaje
         * @param {Object} [opciones] — { title, delay, autohide, html }
         */
        success: function (mensaje, opciones) { mostrar('success', mensaje, opciones); },

        /**
         * Muestra una notificación de error (rojo).
         * @param {string} mensaje
         * @param {Object} [opciones] — { title, delay, autohide, html }
         */
        error: function (mensaje, opciones) { mostrar('error', mensaje, opciones); },

        /**
         * Muestra una notificación de advertencia (amarillo).
         * @param {string} mensaje
         * @param {Object} [opciones] — { title, delay, autohide, html }
         */
        warning: function (mensaje, opciones) { mostrar('warning', mensaje, opciones); },

        /**
         * Muestra una notificación informativa (azul/cyan).
         * @param {string} mensaje
         * @param {Object} [opciones] — { title, delay, autohide, html }
         */
        info: function (mensaje, opciones) { mostrar('info', mensaje, opciones); },

        /**
         * API de bajo nivel — permite especificar el tipo explícitamente.
         * Útil cuando el tipo es una variable.
         * @param {string} tipo    — 'success' | 'error' | 'warning' | 'info'
         * @param {string} mensaje
         * @param {Object} [opciones]
         */
        mostrar: mostrar
    });

})();

