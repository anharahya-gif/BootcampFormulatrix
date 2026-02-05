# IntegrationTest.ps1
# Run against running server at http://localhost:5100
# Usage: Open PowerShell, ensure server running, then: .\IntegrationTest.ps1

$base = 'http://localhost:5100'
$failures = @()

function Fail($msg) { $global:failures += $msg; Write-Host "[FAIL] $msg" -ForegroundColor Red }
function Pass($msg) { Write-Host "[PASS] $msg" -ForegroundColor Green }

Write-Host "Starting integration test against $base"

# Helper to POST JSON and return parsed body or throw with response body
function PostJson($url, $body, $headers = $null) {
    try {
        return Invoke-RestMethod -Method Post -Uri $url -Headers $headers -ContentType 'application/json' -Body ($body | ConvertTo-Json -Depth 5) -ErrorAction Stop
    } catch {
        if ($_.Exception.Response) {
            $sr = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $text = $sr.ReadToEnd(); throw "HTTP Error: $text" }
        throw $_
    }
}

# Register users (ignore conflicts)
try { PostJson "$base/api/auth/register" @{Username='itest1'; Password='password123'}; Pass 'register itest1' } catch { Write-Host "register itest1: $_" }
try { PostJson "$base/api/auth/register" @{Username='itest2'; Password='password123'}; Pass 'register itest2' } catch { Write-Host "register itest2: $_" }

# Login
try {
    $l1 = PostJson "$base/api/auth/login" @{Username='itest1'; Password='password123'}
    $t1 = $l1.Token
    if (-not $t1) { Fail 'login itest1 returned no token' } else { Pass 'login itest1 returned token' }
} catch { Fail "login itest1 failed: $_" }

try {
    $l2 = PostJson "$base/api/auth/login" @{Username='itest2'; Password='password123'}
    $t2 = $l2.Token
    if (-not $t2) { Fail 'login itest2 returned no token' } else { Pass 'login itest2 returned token' }
} catch { Fail "login itest2 failed: $_" }

$hdr1 = @{ Authorization = "Bearer $t1" }
$hdr2 = @{ Authorization = "Bearer $t2" }

# Create table (itest1)
try {
    $create = PostJson "$base/api/table/create" @{TableName='ITestTable'; MinBuyIn=100; MaxBuyIn=10000} $hdr1
    if ($create.table -ne $null -and $create.table.Id) { $tableId = $create.table.Id; Pass "table created ($tableId)" } elseif ($create.tableId) { $tableId = $create.tableId; Pass "table created ($tableId)" } else { Fail 'table create response missing id' }
} catch { Fail "create table failed: $_" }

# Join as seat 1 and 2
try { $j1 = PostJson "$base/api/table/join" @{TableId = [int]$tableId; SeatNumber = 1; ChipDeposit = 500} $hdr1; Pass 'itest1 joined seat 1' } catch { Fail "itest1 join failed: $_" }
try { $j2 = PostJson "$base/api/table/join" @{TableId = [int]$tableId; SeatNumber = 2; ChipDeposit = 500} $hdr2; Pass 'itest2 joined seat 2' } catch { Fail "itest2 join failed: $_" }

# Start game as itest1
try { $s = PostJson "$base/api/game/start/$tableId" @{ } $hdr1; Pass 'game started' } catch { Fail "start game failed: $_" }

# itest1 attempts action (expected: rejected because not current player's turn)
try {
    Invoke-RestMethod -Method Post -Uri "$base/api/game/action/$tableId" -Headers $hdr1 -ContentType 'application/json' -Body (@{Action='check'} | ConvertTo-Json) -ErrorAction Stop
    Fail 'itest1 action unexpectedly succeeded (should be out-of-turn)'
} catch {
    # Expect BadRequest with 'Not your turn' or similar
    $err = $_.Exception.Message
    if ($err -match 'Not your turn' -or $err -match 'Not your turn') { Pass 'out-of-turn action correctly rejected' } else { Fail "unexpected error for out-of-turn action: $err" }
}

# itest2 (seat 2) performs a valid action (call or check)
try {
    $act2 = PostJson "$base/api/game/action/$tableId" @{Action='call'} $hdr2
    Pass 'itest2 action accepted'
} catch { Fail "itest2 action failed: $_" }

# Get game status and assert current player seat is an integer and players present
try {
    $state = Invoke-RestMethod -Method Get -Uri "$base/api/game/status/$tableId" -Headers $hdr1
    if ($state.Players -and $state.Players.Count -ge 2) { Pass 'game state shows players' } else { Fail 'game state players missing or insufficient' }
    if ($state.CurrentPlayerSeat -or $state.CurrentPlayerSeat -eq 0 -or $state.CurrentPlayerSeat -ne $null) { Pass "current player seat present: $($state.CurrentPlayerSeat)" } else { Fail 'current player seat missing' }
} catch { Fail "get status failed: $_" }

# Summary
if ($failures.Count -eq 0) { Write-Host "\nALL TESTS PASSED" -ForegroundColor Green; exit 0 } else { Write-Host "\nTESTS FAILED:`n" -ForegroundColor Red; $failures | ForEach-Object { Write-Host "- $_" }; exit 1 }
