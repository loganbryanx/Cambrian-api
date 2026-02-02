# PostgreSQL + Stripe Setup Script
# This script helps you configure PostgreSQL and Stripe for your Cambrian API

Write-Host "`n════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   Cambrian API Configuration Wizard" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════`n" -ForegroundColor Cyan

# Step 1: PostgreSQL Setup
Write-Host "📦 STEP 1: PostgreSQL Database Setup`n" -ForegroundColor Yellow
Write-Host "Choose your PostgreSQL provider:" -ForegroundColor White
Write-Host "  1. Render PostgreSQL (Recommended - same platform)" -ForegroundColor Gray
Write-Host "  2. Neon (Serverless PostgreSQL)" -ForegroundColor Gray
Write-Host "  3. Supabase (PostgreSQL + extras)" -ForegroundColor Gray
Write-Host "  4. I already have a database" -ForegroundColor Gray
Write-Host "  5. Skip for now (use in-memory storage)`n" -ForegroundColor Gray

$dbChoice = Read-Host "Enter your choice (1-5)"

$billingConnStr = ""
$playEventsConnStr = ""

switch ($dbChoice) {
    "1" {
        Write-Host "`n✨ Setting up Render PostgreSQL...`n" -ForegroundColor Cyan
        Write-Host "Instructions:" -ForegroundColor Yellow
        Write-Host "1. Go to https://dashboard.render.com" -ForegroundColor White
        Write-Host "2. Click 'New +' → 'PostgreSQL'" -ForegroundColor White
        Write-Host "3. Name: cambrian-db" -ForegroundColor White
        Write-Host "4. Database: cambrian" -ForegroundColor White
        Write-Host "5. Click 'Create Database'" -ForegroundColor White
        Write-Host "6. Once created, copy the 'Internal Database URL'`n" -ForegroundColor White
        
        Write-Host "Paste the Internal Database URL (or press Enter to skip):" -ForegroundColor Yellow
        $dbUrl = Read-Host
        
        if ($dbUrl) {
            # Convert PostgreSQL URL to ADO.NET format
            if ($dbUrl -match "postgres://([^:]+):([^@]+)@([^:]+):(\d+)/(.+)") {
                $user = $matches[1]
                $pass = $matches[2]
                $host = $matches[3]
                $port = $matches[4]
                $db = $matches[5]
                
                $billingConnStr = "Host=$host;Port=$port;Database=$db;Username=$user;Password=$pass;SslMode=Require"
                $playEventsConnStr = $billingConnStr
                
                Write-Host "✅ Connection strings generated!`n" -ForegroundColor Green
            }
        }
    }
    "2" {
        Write-Host "`n✨ Setting up Neon PostgreSQL...`n" -ForegroundColor Cyan
        Write-Host "Instructions:" -ForegroundColor Yellow
        Write-Host "1. Go to https://neon.tech" -ForegroundColor White
        Write-Host "2. Sign up and create a new project" -ForegroundColor White
        Write-Host "3. Copy the connection string`n" -ForegroundColor White
        
        Write-Host "Paste the connection string (or press Enter to skip):" -ForegroundColor Yellow
        $dbUrl = Read-Host
        
        if ($dbUrl) {
            $billingConnStr = $dbUrl
            $playEventsConnStr = $dbUrl
            Write-Host "✅ Connection strings set!`n" -ForegroundColor Green
        }
    }
    "3" {
        Write-Host "`n✨ Setting up Supabase PostgreSQL...`n" -ForegroundColor Cyan
        Write-Host "Instructions:" -ForegroundColor Yellow
        Write-Host "1. Go to https://supabase.com" -ForegroundColor White
        Write-Host "2. Create a new project" -ForegroundColor White
        Write-Host "3. Go to Settings → Database" -ForegroundColor White
        Write-Host "4. Copy the connection string (mode: Session)`n" -ForegroundColor White
        
        Write-Host "Paste the connection string (or press Enter to skip):" -ForegroundColor Yellow
        $dbUrl = Read-Host
        
        if ($dbUrl) {
            $billingConnStr = $dbUrl
            $playEventsConnStr = $dbUrl
            Write-Host "✅ Connection strings set!`n" -ForegroundColor Green
        }
    }
    "4" {
        Write-Host "`nEnter your BILLING_CONNECTION_STRING:" -ForegroundColor Yellow
        $billingConnStr = Read-Host
        Write-Host "Enter your PLAY_EVENTS_CONNECTION_STRING:" -ForegroundColor Yellow
        $playEventsConnStr = Read-Host
        Write-Host "✅ Connection strings set!`n" -ForegroundColor Green
    }
    "5" {
        Write-Host "⏭️  Skipping database configuration`n" -ForegroundColor Yellow
    }
}

# Step 2: Stripe Setup
Write-Host "`n💳 STEP 2: Stripe Payment Setup`n" -ForegroundColor Yellow
Write-Host "Do you want to configure Stripe? (y/n)" -ForegroundColor White
$configureStripe = Read-Host

$stripeSecretKey = ""
$stripeListenerPriceId = ""
$stripeCreatorPriceId = ""
$stripeSuccessUrl = ""
$stripeCancelUrl = ""
$stripeWebhookSecret = ""

