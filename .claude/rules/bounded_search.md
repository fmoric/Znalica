---
paths:
  - "**"
---

# File Searches: Bounded, Never From a Root

## Rule

- **Never search from `/` or `/proc`, and never recurse into a whole drive (`/c`, `C:\`) without a depth limit of
  3 or less.** Search the folder the thing should be in.
- **On Windows, Git Bash's `/` is not the C: drive.** It is Git's install folder plus `/proc/registry`, the whole
  Windows registry. `C:\` is `/c/`: not `/`, and not `/mnt/c` (that is WSL). `find /` never reaches your files.
- **Bound every search: a named folder, a depth limit and a time limit.**
  `timeout 60 find /c/<folder> -maxdepth 6 -name '<pattern>'`, or
  `Get-ChildItem <folder> -Recurse -Depth 6 -Filter <pattern>`. Prefer a built-in file search tool (Glob/Grep) where
  the session has one.
- **Ask the system before you search it.**
  - A PowerShell command or script: `(Get-Command <name>).Source`.
  - A module: `(Get-Module -ListAvailable <name>).ModuleBase`.
  - A NuGet assembly: `%USERPROFILE%\.nuget\packages\<id-lowercase>`.
- **"Not found" from a bounded search is a scoped result.** Name the folder you searched; do not conclude the file
  does not exist (`report_what_you_examined`).
- **A search the project's search guard refused is this rule, enforced.** Change the search as its reason says;
  never rebuild the same search in a form the guard cannot read.
