using System;
using HarmonyLib;
using UnityEngine;

internal static class UnityInputProxy
{
    private static readonly Func<KeyCode, bool> GetKeyDownFunc = CreateKeyDelegate("GetKeyDown");
    private static readonly Func<KeyCode, bool> GetKeyFunc = CreateKeyDelegate("GetKey");

    private static Func<KeyCode, bool> CreateKeyDelegate(string methodName)
    {
        Type inputType = AccessTools.TypeByName("UnityEngine.Input");
        if (inputType == null)
        {
            return _ => false;
        }

        var method = AccessTools.Method(inputType, methodName, new[] { typeof(KeyCode) });
        if (method == null)
        {
            return _ => false;
        }

        try
        {
            return (Func<KeyCode, bool>)Delegate.CreateDelegate(typeof(Func<KeyCode, bool>), method);
        }
        catch
        {
            return _ => false;
        }
    }

    public static bool GetKeyDown(KeyCode keyCode)
    {
        return GetKeyDownFunc(keyCode);
    }

    public static bool GetKey(KeyCode keyCode)
    {
        return GetKeyFunc(keyCode);
    }

    public static bool IsCtrlHeld()
    {
        return GetKey(KeyCode.LeftControl) || GetKey(KeyCode.RightControl);
    }

    public static bool IsAltHeld()
    {
        return GetKey(KeyCode.LeftAlt) || GetKey(KeyCode.RightAlt);
    }

    public static bool IsModifierHeld()
    {
        return IsCtrlHeld() || IsAltHeld();
    }
}
