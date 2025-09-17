using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LethalEnergistics2.Patches
{
    [HarmonyPatch(typeof(Terminal))] // When a script of type Terminal
    public class TerminalCommands
    {
        [HarmonyPatch("Start")] // Is started
        [HarmonyPostfix] // Run the following script once finished
        private static void AddTerminalCommandsPostfix(Terminal __instance)
        {
            LethalEnergistics2.Logger.LogError("Start terminal patch at: " + DateTime.Now.ToString("hh:mm:ss")); // Print text in the terminal indicating the start of this patch

            Terminal terminal = __instance; // Save the current instance of Terminal to the variable "terminal"

            StartOfRound startofround = UnityEngine.Object.FindObjectOfType<StartOfRound>(); // Finds an object of type StartOfRound in the scene and binds it to the varaiable "startofround"

            // Checks if start of round is unset and throws an error if it is
            if (startofround == null)
            {
                LethalEnergistics2.Logger.LogWarning("Start of round unset");
                return;
            }

            int chamberId = 28;

            TerminalKeyword[] keywords = terminal.terminalNodes.allKeywords; // Save the terminals list of TerminalKeywords to the variable "keywords"

            List<TerminalNode> specialNodes = terminal.terminalNodes.specialNodes; // Save the terminals list of TerminalNodes to the variable "specialNodes"

            // Save import keywords to variables
            TerminalKeyword buy = keywords.First(o => o.name == "Buy");
            TerminalKeyword confirm = keywords.First(o => o.name == "Confirm");
            TerminalKeyword deny = keywords.First(o => o.name == "Deny");

            // Creates a TerminalNode that will be displayed when cancelled
            TerminalNode cancelPurchaseNode = (TerminalNode)ScriptableObject.CreateInstance("TerminalNode");
            cancelPurchaseNode.name = "CancelPurchase";
            cancelPurchaseNode.displayText = "Cancelled order.\n";
            cancelPurchaseNode.maxCharactersToType = 35;

            // Creates a TerminalNode that will be displayed after the order is confirmed
            TerminalNode buyChamberConfirmNode = (TerminalNode)ScriptableObject.CreateInstance("TerminalNode");
            buyChamberConfirmNode.name = "ChamberLE2BuyConfirm";
            buyChamberConfirmNode.displayText = "Ordered the LE2 Chamber! Your new balance is\n" +
                "[playerCredits].\n" +
                "Press [B] to rearrange objects in your ship and [v] to\n" +
                "confirm.";
            buyChamberConfirmNode.clearPreviousText = true;
            buyChamberConfirmNode.buyUnlockable = true;
            buyChamberConfirmNode.maxCharactersToType = 35;
            buyChamberConfirmNode.itemCost = 1;
            buyChamberConfirmNode.shipUnlockableID = chamberId;

            // Creates a TerminalKeyword that registers "chamber" as a new word and defaults to "buy" if no command is given
            TerminalKeyword chamberNode = (TerminalKeyword)ScriptableObject.CreateInstance("TerminalKeyword");
            chamberNode.name = "ChamberLE2";
            chamberNode.word = "chamber";
            chamberNode.defaultVerb = buy; //.First(item => item.name == "buy").value;
            chamberNode.isVerb = false;

            // Creates a TerminalNode that will be displayed when requesting to buy the object
            TerminalNode buyChamberNode = (TerminalNode)ScriptableObject.CreateInstance("TerminalNode");
            buyChamberNode.name = "ChamberLE2Buy";
            buyChamberNode.displayText = "You have requested to order an LE2 Chamber.\n" +
            "Total cost of item: [totalCost]\n" +
            "\n" +
            "Please CONFIRM or DENY\n" +
            "\n";
            buyChamberNode.clearPreviousText = true;
            //buyChamberNode.isConfirmationNode = true;
            buyChamberNode.maxCharactersToType = 15;
            buyChamberNode.shipUnlockableID = 28;
            buyChamberNode.itemCost = 1;
            buyChamberNode.creatureName = "ChamberLE2";
            buyChamberNode.overrideOptions = true;
            buyChamberNode.terminalOptions =
                [
                new CompatibleNoun()
                {
                    noun = confirm,
                    result = buyChamberConfirmNode,
                },
                new CompatibleNoun()
                {
                    noun = deny,
                    result = cancelPurchaseNode,
                }
            ];
            /// End chamber unlockable buy




            /// Start chamber control



            /// End chamber control





            keywords = keywords.Append(chamberNode).ToArray(); // Adds the chamberNode to the list of keywords
            buy.compatibleNouns = buy.compatibleNouns.Append(new CompatibleNoun { noun = chamberNode, result = buyChamberNode }).ToArray(); // Adds the chamberNode to the list of buy subcommands

            __instance.terminalNodes.allKeywords = keywords; // Updates the terminal Keywords list with our modifications
            __instance.terminalNodes.allKeywords.First(o => o.name == "Buy").compatibleNouns = keywords.First(o => o.name == "Buy").compatibleNouns; // Updates the list of Buy keywords with our modifications

            startofround.unlockablesList.unlockables.First(o => o.unlockableName == "LE2 IO Chamber").shopSelectionNode = __instance.terminalNodes.allKeywords[0].compatibleNouns.Last().result; // Updates the LE2 Chamber unlockable object to have the shop node attached
            startofround.unlockablesList.unlockables.First(o => o.unlockableName == "LE2 IO Chamber").prefabObject.GetComponentInChildren<PlaceableShipObject>(false).unlockableID = chamberId;

            LethalEnergistics2.Logger.LogError("End terminal patch at: " + DateTime.Now.ToString("hh:mm:ss")); // Print text in the terminal indicating the end of this script
        }
    }
}
