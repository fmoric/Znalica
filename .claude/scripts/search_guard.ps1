# Search guard - a PreToolUse hook that refuses a file search which walks a filesystem root, the Windows registry or
# a whole drive. Claude Code (.claude/settings.json, matcher Bash|PowerShell) and GitHub Copilot CLI
# (.github/hooks/search-guard.json, matcher bash|powershell|shell) both run this one file.
#
# Why: in Git Bash, `/` is Git's install folder plus /proc/registry - the whole Windows registry - and not the C:
# drive, which is /c. `find / -name x` therefore never reaches C:\, and Git Bash's find holds one handle per registry
# key it visits (about 1,000 a second, measured). When the agent's shell goes away, find keeps running: four such
# searches on one laptop held 13.3 M handles and about 8.6 GB of kernel memory, for up to 18 hours (2026-10-15/16).
#
# Refused, in every simple command of a command line - inside $( ), backticks, bash -c, cmd /c and
# powershell -Command too:
#   - find from /, /proc or below it, at any depth; from a drive root (/c, C:\, /cygdrive/c, /mnt/c) unless
#     -maxdepth is 3 or less;
#   - ls -R, du and grep -r on /, /proc or a drive root; rg there too, unless --max-depth is 3 or less on a drive;
#   - Get-ChildItem (gci, dir, ls) with -Recurse (alias -s) or -Depth on a drive root, unless -Depth is 3 or less -
#     or on a name under a drive root that is not a folder, which PowerShell then searches for from the root;
#   - cmd's dir /s on a drive root or such a name (dir C:\x.dll /s), and where /r on a drive root.
# A cd earlier in the same command line counts (`cd / && find . -name x`). Everything else passes untouched, and
# text that only mentions a search (echo "find / ...", a heredoc body, a commit message) is not a command.
#
# Contract: reads the hook payload on stdin; prints a deny decision in the calling tool's format, or nothing.
# ALWAYS exits 0: Copilot CLI treats any other exit code as a crashed hook and denies the call. If the guard itself
# fails on a command that names a search tool, it answers "ask" rather than passing it silently.
# Not a security boundary: a command built in a variable, a script file, or a program that walks the disk on its own
# is not seen. It stops the habit, not a determined agent.
#
# Source: the ai-agent-templates developer packet (packets/developer/hooks/search-guard/), search_guard option.
# Tests: tests/test_search_guard.ps1. Needs Windows PowerShell 5.1 or later and nothing else. Keep this file ASCII.

$ErrorActionPreference = 'Stop'

# Run as a hook (not dot-sourced by the tests): whatever fails, exit 0. Copilot CLI denies every call of a hook that
# exits non-zero, so a guard that cannot run - a PowerShell in Constrained Language Mode, say - must pass commands
# rather than block all of them. Dot-sourced, an error still surfaces, so a test sees it.
$GuardRunAsHook = $MyInvocation.InvocationName -ne '.'
trap {
    if ($GuardRunAsHook) { exit 0 }
    break
}

# The only Copilot CLI tools this guard judges; every other tool passes through untouched. The Copilot
# registration's `matcher` (.github/hooks/search-guard.json) names exactly these, because Copilot starts a
# preToolUse hook with no matcher before EVERY tool call. tools/health_check.py Check 20 compares the two.
$CopilotShellTools = @('bash', 'powershell', 'shell')
$MaxDriveDepth = 3

