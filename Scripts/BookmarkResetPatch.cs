using HarmonyLib;

[HarmonyPatch(typeof(MapGO), "Awake")]
internal static class BookmarkResetPatch
{
    private static void Postfix()
    {
        CameraBookmarkHotkeysPatch.ResetBookmarks();
    }
}
