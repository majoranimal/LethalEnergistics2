using Unity.Netcode;
using UnityEngine.InputSystem;

namespace LethalEnergistics2.Classes
{
    struct MonitorPage : INetworkSerializable
    {
        public string pageName;
        public string header;
        public string body;
        public string footer;
        public MonitorSettings defaultSettings;
        public InputBinding enabledBindings;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public MonitorPage()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public MonitorPage(string pageName, string header, string body, string footer, MonitorSettings defaultSettings)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            this.pageName = pageName;
            this.header = header;
            this.body = body;
            this.footer = footer;
            this.defaultSettings = defaultSettings;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref pageName);
            serializer.SerializeValue(ref header);
            serializer.SerializeValue(ref body);
            serializer.SerializeValue(ref footer);
        }

        // Example constructor:
        //new MonitorPage()
        //{
        //    pageName = "",
        //    header = "",
        //    body = "",
        //    footer = ""
        //},
    }
}
