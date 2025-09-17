using GameNetcodeStuff;
using LethalEnergistics2.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace LethalEnergistics2.Scripts
{
    internal class ChamberScript : NetworkBehaviour
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private Transform itemImportLocation;
        private Transform itemExportLocation;

        private GameObject popupContainer;
        private TMP_InputField valueInput;

        private int importCooldown = 0;
        private bool monitorInUse = false;
        private bool usingMonitor = false;
        private ulong userOnMonitor = 0;

        private LayerMask itemLayer;

        private float itemImportRadius;

        private GameObject shipHangar;

        private TMP_Text monitorHeader;
        private TMP_Text monitorBody;
        private TMP_Text monitorFooter;

        private InteractTrigger monitorTrigger;
        private InteractTrigger buttonTrigger;

        private List<Item> allItemsList;

        private Terminal terminal;

        private RoundManager roundManager;

        private StartOfRound startOfRound;

        private TimeOfDay timeOfDay;

        public SystemDatabase systemDatabase;
        public SystemStats currentStats;
        public SystemStats gameStats;
        public SystemStats lifetimeStats;
        private MonitorSettings monitorSettings;

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        private Dictionary<string, MonitorPage> monitorMenus = new()
        {
            { "Disabled", new MonitorPage()
            {
                pageName = "Disabled",
                header = "",
                body = "",
                footer = "",
                defaultSettings = new()
                {
                    currentPage = "Disabled"
                },
                enabledBindings = new()
            }},
            { "MainMenu", new MonitorPage()
            {
                pageName = "MainMenu",
                header = "                 Welcome to Lethal Energistics 2\r\n" +
                "------------------------------------------------------------------------",
                body =                 "\r\n\r\n\r\n\r\n\r\n" +
                "          Select a module:\r\n" +
                "          - [{0}] Show list of items\r\n" +
                "          - [{1}] Show item summaries\r\n" +
                "          - [{2}] Show system statistics\r\n\r\n\r\n" +
                "          - [{3}] Show list of tools \r\n" +
                "          - [{4}] Collect Dropship items (Not implemented yet)",
                footer = "------------------------------------------------------------------------\r\n" +
                "Previous Item [W]   |  Next Item [S]   |  Select [Enter]   |  Exit [Esc]\r\n",
                defaultSettings = new MonitorSettings()
                {
                    currentPage = "MainMenu",
                    hoveredOption = 0,
                },
            }},
            {"ListTools", new MonitorPage()
            {
                pageName = "ListTools",
                header = "Tool Name                        Weight                      Two-Handed\r\n" +
                "------------------------------------------------------------------------",
                body = "[{0}] {1} {2} {3} ({4})\r\n",
                footer = "------------------------------------------------------------------------\r\n" +
                "Previous Tool  [W] |  Select Item            [Enter]  | Main Menu  [Esc]\r\n" +
                "Next Tool      [S] |  Export Selected Tools      [E]  |\r\n",
                defaultSettings = new MonitorSettings()
                {
                    currentPage = "ListTools",
                    hoveredOption = 0,
                    displayStart = 0,
                    displayEnd = 18,
                    sortedTools = new(),
                    selectedTools = new()
                }
            }},
            {"ListItems", new MonitorPage()
            {
                pageName = "ListItems",
                header = "Item Name[{0}]           Value[{1}]          Weight[{2}]         Two-Handed\r\n" +
                "------------------------------------------------------------------------",
                body = "[{0}] {1} {2} {3} ({4})\r\n",
                footer = "------------------------------------------------------------------------\r\n" +
                "Previous Item          [W] |  Select Item    [Enter]  | Sort Mode    [Z]\r\n" +
                "Next Item              [S] |  Bulk Selection     [B]  | Main Menu  [Esc]\r\n" +
                "Export Selected Items  [E] |  ⮡ incl. Overtime   [V]  | Sell Items   [G]\r\n",
                defaultSettings = new MonitorSettings()
                {
                    currentPage = "ListItems",
                    hoveredOption = 0,
                    displayStart = 0,
                    displayEnd = 18,
                    itemsSortOrder = 0,
                    sortedItems = new(),
                    selectedItems = new()
                }
            }},
            {"ListSummaries", new MonitorPage()
            {
                pageName = "ListSummaries",
                header = "                     Number of    Total       Total\r\n" +
                "Item Name[{0}]         Items [{1}]    Value[{2}]    Weight[{3}]    Two-Handed\r\n" +
                "------------------------------------------------------------------------",
                body = "[{0}] {1} {2} {3} {4} ({5})\r\n",
                footer = "------------------------------------------------------------------------\r\n" +
                "Previous Item          [W]  |  Sort Mode    [Z]  |  Select Item  [Enter]\r\n" +
                "Next Item              [S]  |  Main Menu  [Esc]  |  Sell Items   [G]\r\n" +
                "Export Selected Items  [E]",
                defaultSettings = new MonitorSettings()
                {
                    currentPage = "ListSummaries",
                    hoveredOption = 0,
                    displayStart = 0,
                    displayEnd = 18,
                    summariesSortOrder = 0,
                    sortedSummaries = new(),
                    selectedSummaries = new()
                }
            }},
            {"SystemStatistics", new MonitorPage()
            {
                pageName = "SystemStatistics",
                header = "                           System Statistics\r\n" +
                "------------------------------------------------------------------------",
                body = "\r\n" +
                "\r\n" +
                "        OS:                                 Lethal Energistics 2\r\n" +
                "        OS Version: {0}\r\n" +
                "\r\n" +
                "        Current number of stored items: {1}\r\n" +
                "        Current weight of stored items: {2} pounds\r\n" +
                "        Current worth of stored items: {3} credits\r\n" +
                "\r\n" +
                "        Total number of stored items: {4}\r\n" +
                "        Total weight of stored items: {5} pounds\r\n" +
                "        Total worth of stored items: {6} credits\r\n" +
                "\r\n" +
                "        Lifetime number of stored items: {7}\r\n" +
                "        Lifetime weight of stored items: {8} pounds\r\n" +
                "        Lifetime worth of stored items: {9} credits\r\n" +
                "",
                footer = "------------------------------------------------------------------------\r\n" +
                "Copy Statistics   [Ctrl+C]                              Main Menu  [Esc]",
                defaultSettings = new MonitorSettings()
                {
                    currentPage = "SystemStatistics",
                }
            }},
        };

        // Start is called before the first frame update
        private void Start()
        {
            LethalEnergistics2.Logger.LogInfo("Chamber has started");

            itemImportLocation = GetComponentsInChildren<Transform>().First(o => o.name == "ItemImportLocation");
            itemExportLocation = GetComponentsInChildren<Transform>().First(o => o.name == "ItemExportLocation");
            monitorHeader = GetComponentsInChildren<TMP_Text>().First(o => o.transform.name == "Header");
            monitorBody = GetComponentsInChildren<TMP_Text>().First(o => o.transform.name == "Body");
            monitorFooter = GetComponentsInChildren<TMP_Text>().First(o => o.transform.name == "Footer");
            buttonTrigger = GetComponentsInChildren<InteractTrigger>().First(o => o.name == "Button");
            monitorTrigger = GetComponentsInChildren<InteractTrigger>().First(o => o.name == "Monitor");
            popupContainer = GetComponentsInChildren<Transform>().First(o => o.name == "Popup").gameObject;
            valueInput = popupContainer.GetComponentInChildren<TMP_InputField>();
            shipHangar = GameObject.Find("HangarShip");
            itemImportRadius = 0.9f;
            monitorSettings = new();
            itemLayer = LayerMask.GetMask("Props");
            startOfRound = FindObjectOfType<StartOfRound>();
            allItemsList = startOfRound.allItemsList.itemsList;
            terminal = FindObjectOfType<Terminal>();
            roundManager = FindObjectOfType<RoundManager>();
            timeOfDay = FindObjectOfType<TimeOfDay>();
            popupContainer.SetActive(false);

            systemDatabase = (SystemDatabase)ES3.Load("ChamberDatabase", string.Format(LethalEnergistics2.gameDatabaseFile, GameNetworkManager.Instance.currentSaveFileName), defaultValue: new SystemDatabase());
            currentStats = (SystemStats)ES3.Load("CurrentStats", LethalEnergistics2.globalSystemFile, defaultValue: new SystemStats(clientOnly: false));
            gameStats = (SystemStats)ES3.Load("GameStats", LethalEnergistics2.globalSystemFile, defaultValue: new SystemStats(clientOnly: false));
            lifetimeStats = (SystemStats)ES3.Load("LifetimeStats", LethalEnergistics2.globalSystemFile, defaultValue: new SystemStats(clientOnly: true));

            //Sprite[] icons = FindObjectsOfType<Sprite>();
            Sprite pointIcon = FindObjectsOfType<InteractTrigger>(false).First(a => a.name == "TerminalScript").hoverIcon;

            buttonTrigger.hoverIcon = pointIcon;
            monitorTrigger.hoverIcon = pointIcon;

            buttonTrigger.onInteract.AddListener(new UnityAction<PlayerControllerB>(PressImportButtonOnClient));
            monitorTrigger.onInteractEarly.AddListener(new UnityAction<PlayerControllerB>(ActivateMonitorOnClient));
        }


        [ClientRpc]
        private void ModifyCurrentStatsQuantityClientRpc(int modifier)
        {
            if ((currentStats.clientOnly && modifier > 0) || (!currentStats.clientOnly))
            {
                currentStats.quantity += modifier;
            }
        }

        [ClientRpc]
        private void ModifyCurrentStatsWeightClientRpc(float modifier)
        {
            if ((currentStats.clientOnly && modifier > 0) || (!currentStats.clientOnly))
            {
                currentStats.weight += modifier;
            }
        }

        [ClientRpc]
        private void ModifyCurrentStatsValueClientRpc(int modifier)
        {
            if ((currentStats.clientOnly && modifier > 0) || (!currentStats.clientOnly))
            {
                currentStats.value += modifier;
            }
        }

        [ClientRpc]
        private void ModifyGameStatsQuantityClientRpc(int modifier)
        {
            if ((gameStats.clientOnly && modifier > 0) || (!gameStats.clientOnly))
            {
                gameStats.quantity += modifier;
            }
        }

        [ClientRpc]
        private void ModifyGameStatsWeightClientRpc(float modifier)
        {
            if ((gameStats.clientOnly && modifier > 0) || (!gameStats.clientOnly))
            {
                gameStats.weight += modifier;
            }
        }

        [ClientRpc]
        private void ModifyGameStatsValueClientRpc(int modifier)
        {
            if ((gameStats.clientOnly && modifier > 0) || (!gameStats.clientOnly))
            {
                gameStats.value += modifier;
            }
        }

        [ClientRpc]
        private void ModifyLifetimeStatsQuantityClientRpc(int modifier)
        {
            if ((lifetimeStats.clientOnly && modifier > 0) || (!lifetimeStats.clientOnly))
            {
                lifetimeStats.quantity += modifier;
            }
        }

        [ClientRpc]
        private void ModifyLifetimeStatsWeightClientRpc(float modifier)
        {
            if ((lifetimeStats.clientOnly && modifier > 0) || (!lifetimeStats.clientOnly))
            {
                lifetimeStats.weight += modifier;
            }
        }

        [ClientRpc]
        private void ModifyLifetimeStatsValueClientRpc(int modifier)
        {
            if ((lifetimeStats.clientOnly && modifier > 0) || (!lifetimeStats.clientOnly))
            {
                lifetimeStats.value += modifier;
            }
        }

        /// Modify allItems dict
        [ClientRpc]
        private void AddItemsToSystemDatabaseClientRpc(StoredItem[] items)
        {
            foreach (StoredItem item in items)
            {
                systemDatabase.allItems.Add(item.id, item);
            }
        }

        [ClientRpc]
        private void AddItemToSystemDatabaseClientRpc(StoredItem item)
        {
            systemDatabase.allItems.Add(item.id, item);
        }

        [ClientRpc]
        private void RemoveItemsFromSystemDatabaseClientRpc(int[] itemIds)
        {
            foreach (int itemId in itemIds)
            {
                systemDatabase.allItems.Remove(itemId);
            }
        }

        [ClientRpc]
        private void RemoveItemFromSystemDatabaseClientRpc(int itemId)
        {
            systemDatabase.allItems.Remove(itemId);
        }

        [ClientRpc]
        private void AddToolsToSystemDatabaseClientRpc(StoredItem[] tools)
        {
            foreach (StoredItem tool in tools)
            {
                systemDatabase.allTools.Add(tool.id, tool);
            }
        }

        [ClientRpc]
        private void AddToolToSystemDatabaseClientRpc(StoredItem tool)
        {
            systemDatabase.allTools.Add(tool.id, tool);
        }

        [ClientRpc]
        private void RemoveToolsFromSystemDatabaseClientRpc(int[] toolIds)
        {
            foreach (int toolId in toolIds)
            {
                systemDatabase.allTools.Remove(toolId);
            }
        }

        [ClientRpc]
        private void RemoveToolFromSystemDatabaseClientRpc(int toolId)
        {
            systemDatabase.allTools.Remove(toolId);
        }


        /// Modify itemSummaries dict
        [ClientRpc]
        private void AddSummariesToSystemDatabaseClientRpc(ItemSummary[] summaries)
        {
            foreach (ItemSummary summary in summaries)
            {
                systemDatabase.itemSummaries.Add(summary.name, summary);
            }
        }

        [ClientRpc]
        private void AddSummaryToSystemDatabaseClientRpc(ItemSummary summary)
        {
            systemDatabase.itemSummaries.Add(summary.name, summary);
        }

        [ClientRpc]
        private void RemoveSummariesFromSystemDatabaseClientRpc(NetworkString[] summaryNames)
        {
            foreach (string summaryName in summaryNames)
            {
                systemDatabase.itemSummaries.Remove(summaryName);
            }
        }

        [ClientRpc]
        private void RemoveSummaryFromSystemDatabaseClientRpc(string itemName)
        {
            systemDatabase.itemSummaries.Remove(itemName);
        }

        [ClientRpc]
        private void ModifySummaryValuesClientRpc(string itemName, int value, int quantity, float weight)
        {
            systemDatabase.itemSummaries[itemName].totalValue += value;
            systemDatabase.itemSummaries[itemName].totalQuantity += quantity;
            systemDatabase.itemSummaries[itemName].totalWeight += weight;
        }

        private void ModifySystemStats(int quantity, float weight, int value, bool newItem = true)
        {
            ModifyCurrentStatsQuantityClientRpc(quantity);
            ModifyCurrentStatsWeightClientRpc(weight);
            ModifyCurrentStatsValueClientRpc(value);

            if (quantity > 0 && newItem)
            {
                ModifyGameStatsQuantityClientRpc(quantity);
                ModifyLifetimeStatsQuantityClientRpc(quantity);
            }

            if (weight > 0 && newItem)
            {
                ModifyGameStatsWeightClientRpc(weight);
                ModifyLifetimeStatsWeightClientRpc(weight);
            }

            if (value > 0 && newItem)
            {
                ModifyGameStatsValueClientRpc(value);
                ModifyLifetimeStatsValueClientRpc(value);
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (importCooldown > 0)
            {
                importCooldown -= 1;
            }
        }

        private void SetupMonitorActions()
        {
            LethalEnergistics2.Logger.LogDebug("Setting up monitor actions");
            try
            {
                InputActionAsset keybinds = IngamePlayerSettings.Instance.playerInput.actions;
                keybinds.FindAction("OpenMenu").performed += ExitPerformed;
                keybinds.FindAction("SubmitChat").performed += ConfirmPerformed;
                keybinds.FindAction("Move").performed += NavigatePerformed;
                keybinds.FindAction("Interact").performed += ExportPerformed;
                keybinds.FindAction("Discard").performed += SellPerformed;
                keybinds.FindAction("InspectItem").performed += SortPerformed;
                keybinds.FindAction("SetFreeCamera").performed += CopyPerformed;
                keybinds.FindAction("BuildMode").performed += BulkPerformed;
                keybinds.FindAction("ConfirmBuildMode").performed += OvertimePerformed;
                GameNetworkManager.Instance.localPlayerController.playerActions.Movement.Disable();
            }
            catch (Exception ex)
            {
                LethalEnergistics2.Logger.LogError($"Loading keybinds failed: {ex.Message}");
            }
        }

        private void RemoveMonitorActions()
        {
            LethalEnergistics2.Logger.LogError("Removing monitor actions");
            try
            {
                InputActionAsset keybinds = IngamePlayerSettings.Instance.playerInput.actions;
                keybinds.FindAction("OpenMenu").performed -= ExitPerformed;
                keybinds.FindAction("SubmitChat").performed -= ConfirmPerformed;
                keybinds.FindAction("Move").performed -= NavigatePerformed;
                keybinds.FindAction("Interact").performed -= ExportPerformed;
                keybinds.FindAction("Discard").performed -= SellPerformed;
                keybinds.FindAction("InspectItem").performed -= SortPerformed;
                keybinds.FindAction("SetFreeCamera").performed -= CopyPerformed;
                keybinds.FindAction("BuildMode").performed -= BulkPerformed;
                keybinds.FindAction("ConfirmBuildMode").performed -= OvertimePerformed;
                GameNetworkManager.Instance.localPlayerController.playerActions.Movement.Enable();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unloading keybinds failed: {ex.Message}");
            }
        }

        // Key: Escape
        private void ExitPerformed(InputAction.CallbackContext context)
        {
            switch (monitorSettings.currentPage)
            {
                case "MainMenu":
                    DeactivateMonitorOnServerRpc();
                    SetMonitorPage("Disabled");
                    break;
                case "ListTools":
                    SetMonitorPage("MainMenu");
                    return;
                case "ListItems":
                    SetMonitorPage("MainMenu");
                    return;
                case "ListSummaries":
                    SetMonitorPage("MainMenu");
                    return;
                case "SystemStatistics":
                    SetMonitorPage("MainMenu");
                    return;
            }
        }

        // Key: Enter
        private void ConfirmPerformed(InputAction.CallbackContext context)
        {
            int itemId;
            string itemName;
            int toolId;

            switch (monitorSettings.currentPage)
            {
                case "MainMenu":
                    switch (monitorSettings.hoveredOption)
                    {
                        case 0:
                            SetMonitorPage("ListItems");
                            break;
                        case 1:
                            SetMonitorPage("ListSummaries");
                            break;
                        case 2:
                            SetMonitorPage("SystemStatistics");
                            break;
                        case 3:
                            SetMonitorPage("ListTools");
                            break;
                        case 4:
                            //RequestMonitorPageServerRpc("");
                            break;
                    }
                    break;
                case "ListItems":
                    try
                    {
                        itemId = monitorSettings.sortedItems[monitorSettings.hoveredOption];
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        LethalEnergistics2.Logger.LogWarning($"The hovered item doesn't exist: {ex.Message}");
                        return;
                    }

                    if (monitorSettings.selectedItems.Contains(itemId))
                    {
                        monitorSettings.selectedItems.Remove(itemId);
                    }
                    else
                    {
                        monitorSettings.selectedItems.Add(itemId);
                    }
                    break;
                case "ListTools":
                    try
                    {
                        toolId = monitorSettings.sortedTools[monitorSettings.hoveredOption];
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        LethalEnergistics2.Logger.LogWarning($"The hovered tool doesn't exist: {ex.Message}");
                        return;
                    }

                    if (monitorSettings.selectedTools.Contains(toolId))
                    {
                        monitorSettings.selectedTools.Remove(toolId);
                    }
                    else
                    {
                        monitorSettings.selectedTools.Add(toolId);
                    }
                    break;
                case "ListSummaries":
                    try
                    {
                        itemName = monitorSettings.sortedSummaries[monitorSettings.hoveredOption];
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        LethalEnergistics2.Logger.LogWarning($"The hovered summary doesn't exist: {ex.Message}");
                        return;
                    }

                    if (monitorSettings.selectedSummaries.Contains(itemName))
                    {
                        monitorSettings.selectedSummaries.Remove(itemName);
                    }
                    else
                    {
                        monitorSettings.selectedSummaries.Add(itemName);
                    }
                    break;
            }
        }

        // Keys: W,A,S,D,upArrow,downArrow,leftArrow,rightArrow
        private void NavigatePerformed(InputAction.CallbackContext context)
        {
            switch (context.control.name)
            {
                case "w":
                case "upArrow": //case "up":
                    switch (monitorSettings.currentPage)
                    {
                        case "MainMenu":
                            if (monitorSettings.hoveredOption > 0)
                            {
                                monitorSettings.hoveredOption -= 1;
                                SetMonitorPage("MainMenu");
                            }
                            break;
                        case "ListItems":
                            if (monitorSettings.hoveredOption > 0)
                            {
                                monitorSettings.hoveredOption -= 1;

                                if ((monitorSettings.hoveredOption < monitorSettings.displayStart + 3) && (0 < monitorSettings.displayStart))
                                {
                                    monitorSettings.displayStart -= 1;
                                    monitorSettings.displayEnd -= 1;
                                }

                                SetMonitorPage("ListItems");
                            }
                            break;
                        case "ListTools":
                            if (monitorSettings.hoveredOption > 0)
                            {
                                monitorSettings.hoveredOption -= 1;

                                if ((monitorSettings.hoveredOption < monitorSettings.displayStart + 3) && (0 < monitorSettings.displayStart))
                                {
                                    monitorSettings.displayStart -= 1;
                                    monitorSettings.displayEnd -= 1;
                                }

                                SetMonitorPage("ListTools");
                            }
                            break;
                        case "ListSummaries":
                            if (monitorSettings.hoveredOption > 0)
                            {
                                monitorSettings.hoveredOption -= 1;

                                if ((monitorSettings.hoveredOption < monitorSettings.displayStart + 3) && (0 < monitorSettings.displayStart))
                                {
                                    monitorSettings.displayStart -= 1;
                                    monitorSettings.displayEnd -= 1;
                                }

                                SetMonitorPage("ListSummaries");
                            }
                            break;
                    }
                    return;
                case "a":
                case "leftArrow":
                    switch (monitorSettings.currentPage)
                    {
                        case "MainMenu":
                            break;
                    }
                    return;
                case "s":
                case "downArrow":
                    switch (monitorSettings.currentPage)
                    {
                        case "MainMenu":
                            if (monitorSettings.hoveredOption < 4 - 1)
                            {
                                monitorSettings.hoveredOption += 1;
                                SetMonitorPage("MainMenu");
                            }
                            break;
                        case "ListItems":
                            if (monitorSettings.hoveredOption < monitorSettings.sortedItems.Count - 1)
                            {
                                monitorSettings.hoveredOption += 1;

                                if ((monitorSettings.hoveredOption > monitorSettings.displayEnd - 3) && (monitorSettings.sortedItems.Count > monitorSettings.displayEnd))
                                {
                                    monitorSettings.displayStart += 1;
                                    monitorSettings.displayEnd += 1;
                                }

                                SetMonitorPage("ListItems");
                            }
                            break;
                        case "ListTools":
                            if (monitorSettings.hoveredOption < monitorSettings.sortedTools.Count - 1)
                            {
                                monitorSettings.hoveredOption += 1;

                                if ((monitorSettings.hoveredOption > monitorSettings.displayEnd - 3) && (monitorSettings.sortedTools.Count > monitorSettings.displayEnd))
                                {
                                    monitorSettings.displayStart += 1;
                                    monitorSettings.displayEnd += 1;
                                }

                                SetMonitorPage("ListTools");
                            }
                            break;
                        case "ListSummaries":
                            if (monitorSettings.hoveredOption < monitorSettings.sortedSummaries.Count - 1)
                            {
                                monitorSettings.hoveredOption += 1;

                                if ((monitorSettings.hoveredOption > monitorSettings.displayEnd - 3) && (monitorSettings.sortedSummaries.Count > monitorSettings.displayEnd))
                                {
                                    monitorSettings.displayStart += 1;
                                    monitorSettings.displayEnd += 1;
                                }

                                SetMonitorPage("ListSummaries");
                            }
                            break;
                    }
                    return;
                case "d":
                case "rightArrow":
                    switch (monitorSettings.currentPage)
                    {
                        case "MainMenu":
                            break;
                    }
                    return;
            }
        }

        // Key: E
        private void ExportPerformed(InputAction.CallbackContext context)
        {
            List<int> returningItems;
            int itemId;
            int toolId;

            switch (monitorSettings.currentPage)
            {
                case "MainMenu":
                    break;
                case "ListItems":
                    if (monitorSettings.selectedItems.Count > 0)
                    {
                        ReturnItemsOnServerRpc(monitorSettings.selectedItems.ToArray());
                        foreach (int item in monitorSettings.selectedItems)
                        {
                            monitorSettings.sortedItems.Remove(item);
                        }

                        monitorSettings.selectedItems.Clear();
                    }
                    else
                    {

                        try
                        {
                            itemId = monitorSettings.sortedItems[monitorSettings.hoveredOption];
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            LethalEnergistics2.Logger.LogWarning($"Couldn't export hovered item: {ex}");

                            return;
                        }

                        ReturnItemsOnServerRpc(new int[] { itemId });
                        monitorSettings.sortedItems.Remove(itemId);
                    }

                    if (monitorSettings.hoveredOption > monitorSettings.sortedItems.Count - 1)
                    {
                        monitorSettings.hoveredOption = monitorSettings.sortedItems.Count - 1;
                    }

                    if (monitorSettings.displayEnd >= monitorSettings.sortedItems.Count)
                    {
                        monitorSettings.displayStart -= monitorSettings.displayEnd - (monitorSettings.sortedItems.Count);
                        monitorSettings.displayEnd -= monitorSettings.displayEnd - (monitorSettings.sortedItems.Count);
                    }

                    SetMonitorPage("ListItems");
                    break;
                case "ListTools":
                    if (monitorSettings.selectedTools.Count > 0)
                    {
                        ReturnToolsOnServerRpc(monitorSettings.selectedTools.ToArray());
                        foreach (int tool in monitorSettings.selectedTools)
                        {
                            monitorSettings.sortedTools.Remove(tool);
                        }

                        monitorSettings.selectedTools.Clear();
                    }
                    else
                    {

                        try
                        {
                            toolId = monitorSettings.sortedTools[monitorSettings.hoveredOption];
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            LethalEnergistics2.Logger.LogWarning($"Couldn't export hovered tool: {ex}");

                            return;
                        }

                        ReturnToolsOnServerRpc(new int[] { toolId });
                        monitorSettings.sortedTools.Remove(toolId);
                    }

                    if (monitorSettings.hoveredOption > monitorSettings.sortedTools.Count - 1)
                    {
                        monitorSettings.hoveredOption = monitorSettings.sortedTools.Count - 1;
                    }

                    if (monitorSettings.displayEnd >= monitorSettings.sortedTools.Count)
                    {
                        monitorSettings.displayStart -= monitorSettings.displayEnd - (monitorSettings.sortedTools.Count);
                        monitorSettings.displayEnd -= monitorSettings.displayEnd - (monitorSettings.sortedTools.Count);
                    }

                    SetMonitorPage("ListTools");
                    break;
                case "ListSummaries":
                    if (monitorSettings.selectedSummaries.Count > 0)
                    {
                        returningItems = new();
                        foreach (string summaryName in monitorSettings.selectedSummaries)
                        {
                            returningItems.AddRange(systemDatabase.allItems.Where(item => item.Value.name == summaryName).Select(item => item.Key).ToList());
                            monitorSettings.sortedSummaries.Remove(summaryName);
                        }

                        monitorSettings.selectedSummaries.Clear();
                        ReturnItemsOnServerRpc(returningItems.ToArray());
                    }
                    else
                    {
                        returningItems = systemDatabase.allItems.Where(item => item.Value.name == monitorSettings.sortedSummaries[monitorSettings.hoveredOption]).Select(item => item.Key).ToList();
                        ReturnItemsOnServerRpc(returningItems.ToArray());
                    }

                    SetMonitorPage("ListSummaries");
                    break;
            }
        }

        // Key: G
        private void SellPerformed(InputAction.CallbackContext context)
        {
            List<int> returningItems;
            int itemId;


            switch (monitorSettings.currentPage)
            {
                case "MainMenu":
                    break;
                case "ListItems":
                    if ((roundManager.currentLevel.name != "CompanyBuildingLevel") || (startOfRound.shipHasLanded == false))
                    {
                        LethalEnergistics2.Logger.LogInfo("Ship is not on Gordion: Cancelling sell request");
                        return;
                    }

                    if (monitorSettings.selectedItems.Count > 0)
                    {
                        SellItemsOnServerRpc(monitorSettings.selectedItems.ToArray());
                        foreach (int item in monitorSettings.selectedItems)
                        {
                            monitorSettings.sortedItems.Remove(item);
                        }
                        monitorSettings.selectedItems.Clear();
                    }
                    else
                    {

                        try
                        {
                            itemId = monitorSettings.sortedItems[monitorSettings.hoveredOption];
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            LethalEnergistics2.Logger.LogWarning($"Couldn't export hovered item: {ex}");
                            return;
                        }

                        SellItemsOnServerRpc(new int[] { itemId });
                        monitorSettings.sortedItems.Remove(itemId);
                    }

                    if (monitorSettings.hoveredOption > monitorSettings.sortedItems.Count - 1)
                    {
                        monitorSettings.hoveredOption = monitorSettings.sortedItems.Count - 1;
                    }

                    if (monitorSettings.displayEnd >= monitorSettings.sortedItems.Count)
                    {
                        monitorSettings.displayStart -= monitorSettings.displayEnd - (monitorSettings.sortedItems.Count);
                        monitorSettings.displayEnd -= monitorSettings.displayEnd - (monitorSettings.sortedItems.Count);
                    }

                    SetMonitorPage("ListItems");
                    break;
                case "ListSummaries":
                    if ((roundManager.currentLevel.name != "CompanyBuildingLevel") || (startOfRound.shipHasLanded == false))
                    {
                        LethalEnergistics2.Logger.LogWarning("Ship has not landed on the company");
                        return;
                    }

                    if (monitorSettings.selectedSummaries.Count > 0)
                    {
                        returningItems = new();
                        foreach (string summaryName in monitorSettings.selectedSummaries)
                        {
                            returningItems.AddRange(systemDatabase.allItems.Where(item => item.Value.name == summaryName).Select(item => item.Key).ToList());
                            monitorSettings.sortedSummaries.Remove(summaryName);
                        }
                        monitorSettings.selectedSummaries.Clear();
                        SellItemsOnServerRpc(returningItems.ToArray());
                    }
                    else
                    {
                        SellItemsOnServerRpc(systemDatabase.allItems.Where(item => item.Value.name == monitorSettings.sortedSummaries[monitorSettings.hoveredOption]).Select(item => item.Key).ToArray());
                    }

                    SetMonitorPage("ListSummaries");
                    break;
            }
        }

        // Key: Z
        private void SortPerformed(InputAction.CallbackContext context)
        {
            switch (monitorSettings.currentPage)
            {
                case "MainMenu":
                    break;
                case "ListItems":
                    if (monitorSettings.itemsSortOrder < 2)
                        monitorSettings.itemsSortOrder += 1;
                    else
                    {
                        monitorSettings.itemsSortOrder = 0;
                    }
                    SortItems();
                    SetMonitorPage("ListItems");
                    break;
                case "ListSummaries":
                    if (monitorSettings.summariesSortOrder < 3)
                        monitorSettings.summariesSortOrder += 1;
                    else
                    {
                        monitorSettings.summariesSortOrder = 0;
                    }
                    SortSummaries();
                    SetMonitorPage("ListSummaries");
                    break;
            }
        }

        // Key: C
        private void CopyPerformed(InputAction.CallbackContext context)
        {
            switch (monitorSettings.currentPage)
            {
                case "SystemStatistics":

                    break;
            }
        }

        // Key: B
        private void BulkPerformed(InputAction.CallbackContext context)
        {
            switch (monitorSettings.currentPage)
            {
                case "ListItems":
                    RemoveMonitorActions();
                    popupContainer.SetActive(true);
                    valueInput.Select();
                    valueInput.ActivateInputField();

                    InputActionAsset keybinds = IngamePlayerSettings.Instance.playerInput.actions;
                    keybinds.FindAction("OpenMenu").performed += BulkInputCancelled;
                    keybinds.FindAction("SubmitChat").performed += BulkInputConfirmed;
                    break;
            }
        }

        // Key: B
        private void OvertimePerformed(InputAction.CallbackContext context)
        {
            switch (monitorSettings.currentPage)
            {
                case "ListItems":
                    RemoveMonitorActions();
                    popupContainer.SetActive(true);
                    valueInput.Select();
                    valueInput.ActivateInputField();

                    InputActionAsset keybinds = IngamePlayerSettings.Instance.playerInput.actions;
                    keybinds.FindAction("OpenMenu").performed += OvertimeInputCancelled;
                    keybinds.FindAction("SubmitChat").performed += OvertimeInputConfirmed;
                    break;
            }
        }

        private void BulkInputConfirmed(InputAction.CallbackContext context)
        {
            InputActionAsset keybinds = IngamePlayerSettings.Instance.playerInput.actions;
            keybinds.FindAction("OpenMenu").performed -= BulkInputCancelled;
            keybinds.FindAction("SubmitChat").performed -= BulkInputConfirmed;

            ValueInputConfirmed(false);
        }

        private void OvertimeInputConfirmed(InputAction.CallbackContext context)
        {
            InputActionAsset keybinds = IngamePlayerSettings.Instance.playerInput.actions;
            keybinds.FindAction("OpenMenu").performed -= OvertimeInputCancelled;
            keybinds.FindAction("SubmitChat").performed -= OvertimeInputConfirmed;

            ValueInputConfirmed(true);
        }

        private void ValueInputConfirmed(bool overtime)
        {
            valueInput.DeactivateInputField();
            popupContainer.SetActive(false);

            int targetValue = int.Parse(valueInput.text);

            targetValue = (int)((float)targetValue / startOfRound.companyBuyingRate);

            if (overtime)
            {
                if (targetValue < timeOfDay.profitQuota + 10)
                {
                    targetValue = (5 * targetValue + timeOfDay.profitQuota + (-75 * timeOfDay.daysUntilDeadline)) / 6;
                }
            }

            ValuedCombination bestCombo = new(0, new());

            GetBestCombination(targetValue, ref bestCombo);

            if (bestCombo.totalValue == 0)
            {
                LethalEnergistics2.Logger.LogWarning("No valid combination above the target amount was found.");

                SetupMonitorActions();
            }
            else
            {
                LethalEnergistics2.Logger.LogInfo("Updating selected items.");

                List<int> comboIds = bestCombo.valuedItems.Select(item => item.id).ToList();
                monitorSettings.selectedItems = comboIds;
                SetMonitorPage("ListItems");

                SetupMonitorActions();
            }
        }

        private static IEnumerable<IEnumerable<T>> SubSetsOf<T>(IEnumerable<T> source)
        {
            if (!source.Any())
                return Enumerable.Repeat(Enumerable.Empty<T>(), 1);

            var element = source.Take(1);

            var haveNots = SubSetsOf(source.Skip(1));
            var haves = haveNots.Select(set => element.Concat(set));

            return haves.Concat(haveNots);
        }

        private void GetBestCombination(int targetValue, ref ValuedCombination bestCombo)
        {
            bestCombo = SubSetsOf(systemDatabase.allItems.Values).Select(x => new ValuedCombination { totalValue = x.Sum(y => y.value), valuedItems = x.ToList() }).Where(x => x.totalValue >= targetValue).OrderBy(x => x.totalValue).FirstOrDefault();
        }

        private void BulkInputCancelled(InputAction.CallbackContext context)
        {
            InputActionAsset keybinds = IngamePlayerSettings.Instance.playerInput.actions;
            keybinds.FindAction("OpenMenu").performed -= BulkInputCancelled;
            keybinds.FindAction("SubmitChat").performed -= BulkInputConfirmed;

            valueInput.DeactivateInputField();
            popupContainer.SetActive(false);

            SetupMonitorActions();
        }

        private void OvertimeInputCancelled(InputAction.CallbackContext context)
        {
            InputActionAsset keybinds = IngamePlayerSettings.Instance.playerInput.actions;
            keybinds.FindAction("OpenMenu").performed -= OvertimeInputCancelled;
            keybinds.FindAction("SubmitChat").performed -= OvertimeInputConfirmed;

            valueInput.DeactivateInputField();
            popupContainer.SetActive(false);

            SetupMonitorActions();
        }

        private void ImportItems()
        {
            LethalEnergistics2.Logger.LogInfo("Import attempted");
            if (importCooldown <= 0)
            {
                return;
            }
            Collider[] objects = Physics.OverlapSphere(itemImportLocation.position, itemImportRadius, itemLayer);

            if (objects == null || objects.Length == 0)
            {
                LethalEnergistics2.Logger.LogInfo("No objects detected");
                return;
            }

            foreach (Collider collider in objects)
            {
                GameObject itemObject = collider.gameObject;

                if (itemObject == null)
                {
                    LethalEnergistics2.Logger.LogInfo("Failed to get itemObject");
                    return;
                }

                GrabbableObject item = itemObject.GetComponentInChildren<GrabbableObject>();

                if (item == null)
                {
                    LethalEnergistics2.Logger.LogInfo("Failed to get Item script");
                    return;
                }

                if (item.itemProperties.isScrap)
                {
                    StoredItem newItem;
                    try
                    {
                        if (systemDatabase.allItems.Count == 0)
                        {
                            newItem = new(0, item.itemProperties.name, item.insertedBattery.charge, item.insertedBattery.empty, item.scrapValue, item.itemProperties.itemName);
                        }
                        else
                        {
                            newItem = new(systemDatabase.allItems[systemDatabase.allItems.Keys.Max()].id + 1, item.itemProperties.name, item.insertedBattery.charge, item.insertedBattery.empty, item.scrapValue, item.itemProperties.itemName);
                        }

                        Destroy(itemObject);

                        AddItemToSystemDatabaseClientRpc(newItem);

                        if (systemDatabase.itemSummaries.ContainsKey(newItem.name))
                        {
                            ModifySummaryValuesClientRpc(newItem.name, newItem.value, 1, systemDatabase.itemSummaries[newItem.name].weight);
                        }
                        else
                        {
                            AddSummaryToSystemDatabaseClientRpc(new ItemSummary(newItem.name, item.itemProperties.weight, item.itemProperties.twoHanded, newItem.value, item.itemProperties.weight, 1));
                        }

                        ModifySystemStats(1, item.itemProperties.weight, item.scrapValue);
                    }
                    catch (NullReferenceException ex)
                    {
                        LethalEnergistics2.Logger.LogError($"Item could not be imported: {ex.ToString()}");
                        return;
                    }
                }
                else
                {
                    StoredItem newTool;
                    try
                    {
                        if (systemDatabase.allTools.Count == 0)
                        {
                            newTool = new(0, item.itemProperties.name, item.insertedBattery.charge, item.insertedBattery.empty, item.scrapValue, item.itemProperties.itemName);
                        }
                        else
                        {
                            newTool = new(systemDatabase.allTools[systemDatabase.allTools.Keys.Max()].id + 1, item.itemProperties.name, item.insertedBattery.charge, item.insertedBattery.empty, item.scrapValue, item.itemProperties.itemName);
                        }

                        Destroy(itemObject);

                        AddToolToSystemDatabaseClientRpc(newTool);

                        if (systemDatabase.itemSummaries.ContainsKey(newTool.name))
                        {
                            ModifySummaryValuesClientRpc(newTool.name, newTool.value, 1, systemDatabase.itemSummaries[newTool.name].weight);
                        }
                        else
                        {
                            AddSummaryToSystemDatabaseClientRpc(new ItemSummary(newTool.name, item.itemProperties.weight, item.itemProperties.twoHanded, newTool.value, item.itemProperties.weight, 1));
                        }

                        ModifySystemStats(1, item.itemProperties.weight, item.scrapValue);
                    }
                    catch (NullReferenceException ex)
                    {
                        LethalEnergistics2.Logger.LogError($"Tool could not be imported: {ex.ToString()}");
                        return;
                    }
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ReturnItemsOnServerRpc(int[] items, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;

            if ((clientId != userOnMonitor) || !monitorInUse)
            {
                LethalEnergistics2.Logger.LogWarning("User '" + clientId + "' attempted to return items while not using the monitor");
                return;
            }

            LethalEnergistics2.Logger.LogInfo("Return attempted");
            if (items == null || items.Count() <= 0)
            {
                LethalEnergistics2.Logger.LogWarning($"No items in provided list");
                return;
            }

            LethalEnergistics2.Logger.LogWarning($"Items length: {items.Count()}");

            List<int> returnedItems = new();

            foreach (int itemId in items)
            {
                LethalEnergistics2.Logger.LogWarning($"Returned: {itemId}");
                StoredItem currentItem = systemDatabase.allItems[itemId];
                ItemSummary itemSummary = systemDatabase.itemSummaries[currentItem.name];

                try
                {
                    SpawnItem(currentItem);
                    returnedItems.Add(itemId);
                    ModifySummaryValuesClientRpc(currentItem.name, -currentItem.value, -1, -itemSummary.weight);
                    ModifySystemStats(-1, -itemSummary.weight, -currentItem.value);
                }
                catch (Exception ex)
                {
                    LethalEnergistics2.Logger.LogError($"Failed to return item: {ex.ToString()}");
                }
            }

            RemoveItemsFromSystemDatabaseClientRpc(returnedItems.ToArray());
        }

        [ServerRpc(RequireOwnership = false)]
        private void ReturnToolsOnServerRpc(int[] tools, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;

            if ((clientId != userOnMonitor) || !monitorInUse)
            {
                LethalEnergistics2.Logger.LogWarning("User '" + clientId + "' attempted to return tools while not using the monitor");
                return;
            }

            LethalEnergistics2.Logger.LogInfo("Return attempted");
            if (tools == null || tools.Count() <= 0)
            {
                LethalEnergistics2.Logger.LogWarning($"No tools in provided list");
                return;
            }

            LethalEnergistics2.Logger.LogWarning($"Tools length: {tools.Count()}");

            List<int> returnedTools = new();

            foreach (int toolId in tools)
            {
                LethalEnergistics2.Logger.LogWarning($"Returned: {toolId}");
                StoredItem currentTool = systemDatabase.allTools[toolId];
                ItemSummary itemSummary = systemDatabase.itemSummaries[currentTool.name];

                try
                {
                    SpawnItem(currentTool);
                    returnedTools.Add(toolId);
                    ModifySummaryValuesClientRpc(currentTool.name, -currentTool.value, -1, -itemSummary.weight);
                    ModifySystemStats(-1, -itemSummary.weight, -currentTool.value);
                }
                catch (Exception ex)
                {
                    LethalEnergistics2.Logger.LogError($"Failed to return tool: {ex.ToString()}");
                }
            }

            RemoveToolsFromSystemDatabaseClientRpc(returnedTools.ToArray());
        }

        private void SellAllItems()
        {
            if (systemDatabase.allItems == null || systemDatabase.allItems.Count <= 0)
            {
                LethalEnergistics2.Logger.LogWarning("No items found in the system");
                return;
            }
            List<int> allItemIDs = systemDatabase.allItems.Keys.ToList();
            SellItemsOnServerRpc(allItemIDs.ToArray());
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SellItemsOnServerRpc(int[] items, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;

            if ((userOnMonitor != clientId) || !monitorInUse)
            {
                LethalEnergistics2.Logger.LogWarning("User '" + clientId + "' attempted to sell items while not using the monitor");
                return;
            }

            if ((roundManager.currentLevel.name != "CompanyBuildingLevel") || (startOfRound.shipHasLanded == false))
            {
                LethalEnergistics2.Logger.LogWarning("Ship has not landed on the company");
                return;
            }

            LethalEnergistics2.Logger.LogInfo("Return attempted");
            if (items == null || items.Count() <= 0)
            {
                LethalEnergistics2.Logger.LogWarning($"No items in provided list");
                return;
            }

            LethalEnergistics2.Logger.LogWarning($"Items length: {items.Count()}");

            List<int> soldItems = new();

            foreach (int itemId in items)
            {
                LethalEnergistics2.Logger.LogWarning($"Sold: {itemId}");
                StoredItem currentItem = systemDatabase.allItems[itemId];
                ItemSummary itemSummary = systemDatabase.itemSummaries[currentItem.name];

                try
                {
                    terminal.groupCredits += (int)((float)currentItem.value * startOfRound.companyBuyingRate);
                    soldItems.Add(itemId);
                    ModifySummaryValuesClientRpc(currentItem.name, -currentItem.value, -1, -itemSummary.weight);

                    ModifySystemStats(-1, -itemSummary.weight, -currentItem.value);
                }
                catch (Exception ex)
                {
                    LethalEnergistics2.Logger.LogError($"Failed to return item: {ex.ToString()}");
                }
            }

            int profit = (int)((float)soldItems.Sum(item => systemDatabase.allItems[item].value) * startOfRound.companyBuyingRate);

            DepositItemsDesk itemDesk = FindObjectOfType<DepositItemsDesk>();
            itemDesk.SellItemsClientRpc(profit, terminal.groupCredits, soldItems.Count, startOfRound.companyBuyingRate);
            startOfRound.gameStats.scrapValueCollected += profit;
            timeOfDay.quotaFulfilled += profit;

            RemoveItemsFromSystemDatabaseClientRpc(soldItems.ToArray());
        }

        [ClientRpc]
        private void AnimateImportButtonOnClientRpc()
        {
            GetComponentInParent<Transform>().gameObject.GetComponentsInChildren<Animator>().First(a => a.name == "ButtonAnimContainer").SetTrigger("buttonPressed");
        }

        [ClientRpc]
        private void SetMonitorStatusClientRpc(bool active)
        {
            monitorInUse = active;
            GetComponentInParent<Transform>().gameObject.GetComponentsInChildren<Animator>().First(a => a.name == "MonitorAnimContainer").SetBool("monitorActive", active);
        }

        private void PressImportButtonOnClient(PlayerControllerB playerController)
        {
            LethalEnergistics2.Logger.LogInfo("Pressed button as client");
            PressImportButtonOnServerRpc();
        }

        private void ActivateMonitorOnClient(PlayerControllerB playerController)
        {
            ActivateMonitorOnServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void PressImportButtonOnServerRpc()
        {
            LethalEnergistics2.Logger.LogWarning("Button pressed by client");
            if (importCooldown > 0 || monitorInUse)
            {
                return;
            }
            importCooldown = 10;
            AnimateImportButtonOnClientRpc();
            //GetComponentsInChildren<Animator>().First(a => a.name == "ButtonAnimContainer").SetTrigger("buttonPressed");
            ImportItems();
        }

        [ServerRpc(RequireOwnership = false)]
        private void ActivateMonitorOnServerRpc(ServerRpcParams rpcParams = default)
        {
            LethalEnergistics2.Logger.LogWarning("ActivateMonitor attempted");
            ulong clientId = rpcParams.Receive.SenderClientId;
            if (monitorInUse)
            {
                LethalEnergistics2.Logger.LogDebug("Failed to activate: Monitor in use.");
                return;
            }
            SetMonitorStatusClientRpc(true);
            userOnMonitor = clientId;
            ClientRpcParams clientRpcParams = new()
            {
                Send = new()
                {
                    TargetClientIds = new[] { clientId }
                }
            };
            StartUsingMonitorClientRpc(clientRpcParams);
        }

        [ServerRpc(RequireOwnership = false)]
        private void DeactivateMonitorOnServerRpc(ServerRpcParams rpcParams = default)
        {
            LethalEnergistics2.Logger.LogWarning("DeactivateMonitor attempted");
            ulong clientId = rpcParams.Receive.SenderClientId;
            if (clientId != userOnMonitor)
            {
                LethalEnergistics2.Logger.LogWarning("Player '" + clientId + "' attempted to stop using the monitor while not using it");
            }
            LethalEnergistics2.Logger.LogWarning("Stop using monitor");
            if (!monitorInUse)
            {
                return;
            }
            SetMonitorStatusClientRpc(false);
            userOnMonitor = 0;
            ClientRpcParams clientRpcParams = new()
            {
                Send = new()
                {
                    TargetClientIds = new[] { clientId }
                }
            };
            StopUsingMonitorClientRpc(clientRpcParams);
        }

        [ClientRpc]
        private void StartUsingMonitorClientRpc(ClientRpcParams rpcParams)
        {
            usingMonitor = true;

            GameNetworkManager.Instance.localPlayerController.inTerminalMenu = true;

            SetMonitorPage("MainMenu");
            SetupMonitorActions();
        }

        [ClientRpc]
        private void StopUsingMonitorClientRpc(ClientRpcParams rpcParams)
        {
            monitorTrigger.StopSpecialAnimation();
            usingMonitor = false;

            GameNetworkManager.Instance.localPlayerController.inTerminalMenu = false;

            SetMonitorPage("Disabled");
            RemoveMonitorActions();

        }

        // [ClientRpc]
        private void SpawnItem(StoredItem itemInfo)
        {
            GameObject itemPrefab = allItemsList.First(i => i.name == itemInfo.prefab).spawnPrefab;
            Battery itemBattery = new(itemInfo.batteryEmpty, itemInfo.batteryCharge);
            int itemValue = itemInfo.value;

            if (itemPrefab == null)
            {
                return;
            }

            if (itemBattery == null)
            {
                return;
            }

            GameObject newItem = Instantiate(itemPrefab, itemExportLocation.position, Quaternion.identity, shipHangar.transform);

            GrabbableObject newItemGrabbable = newItem.GetComponent<GrabbableObject>();

            newItem.transform.rotation = Quaternion.Euler(newItemGrabbable.itemProperties.restingRotation.x, UnityEngine.Random.Range(0, 360), newItemGrabbable.itemProperties.restingRotation.z);

            newItemGrabbable.fallTime = 0f;
            newItemGrabbable.insertedBattery = itemBattery;
            newItemGrabbable.scrapValue = itemValue;

            NetworkObject newItemNetworkObject = newItem.GetComponent<NetworkObject>();

            newItemNetworkObject.Spawn(false);
        }

        private void SortItems()
        {
            int newSortLength = systemDatabase.allItems.Count;
            if (monitorSettings.itemsSortOrder == monitorSettings.itemsPreviousSortOrder && newSortLength == monitorSettings.itemsPreviousSortLength)
            {
                LethalEnergistics2.Logger.LogDebug($"[{newSortLength}] items are already sorted by order [{monitorSettings.itemsSortOrder}]");
                return;
            }

            monitorSettings.itemsPreviousSortOrder = monitorSettings.itemsSortOrder;
            monitorSettings.itemsPreviousSortLength = newSortLength;

            switch (monitorSettings.itemsSortOrder)
            {
                case 0: // Sort by name
                    monitorSettings.sortedItems = systemDatabase.allItems.OrderBy(item => item.Value.name).ThenByDescending(item => item.Value.name).Select(item => item.Key).ToList();
                    LethalEnergistics2.Logger.LogDebug($"[{newSortLength}] items sorted by order [0]");
                    break;
                case 1: // Sort by value
                    monitorSettings.sortedItems = systemDatabase.allItems.OrderBy(item => item.Value.value).ThenByDescending(item => item.Value.value).Select(item => item.Key).ToList();
                    LethalEnergistics2.Logger.LogDebug($"[{newSortLength}] items sorted by order [1]");
                    break;
                case 2: // Sort by weight
                    monitorSettings.sortedItems = systemDatabase.allItems.OrderBy(item => systemDatabase.itemSummaries[item.Value.name].weight).ThenByDescending(item => systemDatabase.itemSummaries[item.Value.name].weight).Select(item => item.Key).ToList();
                    LethalEnergistics2.Logger.LogDebug($"[{newSortLength}] items sorted by order [2]");
                    break;
            }
        }

        private void SortTools()
        {
            int newSortLength = systemDatabase.allTools.Count;
            if (newSortLength == monitorSettings.toolsPreviousSortLength)
            {
                LethalEnergistics2.Logger.LogDebug($"[{newSortLength}] items are already sorted.");
                return;
            }

            monitorSettings.toolsPreviousSortLength = newSortLength;

            monitorSettings.sortedTools = systemDatabase.allTools.OrderBy(item => item.Value.name).ThenByDescending(item => item.Value.name).Select(item => item.Key).ToList();
            LethalEnergistics2.Logger.LogDebug($"[{newSortLength}] items sorted by order [0]");
        }

        private void SortSummaries()
        {
            int newSortLength = systemDatabase.itemSummaries.Count;
            if (monitorSettings.summariesSortOrder == monitorSettings.summariesPreviousSortOrder && newSortLength == monitorSettings.summariesPreviousSortLength)
            {
                LethalEnergistics2.Logger.LogDebug($"[{newSortLength}] items are already sorted by order [{monitorSettings.summariesSortOrder}]");
                return;
            }

            monitorSettings.summariesPreviousSortOrder = monitorSettings.summariesSortOrder;
            monitorSettings.summariesPreviousSortLength = newSortLength;

            LethalEnergistics2.Logger.LogDebug($"[{newSortLength}] to be sorted by [{monitorSettings.summariesSortOrder}]");

            switch (monitorSettings.summariesSortOrder)
            {
                case 0: // Sort by name
                    monitorSettings.sortedSummaries = systemDatabase.itemSummaries.OrderBy(summary => summary.Value.name).ThenByDescending(summary => summary.Value.name).Select(summary => summary.Key).ToList();
                    LethalEnergistics2.Logger.LogDebug($"[{newSortLength}] items sorted by order [0]");
                    break;
                case 1: // Sort by quantity
                    monitorSettings.sortedSummaries = systemDatabase.itemSummaries.OrderBy(summary => summary.Value.totalQuantity).ThenByDescending(summary => summary.Value.totalQuantity).Select(summary => summary.Key).ToList();
                    LethalEnergistics2.Logger.LogDebug($"[{newSortLength}] items sorted by order [1]");
                    break;
                case 2: // Sort by value
                    monitorSettings.sortedSummaries = systemDatabase.itemSummaries.OrderBy(summary => summary.Value.totalValue).ThenByDescending(summary => summary.Value.totalValue).Select(summary => summary.Key).ToList();
                    LethalEnergistics2.Logger.LogDebug($"[{newSortLength}] items sorted by order [2]");
                    break;
                case 3: // Sort by weight
                    monitorSettings.sortedSummaries = systemDatabase.itemSummaries.OrderBy(summary => summary.Value.totalWeight).ThenByDescending(summary => summary.Value.totalWeight).Select(summary => summary.Key).ToList();
                    LethalEnergistics2.Logger.LogDebug($"[{newSortLength}] items sorted by order [3]");
                    break;
            }
        }

        private IEnumerator ClearMonitorTextDelayed()
        {
            yield return new WaitForSeconds(2);
            monitorHeader.text = "";
            monitorBody.text = "";
            monitorFooter.text = "";
        }

        private void SetMonitorPage(string pageName)
        {
            MonitorPage monitorPage = monitorMenus[pageName];

            string footer = monitorPage.footer;
            string body = monitorPage.body;
            string header = monitorPage.header;

            string status;
            string temp;
            StoredItem currentItem;
            StoredItem currentTool;
            ItemSummary currentSummary;

            List<string> icons;

            if (monitorSettings.currentPage != monitorPage.pageName)
            {
                monitorSettings = monitorMenus[monitorPage.pageName].defaultSettings;

                switch (monitorPage.pageName)
                {
                    case "ListItems":
                        SortItems();
                        break;
                    case "ListTools":
                        SortTools();
                        break;
                    case "ListSummaries":
                        SortSummaries();
                        break;
                }
            }

            switch (monitorPage.pageName)
            {
                case "Disabled":
                    header = monitorHeader.text;
                    body = monitorBody.text;
                    footer = monitorFooter.text;
                    ClearMonitorTextDelayed();
                    break;
                case "MainMenu":
                    icons = new List<string>()
                    {
                        " ", // 0
                        " ", // 1
                        " ", // 2
                        " ", // 3
                        " "  // 4
                    };

                    icons[monitorSettings.hoveredOption] = "*";

                    body = string.Format(body, icons[0], icons[1], icons[2], icons[3], icons[4]);

                    break;
                case "ListItems":
                    icons = new List<string>()
                    {
                        " ", // 0
                        " ", // 1
                        " "  // 2
                    };

                    icons[monitorSettings.itemsSortOrder] = "*";

                    header = string.Format(header, icons[0], icons[1], icons[2]);

                    temp = "";

                    if (monitorSettings.sortedItems.Count() < monitorSettings.displayEnd)
                    {
                        monitorSettings.displayEnd = monitorSettings.sortedItems.Count;
                    }

                    for (int itemLocation = (int)monitorSettings.displayStart; itemLocation < monitorSettings.displayEnd; itemLocation++)
                    {
                        currentItem = systemDatabase.allItems[monitorSettings.sortedItems[itemLocation]];

                        if (itemLocation == monitorSettings.hoveredOption)
                        {
                            status = "*";
                        }
                        else if (monitorSettings.selectedItems.Contains(monitorSettings.sortedItems[itemLocation]))
                        {
                            status = "x";
                        }
                        else
                        {
                            status = " ";
                        }

                        temp += string.Format(body, status, currentItem.name.PadRight(18), (currentItem.value.ToString() + "▮").PadRight(17), (systemDatabase.itemSummaries[currentItem.name].weight.ToString() + " lb").PadRight(17), systemDatabase.itemSummaries[currentItem.name].twoHanded ? "*" : " ");
                    }

                    body = temp;
                    break;
                case "ListTools":
                    temp = "";

                    if (monitorSettings.sortedTools.Count() < monitorSettings.displayEnd)
                    {
                        monitorSettings.displayEnd = monitorSettings.sortedTools.Count;
                    }

                    for (int toolLocation = monitorSettings.displayStart; toolLocation < monitorSettings.displayEnd; toolLocation++)
                    {
                        currentTool = systemDatabase.allTools[monitorSettings.sortedTools[toolLocation]];

                        if (toolLocation == monitorSettings.hoveredOption)
                        {
                            status = "*";
                        }
                        else if (monitorSettings.selectedTools.Contains(monitorSettings.sortedTools[toolLocation]))
                        {
                            status = "x";
                        }
                        else
                        {
                            status = " ";
                        }

                        temp += string.Format(body, status, currentTool.name.PadRight(18), (currentTool.value.ToString() + "▮").PadRight(17), (systemDatabase.itemSummaries[currentTool.name].weight.ToString() + " lb").PadRight(17), systemDatabase.itemSummaries[currentTool.name].twoHanded ? "*" : " ");
                    }

                    body = temp;
                    break;
                case "ListSummaries":
                    icons = new List<string>()
                    {
                        " ", // 0
                        " ", // 1
                        " ", // 2
                        " "  // 3
                    };

                    icons[monitorSettings.summariesSortOrder] = "*";

                    header = string.Format(header, icons[0], icons[1], icons[2], icons[3]);

                    temp = "";

                    if (monitorSettings.sortedSummaries.Count() < monitorSettings.displayEnd)
                    {
                        monitorSettings.displayEnd = monitorSettings.sortedSummaries.Count;
                    }

                    for (int summaryLocation = monitorSettings.displayStart; summaryLocation < monitorSettings.displayEnd; summaryLocation++)
                    {
                        currentSummary = systemDatabase.itemSummaries[monitorSettings.sortedSummaries[summaryLocation]];

                        if (summaryLocation == monitorSettings.hoveredOption)
                        {
                            status = "*";
                        }
                        else if (monitorSettings.selectedSummaries.Contains(monitorSettings.sortedSummaries[summaryLocation]))
                        {
                            status = "x";
                        }
                        else
                        {
                            status = " ";
                        }

                        temp += string.Format(body, status, currentSummary.name.PadRight(16), currentSummary.totalQuantity.ToString().PadRight(12), (currentSummary.totalValue.ToString() + "▮").PadRight(11), (currentSummary.weight.ToString() + " lb").PadRight(12), currentSummary.twoHanded ? "*" : " ");
                    }

                    body = temp;
                    break;
                case "SystemStatistics":
                    body = string.Format(body, ("Version " + MyPluginInfo.PLUGIN_VERSION.ToString()).PadLeft(44), (currentStats.quantity.ToString()).PadLeft(24), (currentStats.weight.ToString()).PadLeft(17), (currentStats.value.ToString()).PadLeft(17), (gameStats.quantity.ToString()).PadLeft(26), (gameStats.weight.ToString()).PadLeft(19), (gameStats.value.ToString()).PadLeft(19), (lifetimeStats.quantity.ToString()).PadLeft(23), (lifetimeStats.weight.ToString()).PadLeft(16), (lifetimeStats.value.ToString()).PadLeft(16));
                    break;
            }

            monitorHeader.text = header;
            monitorBody.text = body;
            monitorFooter.text = footer;
        }
    }
}