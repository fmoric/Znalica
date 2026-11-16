---
name: changelog
description: Use when the changelog is missing or behind, or to cut a release (`release` plus the version) — writes it from git history and Conventional Commits, each line linked to its commit.
---

# Changelog

One file, three readers: **people who use a release** want to know what changed for them; **the team**
wants a running record of what landed; **agents** read the newest entries before they work, so they do not
re-learn recent changes by searching. Every line must be checkable, so **every entry links the commit or
pull request it comes from**, and anything no commit says is asked, not written.

**Two modes.** A bare run brings the *Unreleased* section up to date. `release <version>` (version
optional) turns *Unreleased* into a release entry — Step 5.

## 1 — Find what already exists

- **A pointer in `AGENTS.md`** to a changelog or the tool that writes it — follow it, and keep its paths.
- **Release tooling that owns the file** — a `release-please`, `semantic-release`, `changesets` (`.changeset/`),
  `git-cliff` (`cliff.toml`) or `standard-version` configuration, or a script that writes the changelog.
  **Tooling exists** → run its preview or dry-run if it has one, report what it would write, stop. Edit its
  inputs (commit messages, changeset files), never its output.
- **A changelog file** — `CHANGELOG.md`, `CHANGES.md`, `HISTORY.md`, `docs/**/changelog*`. Keep its file, its
  headings and its order; this skill adds entries in the format already there.

Otherwise create `CHANGELOG.md` at the repository root in the [Keep a Changelog](https://keepachangelog.com/)
form: a title, one line saying what the file is, `## [Unreleased]`, then releases newest first.

**Not a git repository** → *not run: no git history to read*. Say so; do not write a changelog from memory.

## 2 — Find the range

Start from the **newest** of these, and say which one you used:

1. The marker this skill keeps directly under `## [Unreleased]`: `<!-- changelog: through <full sha> -->`.
2. The latest version tag (`git describe --tags --abbrev=0`).
3. The last commit that changed the changelog file (`git log -1 --format=%H -- <file>`).

None of them → the whole history; for a long one, ask before writing more than the last release's worth.
Read the range with bodies: `git log --no-merges --format='%H%x09%s%n%b%x1e' <start>..HEAD`. **Count the
commits read** — the report states it.

## 3 — Sort each commit

Use the [Conventional Commits](https://www.conventionalcommits.org/) types (`<type>(<scope>): <subject>`):

| Commit | Goes to |
|---|---|
| `feat` | **Added** |
| `fix` | **Fixed** |
| `perf`, or `refactor` whose body states an effect a user would notice | **Changed** |
| `!` after the type, or a `BREAKING CHANGE:` footer | **Breaking** — first in the section, with the footer's text |
| a commit that removes a feature, setting or command | **Removed** |
| `docs`, `test`, `chore`, `build`, `ci`, and a `refactor` with no stated effect | not listed — **counted** in one line |
| anything not in that format | **Unsorted** — listed for the user to place; never guessed into a section |

**Most of the range is Unsorted** — the history predates Conventional Commits. Do not list hundreds of
lines for the user to place. Say how many there are and ask: sort the last release's worth together, or
start the changelog here (an empty *Unreleased* whose marker is `HEAD`, with one line saying earlier history
is in git) and let the commits from now on fill it.

**A specs repository** (functional specs and processes rather than code): `docs` commits *are* the content.
List them, grouped by the spec or business process they change, with the spec's state where the spec
records one.

## 4 — Write *Unreleased*

One line per change, in words a user can act on: what is different for them, not how it was built. Each
line ends with its commit (short hash, linked when the remote is known) or its pull request; several
commits for one change become one line with all of them. **Use only what the subject and body say** — no
effect, no reason, no issue number that the commit does not state. Then move the marker to `HEAD`.

Already listed (the commit is linked somewhere in the file) → skip it, so a re-run never duplicates.

## 5 — Release mode: `release [version]`

Move every *Unreleased* entry under `## [<version>] — <YYYY-MM-DD>`, newest release first, and leave an
empty `## [Unreleased]` with the marker.

- **The version** comes from the argument or from the project's own version source (`package.json`,
  `pyproject.toml`, `app.json`, a `*.csproj`, …). **Ask** when they disagree, when there is none, or when the
  *Breaking* section is not empty and the version does not say so (a major bump, or the project's own rule).
- **The date** is today's, unless the user gives the release date.
- **This skill writes the changelog only.** It never bumps a version file, creates a tag or pushes; say
  which of those the release still needs.

## Never invent

- **No version or date** that no source gives.
- **No change** that no commit describes, and no wider effect than the commit states. A one-word subject
  (`fix: typo`) gets a one-line entry, not an interpretation.
- **No issue or ticket reference** that the commit does not carry.

## Report

Say which file you wrote (or which tool owns it and what its preview showed), the range and how it was
chosen, the commits read, how many were listed, how many were counted and not listed (by type), and every
**Unsorted** commit with the question it leaves for the user — or, when most of the range was Unsorted,
their count and the choice you asked.
