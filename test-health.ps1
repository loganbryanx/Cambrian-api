# Health Check Monitor for Render Deployment
# This script will continuously check the /auth/health endpoint until it responds successfully

param(
    [int]$MaxAttempts = 20,
    [int]$WaitSeconds = 15
)

$url = "https://cambrian-api.onrender.com/auth/health"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

Write-Host "Monitoring Render deployment health endpoint..." -ForegroundColor Cyan
Write-Host "URL: $url" -ForegroundColor Gray
Write-Host "Max attempts: $MaxAttempts | Wait between attempts: $WaitSeconds seconds`n" -ForegroundColor Gray

for ($i = 1; $i -le $MaxAttempts; $i++) {
    Write-Host "[$i/$MaxAttempts] Checking health endpoint..." -ForegroundColor Yellow
    
    try {
        $response = Invoke-WebRequest -UseBasicParsing -Uri $url -TimeoutSec 10
        
        Write-Host "`n✅ SUCCESS!" -ForegroundColor Green
        Write-Host "Status Code: $($response.StatusCode)" -ForegroundColor Green
        Write-Host "Response: $($response.Content)" -ForegroundColor Green
        Write-Host "`nDeployment is live and healthy!" -ForegroundColor Green
        exit 0
        
    } catch {
        $errorMessage = $_.Exception.Message
        
        if ($errorMessage -like "*connection was closed*") {
            Write-Host "  ⏳ Deployment still in progress (connection closed)" -ForegroundColor DarkYellow
        } elseif ($errorMessage -like "*timed out*") {
            Write-Host "  ⏳ Deployment still in progress (timeout)" -ForegroundColor DarkYellow
        } else {
            Write-Host "  ❌ Error: $errorMessage" -ForegroundColor Red
        }
        
        if ($i -lt $MaxAttempts) {
            Write-Host "  Waiting $WaitSeconds seconds before next attempt...`n" -ForegroundColor Gray
            Start-Sleep -Seconds $WaitSeconds
        }
    }
}

Write-Host "`n⚠️  Deployment did not complete within the expected time." -ForegroundColor Red
Write-Host "Please check the Render dashboard Events tab for deployment status." -ForegroundColor Yellow
Write-Host "The deployment may still be building Docker images or starting up." -ForegroundColor Yellow
exit 1
