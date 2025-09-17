using System.Collections.Generic;

namespace LethalEnergistics2.Classes
{
    struct ValuedCombination
    {
        public int totalValue;
        public List<StoredItem> valuedItems;

        public ValuedCombination(int totalValue, List<StoredItem> valuedItems)
        {
            this.totalValue = totalValue;
            this.valuedItems = valuedItems;
        }
    }
}
