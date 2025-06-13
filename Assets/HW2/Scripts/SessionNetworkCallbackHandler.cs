using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace HW2.Scripts
{
    public class SessionNetworkCallbackHandler : INetworkRunnerCallbacks
    {
        private readonly Action<PlayerRef> _onPlayerJoined;
        private readonly Action<PlayerRef> _onPlayerLeft;
        private readonly Action _onSessionConnected;
        private readonly Action<NetDisconnectReason> _onSessionDisconnected;

        public SessionNetworkCallbackHandler(
            Action onSessionConnected,
            Action<NetDisconnectReason> onSessionDisconnected,
            Action<PlayerRef> onPlayerJoined,
            Action<PlayerRef> onPlayerLeft)
        {
            _onSessionConnected = onSessionConnected;
            _onSessionDisconnected = onSessionDisconnected;
            _onPlayerJoined = onPlayerJoined;
            _onPlayerLeft = onPlayerLeft;
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("Connected to session server");
            _onSessionConnected?.Invoke();
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            Debug.Log($"Disconnected from session server: {reason}");
            _onSessionDisconnected?.Invoke(reason);
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"Player joined session: {player}");
            _onPlayerJoined?.Invoke(player);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"Player left session: {player}");
            _onPlayerLeft?.Invoke(player);
        }

        // Sessions don't handle session list updates (that's lobby's job)
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        // Empty implementations for unused callbacks in session context
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
