# Tests for search_guard.ps1 - run them after every change to the guard:
#
#   powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/tests/test_search_guard.ps1
#   powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/tests/test_search_guard.ps1 <guard.ps1>
#
# Most cases call the guard's decision function in this process. The contract group runs it the way both tools do -
# a new powershell.exe reading the payload on stdin - and checks the answer format per tool and exit code 0 every
# time. The hook-command group runs the .claude/settings.json entries the way each tool runs them, from a temp folder
# that it deletes. Nothing is searched: the guard only reads command text. Prints one PASS/FAIL/SKIP line per case,
# then "N/N passed". Exit code 1 when any case fails. Keep this file ASCII.

param([string]$Guard)
$ErrorActionPreference = 'Stop'
if (-not $Guard) { $Guard = Join-Path (Split-Path -Parent $PSScriptRoot) 'search_guard.ps1' }
$Guard = (Resolve-Path $Guard).ProviderPath
. $Guard

$script:passed = 0
$script:failed = 0

function Write-Result([string]$Label, [bool]$Ok, [string]$Detail) {
    if ($Ok) { $script:passed++; Write-Output "PASS  $Label" }
    else { $script:failed++; Write-Output "FAIL  $Label  $Detail" }
}

function New-Payload([string]$Tool, [string]$Command, [string]$Cwd) {
    # B / P = Claude Code's Bash / PowerShell tool; cb / cp / cs = Copilot CLI's bash / powershell / shell tool.
    $claude = @{ B = 'Bash'; P = 'PowerShell' }
    $copilot = @{ cb = 'bash'; cp = 'powershell'; cs = 'shell' }
    if ($claude.ContainsKey($Tool)) {
        return (ConvertTo-Json -Compress -InputObject @{ tool_name = $claude[$Tool]; cwd = $Cwd; tool_input = @{ command = $Command } })
    }
    $toolArgs = ConvertTo-Json -Compress -InputObject @{ command = $Command }
    return (ConvertTo-Json -Compress -InputObject @{ toolName = $copilot[$Tool]; cwd = $Cwd; toolArgs = $toolArgs })
}

function Get-Decision([string]$Answer) {
    if (-not $Answer) { return 'allow' }
    $j = $Answer | ConvertFrom-Json
    if ($j.PSObject.Properties.Match('hookSpecificOutput').Count -gt 0) { return $j.hookSpecificOutput.permissionDecision }
    return $j.permissionDecision
}

