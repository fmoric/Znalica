# Leftover agent processes - lists the processes agent tools start that are still running after the shell or session
# that started them is gone: the tree walkers (find, grep, du, rg), Git Bash shells (bash, sh), node (MCP servers,
# dev servers) and the AL language server. REPORT ONLY: it never stops a process. Stopping one can end a process
# the user still wants, and a process started elevated can only be stopped from an elevated shell anyway.
#
# Why: when an agent's shell goes away - a tool timeout, the session ending - Windows does not stop its children.
# Four orphaned `find /` searches on one laptop ran for up to 18 hours and held 13.3 M handles and about 8.6 GB of
# kernel memory (2026-10-15/16); nothing reported them until the machine lagged.
#
# By hand, on either tool:
#   powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/leftover_processes.ps1
# Claude Code also runs it when a session starts (a SessionStart hook, matcher startup, with -Hook): if it finds
# any, the report is shown to the user and handed to the agent; if it finds none, it prints nothing. Copilot CLI
# ignores a sessionStart hook's output, so there it is the `leftover-processes` skill.
#
# "Leftover" = the parent process is gone (or its PID now belongs to a newer process), or the parent is itself a
# leftover, and the process is older than -MinutesOld (default 10). The report groups them by tree: the first
# process of each tree is the one whose parent is gone, and the rest are listed under the process that started them.
# A tree that may still be doing someone's work is listed apart and left out of the stop command: one with a command
# line under a Claude Code session folder (Temp\claude\...), or with a live child outside the watched names - a
# session's background job whose launching shell exited (2026-10-20: another project's live test run, listed as
# leftover with a Stop-Process line that would have ended it). Always exits 0.
#
# Source: the ai-agent-templates developer packet (packets/developer/hooks/search-guard/), search_guard option.
# Tests: tests/test_search_guard.ps1 ([report] cases). Needs Windows PowerShell 5.1 or later. Keep this file ASCII.

# -Also python,postgres adds names to the watch list for this run (0.197.4): a stopped test run's python.exe or
# postgres.exe is not an agent tool, so it is not watched by default, and a by-hand check otherwise needs a CIM query
# that an auto-mode classifier may refuse. ".exe" is added when missing; a name with anything but letters, digits,
# '.', '-' or '_' is skipped. The session-start hook never passes it.
param([switch]$Hook, [int]$MinutesOld = 10, [string[]]$Also = @())

$ErrorActionPreference = 'Stop'
# Run as a script (not dot-sourced by the tests): whatever fails, exit 0. A hook that exits non-zero shows an error
# at every session start (and Copilot CLI, which also runs .claude/settings.json's entries, may treat it as a crash).
# Constrained Language Mode refuses most of what this script calls. Dot-sourced, an error still surfaces.
$ReportRunAsScript = $MyInvocation.InvocationName -ne '.'
trap {
    if ($ReportRunAsScript) { exit 0 }
    break
}
$Watched = @('find.exe', 'grep.exe', 'du.exe', 'rg.exe', 'bash.exe', 'sh.exe', 'node.exe', 'al.exe',
    'Microsoft.Dynamics.Nav.EditorServices.Host.exe')
# Console hosts sit under almost every console process; a child of these names is not a sign of work.
$NotWork = @('conhost.exe', 'OpenConsole.exe')

# The watch list plus -Also's names, normalised: each split on commas, trimmed, ".exe" added, unsafe names dropped
# (they go into a WMI filter), duplicates of a watched name ignored.
function Add-WatchedNames([string[]]$Base, [string[]]$Extra) {
    $out = [System.Collections.Generic.List[string]]::new()
    foreach ($b in $Base) { $out.Add($b) }
    foreach ($e in $Extra) {
        foreach ($n in ($e -split ',')) {
            $n = $n.Trim()
            if (-not $n -or $n -notmatch '^[A-Za-z0-9._-]+$') { continue }
            if ($n -notmatch '(?i)\.exe$') { $n += '.exe' }
            if (-not ($out | Where-Object { $_ -ieq $n })) { $out.Add($n) }
        }
    }
    return @($out)
}
$Watched = Add-WatchedNames $Watched $Also
$SessionFolder = '(?i)[\\/]Temp[\\/]claude[\\/]'

