/**
 * ModalConfirmacion — Modal Bootstrap 5 de confirmacion de acciones.
 *
 * ── API imperativa (para uso programatico desde JS de modulo) ──────────────────
 *
 *   ModalConfirmacion.mostrar({
 *       titulo:      'Eliminar registro',        // default: 'Confirmar accion'
 *       mensaje:     '¿Eliminar "Nombre"?',
 *       variante:    'danger',                   // Variante Bootstrap — default: 'danger'
 *       etiquetaOk:  'Eliminar',                 // Texto del boton OK — default: 'Confirmar'
 *       onConfirm:   () => { ... }               // Callback al presionar OK
 *   });
 *
 * ── API declarativa (zero JS por modulo — usar clase + data-* en HTML) ─────────
 *
 *   PATRON 1 — Submit de form oculto (POST con redirect, maneja antiforgery):
 *
 *   <button class="js-confirmar"
 *           data-titulo="Eliminar perfil"
 *           data-mensaje="¿Eliminar el perfil &quot;Nombre&quot;?"
 *           data-url="/Area/Entidad/Delete/5"
 *           data-form="id-del-form-oculto">
 *   </button>
 *
 *   PATRON 2 — Fetch AJAX (sin navegacion, actualiza el DOM):
 *
 *   <button class="js-confirmar"
 *           data-titulo="Eliminar item"
 *           data-mensaje="¿Confirmar eliminacion?"
 *           data-url="/Area/Entidad/Delete/5"
 *           data-exito="recargar">   <!-- 'recargar' (default) | 'eliminar-fila' -->
 *   </button>
 *
 * ── Atributos data-* disponibles ──────────────────────────────────────────────
 *
 *   data-titulo        Titulo del modal              (default: 'Confirmar accion')
 *   data-mensaje       Cuerpo del modal              (requerido)
 *   data-variante      Variante Bootstrap del btn OK (default: 'danger')
 *   data-etiqueta-ok   Texto del boton OK            (default: 'Confirmar')
 *   data-url           URL del endpoint a invocar    (requerido)
 *   data-form          ID del <form> oculto a submit (Patron 1)
 *   data-exito         Accion post-fetch exitoso     (Patron 2: 'recargar' | 'eliminar-fila')
 *   data-mensaje-exito Mensaje Notify.success tras exito (default: 'Registro eliminado correctamente.')
 *
 * Requiere: _ModalConfirmacion.cshtml incluido UNA VEZ en la vista (partial).
 * Dependencias: Bootstrap 5, ErrorResponse (solo Patron 2 fetch), Notify
 * Ubicacion: wwwroot/js/core/modal-confirmacion.js
 * Version: 2.2.0
 * Cambios:  2.0.1 — Fix: elMensaje usa innerHTML en lugar de textContent para
 *                   soportar HTML (ej. <strong>, entidades) en el cuerpo del modal.
 *           2.0.2 — Fix: ErrorResponse.manejar solo se invoca cuando !response.ok.
 *                   Antes se llamaba siempre: un 200 OK con body JSON caia en
 *                   "Caso 6: Estado desconocido" y mostraba un ErrorDialog con el
 *                   mensaje de exito como si fuera un error.
 *           2.1.0 — feat: Notify.success tras accion exitosa (Patron 2 fetch).
 *                   Nuevo atributo data-mensaje-exito para personalizar el mensaje.
 *                   Default: 'Registro eliminado correctamente.'
 *           2.2.0 — feat: CB_BUSY.start en Patron 1 (_submitForm) usando el trigger
 *                   original como botón con spinner mientras el POST navega.
 *                   Feedback visual entre OK del modal y recarga de página.
 */
