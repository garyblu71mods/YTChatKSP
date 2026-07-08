using System;
using System.IO;
using UnityEngine;

// Config - przechowuje ustawienia moda i zapis/odczyt do pliku settings.cfg
public static class Config
{
    // Główne ustawienia (używane przez okna)
    public static float Opacity = 1f;
    public static int FontSize = 12;
    public static Color FontColor = Color.white;
    public static int WindowWidth = 420;
    public static int WindowHeight = 300;
    public static bool AutoHide = false;
    public static float AutoHideTime = 0f;
    public static bool ColorNicknames = true;
    public static bool FlashNewMessage = true;
    public static bool TextOnlyMode = false;
    public static float RefreshInterval = 2f;
    public static bool LockWindowPosition = false;

    // Dodatkowe pola dla kompatybilności z różnymi nazwami użytymi w kodzie
    public static bool ShowBorder = true;
    public static float AutoHideSeconds = 0f;
    public static bool NickColoring = true;
    public static bool FlashNewMessages = true;
    public static int MessageLimit = 50;

    // Dodatkowe reprezentacje koloru (używane przez refleksję w niektórych miejscach)
    public static float FontColorR = 1f;
    public static float FontColorG = 1f;
    public static float FontColorB = 1f;

    // Ścieżka do pliku konfiguracji (GameData/YTChatKSP/PluginData/settings.cfg)
    private static string ConfigFilePath
    {
        get
        {
            try
            {
                // Application.dataPath zwykle wskazuje na <KSP root>/KSP_Data, więc bierzemy katalog nadrzędny
                var root = Path.GetDirectoryName(Application.dataPath);
                var dir = Path.Combine(root, "GameData", "YTChatKSP", "PluginData");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                return Path.Combine(dir, "settings.cfg");
            }
            catch
            {
                // Fallback: bieżący katalog
                return Path.Combine(Directory.GetCurrentDirectory(), "settings.cfg");
            }
        }
    }

    // Wczytaj ustawienia z pliku
    public static void Load()
    {
        try
        {
            string path = ConfigFilePath;
            if (!File.Exists(path))
            {
                Save();
                return;
            }

            string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            if (string.IsNullOrEmpty(json)) return;

            float fval; int ival; bool bval;
            if (TryExtractFloat(json, "Opacity", out fval)) Opacity = fval;
            if (TryExtractInt(json, "FontSize", out ival)) FontSize = ival;
            float fr = 1f, fg = 1f, fb = 1f;
            if (TryExtractFloat(json, "FontColorR", out fr)) FontColorR = fr;
            if (TryExtractFloat(json, "FontColorG", out fg)) FontColorG = fg;
            if (TryExtractFloat(json, "FontColorB", out fb)) FontColorB = fb;
            FontColor = new Color(FontColorR, FontColorG, FontColorB);
            if (TryExtractInt(json, "WindowWidth", out ival)) WindowWidth = ival;
            if (TryExtractInt(json, "WindowHeight", out ival)) WindowHeight = ival;
            if (TryExtractBool(json, "AutoHide", out bval)) AutoHide = bval;
            if (TryExtractFloat(json, "AutoHideTime", out fval)) AutoHideTime = fval;
            if (TryExtractFloat(json, "AutoHideSeconds", out fval)) AutoHideSeconds = fval;
            if (TryExtractBool(json, "ColorNicknames", out bval)) ColorNicknames = bval;
            if (TryExtractBool(json, "NickColoring", out bval)) NickColoring = bval;
            if (TryExtractBool(json, "FlashNewMessage", out bval)) FlashNewMessage = bval;
            if (TryExtractBool(json, "FlashNewMessages", out bval)) FlashNewMessages = bval;
            if (TryExtractBool(json, "TextOnlyMode", out bval)) TextOnlyMode = bval;
            if (TryExtractInt(json, "MessageLimit", out ival)) MessageLimit = ival;
            if (TryExtractFloat(json, "RefreshInterval", out fval)) RefreshInterval = fval;
            if (TryExtractBool(json, "LockWindowPosition", out bval)) LockWindowPosition = bval;
        }
        catch (Exception ex)
        {
            Debug.Log("[Config] Load failed: " + ex.Message);
        }
    }

