using System;
using System.Reflection;
using UnityEngine;

// SettingsWindow - interaktywne IMGUI okno ustawień
// Zapisuje ustawienia do Config.cs przez reflection i aplikuje je do ChatWindow
public class SettingsWindow
{
    public bool Visible { get; set; } = false;

    private Rect windowRect = new Rect(10, 390, 320, 220);
    private const string ReleaseDate = "2026-08-03 21:30"; // Release date and time

    // Ustawienia lokalne przechowywane w oknie
    private float opacity = 1f;
    private int windowWidth = 420;
    private int windowHeight = 300;
    private int fontSize = 12;
    private Color fontColor = Color.white;
    private float autoHideSeconds = 0f;
    private bool autoHideEnabled = false;
    private float refreshInterval = 5f;
    private bool lockWindowPosition = false;
    private float textBackgroundOpacity = 0.3f;

    public SettingsWindow()
    {
        LoadFromConfig();
    }

    // Rysuj okno
    public void Draw()
    {
        if (!Visible) return;

        // Ensure settings window is fully opaque (not affected by chat window opacity)
        Color originalColor = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 1f);

        string windowTitle = $"Settings [{ReleaseDate}]";
        windowRect = GUILayout.Window(GetWindowId(), windowRect, OnWindow, windowTitle);

