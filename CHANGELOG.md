# Changelog

All notable changes to YTChatKSP are documented in this file.

## [Unreleased]

### Added
- Comprehensive documentation (README, CONTRIBUTING, CODE_OF_CONDUCT)
- GitHub Actions CI/CD pipeline
- CHANGELOG tracking

### Changed
- Throttle interval optimized from 3s to 1s for snappier UI response
- HTTP timeout reduced from 5s to 1s for faster failure detection
- Auto-hide logic completely redesigned for reliable window showing

### Fixed
- Auto-hide timer now resets properly when new messages arrive
- Auto-hide window now shows correctly on new messages
- Settings changes apply faster (1s instead of 3s)

### Removed
- Color Nicknames feature (unused)
- Flash New Messages effect (unused visual effect)
- Text Only Mode feature (unused UI variant)
- Build artifacts from repository (obj/, Debug/)

## [1.0.5] - 2024-01-15

### Added
- Full settings GUI with sliders and toggles
- Auto-hide timeout configuration (0-600 seconds)
- Window position locking feature
- Debug logging to file (YTChatKSP_Debug.log)

### Changed
- Improved message scrolling behavior
- Optimized Config file handling
- Better error messages in logs

### Fixed
- Message display alignment issues
- Config file loading on startup

## [1.0.4] - 2024-01-14

### Added
- Settings window with Save/Reset/Close buttons
- Auto-hide feature (experimental)
- Refresh interval configuration

### Fixed
- Memory leak in message caching
- GUI initialization errors

## [1.0.3] - 2024-01-13

### Added
- Initial release with core features
- Real-time chat message display
- HTTP backend communication
- Settings persistence
- Toolbar integration

### Features
- Chat window with scrollable message list
- Text wrapping for long messages
- Customizable window size and opacity
- Font size and color controls
- Window position locking
- Backend API integration (GET /messages, POST /send)

## [1.0.0] - [Early versions]

Initial development and prototyping.

---

## Version Legend

### Status Labels
- ✅ Stable
- 🔄 Maintenance
- ⚠️ Deprecated
- 🚀 Active Development

### Change Types
- **Added**: New features
- **Changed**: Changes to existing functionality
- **Fixed**: Bug fixes
- **Removed**: Removed features or code
- **Deprecated**: Features marked for removal
- **Security**: Security improvements

## Upgrade Guide

### From 1.0.3 → 1.0.5+

1. Backup your `settings.cfg`
2. Replace `YTChatKSP.dll` in `GameData/YTChatKSP/`
3. Delete old `settings.cfg` to regenerate with new format
4. Reconfigure settings in-game

Settings format changed from key=value to JSON format. Old settings file will be incompatible.

### From 1.0.5 → 1.0.6+

1. Replace `YTChatKSP.dll`
2. Existing settings will be migrated automatically
3. Test auto-hide feature (now works correctly)

No manual configuration needed.

## Known Issues

### Resolved
- ✅ Auto-hide not showing on new messages (FIXED in v1.0.6)
- ✅ Settings changes delayed (FIXED in v1.0.6)
- ✅ FPS spikes on message fetch (FIXED in v1.0.6)

### Current (if any)
- None known at this time

## Future Roadmap

### Planned Features
- [ ] Message persistence across game sessions
- [ ] Sound notifications for new messages
- [ ] Custom message filters/search
- [ ] KSP Part tooltip integration
- [ ] Multi-backend support

### Under Consideration
- [ ] Message reactions/emojis support
- [ ] User roles display (moderators, etc.)
- [ ] Chat history browser
- [ ] Message timestamp display

### Not Planned
- Color Nicknames (too visual, removed)
- Flash animations (distracting)
- Text-only UI mode (not practical)

---

**For detailed release notes, see RELEASE_NOTES.md**

**For contribution guidelines, see CONTRIBUTING.md**
