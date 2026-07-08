using System;
using UnityEngine;

// ChatMessage - reprezentuje pojedynczą wiadomość czatu
// Pola: Nick, Text, NickColor, IsNew
// Konstruktor ustawia NickColor na podstawie hasha nicku
public class ChatMessage
{
    // Nick autora
    public string Nick;

    // Treść wiadomości
    public string Text;

    // Kolor przypisany do nicku
    public Color NickColor;

    // Flaga czy wiadomość jest nowa (może służyć do migania)
    public bool IsNew;

    // Konstruktor
    public ChatMessage(string nick, string text, bool isNew = false)
    {
        Nick = nick ?? string.Empty;
        Text = text ?? string.Empty;
        IsNew = isNew;
        NickColor = ColorForNick(Nick);
    }

    // Konwersja nicku na kolor (hash -> hue)
    private static Color ColorForNick(string nick)
    {
        if (string.IsNullOrEmpty(nick)) return Color.white;
        int h = nick.GetHashCode();
        uint uh = (uint)h;
        float hue = (uh % 360) / 360f;
        float sat = 0.6f;
        float val = 0.95f;
        return Color.HSVToRGB(hue, sat, val);
    }
}
