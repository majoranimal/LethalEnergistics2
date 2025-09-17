using Unity.Netcode;

namespace LethalEnergistics2.Classes
{
    public class NetworkString : INetworkSerializable
    {
        public string storedString;

        public NetworkString(string storedString)
        {
            this.storedString = storedString;
        }

        public static implicit operator string(NetworkString storedString)
        {
            return storedString.storedString;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref storedString);
        }
    }
}
