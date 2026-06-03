/**
 * GestorCrud — Patron estandar guardar + actualizar tabla para modulos CRUD Offcanvas.
 *
 * Encapsula el ciclo completo de una operacion POST desde un Offcanvas:
 *   1. Recolecta datos del formulario (FormStateManager.obtenerDatosGrabar)
 *   2. Envia el POST con fetch
 *   3. Si el guardado falla → ErrorResponse.manejar() muestra el error al usuario
 *   4. Si el guardado es exitoso → llama onExito(resultado) en try/catch
 *   5. Si onExito falla (error de red al recargar, etc.)
 *        → muestra toast de "guardado OK pero no se pudo recargar" (exitoConAdvertenciaRecarga)
 *   6. Si onExito pasa pero resultado.MensajeWarning no es vacio
 *        → muestra toast warning del servidor (guardado OK, evento no critico en la API,
 *           p.ej. no se pudo enviar el correo de notificacion)
 *
 * Uso en modulos (patron completo con posicion configurable y resaltado):
 *
 *   var ok = await GestorCrud.guardarYActualizar({
 *       url:     CFG.urls.nuevoPerfil,
 *       formId:  'form-registro',
 *       token:   '...',
 *       modulo:  'Sistema',
 *       control: 'btn-guardar',
 *       onExito: async function (resultado) {
 *           if (resultado.Registro) {
 *               var pk = resultado.Registro.IdEntidad;
 *               if (modo === 'nuevo') {
 *                   // CrudStandard §6: default = al inicio (true).
 *                   // Usar false solo cuando la funcionalidad lo requiera.
 *                   await tabla.addRow(resultado.Registro, true);
 *               } else {
 *                   await tabla.updateData([resultado.Registro]);
 *               }
 *               // Scroll + highlight verde animado — obligatorio tras addRow/updateData
 *               GestorCrud.resaltarFila(tabla, pk);
 *           } else {
 *               await _actualizarTablaCompleta(); // FALLBACK — no debe ocurrir en op. normal
 *           }
 *           // Limpiar UI / cerrar Offcanvas aqui
 *       }
 *   });
 *   if (!ok) return; // El guardado falló — ErrorResponse ya mostró el error
 *
 * Separacion de responsabilidades:
 *   - Error de guardado    → ErrorResponse.manejar() → modal/formulario de error
 *   - Exito + error recarga→ GestorCrud.mostrarExitoConAdvertenciaRecarga() → toast warning
 *   - Warning del servidor → GestorCrud detecta MensajeWarning → toast warning 10 s
 *   - Exito total          → silencioso (la tabla fue actualizada + fila resaltada)
 *
 * Dependencias: FormStateManager, ErrorResponse, ErrorDialog (todos cargados por _Layout)
 * Ubicacion: wwwroot/js/core/gestor-crud.js
 * Version: 1.5.0
 * Cambios: 1.5.0 — FIX estético CB_BUSY en guardarYActualizar: eliminado `texto` del
 *                  botón en runWith. Solo spinner en el botón (mantenerAncho:true por
 *                  defecto); el texto "Guardando..." se conserva SOLO en el overlay del
 *                  contenedor (contenedorTexto). Pasar texto al botón inyecta
 *                  spinner+text que puede ser más ancho que el contenido original
 *                  ("Guardar" → "Guardando..."), causando layout shift en el Offcanvas.
 *          1.4.0 — resaltarFila ahora soporta Tabulator Y DataTables (CB_DATATABLES).
 *                  Detecta automaticamente el tipo de instancia con duck-typing:
 *                  Tabulator → tabla.getRow() + tabla.scrollToRow() (API existente).
 *                  DataTables → CB_DATATABLES.resaltarFila() delegado al modulo.
 *          1.3.0 — Nuevo metodo publico resaltarFila(tabla, pkValor).
 *                  Scroll al registro insertado/actualizado + animacion verde 2.5 s
 *                  (clase CSS cb-row-guardado definida en tabulator-cb.css v1.5.0).
 *                  CrudStandard §6: addRow con true (inicio) por defecto.
 *          1.2.0 — Paso 6: manejo de resultado.MensajeWarning (toast 10 s).
 *          1.1.0 — Documentacion: addRow/updateData por PK (CrudStandard §6 v2.2.0).
 *          1.0.0 — Version inicial.
 */
