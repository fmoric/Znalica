# Waiting check - a Claude Code Stop hook that refuses to let a turn end on a bare WAITING: line.
#
# Why: the work_while_waiting rule says a turn that ends waiting on a job or a peer session ends with a marked
# WAITING: line, and that the line says what runs beside the wait (`beside it: <what you started>`) or why nothing
# does (`idle by choice: <why>`). Field reports (BE-HUB System, 2026-11-06 and three times on 2026-11-09) showed the
# rule text and a user memory both failing: the agent wrote its report, listed "next steps" for after the job, and
# appended a bare WAITING: line, while ready work sat open. This hook makes the suffix a check instead of a reminder.
#
# Contract (Claude Code, .claude/settings.json hooks.Stop): reads the Stop payload on stdin. If any line of
# `last_assistant_message` starts with WAITING: (upper case; after whitespace, a list marker `-`/`*`/`+`/`1.`, a `#`
# heading, or `*`/`_`/backtick emphasis) and carries none of `beside it:`, `idle by choice:` or `a wake waits on your
# answer`, it prints {"decision":"block","reason":...}, and Claude Code continues the turn with that reason. Lines
# inside a fenced code block and blockquote lines (`>`) are quotations, not the turn's own line, and are skipped.
# Otherwise it prints nothing.
# Subagents: nothing wakes a subagent whose turn has ended, so there ANY WAITING: line is sent back, whatever its
# suffix, once. Field report, BE-HUB System 2026-11-12: a developer subagent ended on "WAITING: ... wake me when they
# exit" and the work sat for 20 minutes. A subagent ends in one of two shapes (both seen on Claude Code 2.1.322):
#   - Plain text (0.188.0, hooks.SubagentStop): the payload carries `last_assistant_message`, and a block continues
#     the subagent; stop_hook_active ends the second try.
#   - A SubagentHandback tool call (0.189.1, hooks.PreToolUse, matcher SubagentHandback): SubagentStop then carries
#     no `last_assistant_message`, and a block there changes nothing - the report is already delivered (probed
#     2026-11-12). So the hand-back itself is judged: the message is `tool_input.message`, the answer is a PreToolUse
#     deny, which sends the subagent back to hand back again. Once per `agent_id`: a marker file in the temp folder
#     stands in for stop_hook_active.
#   - `stop_hook_active` true (this turn already continues because of a Stop hook) -> nothing, so it never loops.
#   - No message, a PreToolUse for another tool or without a usable `agent_id`, unreadable or malformed input, any
#     error -> nothing (the turn or the hand-back goes on as before).
# ALWAYS exits 0. A Stop hook that fails is non-blocking on Claude Code and is skipped on Copilot CLI, so failing
# open costs nothing. The Stop and SubagentStop entries in .claude/settings.json are a bash command that starts
# PowerShell only when the message contains WAITING: (skills/deliver-packet/waiting-check.md). Copilot CLI runs those
# entries in PowerShell, where they fail to parse and are skipped. The PreToolUse entry is in the guards' form instead,
# because a failing tool hook denies the call on Copilot: it parses in both shells and does nothing where
# CLAUDE_PROJECT_DIR is unset. Copilot is not covered (its agentStop input carries no last-message field).
# ASCII only: Windows PowerShell 5.1 can misread a non-ASCII script saved without a BOM.