# A command line that names none of these cannot start a search this guard judges, so it is not parsed at all.
$SearchNames = [regex]'(?i)(^|[^\w.-])(find|ls|dir|vdir|du|e?grep|fgrep|rgrep|rg|where|gci|get-childitem|cmd|bash|sh|zsh|dash|ksh|wsl|powershell|pwsh|iex|invoke-expression|xargs)(\.exe)?($|[^\w-])'
$PosixSpecials = [char[]]@(' ', "`t", "`r", "`n", '\', "'", '"', '$', '`', '(', ')', ';', '&', '|', '<', '>', '#')
$QuotedSpecials = [char[]]@('"', '\', '$', '`')
$HeredocDelimiter = [regex]'\G([''"]?)([A-Za-z_][^\s;|&<>()''"\\]*)\1'   # a word, not the 2 of arithmetic 1<<2

function New-GuardContext {
    $h = $env:USERPROFILE
    if (-not $h) { $h = $env:HOME }
    if (-not $h) { $h = 'C:\Users\unknown' }
    return [pscustomobject]@{
        Findings = [System.Collections.Generic.List[object]]::new()
        HomeDir  = ($h -replace '\\', '/')
        Depth    = 0
    }
}

# ------------------------------------------------------------------------------------------------ paths

function Resolve-GuardSegments([string]$Rest) {
    $out = [System.Collections.Generic.List[string]]::new()
    foreach ($s in $Rest.Split('/')) {
        if ($s -eq '' -or $s -eq '.') { continue }
        if ($s -eq '..') {
            if ($out.Count -gt 0) { $out.RemoveAt($out.Count - 1) }
            continue
        }
        $out.Add($s)
    }
    return ,($out.ToArray())
}

function Join-GuardDrive([string]$Drive, [string]$Rest) {
    $segs = Resolve-GuardSegments $Rest
    return ($Drive.ToLowerInvariant() + ':/' + ($segs -join '/'))
}

function Expand-GuardHome([string]$P, $Ctx) {
    if ($P -eq '~' -or $P -eq '$HOME' -or $P -eq '${HOME}') { return $Ctx.HomeDir }
    foreach ($pre in @('~/', '~\', '$HOME/', '$HOME\', '${HOME}/', '${HOME}\')) {
        if ($P.StartsWith($pre)) { return $Ctx.HomeDir + '/' + $P.Substring($pre.Length) }
    }
    return $P
}

function Expand-GuardEnv([string]$S) {
    # $env:NAME, ${env:NAME} and %NAME% become their values; anything unknown stays as written.
    return [regex]::Replace($S, '(?i)\$\{env:([^}]+)\}|\$env:([A-Za-z_][A-Za-z0-9_]*)|%([A-Za-z_][A-Za-z0-9_()]*)%', {
        param($m)
        $name = $m.Groups[1].Value
        if (-not $name) { $name = $m.Groups[2].Value }
        if (-not $name) { $name = $m.Groups[3].Value }
        $v = [Environment]::GetEnvironmentVariable($name)
        if ($null -ne $v) { $v } else { $m.Value }
    })
}

function ConvertTo-PosixGuardPath([string]$P, [string]$Cwd, $Ctx) {
    # A path as Git Bash, MSYS tools (find, ls, du, grep) and rg read it -> 'c:/a/b' for a drive path, '/a/b' for a
    # path inside Git's own tree (/ is that tree's root), or $null when it cannot be known (a variable, $( )).
    if ([string]::IsNullOrEmpty($P)) { return $null }
    $P = Expand-GuardHome $P $Ctx
    if ($P.Contains('$') -or $P.Contains('`')) { return $null }
    $P = $P.Replace('\', '/')
    if ($P -match '^([A-Za-z]):(.*)$') { return (Join-GuardDrive $Matches[1] $Matches[2]) }
    if ($P.StartsWith('/')) {
        $segs = Resolve-GuardSegments $P
        if ($segs.Count -ge 1 -and $segs[0] -match '^[A-Za-z]$') {
            return (Join-GuardDrive $segs[0] (($segs | Select-Object -Skip 1) -join '/'))
        }
        if ($segs.Count -ge 2 -and ($segs[0] -eq 'cygdrive' -or $segs[0] -eq 'mnt') -and $segs[1] -match '^[A-Za-z]$') {
            return (Join-GuardDrive $segs[1] (($segs | Select-Object -Skip 2) -join '/'))
        }
        return ('/' + ($segs -join '/'))
    }
    if (-not $Cwd) { return $null }
    return (ConvertTo-PosixGuardPath ($Cwd + '/' + $P) $null $Ctx)
}

function ConvertTo-WindowsGuardPath([string]$P, [string]$Cwd, $Ctx) {
    # A path as PowerShell, cmd and where.exe read it: \ or / alone is the current drive's root.
    if ([string]::IsNullOrEmpty($P)) { return $null }
    $P = $P.Trim()
    $P = $P -replace '^(?i)(Microsoft\.PowerShell\.Core\\)?FileSystem::', ''
    # The registry provider: HKLM:\ and HKCU:\ (its default drives) and Registry::<hive>, canonical 'registry:/<hive>'.
    if ($P -match '^(?i)(?:Microsoft\.PowerShell\.Core\\)?Registry::(.*)$') { $reg = $Matches[1] }
    elseif ($P -match '^(?i)HKLM:(.*)$') { $reg = 'HKEY_LOCAL_MACHINE\' + $Matches[1] }   # HKLM:Software is HKLM:\Software
    elseif ($P -match '^(?i)HKCU:(.*)$') { $reg = 'HKEY_CURRENT_USER\' + $Matches[1] }
    else { $reg = $null }
    if ($null -ne $reg) {
        $segs = @($reg.Replace('\', '/').Split('/') | Where-Object { $_ -ne '' })
        return ('registry:/' + ($segs -join '/').ToLowerInvariant())
    }
    $P = Expand-GuardEnv $P
    $P = Expand-GuardHome $P $Ctx
    if ($P.Contains('$') -or $P.Contains('%')) { return $null }
    if ($P.StartsWith('\\?\')) { $P = $P.Substring(4) }
    if ($P.StartsWith('\\') -or $P.StartsWith('//')) { return $null }
    $P = $P.Replace('\', '/')
    if ($P -match '^([A-Za-z]):(.*)$') { return (Join-GuardDrive $Matches[1] $Matches[2]) }
    if ($P.StartsWith('/')) {
        $drive = 'c'
        if ($Cwd -and $Cwd -match '^([a-z]):') { $drive = $Matches[1] }
        elseif ($env:SystemDrive) { $drive = $env:SystemDrive.Substring(0, 1) }
        return (Join-GuardDrive $drive $P)
    }
    if (-not $Cwd -or $Cwd.StartsWith('/')) { return $null }
    return (ConvertTo-WindowsGuardPath ($Cwd + '/' + $P) $null $Ctx)
}

function Get-GuardRootKind([string]$C) {
    # 'root' (Git Bash's / - Git's folder plus the registry), 'proc' (/proc: the registry and process tables),
    # 'drive' (a whole drive), or $null. A wildcard directly under a root counts as that root (/*, C:\*).
    if (-not $C) { return $null }
    if ($C -eq '/') { return 'root' }
    if ($C -match '^/proc(/|$)') { return 'proc' }
    if ($C -match '^/[^/]*[*?\[][^/]*$') { return 'root' }
    if ($C -match '^[a-z]:/$') { return 'drive' }
    if ($C -match '^[a-z]:/[^/]*[*?\[][^/]*$') { return 'drive' }
    if ($C -match '^registry:/[^/]*$') { return 'registry' }   # all hives, or one hive's root (HKLM:\)
    return $null
}

function Add-GuardFinding($Ctx, [string]$Tool, [string]$Path, [string]$Kind) {
    foreach ($f in $Ctx.Findings) {
        if ($f.Tool -eq $Tool -and $f.Path -eq $Path -and $f.Kind -eq $Kind) { return }
    }
    $Ctx.Findings.Add([pscustomobject]@{ Tool = $Tool; Path = $Path; Kind = $Kind })
}

function Test-GuardPaths($Ctx, [string]$Tool, [string[]]$Paths, [string]$Flavor, [string]$Cwd, $Depth, [bool]$DepthCounts, [bool]$LeafIsName = $false) {
    # / and /proc never pass. A whole drive passes only for a command whose depth limit counts ($DepthCounts) and
    # sets one of $MaxDriveDepth or less. $LeafIsName: the command searches a path's PARENT for a leaf that does not
    # exist (Get-ChildItem -Recurse, dir /s), so C:\x.dll walks all of C:.
    foreach ($p in $Paths) {
        if ($Flavor -eq 'windows') { $c = ConvertTo-WindowsGuardPath $p $Cwd $Ctx }
        else { $c = ConvertTo-PosixGuardPath $p $Cwd $Ctx }
        $kind = Get-GuardRootKind $c
        if (-not $kind -and $LeafIsName -and (Test-GuardLeafAtDriveRoot $c)) { $kind = 'drive' }
        if (-not $kind) { continue }
        if (($kind -eq 'drive' -or $kind -eq 'registry') -and $DepthCounts -and $null -ne $Depth -and $Depth -le $MaxDriveDepth) { continue }
        Add-GuardFinding $Ctx $Tool $p $kind
    }
}

function Test-GuardLeafAtDriveRoot([string]$C) {
    # True for a canonical 'x:/name' that is neither a folder nor a file on this machine (the hook runs where the
    # command will). A wildcard name is already a root to Get-GuardRootKind.
    if (-not $C -or $C -notmatch '^[a-z]:/[^/*?\[]+$') { return $false }
    try { return -not ([System.IO.Directory]::Exists($C) -or [System.IO.File]::Exists($C)) } catch { return $true }
}

function ConvertTo-GuardDepth([string]$V) {
    if ($V -match '^\d+$') { return [int]$V }
    return 999   # a limit that cannot be read is no limit
}

# -------------------------------------------------------------------------------------------- the searches

function Test-GuardFind($Ctx, [string[]]$A, [string]$Cwd) {
    $i = 0; $n = $A.Count
    while ($i -lt $n) {
        $t = $A[$i]
        if ($t -ceq '-H' -or $t -ceq '-L' -or $t -ceq '-P') { $i++; continue }
        if ($t -ceq '-D') { $i += 2; continue }
        if ($t -cmatch '^-O\d*$') { $i++; continue }
        if ($t -ceq '--') { $i++ }   # end of options: find -- / searches /
        break
    }
    $paths = [System.Collections.Generic.List[string]]::new()
    while ($i -lt $n) {
        $t = $A[$i]
        if ($t.StartsWith('-') -or $t -eq '(' -or $t -eq '!' -or $t -eq ')' -or $t -eq ',') { break }
        $paths.Add($t); $i++
    }
    $depth = $null
    for ($j = $i; $j -lt $n - 1; $j++) {
        if ($A[$j] -ceq '-maxdepth') { $depth = ConvertTo-GuardDepth $A[$j + 1] }
    }
    if ($paths.Count -eq 0) { $paths.Add('.') }
    Test-GuardPaths $Ctx 'find' $paths.ToArray() 'posix' $Cwd $depth $true
}

function Test-GuardGnuFind([string]$Raw, [string[]]$A) {
    # From PowerShell or cmd, a bare `find` is Windows' find.exe (a text filter: find /i "x" file) unless Git's
    # usr\bin comes first on PATH. Judge it as GNU find only when it is named by that path or uses GNU find's words.
    if ($Raw -match '(?i)[\\/]usr[\\/]bin[\\/]|[\\/]git[\\/]') { return $true }
    foreach ($t in $A) {
        if ($t -cmatch '^-(name|iname|path|ipath|wholename|iwholename|regex|iregex|type|maxdepth|mindepth|newer|mtime|mmin|size|user|perm|exec|execdir|print|print0|printf|fprint|delete|prune|empty|lname|ilname)$') {
            return $true
        }
    }
    return $false
}

function Test-GuardLs($Ctx, [string]$Tool, [string[]]$A, [string]$Cwd) {
    $rec = $false; $opts = $true
    $paths = [System.Collections.Generic.List[string]]::new()
    for ($i = 0; $i -lt $A.Count; $i++) {
        $t = $A[$i]
        if ($opts -and $t -eq '--') { $opts = $false; continue }
        if ($opts -and $t.StartsWith('--')) { if ($t -ceq '--recursive') { $rec = $true }; continue }
        if ($opts -and $t -cmatch '^-[A-Za-z0-9]+$') {
            if ($t.Contains('R')) { $rec = $true }
            if ($t -cmatch '^-[A-Za-z0-9]*[ITw]$') { $i++ }   # -I PATTERN, -T COLS, -w COLS take the next word
            continue
        }
        $paths.Add($t)
    }
    if (-not $rec) { return }
    if ($paths.Count -eq 0) { $paths.Add('.') }
    Test-GuardPaths $Ctx ($Tool + ' -R') $paths.ToArray() 'posix' $Cwd $null $false
}

function Test-GuardDu($Ctx, [string[]]$A, [string]$Cwd) {
    # du walks everything below its paths whatever depth it prints, so no limit makes a root acceptable.
    $opts = $true
    $paths = [System.Collections.Generic.List[string]]::new()
    for ($i = 0; $i -lt $A.Count; $i++) {
        $t = $A[$i]
        if ($opts -and $t -eq '--') { $opts = $false; continue }
        if ($opts -and $t.StartsWith('-') -and $t.Length -gt 1) {
            if ($t -cmatch '^-[A-Za-z]*[BdtX]$') { $i++ }
            continue
        }
        $paths.Add($t)
    }
    if ($paths.Count -eq 0) { $paths.Add('.') }
    Test-GuardPaths $Ctx 'du' $paths.ToArray() 'posix' $Cwd $null $false
}

function Test-GuardGrep($Ctx, [string]$Tool, [string[]]$A, [string]$Cwd) {
    $rec = ($Tool -eq 'rgrep'); $patternGiven = $false; $opts = $true
    $pos = [System.Collections.Generic.List[string]]::new()
    $longWithValue = @('--regexp', '--file', '--max-count', '--after-context', '--before-context', '--context',
        '--label', '--binary-files', '--devices', '--directories', '--include', '--exclude', '--exclude-from',
        '--exclude-dir', '--group-separator')
    for ($i = 0; $i -lt $A.Count; $i++) {
        $t = $A[$i]
        if ($opts -and $t -eq '--') { $opts = $false; continue }
        if ($opts -and $t.StartsWith('--')) {
            $name = $t; $val = $null
            $eq = $t.IndexOf('=')
            if ($eq -gt 0) { $name = $t.Substring(0, $eq); $val = $t.Substring($eq + 1) }
            if ($name -ceq '--recursive' -or $name -ceq '--dereference-recursive') { $rec = $true; continue }
            if ($longWithValue -ccontains $name) {
                if ($null -eq $val -and $i + 1 -lt $A.Count) { $i++; $val = $A[$i] }
                if ($name -ceq '--regexp' -or $name -ceq '--file') { $patternGiven = $true }
                if ($name -ceq '--directories' -and $val -ceq 'recurse') { $rec = $true }
            }
            continue
        }
        if ($opts -and $t -match '^-\d+$') { continue }   # -5: context lines
        if ($opts -and $t.StartsWith('-') -and $t.Length -gt 1) {
            $cl = $t.Substring(1)
            for ($k = 0; $k -lt $cl.Length; $k++) {
                $ch = [string]$cl[$k]
                if ($ch -ceq 'r' -or $ch -ceq 'R') { $rec = $true; continue }
                if ('efmABCdD'.Contains($ch)) {   # takes a value: the rest of the word, or the next word
                    $val = $cl.Substring($k + 1)
                    if (-not $val -and $i + 1 -lt $A.Count) { $i++; $val = $A[$i] }
                    if ($ch -ceq 'e' -or $ch -ceq 'f') { $patternGiven = $true }
                    if ($ch -ceq 'd' -and $val -ceq 'recurse') { $rec = $true }
                    break
                }
            }
            continue
        }
        $pos.Add($t)
    }
    if (-not $rec) { return }
    $paths = @($pos.ToArray())
    if (-not $patternGiven -and $paths.Count -gt 0) { $paths = @($paths | Select-Object -Skip 1) }
    if ($paths.Count -eq 0) { $paths = @('.') }
    Test-GuardPaths $Ctx ($Tool + ' -r') $paths 'posix' $Cwd $null $false
}

function Test-GuardRg($Ctx, [string[]]$A, [string]$Cwd) {
    # ripgrep recurses by default; -d / --max-depth is its depth limit.
    $patternGiven = $false; $filesOnly = $false; $depth = $null; $opts = $true
    $pos = [System.Collections.Generic.List[string]]::new()
    $shortWithValue = 'efgtTmMABCErjd'
    $longWithValue = @('--regexp', '--file', '--glob', '--iglob', '--type', '--type-not', '--max-count', '--max-columns',
        '--max-filesize', '--after-context', '--before-context', '--context', '--encoding', '--ignore-file',
        '--replace', '--sort', '--sortr', '--threads', '--type-add', '--type-clear', '--color', '--colors',
        '--path-separator', '--pre', '--pre-glob', '--field-match-separator', '--field-context-separator',
        '--context-separator', '--dfa-size-limit', '--regex-size-limit', '--engine', '--hostname-bin',
        '--hyperlink-format', '--max-depth', '--maxdepth', '--generate')
    for ($i = 0; $i -lt $A.Count; $i++) {
        $t = $A[$i]
        if ($opts -and $t -eq '--') { $opts = $false; continue }
        if ($opts -and $t.StartsWith('--')) {
            $name = $t; $val = $null
            $eq = $t.IndexOf('=')
            if ($eq -gt 0) { $name = $t.Substring(0, $eq); $val = $t.Substring($eq + 1) }
            if ($name -ceq '--files') { $filesOnly = $true; continue }
            if ($longWithValue -ccontains $name) {
                if ($null -eq $val -and $i + 1 -lt $A.Count) { $i++; $val = $A[$i] }
                if ($name -ceq '--regexp' -or $name -ceq '--file') { $patternGiven = $true }
                if ($name -ceq '--max-depth' -or $name -ceq '--maxdepth') { $depth = ConvertTo-GuardDepth $val }
            }
            continue
        }
        if ($opts -and $t.StartsWith('-') -and $t.Length -gt 1) {
            $cl = $t.Substring(1)
            for ($k = 0; $k -lt $cl.Length; $k++) {
                $ch = [string]$cl[$k]
                if ($shortWithValue.Contains($ch)) {
                    $val = $cl.Substring($k + 1)
                    if (-not $val -and $i + 1 -lt $A.Count) { $i++; $val = $A[$i] }
                    if ($ch -ceq 'e' -or $ch -ceq 'f') { $patternGiven = $true }
                    if ($ch -ceq 'd') { $depth = ConvertTo-GuardDepth $val }
                    break
                }
            }
            continue
        }
        $pos.Add($t)
    }
    $paths = @($pos.ToArray())
    if (-not $patternGiven -and -not $filesOnly -and $paths.Count -gt 0) { $paths = @($paths | Select-Object -Skip 1) }
    if ($paths.Count -eq 0) { $paths = @('.') }
    Test-GuardPaths $Ctx 'rg' $paths 'posix' $Cwd $depth $true
}

function Test-GuardChildItem($Ctx, [string[]]$A, [string]$Cwd) {
    # Get-ChildItem by PowerShell's rules: a parameter may be shortened to any unambiguous prefix, -s is an alias of
    # -Recurse (Windows PowerShell 5.1; PowerShell 7 adds -r), and -Depth implies -Recurse. With recursion, a path
    # whose leaf does not exist is searched for under its parent: C:\x.dll walks the whole drive.
    $valueParams = @('path', 'literalpath', 'filter', 'include', 'exclude', 'attributes', 'erroraction',
        'errorvariable', 'warningaction', 'warningvariable', 'informationaction', 'informationvariable',
        'outvariable', 'outbuffer', 'pipelinevariable')
    $valueAliases = @('lp', 'pspath', 'ea', 'ev', 'wa', 'wv', 'infa', 'iv', 'ov', 'ob', 'pv')
    $recurse = $false; $depth = $null
    $paths = [System.Collections.Generic.List[string]]::new()
    $pos = [System.Collections.Generic.List[string]]::new()
    for ($i = 0; $i -lt $A.Count; $i++) {
        $t = $A[$i]
        if ($t -match '^-([A-Za-z]+)(:(.*))?$') {
            $name = $Matches[1].ToLowerInvariant(); $hasVal = [bool]$Matches[2]; $val = $Matches[3]
            if ($name -eq 's' -or 'recurse'.StartsWith($name)) {
                $recurse = -not ($hasVal -and $val -match '^(?i)\$?(false|0)$')
                continue
            }
            if ($name.Length -ge 2 -and 'depth'.StartsWith($name)) {
                if (-not $hasVal -and $i + 1 -lt $A.Count) { $i++; $val = $A[$i] }
                $depth = ConvertTo-GuardDepth $val
                continue
            }
            $isPath = ($name -eq 'lp') -or ($name -eq 'pspath') -or 'path'.StartsWith($name) -or 'literalpath'.StartsWith($name)
            $takes = $isPath -or ($valueAliases -contains $name)
            if (-not $takes) {
                foreach ($v in $valueParams) { if ($v.StartsWith($name)) { $takes = $true; break } }
            }
            if ($takes -and -not $hasVal -and $i + 1 -lt $A.Count) { $i++; $val = $A[$i] }
            if ($isPath -and $val) { foreach ($v in $val.Split(',')) { if ($v) { $paths.Add($v) } } }
            continue
        }
        $pos.Add($t)
    }
    if ($paths.Count -eq 0 -and $pos.Count -gt 0) { foreach ($v in $pos[0].Split(',')) { if ($v) { $paths.Add($v) } } }
    if (-not $recurse -and $null -eq $depth) { return }
    if ($paths.Count -eq 0) { $paths.Add('.') }
    Test-GuardPaths $Ctx 'Get-ChildItem -Recurse' $paths.ToArray() 'windows' $Cwd $depth $true $true
}

function Test-GuardDirSpecAtRoot([string]$S, [string]$Cwd, $Ctx) {
    # cmd's `dir <spec> /s` lists the spec's folder and everything below it; a spec that is not a folder is a name
    # searched for from the folder above it. So a drive (C:, C:\), \, or a name directly under a drive root that is
    # not a folder (C:\x.dll, C:\*.al) starts at the root - C:\bcartifacts.cache, a folder, does not.
    $s = (Expand-GuardEnv $S.Trim('"')).Replace('/', '\')
    if ($s -match '^[A-Za-z]:\\?$' -or $s -eq '\' -or ($s -eq '.' -and $Cwd -match '^[a-z]:/$')) { return $true }
    if ($s -match '^[A-Za-z]:\\?[^\\]+$' -or $s -match '^\\[^\\]+$' -or ($s -notmatch '[\\:]' -and $Cwd -match '^[a-z]:/$')) {
        $c = ConvertTo-WindowsGuardPath ($s -replace '^([A-Za-z]):(?!\\)', '$1:\') $Cwd $Ctx
        return ((Get-GuardRootKind $c) -eq 'drive' -or (Test-GuardLeafAtDriveRoot $c))
    }
    return $false
}

function Test-GuardCmdDir($Ctx, [string[]]$A, [string]$Cwd) {
    $rec = $false
    $specs = [System.Collections.Generic.List[string]]::new()
    foreach ($t in $A) {
        $w = $t; $sw = ''
        $k = $w.IndexOf('/')
        if ($k -ge 0) { $sw = $w.Substring($k); $w = $w.Substring(0, $k) }
        foreach ($s in $sw.Split('/')) { if ($s -ieq 's') { $rec = $true } }
        if ($w) { $specs.Add($w) }
    }
    if (-not $rec) { return }
    if ($specs.Count -eq 0) { $specs.Add('.') }
    foreach ($s in $specs) {
        if (Test-GuardDirSpecAtRoot $s $Cwd $Ctx) { Add-GuardFinding $Ctx 'dir /s' $s 'drive' }
    }
}

function Test-GuardWhere($Ctx, [string[]]$A, [string]$Cwd) {
    for ($i = 0; $i -lt $A.Count - 1; $i++) {
        if ($A[$i] -match '^(/|//|-)[rR]$') {
            Test-GuardPaths $Ctx 'where /r' @($A[$i + 1]) 'windows' $Cwd $null $false
        }
    }
}

# ------------------------------------------------------------------------------------------- command lines

function New-GuardTokenState {
    return [pscustomobject]@{
        Cmds     = [System.Collections.Generic.List[object]]::new()
        Words    = [System.Collections.Generic.List[string]]::new()
        Sb       = [System.Text.StringBuilder]::new()
        InWord   = $false
        SkipNext = $false
    }
}

function Add-GuardText($St, [string]$S) {
    $null = $St.Sb.Append($S)
    $St.InWord = $true
}

function Complete-GuardWord($St) {
    if ($St.InWord) {
        if ($St.SkipNext) { $St.SkipNext = $false }
        else { $St.Words.Add($St.Sb.ToString()) }
    }
    $null = $St.Sb.Clear()
    $St.InWord = $false
}

function Complete-GuardCommand($St) {
    Complete-GuardWord $St
    if ($St.Words.Count -gt 0) {
        $St.Cmds.Add([string[]]$St.Words.ToArray())
        $St.Words.Clear()
    }
    $St.SkipNext = $false
}

$GuardSubshellIn = [string][char]0 + 'subshell-in'
$GuardSubshellOut = [string][char]0 + 'subshell-out'

function Add-GuardNested($St, [string]$Body) {
    # Commands inside $( ), backticks or ( ) run before the command around them, so they are queued now. They run in
    # a subshell: the markers around them let Invoke-GuardText restore the working directory after them, so the cd
    # in (cd / && ls); find . -name x does not move the find.
    $St.Cmds.Add([string[]]@($GuardSubshellIn))
    foreach ($w in (Get-GuardPosixCommands $Body)) { $St.Cmds.Add($w) }
    $St.Cmds.Add([string[]]@($GuardSubshellOut))
}

function Find-GuardClose([string]$T, [int]$Open) {
    # $T[$Open] is '('. Returns the index of the matching ')', or -1. Quotes and backslashes are honoured.
    $depth = 0; $i = $Open; $n = $T.Length
    while ($i -lt $n) {
        $c = $T[$i]
        if ($c -eq "'") {
            $j = $T.IndexOf("'", $i + 1)
            if ($j -lt 0) { return -1 }
            $i = $j + 1; continue
        }
        if ($c -eq '"') {
            $i++
            while ($i -lt $n -and $T[$i] -ne '"') { if ($T[$i] -eq '\') { $i++ }; $i++ }
            $i++; continue
        }
        if ($c -eq '\') { $i += 2; continue }
        if ($c -eq '(') { $depth++ }
        elseif ($c -eq ')') { $depth--; if ($depth -eq 0) { return $i } }
        $i++
    }
    return -1
}

function Find-GuardBacktick([string]$T, [int]$Open) {
    $i = $Open + 1
    while ($i -lt $T.Length) {
        if ($T[$i] -eq '\') { $i += 2; continue }
        if ($T[$i] -eq '`') { return $i }
        $i++
    }
    return -1
}

function Skip-GuardHeredoc([string]$T, [int]$I, [string]$Delim, [bool]$Strip) {
    # A heredoc body is data, never commands: skip its lines up to and including the delimiter line.
    $n = $T.Length
    while ($I -lt $n) {
        $e = $T.IndexOf("`n", $I)
        if ($e -lt 0) { $e = $n }
        $line = $T.Substring($I, $e - $I).TrimEnd("`r")
        if ($Strip) { $line = $line.TrimStart("`t") }
        $I = $e + 1
        if ($line -ceq $Delim) { return [Math]::Min($I, $n) }
    }
    return $n
}

function Read-GuardDoubleQuoted([string]$T, [int]$I, $St) {
    # $I is just after an opening ". Appends the content to the current word, queues the commands of any $( ) or
    # backtick inside it, and returns the index after the closing quote.
    $n = $T.Length
    $St.InWord = $true
    while ($I -lt $n) {
        $k = $T.IndexOfAny($QuotedSpecials, $I)
        if ($k -lt 0) { Add-GuardText $St $T.Substring($I); return $n }
        if ($k -gt $I) { Add-GuardText $St $T.Substring($I, $k - $I) }
        $c = $T[$k]
        if ($c -eq '"') { return ($k + 1) }
        if ($c -eq '\') {
            if ($k + 1 -lt $n) {
                $nx = [string]$T[$k + 1]
                if ($nx -eq '"' -or $nx -eq '\' -or $nx -eq '$' -or $nx -eq '`') { Add-GuardText $St $nx }
                elseif ($nx -ne "`n") { Add-GuardText $St ('\' + $nx) }
            }
            $I = $k + 2; continue
        }
        if ($c -eq '$' -and $k + 1 -lt $n -and $T[$k + 1] -eq '(') {
            $j = Find-GuardClose $T ($k + 1)
            if ($j -lt 0) { $j = $n - 1 }
            if (-not ($k + 2 -lt $n -and $T[$k + 2] -eq '(')) { Add-GuardNested $St $T.Substring($k + 2, [Math]::Max(0, $j - $k - 2)) }
            Add-GuardText $St $T.Substring($k, $j - $k + 1)
            $I = $j + 1; continue
        }
        if ($c -eq '`') {
            $j = Find-GuardBacktick $T $k
            if ($j -lt 0) { $j = $n }
            Add-GuardNested $St $T.Substring($k + 1, [Math]::Max(0, [Math]::Min($j, $n) - $k - 1))
            Add-GuardText $St '`'
            $I = $j + 1; continue
        }
        Add-GuardText $St ([string]$c)
        $I = $k + 1
    }
    return $n
}

function Get-GuardPosixCommands([string]$Text) {
    # Splits a Git Bash command line into simple commands (word arrays, quotes removed) on ; & && | || and new
    # lines. Commands inside $( ), backticks, ( ) and <( ) are returned too; heredoc bodies, comments and
    # redirection targets are dropped.
    $st = New-GuardTokenState
    $here = [System.Collections.Generic.List[object]]::new()
    $T = $Text; $n = $T.Length; $i = 0
    while ($i -lt $n) {
        $k = $T.IndexOfAny($PosixSpecials, $i)
        if ($k -lt 0) { Add-GuardText $st $T.Substring($i); break }
        if ($k -gt $i) { Add-GuardText $st $T.Substring($i, $k - $i); $i = $k }
        $c = $T[$i]
        if ($c -eq ' ' -or $c -eq "`t" -or $c -eq "`r") { Complete-GuardWord $st; $i++; continue }
        if ($c -eq "`n") {
            Complete-GuardCommand $st
            $i++
            foreach ($h in $here) {
                $start = $i
                $i = Skip-GuardHeredoc $T $i $h.Delim $h.Strip
                if ($h.Shell) {
                    # bash <<'EOF' ... EOF: the body is the script that shell runs, so its commands are judged.
                    $body = $T.Substring($start, [Math]::Max(0, $i - $start))
                    $cut = $body.LastIndexOf($h.Delim)
                    if ($cut -ge 0) { $body = $body.Substring(0, $cut) }
                    Add-GuardNested $st $body
                }
            }
            $here.Clear()
            continue
        }
        if ($c -eq '#') {
            if ($st.InWord) { Add-GuardText $st '#'; $i++; continue }
            $j = $T.IndexOf("`n", $i)
            if ($j -lt 0) { $i = $n } else { $i = $j }
            continue
        }
        if ($c -eq '\') {
            if ($i + 1 -ge $n) { $i++; continue }
            $nx = $T[$i + 1]
            if ($nx -eq "`n") { $i += 2; continue }
            if ($nx -eq "`r" -and $i + 2 -lt $n -and $T[$i + 2] -eq "`n") { $i += 3; continue }
            Add-GuardText $st ([string]$nx); $i += 2; continue
        }
        if ($c -eq "'") {
            $j = $T.IndexOf("'", $i + 1)
            if ($j -lt 0) { Add-GuardText $st $T.Substring($i + 1); $i = $n }
            else { Add-GuardText $st $T.Substring($i + 1, $j - $i - 1); $i = $j + 1 }
            continue
        }
        if ($c -eq '"') { $i = Read-GuardDoubleQuoted $T ($i + 1) $st; continue }
        if ($c -eq '$') {
            if ($i + 1 -lt $n -and $T[$i + 1] -eq '(') {
                $j = Find-GuardClose $T ($i + 1)
                if ($j -lt 0) { $j = $n - 1 }
                if (-not ($i + 2 -lt $n -and $T[$i + 2] -eq '(')) { Add-GuardNested $st $T.Substring($i + 2, [Math]::Max(0, $j - $i - 2)) }
                Add-GuardText $st $T.Substring($i, $j - $i + 1)
                $i = $j + 1; continue
            }
            if ($i + 1 -lt $n -and $T[$i + 1] -eq '{') {
                $j = $T.IndexOf('}', $i + 2)
                if ($j -lt 0) { $j = $n - 1 }
                Add-GuardText $st $T.Substring($i, $j - $i + 1)
                $i = $j + 1; continue
            }
            Add-GuardText $st '$'; $i++; continue
        }
        if ($c -eq '`') {
            $j = Find-GuardBacktick $T $i
            if ($j -lt 0) { $j = $n }
            Add-GuardNested $st $T.Substring($i + 1, [Math]::Max(0, [Math]::Min($j, $n) - $i - 1))
            Add-GuardText $st '`'
            $i = $j + 1; continue
        }
        if ($c -eq '(') {
            $j = Find-GuardClose $T $i
            if ($j -lt 0) { $j = $n }
            Add-GuardNested $st $T.Substring($i + 1, [Math]::Max(0, [Math]::Min($j, $n) - $i - 1))
            if ($st.InWord) { Add-GuardText $st '()' }
            $i = $j + 1; continue
        }
        if ($c -eq ')' -or $c -eq ';') { Complete-GuardCommand $st; $i++; continue }
        if ($c -eq '&') {
            if ($i + 1 -lt $n -and $T[$i + 1] -eq '&') { Complete-GuardCommand $st; $i += 2; continue }
            if ($i + 1 -lt $n -and $T[$i + 1] -eq '>') {   # &> file, &>> file
                Complete-GuardWord $st
                $i += 2
                if ($i -lt $n -and $T[$i] -eq '>') { $i++ }
                $st.SkipNext = $true
                continue
            }
            Complete-GuardCommand $st; $i++; continue
        }
        if ($c -eq '|') {
            Complete-GuardCommand $st
            if ($i + 1 -lt $n -and ($T[$i + 1] -eq '|' -or $T[$i + 1] -eq '&')) { $i += 2 } else { $i++ }
            continue
        }
        # what is left is < or >
        if ($i + 1 -lt $n -and $T[$i + 1] -eq '(') {   # process substitution <( ) / >( )
            Complete-GuardWord $st
            $j = Find-GuardClose $T ($i + 1)
            if ($j -lt 0) { $j = $n }
            Add-GuardNested $st $T.Substring($i + 2, [Math]::Max(0, [Math]::Min($j, $n) - $i - 2))
            $i = $j + 1; continue
        }
        if ($c -eq '<' -and $i + 1 -lt $n -and $T[$i + 1] -eq '<' -and -not ($i + 2 -lt $n -and $T[$i + 2] -eq '<')) {
            Complete-GuardWord $st
            $j = $i + 2; $strip = $false
            if ($j -lt $n -and $T[$j] -eq '-') { $strip = $true; $j++ }
            while ($j -lt $n -and ($T[$j] -eq ' ' -or $T[$j] -eq "`t")) { $j++ }
            $m = $HeredocDelimiter.Match($T, $j)
            if ($m.Success) {
                # The command the heredoc feeds: its first word, past assignments and wrappers (timeout 5 bash).
                $owner = $null
                $ws = [string[]]$st.Words.ToArray()
                $o = 0
                while ($o -lt $ws.Count -and $ws[$o] -cmatch '^[A-Za-z_][A-Za-z0-9_]*=') { $o++ }
                while ($o -lt $ws.Count) { $k = Skip-GuardWrapper $ws $o; if ($k -lt 0) { break }; $o = $k }
                if ($o -lt $ws.Count) { $owner = Get-GuardCommandName $ws[$o] }
                $isShell = (@('bash', 'sh', 'zsh', 'dash', 'ksh') -ccontains $owner)
                # ...and reads it as its script only with no -c string and no script file (bash -c 'cat > f' <<EOF
                # feeds the heredoc to cat as data). Option values (-o pipefail) are skipped; -s and -- end the options.
                for ($p = $o + 1; $isShell -and $p -lt $ws.Count; $p++) {
                    if ($ws[$p] -ceq '-s' -or $ws[$p] -ceq '--') { break }
                    if (@('-o', '+o', '-O', '+O', '--rcfile', '--init-file') -ccontains $ws[$p] -or $ws[$p] -cmatch '^[-+][A-Za-z]*[oO]$') { $p++; continue }
                    if ($ws[$p] -cmatch '^-[A-Za-z]*c[A-Za-z]*$' -or -not ($ws[$p].StartsWith('-') -or $ws[$p].StartsWith('+'))) { $isShell = $false }
                }
                $here.Add([pscustomobject]@{ Delim = $m.Groups[2].Value; Strip = $strip; Shell = $isShell })
                $i = $j + $m.Length
            } else { $i = $j }
            continue
        }
        # any other redirection: drop an fd number before it and the target after it
        if ($st.InWord -and $st.Sb.ToString() -match '^\d+$') { $null = $st.Sb.Clear(); $st.InWord = $false }
        else { Complete-GuardWord $st }
        $j = $i + 1
        while ($j -lt $n -and ($T[$j] -eq '<' -or $T[$j] -eq '>' -or $T[$j] -eq '|')) { $j++ }
        if ($j -lt $n -and $T[$j] -eq '&') {   # >&2, <&-: a descriptor, no target word
            $j++
            while ($j -lt $n -and ([string]$T[$j] -match '[0-9-]')) { $j++ }
        } else { $st.SkipNext = $true }
        $i = $j
    }
    Complete-GuardCommand $st
    return ,$st.Cmds
}

function Get-GuardCmdCommands([string]$Text) {
    # Splits a cmd.exe command line on & && | || and new lines outside double quotes (^ escapes one character).
    $cmds = [System.Collections.Generic.List[object]]::new()
    $words = [System.Collections.Generic.List[string]]::new()
    $sb = [System.Text.StringBuilder]::new()
    $inq = $false; $inw = $false; $skip = $false
    $i = 0; $n = $Text.Length
    while ($i -le $n) {
        if ($i -lt $n) { $c = [string]$Text[$i] } else { $c = "`n" }
        $boundary = (-not $inq) -and (@(' ', "`t", '&', '|', "`n", "`r", '<', '>') -contains $c)
        if ($boundary) {
            if ($inw) {
                if ($skip) { $skip = $false } else { $words.Add($sb.ToString()) }
                $null = $sb.Clear(); $inw = $false
            }
            if ($c -eq '&' -or $c -eq '|' -or $c -eq "`n" -or $c -eq "`r") {
                if ($words.Count -gt 0) { $cmds.Add([string[]]$words.ToArray()); $words.Clear() }
                $skip = $false
            } elseif ($c -eq '<' -or $c -eq '>') {
                if ($words.Count -gt 0 -and $words[$words.Count - 1] -match '^\d$') { $words.RemoveAt($words.Count - 1) }
                if ($i + 1 -lt $n -and $Text[$i + 1] -eq '>') { $i++ }
                if ($i + 1 -lt $n -and $Text[$i + 1] -eq '&') { $i += 2 } else { $skip = $true }
            }
            $i++; continue
        }
        if ($c -eq '"') { $inq = -not $inq; $inw = $true; $i++; continue }
        if ($c -eq '^' -and -not $inq -and $i + 1 -lt $n) { $null = $sb.Append($Text[$i + 1]); $inw = $true; $i += 2; continue }
        $null = $sb.Append($c); $inw = $true; $i++
    }
    return ,$cmds
}

function ConvertTo-GuardPs5Text([string]$T) {
    # Windows PowerShell 5.1 cannot parse && and ||; for finding the commands, ; is equivalent.
    if ($T.IndexOf('&&') -lt 0 -and $T.IndexOf('||') -lt 0) { return $T }
    $sb = [System.Text.StringBuilder]::new(); $q = $null; $i = 0; $n = $T.Length
    while ($i -lt $n) {
        $c = $T[$i]
        if ($q) {
            $null = $sb.Append($c)
            if ($c -eq '`' -and $q -eq '"' -and $i + 1 -lt $n) { $null = $sb.Append($T[$i + 1]); $i += 2; continue }
            if ($c -eq $q) { $q = $null }
            $i++; continue
        }
        if ($c -eq "'" -or $c -eq '"') { $q = $c; $null = $sb.Append($c); $i++; continue }
        if ($i + 1 -lt $n -and (($c -eq '&' -and $T[$i + 1] -eq '&') -or ($c -eq '|' -and $T[$i + 1] -eq '|'))) {
            $null = $sb.Append('; '); $i += 2; continue
        }
        $null = $sb.Append($c); $i++
    }
    return $sb.ToString()
}

function Get-GuardAstText($El) {
    if ($El -is [System.Management.Automation.Language.CommandParameterAst]) {
        $t = '-' + $El.ParameterName
        if ($El.Argument) { $t += ':' + (Get-GuardAstText $El.Argument) }
        return $t
    }
    if ($El -is [System.Management.Automation.Language.StringConstantExpressionAst]) { return $El.Value }
    if ($El -is [System.Management.Automation.Language.ExpandableStringExpressionAst]) { return (Expand-GuardEnv $El.Value) }
    if ($El -is [System.Management.Automation.Language.VariableExpressionAst]) { return (Expand-GuardEnv $El.Extent.Text) }
    if ($El -is [System.Management.Automation.Language.ArrayLiteralAst]) {
        return ((@($El.Elements | ForEach-Object { Get-GuardAstText $_ })) -join ',')
    }
    if ($El -is [System.Management.Automation.Language.ConstantExpressionAst]) { return [string]$El.Value }
    return $El.Extent.Text
}

function Get-GuardPowerShellCommands([string]$Text) {
    # Every command PowerShell would run from this text, nested script blocks and $( ) included, in source order.
    $cmds = [System.Collections.Generic.List[object]]::new()
    $tokens = $null; $errors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseInput((ConvertTo-GuardPs5Text $Text), [ref]$tokens, [ref]$errors)
    $found = $ast.FindAll({ param($x) $x -is [System.Management.Automation.Language.CommandAst] }, $true)
    foreach ($c in $found) {
        $w = [System.Collections.Generic.List[string]]::new()
        foreach ($el in $c.CommandElements) { $w.Add([string](Get-GuardAstText $el)) }
        if ($w.Count -gt 0) { $cmds.Add([string[]]$w.ToArray()) }
    }
    return ,$cmds
}

# ---------------------------------------------------------------------------------------------- dispatch

function Get-GuardCommandName([string]$W) {
    $b = $W.Trim('(', ')', '@', '{', '}')
    $k = [Math]::Max($b.LastIndexOf('/'), $b.LastIndexOf('\'))
    if ($k -ge 0) { $b = $b.Substring($k + 1) }
    $b = $b.ToLowerInvariant()
    if ($b -match '^(.+)\.(exe|com)$') { $b = $Matches[1] }
    return $b
}

function Skip-GuardWrapper([string[]]$W, [int]$I) {
    # When $W[$I] runs the rest of the line as a command (timeout 60 find ...), the index of that command; else -1.
    $name = Get-GuardCommandName $W[$I]
    $n = $W.Count; $j = $I + 1
    if (@('sudo', 'doas', 'gsudo') -ccontains $name) {
        while ($j -lt $n -and $W[$j].StartsWith('-')) { if ($W[$j] -cmatch '^-[ugCDhprtTU]$') { $j++ }; $j++ }
        return $j
    }
    if ($name -ceq 'timeout') {
        while ($j -lt $n -and $W[$j].StartsWith('-')) {
            if ($W[$j] -cmatch '^-[sk]$' -or $W[$j] -ceq '--signal' -or $W[$j] -ceq '--kill-after') { $j++ }
            $j++
        }
        return ($j + 1)   # past the duration
    }
    if ($name -ceq 'nice' -or $name -ceq 'ionice') {
        while ($j -lt $n -and $W[$j].StartsWith('-')) { if ($W[$j] -cmatch '^-[nc]$') { $j++ }; $j++ }
        return $j
    }
    if ($name -ceq 'command') {
        if ($j -lt $n -and ($W[$j] -ceq '-v' -or $W[$j] -ceq '-V')) { return $n }   # only looks a name up
        while ($j -lt $n -and $W[$j].StartsWith('-')) { $j++ }
        return $j
    }
    if (@('nohup', 'builtin', 'winpty', 'exec', 'time', 'chronic') -ccontains $name) {
        while ($j -lt $n -and $W[$j].StartsWith('-')) { if ($W[$j] -ceq '-a') { $j++ }; $j++ }
        return $j
    }
    if ($name -ceq 'stdbuf') {
        while ($j -lt $n -and $W[$j].StartsWith('-')) { if ($W[$j] -cmatch '^-[ioe]$') { $j++ }; $j++ }
        return $j
    }
    if ($name -ceq 'env') {
        while ($j -lt $n -and ($W[$j].StartsWith('-') -or $W[$j] -cmatch '^[A-Za-z_][A-Za-z0-9_]*=')) {
            if ($W[$j] -cmatch '^-[uCS]$') { $j++ }
            $j++
        }
        return $j
    }
    if ($name -ceq 'xargs') {
        while ($j -lt $n -and $W[$j].StartsWith('-')) { if ($W[$j] -cmatch '^-[ILnPdsEa]$') { $j++ }; $j++ }
        return $j
    }
    return -1
}

function Get-GuardSlice([string[]]$W, [int]$From) {
    if ($From -ge $W.Count) { return ,([string[]]@()) }
    return ,([string[]]$W[$From..($W.Count - 1)])
}

function Get-GuardCd($Ctx, [string[]]$A, [string]$Shell, [string]$Cwd) {
    $target = $null
    foreach ($t in $A) {
        if ($Shell -eq 'powershell' -and $t -match '^-\w+:(.+)$') { $target = $Matches[1]; break }
        if ($t.StartsWith('-') -and $t.Length -gt 1) { continue }
        if ($Shell -eq 'cmd' -and $t -match '^/[dD]$') { continue }
        $target = $t; break
    }
    if ($null -eq $target) {
        if ($Shell -eq 'posix') { return (ConvertTo-PosixGuardPath '~' $Cwd $Ctx) }
        return $Cwd
    }
    if ($target -eq '-') { return $null }
    if ($Shell -eq 'posix') { return (ConvertTo-PosixGuardPath $target $Cwd $Ctx) }
    return (ConvertTo-WindowsGuardPath $target $Cwd $Ctx)
}

function Invoke-GuardShellC($Ctx, [string[]]$A, [string]$Cwd) {
    # bash -c '...', sh -lc '...': the string is a command line of its own. A script file is not seen.
    for ($i = 0; $i -lt $A.Count; $i++) {
        $t = $A[$i]
        if ($t -cmatch '^-[A-Za-z]*c[A-Za-z]*$') {
            if ($i + 1 -lt $A.Count) { $null = Invoke-GuardText $Ctx $A[$i + 1] 'posix' $Cwd }
            return
        }
        if (@('-o', '+o', '-O', '+O', '--rcfile', '--init-file') -ccontains $t) { $i++; continue }
        if ($t.StartsWith('-') -or $t.StartsWith('+')) { continue }
        return
    }
}

function Invoke-GuardCmdC($Ctx, [string[]]$A, [string]$Cwd) {
    # cmd /c ... (//c from Git Bash): the rest is a cmd.exe command line.
    for ($i = 0; $i -lt $A.Count; $i++) {
        if ($A[$i] -match '^(/|//)[cCkK]$') {
            $rest = Get-GuardSlice $A ($i + 1)
            if ($rest.Count -gt 0) { $null = Invoke-GuardText $Ctx ($rest -join ' ') 'cmd' $Cwd }
            return
        }
        if ($A[$i] -match '^(?:/|//)[cCkK](\S.*)$') {
            $rest = @($Matches[1]) + @(Get-GuardSlice $A ($i + 1))
            $null = Invoke-GuardText $Ctx ($rest -join ' ') 'cmd' $Cwd
            return
        }
    }
}

function Invoke-GuardPowerShellC($Ctx, [string[]]$A, [string]$Cwd) {
    # powershell / pwsh -Command ... or -EncodedCommand <base64>. A -File script is not seen.
    for ($i = 0; $i -lt $A.Count; $i++) {
        if ($A[$i].ToLowerInvariant() -match '^[-/](c|co|com|comm|comma|comman|command)$') {
            $rest = Get-GuardSlice $A ($i + 1)
            if ($rest.Count -gt 0) { $null = Invoke-GuardText $Ctx ($rest -join ' ') 'powershell' $Cwd }
            return
        }
    }
    for ($i = 0; $i -lt $A.Count - 1; $i++) {
        if ($A[$i].ToLowerInvariant() -match '^[-/](e|ec|en|enc|enco|encod|encode|encoded|encodedc\w*)$') {
            try {
                $txt = [Text.Encoding]::Unicode.GetString([Convert]::FromBase64String($A[$i + 1]))
                $null = Invoke-GuardText $Ctx $txt 'powershell' $Cwd
            } catch { }
            return
        }
    }
    $valueOpts = @('executionpolicy', 'ex', 'ep', 'exec', 'windowstyle', 'w', 'inputformat', 'if', 'outputformat', 'of',
        'version', 'v', 'configurationname', 'workingdirectory', 'wd', 'settingsfile', 'custompipename')
    for ($i = 0; $i -lt $A.Count; $i++) {
        $l = $A[$i].ToLowerInvariant()
        if ($l -match '^[-/](f|fi|fil|file)$') { return }
        if ($l -match '^[-/](.+)$') { if ($valueOpts -contains $Matches[1]) { $i++ }; continue }
        $null = Invoke-GuardText $Ctx ((Get-GuardSlice $A $i) -join ' ') 'powershell' $Cwd   # powershell <command>
        return
    }
}

function Invoke-GuardWsl($Ctx, [string[]]$A) {
    # wsl runs Linux, where / is the Linux root (and /mnt/c the C: drive): judged the same way.
    for ($i = 0; $i -lt $A.Count; $i++) {
        $t = $A[$i]
        if ($t -ceq '-e' -or $t -ceq '--exec') {
            $rest = Get-GuardSlice $A ($i + 1)
            if ($rest.Count -gt 0) { $null = Invoke-GuardSimple $Ctx $rest 'posix' $null }
            return
        }
        if ($t -ceq '--') { $null = Invoke-GuardText $Ctx ((Get-GuardSlice $A ($i + 1)) -join ' ') 'posix' $null; return }
        if (@('-d', '--distribution', '-u', '--user', '--cd', '--shell-type') -ccontains $t) { $i++; continue }
        if ($t.StartsWith('-')) { continue }
        # Judged both ways: as the words the caller's shell passed (wsl bash -c "find / ..." keeps its -c string
        # whole) and as the line wsl hands its Linux shell.
        $rest = Get-GuardSlice $A $i
        $null = Invoke-GuardSimple $Ctx $rest 'posix' $null
        $null = Invoke-GuardText $Ctx ($rest -join ' ') 'posix' $null
        return
    }
}

function Invoke-GuardSimple($Ctx, [string[]]$W, [string]$Shell, [string]$Cwd) {
    # Judges one simple command. $Shell ('posix' for Git Bash, 'powershell', 'cmd') decides what a name means:
    # `dir` is Get-ChildItem in PowerShell and cmd's dir in cmd. Returns the working directory after it.
    $i = 0; $n = $W.Count
    if ($Shell -eq 'posix') {
        $keywords = @('if', 'then', 'else', 'elif', 'fi', 'do', 'done', 'while', 'until', '!', '{', '}', 'case', 'esac', 'coproc')
        while ($i -lt $n -and ($W[$i] -cmatch '^[A-Za-z_][A-Za-z0-9_]*=' -or $keywords -ccontains $W[$i])) { $i++ }
        while ($i -lt $n) {
            $k = Skip-GuardWrapper $W $i
            if ($k -lt 0) { break }
            $i = $k
        }
    }
    if ($i -ge $n) { return $Cwd }
    $raw = $W[$i]
    $name = Get-GuardCommandName $raw
    $A = Get-GuardSlice $W ($i + 1)

    if ($Shell -eq 'posix' -and $name -ceq 'eval') {
        # eval joins its words and runs the result as a command line of this shell.
        return (Invoke-GuardText $Ctx ($A -join ' ') 'posix' $Cwd)
    }
    if ($Shell -eq 'powershell' -and ($raw.StartsWith('(') -or $raw.StartsWith('$'))) {
        # A computed program, & (Join-Path ...) or & $bash: its name cannot be read, so judge the arguments the way a
        # shell would read them (-c "find / ..." for bash, /c ... for cmd).
        Invoke-GuardShellC $Ctx $A $Cwd
        Invoke-GuardCmdC $Ctx $A $Cwd
        # ...and as a command line of their own (& $pwsh -Command "...", & $busybox ls -R /): whatever the program is,
        # arguments that spell a search from a root are one.
        Invoke-GuardPowerShellC $Ctx $A $Cwd
        return $Cwd
    }

    if (@('cd', 'pushd', 'chdir') -ccontains $name -or ($Shell -eq 'powershell' -and @('set-location', 'sl', 'push-location') -ccontains $name)) {
        return (Get-GuardCd $Ctx $A $Shell $Cwd)
    }
    if ($Shell -eq 'powershell') {
        if (@('get-childitem', 'gci', 'dir', 'ls') -ccontains $name) { Test-GuardChildItem $Ctx $A $Cwd; return $Cwd }
        if ($name -ceq 'where') {   # bare `where` is Where-Object here; where.exe is the file finder
            if ($raw -match '(?i)\.exe$') { Test-GuardWhere $Ctx $A $Cwd }
            return $Cwd
        }
        if ($name -ceq 'invoke-expression' -or $name -ceq 'iex') {
            foreach ($a in $A) { if (-not $a.StartsWith('-')) { $null = Invoke-GuardText $Ctx $a 'powershell' $Cwd; break } }
            return $Cwd
        }
        if ($name -ceq 'find' -and -not (Test-GuardGnuFind $raw $A)) { return $Cwd }
    } elseif ($Shell -eq 'cmd') {
        if ($name -ceq 'dir') { Test-GuardCmdDir $Ctx $A $Cwd; return $Cwd }
        if ($name -ceq 'find' -and -not (Test-GuardGnuFind $raw $A)) { return $Cwd }
    }
    if ($name -ceq 'where') { Test-GuardWhere $Ctx $A $Cwd; return $Cwd }
    if ($name -ceq 'find') { Test-GuardFind $Ctx $A $Cwd; return $Cwd }
    if (@('ls', 'dir', 'vdir') -ccontains $name) { Test-GuardLs $Ctx $name $A $Cwd; return $Cwd }
    if ($name -ceq 'du') { Test-GuardDu $Ctx $A $Cwd; return $Cwd }
    if (@('grep', 'egrep', 'fgrep', 'rgrep') -ccontains $name) { Test-GuardGrep $Ctx $name $A $Cwd; return $Cwd }
    if ($name -ceq 'rg') { Test-GuardRg $Ctx $A $Cwd; return $Cwd }
    if (@('bash', 'sh', 'zsh', 'dash', 'ksh') -ccontains $name) { Invoke-GuardShellC $Ctx $A $Cwd; return $Cwd }
    if ($name -ceq 'cmd') { Invoke-GuardCmdC $Ctx $A $Cwd; return $Cwd }
    if ($name -ceq 'powershell' -or $name -ceq 'pwsh') { Invoke-GuardPowerShellC $Ctx $A $Cwd; return $Cwd }
    if ($name -ceq 'wsl') { Invoke-GuardWsl $Ctx $A; return $Cwd }
    return $Cwd
}

function Invoke-GuardText($Ctx, [string]$Text, [string]$Shell, [string]$Cwd) {
    if ($Ctx.Depth -ge 6 -or [string]::IsNullOrWhiteSpace($Text)) { return $Cwd }
    $Ctx.Depth++
    try {
        if ($Shell -eq 'powershell') { $cmds = Get-GuardPowerShellCommands $Text }
        elseif ($Shell -eq 'cmd') { $cmds = Get-GuardCmdCommands $Text }
        else { $cmds = Get-GuardPosixCommands $Text }
        $saved = [System.Collections.Generic.Stack[object]]::new()
        foreach ($w in $cmds) {
            if ($w.Count -eq 1 -and $w[0] -ceq $GuardSubshellIn) { $saved.Push($Cwd); continue }
            if ($w.Count -eq 1 -and $w[0] -ceq $GuardSubshellOut) { if ($saved.Count -gt 0) { $Cwd = $saved.Pop() }; continue }
            $Cwd = Invoke-GuardSimple $Ctx $w $Shell $Cwd
        }
    } finally {
        $Ctx.Depth--
    }
    return $Cwd
}

# ------------------------------------------------------------------------------------------------ answer

function Get-GuardPayload([string]$Raw) {
    # -> Copilot, Command, Shells, Cwd for a shell tool call this guard judges; $null for anything else.
    try { $data = $Raw | ConvertFrom-Json } catch { return $null }
    if ($null -eq $data -or $data -isnot [System.Management.Automation.PSCustomObject]) { return $null }
    $names = @($data.PSObject.Properties | ForEach-Object { $_.Name })
    $copilot = ($names -contains 'toolName') -and -not ($names -contains 'tool_input')
    if ($copilot) {
        $tool = ([string]$data.toolName).ToLowerInvariant()
        if ($CopilotShellTools -notcontains $tool) { return $null }
        $toolArgs = $data.toolArgs
        if ($toolArgs -is [string]) {
            try { $toolArgs = $toolArgs | ConvertFrom-Json } catch { $toolArgs = [string]$data.toolArgs }
        }
        $cmd = ''
        if ($toolArgs -is [System.Management.Automation.PSCustomObject]) {
            if ($toolArgs.PSObject.Properties.Match('command').Count -gt 0 -and $toolArgs.command -is [string]) { $cmd = $toolArgs.command }
            else { $cmd = (@($toolArgs.PSObject.Properties | Where-Object { $_.Value -is [string] } | ForEach-Object { $_.Value })) -join "`n" }
        } elseif ($toolArgs -is [string]) { $cmd = $toolArgs }
        if ($tool -eq 'bash') { $shells = @('posix') }
        elseif ($tool -eq 'powershell') { $shells = @('powershell') }
        else { $shells = @('posix', 'powershell') }   # `shell`: its syntax is not known, so it is read both ways
    } else {
        $tool = [string]$data.tool_name
        if ($tool -ne 'Bash' -and $tool -ne 'PowerShell') { return $null }
        $cmd = ''
        if ($data.tool_input) { $cmd = [string]$data.tool_input.command }
        if ($tool -eq 'PowerShell') { $shells = @('powershell') } else { $shells = @('posix') }
    }
    $cwd = [string]$data.cwd
    if (-not $cwd) { $cwd = (Get-Location).ProviderPath }
    return [pscustomobject]@{ Copilot = $copilot; Command = $cmd; Shells = $shells; Cwd = $cwd }
}

function Format-GuardReason($Findings) {
    $parts = foreach ($f in $Findings) {
        if ($f.Kind -eq 'root') {
            "'$($f.Tool)' from $($f.Path) - in Git Bash, / is Git's install folder plus the whole Windows registry (/proc/registry), not the C: drive (that is /c)"
        } elseif ($f.Kind -eq 'proc') {
            "'$($f.Tool)' from $($f.Path) - /proc is the Windows registry and process tables, not files"
        } elseif ($f.Kind -eq 'registry') {
            "'$($f.Tool)' over the registry root $($f.Path) with no depth limit of $MaxDriveDepth or less - a whole hive, not files"
        } else {
            $hint = ''
            if ($f.Path -match '^/mnt/[A-Za-z](/|$)') { $hint = ' (in Git Bash /mnt/c is not the C: drive - that is WSL syntax; C:\ is /c)' }
            "'$($f.Tool)' over the whole drive $($f.Path) with no depth limit of $MaxDriveDepth or less$hint"
        }
    }
    return ('Blocked by the search guard: ' + ($parts -join '; ') + '. A search from a root never finds what lies ' +
        'outside it, a Git Bash walk of the registry leaks a handle per key, and an unbounded search can run for ' +
        'hours after its shell is gone. Search the folder the file should be in, with a depth and a time limit: ' +
        "timeout 60 find /c/<folder> -maxdepth 6 -name '<pattern>', or Get-ChildItem <folder> -Recurse -Depth 6 " +
        '-Filter <pattern>, or the built-in file search (Glob/Grep) tools. To locate a command or module, ask ' +
        'the system: (Get-Command <name>).Source or (Get-Module -ListAvailable <name>).ModuleBase. A not-found ' +
        'from a bounded search means not in that folder, not that the file does not exist.')
}

function Format-GuardAnswer([bool]$Copilot, [string]$Decision, [string]$Reason) {
    if ($Copilot) {
        return (ConvertTo-Json -Compress -InputObject ([ordered]@{ permissionDecision = $Decision; permissionDecisionReason = $Reason }))
    }
    $inner = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = $Decision; permissionDecisionReason = $Reason }
    return (ConvertTo-Json -Compress -Depth 3 -InputObject ([ordered]@{ hookSpecificOutput = $inner }))
}

function Invoke-SearchGuard([string]$Raw) {
    # The whole decision: the JSON answer to print, or '' for none (the call goes on to the normal permission
    # flow). The tests call this directly.
    $p = Get-GuardPayload $Raw
    if ($null -eq $p -or [string]::IsNullOrWhiteSpace($p.Command)) { return '' }
    if (-not $SearchNames.IsMatch($p.Command)) { return '' }
    $ctx = New-GuardContext
    if ($p.Cwd -match '^/') { $cwd = ConvertTo-PosixGuardPath $p.Cwd $null $ctx }
    else { $cwd = ConvertTo-WindowsGuardPath $p.Cwd $null $ctx }
    foreach ($shell in $p.Shells) { $null = Invoke-GuardText $ctx $p.Command $shell $cwd }
    if ($ctx.Findings.Count -eq 0) { return '' }
    return (Format-GuardAnswer $p.Copilot 'deny' (Format-GuardReason $ctx.Findings))
}

function Get-GuardCrashProbe([string]$Raw) {
    # -> the text the crash path tests for a search tool: the command alone when the payload parses, never the
    # whole payload - that holds the tool's own name ("Bash", "powershell"), which $SearchNames matches, so every
    # shell call would be asked about (found 2026-10-19). Unparsable: the payload minus its tool-name fields.
    try {
        $d = $Raw | ConvertFrom-Json
        if ($d.PSObject.Properties.Match('tool_input').Count -gt 0 -and $d.tool_input.command -is [string]) {
            return [string]$d.tool_input.command
        }
        if ($d.PSObject.Properties.Match('toolArgs').Count -gt 0) {
            $a = $d.toolArgs
            if ($a -is [string]) {
                try { $j = $a | ConvertFrom-Json; if ($j.command -is [string]) { return [string]$j.command } } catch { }
                return $a
            }
            if ($a.command -is [string]) { return [string]$a.command }
            return [string]$a
        }
        return ''
    } catch {
        return ($Raw -replace '"(tool_name|toolName)"\s*:\s*"[^"]*"', '')
    }
}

function Get-GuardCrashAnswer([string]$Raw, [string]$Message) {
    # -> "ask" in the calling tool's format when the command names a search tool, '' otherwise.
    if (-not $SearchNames.IsMatch((Get-GuardCrashProbe $Raw))) { return '' }
    $isCopilot = ($Raw -match '"toolName"') -and ($Raw -notmatch '"tool_input"')
    $msg = $Message -replace '[^\x20-\x7E]', '?'
    return (Format-GuardAnswer $isCopilot 'ask' ("The search guard could not check this command ($msg). " +
        'Approve it only if it is a bounded search: a named folder, a depth limit and a time limit.'))
}

function Invoke-SearchGuardMain {
    $raw = ''
    $answer = ''
    try {
        $buffer = New-Object System.IO.MemoryStream
        [Console]::OpenStandardInput().CopyTo($buffer)
        $raw = [System.Text.Encoding]::UTF8.GetString($buffer.ToArray()).TrimStart([char]0xFEFF)
        $answer = Invoke-SearchGuard $raw
    } catch {
        # Never pass a search command silently because of a bug here: ask instead. Anything else stays silent.
        # Every step here may itself fail (Constrained Language Mode refuses most .NET calls), so each is guarded.
        try { $answer = Get-GuardCrashAnswer $raw $_.Exception.Message } catch { $answer = '' }
        try { [Console]::Error.WriteLine('search guard: ' + $_.Exception.Message) } catch { }
    }
    if ($answer) {
        try {
            $bytes = [System.Text.Encoding]::UTF8.GetBytes($answer)
            $out = [Console]::OpenStandardOutput()
            $out.Write($bytes, 0, $bytes.Length)
            $out.Flush()
        } catch {
            try { Write-Output $answer } catch { }
        }
    }
}

if ($GuardRunAsHook) {
    Invoke-SearchGuardMain
    exit 0
}
