using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LethalEnergistics2.Patches;

[HarmonyPatch(typeof(StartOfRound))] // When a script of type StartOfRound
internal class UnlockablesPatch
{
    [HarmonyPatch("Awake")] // Is awakened
    [HarmonyPrefix] // Run this script before the main script
    private static void AddUnlockablesPrefix(StartOfRound __instance)
    {
        LethalEnergistics2.Logger.LogError("Start unlockable patch at: " + DateTime.Now.ToString("hh:mm:ss")); // Print a message to the terminal indicating the start of this patch

        List<UnlockableItem> unlockables = __instance.unlockablesList.unlockables; // Gets the list of unlockable items from this instance of StartOfRound and saves it to the variable "unlockables"

        GameObject chamberPrefab = NetworkObjectManager.chamberPrefab;

        // Create an UnlockableItem for the IOChamber
        UnlockableItem IOChamber = new()
        {
            unlockableName = "LE2 IO Chamber",
            prefabObject = chamberPrefab,
            unlockableType = 1,
            alwaysInStock = true,
            IsPlaceable = true,
            hasBeenMoved = false,
            placedPosition = new Vector3((float)0, (float)0, (float)0),
            placedRotation = new Vector3((float)0, (float)0, (float)0),
            inStorage = false,
            canBeStored = true,
            maxNumber = 1,
            hasBeenUnlockedByPlayer = false,
            alreadyUnlocked = false,
            unlockedInChallengeFile = false,
            spawnPrefab = true,
        };

        unlockables = unlockables.Append(IOChamber).ToList(); // Adds the IOChamber UnlockableItem to the list of unlockables

        __instance.unlockablesList.unlockables = unlockables; // Updates the instances unlockables list to reflect our updates

        LethalEnergistics2.Logger.LogError("End unlockable patch at: " + DateTime.Now.ToString("hh:mm:ss")); // Prints a message to the terminal indicating the end of this patch
    }
}
