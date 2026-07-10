# YTChatKSP Setup Guide

Quick setup guide to get YTChatKSP working in minutes.

## Prerequisites

- Kerbal Space Program (1.8+)
- A running backend API service on `http://localhost:5000`
- Internet connection during gameplay

## 5-Minute Setup

### Step 1: Install the Mod

1. Download latest release from: https://github.com/garyblu71mods/YTChatKSP/releases
2. Extract the zip file
3. Copy the `GameData/YTChatKSP/` folder to your KSP installation:
   ```
   C:\Program Files\Epic Games\KerbalSpaceProgram\GameData\YTChatKSP\
   ```

### Step 2: Start Backend Service

Your backend service must be running before launching KSP. It should:
- Listen on `http://localhost:5000`
- Respond to `GET /messages` with JSON array
- (Optional) Accept `POST /send` for replies

**Example backend response:**
```json
[
  {
	"id": "msg_1",
	"nick": "StreamViewer42",
	"text": "Nice rocket!",
	"timestamp": "2024-01-15T14:30:45Z"
  }
]
```

### Step 3: Launch KSP

1. Start KSP normally
2. Look for the **red YTChat icon** in the application launcher (top-right)
3. Click the icon to open the chat window

### Step 4: Configure Settings (Optional)

1. Click **Settings** inside the chat window
2. Adjust preferences:
   - **Opacity** — How transparent the window is (0-100%)
   - **Font Size** — Text size (8-40px)
   - **Font Color** — RGB sliders for text color
   - **Window Size** — Width and height
   - **Auto-hide** — Timeout to hide window (0-600 seconds)
3. Click **Save & Close**

Your settings are saved automatically to: `GameData/YTChatKSP/PluginData/settings.cfg`

## Troubleshooting

### Chat window doesn't appear

**Solution:**
1. Check backend is running: `curl http://localhost:5000/messages`
2. Look for error in debug log: `C:\Users\[YourUsername]\YTChatKSP_Debug.log`
3. If backend down, window still loads (just empty)

### Messages not showing up

**Check in this order:**

1. **Backend connectivity**
   ```powershell
   $response = Invoke-WebRequest http://localhost:5000/messages
   $response | ConvertFrom-Json | Format-Table
   ```
   Should return message objects with `id`, `nick`, `text`, `timestamp`

2. **Message format**
   Required fields:
   - `id` (string) — Unique identifier
   - `nick` (string) — Username
   - `text` (string) — Message content
   - `timestamp` (string) — ISO 8601 format

3. **Debug log**
   ```
   [timestamp] [ServerClient] Starting fetch from http://localhost:5000/messages
   [timestamp] [ServerClient] Successfully parsed X messages
   ```
   If you don't see these, backend isn't running or unreachable.

### FPS drops when chat is visible

**Solution:**
1. Reduce window size (smaller = faster)
2. Increase `Refresh Interval` in backend settings
3. Disable other visual mods temporarily
4. Check debug log for performance issues

### Settings not saving

**Solution:**
1. Verify folder exists: `GameData/YTChatKSP/PluginData/`
2. Create if missing (mod should create it)
3. Check Windows file permissions (read/write to GameData)
4. Restart KSP after changing settings

## Backend API Details

### GET /messages

**Request:**
```
GET http://localhost:5000/messages
```

**Expected Response:**
```json
[
  {
	"id": "unique_id_123",
	"nick": "StreamViewer42",
	"text": "Nice rocket! Go to the Mun!",
	"timestamp": "2024-01-15T14:30:45Z"
  },
  {
	"id": "unique_id_124",
	"nick": "KSPFan",
	"text": "Love this stream format!",
	"timestamp": "2024-01-15T14:31:12Z"
  }
]
```

**Response Requirements:**
- HTTP 200 OK status
- Valid JSON array
- Each message has required fields
- Timestamp in ISO 8601 format

### POST /send (Optional)

If your backend supports it, users can reply:

**Request:**
```json
POST http://localhost:5000/send
Content-Type: application/json

{
  "text": "Thanks for watching!"
}
```

**Response:**
Should return HTTP 200 OK or error code.

## File Locations

| File/Folder | Location |
|---|---|
| Mod DLL | `GameData/YTChatKSP/YTChatKSP.dll` |
| Settings | `GameData/YTChatKSP/PluginData/settings.cfg` |
| Debug Log | `C:\Users\[YourUsername]\YTChatKSP_Debug.log` |
| Release Notes | See RELEASE_NOTES.md |

## Environment Variables (Optional)

You can override the backend URL via environment variable:
```powershell
$env:YTCHAT_API = "http://your-server:5000"
# Then launch KSP
```

(Not yet implemented - requires code change)

## Keyboard Shortcuts

- **Alt+Ctrl+Y** — Toggle chat window (when ToolbarControl installed)
- **Click red icon** — Toggle chat window

## Performance Tips

1. **Reduce message fetch frequency**
   - Increase `Refresh Interval` in backend
   - Default: 3 seconds, can go up to 10s

2. **Minimize window opacity changes**
   - Set opacity once, don't adjust every frame

3. **Limit message history**
   - Keep `MessageLimit` at 50 or below
   - Older messages scroll off

4. **Monitor FPS**
   - Debug log shows FPS impact
   - Should be <2ms per frame

## Uninstall

Simply delete the `GameData/YTChatKSP/` folder from your KSP installation.

Settings file at `PluginData/settings.cfg` is left behind (safe to delete).

## Getting Help

1. Check this guide for common issues
2. Read README.md for detailed documentation
3. Check debug log: `YTChatKSP_Debug.log`
4. Open an issue on GitHub: https://github.com/garyblu71mods/YTChatKSP/issues

## Next Steps

- ✅ Install mod
- ✅ Start backend service
- ✅ Launch KSP
- ✅ Open chat window
- ✅ Customize settings
- 🚀 **Enjoy your stream!**

---

**Need help? See CONTRIBUTING.md for community support.**
