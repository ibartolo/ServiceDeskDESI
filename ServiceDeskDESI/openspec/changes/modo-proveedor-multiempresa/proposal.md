# Proposal: Modo Proveedor Multiempresa (MSP) — IDEA, no aprobada

- **Change**: `modo-proveedor-multiempresa`
- **Fecha**: 2026-09-09
- **Estado**: 💡 **Idea para trabajar después** — NO explorada, NO aprobada, NO implementada.
- **Origen**: Consulta del usuario (desarrollador con varios softwares instalados en distintas empresas: seguros, consultorio médico, ERP, etc.).

## Intent

Evaluar si el sistema puede servir para **dos escenarios** con el mismo producto:

- **Escenario A (actual):** el área de Sistemas/TI de **una empresa** atiende a sus propias áreas internas (RH, contabilidad, etc.). El tenant es la empresa dueña del sistema.
- **Escenario B (idea):** el dueño del software actúa como **proveedor** que da soporte a **varias empresas cliente**, cada una con sus propios usuarios que levantan tickets. El tenant es cada cliente; el proveedor es transversal a todos.

El requisito clave es que **el Escenario A siga funcionando igual** (cambio aditivo, no sustitutivo).

## Hallazgos del análisis inicial

**A favor (base ya existente):**
- `Empresa` es el tenant y **pueden coexistir varias** en la misma BD.
- Existe flujo de **registro de empresa** (Empresa + Sucursal + Área + usuario admin + roles base + permisos).
- `Empresa` ya tiene **vigencia y periodo de prueba** (`FechaVigenciaInicio/Fin`, `EsPeriodoPrueba`) → el modelo se pensó multi-empresa/SaaS.
- Cada empresa tiene **Usuarios, Roles, Permisos, Páginas** propios.
- Tickets con foliador, estatus y asignación ya operativos.

**Huecos para el Escenario B:**
1. **No existe un rol "proveedor" cross-tenant.** Hoy un usuario pertenece a una empresa y ve solo lo suyo.
2. **Aislamiento "por inferencia":** las tablas de dominio no tienen `EmpresaId`; la pertenencia se deduce con `tabla.CreadoPor = Usuarios.NombreUsuario`. Válido para "una empresa", frágil si el proveedor ve todo.
3. **Asignación/enrutamiento entre clientes** (quién atiende qué cliente, SLA por cliente).
4. **Contexto de sesión conmutable** (hoy la "empresa activa" es fija).

**Nota:** el catálogo `Compania` (aislado, con CRUD completo y sin FKs) podría ser la semilla de este concepto ("empresas cliente"), aunque no está confirmado.

## Caminos posibles (sin decidir)

| Opción | Implica | Pro | Contra |
|---|---|---|---|
| **A. Multi-tenant + capa proveedor** | Una instalación, N empresas, rol proveedor global | Un despliegue, escalable | Rediseño de tenancy/permisos |
| **B. Una instalación por empresa** | Desplegar N veces | Aislamiento natural, cero cambios | N despliegues/mantenimientos |
| **C. Híbrido** | Instancia proveedor + instancias cliente | Control | Integración compleja |

## Veredicto

- **Viable: SÍ** (la base multi-empresa ya existe).
- **Complejidad: MEDIA-ALTA** (el reto es el modelo de acceso del proveedor y el aislamiento, no "crear empresas").
- **Valor: SÍ**, pero **cambia el posicionamiento** de "helpdesk interno" a **MSP/PSA** (soporte a múltiples clientes).

## Alcance de esta entrada

Este documento es **solo un registro de idea**. No incluye diseño técnico, tareas ni migraciones. Para avanzar debe hacerse una fase de exploración (`sdd-explore`) que decida el camino A/B/C y defina el modelo de acceso del proveedor.

## Entregables de esta sesión

- Análisis en PDF: `Analisis - Modo Proveedor Multiempresa.pdf` (raíz del proyecto), con diagramas de flujo básicos, implicaciones y problemas.

## Archivos

- `openspec/changes/modo-proveedor-multiempresa/proposal.md` (este archivo)
- `Análisis - Modo Proveedor Multiempresa.pdf` (raíz)
