# Contributing to YTChatKSP

Thank you for your interest in contributing to YTChatKSP! This document provides guidelines for contributing to the project.

## Code of Conduct

Be respectful, constructive, and considerate in all interactions. We aim to maintain a welcoming community.

## How to Contribute

### Reporting Bugs
1. Check existing issues to avoid duplicates
2. Provide clear steps to reproduce the bug
3. Include your KSP version, mod version, and any relevant logs
4. Attach the debug log file (`YTChatKSP_Debug.log`)

### Suggesting Features
1. Open an issue with the `enhancement` label
2. Describe the feature and its use case
3. Explain why this feature would be valuable

### Submitting Pull Requests
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Make your changes following the code style (see below)
4. Test thoroughly in KSP
5. Commit with clear messages
6. Push to your fork and open a pull request

## Code Style

- **Language:** C# (Unity/.NET Framework 4.7.2)
- **Indentation:** 4 spaces (no tabs)
- **Naming:**
  - Classes: `PascalCase` (e.g., `ChatWindow`, `ServerClient`)
  - Methods: `PascalCase` (e.g., `FetchMessages()`, `DrawContents()`)
  - Private fields: `camelCase` prefixed with underscore (e.g., `_isFlashing`)
  - Public properties: `PascalCase` (e.g., `Visible`)
- **Comments:** Use `//` for single-line, `/* */` for multi-line
- **Logging:** Use `LogToFile()` for debug logging

## Build Requirements

- Visual Studio 2022 (Community Edition)
- .NET Framework 4.7.2
- Kerbal Space Program installation (for references)

## Testing

Before submitting:
1. Build Release configuration
2. Test the DLL in KSP
3. Check the debug log for errors
4. Verify all settings work as expected
5. Test with auto-hide enabled/disabled
6. Verify message display and scrolling

## Project Structure

```
YTChatKSP/
├── YTChatKSP.cs              # Main addon entry point
├── ServerClient.cs           # HTTP communication
├── Config.cs                 # Settings persistence
├── ChatMessage.cs            # Message data model
├── SettingsWindow.cs         # Settings UI
├── ReplyWindow.cs            # Reply functionality
├── PerformanceMonitor.cs     # Debug logging
└── bin/Release/YTChatKSP.dll # Compiled output
```

## Backend API

The mod expects a backend service on `http://localhost:5000` with:
- `GET /messages` — Returns JSON array of chat messages
- `POST /send` — Sends a reply message (optional)

Message format:
```json
{
  "id": "unique_id",
  "nick": "username",
  "text": "message content",
  "timestamp": "2024-01-15T14:30:45Z"
}
```

## Performance Considerations

- Minimize OnGUI() calls
- Cache reflection results
- Use throttling for frequent operations
- Keep network requests batched
- Monitor CPU and memory usage with PerformanceMonitor

## Questions?

Open an issue with the `question` label and we'll help!

Thank you for contributing! 🚀
