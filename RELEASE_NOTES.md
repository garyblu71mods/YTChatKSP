# YTChatKSP v1.0.0 Release Notes

## 🎉 Initial Release

Welcome to YTChatKSP v1.0.0 — the first official release of YouTube Chat integration for Kerbal Space Program!

## ✨ Features Included

### Core Functionality
- ✅ **Real-time YouTube Chat Display** — See live chat messages in-game
- ✅ **Auto-scrolling Chat Window** — Latest messages always visible
- ✅ **Backend API Integration** — Connects to custom backend service
- ✅ **File-based Logging** — Debug logs to `YTChatKSP_Debug.log`

### Customization Options
- ✅ **Window Opacity** — Adjust transparency (0-100%)
- ✅ **Window Size** — Customize dimensions (200-1200px wide, 100-800px tall)
- ✅ **Font Size & Color** — Full RGB color control (10-40px size)
- ✅ **Text Wrapping** — Automatic line wrapping for long messages

### Auto-Management
- ✅ **Auto-hide Timeout** — Hide window after inactivity
- ✅ **Auto-show on Messages** — Chat window appears when new messages arrive
- ✅ **Refresh Interval** — Configurable message fetch rate
- ✅ **Window Locking** — Prevent accidental window dragging

### Settings Persistence
- ✅ **Auto-save Settings** — All configuration saved to `settings.cfg`
- ✅ **Reset to Defaults** — One-click button to restore original settings

## 📥 Installation

1. Download `YTChatKSP.dll` from this release
2. Create folder: `GameData/YTChatKSP/`
3. Copy `YTChatKSP.dll` into that folder
4. Ensure backend API is running on `http://localhost:5000`
5. Launch KSP — mod loads automatically

## 🔧 Backend Requirements

Your backend service must provide:

**GET `/messages`** — Returns JSON array:
```json
[
  {
	"id": "msg_123",
	"nick": "Username",
	"text": "Message content",
	"timestamp": "2024-01-15T14:30:00Z"
  }
]
```

**POST `/send`** — Optional endpoint to send messages back

## 🐛 Known Issues

- Messages are not persisted between game sessions
- Maximum ~100 messages displayed (scrollable)
- Auto-hide timeout resets with each new message (by design)

## 📝 Configuration File

Settings are saved to: `GameData/YTChatKSP/PluginData/settings.cfg`

Example:
```
Opacity=0.85
FontSize=14
FontColor=1.0,1.0,1.0
WindowWidth=420
WindowHeight=300
AutoHide=True
AutoHideTime=10
RefreshInterval=2
```

## 🔍 Debugging

If messages don't appear, check: `C:\Users\[YourUsername]\YTChatKSP_Debug.log`

Log shows:
- Backend connection attempts
- JSON parsing results
- Message fetch success/failure
- Font/color application

## 🙏 Credits

- Built for [Kerbal Space Program](https://www.kerbalspaceprogram.com/)
- Uses [ClickThroughBlocker](https://github.com/linuxgurugamer/ClickThroughBlocker)
- Optional [ToolbarControl](https://github.com/linuxgurugamer/ToolbarControl)

## 📦 What's Included

- `YTChatKSP.dll` — Compiled mod binary
- Source code available on GitHub

## 🚀 What's Next?

Future versions may include:
- Message history persistence
- Sound notifications
- Emote support
- Username filtering
- Multiplayer sync features

## 🐞 Bug Reports

Found an issue? Please open a GitHub issue with:
- KSP version
- Mod version (v1.0.0)
- Steps to reproduce
- Debug log contents

## 📄 License

MIT License — Free to use and modify

---

**Enjoy streaming KSP with your viewers! 🎮🚀**

For more information, see [README.md](https://github.com/garyblu71mods/YTChatKSP/blob/main/README.md)
