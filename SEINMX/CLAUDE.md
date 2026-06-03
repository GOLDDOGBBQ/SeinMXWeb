# Instrucciones para CLAUDE (SEINMX - Frontend ASP.NET Core MVC) - Estándar de Programación

Estas instrucciones aplican a **TODO** el código del proyecto **SEINMX** (Controllers, Views, ViewModels, HttpClients,
configuración).
CLAUDE debe generar y modificar código siguiendo estrictamente este estándar **y** gestionar el **contexto persistente
del proyecto**.

---

## Fuente de verdad y prioridad

- Al iniciar cualquier tarea, leer primero la documentacion relevante dentro de `Docs/` y validar cualquier duda contra
  el codigo real.
- El codigo del repositorio es la verdad final del comportamiento actual.
- `Docs/` es la verdad documental del proyecto.
- `CLAUDE.md` contiene las reglas originales de operacion.
- Si existe conflicto entre documentacion y codigo, corregir primero la documentacion dentro de `Docs/` para reflejar el
  comportamiento real verificado.

---

### 0.2 Regla de inicio de conversación (carga de contexto)

Al **iniciar cualquier conversación/sesión de trabajo**, CLAUDE debe:

1. Determinar el proyecto actual.
2. Buscar en la ruta indicada un archivo `.md` de contexto correspondiente al proyecto.
3. Si existe, **leerlo y usarlo como fuente de verdad** para:
    - dominio del negocio
    - convenciones adicionales
    - objetos existentes relevantes (áreas/controllers/views)
    - decisiones técnicas y restricciones

**Si hay múltiples archivos candidatos o no es posible determinar el correcto:**

- CLAUDE debe **preguntar** cuál archivo de contexto usar (sin asumir).

**Si no existe archivo de contexto del proyecto:**

- CLAUDE debe continuar con el estándar,
- y marcar como obligación **crear el contexto** en cuanto se haga la primera modificación.

## Estructura obligatoria de docs

Toda la documentacion vive bajo `Docs/`:

```
Docs/
  README.md
  Standards/          # Reglas estables del proyecto
  Context/            # Contexto funcional por feature
    sistema/
    facturacion/
    compras/
    inventario/
    desarrollo/
    recursos-humanos/
    servicio-al-cliente/
    fletes/
    archivo-digital/
    directorio/
  Technical/          # Documento tecnico espejo por archivo de codigo
    lib/
    Areas/
```

- `Docs/Context/` contiene contexto funcional por feature.
- `Docs/Standards/` contiene reglas estables: arquitectura, UI/UX, localizacion, APIs, testing, documentacion, git.
- `Docs/Technical/` contiene un documento tecnico espejo por cada archivo de codigo relevante.
- La ruta en `Docs/Technical/` debe espejar la ruta real del archivo documentado.
- Formato: `Areas/Sistema/Controllers/AccesoController.cs` ->
  `Docs/Technical/Areas/Sistema/Controllers/AccesoController.cs.md`

---

## Protocolo obligatorio de trabajo

1. Determinar la seccion y los features impactados antes de editar codigo.
2. Leer primero los estandares relevantes en `Docs/Standards/`.
3. Leer el contexto general del proyecto en `Docs/Context/README.md`.
4. Leer el contexto funcional del feature en `Docs/Context/`.
5. Leer la documentacion tecnica de los archivos que vayan a tocarse en `Docs/Technical/`.
6. Validar cualquier duda contra el codigo real del repositorio.
7. Identificar desde el inicio que documentos deberan actualizarse al terminar el cambio.
8. Cerrar la tarea con codigo y documentacion consistentes en la misma entrega.

Si la documentacion aun no existe para un feature o archivo impactado, crearla como parte del trabajo.

---

## Uso obligatorio de skills

Antes de modificar, documentar o versionar, usar la skill adecuada si esta instalada.

| Tipo de trabajo                                                       | Skill obligatoria                               |
|-----------------------------------------------------------------------|-------------------------------------------------|
| Mover pantallas, redisenar flujos, ajustar UX/UI, Formularios, diseño | `ui-ux-expert`, `bootstrap`, y `flutter-expert` |
| Crear, editar o revisar documentacion markdown                        | `documentation-standard`                        |
| Documentacion markdown amplia                                         | `markdown-documentation`                        |
| Documentacion de APIs                                                 | `api-documentation-generator`                   |
| Commits de git                                                        | `git-commit`                                    |

Si la tarea cae en varias categorias, usar todas las skills obligatorias aplicables.

---

## Reglas para contextos por feature

