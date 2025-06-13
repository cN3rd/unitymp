using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace HW2.Scripts
{
    public class LobbyNetworkCallbackHandler : INetworkRunnerCallbacks
    {
        private readonly Action _onLobbyConnected;
        private readonly Action<NetDisconnectReason> _onLobbyDisconnected;
        private readonly Action<List<SessionInfo>> _onSessionListUpdated;

        public LobbyNetworkCallbackHandler(
            Action<List<SessionInfo>> onSessionListUpdated,
            Action onLobbyConnected,
            Action<NetDisconnectReason> onLobbyDisconnected)
        {
            _onSessionListUpdated = onSessionListUpdated;
            _onLobbyConnected = onLobbyConnected;
            _onLobbyDisconnected = onLobbyDisconnected;
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) =>
            _onSessionListUpdated?.Invoke(sessionList);

        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("Connected to lobby server");
            _onLobbyConnected?.Invoke();
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            Debug.Log($"Disconnected from lobby server: {reason}");
            _onLobbyDisconnected?.Invoke(reason);
        }

        // Lobby doesn't typically handle individual players joining/leaving
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

        // Empty implementations for unused callbacks in lobby context
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnConnectRequest(NetworkRunner runner,
            NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress,
            NetConnectFailedReason reason)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key,
            ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key,
            float progress)
        {
        }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner,
            Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}
