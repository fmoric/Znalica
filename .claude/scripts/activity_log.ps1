# activity_log.ps1 - appends one line per subagent start or stop to .agent-activity/<date>.jsonl.
#
# Registered on Claude Code's SubagentStart / SubagentStop (.claude/settings.json) and on Copilot CLI's
# subagentStart / subagentStop (.github/hooks/activity-log.json). The work-monitor skill reads the lines.
#
# Contract: reads the hook payload on stdin; prints nothing; ALWAYS exits 0 (Copilot CLI denies the work of a hook
# that exits non-zero). Writes an allow-list of fields only - time, tool, event, session, agent id, agent type,
# parent - and never content: no prompt, no response, no transcript path. The folder ignores itself (.gitignore *).
#
# Which tool sent it: a payload with hook_event_name is Claude-shaped. Copilot CLI also runs .claude/settings.json
# entries and sends them a Claude-shaped payload - and on SubagentStop it set CLAUDE_PROJECT_DIR too (measured
# 2026-11-18, Copilot CLI 1.0.112). That payload carries a `timestamp`, which Claude Code's does not (its hooks docs,
# common input fields), so a Claude-shaped payload with one is skipped: Copilot's own registration logs that event.
# As a second line, a start already logged for the same session and agent id is not written again. Stops are all
# written: a subagent the waiting check sends back stops twice, and the last stop is its real end.
#
# Source: the ai-agent-templates developer packet (packets/developer/hooks/activity-log/), activity_log option.
# Tests: tests/test_activity_log.ps1. Needs Windows PowerShell 5.1 or later and nothing else. Keep this file ASCII.

param([string]$Event)   # Copilot's registration passes -Event start|stop: its payload does not name the event

$ErrorActionPreference = 'Stop'

# Run as a hook (not dot-sourced by the tests): whatever fails, exit 0, silently.
$ActivityRunAsHook = $MyInvocation.InvocationName -ne '.'
trap {
    if ($ActivityRunAsHook) { exit 0 }
    break
}

function Get-Field($obj, [string[]]$names) {
    foreach ($n in $names) {
        $p = $obj.PSObject.Properties[$n]
        if ($p -and $null -ne $p.Value -and "$($p.Value)" -ne '') { return [string]$p.Value }
    }
    return $null
}

# Returns the line to write as an ordered hashtable, or $null when this payload is not logged here.
function ConvertTo-ActivityRecord([string]$raw, [string]$claudeDir, [string]$copilotEvent) {
    if (-not $raw -or -not $raw.Trim()) { return $null }
    $p = $raw | ConvertFrom-Json
    $hookEvent = Get-Field $p @('hook_event_name')
    if ($hookEvent) {
        if (-not $claudeDir) { return $null }          # no project to log into
        if ($p.PSObject.Properties['timestamp']) { return $null }   # Copilot running the Claude entry: its own entry logs it
        $tool = 'claude'
        switch ($hookEvent) { 'SubagentStart' { $ev = 'start' } 'SubagentStop' { $ev = 'stop' } default { return $null } }
        $t = (Get-Date).ToUniversalTime()
    } else {
        $tool = 'copilot'
        if ($copilotEvent -ne 'start' -and $copilotEvent -ne 'stop') { return $null }
        $ev = $copilotEvent
        $ms = Get-Field $p @('timestamp')
        if ($ms -and $ms -match '^\d{13}$') {
            $t = ([DateTime]'1970-01-01Z').ToUniversalTime().AddMilliseconds([double]$ms)
        } else { $t = (Get-Date).ToUniversalTime() }
    }
    $rec = [ordered]@{
        t          = $t.ToString('yyyy-MM-ddTHH:mm:ss.fffZ', [Globalization.CultureInfo]::InvariantCulture)
        tool       = $tool
        event      = $ev
        session    = Get-Field $p @('session_id', 'sessionId')
        agent_id   = Get-Field $p @('agent_id', 'agentId')
        agent_type = Get-Field $p @('agent_type', 'agentType', 'agentName')
        parent     = Get-Field $p @('parent_agent_id', 'parentAgentId')
    }
    if (-not $rec.agent_id) { return $null }   # not a subagent event this log can pair
    return $rec
}

function Write-ActivityRecord($rec, [string]$root) {
    $dir = Join-Path $root '.agent-activity'
    if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    $ignore = Join-Path $dir '.gitignore'
    if (-not (Test-Path -LiteralPath $ignore)) { [IO.File]::WriteAllText($ignore, "*`n") }
    $file = Join-Path $dir ($rec.t.Substring(0, 10) + '.jsonl')
    if ($rec.event -eq 'start' -and (Test-Path -LiteralPath $file)) {
        $idPart = '"agent_id":' + (ConvertTo-Json ([string]$rec.agent_id) -Compress)
        $sesPart = '"session":' + (ConvertTo-Json ([string]$rec.session) -Compress)
        foreach ($l in [IO.File]::ReadAllLines($file)) {
            if ($l.Contains('"event":"start"') -and $l.Contains($idPart) -and $l.Contains($sesPart)) { return }
        }
    }
    $line = (New-Object PSObject -Property $rec | ConvertTo-Json -Compress)
    [IO.File]::AppendAllText($file, $line + "`n")
}

if ($ActivityRunAsHook) {
    $raw = [Console]::In.ReadToEnd()
    $rec = ConvertTo-ActivityRecord $raw $env:CLAUDE_PROJECT_DIR $Event
    if ($rec) {
        if ($rec.tool -eq 'claude') { $root = $env:CLAUDE_PROJECT_DIR } else { $root = (Get-Location).Path }
        Write-ActivityRecord $rec $root
    }
    exit 0
}
