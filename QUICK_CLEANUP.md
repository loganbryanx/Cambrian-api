# Quick Cleanup Reference

## ⚡ Fast Track (2 minutes)

```powershell
# Windows PowerShell
.\cleanup.ps1
git add -A
git commit -m "chore: clean up repository structure"
git push
```

```bash
# Linux/Mac/WSL
chmod +x cleanup.sh
./cleanup.sh
git add -A
git commit -m "chore: clean up repository structure"
git push
```

## 📋 What Gets Changed

| Action | Before | After |
|--------|--------|-------|
| **Remove Duplicates** | `src/auth/Cambrian.Api/` | ❌ Deleted |
| **Remove Metadata** | `.vs/` folder | ❌ Deleted |
| **Move Docs** | `*.md` files at root | ✅ `docs/*.md` |
| **Move Scripts** | `*.ps1` files at root | ✅ `scripts/*.ps1` |
| **Fix Conflicts** | Program.cs with `<<<<<<<` | ✅ Resolved |

## ✅ Verification Steps

```bash
# 1. Check structure
ls -la src/         # Should NOT contain 'auth' folder
ls -la docs/        # Should contain 8+ markdown files
ls -la scripts/     # Should contain 6+ script files

# 2. Verify build
dotnet build Cambrian.sln

# 3. Check git status
git status          # Review all changes

# 4. Test run
dotnet run --project src/Cambrian.Api
curl http://localhost:3000/auth/health
```

## 🔙 Rollback (if needed)

```bash
# Undo all uncommitted changes
git reset --hard HEAD

# Or create backup first
git checkout -b backup-before-cleanup
git checkout main
```

## 📚 Full Documentation

- **Detailed Plan**: See [CLEANUP_PLAN.md](CLEANUP_PLAN.md)
- **Full Summary**: See [CLEANUP_SUMMARY.md](CLEANUP_SUMMARY.md)

## 🎯 Files Changed

✅ Fixed: `src/Cambrian.Api/Program.cs` (merge conflicts)  
✅ Created: `cleanup.ps1` (PowerShell script)  
✅ Created: `cleanup.sh` (Bash script)  
✅ Created: `CLEANUP_PLAN.md` (detailed analysis)  
✅ Created: `CLEANUP_SUMMARY.md` (executive summary)  
✅ Created: `QUICK_CLEANUP.md` (this file)  

## ⚠️ Important Notes

- `.gitignore` already contains `.vs/` - no changes needed
- Solution file already correct - references only `src/Cambrian.Api/`
- All operations use `git mv` and `git rm` to preserve history
- Migrations folder kept if it contains files

## 🚀 After Cleanup

Update documentation references in:
- [ ] `README.md` - Point to `docs/` folder
- [ ] `.github/workflows/` - Update script paths
- [ ] Any internal documentation

---

**Ready?** Run the cleanup script and you're done! 🎉
