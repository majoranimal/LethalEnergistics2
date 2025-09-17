using HarmonyLib;
using LethalEnergistics2.Scripts;
using Unity.Netcode;
using UnityEngine;

namespace LethalEnergistics2.Patches
{
    [HarmonyPatch]
    public class NetworkObjectManager
    {
        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), "Start")]
        private static void Init()
        {
            if (networkPrefab != null)
            {
                return;
            }

            networkPrefab = (GameObject)LethalEnergistics2.ImportedAssets.LoadAsset("MainNetworkHandler");
            networkPrefab.AddComponent<MainNetworkHandler>();

            chamberPrefab = (GameObject)LethalEnergistics2.ImportedAssets.LoadAsset("Chamber"); // Load the GameObject "Chamber" from the embedded AssetBundle and save it to the variable "IOChamberPrefab"
            chamberPrefab.AddComponent<ChamberScript>();

            NetworkManager.Singleton.AddNetworkPrefab(chamberPrefab);
            NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), "Awake")]
        private static void SpawnNetworkHandler()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                var networkHandlerHost = Object.Instantiate(networkPrefab, Vector3.zero, Quaternion.identity);
                networkHandlerHost.GetComponent<NetworkObject>().Spawn();
            }
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private static GameObject networkPrefab;
        public static GameObject chamberPrefab;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }
}
