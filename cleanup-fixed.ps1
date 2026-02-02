# Repository Cleanup Script (PowerShell)
$ErrorActionPreference = "Stop"

Write-Host " Starting repository cleanup..." -ForegroundColor Cyan

if (-not (Test-Path ".git")) {
    Write-Host " Error: Not in a git repository root" -ForegroundColor Red
    exit 1
}

if (Test-Path "src/auth") {
    Write-Host " Removing duplicate src/auth folder..." -ForegroundColor Yellow
    git rm -r src/auth
} else {
    Write-Host " src/auth already removed" -ForegroundColor Green
}

if (Test-Path ".vs") {
    Write-Host "  Removing Visual Studio metadata..." -ForegroundColor Yellow
    git rm -r .vs
} else {
    Write-Host " .vs already removed" -ForegroundColor Green
}

if (-not (Test-Path "docs")) {
    Write-Host " Creating docs folder..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path "docs" | Out-Null
}

Write-Host ""
Write-Host " Git status:" -ForegroundColor Cyan
git status --short

Write-Host ""
Write-Host " Cleanup complete!" -ForegroundColor Green
