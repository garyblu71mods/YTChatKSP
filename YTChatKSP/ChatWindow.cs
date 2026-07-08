using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// ChatWindow - IMGUI okno czatu wykorzystujące ClickThroughBlocker.GUILayoutWindow (jeśli dostępny)
// Funkcje: wyświetlanie wiadomości z ServerClient, wrap tekstu, kolorowanie nicków, miganie nowych wiadomości,
// auto-hide według Config, opacity, ramka ON/OFF, tryb "tylko tekst", drag & drop, limit wiadomości.
public class ChatWindow
{
    // Widoczność okna
    public bool Visible { get; set; } = false;

    // Pozycja i rozmiary okna
    private Rect windowRect = new Rect(10, 80, 420, 300);

    // Przewijanie listy wiadomości
    private Vector2 scrollPos = Vector2.zero;

    // Lista wewnętrzna wyświetlanych wiadomości
    private readonly List<ChatMessage> messages = new List<ChatMessage>();

    // Id wiadomości już widzianych (do flasha)
    private readonly HashSet<string> seenMessageIds = new HashSet<string>();
    private readonly Dictionary<string, float> flashStart = new Dictionary<string, float>();

    // Ustawienia (mogą być nadpisane z Config)
    private int messageLimit = 50;
    private float autoHideSeconds = 0f; // 0 = off
    private float opacity = 1f;
    private bool showBorder = true;
    private bool textOnly = false;

    // Ostatnia aktywność (ostatnia otrzymana wiadomość)
    private float lastActivityTime = 0f;

    // Styl do wrapowania tekstu
    private GUIStyle messageStyle;

    // Konstruktor - załaduj ustawienia z Config (przez reflection) i ustaw style
    public ChatWindow()
    {
        LoadConfig();

        messageStyle = new GUIStyle(GUI.skin.label);
        messageStyle.wordWrap = true;
    }

    // Główna metoda rysująca okno (wywoływana z OnGUI)
    public void Draw()
    {
        if (!Visible) return;

        // Odśwież wiadomości (pobierz z ServerClient)
        FetchMessagesIfNeeded();

        // Auto-hide: jeśli włączone i brak aktywności dłuższej niż autoHideSeconds
        if (autoHideSeconds > 0f && Time.realtimeSinceStartup - lastActivityTime > autoHideSeconds)
        {
            Visible = false;
            return;
        }

        // Ustawienie alpha (opacity)
        Color prevColor = GUI.color;
        GUI.color = new Color(prevColor.r, prevColor.g, prevColor.b, Mathf.Clamp01(opacity));

        // Użyj ClickThroughBlocker.GUILayoutWindow jeśli dostępny, aby okno było nieklikalne poza elementami
        bool usedCTB = false;
        try
        {
            var ctbType = Type.GetType("ClickThroughBlocker, Assembly-CSharp") ?? Type.GetType("ClickThroughBlocker");
            if (ctbType != null)
            {
                var method = ctbType.GetMethod("GUILayoutWindow", BindingFlags.Public | BindingFlags.Static);
                if (method != null)
                {
                    // Wywołanie: zwróci Rect podobnie jak GUILayout.Window
                    object ret = method.Invoke(null, new object[] { GetWindowId(), windowRect, (GUI.WindowFunction)OnWindow, "YT Chat" });
                    if (ret is Rect r) windowRect = r;
                    usedCTB = true;
                }
            }
        }
        catch (Exception)
        {
            // ignore i fallback
        }

        if (!usedCTB)
        {
            // Fallback: zwykłe GUILayout.Window (klikowalne)
            windowRect = GUILayout.Window(GetWindowId(), windowRect, OnWindow, "YT Chat");
        }

        // Przywróć kolor
        GUI.color = prevColor;
    }

    // Unikalne id okna (może być stałe)
    private int GetWindowId() => "YTChatWindow".GetHashCode();

