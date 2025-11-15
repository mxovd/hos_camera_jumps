using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[HarmonyPatch(typeof(CameraGO), "Update")]
internal static class CameraBookmarkHotkeysPatch
{
    private const int SlotCount = 10;

    private static readonly CameraBookmark[] Slots = new CameraBookmark[SlotCount];
    private static int _cycleIndex = -1;

    private struct CameraBookmark
    {
        public Vector3 Position;
        public float Zoom;
        public bool IsSet;
    }

    private static readonly KeyCode[] AlphaNumberKeys =
    {
        KeyCode.Alpha0,
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9
    };

    private static readonly KeyCode[] KeypadNumberKeys =
    {
        KeyCode.Keypad0,
        KeyCode.Keypad1,
        KeyCode.Keypad2,
        KeyCode.Keypad3,
        KeyCode.Keypad4,
        KeyCode.Keypad5,
        KeyCode.Keypad6,
        KeyCode.Keypad7,
        KeyCode.Keypad8,
        KeyCode.Keypad9
    };

    static void Postfix(CameraGO __instance)
    {
        if (__instance == null || __instance.cam == null)
        {
            return;
        }

        if (!Application.isFocused || UIManager.isUIOpen)
        {
            return;
        }

        if (IsTypingInInputField())
        {
            return;
        }

        HandleBookmarkHotkeys(__instance);
    }

    private static void HandleBookmarkHotkeys(CameraGO cameraGO)
    {
        bool ctrlHeld = IsControlHeld();
        bool altHeld = IsAltHeld();

        if (!ctrlHeld && !altHeld)
        {
            return;
        }

        for (int i = 0; i < SlotCount; i++)
        {
            if (ctrlHeld && IsNumberKeyPressed(i))
            {
                SaveBookmark(i, cameraGO);
                return;
            }

            if (altHeld && IsNumberKeyPressed(i))
            {
                JumpToBookmark(i, cameraGO);
                return;
            }
        }

        if (altHeld)
        {
            if (UnityInputProxy.GetKeyDown(KeyCode.LeftBracket))
            {
                CycleBookmark(-1, cameraGO);
            }
            else if (UnityInputProxy.GetKeyDown(KeyCode.RightBracket))
            {
                CycleBookmark(1, cameraGO);
            }
        }
    }

    private static bool IsNumberKeyPressed(int index)
    {
        return UnityInputProxy.GetKeyDown(AlphaNumberKeys[index]) || UnityInputProxy.GetKeyDown(KeypadNumberKeys[index]);
    }

    private static bool IsControlHeld()
    {
        return UnityInputProxy.IsCtrlHeld();
    }

    private static bool IsAltHeld()
    {
        return UnityInputProxy.IsAltHeld();
    }

    private static void SaveBookmark(int slot, CameraGO cameraGO)
    {
        Vector3 clampedPosition = new Vector3(
            Mathf.Clamp(cameraGO.transform.position.x, cameraGO.limitXMINUS, cameraGO.limitXPLUS),
            Mathf.Clamp(cameraGO.transform.position.y, cameraGO.limitYMINUS, cameraGO.limitYPLUS),
            cameraGO.transform.position.z
        );

        float clampedZoom = Mathf.Clamp(cameraGO.cam.orthographicSize, cameraGO.minZoom, cameraGO.maxZoom);

        Slots[slot] = new CameraBookmark
        {
            Position = clampedPosition,
            Zoom = clampedZoom,
            IsSet = true
        };

        _cycleIndex = slot;

        PlaySaveSound();
    }

    private static void PlaySaveSound()
    {
        if (SoundManager.instance == null || SoundManager.instance.UI_Source == null)
        {
            return;
        }

        AudioClip clip = SoundManager.GetAttackSound("savebookmark");
        if (clip == null)
        {
            return;
        }

        SoundManager.instance.UI_Source.PlayOneShot(clip);
    }

    private static void PlayLoadSound()
    {
        if (SoundManager.instance == null || SoundManager.instance.UI_Source == null)
        {
            return;
        }

        AudioClip clip = SoundManager.GetAttackSound("loadbookmark");
        if (clip == null)
        {
            return;
        }

        SoundManager.instance.UI_Source.PlayOneShot(clip);
    }


    private static void JumpToBookmark(int slot, CameraGO cameraGO)
    {
        if (!Slots[slot].IsSet)
        {
            return;
        }

        ApplyBookmark(Slots[slot], cameraGO);
        _cycleIndex = slot;
        PlayLoadSound();
    }

    private static void ApplyBookmark(CameraBookmark bookmark, CameraGO cameraGO)
    {
        Vector3 targetPosition = new Vector3(
            Mathf.Clamp(bookmark.Position.x, cameraGO.limitXMINUS, cameraGO.limitXPLUS),
            Mathf.Clamp(bookmark.Position.y, cameraGO.limitYMINUS, cameraGO.limitYPLUS),
            cameraGO.transform.position.z
        );

        float targetZoom = Mathf.Clamp(bookmark.Zoom, cameraGO.minZoom, cameraGO.maxZoom);

        cameraGO.transform.position = targetPosition;
        cameraGO.targetZoom = targetZoom;
        cameraGO.cam.orthographicSize = targetZoom;
    }

    private static void CycleBookmark(int direction, CameraGO cameraGO)
    {
        if (!HasAnyBookmark())
        {
            return;
        }

        int nextIndex = FindNextBookmarkIndex(direction);
        if (nextIndex == -1)
        {
            return;
        }

        JumpToBookmark(nextIndex, cameraGO);
    }

    private static int FindNextBookmarkIndex(int direction)
    {
        if (direction == 0)
        {
            return -1;
        }

        if (_cycleIndex == -1)
        {
            return direction > 0 ? GetFirstBookmarkIndex() : GetLastBookmarkIndex();
        }

        for (int step = 1; step <= SlotCount; step++)
        {
            int candidate = WrapIndex(_cycleIndex + direction * step);
            if (Slots[candidate].IsSet)
            {
                return candidate;
            }
        }

        return -1;
    }

    private static int GetFirstBookmarkIndex()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (Slots[i].IsSet)
            {
                return i;
            }
        }

        return -1;
    }

    private static int GetLastBookmarkIndex()
    {
        for (int i = SlotCount - 1; i >= 0; i--)
        {
            if (Slots[i].IsSet)
            {
                return i;
            }
        }

        return -1;
    }

    private static int WrapIndex(int value)
    {
        int mod = value % SlotCount;
        if (mod < 0)
        {
            mod += SlotCount;
        }

        return mod;
    }

    private static bool HasAnyBookmark()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (Slots[i].IsSet)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsTypingInInputField()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null)
        {
            return false;
        }

        return selected.GetComponent<InputField>() != null || selected.GetComponent<TMP_InputField>() != null;
    }

    public static void ResetBookmarks()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            Slots[i] = default;
        }

        _cycleIndex = -1;
    }
}
