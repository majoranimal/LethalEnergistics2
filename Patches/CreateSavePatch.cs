using HarmonyLib;
using LethalEnergistics2.Scripts;
using System;

namespace LethalEnergistics2.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class CreateSavePatch
    {
        [HarmonyPatch("SaveGame")]
        [HarmonyPostfix]
        private static void SaveSystemData(GameNetworkManager __instance)
        {
            ChamberScript chamber;
            try
            {
                chamber = GameNetworkManager.FindObjectOfType<ChamberScript>(false).GetComponent<ChamberScript>();
            }
            catch (NullReferenceException ex)
            {
                LethalEnergistics2.Logger.LogWarning($"The chamber object could not be found: {ex.Message}");
                return;
            }

            ES3.Save("LifetimeStats", chamber.lifetimeStats, LethalEnergistics2.globalSystemFile);

            if (!__instance.isHostingGame)
            {
                return;
            }

            ES3.Save("ChamberDatabase", chamber.systemDatabase, string.Format(LethalEnergistics2.gameDatabaseFile, __instance.currentSaveFileName));
            ES3.Save("CurrentStats", chamber.currentStats, string.Format(LethalEnergistics2.gameDatabaseFile, __instance.currentSaveFileName));
            ES3.Save("GameStats", chamber.gameStats, string.Format(LethalEnergistics2.gameDatabaseFile, __instance.currentSaveFileName));
        }
    }
}
