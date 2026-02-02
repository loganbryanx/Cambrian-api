# Repository Cleanup Plan

## Issues Identified

### 1. ✅ Duplicate API Folders
- **Problem**: Two Cambrian.Api projects exist
  - `src/auth/Cambrian.Api/` (duplicate/outdated with merge conflicts)
  - `src/Cambrian.Api/` (main, production version)
- **Action**: Delete `src/auth/` folder entirely
- **Command**: `git rm -r src/auth`

### 2. ⚠️ Git Merge Conflict Markers
- **Problem**: Unresolved merge conflicts in `src/Cambrian.Api/Program.cs`
- **Lines**: 9-12, 33-38 contain `<<<<<<< HEAD`, `=======`, `>>>>>>>` markers
- **Action**: Resolve conflicts in Program.cs

### 3. ⚠️ Visual Studio Metadata
- **Problem**: `.vs/` folder committed to repository
- **Files**: 
  - `.vs/Cambrian-api.slnx/v18/DocumentLayout.json`
  - `.vs/VSWorkspaceState.json`
  - `.vs/Cambrian-api.slnx/v18/.wsuo`
- **Action**: Remove `.vs/` folder and ensure it's in `.gitignore`
- **Command**: `git rm -r .vs`

### 4. ⚠️ Duplicate Docker Configuration
- **Problem**: Dockerfile exists at root (not shown in structure but may exist)
- **Action**: Keep only `docker/Dockerfile`, remove root-level if exists
- **Command**: `git rm Dockerfile` (if exists at root)

### 5. ⚠️ Disorganized Documentation
- **Problem**: 8+ markdown files at repository root
- **Files to move to `docs/`**:
  - `API_REFERENCE.md`
  - `AUDIO_PLAYER_BACKEND_SUPPORT.md`
  - `BACKEND_UPDATES.md`
  - `CONFIGURATION_GUIDE.md`
  - `DEPLOYMENT.md`
  - `RENDER_SETUP.md`
  - `SETUP_COMPLETE.md`
  - `TROUBLESHOOTING.md`
- **Keep at root**: `README.md`, `LICENSE`

### 6. ⚠️ Scattered Scripts
- **Problem**: PowerShell scripts at root level
- **Files to move to `scripts/`**:
  - `setup-configuration.ps1`
  - `test-api-endpoints.ps1`
  - `test-health.ps1`

### 7. ⚠️ Empty Migrations Folder
- **Problem**: `migrations/` folder exists but appears empty
- **Action**: Remove if truly empty, or add README explaining purpose

## Cleanup Commands

Run these commands from the repository root:

```bash
# 1. Remove duplicate auth API folder
git rm -r src/auth

# 2. Remove Visual Studio metadata
git rm -r .vs

# 3. Create docs folder if it doesn't exist
mkdir -p docs

# 4. Move documentation files
git mv API_REFERENCE.md docs/
git mv AUDIO_PLAYER_BACKEND_SUPPORT.md docs/
git mv BACKEND_UPDATES.md docs/
git mv CONFIGURATION_GUIDE.md docs/
git mv CONFIGURATION_GUIDE.md docs/
git mv DEPLOYMENT.md docs/
git mv RENDER_SETUP.md docs/
git mv SETUP_COMPLETE.md docs/
git mv TROUBLESHOOTING.md docs/

# 5. Move PowerShell scripts
git mv setup-configuration.ps1 scripts/
git mv test-api-endpoints.ps1 scripts/
git mv test-health.ps1 scripts/

# 6. Check if migrations is empty
rmdir migrations 2>/dev/null || echo "migrations not empty"

# 7. Update .gitignore to include .vs/
echo ".vs/" >> .gitignore

# 8. Commit cleanup
git add -A
git commit -m "chore: clean up repository structure

- Remove duplicate src/auth API folder
- Remove .vs Visual Studio metadata
- Organize documentation in docs/ folder
- Move PowerShell scripts to scripts/ folder
- Add .vs/ to .gitignore"
```

## Priority Actions (Do First)

1. **Fix merge conflicts** in `src/Cambrian.Api/Program.cs`
2. **Remove duplicate** `src/auth/` folder
3. **Update documentation** references to new paths

## After Cleanup

### Update README.md
Update any references to moved files:
- Documentation links should point to `docs/` folder
- Script references should point to `scripts/` folder

### Update CI/CD
Check if any CI/CD pipelines reference:
- `src/auth/` folder
- Old script paths
- Documentation paths

### Verify Build
```bash
dotnet restore
dotnet build Cambrian.sln
dotnet test
```

## Benefits

- ✅ Single source of truth for API code
- ✅ No merge conflicts blocking development
- ✅ Clean root directory
- ✅ Organized documentation
- ✅ Better maintainability
- ✅ Smaller repository size