Estructura minima obligatoria de cada contexto:

- `# Contexto de Feature`
- `## Proposito`
- `## Alcance actual`
- `## Reglas de negocio y flujo actual`
- `## UI/UX actual` (cuando aplique)
- `## Integraciones y dependencias`
- `## Archivos clave`
- `## Riesgos / consideraciones`
- `## Registro de cambios del contexto`

El cuerpo principal describe solo el estado actual. El historial va en `Registro de cambios del contexto`.

---

## Reglas para documentacion tecnica por archivo

Estructura minima obligatoria:

- `# Documento tecnico`
- `## Archivo fuente`
- `## Responsabilidad`
- `## Donde se usa`
- `## Dependencias`
- `## Flujo interno / comportamiento`
- `## Contratos y efectos secundarios` (cuando aplique)
- `## Consideraciones para modificarlo`
- `## Registro de cambios del documento`

---

## Checklist post-modificacion

- Confirmar que features y archivos reales cambiaron.
- Actualizar `Docs/Context/README.md` si cambio el mapa funcional.
- Actualizar el contexto de cada feature impactado en `Docs/Context/`.
- Actualizar la documentacion tecnica espejo de cada archivo tocado en `Docs/Technical/`.
- Actualizar `Docs/Standards/` si el cambio introduce o modifica una regla estable.
- Agregar entrada en el registro de cambios de cada documento actualizado.
- Informar al usuario que documentacion fue creada o actualizada.

---

## Convenciones del proyecto

- **Lenguaje de documentacion:** Espanol
- **State management:** Provider (`SessionProvider`, `ShopProvider`)
- **Commits:** Conventional commits en espanol: `tipo(alcance): descripcion breve`

---

## Prohibiciones

- No documentar cambios como relato historico dentro del cuerpo principal de un contexto.
- No inventar features, rutas, dependencias o usos no verificados en el codigo.
- No dejar cambios de codigo sin actualizar su documentacion correspondiente.
- No crear documentacion generica o vacia.

### Regla por cada modificación (actualizar/crear contexto)

**Cada vez que CLAUDE realice una modificación** (crear/alterar controllers, views, clientes HTTP, cambiar lógica,
etc.), debe:

- **Actualizar** el archivo de contexto del proyecto si existe, o
- **Crear** el archivo de contexto si no existe,
  siempre como un archivo **Markdown `.md`** guardado en la ruta indicada.

**IMPORTANTE:** SEINMX mantiene su contexto **ÚNICAMENTE** en su repositorio local. No tiene contexto dual.

#### Contenido mínimo requerido del archivo de contexto (.md)

El archivo debe mantenerse **conciso y útil** y contener al menos:

- `# Contexto del Proyecto`
- `## Propósito / Descripción`
- `## Convenciones y Estándares del Proyecto` (si hay extras además de este estándar)
- `## Objetos relevantes` (áreas/controllers/views/clientes HTTP tocados o dependencias importantes)
- `## Decisiones técnicas` (por qué se hizo algo relevante)
- `## Riesgos / Consideraciones`
- `## Registro de cambios de contexto`
    - fecha
    - qué cambió
    - por qué

> Nota: El contexto debe reflejar **hechos del repositorio** y decisiones reales.  
> **No inventar** elementos no verificados.

---

## 1) Lectura Obligatoria de Arquitectura (CRÍTICO)

**ANTES** de realizar cualquier modificación en SEINMX, CLAUDE **DEBE** leer el archivo:

📄 **`Docs/Arquitectura.md`**

Este archivo contiene:

- Estructura completa de áreas
- Convenciones de carpetas y naming
- Configuración de autenticación y autorización
- Reglas de aislamiento entre módulos
- Ejemplos de código para cada capa

**No asumir** nombres de áreas, controllers, o estructura sin consultar primero este archivo.

---

## Reglas Fundamentales

- **No inventar** nombres de áreas, controllers, views, o endpoints que no existan en el contexto o en
  `Arquitectura.md`.
- **Declaraciones al inicio** — Variables se declaran e inicializan al principio de métodos.
- **Documentar lógica compleja** — Agregar comentarios XML en métodos públicos y comentarios inline en lógica de
  negocio.

---

## Regla de Codificación de Archivos (OBLIGATORIO)

Para evitar texto corrupto (ej. `CÃ¡lculo`) al abrir archivos en distintos editores:

