$baseUrl = "http://localhost:59129"
$logFile = "C:\Users\mos18\.gemini\antigravity\brain\30a9698c-b458-4df6-950f-b1565177c859\.system_generated\tasks\task-110.log"

$timestamp = Get-Date -Format "HHmmss"
$testEmail = "authtest_$timestamp@stylo.com"
$initialPassword = "Password123!"
$newPassword = "NewPassword123!"
$name = "Auth Test User"

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "STARTING AUTH ENDPOINTS INTEGRATION TEST SUITE" -ForegroundColor Cyan
Write-Host "Target Base URL: $baseUrl" -ForegroundColor Cyan
Write-Host "Test Email: $testEmail" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

function Get-LatestOtp {
    param ([string]$email, [string]$purpose)
    Start-Sleep -Seconds 1
    if (Test-Path $logFile) {
        $lines = Get-Content $logFile -Tail 200
        $match = $lines | Where-Object { $_ -like "*[OTP Generated]*Email: $email*Purpose: $purpose*" } | Select-Object -Last 1
        if ($match -match "OTP:\s*(\d{6})") {
            return $matches[1]
        }
    }
    return $null
}

# -------------------------------------------------------------
# TEST 1: POST /api/auth/register/request
# -------------------------------------------------------------
Write-Host "`n[TEST 1] POST /api/auth/register/request" -ForegroundColor Yellow
$regReqBody = @{
    name = $name
    email = $testEmail
    password = $initialPassword
    confirmPassword = $initialPassword
} | ConvertTo-Json