$ErrorActionPreference = 'Stop'
try {
    $stdin = [Console]::OpenStandardInput()
    $buffer = New-Object System.IO.MemoryStream
    $stdin.CopyTo($buffer)
    # TrimStart: a byte-order mark makes Windows PowerShell 5.1's ConvertFrom-Json fail, and the check then passes
    # everything silently (the test suite's stdin carries one).
    $text = [System.Text.Encoding]::UTF8.GetString($buffer.ToArray()).TrimStart([char]0xFEFF)
    if (-not $text.Trim()) { exit 0 }
    $payload = $text | ConvertFrom-Json

    if ($payload.PSObject.Properties.Name -contains 'stop_hook_active' -and $payload.stop_hook_active -eq $true) { exit 0 }
    $event = ''
    if ($payload.PSObject.Properties.Name -contains 'hook_event_name') { $event = [string]$payload.hook_event_name }
    $handback = $event -eq 'PreToolUse'
    if ($handback) {
        # 0.189.1: the subagent's report, read before it is delivered (hooks.PreToolUse, matcher SubagentHandback).
        if ([string]$payload.tool_name -ne 'SubagentHandback') { exit 0 }
        $agentId = [string]$payload.agent_id
        if ($agentId -notmatch '^[A-Za-z0-9_-]{1,64}$') { exit 0 }
        if (-not ($payload.PSObject.Properties.Name -contains 'tool_input') -or -not $payload.tool_input) { exit 0 }
        $message = [string]$payload.tool_input.message
    } else {
        if (-not ($payload.PSObject.Properties.Name -contains 'last_assistant_message')) { exit 0 }
        $message = [string]$payload.last_assistant_message
    }
    if (-not $message) { exit 0 }

    $subagent = $handback -or ($event -eq 'SubagentStop')

    $bare = @()
    $inFence = $false
    foreach ($line in ($message -split "`r?`n")) {
        if ($line -match '^\s*(```|~~~)') { $inFence = -not $inFence; continue }
        if ($inFence) { continue }
        # -cmatch: case-sensitive, so prose such as "Waiting: for your reply" is not the marked line
        if ($line -cmatch '^\s*(?:[-*+]\s+|\d+[.)]\s+|#+\s+)?[*_`]*WAITING:') {
            if ($subagent -or $line -notmatch 'beside it:|idle by choice:|a wake waits on your answer') { $bare += $line.Trim() }
        }
    }
    if ($bare.Count -eq 0) { exit 0 }

    $reason = 'Your turn ends on a WAITING: line that says neither what runs beside the wait nor why nothing does ' +
        '(work_while_waiting). Before you end the turn, decide it from the open items, not from your report''s "next ' +
        'steps" for after the job: (1) ready work of the task you were asked to do that touches nothing the job is ' +
        'testing - do it, and end with "WAITING: ...; beside it: <what you did>"; (2) otherwise a pending item from the ' +
        'tracker, the resume pointer or a follow-up the user named - start it only if the user told you to start such ' +
        'work without asking (then "beside it: <item>"), else ask the user once with the question tool, under a ' +
        'line ending "a wake waits on your answer", and end on the answer: "beside it: <item>" or "idle by ' +
        'choice: declined" (with no question tool, ask in plain text and end "idle by choice: awaiting your ' +
        'answer above"); (3) if nothing qualifies, "idle by choice: <why>". ' +
        'The line was: ' + $bare[0]
    if ($subagent) {
        $reason = 'You are a subagent: when your turn ends, nothing wakes you, so a WAITING: line here is a stall ' +
            'nobody sees. Do not end waiting. If the command is yours to run, run it now in the foreground under a ' +
            'timeout set to its budget and report its result. If it is too long for you (a full-suite run), do not ' +
            'start it: report the commit, what you ran, and that the run is your caller''s. If something you ' +
            'started is still going, stop it or wait for it in the foreground first. If you are only quoting such a ' +
            'line, put it in a code block. The line was: ' + $bare[0]
    }
    if ($handback) {
        # Once per subagent: PreToolUse carries no stop_hook_active, so a marker file stands in for it. A subagent
        # that writes WAITING: again on its second hand-back is let through rather than looped.
        $dir = Join-Path ([System.IO.Path]::GetTempPath()) 'waiting_check'
        $marker = Join-Path $dir $agentId
        if (Test-Path -LiteralPath $marker) { exit 0 }
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
        New-Item -ItemType File -Force -Path $marker | Out-Null
        $reason = $reason -replace 'when your turn ends, nothing wakes you', 'once your report is handed back, nothing wakes you' `
            -replace 'Do not end waiting\.', 'Do not hand back waiting.' `
            -replace 'The line was: ', 'Then hand back your report again. The line was: '
        $out = @{ hookSpecificOutput = @{ hookEventName = 'PreToolUse'; permissionDecision = 'deny';
            permissionDecisionReason = $reason } } | ConvertTo-Json -Compress
    } else {
        $out = @{ decision = 'block'; reason = $reason } | ConvertTo-Json -Compress
    }
    [Console]::Out.Write($out)
} catch {
    # fail open: a broken check must never trap the user in a turn
}
exit 0