# The classification, kept free of process calls so the tests can feed it any shape.
#   $Candidates  - the watched processes: ProcessId, ParentProcessId, Name, CreationDate (+ what the report prints).
#   $ParentStart - PID -> start time for each candidate's parent that is alive ($null when unreadable). A parent PID
#                  missing from it is gone.
#   $GetAll      - returns every live process (ProcessId, ParentProcessId, Name, CreationDate, CommandLine); called
#                  only when there is a leftover, to see the children the watch list misses.
# Returns one object per tree: Root, Members (root first, then descendants in tree order) and Flag (why the tree may
# be a session's background job, or '').
function Group-LeftoverTrees($Candidates, [hashtable]$ParentStart, [datetime]$Cutoff, [scriptblock]$GetAll) {
    $byId = @{}
    foreach ($c in $Candidates) { $byId[[int]$c.ProcessId] = $c }
    $rootOf = @{}
    foreach ($c in $Candidates) {
        $ppid = [int]$c.ParentProcessId
        $gone = -not $ParentStart.ContainsKey($ppid)
        if (-not $gone) {
            # The PID may have been reused by a newer process - often another bash or node - and a parent cannot
            # start after its child.
            $started = $ParentStart[$ppid]
            if ($started -and $c.CreationDate -and $started -gt $c.CreationDate) { $gone = $true }
        }
        if ($gone) { $rootOf[[int]$c.ProcessId] = [int]$c.ProcessId }
    }
    do {   # children of a leftover are leftovers too (an orphaned bash and the find it started), in the same tree
        $added = $false
        foreach ($c in $Candidates) {
            $id = [int]$c.ProcessId
            $ppid = [int]$c.ParentProcessId
            if (-not $rootOf.ContainsKey($id) -and $rootOf.ContainsKey($ppid)) { $rootOf[$id] = $rootOf[$ppid]; $added = $true }
        }
    } while ($added)
    $roots = @($rootOf.Keys | Where-Object { $rootOf[$_] -eq $_ } | ForEach-Object { $byId[$_] } |
        Where-Object { $_.CreationDate -and $_.CreationDate -lt $Cutoff })
    if ($roots.Count -eq 0) { return @() }

    $all = @(& $GetAll)
    $allById = @{}
    $childrenOf = @{}
    foreach ($p in $all) {
        $allById[[int]$p.ProcessId] = $p
        $ppid = [int]$p.ParentProcessId
        if (-not $childrenOf.ContainsKey($ppid)) { $childrenOf[$ppid] = [System.Collections.Generic.List[object]]::new() }
        $childrenOf[$ppid].Add($p)
    }
    $trees = foreach ($root in $roots) {
        $members = [System.Collections.Generic.List[object]]::new()
        $reasons = [System.Collections.Generic.List[string]]::new()
        $queue = [System.Collections.Generic.Queue[object]]::new()
        $queue.Enqueue($root)
        $seen = @{}
        while ($queue.Count -gt 0) {
            $p = $queue.Dequeue()
            $id = [int]$p.ProcessId
            if ($seen.ContainsKey($id)) { continue }
            $seen[$id] = $true
            $watchedMember = $byId.ContainsKey($id) -and $rootOf.ContainsKey($id)
            if ($watchedMember) {
                $p = $byId[$id]
                if ($p.CreationDate -and $p.CreationDate -lt $Cutoff) { $members.Add($p) }
            }
            $cmd = $p.CommandLine
            if (-not $cmd -and $allById.ContainsKey($id)) { $cmd = $allById[$id].CommandLine }
            if ($cmd -and $cmd -match $SessionFolder -and -not ($reasons -like 'runs from*')) {
                $reasons.Add('runs from a Claude Code session folder (Temp\claude\...)')
            }
            if (-not $watchedMember -and $p.Name -notin $NotWork -and -not ($reasons -like 'has a working child*')) {
                $reasons.Add(('has a working child: {0} (PID {1})' -f $p.Name, $id))
            }
            if ($childrenOf.ContainsKey($id)) {
                foreach ($k in $childrenOf[$id]) {
                    # A child that started before this process belongs to an earlier owner of the reused PID.
                    if ($k.CreationDate -and $p.CreationDate -and $k.CreationDate -lt $p.CreationDate) { continue }
                    $queue.Enqueue($k)
                }
            }
        }
        $handles = 0
        foreach ($m in $members) { $handles += [long]$m.HandleCount }
        [pscustomobject]@{ Root = $root; Members = @($members); Flag = ($reasons -join '; '); Handles = $handles }
    }
    return @($trees | Sort-Object -Property Handles -Descending)
}

