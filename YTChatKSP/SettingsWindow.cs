using System;
using System.Reflection;
using UnityEngine;

// SettingsWindow - interaktywne IMGUI okno ustawień
// Zapisuje ustawienia do Config.cs przez reflection i aplikuje je do ChatWindow
public class SettingsWindow
{
    public bool Visible { get; set; } = false;

    private Rect windowRect = new Rect(10, 390, 320, 220);

    // Ustawienia lokalne przechowywane w oknie
    private float opacity = 1f;
    private int fontSize = 12;
    private Color fontColor = Color.white;
    private int windowWidth = 420;
    private int windowHeight = 300;
    private bool showBorder = true;
    private float autoHideSeconds = 0f;

    public SettingsWindow()
    {
        LoadFromConfig();
    }

    // Rysuj okno
    public void Draw()
    {
        if (!Visible) return;
        windowRect = GUILayout.Window(GetWindowId(), windowRect, OnWindow, "Settings");
    }

    private int GetWindowId() => "YTSettingsWindow".GetHashCode();

    private void OnWindow(int id)
    {
        GUILayout.BeginVertical();

        GUILayout.Label("Opacity");
        opacity = GUILayout.HorizontalSlider(opacity, 0.1f, 1f);

        GUILayout.Label($"Font Size: {fontSize}");
        fontSize = (int)GUILayout.HorizontalSlider(fontSize, 8, 32);

        GUILayout.Label("Font Color");
        // Simple color picker using RGB sliders (runtime Unity doesn't have Editor color picker)
        fontColor.r = GUILayout.HorizontalSlider(fontColor.r, 0f, 1f);
        fontColor.g = GUILayout.HorizontalSlider(fontColor.g, 0f, 1f);
        fontColor.b = GUILayout.HorizontalSlider(fontColor.b, 0f, 1f);
        GUILayout.Box(" ", GUILayout.Width(20), GUILayout.Height(20));

        GUILayout.Label("Window Size");
        GUILayout.BeginHorizontal();
        GUILayout.Label("W", GUILayout.Width(20));
        windowWidth = (int)GUILayout.HorizontalSlider(windowWidth, 200, 800);
        GUILayout.Label("H", GUILayout.Width(20));
        windowHeight = (int)GUILayout.HorizontalSlider(windowHeight, 100, 800);
        GUILayout.EndHorizontal();

        showBorder = GUILayout.Toggle(showBorder, "Show border");
        GUILayout.Label("Auto-hide seconds (0 = off)");
        autoHideSeconds = GUILayout.HorizontalSlider(autoHideSeconds, 0f, 600f);

        GUILayout.Space(6);

        GUILayout.BeginHorizontal();
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
            var fs = getInt("FontSize"); if (fs > 0) fontSize = fs;
            float fr = getFloat("FontColorR"); float fg = getFloat("FontColorG"); float fb = getFloat("FontColorB");
            if (fr != 0 || fg != 0 || fb != 0) fontColor = new Color(fr, fg, fb);
            var ww = getInt("WindowWidth"); if (ww > 0) windowWidth = ww;
            var wh = getInt("WindowHeight"); if (wh > 0) windowHeight = wh;
            showBorder = getBool("ShowBorder");
            autoHideSeconds = getFloat("AutoHideSeconds");
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
            setValue("FontSize", fontSize);
            setValue("FontColorR", fontColor.r);
            setValue("FontColorG", fontColor.g);
            setValue("FontColorB", fontColor.b);
            setValue("WindowWidth", windowWidth);
            setValue("WindowHeight", windowHeight);
            setValue("ShowBorder", showBorder);
            setValue("AutoHideSeconds", autoHideSeconds);

            // Synchronizuj AutoHideTime i AutoHide bool na podstawie AutoHideSeconds
            if (autoHideSeconds > 0)
            {
                setValue("AutoHideTime", autoHideSeconds);
                setValue("AutoHide", true);
            }
            else
            {
                setValue("AutoHide", false);
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

            // Ustaw ramkę
            var brField = chatObj.GetType().GetField("showBorder", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (brField != null) brField.SetValue(chatObj, showBorder);

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
