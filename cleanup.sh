#!/usr/bin/env bash
set -euo pipefail

# Repository Cleanup Script
# This script organizes the repository structure

echo "🧹 Starting repository cleanup..."

# Check if we're in a git repository
if [ ! -d ".git" ]; then
  echo "❌ Error: Not in a git repository root"
  exit 1
fi

# 1. Remove duplicate auth API folder
if [ -d "src/auth" ]; then
  echo "📁 Removing duplicate src/auth folder..."
  git rm -r src/auth
else
  echo "✓ src/auth already removed"
fi

# 2. Remove Visual Studio metadata
if [ -d ".vs" ]; then
  echo "🗑️  Removing Visual Studio metadata..."
  git rm -r .vs
else
  echo "✓ .vs already removed"
fi

# 3. Ensure docs folder exists
if [ ! -d "docs" ]; then
  echo "📂 Creating docs folder..."
  mkdir -p docs
fi

# 4. Move documentation files
echo "📚 Moving documentation files to docs/..."
for file in API_REFERENCE.md AUDIO_PLAYER_BACKEND_SUPPORT.md BACKEND_UPDATES.md \
            CONFIGURATION_GUIDE.md DEPLOYMENT.md RENDER_SETUP.md \
            SETUP_COMPLETE.md TROUBLESHOOTING.md; do
  if [ -f "$file" ]; then
    git mv "$file" docs/ 2>/dev/null || mv "$file" docs/
    echo "  ✓ Moved $file"
  fi
done

# 5. Move PowerShell scripts
echo "🔧 Moving PowerShell scripts to scripts/..."
for script in setup-configuration.ps1 test-api-endpoints.ps1 test-health.ps1; do
  if [ -f "$script" ]; then
    git mv "$script" scripts/ 2>/dev/null || mv "$script" scripts/
    echo "  ✓ Moved $script"
  fi
done

# 6. Check migrations folder
if [ -d "migrations" ]; then
  if [ -z "$(ls -A migrations)" ]; then
    echo "🗂️  Removing empty migrations folder..."
    rmdir migrations
  else
    echo "ℹ️  migrations folder contains files, keeping it"
  fi
fi

# 7. Update .gitignore
echo "📝 Updating .gitignore..."
if ! grep -q "^\.vs/$" .gitignore 2>/dev/null; then
  echo ".vs/" >> .gitignore
  echo "  ✓ Added .vs/ to .gitignore"
else
  echo "  ✓ .vs/ already in .gitignore"
fi

# 8. Show status
echo ""
echo "📊 Git status:"
git status --short

echo ""
echo "✅ Cleanup complete!"
echo ""
echo "Next steps:"
echo "  1. Review changes: git status"
echo "  2. Fix merge conflicts in src/Cambrian.Api/Program.cs"
echo "  3. Update references in README.md"
echo "  4. Commit: git commit -m 'chore: clean up repository structure'"
echo "  5. Test build: dotnet build Cambrian.sln"
