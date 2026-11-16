# `external_tool_state_change` — rationale

Moved verbatim out of the `external_tool_state_change` rule (both mirrors) at 0.105.0, to keep the instruction files every session loads under Claude Code's total limit. **The rule file is what governs**; read this for why it exists and how it composes with the rules around it.

## Why this lands on the agent

Nothing else is positioned to catch it. The repository is unchanged, so no diff, test or review sees
anything. The person who asked described an outcome — *"set up X"* — not a command, and cannot know which
command you are about to compose. You are the only participant who sees both the command and the state it
would overwrite.

## Source

2026-07-31. Asked to **register** an MCP server pointing at an existing Serena installation, an agent ran
`uv tool install --from git+…` — an upgrade nobody requested. The package manager uninstalled the pinned
version, then failed on a directory locked by a **different live session serving another project**, leaving
the tool not installed at all. Six gates were reviewed afterwards and **none fired in time**; the one that
fired at all did so twice, both times after the damage.

Each step above maps to one thing that went wrong: the curated source was connected and unread (1), the tool
was already installed and unchecked (2), another session was using it and was invisible (3), and nothing
required consent before replacing a working install (4).

## Relationship to the neighbouring rules

- **`no_silent_tool_loss` is this rule's other half.** It fires when a capability you expected is **missing**;
  this one fires when you are about to make one missing for someone else. Both `no_silent_tool_loss` and
  `task_assessment` name an install command in their own examples — **naming the remedy is their job, running
  it is this rule's moment.**
- **The `mcp-server-setup` skill is the MCP-specific instance**, carrying the packet's own per-server
  procedure. Every step here still applies there.
- **`discussion_first` is broader and this rule is the specific one, so this one governs.** That rule says to
  confirm before anything outward-facing or hard to reverse, and names no procedure. Where a change is shared
  state, **step 4 is the confirmation decision for it** — including when step 4 deliberately does not fire,
  on a genuinely first-time install with no other live consumer. That is a decision this rule makes, not a
  gap in it: *stop and ask* on every fresh install is exactly the reflex that gets waved through when it
  matters. Steps 1–3 still run.
- **Two rules own acts inside this class and answer them with a refusal rather than a question.**
  `production_is_read_only` owns **going to a system of record or a running environment that names itself
  as production, to write or for data, for work that has not been released**; `no_personal_data_recorded` owns **writing a real
  person's data into anything that persists**, shared state included. Named by the act, not only by the rule.
  Steps 1–3 still run in both cases — what changes is step 4, which those rules replace with a refusal that
  no answer satisfies. Everything else here is unaffected: a **shared dev database**, a **staging**
  environment, a shared cluster that is nobody's production remain stop-and-ask, exactly as before, unless the target
  is known to hold real client data, which is `production_is_read_only`'s (0.171.0).
- **`pr_merge_approval` owns one member of this class in full: landing work on the protected branch** —
  merging into it, pushing or force-pushing to it, however it is spelled. **Named by the act, not only by the
  rule**, and that is deliberate: a hand-off that names only a rule reopens silently the moment that rule
  acquires a condition under which it does not apply. This one did, and for a while nothing gated the merge
  in any project without pull requests. If for any reason that rule does not reach your case, **this rule
  still does** — shared state is shared state, and a delegation is not a licence to stop thinking.

## Moved at 0.105.0 tightening

Passages cut from the rule body because they explain *why* rather than instruct. Verbatim, grouped by the
section they came from.

**From *Rule* — the trigger and steps 1–4:**

> **The trigger, and it is observable rather than a feeling:** *am I about to change state that is shared
> beyond this repository?*

> This is
> `knowledge_before_framework_code` applied to *acting on* a tool rather than *reading about* one.
> **This step has no session cache**: having read about a tool earlier does not cover the command you are
> about to run against it, because what you need here is how *this operation* behaves, not how the tool
> behaves. In the incident in the rationale doc the answer sat one call away, in an article written for that exact tool.

> One read-only call ends most of these
> tasks outright

> This is the check with no natural prompt, so it has to be a step — and it is
> the one that would have caught the incident in the rationale doc, where the fatal detail was another session's lock.

> — and a reinstall performs the same unlink-and-relink that caused the damage below. **"Nothing is being
> upgraded" is not the test.**

> A genuinely first-time install of something absent, with **no other live
> consumer found in step 3**, has nothing to overwrite and nobody to disturb — steps 1–3 apply, step 4 does
> not. A guard that says "stop and ask" on every fresh install is one that gets waved through when it matters.

**From *When this fires*:**

> Not at the start of the task,
> when you do not yet know whether shared state is involved; not after, when it is already gone. That is also
> this guard's blast radius: **it fires before anything has changed**, so a false stop costs one question and a
> missed one is unbounded.

> — and it is
> worse, because it will run on machines you will never see. The file you edit is inside the repository; what
> it does is not.

**From *What this does not fire on*:**

> Nothing outside the repo behaves differently.

> An immutable, content-keyed entry changes no other consumer's
> behaviour.

> Two limits on the provisioning exemption, because this is the part a guard leaks through: [...] The exemption exists so a rule that would fire on every commit in a
> provisioning repo does not get ignored there. Nothing more.

> Do not reach for it because *the task at hand* is about tooling. The incident in the rationale doc was a session whose task
> was registering an MCP server; that is the case this rule exists for, and a task-shaped exemption would have
> released it.

**From *What an upgrade costs even when it works*:**

> Nothing errors — the guidance simply stops matching the tool, and the next
> reader cannot tell.
