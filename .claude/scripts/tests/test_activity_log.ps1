# Tests for activity_log.ps1 - run them after every change to the script:
#
#   powershell -NoProfile -ExecutionPolicy RemoteSigned -File .claude/scripts/tests/test_activity_log.ps1
#   powershell -NoProfile -ExecutionPolicy RemoteSigned -File .claude/scripts/tests/test_activity_log.ps1 <activity_log.ps1>
#
# Every case runs the script in a new Windows PowerShell (powershell.exe), as both tools' registrations do, reading the
# payload on stdin, in a temp
# folder that it deletes. The payloads are the shapes measured on 2026-11-18 (Claude Code's hooks docs; Copilot CLI
# 1.0.112), with every content field replaced by a marker that must never reach the log. Prints one PASS/FAIL line
# per case, then "N/N passed". Exit code 1 when any case fails. Keep this file ASCII.

param([string]$Script)
$ErrorActionPreference = 'Stop'
if (-not $Script) { $Script = Join-Path (Split-Path -Parent $PSScriptRoot) 'activity_log.ps1' }
$Script = (Resolve-Path $Script).ProviderPath

$script:passed = 0
$script:failed = 0
$Marker = 'SECRET-CONTENT-MARKER'
$Allowed = 't,tool,event,session,agent_id,agent_type,parent'

function Write-Result([string]$Label, [bool]$Ok, [string]$Detail) {
    if ($Ok) { $script:passed++; Write-Output "PASS  $Label" }
    else { $script:failed++; Write-Output "FAIL  $Label  $Detail" }
}

$ClaudeStart = '{"session_id":"s-1","transcript_path":"C:/x/' + $Marker + '.jsonl","cwd":"C:/x","hook_event_name":"SubagentStart","agent_id":"agent-abc","agent_type":"Explore"}'
$ClaudeStop = '{"session_id":"s-1","transcript_path":"C:/x/t.jsonl","cwd":"C:/x","hook_event_name":"SubagentStop","stop_hook_active":false,"agent_id":"agent-abc","agent_type":"Explore","agent_transcript_path":"C:/x/' + $Marker + '.jsonl","last_assistant_message":"' + $Marker + '"}'
$CopilotStart = '{"sessionId":"s-2","timestamp":1795008085163,"cwd":"C:\\x","agentName":"explore","agentId":"a-659","agentType":"explore","parentAgentId":"s-2"}'
$CopilotStop = '{"sessionId":"s-2","timestamp":1795008104106,"cwd":"C:\\x","agentId":"a-659","agentType":"explore","agentName":"explore","response":"' + $Marker + '","stopReason":"end_turn","stop_hook_active":false,"parentAgentId":"s-2"}'

