## 1. Discovery

- [ ] 1.1 Revisar el issue Jira SCRUM-47 y confirmar alcance.
- [ ] 1.2 Confirmar repos impactados y rutas destino antes de crear Controllers/DTOs/Models/funciones.
- [ ] 1.3 Solicitar estructura de tabla si se requiere nuevo modelo.

## 2. Specs

- [ ] 2.1 Completar specs/jira-scrum-47/spec.md con requisitos finales.
- [ ] 2.2 Incluir referencia explicita a openspec/context/OPSXJ_BACKEND_RULES.md.
- [ ] 2.3 Verificar escenarios testables por requisito.

## 3. Application

- [ ] 3.1 Aplicar patron ApiController + Service + AutoMapper + Repository con AppResponses y try/catch.
- [ ] 3.2 Registrar interfaces en Program.cs (Services L / Repositories R).
- [ ] 3.3 Integrar cambios de aplicacion y verificar compilacion local.

## 4. Test

- [ ] 4.1 Implementar Unit/Integration/Contract tests y documentar evidencia.
- [ ] 4.2 Ejecutar dotnet test (o skipped explicito si Docker no disponible).
- [ ] 4.3 Validar y archivar con OpenSpec.