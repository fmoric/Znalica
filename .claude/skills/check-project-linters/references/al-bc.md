# AL analyzer activation and invocation

Read with `check-project-linters` **only for AL/BC extension projects** (`al-bc`). Do
not use the AL MCP compiler to lint C/AL / NAV object files.

## Find the project's enabled analyzers

- Inspect workspace `.vscode/settings.json` or `.code-workspace` settings for
  `"al.enableCodeAnalysis": true` **and** the selected `al.codeAnalyzers`, plus
  any `al.ruleSetPath`.
- In AL-Go, inspect `.github/AL-Go-settings.json`, each project's `.AL-Go/settings.json`,
  and any documented overrides; resolve *effective* settings for `enableCodeCop`,
  `enableUICop`, `enableAppSourceCop`, `enablePerTenantExtensionCop`,
  `customCodeCops`, `rulesetFile`, and `enableCodeAnalyzersOnTestApps` if tests
  changed. External variables and project overrides can change checked-in values;
  AppSourceCop/PerTenantExtensionCop can also be derived from the AL-Go project
  `type` without an explicit flag. Unavailable settings are **unchecked**.
- `app.json`, `AppSourceCop.json`, and a ruleset file **alone** do not prove
  analyzer activation. **ALOps** supplies CI pipeline tasks, not a code analyzer.
  **ALCops** is a distinct custom analyzer: count it only if this project actually
  configures its DLL. Do not enable a cop this project did not select.

## Prove the check ran, not merely that it found nothing

AL MCP `al_compile` / `al_build` leave `enableCodeAnalysis` and `codeAnalyzers` at
unspecified **server defaults** unless set. `al_compile` also returns **errors
only** by default. Register with `al_addproject` in this session. Reuse a
compile/build only when its analyzer settings and app scope match; otherwise
use a safe project command or pass only the project's selected built-in
analyzers via `enableCodeAnalysis: true`, `codeAnalyzers` and
`onlyErrors: false` where supported. The AL-Go compiler token `PTECop` maps
to `${PerTenantExtensionCop}` on the AL MCP interface; use the MCP tool's
documented `${...}` values, not raw CI flags or a custom analyzer DLL.

Built-in `codeAnalyzers` options do **not** prove that a separate ruleset or
custom DLL was loaded. Establish its actual invocation, or mark that portion
**not run**. After the compile, request `al_getdiagnostics` with
`severities: ["error", "warning"]` for the affected project. Report the limit
and any truncated or unknown diagnostic count. A successful **explicit
analyzer invocation** or analyzer-sourced diagnostics is evidence; an empty
diagnostic list after a bare default build is **not**. A static compile does
not require a live BC connection; never publish to production to run lint.

## Fixing AL diagnostics (fix mode)

Assume no command-line fixer: the AL quick fixes are VS Code code actions, not something
this session can run, unless the project declares a fix command. Fix each `warning` / `error`
diagnostic in the changed objects by hand, then compile again with **the same analyzer
options** and read `al_getdiagnostics` again — a re-compile without them reports nothing
about CodeCop. Do not add `#pragma warning disable AA0xxx`, change the ruleset, or change
`al.codeAnalyzers` / AL-Go settings to clear a finding. Renaming a field, a procedure's
parameters or a public object is a behavior change for its callers and for an upgrade:
report the finding instead of renaming.

Primary sources: [Microsoft AL code-analysis settings](https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/developer/devenv-using-code-analysis-tool),
[AL MCP compile options](https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/developer/al-agent-tools/al-tool-compile),
and [AL diagnostics filtering](https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/developer/al-agent-tools/al-tool-get-diagnostics).