    // Metoda rysująca zawartość okna
    private void OnWindow(int id)
    {
        // Opcje nagłówka (opacity, ramka, text-only)
        GUILayout.BeginHorizontal();
        GUILayout.Label("Opacity:");
        opacity = GUILayout.HorizontalSlider(opacity, 0.1f, 1f);
        showBorder = GUILayout.Toggle(showBorder, "Border");
        textOnly = GUILayout.Toggle(textOnly, "Text only");
        if (GUILayout.Button("Close")) Visible = false;
        GUILayout.EndHorizontal();

        // Rysuj ramkę tła jeśli nie jesteśmy w textOnly i border włączony
        if (!textOnly && showBorder)
        {
            // Narysuj box jako tło - użyj GUI.Box z obrysem poprzez GUI.skin.box
            // Obszar wewnętrzny dla treści obliczamy tak, by nie nachodzić na kontrolki nagłówka
            // Tutaj po prostu rysujemy box obejmujący całe okno
            GUI.Box(new Rect(0, 20, windowRect.width, windowRect.height - 20), GUIContent.none);
        }

        // Lista wiadomości z przewijaniem
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(windowRect.width - 10), GUILayout.Height(windowRect.height - 60));

        // Wyświetl wiadomości w odwrotnej kolejności (najnowsze u góry)
        for (int i = messages.Count - 1; i >= 0; i--)
        {
            var m = messages[i];

            // Kolor nicku
            Color nickColor = ColorForNick(m.Nick ?? "");

            GUILayout.BeginHorizontal();

            // Nick
            GUIStyle nickStyle = new GUIStyle(GUI.skin.label);
            nickStyle.normal.textColor = nickColor;
            nickStyle.wordWrap = false;
            GUILayout.Label(m.Nick + ":", nickStyle, GUILayout.Width(120));

            // Treść wiadomości - obsługa flash jeśli nowa
            bool isFlashing = false;
            if (!string.IsNullOrEmpty(m.Id) && flashStart.TryGetValue(m.Id, out float startT))
            {
                float elapsed = Time.realtimeSinceStartup - startT;
                float totalFlashDuration = 3f; // 3 sekundy = 3 migania po 0.5s
                if (elapsed < totalFlashDuration)
                {
                    int phase = (int)Mathf.Floor(elapsed / 0.5f);
                    isFlashing = (phase % 2) == 0; // na przemian
                }
            }

            GUIStyle contentStyle = new GUIStyle(messageStyle);
            if (isFlashing)
            {
                contentStyle.normal.textColor = Color.red; // miganie na czerwono
            }

            GUILayout.Label(m.Text, contentStyle);

            GUILayout.EndHorizontal();
            GUILayout.Space(6);
        }

        GUILayout.EndScrollView();