- Guardar archivos del repositorio (`.cs`, `.cshtml`, `.md`, `.json`, `.yml`, `.yaml`, `.xml`) en **UTF-8 con BOM**.
- Mantener fin de línea **CRLF** en Windows.
- En comentarios XML y documentación, preferir texto **ASCII sin tildes/ñ** cuando el archivo deba abrirse en editores
  heredados.
- Si un archivo viene en otra codificación y no se puede garantizar compatibilidad, **normalizar a UTF-8 con BOM** antes
  de cerrar el cambio.

---

## Áreas y Estructura de Carpetas

### Correspondencia de módulos (ver `Docs/Arquitectura.md` para detalles completos)

| Área (carpeta) | Esquema SQL |
|----------------|-------------|
| `Inventario`   | `INV`       | 
| `Directorio`   | `DRM`       |
| `Compras`      | `CMP`       |


**IMPORTANTE:** No usar tildes ni ñ en nombres de áreas para evitar problemas en URLs.

---


## ViewModels, DTOs y Validaciones

### Convención de naming

| Tipo          | Sufijo       | Uso                  | Ubicación                                     |
|---------------|--------------|----------------------|-----------------------------------------------|
| **DTO**       | `*Dto`       | Respuestas de la API | `Models/Dtos/`                                |
| **ViewModel** | `*ViewModel` | Formularios y vistas | `Models/ViewModels/` o `Areas/{Area}/Models/` |

### Reglas de ViewModels

- **DataAnnotations obligatorias:** Toda propiedad de input debe tener validación.
- **Display names:** Usar `[Display(Name = "...")]` para etiquetas en español.
- **Mensajes de error personalizados:** Usar `ErrorMessage` en español.

```csharp
// Models/ViewModels/FacturaViewModel.cs
namespace SEINMX.Models.ViewModels;

public class FacturaViewModel
{
    public int IdFactura { get; set; }

    [Required(ErrorMessage = "El folio es requerido")]
    [Display(Name = "Folio")]
    [StringLength(50, ErrorMessage = "El folio no puede exceder 50 caracteres")]
    public string Folio { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha es requerida")]
    [Display(Name = "Fecha de emisión")]
    [DataType(DataType.Date)]
    public DateTime FechaEmision { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "El cliente es requerido")]
    [Display(Name = "Cliente")]
    public int IdCliente { get; set; }

    [Display(Name = "Observaciones")]
    [DataType(DataType.MultilineText)]
    [StringLength(500)]
    public string? Observaciones { get; set; }

    [Required(ErrorMessage = "El total es requerido")]
    [Display(Name = "Total")]
    [DataType(DataType.Currency)]
    [Range(0.01, double.MaxValue, ErrorMessage = "El total debe ser mayor a cero")]
    public decimal Total { get; set; }
}
```

### Validación en cliente

- Habilitar jQuery Validation Unobtrusive en todas las vistas con formularios.
- Incluir scripts en layout o en la vista específica:

```cshtml
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

---

## Controllers y Actions (Naming + Async)

### Naming de controllers

- Formato: `{Entidad}Controller.cs`
- Ejemplos: `FacturasController.cs`, `ClientesController.cs`, `ProductosController.cs`
- **Decorador obligatorio:** `[Area("NombreArea")]`

### Actions estándar (patrón CRUD)

| Action         | Verbo HTTP | Propósito                  | Retorno típico                    |
|----------------|------------|----------------------------|-----------------------------------|
| `Index`        | GET        | Listar registros           | `View(List<T>)`                   |
| `Details/{id}` | GET        | Ver detalle                | `View(T)`                         |
| `Create`       | GET        | Mostrar formulario         | `View()`                          |
| `Create`       | POST       | Crear registro             | `RedirectToAction(nameof(Index))` |
| `Edit/{id}`    | GET        | Mostrar formulario edición | `View(T)`                         |
| `Edit/{id}`    | POST       | Actualizar registro        | `RedirectToAction(nameof(Index))` |
| `Delete/{id}`  | GET        | Confirmar eliminación      | `View(T)`                         |
| `Delete/{id}`  | POST       | Eliminar registro          | `RedirectToAction(nameof(Index))` |



### Reglas obligatorias

- **Async/await:** Todas las llamadas deben ser asíncronas.
- **ValidateAntiForgeryToken:** Obligatorio en todos los POST.
- **ModelState.IsValid:** Validar antes de procesar formularios.
- **TempData para mensajes:** Usar para mensajes de éxito/error entre redirects.
- **Comentarios XML:** Documentar métodos públicos.

---

## Vistas Razor y UI (Bootstrap 5 + AdminLTE v4)

### Estructura de vistas

```
Views/
├── Shared/
│   ├── _Layout.cshtml              # Layout principal con AdminLTE
│   ├── _LayoutLogin.cshtml         # Layout para login (sin sidebar)
│   ├── _Navbar.cshtml              # Barra de navegación superior
│   ├── _Sidebar.cshtml             # Menú lateral
│   ├── _ValidationScriptsPartial.cshtml
│   ├── Error.cshtml
│   └── Components/                 # ViewComponents compartidos
│       └── ...
└── ...

