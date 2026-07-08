using System;
using System.Reflection;
using UnityEngine;
using KSP.UI.Screens;

// ToolbarControlWrapper - pomocnik integracji z ToolbarControl (jeśli zainstalowany)
// Używa reflection aby nie wymagać obecności biblioteki w kompilacji.
public class ToolbarControlWrapper
{
    private Type toolbarType;
    private Component toolbarComponent;
    private GameObject hostObject;

    // Czy ToolbarControl jest dostępny
    public bool Available => toolbarType != null;

    // Konstruktor próbuje zainicjalizować ToolbarControl (jeśli obecny)
    public ToolbarControlWrapper()
    {
        Init();
    }

    private void Init()
    {
        try
        {
            // Typ najczęściej nazywa się ToolbarControl.ToolbarControl w assembly ToolbarControl
            toolbarType = Type.GetType("ToolbarControl.ToolbarControl, ToolbarControl") ?? Type.GetType("ToolbarControl.ToolbarControl");
            if (toolbarType == null) return;

            // Stwórz obiekt-host w scenie, aby dodać komponent ToolbarControl
            hostObject = new GameObject("YTChatKSP_ToolbarControlHost");
            UnityEngine.Object.DontDestroyOnLoad(hostObject);
            toolbarComponent = hostObject.AddComponent(toolbarType) as Component;
        }
        catch (Exception ex)
        {
            Debug.Log("[ToolbarControlWrapper] Init failed: " + ex.Message);
            toolbarType = null;
        }
    }

    // Dodaj przycisk do ToolbarControl. Zwraca obiekt reprezentujący przycisk lub null.
    // leftAction - wywoływane przy LPM, rightAction - przy PPM (jeśli ToolbarControl to wspiera)
    public object AddButton(string modId, string name, Texture2D icon, Action leftAction, Action rightAction)
    {
        if (!Available || toolbarComponent == null) return null;

        try
        {
            // Szukaj możliwych metod dodawania przycisku
            MethodInfo addToAll = toolbarType.GetMethod("AddToAllToolbars");
            MethodInfo add = toolbarType.GetMethod("Add");

            // Przygotuj delegaty
            Delegate leftDel = leftAction != null ? Delegate.CreateDelegate(typeof(Action), leftAction.Target, leftAction.Method) : null;
            Delegate rightDel = rightAction != null ? Delegate.CreateDelegate(typeof(Action), rightAction.Target, rightAction.Method) : null;

            object result = null;

            if (addToAll != null)
            {
                // Typowa sygnatura: AddToAllToolbars(Action, Action, ApplicationLauncher.AppScenes, string, string, string, string)
                try
                {
                    result = addToAll.Invoke(toolbarComponent, new object[] { leftDel, rightDel, ApplicationLauncher.AppScenes.ALWAYS, modId, name, modId, name });
                    return result;
                }
                catch { /* ignore and try other overloads */ }
            }

            if (add != null)
            {
                // Spróbuj kilku możliwych sygnatur dla Add
                try
                {
                    // Sygnatura: Add(Action, Action, Texture2D, string, string)
                    result = add.Invoke(toolbarComponent, new object[] { leftDel, rightDel, icon, modId, name });
                    return result;
                }
                catch { }

                try
                {
                    // Sygnatura: Add(Action, Action, ApplicationLauncher.AppScenes, Texture2D, string, string)
                    result = add.Invoke(toolbarComponent, new object[] { leftDel, rightDel, ApplicationLauncher.AppScenes.ALWAYS, icon, modId, name });
                    return result;
                }
                catch { }

                try
                {
                    // Sygnatura: Add(Action, Action, string)
                    result = add.Invoke(toolbarComponent, new object[] { leftDel, rightDel, name });
                    return result;
                }
                catch { }
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.Log("[ToolbarControlWrapper] AddButton failed: " + ex.Message);
            return null;
        }
    }

    // Usuń host i wszystkie przyciski (jeśli istnieją)
    public void Dispose()
    {
        try
        {
            if (hostObject != null)
            {
                UnityEngine.Object.Destroy(hostObject);
                hostObject = null;
                toolbarComponent = null;
                toolbarType = null;
            }
        }
        catch (Exception ex)
        {
            Debug.Log("[ToolbarControlWrapper] Dispose failed: " + ex.Message);
        }
    }
}