        // Drag okna
        GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));
    }

    // Pobierz wiadomości z ServerClient (przez reflection) i zmapuj do ChatMessage
    private void FetchMessagesIfNeeded()
    {
        try
        {
            var scType = Type.GetType("ServerClient");
            IEnumerable raw = null;
            if (scType != null)
            {
                // Preferowana metoda: public static IEnumerable GetMessages()
                var gm = scType.GetMethod("GetMessages", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (gm != null)
                {
                    var ret = gm.Invoke(null, null);
                    raw = ret as IEnumerable;
                }
                else
                {
                    // Alternatywa: statyczna property Messages
                    var prop = scType.GetProperty("Messages", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (prop != null)
                    {
                        var ret = prop.GetValue(null, null);
                        raw = ret as IEnumerable;
                    }
                }
            }

            if (raw == null) return;

            // Mapowanie surowych obiektów do ChatMessage
            var latest = new List<ChatMessage>();
            foreach (var item in raw)
            {
                if (item == null) continue;

                string nick = TryGetString(item, new[] { "Nick", "nick", "User", "user", "Author", "author", "Username", "username" });
                string text = TryGetString(item, new[] { "Text", "text", "Message", "message", "Content", "content", "Body", "body" });
                string id = TryGetString(item, new[] { "Id", "id", "MessageId", "messageId" });
                DateTime timestamp = TryGetDateTime(item, new[] { "Time", "time", "Timestamp", "timestamp", "Created", "created" });

                if (string.IsNullOrEmpty(text)) continue;

                latest.Add(new ChatMessage { Id = id ?? Guid.NewGuid().ToString(), Nick = nick ?? "?", Text = text, Timestamp = timestamp });
            }

            // Jeśli nie ma zmian, nic nie rób
            // Porównaj po Id
            bool hasNew = false;
            foreach (var m in latest)
            {
                if (!seenMessageIds.Contains(m.Id))
                {
                    hasNew = true;
                    seenMessageIds.Add(m.Id);
                    flashStart[m.Id] = Time.realtimeSinceStartup; // rozpocznij miganie
                    lastActivityTime = Time.realtimeSinceStartup;
                }
            }

            if (hasNew || messages.Count == 0)
            {
                // Zaktualizuj wewnętrzną listę (ostatnie N wiadomości)
                messages.Clear();
                // Zachowaj tylko ostatnie messageLimit elementów
                int start = Math.Max(0, latest.Count - messageLimit);
                for (int i = start; i < latest.Count; i++) messages.Add(latest[i]);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("[ChatWindow] FetchMessages failed: " + ex.Message);
        }
    }

    // Pomoc: próbuj pobrać string z właściwości/pola obiektu
    private static string TryGetString(object src, string[] names)
    {
        var t = src.GetType();
        foreach (var n in names)
        {
            var p = t.GetProperty(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (p != null)
            {
                var v = p.GetValue(src, null);
                if (v != null) return v.ToString();
            }
            var f = t.GetField(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null)
            {
                var v = f.GetValue(src);
                if (v != null) return v.ToString();
            }
        }
        return null;
    }

    // Pomoc: próbuj pobrać DateTime z właściwości/fieldu
    private static DateTime TryGetDateTime(object src, string[] names)
    {
        var s = TryGetString(src, names);
        if (string.IsNullOrEmpty(s)) return DateTime.MinValue;
        if (DateTime.TryParse(s, out var dt)) return dt;
        // Spróbuj jeśli wartość była liczba unix
        if (long.TryParse(s, out var l))
        {
            try
            {
                // traktuj jako unix seconds
                DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(l);
                return dto.DateTime;
            }
            catch { }
        }
        return DateTime.MinValue;
    }

    // Konwersja nicku na kolor (hash -> odcień)
    private static Color ColorForNick(string nick)
    {
        if (string.IsNullOrEmpty(nick)) return Color.white;
        int h = nick.GetHashCode();
        uint uh = (uint)h;
        float hue = (uh % 360) / 360f;
        float sat = 0.6f;
        float val = 0.9f;
        return Color.HSVToRGB(hue, sat, val);
    }

    // Załaduj konfigurację z typu Config (przez reflection) jeśli dostępny
    private void LoadConfig()
    {
        try
        {
            var cfgType = Type.GetType("Config");
            if (cfgType == null) return;

            // Próbuj pobrać pola/properties
            var ml = cfgType.GetField("MessageLimit", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (ml != null && ml.GetValue(null) is int mi) messageLimit = mi;

            var ah = cfgType.GetField("AutoHideSeconds", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (ah != null && ah.GetValue(null) is float af) autoHideSeconds = af;
            else
            {
                var ahp = cfgType.GetField("AutoHide", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (ahp != null && ahp.GetValue(null) is float af2) autoHideSeconds = af2;
            }

            var op = cfgType.GetField("Opacity", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (op != null && op.GetValue(null) is float opf) opacity = opf;

            var br = cfgType.GetField("ShowBorder", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (br != null && br.GetValue(null) is bool brb) showBorder = brb;

            var to = cfgType.GetField("TextOnly", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (to != null && to.GetValue(null) is bool tob) textOnly = tob;
        }
        catch (Exception ex)
        {
            Debug.Log("[ChatWindow] LoadConfig failed: " + ex.Message);
        }
    }

    // Prosty model wiadomości używany wewnętrznie
    private class ChatMessage
    {
        public string Id;
        public string Nick;
        public string Text;
        public DateTime Timestamp;
    }
}