function Get-LeftoverTrees([int]$Minutes) {
    $filter = ($Watched | ForEach-Object { "Name='$_'" }) -join ' OR '
    $candidates = @(Get-CimInstance -ClassName Win32_Process -Filter $filter -Property ProcessId, ParentProcessId, Name,
        CreationDate, HandleCount, WorkingSetSize, CommandLine)
    $live = @{}
    foreach ($p in [System.Diagnostics.Process]::GetProcesses()) { $live[$p.Id] = $p }
    $byId = @{}
    foreach ($c in $candidates) { $byId[[int]$c.ProcessId] = $c }
    $parentStart = @{}
    foreach ($c in $candidates) {
        $ppid = [int]$c.ParentProcessId
        if ($live.ContainsKey($ppid) -and -not $parentStart.ContainsKey($ppid)) {
            # Prefer WMI's creation time; .NET's StartTime is refused for elevated processes.
            $started = $null
            try {
                if ($byId.ContainsKey($ppid)) { $started = $byId[$ppid].CreationDate } else { $started = $live[$ppid].StartTime }
            } catch { }
            $parentStart[$ppid] = $started
        }
    }
    $getAll = { @(Get-CimInstance -ClassName Win32_Process -Property ProcessId, ParentProcessId, Name, CreationDate, CommandLine) }
    return @(Group-LeftoverTrees $candidates $parentStart (Get-Date).AddMinutes(-$Minutes) $getAll)
}

function Format-Age([datetime]$Since) {
    $span = (Get-Date) - $Since
    if ($span.TotalHours -ge 1) { return ('{0}h {1}m' -f [int][Math]::Floor($span.TotalHours), $span.Minutes) }
    return ('{0}m' -f [int][Math]::Floor($span.TotalMinutes))
}

function Add-TreeRows($Lines, $Trees) {
    $Lines.Add(('{0,-8} {1,-14} {2,-9} {3,12} {4,10}  {5}' -f 'PID', 'Name', 'Age', 'Handles', 'Memory', 'Command line'))
    foreach ($t in $Trees) {
        foreach ($p in $t.Members) {
            $cmd = $p.CommandLine
            if (-not $cmd) { $cmd = '(not readable from this shell - it may run elevated)' }
            elseif ($cmd.Length -gt 100) { $cmd = $cmd.Substring(0, 97) + '...' }
            $mem = '{0:N0} MB' -f ([double]$p.WorkingSetSize / 1MB)
            $pidText = [string]$p.ProcessId
            if ($p.ProcessId -ne $t.Root.ProcessId) { $pidText = '  ' + $pidText; $cmd = "[under $($p.ParentProcessId)] " + $cmd }
            $Lines.Add(('{0,-8} {1,-14} {2,-9} {3,12:N0} {4,10}  {5}' -f $pidText, $p.Name, (Format-Age $p.CreationDate), $p.HandleCount, $mem, $cmd))
        }
    }
}

