## Repo Impact Plan

Ticket: $(System.Collections.Hashtable.key)  
Summary: $(System.Collections.Hashtable.summary)  
Change: openspec/changes/scrum-48-pruebas-flujo-radicacion-entrante/",
            ",
            

| $(Repo) | $(no) | $(fuera de alcance) | $(n/a) | $(n/a) | $(n/a) | $(n_a) |
| $(---) | $(no) | $(fuera de alcance) | $(n/a) | $(n/a) | $(n/a) | $(n_a) |
| $(DocuArchi.Api) | $(yes) | $(<definir alcance>) | $(pending) | $(pending) | $(pending) | $(todo) |
| $(DocuArchiCore) | $(no) | $(fuera de alcance) | $(n/a) | $(n/a) | $(n/a) | $(n_a) |
| $(DocuArchiCore.Abstractions) | $(no) | $(fuera de alcance) | $(n/a) | $(n/a) | $(n/a) | $(n_a) |
| $(DocuArchiCore.Web) | $(yes) | $(<definir alcance>) | $(pending) | $(pending) | $(pending) | $(todo) |
| $(MiApp.DTOs) | $(yes) | $(<definir alcance>) | $(pending) | $(pending) | $(pending) | $(todo) |
| $(MiApp.Services) | $(yes) | $(<definir alcance>) | $(pending) | $(pending) | $(pending) | $(todo) |
| $(MiApp.Repository) | $(yes) | $(<definir alcance>) | $(pending) | $(pending) | $(pending) | $(todo) |
| $(MiApp.Models) | $(yes) | $(<definir alcance>) | $(pending) | $(pending) | $(pending) | $(todo) |

## Rule

- Mark impacted repos first (Impacta? = yes/no).
- Run opsxj:new SCRUM-48 only in repos marked yes.
- Run opsxj:archive SCRUM-48 only after PR is merged in that repo.