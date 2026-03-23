# jira-open Commands

This folder exposes PowerShell-based integration commands for OpenSpec + Jira.

Guia recomendada de operacion multi-repo:
- `Docs/Guia_Opsxj_MultiRepo.md`

## Prerequisites

- `openspec.cmd` available in PATH
- `git` with a configured `origin` remote
- GitHub connection:
  - Preferred: `GITHUB_TOKEN` (PAT) configured in `Tools/jira-open/.jira-open.env` or env var
  - Fallback: `gh` (GitHub CLI) in PATH and authenticated (`gh auth login`)
- PowerShell execution with `-ExecutionPolicy Bypass`
- Jira config:
  - Fill `Tools/jira-open/.jira-open.env` (local file), or
  - Export env vars `JIRA_BASE_URL`, `JIRA_EMAIL`, `JIRA_API_TOKEN`
- Git/GitHub connection config in the same `Tools/jira-open/.jira-open.env`:
  - `GIT_REMOTE_NAME` (default: `origin`)
  - `GIT_BASE_BRANCH` (optional, overrides base branch auto-detection for PRs)
  - `GITHUB_TOKEN` (recommended; enables direct GitHub API auth for PR create/list/merge checks)
  - `GITHUBREPO` or `GITHUB_REPO` (optional). If empty, `opsxj` auto-detects `owner/repo` from `git remote get-url <GIT_REMOTE_NAME>`.
  - `OPSXJ_IMPACT_REPOS` (optional, comma-separated repo names from catalog; forces impacted repos for `opsxj:new`)
  - `OPSXJ_READONLY_REPOS` (optional, comma-separated repo names from catalog; marked as `solo consulta (sin cambios)` and excluded from impact)
  - `OPSXJ_MIGRATION_READONLY_REPO_PATHS` (optional, semicolon-separated absolute paths to legacy/migration repos used only as reference when issue text contains `MIGRACION-NET <NombreFuncion>`)

## Commands

Run from repository root:

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:test-jira
```

Then create a change:

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:new -- <ISSUE-KEY>
```

Enterprise/headless mode:

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:new -- <ISSUE-KEY> -NonInteractive
```

Run environment diagnostics before `new/archive`:

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:doctor -- [ISSUE-KEY]
```

Validate non-interactive prerequisites:

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:doctor -- [ISSUE-KEY] -NonInteractive
```

Repository target mode:

- Auto-detection (default) or explicit configuration via `OPSXJ_IMPACT_REPOS`:

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:new -- <ISSUE-KEY>
```

Run PR flow tests:

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:test-pr
```

Run archive flow tests:

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:test-archive
```

Archive by issue key (or explicit change name):

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:archive -- <ISSUE-KEY|CHANGE-NAME> [-Yes] [-SkipSpecs]
```

Archive in enterprise/headless mode:

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:archive -- <ISSUE-KEY|CHANGE-NAME> -NonInteractive [-Yes] [-SkipSpecs]
```

Example:

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:archive -- OPSXJ-200 -Yes -SkipSpecs
```

Transition Jira issue to done state (without local archive operations):

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:jira-done -- <ISSUE-KEY>
```

List pending Jira tickets (read-only):

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:jira-pending -- [PROJECT-KEY|ISSUE-KEY|jql:<QUERY>]
```

## Notes

- `opsxj:new` creates/updates `openspec/changes/<change-name>/` artifacts (including `sync.md` impact matrix), runs `openspec.cmd validate`, transitions Jira to `En curso` after successful OpenSpec validation, creates/pushes a branch, opens or reuses a GitHub PR, transitions Jira to `En Revision` when the PR is available, and adds a Jira comment with the PR URL plus the manual-merge reminder when the PR is newly created.
- `opsxj:new -NonInteractive` keeps the same flow but requires preauthorized Jira/GitHub credentials and blocks interactive GitHub auth fallback.
- `opsxj:new` requires a valid Jira issue. If Jira lookup fails, the command stops and does not create artifacts.
- `opsxj:doctor` validates tooling, clean working tree, Jira/GitHub token configuration, git remote/base branch, and optional Jira issue lookup.
- `opsxj:doctor -NonInteractive` reports `execution_mode: noninteractive` and validates the same flow for headless use.
- `opsxj:jira-pending` lists pending Jira tickets (`statusCategory != Done`) for a project or custom JQL without changing Jira/Git/GitHub state.
- Missing parameters are handled as strict validation errors (no auto-correction or fallback execution).
- Interactive repo selection is disabled by policy. Use `OPSXJ_IMPACT_REPOS` and `OPSXJ_READONLY_REPOS` in `.jira-open.env`.
- `OPSXJ_READONLY_REPOS` entries are forced as non-impacted in `sync.md` with reason `solo consulta (sin cambios)`.
- `OPSXJ_MIGRATION_READONLY_REPO_PATHS` is appended to generated `sync.md` only when Jira ticket text contains `MIGRACION-NET <NombreFuncion>`.
- Trigger example:
  - `MIGRACION-NET ExecuteQ07InsertLogRespuestaAsync`
  - `MIGRACION-NET: RegistrarRadicacionEntranteAsync`
- `opsxj:new` now includes backend update baseline rules in generated artifacts via `openspec/context/OPSXJ_BACKEND_RULES.md`.
- PR title is based on Jira ticket `summary`.
- GitHub auth priority is `GITHUB_TOKEN` first, then `gh` CLI as fallback.
- In `-NonInteractive`, GitHub auth is token-only: `GITHUB_TOKEN` is mandatory and `gh auth login` is not used as an operational fallback.
- If a PR already exists for the change branch, `opsxj:new` reports that PR instead of creating a duplicate.
- Pull requests created by `opsxj` are never merged automatically; merge remains a manual reviewer action.
- `opsxj:archive` enforces merge validations and Jira transition; policy blocks `-NoValidate` and `-SkipJira`.
- `opsxj:archive` can only finish after all PRs associated in `sync.md` are effectively merged.
- `opsxj:archive` transitions Jira to done before local archive to avoid local/Jira drift.
- `opsxj:jira-done` is available for explicit Jira-only transition.
- Audit entries in `openspec/logs/*.log.jsonl` include the execution mode (`legacy` or `noninteractive`).
- Tooling protection: changes under `Tools/jira-open/**` are blocked by CI unless PR/commit metadata includes `opsx:explore` and authorization key `Maria20230126*`.
- In this environment, running via `npm --prefix Tools/jira-open` from repo root is more reliable than running from `Tools/jira-open` directly.
