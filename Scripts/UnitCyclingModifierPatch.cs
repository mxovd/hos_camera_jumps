using HarmonyLib;

[HarmonyPatch(typeof(UIManager))]
internal static class UnitCyclingModifierPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIManager.NextUnit))]
    private static bool PreventNextUnitWhenModifierHeld()
    {
        return !UnityInputProxy.IsModifierHeld();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIManager.PrevUnit))]
    private static bool PreventPrevUnitWhenModifierHeld()
    {
        return !UnityInputProxy.IsModifierHeld();
    }
}