if ($configureStripe -eq "y") {
    Write-Host "`n✨ Setting up Stripe...`n" -ForegroundColor Cyan
    
    Write-Host "Instructions:" -ForegroundColor Yellow
    Write-Host "1. Go to https://dashboard.stripe.com/apikeys" -ForegroundColor White
    Write-Host "2. Copy your 'Secret key' (sk_test_... or sk_live_...)`n" -ForegroundColor White
    
    Write-Host "Paste your Stripe Secret Key:" -ForegroundColor Yellow
    $stripeSecretKey = Read-Host
    
    if ($stripeSecretKey) {
        Write-Host "`n3. Go to https://dashboard.stripe.com/products" -ForegroundColor White
        Write-Host "4. Create a product 'Listener Plan' with price $9/month" -ForegroundColor White
        Write-Host "5. Copy the Price ID (price_...)`n" -ForegroundColor White
        
        Write-Host "Paste Listener Price ID:" -ForegroundColor Yellow
        $stripeListenerPriceId = Read-Host
        
        Write-Host "`n6. Create a product 'Creator Plan' with price $19/month" -ForegroundColor White
        Write-Host "7. Copy the Price ID (price_...)`n" -ForegroundColor White
        
        Write-Host "Paste Creator Price ID:" -ForegroundColor Yellow
        $stripeCreatorPriceId = Read-Host
        
        Write-Host "`nEnter your frontend URL (e.g., https://your-app.vercel.app):" -ForegroundColor Yellow
        $frontendUrl = Read-Host
        
        if ($frontendUrl) {
            $stripeSuccessUrl = "$frontendUrl/account?status=success"
            $stripeCancelUrl = "$frontendUrl/account?status=cancel"
        }
        
        Write-Host "`n8. Go to https://dashboard.stripe.com/webhooks" -ForegroundColor White
        Write-Host "9. Add endpoint: https://cambrian-api.onrender.com/webhook/stripe" -ForegroundColor White
        Write-Host "10. Select event: checkout.session.completed" -ForegroundColor White
        Write-Host "11. Copy the Signing secret (whsec_...)`n" -ForegroundColor White
        
        Write-Host "Paste Webhook Secret (or press Enter to skip):" -ForegroundColor Yellow
        $stripeWebhookSecret = Read-Host
        
        Write-Host "✅ Stripe configuration complete!`n" -ForegroundColor Green
    }
} else {
    Write-Host "⏭️  Skipping Stripe configuration`n" -ForegroundColor Yellow
}

# Step 3: CORS Setup
Write-Host "`n🌐 STEP 3: Frontend CORS Configuration`n" -ForegroundColor Yellow
Write-Host "Enter your frontend URL(s) (comma-separated if multiple):" -ForegroundColor White
Write-Host "Example: https://cambrian-app.vercel.app,https://staging.cambrian-app.vercel.app" -ForegroundColor Gray
$corsOrigins = Read-Host

# Generate Summary
Write-Host "`n════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   Configuration Summary" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════`n" -ForegroundColor Cyan

$envVars = @()

if ($corsOrigins) {
    $envVars += "CORS_ORIGINS=$corsOrigins"
}

if ($billingConnStr) {
    $envVars += "BILLING_CONNECTION_STRING=$billingConnStr"
}

if ($playEventsConnStr) {
    $envVars += "PLAY_EVENTS_CONNECTION_STRING=$playEventsConnStr"
}

if ($stripeSecretKey) {
    $envVars += "STRIPE_SECRET_KEY=$stripeSecretKey"
}

if ($stripeListenerPriceId) {
    $envVars += "STRIPE_LISTENER_PRICE_ID=$stripeListenerPriceId"
}

if ($stripeCreatorPriceId) {
    $envVars += "STRIPE_CREATOR_PRICE_ID=$stripeCreatorPriceId"
}

if ($stripeSuccessUrl) {
    $envVars += "STRIPE_SUCCESS_URL=$stripeSuccessUrl"
}

if ($stripeCancelUrl) {
    $envVars += "STRIPE_CANCEL_URL=$stripeCancelUrl"
}

if ($stripeWebhookSecret) {
    $envVars += "STRIPE_WEBHOOK_SECRET=$stripeWebhookSecret"
}

if ($envVars.Count -gt 0) {
    Write-Host "Add these environment variables to Render:`n" -ForegroundColor Yellow
    
    foreach ($var in $envVars) {
        Write-Host "  $var" -ForegroundColor White
    }
    
    # Save to file
    $envVars | Out-File -FilePath "render-env-vars.txt" -Encoding UTF8
    
    Write-Host "`n✅ Saved to: render-env-vars.txt`n" -ForegroundColor Green
    
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Go to https://dashboard.render.com" -ForegroundColor White
    Write-Host "2. Select your cambrian-api service" -ForegroundColor White
    Write-Host "3. Go to Environment tab" -ForegroundColor White
    Write-Host "4. Add each variable listed above" -ForegroundColor White
    Write-Host "5. Click 'Save Changes' (will auto-redeploy)`n" -ForegroundColor White
} else {
    Write-Host "No configuration changes made.`n" -ForegroundColor Yellow
}

Write-Host "════════════════════════════════════════`n" -ForegroundColor Cyan
