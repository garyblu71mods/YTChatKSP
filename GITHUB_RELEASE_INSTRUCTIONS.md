# GitHub Release Upload - Manual Instructions

## Jak stworzyć GitHub Release dla v1.0.0

### Krok 1: Otwórz GitHub
1. Przejdź do: https://github.com/garyblu71mods/YTChatKSP
2. Kliknij na **"Releases"** (na prawo strony, poniżej About)

### Krok 2: Utwórz nowy release
1. Kliknij na **"Create a new release"** (niebieski przycisk)
2. Lub jeśli nie ma takiego przycisku, kliknij na **"Draft a new release"**

### Krok 3: Wypełnij formularz

**Tag version:**
- Wpisz: `v1.0.0`

**Target:**
- Zostaw: `main`

**Release title:**
- Wpisz: `v1.0.0 - Initial Release`

**Description:**
- Skopiuj treść z poniżej:

```
## 🎉 Initial Release

Welcome to YTChatKSP v1.0.0 — the first official release of YouTube Chat integration for Kerbal Space Program!

## ✨ Features Included

### Core Functionality
- ✅ Real-time YouTube Chat Display
- ✅ Auto-scrolling Chat Window
- ✅ Backend API Integration
- ✅ File-based Logging

### Customization
- ✅ Window Opacity (0-100%)
- ✅ Custom Window Size
- ✅ Font Size & Color Control
- ✅ Text Wrapping
- ✅ Nickname Colors

### Auto-Management
- ✅ Auto-hide Timeout
- ✅ Auto-show on New Messages
- ✅ Configurable Refresh Interval
- ✅ Window Locking

### Visual Effects
- ✅ New Message Flash Effect
- ✅ Nickname Coloring
- ✅ Text-Only Mode

## 📥 Installation

1. Download `YTChatKSP.dll` below
2. Create folder: `GameData/YTChatKSP/`
3. Copy DLL into that folder
4. Ensure backend API runs on `http://localhost:5000`
5. Launch KSP

For full documentation, see [README.md](https://github.com/garyblu71mods/YTChatKSP/blob/main/README.md)

## 🔍 Debugging

Check debug log: `C:\Users\[YourUsername]\YTChatKSP_Debug.log`

## 📄 License

MIT License
```

### Krok 4: Dodaj załącznik (DLL file)

1. W sekcji **"Attach binaries"** (na dole) kliknij na pole drag-and-drop
2. Lub kliknij **"choose your files"**
3. Wybierz plik: `C:\Users\grzeg\source\repos\YTChatKSP\YTChatKSP.dll`

### Krok 5: Opcje publikacji

Zaznacz:
- ☐ **This is a pre-release** (ODZNACZ - to jest full release)
- ☑ **Discuss this release** (opcjonalnie)

### Krok 6: Opublikuj

Kliknij na **"Publish release"** (zielony przycisk)

---

## Rezultat

Po wykonaniu powyższych kroków:
- ✅ Release v1.0.0 będzie widoczny
- ✅ DLL będzie dostępny do pobrania
- ✅ Release notes będą wyświetlone
- ✅ GitHub pokaże link do pobierania

## Alternatywa: GitHub CLI

Jeśli masz zainstalowanego GitHub CLI, możesz użyć:

```powershell
gh release create v1.0.0 `
  --title "v1.0.0 - Initial Release" `
  --notes "See RELEASE_NOTES.md for details" `
  "C:\Users\grzeg\source\repos\YTChatKSP\YTChatKSP.dll"
```

---

**Gotowe! Teraz każdy może pobrać Twoją mod z GitHub Releases! 🚀**
