using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace LethalEnergistics2;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class LethalEnergistics2 : BaseUnityPlugin
{
    public static LethalEnergistics2 Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }
    //public static TerminalNode? buyChamberNode { get; set; }
    public static string globalSystemFile = "LCGeneralSaveData.le2";
    public static string gameDatabaseFile = "{0}.le2";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public static AssetBundle ImportedAssets;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private void Awake()
    {
        NetcodePatcher();

        Logger = base.Logger;
        Instance = this;

        Patch();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");

        // Access the AssetBundle stored as an embedded resource
        var assembly = Assembly.GetExecutingAssembly();
        var chamberBundle = "LethalEnergistics2.le2-chamber-bundle";

        Stream stream = assembly.GetManifestResourceStream(chamberBundle);
        ImportedAssets = AssetBundle.LoadFromStream(stream);

        // Check if the AssetBundle loaded, if it failed crash
        if (ImportedAssets == null)
        {
            Logger.LogFatal("Failed to load custom assets."); // ManualLogSource for your plugin
            return;
        }
    }

    private static void NetcodePatcher()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length > 0)
                {
                    method.Invoke(null, null);
                }
            }
        }
    }

    internal static void Patch()
    {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll();

        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch()
    {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }
}