function Format-Report($Trees, [int]$Minutes) {
    $plain = @($Trees | Where-Object { -not $_.Flag })
    $flagged = @($Trees | Where-Object { $_.Flag })
    $count = 0
    foreach ($t in $Trees) { $count += $t.Members.Count }
    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add("Leftover agent processes - in each tree the first process lost its parent, and all are older than $Minutes minutes: $($Trees.Count) tree(s), $count process(es)")
    if ($plain.Count -gt 0) {
        $lines.Add('')
        Add-TreeRows $lines $plain
    }
    if ($flagged.Count -gt 0) {
        $lines.Add('')
        $lines.Add("Possibly a session's background job - listed, but left out of any stop command:")
        foreach ($t in $flagged) { $lines.Add("  tree $($t.Root.ProcessId): $($t.Flag)") }
        Add-TreeRows $lines $flagged
    }
    $lines.Add('')
    if ($plain.Count -gt 0) {
        # Stop-Process stops one process, not its tree, so the line names every listed PID of each tree.
        $ids = ($plain | ForEach-Object { $_.Members } | ForEach-Object { $_.ProcessId }) -join ','
        $lines.Add('Nothing was stopped. Check the list first: a node or bash process may be one you still want.')
        $lines.Add('To stop exactly the first list, run this in an elevated PowerShell (needed for any that run elevated):')
        $lines.Add("  Stop-Process -Id $ids -Force")
    } else {
        $lines.Add("Nothing was stopped, and no stop command is given: each tree may be a session's background job.")
        $lines.Add('Stop one only once you know whose work it is.')
    }
    return ($lines -join "`n")
}

function Invoke-LeftoverReport {
    $report = ''
    try {
        if ($Hook) {
            # Copilot CLI also runs the entries in .claude/settings.json (since 1.0.12); there the entry's gate does
            # nothing (no CLAUDE_PROJECT_DIR before 1.0.112, COPILOT_HOOK_DEPTH set from 1.0.112). As a second line, a
            # payload in Copilot's own shape (no hook_event_name) skips the scan: Copilot ignores what a session-start
            # hook prints. 1.0.112 sends a Claude-shaped payload, so this second line no longer catches it there.
            $payload = ''
            try { $payload = [Console]::In.ReadToEnd() } catch { }
            if ($payload -notmatch '"hook_event_name"') { exit 0 }
        }
        $found = @(Get-LeftoverTrees $MinutesOld)
        if ($found.Count -gt 0) { $report = Format-Report $found $MinutesOld }
    } catch {
        try {
            if (-not $Hook) { Write-Output ('Could not list processes: ' + $_.Exception.Message) }
            [Console]::Error.WriteLine('leftover processes: ' + $_.Exception.Message)
        } catch { }
        exit 0
    }

    if ($Hook) {
        if ($report) {
            $context = $report + "`n`nTell the user about these processes in your first reply, with the command above if there is one. " +
                'Do not stop them yourself: some may be theirs or another session''s, and stopping a process changes state beyond this repository.'
            $answer = [ordered]@{
                systemMessage      = $report
                hookSpecificOutput = [ordered]@{ hookEventName = 'SessionStart'; additionalContext = $context }
            }
            $bytes = [System.Text.Encoding]::UTF8.GetBytes((ConvertTo-Json -Compress -Depth 3 -InputObject $answer))
            $out = [Console]::OpenStandardOutput()
            $out.Write($bytes, 0, $bytes.Length)
            $out.Flush()
        }
    } elseif ($report) {
        Write-Output $report
    } else {
        Write-Output "No leftover agent processes: checked $($Watched -join ', ') for a missing parent and an age over $MinutesOld minutes."
    }
}

if ($ReportRunAsScript) {
    Invoke-LeftoverReport
    exit 0
}
