# YTChatKSP — YouTube Chat Integration for Kerbal Space Program

A Kerbal Space Program mod that brings YouTube live chat directly into the game, enabling real-time interaction between stream viewers and KSP players during live streams.

## Overview

YTChatKSP creates an in-game chat window that displays live YouTube chat messages while you're playing Kerbal Space Program. Perfect for streamers who want to see viewer chat without alt-tabbing, and for community members watching the stream to feel more connected to the gameplay.

The mod communicates with a backend service that proxies YouTube's live chat API, allowing the mod to fetch and display messages in real-time.

## Features

### Chat Display
- **Real-time Message Rendering** — Messages appear in a scrollable in-game window
- **Auto-scroll to Latest** — Always shows the newest messages at the bottom
- **Text Wrapping** — Long messages wrap to multiple lines automatically

### Customization
- **Window Opacity** — Adjust transparency from 0-100% so you can see the game behind the chat
- **Custom Size** — Resize the chat window (200-1200px wide, 100-800px tall)
- **Font Control** — Change text size (10-40px) and color (full RGB control)
- **Window Locking** — Lock the window position to prevent accidental moving

### Auto-Management
- **Auto-hide** — Automatically hide the chat window after a configurable timeout
- **Auto-show on Messages** — Chat window automatically appears when new messages arrive
- **Refresh Interval** — Control how often the mod checks for new messages

## Installation

### Requirements
- Kerbal Space Program 1.8 or later (tested on 1.12.x)
- A running backend API service on `http://localhost:5000`

### Steps
1. Download the latest release (`.dll` file)
2. Extract to `GameData/YTChatKSP/` in your KSP installation directory
3. Ensure the backend service is running before launching KSP
4. Launch KSP — the mod loads automatically via KSPAddon

### Folder Structure
```
GameData/
└── YTChatKSP/
	├── YTChatKSP.dll
	└── PluginData/
		└── settings.cfg  (auto-created on first run)
```

## Usage

### Opening the Chat Window
- Click the mod button in the application launcher (red icon)
- Or use the toolbar (if ToolbarControl is installed)

### Settings
1. Click the **Settings** button inside the chat window
2. Adjust any option using sliders/toggles
3. Click **Save & Close** to persist settings
4. Click **Reset Defaults** to restore original settings

### Configuration File
Settings are saved to: `GameData/YTChatKSP/PluginData/settings.cfg`

You can manually edit this file if needed:
```
{
  "Opacity": 0.85,
  "FontSize": 14,
  "FontColorR": 1.0,
  "FontColorG": 1.0,
  "FontColorB": 1.0,
  "WindowWidth": 420,
  "WindowHeight": 300,
  "ShowBorder": true,
  "AutoHide": false,
  "AutoHideTime": 10,
  "AutoHideSeconds": 0,
  "RefreshInterval": 3,
  "LockWindowPosition": false,
  "MessageLimit": 50
}
```

## Backend API Setup

YTChatKSP requires a backend service running on `http://localhost:5000` with two endpoints:

### GET `/messages`
Returns the current list of chat messages.

**Response:**
```json
[
  {
	"id": "msg_abc123",
	"nick": "StreamViewer42",
	"text": "Nice rocket! Go to the Mun!",
	"timestamp": "2024-01-15T14:30:45Z"
  },
  {
	"id": "msg_abc124",
	"nick": "KSPFan",
	"text": "Love this stream format!",
	"timestamp": "2024-01-15T14:31:12Z"
  }
]
```

**Required Fields:**
- `id` (string) — Unique message identifier
- `nick` (string) — Username/nickname
- `text` (string) — Message content
- `timestamp` (string) — ISO 8601 timestamp

### POST `/send` (Optional)
Send a reply message back to the chat service.

**Request:**
```json
{
  "text": "Thanks for watching!"
}
```

## Debugging

If messages aren't appearing, check the debug log file:

**Log Location:** `C:\Users\[YourUsername]\YTChatKSP_Debug.log`

**Log Format:**
```
[14:30:45] [ServerClient] Starting fetch from http://localhost:5000/messages
[14:30:45] [ServerClient] Raw JSON received: [{"id":"msg_123"...}]
[14:30:45] [ServerClient] Successfully parsed 5 messages
[14:30:45] [ChatWindow] Successfully displayed 5 messages
```

### Common Issues

1. **Chat window won't show**
   - Check if backend API is running on `http://localhost:5000`
   - Check debug log for connection errors
   - Verify backend returns valid JSON

2. **Messages don't update**
   - Check `RefreshInterval` in settings (default 2 seconds)
   - Verify backend is returning new messages
   - Check debug log for fetch errors

3. **Text is cut off or overlapping**
   - Adjust `Font Size` setting (smaller if overlapping, larger if cut off)
   - Increase `Window Height` to show more messages at once

4. **Chat appears/disappears constantly**
   - Disable `Auto-hide` or increase `Auto-hide Timeout`
   - Check if backend is sending many rapid updates

## Technical Details

### Architecture
- **Platform:** Unity 2019.2.2f1 + .NET Framework 4.7.2
- **Communication:** UnityWebRequest (synchronous-compatible HTTP)
- **UI System:** IMGUI (in-game GUI)
- **Data Format:** JSON (manual parsing for compatibility)

### Key Files
- `YTChatKSP.cs` — Main addon entry point (650+ lines)
- `ServerClient.cs` — HTTP backend communication with logging
- `Config.cs` — Settings persistence
- `ChatMessage.cs` — Message data model
- `ToolbarControlWrapper.cs` — Optional toolbar integration

### Build from Source

**Requirements:**
- Visual Studio 2022 (Community Edition works)
- .NET Framework 4.7.2
- KSP installation (for DLL references)

**Steps:**
```bash
git clone https://github.com/garyblu71mods/YTChatKSP.git
cd YTChatKSP
# Edit YTChatKSP.csproj to point to your KSP installation
dotnet build YTChatKSP.sln
```

**Output:** `bin/Debug/YTChatKSP.dll`

## Mod Compatibility

- ✅ **ToolbarControl** — Optional toolbar integration
- ✅ **ClickThroughBlocker** — Prevents UI click-through
- ✅ **ModuleManager** — Not required
- ✅ **Most visual mods** — No conflicts

## Performance

- **CPU Impact:** Minimal (~1-2ms per check)
- **Memory:** ~5-10 MB
- **Network:** Single HTTP request every N seconds (configurable, default 2s)
- **FPS Impact:** Negligible (only renders when visible)

## Known Limitations

- Messages are not persisted between game sessions (in-memory only)
- Maximum ~100 messages displayed at once (scrollable)
- Auto-hide timeout resets with each new message (by design)
- Font colors use named colors only (no custom fonts)

## License

MIT License — See LICENSE file for details

## Support & Bug Reports

For issues, questions, or suggestions:
- Open an issue on GitHub
- Check the debug log (`YTChatKSP_Debug.log`)
- Provide your game version and mod version

## Credits

- Built for [Kerbal Space Program](https://www.kerbalspaceprogram.com/)
- Uses [ClickThroughBlocker](https://github.com/linuxgurugamer/ClickThroughBlocker) by linuxgurugamer
- Optional [ToolbarControl](https://github.com/linuxgurugamer/ToolbarControl) by linuxgurugamer
- YouTube chat integration via custom backend proxy

---

**Enjoy sharing your KSP adventures with your stream viewers! 🚀**
