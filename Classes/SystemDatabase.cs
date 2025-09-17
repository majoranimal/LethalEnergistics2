using System;
using System.Collections.Generic;

namespace LethalEnergistics2.Classes
{
    [Serializable]
    class SystemDatabase
    {
        public Dictionary<int, StoredItem> allTools = new();
        public Dictionary<int, StoredItem> allItems = new();
        public Dictionary<string, ItemSummary> itemSummaries = new();

        public SystemDatabase(Dictionary<int, StoredItem> allItems, Dictionary<string, ItemSummary> itemSummaries)
        {
            this.allItems = allItems;
            this.itemSummaries = itemSummaries;
        }

        public SystemDatabase()
        {
        }
    }
}