const GestorCrud = (function () {
    'use strict';

    // Duracion de la animacion CSS cb-row-guardado (debe coincidir con tabulator-cb.css)
    var ROW_HIGHLIGHT_MS = 2500;

    // ── Helpers privados ─────────────────────────────────────────────────────────


    // ── API publica ───────────────────────────────────────────────────────────────

    /**
     * Ciclo completo de guardado + actualizacion para formularios Offcanvas.
     *
     * @param {Object}   config
     * @param {string}   config.url       — URL del endpoint POST
     * @param {string}   config.formId    — ID del <form> (usado por FormStateManager)
     * @param {string}   config.token     — Token antiforgery (RequestVerificationToken)
     * @param {Function} config.onExito   — async (resultado) => void — ejecuta la recarga optimista
     * @param {string}   [config.modulo]  — Nombre del modulo para ErrorResponse / ErrorDialog
     * @param {string}   [config.control] — Nombre del control para ErrorResponse / ErrorDialog
     *
     * @returns {Promise<boolean>} true si el guardado fue exitoso (independientemente de la recarga),
     *                             false si el guardado fallo.
     */
    /**
     * Ciclo completo de guardado + actualizacion para formularios Offcanvas.
     *
     * @param {Object}   config
     * @param {string}   config.url       — URL del endpoint POST
     * @param {string}   config.formId    — ID del <form> (usado por FormStateManager)
     * @param {string}   config.token     — Token antiforgery (RequestVerificationToken)
     * @param {Function} config.onExito   — async (resultado) => void — ejecuta la recarga optimista
     * @param {string}   [config.modulo]  — Nombre del modulo para ErrorResponse / ErrorDialog
     * @param {string}   [config.control] — Nombre del control para ErrorResponse / ErrorDialog
     * @param {HTMLElement|string} [config.btnSubmit]   — Boton Guardar (selector o elemento).
     *                                                    Si se omite, se intenta autodetectar via #{config.control}.
     *                                                    Pasa por CB_BUSY.start/stop (spinner + anti doble-click).
     * @param {HTMLElement|string} [config.contenedor]  — Contenedor a bloquear con overlay
     *                                                    (offcanvas-body, modal-content, card-body).
     * @param {string}             [config.textoBusy='Guardando...'] — Texto bajo el spinner del overlay.
     *
     * @returns {Promise<boolean>} true si el guardado fue exitoso (independientemente de la recarga),
     *                             false si el guardado fallo o si el boton ya estaba busy.
     */
    async function guardarYActualizar(config) {
        var formId  = config.formId;
        var modulo  = config.modulo  || 'General';
        var control = config.control || 'btn-guardar';

        // ── Resolver botón submit (explícito o por convención #{control}) ─────
        var btn = null;
        if (typeof CB_BUSY !== 'undefined') {
            btn = config.btnSubmit
                ? (typeof config.btnSubmit === 'string' ? document.querySelector(config.btnSubmit) : config.btnSubmit)
                : document.getElementById(control);
            // Anti doble-click: si el botón ya está procesando, abortar.
            if (btn && btn.getAttribute('aria-busy') === 'true') return false;
        }

        // ── Wrapper CB_BUSY (botón + overlay del contenedor) ──────────────────
        var resultadoFinal = false;
        var ejecutar = async function () {
            resultadoFinal = await _ejecutarCiclo(config, formId, modulo, control);
        };

        if (typeof CB_BUSY !== 'undefined' && (btn || config.contenedor)) {
            await CB_BUSY.runWith(btn, config.contenedor || null, ejecutar, {
                // `texto` OMITIDO en el botón — solo spinner, sin cambio de tamaño.
                // El texto "Guardando..." se muestra únicamente en el overlay del contenedor.
                contenedorTexto : config.textoBusy || 'Guardando...'
            });
        } else {
            await ejecutar();
        }

        return resultadoFinal;
    }

    /** Ciclo interno de guardado (fetch + manejo de errores + onExito + warning). */
    async function _ejecutarCiclo(config, formId, modulo, control) {
        // ── 1. Recolectar datos del formulario ─────────────────────────────────
        var datos = FormStateManager.obtenerDatosGrabar(formId);

        // ── 2. Enviar POST ─────────────────────────────────────────────────────
        var response;
        try {
            response = await fetch(config.url, {
                method : 'POST',
                headers: {
                    'Content-Type'            : 'application/x-www-form-urlencoded',
                    'X-Requested-With'        : 'XMLHttpRequest',
                    'RequestVerificationToken': config.token
                },
                body: new URLSearchParams(datos)
            });
        } catch (err) {
            // Error de red antes de llegar al servidor
            ErrorDialog.manejarYMostrar(err, { modulo: modulo, control: control });
            return false;
        }

        // ── 3. Manejar error de guardado ───────────────────────────────────────
        if (!response.ok) {
            await ErrorResponse.manejar(response, {
                mostrarErrorDialog : response.status !== 400,
                mostrarEnFormulario: true,
                formularioId       : formId,
                modulo             : modulo,
                control            : control
            });
            return false;
        }

        // ── 4. Guardado exitoso — parsear respuesta ────────────────────────────
        var resultado = {};
        try { resultado = await response.json(); } catch { /* sin body JSON — es valido */ }

        // ── 5. Ejecutar actualizacion optimista de la tabla ────────────────────
        if (typeof config.onExito === 'function') {
            try {
                await config.onExito(resultado);
            } catch (err) {
                console.warn('[GestorCrud] Guardado exitoso pero error en actualizacion de tabla:', err);
                mostrarExitoConAdvertenciaRecarga();
                return true;
            }

            // ── 6. Warning del servidor (guardado OK, evento no critico en API) ──────
            if (resultado.MensajeWarning) {
                Notify.warning(resultado.MensajeWarning, { delay: 10000 });
            }
        }

        return true;
    }

    /**
     * Muestra un toast de advertencia cuando el guardado fue exitoso
     * pero la actualizacion de la tabla fallo (error de red, timeout, etc.).
     *
     * El mensaje indica al usuario que refresque la pagina manualmente.
     * No interrumpe el flujo — es solo una notificacion no bloqueante.
     */
    function mostrarExitoConAdvertenciaRecarga() {
        Notify.warning(
            'El registro fue guardado correctamente. ' +
            'No fue posible recargar los datos. Actualice la página manualmente.',
            { delay: 10000 }
        );
    }

    /**
     * Desplaza la tabla al registro insertado o actualizado y aplica una animacion
     * verde de 2.5 s para que el usuario lo identifique visualmente.
     *
     * Soporta Tabulator y DataTables (CB_DATATABLES) — detecta el tipo automaticamente:
     *   - Tabulator : usa tabla.getRow() + tabla.scrollToRow() (API nativa Tabulator).
     *   - DataTables: delega a CB_DATATABLES.resaltarFila() si el modulo esta disponible.
     *
     * Debe llamarse DESPUES de tabla.addRow()/tabla.updateData() (Tabulator) o
     * tabla.row.add()/tabla.row().data() (DataTables) en onExito, siempre que se
     * conozca el valor de la PK del registro afectado.
     *
     * La animacion usa la clase CSS .cb-row-guardado (tabulator-cb.css / datatables-cb.css).
     * Compatible con modo claro y oscuro.
     *
     * @param {object} tabla    — Instancia Tabulator o DataTables del modulo
     * @param {*}      pkValor  — Valor de la PK del registro
     *
     * Ejemplos:
     *   // Tabulator
     *   await tabla.addRow(resultado.Registro, true);
     *   GestorCrud.resaltarFila(tabla, resultado.Registro.IdEntidad);
     *
     *   // DataTables
     *   tabla.row.add(resultado.Registro).draw(false);
     *   GestorCrud.resaltarFila(tabla, resultado.Registro.IdEntidad);
     */
    function resaltarFila(tabla, pkValor) {
        if (!tabla || pkValor == null) return;

        // ── Deteccion de libreria (duck-typing) ───────────────────────────────
        // Tabulator expone getRow() + scrollToRow(); DataTables expone row() + draw().
        var esTabulator  = typeof tabla.getRow === 'function' && typeof tabla.scrollToRow === 'function';
        var esDataTables = !esTabulator && typeof tabla.row === 'function' && typeof tabla.draw === 'function';

        if (esTabulator) {
            // ── Tabulator ─────────────────────────────────────────────────────
            var row;
            try { row = tabla.getRow(pkValor); } catch (e) { return; }
            if (!row) return;

            tabla.scrollToRow(row, 'center', false).catch(function () { /* paginacion u otro error */ });

            var el = row.getElement();
            el.classList.remove('cb-row-guardado');
            void el.offsetWidth; // reflow para reiniciar animacion
            el.classList.add('cb-row-guardado');
            setTimeout(function () { el.classList.remove('cb-row-guardado'); }, ROW_HIGHLIGHT_MS + 150);

        } else if (esDataTables) {
            // ── DataTables — delegar a CB_DATATABLES si esta disponible ──────
            if (window.CB_DATATABLES && typeof window.CB_DATATABLES.resaltarFila === 'function') {
                window.CB_DATATABLES.resaltarFila(tabla, pkValor);
            }
        }
    }


    // ── Exportar API publica (inmutable) ──────────────────────────────────────────
    return Object.freeze({
        guardarYActualizar,
        mostrarExitoConAdvertenciaRecarga,
        resaltarFila
    });

})();
