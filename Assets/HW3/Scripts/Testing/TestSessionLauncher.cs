using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace HW3.Scripts.Testing
{
    public class TestLauncher : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkRunner sessionRunner;
        [SerializeField] private GameObject playerDataPrefab;
        [Range(1,4),SerializeField] private int maxPlayers = 2;

        private int _playerNum;
        private bool _hasJoined;
        
        private static int GetPlayerNum()
        {
            string[] args = Environment.GetCommandLineArgs();
            
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] != "-name" || i + 1 >= args.Length) continue;

                string playerName = args[i + 1];
                if (playerName.StartsWith("Player ") && int.TryParse(playerName.Replace("Player ", ""), out int num))
                {
                    return num;
                }
            }
            return 1; // Default to Player 1
        }
        
        private async void Start()
        {
            _playerNum = GetPlayerNum();
            Debug.Log($"Player {_playerNum} starting...");
            
            sessionRunner.AddCallbacks(this);
            await sessionRunner.JoinSessionLobby(SessionLobby.Custom, "TestLobby");

            // Player 1 creates the session, others wait for session list updates
            if (_playerNum == 1)
            {
                await JoinGame();
            }
        }

        private async Task JoinGame()
        {
            if (_hasJoined) return;
            _hasJoined = true;
            
            Debug.Log($"Player {_playerNum} joining game...");
            await sessionRunner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Shared,
                SessionName = "TestSession",
                PlayerCount = maxPlayers,
                CustomLobbyName = "TestLobby",
            });

            CreateOrGetPlayerDataObject();
        }

        private PlayerData CreateOrGetPlayerDataObject()
        {
            if (sessionRunner.TryGetPlayerObject(sessionRunner.LocalPlayer, out var playerDataObject))
                return playerDataObject.GetComponent<PlayerData>();
            
            playerDataObject = sessionRunner.Spawn(playerDataPrefab);
            sessionRunner.SetPlayerObject(sessionRunner.LocalPlayer, playerDataObject);
        
            var playerData = playerDataObject.GetComponent<PlayerData>();
            playerData.Nickname = SillyId.GenerateGamertag();
            
            Debug.Log($"Registered current player as \"{playerData.Nickname}\"");
            return playerData;
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            if (_hasJoined || _playerNum == 1) return;
            
            // Find the test session
            foreach (var session in sessionList)
            {
                if (session.Name == "TestSession" && session.PlayerCount < maxPlayers)
                {
                    _ = JoinGame(); // Join when session is available
                    break;
                }
            }
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"Player joined. Total: {runner.SessionInfo.PlayerCount}/{maxPlayers}");
            
            // Load scene when all players have joined (only master client does this)
            if (runner.SessionInfo.PlayerCount == maxPlayers && runner.IsSharedModeMasterClient && runner.IsSceneAuthority)
            {
                CreateOrGetPlayerDataObject();
                runner.LoadScene("GameScene");
            }
        }

        public void OnSceneLoadDone(NetworkRunner runner) => runner.RemoveCallbacks(this);

        // Empty required callbacks
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {}
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {}
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}
        public void OnInput(NetworkRunner runner, NetworkInput input) {}
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}
        public void OnConnectedToServer(NetworkRunner runner) {}
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}
        public void OnSceneLoadStart(NetworkRunner runner) {}
    }
}
