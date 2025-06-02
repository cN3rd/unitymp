using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HW1.Scripts
{
    public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkRunner runner;
        [SerializeField] private TMP_Dropdown lobbyDropdown;
        [SerializeField] private Button joinLobbyButton;
        [SerializeField] private Button newRoomButton;
        [SerializeField] private ErrorPopup errorPopup;
        [SerializeField] private CreateRoomPopup createRoomPopup;
        [SerializeField] private TextMeshProUGUI noLobbySelectedText;
        [SerializeField] private TextMeshProUGUI noRoomsText;

        private bool _canEnableJoinLobbyButton = true;
        private List<SessionInfo> _sessions = new List<SessionInfo>();

        public void Start()
        {
            runner.AddCallbacks(this);
            joinLobbyButton.onClick.AddListener(OnJoinLobbyClicked);
            lobbyDropdown.onValueChanged.AddListener(OnLobbyDropdownValueChanged);
            createRoomPopup.OnCreateRoom += OnCreateRoom;
            UpdateUIState();
        }

        private async void OnCreateRoom(CreateRoomDetails details)
        {
            var result = await runner.StartGame(new StartGameArgs()
            {
                CustomLobbyName = lobbyDropdown.options[lobbyDropdown.value].text
            });

            if (!result.Ok)
            {
                errorPopup.ShowError(result.ErrorMessage);
                Debug.LogError(result.ErrorMessage);
            }
            UpdateUIState();
        }

        private void OnLobbyDropdownValueChanged(int newValue) => UpdateUIState();

        private void UpdateUIState()
        {
            // disable top-ui buttons
            newRoomButton.interactable = runner.LobbyInfo.IsValid &&
                                         runner.LobbyInfo.Name == $"lobby{lobbyDropdown.value}";
            joinLobbyButton.interactable = _canEnableJoinLobbyButton && (!runner.LobbyInfo.IsValid ||
                runner.LobbyInfo.Name !=
                $"lobby{lobbyDropdown.value}");

            // update lobby text
            noLobbySelectedText.gameObject.SetActive(!runner.LobbyInfo.IsValid);
            noRoomsText.gameObject.SetActive(runner.LobbyInfo.IsValid && (_sessions == null || _sessions.Count == 0));
            
            // generate sessions button
        }

        private async void OnJoinLobbyClicked()
        {
            _canEnableJoinLobbyButton = false;
            UpdateUIState();

            var result =
                await runner.JoinSessionLobby(SessionLobby.Custom, $"lobby{lobbyDropdown.value}");

            _canEnableJoinLobbyButton = true;
            UpdateUIState();

            if (!result.Ok)
            {
                errorPopup.ShowError(result.ErrorMessage);
                Debug.LogError(result.ErrorMessage);
            }

            Debug.Log($"Joined Lobby \"{lobbyDropdown.options[lobbyDropdown.value].text}\"");
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            Debug.Log($"Session list updated. Found {sessionList.Count} sessions.");
            foreach (var session in sessionList)
            {
                Debug.Log($"Session Name: {session.Name}, Player Count: {session.PlayerCount}");
            }
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("Connected to server.");
            Debug.Log($"Is lobby valid: {runner.LobbyInfo.IsValid}");
            Debug.Log($"Current lobby: {(runner.LobbyInfo.IsValid ? runner.LobbyInfo.Name : "None")}");
            UpdateUIState();
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            Debug.Log($"Disconnected from server: {reason}");
            UpdateUIState();
        }


        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

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