        GUI.color = originalColor;
    }

    private int GetWindowId() => "YTSettingsWindow".GetHashCode();

    private void OnWindow(int id)
    {
        GUILayout.BeginVertical(GUILayout.Height(380));

        // 1. Opacity
        GUILayout.Label("Opacity", GUILayout.Height(14));
        opacity = GUILayout.HorizontalSlider(opacity, 0.1f, 1f, GUILayout.Height(10));

        // 2. Text Background Opacity
        GUILayout.Label("Text Background Opacity", GUILayout.Height(14));
        textBackgroundOpacity = GUILayout.HorizontalSlider(textBackgroundOpacity, 0f, 1f, GUILayout.Height(10));

        // 3. Window Width
        GUILayout.Label($"Window Width: {windowWidth}", GUILayout.Height(14));
        windowWidth = (int)GUILayout.HorizontalSlider(windowWidth, 200, 800, GUILayout.Height(10));

        // 4. Window Height
        GUILayout.Label($"Window Height: {windowHeight}", GUILayout.Height(14));
        windowHeight = (int)GUILayout.HorizontalSlider(windowHeight, 100, 800, GUILayout.Height(10));

        // 5. Font Size
        GUILayout.Label($"Font Size: {fontSize}", GUILayout.Height(14));
        fontSize = (int)GUILayout.HorizontalSlider(fontSize, 8, 32, GUILayout.Height(10));

        // 6. Font Color (RGB)
        GUILayout.Label("Font Color (RGB)", GUILayout.Height(14));
        fontColor.r = GUILayout.HorizontalSlider(fontColor.r, 0f, 1f, GUILayout.Height(10));
        fontColor.g = GUILayout.HorizontalSlider(fontColor.g, 0f, 1f, GUILayout.Height(10));
        fontColor.b = GUILayout.HorizontalSlider(fontColor.b, 0f, 1f, GUILayout.Height(10));
        GUILayout.Box(" ", GUILayout.Width(20), GUILayout.Height(10));

        // 7. Auto-hide seconds
        GUILayout.Label($"Auto-hide seconds: {autoHideSeconds:F0}", GUILayout.Height(14));
        autoHideSeconds = GUILayout.HorizontalSlider(autoHideSeconds, 0f, 600f, GUILayout.Height(10));

        // 8. Enable Auto-hide
        autoHideEnabled = GUILayout.Toggle(autoHideEnabled, "Enable Auto-hide", GUILayout.Height(14));

        // 9. Refresh Interval
        GUILayout.Label($"Refresh Interval: {refreshInterval:F1}s", GUILayout.Height(14));
        refreshInterval = GUILayout.HorizontalSlider(refreshInterval, 1f, 60f, GUILayout.Height(10));

        // 10. Lock Window Position
        lockWindowPosition = GUILayout.Toggle(lockWindowPosition, "Lock Window Position", GUILayout.Height(14));

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal(GUILayout.Height(20));
        if (GUILayout.Button("Save"))
        {
            SaveToConfig();
            ApplyToChatWindow();
            Visible = false;
        }

        if (GUILayout.Button("Reset Position"))
        {
            ResetWindowPosition();
        }

        if (GUILayout.Button("Close")) Visible = false;
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));
    }

    // Załaduj ustawienia z Config.cs (jeśli dostępny)
    private void LoadFromConfig()
    {
        try
        {
            var cfgType = Type.GetType("Config");
            if (cfgType == null) return;

            var getFloat = new Func<string, float>((name) =>
            {
                var f = cfgType.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null && f.GetValue(null) is float fv) return fv;
                var p = cfgType.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (p != null && p.GetValue(null, null) is float pv) return pv;
                return 0f;
            });

            var getInt = new Func<string, int>((name) =>
            {
                var f = cfgType.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null && f.GetValue(null) is int iv) return iv;
                var p = cfgType.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (p != null && p.GetValue(null, null) is int ip) return ip;
                return 0;
            });

            var getBool = new Func<string, bool>((name) =>
            {
                var f = cfgType.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null && f.GetValue(null) is bool bv) return bv;
                var p = cfgType.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (p != null && p.GetValue(null, null) is bool bp) return bp;
                return false;
            });

            opacity = getFloat("Opacity");
            var ww = getInt("WindowWidth"); if (ww > 0) windowWidth = ww;
            var wh = getInt("WindowHeight"); if (wh > 0) windowHeight = wh;
            var fs = getInt("FontSize"); if (fs > 0) fontSize = fs;
            float fr = getFloat("FontColorR"); float fg = getFloat("FontColorG"); float fb = getFloat("FontColorB");
            if (fr != 0 || fg != 0 || fb != 0) fontColor = new Color(fr, fg, fb);
            autoHideSeconds = getFloat("AutoHideSeconds");
            autoHideEnabled = getBool("AutoHide");
            refreshInterval = getFloat("RefreshInterval");
            lockWindowPosition = getBool("LockWindowPosition");
            textBackgroundOpacity = getFloat("TextBackgroundOpacity");
        }
        catch (Exception ex)
        {
            Debug.Log("[SettingsWindow] LoadFromConfig failed: " + ex.Message);
        }
    }

    // Zapisz ustawienia do Config.cs przez reflection
    private void SaveToConfig()
    {
        try
        {
            var cfgType = Type.GetType("Config");
            if (cfgType == null) return;

            Action<string, object> setValue = (name, val) =>
            {
                var f = cfgType.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null) { try { f.SetValue(null, val); return; } catch { } }
                var p = cfgType.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (p != null && p.CanWrite) { try { p.SetValue(null, val, null); return; } catch { } }
            };

            setValue("Opacity", opacity);
            setValue("WindowWidth", windowWidth);
            setValue("WindowHeight", windowHeight);
            setValue("FontSize", fontSize);
            setValue("FontColorR", fontColor.r);
            setValue("FontColorG", fontColor.g);
            setValue("FontColorB", fontColor.b);
            setValue("AutoHideSeconds", autoHideSeconds);
            setValue("AutoHide", autoHideEnabled);
            setValue("RefreshInterval", refreshInterval);
            setValue("LockWindowPosition", lockWindowPosition);
            setValue("TextBackgroundOpacity", textBackgroundOpacity);

            // Synchronizuj AutoHideTime na podstawie AutoHideSeconds
            if (autoHideSeconds > 0)
            {
                setValue("AutoHideTime", autoHideSeconds);
            }

            // Jeśli Config ma metodę Save lub SaveConfig -> wywołaj
            var saveMethod = cfgType.GetMethod("Save", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                             ?? cfgType.GetMethod("SaveConfig", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            saveMethod?.Invoke(null, null);
        }
        catch (Exception ex)
        {
            Debug.Log("[SettingsWindow] SaveToConfig failed: " + ex.Message);
        }
    }

    // Zresetuj pozycję okna czatu - ustawia domyślną pozycję bezpośrednio na instancji ChatWindow
    private void ResetWindowPosition()
    {
        try
        {
            var modType = Type.GetType("YTChatKSPMain");
            if (modType == null) return;

            var find = typeof(UnityEngine.Object).GetMethod("FindObjectOfType", new Type[] { typeof(Type) });
            var modInstance = find.Invoke(null, new object[] { modType });
            if (modInstance == null) return;

            var chatField = modType.GetField("chatWindow", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (chatField == null) return;

            var chatObj = chatField.GetValue(modInstance);
            if (chatObj == null) return;

            var rectField = chatObj.GetType().GetField("windowRect", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (rectField != null)
            {
                rectField.SetValue(chatObj, new Rect(10, 80, windowWidth, windowHeight));
            }
        }
        catch (Exception ex)
        {
            Debug.Log("[SettingsWindow] ResetWindowPosition failed: " + ex.Message);
        }
    }

    // Aplikuj ustawienia bezpośrednio do instancji ChatWindow (jeśli dostępna)
    private void ApplyToChatWindow()
    {
        try
        {
            var modType = Type.GetType("YTChatKSPMain");
            if (modType == null) return;

            var find = typeof(UnityEngine.Object).GetMethod("FindObjectOfType", new Type[] { typeof(Type) });
            var modInstance = find.Invoke(null, new object[] { modType });
            if (modInstance == null) return;

            var chatField = modType.GetField("chatWindow", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (chatField == null) return;

            var chatObj = chatField.GetValue(modInstance);
            if (chatObj == null) return;

            // Ustaw opacity
            var opField = chatObj.GetType().GetField("opacity", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (opField != null) opField.SetValue(chatObj, opacity);

            // Rozmiary okna
            var rectField = chatObj.GetType().GetField("windowRect", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (rectField != null)
            {
                var rect = (Rect)rectField.GetValue(chatObj);
                rect.width = windowWidth;
                rect.height = windowHeight;
                rectField.SetValue(chatObj, rect);
            }

            // Font size: spróbuj ustawić messageStyle.fontSize
            var styleField = chatObj.GetType().GetField("messageStyle", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (styleField != null)
            {
                var style = styleField.GetValue(chatObj) as GUIStyle;
                if (style != null) style.fontSize = fontSize;
            }

            // Zapisz do Config też
            SaveToConfig();
        }
        catch (Exception ex)
        {
            Debug.Log("[SettingsWindow] ApplyToChatWindow failed: " + ex.Message);
        }
    }
}
