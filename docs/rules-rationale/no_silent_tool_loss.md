# `no_silent_tool_loss` — rationale

Moved verbatim out of the `no_silent_tool_loss` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Why this lands on the agent

Nothing else is positioned to notice. A static check reads files and the files are right. The person who
cloned the repo has no reason to suspect anything. You are the only participant who can see both what the
configuration promised and what actually loaded — so when those disagree, you are the one who has to say it.

## Source

Second-machine MCP onboarding gap: a `serena` server committed to a project's `.mcp.json` whose binary was
never installed on a teammate's machine. Every static check passed; only `claude mcp list` surfaced it, and
an agent there would have lost code navigation while the config looked correct. Converges with two 2026-07-15
notes from using the packet — *"if serena is deployed first task should be to onboard it, or it will not
work"* — and with a 2026-07-26 case where three registered Serena endpoints were all unreachable.

## Field notes moved from the rule body

  - ⚠️ **This rule claimed the fingerprint had "essentially one cause" until 2026-08-08.** It has two, and
    the trust half is the one that hits a reader who finds no `${VAR}` and concludes the fingerprint does
    not apply.

- **`copilot mcp list` no longer separates the two, and it never was a verdict.** At the 2026-08-07
  measurement (CLI version not recorded) it still printed an untrusted folder's `Workspace servers:` section,
  while the `url` discard removed it. **On Copilot CLI 1.0.85 (measured 2026-09-17) an untrusted folder
  removes the section too** — so a missing section can be either cause, and a present one still proves
  nothing about the session.
  - **It cannot be more than that, and the reason is in the packet library's platform notes (not delivered):
    `copilot mcp list` reports configuration with NO health.** A server it lists may equally have failed to
    spawn. Read `~/.copilot/config.json` for the project path — as text, since it is JSONC and a strict
    JSON parser fails on a healthy file — and settle it there: a named absence in a
    file you opened beats an inference from a listing. **The inverse arm is weaker still:** a missing
    section also matches no committed `.mcp.json`, malformed JSON, or every server disabled.

 That misreading is what produced class C's cause below — in the field, on this
  packet: someone chased a per-server schema fix for a whole-file trust gate, and the "fix" broke the other
  tool.

 *(A/B measured 2026-08-08, one variable, both file states, both
  tools: Copilot CLI **1.0.78** loaded the server from **either** form; Claude Code **2.1.226** loaded it
  only from the bare form.)*

  - **Do not assume the edit was careless — it is the natural move.** The vendor documentation a person
    finds while debugging a Copilot problem describes Copilot's form, and applying it to the shared file
    looks like hardening. In the case on record the edit was an attempted fix for **class B's folder-trust
    cause**, which it did not address.

### What the classes are for

**Every one of these leaves a capability quietly absent from your tools while the file itself parses and
reads correctly.** But they do not mean the same thing, and the split matters more than the list. It is
**three** classes, not a machine-versus-repo binary — that binary is what routed a trust outage to an
install:

- **Class A** — the committed file is genuinely correct and nothing in the repo needs changing. The fault is
  a machine. **This is the only class an install fixes.**
- **Class B** — the file is valid JSON and still costs **every** Copilot user in the project the entire MCP
  set. Folder trust is that person's own config and touches nothing shared; a `${VAR}` in a `url` is a
  routing decision that may well end in a change to the committed file. *(The `url` cause is Copilot CLI
  before 1.0.89 only: from 1.0.89 the `url` expands and an unset variable fails that server alone —
  measured 2026-09-29.)* **Never report either as an
  install, and never as "nothing in the repo needs changing" without knowing which of the two you have.**
- **Class C** — the committed file really is wrong, every colleague on that commit hits it, and the fix is a
  change to the repo. **Never fold it in with class A.**

Six causes produce that state and they sort into **three remedy
  classes**; the `diagnose-mcp` skill's *"Why the failure is silent"* section names them (it moved there
  from the rule body in 0.109.0). **The steps below pick the class off cheap reads you already have, and
  *"install the binary"* — which the rule now forbids as a default — is right for only one of the three:**
  1. **Count what the committed `.mcp.json` declares — then check the other tool before you commit to the
    answer.** ***Every*** **declared server absent, on Copilot,** ***and the same repo working under
    Claude*** **→ class B**: the whole file was discarded — either for a `${VAR}` in a `url` (Copilot CLI
    before 1.0.89) or because the project folder is not in `trustedFolders`. **No install helps**; the remedy is a routing decision or a
    line in that person's own config.
    - **The cross-tool half of that test is load-bearing, not decoration.** Class B's causes stop at
      Copilot; a setup gap does not. A committed file declaring **two** servers is the shipped shape, so
      *"every declared server absent"* is **also** exactly what two uninstalled binaries look like — and
      those are **class A**, where installs are precisely the fix. Absent on **both** tools → you are not
      in class B, whatever the breadth says.
    - **Count the file, not your tool list** — with user-scoped servers still loading, *"all of the
      workspace ones"* and *"one server"* look identical from inside a session, and that misreading is the
      one on record.
    - **On Claude, class B does not apply** — neither of its causes affects it — so every declared server
      absent there is a different question (no committed file loaded, malformed JSON, every server
      disabled). **Those three sit outside the three classes**, which is why no remedy class fits them:
      `claude mcp list` is the read that answers it, and you **name the cause you actually found** rather
      than forcing it into A.
  2. **Anything less than every → read the absent entries, one at a time.** This is the branch for **one**
    absent server and equally for **some but not all**: two of three gone is two separate answers, not a
    weaker version of *"every"*. Carrying `"type": "local"` → **class C**: the committed file is
    wrong for your tool, every colleague on that commit sees the identical absence, and no install will
    change it. Its remedy is a change to the repo, and **`claude mcp list` names it in one line**
    (`Skipped — unknown MCP server type "local"`) where your own tool list never will. **Anything else →
    class A**, the setup gap — and *only here* is *"install the binary"* (or start the service, or export
    the variable) the remedy to name.

**Measured
  (2026-08-01): the affected server fails ALONE** — an unset `url` variable produced *"'url' is not a valid
  URL"* for that one server while the others loaded normally.

 **Measured 2026-08-04 on Copilot CLI
  1.0.70**, one field at a time: `${VAR}` in `env`, `args` and `headers` all load fine; only `url` does
  this.

 *(Measured 2026-08-07; CLI version not recorded.)*

Do not rely on an MCP status panel — on 2026-07-10 a panel reported "No MCP servers configured" in a session
where that server's tools were live and working.

## Moved at 0.105.0 tightening

> **Announcing a missing capability must not become the thing that removes one.**

> The causes are **named, never numbered** — you route on the class boundary, not on the
> order, and the counts differ anyway.

> **Nothing in the sections below overrides that order**,
> and the denominator is the part that has actually been got wrong.

> **This is the only class the disclosure template's *"install the binary"* fits**, and it is the class the
> rest of this section exists to stop you defaulting to.

> **Two causes, one fingerprint:** *every* team server missing at once, on Copilot, while the same repo works
> under Claude. That is why they belong together — and why the fingerprint alone does not pick between them.

> **Do not diagnose from WHICH servers survived** — a different trap from the count above, and the one that
> actually bit.

> Nothing varies by
> machine — a colleague on the same commit sees exactly what you see.

> This is the case where *"check your own tool list"* tells you
> something is wrong and cannot tell you why.
