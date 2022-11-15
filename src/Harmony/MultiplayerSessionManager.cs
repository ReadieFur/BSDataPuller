using DataPuller.Data;
using HarmonyLib;

#nullable enable
namespace DataPuller.Harmony
{
    [HarmonyPatch(typeof(global::MultiplayerSessionManager), nameof(global::MultiplayerSessionManager.StartSession))]
    internal class MultiplayerSessionManager
    {
        private static ConnectedPlayerManager? _connectedPlayerManager;
        private static int _maxPlayerCount;
        
        [HarmonyPostfix]
        public static void StartSession_PostFix(ref ConnectedPlayerManager connectedPlayerManager)
        {
            MapData.Instance.IsMultiplayer = true;
            _connectedPlayerManager = connectedPlayerManager;
            _connectedPlayerManager.connectedEvent += UpdatePlayerCount;
            _connectedPlayerManager.disconnectedEvent += UpdatePlayerCount;
            _connectedPlayerManager.playerConnectedEvent += UpdatePlayerCount;
            _connectedPlayerManager.playerDisconnectedEvent += UpdatePlayerCount;
            UpdatePlayerCount();
        }

        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(global::MultiplayerSessionManager), nameof(global::MultiplayerSessionManager.SetMaxPlayerCount))]
        public static void SetMaxPlayerCount_PostFix(ref int maxPlayerCount)
        {
            _maxPlayerCount = maxPlayerCount;
            UpdatePlayerCount();
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(global::MultiplayerSessionManager), nameof(global::MultiplayerSessionManager.EndSession))]
        public static void EndSession_PostFix()
        {
            MapData.Instance.IsMultiplayer = false;
            _maxPlayerCount = 0;

            if (_connectedPlayerManager is not null)
            {
                _connectedPlayerManager.connectedEvent -= UpdatePlayerCount;
                _connectedPlayerManager.disconnectedEvent -= UpdatePlayerCount;
                _connectedPlayerManager.playerConnectedEvent -= UpdatePlayerCount;
                _connectedPlayerManager.playerDisconnectedEvent -= UpdatePlayerCount;
                _connectedPlayerManager = null;
            }

            UpdatePlayerCount();
        }

        private static void UpdatePlayerCount(object e)
        {
            UpdatePlayerCount();
        }

        private static void UpdatePlayerCount(DisconnectedReason disconnectedReason)
        {
            UpdatePlayerCount();
        }

        private static void UpdatePlayerCount()
        {
            MapData.Instance.MultiplayerLobbyMaxSize = _maxPlayerCount;
            MapData.Instance.MultiplayerLobbyCurrentSize = _connectedPlayerManager?.connectedPlayerCount ?? 0;
            MapData.Instance.Send();
        }
    }

}
