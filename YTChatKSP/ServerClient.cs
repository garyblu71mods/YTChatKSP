using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// ServerClient - klient HTTP używający UnityWebRequest (kompatybilny z Unity 2019.2.2f1 i KSP)
// GET http://localhost:5000/messages -> pobiera listę wiadomości (JSON)
// POST http://localhost:5000/send -> wysyła wiadomość (JSON)
public static class ServerClient
{
    private static string logPath = @"C:\Users\grzeg\YTChatKSP_Debug.log";

    private static void LogToFile(string message)
    {
        try
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            string logEntry = $"[{timestamp}] [ServerClient] {message}";
            System.IO.File.AppendAllText(logPath, logEntry + System.Environment.NewLine);
        }
        catch { }
    }

    // DTO wiadomości zgodne z JSON-em
    public class MessageDto
    {
        public string Id;
        public string Nick;
        public string Text;
        public string Timestamp;
    }

    // Cache wiadomości zwracany synchronously przez GetMessages()
    private static List<MessageDto> cachedMessages = new List<MessageDto>();

    // Flaga czy fetch jest w toku
    private static bool isFetching = false;

    // Runner do uruchamiania coroutine
    private static ServerClientRunner runner;

    // Publiczna metoda zwracająca aktualne wiadomości (szybkie, zwraca cache)
    // Można ją wywołać często; aktualizacje wykonywane asynchronicznie jako coroutine
    public static IEnumerable GetMessages()
    {
        EnsureRunner();

        if (!isFetching)
        {
            isFetching = true;
            LogToFile("Triggering FetchMessagesCoroutine");
            runner.StartCoroutine(FetchMessagesCoroutine());
        }

        return cachedMessages;
    }

    // Wymuszony fetch (asynchroniczny)
    public static void ForceFetch()
    {
        EnsureRunner();
        if (!isFetching)
        {
            isFetching = true;
            runner.StartCoroutine(FetchMessagesCoroutine());
        }
    }

    // Wyślij wiadomość (uruchamia coroutine)
    public static void SendMessage(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        EnsureRunner();
        runner.StartCoroutine(PostMessageCoroutine(text));
    }

    // Upewnij się, że runner istnieje
    private static void EnsureRunner()
    {
        if (runner != null) return;
        var go = new GameObject("ServerClientRunner");
        UnityEngine.Object.DontDestroyOnLoad(go);
        runner = go.AddComponent<ServerClientRunner>();
    }

    // Coroutine GET /messages
    private static IEnumerator FetchMessagesCoroutine()
    {
        string url = "http://localhost:5000/messages";
        LogToFile($"Starting fetch from {url}");

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.timeout = 1; // Timeout po 1 sekundzie - szybko fail jeśli serwer nie dostępny
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            try
            {
                // Unity 2019 uses isNetworkError / isHttpError
                if (www.isNetworkError || www.isHttpError)
                {
                    LogToFile($"GET messages error: {www.error} (code: {www.responseCode})");
                }
                else
                {
                    string json = www.downloadHandler.text;
                    LogToFile($"Raw JSON received: {json}");

                    var parsed = ParseMessagesFromJson(json);
                    if (parsed != null && parsed.Count > 0)
                    {
                        lock (cachedMessages)
                        {
                            cachedMessages.Clear();
                            cachedMessages.AddRange(parsed);
                            LogToFile($"Successfully parsed {parsed.Count} messages");
                        }
                    }
                    else
                    {
                        LogToFile($"ParseMessagesFromJson returned null or empty list");
                    }
                }
            }
            catch (Exception ex)
            {
                LogToFile($"Exception in FetchMessagesCoroutine: {ex.Message} | {ex.StackTrace}");
            }
            finally
            {
                isFetching = false;
            }
        }
    }

    // Coroutine POST /send
    private static IEnumerator PostMessageCoroutine(string text)
    {
        string url = "http://localhost:5000/send";
        LogToFile($"Sending message: {text}");
        byte[] bodyRaw = Encoding.UTF8.GetBytes("{\"text\":\"" + EscapeJsonString(text) + "\"}");

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.timeout = 5; // Timeout po 5 sekundach
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.uploadHandler.contentType = "application/json";
            www.downloadHandler = new DownloadHandlerBuffer();

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                LogToFile($"POST send error: {www.error}");
            }
            else
            {
                LogToFile($"Message sent successfully");
            }
        }
    }

    // Prosty parser JSON dla formatu z backendu
    private static List<MessageDto> ParseMessagesFromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            LogToFile("ParseMessagesFromJson: json is null or empty");
            return new List<MessageDto>();
        }

        try
        {
            var list = new List<MessageDto>();

            // Sprawdź czy to tablica JSON
            json = json.Trim();
            if (!json.StartsWith("[") || !json.EndsWith("]"))
            {
                LogToFile($"ParseMessagesFromJson: json doesn't look like array: {json.Substring(0, Math.Min(100, json.Length))}");
                return new List<MessageDto>();
            }

            // Znajdź wszystkie obiekty
            var objMatches = Regex.Matches(json, "\\{([^}]*)\\}");
            LogToFile($"ParseMessagesFromJson: found {objMatches.Count} objects");

            foreach (Match m in objMatches)
            {
                var body = m.Groups[1].Value;

                // Parsuj każde pole (obsługuj zarówno "id" jak i "Id")
                string id = ExtractJsonString(body, "id") ?? ExtractJsonString(body, "Id") ?? "";
                string nick = ExtractJsonString(body, "nick") ?? ExtractJsonString(body, "Nick") ?? ExtractJsonString(body, "author") ?? "";
                string text = ExtractJsonString(body, "text") ?? ExtractJsonString(body, "Text") ?? ExtractJsonString(body, "message") ?? "";

                // DEBUG: sprawdź co jest w body
                if (string.IsNullOrEmpty(nick) && !string.IsNullOrEmpty(text))
                {
                    LogToFile($"DEBUG: Extracting nick from text: '{text}'");
                    int colonIndex = text.IndexOf(':');
                    if (colonIndex > 0 && colonIndex < text.Length - 1)
                    {
                        nick = text.Substring(0, colonIndex).Trim();
                        text = text.Substring(colonIndex + 1).Trim();
                        LogToFile($"DEBUG: Extracted nick='{nick}' text='{text}'");
                    }
                }

                // Fallback na "Unknown" jeśli nick pusty
                if (string.IsNullOrEmpty(nick))
                    nick = "Unknown";
                string timestamp = ExtractJsonString(body, "timestamp") ?? ExtractJsonString(body, "Timestamp") ?? "";

                if (string.IsNullOrEmpty(text))
                {
                    LogToFile($"ParseMessagesFromJson: skipping message with empty text");
                    continue;
                }

                var msg = new MessageDto
                {
                    Id = id,
                    Nick = nick,
                    Text = text,
                    Timestamp = timestamp
                };

                list.Add(msg);
                LogToFile($"Parsed: nick='{nick}' text='{text}'");
            }

            LogToFile($"ParseMessagesFromJson: total {list.Count} messages added to list");
            return list;
        }
        catch (Exception ex)
        {
            LogToFile($"ParseMessagesFromJson exception: {ex.Message}");
            return new List<MessageDto>();
        }
    }

    private static string ExtractJsonString(string src, string key)
    {
        var pattern = "\"" + Regex.Escape(key) + "\"\\s*:\\s*\"([^\"]*)\"";
        var m = Regex.Match(src, pattern);
        if (m.Success) return UnescapeJsonString(m.Groups[1].Value);
        return null;
    }

    private static string EscapeJsonString(string s)
    {
        if (s == null) return string.Empty;
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }

    private static string UnescapeJsonString(string s)
    {
        if (s == null) return null;
        return s.Replace("\\\"", "\"").Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\\\", "\\");
    }

    // Mały MonoBehaviour do uruchamiania coroutine
    private class ServerClientRunner : MonoBehaviour { }
}