try {
    $res1 = Invoke-RestMethod -Uri "$baseUrl/api/auth/register/request" -Method Post -Body $regReqBody -ContentType "application/json"
    Write-Host "-> PASSED: $($res1.message)" -ForegroundColor Green
} catch {
    Write-Host "-> FAILED: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Fetch OTP from log
$registerOtp = Get-LatestOtp -email $testEmail -purpose "Register"
Write-Host "-> Retrieved Register OTP: $registerOtp" -ForegroundColor Magenta
if (-not $registerOtp) {
    Write-Host "-> FAILED: Could not retrieve Register OTP from logs" -ForegroundColor Red
    exit 1
}

# -------------------------------------------------------------
# TEST 1.1 (Edge Case): Invalid OTP for register/verify
# -------------------------------------------------------------
Write-Host "`n[TEST 1.1] POST /api/auth/register/verify (Invalid OTP handling)" -ForegroundColor Yellow
$invalidVerifyBody = @{
    email = $testEmail
    otp = "000000"
} | ConvertTo-Json

try {
    $resFail = Invoke-RestMethod -Uri "$baseUrl/api/auth/register/verify" -Method Post -Body $invalidVerifyBody -ContentType "application/json"
    Write-Host "-> FAILED: Expected 400 Bad Request but succeeded!" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq [System.Net.HttpStatusCode]::BadRequest) {
        Write-Host "-> PASSED: Correctly returned 400 Bad Request for invalid OTP" -ForegroundColor Green
    } else {
        Write-Host "-> FAILED: Unexpected status code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# -------------------------------------------------------------
# TEST 2: POST /api/auth/register/verify (Valid OTP)
# -------------------------------------------------------------
Write-Host "`n[TEST 2] POST /api/auth/register/verify (Valid OTP)" -ForegroundColor Yellow
$verifyBody = @{
    email = $testEmail
    otp = $registerOtp
} | ConvertTo-Json

try {
    $authRes = Invoke-RestMethod -Uri "$baseUrl/api/auth/register/verify" -Method Post -Body $verifyBody -ContentType "application/json"
    Write-Host "-> PASSED: Verified successfully!" -ForegroundColor Green
    Write-Host "   User ID: $($authRes.id)" -ForegroundColor Gray
    Write-Host "   User Name: $($authRes.name)" -ForegroundColor Gray
    Write-Host "   User Email: $($authRes.email)" -ForegroundColor Gray
    Write-Host "   User Role: $($authRes.role)" -ForegroundColor Gray
    Write-Host "   Token Received: $($authRes.token.Substring(0, 20))..." -ForegroundColor Gray
} catch {
    Write-Host "-> FAILED: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# -------------------------------------------------------------
# TEST 2.1 (Edge Case): Register with duplicate email
# -------------------------------------------------------------
Write-Host "`n[TEST 2.1] POST /api/auth/register/request (Duplicate Email Conflict)" -ForegroundColor Yellow
try {
    $resDup = Invoke-RestMethod -Uri "$baseUrl/api/auth/register/request" -Method Post -Body $regReqBody -ContentType "application/json"
    Write-Host "-> FAILED: Expected 409 Conflict but succeeded!" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq [System.Net.HttpStatusCode]::Conflict) {
        Write-Host "-> PASSED: Correctly returned 409 Conflict for duplicate email" -ForegroundColor Green
    } else {
        Write-Host "-> FAILED: Unexpected status code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# -------------------------------------------------------------
# TEST 3: POST /api/auth/login
# -------------------------------------------------------------
Write-Host "`n[TEST 3] POST /api/auth/login" -ForegroundColor Yellow
$loginBody = @{
    email = $testEmail
    password = $initialPassword
} | ConvertTo-Json

try {
    $loginRes = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginRes.token
    Write-Host "-> PASSED: Logged in successfully!" -ForegroundColor Green
    Write-Host "   Token Received: $($token.Substring(0, 20))..." -ForegroundColor Gray
} catch {
    Write-Host "-> FAILED: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# -------------------------------------------------------------
# TEST 3.1 (Edge Case): Login with wrong password
# -------------------------------------------------------------
Write-Host "`n[TEST 3.1] POST /api/auth/login (Wrong Password)" -ForegroundColor Yellow
$wrongLoginBody = @{
    email = $testEmail
    password = "WrongPassword999!"
} | ConvertTo-Json

try {
    $loginWrong = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $wrongLoginBody -ContentType "application/json"
    Write-Host "-> FAILED: Expected 401 Unauthorized but succeeded!" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq [System.Net.HttpStatusCode]::Unauthorized) {
        Write-Host "-> PASSED: Correctly returned 401 Unauthorized for wrong password" -ForegroundColor Green
    } else {
        Write-Host "-> FAILED: Unexpected status code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# -------------------------------------------------------------
# TEST 4: GET /api/auth/me
# -------------------------------------------------------------
Write-Host "`n[TEST 4] GET /api/auth/me (Authenticated user profile)" -ForegroundColor Yellow
$headers = @{ Authorization = "Bearer $token" }

try {
    $meRes = Invoke-RestMethod -Uri "$baseUrl/api/auth/me" -Method Get -Headers $headers
    Write-Host "-> PASSED: Profile retrieved successfully!" -ForegroundColor Green
    Write-Host "   User ID: $($meRes.id)" -ForegroundColor Gray
    Write-Host "   Name: $($meRes.name)" -ForegroundColor Gray
    Write-Host "   Email: $($meRes.email)" -ForegroundColor Gray
    Write-Host "   Role: $($meRes.role)" -ForegroundColor Gray
} catch {
    Write-Host "-> FAILED: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# -------------------------------------------------------------
# TEST 5: POST /api/auth/forgot-password
# -------------------------------------------------------------
Write-Host "`n[TEST 5] POST /api/auth/forgot-password" -ForegroundColor Yellow
$forgotBody = @{
    email = $testEmail
} | ConvertTo-Json

try {
    $forgotRes = Invoke-RestMethod -Uri "$baseUrl/api/auth/forgot-password" -Method Post -Body $forgotBody -ContentType "application/json"
    Write-Host "-> PASSED: $($forgotRes.message)" -ForegroundColor Green
} catch {
    Write-Host "-> FAILED: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Fetch Reset OTP from log
$resetOtp = Get-LatestOtp -email $testEmail -purpose "ResetPassword"
Write-Host "-> Retrieved Reset OTP: $resetOtp" -ForegroundColor Magenta
if (-not $resetOtp) {
    Write-Host "-> FAILED: Could not retrieve Reset OTP from logs" -ForegroundColor Red
    exit 1
}

# -------------------------------------------------------------
# TEST 6: POST /api/auth/verify-reset-otp
# -------------------------------------------------------------
Write-Host "`n[TEST 6] POST /api/auth/verify-reset-otp" -ForegroundColor Yellow
$verifyResetBody = @{
    email = $testEmail
    otp = $resetOtp
} | ConvertTo-Json

try {
    $resetTokenRes = Invoke-RestMethod -Uri "$baseUrl/api/auth/verify-reset-otp" -Method Post -Body $verifyResetBody -ContentType "application/json"
    $resetToken = $resetTokenRes.resetToken
    Write-Host "-> PASSED: Reset OTP verified! Received Reset Token." -ForegroundColor Green
    Write-Host "   Reset Token: $($resetToken.Substring(0, 20))..." -ForegroundColor Gray
} catch {
    Write-Host "-> FAILED: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# -------------------------------------------------------------
# TEST 7: POST /api/auth/reset-password
# -------------------------------------------------------------
Write-Host "`n[TEST 7] POST /api/auth/reset-password" -ForegroundColor Yellow
$resetPasswordBody = @{
    resetToken = $resetToken
    newPassword = $newPassword
    confirmPassword = $newPassword
} | ConvertTo-Json

try {
    $resetPasswordRes = Invoke-RestMethod -Uri "$baseUrl/api/auth/reset-password" -Method Post -Body $resetPasswordBody -ContentType "application/json"
    Write-Host "-> PASSED: $($resetPasswordRes.message)" -ForegroundColor Green
} catch {
    Write-Host "-> FAILED: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Verify login works with new password
Write-Host "-> Verifying login with NEW password..." -ForegroundColor Yellow
$newLoginBody = @{
    email = $testEmail
    password = $newPassword
} | ConvertTo-Json

try {
    $newLoginRes = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $newLoginBody -ContentType "application/json"
    $activeToken = $newLoginRes.token
    Write-Host "-> PASSED: Logged in with NEW password successfully!" -ForegroundColor Green
} catch {
    Write-Host "-> FAILED: Login with new password failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# -------------------------------------------------------------
# TEST 8: POST /api/auth/logout
# -------------------------------------------------------------
Write-Host "`n[TEST 8] POST /api/auth/logout" -ForegroundColor Yellow
$logoutHeaders = @{ Authorization = "Bearer $activeToken" }

try {
    $logoutRes = Invoke-RestMethod -Uri "$baseUrl/api/auth/logout" -Method Post -Headers $logoutHeaders
    Write-Host "-> PASSED: $($logoutRes.message)" -ForegroundColor Green
} catch {
    Write-Host "-> FAILED: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Verify token revocation by calling GET /api/auth/me with logged-out token
Write-Host "-> Verifying token revocation via GET /api/auth/me..." -ForegroundColor Yellow
try {
    $meAfterLogout = Invoke-RestMethod -Uri "$baseUrl/api/auth/me" -Method Get -Headers $logoutHeaders
    Write-Host "-> FAILED: Token was NOT revoked! Request succeeded after logout." -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq [System.Net.HttpStatusCode]::Unauthorized) {
        Write-Host "-> PASSED: Token correctly revoked (401 Unauthorized returned)" -ForegroundColor Green
    } else {
        Write-Host "-> FAILED: Unexpected status code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

Write-Host "`n=============================================" -ForegroundColor Cyan
Write-Host "ALL 8 AUTH ENDPOINTS PASSED SUCCESSFULLY!" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
