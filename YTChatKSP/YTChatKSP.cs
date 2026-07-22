using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using KSP.UI.Screens;

[KSPAddon(KSPAddon.Startup.Instantly, true)]
public class YTChatKSPMain : MonoBehaviour
{
    private ChatWindow chatWindow;
    private ReplyWindow replyWindow;
    private SettingsWindow settingsWindow;

    private ApplicationLauncherButton appButton;
    private Texture2D iconTexture;

    private bool initialized = false;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (initialized) return;

        try
        {
            var cfgType = Type.GetType("Config");
            if (cfgType != null)
            {
                var loadMethod = cfgType.GetMethod("Load", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                loadMethod?.Invoke(null, null);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("[YTChatKSP] Config.Load() failed: " + ex.Message);
        }

        chatWindow = new ChatWindow();
        replyWindow = new ReplyWindow();
        settingsWindow = new SettingsWindow();

        // Connect Settings button callback
        chatWindow.OnOpenSettings = () =>
        {
            if (settingsWindow != null)
            {
                settingsWindow.Visible = !settingsWindow.Visible;
                Debug.Log("[YTChatKSP] Settings window toggled: " + settingsWindow.Visible);
            }
        };

        // Spróbuj załadować ikonę z pliku
        iconTexture = LoadIconTexture();

        if (!TryInitToolbarControl())
        {
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
        }

        initialized = true;
    }

    private Texture2D LoadIconTexture()
    {
        try
        {
            // Spróbuj załadować z GameData/YTChatKSP/icon.png
            string iconPath = KSPUtil.ApplicationRootPath + "GameData/YTChatKSP/icon.png";

             if (System.IO.File.Exists(iconPath))
            {
                Debug.Log("[YTChatKSP] Icon file exists at: " + iconPath);

                // Use GameDatabase to load texture
                Texture2D tex = GameDatabase.Instance.GetTexture("YTChatKSP/icon", false);

                Debug.Log("[YTChatKSP] Icon loaded successfully");
                return tex;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[YTChatKSP] Exception in LoadIconTexture: " + ex.Message);
        }

        // Zwróć null - KSP będzie używał domyślnej białej ikony
        Debug.Log("[YTChatKSP] No custom icon, using default");
        return null;
    }

    private bool TryInitToolbarControl()
    {
        try
        {
            var tcType = Type.GetType("ToolbarControl.ToolbarControl, ToolbarControl")
                      ?? Type.GetType("ToolbarControl.ToolbarControl");

            if (tcType == null)
                return false;

            var component = this.gameObject.AddComponent(tcType);

            var addMethod = tcType.GetMethod("AddToAllToolbars")
                          ?? tcType.GetMethod("Add");

            if (addMethod != null)
            {
                Action left = () => OnToolbarLeftClick();
                Action right = () => OnToolbarLeftClick(); // Right-click also toggles chat for now

                try
                {
                    addMethod.Invoke(component, new object[] {
                        left, right,
                        ApplicationLauncher.AppScenes.ALWAYS,
                        "YTChatKSP", "YTChatKSP", "YTChatKSP", "YTChatKSP"
                    });
                }
                catch
                {
                    try
                    {
                        addMethod.Invoke(component, new object[] {
                            left, right,
                            iconTexture,
                            "YTChatKSP", "YTChatKSP"
                        });
                    }
                    catch { }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.Log("[YTChatKSP] TryInitToolbarControl failed: " + ex.Message);
            return false;
        }
    }

    private void OnGUIAppLauncherReady()
    {
        if (appButton != null) return;

        appButton = ApplicationLauncher.Instance.AddModApplication(
            OnToolbarLeftClick,
            OnToolbarLeftClick,
            null, null, null, null,
            ApplicationLauncher.AppScenes.ALWAYS,
            iconTexture
        );
    }

    private void OnGUIAppLauncherDestroyed() => appButton = null;

    private void OnToolbarLeftClick()
    {
        Debug.Log("[YTChatKSP] OnToolbarLeftClick called - toggling visibility");
        if (chatWindow == null)
        {
            Debug.LogError("[YTChatKSP] OnToolbarLeftClick: chatWindow is null!");
            return;
        }
        chatWindow.Visible = !chatWindow.Visible;
        Debug.Log("[YTChatKSP] chatWindow.Visible now: " + chatWindow.Visible);
    }

    void Update()
    {
        // GetMessages() jest już wywoływane z ChatWindow.FetchMessagesIfNeeded()
        // Nie trzeba podwajać tutaj
    }

    void OnGUI()
    {
        PerformanceMonitor.Instance.OnFrameBegin();

        // Null safety checks - prevent errors if initialization failed
        if (chatWindow == null || replyWindow == null || settingsWindow == null)
        {
            PerformanceMonitor.Instance.OnFrameEnd();
            return;
        }

        try
        {
            var ctb = Type.GetType("ClickThroughBlocker, Assembly-CSharp")
                   ?? Type.GetType("ClickThroughBlocker");

            if (ctb != null)
            {
                var begin = ctb.GetMethod("Begin", BindingFlags.Static | BindingFlags.Public)
                         ?? ctb.GetMethod("Enable", BindingFlags.Static | BindingFlags.Public);

                var end = ctb.GetMethod("End", BindingFlags.Static | BindingFlags.Public)
                       ?? ctb.GetMethod("Disable", BindingFlags.Static | BindingFlags.Public);

                begin?.Invoke(null, null);

                chatWindow.Draw();
                replyWindow.Draw();
                settingsWindow.Draw();

                end?.Invoke(null, null);

                PerformanceMonitor.Instance.OnFrameEnd();
                return;
            }
        }
        catch { }

        chatWindow.Draw();
        replyWindow.Draw();
        settingsWindow.Draw();

        PerformanceMonitor.Instance.OnFrameEnd();
    }

    void OnDestroy()
    {
        GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
        GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnGUIAppLauncherDestroyed);
    }

    // ---------------- WINDOWS ----------------

    private class ChatWindow
    {
        private const int WINDOW_ID = 10001;
        public bool Visible { get; set; } = false;
        private Rect windowRect = new Rect(10, 80, 420, 300);

        // Callback to open settings window
        public System.Action OnOpenSettings { get; set; }

        // Lokalne kopie ustawie?
        private float currentOpacity = 1f;
        private int currentFontSize = 12;
        private Color currentFontColor = Color.white;
        private bool currentLockPosition = false;
        private float autoHideTimer = 0f;
        private float lastConfigApplyTime = 0f; // Timer for config refresh throttling

        // Lista wiadomości wyświetlanych w oknie
        private List<ChatMessage> displayedMessages = new List<ChatMessage>();
        private List<string> cachedMessageLines = new List<string>();
        private Vector2 scrollPosition = Vector2.zero;
        private int lastMessageCount = 0;
        private bool shouldScrollToBottom = false;

        // Cached reflection for performance
        private Type serverClientType = null;
        private MethodInfo getMessagesMethod = null;
        private bool reflectionCached = false;

        private static string logPath = @"C:\Users\grzeg\YTChatKSP_Debug.log";

        private static void LogToFile(string message)
        {
            try
            {
                string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
                string logEntry = $"[{timestamp}] [ChatWindow] {message}";
                System.IO.File.AppendAllText(logPath, logEntry + System.Environment.NewLine);
            }
            catch { }
        }

        public void Draw()
        {
            // Zawsze sprawdzaj timeout i pobierz wiadomości, nawet gdy niewidoczny
            ApplyConfigSettings();

            if (!Visible) return;

            // Zaktualizuj ustawienia z Config
            // ApplyConfigSettings();  // Już się wykonała wyżej

            // Zapamietaj oryginalny opacity
            Color originalColor = GUI.color;

            // Zastosuj opacity dla ramki okna
            GUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, currentOpacity);

            windowRect = SafeWindow(WINDOW_ID, windowRect, DrawContents, "YT Chat");

            // Przywroc oryginalny kolor
            GUI.color = originalColor;
        }

        private void ApplyConfigSettings()
        {
            // Throttle config refresh to reduce frame overhead - only apply every 1 second for snappier UI response
            float timeSinceLastApply = Time.time - lastConfigApplyTime;
            if (timeSinceLastApply < 1f)
            {
                // Only apply opacity/position/size which are cheap operations
                currentOpacity = Config.Opacity;
                currentLockPosition = Config.LockWindowPosition;
                windowRect.width = Config.WindowWidth;
                windowRect.height = Config.WindowHeight;

                // Check auto-hide timer even if we skip other updates
                if (Config.AutoHide && Config.AutoHideTime > 0 && Visible)
                {
                    autoHideTimer += Time.deltaTime;
                    if (autoHideTimer >= Config.AutoHideTime)
                    {
                        Visible = false;
                        autoHideTimer = 0f;
                    }
                }
                else if (!Config.AutoHide)
                {
                    autoHideTimer = 0f;
                }

                return; // Skip expensive operations (font updates, message fetching)
            }

            // Full config update once per second
            lastConfigApplyTime = Time.time;

            currentOpacity = Config.Opacity;
            currentFontSize = Config.FontSize;
            currentFontColor = Config.FontColor;
            currentLockPosition = Config.LockWindowPosition;

            // Zaktualizuj rozmiar okna z Config
            windowRect.width = Config.WindowWidth;
            windowRect.height = Config.WindowHeight;

            // Auto-hide timer - liczy się tylko gdy AutoHide jest aktywny
            if (Config.AutoHide && Config.AutoHideTime > 0 && Visible)
            {
                // Odmierzaj czas
                autoHideTimer += Time.deltaTime;
                if (autoHideTimer >= Config.AutoHideTime)
                {
                    Visible = false;
                    autoHideTimer = 0f;
                }
            }
            else if (!Config.AutoHide)
            {
                // Auto-hide wyłączony
                autoHideTimer = 0f;
            }

            // Pobierz wiadomości z ServerClient (expensive operation - do this once per second)
            FetchMessages();
        }

        private void FetchMessages()
        {
            try
            {
                // Cache reflection results on first call
                if (!reflectionCached)
                {
                    serverClientType = Type.GetType("ServerClient");
                    if (serverClientType != null)
                    {
                        getMessagesMethod = serverClientType.GetMethod("GetMessages", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    }
                    reflectionCached = true;
                }

                // Use cached values
                if (serverClientType == null || getMessagesMethod == null)
                {
                    LogToFile("ServerClient or GetMessages not found");
                    return;
                }

                var messages = getMessagesMethod.Invoke(null, null) as System.Collections.IEnumerable;
                if (messages == null)
                {
                    LogToFile("GetMessages returned null");
                    return;
                }

                // Konwertuj MessageDto na stringi do wy?wietlenia
                cachedMessageLines.Clear();
                int messageCount = 0;

                foreach (var msg in messages)
                {
                    if (msg == null) continue;

                    var msgType = msg.GetType();
                    var nickField = msgType.GetField("Nick");
                    var textField = msgType.GetField("Text");

                    if (nickField != null && textField != null)
                    {
                        string nick = (string)nickField.GetValue(msg) ?? "Unknown";
                        string text = (string)textField.GetValue(msg) ?? "";

                        // Format: "Nick: Text"
                        string line = nick + ": " + text;
                        cachedMessageLines.Add(line);
                        messageCount++;
                    }
                }

                if (messageCount > 0)
                {
                    LogToFile($"Successfully displayed {messageCount} messages");
                    // Jesli liczba wiadomosci sie zmienila, scroll na dol
                    if (messageCount != lastMessageCount)
                    {
                        shouldScrollToBottom = true;
                        lastMessageCount = messageCount;

                        LogToFile($"New messages! Showing window and resetting auto-hide timer");

                        // Pokaż chat gdy pojawią się nowe wiadomości - ZAWSZE włącz okno
                        Visible = true;

                        // Resetuj timer przy nowej wiadomości - licznik zacznie się od nowa
                        autoHideTimer = 0f;
                    }
                }
                else
                {
                    LogToFile("No messages found in server response");
                }
            }
            catch (System.Exception ex)
            {
                LogToFile($"FetchMessages failed: {ex.Message} | {ex.StackTrace}");
            }
        }

        private void DrawContents(int id)
        {
            // Zapamietaj oryginalny font size i color
            int originalFontSize = GUI.skin.label.fontSize;
            Color originalFontColor = GUI.skin.label.normal.textColor;

            // Zastosuj font size i color z Config
            GUI.skin.label.fontSize = currentFontSize;
            GUI.skin.label.normal.textColor = currentFontColor;

            // Top button bar
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Settings", GUILayout.Width(70)))
            {
                OnOpenSettings?.Invoke();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", GUILayout.Width(60)))
            {
                Visible = false;
                autoHideTimer = 0f;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Scrollable area z wiadomosciami - BEZ widocznego scrollbara
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUIStyle.none, GUIStyle.none);

            // Wyswietl kazda wiadomosc z zawijaniem tekstu
            // Ustaw style do zawijania z wordwrap
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.wordWrap = true;

            foreach (var line in cachedMessageLines)
            {
                GUILayout.Label(line, labelStyle, GUILayout.ExpandWidth(true));
            }

            // Jesli brak wiadomosci
            if (cachedMessageLines.Count == 0)
            {
                GUILayout.Label("Waiting for messages...", labelStyle, GUILayout.ExpandWidth(true));
            }

            GUILayout.EndScrollView();

            // Auto-scroll na dol gdy sie pojawia nowe wiadomosci
            if (shouldScrollToBottom)
            {
                scrollPosition.y = float.MaxValue;
                shouldScrollToBottom = false;
            }

            // Blokada przeciagania - tylko jesli nie jest zablokowana
            if (!currentLockPosition)
            {
                GUI.DragWindow();
            }

            // Przywroc oryginalny font size i color na koniec
            GUI.skin.label.fontSize = originalFontSize;
            GUI.skin.label.normal.textColor = originalFontColor;
        }
    }

    private class ReplyWindow
    {
        private const int WINDOW_ID = 10002;
        public bool Visible { get; set; } = false;
        private Rect windowRect = new Rect(440, 80, 300, 200);

        public void Draw()
        {
            if (!Visible) return;
            windowRect = SafeWindow(WINDOW_ID, windowRect, DrawContents, "Reply");
        }

        private void DrawContents(int id)
        {
            GUILayout.Label("Reply to selected message...");
            if (GUILayout.Button("Close")) Visible = false;
            GUI.DragWindow();
        }
    }

    private class SettingsWindow
    {
        private const int WINDOW_ID = 10003;
        public bool Visible { get; set; } = false;
        private Rect windowRect = new Rect(450, 80, 400, 600);
        private Vector2 scrollPosition = Vector2.zero;

        public void Draw()
        {
            if (!Visible) return;
            windowRect = SafeWindow(WINDOW_ID, windowRect, DrawContents, "Settings");
        }

        private void DrawContents(int id)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(560));

            // Opacity (przezroczysto??)
            GUILayout.Label("Opacity: " + (Config.Opacity * 100).ToString("F0") + "%", GUILayout.Height(20));
            Config.Opacity = GUILayout.HorizontalSlider(Config.Opacity, 0f, 1f, GUILayout.Height(20));

            GUILayout.Space(10);

            // Window Width
            GUILayout.Label("Window Width: " + Config.WindowWidth + " px", GUILayout.Height(20));
            Config.WindowWidth = (int)GUILayout.HorizontalSlider(Config.WindowWidth, 200f, 1200f, GUILayout.Height(20));

            GUILayout.Space(10);

            // Window Height
            GUILayout.Label("Window Height: " + Config.WindowHeight + " px", GUILayout.Height(20));
            Config.WindowHeight = (int)GUILayout.HorizontalSlider(Config.WindowHeight, 100f, 800f, GUILayout.Height(20));

            GUILayout.Space(10);

            // Font Size
            GUILayout.Label("Font Size: " + Config.FontSize + " px", GUILayout.Height(20));
            Config.FontSize = (int)GUILayout.HorizontalSlider(Config.FontSize, 10f, 40f, GUILayout.Height(20));

            GUILayout.Space(10);

            // Font Color (RGB)
            GUILayout.Label("Font Color (RGB):", GUILayout.Height(20));
            GUILayout.Label("Red: " + Config.FontColor.r.ToString("F2"), GUILayout.Height(20));
            Config.FontColor = new Color(
                GUILayout.HorizontalSlider(Config.FontColor.r, 0f, 1f, GUILayout.Height(20)),
                Config.FontColor.g,
                Config.FontColor.b,
                Config.FontColor.a
            );

            GUILayout.Label("Green: " + Config.FontColor.g.ToString("F2"), GUILayout.Height(20));
            Config.FontColor = new Color(
                Config.FontColor.r,
                GUILayout.HorizontalSlider(Config.FontColor.g, 0f, 1f, GUILayout.Height(20)),
                Config.FontColor.b,
                Config.FontColor.a
            );

            GUILayout.Label("Blue: " + Config.FontColor.b.ToString("F2"), GUILayout.Height(20));
            Config.FontColor = new Color(
                Config.FontColor.r,
                Config.FontColor.g,
                GUILayout.HorizontalSlider(Config.FontColor.b, 0f, 1f, GUILayout.Height(20)),
                Config.FontColor.a
            );

            GUILayout.Space(10);

            // Auto-hide timeout (1s to 10 min)
            float timeoutSeconds = Config.AutoHideTime;
            string timeoutDisplay;
            if (timeoutSeconds < 60f)
            {
                timeoutDisplay = timeoutSeconds.ToString("F1") + " sec";
            }
            else
            {
                float minutes = timeoutSeconds / 60f;
                timeoutDisplay = minutes.ToString("F1") + " min";
            }

            GUILayout.Label("Auto-hide Timeout: " + timeoutDisplay, GUILayout.Height(20));
            Config.AutoHideTime = GUILayout.HorizontalSlider(Config.AutoHideTime, 1f, 600f, GUILayout.Height(20));

            GUILayout.Space(10);

            // Auto-hide ON/OFF
            GUILayout.BeginHorizontal();
            GUILayout.Label("Enable Auto-hide:", GUILayout.Width(150));
            Config.AutoHide = GUILayout.Toggle(Config.AutoHide, "", GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Refresh Interval
            GUILayout.Label("Refresh Interval: " + Config.RefreshInterval.ToString("F1") + " sec", GUILayout.Height(20));
            Config.RefreshInterval = GUILayout.HorizontalSlider(Config.RefreshInterval, 0.5f, 10f, GUILayout.Height(20));

            GUILayout.Space(10);

            // Lock window position
            GUILayout.BeginHorizontal();
            GUILayout.Label("Lock Window Position:", GUILayout.Width(150));
            Config.LockWindowPosition = GUILayout.Toggle(Config.LockWindowPosition, "", GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.EndScrollView();

            GUILayout.Space(10);

            // Buttons at bottom
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save & Close", GUILayout.Height(30)))
            {
                Config.Save();
                Visible = false;
            }
            if (GUILayout.Button("Reset Defaults", GUILayout.Height(30)))
            {
                ResetToDefaults();
            }
            if (GUILayout.Button("Close", GUILayout.Height(30)))
            {
                Visible = false;
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        private void ResetToDefaults()
        {
            Config.Opacity = 1f;
            Config.FontSize = 12;
            Config.FontColor = Color.white;
            Config.WindowWidth = 420;
            Config.WindowHeight = 300;
            Config.AutoHide = false;
            Config.AutoHideTime = 10f;
            Config.RefreshInterval = 2f;
            Config.LockWindowPosition = false;
            Config.Save();
        }
    }

    private static Rect SafeWindow(int id, Rect rect, GUI.WindowFunction func, string title)
    {
        try
        {
            return GUILayout.Window(id, rect, func, title);
        }
        catch
        {
            return GUI.Window(id, rect, func, title);
        }
    }
}
