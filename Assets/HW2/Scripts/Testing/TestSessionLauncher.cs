using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace HW2.Scripts.Testing
{
    public class TestSessionLauncher : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkRunner sessionRunner;
        [SerializeField] private int maxPlayers = 2;
        [SerializeField] private float joinDelayPerPlayer = 2f;
        [SerializeField] private float maxWaitTime = 30f;

        private string _playerId;
        private string _playerName;
        private int _playerNum;
        private bool _isClone;
        private bool _hasJoinedGame;
        private bool _isJoiningGame;
        
        private static (string playerId, string playerName, int playerNum, bool isClone) GetPlayModeInfo()
        {
            const string vpIdArg = "-vpId";
            const string nameArg = "-name";
            const string isCloneArg = "--virtual-project-clone";

            string[] args = System.Environment.GetCommandLineArgs();
    
            string playerId = "Unknown";
            string playerName = "Player 1";
            bool isClone = false;

            for (int index = 0; index < args.Length; index++)
            {
                string arg = args[index];
                
                if (arg.StartsWith(vpIdArg))
                {
                    playerId = arg.Replace($"{vpIdArg}=", string.Empty);
                }
                else if (arg.StartsWith(nameArg) && index + 1 < args.Length)
                {
                    playerName = args[index + 1];
                }
                else if (arg == isCloneArg)
                {
                    isClone = true;
                }
            }

            // Extract player number from name (e.g., "Player 2" -> 2)
            int playerNum = 1;
            if (playerName.StartsWith("Player ") && playerName.Length > 7)
            {
                if (int.TryParse(playerName.Substring(7), out int parsed))
                {
                    playerNum = parsed;
                }
            }

            return (playerId, playerName, playerNum, isClone);
        }
        
        private async void Start()
        {
            (_playerId, _playerName, _playerNum, _isClone) = GetPlayModeInfo();
            Debug.Log($"Current player: {_playerName} (#{_playerNum}, {_playerId}), clone: {_isClone}");
            
            if (sessionRunner == null)
            {
                Debug.LogError("SessionRunner is null! Please assign it in the inspector.");
                return;
            }
            
            sessionRunner.AddCallbacks(this);
            
            try
            {
                var joinResult = await sessionRunner.JoinSessionLobby(SessionLobby.Custom, "TestLobby");
                if (!joinResult.Ok)
                {
                    Debug.LogError($"Failed to join lobby: {joinResult.ShutdownReason}");
                    return;
                }
                Debug.Log("Successfully joined lobby");

                // Only Player 1 creates the session
                if (_playerNum == 1)
                {
                    await TryStartGame();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during initialization: {e.Message}");
            }
        }

        private async Task TryStartGame()
        {
            if (_hasJoinedGame || _isJoiningGame) return;
            
            _isJoiningGame = true;
            
            try
            {
                Debug.Log($"Player {_playerNum} attempting to start/join game...");
                var result = await sessionRunner.StartGame(new StartGameArgs()
                {
                    GameMode = GameMode.Shared,
                    SessionName = "TestSession",
                    PlayerCount = maxPlayers,
                    CustomLobbyName = "TestLobby",
                });
                
                if (result.Ok)
                {
                    _hasJoinedGame = true;
                    Debug.Log($"Player {_playerNum} successfully joined game");
                }
                else
                {
                    Debug.LogWarning($"Player {_playerNum} failed to join game: {result.ShutdownReason}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception during game join: {e.Message}");
            }
            finally
            {
                _isJoiningGame = false;
            }
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            // Prevent multiple join attempts
            if (_hasJoinedGame || _isJoiningGame) return;
            
            // Find our target session
            // It's fair to assume sessionList[0] is our target SESSION
            SessionInfo targetSession = sessionList.Count == 0 ? null : sessionList[0];
            if (targetSession == null) 
            {
                Debug.Log("Target session not found in session list");
                return;
            }
            
            // Session is full
            if (targetSession.PlayerCount >= targetSession.MaxPlayers) 
            {
                Debug.Log($"Session is full ({targetSession.PlayerCount}/{targetSession.MaxPlayers})");
                return;
            }
            
            // Check if it's our turn to join based on player count and our number
            // Player 2 should only join when there's exactly 1 player in the session
            int expectedPlayerCount = _playerNum - 1;
            if (targetSession.PlayerCount != expectedPlayerCount)
            {
                Debug.Log($"Waiting for turn. Current players: {targetSession.PlayerCount}, Expected: {expectedPlayerCount} for Player {_playerNum}");
                return;
            }

            Debug.Log($"Player {_playerNum} joining existing session with {targetSession.PlayerCount} players");
            _ = TryStartGame(); // Fire and forget async call
        }
        
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"Player joined. Total players: {runner.SessionInfo.PlayerCount}/{runner.SessionInfo.MaxPlayers}");
            
            // Only proceed if we have max players and this client is the master
            if (runner.SessionInfo.PlayerCount != runner.SessionInfo.MaxPlayers) return;
            if (!runner.IsSharedModeMasterClient) return;

            Debug.Log("All players joined. Master client loading game scene...");
            runner.LoadScene("GameScene");
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) => Debug.Log($"Player left. Remaining players: {runner.SessionInfo.PlayerCount}");

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) 
        {
            Debug.Log($"Network runner shutdown: {shutdownReason}");
            _hasJoinedGame = false;
            _isJoiningGame = false;
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) 
        {
            Debug.Log($"Disconnected from server: {reason}");
            _hasJoinedGame = false;
            _isJoiningGame = false;
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) 
        {
            Debug.LogError($"Connection failed: {reason}");
            _isJoiningGame = false;
        }

        public void OnSceneLoadDone(NetworkRunner runner) 
        {
            Debug.Log("Scene load completed");
            runner.RemoveCallbacks(this);
        }

        // Empty callbacks
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}
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
