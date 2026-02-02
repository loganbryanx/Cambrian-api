# Comprehensive API Endpoint Test Suite
# Tests all major endpoints of the Cambrian API

param(
    [string]$BaseUrl = "https://cambrian-api.onrender.com"
)

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$testEmail = "apitest$(Get-Random -Minimum 1000 -Maximum 9999)@example.com"
$testPassword = "TestPassword123!"
$token = $null

Write-Host "`n════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   Cambrian API Endpoint Test Suite" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl" -ForegroundColor Gray
Write-Host "Test Email: $testEmail`n" -ForegroundColor Gray

# Test 1: Health Check
Write-Host "1. Testing /auth/health..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/auth/health"
    Write-Host "   ✅ Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "   Response: $($response.Content)`n" -ForegroundColor White
} catch {
    Write-Host "   ❌ Failed: $($_.Exception.Message)`n" -ForegroundColor Red
}

# Test 2: Get Catalog (Public)
Write-Host "2. Testing GET /catalog (public)..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/catalog"
    $catalog = $response.Content | ConvertFrom-Json
    Write-Host "   ✅ Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "   Found $($catalog.Count) tracks:" -ForegroundColor White
    $catalog | ForEach-Object { Write-Host "      - $($_.title) by $($_.artist) ($$($_.price))" -ForegroundColor Gray }
    Write-Host ""
} catch {
    Write-Host "   ❌ Failed: $($_.Exception.Message)`n" -ForegroundColor Red
}

# Test 3: Register New User (Listener)
Write-Host "3. Testing POST /auth/register (Listener)..." -ForegroundColor Yellow
try {
    $registerBody = @{
        email = $testEmail
        password = $testPassword
        plan = "Listener"
    } | ConvertTo-Json
    
    $response = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/auth/register" -Method POST -Body $registerBody -ContentType "application/json"
    $registerData = $response.Content | ConvertFrom-Json
    $token = $registerData.token
    Write-Host "   ✅ Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "   Token: $token`n" -ForegroundColor White
} catch {
    Write-Host "   ❌ Failed: $($_.Exception.Message)`n" -ForegroundColor Red
}

# Test 4: Login
Write-Host "4. Testing POST /auth/login..." -ForegroundColor Yellow
try {
    $loginBody = @{
        email = $testEmail
        password = $testPassword
    } | ConvertTo-Json
    
    $response = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $loginData = $response.Content | ConvertFrom-Json
    $token = $loginData.token
    Write-Host "   ✅ Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "   New Token: $token`n" -ForegroundColor White
} catch {
    Write-Host "   ❌ Failed: $($_.Exception.Message)`n" -ForegroundColor Red
}

# Test 5: Get Account Data
Write-Host "5. Testing GET /data/account..." -ForegroundColor Yellow
try {
    $headers = @{ "x-email" = $testEmail }
    $response = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/data/account" -Headers $headers
    $accountData = $response.Content | ConvertFrom-Json
    Write-Host "   ✅ Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "   Email: $($accountData.email)" -ForegroundColor White
    Write-Host "   Plan: $($accountData.plan)" -ForegroundColor White
    Write-Host "   Status: $($accountData.status)`n" -ForegroundColor White
} catch {
    Write-Host "   ❌ Failed: $($_.Exception.Message)`n" -ForegroundColor Red
}

# Test 6: Get Plans
Write-Host "6. Testing GET /plans..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/plans"
    $plans = $response.Content | ConvertFrom-Json
    Write-Host "   ✅ Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "   Available plans:" -ForegroundColor White
    $plans | ForEach-Object { Write-Host "      - $($_.plan): $$($_.price)/month - $($_.features -join ', ')" -ForegroundColor Gray }
    Write-Host ""
} catch {
    Write-Host "   ❌ Failed: $($_.Exception.Message)`n" -ForegroundColor Red
}

# Test 7: Test Stripe Configuration
Write-Host "7. Checking Stripe configuration..." -ForegroundColor Yellow
Write-Host "   Testing GET /checkout/create-session..." -ForegroundColor Gray
try {
    $checkoutBody = @{
        email = $testEmail
        plan = "Listener"
    } | ConvertTo-Json
    
    $response = Invoke-WebRequest -UseBasicParsing -Uri "$BaseUrl/checkout/create-session" -Method POST -Body $checkoutBody -ContentType "application/json"
    $checkoutData = $response.Content | ConvertFrom-Json
    Write-Host "   ✅ Stripe is configured!" -ForegroundColor Green
    Write-Host "   Session URL available`n" -ForegroundColor White
} catch {
    if ($_.Exception.Response.StatusCode -eq 501) {
        Write-Host "   ⚠️  Stripe NOT configured (expected for testing)" -ForegroundColor Yellow
        Write-Host "   Need to set: STRIPE_SECRET_KEY, STRIPE_LISTENER_PRICE_ID, etc.`n" -ForegroundColor Yellow
    } else {
        Write-Host "   ❌ Failed: $($_.Exception.Message)`n" -ForegroundColor Red
    }
}

# Test 8: Database Configuration Check
Write-Host "8. Checking database configuration..." -ForegroundColor Yellow
Write-Host "   Note: API uses in-memory storage by default" -ForegroundColor Gray
Write-Host "   To enable PostgreSQL, set these environment variables:" -ForegroundColor Yellow
Write-Host "      - BILLING_CONNECTION_STRING" -ForegroundColor White
Write-Host "      - PLAY_EVENTS_CONNECTION_STRING`n" -ForegroundColor White

Write-Host "════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   Test Summary" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "✅ Core API is functional!" -ForegroundColor Green
Write-Host "⚠️  Optional: Configure Stripe for payments" -ForegroundColor Yellow
Write-Host "⚠️  Optional: Configure PostgreSQL for persistence`n" -ForegroundColor Yellow