const ModalConfirmacion = (function () {
    'use strict';

    var _onConfirm = null;

    // ── Delegacion para el boton OK del modal ─────────────────────────────────
    document.addEventListener('click', function (e) {
        if (e.target && e.target.id === 'modal-conf-btn-ok') {
            var elModal = document.getElementById('modal-confirmacion');
            if (elModal) bootstrap.Modal.getInstance(elModal)?.hide();
            if (typeof _onConfirm === 'function') {
                var cb = _onConfirm;
                _onConfirm = null;
                cb();
            }
        }
    });

    // ── Delegacion global para elementos con clase js-confirmar ───────────────
    document.addEventListener('click', function (e) {
        var trigger = e.target.closest('.js-confirmar');
        if (!trigger) return;
        e.preventDefault();

        mostrar({
            titulo:        trigger.dataset.titulo      || 'Confirmar accion',
            mensaje:       trigger.dataset.mensaje     || '',
            variante:      trigger.dataset.variante    || 'danger',
            etiquetaOk:    trigger.dataset.etiquetaOk  || 'Confirmar',
            mensajeExito:  trigger.dataset.mensajeExito || 'Registro eliminado correctamente.',
            onConfirm:  trigger.dataset.form
                ? function () { _submitForm(trigger.dataset.form, trigger.dataset.url); }
                : function () { _fetchConfirmado(trigger.dataset.url, trigger); }
        });
    });

    // ── Patron 1: submit de form oculto ───────────────────────────────────────
    function _submitForm(formId, url) {
        var form = document.getElementById(formId);
        if (!form) { console.error('ModalConfirmacion: form no encontrado: ' + formId); return; }
        form.action = url;
        form.submit();
    }

    // ── Patron 2: fetch AJAX ──────────────────────────────────────────────────
    async function _fetchConfirmado(url, trigger) {
        var token = (document.querySelector('[name="__RequestVerificationToken"]') || {}).value || '';

        // Bloquear el boton trigger (de la fila o ActionBar) con spinner +
        // anti doble-click. El modal ya se cerro en el click handler del OK,
        // por lo que el feedback visual debe vivir en el boton que origino la accion.
        var ejecutar = async function () {
            var response = await fetch(url, {
                method: 'POST',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': token
                }
            });

            if (!response.ok) {
                if (typeof ErrorResponse !== 'undefined') {
                    await ErrorResponse.manejar(response, { mostrarErrorDialog: true });
                }
                return;
            }

            var exito        = trigger.dataset.exito || 'recargar';
            var mensajeExito = trigger.dataset.mensajeExito || 'Registro eliminado correctamente.';

            if (typeof Notify !== 'undefined') {
                Notify.success(mensajeExito);
            }

            if (exito === 'eliminar-fila') {
                trigger.closest('tr')?.remove();
            } else {
                window.location.reload();
            }
        };

        if (typeof CB_BUSY !== 'undefined' && trigger) {
            await CB_BUSY.run(trigger, ejecutar);
        } else {
            await ejecutar();
        }
    }

    /**
     * Muestra el modal de confirmacion con el contenido indicado.
     * @param {object}   opciones
     * @param {string}   [opciones.titulo='Confirmar accion']               Titulo del modal
     * @param {string}   [opciones.mensaje='']                              Cuerpo del modal
     * @param {string}   [opciones.variante='danger']                       Variante Bootstrap del btn OK
     * @param {string}   [opciones.etiquetaOk='Confirmar']                  Texto del boton OK
     * @param {string}   [opciones.mensajeExito='Registro eliminado...']    Mensaje Notify.success tras exito (Patron 2)
     * @param {Function}  opciones.onConfirm                                Callback al confirmar
     */
    function mostrar({ titulo = 'Confirmar accion', mensaje = '', variante = 'danger', etiquetaOk = 'Confirmar', mensajeExito, onConfirm } = {}) {
        var elModal   = document.getElementById('modal-confirmacion');
        if (!elModal) return;

        var elTitulo  = document.getElementById('modal-conf-titulo');
        var elMensaje = document.getElementById('modal-conf-mensaje');
        var elBtnOk   = document.getElementById('modal-conf-btn-ok');

        if (elTitulo)  elTitulo.textContent = titulo;
        if (elMensaje) elMensaje.innerHTML  = mensaje; // innerHTML: permite HTML en mensaje (ej. <strong>)
        if (elBtnOk) {
            elBtnOk.className   = 'btn btn-sm btn-' + variante;
            elBtnOk.textContent = etiquetaOk;
        }

        _onConfirm = onConfirm || null;

        bootstrap.Modal.getOrCreateInstance(elModal).show();
    }

    return { mostrar };
})();
