using System;
using Unity.Netcode;

namespace LethalEnergistics2.Classes
{
    [Serializable]
    struct StoredItem : INetworkSerializable
    {
        public int id;
        public string prefab;
        public float batteryCharge;
        public bool batteryEmpty;
        public int value;
        public string name;

        public StoredItem(int id, string prefab, float batteryCharge, bool batteryEmpty, int value, string name)
        {
            this.id = id;
            this.prefab = prefab;
            this.batteryCharge = batteryCharge;
            this.batteryEmpty = batteryEmpty;
            this.value = value;
            this.name = name;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref id);
            serializer.SerializeValue(ref prefab);
            serializer.SerializeValue(ref batteryCharge);
            serializer.SerializeValue(ref batteryEmpty);
            serializer.SerializeValue(ref value);
            serializer.SerializeValue(ref name);
        }
    }
}
