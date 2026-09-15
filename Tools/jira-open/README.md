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

## Commands

Run from repository root:

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:test-jira
```

Then create a change:

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:new -- <ISSUE-KEY>
```

Run environment diagnostics before `new/archive`:

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:doctor -- [ISSUE-KEY]
```

Repository target mode:

- Auto-detection (default):

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:new -- <ISSUE-KEY>
```

- Manual selection (interactive list):

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:new -- <ISSUE-KEY> -SelectRepos
```

Examples:

```powershell
npm.cmd --prefix Tools/jira-open run opsxj:new -- OPSXJ-200
npm.cmd --prefix Tools/jira-open run opsxj:new -- OPSXJ-200 -SelectRepos
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

- `opsxj:new` creates/updates `openspec/changes/<change-name>/` artifacts (including `sync.md` impact matrix), runs `openspec.cmd validate`, creates/pushes a branch, and opens a GitHub PR.
- `opsxj:new` requires a valid Jira issue. If Jira lookup fails, the command stops and does not create artifacts.
- `opsxj:doctor` validates tooling, clean working tree, Jira/GitHub token configuration, git remote/base branch, and optional Jira issue lookup.
- `opsxj:jira-pending` lists pending Jira tickets (`statusCategory != Done`) for a project or custom JQL without changing Jira/Git/GitHub state.
- Missing parameters are handled as strict validation errors (no auto-correction or fallback execution).
- `opsxj:new -SelectRepos` asks interactively which repositories are impacted and pre-fills `sync.md` (`yes/no`, `pending/n/a`).
- `opsxj:new` now includes backend update baseline rules in generated artifacts via `openspec/context/OPSXJ_BACKEND_RULES.md`.
- PR title is based on Jira ticket `summary`.
- GitHub auth priority is `GITHUB_TOKEN` first, then `gh` CLI as fallback.
- If a PR already exists for the change branch, `opsxj:new` reports that PR instead of creating a duplicate.
- `opsxj:archive` enforces merge validations and Jira transition; policy blocks `-NoValidate` and `-SkipJira`.
- `opsxj:archive` transitions Jira to done before local archive to avoid local/Jira drift.
- `opsxj:jira-done` is available for explicit Jira-only transition.
- Tooling protection: changes under `Tools/jira-open/**` are blocked by CI unless PR/commit metadata includes `opsx:explore` and authorization key `Maria20230126*`.
- In this environment, running via `npm --prefix Tools/jira-open` from repo root is more reliable than running from `Tools/jira-open` directly.
