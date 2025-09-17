using Unity.Netcode;

namespace LethalEnergistics2.Classes
{
    class ItemSummary : INetworkSerializable
    {
        public string name;
        public float weight;
        public bool twoHanded;
        public int totalValue;
        public float totalWeight;
        public int totalQuantity;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ItemSummary()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
        }

        public ItemSummary(string name, float weight, bool twoHanded, int totalValue, float totalWeight, int totalQuantity)
        {
            this.name = name;
            this.weight = weight;
            this.twoHanded = twoHanded;
            this.totalValue = totalValue;
            this.totalWeight = totalWeight;
            this.totalQuantity = totalQuantity;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref name);
            serializer.SerializeValue(ref weight);
            serializer.SerializeValue(ref twoHanded);
            serializer.SerializeValue(ref totalValue);
            serializer.SerializeValue(ref totalWeight);
            serializer.SerializeValue(ref totalQuantity);
        }
    }
}
