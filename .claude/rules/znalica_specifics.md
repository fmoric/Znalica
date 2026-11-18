# Znalica Specifics

Facts about this repository that the agent packet does not carry. This file is Znalica's own: the packet ships no
file by this name, so a re-delivery leaves it in place (a re-delivery writes over `AGENTS.md` and
`.claude/CLAUDE.md`, which is why these lines are not kept there).

- **No CI.** The repository has no `.github/workflows`, so the local full EditMode run is the only full-suite run a
  pull request gets. A PR report says so; it never says CI passed.
- **`Unity.exe` is not on PATH on this PC.** The `AGENTS.md` commands call a bare `Unity.exe`; call the editor by its
  full path, `C:\Program Files\Unity\Hub\Editor\6000.3.28f1\Editor\Unity.exe` (the version is in
  `ProjectSettings/ProjectVersion.txt`). Making the commands portable is tracker item T5.
- **Select tests by name.** `-testFilter` takes full class names (`Znalica.Tests.EditMode.<Class>`) joined by `;`.
  The test command with no `-testFilter` is the full suite, and needs the user's yes for that run (`AGENTS.md`), however fast
  it is. Run logs go in `Logs/test-runs`, which git ignores.
- **Some storage behaviour is unverified in an installed player.** That `FileStream.Flush(true)` reaches the disk, and
  that `File.Replace` behaves under Unity's Mono runtime as it does on .NET, are derived from source and not yet
  checked in a built player. Say so wherever a claim rests on them.
- **Before "the only caller", "nothing else uses X" or "unused", query the code graph.** Use graphify
  (`get_neighbors`) or serena (`find_referencing_symbols`) where connected, and name the query in the report. An
  empty result (`{}`) from serena carries no information: fall back to text search and tag the claim
  "text search, a graph query not run". The packet carries this from 0.197.3; this line can go once Znalica is
  re-delivered at that version or later.
