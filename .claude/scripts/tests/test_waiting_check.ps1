# Tests for waiting_check.ps1. Run from anywhere:
#   powershell -NoProfile -ExecutionPolicy RemoteSigned -File .claude/scripts/tests/test_waiting_check.ps1
# Each case pipes a Stop payload through the script in a fresh powershell process, as Claude Code does, and checks
# whether it answered with a block. Prints one line per case and an `N/N passed` total; exits 1 on any failure.
# The blocking cases are what prove the check is live: a script that fails to parse its input passes every
# allow case too (found while writing this suite: a byte-order mark on stdin did exactly that).
# ASCII only (see waiting_check.ps1).

$script = Join-Path (Split-Path $PSScriptRoot -Parent) 'waiting_check.ps1'
if (-not (Test-Path $script)) { Write-Output "FAIL [setup] waiting_check.ps1 not found at $script"; exit 1 }

function Invoke-Check([string]$payload) {
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = 'powershell'
    $psi.Arguments = '-NoProfile -ExecutionPolicy Bypass -File "' + $script + '"'
    $psi.UseShellExecute = $false
    $psi.RedirectStandardInput = $true
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $p = [System.Diagnostics.Process]::Start($psi)
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($payload)
    $p.StandardInput.BaseStream.Write($bytes, 0, $bytes.Length)
    $p.StandardInput.Close()
    $out = $p.StandardOutput.ReadToEnd()
    $p.WaitForExit()
    return @{ Out = $out; Code = $p.ExitCode }
}

function Payload([string]$message, [bool]$active = $false, [string]$event = 'Stop') {
    return (@{ hook_event_name = $event; stop_hook_active = $active; last_assistant_message = $message } |
        ConvertTo-Json -Compress)
}
function SubPayload([string]$message, [bool]$active = $false) { return (Payload $message $active 'SubagentStop') }
# A hand-back (0.189.1): a PreToolUse payload for the SubagentHandback tool. Each run uses its own agent ids, so a
# marker left by an earlier run cannot turn a deny case into a pass; the markers are removed at the end.
$run = [guid]::NewGuid().ToString('N').Substring(0, 12)
function HbPayload([string]$message, [string]$agent, [string]$tool = 'SubagentHandback') {
    $p = @{ hook_event_name = 'PreToolUse'; tool_name = $tool; tool_input = @{ message = $message } }
    if ($agent) { $p.agent_id = "t$run$agent" }
    return ($p | ConvertTo-Json -Compress)
}