$repo = 'C:\VS\MyRepo'
$commitWithHeredoc = "git commit -m `"`$(cat <<'EOF'`ndocs: explain why find / -name x is blocked (it's the registry)`nEOF`n)`""
$notesWithHeredoc = "cat > notes.md <<'EOF'`nRun find / -name x.dll to see the problem`nls -R /`nEOF`necho done"
$bashWithHeredoc = "bash <<'EOF'`nfind / -name x.dll`nEOF"
$timeoutBashHeredoc = "timeout 5 bash -s <<EOF`ncd / && find . -name x`nEOF"
$catWithHeredoc = "cat <<'EOF'`nfind / -name x`nEOF"
$bashCHeredoc = "bash -c 'cat > f.txt' <<'EOF'`nfind / -name x`nEOF"
$arithShift = "echo `$((1<<2))`nfind / -name x"
$bashOptHeredoc = "bash -euo pipefail <<'EOF'`nfind / -name x`nEOF"

# tool, command, expected, where the case comes from ('brief' = the issue's test matrix)
$cases = @(
    # --- the brief's matrix: must block
    @('B', 'find / -name x.dll', 'deny', 'brief'),
    @('B', 'find / -iname ''*.al'' -path "*Program Files*"', 'deny', 'brief'),
    @('B', 'timeout 60 find / -name x', 'deny', 'brief'),
    @('B', 'cd /c/VS && find / -name x', 'deny', 'brief'),
    @('B', 'find /proc/registry -name x', 'deny', 'brief'),
    @('B', 'find /c -name x.dll', 'deny', 'brief'),
    @('B', 'ls -R /', 'deny', 'brief'),
    @('B', 'grep -r Foo /', 'deny', 'brief'),
    @('P', 'Get-ChildItem C:\ -Recurse -Filter x.dll', 'deny', 'brief'),
    @('P', 'gci -Path C:\ -Recurse -Include *.al', 'deny', 'brief'),
    @('B', 'cmd /c dir C:\x.dll /s', 'deny', 'brief'),
    @('P', 'cmd /c dir C:\x.dll /s', 'deny', 'brief'),
    # --- the brief's matrix: must allow
    @('B', 'find . -name ''*.al''', 'allow', 'brief'),
    @('B', 'find /c/VS/MyRepo -maxdepth 6 -name ''*.al''', 'allow', 'brief'),
    @('B', 'timeout 60 find "$HOME/.nuget/packages" -maxdepth 3 -iname ''microsoft.*''', 'allow', 'brief'),
    @('B', 'find /c -maxdepth 2 -name bcartifacts.cache', 'allow', 'brief'),
    @('B', 'ls /', 'allow', 'brief'),
    @('B', 'git ls-files', 'allow', 'brief'),
    @('P', 'Get-ChildItem C:\VS\MyRepo -Recurse -Filter *.al', 'allow', 'brief'),
    @('P', 'Get-ChildItem C:\ -Directory', 'allow', 'brief'),
    @('P', 'Get-ChildItem C:\ -Recurse -Depth 2 -Filter x', 'allow', 'brief'),
    @('B', 'echo "find / is blocked"', 'allow', 'brief'),
    # --- more forms that must block
    @('B', 'cd / && find . -name x', 'deny', 'cd before a relative find (the deny-rule gap)'),
    @('B', 'pushd /c; find . -name x.dll; popd', 'deny', 'pushd to a drive root'),
    @('B', 'find / -name x 2>/dev/null | head -1', 'deny', 'redirection and pipe'),
    @('B', 'DLL=$(find / -name ALCops.LinterCop.dll 2>/dev/null | head -1)', 'deny', 'inside $( )'),
    @('B', 'echo `find / -name x`', 'deny', 'inside backticks'),
    @('B', 'find "/" -name x', 'deny', 'quoted root'),
    @('B', 'find /* -name x', 'deny', 'wildcard under the root'),
    @('B', 'find / -maxdepth 1 -name x', 'deny', 'the root is refused at any depth'),
    @('B', 'find C:/ -name x', 'deny', 'C:/ in Git Bash'),
    @('B', 'find /cygdrive/c -name x', 'deny', '/cygdrive/c'),
    @('B', 'find /mnt/c -name x', 'deny', '/mnt/c (WSL syntax)'),
    @('B', 'find /c -maxdepth 4 -name x', 'deny', 'a drive deeper than 3'),
    @('B', 'sudo find / -name x', 'deny', 'behind sudo'),
    @('B', 'echo x | xargs find / -name', 'deny', 'behind xargs'),
    @('B', 'ls -laR /c', 'deny', 'ls -R in a flag cluster'),
    @('B', 'du -sh /c', 'deny', 'du on a drive'),
    @('B', 'du -sh /', 'deny', 'du on the root'),
    @('B', 'grep -rn TODO /proc', 'deny', 'grep -r on /proc'),
    @('B', 'rg Foo /', 'deny', 'rg on the root'),
    @('B', 'rg --max-depth 5 Foo C:/', 'deny', 'rg on a drive, too deep'),
    @('B', 'bash -c "find / -name x"', 'deny', 'bash -c'),
    @('B', 'cmd //c "dir C:\\x.dll /s"', 'deny', 'cmd //c from Git Bash'),
    @('B', 'where //r C:/ x.dll', 'deny', 'where /r on a drive'),
    @('B', 'powershell -NoProfile -Command "Get-ChildItem C:\\ -Recurse -Filter x.dll"', 'deny', 'powershell -Command from Git Bash'),
    @('B', 'wsl find / -name x', 'deny', 'wsl'),
    @('P', 'bash -c "find / -name x"', 'deny', 'bash -c from PowerShell'),
    @('P', 'cmd.exe /c "dir C:\ /s /b"', 'deny', 'cmd.exe /c, quoted'),
    @('P', 'where.exe /r C:\ x.dll', 'deny', 'where.exe /r'),
    @('P', 'Set-Location C:\; Get-ChildItem -Recurse -Filter x.dll', 'deny', 'Set-Location to a drive root'),
    @('P', 'gci \ -r -fi x.dll', 'deny', 'current drive root, short parameters'),
    @('P', 'Get-ChildItem $env:SystemDrive\ -Recurse -Filter x.dll', 'deny', '$env:SystemDrive'),
    @('P', 'Get-ChildItem -LiteralPath ''C:\'' -Recurse', 'deny', '-LiteralPath'),
    @('P', 'Get-ChildItem C:\ -Recurse -Depth 5', 'deny', '-Depth deeper than 3'),
    @('P', 'ls -R C:\', 'deny', 'ls is Get-ChildItem in PowerShell'),
    @('P', '(Get-ChildItem C:\ -Recurse -Filter x.dll).FullName', 'deny', 'inside parentheses'),
    @('P', 'Get-ChildItem C:\ -Recurse -Filter x.dll && echo done', 'deny', '&& that Windows PowerShell 5.1 cannot parse'),
    @('P', '& ''C:\Program Files\Git\usr\bin\find.exe'' / -name x.dll', 'deny', 'Git''s find.exe by path'),
    @('P', 'powershell -NoProfile -Command "Get-ChildItem C:\ -Recurse"', 'deny', 'nested powershell -Command'),
    @('cb', 'find / -name x', 'deny', 'Copilot bash'),
    @('cp', 'Get-ChildItem C:\ -Recurse -Filter x.dll', 'deny', 'Copilot powershell'),
    @('cs', 'find / -name x', 'deny', 'Copilot shell, read both ways'),
    @('P', 'Get-ChildItem C:\ -s -Filter x.dll', 'deny', '-s, the alias of -Recurse'),
    @('P', 'dir C:\ -s', 'deny', 'dir -s'),
    @('P', 'gci -Path C:\ -s', 'deny', 'gci -s'),
    @('P', 'Get-ChildItem C:\sg-no-such-file-4711.dll -Recurse', 'deny', 'a missing leaf is searched for from the drive root'),
    @('P', 'Get-ChildItem -Path C:\ALCops.LinterCop.dll -Recurse -ErrorAction SilentlyContinue', 'deny', 'the incident''s DLL as a missing leaf'),
    @('P', 'Get-ChildItem \sg-no-such-file-4711.dll -Recurse', 'deny', 'a missing leaf under the current drive root'),
    @('B', 'cmd //c "dir C:\\sg-no-such-4711.cache /s /b"', 'deny', 'dir /s for a name that is not a folder'),
    # --- S2026-11-12-18 (0.190.31): forms the 2026-11-12 review found passing
    @('B', 'eval "find / -name x"', 'deny', 'eval'),
    @('B', 'eval find / -name x', 'deny', 'eval, unquoted words'),
    @('B', $bashWithHeredoc, 'deny', 'a heredoc into bash is its script'),
    @('B', $timeoutBashHeredoc, 'deny', 'a heredoc into timeout ... bash -s'),
    @('B', 'find -- / -name x', 'deny', 'find -- /'),
    @('B', 'wsl bash -c "find / -name x"', 'deny', 'wsl bash -c keeps its -c string whole'),
    @('P', 'wsl bash -c "find / -name x"', 'deny', 'wsl bash -c from PowerShell'),
    @('P', 'Get-ChildItem HKLM:\ -Recurse', 'deny', 'a registry hive root'),
    @('P', 'gci Registry::HKEY_CURRENT_USER -r', 'deny', 'Registry:: hive root'),
    @('P', 'Get-ChildItem Registry:: -Recurse', 'deny', 'every hive'),
    @('P', '& (Join-Path (Split-Path (Split-Path (git --exec-path))) ''bin\bash.exe'') -c "cd / && find . -name x"', 'deny', 'a computed program reading -c'),
    @('P', '& $bash -c "find / -name x"', 'deny', 'a program in a variable reading -c'),
    @('B', '(cd / && ls); find . -name x', 'allow', 'a subshell''s cd does not leak'),
    @('B', 'x=$(cd / && pwd); find . -name x', 'allow', 'a $( ) cd does not leak'),
    @('B', '(cd / && find . -name x)', 'deny', 'a cd inside the subshell still counts there'),
    @('P', 'Get-ChildItem HKLM:\SOFTWARE\Microsoft -Recurse -Depth 2', 'allow', 'a registry key below the hive root'),
    @('P', 'Get-ChildItem HKCU:\ -Recurse -Depth 2', 'allow', 'a hive root with depth 2'),
    @('B', $catWithHeredoc, 'allow', 'a heredoc into cat stays data'),
    @('P', '& ''C:\tools\app.exe'' -c config.json', 'allow', 'a literal program with -c is not a shell'),
    @('P', '& $pwsh -Command "Get-ChildItem C:\ -Recurse -Filter x.dll"', 'deny', 'a computed program reading -Command'),
    @('P', '& $tool -c config', 'allow', 'a computed program with -c config'),
    @('P', 'Get-ChildItem HKCU:Software -Recurse -Depth 2', 'allow', 'HKCU:Software is below the hive root'),
    @('B', $bashCHeredoc, 'allow', 'a heredoc fed to bash -c is data for its command'),
    @('B', $arithShift, 'deny', 'arithmetic << is not a heredoc'),
    @('B', $bashOptHeredoc, 'deny', 'bash -o pipefail still reads the heredoc as its script'),
    @('P', '& $busybox ls -R /', 'deny', 'a computed program whose arguments spell a root search'),
    # --- more forms that must pass
    @('B', 'find /c/Users/me/repo -name x', 'allow', 'a folder below a drive'),
    @('B', 'find "C:\Program Files" -maxdepth 3 -name x', 'allow', 'a named folder'),
    @('B', 'du -sh ~/x /c/Users/me/repo', 'allow', 'du on named folders'),
    @('B', 'grep -rn TODO .', 'allow', 'grep -r in the repo'),
    @('B', 'grep Foo /', 'allow', 'grep without -r reads no tree'),
    @('B', 'ls -la /c', 'allow', 'ls without -R'),
    @('B', 'rg --max-depth 2 Foo C:/', 'allow', 'rg on a drive, depth 2'),
    @('B', 'rg Foo src/', 'allow', 'rg in the repo'),
    @('B', 'cd /c && ls', 'allow', 'cd to a drive, then a plain ls'),
    @('B', $commitWithHeredoc, 'allow', 'a commit message heredoc that mentions find /'),
    @('B', $notesWithHeredoc, 'allow', 'a heredoc body is data'),
    @('B', 'echo ''find / -name x'' > tip.txt', 'allow', 'quoted text'),
    @('B', '# find / -name x', 'allow', 'a comment'),
    @('B', 'git grep -n "find /"', 'allow', 'git grep'),
    @('B', 'command -v find', 'allow', 'command -v only looks a name up'),
    @('P', 'Get-ChildItem C:\ -Recurse:$false', 'allow', '-Recurse:$false'),
    @('P', 'Get-ChildItem -Recurse -Filter *.al', 'allow', 'recursion in the repo'),
    @('P', 'Write-Output "find / -name x"', 'allow', 'text in PowerShell'),
    @('P', 'Get-Process | where Name -eq find', 'allow', 'where is Where-Object in PowerShell'),
    @('P', 'find /i "text" notes.txt', 'allow', 'Windows find.exe: /i is a switch, not drive I:'),
    @('P', 'find /c "text" notes.txt', 'allow', 'Windows find.exe: /c counts lines'),
    @('P', 'cmd /c dir C:\Users /s', 'allow', 'dir /s on a folder below the root'),
    @('P', 'Get-ChildItem $env:SystemRoot -Recurse -Filter x', 'allow', 'an existing folder under the drive root is not the root'),
    @('P', 'cmd /c dir %SystemRoot% /s /b', 'allow', 'dir /s on an existing folder, even one at the root'),
    @('P', 'Get-ChildItem C:\ -Directory -s:$false', 'allow', '-s:$false'),
    @('cb', 'find . -name x', 'allow', 'Copilot bash, bounded'),
    @('cp', 'Get-ChildItem . -Recurse -Filter x.dll', 'allow', 'Copilot powershell, bounded')
)

foreach ($c in $cases) {
    $tool, $command, $expect, $from = $c
    $got = Get-Decision (Invoke-SearchGuard (New-Payload $tool $command $repo))
    $shown = ($command -replace "`n", '\n')
    Write-Result "[$from] $tool $shown -> $expect" ($got -eq $expect) "(got $got)"
}

# A relative path is judged from the payload's working folder.
$got = Get-Decision (Invoke-SearchGuard (New-Payload 'B' 'find . -name x.dll' 'C:\'))
Write-Result '[cwd] B find . from a cwd of C:\ -> deny' ($got -eq 'deny') "(got $got)"
$got = Get-Decision (Invoke-SearchGuard (New-Payload 'P' 'Get-ChildItem -Recurse -Filter x.dll' 'C:\'))
Write-Result '[cwd] P Get-ChildItem -Recurse from a cwd of C:\ -> deny' ($got -eq 'deny') "(got $got)"
$got = Get-Decision (Invoke-SearchGuard (New-Payload 'P' 'Get-ChildItem sg-no-such-file-4711.dll -Recurse' 'C:\'))
Write-Result '[cwd] P a missing leaf from a cwd of C:\ -> deny' ($got -eq 'deny') "(got $got)"

# Payloads that are not shell calls get no answer at all.
foreach ($raw in @('not json', '', '{"toolName":"view","toolArgs":"{\"path\":\"find / -name x\"}"}',
        '{"tool_name":"Read","tool_input":{"file_path":"/"}}', '[1,2]')) {
    $answer = Invoke-SearchGuard $raw
    Write-Result "[payload] no answer for: $raw" (-not $answer) "(got $answer)"
}

# The deny reason teaches the bounded alternative.
$reason = (Invoke-SearchGuard (New-Payload 'B' 'find / -name x' $repo) | ConvertFrom-Json).hookSpecificOutput.permissionDecisionReason
Write-Result '[reason] names a bounded find and the system lookups' ($reason -match 'maxdepth' -and $reason -match 'Get-Command') "(got $reason)"

# The Copilot matcher names exactly the tools the guard judges (Copilot starts an unscoped hook before every call).
$regs = @((Join-Path (Split-Path -Parent $Guard) 'search-guard.copilot.json'),
          (Join-Path (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $Guard))) '.github\hooks\search-guard.json'))
$reg = $regs | Where-Object { Test-Path $_ } | Select-Object -First 1
if ($reg) {
    $entries = @((Get-Content -Raw $reg | ConvertFrom-Json).hooks.preToolUse | Where-Object { ($_ | ConvertTo-Json -Compress) -match 'search_guard' })
    Write-Result "[matcher] $reg registers the guard once" ($entries.Count -eq 1) "(found $($entries.Count))"
    foreach ($e in $entries) {
        # Copilot denies every call of a hook that exits non-zero: both commands must end in a success.
        Write-Result "[contract] the powershell command ends in '; exit 0'" ([string]$e.powershell -match ';\s*exit 0\s*$') "($($e.powershell))"
        Write-Result "[contract] the bash command ends in '|| true'" ([string]$e.bash -match '\|\|\s*true\s*$') "($($e.bash))"
        $rx = '^(?:' + $e.matcher + ')$'
        foreach ($t in $CopilotShellTools) { Write-Result "[matcher] '$($e.matcher)' matches $t" ($t -cmatch $rx) '' }
        foreach ($t in @('view', 'edit', 'create', 'grep', 'glob', 'read_powershell', 'write_powershell', 'stop_powershell', 'task', 'web_fetch', 'sql')) {
            Write-Result "[matcher] '$($e.matcher)' skips $t" (-not ($t -cmatch $rx)) ''
        }
    }
} else {
    Write-Output 'SKIP  [matcher] no Copilot registration found beside the guard or in .github/hooks - not checked'
}

# The contract, run the way both tools run it: a new powershell.exe, payload on stdin, exit code 0 every time.
$exe = (Get-Command powershell.exe -ErrorAction SilentlyContinue).Source
if (-not $exe) { $exe = (Get-Command pwsh -ErrorAction SilentlyContinue).Source }
$contract = @(
    @((New-Payload 'B' 'find / -name x' $repo), 'claude-deny'),
    @((New-Payload 'cp' 'Get-ChildItem C:\ -Recurse' $repo), 'copilot-deny'),
    @((New-Payload 'cb' 'git status' $repo), 'empty'),
    @('{"toolName":"view","toolArgs":"{}"}', 'empty'),
    @('this is not json', 'empty')
)
foreach ($c in $contract) {
    $payload, $expect = $c
    $out = ($payload | & $exe -NoProfile -ExecutionPolicy Bypass -File $Guard) -join "`n"
    $code = $LASTEXITCODE
    $ok = $code -eq 0
    if ($expect -eq 'empty') { $ok = $ok -and -not $out }
    else {
        $j = $null
        try { $j = $out | ConvertFrom-Json } catch { }
        if ($expect -eq 'claude-deny') { $ok = $ok -and $j -and $j.hookSpecificOutput.permissionDecision -eq 'deny' -and $j.hookSpecificOutput.hookEventName -eq 'PreToolUse' }
        else { $ok = $ok -and $j -and $j.permissionDecision -eq 'deny' -and -not $j.PSObject.Properties.Match('hookSpecificOutput').Count }
    }
    Write-Result "[contract] $expect, exit 0" $ok "(exit $code, out: $out)"
}

# In Constrained Language Mode (WDAC/AppLocker) most .NET calls throw, the catch block's too. The guard must still
# exit 0 and print no error, or Copilot would deny every shell call on such a machine.
$clmPayload = New-Payload 'cb' 'find / -name x' $repo
$clmCommand = "`$ExecutionContext.SessionState.LanguageMode = 'ConstrainedLanguage'; & '$Guard'; exit `$LASTEXITCODE"
$ErrorActionPreference = 'Continue'   # under Stop, Windows PowerShell 5.1 turns the child's redirected stderr into a throw
try { $out = (@($clmPayload | & $exe -NoProfile -ExecutionPolicy Bypass -Command $clmCommand 2>&1) | ForEach-Object { "$_" }) -join "`n" }
catch { $out = "the test runner failed: $_" }
$code = $LASTEXITCODE
$ErrorActionPreference = 'Stop'
Write-Result '[contract] Constrained Language Mode: exit 0, no error' ($code -eq 0 -and $out -notmatch 'Cannot|Exception|error') "(exit $code, out: $out)"

# The .claude/settings.json entries, run the way each tool runs them on Windows. Claude Code sets CLAUDE_PROJECT_DIR and
# runs them in Git Bash, or in PowerShell when there is no Git Bash. Copilot CLI runs the same entries in PowerShell,
# without the variable and not from the project (github/copilot-cli#4001), and denies the tool call when a preToolUse
# hook fails - so there they must print nothing and exit 0. So must bash on a machine with no PowerShell (macOS, Linux).
function Get-HookCommandSource {
    # The project's registered commands, else (in the library) the ones deliver-packet's search-guard.md writes.
    $scripts = Split-Path -Parent $Guard
    $settings = Join-Path (Split-Path -Parent $scripts) 'settings.json'
    if (Test-Path -LiteralPath $settings) {
        try {
            $hooks = (Get-Content -Raw -LiteralPath $settings | ConvertFrom-Json).hooks
            $commands = @(foreach ($entry in @($hooks.PreToolUse) + @($hooks.SessionStart)) {
                foreach ($h in @($entry.hooks)) { [string]$h.command } })
            if ($commands -match 'search_guard\.ps1') { return @{ Origin = '.claude/settings.json'; Commands = $commands } }
        } catch { }
    }
    $doc = Join-Path $scripts '..\..\..\..\skills\deliver-packet\search-guard.md'
    if (Test-Path -LiteralPath $doc) {
        $commands = @(foreach ($m in [regex]::Matches((Get-Content -Raw -LiteralPath $doc), '"command":\s*("(?:[^"\\\r\n]|\\.)*")')) {
            [string](ConvertFrom-Json $m.Groups[1].Value) })
        return @{ Origin = 'search-guard.md'; Commands = $commands }
    }
    return $null
}

function Find-GitBash {
    # Git for Windows' bash, which Claude Code runs hooks in; never a WSL launcher (System32, WindowsApps).
    $candidates = @($env:CLAUDE_CODE_GIT_BASH_PATH)
    foreach ($d in ($env:PATH -split ';')) {
        if (-not $d) { continue }
        try {
            if (Test-Path -LiteralPath (Join-Path $d 'git.exe')) {
                $candidates += (Join-Path $d '..\bin\bash.exe'), (Join-Path $d '..\..\bin\bash.exe')
            }
            $candidates += Join-Path $d 'bash.exe'
        } catch { }
    }
    foreach ($c in $candidates) {
        if (-not $c) { continue }
        try { $full = [IO.Path]::GetFullPath($c) } catch { continue }
        if ((Test-Path -LiteralPath $full -PathType Leaf) -and $full -notmatch 'WindowsApps' -and
            -not $full.StartsWith($env:SystemRoot, [StringComparison]::OrdinalIgnoreCase)) { return $full }
    }
    return $null
}

function ConvertTo-WindowsArgument([string]$Arg) {
    # One argument of a Windows command line, quoted the way the C runtime (and MSYS2's bash) splits it back.
    if ($Arg -and $Arg -notmatch '[\s"]') { return $Arg }
    $s = '"'
    $slashes = 0
    foreach ($ch in $Arg.ToCharArray()) {
        if ($ch -eq '\') { $slashes++; continue }
        if ($ch -eq '"') { $s += ('\' * (2 * $slashes + 1)) + '"' } else { $s += ('\' * $slashes) + $ch }
        $slashes = 0
    }
    return $s + ('\' * (2 * $slashes)) + '"'
}

function Invoke-HookShape([string]$Exe, [string[]]$Arguments, [string]$Cwd, [string]$ProjectDir, [string]$Payload,
                          [string]$PathOverride, [string]$HookDepth) {
    # $ProjectDir empty = Copilot CLI: no CLAUDE_PROJECT_DIR in the hook's environment.
    $psi = New-Object Diagnostics.ProcessStartInfo
    $psi.FileName = $Exe
    $psi.Arguments = (@($Arguments | ForEach-Object { ConvertTo-WindowsArgument $_ })) -join ' '
    $psi.WorkingDirectory = $Cwd
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true
    $psi.RedirectStandardInput = $true
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    if ($ProjectDir) { $psi.EnvironmentVariables['CLAUDE_PROJECT_DIR'] = $ProjectDir }
    else { $psi.EnvironmentVariables.Remove('CLAUDE_PROJECT_DIR') }
    if ($PathOverride) { $psi.EnvironmentVariables['PATH'] = $PathOverride }
    # Copilot CLI 1.0.112 sets COPILOT_HOOK_DEPTH for the hooks it runs (measured 2026-11-18); nothing else does.
    if ($HookDepth) { $psi.EnvironmentVariables['COPILOT_HOOK_DEPTH'] = $HookDepth }
    else { $psi.EnvironmentVariables.Remove('COPILOT_HOOK_DEPTH') }
    try { $p = [Diagnostics.Process]::Start($psi) } catch { return @{ Code = 'not started'; Out = ''; Err = "$_" } }
    $out = $p.StandardOutput.ReadToEndAsync()
    $err = $p.StandardError.ReadToEndAsync()
    $p.StandardInput.Write($Payload)
    $p.StandardInput.Close()
    if (-not $p.WaitForExit(60000)) { $p.Kill(); return @{ Code = 'timeout'; Out = ''; Err = '' } }
    [void]$out.Wait(10000); [void]$err.Wait(10000)
    return @{ Code = $p.ExitCode; Out = ([string]$out.Result).Trim(); Err = ([string]$err.Result).Trim() }
}

$source = Get-HookCommandSource
if (-not $source -or -not ($source.Commands -match 'search_guard\.ps1')) {
    Write-Output 'SKIP  [hook-command] no search_guard.ps1 entry in .claude/settings.json beside the scripts, and no deliver-packet search-guard.md - not checked'
} else {
    $hookProject = Join-Path ([IO.Path]::GetTempPath()) ('search-guard-hook-' + [guid]::NewGuid().ToString('N').Substring(0, 8))
    $hookScripts = Join-Path $hookProject '.claude\scripts'
    [void](New-Item -ItemType Directory -Force -Path $hookScripts)
    try {
        Copy-Item -LiteralPath $Guard -Destination (Join-Path $hookScripts 'search_guard.ps1')
        # A stand-in for the report, reading stdin the way it does: what the real one finds depends on this machine.
        $stub = "param([switch]`$Hook)`r`n`$payload = [Console]::In.ReadToEnd()`r`n" +
                "`$state = if (`$payload -match '`"hook_event_name`"') { 'payload' } else { 'no payload' }`r`n" +
                "'report ran' + `$(if (`$Hook) { ' -Hook' } else { '' }) + ' ' + `$state`r`n"
        [IO.File]::WriteAllText((Join-Path $hookScripts 'leftover_processes.ps1'), $stub, [Text.Encoding]::ASCII)
        $copilotCwd = [IO.Path]::GetTempPath()
        $bash = Find-GitBash
        $pwsh = (Get-Command pwsh -ErrorAction SilentlyContinue).Source
        # A teammate on macOS or Linux: bash, and no powershell on PATH. Both tools must still get a success.
        $noPsPath = $null
        if ($bash) {
            $noPsPath = (@((Split-Path -Parent $bash), (Join-Path (Split-Path -Parent (Split-Path -Parent $bash)) 'usr\bin')) |
                         Where-Object { Test-Path -LiteralPath $_ }) -join ';'
            $probe = Invoke-HookShape $bash @('-c', 'command -v powershell powershell.exe pwsh || true') $copilotCwd '' '' $noPsPath
            if ($probe.Out) { $noPsPath = $null }
        }
        $sessionStart = ConvertTo-Json -Compress -InputObject @{ hook_event_name = 'SessionStart'; session_id = 't'
                                                                 cwd = $hookProject; source = 'startup' }
        $hookEntries = @(
            @{ Name = 'search guard'; Pattern = 'search_guard\.ps1'; Claude = 'deny'
               ClaudeB = (New-Payload 'B' 'find / -name x' $hookProject)
               ClaudeP = (New-Payload 'P' 'Get-ChildItem C:\ -Recurse -Filter x.dll' $hookProject)
               Copilot = (ConvertTo-Json -Compress -InputObject @{ hook_event_name = 'PreToolUse'; session_id = 't'
                          cwd = $hookProject; tool_name = 'Bash'; tool_input = @{ command = 'find / -name x' } }) },
            @{ Name = 'leftover report'; Pattern = 'leftover_processes\.ps1'; Claude = 'report ran -Hook payload'
               ClaudeB = $sessionStart; ClaudeP = $sessionStart; Copilot = $sessionStart }
        )
        foreach ($he in $hookEntries) {
            $command = @($source.Commands | Where-Object { $_ -match $he.Pattern })[0]
            if (-not $command) {
                Write-Output "SKIP  [hook-command] no $($he.Name) entry in $($source.Origin) - not checked"
                continue
            }
            # Claude Code rewrites the placeholder to PowerShell's form when PowerShell runs a hook; Copilot does not.
            $claudePs = $command.Replace('${CLAUDE_PROJECT_DIR}', '${env:CLAUDE_PROJECT_DIR}')
            $shapes = @(@{ Label = 'claude, PowerShell'; Exe = $exe; Args = @('-NoProfile', '-Command', $claudePs)
                           Cwd = $hookProject; Dir = $hookProject; Payload = $he.ClaudeP; Expect = $he.Claude })
            if ($bash) {
                $shapes += @{ Label = 'claude, Git Bash'; Exe = $bash; Args = @('-c', $command)
                              Cwd = $hookProject; Dir = $hookProject; Payload = $he.ClaudeB; Expect = $he.Claude }
            } else {
                Write-Output "SKIP  [hook-command] $($he.Name), Git Bash shapes: no Git for Windows bash found - not checked"
            }
            if ($noPsPath) {
                $shapes += @{ Label = 'bash without PowerShell (macOS, Linux)'; Exe = $bash; Args = @('-c', $command)
                              Cwd = $hookProject; Dir = $hookProject; Payload = $he.ClaudeB; Expect = 'nothing'; Path = $noPsPath }
            } elseif ($bash) {
                Write-Output "SKIP  [hook-command] $($he.Name), bash without PowerShell: PowerShell is still on Git's own PATH - not checked"
            }
            $shapes += @{ Label = 'copilot, powershell, no CLAUDE_PROJECT_DIR'; Exe = $exe; Args = @('-NoProfile', '-Command', $command)
                          Cwd = $copilotCwd; Dir = ''; Payload = $he.Copilot; Expect = 'nothing' }
            # Copilot CLI 1.0.112: CLAUDE_PROJECT_DIR set, run from the project, COPILOT_HOOK_DEPTH set (2026-11-18).
            $shapes += @{ Label = 'copilot 1.0.112, powershell, CLAUDE_PROJECT_DIR and COPILOT_HOOK_DEPTH set'; Exe = $exe
                          Args = @('-NoProfile', '-Command', $command); Cwd = $hookProject; Dir = $hookProject
                          Payload = $he.Copilot; Expect = 'nothing'; Depth = '1' }
            if ($pwsh) {
                $shapes += @{ Label = 'copilot, pwsh, no CLAUDE_PROJECT_DIR'; Exe = $pwsh; Args = @('-NoProfile', '-Command', $command)
                              Cwd = $copilotCwd; Dir = ''; Payload = $he.Copilot; Expect = 'nothing' }
            }
            foreach ($s in $shapes) {
                $r = Invoke-HookShape $s.Exe $s.Args $s.Cwd $s.Dir $s.Payload $s.Path $s.Depth
                if ($s.Expect -eq 'nothing') { $got = if ($r.Out) { $r.Out } else { 'nothing' } }
                elseif ($s.Expect -eq 'deny') {
                    $got = 'junk'
                    try { if (($r.Out | ConvertFrom-Json).hookSpecificOutput.permissionDecision -eq 'deny') { $got = 'deny' } } catch { }
                } else { $got = $r.Out }
                $ok = $r.Code -eq 0 -and $got -eq $s.Expect
                $detail = "(exit $($r.Code), out: $($r.Out), err: $($r.Err))"
                if ($detail.Length -gt 400) { $detail = $detail.Substring(0, 400) + ' ...)' }
                Write-Result "[hook-command] $($he.Name), $($s.Label) -> $($s.Expect), exit 0 (from $($source.Origin))" $ok $detail
            }
        }
    } finally {
        Remove-Item -LiteralPath $hookProject -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# The crash path: when the guard itself fails, it asks only about a command that names a search tool. The probe is
# the command, never the whole payload - the payload holds the tool's own name, which the pattern matches.
foreach ($c in @(
        @('B', 'npm test', 'allow'), @('P', 'Get-Content README.md', 'allow'), @('cb', 'npm test', 'allow'),
        @('cp', 'git status', 'allow'), @('B', 'find . -name x', 'ask'), @('cb', 'grep -r Foo src', 'ask'),
        @('P', 'gci -Recurse', 'ask'))) {
    $got = Get-Decision (Get-GuardCrashAnswer (New-Payload $c[0] $c[1] $repo) 'planted failure')
    Write-Result "[crash]  $($c[0]) '$($c[1])' -> $($c[2])" ($got -eq $c[2]) "(got $got)"
}
$got = Get-Decision (Get-GuardCrashAnswer '{"tool_name":"Bash","tool_input":{"command":"find / -name x' 'planted failure')
Write-Result "[crash]  unparsable payload naming find -> ask" ($got -eq 'ask') "(got $got)"
$got = Get-Decision (Get-GuardCrashAnswer '{"tool_name":"Bash","tool_input":{"command":"npm test' 'planted failure')
Write-Result "[crash]  unparsable payload, only the tool name matches -> allow" ($got -eq 'allow') "(got $got)"

# The leftover report's classification, fed made-up process records - nothing on this machine is read. Leftovers are
# grouped by tree, and a tree that may be a session's background job is listed but kept out of the stop command
# (2026-10-20: another project's live test run, a run_in_background job whose shell had exited, was offered for
# Stop-Process, and its child row read "parent is gone" while its parent was alive).
$reportScript = Join-Path (Split-Path -Parent $Guard) 'leftover_processes.ps1'
if (-not (Test-Path -LiteralPath $reportScript)) {
    Write-Output 'SKIP  [report] no leftover_processes.ps1 beside the guard - not checked'
} else {
    . $reportScript
    $now = Get-Date
    $cutoff = $now.AddMinutes(-10)
    function New-Proc([int]$Id, [int]$Parent, [string]$Name, [int]$AgeMin, [string]$Cmd = 'x') {
        [pscustomobject]@{ ProcessId = $Id; ParentProcessId = $Parent; Name = $Name; CreationDate = $now.AddMinutes(-$AgeMin)
                           HandleCount = 100; WorkingSetSize = 1MB; CommandLine = $Cmd }
    }
    # 0.197.4: -Also adds names to the watch list (a stopped test run's python.exe or postgres.exe).
    $names = @(Add-WatchedNames @('find.exe') @('python', 'Postgres.exe,node', 'bad name', 'find', 'x;y'))
    Write-Result "[report] -Also adds names with .exe, splits on commas, skips unsafe names and duplicates" `
        (($names -join ' ') -eq 'find.exe python.exe Postgres.exe node.exe') "(got $($names -join ' '))"
    # name, watched candidates, live parents (PID -> start), every live process, expected: trees, flagged trees,
    # PIDs the stop line must name ('' = no stop line), PIDs it must not name
    $job = 'sh C:/Users/U/AppData/Local/Temp/claude/C--Development-Other/1234/scratchpad/suite/run_segments.sh'
    $peerRoot = New-Proc 25404 23664 'sh.exe' 41 $job
    $peerKid = New-Proc 7840 25404 'sh.exe' 27 $job
    $uv = New-Proc 9000 7840 'uv.exe' 27 'uv run pytest -q packages/x/tests'
    $bashOrphan = New-Proc 500 400 'bash.exe' 60 'bash -c find / -name x'
    $findKid = New-Proc 501 500 'find.exe' 60 'find / -name x'
    $console = New-Proc 502 500 'conhost.exe' 60 'conhost.exe 0xffffffff'
    $reused = New-Proc 600 601 'node.exe' 30 'node server.js'
    $young = New-Proc 700 401 'find.exe' 5 'find / -name y'
    $plainRoot = New-Proc 800 402 'sh.exe' 30 'sh -c ./long.sh'
    $plainUv = New-Proc 801 800 'uv.exe' 30 'uv run pytest'
    $reportCases = @(
        @{ Name = 'peer shape: temp-folder job with a live uv child -> one flagged tree, no stop line'
           C = @($peerRoot, $peerKid); P = @{ 25404 = $peerRoot.CreationDate }; A = @($peerRoot, $peerKid, $uv)
           Trees = 1; Flagged = 1; Stop = ''; NotStop = @(25404, 7840) },
        @{ Name = 'a job under Temp\claude\ with no child -> flagged'
           C = @($peerRoot); P = @{}; A = @($peerRoot); Trees = 1; Flagged = 1; Stop = ''; NotStop = @(25404) },
        @{ Name = 'an orphan with a live child outside the watched names -> flagged'
           C = @($plainRoot); P = @{}; A = @($plainRoot, $plainUv); Trees = 1; Flagged = 1; Stop = ''; NotStop = @(800) },
        @{ Name = 'orphaned bash and its find, plus a conhost child -> one tree, both in the stop line'
           C = @($bashOrphan, $findKid); P = @{ 500 = $bashOrphan.CreationDate }; A = @($bashOrphan, $findKid, $console)
           Trees = 1; Flagged = 0; Stop = '500,501'; NotStop = @(502) },
        @{ Name = 'parent PID reused by a newer process -> still a leftover'
           C = @($reused); P = @{ 601 = $now.AddMinutes(-1) }; A = @($reused); Trees = 1; Flagged = 0; Stop = '600'; NotStop = @() },
        @{ Name = 'an orphan younger than the cutoff -> not listed'
           C = @($young); P = @{}; A = @($young); Trees = 0; Flagged = 0; Stop = $null; NotStop = @() },
        @{ Name = 'one plain tree and one job -> the stop line names the plain tree only'
           C = @($bashOrphan, $findKid, $peerRoot, $peerKid); P = @{ 500 = $bashOrphan.CreationDate; 25404 = $peerRoot.CreationDate }
           A = @($bashOrphan, $findKid, $peerRoot, $peerKid, $uv); Trees = 2; Flagged = 1; Stop = '500,501'; NotStop = @(25404, 7840) }
    )
    foreach ($rc in $reportCases) {
        $all = $rc.A
        $trees = @(Group-LeftoverTrees $rc.C $rc.P $cutoff { $all }.GetNewClosure())
        $flagged = @($trees | Where-Object { $_.Flag }).Count
        $ok = ($trees.Count -eq $rc.Trees) -and ($flagged -eq $rc.Flagged)
        $detail = "(got $($trees.Count) tree(s), $flagged flagged)"
        if ($ok -and $trees.Count -gt 0) {
            $text = Format-Report $trees 10
            $stop = @($text -split "`n" | Where-Object { $_ -match '^\s*Stop-Process -Id ' })
            if ($rc.Stop) {
                $ok = $stop.Count -eq 1 -and $stop[0] -match ('-Id ' + [regex]::Escape($rc.Stop) + ' ')
            } else {
                $ok = $stop.Count -eq 0
            }
            foreach ($n in $rc.NotStop) { if ($stop.Count -gt 0 -and $stop[0] -match "\b$n\b") { $ok = $false } }
            # A child row names its parent; only a tree's first row is the one whose parent is gone.
            if ($rc.C -contains $peerKid -and $text -notmatch '\[under 25404\]') { $ok = $false }
            $detail = "(stop line: $(if ($stop.Count) { $stop[0].Trim() } else { 'none' }))"
        }
        Write-Result "[report] $($rc.Name)" $ok $detail
    }
}

# A long command (a big heredoc) is still answered quickly.
$big = "cat > big.md <<'EOF'`n" + (('find / -name x is what not to do; ' * 30 + "`n") * 60) + "EOF`necho ok"
$sw = [Diagnostics.Stopwatch]::StartNew()
$got = Get-Decision (Invoke-SearchGuard (New-Payload 'B' $big $repo))
$ms = $sw.ElapsedMilliseconds
Write-Result "[speed] a $($big.Length)-char command is allowed in $ms ms (limit 5000)" ($got -eq 'allow' -and $ms -lt 5000) "(got $got)"

# 0.190.31: a heredoc fed to bash is parsed as a script, the slower path.
$bigBash = "bash <<'EOF'`n" + (('echo find is fine here; ls -la; ' * 30 + "`n") * 60) + "EOF"
$sw = [Diagnostics.Stopwatch]::StartNew()
$got = Get-Decision (Invoke-SearchGuard (New-Payload 'B' $bigBash $repo))
$ms = $sw.ElapsedMilliseconds
Write-Result "[speed] a $($bigBash.Length)-char script fed to bash is allowed in $ms ms (limit 5000)" ($got -eq 'allow' -and $ms -lt 5000) "(got $got)"

$total = $script:passed + $script:failed
Write-Output "$($script:passed)/$total passed"
if ($script:failed -gt 0) { exit 1 }
exit 0
