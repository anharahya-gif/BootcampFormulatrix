$baseUrl = "http://localhost:5076"

function Invoke-Api {
    param($Uri, $Method, $Body, $Headers, $ContentType = "application/json")
    try {
        $response = Invoke-RestMethod -Uri $Uri -Method $Method -Body $Body -Headers $Headers -ContentType $ContentType
        return $response
    }
    catch {
        Write-Host "API Error ($Method $Uri): $($_.Exception.Message)"
        if ($_.Exception.Response) {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            Write-Host "Response Body: $($reader.ReadToEnd())"
        }
        return $null
    }
}

# 1. Login
$adminBody = @{ email = "admin@local"; password = "Admin123!" } | ConvertTo-Json
$auth = Invoke-Api -Uri "$baseUrl/api/Auth/login" -Method Post -Body $adminBody
if (!$auth -or !$auth.data.token) { Write-Host "Login Failed"; exit }
$token = $auth.data.token
Write-Host "Admin Token Acquired"

# 2. Get Rooms
$headers = @{ Authorization = "Bearer $token" }
$roomsRes = Invoke-Api -Uri "$baseUrl/api/Rooms" -Method Get -Headers $headers
$roomId = $null

if ($roomsRes -and $roomsRes.data -and $roomsRes.data.Count -gt 0) {
    Write-Host "Rooms Found: $($roomsRes.data.Count)"
    $firstRoom = $roomsRes.data[0]
    $roomId = $firstRoom.id
    if (!$roomId) { $roomId = $firstRoom.Id }
    Write-Host "Using Existing Room ID: $roomId"
}
else {
    Write-Host "No Rooms Found. Attempting to create one..."
    $newRoom = @{
        name         = "Test Room Created via Verification"
        capacity     = 10
        location     = "Virtual"
        hasProjector = $true
    } | ConvertTo-Json
    
    $createRes = Invoke-Api -Uri "$baseUrl/api/Rooms" -Method Post -Body $newRoom -Headers $headers
    if ($createRes -and $createRes.success) {
        $roomId = $createRes.data.id
        if (!$roomId) { $roomId = $createRes.data.Id }
        Write-Host "Created Room ID: $roomId"
    }
    else {
        Write-Host "Failed to create room."
        exit
    }
}

if (!$roomId) { Write-Host "No Room ID available."; exit }

# 3. Create Booking
$startTime = (Get-Date).AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss")
$endTime = (Get-Date).AddDays(1).AddHours(1).ToString("yyyy-MM-ddTHH:mm:ss")
$bookingBody = @{
    title     = "Test Meeting"
    roomId    = $roomId
    startTime = $startTime
    endTime   = $endTime
}
$jsonBody = $bookingBody | ConvertTo-Json -Depth 5
Write-Host "Sending Booking JSON: $jsonBody"

$bookingRes = Invoke-Api -Uri "$baseUrl/api/Bookings" -Method Post -Body $jsonBody -Headers $headers
if ($bookingRes) {
    Write-Host "Booking Created: $($bookingRes.data.id)"
}

# 4. My Bookings
$myBookings = Invoke-Api -Uri "$baseUrl/api/Bookings/my" -Method Get -Headers $headers
if ($myBookings) {
    Write-Host "My Bookings: $($myBookings.data.Count)"
}
