## Context

- Jira issue key: SCRUM-47
- Jira summary: ORQUESTADOR-VALIDACION-CAMPOS-RADICACION
- Jira URL: https://contasoftcompany.atlassian.net/browse/SCRUM-47

## Context Reference

- openspec/context/multi-repo-context.md
- openspec/context/OPSXJ_BACKEND_RULES.md

## Problem Statement

En el proyecto multirepo necesito crear un servicio llamado ValidaCamposRadicacion. Ubicación: Repositorio: MiApp.Service Ruta: /Service/Radicacion/Tramite Objetivo: El servicio debe retornar List<ValidationError> envuelto en AppResponses. Si existen errores, deben ser retornados utilizando ValidationHelper. El servicio debe orquestar todas las funciones de validación de las plantillas de radicación. Parámetros: defaultDbAlias → parámetro enviado. Dependencias: ValidaCamposDinamicosUnicosRadicacion ValidaDimensionCampos ValidaCamposObligatorios Requerimientos funcionales: El servicio debe implementarse dentro de la función RegistrarRadicacionEntranteAsync para validar los datos antes de registrar. Retornar la estructura envuelta en la clase AppResponses. El servicio debe estar envuelto en bloque try/catch para manejo controlado de errores. Requerimientos adicionales: Implementar siguiendo el patrón Service → Repository. Crear una interfaz en el mismo archivo que la clase para facilitar pruebas unitarias: Ejemplo: public interface IValidaCamposRadicacionService {
    Task<AppResponses<List<ValidationError>>> ValidaCamposRadicacionAsync(string defaultDbAlias, RadicacionDTO dto);
}
public class ValidaCamposRadicacionService : IValidaCamposRadicacionService { ... } Registrar la interfaz y la clase en Program.cs del repositorio DocuArchi.Api bajo la sección de servicios. Todas las consultas deben ser parametrizadas para evitar SQL Injection. Documentar la función con comentarios XML indicando propósito, parámetros y retorno. Validar que si no existen registros coincidentes, se retorne Success=true con Data=null y Message="Sin resultados". En caso de excepción, retornar Success=false con:  y Data vacío. 8. Incluir pruebas unitarias: Caso Success con datos válidos. Caso sin resultados. Caso excepción simulada. Incluir pruebas de integración con MySQL usando Testcontainers/Docker: Validar que las funciones de validación se ejecutan correctamente contra datos reales. Mantener separación de responsabilidades (SoC), bajo acoplamiento y alta cohesión. Cumplir principios SOLID en la implementación. Agregar en la documentación los diagramas de casos de uso, diagrama de clases, diagrama de secuencia y diagrama de estado en el repositorio DocuArchiCore /Docs/Radicacion/Tramite. Agregar documentación técnica de implementación para el frontend: Descripción del DTO. Parámetros de envío a la API. Ejemplo de respuesta con errores de validación.

## Approach

- Convertir requerimientos del issue en deltas OpenSpec claros y testeables.
- Aplicar restricciones de repositorio, arquitectura y pruebas de OPSXJ_BACKEND_RULES.
- Definir alcance y no-alcance antes de implementar.
- Validar con openspec.cmd validate scrum-47-orquestador-validacion-campos-radicacion.