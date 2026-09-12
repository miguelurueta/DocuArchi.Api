# SCRUM-397 — Ciclos de archivo por instrumento

Este repositorio implementa `GET /api/instrumentos-archivisticos/{idInstrumento:int}/ciclos` en `InstrumentosArchivisticosController.ListCycles` y registra `IInstrumentoArchivisticoCicloService` e `IInstrumentoArchivisticoCicloRepository` mediante `AddInstrumentosArchivisticos`.

El cliente no proporciona alias ni tipo de instrumento. Ambos se resuelven en backend. El contrato y la evidencia E2E completos se mantienen en DocuArchiCore bajo `Docs/GestionDocumental/InstrumentosArchivisticos/Core/SCRUM-397-instrumentos-ciclos-archivo/`.
