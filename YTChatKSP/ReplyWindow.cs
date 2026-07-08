using System;
using System.Reflection;
using UnityEngine;

// ReplyWindow - interaktywne IMGUI okno do wysyłania wiadomości
// Funkcje: pole tekstowe, przycisk Wyślij -> ServerClient.SendMessage(), przycisk Settings -> otwiera SettingsWindow, drag & drop
public class ReplyWindow
{
    public bool Visible { get; set; } = false;

    private Rect windowRect = new Rect(440, 80, 300, 200);
    private string messageText = string.Empty;

    // Metoda rysująca okno (wywoływana z głównego OnGUI)
    public void Draw()
    {
        if (!Visible) return;

        windowRect = GUILayout.Window(GetWindowId(), windowRect, OnWindow, "Reply");
    }

    private int GetWindowId() => "YTReplyWindow".GetHashCode();

    private void OnWindow(int id)
    {
        GUILayout.BeginVertical();

        GUILayout.Label("Write your reply:");
        // Pole tekstowe wieloliniowe
        messageText = GUILayout.TextArea(messageText, GUILayout.Height(100));

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Wyślij"))
        {
            SendMessageToServer(messageText);
            messageText = string.Empty;
            Visible = false; // opcjonalnie zamknij okno po wysłaniu
        }

        if (GUILayout.Button("Settings"))
        {
            OpenSettingsWindow();
        }

        if (GUILayout.Button("Close"))
        {
            Visible = false;
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        // Drag area
        GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));
    }

    // Wyślij wiadomość przez ServerClient.SendMessage() (przez reflection)
    private void SendMessageToServer(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        try
        {
            var scType = Type.GetType("ServerClient");
            if (scType != null)
            {
                // Spróbuj znaleźć metodę SendMessage(string)
                var m = scType.GetMethod("SendMessage", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null);
                if (m != null)
                {
                    m.Invoke(null, new object[] { text });
                    return;
                }

                // Alternatywne sygnatury: Send(string), PostMessage(object)
                m = scType.GetMethod("Send", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null);
                if (m != null) { m.Invoke(null, new object[] { text }); return; }

                m = scType.GetMethod("PostMessage", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (m != null)
                {
                    // jeśli oczekuje obiektu, tworzymy prosty anonimowy obiekt z polem Text
                    var param = m.GetParameters();
                    if (param.Length == 1)
                    {
                        var pType = param[0].ParameterType;
                        try
                        {
                            var obj = Activator.CreateInstance(pType);
                            // ustaw pole/property Text jeśli istnieje
                            var prop = pType.GetProperty("Text", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                            if (prop != null && prop.CanWrite) prop.SetValue(obj, text, null);
                            var field = pType.GetField("Text", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                            if (field != null) field.SetValue(obj, text);
                            m.Invoke(null, new object[] { obj });
                        }
                        catch { }
                    }
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log("[ReplyWindow] SendMessage failed: " + ex.Message);
        }
    }

    // Otwórz SettingsWindow znajdując instancję YTChatKSP i ustawiając pole settingsWindow.Visible = true przez reflection
    private void OpenSettingsWindow()
    {
        try
        {
            var modType = Type.GetType("YTChatKSPMain");
            if (modType == null) return;

            // UnityEngine.Object.FindObjectOfType(Type)
            var findMethod = typeof(UnityEngine.Object).GetMethod("FindObjectOfType", new Type[] { typeof(Type) });
            var modInstance = findMethod.Invoke(null, new object[] { modType });
            if (modInstance == null) return;

            // Znajdź pole settingsWindow (może być private)
            var field = modType.GetField("settingsWindow", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null)
            {
                var settingsObj = field.GetValue(modInstance);
                if (settingsObj != null)
                {
                    var vis = settingsObj.GetType().GetProperty("Visible", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (vis != null && vis.CanWrite)
                    {
                        vis.SetValue(settingsObj, true, null);
                        return;
                    }

                    // fallback: pole Visible
                    var fvis = settingsObj.GetType().GetField("Visible", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (fvis != null)
                    {
                        fvis.SetValue(settingsObj, true);
                        return;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log("[ReplyWindow] OpenSettingsWindow failed: " + ex.Message);
        }
    }
}
