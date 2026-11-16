@../AGENTS.md

## Claude Code specifics

- Use **plan mode** for changes that touch more than a few files or alter shared infrastructure.
- **Before** the build, delegate the architect's technical spec to the **design-reviewer** subagent
  (`@agent-design-reviewer`) for its A–D grade, and route its findings back to the architect. **It has no
  shell, so hand it the paths** — the technical spec, and the functional spec it came from when one exists.
  **B** or better to build. The `write-tech-spec` skill does this hand-off for you.
- **After** a change, delegate review to the **code-reviewer** subagent (`@agent-code-reviewer`) and route its
  findings back to the developer. **It has no shell, so hand it the diff** — inline for a small change, or
  written to a file whose path you pass — plus the list of changed files. Delegating without them gets you a
  verdict over nothing. A second round covers only what the fixes changed — see `AGENTS.md` → *Gates on each pull
  request*.
- **The two are not interchangeable**, and their names invite the mistake: `design-reviewer` reads documents
  and the codebase before any code exists; `code-reviewer` reads a diff after. Neither can do the other's job,
  and each returns a verdict over nothing if handed the wrong input.
- **Naming a subagent's MCP servers when you delegate** — see `AGENTS.md` → *Delegating to an agent*. An
  agent's `tools:` list is the frontmatter of `.claude/agents/<name>.md`.
- **Starting a subagent that writes in the background** — in the same turn, start its watcher (`AGENTS.md` →
  *Delegating to an agent*) with Bash `run_in_background`, so its exit wakes you. With no Bash tool, run the block
  through Git's own bash from PowerShell, in the background the same way:
  `& (Join-Path (Split-Path (Split-Path (Get-Command git).Source)) 'bin\bash.exe') -c '<block>'`, never a bare
  `bash`, which on Windows can be the WSL launcher; the block holds no single quote, so it passes as written. If
  that `bash.exe` does not exist (git found through `mingw64\bin` or a shim), say the watcher could not start. Set
  `d` to the agent's
  worktree, `b` to its branch, `m` to its budget in minutes (60 if it has none) and `l` to the run log folder its task
  names, relative to `d` (empty if none; a test run changes no files, so without it a run over 10 minutes reads as
  `STALLED`):

  ```sh
  sig() { { git -C "$d" --no-optional-locks status --porcelain -uall; git -C "$d" --no-optional-locks diff HEAD
    git -C "$d" ls-files -oz --exclude-standard | (cd "$d" && xargs -0r git hash-object --)
    [ -z "$l" ] || ls -l "$d/$l"; } 2>/dev/null |
    git hash-object --stdin; }
  git -C "$d" rev-parse --git-dir >/dev/null 2>&1 || { echo "NOREPO $d"; exit 1; }
  h=$(git -C "$d" rev-parse -q --verify "$b") || { echo "NOBRANCH $b"; exit 1; }; s=$(sig); n=0; c=$h; q=0
  t0=$(date +%s)
  while :; do sleep 30; t=$(sig); k=$(git -C "$d" rev-parse -q --verify "$b")
    if [ "$k" != "$c" ]; then c=$k; q=0; n=0; else q=$((q+1)); fi
    [ "$c" != "$h" ] && [ "$q" -ge 4 ] && { echo COMMIT; exit 0; }
    if [ "$t" != "$s" ]; then s=$t; n=0; else n=$((n+1)); fi
    [ "$n" -ge 20 ] && { echo STALLED; exit 0; }
    [ $(($(date +%s) - t0)) -ge $((m * 60)) ] && {
      [ "$c" != "$h" ] && echo COMMIT || echo TIMELIMIT; exit 0; }
  done
  ```

  Arm it once the branch exists: `NOREPO` or `NOBRANCH` means `d` or `b` is wrong. `COMMIT` comes once a new commit
  has landed and no further one for 2 minutes, so commits less than 2 minutes apart are one wake. On every other wake — `COMMIT`,
  `STALLED`, `TIMELIMIT` (at `m` minutes, which catches an agent editing in a loop) or the background timeout —
  check the branch: if the task's final commit is there and rebased (after `git fetch`, `git merge-base
  --is-ancestor origin/<default branch>
  <branch>` succeeds), hand it to its reviewer and re-arm, so an overrunning suite run still wakes you; take that
  run over only as `AGENTS.md` → *Delegating to an agent* says. Otherwise, on `COMMIT` re-arm; on any
  other wake, tell the user what the branch shows before you re-arm. Stop the watcher when the agent reports.
- Roles and standards are defined once in `AGENTS.md` — don't restate them here; keep this file to
  Claude-specific guidance only.
