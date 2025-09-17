using System;
using Unity.Netcode;

namespace LethalEnergistics2.Scripts
{
    internal class MainNetworkHandler : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            LevelEvent = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            Instance = this;

            base.OnNetworkSpawn();
        }

        [ClientRpc]
        private void EventClientRpc(string eventName)
        {
            LevelEvent?.Invoke(eventName); // If the event has subscribers (does not equal null), invoke the event
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private static event Action<string> LevelEvent;
        public static MainNetworkHandler Instance { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }
}