    // Zapisz ustawienia do pliku
    public static void Save()
    {
        try
        {
            // synchronizuj pola kolorów
            FontColorR = FontColor.r; FontColorG = FontColor.g; FontColorB = FontColor.b;
            AutoHideSeconds = AutoHideTime;
            NickColoring = ColorNicknames;
            FlashNewMessages = FlashNewMessage;

            // Ręczne zbudowanie prostego JSON-a
            var sb = new System.Text.StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"Opacity\":{0},", Opacity);
            sb.AppendFormat("\"FontSize\":{0},", FontSize);
            sb.AppendFormat("\"FontColorR\":{0},", FontColorR);
            sb.AppendFormat("\"FontColorG\":{0},", FontColorG);
            sb.AppendFormat("\"FontColorB\":{0},", FontColorB);
            sb.AppendFormat("\"WindowWidth\":{0},", WindowWidth);
            sb.AppendFormat("\"WindowHeight\":{0},", WindowHeight);
            sb.AppendFormat("\"AutoHide\":{0},", AutoHide.ToString().ToLower());
            sb.AppendFormat("\"AutoHideTime\":{0},", AutoHideTime);
            sb.AppendFormat("\"AutoHideSeconds\":{0},", AutoHideSeconds);
            sb.AppendFormat("\"ColorNicknames\":{0},", ColorNicknames.ToString().ToLower());
            sb.AppendFormat("\"NickColoring\":{0},", NickColoring.ToString().ToLower());
            sb.AppendFormat("\"FlashNewMessage\":{0},", FlashNewMessage.ToString().ToLower());
            sb.AppendFormat("\"FlashNewMessages\":{0},", FlashNewMessages.ToString().ToLower());
            sb.AppendFormat("\"TextOnlyMode\":{0},", TextOnlyMode.ToString().ToLower());
            sb.AppendFormat("\"MessageLimit\":{0},", MessageLimit);
            sb.AppendFormat("\"RefreshInterval\":{0},", RefreshInterval);
            sb.AppendFormat("\"LockWindowPosition\":{0}", LockWindowPosition.ToString().ToLower());
            sb.Append("}");

            File.WriteAllText(ConfigFilePath, sb.ToString(), System.Text.Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Debug.Log("[Config] Save failed: " + ex.Message);
        }
    }

    private static bool TryExtractFloat(string json, string key, out float value)
    {
        value = 0f;
        var m = System.Text.RegularExpressions.Regex.Match(json, "\"" + System.Text.RegularExpressions.Regex.Escape(key) + "\"\\s*:\\s*([0-9.+-eE]+)");
        if (m.Success) return float.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value);
        return false;
    }

    private static bool TryExtractInt(string json, string key, out int value)
    {
        value = 0;
        var m = System.Text.RegularExpressions.Regex.Match(json, "\"" + System.Text.RegularExpressions.Regex.Escape(key) + "\"\\s*:\\s*([0-9]+)");
        if (m.Success) return int.TryParse(m.Groups[1].Value, out value);
        return false;
    }

    private static bool TryExtractBool(string json, string key, out bool value)
    {
        value = false;
        var m = System.Text.RegularExpressions.Regex.Match(json, "\"" + System.Text.RegularExpressions.Regex.Escape(key) + "\"\\s*:\\s*(true|false)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (m.Success) return bool.TryParse(m.Groups[1].Value, out value);
        return false;
    }

    // Struktura używana do serializacji
    [Serializable]
    private class ConfigData
    {
        public float Opacity = 1f;
        public int FontSize = 12;
        public float FontColorR = 1f;
        public float FontColorG = 1f;
        public float FontColorB = 1f;
        public int WindowWidth = 420;
        public int WindowHeight = 300;
        public bool ShowFrame = true;
        public bool ShowBorder = true;
        public bool AutoHide = false;
        public float AutoHideTime = 0f;
        public float AutoHideSeconds = 0f;
        public bool ColorNicknames = true;
        public bool NickColoring = true;
        public bool FlashNewMessage = true;
        public bool FlashNewMessages = true;
        public bool TextOnlyMode = false;
        public int MessageLimit = 50;
    }
}
