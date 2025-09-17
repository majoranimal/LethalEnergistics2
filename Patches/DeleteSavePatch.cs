using HarmonyLib;

namespace LethalEnergistics2.Patches
{
    [HarmonyPatch(typeof(DeleteFileButton))]
    internal class DeleteSavePatch
    {
        [HarmonyPatch("DeleteFile")]
        [HarmonyPostfix]
        private static void ResetNonPersistentSystemData(DeleteFileButton __instance)
        {
            ES3.DeleteFile(string.Format(LethalEnergistics2.gameDatabaseFile, $"LCSaveFile{__instance.fileToDelete + 1}"));
            //ES3.DeleteFile(LethalEnergistics2.gameDatabaseFile, new ES3Settings(ES3.Location.Cache));
        }
    }
}