$cases = @(
    @{ Name = 'bare WAITING line blocks';            Block = $true;  In = (Payload "Report.`n`nWAITING: the batch, started 14:06, wakes me by 15:00") },
    @{ Name = 'bold bare WAITING line blocks';       Block = $true;  In = (Payload "Report.`n**WAITING:** the batch, started 14:06") },
    @{ Name = 'one bare among two lines blocks';     Block = $true;  In = (Payload "WAITING: a; beside it: fix T-1`nWAITING: b, started 14:00") },
    @{ Name = 'beside it passes';                    Block = $false; In = (Payload "Report.`nWAITING: the batch, started 14:06; beside it: T-375 fix") },
    @{ Name = 'idle by choice passes';               Block = $false; In = (Payload "WAITING: the review, started 14:06; idle by choice: every next step needs it") },
    @{ Name = 'question form passes';                Block = $false; In = (Payload "WAITING: the batch, started 14:06; a wake waits on your answer") },
    @{ Name = 'stop_hook_active passes';             Block = $false; In = (Payload "WAITING: the batch, started 14:06" $true) },
    @{ Name = 'no WAITING line passes';              Block = $false; In = (Payload "Done. PR #12 is merged.") },
    @{ Name = 'WAITING quoted mid-line passes';      Block = $false; In = (Payload "The rule asks for a ``WAITING:`` line with a suffix.") },
    @{ Name = 'missing last_assistant_message';      Block = $false; In = '{"hook_event_name":"Stop","stop_hook_active":false}' },
    @{ Name = 'malformed JSON passes';               Block = $false; In = '{"hook_event_name":' },
    @{ Name = 'empty input passes';                  Block = $false; In = '' },
    @{ Name = 'list item bare line blocks';          Block = $true;  In = (Payload "- WAITING: the batch, started 14:06") },
    @{ Name = 'numbered bare line blocks';           Block = $true;  In = (Payload "1. WAITING: the batch, started 14:06") },
    @{ Name = 'heading bare line blocks';            Block = $true;  In = (Payload "## WAITING: the batch") },
    @{ Name = 'rule example in a code fence passes'; Block = $false; In = (Payload "The form is:`n``````n WAITING: <job>, started <time>`n``````n`nDone.") },
    @{ Name = 'blockquote passes';                   Block = $false; In = (Payload "> WAITING: the batch, quoted from the rule") },
    @{ Name = 'lower-case Waiting passes';           Block = $false; In = (Payload "Waiting: for your reply on the plan.") },
    @{ Name = 'old offer suffix still passes';       Block = $false; In = (Payload "Shall I fix T-1?`nWAITING: the batch; idle by choice: awaiting an answer to the offer above") },
    @{ Name = 'non-ASCII bare line blocks';          Block = $true;  In = (Payload ("Gotovo " + [char]0x0161 + [char]0x0107 + ".`nWAITING: batch " + [char]0x017E)) },
    @{ Name = 'subagent bare line blocks';           Block = $true;  In = (SubPayload "Fixes applied.`nWAITING: the quick checks (started now) wake me when they exit") },
    @{ Name = 'subagent beside-it line blocks';      Block = $true;  In = (SubPayload "WAITING: the run, started 09:36; beside it: the report") },
    @{ Name = 'subagent idle-by-choice blocks';      Block = $true;  In = (SubPayload "WAITING: the run; idle by choice: every next step needs it") },
    @{ Name = 'subagent block names the subagent';   Block = $true;  Match = 'You are a subagent'; In = (SubPayload "WAITING: the run") },
    @{ Name = 'main-session reason is not subagent'; Block = $true;  NoMatch = 'You are a subagent'; In = (Payload "WAITING: the run") },
    @{ Name = 'reason sends an offer to the question tool'; Block = $true; Match = 'question tool'; In = (Payload "WAITING: the run") },
    @{ Name = 'reason no longer says in prose';      Block = $true;  NoMatch = 'in prose'; In = (Payload "WAITING: the run") },
    @{ Name = 'reason names the wake-waits line';    Block = $true;  Match = 'a wake waits on your answer'; In = (Payload "WAITING: the run") },
    @{ Name = 'subagent with no WAITING passes';     Block = $false; In = (SubPayload "Done: 278 tests passed in 72 s on 59316d02.") },
    @{ Name = 'subagent stop_hook_active passes';    Block = $false; In = (SubPayload "WAITING: the run" $true) },
    @{ Name = 'no hook_event_name is main session';  Block = $false; In = '{"stop_hook_active":false,"last_assistant_message":"WAITING: the run; beside it: the report"}' },
    @{ Name = 'no hook_event_name bare blocks';      Block = $true;  NoMatch = 'You are a subagent'; In = '{"stop_hook_active":false,"last_assistant_message":"WAITING: the run"}' },
    @{ Name = 'subagent fenced quote passes';        Block = $false; In = (SubPayload "The brief said:`n``````n WAITING: <job>`n``````n`nDone.") },
    @{ Name = 'handback bare line denies';           Block = $true;  Deny = $true; Match = 'You are a subagent'; In = (HbPayload "Fixes applied.`nWAITING: the checks wake me" 'a') },
    @{ Name = 'handback deny says hand back again';  Block = $true;  Deny = $true; Match = 'hand back your report again'; In = (HbPayload "WAITING: the run" 'b') },
    @{ Name = 'handback same agent second passes';   Block = $false; In = (HbPayload "WAITING: the run" 'b') },
    @{ Name = 'handback beside-it line denies';      Block = $true;  Deny = $true; In = (HbPayload "WAITING: the run; beside it: the report" 'c') },
    @{ Name = 'handback with no WAITING passes';     Block = $false; In = (HbPayload "Done: 278 tests passed." 'd') },
    @{ Name = 'handback fenced quote passes';        Block = $false; In = (HbPayload "The brief said:`n``````n WAITING: <job>`n``````n`nDone." 'e') },
    @{ Name = 'handback without agent_id passes';    Block = $false; In = (HbPayload "WAITING: the run" '') },
    @{ Name = 'PreToolUse for another tool passes';  Block = $false; In = (HbPayload "WAITING: the run" 'f' 'Bash') },
    @{ Name = 'handback without tool_input passes';  Block = $false; In = ('{"hook_event_name":"PreToolUse","tool_name":"SubagentHandback","agent_id":"t' + $run + 'g"}') },
    # 0.190.5: an agent id that is not a plain name never becomes a file name, and a cut-off payload is not judged.
    @{ Name = 'handback path-like agent id passes';  Block = $false; In = ('{"hook_event_name":"PreToolUse","tool_name":"SubagentHandback","agent_id":"../../t' + $run + 'h","tool_input":{"message":"WAITING: the run"}}') },
    @{ Name = 'handback dotted agent id passes';     Block = $false; In = ('{"hook_event_name":"PreToolUse","tool_name":"SubagentHandback","agent_id":"t' + $run + '.i","tool_input":{"message":"WAITING: the run"}}') },
    @{ Name = 'handback truncated payload passes';   Block = $false; In = ('{"hook_event_name":"PreToolUse","tool_name":"SubagentHandback","agent_id":"t' + $run + 'j","tool_input":{"message":"WAITING: the') }
)

$passed = 0
foreach ($c in $cases) {
    $r = Invoke-Check $c.In
    $denied = $r.Out -match '"permissionDecision"\s*:\s*"deny"'
    $blocked = ($r.Out -match '"decision"\s*:\s*"block"') -or $denied
    if ($r.Code -ne 0) { Write-Output "FAIL [$($c.Name)] exit code $($r.Code), expected 0"; continue }
    # A hand-back is answered in PreToolUse's form; a Stop-shaped block there would be ignored by Claude Code.
    if ($c.Block -and [bool]$c.Deny -ne $denied) { Write-Output "FAIL [$($c.Name)] deny form=$denied, expected $([bool]$c.Deny): $($r.Out)"; continue }
    if ($c.Match -and $r.Out -notmatch $c.Match) { Write-Output "FAIL [$($c.Name)] output lacks '$($c.Match)': $($r.Out)"; continue }
    if ($c.NoMatch -and $r.Out -match $c.NoMatch) { Write-Output "FAIL [$($c.Name)] output has '$($c.NoMatch)': $($r.Out)"; continue }
    if ($blocked -eq $c.Block) { $passed++; Write-Output "PASS [$($c.Name)]" }
    else { Write-Output "FAIL [$($c.Name)] blocked=$blocked, expected $($c.Block); output: $($r.Out)" }
}
Get-ChildItem -LiteralPath (Join-Path ([System.IO.Path]::GetTempPath()) 'waiting_check') -Filter "t$run*" -ErrorAction SilentlyContinue |
    Remove-Item -Force -ErrorAction SilentlyContinue
Write-Output "$passed/$($cases.Count) passed"
if ($passed -ne $cases.Count) { exit 1 }
exit 0
