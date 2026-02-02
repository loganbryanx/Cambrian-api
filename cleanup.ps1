# Repository Cleanup Script (PowerShell)
# This script organizes the repository structure

$ErrorActionPreference = "Stop"

Write-Host "🧹 Starting repository cleanup..." -ForegroundColor Cyan

# Check if we're in a git repository
if (-not (Test-Path ".git")) {
    Write-Host "❌ Error: Not in a git repository root" -ForegroundColor Red
    exit 1
}

# 1. Remove duplicate auth API folder
if (Test-Path "src/auth") {
    Write-Host "📁 Removing duplicate src/auth folder..." -ForegroundColor Yellow
    git rm -r src/auth
} else {
    Write-Host "✓ src/auth already removed" -ForegroundColor Green
}

# 2. Remove Visual Studio metadata
if (Test-Path ".vs") {
    Write-Host "🗑️  Removing Visual Studio metadata..." -ForegroundColor Yellow
    git rm -r .vs
} else {
    Write-Host "✓ .vs already removed" -ForegroundColor Green
}

# 3. Ensure docs folder exists
if (-not (Test-Path "docs")) {
    Write-Host "📂 Creating docs folder..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path "docs" | Out-Null
}

# 4. Move documentation files
Write-Host "📚 Moving documentation files to docs/..." -ForegroundColor Yellow
$docFiles = @(
    "API_REFERENCE.md",
    "AUDIO_PLAYER_BACKEND_SUPPORT.md",
    "BACKEND_UPDATES.md",
    "CONFIGURATION_GUIDE.md",
    "DEPLOYMENT.md",
    "RENDER_SETUP.md",
    "SETUP_COMPLETE.md",
    "TROUBLESHOOTING.md"
)

foreach ($file in $docFiles) {
    if (Test-Path $file) {
        try {
            git mv $file docs/ 2>$null
            Write-Host "  ✓ Moved $file" -ForegroundColor Green
        } catch {
            Move-Item $file docs/
            Write-Host "  ✓ Moved $file" -ForegroundColor Green
        }
    }
}

# 5. Move PowerShell scripts
Write-Host "🔧 Moving PowerShell scripts to scripts/..." -ForegroundColor Yellow
$scriptFiles = @(
    "setup-configuration.ps1",
    "test-api-endpoints.ps1",
    "test-health.ps1"
)

foreach ($script in $scriptFiles) {
    if (Test-Path $script) {
        try {
            git mv $script scripts/ 2>$null
            Write-Host "  ✓ Moved $script" -ForegroundColor Green
        } catch {
            Move-Item $script scripts/
            Write-Host "  ✓ Moved $script" -ForegroundColor Green
        }
    }
}

# 6. Check migrations folder
if (Test-Path "migrations") {
    $items = Get-ChildItem "migrations" -Force
    if ($items.Count -eq 0) {
        Write-Host "🗂️  Removing empty migrations folder..." -ForegroundColor Yellow
        Remove-Item "migrations"
    } else {
        Write-Host "ℹ️  migrations folder contains files, keeping it" -ForegroundColor Blue
    }
}

# 7. Update .gitignore
Write-Host "📝 Updating .gitignore..." -ForegroundColor Yellow
$gitignoreContent = Get-Content .gitignore -ErrorAction SilentlyContinue
if (-not ($gitignoreContent -contains ".vs/")) {
    Add-Content .gitignore ".vs/"
    Write-Host "  ✓ Added .vs/ to .gitignore" -ForegroundColor Green
} else {
    Write-Host "  ✓ .vs/ already in .gitignore" -ForegroundColor Green
}

# 8. Show status
Write-Host ""
Write-Host "📊 Git status:" -ForegroundColor Cyan
git status --short

Write-Host ""
Write-Host "✅ Cleanup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review changes: git status" -ForegroundColor White
Write-Host "  2. Fix merge conflicts in src/Cambrian.Api/Program.cs" -ForegroundColor White
Write-Host "  3. Update references in README.md" -ForegroundColor White
Write-Host "  4. Commit: git commit -m 'chore: clean up repository structure'" -ForegroundColor White
Write-Host "  5. Test build: dotnet build Cambrian.sln" -ForegroundColor White