Areas/{NombreArea}/Views/
├── {Entidad}/
│   ├── Index.cshtml
│   ├── Create.cshtml
│   ├── Edit.cshtml
│   ├── Details.cshtml
│   └── Delete.cshtml
├── _ViewImports.cshtml
└── _ViewStart.cshtml
```

### _ViewImports.cshtml del área

> ⚠️ **AMBAS directivas `@addTagHelper` son obligatorias.** El `_ViewImports.cshtml` raíz
> (`Views/_ViewImports.cshtml`) **no** propaga sus directivas a las vistas dentro de `Areas/`.
> Cada área es completamente independiente. Sin `@addTagHelper *, SEINMX`, los TagHelpers
> personalizados del proyecto (ej. `cb-catalogo`) no funcionan. Ver `Docs/Standards/CatalogoComboboxStandard.md §2`.

```cshtml
@* Areas/Facturacion/Views/_ViewImports.cshtml *@
@using SEINMX
@using SEINMX.Areas.Facturacion
@using SEINMX.Areas.Facturacion.Controllers
@using SEINMX.Models.ViewModels
@using SEINMX.Models.Dtos
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, SEINMX
```

### _ViewStart.cshtml del área

```cshtml
@* Areas/Facturacion/Views/_ViewStart.cshtml *@
@{
    Layout = "~/Views/Shared/_Layout.cshtml";
}
```

### Convenciones de vistas

- **Tag Helpers:** Preferir Tag Helpers sobre helpers HTML tradicionales.
- **Formularios:** Usar `asp-action`, `asp-controller`, `asp-area`, `asp-route-*`.
- **Validación:** Incluir `asp-validation-for` y `asp-validation-summary`.
- **Bootstrap 5:** Usar clases de Bootstrap para formularios y layout.
- **AdminLTE v4:** Seguir patrones de AdminLTE para cards, boxes, buttons, tables.


## Reglas de Aislamiento (CRÍTICO)

| Regla                          | Descripción                                                                               |
|--------------------------------|-------------------------------------------------------------------------------------------|
| **Vistas compartidas**         | Layouts y componentes comunes van en `Views/Shared/`. Nunca dentro de un área específica. |
| **Configuración por entorno**  | La URL de la API se define en `appsettings.json`. Nunca en código.                        |
| **Nomenclatura de áreas**      | Nombre completo del módulo en español sin tildes (ej. `Facturacion`, `RecursosHumanos`).  |

---

## Filosofía de Desarrollo

> *"No escribimos código. Dejamos huella en el universo."*

### Principios Clave

1. **Piensa diferente** — Cuestiona suposiciones. Busca la solución más elegante.
2. **Obsesiónate con los detalles** — Comprende los patrones y la filosofía del código existente.
3. **Planifica antes de codificar** — Diseña la arquitectura mentalmente antes de escribir.
4. **Simplifica sin piedad** — La elegancia es cuando no queda nada por quitar.
5. **Itera sin descanso** — Refina hasta que sea excelente, no solo funcional.

### El Código Debe:

- Integrarse perfectamente en el flujo de trabajo humano
- Sentirse intuitivo, no mecánico
- Resolver el problema real, no solo el declarado
- Dejar la base de código mejor de como la encontraste

---

## ⚠️ CHECKLIST OBLIGATORIO (Ejecutar SIEMPRE después de cada modificación)

> **IMPORTANTE:** CLAUDE debe ejecutar este checklist **INMEDIATAMENTE** después de completar cualquier modificación de
> código.  
> No esperar a que el usuario lo solicite.

### Checklist post-modificación:

- [ ] **1. ¿Se modificó/creó algún objeto del proyecto?** (Controller, View, HttpClient, ViewModel, configuración)
    - Si **SÍ** → Continuar al paso 2
    - Si **NO** → Fin del checklist

- [ ] **2. ¿Existe archivo de contexto para este proyecto?**
    - Buscar en `Docs/`
    - Si **SÍ** → Actualizar el archivo existente
    - Si **NO** → Crear nuevo archivo `.md`

- [ ] **3. Actualizar/Crear el archivo de contexto con:**
    - Descripción del cambio realizado
    - Fecha del cambio
    - Ticket/motivo asociado
    - Decisiones técnicas relevantes

- [ ] **4. Confirmar al usuario:**
    - Mencionar que el contexto fue actualizado/creado
    - Indicar la ruta del archivo

### Ejemplo de mensaje de confirmación:

```
✅ Modificación completada
📄 Contexto actualizado: `Docs//SEINMX.md`
```

---

## Protocolo de Edición Segura de Documentación

> Aplica a todos los archivos `.md` en `Docs/` de este proyecto.
> Ver también las reglas globales ED-01 a ED-05 en el `CLAUDE.md` raíz de la solución.

### Reglas específicas para `Docs/Standards/`

Los archivos en `Docs/Standards/` son los más críticos del proyecto — errores aquí afectan
a todos los módulos. Se aplican reglas adicionales:

| Regla | Descripción |
|---|---|
| **EDS-01** | **Leer el archivo completo** antes de editar cualquier archivo en `Docs/Standards/`. |
| **EDS-02** | Editar **una sección por llamada**. Verificar el resultado antes de continuar con la siguiente sección. |
| **EDS-03** | Incluir **mínimo 10 líneas de contexto real** (copiado del archivo) antes y después de cada bloque modificado. |
| **EDS-04** | Para archivos de más de 500 líneas (ej. `ViewsStandard.md`), leer el archivo y **listar las secciones** que se modificarán antes de hacer cualquier cambio. |
| **EDS-05** | Después de cada edición, **verificar** que el número de líneas del resultado sea coherente con el cambio realizado. |

### Archivos de alto riesgo (más de 500 líneas)

| Archivo | Líneas aprox. | Precaución |
|---|---|---|
| `Docs/Standards/ViewsStandard.md` | ~1900 | Editar máximo 1 sección por llamada |
| `Docs/Standards/CrudStandard.md` | variable | Leer antes de editar |
| `Docs/Arquitectura.md` | variable | Cualquier cambio requiere verificación |

---

## 🚫 ERRORES COMUNES A EVITAR

| Error                       | Consecuencia                       | Solución                                                                           |
|-----------------------------|------------------------------------|------------------------------------------------------------------------------------|
| Olvidar actualizar contexto | Pérdida de historial de decisiones | Ejecutar checklist SIEMPRE                                                         |
| Asumir archivo de contexto  | Usar contexto incorrecto           | PREGUNTAR si hay duda                                                              |
| Inventar áreas/controllers  | Código inválido                    | Verificar `Arquitectura.md` primero                                                |
| Omitir TKT en cambios       | Sin trazabilidad                   | Siempre pedir/documentar ticket                                                    |
| **Renderizar `TempData` en vistas** | **Mensajes duplicados** | **`_Layout.cshtml` ya muestra `TempData["SuccessMessage"]` y `TempData["ErrorMessage"]`. NUNCA agregar estos bloques en una vista. Ver R-03 en `Docs/Standards/ViewsStandard.md`.** |

---

## 📋 SECCIONES PENDIENTES DE DEFINIR

Estas secciones se agregarán conforme se establezcan los estándares:

### JavaScript / TypeScript

- Archivos de modulo en `wwwroot/js/{area}/` — ejemplo: `sistema/control-acceso-menu.js`
- Archivos de core compartidos en `wwwroot/js/core/` — ejemplo: `core/modal-confirmacion.js`
- Modulos envueltos en IIFE `(function(){ 'use strict'; })()` — sin contaminar el scope global
- **Prohibido exponer funciones globales** (`window.MODULO.fn()`) para callbacks de Tabulator u otros
- Configuracion de vista inyectada via `window.{MODULO}_CONFIG` (no inline JS en la vista)
- **DataTables** → todas las  tablas planas. UI de filtros más robusta (ColumnControl ☰),
  ColReorder persistente, paginación `full_numbers`.
  - Ver `Docs/Standards/ViewsStandard.md §9.5` y `§9.6` (fachada `CB_TABLA`).
  - Código transversal usa `CB_TABLA.*` (agnóstico del motor) — nunca hacer
    `if (esTabulator) … else …` en módulos de vista.

---

## 📚 Referencias Obligatorias

1. **Arquitectura del Proyecto:** `Docs/Arquitectura.md`
2. **Contexto del Proyecto:** `Docs/SEINMX.md`

---

**Última actualización:** 2026-04-28  
**Versión:** 1.3.0  
**Mantenedor:** Equipo de Desarrollo SEINMX

