# `agent_handoff` — rationale

**The rule file is what governs**; read this for why it exists.

## The buried hand-off (moved from the rule, 0.178.1)

**Bad (buried — autopilot continues):**
> ...and the next step is updating the config. Run the migration when ready, then we'll start fresh.

The agent keeps going because the instruction was not a hard stop: under an autopilot or a skim-reading user, a
hand-off in the middle of a reply reads as narration, and the next sentence is taken as the plan.
