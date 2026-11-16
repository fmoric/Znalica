---
name: diagnose-mcp
description: Use when an MCP capability expected by this project or granted to an agent is absent from your loaded tools; distinguish deliberate scope, personal configuration, setup gaps, workspace-file loss, and cross-tool schema defects before suggesting a remedy.
---

# Diagnose Missing MCP Tools

The always-on `no_silent_tool_loss` rule requires disclosure **before a fallback**. This skill is for
diagnosis **after a capability is actually missing**, not a session-start health check. Never make an
unverified cause sound like a confirmed fix, and never hide a tool error behind successful fallback output.

## Establish what was supposed to load

Use **your own loaded tools**, not an MCP status panel: Claude Code names them `mcp__<server>__*`; Copilot
CLI uses `<server>/*`. Check the delivered agent definition's frontmatter `tools:` allowlist
(`.claude/agents/<name>.md` or `.github/agents/<name>.agent.md`). If the server is not granted, it was
withheld deliberately; report an answer limitation if needed, **not** an installation failure. A main
session has no agent definition and receives all enabled servers.

Only for a granted-but-absent server, compare the committed `.mcp.json`. If it has **no entry**, this is a
personal server: investigate the user's own MCP configuration, not a supposed defect in the repository.
Do not expose tokens or other credential values while checking configuration or environment variables.
For a **committed entry** that failed to load, use the classes below.

## Why the failure is silent: classify by breadth first

The shared `.mcp.json` declares servers but does not install their binaries, tokens or local services.
Different failures look alike from inside a session; the remedies do not:

| Class | Causes | Scope | Whose fix |
|---|---|---|---|
| **A — setup gap** | missing binary, dead endpoint, unset `${VAR}` | usually one entry per cause; several may fail separately | that machine |
| **B — Copilot discards the workspace file** | untrusted folder, or `${VAR}` in a server `url` (Copilot CLI before 1.0.89) | **every** server the committed file declares | routing decision or that user's `trustedFolders` |
| **C — cross-tool schema defect** | `"type": "local"` in shared config | that entry on Claude Code | the repository |

**Count entries in the committed file, not servers visible in your tools.** Personal servers can still be
loaded when every workspace server is gone. **Every** declared server absent on Copilot **and the same repo
working under Claude** points to B; both halves matter. Two uninstalled binaries in a two-server file also
look like "everything missing" without the cross-tool check. If that check is unavailable, do not assert B.
If **some but not all** entries are absent, examine **each** absent entry; on Claude, `"type": "local"`
points to C, while an ordinary entry may have an A cause that must still be checked. Do not diagnose from
which personal servers survived. In an agent-scoped session, **withheld** entries are not failed entries:
if you cannot observe all declared servers, do not infer whole-file loss from that agent's tool list.

**Every declared server absent on Claude is not B**: a missing/unloaded committed file, invalid JSON or
all servers disabled can fit no class here. Inspect `claude mcp list` and report the actual cause instead
of forcing A. If the evidence fits no class, report that the cause is unverified and name the next check.

## A — setup gap on this machine

Check the particular server's stdio binary, HTTP endpoint and referenced `${VAR}` rather than advising an
install by default. An unset variable in a `url` fails **that server alone**; `.env` is **not auto-loaded**,
so check for a project launcher before telling the user to export a value manually (the fix may be creating
the `.env` file). **Not established:** whether an unset `${VAR}` in stdio `env` or HTTP `headers` behaves
the same way. Report which cause you confirmed; only a **missing binary** warrants "install it."

## B — Copilot discards the whole workspace file

Fingerprint: **all** committed servers absent on Copilot while the same repository works in Claude.
Neither cause is fixed by installing a server:

1. **Check folder trust first.** Copilot loads workspace `.mcp.json` only for a folder in
   `trustedFolders` in `~/.copilot/config.json` (or when a prompt-mode opt-in environment variable is
   set). Read that file as text: it is JSONC (comments allowed), so `jq` or a strict JSON parser fails on
   a healthy file. If untrusted, seek authorization to add the path and re-check; a `url` problem can coexist.
   **Adding it:** the user can start `copilot` in that folder and accept its trust prompt (whether that
   persists the entry is unmeasured here). For a hand edit, back the file up first and write each `\` in the
   path as `\\`; never insert it with `sed` or another text substitution — one such insert collapsed the
   escaping and broke the file (2026-11-11). Re-read `trustedFolders` afterwards; to confirm the servers load,
   use a live Copilot session's tools, not a listing — on 1.0.105 `copilot mcp list` can omit a loaded workspace
   file, and `copilot mcp get` is a configuration view.
   A live untrusted session on Copilot CLI 1.0.89 still skipped the file silently (measured 2026-09-29).
   Confirm the cause in the user's session before treating a missing trust entry as the whole
   explanation; never change personal config unasked.
2. Once trust is confirmed, check every workspace server's **`url`** for `${VAR}`. **Before Copilot CLI
   1.0.89** that discards the entire file, even entries without a variable; **from 1.0.89** the `url` is
   expanded and an unset variable fails that server alone — class A, not B. Check the user's version
   (`copilot --version`); treat an unknown version as the old behaviour. **On 1.0.89 or later with trust
   confirmed, a `url` reference does not explain every server being absent** — report that cause as
   unverified and name the next check rather than stopping at "export the variable". Claude is unaffected;
   variables in `env`, `args` and `headers` were measured not to have this whole-file effect (1.0.70). Report the offending server and let the user decide whether to move its endpoint to
   personal MCP config or make the committed URL literal.

`copilot mcp list` is **not a health verdict**: both causes can remove its `Workspace servers:` section,
on 1.0.105 the section can be missing even when the file loaded,
and it reports configuration, not reachability. Entries in `~/.copilot/mcp-config.json`, the `github`
server and the VS Code bridge may still load, so surviving tools do not disprove whole-file loss.

## C — shared config contains another tool's key

Copilot documents `"type": "local"`; Claude Code **skips** that entry without an in-session error.
`claude mcp list` reports `Skipped — unknown MCP server type "local"`. Confirm the shared entry and
Claude's skip; cross-check the other tool when available. This is a **repository portability defect**,
not a missing install. A shared stdio entry uses `command` + `args` without this tool-specific type; keep
tool-specific options in personal config (`~/.copilot/mcp-config.json` or `claude mcp add --scope user|local`).
One exception is delivered on purpose: `"alwaysLoad": true`, a Claude Code key Copilot CLI 1.0.90 was measured
to load unchanged. It is not this defect; do not remove it as a fix.
**Not established:** whether Copilot accepts `"type": "stdio"`, or whether Claude *honours* (rather than
just tolerates) `tools` / `timeout`. Only `"type": "local"` was measured fatal.

## Report before recommending action

Separate the observed missing tools, the fallback and its cost, the checks you actually ran, and any
remaining uncertainty. Prescribe a remedy **only for a verified cause**; otherwise name the next diagnostic
check instead of guessing an install or a repo edit. `mcp-server-setup` is a different skill: use it and
the generated `MCP-SETUP.md` **if** the confirmed remedy calls for registering/installing a server. Before
changing user config, installing anything or editing shared state, follow `external_tool_state_change`.
