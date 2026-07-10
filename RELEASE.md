# Release to GitHub

## Pre-Release Checklist

- [ ] All tests pass in KSP
- [ ] Debug log is clean (no errors)
- [ ] Auto-hide feature works correctly
- [ ] Settings save and load properly
- [ ] FPS impact is minimal (~1-2ms)
- [ ] Version number updated in code
- [ ] RELEASE_NOTES.md updated
- [ ] README.md is current
- [ ] Build artifacts cleaned (obj/, Debug/)

## Release Steps

### 1. Prepare Release

```powershell
# Clean and build Release configuration
cd C:\Users\grzeg\source\repos\YTChatKSP\YTChatKSP
msbuild YTChatKSP.csproj -p:Configuration=Release

# Copy DLL to release folder
Copy-Item -Path "bin\Release\YTChatKSP.dll" `
  -Destination "..\Release\GameData\YTChatKSP\YTChatKSP.dll" -Force
```

### 2. Create Release Zip

```powershell
# Create versioned zip file
$version = "1.0.6"  # Update this
Compress-Archive -Path "..\Release\GameData" `
  -DestinationPath "..\Release\YTChatKSP-v$version.zip" -Force

Write-Host "Release zip created: YTChatKSP-v$version.zip"
```

### 3. Commit and Tag

```powershell
cd C:\Users\grzeg\source\repos\YTChatKSP

git add -A
git commit -m "Release v1.0.6: Auto-hide fixes and cleanup"
git tag -a v1.0.6 -m "Version 1.0.6 Release"
git push origin main
git push origin v1.0.6
```

### 4. GitHub Release

1. Go to https://github.com/garyblu71mods/YTChatKSP/releases
2. Click "Draft a new release"
3. Select tag: `v1.0.6`
4. Title: `YTChatKSP v1.0.6`
5. Description: Copy from RELEASE_NOTES.md
6. Upload binary: `YTChatKSP-v1.0.6.zip`
7. Publish release

## Version Numbering

Use semantic versioning: `MAJOR.MINOR.PATCH`

- **MAJOR**: Breaking changes (rare)
- **MINOR**: New features or significant improvements
- **PATCH**: Bug fixes and minor improvements

## Release Notes Template

```markdown
# YTChatKSP v1.0.6

## 🎯 Changes

### ✨ Features
- Auto-hide now properly resets on new messages
- Improved performance (reduced FPS spikes)

### 🐛 Bug Fixes
- Fixed auto-hide not showing window on new messages
- Removed flash animation feature

### ⚡ Performance
- Reduced timeout from 5s to 1s
- Throttle interval adjusted from 3s to 1s

### 🧹 Cleanup
- Removed unused features (Color Nicknames, Text Only Mode)
- Cleaned build artifacts

## 📥 Installation

1. Download `YTChatKSP-v1.0.6.zip`
2. Extract to `GameData/YTChatKSP/` in your KSP folder
3. Ensure backend API is running on `http://localhost:5000`
4. Launch KSP

## ⚙️ Backend Requirements

Backend service on `http://localhost:5000`:
- `GET /messages` — Returns chat messages JSON
- `POST /send` (optional) — Send reply messages

## 🆘 Support

- Report issues: https://github.com/garyblu71mods/YTChatKSP/issues
- Check debug log: `YTChatKSP_Debug.log`
- See README.md for detailed documentation

---

**Special thanks to all contributors!** 🚀
```

## Troubleshooting Release

### DLL not loading in KSP
- Check GameData folder structure: `GameData/YTChatKSP/YTChatKSP.dll`
- Verify assembly versions match
- Check KSP debug log for load errors

### Settings not saving
- Verify `GameData/YTChatKSP/PluginData/` folder exists
- Check file permissions
- Verify JSON format in `settings.cfg`

### Messages not appearing
- Verify backend API is running on `http://localhost:5000`
- Check debug log: `YTChatKSP_Debug.log`
- Test endpoint: `curl http://localhost:5000/messages`

## Rollback Procedure

If release has critical bugs:

```powershell
git tag -d v1.0.6
git push origin :v1.0.6
git revert <commit-hash>
git push origin main
```

Then create new release with fixed version.
