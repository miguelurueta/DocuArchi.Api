## Repo Impact Plan

Ticket: $(System.Collections.Hashtable.key)  
Summary: $(System.Collections.Hashtable.summary)  
Change: openspec/changes/scrum-47-orquestador-validacion-campos-radicacion/",
            ",
            

| $(Repo) | $(no) | $(fuera de alcance) | $(n/a) | $(n/a) | $(n/a) | $(n_a) |
| $(---) | $(no) | $(fuera de alcance) | $(n/a) | $(n/a) | $(n/a) | $(n_a) |
| $(DocuArchi.Api) | $(yes) | $(<definir alcance>) | $(pending) | $(pending) | $(pending) | $(todo) |
| $(DocuArchiCore) | $(yes) | $(<definir alcance>) | $(pending) | $(pending) | $(pending) | $(todo) |
| $(DocuArchiCore.Abstractions) | $(no) | $(fuera de alcance) | $(n/a) | $(n/a) | $(n/a) | $(n_a) |
| $(DocuArchiCore.Web) | $(yes) | $(<definir alcance>) | $(pending) | $(pending) | $(pending) | $(todo) |
| $(MiApp.DTOs) | $(yes) | $(<definir alcance>) | $(pending) | $(pending) | $(pending) | $(todo) |
| $(MiApp.Services) | $(yes) | $(<definir alcance>) | $(pending) | $(pending) | $(pending) | $(todo) |
| $(MiApp.Repository) | $(yes) | $(<definir alcance>) | $(pending) | $(pending) | $(pending) | $(todo) |
| $(MiApp.Models) | $(no) | $(fuera de alcance) | $(n/a) | $(n/a) | $(n/a) | $(n_a) |

## Rule

- Mark impacted repos first (Impacta? = yes/no).
- Run opsxj:new SCRUM-47 only in repos marked yes.
- Run opsxj:archive SCRUM-47 only after PR is merged in that repo.