# Runs the script once in a fresh temp folder; returns @{ Exit; Lines; Dir }.
function Invoke-Hook([string]$Payload, [string]$ClaudeDir, [string[]]$Arguments) {
    $dir = Join-Path ([IO.Path]::GetTempPath()) ('activity-test-' + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $dir | Out-Null
    $saved = $env:CLAUDE_PROJECT_DIR
    try {
        if ($ClaudeDir -eq '<temp>') { $env:CLAUDE_PROJECT_DIR = $dir } else { Remove-Item Env:CLAUDE_PROJECT_DIR -ErrorAction SilentlyContinue }
        Push-Location $dir
        try {
            $out = $Payload | & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $Script @Arguments
            $code = $LASTEXITCODE
        } finally { Pop-Location }
    } finally {
        if ($null -ne $saved) { $env:CLAUDE_PROJECT_DIR = $saved } else { Remove-Item Env:CLAUDE_PROJECT_DIR -ErrorAction SilentlyContinue }
    }
    $logDir = Join-Path $dir '.agent-activity'
    $lines = @()
    if (Test-Path -LiteralPath $logDir) { $lines = @(Get-ChildItem -LiteralPath $logDir -Filter '*.jsonl' | Get-Content) }
    return @{ Exit = $code; Lines = $lines; Dir = $dir; Out = "$out" }
}

function Remove-Temp($r) { Remove-Item -LiteralPath $r.Dir -Recurse -Force -ErrorAction SilentlyContinue }

function Test-Line($r, [string]$Label, [string]$Tool, [string]$Event, [string]$Type) {
    $ok = $r.Exit -eq 0 -and $r.Lines.Count -eq 1 -and $r.Out -eq ''
    $detail = "exit=$($r.Exit) lines=$($r.Lines.Count) out='$($r.Out)'"
    if ($ok) {
        $rec = $r.Lines[0] | ConvertFrom-Json
        $keys = ($rec.PSObject.Properties | ForEach-Object { $_.Name }) -join ','
        $ok = $keys -eq $Allowed -and $rec.tool -eq $Tool -and $rec.event -eq $Event -and $rec.agent_type -eq $Type -and
              $r.Lines[0] -notmatch $Marker -and $r.Lines[0] -match '^\{"t":"\d{4}-\d\d-\d\dT\d\d:\d\d:\d\d\.\d{3}Z"'
        $detail = "keys=$keys line=$($r.Lines[0])"
    }
    Write-Result $Label $ok $detail
}

# --- Claude Code: logged only where CLAUDE_PROJECT_DIR is set -------------------------------------------------
$r = Invoke-Hook $ClaudeStart '<temp>' @()
Test-Line $r 'claude start: one allow-listed line' 'claude' 'start' 'Explore'
Write-Result 'claude start: the folder ignores itself' ((Get-Content -LiteralPath (Join-Path $r.Dir '.agent-activity/.gitignore') -Raw) -eq "*`n") ''
Remove-Temp $r

$r = Invoke-Hook $ClaudeStop '<temp>' @()
Test-Line $r 'claude stop: transcript path and message dropped' 'claude' 'stop' 'Explore'
Remove-Temp $r

$r = Invoke-Hook $ClaudeStart '' @()
Write-Result 'claude payload without CLAUDE_PROJECT_DIR (Copilot running the Claude entry): nothing written' ($r.Exit -eq 0 -and $r.Lines.Count -eq 0 -and -not (Test-Path (Join-Path $r.Dir '.agent-activity'))) "exit=$($r.Exit) lines=$($r.Lines.Count)"
Remove-Temp $r

$r = Invoke-Hook ($ClaudeStart -replace 'SubagentStart', 'PostToolUse') '<temp>' @()
Write-Result 'claude payload of another event: nothing written' ($r.Exit -eq 0 -and $r.Lines.Count -eq 0) "exit=$($r.Exit) lines=$($r.Lines.Count)"
Remove-Temp $r

# Copilot runs the .claude/settings.json entries too, with a Claude-shaped payload that carries `timestamp`, and on
# SubagentStop it set CLAUDE_PROJECT_DIR (measured 2026-11-18, Copilot CLI 1.0.112): not logged, its own entry logs it.
$CopilotAsClaude = '{"hook_event_name":"SubagentStop","session_id":"s-2","timestamp":"2026-11-18T13:50:35.000Z","cwd":"C:/x","agent_id":"a-659","agent_type":"general-purpose","parent_agent_id":"s-2","last_assistant_message":"' + $Marker + '"}'
$r = Invoke-Hook $CopilotAsClaude '<temp>' @()
Write-Result 'claude-shaped payload with a timestamp (Copilot running the Claude entry): nothing written' ($r.Exit -eq 0 -and $r.Lines.Count -eq 0) "exit=$($r.Exit) lines=$($r.Lines.Count)"
Remove-Temp $r

# --- Copilot CLI: its own registration passes -Event ------------------------------------------------------------
$r = Invoke-Hook $CopilotStart '' @('-Event', 'start')
Test-Line $r 'copilot start: one allow-listed line' 'copilot' 'start' 'explore'
if ($r.Lines.Count -eq 1) {
    # Read as text: PowerShell 7's ConvertFrom-Json turns an ISO string into a DateTime.
    $ok = $r.Lines[0].StartsWith('{"t":"2026-11-18T13:21:25.163Z"')
    Write-Result 'copilot start: time taken from the payload timestamp' $ok "line=$($r.Lines[0])"
}
Remove-Temp $r

$r = Invoke-Hook $CopilotStop '' @('-Event', 'stop')
Test-Line $r 'copilot stop: response dropped' 'copilot' 'stop' 'explore'
Remove-Temp $r

$r = Invoke-Hook $CopilotStart '' @()
Write-Result 'copilot payload without -Event: nothing written' ($r.Exit -eq 0 -and $r.Lines.Count -eq 0) "exit=$($r.Exit) lines=$($r.Lines.Count)"
Remove-Temp $r

# The same start twice (both registrations firing): one line.
$dir = Join-Path ([IO.Path]::GetTempPath()) ('activity-test-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $dir | Out-Null
Push-Location $dir
try {
    1..2 | ForEach-Object { $CopilotStart | & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $Script -Event start }
} finally { Pop-Location }
$n = @(Get-ChildItem -LiteralPath (Join-Path $dir '.agent-activity') -Filter '*.jsonl' | Get-Content).Count
Write-Result 'the same start for the same agent twice: one line' ($n -eq 1) "lines=$n"
Remove-Item -LiteralPath $dir -Recurse -Force -ErrorAction SilentlyContinue

# Two stops for one agent (the waiting check sent it back): both written, the last is its end.
$r = Invoke-Hook $ClaudeStop '<temp>' @()
$saved = $env:CLAUDE_PROJECT_DIR; $env:CLAUDE_PROJECT_DIR = $r.Dir; Push-Location $r.Dir
try { $ClaudeStop | & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $Script } finally {
    Pop-Location; if ($null -ne $saved) { $env:CLAUDE_PROJECT_DIR = $saved } else { Remove-Item Env:CLAUDE_PROJECT_DIR -ErrorAction SilentlyContinue } }
$n = @(Get-ChildItem -LiteralPath (Join-Path $r.Dir '.agent-activity') -Filter '*.jsonl' | Get-Content).Count
Write-Result 'two stops for one agent: both written' ($n -eq 2) "lines=$n"
Remove-Temp $r

# The same agent id in two sessions: both starts written.
$r = Invoke-Hook $CopilotStart '' @('-Event', 'start')
Push-Location $r.Dir
try { ($CopilotStart -replace '"sessionId":"s-2"', '"sessionId":"s-3"') | & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $Script -Event start } finally { Pop-Location }
$n = @(Get-ChildItem -LiteralPath (Join-Path $r.Dir '.agent-activity') -Filter '*.jsonl' | Get-Content).Count
Write-Result 'one agent id in two sessions: both starts written' ($n -eq 2) "lines=$n"
Remove-Temp $r

# The delivered Claude Code entry (-Command, not -File), with CLAUDE_PROJECT_DIR set: one line.
$dir = Join-Path ([IO.Path]::GetTempPath()) ('activity-test-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path (Join-Path $dir '.claude/scripts') -Force | Out-Null
Copy-Item -LiteralPath $Script -Destination (Join-Path $dir '.claude/scripts/activity_log.ps1')
$entry = 'if ($env:CLAUDE_PROJECT_DIR -and -not $env:COPILOT_HOOK_DEPTH) { & (Join-Path $env:CLAUDE_PROJECT_DIR .claude/scripts/activity_log.ps1) }; exit 0'
$saved = $env:CLAUDE_PROJECT_DIR; $env:CLAUDE_PROJECT_DIR = $dir
try { $out = $ClaudeStart | & powershell.exe -NoProfile -ExecutionPolicy Bypass -Command $entry; $code = $LASTEXITCODE } finally {
    if ($null -ne $saved) { $env:CLAUDE_PROJECT_DIR = $saved } else { Remove-Item Env:CLAUDE_PROJECT_DIR -ErrorAction SilentlyContinue } }
$n = @(Get-ChildItem -LiteralPath (Join-Path $dir '.agent-activity') -Filter '*.jsonl' -ErrorAction SilentlyContinue | Get-Content).Count
Write-Result 'the delivered Claude entry (-Command form): exit 0, one line' ($code -eq 0 -and $n -eq 1 -and "$out" -eq '') "exit=$code lines=$n"

# The same entry as Copilot CLI 1.0.112 runs it: CLAUDE_PROJECT_DIR and COPILOT_HOOK_DEPTH set (2026-11-18). Nothing.
Remove-Item -LiteralPath (Join-Path $dir '.agent-activity') -Recurse -Force -ErrorAction SilentlyContinue
$saved = $env:CLAUDE_PROJECT_DIR; $env:CLAUDE_PROJECT_DIR = $dir; $env:COPILOT_HOOK_DEPTH = '1'
try { $out = $ClaudeStart | & powershell.exe -NoProfile -ExecutionPolicy Bypass -Command $entry; $code = $LASTEXITCODE } finally {
    Remove-Item Env:COPILOT_HOOK_DEPTH -ErrorAction SilentlyContinue
    if ($null -ne $saved) { $env:CLAUDE_PROJECT_DIR = $saved } else { Remove-Item Env:CLAUDE_PROJECT_DIR -ErrorAction SilentlyContinue } }
Write-Result 'the delivered Claude entry under Copilot 1.0.112 (COPILOT_HOOK_DEPTH set): exit 0, nothing written' ($code -eq 0 -and -not (Test-Path (Join-Path $dir '.agent-activity')) -and "$out" -eq '') "exit=$code"
Remove-Item -LiteralPath $dir -Recurse -Force -ErrorAction SilentlyContinue

# Under a culture whose time separator is not ':' the line still uses ISO time.
$dir = Join-Path ([IO.Path]::GetTempPath()) ('activity-test-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $dir | Out-Null
Push-Location $dir
try {
    $wrapper = "[Threading.Thread]::CurrentThread.CurrentCulture = 'fi-FI'; & '$Script' -Event start"
    $CopilotStart | & powershell.exe -NoProfile -ExecutionPolicy Bypass -Command $wrapper
} finally { Pop-Location }
$line = @(Get-ChildItem -LiteralPath (Join-Path $dir '.agent-activity') -Filter '*.jsonl' -ErrorAction SilentlyContinue | Get-Content)
Write-Result 'fi-FI culture: ISO time and file name' ($line.Count -eq 1 -and $line[0].StartsWith('{"t":"2026-11-18T13:21:25.163Z"') -and (Test-Path (Join-Path $dir '.agent-activity/2026-11-18.jsonl'))) "line=$line"
Remove-Item -LiteralPath $dir -Recurse -Force -ErrorAction SilentlyContinue

# --- Never fails the tool -------------------------------------------------------------------------------------
foreach ($bad in @('not json {', '', '[1,2]', '{}')) {
    $r = Invoke-Hook $bad '<temp>' @('-Event', 'start')
    Write-Result "bad stdin '$bad': exit 0, no output, nothing written" ($r.Exit -eq 0 -and $r.Out -eq '' -and $r.Lines.Count -eq 0) "exit=$($r.Exit) out='$($r.Out)' lines=$($r.Lines.Count)"
    Remove-Temp $r
}

$total = $script:passed + $script:failed
Write-Output "$($script:passed)/$total passed"
if ($script:failed -gt 0) { exit 1 }
exit 0
