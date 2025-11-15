using HarmonyLib;

class CameraJumpsMod : GameModification
{
    private Harmony _harmony;

    public CameraJumpsMod(Mod mod) : base(mod)
    {
        Log("Registering Camera Jumps...");
    }

    public override void OnModInitialization(Mod mod)
    {
        Log("Initializing Camera Jumps...");

        PatchGame();
    }

    public override void OnModUnloaded()
    {
        Log("Unloading Camera Jumps...");

        _harmony?.UnpatchAll(_harmony.Id);
    }

    private void PatchGame()
    {
        Log("Patching...");

        _harmony = new Harmony("com.hexofsteel.camera-jumps");
        _harmony.PatchAll();
    }
}
